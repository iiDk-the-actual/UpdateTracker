using System;
using UnityEngine;

public class RandomAudioStart : MonoBehaviour, IBuildValidation
{
	public bool BuildValidationCheck()
	{
		if (this.audioSource == null)
		{
			Debug.LogError("audio source is missing for RandomAudioStart, it won't work correctly", base.gameObject);
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		this.audioSource.time = Random.value * this.audioSource.clip.length;
	}

	[ContextMenu("Assign Audio Source")]
	public void AssignAudioSource()
	{
		this.audioSource = base.GetComponent<AudioSource>();
	}

	public AudioSource audioSource;
}
