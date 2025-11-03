using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

namespace GorillaTag
{
	[AddComponentMenu("GorillaTag/ContainerLiquid (GTag)")]
	[ExecuteInEditMode]
	public class ContainerLiquid : MonoBehaviour
	{
		[DebugReadout]
		public bool isEmpty
		{
			get
			{
				return this.fillAmount <= this.refillThreshold;
			}
		}

		public Vector3 cupTopWorldPos { get; private set; }

		public Vector3 bottomLipWorldPos { get; private set; }

		public Vector3 liquidPlaneWorldPos { get; private set; }

		public Vector3 liquidPlaneWorldNormal { get; private set; }

		protected bool IsValidLiquidSurfaceValues()
		{
			return this.meshRenderer != null && this.meshFilter != null && this.spillParticleSystem != null && !string.IsNullOrEmpty(this.liquidColorShaderPropertyName) && !string.IsNullOrEmpty(this.liquidPlaneNormalShaderPropertyName) && !string.IsNullOrEmpty(this.liquidPlanePositionShaderPropertyName);
		}

		protected void InitializeLiquidSurface()
		{
			this.liquidColorShaderProp = Shader.PropertyToID(this.liquidColorShaderPropertyName);
			this.liquidPlaneNormalShaderProp = Shader.PropertyToID(this.liquidPlaneNormalShaderPropertyName);
			this.liquidPlanePositionShaderProp = Shader.PropertyToID(this.liquidPlanePositionShaderPropertyName);
			this.localMeshBounds = this.meshFilter.sharedMesh.bounds;
		}

		protected void InitializeParticleSystem()
		{
			this.spillParticleSystem.main.startColor = this.liquidColor;
		}

		protected void Awake()
		{
			this.matPropBlock = new MaterialPropertyBlock();
			this.topVerts = this.GetTopVerts();
		}

		protected void OnEnable()
		{
			if (Application.isPlaying)
			{
				base.enabled = this.useLiquidShader && this.IsValidLiquidSurfaceValues();
				if (base.enabled)
				{
					this.InitializeLiquidSurface();
				}
				this.InitializeParticleSystem();
				this.useFloater = this.floater != null;
			}
		}

		protected void LateUpdate()
		{
			this.UpdateRefillTimer();
			Transform transform = base.transform;
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			Bounds bounds = this.meshRenderer.bounds;
			Vector3 vector = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
			Vector3 vector2 = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
			this.liquidPlaneWorldPos = Vector3.Lerp(vector, vector2, this.fillAmount);
			Vector3 vector3 = transform.InverseTransformPoint(this.liquidPlaneWorldPos);
			float deltaTime = Time.deltaTime;
			this.temporalWobbleAmp = Vector2.Lerp(this.temporalWobbleAmp, Vector2.zero, deltaTime * this.recovery);
			float num = 6.2831855f * this.wobbleFrequency;
			float num2 = Mathf.Lerp(this.lastSineWave, Mathf.Sin(num * Time.realtimeSinceStartup), deltaTime * Mathf.Clamp(this.lastVelocity.magnitude + this.lastAngularVelocity.magnitude, this.thickness, 10f));
			Vector2 vector4 = this.temporalWobbleAmp * num2;
			this.liquidPlaneWorldNormal = new Vector3(vector4.x, -1f, vector4.y).normalized;
			Vector3 vector5 = transform.InverseTransformDirection(this.liquidPlaneWorldNormal);
			if (this.useLiquidShader)
			{
				this.matPropBlock.SetVector(this.liquidPlaneNormalShaderProp, vector5);
				this.matPropBlock.SetVector(this.liquidPlanePositionShaderProp, vector3);
				this.matPropBlock.SetVector(this.liquidColorShaderProp, this.liquidColor.linear);
				if (this.useLiquidVolume)
				{
					float num3 = MathUtils.Linear(this.fillAmount, 0f, 1f, this.liquidVolumeMinMax.x, this.liquidVolumeMinMax.y);
					this.matPropBlock.SetFloat(ShaderProps._LiquidFill, num3);
				}
				this.meshRenderer.SetPropertyBlock(this.matPropBlock);
			}
			if (this.useFloater)
			{
				float num4 = Mathf.Lerp(this.localMeshBounds.min.y, this.localMeshBounds.max.y, this.fillAmount);
				this.floater.localPosition = this.floater.localPosition.WithY(num4);
			}
			Vector3 vector6 = (this.lastPos - position) / deltaTime;
			Vector3 angularVelocity = GorillaMath.GetAngularVelocity(this.lastRot, rotation);
			this.temporalWobbleAmp.x = this.temporalWobbleAmp.x + Mathf.Clamp((vector6.x + vector6.y * 0.2f + angularVelocity.z + angularVelocity.y) * this.wobbleMax, -this.wobbleMax, this.wobbleMax);
			this.temporalWobbleAmp.y = this.temporalWobbleAmp.y + Mathf.Clamp((vector6.z + vector6.y * 0.2f + angularVelocity.x + angularVelocity.y) * this.wobbleMax, -this.wobbleMax, this.wobbleMax);
			this.lastPos = position;
			this.lastRot = rotation;
			this.lastSineWave = num2;
			this.lastVelocity = vector6;
			this.lastAngularVelocity = angularVelocity;
			this.meshRenderer.enabled = !this.keepMeshHidden && !this.isEmpty;
			float x = transform.lossyScale.x;
			float num5 = this.localMeshBounds.extents.x * x;
			float y = this.localMeshBounds.extents.y;
			Vector3 vector7 = this.localMeshBounds.center + new Vector3(0f, y, 0f);
			this.cupTopWorldPos = transform.TransformPoint(vector7);
			Vector3 up = transform.up;
			Vector3 vector8 = transform.InverseTransformDirection(Vector3.down);
			float num6 = float.MinValue;
			Vector3 vector9 = Vector3.zero;
			for (int i = 0; i < this.topVerts.Length; i++)
			{
				float num7 = Vector3.Dot(this.topVerts[i], vector8);
				if (num7 > num6)
				{
					num6 = num7;
					vector9 = this.topVerts[i];
				}
			}
			this.bottomLipWorldPos = transform.TransformPoint(vector9);
			float num8 = Mathf.Clamp01((this.liquidPlaneWorldPos.y - this.bottomLipWorldPos.y) / (num5 * 2f));
			bool flag = num8 > 1E-05f;
			ParticleSystem.EmissionModule emission = this.spillParticleSystem.emission;
			emission.enabled = flag;
			if (flag)
			{
				if (!this.spillSoundBankPlayer.isPlaying)
				{
					this.spillSoundBankPlayer.Play();
				}
				this.spillParticleSystem.transform.position = Vector3.Lerp(this.bottomLipWorldPos, this.cupTopWorldPos, num8);
				this.spillParticleSystem.shape.radius = num5 * num8;
				ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
				float num9 = num8 * this.maxSpillRate;
				rateOverTime.constant = num9;
				emission.rateOverTime = rateOverTime;
				this.fillAmount -= num9 * deltaTime * 0.01f;
			}
			if (this.isEmpty && !this.wasEmptyLastFrame && !this.emptySoundBankPlayer.isPlaying)
			{
				this.emptySoundBankPlayer.Play();
			}
			else if (!this.isEmpty && this.wasEmptyLastFrame && !this.refillSoundBankPlayer.isPlaying)
			{
				this.refillSoundBankPlayer.Play();
			}
			this.wasEmptyLastFrame = this.isEmpty;
		}

