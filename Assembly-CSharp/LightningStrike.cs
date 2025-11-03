using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
public class LightningStrike : MonoBehaviour
{
	private void Initialize()
	{
		this.ps = base.GetComponent<ParticleSystem>();
		this.psMain = this.ps.main;
		this.psMain.playOnAwake = true;
		this.psMain.stopAction = ParticleSystemStopAction.Disable;
		this.psShape = this.ps.shape;
		this.psTrails = this.ps.trails;
		this.audioSource = base.GetComponent<AudioSource>();
		this.audioSource.playOnAwake = true;
	}

	public void Play(Vector3 p1, Vector3 p2, float beamWidthMultiplier, float audioVolume, float duration, Gradient colorOverLifetime)
	{
		if (this.ps == null)
		{
			this.Initialize();
		}
		base.transform.position = p1;
		base.transform.rotation = Quaternion.LookRotation(p1 - p2);
		this.psShape.radius = Vector3.Distance(p1, p2) * 0.5f;
		this.psShape.position = new Vector3(0f, 0f, -this.psShape.radius);
		this.psShape.randomPositionAmount = Mathf.Clamp(this.psShape.radius / 50f, 0f, 1f);
		this.psTrails.widthOverTrail = new ParticleSystem.MinMaxCurve(beamWidthMultiplier * 0.1f, beamWidthMultiplier);
		this.psTrails.colorOverLifetime = colorOverLifetime;
		this.psMain.duration = duration;
		this.audioSource.volume = Mathf.Clamp(this.psShape.radius / 5f, 0f, 1f) * audioVolume;
		base.gameObject.SetActive(true);
	}

	public static SRand rand = new SRand("LightningStrike");

	private ParticleSystem ps;

	private ParticleSystem.MainModule psMain;

	private ParticleSystem.ShapeModule psShape;

	private ParticleSystem.TrailModule psTrails;

	private AudioSource audioSource;
}
