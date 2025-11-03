using System;
using System.Collections.Generic;
using UnityEngine;

public class GRProgressionScriptableObject : ScriptableObject
{
	[SerializeField]
	[Header("Progression Tiers")]
	public List<GRPlayer.ProgressionLevels> progressionData;
}
