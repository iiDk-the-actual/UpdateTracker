using System;
using System.Collections.Generic;
using CjLib;
using GorillaNetworking;
using UnityEngine;

public class MoonController : MonoBehaviour
{
	public float Distance
	{
		get
		{
			return this.distance;
		}
	}

	private float TimeOfDay
	{
		get
		{
			if (this.debugOverrideTimeOfDay)
			{
				return Mathf.Repeat(this.timeOfDayOverride, 1f);
			}
			if (!(BetterDayNightManager.instance != null))
			{
				return 1f;
			}
			return BetterDayNightManager.instance.NormalizedTimeOfDay;
		}
	}

	public void SetEyeOpenAnimation()
	{
		this.openMoonAnimator.SetBool(this.eyeOpenHash, true);
	}

	public void StartEyeCloseAnimation()
	{
		this.openMoonAnimator.SetBool(this.eyeOpenHash, false);
	}

	private void Start()
	{
		this.eyeOpenHash = Animator.StringToHash("EyeOpen");
		this.zoneToSceneMapping.Add(GTZone.forest, MoonController.Scenes.Forest);
		this.zoneToSceneMapping.Add(GTZone.city, MoonController.Scenes.City);
		this.zoneToSceneMapping.Add(GTZone.basement, MoonController.Scenes.City);
		this.zoneToSceneMapping.Add(GTZone.canyon, MoonController.Scenes.Canyon);
		this.zoneToSceneMapping.Add(GTZone.beach, MoonController.Scenes.Beach);
		this.zoneToSceneMapping.Add(GTZone.mountain, MoonController.Scenes.Mountain);
		this.zoneToSceneMapping.Add(GTZone.skyJungle, MoonController.Scenes.Clouds);
		this.zoneToSceneMapping.Add(GTZone.cave, MoonController.Scenes.Forest);
		this.zoneToSceneMapping.Add(GTZone.cityWithSkyJungle, MoonController.Scenes.City);
		this.zoneToSceneMapping.Add(GTZone.tutorial, MoonController.Scenes.Forest);
		this.zoneToSceneMapping.Add(GTZone.rotating, MoonController.Scenes.Forest);
		this.zoneToSceneMapping.Add(GTZone.none, MoonController.Scenes.Forest);
		this.zoneToSceneMapping.Add(GTZone.Metropolis, MoonController.Scenes.Metropolis);
		this.zoneToSceneMapping.Add(GTZone.cityNoBuildings, MoonController.Scenes.City);
		this.zoneToSceneMapping.Add(GTZone.attic, MoonController.Scenes.Forest);
		this.zoneToSceneMapping.Add(GTZone.arcade, MoonController.Scenes.City);
		this.zoneToSceneMapping.Add(GTZone.bayou, MoonController.Scenes.Bayou);
		if (ZoneManagement.instance != null)
		{
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
		}
		if (GreyZoneManager.Instance != null)
		{
			GreyZoneManager.Instance.RegisterMoon(this);
		}
		this.crackStartDayOfYear = new DateTime(2024, 10, 4).DayOfYear;
		this.crackEndDayOfYear = new DateTime(2024, 10, 25).DayOfYear;
		if (this.crackRenderer != null)
		{
			this.currentlySetCrackProgress = 1f;
			this.crackMaterialPropertyBlock = new MaterialPropertyBlock();
			this.crackRenderer.GetPropertyBlock(this.crackMaterialPropertyBlock);
			this.crackMaterialPropertyBlock.SetFloat(ShaderProps._Progress, this.currentlySetCrackProgress);
			this.crackRenderer.SetPropertyBlock(this.crackMaterialPropertyBlock);
		}
		this.orbitAngle = 0f;
		this.UpdateCrack();
		this.UpdatePlacement();
	}

	private void OnDestroy()
	{
		if (GreyZoneManager.Instance != null)
		{
			GreyZoneManager.Instance.UnregisterMoon(this);
		}
	}

