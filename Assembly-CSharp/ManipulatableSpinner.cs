using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(BezierSpline))]
public class ManipulatableSpinner : ManipulatableObject
{
	public float angle { get; private set; }

	private void Awake()
	{
		this.spline = base.GetComponent<BezierSpline>();
	}

	protected override void OnStartManipulation(GameObject grabbingHand)
	{
		Vector3 position = grabbingHand.transform.position;
		float num = this.FindPositionOnSpline(position);
		this.previousHandT = num;
	}

	protected override void OnStopManipulation(GameObject releasingHand, Vector3 releaseVelocity)
	{
	}

	protected override bool ShouldHandDetach(GameObject hand)
	{
		if (!this.spline.Loop && (this.currentHandT >= 0.99f || this.currentHandT <= 0.01f))
		{
			return true;
		}
		Vector3 position = hand.transform.position;
		Vector3 point = this.spline.GetPoint(this.currentHandT);
		return Vector3.SqrMagnitude(position - point) > this.breakDistance * this.breakDistance;
	}

	protected override void OnHeldUpdate(GameObject hand)
	{
		float angle = this.angle;
		Vector3 position = hand.transform.position;
		this.currentHandT = this.FindPositionOnSpline(position);
		float num = this.currentHandT - this.previousHandT;
		if (this.spline.Loop)
		{
			if (num > 0.5f)
			{
				num -= 1f;
			}
			else if (num < -0.5f)
			{
				num += 1f;
			}
		}
		this.angle += num;
		this.previousHandT = this.currentHandT;
		if (this.applyReleaseVelocity && this.currentHandT <= 0.99f && this.currentHandT >= 0.01f)
		{
			this.tVelocity = (this.angle - angle) / Time.deltaTime;
		}
	}

	protected override void OnReleasedUpdate()
	{
		if (this.tVelocity != 0f)
		{
			this.angle += this.tVelocity * Time.deltaTime;
			if (Mathf.Abs(this.tVelocity) < this.lowSpeedThreshold)
			{
				this.tVelocity *= 1f - this.lowSpeedDrag * Time.deltaTime;
				return;
			}
			this.tVelocity *= 1f - this.releaseDrag * Time.deltaTime;
		}
	}

	private float FindPositionOnSpline(Vector3 grabPoint)
	{
		int i = 0;
		int num = 200;
		float num2 = 0.001f;
		float num3 = 1f / (float)num;
		float3 @float = base.transform.InverseTransformPoint(grabPoint);
		float num4 = 0f;
		float num5 = float.PositiveInfinity;
		while (i < num)
		{
			float num6 = math.distancesq(this.spline.GetPointLocal(num2), @float);
			if (num6 < num5)
			{
				num5 = num6;
				num4 = num2;
			}
			num2 += num3;
			i++;
		}
		return num4;
	}

	public void SetAngle(float newAngle)
	{
		this.angle = newAngle;
	}

	public void SetVelocity(float newVelocity)
	{
		this.tVelocity = newVelocity;
	}

	public float breakDistance = 0.2f;

	public bool applyReleaseVelocity;

	public float releaseDrag = 1f;

	public float lowSpeedThreshold = 0.12f;

	public float lowSpeedDrag = 3f;

	private BezierSpline spline;

	private float previousHandT;

	private float currentHandT;

	private float tVelocity;
}
