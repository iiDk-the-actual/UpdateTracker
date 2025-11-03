using System;
using UnityEngine;

namespace GorillaTag.Rendering
{
	public class PFXExtraAnimControls : MonoBehaviour
	{
		protected void Awake()
		{
			this.emissionModules = new ParticleSystem.EmissionModule[this.particleSystems.Length];
			this.cachedEmitBursts = new ParticleSystem.Burst[this.particleSystems.Length][];
			this.adjustedEmitBursts = new ParticleSystem.Burst[this.particleSystems.Length][];
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				ParticleSystem.EmissionModule emission = this.particleSystems[i].emission;
				this.cachedEmitBursts[i] = new ParticleSystem.Burst[emission.burstCount];
				this.adjustedEmitBursts[i] = new ParticleSystem.Burst[emission.burstCount];
				for (int j = 0; j < emission.burstCount; j++)
				{
					this.cachedEmitBursts[i][j] = emission.GetBurst(j);
					this.adjustedEmitBursts[i][j] = emission.GetBurst(j);
				}
				this.emissionModules[i] = emission;
			}
		}

		protected void LateUpdate()
		{
			for (int i = 0; i < this.emissionModules.Length; i++)
			{
				this.emissionModules[i].rateOverTimeMultiplier = this.emitRateMult;
				Mathf.Min(this.emissionModules[i].burstCount, this.cachedEmitBursts[i].Length);
				for (int j = 0; j < this.cachedEmitBursts[i].Length; j++)
				{
					this.adjustedEmitBursts[i][j].probability = this.cachedEmitBursts[i][j].probability * this.emitBurstProbabilityMult;
				}
				this.emissionModules[i].SetBursts(this.adjustedEmitBursts[i]);
			}
		}

		public float emitRateMult = 1f;

		public float emitBurstProbabilityMult = 1f;

		[SerializeField]
		private ParticleSystem[] particleSystems;

		private ParticleSystem.EmissionModule[] emissionModules;

		private ParticleSystem.Burst[][] cachedEmitBursts;

		private ParticleSystem.Burst[][] adjustedEmitBursts;
	}
}
