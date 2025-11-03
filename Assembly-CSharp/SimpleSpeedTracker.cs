using System;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class SimpleSpeedTracker : MonoBehaviour, IGorillaSliceableSimple
{
	public void OnEnable()
	{
		if (this.target == null)
		{
			this.target = base.transform;
		}
		this.lastPos = this.target.position;
		this.lastSliceTime = Time.time;
		this.lastVelocity = Vector3.zero;
		this.lastRawSpeed = 0f;
		this.lastSpeed = 0f;
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		float num = Mathf.Max(1E-06f, Time.time - this.lastSliceTime);
		Vector3 position = this.target.position;
		Vector3 vector = (position - this.lastPos) / num;
		float magnitude = vector.magnitude;
		this.lastSpeed = (this.useRawSpeed ? magnitude : Mathf.Lerp(this.lastSpeed, magnitude, 1f - Mathf.Exp(-this.responsiveness * num)));
		float num2 = this.postprocessCurve.Evaluate(this.lastSpeed);
		this.continuousProperties.ApplyAll(num2);
		float num3 = (this.useRawSpeed ? magnitude : num2);
		UnityEvent<float> unityEvent = this.onSpeedUpdated;
		if (unityEvent != null)
		{
			unityEvent.Invoke(num3);
		}
		this.debugCurrentSpeed = num3;
		bool flag = num3 >= this.eventThreshold;
		if (flag && !this.wasAboveThreshold)
		{
			UnityEvent unityEvent2 = this.onSpeedAboveThreshold;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke();
			}
		}
		else if (!flag && this.wasAboveThreshold)
		{
			UnityEvent unityEvent3 = this.onSpeedBelowThreshold;
			if (unityEvent3 != null)
			{
				unityEvent3.Invoke();
			}
		}
		this.wasAboveThreshold = flag;
		this.lastVelocity = vector;
		this.lastRawSpeed = magnitude;
		this.lastPos = position;
		this.lastSliceTime = Time.time;
	}

	public float GetPostProcessSpeed()
	{
		return this.postprocessCurve.Evaluate(this.lastSpeed);
	}

	public float GetRawSpeed()
	{
		return this.lastRawSpeed;
	}

	public Vector3 GetWorldVelocity()
	{
		return this.lastVelocity;
	}

	public Vector3 GetLocalVelocity()
	{
		if (this.useWorldAxes)
		{
			return this.lastVelocity;
		}
		if (this.target != null)
		{
			return this.target.InverseTransformDirection(this.lastVelocity);
		}
		return base.transform.InverseTransformDirection(this.lastVelocity);
	}

	public float GetSignedSpeedAlongForward(Transform reference)
	{
		if (reference == null)
		{
			return 0f;
		}
		return Vector3.Dot(this.lastVelocity, reference.forward);
	}

	public float GetSignedSpeedX()
	{
		return Vector3.Dot(this.lastVelocity, this.ResolveAxisRight());
	}

	public float GetSignedSpeedY()
	{
		return Vector3.Dot(this.lastVelocity, this.ResolveAxisUp());
	}

	public float GetSignedSpeedZ()
	{
		return Vector3.Dot(this.lastVelocity, this.ResolveAxisForward());
	}

	public Vector3 GetVelocityInAxisSpace()
	{
		Vector3 vector = this.ResolveAxisRight();
		Vector3 vector2 = this.ResolveAxisUp();
		Vector3 vector3 = this.ResolveAxisForward();
		return new Vector3(Vector3.Dot(this.lastVelocity, vector), Vector3.Dot(this.lastVelocity, vector2), Vector3.Dot(this.lastVelocity, vector3));
	}

	private Vector3 ResolveAxisRight()
	{
		if (this.useWorldAxes)
		{
			if (this.worldSpace != null)
			{
				return this.worldSpace.right;
			}
			return Vector3.right;
		}
		else
		{
			if (!(this.target != null))
			{
				return base.transform.right;
			}
			return this.target.right;
		}
	}

	private Vector3 ResolveAxisUp()
	{
		if (this.useWorldAxes)
		{
			if (this.worldSpace != null)
			{
				return this.worldSpace.up;
			}
			return Vector3.up;
		}
		else
		{
			if (!(this.target != null))
			{
				return base.transform.up;
			}
			return this.target.up;
		}
	}

	private Vector3 ResolveAxisForward()
	{
		if (this.useWorldAxes)
		{
			if (this.worldSpace != null)
			{
				return this.worldSpace.forward;
			}
			return Vector3.forward;
		}
		else
		{
			if (!(this.target != null))
			{
				return base.transform.forward;
			}
			return this.target.forward;
		}
	}

	[Header("Settings")]
	[Tooltip("Transform whose movement speed is tracked. If left empty, uses this object’s transform.")]
	[SerializeField]
	private Transform target;

	[Tooltip("If enabled, speed and direction calculations use world (global) space, otherwise local space.\nUse Local Space when you want speed relative to the object’s facing direction (e.g., how fast a sword swings forward)")]
	[SerializeField]
	private bool useWorldAxes;

	[Tooltip("Optional transform defining a custom world reference.\nIf set, that transform’s Right/Up/Forward axes are treated as world axes.\nIf left empty, Unity’s global world axes are used.")]
	[SerializeField]
	private Transform worldSpace;

	[Tooltip("If true, uses raw instantaneous speed without smoothing.\nIf false, smooths speed using the Responsiveness setting below.")]
	[SerializeField]
	private bool useRawSpeed;

	[SerializeField]
	private float responsiveness = 10f;

	[SerializeField]
	private AnimationCurve postprocessCurve = AnimationCurve.Linear(0f, 0f, 10f, 10f);

	[Header("Property Output")]
	[SerializeField]
	private ContinuousPropertyArray continuousProperties;

	[Header("Events")]
	[Tooltip("Speed threshold used to trigger events.")]
	[SerializeField]
	private float eventThreshold = 1f;

	public UnityEvent<float> onSpeedUpdated;

	public UnityEvent onSpeedAboveThreshold;

	public UnityEvent onSpeedBelowThreshold;

	[Header("Debug")]
	[Tooltip("Current displayed speed value (raw or smoothed).")]
	public float debugCurrentSpeed;

	private float lastSpeed;

	private float lastRawSpeed;

	private Vector3 lastVelocity;

	private Vector3 lastPos;

	private float lastSliceTime;

	private bool wasAboveThreshold;
}
