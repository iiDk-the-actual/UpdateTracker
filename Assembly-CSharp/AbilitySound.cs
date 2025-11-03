using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AbilitySound
{
	public bool IsValid()
	{
		return this.sounds != null && this.sounds.Count > 0;
	}

	private void UpdateNextSound()
	{
		AbilitySound.SoundSelectMode soundSelectMode = this.soundSelectMode;
		if (soundSelectMode == AbilitySound.SoundSelectMode.Sequential)
		{
			this.nextSound = (this.nextSound + 1) % this.sounds.Count;
			return;
		}
		if (soundSelectMode != AbilitySound.SoundSelectMode.Random)
		{
			return;
		}
		this.nextSound = Random.Range(0, this.sounds.Count);
	}

	public void Play(AudioSource audioSourceIn)
	{
		this.usedAudioSource = ((audioSourceIn != null) ? audioSourceIn : this.audioSource);
		if (this.sounds != null && this.sounds.Count > 0 && this.usedAudioSource != null)
		{
			if (this.nextSound < 0)
			{
				this.UpdateNextSound();
			}
			AudioClip audioClip = this.sounds[this.nextSound];
			this.UpdateNextSound();
			if (audioClip != null)
			{
				this.usedAudioSource.clip = audioClip;
				this.usedAudioSource.volume = this.volume;
				this.usedAudioSource.pitch = this.pitch;
				this.usedAudioSource.loop = this.loop;
				if (this.delay <= 0f)
				{
					this.usedAudioSource.Play();
				}
				else
				{
					this.usedAudioSource.PlayDelayed(this.delay);
				}
				this.currentSound = audioClip;
			}
		}
	}

	public void Stop()
	{
		if (this.usedAudioSource != null && this.usedAudioSource.clip == this.currentSound)
		{
			this.usedAudioSource.Stop();
			this.currentSound = null;
			this.usedAudioSource = null;
		}
	}

	public float volume = 1f;

	public float pitch = 1f;

	public bool loop;

	public float delay;

	public List<AudioClip> sounds;

	private AudioClip currentSound;

	public AudioSource audioSource;

	private AudioSource usedAudioSource;

	private int nextSound = -1;

	public AbilitySound.SoundSelectMode soundSelectMode;

	public enum SoundSelectMode
	{
		Sequential,
		Random
	}
}
