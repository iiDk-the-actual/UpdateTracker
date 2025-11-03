using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

public class CosmeticTiltReactor : MonoBehaviour, IGorillaSliceableSimple
{
	private void Awake()
	{
		this.referenceDirection.Normalize();
		if (!this.useTransform && this.referenceDirection == Vector3.zero)
		{
			GTDev.LogError<string>("CosmeticTiltReactor " + base.gameObject.name + " referenceDirection cannot be 0 vector", null);
		}
		if (this.useTransform && this.referenceTransform == null)
		{
			GTDev.LogError<string>("CosmeticTiltReactor " + base.gameObject.name + " referenceTransform cannot be null", null);
		}
		this.hasContinuousProperties = this.continuousProperties != null && this.continuousProperties.Count > 0;
		this.calculateDot = this.hasContinuousProperties;
		using (List<CosmeticTiltReactor.TiltEvent>.Enumerator enumerator = this.events.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.comparisonMethod == CosmeticTiltReactor.TiltEvent.ComparisonMethod.DotProduct)
				{
					this.calculateDot = true;
				}
				else
				{
					this.calculateAngle = true;
				}
				if (this.calculateDot && this.calculateAngle)
				{
					break;
				}
			}
		}
		this._rig = base.GetComponentInParent<VRRig>();
		this.parentTransferable = base.GetComponentInParent<TransferrableObject>();
		if (this._rig == null && base.gameObject.GetComponentInParent<GTPlayer>() != null)
		{
			this._rig = GorillaTagger.Instance.offlineVRRig;
		}
		if (this._rig == null && !this.syncForAllPlayers)
		{
			GTDev.LogError<string>("CosmeticTiltReactor on " + base.gameObject.name + " set to not syncForAllPlayers and has no VR Rig parent. Events will not fire", null);
		}
		else if (this._rig != null)
		{
			this.isLocallyOwned = this._rig.isLocal;
		}
		if (this.parentTransferable == null && this.onlyWhileHeld)
		{
			GTDev.LogError<string>("CosmeticTiltReactor on " + base.gameObject.name + " set to OnlyWhileHeld but has no TransferrableObject parent. Events will not fire", null);
		}
	}

	public void OnEnable()
	{
		if (!this.syncForAllPlayers && !this.isLocallyOwned)
		{
			return;
		}
		if (this.useTransform && this.referenceTransform == null)
		{
			return;
		}
		Vector3 vector = (this.useTransform ? this.referenceTransform.up : this.referenceDirection);
		if (this.calculateAngle)
		{
			this.angle = Vector3.Angle(base.transform.up, vector);
		}
		if (this.calculateDot)
		{
			this.dotProduct = Vector3.Dot(base.transform.up, vector);
		}
		this.ResetEvents();
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		if (!this.syncForAllPlayers && !this.isLocallyOwned)
		{
			return;
		}
		if (this.useTransform && this.referenceTransform == null)
		{
			return;
		}
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		if (this.onlyWhileHeld)
		{
			bool flag = this.parentTransferable != null && this.parentTransferable.InHand();
			if (!flag && this.wasInHand)
			{
				this.ResetEvents();
			}
			this.wasInHand = flag;
			if (!flag)
			{
				return;
			}
		}
		Vector3 vector = (this.useTransform ? this.referenceTransform.up : this.referenceDirection);
		if (this.calculateAngle)
		{
			this.angle = Vector3.Angle(base.transform.up, vector);
		}
		if (this.calculateDot)
		{
			this.dotProduct = Vector3.Dot(base.transform.up, vector);
		}
		this.FireEvents();
		if (this.hasContinuousProperties)
		{
			this.continuousProperties.ApplyAll(this.dotProduct);
		}
	}

	private void ResetEvents()
	{
		if (this.events == null || this.events.Count <= 0)
		{
			return;
		}
		foreach (CosmeticTiltReactor.TiltEvent tiltEvent in this.events)
		{
			switch (tiltEvent.tiltEventType)
			{
			case CosmeticTiltReactor.TiltEvent.TiltEventType.LessThanThreshold:
				tiltEvent.wasGreater = true;
				break;
			case CosmeticTiltReactor.TiltEvent.TiltEventType.GreaterThanThreshold:
				tiltEvent.wasGreater = false;
				break;
			case CosmeticTiltReactor.TiltEvent.TiltEventType.LessThanThresholdForDuration:
				tiltEvent.wasGreater = true;
				tiltEvent.hasFired = false;
				break;
			case CosmeticTiltReactor.TiltEvent.TiltEventType.GreaterThanThresholdForDuration:
				tiltEvent.wasGreater = false;
				tiltEvent.hasFired = false;
				break;
			}
			tiltEvent.thresholdCrossTime = double.MinValue;
		}
	}

	private void FireEvents()
	{
		if (this.events == null || this.events.Count <= 0)
		{
			return;
		}
		foreach (CosmeticTiltReactor.TiltEvent tiltEvent in this.events)
		{
			bool flag = ((tiltEvent.comparisonMethod == CosmeticTiltReactor.TiltEvent.ComparisonMethod.Angle) ? (this.angle > tiltEvent.angleThreshold) : (this.dotProduct > tiltEvent.dotThreshold));
			CosmeticTiltReactor.TiltEvent.TiltEventType tiltEventType = tiltEvent.tiltEventType;
			if (tiltEventType == CosmeticTiltReactor.TiltEvent.TiltEventType.LessThanThreshold || tiltEventType == CosmeticTiltReactor.TiltEvent.TiltEventType.GreaterThanThreshold)
			{
				if (flag != tiltEvent.wasGreater)
				{
					if (tiltEvent.tiltEventType == CosmeticTiltReactor.TiltEvent.TiltEventType.GreaterThanThreshold && flag)
					{
						if (tiltEvent.thresholdCrossTime + (double)tiltEvent.retriggerDelay <= Time.timeAsDouble)
						{
							tiltEvent.thresholdCrossTime = Time.timeAsDouble;
							tiltEvent.wasGreater = true;
							UnityEvent onTiltEvent = tiltEvent.OnTiltEvent;
							if (onTiltEvent != null)
							{
								onTiltEvent.Invoke();
							}
						}
					}
					else if (tiltEvent.tiltEventType == CosmeticTiltReactor.TiltEvent.TiltEventType.LessThanThreshold && !flag)
					{
						if (tiltEvent.thresholdCrossTime + (double)tiltEvent.retriggerDelay <= Time.timeAsDouble)
						{
							tiltEvent.thresholdCrossTime = Time.timeAsDouble;
							tiltEvent.wasGreater = false;
							UnityEvent onTiltEvent2 = tiltEvent.OnTiltEvent;
							if (onTiltEvent2 != null)
							{
								onTiltEvent2.Invoke();
							}
						}
					}
					else
					{
						tiltEvent.wasGreater = flag;
					}
				}
			}
			else
			{
				if (tiltEvent.tiltEventType == CosmeticTiltReactor.TiltEvent.TiltEventType.GreaterThanThresholdForDuration)
				{
					if (flag)
					{
						if (!tiltEvent.wasGreater)
						{
							tiltEvent.thresholdCrossTime = Time.timeAsDouble;
						}
						else if (!tiltEvent.hasFired && tiltEvent.thresholdCrossTime + (double)tiltEvent.duration <= Time.timeAsDouble)
						{
							UnityEvent onTiltEvent3 = tiltEvent.OnTiltEvent;
							if (onTiltEvent3 != null)
							{
								onTiltEvent3.Invoke();
							}
							tiltEvent.hasFired = true;
						}
					}
					else
					{
						tiltEvent.hasFired = false;
					}
				}
				if (tiltEvent.tiltEventType == CosmeticTiltReactor.TiltEvent.TiltEventType.LessThanThresholdForDuration)
				{
					if (!flag)
					{
						if (tiltEvent.wasGreater)
						{
							tiltEvent.thresholdCrossTime = Time.timeAsDouble;
						}
						else if (!tiltEvent.hasFired && tiltEvent.thresholdCrossTime + (double)tiltEvent.duration <= Time.timeAsDouble)
						{
							UnityEvent onTiltEvent4 = tiltEvent.OnTiltEvent;
							if (onTiltEvent4 != null)
							{
								onTiltEvent4.Invoke();
							}
							tiltEvent.hasFired = true;
						}
					}
					else
					{
						tiltEvent.hasFired = false;
					}
				}
				tiltEvent.wasGreater = flag;
			}
		}
	}

	[SerializeField]
	private bool useTransform;

	[Tooltip("Direction to which this transform's y is compared in world space")]
	[SerializeField]
	private Vector3 referenceDirection = Vector3.up;

	[Tooltip("compare referenceTransform's y to this transform's y")]
	[SerializeField]
	private Transform referenceTransform;

	[SerializeField]
	private List<CosmeticTiltReactor.TiltEvent> events;

	[Tooltip("input for continuous properties is the dot product of this transform's y and the reference direction")]
	[SerializeField]
	private ContinuousPropertyArray continuousProperties;

	[Tooltip("Should this script be run for all clients or just the owner")]
	[SerializeField]
	private bool syncForAllPlayers = true;

	[Tooltip("option to run only if this transferrable object is in the hand")]
	[SerializeField]
	private bool onlyWhileHeld;

	private VRRig _rig;

	private TransferrableObject parentTransferable;

	private bool isLocallyOwned;

	private bool hasContinuousProperties;

	private float angle;

	private float dotProduct;

	private bool calculateAngle;

	private bool calculateDot;

	private bool wasInHand;

	[Serializable]
	public class TiltEvent
	{
		public TiltEvent()
		{
			this.tiltEventType = CosmeticTiltReactor.TiltEvent.TiltEventType.LessThanThreshold;
			this.comparisonMethod = CosmeticTiltReactor.TiltEvent.ComparisonMethod.DotProduct;
			this.angleThreshold = 15f;
			this.retriggerDelay = 0f;
			this.duration = 0.5f;
		}

		public CosmeticTiltReactor.TiltEvent.ComparisonMethod comparisonMethod;

		public CosmeticTiltReactor.TiltEvent.TiltEventType tiltEventType;

		[Range(0f, 180f)]
		[Tooltip("Angle in degrees from the reference direction")]
		public float angleThreshold;

		[Range(-1f, 1f)]
		[Tooltip("Dot product compared to the reference direction")]
		public float dotThreshold;

		[Tooltip("Minimum time between events firing")]
		public float retriggerDelay;

		[Tooltip("Amount of time the angle or dot product should be less/greater than the threshold before firing an event")]
		public float duration;

		public UnityEvent OnTiltEvent;

		[NonSerialized]
		public bool wasGreater;

		[NonSerialized]
		public bool hasFired;

		[NonSerialized]
		public double thresholdCrossTime = double.MinValue;

		public enum ComparisonMethod
		{
			DotProduct,
			Angle
		}

		public enum TiltEventType
		{
			LessThanThreshold,
			GreaterThanThreshold,
			LessThanThresholdForDuration,
			GreaterThanThresholdForDuration
		}
	}
}
