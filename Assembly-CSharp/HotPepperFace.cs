using System;
using UnityEngine;

public class HotPepperFace : MonoBehaviour
{
	public void PlayFX(float delay)
	{
		if (delay < 0f)
		{
			this.PlayFX();
			return;
		}
		base.Invoke("PlayFX", delay);
	}

	public void PlayFX()
	{
		this._faceMesh.SetActive(true);
		this._thermalSourceVolume.SetActive(true);
		this._fireFX.Play();
		this._flameSpeaker.GTPlay();
		this._breathSpeaker.GTPlay();
		base.Invoke("StopFX", this._effectLength);
	}

	public void StopFX()
	{
		this._faceMesh.SetActive(false);
		this._thermalSourceVolume.SetActive(false);
		this._fireFX.Stop();
		this._flameSpeaker.GTStop();
		this._breathSpeaker.GTStop();
	}

	[SerializeField]
	private GameObject _faceMesh;

	[SerializeField]
	private ParticleSystem _fireFX;

	[SerializeField]
	private AudioSource _flameSpeaker;

	[SerializeField]
	private AudioSource _breathSpeaker;

	[SerializeField]
	private float _effectLength = 1.5f;

	[SerializeField]
	private GameObject _thermalSourceVolume;
}
