using System;
using UnityEngine;

public class PlayerSpeedBasedAudio : MonoBehaviour
{
	private void Start()
	{
		this.fadeRate = 1f / this.fadeTime;
		this.baseVolume = this.audioSource.volume;
		this.localPlayerVelocityEstimator.TryResolve<GorillaVelocityEstimator>(out this.velocityEstimator);
	}

	private void Update()
	{
		this.currentFadeLevel = Mathf.MoveTowards(this.currentFadeLevel, Mathf.InverseLerp(this.minVolumeSpeed, this.fullVolumeSpeed, this.velocityEstimator.linearVelocity.magnitude), this.fadeRate * Time.deltaTime);
		if (this.baseVolume == 0f || this.currentFadeLevel == 0f)
		{
			this.audioSource.volume = 0.0001f;
			return;
		}
		this.audioSource.volume = this.baseVolume * this.currentFadeLevel;
	}

	[SerializeField]
	private float minVolumeSpeed;

	[SerializeField]
	private float fullVolumeSpeed;

	[SerializeField]
	private float fadeTime;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private XSceneRef localPlayerVelocityEstimator;

	private GorillaVelocityEstimator velocityEstimator;

	private float baseVolume;

	private float fadeRate;

	private float currentFadeLevel;
}
