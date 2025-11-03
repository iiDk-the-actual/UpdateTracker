using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTagScripts.Builder
{
	public class BuilderScaleAudioRadius : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this.useLossyScaleOnEnable)
			{
				this.setScaleNextFrame = true;
				this.enableFrame = Time.frameCount;
			}
		}

		private void OnDisable()
		{
			if (this.useLossyScaleOnEnable)
			{
				this.RevertScale();
			}
		}

		private void LateUpdate()
		{
			if (this.setScaleNextFrame && Time.frameCount > this.enableFrame)
			{
				if (this.useLossyScaleOnEnable)
				{
					this.SetScale(base.transform.lossyScale.x);
				}
				this.setScaleNextFrame = false;
			}
		}

		private void PlaySound()
		{
			if (this.autoPlaySoundBank != null)
			{
				this.autoPlaySoundBank.Play();
				return;
			}
			if (this.audioSource.clip != null)
			{
				this.audioSource.Play();
			}
		}

		public void SetScale(float inScale)
		{
			if (Mathf.Approximately(inScale, this.scale))
			{
				if (this.autoPlay)
				{
					this.PlaySound();
				}
				return;
			}
			this.scale = inScale;
			this.RevertScale();
			if (Mathf.Approximately(this.scale, 1f))
			{
				if (this.autoPlay)
				{
					this.PlaySound();
				}
				return;
			}
			AudioRolloffMode rolloffMode = this.audioSource.rolloffMode;
			if (rolloffMode > AudioRolloffMode.Linear)
			{
				if (rolloffMode == AudioRolloffMode.Custom)
				{
					this.maxDist = this.audioSource.maxDistance;
					this.audioSource.maxDistance *= this.scale;
				}
			}
			else
			{
				this.minDist = this.audioSource.minDistance;
				this.maxDist = this.audioSource.maxDistance;
				this.audioSource.maxDistance *= this.scale;
				this.audioSource.minDistance *= this.scale;
			}
			if (this.autoPlay)
			{
				this.PlaySound();
			}
			this.shouldRevert = true;
		}

		public void RevertScale()
		{
			if (!this.shouldRevert)
			{
				return;
			}
			AudioRolloffMode rolloffMode = this.audioSource.rolloffMode;
			if (rolloffMode > AudioRolloffMode.Linear)
			{
				if (rolloffMode == AudioRolloffMode.Custom)
				{
					this.audioSource.maxDistance = this.maxDist;
				}
			}
			else
			{
				this.audioSource.minDistance = this.minDist;
				this.audioSource.maxDistance = this.maxDist;
			}
			this.scale = 1f;
			this.shouldRevert = false;
		}

		[Tooltip("Scale particles on enable using lossy scale")]
		[SerializeField]
		private bool useLossyScaleOnEnable;

		[Tooltip("Play sound after scaling")]
		[SerializeField]
		private bool autoPlay;

		[SerializeField]
		private AudioSource audioSource;

		[FormerlySerializedAs("soundBankToPlay")]
		[SerializeField]
		private SoundBankPlayer autoPlaySoundBank;

		private float minDist;

		private float maxDist = 1f;

		private AnimationCurve customCurve;

		private AnimationCurve scaledCurve = new AnimationCurve();

		private float scale = 1f;

		private bool shouldRevert;

		private bool setScaleNextFrame;

		private int enableFrame;
	}
}
