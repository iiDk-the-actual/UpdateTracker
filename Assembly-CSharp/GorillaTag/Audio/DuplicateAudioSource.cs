using System;
using UnityEngine;

namespace GorillaTag.Audio
{
	public class DuplicateAudioSource : MonoBehaviour
	{
		public void SetTargetAudioSource(AudioSource target)
		{
			this.TargetAudioSource = target;
			this.StartDuplicating();
		}

		[ContextMenu("Start Duplicating")]
		public void StartDuplicating()
		{
			this._isDuplicating = true;
			this._audioSource.loop = this.TargetAudioSource.loop;
			this._audioSource.clip = this.TargetAudioSource.clip;
			if (this.TargetAudioSource.isPlaying)
			{
				this._audioSource.Play();
			}
		}

		[ContextMenu("Stop Duplicating")]
		public void StopDuplicating()
		{
			this._isDuplicating = false;
			this._audioSource.Stop();
		}

		public void LateUpdate()
		{
			if (this._isDuplicating)
			{
				if (this.TargetAudioSource.isPlaying && !this._audioSource.isPlaying)
				{
					this._audioSource.Play();
					return;
				}
				if (!this.TargetAudioSource.isPlaying && this._audioSource.isPlaying)
				{
					this._audioSource.Stop();
				}
			}
		}

		public AudioSource TargetAudioSource;

		[SerializeField]
		private AudioSource _audioSource;

		[SerializeField]
		private bool _isDuplicating;
	}
}
