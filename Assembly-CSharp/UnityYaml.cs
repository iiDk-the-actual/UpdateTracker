using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class UnityYaml
{
	private static readonly Assembly EngineAssembly = Assembly.GetAssembly(typeof(MonoBehaviour));

	private static readonly Assembly TerrainAssembly = Assembly.GetAssembly(typeof(Tree));

	public static Dictionary<int, Type> ClassIDToType = new Dictionary<int, Type>();
}
