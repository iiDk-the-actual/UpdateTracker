using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GT_CustomMapSupportRuntime;
using UnityEngine;

namespace GorillaTagScripts.CustomMapSupport
{
	public class CMSLoadingZone : MonoBehaviour
	{
		private void Start()
		{
			base.gameObject.layer = UnityLayer.GorillaTrigger.ToLayerIndex();
		}

		public void SetupLoadingZone(LoadZoneSettings settings, in string[] assetBundleSceneFilePaths)
		{
			this.scenesToLoad = this.GetSceneIndexes(settings.scenesToLoad, in assetBundleSceneFilePaths);
			this.scenesToUnload = this.CleanSceneUnloadArray(settings.scenesToUnload, settings.scenesToLoad, in assetBundleSceneFilePaths);
			this.useDynamicLighting = settings.useDynamicLighting;
			this.dynamicLightingAmbientColor = settings.UberShaderAmbientDynamicLight;
			base.gameObject.layer = UnityLayer.GorillaBoundary.ToLayerIndex();
			Collider[] components = base.gameObject.GetComponents<Collider>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].isTrigger = true;
			}
		}

		private int[] GetSceneIndexes(List<string> sceneNames, in string[] assetBundleSceneFilePaths)
		{
			int[] array = new int[sceneNames.Count];
			for (int i = 0; i < sceneNames.Count; i++)
			{
				for (int j = 0; j < assetBundleSceneFilePaths.Length; j++)
				{
					if (string.Equals(sceneNames[i], this.GetSceneNameFromFilePath(assetBundleSceneFilePaths[j])))
					{
						array[i] = j;
						break;
					}
				}
			}
			return array;
		}

		private int[] CleanSceneUnloadArray(List<string> unload, List<string> load, in string[] assetBundleSceneFilePaths)
		{
			for (int i = 0; i < load.Count; i++)
			{
				if (unload.Contains(load[i]))
				{
					unload.Remove(load[i]);
				}
			}
			return this.GetSceneIndexes(unload, in assetBundleSceneFilePaths);
		}

		public void OnTriggerEnter(Collider other)
		{
			if (other == GTPlayer.Instance.bodyCollider)
			{
				if (this.useDynamicLighting)
				{
					GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
					GameLightingManager.instance.SetAmbientLightDynamic(this.dynamicLightingAmbientColor);
				}
				else
				{
					GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
					GameLightingManager.instance.SetAmbientLightDynamic(Color.black);
				}
				CustomMapManager.LoadZoneTriggered(this.scenesToLoad, this.scenesToUnload);
			}
		}

		private string GetSceneNameFromFilePath(string filePath)
		{
			string[] array = filePath.Split("/", StringSplitOptions.None);
			return array[array.Length - 1].Split(".", StringSplitOptions.None)[0];
		}

		private int[] scenesToLoad;

		private int[] scenesToUnload;

		private bool useDynamicLighting;

		private Color dynamicLightingAmbientColor;
	}
}
