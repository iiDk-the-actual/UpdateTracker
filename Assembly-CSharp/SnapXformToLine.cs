using System;
using UnityEngine;
using UnityEngine.Events;

public class SnapXformToLine : MonoBehaviour
{
	public Vector3 linePoint
	{
		get
		{
			return this._closest;
		}
	}

	public float linearDistance
	{
		get
		{
			return this._linear;
		}
	}

	public void SnapTarget(bool applyToXform = true)
	{
		this.Snap(this.target, true);
	}

	public void SnapTarget(Vector3 point)
	{
		if (this.target)
		{
			this.target.position = this.GetSnappedPoint(this.target.position);
		}
	}

	public void SnapTargetLinear(float t)
	{
		if (this.target && this.from && this.to)
		{
			this.target.position = Vector3.Lerp(this.from.position, this.to.position, t);
		}
	}

	public Vector3 GetSnappedPoint(Transform t)
	{
		return this.GetSnappedPoint(t.position);
	}

	public Vector3 GetSnappedPoint(Vector3 point)
	{
		if (!this.apply)
		{
			return point;
		}
		if (!this.from || !this.to)
		{
			return point;
		}
		return SnapXformToLine.GetClosestPointOnLine(point, this.from.position, this.to.position);
	}

	public void Snap(Transform xform, bool applyToXform = true)
	{
		if (!this.apply || !xform || !this.from || !this.to)
		{
			return;
		}
		Vector3 position = xform.position;
		Vector3 position2 = this.from.position;
		Vector3 position3 = this.to.position;
		Vector3 closestPointOnLine = SnapXformToLine.GetClosestPointOnLine(position, position2, position3);
		float num = Vector3.Distance(position2, position3);
		float num2 = Vector3.Distance(closestPointOnLine, position2);
		Vector3 closest = this._closest;
		Vector3 vector = closestPointOnLine;
		float linear = this._linear;
		float num3 = (Mathf.Approximately(num, 0f) ? 0f : (num2 / (num + Mathf.Epsilon)));
		this._closest = vector;
		this._linear = num3;
		if (this.output)
		{
			IRangedVariable<float> asT = this.output.AsT;
			asT.Set(asT.Min + this._linear * asT.Range);
		}
		if (applyToXform)
		{
			xform.position = this._closest;
			if (!Mathf.Approximately(closest.x, vector.x) || !Mathf.Approximately(closest.y, vector.y) || !Mathf.Approximately(closest.z, vector.z))
			{
				UnityEvent<Vector3> unityEvent = this.onPositionChanged;
				if (unityEvent != null)
				{
					unityEvent.Invoke(this._closest);
				}
			}
			if (!Mathf.Approximately(linear, num3))
			{
				UnityEvent<float> unityEvent2 = this.onLinearDistanceChanged;
				if (unityEvent2 != null)
				{
					unityEvent2.Invoke(this._linear);
				}
			}
			if (this.snapOrientation)
			{
				xform.forward = (position3 - position2).normalized;
				xform.up = Vector3.Lerp(this.from.up.normalized, this.to.up.normalized, this._linear);
			}
		}
	}

	private void OnDisable()
	{
		if (this.resetOnDisable)
		{
			this.SnapTargetLinear(0f);
		}
	}

	private void LateUpdate()
	{
		this.SnapTarget(true);
	}

	private static Vector3 GetClosestPointOnLine(Vector3 p, Vector3 a, Vector3 b)
	{
		Vector3 vector = p - a;
		Vector3 vector2 = b - a;
		float sqrMagnitude = vector2.sqrMagnitude;
		float num = Mathf.Clamp(Vector3.Dot(vector, vector2) / sqrMagnitude, 0f, 1f);
		return a + vector2 * num;
	}

	public bool apply = true;

	public bool snapOrientation = true;

	public bool resetOnDisable = true;

	[Space]
	public Transform target;

	[Space]
	public Transform from;

	public Transform to;

	private Vector3 _closest;

	private float _linear;

	public Ref<IRangedVariable<float>> output;

	public UnityEvent<float> onLinearDistanceChanged;

	public UnityEvent<Vector3> onPositionChanged;
}
