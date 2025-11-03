using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostReactorDropTableOverrides", menuName = "ScriptableObjects/GhostReactorDropTableOverride")]
public class GRDropTableOverrides : ScriptableObject
{
	public GRBreakableItemSpawnConfig GetOverride(GRBreakableItemSpawnConfig table)
	{
		for (int i = 0; i < this.overrides.Count; i++)
		{
			if (this.overrides[i].table == table)
			{
				return this.overrides[i].overrideTable;
			}
		}
		return null;
	}

	public List<GRDropTableOverrides.DropTableOverride> overrides;

	[Serializable]
	public class DropTableOverride
	{
		public GRBreakableItemSpawnConfig table;

		public GRBreakableItemSpawnConfig overrideTable;
	}
}
