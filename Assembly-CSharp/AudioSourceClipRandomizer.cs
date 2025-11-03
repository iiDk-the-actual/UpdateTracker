using System;
using GorillaNetworking;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceClipRandomizer : MonoBehaviour
{
	private void Awake()
	{
		this.source = base.GetComponent<AudioSource>();
		this.playOnAwake = this.source.playOnAwake;
		this.source.playOnAwake = false;
	}

	public void Play()
	{
		int num = Random.Range(0, 60);
		if (GorillaComputer.instance != null)
		{
			num = GorillaComputer.instance.GetServerTime().Second;
		}
		this.source.clip = this.clips[num % this.clips.Length];
		this.source.GTPlay();
	}

	private void OnEnable()
	{
		if (this.playOnAwake)
		{
			this.Play();
		}
	}

	[SerializeField]
	private AudioClip[] clips;

	private AudioSource source;

	private bool playOnAwake;
}
