using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTag.Rendering
{
	[Serializable]
	public class EdMeshCombinedPrefabData
	{
		public void Clear()
		{
		}

		public string path;

		public List<Renderer> disabled = new List<Renderer>(512);

		public List<GameObject> combined = new List<GameObject>(64);
	}
}
