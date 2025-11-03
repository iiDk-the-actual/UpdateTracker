using System;
using System.Diagnostics;
using UnityEngine;

namespace BuildSafe
{
	public static class AssetDatabase
	{
		public static T LoadAssetAtPath<T>(string assetPath) where T : Object
		{
			return default(T);
		}

		public static T[] LoadAssetsOfType<T>() where T : Object
		{
			return Array.Empty<T>();
		}

		public static string[] FindAssetsOfType<T>() where T : Object
		{
			return Array.Empty<string>();
		}

		[Conditional("UNITY_EDITOR")]
		public static void SaveToDisk(params Object[] assetsToSave)
		{
			AssetDatabase.SaveAssetsToDisk(assetsToSave, true);
		}

		public static void SaveAssetsToDisk(Object[] assetsToSave, bool saveProject = true)
		{
		}
	}
}
