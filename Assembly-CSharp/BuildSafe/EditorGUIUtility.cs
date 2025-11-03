using System;
using System.Diagnostics;
using UnityEngine;

namespace BuildSafe
{
	public static class EditorGUIUtility
	{
		[Conditional("UNITY_EDITOR")]
		public static void PingObject(Object obj)
		{
		}
	}
}
