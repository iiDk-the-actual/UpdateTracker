using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicSource : MonoBehaviour
{
	public AudioSource AudioSource
	{
		get
		{
			return this.audioSource;
		}
	}

	public float DefaultVolume
	{
		get
		{
			return this.defaultVolume;
		}
	}

	public bool VolumeOverridden
	{
		get
		{
			return this.volumeOverride != null;
		}
	}

	private void Awake()
	{
		if (this.audioSource == null)
		{
			this.audioSource = base.GetComponent<AudioSource>();
		}
		if (this.setDefaultVolumeFromAudioSourceOnAwake)
		{
			this.defaultVolume = this.audioSource.volume;
		}
	}

	private void OnEnable()
	{
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.RegisterMusicSource(this);
		}
	}

	private void OnDisable()
	{
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.UnregisterMusicSource(this);
		}
	}

	public void SetVolumeOverride(float volume)
	{
		this.volumeOverride = new float?(volume);
		this.audioSource.volume = this.volumeOverride.Value;
	}

	public void UnsetVolumeOverride()
	{
		this.volumeOverride = null;
		this.audioSource.volume = this.defaultVolume;
	}

	[SerializeField]
	private float defaultVolume = 1f;

	[SerializeField]
	private bool setDefaultVolumeFromAudioSourceOnAwake = true;

	private AudioSource audioSource;

	private float? volumeOverride;
}
