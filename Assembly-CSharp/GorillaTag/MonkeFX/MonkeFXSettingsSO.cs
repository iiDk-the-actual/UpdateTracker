using System;
using UnityEngine;

namespace GorillaTag.MonkeFX
{
	[CreateAssetMenu(fileName = "MeshGenerator", menuName = "ScriptableObjects/MeshGenerator", order = 1)]
	public class MonkeFXSettingsSO : ScriptableObject
	{
		protected void Awake()
		{
			MonkeFX.Register(this);
		}

		public GTDirectAssetRef<Mesh>[] sourceMeshes;

		[HideInInspector]
		public Mesh combinedMesh;
	}
}
