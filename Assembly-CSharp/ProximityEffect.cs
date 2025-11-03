using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

public class ProximityEffect : MonoBehaviour, ITickSystemTick
{
	private void Awake()
	{
		this.rig = base.GetComponentInParent<VRRig>();
		this.enableVisualization = false;
		if (this.visualizer)
		{
			Object.Destroy(this.visualizer);
		}
	}

	public void AddReceiver(IProximityEffectReceiver receiver)
	{
		if (this.receivers == null)
		{
			this.receivers = new List<IProximityEffectReceiver> { receiver };
			return;
		}
		if (!this.receivers.Contains(receiver))
		{
			this.receivers.Add(receiver);
		}
	}

	public void RemoveReceiver(IProximityEffectReceiver receiver)
	{
		this.receivers.Remove(receiver);
	}

	private void StartCalculating()
	{
		this.centerTransform.position = (this.leftTransform.position + this.rightTransform.position) / 2f;
		TickSystem<object>.AddTickCallback(this);
	}

	private void StopCalculating()
	{
		ProximityEffect.ProximityEvent[] array = this.events;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ResetAllEvents();
		}
		ContinuousPropertyArray continuousPropertyArray = this.continuousProperties;
		if (continuousPropertyArray != null)
		{
			continuousPropertyArray.ApplyAll(0f);
		}
		UnityEvent<float> unityEvent = this.onScoreCalculated;
		if (unityEvent != null)
		{
			unityEvent.Invoke(0f);
		}
		TickSystem<object>.RemoveTickCallback(this);
	}

	private void OnEnable()
	{
		if (this.triggersToActivate == 0)
		{
			this.StartCalculating();
		}
	}

	private void OnDisable()
	{
		if (this.triggersToActivate == 0)
		{
			this.StopCalculating();
		}
	}

	public void AddTrigger()
	{
		if (this.numTriggers < this.triggersToActivate)
		{
			this.numTriggers++;
			if (this.numTriggers == this.triggersToActivate)
			{
				this.StartCalculating();
			}
		}
	}

	public void RemoveTrigger()
	{
		if (this.numTriggers > 0)
		{
			if (this.numTriggers == this.triggersToActivate)
			{
				this.StopCalculating();
			}
			this.numTriggers--;
		}
	}

	private void CalculateProximityScores()
	{
		float num;
		float num2;
		float num3;
		Vector3 vector;
		this.CalculateProximityScores(true, out num, out num2, out num3, out vector);
	}

	private void CalculateProximityScores(out float distance, out float alignment, out float parallel, out Vector3 midpoint)
	{
		this.CalculateProximityScores(false, out distance, out alignment, out parallel, out midpoint);
	}

	private void CalculateProximityScores(bool drawGizmos, out float distance, out float alignment, out float parallel, out Vector3 midpoint)
	{
		float num = ((this.rig != null) ? this.rig.scaleFactor : 1f);
		Vector3 position = this.leftTransform.position;
		Vector3 position2 = this.rightTransform.position;
		Vector3 forward = this.leftTransform.forward;
		Vector3 forward2 = this.rightTransform.forward;
		Vector3 vector = (position2 - position) / num;
		float magnitude = vector.magnitude;
		Vector3 vector2 = vector / magnitude;
		distance = this.scoreCurves.distanceModifierCurve.Evaluate(magnitude);
		alignment = this.scoreCurves.alignmentModifierCurve.Evaluate(-Vector3.Dot(forward, forward2));
		parallel = this.scoreCurves.parallelModifierCurve.Evaluate((Vector3.Dot(forward, vector2) + Vector3.Dot(forward2, -vector2)) / 2f);
		midpoint = position + 0.5f * vector;
	}

	private void MoveTransform(Transform target, float score, Vector3 midpoint)
	{
		Vector3 vector;
		Quaternion quaternion;
		target.GetPositionAndRotation(out vector, out quaternion);
		Vector3 vector2 = Vector3.Lerp(vector, midpoint, ProximityEffect.<MoveTransform>g__ExpT|40_0(this.positionCTLerpSpeed));
		if (this.rotateCT)
		{
			Vector3 vector3 = (vector2 - vector) / Time.deltaTime;
			if (vector3 != Vector3.zero)
			{
				Quaternion quaternion2 = Quaternion.LookRotation(vector3);
				Quaternion quaternion3 = Quaternion.LookRotation(vector2 - this.rig.syncPos);
				Quaternion quaternion4 = Quaternion.Slerp(quaternion, Quaternion.Slerp(quaternion3, quaternion2, vector3.magnitude), ProximityEffect.<MoveTransform>g__ExpT|40_0(this.rotationCTLerpSpeed));
				target.SetPositionAndRotation(vector2, quaternion4);
			}
		}
		else
		{
			target.position = vector2;
		}
		if (this.scaleCT)
		{
			target.localScale = Vector3.Lerp(target.localScale, score * this.scaleCTMult * Vector3.one, ProximityEffect.<MoveTransform>g__ExpT|40_0(this.scaleCTLerpSpeed));
		}
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		float num;
		float num2;
		float num3;
		Vector3 vector;
		this.CalculateProximityScores(out num, out num2, out num3, out vector);
		if (this.receivers != null)
		{
			for (int i = 0; i < this.receivers.Count; i++)
			{
				this.receivers[i].OnProximityCalculated(num, num2, num3);
			}
		}
		float num4 = num * num2 * num3;
		ContinuousPropertyArray continuousPropertyArray = this.continuousProperties;
		if (continuousPropertyArray != null)
		{
			continuousPropertyArray.ApplyAll(num4);
		}
		UnityEvent<float> unityEvent = this.onScoreCalculated;
		if (unityEvent != null)
		{
			unityEvent.Invoke(num4);
		}
		if (this.centerTransform != null)
		{
			this.MoveTransform(this.centerTransform, num4, vector);
		}
		this.anyAboveThreshold = false;
		foreach (ProximityEffect.ProximityEvent proximityEvent in this.events)
		{
			this.anyAboveThreshold = proximityEvent.Evaluate(num4) || this.anyAboveThreshold;
		}
	}

	[CompilerGenerated]
	internal static float <MoveTransform>g__ExpT|40_0(float speed)
	{
		return 1f - Mathf.Exp(-speed * Time.deltaTime);
	}

	[SerializeField]
	private Transform leftTransform;

	[SerializeField]
	private Transform rightTransform;

	[SerializeField]
	[Tooltip("How many times AddTrigger() needs to be called before the events are allowed to be invoked. Used for pausing events until certain actions are performed (like squeezing the triggers of both controllers).")]
	private int triggersToActivate;

	[Space]
	[SerializeField]
	[Tooltip("The transform that moves to follow the midpoint of the left and right transforms.")]
	private Transform centerTransform;

	private const string SHOW_CONDITION = "@centerTransform != null";

	[SerializeField]
	private float positionCTLerpSpeed = 10f;

	[SerializeField]
	private bool rotateCT;

	private const string SHOW_ROTATE_CONDITION = "@centerTransform != null && rotateCT";

	[SerializeField]
	private float rotationCTLerpSpeed = 10f;

	[SerializeField]
	private bool scaleCT;

	private const string SHOW_SCALE_CONDITION = "@centerTransform != null && scaleCT";

	[SerializeField]
	private float scaleCTLerpSpeed = 10f;

	[SerializeField]
	private float scaleCTMult = 1f;

	[Space]
	[SerializeField]
	[Tooltip("The curves that get evaluated to determine the alignment score. They get multiplied together, so their Y values should all range from 0-1. The result is compared against the thresholds of the ProximityEvents.")]
	private ProximityEffectScoreCurvesSO scoreCurves;

	[Space]
	[SerializeField]
	private ContinuousPropertyArray continuousProperties;

	[SerializeField]
	private UnityEvent<float> onScoreCalculated;

	[SerializeField]
	private ProximityEffect.ProximityEvent[] events;

	[Header("Editor Only")]
	[SerializeField]
	private Vector3 defaultLeftHandLocalPosition = new Vector3(-0.0568f, 0.04311f, 0.00249f);

	[SerializeField]
	private Vector3 defaultLeftHandLocalEuler = new Vector3(173.176f, 80.201f, 3.615f);

	[Header("Visualization is currently NOT WORKING IN PLAY MODE due to tick optimization")]
	[SerializeField]
	private bool enableVisualization = true;

	[SerializeField]
	private Material visualizationMaterial;

	[SerializeField]
	[Range(0f, 1f)]
	private float visualizationLineThickness = 0.01f;

	[SerializeField]
	[HideInInspector]
	private LineRenderer visualizer;

	private List<IProximityEffectReceiver> receivers;

	private VRRig rig;

	private bool anyAboveThreshold;

	private int numTriggers;

	[Serializable]
	private class ProximityEvent
	{
		public bool Evaluate(float score)
		{
			if (score >= this.highThreshold)
			{
				if (!this.wasAboveThreshold && Time.time - this.lastThresholdTime >= this.highThresholdBufferTime)
				{
					UnityEvent unityEvent = this.onThresholdHigh;
					if (unityEvent != null)
					{
						unityEvent.Invoke();
					}
					this.wasAboveThreshold = true;
					this.wasBelowThreshold = false;
				}
				if (this.wasAboveThreshold)
				{
					this.lastThresholdTime = Time.time;
				}
				return true;
			}
			if (score < this.lowThreshold)
			{
				if (!this.wasBelowThreshold && Time.time - this.lastThresholdTime >= this.lowThresholdBufferTime)
				{
					UnityEvent unityEvent2 = this.onThresholdLow;
					if (unityEvent2 != null)
					{
						unityEvent2.Invoke();
					}
					this.wasAboveThreshold = false;
					this.wasBelowThreshold = true;
				}
				if (this.wasBelowThreshold)
				{
					this.lastThresholdTime = Time.time;
				}
			}
			return false;
		}

		public void ResetAllEvents()
		{
			this.wasAboveThreshold = false;
			this.wasBelowThreshold = true;
		}

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("High-threshold events will only fire if the alignment score is above this value.")]
		private float highThreshold = 0.5f;

		[SerializeField]
		[Tooltip("Wait this many seconds before activating the high-threshold events.")]
		private float highThresholdBufferTime;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Low-threshold events will only fire if the alignment score is below this value.")]
		private float lowThreshold = 0.3f;

		[SerializeField]
		[Tooltip("Wait this many seconds before activating the low-threshold events.")]
		private float lowThresholdBufferTime;

		public UnityEvent onThresholdHigh;

		public UnityEvent onThresholdLow;

		private bool wasAboveThreshold;

		private bool wasBelowThreshold = true;

		private float lastThresholdTime = -100f;
	}
}
