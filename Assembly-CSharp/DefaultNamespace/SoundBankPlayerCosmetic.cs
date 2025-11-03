using System;
using UnityEngine;

namespace DefaultNamespace
{
	[RequireComponent(typeof(SoundBankPlayer))]
	public class SoundBankPlayerCosmetic : MonoBehaviour, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		private void Awake()
		{
			this.playAudioLoop = false;
		}

		private void OnEnable()
		{
			TickSystem<object>.AddTickCallback(this);
		}

		private void OnDisable()
		{
			TickSystem<object>.RemoveTickCallback(this);
		}

		public void Tick()
		{
			if (!this.playAudioLoop)
			{
				return;
			}
			if (this.soundBankPlayer != null && this.soundBankPlayer.audioSource != null && this.soundBankPlayer.soundBank != null && !this.soundBankPlayer.audioSource.isPlaying)
			{
				this.soundBankPlayer.Play();
			}
		}

		public void PlayAudio()
		{
			if (this.soundBankPlayer != null && this.soundBankPlayer.audioSource != null && this.soundBankPlayer.soundBank != null)
			{
				this.soundBankPlayer.Play();
			}
		}

		public void PlayAudioLoop()
		{
			this.playAudioLoop = true;
		}

		public void PlayAudioNonInterrupting()
		{
			if (this.soundBankPlayer != null && this.soundBankPlayer.audioSource != null && this.soundBankPlayer.soundBank != null)
			{
				if (this.soundBankPlayer.audioSource.isPlaying)
				{
					return;
				}
				this.soundBankPlayer.Play();
			}
		}

		public void PlayAudioWithTunableVolume(bool leftHand, float fingerValue)
		{
			if (this.soundBankPlayer != null && this.soundBankPlayer.audioSource != null && this.soundBankPlayer.soundBank != null)
			{
				float num = Mathf.Clamp01(fingerValue);
				this.soundBankPlayer.audioSource.volume = num;
				this.soundBankPlayer.Play();
			}
		}

		public void StopAudio()
		{
			if (this.soundBankPlayer != null && this.soundBankPlayer.audioSource != null && this.soundBankPlayer.soundBank != null)
			{
				this.soundBankPlayer.audioSource.Stop();
			}
			this.playAudioLoop = false;
		}

		[SerializeField]
		private SoundBankPlayer soundBankPlayer;

		private bool playAudioLoop;
	}
}
