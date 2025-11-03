using System;
using UnityEngine;

public class GRToolUpgrade : ScriptableObject
{
	public string upgradeName;

	public string description;

	public string upgradeId;

	[SerializeField]
	public GRToolUpgrade.ToolUpgradeLevel[] upgradeLevels;

	[Serializable]
	public struct ToolUpgradeLevel
	{
		[SerializeField]
		public int Cost;

		[SerializeField]
		public float upgradeAmount;
	}
}
