using System;
using System.IO;
using UnityEngine;

namespace GorillaNetworking.Store
{
	[Serializable]
	public class StoreItem
	{
		public static void SerializeItemsAsJSON(StoreItem[] items)
		{
			string text = "";
			foreach (StoreItem storeItem in items)
			{
				text = text + JsonUtility.ToJson(storeItem) + ";";
			}
			Debug.LogError(text);
			File.WriteAllText(Application.dataPath + "/Resources/StoreItems/FeaturedStoreItemsList.json", text);
		}

		public static void ConvertCosmeticItemToSToreItem(CosmeticsController.CosmeticItem cosmeticItem, ref StoreItem storeItem)
		{
			storeItem.itemName = cosmeticItem.itemName;
			storeItem.itemCategory = (int)cosmeticItem.itemCategory;
			storeItem.itemPictureResourceString = cosmeticItem.itemPictureResourceString;
			storeItem.displayName = cosmeticItem.displayName;
			storeItem.overrideDisplayName = cosmeticItem.overrideDisplayName;
			storeItem.bundledItems = cosmeticItem.bundledItems;
			storeItem.canTryOn = cosmeticItem.canTryOn;
			storeItem.bothHandsHoldable = cosmeticItem.bothHandsHoldable;
			storeItem.AssetBundleName = "";
			storeItem.bUsesMeshAtlas = cosmeticItem.bUsesMeshAtlas;
			storeItem.MeshResourceName = cosmeticItem.meshResourceString;
			storeItem.MeshAtlasResourceName = cosmeticItem.meshAtlasResourceString;
			storeItem.MaterialResrouceName = cosmeticItem.materialResourceString;
		}

		public string itemName = "";

		public int itemCategory;

		public string itemPictureResourceString = "";

		public string displayName = "";

		public string overrideDisplayName = "";

		public string[] bundledItems = new string[0];

		public bool canTryOn;

		public bool bothHandsHoldable;

		public string AssetBundleName = "";

		public bool bUsesMeshAtlas;

		public string MeshAtlasResourceName = "";

		public string MeshResourceName = "";

		public string MaterialResrouceName = "";

		public Vector3 translationOffset = Vector3.zero;

		public Vector3 rotationOffset = Vector3.zero;

		public Vector3 scale = Vector3.one;
	}
}
