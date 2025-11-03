using System;
using UnityEngine;

public abstract class CosmeticCritterSpawnerTimed : CosmeticCritterSpawnerIndependent
{
	protected override CallLimiter CreateCallLimiter()
	{
		return new CallLimiter(5, this.spawnIntervalMinMax.x, 0.5f);
	}

	public override bool CanSpawnLocal()
	{
		if (Time.time >= this.nextLocalSpawnTime)
		{
			this.nextLocalSpawnTime = Time.time + Random.Range(this.spawnIntervalMinMax.x, this.spawnIntervalMinMax.y);
			return base.CanSpawnLocal();
		}
		return false;
	}

	public override bool CanSpawnRemote(double serverTime)
	{
		return base.CanSpawnRemote(serverTime);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsLocal)
		{
			this.nextLocalSpawnTime = Time.time + Random.Range(this.spawnIntervalMinMax.x, this.spawnIntervalMinMax.y);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	[Tooltip("The minimum and maximum time to wait between spawn attempts.")]
	[SerializeField]
	private Vector2 spawnIntervalMinMax = new Vector2(2f, 5f);

	[Tooltip("Currently does nothing.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float spawnChance = 1f;
}
