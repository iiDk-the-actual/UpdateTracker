using System;
using UnityEngine;

public class AudioFader : MonoBehaviour
{
	private void Start()
	{
		this.fadeInSpeed = this.maxVolume / this.fadeInDuration;
		this.fadeOutSpeed = this.maxVolume / this.fadeOutDuration;
	}

	public void FadeIn()
	{
		this.targetVolume = this.maxVolume;
		if (this.fadeInDuration > 0f)
		{
			base.enabled = true;
			this.currentFadeSpeed = this.fadeInSpeed;
		}
		else
		{
			this.currentVolume = this.maxVolume;
		}
		this.audioToFade.volume = this.currentVolume;
		if (!this.audioToFade.isPlaying)
		{
			this.audioToFade.GTPlay();
		}
	}

	public void FadeOut()
	{
		this.targetVolume = 0f;
		if (this.fadeOutDuration > 0f)
		{
			base.enabled = true;
			this.currentFadeSpeed = this.fadeOutSpeed;
		}
		else
		{
			this.currentVolume = 0f;
			if (this.audioToFade.isPlaying)
			{
				this.audioToFade.Stop();
			}
		}
		if (this.outro != null && this.currentVolume > 0f)
		{
			this.outro.volume = this.currentVolume;
			this.outro.GTPlay();
		}
	}

	private void Update()
	{
		this.currentVolume = Mathf.MoveTowards(this.currentVolume, this.targetVolume, this.currentFadeSpeed * Time.deltaTime);
		this.audioToFade.volume = this.currentVolume;
		if (this.currentVolume == this.targetVolume)
		{
			base.enabled = false;
			if (this.currentVolume == 0f && this.audioToFade.isPlaying)
			{
				this.audioToFade.Stop();
			}
		}
	}

	[SerializeField]
	private AudioSource audioToFade;

	[SerializeField]
	private AudioSource outro;

	[SerializeField]
	private float fadeInDuration = 0.3f;

	[SerializeField]
	private float fadeOutDuration = 0.3f;

	[SerializeField]
	private float maxVolume = 1f;

	private float currentVolume;

	private float targetVolume;

	private float currentFadeSpeed;

	private float fadeInSpeed;

	private float fadeOutSpeed;
}
