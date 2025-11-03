using System;
using System.Collections.Generic;
using GorillaTag.Rendering.Shaders;
using UnityEngine;

public class BetterBakerBakeMe : FlagForBaking
{
	public GameObject[] stuffIncludingParentsToBake;

	public GameObject getMatStuffFromHere;

	public List<ShaderConfigData.ShaderConfig> allConfigs = new List<ShaderConfigData.ShaderConfig>();
}
