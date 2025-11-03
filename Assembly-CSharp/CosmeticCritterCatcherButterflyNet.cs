using System;
using UnityEngine;

public class CosmeticCritterCatcherButterflyNet : CosmeticCritterCatcher
{
	public override CosmeticCritterAction GetLocalCatchAction(CosmeticCritter critter)
	{
		if (!(critter is CosmeticCritterButterfly) || (critter.transform.position - this.velocityEstimator.transform.position).sqrMagnitude > this.maxCatchRadius * this.maxCatchRadius || this.velocityEstimator.linearVelocity.sqrMagnitude < this.minCatchSpeed * this.minCatchSpeed)
		{
			return CosmeticCritterAction.None;
		}
		return CosmeticCritterAction.RPC | CosmeticCritterAction.Despawn;
	}

	public override bool ValidateRemoteCatchAction(CosmeticCritter critter, CosmeticCritterAction catchAction, double serverTime)
	{
		return base.ValidateRemoteCatchAction(critter, catchAction, serverTime) && critter is CosmeticCritterButterfly && (critter.transform.position - this.velocityEstimator.transform.position).sqrMagnitude <= this.maxCatchRadius * this.maxCatchRadius + 1f && this.velocityEstimator.linearVelocity.sqrMagnitude >= this.minCatchSpeed * this.minCatchSpeed - 1f && catchAction == (CosmeticCritterAction.RPC | CosmeticCritterAction.Despawn);
	}

	public override void OnCatch(CosmeticCritter critter, CosmeticCritterAction catchAction, double serverTime)
	{
		this.caughtButterflyParticleSystem.Emit((critter as CosmeticCritterButterfly).GetEmitParams, 1);
		this.catchFX.Play();
		this.catchSFX.Play();
	}

	[Tooltip("Use this for calculating the catch position and velocity.")]
	[SerializeField]
	private GorillaVelocityEstimator velocityEstimator;

	[Tooltip("Catch the Butterfly if it is within this radius.")]
	[SerializeField]
	private float maxCatchRadius;

	[Tooltip("Only catch the Butterfly if the net is moving faster than this speed.")]
	[SerializeField]
	private float minCatchSpeed;

	[Tooltip("Spawn a particle inside the net representing the caught Butterfly.")]
	[SerializeField]
	private ParticleSystem caughtButterflyParticleSystem;

	[Tooltip("Play this particle effect when catching a Butterfly.")]
	[SerializeField]
	private ParticleSystem catchFX;

	[Tooltip("Play this sound when catching a Butterfly.")]
	[SerializeField]
	private AudioSource catchSFX;
}
