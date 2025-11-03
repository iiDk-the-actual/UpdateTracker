using System;
using UnityEngine;

namespace GameObjectScheduling
{
	[CreateAssetMenu(fileName = "New Mesh Material Replacement", menuName = "Game Object Scheduling/New Mesh Material Replacement", order = 1)]
	public class MeshMaterialReplacement : ScriptableObject
	{
		public Mesh mesh;

		public Material[] materials;
	}
}
