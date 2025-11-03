using System;
using UnityEngine;

public class GRReviveMeter : MonoBehaviourTick
{
	public void Awake()
	{
	}

	public override void Tick()
	{
		float num = 0f;
		if (this.reviveStation != null && VRRig.LocalRig.OwningNetPlayer != null && this.reviveStation.GetReviveCooldownSeconds() > 0.0)
		{
			num = (float)this.reviveStation.CalculateRemainingReviveCooldownSeconds(VRRig.LocalRig.OwningNetPlayer.ActorNumber) / (float)this.reviveStation.GetReviveCooldownSeconds();
		}
		num = Mathf.Clamp(num, 0f, 1f);
		num = 1f - num;
		this.meter.localScale = new Vector3(1f, num, 1f);
	}

	[SerializeField]
	private GRReviveStation reviveStation;

	[SerializeField]
	private Transform meter;
}
