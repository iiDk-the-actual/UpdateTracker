using System;
using System.Collections;
using UnityEngine;

public class TimeOfDayDependentAudio : MonoBehaviour, IGorillaSliceableSimple, IBuildValidation
{
	private void Awake()
	{
		this.stepTime = 1f;
		if (this.myParticleSystem != null)
		{
			this.myEmissionModule = this.myParticleSystem.emission;
			this.startingEmissionRate = this.myEmissionModule.rateOverTime.constant;
		}
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.FixedUpdate);
		base.StopAllCoroutines();
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.FixedUpdate);
		base.StartCoroutine(this.UpdateTimeOfDay());
	}

	public void SliceUpdate()
	{
		this.isModified = false;
	}

	private IEnumerator UpdateTimeOfDay()
	{
		yield return 0;
		for (;;)
		{
			if (BetterDayNightManager.instance != null)
			{
				if (this.isModified)
				{
					this.positionMultiplier = this.positionMultiplierSet;
				}
				else
				{
					this.positionMultiplier = 1f;
				}
				if (this.myWeather == BetterDayNightManager.WeatherType.All || BetterDayNightManager.instance.CurrentWeather() == this.myWeather || BetterDayNightManager.instance.NextWeather() == this.myWeather)
				{
					if (!this.dependentStuff.activeSelf && (!this.includesAudio || this.dependentStuff != this.timeOfDayDependent))
					{
						this.dependentStuff.SetActive(true);
					}
					if (this.includesAudio)
					{
						if (this.timeOfDayDependent != null)
						{
							if (this.volumes[BetterDayNightManager.instance.currentTimeIndex] == 0f)
							{
								if (this.timeOfDayDependent.activeSelf)
								{
									this.timeOfDayDependent.SetActive(false);
								}
							}
							else if (!this.timeOfDayDependent.activeSelf)
							{
								this.timeOfDayDependent.SetActive(true);
							}
						}
						if (this.volumes[BetterDayNightManager.instance.currentTimeIndex] != this.audioSources[0].volume)
						{
							if (BetterDayNightManager.instance.currentLerp < 0.05f)
							{
								this.currentVolume = Mathf.Lerp(this.currentVolume, this.volumes[BetterDayNightManager.instance.currentTimeIndex], BetterDayNightManager.instance.currentLerp * 20f);
							}
							else
							{
								this.currentVolume = this.volumes[BetterDayNightManager.instance.currentTimeIndex];
							}
						}
					}
					if (this.myWeather == BetterDayNightManager.WeatherType.All || BetterDayNightManager.instance.CurrentWeather() == this.myWeather)
					{
						if (this.myWeather == BetterDayNightManager.WeatherType.All || BetterDayNightManager.instance.NextWeather() == this.myWeather)
						{
							if (this.myParticleSystem != null)
							{
								this.newRate = this.startingEmissionRate;
							}
							if (this.includesAudio && this.myParticleSystem != null)
							{
								this.currentVolume = Mathf.Lerp(this.volumes[BetterDayNightManager.instance.currentTimeIndex], this.volumes[(BetterDayNightManager.instance.currentTimeIndex + 1) % this.volumes.Length], BetterDayNightManager.instance.currentLerp);
							}
							else if (this.includesAudio)
							{
								if (BetterDayNightManager.instance.currentLerp < 0.05f)
								{
									this.currentVolume = Mathf.Lerp(this.currentVolume, this.volumes[BetterDayNightManager.instance.currentTimeIndex], BetterDayNightManager.instance.currentLerp * 20f);
								}
								else
								{
									this.currentVolume = this.volumes[BetterDayNightManager.instance.currentTimeIndex];
								}
							}
						}
						else
						{
							if (this.myParticleSystem != null)
							{
								this.newRate = ((BetterDayNightManager.instance.currentLerp < 0.5f) ? Mathf.Lerp(this.startingEmissionRate, 0f, BetterDayNightManager.instance.currentLerp * 2f) : 0f);
							}
							if (this.includesAudio)
							{
								this.currentVolume = ((BetterDayNightManager.instance.currentLerp < 0.5f) ? Mathf.Lerp(this.volumes[BetterDayNightManager.instance.currentTimeIndex], 0f, BetterDayNightManager.instance.currentLerp * 2f) : 0f);
							}
						}
					}
					else
					{
						if (this.myParticleSystem != null)
						{
							this.newRate = ((BetterDayNightManager.instance.currentLerp > 0.5f) ? Mathf.Lerp(0f, this.startingEmissionRate, (BetterDayNightManager.instance.currentLerp - 0.5f) * 2f) : 0f);
						}
						if (this.includesAudio)
						{
							this.currentVolume = ((BetterDayNightManager.instance.currentLerp > 0.5f) ? Mathf.Lerp(0f, this.volumes[(BetterDayNightManager.instance.currentTimeIndex + 1) % this.volumes.Length], (BetterDayNightManager.instance.currentLerp - 0.5f) * 2f) : 0f);
						}
					}
					if (this.myParticleSystem != null)
					{
						this.myEmissionModule = this.myParticleSystem.emission;
						this.myEmissionModule.rateOverTime = this.newRate;
					}
					if (this.includesAudio)
					{
						for (int i = 0; i < this.audioSources.Length; i++)
						{
							MusicSource component = this.audioSources[i].gameObject.GetComponent<MusicSource>();
							if (!(component != null) || !component.VolumeOverridden)
							{
								this.audioSources[i].volume = this.currentVolume * this.positionMultiplier;
								this.audioSources[i].enabled = this.currentVolume != 0f;
							}
						}
					}
				}
				else if (this.dependentStuff.activeSelf)
				{
					this.dependentStuff.SetActive(false);
				}
			}
			yield return new WaitForSeconds(this.stepTime);
		}
		yield break;
	}

	public bool BuildValidationCheck()
	{
		for (int i = 0; i < this.audioSources.Length; i++)
		{
			if (this.audioSources[i] == null)
			{
				Debug.LogError("audio source array contains null references", this);
				return false;
			}
		}
		return true;
	}

	public AudioSource[] audioSources;

	public float[] volumes;

	public float currentVolume;

	public float stepTime;

	public BetterDayNightManager.WeatherType myWeather;

	public GameObject dependentStuff;

	public GameObject timeOfDayDependent;

	public bool includesAudio;

	public ParticleSystem myParticleSystem;

	private float startingEmissionRate;

	private int lastEmission;

	private int nextEmission;

	private ParticleSystem.MinMaxCurve newCurve;

	private ParticleSystem.EmissionModule myEmissionModule;

	private float newRate;

	public float positionMultiplierSet;

	public float positionMultiplier = 1f;

	public bool isModified;
}
