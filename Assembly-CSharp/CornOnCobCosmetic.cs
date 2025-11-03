using System;
using UnityEngine;

public class CornOnCobCosmetic : MonoBehaviour
{
	protected void Awake()
	{
		this.emissionModule = this.particleSys.emission;
		this.maxBurstProbability = ((this.emissionModule.burstCount > 0) ? this.emissionModule.GetBurst(0).probability : 0.2f);
	}

	protected void LateUpdate()
	{
		for (int i = 0; i < this.emissionModule.burstCount; i++)
		{
			ParticleSystem.Burst burst = this.emissionModule.GetBurst(i);
			burst.probability = this.maxBurstProbability * this.particleEmissionCurve.Evaluate(this.thermalReceiver.celsius);
			this.emissionModule.SetBurst(i, burst);
		}
		int particleCount = this.particleSys.particleCount;
		if (particleCount > this.previousParticleCount)
		{
			this.soundBankPlayer.Play();
		}
		this.previousParticleCount = particleCount;
	}

	[Tooltip("The corn will start popping based on the temperature from this ThermalReceiver.")]
	public ThermalReceiver thermalReceiver;

	[Tooltip("The particle system that will be emitted when the heat source is hot enough.")]
	public ParticleSystem particleSys;

	[Tooltip("The curve that determines how many particles will be emitted based on the heat source's temperature.\n\nThe x-axis is the heat source's temperature and the y-axis is the number of particles to emit.")]
	public AnimationCurve particleEmissionCurve;

	public SoundBankPlayer soundBankPlayer;

	private ParticleSystem.EmissionModule emissionModule;

	private float maxBurstProbability;

	private int previousParticleCount;
}
