using System;

public class SIResourceRegion : SpawnRegion<GameEntity, SIResourceRegion>
{
	public float LastSpawnTime { get; set; }

	public SIResource resourcePrefab;
}
