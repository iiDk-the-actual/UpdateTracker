using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FixedSizeTrail : MonoBehaviour
{
	public LineRenderer renderer
	{
		get
		{
			return this._lineRenderer;
		}
	}

	public float length
	{
		get
		{
			return this._length;
		}
		set
		{
			this._length = Math.Clamp(value, 0f, 128f);
		}
	}

	public Vector3[] points
	{
		get
		{
			return this._points;
		}
	}

	private void Reset()
	{
		this.Setup();
	}

	private void Awake()
	{
		this.Setup();
	}

	private void Setup()
	{
		this._transform = base.transform;
		if (this._lineRenderer == null)
		{
			this._lineRenderer = base.GetComponent<LineRenderer>();
		}
		if (!this._lineRenderer)
		{
			return;
		}
		this._lineRenderer.useWorldSpace = true;
		Vector3 position = this._transform.position;
		Vector3 forward = this._transform.forward;
		int num = this._segments + 1;
		this._points = new Vector3[num];
		float num2 = this._length / (float)this._segments;
		for (int i = 0; i < num; i++)
		{
			this._points[i] = position - forward * num2 * (float)i;
		}
		this._lineRenderer.positionCount = num;
		this._lineRenderer.SetPositions(this._points);
		this.Update();
	}

	private void Update()
	{
		if (!this.manualUpdate)
		{
			this.Update(Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (!this.applyPhysics)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		int num = this._points.Length - 1;
		float num2 = this._length / (float)num;
		for (int i = 1; i < num; i++)
		{
			float num3 = (float)(i - 1) / (float)num;
			float num4 = this.gravityCurve.Evaluate(num3);
			Vector3 vector = this.gravity * (num4 * deltaTime);
			this._points[i] += vector;
			this._points[i + 1] += vector;
		}
	}

	public void Update(float dt)
	{
		float num = this._length / (float)(this._segments - 1);
		Vector3 position = this._transform.position;
		this._points[0] = position;
		float num2 = Vector3.Distance(this._points[0], this._points[1]);
		float num3 = num - num2;
		if (num2 > num)
		{
			Array.Copy(this._points, 0, this._points, 1, this._points.Length - 1);
		}
		for (int i = 0; i < this._points.Length - 1; i++)
		{
			Vector3 vector = this._points[i];
			Vector3 vector2 = this._points[i + 1] - vector;
			if (vector2.sqrMagnitude > num * num)
			{
				this._points[i + 1] = vector + vector2.normalized * num;
			}
		}
		if (num3 > 0f)
		{
			int num4 = this._points.Length - 1;
			int num5 = num4 - 1;
			Vector3 vector3 = this._points[num4] - this._points[num5];
			Vector3 vector4 = vector3.normalized;
			if (this.applyPhysics)
			{
				Vector3 normalized = (this._points[num5] - this._points[num5 - 1]).normalized;
				vector4 = Vector3.Lerp(vector4, normalized, 0.5f);
			}
			this._points[num4] = this._points[num5] + vector4 * Math.Min(vector3.magnitude, num3);
		}
		this._lineRenderer.SetPositions(this._points);
	}

	private static float CalcLength(in Vector3[] positions)
	{
		float num = 0f;
		for (int i = 0; i < positions.Length - 1; i++)
		{
			num += Vector3.Distance(positions[i], positions[i + 1]);
		}
		return num;
	}

	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private LineRenderer _lineRenderer;

	[SerializeField]
	[Range(1f, 128f)]
	private int _segments = 8;

	[SerializeField]
	private float _length = 8f;

	public bool manualUpdate;

	[Space]
	public bool applyPhysics;

	public Vector3 gravity = new Vector3(0f, -9.8f, 0f);

	public AnimationCurve gravityCurve = AnimationCurves.EaseInCubic;

	[Space]
	private Vector3[] _points = new Vector3[8];
}
