using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class MaterialChangerCosmetic : MonoBehaviour
	{
		public void ChangeMaterial(Material newMaterial)
		{
			if (this.targetRenderer == null || newMaterial == null || this.materialIndex < 0)
			{
				return;
			}
			Material[] materials = this.targetRenderer.materials;
			if (this.materialIndex >= materials.Length)
			{
				Debug.LogWarning(string.Format("Material index {0} is out of range.", this.materialIndex));
				return;
			}
			materials[this.materialIndex] = newMaterial;
			this.targetRenderer.materials = materials;
		}

		public void ChangeAllMaterials(Material newMat)
		{
			if (this.targetRenderer == null || newMat == null)
			{
				return;
			}
			Material[] array = new Material[this.targetRenderer.materials.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = newMat;
			}
			this.targetRenderer.materials = array;
		}

		[SerializeField]
		private SkinnedMeshRenderer targetRenderer;

		[SerializeField]
		private int materialIndex;
	}
}
