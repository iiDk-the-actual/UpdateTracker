using System;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
	public Vector3 GetPoint(float t)
	{
		Vector3 vector = ((this.points.Length == 3) ? Bezier.GetPoint(this.points[0], this.points[1], this.points[2], t) : Bezier.GetPoint(this.points[0], this.points[1], this.points[2], this.points[3], t));
		if (!this.referenceTransform)
		{
			return vector;
		}
		return this.referenceTransform.TransformPoint(vector);
	}

	public Vector3 GetVelocity(float t)
	{
		Vector3 vector = ((this.points.Length == 3) ? Bezier.GetFirstDerivative(this.points[0], this.points[1], this.points[2], t) : Bezier.GetFirstDerivative(this.points[0], this.points[1], this.points[2], this.points[3], t));
		if (!this.referenceTransform)
		{
			return vector;
		}
		return this.referenceTransform.TransformPoint(vector) - this.referenceTransform.position;
	}

	public Vector3 GetDirection(float t)
	{
		return this.GetVelocity(t).normalized;
	}

	public void Reset()
	{
		this.referenceTransform = base.transform;
		this.points = new Vector3[]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
	}

	public Transform referenceTransform;

	public Vector3[] points;
}
