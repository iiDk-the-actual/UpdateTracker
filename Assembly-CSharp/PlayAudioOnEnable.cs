using System;
using UnityEngine;

public class PlayAudioOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		this.audioSource.clip = this.audioClips[Random.Range(0, this.audioClips.Length)];
		this.audioSource.GTPlay();
	}

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip[] audioClips;
}
