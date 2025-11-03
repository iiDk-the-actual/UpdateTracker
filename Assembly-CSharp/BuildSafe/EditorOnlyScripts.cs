using System;
using System.Diagnostics;
using UnityEngine;

namespace BuildSafe
{
	internal static class EditorOnlyScripts
	{
		[Conditional("UNITY_EDITOR")]
		public static void Cleanup(GameObject[] rootObjects, bool force = false)
		{
		}
	}
}
