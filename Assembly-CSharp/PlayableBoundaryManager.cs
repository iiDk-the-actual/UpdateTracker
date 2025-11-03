using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GorillaTag;
using Unity.Mathematics;
using UnityEngine;

public class PlayableBoundaryManager : MonoBehaviour
{
	public static bool ShouldRender
	{
		get
		{
			return Shader.GetGlobalFloat(PlayableBoundaryManager._GTGameModes_PlayableBoundary_IsEnabled) > 0f;
		}
		set
		{
			Shader.SetGlobalFloat(PlayableBoundaryManager._GTGameModes_PlayableBoundary_IsEnabled, (float)(value ? 1 : 0));
		}
	}

	protected void Awake()
	{
		if (!Application.isPlaying)
		{
			base.enabled = false;
		}
	}

	public void Setup()
	{
		Shader.SetGlobalFloat(PlayableBoundaryManager._GTGameModes_PlayableBoundary_NonZeroSmoothRadius, this.m_smoothFactor);
		Vector3 position = base.transform.position;
		SRand srand = new SRand(StaticHash.Compute(position.x, position.y, position.z));
		this._cylinders_centers[0] = new Vector3(position.x, position.y, position.z);
		this._cylinders_radiusHeights[0] = new Vector2(this.m_bigCylinderRadius * this.radiusScale, 100f);
		for (int i = 1; i < 8; i++)
		{
			Vector3 vector = position + srand.NextPointInsideSphere(this.m_bigCylinderRadius * this.radiusScale);
			this._cylinders_centers[i] = new Vector4(vector.x, vector.y, vector.z, 0f);
			this._cylinders_radiusHeights[i] = new Vector4(this.m_smallCylindersRadius * this.radiusScale, 100f, 0f, 0f);
		}
	}

	private void OnEnable()
	{
		PlayableBoundaryManager.ShouldRender = true;
		this.Setup();
	}

	private void OnDisable()
	{
		PlayableBoundaryManager.ShouldRender = false;
	}

	public unsafe void UpdateSim()
	{
		if (Time.frameCount == this._lastFrameUpdated)
		{
			return;
		}
		this._lastFrameUpdated = Time.frameCount;
		Vector4[] array = this._cylinders_centers;
		if (array != null && array.Length == 8)
		{
			array = this._cylinders_radiusHeights;
			if (array != null && array.Length == 8)
			{
				if (this.m_smallCylindersMoveTimeScale > 0.0)
				{
					Vector3 position = base.transform.position;
					float num = (float)((double)(GTTime.TimeAsMilliseconds() % 86400000L) * this.m_smallCylindersMoveTimeScale / 1000.0);
					this._cylinders_centers[0] = new Vector3(position.x, position.y, position.z);
					this._cylinders_radiusHeights[0] = new Vector2(this.m_bigCylinderRadius * this.radiusScale, 100f);
					for (int i = 1; i < 8; i++)
					{
						float num2 = (float)i * 0.125f;
						Vector3 vector = *PlayableBoundaryManager.Hash3(num2 * 1.17f) + *PlayableBoundaryManager.Hash3(num2 * 13.7f) * num;
						Vector3 vector2 = position + vector.Sin() * this.m_bigCylinderRadius * this.radiusScale;
						this._cylinders_centers[i] = new Vector4(vector2.x, vector2.y, vector2.z, 0f);
						this._cylinders_radiusHeights[i] = new Vector4(this.m_smallCylindersRadius * this.radiusScale, 100f, 0f, 0f);
					}
				}
				Shader.SetGlobalVectorArray(PlayableBoundaryManager._GTGameModes_PlayableBoundary_Cylinders_Centers, this._cylinders_centers);
				Shader.SetGlobalVectorArray(PlayableBoundaryManager._GTGameModes_PlayableBoundary_Cylinders_RadiusHeights, this._cylinders_radiusHeights);
				for (int j = 0; j < this.tracked.Count; j++)
				{
					PlayableBoundaryTracker playableBoundaryTracker = this.tracked[j];
					if (playableBoundaryTracker)
					{
						playableBoundaryTracker.UpdateSignedDistanceToBoundary(this._GetSignedDistanceToBoundary(playableBoundaryTracker.transform.position, playableBoundaryTracker.radius), Time.deltaTime);
					}
				}
				Shader.SetGlobalFloat(PlayableBoundaryManager._GTGameModes_PlayableBoundary_NonZeroSmoothRadius, this.m_smoothFactor);
				return;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float _GetSignedDistanceToBoundary(float3 tracked_center, float tracked_radius)
	{
		float num = float.MaxValue;
		float smoothFactor = this.GetSmoothFactor();
		for (int i = 0; i < 8; i++)
		{
			float3 @float = this._cylinders_centers[i].xyz - tracked_center;
			float x = this._cylinders_radiusHeights[i].x;
			float num2 = math.length(@float.xz) - x;
			num = this.SDFSmoothMerge(num, num2, smoothFactor);
		}
		return num - tracked_radius;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float SDFSmoothMerge(float signedDist1, float signedDist2, float smoothRadius)
	{
		float num = -math.length(math.min(new float2(signedDist1 - smoothRadius, signedDist2 - smoothRadius), new float2(0f, 0f)));
		float num2 = math.max(math.min(signedDist1, signedDist2), smoothRadius);
		return num + num2;
	}

	private static ref Vector3 Hash3(float n)
	{
		PlayableBoundaryManager.kHashVec.x = Mathf.Sin(n) * 43758.547f % 1f;
		PlayableBoundaryManager.kHashVec.y = Mathf.Sin(n + 1f) * 22578.146f % 1f;
		PlayableBoundaryManager.kHashVec.z = Mathf.Sin(n + 2f) * 19642.35f % 1f;
		return ref PlayableBoundaryManager.kHashVec;
	}

	private float GetSmoothFactor()
	{
		float num = this.m_smoothFactor;
		if (this.m_bigCylinderRadius <= 1f)
		{
			num *= math.max(this.m_bigCylinderRadius, 0f);
		}
		return math.max(num, 1E-06f);
	}

	public List<PlayableBoundaryTracker> tracked = new List<PlayableBoundaryTracker>(10);

	[Space]
	[Range(0f, 128f)]
	public float m_bigCylinderRadius = 8f;

	public float m_smoothFactor = 1.5f;

	public float m_smallCylindersRadius = 3f;

	[SerializeField]
	private double m_smallCylindersMoveTimeScale = 0.25;

	[Space]
	private readonly Vector4[] _cylinders_centers = new Vector4[8];

	private readonly Vector4[] _cylinders_radiusHeights = new Vector4[8];

	private static ShaderHashId _GTGameModes_PlayableBoundary_Cylinders_Centers = "_GTGameModes_PlayableBoundary_Cylinders_Centers";

	private static ShaderHashId _GTGameModes_PlayableBoundary_Cylinders_RadiusHeights = "_GTGameModes_PlayableBoundary_Cylinders_RadiusHeights";

	private static ShaderHashId _GTGameModes_PlayableBoundary_NonZeroSmoothRadius = "_GTGameModes_PlayableBoundary_NonZeroSmoothRadius";

	private static ShaderHashId _GTGameModes_PlayableBoundary_IsEnabled = "_GTGameModes_PlayableBoundary_IsEnabled";

	private const int _k_cylinders_count = 8;

	[NonSerialized]
	public float radiusScale = 1f;

	private int _lastFrameUpdated = -1;

	private static Vector3 kHashVec = Vector3.zero;
}
