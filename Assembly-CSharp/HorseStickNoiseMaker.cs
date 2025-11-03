using System;
using GorillaExtensions;
using UnityEngine;

public class HorseStickNoiseMaker : MonoBehaviour
{
	protected void OnEnable()
	{
		if (!this.gorillaPlayerXform && !base.transform.TryFindByPath(this.gorillaPlayerXform_path, out this.gorillaPlayerXform, false))
		{
			Debug.LogError(string.Concat(new string[]
			{
				"HorseStickNoiseMaker: DEACTIVATING! Could not find gorillaPlayerXform using path: \"",
				this.gorillaPlayerXform_path,
				"\"\nThis component's transform path: \"",
				base.transform.GetPath(),
				"\""
			}));
			base.gameObject.SetActive(false);
			return;
		}
		this.oldPos = this.gorillaPlayerXform.position;
		this.distElapsed = 0f;
		this.timeSincePlay = 0f;
	}

	protected void LateUpdate()
	{
		Vector3 position = this.gorillaPlayerXform.position;
		Vector3 vector = position - this.oldPos;
		this.distElapsed += vector.magnitude;
		this.timeSincePlay += Time.deltaTime;
		this.oldPos = position;
		if (this.distElapsed >= this.metersPerClip && this.timeSincePlay >= this.minSecBetweenClips)
		{
			this.soundBankPlayer.Play();
			this.distElapsed = 0f;
			this.timeSincePlay = 0f;
			if (this.particleFX != null)
			{
				this.particleFX.Play();
			}
		}
	}

	[Tooltip("Meters the object should traverse between playing a provided audio clip.")]
	public float metersPerClip = 4f;

	[Tooltip("Number of seconds that must elapse before playing another audio clip.")]
	public float minSecBetweenClips = 1.5f;

	public SoundBankPlayer soundBankPlayer;

	[Tooltip("Transform assigned in Gorilla Player Networked Prefab to the Gorilla Player Networked parent to keep track of distance traveled.")]
	public Transform gorillaPlayerXform;

	[Delayed]
	public string gorillaPlayerXform_path;

	[Tooltip("Optional particle FX to spawn when sound plays")]
	public ParticleSystem particleFX;

	private Vector3 oldPos;

	private float timeSincePlay;

	private float distElapsed;
}
