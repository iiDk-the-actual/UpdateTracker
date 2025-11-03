using System;
using System.Collections;
using System.Collections.Generic;
using GorillaNetworking;
using UnityEngine;

public class BetterDayNightManager : MonoBehaviour, IGorillaSliceableSimple, ITimeOfDaySystem
{
	public static void Register(PerSceneRenderData data)
	{
		BetterDayNightManager.allScenesRenderData.Add(data);
	}

	public static void Unregister(PerSceneRenderData data)
	{
		BetterDayNightManager.allScenesRenderData.Remove(data);
	}

	public string currentTimeOfDay { get; private set; }

	public float NormalizedTimeOfDay
	{
		get
		{
			return Mathf.Clamp01((float)((this.baseSeconds + (double)Time.realtimeSinceStartup * this.timeMultiplier) % this.totalSeconds / this.totalSeconds));
		}
	}

	double ITimeOfDaySystem.currentTimeInSeconds
	{
		get
		{
			return this.currentTime;
		}
	}

	double ITimeOfDaySystem.totalTimeInSeconds
	{
		get
		{
			return this.totalSeconds;
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		if (BetterDayNightManager.instance == null)
		{
			BetterDayNightManager.instance = this;
		}
		else if (BetterDayNightManager.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		this.currentLerp = 0f;
		this.totalHours = 0.0;
		for (int i = 0; i < this.timeOfDayRange.Length; i++)
		{
			this.totalHours += this.timeOfDayRange[i];
		}
		this.totalSeconds = this.totalHours * 60.0 * 60.0;
		this.currentTimeIndex = 0;
		this.baseSeconds = 0.0;
		this.computerInit = false;
		this.randomNumberGenerator = new Random(this.mySeed);
		this.GenerateWeatherEventTimes();
		this.ChangeMaps(0, 1);
		base.StartCoroutine(this.InitialUpdate());
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	protected void OnDestroy()
	{
	}

	private Vector4 MaterialColorCorrection(Vector4 color)
	{
		if (color.x < 0.5f)
		{
			color.x += 3E-08f;
		}
		if (color.y < 0.5f)
		{
			color.y += 3E-08f;
		}
		if (color.z < 0.5f)
		{
			color.z += 3E-08f;
		}
		if (color.w < 0.5f)
		{
			color.w += 3E-08f;
		}
		return color;
	}

	public void UpdateTimeOfDay()
	{
		if (Time.time < this.lastTimeChecked + this.currentTimestep)
		{
			return;
		}
		this.lastTimeChecked = Time.time;
		if (this.animatingLightFlash != null)
		{
			return;
		}
		try
		{
			if (!this.computerInit && GorillaComputer.instance != null && GorillaComputer.instance.startupMillis != 0L)
			{
				this.computerInit = true;
				this.initialDayCycles = (long)(TimeSpan.FromMilliseconds((double)GorillaComputer.instance.startupMillis).TotalSeconds * this.timeMultiplier / this.totalSeconds);
				this.currentWeatherIndex = (int)(this.initialDayCycles * (long)this.dayNightLightmapNames.Length) % this.weatherCycle.Length;
				this.baseSeconds = TimeSpan.FromMilliseconds((double)GorillaComputer.instance.startupMillis).TotalSeconds * this.timeMultiplier % this.totalSeconds;
				this.currentTime = (this.baseSeconds + (double)Time.realtimeSinceStartup * this.timeMultiplier) % this.totalSeconds;
				this.currentIndexSeconds = 0.0;
				for (int i = 0; i < this.timeOfDayRange.Length; i++)
				{
					this.currentIndexSeconds += this.timeOfDayRange[i] * 3600.0;
					if (this.currentIndexSeconds > this.currentTime)
					{
						this.currentTimeIndex = i;
						break;
					}
				}
				this.currentWeatherIndex += this.currentTimeIndex;
			}
			else if (!this.computerInit && this.baseSeconds == 0.0)
			{
				this.initialDayCycles = (long)(TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds * this.timeMultiplier / this.totalSeconds);
				this.currentWeatherIndex = (int)(this.initialDayCycles * (long)this.dayNightLightmapNames.Length) % this.weatherCycle.Length;
				this.baseSeconds = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds * this.timeMultiplier % this.totalSeconds;
				this.currentTime = this.baseSeconds % this.totalSeconds;
				this.currentIndexSeconds = 0.0;
				for (int j = 0; j < this.timeOfDayRange.Length; j++)
				{
					this.currentIndexSeconds += this.timeOfDayRange[j] * 3600.0;
					if (this.currentIndexSeconds > this.currentTime)
					{
						this.currentTimeIndex = j;
						break;
					}
				}
				this.currentWeatherIndex += this.currentTimeIndex - 1;
				if (this.currentWeatherIndex < 0)
				{
					this.currentWeatherIndex = this.weatherCycle.Length - 1;
				}
			}
			this.currentTime = ((this.currentSetting == TimeSettings.Normal) ? ((this.baseSeconds + (double)Time.realtimeSinceStartup * this.timeMultiplier) % this.totalSeconds) : this.currentTime);
			this.currentIndexSeconds = 0.0;
			for (int k = 0; k < this.timeOfDayRange.Length; k++)
			{
				this.currentIndexSeconds += this.timeOfDayRange[k] * 3600.0;
				if (this.currentIndexSeconds > this.currentTime)
				{
					this.currentTimeIndex = k;
					break;
				}
			}
			if (this.timeIndexOverrideFunc != null)
			{
				this.currentTimeIndex = this.timeIndexOverrideFunc(this.currentTimeIndex);
			}
			if (this.currentTimeIndex != this.lastIndex)
			{
				this.currentWeatherIndex = (this.currentWeatherIndex + 1) % this.weatherCycle.Length;
				this.ChangeMaps(this.currentTimeIndex, (this.currentTimeIndex + 1) % this.timeOfDayRange.Length);
			}
			this.currentLerp = (float)(1.0 - (this.currentIndexSeconds - this.currentTime) / (this.timeOfDayRange[this.currentTimeIndex] * 3600.0));
			this.ChangeLerps(this.currentLerp);
			this.lastIndex = this.currentTimeIndex;
			this.currentTimeOfDay = this.dayNightLightmapNames[this.currentTimeIndex];
		}
		catch (Exception ex)
		{
			string text = "Error in BetterDayNightManager: ";
			Exception ex2 = ex;
			Debug.LogError(text + ((ex2 != null) ? ex2.ToString() : null), this);
		}
		this.gameEpochDay = (long)((this.baseSeconds + (double)Time.realtimeSinceStartup * this.timeMultiplier) / this.totalSeconds + (double)this.initialDayCycles);
		foreach (BetterDayNightManager.ScheduledEvent scheduledEvent in BetterDayNightManager.scheduledEvents.Values)
		{
			if (scheduledEvent.lastDayCalled != this.gameEpochDay && scheduledEvent.hour == this.currentTimeIndex)
			{
				scheduledEvent.lastDayCalled = this.gameEpochDay;
				scheduledEvent.action();
			}
		}
	}

	private void ChangeLerps(float newLerp)
	{
		Shader.SetGlobalFloat(this._GlobalDayNightLerpValue, newLerp);
		for (int i = 0; i < this.standardMaterialsUnlit.Length; i++)
		{
			this.tempLerp = Mathf.Lerp(this.colorFrom, this.colorTo, newLerp);
			this.standardMaterialsUnlit[i].color = new Color(this.tempLerp, this.tempLerp, this.tempLerp);
		}
		for (int j = 0; j < this.standardMaterialsUnlitDarker.Length; j++)
		{
			this.tempLerp = Mathf.Lerp(this.colorFromDarker, this.colorToDarker, newLerp);
			Color.RGBToHSV(this.standardMaterialsUnlitDarker[j].color, out this.h, out this.s, out this.v);
			this.standardMaterialsUnlitDarker[j].color = Color.HSVToRGB(this.h, this.s, this.tempLerp);
		}
	}

	private void ChangeMaps(int fromIndex, int toIndex)
	{
		this.fromWeatherIndex = this.currentWeatherIndex;
		this.toWeatherIndex = (this.currentWeatherIndex + 1) % this.weatherCycle.Length;
		if (this.weatherCycle[this.fromWeatherIndex] == BetterDayNightManager.WeatherType.Raining)
		{
			this.fromSky = this.dayNightWeatherSkyboxTextures[fromIndex];
		}
		else
		{
			this.fromSky = this.dayNightSkyboxTextures[fromIndex];
		}
		this.fromSky2 = this.cloudsDayNightSkyboxTextures[fromIndex];
		this.fromSky3 = this.beachDayNightSkyboxTextures[fromIndex];
		if (this.weatherCycle[this.toWeatherIndex] == BetterDayNightManager.WeatherType.Raining)
		{
			this.toSky = this.dayNightWeatherSkyboxTextures[toIndex];
		}
		else
		{
			this.toSky = this.dayNightSkyboxTextures[toIndex];
		}
		this.toSky2 = this.cloudsDayNightSkyboxTextures[toIndex];
		this.toSky3 = this.beachDayNightSkyboxTextures[toIndex];
		this.PopulateAllLightmaps(fromIndex, toIndex);
		Shader.SetGlobalTexture(this._GlobalDayNightSkyTex1, this.fromSky);
		Shader.SetGlobalTexture(this._GlobalDayNightSkyTex2, this.toSky);
		Shader.SetGlobalTexture(this._GlobalDayNightSky2Tex1, this.fromSky2);
		Shader.SetGlobalTexture(this._GlobalDayNightSky2Tex2, this.toSky2);
		Shader.SetGlobalTexture(this._GlobalDayNightSky3Tex1, this.fromSky3);
		Shader.SetGlobalTexture(this._GlobalDayNightSky3Tex2, this.toSky3);
		this.colorFrom = this.standardUnlitColor[fromIndex];
		this.colorTo = this.standardUnlitColor[toIndex];
		this.colorFromDarker = this.standardUnlitColorWithPremadeColorDarker[fromIndex];
		this.colorToDarker = this.standardUnlitColorWithPremadeColorDarker[toIndex];
	}

	public void SliceUpdate()
	{
		if (!this.shouldRepopulate)
		{
			using (List<PerSceneRenderData>.Enumerator enumerator = BetterDayNightManager.allScenesRenderData.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.CheckShouldRepopulate())
					{
						this.shouldRepopulate = true;
					}
				}
			}
		}
		if (this.shouldRepopulate)
		{
			this.PopulateAllLightmaps();
			this.shouldRepopulate = false;
		}
		this.UpdateTimeOfDay();
	}

	private IEnumerator InitialUpdate()
	{
		yield return null;
		this.SliceUpdate();
		yield break;
	}

	public void RequestRepopulateLightmaps()
	{
		this.shouldRepopulate = true;
	}

	public void PopulateAllLightmaps()
	{
		this.PopulateAllLightmaps(this.currentTimeIndex, (this.currentTimeIndex + 1) % this.timeOfDayRange.Length);
	}

	public void PopulateAllLightmaps(int fromIndex, int toIndex)
	{
		string text;
		if (this.weatherCycle[this.fromWeatherIndex] == BetterDayNightManager.WeatherType.Raining)
		{
			text = this.dayNightWeatherLightmapNames[fromIndex];
		}
		else
		{
			text = this.dayNightLightmapNames[fromIndex];
		}
		string text2;
		if (this.weatherCycle[this.toWeatherIndex] == BetterDayNightManager.WeatherType.Raining)
		{
			text2 = this.dayNightWeatherLightmapNames[toIndex];
		}
		else
		{
			text2 = this.dayNightLightmapNames[toIndex];
		}
		LightmapData[] lightmaps = LightmapSettings.lightmaps;
		foreach (PerSceneRenderData perSceneRenderData in BetterDayNightManager.allScenesRenderData)
		{
			perSceneRenderData.PopulateLightmaps(text, text2, lightmaps);
		}
		LightmapSettings.lightmaps = lightmaps;
	}

	public BetterDayNightManager.WeatherType CurrentWeather()
	{
		if (!this.overrideWeather)
		{
			return this.weatherCycle[this.currentWeatherIndex];
		}
		return this.overrideWeatherType;
	}

	public BetterDayNightManager.WeatherType NextWeather()
	{
		if (!this.overrideWeather)
		{
			return this.weatherCycle[(this.currentWeatherIndex + 1) % this.weatherCycle.Length];
		}
		return this.overrideWeatherType;
	}

	public BetterDayNightManager.WeatherType LastWeather()
	{
		if (!this.overrideWeather)
		{
			return this.weatherCycle[(this.currentWeatherIndex - 1) % this.weatherCycle.Length];
		}
		return this.overrideWeatherType;
	}

	private void GenerateWeatherEventTimes()
	{
		this.weatherCycle = new BetterDayNightManager.WeatherType[100 * this.dayNightLightmapNames.Length];
		this.rainChance = this.rainChance * 2f / (float)this.maxRainDuration;
		for (int i = 1; i < this.weatherCycle.Length; i++)
		{
			this.weatherCycle[i] = (((float)this.randomNumberGenerator.Next(100) < this.rainChance * 100f) ? BetterDayNightManager.WeatherType.Raining : BetterDayNightManager.WeatherType.None);
			if (this.weatherCycle[i] == BetterDayNightManager.WeatherType.Raining)
			{
				this.rainDuration = this.randomNumberGenerator.Next(1, this.maxRainDuration + 1);
				for (int j = 1; j < this.rainDuration; j++)
				{
					if (i + j < this.weatherCycle.Length)
					{
						this.weatherCycle[i + j] = BetterDayNightManager.WeatherType.Raining;
					}
				}
				i += this.rainDuration - 1;
			}
		}
	}

	public static int RegisterScheduledEvent(int hour, Action action)
	{
		int num = (int)(DateTime.Now.Ticks % 2147483647L);
		while (BetterDayNightManager.scheduledEvents.ContainsKey(num))
		{
			num++;
		}
		BetterDayNightManager.scheduledEvents.Add(num, new BetterDayNightManager.ScheduledEvent
		{
			lastDayCalled = -1L,
			hour = hour,
			action = action
		});
		return num;
	}

	public static void UnregisterScheduledEvent(int id)
	{
		BetterDayNightManager.scheduledEvents.Remove(id);
	}

	public void SetTimeIndexOverrideFunction(Func<int, int> overrideFunction)
	{
		this.timeIndexOverrideFunc = overrideFunction;
	}

	public void UnsetTimeIndexOverrideFunction()
	{
		this.timeIndexOverrideFunc = null;
	}

	public void SetOverrideIndex(int index)
	{
		this.overrideIndex = index;
		this.currentWeatherIndex = this.overrideIndex;
		this.currentTimeIndex = this.overrideIndex;
		this.currentTimeOfDay = this.dayNightLightmapNames[this.currentTimeIndex];
		this.ChangeMaps(this.currentTimeIndex, (this.currentTimeIndex + 1) % this.timeOfDayRange.Length);
	}

	public void AnimateLightFlash(int index, float fadeInDuration, float holdDuration, float fadeOutDuration)
	{
		if (this.animatingLightFlash != null)
		{
			base.StopCoroutine(this.animatingLightFlash);
		}
		this.animatingLightFlash = base.StartCoroutine(this.AnimateLightFlashCo(index, fadeInDuration, holdDuration, fadeOutDuration));
	}

	private IEnumerator AnimateLightFlashCo(int index, float fadeInDuration, float holdDuration, float fadeOutDuration)
	{
		int startMap = ((this.currentLerp < 0.5f) ? this.currentTimeIndex : ((this.currentTimeIndex + 1) % this.timeOfDayRange.Length));
		this.ChangeMaps(startMap, index);
		float endTimestamp = Time.time + fadeInDuration;
		while (Time.time < endTimestamp)
		{
			this.ChangeLerps(1f - (endTimestamp - Time.time) / fadeInDuration);
			yield return null;
		}
		this.ChangeMaps(index, index);
		this.ChangeLerps(0f);
		endTimestamp = Time.time + fadeInDuration;
		while (Time.time < endTimestamp)
		{
			yield return null;
		}
		this.ChangeMaps(index, startMap);
		endTimestamp = Time.time + fadeOutDuration;
		while (Time.time < endTimestamp)
		{
			this.ChangeLerps(1f - (endTimestamp - Time.time) / fadeInDuration);
			yield return null;
		}
		this.ChangeMaps(this.currentTimeIndex, (this.currentTimeIndex + 1) % this.timeOfDayRange.Length);
		this.ChangeLerps(this.currentLerp);
		this.animatingLightFlash = null;
		yield break;
	}

	public void SetTimeOfDay(int timeIndex)
	{
		double num = 0.0;
		for (int i = 0; i < timeIndex; i++)
		{
			num += this.timeOfDayRange[i];
		}
		this.currentTime = num * 3600.0;
		this.currentSetting = TimeSettings.Static;
	}

	public void FastForward(float seconds)
	{
		this.baseSeconds += (double)seconds;
	}

	public void SetFixedWeather(BetterDayNightManager.WeatherType weather)
	{
		this.overrideWeather = true;
		this.overrideWeatherType = weather;
	}

	public void ClearFixedWeather()
	{
		this.overrideWeather = false;
	}

	[OnEnterPlay_SetNull]
	public static volatile BetterDayNightManager instance;

	[OnEnterPlay_Clear]
	public static List<PerSceneRenderData> allScenesRenderData = new List<PerSceneRenderData>();

	public Shader standard;

	public Shader standardCutout;

	public Shader gorillaUnlit;

	public Shader gorillaUnlitCutout;

	public Material[] standardMaterialsUnlit;

	public Material[] standardMaterialsUnlitDarker;

	public Material[] dayNightSupportedMaterials;

	public Material[] dayNightSupportedMaterialsCutout;

	public string[] dayNightLightmapNames;

	public string[] dayNightWeatherLightmapNames;

	public Texture2D[] dayNightSkyboxTextures;

	public Texture2D[] cloudsDayNightSkyboxTextures;

	public Texture2D[] beachDayNightSkyboxTextures;

	public Texture2D[] dayNightWeatherSkyboxTextures;

	public float[] standardUnlitColor;

	public float[] standardUnlitColorWithPremadeColorDarker;

	public float currentLerp;

	public float currentTimestep;

	public double[] timeOfDayRange;

	public double timeMultiplier;

	private float lastTime;

	private double currentTime;

	private double totalHours;

	private double totalSeconds;

	private float colorFrom;

	private float colorTo;

	private float colorFromDarker;

	private float colorToDarker;

	public int currentTimeIndex;

	public int currentWeatherIndex;

	private int lastIndex;

	private double currentIndexSeconds;

	private float tempLerp;

	private double baseSeconds;

	private bool computerInit;

	private float h;

	private float s;

	private float v;

	public int mySeed;

	public Random randomNumberGenerator = new Random();

	public BetterDayNightManager.WeatherType[] weatherCycle;

	public bool overrideWeather;

	public BetterDayNightManager.WeatherType overrideWeatherType;

	public float rainChance = 0.3f;

	public int maxRainDuration = 5;

	private int rainDuration;

	private float remainingSeconds;

	private long initialDayCycles;

	private long gameEpochDay;

	private int currentWeatherCycle;

	private int fromWeatherIndex;

	private int toWeatherIndex;

	private Texture2D fromSky;

	private Texture2D fromSky2;

	private Texture2D fromSky3;

	private Texture2D toSky;

	private Texture2D toSky2;

	private Texture2D toSky3;

	public AddCollidersToParticleSystemTriggers[] weatherSystems;

	public List<Collider> collidersToAddToWeatherSystems = new List<Collider>();

	private float lastTimeChecked;

	private Func<int, int> timeIndexOverrideFunc;

	public int overrideIndex = -1;

	[OnEnterPlay_Clear]
	private static readonly Dictionary<int, BetterDayNightManager.ScheduledEvent> scheduledEvents = new Dictionary<int, BetterDayNightManager.ScheduledEvent>(256);

	public TimeSettings currentSetting;

	private ShaderHashId _Color = "_Color";

	private ShaderHashId _GlobalDayNightLerpValue = "_GlobalDayNightLerpValue";

	private ShaderHashId _GlobalDayNightSkyTex1 = "_GlobalDayNightSkyTex1";

	private ShaderHashId _GlobalDayNightSkyTex2 = "_GlobalDayNightSkyTex2";

	private ShaderHashId _GlobalDayNightSky2Tex1 = "_GlobalDayNightSky2Tex1";

	private ShaderHashId _GlobalDayNightSky2Tex2 = "_GlobalDayNightSky2Tex2";

	private ShaderHashId _GlobalDayNightSky3Tex1 = "_GlobalDayNightSky3Tex1";

	private ShaderHashId _GlobalDayNightSky3Tex2 = "_GlobalDayNightSky3Tex2";

	private bool shouldRepopulate;

	private Coroutine animatingLightFlash;

	public enum WeatherType
	{
		None,
		Raining,
		All
	}

	private class ScheduledEvent
	{
		public long lastDayCalled;

		public int hour;

		public Action action;
	}
}
