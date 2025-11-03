using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class ChargeableCosmeticEffects : MonoBehaviour, ITickSystemTick
	{
		private bool HasFractionals()
		{
			return this.continuousProperties.Count > 0 || this.whileCharging.GetPersistentEventCount() > 0;
		}

		private void Awake()
		{
			this.inverseMaxChargeSeconds = 1f / this.maxChargeSeconds;
			this.hasFractionalsCached = this.HasFractionals();
		}

		public void SetMaxChargeSeconds(float s)
		{
			this.maxChargeSeconds = s;
			this.inverseMaxChargeSeconds = 1f / this.maxChargeSeconds;
			this.SetChargeTime(this.chargeTime);
		}

		public void SetChargeState(bool state)
		{
			if (this.isCharging != state)
			{
				TickSystem<object>.AddTickCallback(this);
				this.isCharging = state;
			}
		}

		public void StartCharging()
		{
			this.SetChargeState(true);
		}

		public void StopCharging()
		{
			this.SetChargeState(false);
		}

		public void ToggleCharging()
		{
			this.SetChargeState(!this.isCharging);
		}

		public void SetChargeTime(float t)
		{
			if (t >= this.maxChargeSeconds)
			{
				if (this.chargeTime < this.maxChargeSeconds)
				{
					this.RunMaxCharge();
					return;
				}
			}
			else if (t <= 0f)
			{
				if (this.chargeTime > 0f)
				{
					this.RunNoCharge();
					return;
				}
			}
			else
			{
				TickSystem<object>.AddTickCallback(this);
				this.chargeTime = t;
				if (this.hasFractionalsCached)
				{
					this.RunChargeFrac();
				}
			}
		}

		public void SetChargeFrac(float f)
		{
			this.SetChargeTime(f * this.maxChargeSeconds);
		}

		public void EmptyCharge()
		{
			this.SetChargeTime(0f);
		}

		public void FillCharge()
		{
			this.SetChargeTime(this.maxChargeSeconds);
		}

		public void EmptyAndStop()
		{
			this.isCharging = false;
			this.EmptyCharge();
		}

		public void FillAndStop()
		{
			this.StopCharging();
			this.FillCharge();
		}

		public void EmptyAndStart()
		{
			this.StartCharging();
			this.EmptyCharge();
		}

		public void FillAndStart()
		{
			this.isCharging = true;
			this.FillCharge();
		}

		private void OnEnable()
		{
			if ((this.chargeTime <= 0f && this.isCharging) || (this.chargeTime >= this.maxChargeSeconds && !this.isCharging) || (this.chargeTime > 0f && this.chargeTime < this.maxChargeSeconds))
			{
				TickSystem<object>.AddTickCallback(this);
			}
		}

		private void OnDisable()
		{
			TickSystem<object>.RemoveTickCallback(this);
		}

		private void RunMaxCharge()
		{
			if (this.isCharging)
			{
				TickSystem<object>.RemoveTickCallback(this);
			}
			else
			{
				TickSystem<object>.AddTickCallback(this);
			}
			this.chargeTime = this.maxChargeSeconds;
			UnityEvent unityEvent = this.onMaxCharge;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			UnityEvent<float> unityEvent2 = this.whileCharging;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke(1f);
			}
			this.continuousProperties.ApplyAll(1f);
		}

		private void RunNoCharge()
		{
			if (!this.isCharging)
			{
				TickSystem<object>.RemoveTickCallback(this);
			}
			else
			{
				TickSystem<object>.AddTickCallback(this);
			}
			this.chargeTime = 0f;
			UnityEvent unityEvent = this.onNoCharge;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			UnityEvent<float> unityEvent2 = this.whileCharging;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke(0f);
			}
			this.continuousProperties.ApplyAll(0f);
		}

		private void RunChargeFrac()
		{
			float num = this.masterChargeRemapCurve.Evaluate(this.chargeTime * this.inverseMaxChargeSeconds);
			UnityEvent<float> unityEvent = this.whileCharging;
			if (unityEvent != null)
			{
				unityEvent.Invoke(num);
			}
			this.continuousProperties.ApplyAll(num);
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (this.isCharging && this.chargeTime < this.maxChargeSeconds)
			{
				this.chargeTime += Time.deltaTime * this.chargeGainSpeed;
				if (this.chargeTime >= this.maxChargeSeconds)
				{
					this.RunMaxCharge();
					return;
				}
				if (this.hasFractionalsCached)
				{
					this.RunChargeFrac();
					return;
				}
			}
			else if (!this.isCharging && this.chargeTime > 0f)
			{
				this.chargeTime -= Time.deltaTime * this.chargeLossSpeed;
				if (this.chargeTime <= 0f)
				{
					this.RunNoCharge();
					return;
				}
				if (this.hasFractionalsCached)
				{
					this.RunChargeFrac();
				}
			}
		}

		[SerializeField]
		private float maxChargeSeconds = 1f;

		[SerializeField]
		private float chargeGainSpeed = 1f;

		[SerializeField]
		private float chargeLossSpeed = 1f;

		[Tooltip("This will remap the internal charge output to whatever you set. The remapped value will be output by 'whileCharging' and the 'continuousProperties' (keep in mind that the remapped value will then be used as an INPUT for the curves on each ContinuousProperty).\n\nIt should start at (0,0) and end at (1,1).\n\nDisabled if there are no ContinuousProperties and no whileCharging event callbacks.")]
		[SerializeField]
		private AnimationCurve masterChargeRemapCurve = AnimationCurves.Linear;

		[SerializeField]
		private bool isCharging;

		[SerializeField]
		private ContinuousPropertyArray continuousProperties;

		[SerializeField]
		private UnityEvent<float> whileCharging;

		[SerializeField]
		private UnityEvent onMaxCharge;

		[SerializeField]
		private UnityEvent onNoCharge;

		private float chargeTime;

		private float inverseMaxChargeSeconds;

		private bool hasFractionalsCached;
	}
}
