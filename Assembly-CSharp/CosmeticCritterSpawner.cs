using System;
using UnityEngine;

public abstract class CosmeticCritterSpawner : CosmeticCritterHoldable
{
	public GameObject GetCritterPrefab()
	{
		return this.critterPrefab;
	}

	public CosmeticCritter GetCritter()
	{
		return this.cachedCritter;
	}

	public Type GetCritterType()
	{
		return this.cachedType;
	}

	public virtual void SetRandomVariables(CosmeticCritter critter)
	{
	}

	public virtual void OnSpawn(CosmeticCritter critter)
	{
		this.numCritters++;
	}

	public virtual void OnDespawn(CosmeticCritter critter)
	{
		this.numCritters = Math.Max(this.numCritters - 1, 0);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.cachedCritter == null)
		{
			this.cachedCritter = this.critterPrefab.GetComponent<CosmeticCritter>();
			this.cachedType = this.cachedCritter.GetType();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	[Tooltip("The critter prefab to spawn.")]
	[SerializeField]
	protected GameObject critterPrefab;

	[Tooltip("The maximum number of critters that this spawner can have active at once.")]
	[SerializeField]
	protected int maxCritters;

	protected CosmeticCritter cachedCritter;

	protected Type cachedType;

	protected int numCritters;

	protected float nextLocalSpawnTime;
}
