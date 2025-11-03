using System;
using UnityEngine;

public abstract class CosmeticCritterCatcher : CosmeticCritterHoldable
{
	public CosmeticCritterSpawner GetLinkedSpawner()
	{
		return this.optionalLinkedSpawner;
	}

	public abstract CosmeticCritterAction GetLocalCatchAction(CosmeticCritter critter);

	public virtual bool ValidateRemoteCatchAction(CosmeticCritter critter, CosmeticCritterAction catchAction, double serverTime)
	{
		return this.callLimiter.CheckCallServerTime(serverTime);
	}

	public abstract void OnCatch(CosmeticCritter critter, CosmeticCritterAction catchAction, double serverTime);

	protected override void OnEnable()
	{
		base.OnEnable();
		CosmeticCritterManager.Instance.RegisterCatcher(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		CosmeticCritterManager.Instance.UnregisterCatcher(this);
	}

	[SerializeField]
	[Tooltip("If this catcher is capable of spawning immediately after catching, the linked spawner must be assigned here.")]
	protected CosmeticCritterSpawner optionalLinkedSpawner;
}
