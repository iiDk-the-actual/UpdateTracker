using System;
using GorillaLocomotion;
using UnityEngine;

public class DampenVolumeSpace : MonoBehaviour
{
	private void Awake()
	{
		if (this.audioSource == null)
		{
			base.enabled = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		GTPlayer componentInParent = other.GetComponentInParent<GTPlayer>();
		if (componentInParent != null && componentInParent == GTPlayer.Instance)
		{
			this.audioSource.volume = this.setVolume;
		}
	}

	public AudioSource audioSource;

	public float setVolume;
}
