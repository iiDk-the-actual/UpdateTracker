using System;
using UnityEngine;

namespace GorillaTagScripts
{
	public class UnMuteAudioSourceOnEnable : MonoBehaviour
	{
		public void Awake()
		{
			this.originalVolume = this.audioSource.volume;
		}

		public void OnEnable()
		{
			this.audioSource.volume = this.originalVolume;
		}

		public void OnDisable()
		{
			this.audioSource.volume = 0f;
		}

		public AudioSource audioSource;

		public float originalVolume;
	}
}