	private void OnZoneChanged()
	{
		ZoneManagement instance = ZoneManagement.instance;
		MoonController.Scenes scenes = MoonController.Scenes.Forest;
		for (int i = 0; i < instance.activeZones.Count; i++)
		{
			MoonController.Scenes scenes2;
			if (this.zoneToSceneMapping.TryGetValue(instance.activeZones[i], out scenes2) && scenes2 > scenes)
			{
				scenes = scenes2;
			}
		}
		this.UpdateActiveScene(scenes);
	}

	private void UpdateActiveScene(MoonController.Scenes nextScene)
	{
		this.activeScene = nextScene;
		this.UpdateCrack();
		this.UpdatePlacement();
	}

	private void Update()
	{
		this.UpdateCrack();
		if (!this.alwaysInTheSky)
		{
			float timeOfDay = this.TimeOfDay;
			bool flag = timeOfDay > 0.53999996f && timeOfDay < 0.6733333f;
			bool flag2 = timeOfDay > 0.086666666f && timeOfDay < 0.22f;
			bool flag3 = timeOfDay <= 0.086666666f || timeOfDay >= 0.6733333f;
			if (timeOfDay >= 0.22f)
			{
				bool flag4 = timeOfDay <= 0.53999996f;
			}
			float num = this.orbitAngle;
			if (flag)
			{
				num = Mathf.Lerp(3.1415927f, 0f, (timeOfDay - 0.53999996f) / 0.13333333f);
			}
			else if (flag2)
			{
				num = Mathf.Lerp(0f, -3.1415927f, (timeOfDay - 0.086666666f) / 0.13333333f);
			}
			else if (flag3)
			{
				num = 0f;
			}
			else
			{
				num = 3.1415927f;
			}
			if (this.orbitAngle != num)
			{
				this.orbitAngle = num;
				this.UpdateCrack();
				this.UpdatePlacement();
			}
		}
	}

	public void UpdateDistance(float nextDistance)
	{
		this.distance = nextDistance;
		this.UpdateVisualState();
		this.UpdatePlacement();
	}

	public void UpdateVisualState()
	{
		bool flag = false;
		if (GreyZoneManager.Instance != null)
		{
			flag = GreyZoneManager.Instance.GreyZoneActive;
		}
		if (flag && this.openEyeModelEnabled && this.distance < this.eyeOpenDistThreshold && !this.openMoonAnimator.GetBool(this.eyeOpenHash))
		{
			this.openMoonAnimator.SetBool(this.eyeOpenHash, true);
			return;
		}
		if (!flag && this.distance > this.eyeCloseDistThreshold && this.openMoonAnimator.GetBool(this.eyeOpenHash))
		{
			this.openMoonAnimator.SetBool(this.eyeOpenHash, false);
		}
	}

	public void UpdatePlacement()
	{
		if (this.alwaysInTheSky)
		{
			this.UpdatePlacementSimple();
			return;
		}
		this.UpdatePlacementOrbit();
	}

	private void UpdatePlacementSimple()
	{
		MoonController.SceneData sceneData = this.scenes[(int)this.activeScene];
		Transform referencePoint = sceneData.referencePoint;
		MoonController.Placement placement = (sceneData.overridePlacement ? sceneData.PlacementOverride : this.defaultPlacement);
		float num = Mathf.Lerp(placement.heightRange.x, placement.heightRange.y, this.distance);
		float num2 = Mathf.Lerp(placement.radiusRange.x, placement.radiusRange.y, this.distance);
		float num3 = Mathf.Lerp(placement.scaleRange.x, placement.scaleRange.y, this.distance);
		float restAngle = placement.restAngle;
		Vector3 position = referencePoint.position;
		position.y += num;
		position.x += num2 * Mathf.Cos(restAngle * 0.017453292f);
		position.z += num2 * Mathf.Sin(restAngle * 0.017453292f);
		base.transform.position = position;
		base.transform.rotation = Quaternion.LookRotation(referencePoint.position - base.transform.position);
		base.transform.localScale = Vector3.one * num3;
	}

