using System;
using System.Collections.Generic;
using System.IO;
using GorillaTagScripts.GhostReactor;

public class GRShiftStat
{
	public IReadOnlyDictionary<GREnemyType, int> EnemyKills
	{
		get
		{
			return this.enemyKills;
		}
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(this.GetShiftStat(GRShiftStatType.EnemyDeaths));
		writer.Write(this.GetShiftStat(GRShiftStatType.PlayerDeaths));
		writer.Write(this.GetShiftStat(GRShiftStatType.CoresCollected));
		writer.Write(this.GetShiftStat(GRShiftStatType.SentientCoresCollected));
		writer.Write(this.enemyKills.Count);
		foreach (KeyValuePair<GREnemyType, int> keyValuePair in this.enemyKills)
		{
			writer.Write((int)keyValuePair.Key);
			writer.Write(keyValuePair.Value);
		}
	}

	public void Deserialize(BinaryReader reader)
	{
		this.shiftStats[GRShiftStatType.EnemyDeaths] = reader.ReadInt32();
		this.shiftStats[GRShiftStatType.PlayerDeaths] = reader.ReadInt32();
		this.shiftStats[GRShiftStatType.CoresCollected] = reader.ReadInt32();
		this.shiftStats[GRShiftStatType.SentientCoresCollected] = reader.ReadInt32();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			GREnemyType grenemyType = (GREnemyType)reader.ReadInt32();
			this.enemyKills[grenemyType] = reader.ReadInt32();
		}
	}

	public void SetShiftStat(GRShiftStatType stat, int newValue)
	{
		this.shiftStats[stat] = newValue;
		GhostReactor.instance.shiftManager.RefreshDepthDisplay();
	}

	public void IncrementShiftStat(GRShiftStatType stat)
	{
		if (this.shiftStats.ContainsKey(stat))
		{
			Dictionary<GRShiftStatType, int> dictionary = this.shiftStats;
			int num = dictionary[stat];
			dictionary[stat] = num + 1;
			return;
		}
		this.shiftStats[stat] = 1;
		GhostReactor.instance.shiftManager.RefreshDepthDisplay();
	}

	public void IncrementEnemyKills(GREnemyType type)
	{
		if (!this.enemyKills.TryAdd(type, 1))
		{
			Dictionary<GREnemyType, int> dictionary = this.enemyKills;
			int num = dictionary[type];
			dictionary[type] = num + 1;
		}
		GhostReactor.instance.shiftManager.RefreshDepthDisplay();
	}

	public void ResetShiftStats()
	{
		this.shiftStats[GRShiftStatType.EnemyDeaths] = 0;
		this.shiftStats[GRShiftStatType.PlayerDeaths] = 0;
		this.shiftStats[GRShiftStatType.CoresCollected] = 0;
		this.shiftStats[GRShiftStatType.SentientCoresCollected] = 0;
		this.enemyKills.Clear();
		GhostReactor.instance.shiftManager.RefreshDepthDisplay();
	}

	public int GetShiftStat(GRShiftStatType stat)
	{
		if (this.shiftStats.ContainsKey(stat))
		{
			return this.shiftStats[stat];
		}
		return 0;
	}

	public Dictionary<GRShiftStatType, int> shiftStats = new Dictionary<GRShiftStatType, int>();

	private Dictionary<GREnemyType, int> enemyKills = new Dictionary<GREnemyType, int>();
}
