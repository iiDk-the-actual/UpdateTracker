using System;
using UnityEngine;

[Serializable]
public class VoiceLoudnessReactorParticleSystemTarget
{
	public float InitialSpeed
	{
		get
		{
			return this.initialSpeed;
		}
		set
		{
			this.initialSpeed = value;
		}
	}

	public float InitialRate
	{
		get
		{
			return this.initialRate;
		}
		set
		{
			this.initialRate = value;
		}
	}

	public float InitialSize
	{
		get
		{
			return this.initialSize;
		}
		set
		{
			this.initialSize = value;
		}
	}

	public ParticleSystem particleSystem;

	public bool UseSmoothedLoudness;

	public float Scale = 1f;

	private float initialSpeed;

	private float initialRate;

	private float initialSize;

	public AnimationCurve speed;

	public AnimationCurve rate;

	public AnimationCurve size;

	[HideInInspector]
	public ParticleSystem.MainModule Main;

	[HideInInspector]
	public ParticleSystem.EmissionModule Emission;
}
