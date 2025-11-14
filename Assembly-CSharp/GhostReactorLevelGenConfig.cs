using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostReactorLevelGenConfig", menuName = "ScriptableObjects/GhostReactorLevelGenConfig")]
public class GhostReactorLevelGenConfig : ScriptableObject
{
	private void OnValidate()
	{
		for (int i = 0; i < this.treeLevels.Count; i++)
		{
			GhostReactorLevelGeneratorV2.TreeLevelConfig treeLevelConfig = this.treeLevels[i];
			treeLevelConfig.minHubs = Mathf.Abs(treeLevelConfig.minHubs);
			treeLevelConfig.maxHubs = Mathf.Abs(treeLevelConfig.maxHubs);
			treeLevelConfig.minCaps = Mathf.Abs(treeLevelConfig.minCaps);
			treeLevelConfig.maxCaps = Mathf.Abs(treeLevelConfig.maxCaps);
			if (treeLevelConfig.minHubs > treeLevelConfig.maxHubs)
			{
				treeLevelConfig.maxHubs = treeLevelConfig.minHubs;
			}
			if (treeLevelConfig.minCaps > treeLevelConfig.maxCaps)
			{
				treeLevelConfig.maxCaps = treeLevelConfig.minCaps;
			}
			this.treeLevels[i] = treeLevelConfig;
		}
		GhostReactorLevelGeneratorV2.TreeLevelConfig treeLevelConfig2 = this.treeLevels[this.treeLevels.Count - 1];
		if (treeLevelConfig2.minHubs > 0 || treeLevelConfig2.maxHubs > 0)
		{
			Debug.LogError("Ghost Reactor Level Gen Setup Error: The last tree level can only spawn end caps around the furthest level of hubs. Otherwise it would spawn hubs without a further level to spawn end caps around them");
			treeLevelConfig2.minHubs = 0;
			treeLevelConfig2.maxHubs = 0;
			this.treeLevels[this.treeLevels.Count - 1] = treeLevelConfig2;
		}
		using (List<GREnemyCount>.Enumerator enumerator = this.minEnemyKills.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Count < 0)
				{
					Debug.LogError("Ghost Reactor Level Gen Setup Error: cannot have negative required enemy kills");
				}
			}
		}
	}

	public int shiftDuration;

	public int coresRequired;

	public int shiftBonus;

	public int sentientCoresRequired;

	public int maxPlayerDeaths = -1;

	public List<GREnemyCount> minEnemyKills = new List<GREnemyCount>();

	[ColorUsage(true, true)]
	public Color ambientLight = Color.black;

	public List<GhostReactorLevelGeneratorV2.TreeLevelConfig> treeLevels = new List<GhostReactorLevelGeneratorV2.TreeLevelConfig>();

	public List<GRBonusEntry> enemyGlobalBonuses = new List<GRBonusEntry>();

	public GRDropTableOverrides dropTableOverrides;
}
