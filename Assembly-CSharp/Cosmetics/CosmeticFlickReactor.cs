using System;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;

namespace Cosmetics
{
	public class CosmeticFlickReactor : MonoBehaviour
	{
		private void Reset()
		{
			if (this.speedTracker == null)
			{
				this.speedTracker = base.GetComponent<SimpleSpeedTracker>();
			}
			if (this.rb == null)
			{
				this.rb = base.GetComponent<Rigidbody>();
			}
		}

		private void Awake()
		{
			this.rig = base.GetComponentInParent<VRRig>();
			if (this.rig == null && base.gameObject.GetComponentInParent<GTPlayer>() != null)
			{
				this.rig = GorillaTagger.Instance.offlineVRRig;
			}
			this.isLocal = this.rig != null && this.rig.isLocal;
			this.ResetState();
			this.blockUntilTime = 0f;
			this.hasLastPosition = false;
		}

		private void Update()
		{
			Vector3 vector = this.ResolveAxisDirection();
			if (vector.sqrMagnitude < 0.5f)
			{
				return;
			}
			float signedSpeedAlong = this.GetSignedSpeedAlong(vector);
			if (Mathf.Abs(signedSpeedAlong) >= this.minSpeedThreshold)
			{
				int num = ((signedSpeedAlong > 0f) ? 1 : (-1));
				if (num != this.lastPeakSign || Mathf.Abs(signedSpeedAlong) > Mathf.Abs(this.lastPeakSpeed))
				{
					if (this.lastPeakSign == 0 || num != -this.lastPeakSign)
					{
						this.lastPeakSign = num;
						this.lastPeakSpeed = signedSpeedAlong;
						this.lastPeakTime = Time.time;
						return;
					}
					float num2 = Time.time - this.lastPeakTime;
					float num3 = Mathf.Abs(this.lastPeakSpeed) + Mathf.Abs(signedSpeedAlong);
					bool flag = num2 <= this.flickWindowSeconds;
					bool flag2 = num3 >= this.directionChangeRequired;
					bool flag3 = Time.time >= this.blockUntilTime;
					if (flag && flag2 && flag3)
					{
						this.FireEvents(Mathf.Abs(signedSpeedAlong));
						this.blockUntilTime = Time.time + this.retriggerBufferSeconds;
						this.ResetState();
						return;
					}
					this.lastPeakSign = num;
					this.lastPeakSpeed = signedSpeedAlong;
					this.lastPeakTime = Time.time;
					return;
				}
			}
			else if (Time.time - this.lastPeakTime > this.flickWindowSeconds)
			{
				this.ResetState();
			}
		}

		private Vector3 ResolveAxisDirection()
		{
			switch (this.axisMode)
			{
			case CosmeticFlickReactor.AxisMode.X:
				if (!this.useWorldAxes)
				{
					return base.transform.right;
				}
				if (!(this.worldSpace != null))
				{
					return Vector3.right;
				}
				return this.worldSpace.right;
			case CosmeticFlickReactor.AxisMode.Y:
				if (!this.useWorldAxes)
				{
					return base.transform.up;
				}
				if (!(this.worldSpace != null))
				{
					return Vector3.up;
				}
				return this.worldSpace.up;
			case CosmeticFlickReactor.AxisMode.Z:
				if (!this.useWorldAxes)
				{
					return base.transform.forward;
				}
				if (!(this.worldSpace != null))
				{
					return Vector3.forward;
				}
				return this.worldSpace.forward;
			case CosmeticFlickReactor.AxisMode.CustomForward:
				if (!(this.axisReference != null))
				{
					return Vector3.zero;
				}
				return this.axisReference.forward;
			default:
				return Vector3.zero;
			}
		}

		private float GetSignedSpeedAlong(Vector3 axis)
		{
			Vector3 vector;
			if (this.speedTracker != null)
			{
				vector = this.speedTracker.GetWorldVelocity();
			}
			else if (this.rb != null)
			{
				vector = this.rb.linearVelocity;
			}
			else
			{
				if (!this.hasLastPosition)
				{
					this.lastPosition = base.transform.position;
					this.hasLastPosition = true;
					return 0f;
				}
				Vector3 vector2 = base.transform.position - this.lastPosition;
				float num = ((Time.deltaTime > Mathf.Epsilon) ? (1f / Time.deltaTime) : 0f);
				vector = vector2 * num;
				this.lastPosition = base.transform.position;
			}
			return Vector3.Dot(vector, axis.normalized);
		}