	public void UpdatePlacementOrbit()
	{
		MoonController.SceneData sceneData = this.scenes[(int)this.activeScene];
		Transform referencePoint = sceneData.referencePoint;
		MoonController.Placement placement = (sceneData.overridePlacement ? sceneData.PlacementOverride : this.defaultPlacement);
		float y = placement.heightRange.y;
		float y2 = placement.radiusRange.y;
		Vector3 position = referencePoint.position;
		position.y += y;
		position.x += y2 * Mathf.Cos(placement.restAngle * 0.017453292f);
		position.z += y2 * Mathf.Sin(placement.restAngle * 0.017453292f);
		float num = Mathf.Sqrt(y * y + y2 * y2);
		float num2 = Mathf.Atan2(y, y2);
		Quaternion quaternion = Quaternion.AngleAxis(57.29578f * num2, Vector3.Cross(position - referencePoint.position, Vector3.up));
		float num3 = placement.restAngle * 0.017453292f + this.orbitAngle;
		Vector3 vector = referencePoint.position + quaternion * new Vector3(Mathf.Cos(num3), 0f, Mathf.Sin(num3)) * num;
		if (this.distance < 1f)
		{
			Vector3 position2 = referencePoint.position;
			position2.y += placement.heightRange.x;
			position2.x += placement.radiusRange.x * Mathf.Cos(placement.restAngle * 0.017453292f);
			position2.z += placement.radiusRange.x * Mathf.Sin(placement.restAngle * 0.017453292f);
			if (Mathf.Abs(this.orbitAngle) < 0.9424779f)
			{
				vector = Vector3.Lerp(position2, vector, this.distance);
			}
			else
			{
				vector = Vector3.Lerp(position2, position, this.distance);
			}
		}
		base.transform.position = vector;
		base.transform.rotation = Quaternion.LookRotation(referencePoint.position - base.transform.position);
		base.transform.localScale = Vector3.one * Mathf.Lerp(placement.scaleRange.x, placement.scaleRange.y, this.distance);
		if (this.debugDrawOrbit)
		{
			int num4 = 32;
			float timeOfDay = this.TimeOfDay;
			float num5 = 0.086666666f;
			float num6 = 0.24666667f;
			float num7 = 0.6333333f;
			float num8 = 0.76f;
			bool flag = timeOfDay > num5 && timeOfDay < num6;
			bool flag2 = timeOfDay > num7 && timeOfDay < num8;
			bool flag3 = timeOfDay <= num5 || timeOfDay >= num8;
			if (timeOfDay >= num6)
			{
				bool flag4 = timeOfDay <= num7;
			}
			Color color = (flag2 ? Color.red : (flag3 ? Color.green : (flag ? Color.yellow : Color.blue)));
			Vector3 vector2 = referencePoint.position + quaternion * new Vector3(Mathf.Cos(0f), 0f, Mathf.Sin(0f)) * num;
			for (int i = 1; i <= num4; i++)
			{
				float num9 = (float)i / (float)num4;
				Vector3 vector3 = referencePoint.position + quaternion * new Vector3(Mathf.Cos(6.2831855f * num9), 0f, Mathf.Sin(6.2831855f * num9)) * num;
				DebugUtil.DrawLine(vector2, vector3, color, false);
				vector2 = vector3;
			}
		}
	}

