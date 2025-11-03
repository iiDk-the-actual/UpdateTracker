using System;
using UnityEngine;

public class SIResourceMonkeIdol : SIResource
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.depositEnabledParticle.SetActive(SIPlayer.LocalPlayer.CanLimitedResourceBeDeposited(this.limitedDepositType));
	}

	public override void HandleDepositAuth(SIPlayer depositingPlayer)
	{
		SIPlayer.LocalPlayer.TriggerIdolDepositedCelebration(base.transform.position);
	}

	[SerializeField]
	private GameObject depositEnabledParticle;
}
