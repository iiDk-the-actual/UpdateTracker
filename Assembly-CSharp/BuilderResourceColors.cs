using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuilderMaterialResourceColors", menuName = "Gorilla Tag/Builder/ResourceColors", order = 0)]
public class BuilderResourceColors : ScriptableObject
{
	public Color GetColorForType(BuilderResourceType type)
	{
		foreach (BuilderResourceColor builderResourceColor in this.colors)
		{
			if (builderResourceColor.type == type)
			{
				return builderResourceColor.color;
			}
		}
		return Color.black;
	}

	public List<BuilderResourceColor> colors;
}
