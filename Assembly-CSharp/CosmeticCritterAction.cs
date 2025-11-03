using System;

[Flags]
public enum CosmeticCritterAction
{
	None = 0,
	RPC = 1,
	Spawn = 2,
	Despawn = 4,
	SpawnLinked = 8,
	ShadeHeartbeat = 16
}
