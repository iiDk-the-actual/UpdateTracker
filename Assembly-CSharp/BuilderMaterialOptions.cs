using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuilderMaterialOptions01a", menuName = "Gorilla Tag/Builder/Options", order = 0)]
public class BuilderMaterialOptions : ScriptableObject
{
	public void GetMaterialFromType(int materialType, out Material material, out int soundIndex)
	{
		if (this.options == null)
		{
			material = null;
			soundIndex = -1;
			return;
		}
		foreach (BuilderMaterialOptions.Options options in this.options)
		{
			if (options.materialId.GetHashCode() == materialType)
			{
				material = options.material;
				soundIndex = options.soundIndex;
				return;
			}
		}
		material = null;
		soundIndex = -1;
	}

	public void GetDefaultMaterial(out int materialType, out Material material, out int soundIndex)
	{
		if (this.options.Count > 0)
		{
			materialType = this.options[0].materialId.GetHashCode();
			material = this.options[0].material;
			soundIndex = this.options[0].soundIndex;
			return;
		}
		materialType = -1;
		material = null;
		soundIndex = -1;
	}

	public List<BuilderMaterialOptions.Options> options;

	[Serializable]
	public class Options
	{
		public string materialId;

		public Material material;

		[GorillaSoundLookup]
		public int soundIndex;

		[NonSerialized]
		public int materialType;
	}
}
