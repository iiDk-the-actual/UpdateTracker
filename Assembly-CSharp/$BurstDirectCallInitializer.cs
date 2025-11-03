using System;
using Unity.Burst;
using UnityEngine;

internal static class $BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Initialize()
	{
		BurstCompilerOptions options = BurstCompiler.Options;
	}
}
