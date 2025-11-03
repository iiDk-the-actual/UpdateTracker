using System;
using UnityEngine;

namespace GorillaTag.Reactions
{
	public class ShakeReaction : MonoBehaviour, ITickSystemPost
	{
		private float loopSoundTotalDuration
		{
			get
			{
				return this.loopSoundFadeInDuration + this.loopSoundSustainDuration + this.loopSoundFadeOutDuration;
			}
		}

		bool ITickSystemPost.PostTickRunning { get; set; }

		protected void Awake()
		{
			this.sampleHistoryPos = new Vector3[256];
			this.sampleHistoryTime = new float[256];
			this.sampleHistoryVel = new Vector3[256];
			if (this.particles != null)
			{
				this.maxEmissionRate = this.particles.emission.rateOverTime.constant;
			}
			Application.quitting += this.HandleApplicationQuitting;
		}

		protected void OnEnable()
		{
			float unscaledTime = Time.unscaledTime;
			Vector3 position = this.shakeXform.position;
			for (int i = 0; i < 256; i++)
			{
				this.sampleHistoryTime[i] = unscaledTime;
				this.sampleHistoryPos[i] = position;
				this.sampleHistoryVel[i] = Vector3.zero;
			}
			if (this.loopSoundAudioSource != null)
			{
				this.loopSoundAudioSource.loop = true;
				this.loopSoundAudioSource.GTPlay();
			}
			this.hasLoopSound = this.loopSoundAudioSource != null;
			this.hasShakeSound = this.shakeSoundBankPlayer != null;
			this.hasParticleSystem = this.particles != null;
			TickSystem<object>.AddPostTickCallback(this);
		}

		protected void OnDisable()
		{
			if (this.loopSoundAudioSource != null)
			{
				this.loopSoundAudioSource.GTStop();
			}
			TickSystem<object>.RemovePostTickCallback(this);
		}

		private void HandleApplicationQuitting()
		{
			TickSystem<object>.RemovePostTickCallback(this);
		}

		void ITickSystemPost.PostTick()
		{
			float unscaledTime = Time.unscaledTime;
			Vector3 position = this.shakeXform.position;
			int num = (this.currentIndex - 1 + 256) % 256;
			this.currentIndex = (this.currentIndex + 1) % 256;
			this.sampleHistoryTime[this.currentIndex] = unscaledTime;
			float num2 = unscaledTime - this.sampleHistoryTime[num];
			this.sampleHistoryPos[this.currentIndex] = position;
			if (num2 > 0f)
			{
				Vector3 vector = position - this.sampleHistoryPos[num];
				this.sampleHistoryVel[this.currentIndex] = vector / num2;
			}
			else
			{
				this.sampleHistoryVel[this.currentIndex] = Vector3.zero;
			}
			float sqrMagnitude = (this.sampleHistoryVel[num] - this.sampleHistoryVel[this.currentIndex]).sqrMagnitude;
			this.poopVelocity = Mathf.Round(Mathf.Sqrt(sqrMagnitude) * 1000f) / 1000f;
			float num3 = this.shakeXform.lossyScale.x * this.velocityThreshold * this.velocityThreshold;
			if (sqrMagnitude >= num3)
			{
				this.lastShakeTime = unscaledTime;
			}
			float num4 = unscaledTime - this.lastShakeTime;
			float num5 = Mathf.Clamp01(num4 / this.particleDuration);
			if (this.hasParticleSystem)
			{
				this.particles.emission.rateOverTime = this.emissionCurve.Evaluate(num5) * this.maxEmissionRate;
			}
			if (this.hasShakeSound && this.lastShakeTime - this.lastShakeSoundTime > this.shakeSoundCooldown)
			{
				this.shakeSoundBankPlayer.Play();
				this.lastShakeSoundTime = unscaledTime;
			}
			if (this.hasLoopSound)
			{
				if (num4 < this.loopSoundFadeInDuration)
				{
					this.loopSoundAudioSource.volume = this.loopSoundBaseVolume * this.loopSoundFadeInCurve.Evaluate(Mathf.Clamp01(num4 / this.loopSoundFadeInDuration));
					return;
				}
				if (num4 < this.loopSoundFadeInDuration + this.loopSoundSustainDuration)
				{
					this.loopSoundAudioSource.volume = this.loopSoundBaseVolume;
					return;
				}
				this.loopSoundAudioSource.volume = this.loopSoundBaseVolume * this.loopSoundFadeOutCurve.Evaluate(Mathf.Clamp01((num4 - this.loopSoundFadeInDuration - this.loopSoundSustainDuration) / this.loopSoundFadeOutDuration));
			}
		}

		[SerializeField]
		private Transform shakeXform;

		[SerializeField]
		private float velocityThreshold = 5f;

		[SerializeField]
		private SoundBankPlayer shakeSoundBankPlayer;

		[SerializeField]
		private float shakeSoundCooldown = 1f;

		[SerializeField]
		private AudioSource loopSoundAudioSource;

		[SerializeField]
		private float loopSoundBaseVolume = 1f;

		[SerializeField]
		private float loopSoundSustainDuration = 1f;

		[SerializeField]
		private float loopSoundFadeInDuration = 1f;

		[SerializeField]
		private AnimationCurve loopSoundFadeInCurve;

		[SerializeField]
		private float loopSoundFadeOutDuration = 1f;

		[SerializeField]
		private AnimationCurve loopSoundFadeOutCurve;

		[SerializeField]
		private ParticleSystem particles;

		[SerializeField]
		private AnimationCurve emissionCurve;

		[SerializeField]
		private float particleDuration = 5f;

		private const int sampleHistorySize = 256;

		private float[] sampleHistoryTime;

		private Vector3[] sampleHistoryPos;

		private Vector3[] sampleHistoryVel;

		private int currentIndex;

		private float lastShakeSoundTime = float.MinValue;

		private float lastShakeTime = float.MinValue;

		private float maxEmissionRate;

		private bool hasLoopSound;

		private bool hasShakeSound;

		private bool hasParticleSystem;

		[DebugReadout]
		private float poopVelocity;
	}
}
