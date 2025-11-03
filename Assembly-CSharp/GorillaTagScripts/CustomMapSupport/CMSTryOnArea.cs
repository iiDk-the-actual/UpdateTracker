using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GorillaTagScripts.CustomMapSupport
{
	public class CMSTryOnArea : MonoBehaviour
	{
		public void InitializeForCustomMap(CompositeTriggerEvents customMapTryOnArea, Scene customMapScene)
		{
			this.originalScene = customMapScene;
			if (this.tryOnAreaCollider.IsNull())
			{
				return;
			}
			customMapTryOnArea.AddCollider(this.tryOnAreaCollider);
		}

		public void RemoveFromCustomMap(CompositeTriggerEvents customMapTryOnArea)
		{
			if (this.tryOnAreaCollider.IsNull())
			{
				return;
			}
			customMapTryOnArea.RemoveCollider(this.tryOnAreaCollider);
		}

		public bool IsFromScene(Scene unloadingScene)
		{
			return unloadingScene == this.originalScene;
		}

		private Scene originalScene;

		public BoxCollider tryOnAreaCollider;
	}
}
