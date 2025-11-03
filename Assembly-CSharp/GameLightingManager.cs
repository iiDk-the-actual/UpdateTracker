using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GameLightingManager : MonoBehaviourTick, IGorillaSliceableSimple
{
	private void Awake()
	{
		this.InitData();
	}

	private void InitData()
	{
		GameLightingManager.instance = this;
		this.gameLights = new List<GameLight>(512);
		for (int i = 0; i < this.lightDistanceBins.Length; i++)
		{
			this.lightDistanceBins[i] = new List<GameLight>();
		}
		this.lightDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 50, UnsafeUtility.SizeOf<GameLightingManager.LightData>());
		this.lightData = new NativeArray<GameLightingManager.LightData>(50, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		this.nextLightUpdate = 0;
		this.ClearGameLights();
		this.SetDesaturateAndTintEnabled(false, Color.black);
		this.SetAmbientLightDynamic(Color.black);
		this.SetCustomDynamicLightingEnabled(false);
		this.SetMaxLights(20);
	}

	private void OnDestroy()
	{
		this.ClearGameLights();
		this.SetDesaturateAndTintEnabled(false, Color.black);
		this.SetAmbientLightDynamic(Color.black);
		this.SetCustomDynamicLightingEnabled(false);
		this.lightData.Dispose();
	}

	public new void OnEnable()
	{
		base.OnEnable();
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public new void OnDisable()
	{
		base.OnDisable();
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void ZoneEnableCustomDynamicLighting(bool enable)
	{
		if (enable)
		{
			if (this.zoneDynamicLightingEnableCount == 0)
			{
				this.SetCustomDynamicLightingEnabled(true);
			}
			this.zoneDynamicLightingEnableCount++;
			return;
		}
		this.zoneDynamicLightingEnableCount--;
		if (this.zoneDynamicLightingEnableCount == 0)
		{
			this.SetCustomDynamicLightingEnabled(false);
		}
		if (this.zoneDynamicLightingEnableCount < 0)
		{
			Debug.LogErrorFormat("Zone Dynamic Lighting Ref count is {0} and should never be less that 0", new object[] { this.zoneDynamicLightingEnableCount });
			this.zoneDynamicLightingEnableCount = 0;
		}
	}

	public void SetCustomDynamicLightingEnabled(bool enable)
	{
		this.customVertexLightingEnabled = enable;
		if (this.customVertexLightingEnabled)
		{
			Shader.EnableKeyword("_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX");
			return;
		}
		Shader.DisableKeyword("_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX");
	}

	public void SetAmbientLightDynamic(Color color)
	{
		Shader.SetGlobalColor("_GT_GameLight_Ambient_Color", color);
	}

	public void SetMaxLights(int maxLights)
	{
		maxLights = Mathf.Min(maxLights, 50);
		this.maxUseTestLights = maxLights;
		Shader.SetGlobalInteger("_GT_GameLight_UseMaxLights", maxLights);
	}

	public void SetDesaturateAndTintEnabled(bool enable, Color tint)
	{
		Shader.SetGlobalColor("_GT_DesaturateAndTint_TintColor", tint);
		Shader.SetGlobalFloat("_GT_DesaturateAndTint_TintAmount", enable ? 1f : 0f);
		this.desaturateAndTintEnabled = enable;
	}

	public void SliceUpdate()
	{
		if (this.skipNextSlice)
		{
			this.skipNextSlice = false;
			return;
		}
		this.immediateSort = false;
		this.SortLights();
	}

	public void SortLights()
	{
		if (this.gameLights.Count <= this.maxUseTestLights)
		{
			return;
		}
		if (this.mainCameraTransform == null)
		{
			this.mainCameraTransform = Camera.main.transform;
		}
		this.cameraPosForSort = this.mainCameraTransform.position;
		this.gameLights.Sort(new Comparison<GameLight>(this.CompareDistFromCamera));
	}

	private int CompareDistFromCamera(GameLight a, GameLight b)
	{
		if (a == null || a.light == null)
		{
			if (b == null || b.light == null)
			{
				return 0;
			}
			return -1;
		}
		else
		{
			if (b == null || b.light == null)
			{
				return 1;
			}
			float num = Mathf.Clamp(a.cachedColorAndIntensity.x + a.cachedColorAndIntensity.y + a.cachedColorAndIntensity.z, 0.01f, 6f);
			float num2 = Mathf.Clamp(b.cachedColorAndIntensity.x + b.cachedColorAndIntensity.y + b.cachedColorAndIntensity.z, 0.01f, 6f);
			float num3 = (this.cameraPosForSort - a.cachedPosition).sqrMagnitude / num;
			float num4 = (this.cameraPosForSort - b.cachedPosition).sqrMagnitude / num2;
			return num3.CompareTo(num4);
		}
	}

	public override void Tick()
	{
		this.RefreshLightData();
	}

	private void RefreshLightData()
	{
		NativeArray<GameLightingManager.LightData> nativeArray = this.lightData;
		if (this.customVertexLightingEnabled)
		{
			int num = 10;
			if (this.immediateSort)
			{
				this.immediateSort = false;
				this.skipNextSlice = true;
				this.CacheAllLightData();
				this.SortLights();
				num = this.maxUseTestLights;
			}
			else
			{
				int num2 = 5;
				this.CacheLightDataForNonCloseLights(num2);
			}
			this.PullLightData(num);
			this.lightDataBuffer.SetData<GameLightingManager.LightData>(this.lightData);
			Shader.SetGlobalBuffer("_GT_GameLight_Lights", this.lightDataBuffer);
		}
	}

	public void CacheAllLightData()
	{
		for (int i = 0; i < this.gameLights.Count; i++)
		{
			GameLight gameLight = this.gameLights[i];
			if (gameLight != null && gameLight.light != null)
			{
				gameLight.cachedPosition = gameLight.transform.position;
				gameLight.cachedColorAndIntensity = (float)gameLight.intensityMult * gameLight.light.intensity * (gameLight.negativeLight ? (-1f) : 1f) * gameLight.light.color;
			}
		}
	}

	public void CacheLightDataForNonCloseLights(int numLightsToUpdateCache)
	{
		int num = this.gameLights.Count - this.maxUseTestLights;
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < numLightsToUpdateCache; i++)
		{
			this.nextLightCacheUpdate = (this.nextLightCacheUpdate + 1) % num;
			GameLight gameLight = this.gameLights[this.maxUseTestLights + this.nextLightCacheUpdate];
			if (gameLight != null && gameLight.light != null)
			{
				gameLight.cachedPosition = gameLight.transform.position;
				gameLight.cachedColorAndIntensity = (float)gameLight.intensityMult * gameLight.light.intensity * (gameLight.negativeLight ? (-1f) : 1f) * gameLight.light.color;
			}
		}
	}

	public void PullLightData(int numLightsToPull)
	{
		for (int i = 0; i < this.maxUseTestLights; i++)
		{
			if (i < this.gameLights.Count && this.gameLights[i] != null && this.gameLights[i].isHighPriorityPlayerLight)
			{
				this.GetFromLight(i, i);
			}
		}
		for (int j = 0; j < numLightsToPull; j++)
		{
			this.nextLightUpdate = (this.nextLightUpdate + 1) % this.maxUseTestLights;
			if (this.nextLightUpdate < this.gameLights.Count)
			{
				this.GetFromLight(this.nextLightUpdate, this.nextLightUpdate);
				if (this.gameLights[this.nextLightUpdate] != null && this.gameLights[this.nextLightUpdate].isHighPriorityPlayerLight)
				{
				}
			}
			else
			{
				this.ResetLight(this.nextLightUpdate);
			}
		}
	}

	public int AddGameLight(GameLight light, bool ignoreUnityLightDisable = false)
	{
		if (light == null || !light.gameObject.activeInHierarchy || light.light == null || !light.light.enabled)
		{
			return -1;
		}
		if (this.gameLights.Contains(light))
		{
			return -1;
		}
		if (!ignoreUnityLightDisable)
		{
			light.light.enabled = false;
		}
		this.gameLights.Add(light);
		this.immediateSort = true;
		return this.gameLights.Count - 1;
	}

	public void RemoveGameLight(GameLight light)
	{
		if (light != null && light.light != null)
		{
			light.light.enabled = true;
		}
		int num = this.gameLights.IndexOf(light);
		if (num >= 0)
		{
			this.gameLights.RemoveAt(num);
		}
	}

	public void ClearGameLights()
	{
		if (this.gameLights != null)
		{
			this.gameLights.Clear();
		}
		NativeArray<GameLightingManager.LightData> nativeArray = this.lightData;
		for (int i = 0; i < this.lightData.Length; i++)
		{
			this.ResetLight(i);
		}
		this.lightDataBuffer.SetData<GameLightingManager.LightData>(this.lightData);
		Shader.SetGlobalBuffer("_GT_GameLight_Lights", this.lightDataBuffer);
	}

	public void GetFromLight(int lightIndex, int gameLightIndex)
	{
		NativeArray<GameLightingManager.LightData> nativeArray = this.lightData;
		GameLight gameLight = null;
		if (gameLightIndex >= 0 && gameLightIndex < this.gameLights.Count)
		{
			gameLight = this.gameLights[gameLightIndex];
		}
		if (gameLight == null || gameLight.light == null)
		{
			return;
		}
		gameLight.cachedPosition = gameLight.transform.position;
		gameLight.cachedColorAndIntensity = (float)gameLight.intensityMult * gameLight.light.intensity * (gameLight.negativeLight ? (-1f) : 1f) * gameLight.light.color;
		Vector4 vector = gameLight.cachedPosition;
		vector.w = 1f;
		Vector4 cachedColorAndIntensity = gameLight.cachedColorAndIntensity;
		Vector3 zero = Vector3.zero;
		GameLightingManager.LightData lightData = new GameLightingManager.LightData
		{
			lightPos = vector,
			lightColor = cachedColorAndIntensity,
			lightDirection = zero
		};
		this.lightData[lightIndex] = lightData;
	}

	private void ResetLight(int lightIndex)
	{
		GameLightingManager.LightData lightData = new GameLightingManager.LightData
		{
			lightPos = Vector4.zero,
			lightColor = Color.black,
			lightDirection = Vector4.zero
		};
		this.lightData[lightIndex] = lightData;
	}

	[OnEnterPlay_SetNull]
	public static volatile GameLightingManager instance;

	public const int MAX_VERTEX_LIGHTS = 50;

	public const int USE_MAX_VERTEX_LIGHTS = 20;

	public const int MAX_UPDATE_LIGHTS_PER_FRAME = 10;

	private const int MAX_LIGHT_POWER = 100;

	private const int LIGHT_POWER_BIN_SIZE = 5;

	public Transform testLightsCenter;

	[ColorUsage(true, true)]
	public Color testAmbience = Color.black;

	[ColorUsage(true, true)]
	public Color testLightColor = Color.white;

	public float testLightBrightness = 10f;

	public float testLightRadius = 2f;

	public int maxUseTestLights = 1;

	[ReadOnly]
	[SerializeField]
	private List<GameLight> gameLights;

	private bool customVertexLightingEnabled;

	private bool desaturateAndTintEnabled;

	private Transform mainCameraTransform;

	private int zoneDynamicLightingEnableCount;

	private List<GameLight>[] lightDistanceBins = new List<GameLight>[20];

	private NativeArray<GameLightingManager.LightData> lightData;

	private GraphicsBuffer lightDataBuffer;

	private Vector3 cameraPosForSort;

	private bool skipNextSlice;

	private bool immediateSort;

	private int nextLightUpdate;

	private int nextLightCacheUpdate;

	public struct LightInput
	{
		public Color color;

		public float intensity;

		public float intensityMult;
	}

	public struct LightData
	{
		public Vector4 lightPos;

		public Vector4 lightColor;

		public Vector4 lightDirection;
	}
}
