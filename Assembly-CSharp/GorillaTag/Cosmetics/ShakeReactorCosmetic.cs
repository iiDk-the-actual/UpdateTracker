using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class ShakeReactorCosmetic : MonoBehaviour, ISpawnable
	{
		private void OnEnable()
		{
			this.lastReversalTime = Time.time;
			this.pathSinceLastReversal = 0f;
			this.recentHalfCycleDurations.Clear();
			this.hasLastDir = false;
			this.lastPosition = ((this.speedTracker != null) ? this.speedTracker.transform.position : base.transform.position);
			this.isShaking = false;
			this.debugCurrentHalfCycleDistance = 0f;
			this.debugCurrentRateHz = 0f;
			this.lastAmplitudeMeters = 0f;
			this.nextAllowedShakeStartTime = Time.time;
			if (this.myRig == null)
			{
				this.myRig = base.GetComponentInParent<VRRig>();
			}
			if (this._events == null)
			{
				this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
			}
			NetPlayer netPlayer = ((this.myRig != null) ? (this.myRig.creator ?? NetworkSystem.Instance.LocalPlayer) : NetworkSystem.Instance.LocalPlayer);
			if (netPlayer != null)
			{
				this._events.Init(netPlayer);
			}
			if (!this.subscribed && this._events.Activate != null)
			{
				this._events.Activate.reliable = true;
				this._events.Activate += this.OnShake;
				this.subscribed = true;
			}
		}

		private void OnDisable()
		{
			if (this._events != null)
			{
				this._events.Activate -= this.OnShake;
				this.subscribed = false;
				this._events.Dispose();
				this._events = null;
			}
		}

		private void Update()
		{
			if (this.myRig != null && !this.myRig.isLocal)
			{
				return;
			}
			if (this.speedTracker == null)
			{
				if (this.isShaking)
				{
					this.isShaking = false;
					if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
					{
						this._events.Activate.RaiseOthers(new object[] { this.isShaking });
					}
					UnityEvent shakeEndShared = this.ShakeEndShared;
					if (shakeEndShared != null)
					{
						shakeEndShared.Invoke();
					}
					UnityEvent shakeEndLocal = this.ShakeEndLocal;
					if (shakeEndLocal != null)
					{
						shakeEndLocal.Invoke();
					}
					this.nextAllowedShakeStartTime = Time.time + Mathf.Max(0f, this.startCooldownSeconds);
				}
				return;
			}
			Vector3 position = this.speedTracker.transform.position;
			float magnitude = (position - this.lastPosition).magnitude;
			if (magnitude > 0f)
			{
				this.pathSinceLastReversal += magnitude;
				this.debugCurrentHalfCycleDistance = this.pathSinceLastReversal;
			}
			Vector3 worldVelocity = this.speedTracker.GetWorldVelocity();
			float magnitude2 = worldVelocity.magnitude;
			Vector3 vector = ((worldVelocity.sqrMagnitude > 1E-06f) ? worldVelocity.normalized : this.lastVelocityDir);
			bool flag = false;
			if (this.hasLastDir)
			{
				if (Vector3.Angle(this.lastVelocityDir, vector) >= this.angleToleranceDeg && magnitude2 >= this.minSpeedForReversal)
				{
					float num = Time.time - this.lastReversalTime;
					if (num > 0.0005f)
					{
						this.EnqueueHalfCycle(num);
						this.lastAmplitudeMeters = this.pathSinceLastReversal;
						this.lastReversalTime = Time.time;
						this.pathSinceLastReversal = 0f;
						flag = true;
					}
				}
			}
			else
			{
				this.hasLastDir = true;
				this.lastVelocityDir = vector;
				this.lastReversalTime = Time.time;
			}
			this.lastVelocityDir = vector;
			this.lastPosition = position;
			float averageHalfCycleDuration = this.GetAverageHalfCycleDuration();
			float num2 = Time.time - this.lastReversalTime;
			float num3 = Mathf.Max((averageHalfCycleDuration > 1E-05f) ? averageHalfCycleDuration : float.PositiveInfinity, num2);
			float num4 = ((num3 < float.PositiveInfinity) ? (0.5f / num3) : 0f);
			this.debugCurrentRateHz = num4;
			bool flag2 = num4 >= this.shakeRateThreshold;
			bool flag3 = this.lastAmplitudeMeters >= this.shakeAmplitudeThreshold;
			if (!this.isShaking)
			{
				if (Time.time >= this.nextAllowedShakeStartTime && flag2 && flag3)
				{
					this.isShaking = true;
					if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
					{
						this._events.Activate.RaiseOthers(new object[] { this.isShaking });
					}
					UnityEvent shakeStartLocal = this.ShakeStartLocal;
					if (shakeStartLocal != null)
					{
						shakeStartLocal.Invoke();
					}
					UnityEvent shakeStartShared = this.ShakeStartShared;
					if (shakeStartShared != null)
					{
						shakeStartShared.Invoke();
					}
				}
			}
			else
			{
				float num5 = ((this.shakeRateThreshold > 1E-05f) ? (0.5f / this.shakeRateThreshold) : float.PositiveInfinity);
				float num6 = 1f * num5;
				bool flag4 = Time.time - this.lastReversalTime > num6;
				if ((!flag2 && !flag) || flag4)
				{
					this.isShaking = false;
					if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
					{
						this._events.Activate.RaiseOthers(new object[] { this.isShaking });
					}
					UnityEvent shakeEndLocal2 = this.ShakeEndLocal;
					if (shakeEndLocal2 != null)
					{
						shakeEndLocal2.Invoke();
					}
					UnityEvent shakeEndShared2 = this.ShakeEndShared;
					if (shakeEndShared2 != null)
					{
						shakeEndShared2.Invoke();
					}
					this.nextAllowedShakeStartTime = Time.time + Mathf.Max(0f, this.startCooldownSeconds);
				}
			}
			if (this.useMaxes && this.isShaking)
			{
				bool flag5 = num4 >= this.maxShakeRate;
				bool flag6 = this.lastAmplitudeMeters >= this.maxShakeAmplitude;
				if (flag5 || flag6)
				{
					UnityEvent maxShake = this.MaxShake;
					if (maxShake != null)
					{
						maxShake.Invoke();
					}
				}
			}
			float num7 = 0f;
			if (this.isShaking)
			{
				float num8 = Mathf.Max(1E-05f, this.shakeAmplitudeThreshold);
				if (this.useMaxes && this.maxShakeAmplitude > num8)
				{
					num7 = Mathf.InverseLerp(num8, this.maxShakeAmplitude, this.lastAmplitudeMeters);
				}
				else
				{
					float num9 = Mathf.Max(num8, this.shakeAmplitudeThreshold * Mathf.Max(1f, this.softMaxMultiplier));
					num7 = Mathf.InverseLerp(num8, num9, this.lastAmplitudeMeters);
				}
			}
			this.ApplyStrength(num7);
		}

		private void EnqueueHalfCycle(float duration)
		{
			this.recentHalfCycleDurations.Enqueue(duration);
			while (this.recentHalfCycleDurations.Count > Mathf.Max(1, 1))
			{
				this.recentHalfCycleDurations.Dequeue();
			}
		}

		private float GetAverageHalfCycleDuration()
		{
			if (this.recentHalfCycleDurations.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			foreach (float num2 in this.recentHalfCycleDurations)
			{
				num += num2;
			}
			return num / (float)this.recentHalfCycleDurations.Count;
		}

		private void ApplyStrength(float strength01)
		{
			if (this.continuousProperties != null)
			{
				this.continuousProperties.ApplyAll(strength01);
			}
		}

		private void OnShake(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target || info.senderID != this.myRig.creator.ActorNumber)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "OnShake");
			if (!this.callLimiter.CheckCallTime(Time.time))
			{
				return;
			}
			if (args.Length != 1)
			{
				return;
			}
			object obj = args[0];
			if (!(obj is bool))
			{
				return;
			}
			bool flag = (bool)obj;
			if (flag)
			{
				UnityEvent shakeStartShared = this.ShakeStartShared;
				if (shakeStartShared == null)
				{
					return;
				}
				shakeStartShared.Invoke();
				return;
			}
			else
			{
				UnityEvent shakeEndShared = this.ShakeEndShared;
				if (shakeEndShared == null)
				{
					return;
				}
				shakeEndShared.Invoke();
				return;
			}
		}

		public bool IsSpawned { get; set; }

		public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

		public void OnSpawn(VRRig rig)
		{
			this.myRig = rig;
		}

		public void OnDespawn()
		{
		}

		[Header("Speed Source")]
		[Tooltip("Speed component provider")]
		[SerializeField]
		private SimpleSpeedTracker speedTracker;

		[Header("Settings")]
		[Tooltip("Minimum reversals-per-second required to consider motion a shake - Hz.")]
		[SerializeField]
		private float shakeRateThreshold = 1f;

		[Tooltip("Minimum distance traveled between direction reversals to count as a valid half-cycle.")]
		[SerializeField]
		private float shakeAmplitudeThreshold = 0.1f;

		[Tooltip("Minimum angle change (degrees) between consecutive lobes to register a reversal. Higher = stricter.")]
		[SerializeField]
		[Range(10f, 170f)]
		private float angleToleranceDeg = 120f;

		[Tooltip("Minimum speed required to accept a direction reversal, ignores tiny jitter near stop.")]
		[SerializeField]
		private float minSpeedForReversal = 0.2f;

		[Tooltip("After a shake ends, how long to wait before ShakeStartLocal can fire again")]
		[SerializeField]
		private float startCooldownSeconds = 0.2f;

		[SerializeField]
		private bool useMaxes;

		[Tooltip("If enabled, exceeding this rate is considered a max shake.")]
		[SerializeField]
		private float maxShakeRate = 6f;

		[Tooltip("If enabled, exceeding this amplitude per half cycle is considered a max shake.")]
		[SerializeField]
		private float maxShakeAmplitude = 0.3f;

		[Header("Continuous Output")]
		[SerializeField]
		private ContinuousPropertyArray continuousProperties;

		[Header("Advanced")]
		[Tooltip("When no hard max amplitude is defined, strength is mapped to Threshold × this multiplier.")]
		[SerializeField]
		private float softMaxMultiplier = 3f;

		[FormerlySerializedAs("ShakeStart")]
		[Header("Events")]
		public UnityEvent ShakeStartLocal;

		public UnityEvent ShakeStartShared;

		[FormerlySerializedAs("ShakeEnd")]
		public UnityEvent ShakeEndLocal;

		public UnityEvent ShakeEndShared;

		public UnityEvent MaxShake;

		[Header("Debug")]
		public bool isShaking;

		public float lastAmplitudeMeters;

		public float debugCurrentHalfCycleDistance;

		public float debugCurrentRateHz;

		private const int kFrequencyHistoryCount = 1;

		private const float kNoReversalGraceMultiplier = 1f;

		private readonly Queue<float> recentHalfCycleDurations = new Queue<float>();

		private Vector3 lastVelocityDir;

		private bool hasLastDir;

		private float lastReversalTime;

		private Vector3 lastPosition;

		private float pathSinceLastReversal;

		private float nextAllowedShakeStartTime;

		private const float kEpsilon = 1E-05f;

		private const float kTinyVelocitySqr = 1E-06f;

		private const float kMinHalfCycleDuration = 0.0005f;

		private const float kHalfPerCycle = 0.5f;

		private RubberDuckEvents _events;

		private CallLimiter callLimiter = new CallLimiter(10, 1f, 0.5f);

		private VRRig myRig;

		private bool subscribed;
	}
}
