using System;
using UnityEngine;

namespace MTAssets.EasyMeshCombiner
{
	public class CombineInRuntimeDemo : MonoBehaviour
	{
		private void Update()
		{
			if (!this.runtimeCombiner.isTargetMeshesMerged())
			{
				this.combineButton.SetActive(true);
				this.undoButton.SetActive(false);
			}
			if (this.runtimeCombiner.isTargetMeshesMerged())
			{
				this.combineButton.SetActive(false);
				this.undoButton.SetActive(true);
			}
		}

		public void CombineMeshes()
		{
			this.runtimeCombiner.CombineMeshes();
		}

		public void UndoMerge()
		{
			this.runtimeCombiner.UndoMerge();
		}

		public GameObject combineButton;

		public GameObject undoButton;

		public RuntimeMeshCombiner runtimeCombiner;
	}
}