		private void FireEvents(float currentAbsSpeed)
		{
			if (this.isLocal)
			{
				UnityEvent onFlickLocal = this.OnFlickLocal;
				if (onFlickLocal != null)
				{
					onFlickLocal.Invoke();
				}
			}
			UnityEvent onFlickShared = this.OnFlickShared;
			if (onFlickShared != null)
			{
				onFlickShared.Invoke();
			}
			if (this.maxSpeedThreshold > 0f)
			{
				float num = Mathf.InverseLerp(this.minSpeedThreshold, this.maxSpeedThreshold, currentAbsSpeed);
				UnityEvent<float> unityEvent = this.onFlickStrength;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke(Mathf.Clamp01(num));
			}
		}

		private void ResetState()
		{
			this.lastPeakSign = 0;
			this.lastPeakSpeed = 0f;
			this.lastPeakTime = -9999f;
		}

		[Header("Axis")]
		[Tooltip("Which single axis/direction to use for flick detection.\n- X/Y/Z use the axes defined by the Space settings below (Local vs World).\n- CustomForward uses axisReference.forward (ignores Space).")]
		[SerializeField]
		private CosmeticFlickReactor.AxisMode axisMode = CosmeticFlickReactor.AxisMode.Z;

		[Tooltip("Used only when AxisMode = CustomForward. The forward/back of this transform defines the direction.")]
		[SerializeField]
		private Transform axisReference;

		[Header("Space")]
		[Tooltip("If enabled, X/Y/Z use world axes, otherwise local axes.\nUse Local for movement relative to the object’s facing.\nUse World for absolute directions independent of rotation.")]
		[SerializeField]
		private bool useWorldAxes;

		[Tooltip("Optional transform to define a custom world frame for X/Y/Z.\nIf assigned and Space is World, this transform’s Right/Up/Forward act as the world axes.\nIf not assigned, Unity’s global axes are used.")]
		[SerializeField]
		private Transform worldSpace;

		[Header("Velocity Source")]
		[Tooltip("Primary velocity tracker.")]
		[SerializeField]
		private SimpleSpeedTracker speedTracker;

		[Tooltip("Fallback velocity source if speedTracker is missing.")]
		[SerializeField]
		private Rigidbody rb;

		[Header("Thresholds")]
		[Tooltip("Minimum absolute signed speed along the chosen axis required to consider a object movement (m/s).")]
		[SerializeField]
		private float minSpeedThreshold = 2f;

		[Tooltip("Optional upper bound for mapping flick strength to 0–1.\nSet <= 0 to disable onFlickStrength.")]
		[SerializeField]
		private float maxSpeedThreshold;

		[Tooltip("How much back-and-forth reversal is required to register a flick.\nExample: 2.5 means => +1.3 then -1.2 within the window (|1.3| + |1.2| = 2.5).")]
		[SerializeField]
		private float directionChangeRequired = 2f;

		[Header("Timing")]
		[Tooltip("Max time allowed between the initial peak and its reversal (seconds).")]
		[SerializeField]
		private float flickWindowSeconds = 0.2f;

		[Tooltip("Buffer time after a successful flick during which no new flicks are allowed (seconds).")]
		[SerializeField]
		private float retriggerBufferSeconds = 0.15f;

		[Header("Events")]
		public UnityEvent OnFlickShared;

		public UnityEvent OnFlickLocal;

		public UnityEvent<float> onFlickStrength;

		private Vector3 lastPosition;

		private bool hasLastPosition;

		private float lastPeakSpeed;

		private float lastPeakTime = -999f;

		private int lastPeakSign;

		private float blockUntilTime;

		private VRRig rig;

		private bool isLocal;

		private enum AxisMode
		{
			X,
			Y,
			Z,
			CustomForward
		}
	}
}
