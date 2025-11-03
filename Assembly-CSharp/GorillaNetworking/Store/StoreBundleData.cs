using System;
using UnityEngine;

namespace GorillaNetworking.Store
{
	public class StoreBundleData : ScriptableObject
	{
		public void OnValidate()
		{
			if (this.playfabBundleID.Contains(' '))
			{
				Debug.LogError("ERROR THERE IS A SPACE IN THE PLAYFAB BUNDLE ID " + base.name);
			}
			if (this.bundleSKU.Contains(' '))
			{
				Debug.LogError("ERROR THERE IS A SPACE IN THE BUNDLE SKU " + base.name);
			}
		}

		public string playfabBundleID = "NULL";

		public string bundleSKU = "NULL SKU";

		public Sprite bundleImage;

		public string bundleDescriptionText = "THE NULL_BUNDLE PACK WITH 10,000 SHINY ROCKS IN THIS LIMITED TIME DLC!";
	}
}
