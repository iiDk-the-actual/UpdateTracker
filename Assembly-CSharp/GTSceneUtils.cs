using System;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public static class GTSceneUtils
{
	[Conditional("UNITY_EDITOR")]
	public static void AddToBuild(GTScene scene)
	{
	}

	public static bool Equals(GTScene x, Scene y)
	{
		return !(x == null) && y.IsValid() && x.Equals(y);
	}

	public static GTScene[] ScenesInBuild()
	{
		return Array.Empty<GTScene>();
	}

	[Conditional("UNITY_EDITOR")]
	public static void SyncBuildScenes()
	{
	}
}
