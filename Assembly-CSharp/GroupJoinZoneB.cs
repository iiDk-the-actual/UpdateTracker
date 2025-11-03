using System;

[Flags]
public enum GroupJoinZoneB
{
	HoverboardTunnel = 1,
	Critters = 2,
	CrittersTunnel = 4,
	GhostReactor = 8,
	MonkeBlocksShared = 16,
	MonkeBlocksSharedTunnel = 32,
	GhostReactorTunnel = 64,
	RankedForest = 128,
	RankedForestTunnel = 256,
	GhostReactorDrill = 512
}
