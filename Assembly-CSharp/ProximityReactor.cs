using System;
using UnityEngine;
using UnityEngine.Events;

public class ProximityReactor : MonoBehaviour
{
	public float proximityRange
	{
		get
		{
			return this.proximityMax - this.proximityMin;
		}
	}

	public float distance
	{
		get
		{
			return this._distance;
		}
	}

	public float distanceLinear
	{
		get
		{
			return this._distanceLinear;
		}
	}

	public void SetRigFrom()
	{
		VRRig componentInParent = base.GetComponentInParent<VRRig>(true);
		if (componentInParent != null)
		{
			this.from = componentInParent.transform;
		}
	}

	public void SetRigTo()
	{
		VRRig componentInParent = base.GetComponentInParent<VRRig>(true);
		if (componentInParent != null)
		{
			this.to = componentInParent.transform;
		}
	}

	public void SetTransformFrom(Transform t)
	{
		this.from = t;
	}

	public void SetTransformTo(Transform t)
	{
		this.to = t;
	}

	private void Setup()
	{
		this._distance = 0f;
		this._distanceLinear = 0f;
	}

	private void OnEnable()
	{
		this.Setup();
	}

	private void Update()
	{
		if (!this.from || !this.to)
		{
			this._distance = 0f;
			this._distanceLinear = 0f;
			return;
		}
		Vector3 position = this.from.position;
		float magnitude = (this.to.position - position).magnitude;
		if (!this._distance.Approx(magnitude, 1E-06f))
		{
			UnityEvent<float> unityEvent = this.onProximityChanged;
			if (unityEvent != null)
			{
				unityEvent.Invoke(magnitude);
			}
		}
		this._distance = magnitude;
		float num = (this.proximityRange.Approx0(1E-06f) ? 0f : MathUtils.LinearUnclamped(magnitude, this.proximityMin, this.proximityMax, 0f, 1f));
		if (!this._distanceLinear.Approx(num, 1E-06f))
		{
			UnityEvent<float> unityEvent2 = this.onProximityChangedLinear;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke(num);
			}
		}
		this._distanceLinear = num;
		if (this._distanceLinear < 0f)
		{
			UnityEvent<float> unityEvent3 = this.onBelowMinProximity;
			if (unityEvent3 != null)
			{
				unityEvent3.Invoke(magnitude);
			}
		}
		if (this._distanceLinear > 1f)
		{
			UnityEvent<float> unityEvent4 = this.onAboveMaxProximity;
			if (unityEvent4 == null)
			{
				return;
			}
			unityEvent4.Invoke(magnitude);
		}
	}

	public Transform from;

	public Transform to;

	[Space]
	public float proximityMin;

	public float proximityMax = 1f;

	[Space]
	[NonSerialized]
	private float _distance;

	[NonSerialized]
	private float _distanceLinear;

	[Space]
	public UnityEvent<float> onProximityChanged;

	public UnityEvent<float> onProximityChangedLinear;

	[Space]
	public UnityEvent<float> onBelowMinProximity;

	public UnityEvent<float> onAboveMaxProximity;
}
