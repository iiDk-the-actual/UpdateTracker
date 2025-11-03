using System;
using UnityEngine;

public static class MaterialUtils
{
	public static string GetTrimmedMaterialName(Material material)
	{
		return material.name.Replace(" (Instance)", "").Trim();
	}

	public static void SwapMaterial(MeshAndMaterials meshAndMaterial, bool isOnToOff)
	{
		Material[] sharedMaterials = meshAndMaterial.meshRenderer.sharedMaterials;
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			string trimmedMaterialName = MaterialUtils.GetTrimmedMaterialName(sharedMaterials[i]);
			string text = (isOnToOff ? ((meshAndMaterial.onMaterial != null) ? MaterialUtils.GetTrimmedMaterialName(meshAndMaterial.onMaterial) : null) : ((meshAndMaterial.offMaterial != null) ? MaterialUtils.GetTrimmedMaterialName(meshAndMaterial.offMaterial) : null));
			if (text != null && trimmedMaterialName == text)
			{
				sharedMaterials[i] = (isOnToOff ? meshAndMaterial.offMaterial : meshAndMaterial.onMaterial);
			}
		}
		meshAndMaterial.meshRenderer.sharedMaterials = sharedMaterials;
	}
}
