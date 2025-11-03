using System;
using UnityEngine;

public class CrittersStickyGoo : CrittersActor
{
	public override void Initialize()
	{
		base.Initialize();
		this.readyToDisable = false;
	}

	public bool CanAffect(Vector3 position)
	{
		return (base.transform.position - position).magnitude < this.range;
	}

	public void EffectApplied(CrittersPawn critter)
	{
		if (this.destroyOnApply)
		{
			this.readyToDisable = true;
		}
		CrittersManager.instance.TriggerEvent(CrittersManager.CritterEvent.StickyTriggered, this.actorId, critter.transform.position, Quaternion.LookRotation(critter.transform.up));
	}

	public override bool ProcessLocal()
	{
		bool flag = base.ProcessLocal();
		if (this.readyToDisable)
		{
			base.gameObject.SetActive(false);
			return true;
		}
		return flag;
	}

	[Header("Sticky Goo")]
	public float range = 1f;

	public float slowModifier = 0.3f;

	public float slowDuration = 3f;

	public bool destroyOnApply = true;

	private bool readyToDisable;
}
