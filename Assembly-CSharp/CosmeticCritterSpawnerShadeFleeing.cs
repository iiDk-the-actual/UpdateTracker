using System;
using UnityEngine;

public class CosmeticCritterSpawnerShadeFleeing : CosmeticCritterSpawner
{
	public void SetSpawnPosition(Vector3 pos)
	{
		this.spawnPosition = pos;
	}

	public override void OnSpawn(CosmeticCritter critter)
	{
		base.OnSpawn(critter);
		(critter as CosmeticCritterShadeFleeing).SetFleePosition(this.spawnPosition, base.transform.position);
	}

	private Vector3 spawnPosition;
}
