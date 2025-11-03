using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioLooper : MonoBehaviour
{
	protected virtual void Awake()
	{
		this.audioSource = base.GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (!this.audioSource.isPlaying)
		{
			if (this.audioSource.clip == this.loopClip && this.interjectionClips.Length != 0 && Random.value < this.interjectionLikelyhood)
			{
				this.audioSource.clip = this.interjectionClips[Random.Range(0, this.interjectionClips.Length)];
			}
			else
			{
				this.audioSource.clip = this.loopClip;
			}
			this.audioSource.GTPlay();
		}
	}

	private AudioSource audioSource;

	[SerializeField]
	private AudioClip loopClip;

	[SerializeField]
	private AudioClip[] interjectionClips;

	[SerializeField]
	private float interjectionLikelyhood = 0.5f;
}
