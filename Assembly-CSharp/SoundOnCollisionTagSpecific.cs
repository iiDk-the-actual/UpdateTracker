using System;
using UnityEngine;

public class SoundOnCollisionTagSpecific : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		if (Time.time > this.nextSound && collider.gameObject.CompareTag(this.tagName))
		{
			this.nextSound = Time.time + this.noiseCooldown;
			this.audioSource.GTPlayOneShot(this.collisionSounds[Random.Range(0, this.collisionSounds.Length)], 0.5f);
		}
	}

	public string tagName;

	public float noiseCooldown = 1f;

	private float nextSound;

	public AudioSource audioSource;

	public AudioClip[] collisionSounds;
}
