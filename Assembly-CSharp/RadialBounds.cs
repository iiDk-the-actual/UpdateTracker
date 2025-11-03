using System;
using UnityEngine;
using UnityEngine.Events;

public class RadialBounds : MonoBehaviour
{
	public Vector3 localCenter
	{
		get
		{
			return this._localCenter;
		}
		set
		{
			this._localCenter = value;
		}
	}

	public float localRadius
	{
		get
		{
			return this._localRadius;
		}
		set
		{
			this._localRadius = value;
		}
	}

	public Vector3 center
	{
		get
		{
			return base.transform.TransformPoint(this._localCenter);
		}
	}

	public float radius
	{
		get
		{
			return MathUtils.GetScaledRadius(this._localRadius, base.transform.lossyScale);
		}
	}

	[SerializeField]
	private Vector3 _localCenter;

	[SerializeField]
	private float _localRadius = 1f;

	[Space]
	public UnityEvent<RadialBounds> onOverlapEnter;

	public UnityEvent<RadialBounds> onOverlapExit;

	public UnityEvent<RadialBounds, float> onOverlapStay;
}
