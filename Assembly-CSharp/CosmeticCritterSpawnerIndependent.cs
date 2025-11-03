using System;

public class CosmeticCritterSpawnerIndependent : CosmeticCritterSpawner
{
	public virtual bool CanSpawnLocal()
	{
		return this.numCritters < this.maxCritters;
	}

	public virtual bool CanSpawnRemote(double serverTime)
	{
		return this.numCritters < this.maxCritters && this.callLimiter.CheckCallServerTime(serverTime);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		CosmeticCritterManager.Instance.RegisterIndependentSpawner(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		CosmeticCritterManager.Instance.UnregisterIndependentSpawner(this);
	}
}
