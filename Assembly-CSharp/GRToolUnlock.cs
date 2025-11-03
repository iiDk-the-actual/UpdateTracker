using System;
using UnityEngine;

public class GRToolUnlock : ScriptableObject
{
	public string toolName;

	public string toolId;

	public int unlockLevel;

	public int unlockCost;

	public GRToolUpgrade[] toolUpgrades;
}
