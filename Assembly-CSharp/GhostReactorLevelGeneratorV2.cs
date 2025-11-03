using System;
using System.Collections.Generic;

public class GhostReactorLevelGeneratorV2
{
	[Serializable]
	public struct TreeLevelConfig
	{
		public int minHubs;

		public int maxHubs;

		public int minCaps;

		public int maxCaps;

		public List<GhostReactorSpawnConfig> sectionSpawnConfigs;

		public List<GhostReactorSpawnConfig> endCapSpawnConfigs;

		public List<GhostReactorLevelSection> hubs;

		public List<GhostReactorLevelSection> endCaps;

		public List<GhostReactorLevelSection> blockers;

		public List<GhostReactorLevelSectionConnector> connectors;
	}
}