		public void UpdateRefillTimer()
		{
			if (this.refillDelay < 0f || !this.isEmpty)
			{
				return;
			}
			if (this.refillTimer < 0f)
			{
				this.refillTimer = this.refillDelay;
				this.fillAmount = this.refillAmount;
				return;
			}
			this.refillTimer -= Time.deltaTime;
		}

		private Vector3[] GetTopVerts()
		{
			Vector3[] vertices = this.meshFilter.sharedMesh.vertices;
			List<Vector3> list = new List<Vector3>(vertices.Length);
			float num = float.MinValue;
			foreach (Vector3 vector in vertices)
			{
				if (vector.y > num)
				{
					num = vector.y;
				}
			}
			foreach (Vector3 vector2 in vertices)
			{
				if (Mathf.Abs(vector2.y - num) < 0.001f)
				{
					list.Add(vector2);
				}
			}
			return list.ToArray();
		}

		[Tooltip("Used to determine the world space bounds of the container.")]
		public MeshRenderer meshRenderer;

		[Tooltip("Used to determine the local space bounds of the container.")]
		public MeshFilter meshFilter;

		[Tooltip("If you are only using the liquid mesh to calculate the volume of the container and do not need visuals then set this to true.")]
		public bool keepMeshHidden;

		[Tooltip("The object that will float on top of the liquid.")]
		public Transform floater;

		public bool useLiquidShader = true;

		public bool useLiquidVolume;

		public Vector2 liquidVolumeMinMax = Vector2.up;

		public string liquidColorShaderPropertyName = "_BaseColor";

		public string liquidPlaneNormalShaderPropertyName = "_LiquidPlaneNormal";

		public string liquidPlanePositionShaderPropertyName = "_LiquidPlanePosition";

		[Tooltip("Emits drips when pouring.")]
		public ParticleSystem spillParticleSystem;

		[SoundBankInfo]
		public SoundBankPlayer emptySoundBankPlayer;

		[SoundBankInfo]
		public SoundBankPlayer refillSoundBankPlayer;

		[SoundBankInfo]
		public SoundBankPlayer spillSoundBankPlayer;

		public Color liquidColor = new Color(0.33f, 0.25f, 0.21f, 1f);

		[Tooltip("The amount of liquid currently in the container. This value is passed to the shader.")]
		[Range(0f, 1f)]
		public float fillAmount = 0.85f;

		[Tooltip("This is what fillAmount will be after automatic refilling.")]
		public float refillAmount = 0.85f;

		[Tooltip("Set to a negative value to disable.")]
		public float refillDelay = 10f;

		[Tooltip("The point that the liquid should be considered empty and should be auto refilled.")]
		public float refillThreshold = 0.1f;

		public float wobbleMax = 0.2f;

		public float wobbleFrequency = 1f;

		public float recovery = 1f;

		public float thickness = 1f;

		public float maxSpillRate = 100f;

		[DebugReadout]
		private bool wasEmptyLastFrame;

		private int liquidColorShaderProp;

		private int liquidPlaneNormalShaderProp;

		private int liquidPlanePositionShaderProp;

		private float refillTimer;

		private float lastSineWave;

		private float lastWobble;

		private Vector2 temporalWobbleAmp;

		private Vector3 lastPos;

		private Vector3 lastVelocity;

		private Vector3 lastAngularVelocity;

		private Quaternion lastRot;

		private MaterialPropertyBlock matPropBlock;

		private Bounds localMeshBounds;

		private bool useFloater;

		private Vector3[] topVerts;
	}
}