	private void UpdateCrack()
	{
		bool flag = GreyZoneManager.Instance != null && GreyZoneManager.Instance.GreyZoneAvailable;
		if (flag && !this.openEyeModelEnabled)
		{
			this.openEyeModelEnabled = true;
			this.defaultMoon.gameObject.SetActive(false);
			this.openMoon.gameObject.SetActive(true);
		}
		else if (!flag && this.openEyeModelEnabled)
		{
			this.openEyeModelEnabled = false;
			this.defaultMoon.gameObject.SetActive(true);
			this.openMoon.gameObject.SetActive(false);
		}
		if (!flag && GorillaComputer.instance != null)
		{
			DateTime serverTime = GorillaComputer.instance.GetServerTime();
			if (this.debugOverrideCrackDayInOctober)
			{
				serverTime = new DateTime(2024, 10, Mathf.Clamp(this.crackDayInOctoberOverride, 1, 31));
			}
			float num = Mathf.InverseLerp((float)this.crackStartDayOfYear, (float)this.crackEndDayOfYear, (float)serverTime.DayOfYear);
			if (this.debugOverrideCrackProgress)
			{
				num = this.crackProgress;
			}
			float num2 = 1f - Mathf.Clamp01(num);
			if (this.crackRenderer != null && Mathf.Abs(num2 - this.currentlySetCrackProgress) > Mathf.Epsilon)
			{
				this.currentlySetCrackProgress = num2;
				this.crackMaterialPropertyBlock.SetFloat(ShaderProps._Progress, this.currentlySetCrackProgress);
				this.crackRenderer.SetPropertyBlock(this.crackMaterialPropertyBlock);
			}
		}
	}

	[SerializeField]
	private List<MoonController.SceneData> scenes = new List<MoonController.SceneData>();

	[SerializeField]
	private MoonController.Scenes activeScene;

	[SerializeField]
	private MoonController.Placement defaultPlacement;

	[SerializeField]
	[Range(0f, 1f)]
	private float distance;

	[SerializeField]
	private bool alwaysInTheSky;

	[Header("Model Swap")]
	[SerializeField]
	private Transform defaultMoon;

	[SerializeField]
	private Transform openMoon;

	[Header("Animation")]
	[SerializeField]
	private Animator openMoonAnimator;

	[SerializeField]
	private float eyeOpenDistThreshold = 0.9f;

	[SerializeField]
	private float eyeCloseDistThreshold = 0.05f;

	[Header("Debug")]
	[SerializeField]
	private bool debugOverrideTimeOfDay;

	[SerializeField]
	[Range(0f, 4f)]
	private float timeOfDayOverride;

	[SerializeField]
	private bool debugOverrideCrackProgress;

	[SerializeField]
	[Range(0f, 1f)]
	private float crackProgress;

	[SerializeField]
	private bool debugOverrideCrackDayInOctober;

	[SerializeField]
	[Range(1f, 31f)]
	private int crackDayInOctoberOverride = 4;

	[SerializeField]
	private MeshRenderer crackRenderer;

	private int crackStartDayOfYear;

	private int crackEndDayOfYear;

	private float orbitAngle;

	private int eyeOpenHash;

	private bool openEyeModelEnabled;

	private float currentlySetCrackProgress;

	private MaterialPropertyBlock crackMaterialPropertyBlock;

	private bool debugDrawOrbit;

	private Dictionary<GTZone, MoonController.Scenes> zoneToSceneMapping = new Dictionary<GTZone, MoonController.Scenes>();

	private const float moonFallStart = 0.086666666f;

	private const float moonFallEnd = 0.22f;

	private const float moonRiseStart = 0.53999996f;

	private const float moonRiseEnd = 0.6733333f;

	public enum Scenes
	{
		Forest,
		Bayou,
		Beach,
		Canyon,
		Clouds,
		City,
		Metropolis,
		Mountain
	}

	[Serializable]
	public struct SceneData
	{
		public MoonController.Scenes scene;

		public Transform referencePoint;

		public bool overridePlacement;

		public MoonController.Placement PlacementOverride;
	}

	[Serializable]
	public struct Placement
	{
		public Vector2 radiusRange;

		public Vector2 heightRange;

		public Vector2 scaleRange;

		public float restAngle;
	}
}
