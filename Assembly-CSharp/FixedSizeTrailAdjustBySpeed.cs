using System;
using UnityEngine;
using UnityEngine.Serialization;

public class FixedSizeTrailAdjustBySpeed : MonoBehaviour
{
	private void Start()
	{
		this.Setup();
	}

	private void Setup()
	{
		this._lastPosition = base.transform.position;
		this._rawVelocity = Vector3.zero;
		this._rawSpeed = 0f;
		this._speed = 0f;
		if (this.trail)
		{
			this._initGravity = this.trail.gravity;
			this.trail.applyPhysics = this.adjustPhysics;
		}
		this.LerpTrailColors(0.5f);
	}

	private void LerpTrailColors(float t = 0.5f)
	{
		GradientColorKey[] colorKeys = this._mixGradient.colorKeys;
		int num = colorKeys.Length;
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i / (float)(num - 1);
			Color color = this.minColors.Evaluate(num2);
			Color color2 = this.maxColors.Evaluate(num2);
			Color color3 = Color.Lerp(color, color2, t);
			colorKeys[i].color = color3;
			colorKeys[i].time = num2;
		}
		this._mixGradient.colorKeys = colorKeys;
		if (this.trail)
		{
			this.trail.renderer.colorGradient = this._mixGradient;
		}
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		Vector3 position = base.transform.position;
		this._rawVelocity = (position - this._lastPosition) / deltaTime;
		this._rawSpeed = this._rawVelocity.magnitude;
		if (this._rawSpeed > this.retractMin)
		{
			this._speed += this.expandSpeed * deltaTime;
		}
		if (this._rawSpeed <= this.retractMin)
		{
			this._speed -= this.retractSpeed * deltaTime;
		}
		if (this._speed > this.maxSpeed)
		{
			this._speed = this.maxSpeed;
		}
		this._speed = Mathf.Lerp(this._lastSpeed, this._speed, 0.5f);
		if (this._speed < 0.01f)
		{
			this._speed = 0f;
		}
		this.AdjustTrail();
		this._lastSpeed = this._speed;
		this._lastPosition = position;
	}

	private void AdjustTrail()
	{
		if (!this.trail)
		{
			return;
		}
		float num = MathUtils.Linear(this._speed, this.minSpeed, this.maxSpeed, 0f, 1f);
		float num2 = MathUtils.Linear(num, 0f, 1f, this.minLength, this.maxLength);
		this.trail.length = num2;
		this.LerpTrailColors(num);
		if (this.adjustPhysics)
		{
			Transform transform = base.transform;
			Vector3 vector = transform.forward * this.gravityOffset.z + transform.right * this.gravityOffset.x + transform.up * this.gravityOffset.y;
			Vector3 vector2 = (this._initGravity + vector) * (1f - num);
			this.trail.gravity = Vector3.Lerp(Vector3.zero, vector2, 0.5f);
		}
	}

	public FixedSizeTrail trail;

	public bool adjustPhysics = true;

	private Vector3 _rawVelocity;

	private float _rawSpeed;

	private float _speed;

	private float _lastSpeed;

	private Vector3 _lastPosition;

	private Vector3 _initGravity;

	public Vector3 gravityOffset = Vector3.zero;

	[Space]
	public float retractMin = 0.5f;

	[Space]
	[FormerlySerializedAs("sizeIncreaseSpeed")]
	public float expandSpeed = 16f;

	[FormerlySerializedAs("sizeDecreaseSpeed")]
	public float retractSpeed = 4f;

	[Space]
	public float minSpeed;

	public float minLength = 1f;

	public Gradient minColors = GradientHelper.FromColor(new Color(0f, 1f, 1f, 1f));

	[Space]
	public float maxSpeed = 10f;

	public float maxLength = 8f;

	public Gradient maxColors = GradientHelper.FromColor(new Color(1f, 1f, 0f, 1f));

	[Space]
	[SerializeField]
	private Gradient _mixGradient = new Gradient
	{
		colorKeys = new GradientColorKey[8],
		alphaKeys = Array.Empty<GradientAlphaKey>()
	};

	[Serializable]
	public struct GradientKey
	{
		public GradientKey(Color color, float time)
		{
			this.color = color;
			this.time = time;
		}

		public Color color;

		public float time;
	}
}
