using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HangingClaw : MonoBehaviourPostTick
{
	public new bool PostTickRunning { get; set; }

	protected void Awake()
	{
		this.lineRenderer = base.GetComponent<LineRenderer>();
		Vector3 position = base.transform.position;
		this.segmentCount = 4;
		float magnitude = (this.endTransform.position - position).magnitude;
		this.segmentCount = Mathf.Max(2, this.segmentCount);
		this.baseSegLen = magnitude / (float)this.segmentCount;
		this.ropeSegs = new HangingClaw.RopeSegment[this.segmentCount];
		this.invMass = new float[this.segmentCount];
		for (int i = 0; i < this.segmentCount; i++)
		{
			Vector3 vector = Vector3.Lerp(position, this.endTransform.position, (float)i / (float)(this.segmentCount - 1));
			this.ropeSegs[i] = new HangingClaw.RopeSegment(vector);
		}
		this.invMass[0] = 0f;
		for (int j = 1; j < this.segmentCount - 1; j++)
		{
			this.invMass[j] = 1f / Mathf.Max(0.0001f, this.segmentMassKg);
		}
		this.invMass[this.segmentCount - 1] = 1f / Mathf.Max(0.0001f, this.endMassKg);
	}

	public override void PostTick()
	{
		this.Simulate();
		this.DrawRope();
		int num = this.segmentCount - 1;
		int num2 = this.segmentCount;
		this.endTransform.position = this.ropeSegs[num].pos;
	}

	private void Simulate()
	{
		float num = this.baseSegLen;
		this.targetSegLenScaled = num * (1f + this.slackFraction);
		float num2 = 0.01111f;
		float num3 = Time.time * 0.5f;
		Vector3 vector = this.gravity * num2 * num2;
		Vector3 vector2 = base.transform.position + new Vector3(0f, 0.012f * Mathf.Sin(num3), 0.02f * Mathf.Cos(num3));
		for (int i = 1; i < this.segmentCount; i++)
		{
			Vector3 vector3 = this.ropeSegs[i].pos - this.ropeSegs[i].posOld;
			this.ropeSegs[i].posOld = this.ropeSegs[i].pos;
			HangingClaw.RopeSegment[] array = this.ropeSegs;
			int num4 = i;
			array[num4].pos = array[num4].pos + (vector3 * this.velocityDamping + vector);
		}
		int num5 = 3;
		for (int j = 0; j < num5; j++)
		{
			this.ApplyConstraints(vector2);
		}
	}

	private void ApplyConstraints(Vector3 topPos)
	{
		this.ropeSegs[0].pos = topPos;
		this.ropeSegs[0].posOld = topPos;
		float num = Mathf.Clamp01(this.ropeStiffness);
		for (int i = 0; i < this.segmentCount - 1; i++)
		{
			this.ApplyConstraintSegment(ref this.ropeSegs[i], ref this.ropeSegs[i + 1], this.invMass[i], this.invMass[i + 1], num);
		}
	}

	private void ApplyConstraintSegment(ref HangingClaw.RopeSegment a, ref HangingClaw.RopeSegment b, float wA, float wB, float stiffness)
	{
		Vector3 vector = b.pos - a.pos;
		float magnitude = vector.magnitude;
		if (magnitude < 1E-06f)
		{
			return;
		}
		float num = magnitude - this.targetSegLenScaled;
		if (Mathf.Abs(num) < 1E-06f)
		{
			return;
		}
		Vector3 vector2 = vector / magnitude;
		float num2 = wA + wB;
		if (num2 <= 0f)
		{
			return;
		}
		Vector3 vector3 = vector2 * (num * stiffness);
		a.pos += vector3 * (wA / num2);
		b.pos += -vector3 * (wB / num2);
	}

	private void DrawRope()
	{
		if (this.lineRenderer == null)
		{
			return;
		}
		this.lineRenderer.positionCount = this.segmentCount;
		for (int i = 0; i < this.segmentCount; i++)
		{
			Vector3 pos = this.ropeSegs[i].pos;
			if (this.heightCap && pos.y > this.heightCap.position.y)
			{
				pos.y = this.heightCap.position.y;
			}
			this.lineRenderer.SetPosition(i, this.ropeSegs[i].pos);
		}
	}

	public Transform endTransform;

	public Transform heightCap;

	private int segmentCount = 6;

	public float segmentMassKg = 1f;

	public float endMassKg = 5f;

	public float ropeStiffness = 0.9f;

	public float slackFraction = 0.02f;

	public Vector3 gravity = new Vector3(0f, -9.8f, 0f);

	public float velocityDamping = 0.98f;

	private float maxY;

	private LineRenderer lineRenderer;

	private HangingClaw.RopeSegment[] ropeSegs;

	private float baseSegLen;

	private float targetSegLenScaled;

	private float[] invMass;

	public struct RopeSegment
	{
		public RopeSegment(Vector3 p)
		{
			this.pos = p;
			this.posOld = p;
		}

		public Vector3 pos;

		public Vector3 posOld;
	}
}
