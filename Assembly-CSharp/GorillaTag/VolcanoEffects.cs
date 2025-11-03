using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaTag.GuidedRefs;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag
{
	public class VolcanoEffects : BaseGuidedRefTargetMono
	{
		protected override void Awake()
		{
			base.Awake();
			if (this.RemoveNullsFromArray<ParticleSystem>(ref this.lavaSpewParticleSystems))
			{
				this.LogNullsFoundInArray("lavaSpewParticleSystems");
			}
			if (this.RemoveNullsFromArray<ParticleSystem>(ref this.smokeParticleSystems))
			{
				this.LogNullsFoundInArray("smokeParticleSystems");
			}
			this.hasVolcanoAudioSrc = this.volcanoAudioSource != null;
			this.hasForestSpeakerAudioSrc = this.forestSpeakerAudioSrc != null;
			this.lavaSpewEmissionModules = new ParticleSystem.EmissionModule[this.lavaSpewParticleSystems.Length];
			this.lavaSpewEmissionDefaultRateMultipliers = new float[this.lavaSpewParticleSystems.Length];
			this.lavaSpewDefaultEmitBursts = new ParticleSystem.Burst[this.lavaSpewParticleSystems.Length][];
			this.lavaSpewAdjustedEmitBursts = new ParticleSystem.Burst[this.lavaSpewParticleSystems.Length][];
			for (int i = 0; i < this.lavaSpewParticleSystems.Length; i++)
			{
				ParticleSystem.EmissionModule emission = this.lavaSpewParticleSystems[i].emission;
				this.lavaSpewEmissionDefaultRateMultipliers[i] = emission.rateOverTimeMultiplier;
				this.lavaSpewDefaultEmitBursts[i] = new ParticleSystem.Burst[emission.burstCount];
				this.lavaSpewAdjustedEmitBursts[i] = new ParticleSystem.Burst[emission.burstCount];
				for (int j = 0; j < emission.burstCount; j++)
				{
					ParticleSystem.Burst burst = emission.GetBurst(j);
					this.lavaSpewDefaultEmitBursts[i][j] = burst;
					this.lavaSpewAdjustedEmitBursts[i][j] = new ParticleSystem.Burst(burst.time, burst.minCount, burst.maxCount, burst.cycleCount, burst.repeatInterval);
					this.lavaSpewAdjustedEmitBursts[i][j].count = burst.count;
				}
				this.lavaSpewEmissionModules[i] = emission;
			}
			this.smokeMainModules = new ParticleSystem.MainModule[this.smokeParticleSystems.Length];
			this.smokeEmissionModules = new ParticleSystem.EmissionModule[this.smokeParticleSystems.Length];
			this.smokeEmissionDefaultRateMultipliers = new float[this.smokeParticleSystems.Length];
			for (int k = 0; k < this.smokeParticleSystems.Length; k++)
			{
				this.smokeMainModules[k] = this.smokeParticleSystems[k].main;
				this.smokeEmissionModules[k] = this.smokeParticleSystems[k].emission;
				this.smokeEmissionDefaultRateMultipliers[k] = this.smokeEmissionModules[k].rateOverTimeMultiplier;
			}
			this.InitState(this.drainedStateFX);
			this.InitState(this.eruptingStateFX);
			this.InitState(this.risingStateFX);
			this.InitState(this.fullStateFX);
			this.InitState(this.drainingStateFX);
			this.currentStateFX = this.drainedStateFX;
			this.UpdateDrainedState(0f);
		}

		public void OnVolcanoBellyEmpty()
		{
			if (!this.hasForestSpeakerAudioSrc)
			{
				return;
			}
			if (Time.time - this.timeVolcanoBellyWasLastEmpty < this.warnVolcanoBellyEmptied.length)
			{
				return;
			}
			this.forestSpeakerAudioSrc.gameObject.SetActive(true);
			this.forestSpeakerAudioSrc.GTPlayOneShot(this.warnVolcanoBellyEmptied, 1f);
		}

		public void OnStoneAccepted(double activationProgress)
		{
			if (!this.hasVolcanoAudioSrc)
			{
				return;
			}
			this.volcanoAudioSource.gameObject.SetActive(true);
			if (activationProgress > 1.0)
			{
				this.volcanoAudioSource.GTPlayOneShot(this.volcanoAcceptLastStone, 1f);
				return;
			}
			this.volcanoAudioSource.GTPlayOneShot(this.volcanoAcceptStone, 1f);
		}

		private void InitState(VolcanoEffects.LavaStateFX fx)
		{
			fx.startSoundExists = fx.startSound != null;
			fx.endSoundExists = fx.endSound != null;
			fx.loop1Exists = fx.loop1AudioSrc != null;
			fx.loop2Exists = fx.loop2AudioSrc != null;
			if (fx.loop1Exists)
			{
				fx.loop1DefaultVolume = fx.loop1AudioSrc.volume;
				fx.loop1AudioSrc.volume = 0f;
			}
			if (fx.loop2Exists)
			{
				fx.loop2DefaultVolume = fx.loop2AudioSrc.volume;
				fx.loop2AudioSrc.volume = 0f;
			}
		}

		private void SetLavaAudioEnabled(bool toEnable)
		{
			AudioSource[] array = this.lavaSurfaceAudioSrcs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(toEnable);
			}
		}

		private void SetLavaAudioEnabled(bool toEnable, float volume)
		{
			foreach (AudioSource audioSource in this.lavaSurfaceAudioSrcs)
			{
				audioSource.volume = volume;
				audioSource.gameObject.SetActive(toEnable);
			}
		}

		private void ResetState()
		{
			if (this.currentStateFX == null)
			{
				return;
			}
			this.currentStateFX.startSoundPlayed = false;
			this.currentStateFX.endSoundPlayed = false;
			if (this.currentStateFX.startSoundExists)
			{
				this.currentStateFX.startSoundAudioSrc.gameObject.SetActive(false);
			}
			if (this.currentStateFX.endSoundExists)
			{
				this.currentStateFX.endSoundAudioSrc.gameObject.SetActive(false);
			}
			if (this.currentStateFX.loop1Exists)
			{
				this.currentStateFX.loop1AudioSrc.gameObject.SetActive(false);
			}
			if (this.currentStateFX.loop2Exists)
			{
				this.currentStateFX.loop2AudioSrc.gameObject.SetActive(false);
			}
		}

		private void UpdateState(float time, float timeRemaining, float progress)
		{
			if (this.currentStateFX == null)
			{
				return;
			}
			if (this.currentStateFX.startSoundExists && !this.currentStateFX.startSoundPlayed && time >= this.currentStateFX.startSoundDelay)
			{
				this.currentStateFX.startSoundPlayed = true;
				this.currentStateFX.startSoundAudioSrc.gameObject.SetActive(true);
				this.currentStateFX.startSoundAudioSrc.GTPlayOneShot(this.currentStateFX.startSound, this.currentStateFX.startSoundVol);
			}
			if (this.currentStateFX.endSoundExists && !this.currentStateFX.endSoundPlayed && timeRemaining <= this.currentStateFX.endSound.length + this.currentStateFX.endSoundPadTime)
			{
				this.currentStateFX.endSoundPlayed = true;
				this.currentStateFX.endSoundAudioSrc.gameObject.SetActive(true);
				this.currentStateFX.endSoundAudioSrc.GTPlayOneShot(this.currentStateFX.endSound, this.currentStateFX.endSoundVol);
			}
			if (this.currentStateFX.loop1Exists)
			{
				this.currentStateFX.loop1AudioSrc.volume = this.currentStateFX.loop1VolAnim.Evaluate(progress) * this.currentStateFX.loop1DefaultVolume;
				if (!this.currentStateFX.loop1AudioSrc.isPlaying)
				{
					this.currentStateFX.loop1AudioSrc.gameObject.SetActive(true);
					this.currentStateFX.loop1AudioSrc.GTPlay();
				}
			}
			if (this.currentStateFX.loop2Exists)
			{
				this.currentStateFX.loop2AudioSrc.volume = this.currentStateFX.loop2VolAnim.Evaluate(progress) * this.currentStateFX.loop2DefaultVolume;
				if (!this.currentStateFX.loop2AudioSrc.isPlaying)
				{
					this.currentStateFX.loop2AudioSrc.gameObject.SetActive(true);
					this.currentStateFX.loop2AudioSrc.GTPlay();
				}
			}
			for (int i = 0; i < this.smokeMainModules.Length; i++)
			{
				this.smokeMainModules[i].startColor = this.currentStateFX.smokeStartColorAnim.Evaluate(progress);
				this.smokeEmissionModules[i].rateOverTimeMultiplier = this.currentStateFX.smokeEmissionAnim.Evaluate(progress) * this.smokeEmissionDefaultRateMultipliers[i];
			}
			this.SetParticleEmissionRateAndBurst(this.currentStateFX.lavaSpewEmissionAnim.Evaluate(progress), this.lavaSpewEmissionModules, this.lavaSpewEmissionDefaultRateMultipliers, this.lavaSpewDefaultEmitBursts, this.lavaSpewAdjustedEmitBursts);
			if (this.applyShaderGlobals)
			{
				Shader.SetGlobalColor(this.shaderProp_ZoneLiquidLightColor, this.currentStateFX.lavaLightColor.Evaluate(progress) * this.currentStateFX.lavaLightIntensityAnim.Evaluate(progress));
				Shader.SetGlobalFloat(this.shaderProp_ZoneLiquidLightDistScale, this.currentStateFX.lavaLightAttenuationAnim.Evaluate(progress));
			}
		}

		public void SetDrainedState()
		{
			this.ResetState();
			this.SetLavaAudioEnabled(false);
			this.currentStateFX = this.drainedStateFX;
		}

		public void UpdateDrainedState(float time)
		{
			this.ResetState();
			this.UpdateState(time, float.MaxValue, float.MinValue);
		}

		public void SetEruptingState()
		{
			this.ResetState();
			this.SetLavaAudioEnabled(false, 0f);
			this.currentStateFX = this.eruptingStateFX;
		}

		public void UpdateEruptingState(float time, float timeRemaining, float progress)
		{
			this.UpdateState(time, timeRemaining, progress);
		}

		public void SetRisingState()
		{
			this.ResetState();
			this.SetLavaAudioEnabled(true, 0f);
			this.currentStateFX = this.risingStateFX;
		}

		public void UpdateRisingState(float time, float timeRemaining, float progress)
		{
			this.UpdateState(time, timeRemaining, progress);
			AudioSource[] array = this.lavaSurfaceAudioSrcs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].volume = Mathf.Lerp(0f, 1f, Mathf.Clamp01(time));
			}
		}

		public void SetFullState()
		{
			this.ResetState();
			this.SetLavaAudioEnabled(true, 1f);
			this.currentStateFX = this.fullStateFX;
		}

		public void UpdateFullState(float time, float timeRemaining, float progress)
		{
			this.UpdateState(time, timeRemaining, progress);
		}

		public void SetDrainingState()
		{
			this.ResetState();
			this.SetLavaAudioEnabled(true, 1f);
			this.currentStateFX = this.drainingStateFX;
		}

		public void UpdateDrainingState(float time, float timeRemaining, float progress)
		{
			this.UpdateState(time, timeRemaining, progress);
			AudioSource[] array = this.lavaSurfaceAudioSrcs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].volume = Mathf.Lerp(1f, 0f, progress);
			}
		}

		private void SetParticleEmissionRateAndBurst(float multiplier, ParticleSystem.EmissionModule[] emissionModules, float[] defaultRateMultipliers, ParticleSystem.Burst[][] defaultEmitBursts, ParticleSystem.Burst[][] adjustedEmitBursts)
		{
			for (int i = 0; i < emissionModules.Length; i++)
			{
				emissionModules[i].rateOverTimeMultiplier = multiplier * defaultRateMultipliers[i];
				int num = Mathf.Min(emissionModules[i].burstCount, defaultEmitBursts[i].Length);
				for (int j = 0; j < num; j++)
				{
					adjustedEmitBursts[i][j].probability = defaultEmitBursts[i][j].probability * multiplier;
				}
				emissionModules[i].SetBursts(adjustedEmitBursts[i]);
			}
		}

		private bool RemoveNullsFromArray<T>(ref T[] array) where T : Object
		{
			List<T> list = new List<T>(array.Length);
			foreach (T t in array)
			{
				if (t != null)
				{
					list.Add(t);
				}
			}
			int num = array.Length;
			array = list.ToArray();
			return num != array.Length;
		}

		private void LogNullsFoundInArray(string nameOfArray)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Null reference found in ",
				nameOfArray,
				" array of component: \"",
				this.GetComponentPath(int.MaxValue),
				"\""
			}), this);
		}

		[Tooltip("Only one VolcanoEffects should change shader globals in the scene (lava color, lava light) at a time.")]
		[SerializeField]
		private bool applyShaderGlobals = true;

		[Tooltip("Game trigger notification sounds will play through this.")]
		[SerializeField]
		private AudioSource forestSpeakerAudioSrc;

		[Tooltip("The accumulator value of rocks being thrown into the volcano has been reset.")]
		[SerializeField]
		private AudioClip warnVolcanoBellyEmptied;

		[Tooltip("Accept stone sounds will play through here.")]
		[SerializeField]
		private AudioSource volcanoAudioSource;

		[Tooltip("volcano ate rock but needs more.")]
		[SerializeField]
		private AudioClip volcanoAcceptStone;

		[Tooltip("volcano ate last needed rock.")]
		[SerializeField]
		private AudioClip volcanoAcceptLastStone;

		[Tooltip("This will be faded in while lava is rising.")]
		[SerializeField]
		private AudioSource[] lavaSurfaceAudioSrcs;

		[Tooltip("Emission will be adjusted for these particles during eruption.")]
		[SerializeField]
		private ParticleSystem[] lavaSpewParticleSystems;

		[Tooltip("Smoke emits during all states but it's intensity and color will change when erupting/idling.")]
		[SerializeField]
		private ParticleSystem[] smokeParticleSystems;

		[SerializeField]
		private VolcanoEffects.LavaStateFX drainedStateFX;

		[SerializeField]
		private VolcanoEffects.LavaStateFX eruptingStateFX;

		[SerializeField]
		private VolcanoEffects.LavaStateFX risingStateFX;

		[SerializeField]
		private VolcanoEffects.LavaStateFX fullStateFX;

		[SerializeField]
		private VolcanoEffects.LavaStateFX drainingStateFX;

		private VolcanoEffects.LavaStateFX currentStateFX;

		private ParticleSystem.EmissionModule[] lavaSpewEmissionModules;

		private float[] lavaSpewEmissionDefaultRateMultipliers;

		private ParticleSystem.Burst[][] lavaSpewDefaultEmitBursts;

		private ParticleSystem.Burst[][] lavaSpewAdjustedEmitBursts;

		private ParticleSystem.MainModule[] smokeMainModules;

		private ParticleSystem.EmissionModule[] smokeEmissionModules;

		private float[] smokeEmissionDefaultRateMultipliers;

		private int shaderProp_ZoneLiquidLightColor = Shader.PropertyToID("_ZoneLiquidLightColor");

		private int shaderProp_ZoneLiquidLightDistScale = Shader.PropertyToID("_ZoneLiquidLightDistScale");

		private float timeVolcanoBellyWasLastEmpty;

		private bool hasVolcanoAudioSrc;

		private bool hasForestSpeakerAudioSrc;

		[Serializable]
		public class LavaStateFX
		{
			public AudioClip startSound;

			public AudioSource startSoundAudioSrc;

			[Tooltip("Multiplied by the AudioSource's volume.")]
			public float startSoundVol = 1f;

			[FormerlySerializedAs("startSoundPad")]
			public float startSoundDelay;

			public AudioClip endSound;

			public AudioSource endSoundAudioSrc;

			[Tooltip("Multiplied by the AudioSource's volume.")]
			public float endSoundVol = 1f;

			[Tooltip("How much time should there be between the end of the clip playing and the end of the state.")]
			public float endSoundPadTime;

			public AudioSource loop1AudioSrc;

			public AnimationCurve loop1VolAnim;

			public AudioSource loop2AudioSrc;

			public AnimationCurve loop2VolAnim;

			public AnimationCurve lavaSpewEmissionAnim;

			public AnimationCurve smokeEmissionAnim;

			public Gradient smokeStartColorAnim;

			public Gradient lavaLightColor;

			public AnimationCurve lavaLightIntensityAnim = AnimationCurve.Constant(0f, 1f, 60f);

			public AnimationCurve lavaLightAttenuationAnim = AnimationCurve.Constant(0f, 1f, 0.1f);

			[NonSerialized]
			public bool startSoundExists;

			[NonSerialized]
			public bool startSoundPlayed;

			[NonSerialized]
			public bool endSoundExists;

			[NonSerialized]
			public bool endSoundPlayed;

			[NonSerialized]
			public bool loop1Exists;

			[NonSerialized]
			public float loop1DefaultVolume;

			[NonSerialized]
			public bool loop2Exists;

			[NonSerialized]
			public float loop2DefaultVolume;
		}
	}
}
