using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaNetworking;
using GorillaNetworking.Store;
using GT_CustomMapSupportRuntime;
using PlayFab;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.VirtualStumpCustomMaps
{
	[CreateAssetMenu(menuName = "ScriptableObjects/CustomMapCosmeticDataSO", order = 0)]
	[Serializable]
	public class CustomMapCosmeticsData : ScriptableObject
	{
		public void OnEnable()
		{
			this.initializedFromTitleData = false;
		}

		public void OnDestroy()
		{
			if (PlayFabTitleDataCache.Instance.IsNotNull())
			{
				PlayFabTitleDataCache.Instance.OnTitleDataUpdate.RemoveListener(new UnityAction<string>(this.OnTitleDataUpdated));
			}
		}

		public bool TryGetItem(GTObjectPlaceholder.ECustomMapCosmeticItem customMapItemSlot, out CustomMapCosmeticItem foundItem)
		{
			if (!this.initializedFromTitleData)
			{
				this.UpdateFromTitleData();
			}
			foundItem = new CustomMapCosmeticItem
			{
				bustType = HeadModel_CosmeticStand.BustType.Disabled,
				playFabID = "INVALID"
			};
			for (int i = 0; i < this.customMapCosmeticItemList.Count; i++)
			{
				if (this.customMapCosmeticItemList[i].customMapItemSlot == customMapItemSlot)
				{
					foundItem = this.customMapCosmeticItemList[i];
					return true;
				}
			}
			for (int j = 0; j < this.fallbackItems.Count; j++)
			{
				if (this.fallbackItems[j].customMapItemSlot == customMapItemSlot)
				{
					foundItem = this.fallbackItems[j];
					return true;
				}
			}
			return false;
		}

		private void UpdateFromTitleData()
		{
			if (this.initializedFromTitleData)
			{
				return;
			}
			if (PlayFabTitleDataCache.Instance.IsNull())
			{
				return;
			}
			PlayFabTitleDataCache.Instance.OnTitleDataUpdate.RemoveListener(new UnityAction<string>(this.OnTitleDataUpdated));
			PlayFabTitleDataCache.Instance.OnTitleDataUpdate.AddListener(new UnityAction<string>(this.OnTitleDataUpdated));
			if (PlayFabTitleDataCache.Instance == null)
			{
				Debug.LogError("[CustomMapCosmeticsData::UpdateFromTitleData] TitleData not available, using fallback item data.");
				this.initializedFromTitleData = true;
				return;
			}
			PlayFabTitleDataCache.Instance.GetTitleData(this.titleDataKey, new Action<string>(this.OnGetCosmeticsDataFromTitleData), new Action<PlayFabError>(this.OnPlayFabError), false);
			this.initializedFromTitleData = true;
		}

		private void OnTitleDataUpdated(string updatedKey)
		{
			if (updatedKey == this.titleDataKey)
			{
				this.initializedFromTitleData = false;
				this.UpdateFromTitleData();
			}
		}

		private void OnGetCosmeticsDataFromTitleData(string cosmeticsData)
		{
			string[] array = cosmeticsData.Split("|", StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				string text2 = text;
				text2 = text2.RemoveAll('\\', StringComparison.OrdinalIgnoreCase);
				text2 = text2.Trim('"');
				CustomMapCosmeticItem itemFromJson = JsonUtility.FromJson<CustomMapCosmeticItem>(text2);
				this.customMapCosmeticItemList.RemoveAll((CustomMapCosmeticItem item) => item.customMapItemSlot == itemFromJson.customMapItemSlot);
				this.customMapCosmeticItemList.Add(itemFromJson);
			}
		}

		private void OnPlayFabError(PlayFabError error)
		{
			Debug.LogError("[CustomMapCosmeticsData::OnPlayFabError] failed to retrieve CosmeticsData from PlayFab: " + error.ErrorMessage);
		}

		[SerializeField]
		private List<CustomMapCosmeticItem> fallbackItems;

		[SerializeField]
		private List<CustomMapCosmeticItem> customMapCosmeticItemList;

		public string titleDataKey = "CustomMapCosmeticData";

		private bool initializedFromTitleData;
	}
}
