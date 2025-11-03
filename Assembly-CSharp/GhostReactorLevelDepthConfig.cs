using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostReactorLevelDepthConfig", menuName = "ScriptableObjects/GhostReactorLevelDepthConfig")]
public class GhostReactorLevelDepthConfig : ScriptableObject
{
	public string displayName;

	public List<GhostReactorLevelGenConfig> configGenOptions = new List<GhostReactorLevelGenConfig>();

	public List<GhostReactorLevelDepthConfig.LevelOption> options = new List<GhostReactorLevelDepthConfig.LevelOption>();

	[Serializable]
	public class LevelOption
	{
		public int weight = 100;

		public GhostReactorLevelGenConfig levelConfig;
	}
}
