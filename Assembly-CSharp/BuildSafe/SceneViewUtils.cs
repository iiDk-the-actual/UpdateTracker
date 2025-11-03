using System;
using UnityEngine;

namespace BuildSafe
{
	public static class SceneViewUtils
	{
		private static bool RaycastWorldSafe(Vector2 screenPos, out RaycastHit hit)
		{
			hit = default(RaycastHit);
			return false;
		}

		public static readonly SceneViewUtils.FuncRaycastWorld RaycastWorld = new SceneViewUtils.FuncRaycastWorld(SceneViewUtils.RaycastWorldSafe);

		public delegate bool FuncRaycastWorld(Vector2 screenPos, out RaycastHit hit);

		public delegate GameObject FuncPickClosestGameObject(Camera cam, int layers, Vector2 position, GameObject[] ignore, GameObject[] filter, out int materialIndex);
	}
}
