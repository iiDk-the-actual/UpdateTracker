using System;
using UnityEngine;

public class AnimationEventListener : MonoBehaviour
{
	public void PlaySoundAtIndex(int index)
	{
		if (this.audioClips.Length <= index || index < 0)
		{
			return;
		}
		if (this.audioSource == null)
		{
			return;
		}
		if (this.audioClips[index] == null)
		{
			return;
		}
		this.audioSource.GTPlayOneShot(this.audioClips[index], 1f);
	}

	public void StopAudio()
	{
		if (this.audioSource == null)
		{
			return;
		}
		if (this.audioSource.isPlaying)
		{
			this.audioSource.Stop();
		}
	}

	public void ActivateObject()
	{
		if (this.targetObject != null)
		{
			this.targetObject.SetActive(true);
		}
	}

	public void DeactivateObject()
	{
		if (this.targetObject != null)
		{
			this.targetObject.SetActive(false);
		}
	}

	public void ToggleObject()
	{
		if (this.targetObject != null)
		{
			this.targetObject.SetActive(!this.targetObject.activeSelf);
		}
	}

	public void PlayParticles()
	{
		if (this.particles != null && !this.particles.isPlaying)
		{
			this.particles.Play();
		}
	}

	public void StopParticles()
	{
		if (this.particles != null && this.particles.isPlaying)
		{
			this.particles.Stop();
		}
	}

	[Tooltip("Set this if calling ActivateObject, DeactivateObject, or ToggleObject")]
	[SerializeField]
	private GameObject targetObject;

	[Tooltip("Set this if calling PlayParticles or StopParticles")]
	[SerializeField]
	private ParticleSystem particles;

	[Tooltip("Set this if calling PlaySoundAtIndex or StopAudio")]
	[SerializeField]
	private AudioSource audioSource;

	[Tooltip("Set this if calling PlaySoundAtIndex")]
	[SerializeField]
	private AudioClip[] audioClips;
}
