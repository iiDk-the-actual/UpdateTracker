using System;
using UnityEngine;

public class SpiderDangler : MonoBehaviour
{
	protected void Awake()
	{
		this.lineRenderer = base.GetComponent<LineRenderer>();
		Vector3 position = base.transform.position;
		float magnitude = (this.endTransform.position - position).magnitude;
		this.ropeSegLen = magnitude / 6f;
		this.ropeSegs = new SpiderDangler.RopeSegment[6];
		for (int i = 0; i < 6; i++)
		{
			this.ropeSegs[i] = new SpiderDangler.RopeSegment(position);
			position.y -= this.ropeSegLen;
		}
	}

	protected void FixedUpdate()
	{
		this.Simulate();
	}

	protected void LateUpdate()
	{
		this.DrawRope();
		Vector3 normalized = (this.ropeSegs[this.ropeSegs.Length - 2].pos - this.ropeSegs[this.ropeSegs.Length - 1].pos).normalized;
		this.endTransform.position = this.ropeSegs[this.ropeSegs.Length - 1].pos;
		this.endTransform.up = normalized;
		Vector4 vector = this.spinSpeeds * Time.time;
		vector = new Vector4(Mathf.Sin(vector.x), Mathf.Sin(vector.y), Mathf.Sin(vector.z), Mathf.Sin(vector.w));
		vector.Scale(this.spinScales);
		this.endTransform.Rotate(Vector3.up, vector.x + vector.y + vector.z + vector.w);
	}

	private void Simulate()
	{
		this.ropeSegLenScaled = this.ropeSegLen * base.transform.lossyScale.x;
		Vector3 vector = new Vector3(0f, -0.5f, 0f) * Time.fixedDeltaTime;
		for (int i = 1; i < 6; i++)
		{
			Vector3 vector2 = this.ropeSegs[i].pos - this.ropeSegs[i].posOld;
			this.ropeSegs[i].posOld = this.ropeSegs[i].pos;
			SpiderDangler.RopeSegment[] array = this.ropeSegs;
			int num = i;
			array[num].pos = array[num].pos + vector2 * 0.95f;
			SpiderDangler.RopeSegment[] array2 = this.ropeSegs;
			int num2 = i;
			array2[num2].pos = array2[num2].pos + vector;
		}
		for (int j = 0; j < 8; j++)
		{
			this.ApplyConstraint();
		}
	}

	private void ApplyConstraint()
	{
		this.ropeSegs[0].pos = base.transform.position;
		this.ApplyConstraintSegment(ref this.ropeSegs[0], ref this.ropeSegs[1], 0f, 1f);
		for (int i = 1; i < 5; i++)
		{
			this.ApplyConstraintSegment(ref this.ropeSegs[i], ref this.ropeSegs[i + 1], 0.5f, 0.5f);
		}
	}

	private void ApplyConstraintSegment(ref SpiderDangler.RopeSegment segA, ref SpiderDangler.RopeSegment segB, float dampenA, float dampenB)
	{
		float num = (segA.pos - segB.pos).magnitude - this.ropeSegLenScaled;
		Vector3 vector = (segA.pos - segB.pos).normalized * num;
		segA.pos -= vector * dampenA;
		segB.pos += vector * dampenB;
	}

	private void DrawRope()
	{
		Vector3[] array = new Vector3[6];
		for (int i = 0; i < 6; i++)
		{
			array[i] = this.ropeSegs[i].pos;
		}
		this.lineRenderer.positionCount = array.Length;
		this.lineRenderer.SetPositions(array);
	}

	public Transform endTransform;

	public Vector4 spinSpeeds = new Vector4(0.1f, 0.2f, 0.3f, 0.4f);

	public Vector4 spinScales = new Vector4(180f, 90f, 120f, 180f);

	private LineRenderer lineRenderer;

	private SpiderDangler.RopeSegment[] ropeSegs;

	private float ropeSegLen;

	private float ropeSegLenScaled;

	private const int kSegmentCount = 6;

	private const float kVelocityDamper = 0.95f;

	private const int kConstraintCalculationIterations = 8;

	public struct RopeSegment
	{
		public RopeSegment(Vector3 pos)
		{
			this.pos = pos;
			this.posOld = pos;
		}

		public Vector3 pos;

		public Vector3 posOld;
	}
}
