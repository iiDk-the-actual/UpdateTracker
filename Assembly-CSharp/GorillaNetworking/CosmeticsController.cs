using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CosmeticRoom;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking.Store;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using Steamworks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace GorillaNetworking
{
	public class CosmeticsController : MonoBehaviour, IGorillaSliceableSimple, IBuildValidation
	{
		public CosmeticInfoV2[] v2_allCosmetics { get; private set; }

		public bool v2_allCosmeticsInfoAssetRef_isLoaded { get; private set; }

		public bool v2_isGetCosmeticsPlayCatalogDataWaitingForCallback { get; private set; }

		public bool v2_isCosmeticPlayFabCatalogDataLoaded { get; private set; }

		private void V2Awake()
		{
			this._allCosmetics = null;
			base.StartCoroutine(this.V2_allCosmeticsInfoAssetRefSO_LoadCoroutine());
		}

		private IEnumerator V2_allCosmeticsInfoAssetRefSO_LoadCoroutine()
		{
			while (!PlayFabAuthenticator.instance)
			{
				yield return new WaitForSeconds(1f);
			}
			float[] retryWaitTimes = new float[]
			{
				1f, 2f, 4f, 4f, 10f, 10f, 10f, 10f, 10f, 10f,
				10f, 10f, 10f, 10f, 30f
			};
			int retryCount = 0;
			AsyncOperationHandle<AllCosmeticsArraySO> newSysAllCosmeticsAsyncOp;
			for (;;)
			{
				Debug.Log(string.Format("Attempting to load runtime key \"{0}\" ", this.v2_allCosmeticsInfoAssetRef.RuntimeKey) + string.Format("(Attempt: {0})", retryCount + 1));
				newSysAllCosmeticsAsyncOp = this.v2_allCosmeticsInfoAssetRef.LoadAssetAsync();
				yield return newSysAllCosmeticsAsyncOp;
				if (ApplicationQuittingState.IsQuitting)
				{
					break;
				}
				if (!newSysAllCosmeticsAsyncOp.IsValid())
				{
					Debug.LogError("`newSysAllCosmeticsAsyncOp` (should never happen) became invalid some how.");
				}
				if (newSysAllCosmeticsAsyncOp.Status == AsyncOperationStatus.Succeeded)
				{
					goto Block_4;
				}
				Debug.LogError(string.Format("Failed to load \"{0}\". ", this.v2_allCosmeticsInfoAssetRef.RuntimeKey) + "Error: " + newSysAllCosmeticsAsyncOp.OperationException.Message);
				float num = retryWaitTimes[Mathf.Min(retryCount, retryWaitTimes.Length - 1)];
				yield return new WaitForSecondsRealtime(num);
				int num2 = retryCount;
				retryCount = num2 + 1;
				newSysAllCosmeticsAsyncOp = default(AsyncOperationHandle<AllCosmeticsArraySO>);
			}
			yield break;
			Block_4:
			this.V2_allCosmeticsInfoAssetRef_LoadSucceeded(newSysAllCosmeticsAsyncOp.Result);
			yield break;
		}

		private void V2_allCosmeticsInfoAssetRef_LoadSucceeded(AllCosmeticsArraySO allCosmeticsSO)
		{
			this.v2_allCosmetics = new CosmeticInfoV2[allCosmeticsSO.sturdyAssetRefs.Length];
			for (int i = 0; i < allCosmeticsSO.sturdyAssetRefs.Length; i++)
			{
				this.v2_allCosmetics[i] = allCosmeticsSO.sturdyAssetRefs[i].obj.info;
			}
			this._allCosmetics = new List<CosmeticsController.CosmeticItem>(allCosmeticsSO.sturdyAssetRefs.Length);
			for (int j = 0; j < this.v2_allCosmetics.Length; j++)
			{
				CosmeticInfoV2 cosmeticInfoV = this.v2_allCosmetics[j];
				string playFabID = cosmeticInfoV.playFabID;
				this._allCosmeticsDictV2[playFabID] = cosmeticInfoV;
				CosmeticsController.CosmeticItem cosmeticItem = new CosmeticsController.CosmeticItem
				{
					itemName = playFabID,
					itemCategory = cosmeticInfoV.category,
					isHoldable = cosmeticInfoV.hasHoldableParts,
					displayName = playFabID,
					itemPicture = cosmeticInfoV.icon,
					overrideDisplayName = cosmeticInfoV.displayName,
					bothHandsHoldable = cosmeticInfoV.usesBothHandSlots,
					isNullItem = false
				};
				this._allCosmetics.Add(cosmeticItem);
			}
			this.v2_allCosmeticsInfoAssetRef_isLoaded = true;
			Action v2_allCosmeticsInfoAssetRef_OnPostLoad = this.V2_allCosmeticsInfoAssetRef_OnPostLoad;
			if (v2_allCosmeticsInfoAssetRef_OnPostLoad == null)
			{
				return;
			}
			v2_allCosmeticsInfoAssetRef_OnPostLoad();
		}

		public bool TryGetCosmeticInfoV2(string playFabId, out CosmeticInfoV2 cosmeticInfo)
		{
			return this._allCosmeticsDictV2.TryGetValue(playFabId, out cosmeticInfo);
		}

		private void V2_ConformCosmeticItemV1DisplayName(ref CosmeticsController.CosmeticItem cosmetic)
		{
			if (cosmetic.itemName == cosmetic.displayName)
			{
				return;
			}
			cosmetic.overrideDisplayName = cosmetic.displayName;
			cosmetic.displayName = cosmetic.itemName;
		}

		internal void InitializeCosmeticStands()
		{
			foreach (CosmeticStand cosmeticStand in this.cosmeticStands)
			{
				if (cosmeticStand != null)
				{
					cosmeticStand.InitializeCosmetic();
				}
			}
		}

		public static bool hasInstance { get; private set; }

		public List<CosmeticsController.CosmeticItem> allCosmetics
		{
			get
			{
				return this._allCosmetics;
			}
			set
			{
				this._allCosmetics = value;
			}
		}

		public bool allCosmeticsDict_isInitialized { get; private set; }

		public Dictionary<string, CosmeticsController.CosmeticItem> allCosmeticsDict
		{
			get
			{
				return this._allCosmeticsDict;
			}
		}

		public bool allCosmeticsItemIDsfromDisplayNamesDict_isInitialized { get; private set; }

		public Dictionary<string, string> allCosmeticsItemIDsfromDisplayNamesDict
		{
			get
			{
				return this._allCosmeticsItemIDsfromDisplayNamesDict;
			}
		}

		public CosmeticAnchorAntiIntersectOffsets defaultClipOffsets
		{
			get
			{
				return CosmeticAnchorAntiIntersectOffsets.Identity;
			}
		}

		public bool isHidingCosmeticsFromRemotePlayers { get; private set; }

		public void AddWardrobeInstance(WardrobeInstance instance)
		{
			this.wardrobes.Add(instance);
			if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
			{
				this.UpdateWardrobeModelsAndButtons();
			}
		}

		public void RemoveWardrobeInstance(WardrobeInstance instance)
		{
			this.wardrobes.Remove(instance);
		}

		public int CurrencyBalance
		{
			get
			{
				return this.currencyBalance;
			}
		}

		public void Awake()
		{
			if (CosmeticsController.instance == null)
			{
				CosmeticsController.instance = this;
				CosmeticsController.hasInstance = true;
			}
			else if (CosmeticsController.instance != this)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			this.V2Awake();
			if (base.gameObject.activeSelf)
			{
				this.catalog = "DLC";
				this.currencyName = "SR";
				this.nullItem = default(CosmeticsController.CosmeticItem);
				this.nullItem.itemName = "null";
				this.nullItem.displayName = "NOTHING";
				this.nullItem.itemPicture = Resources.Load<Sprite>("CosmeticNull_Icon");
				this.nullItem.itemPictureResourceString = "";
				this.nullItem.overrideDisplayName = "NOTHING";
				this.nullItem.meshAtlasResourceString = "";
				this.nullItem.meshResourceString = "";
				this.nullItem.materialResourceString = "";
				this.nullItem.isNullItem = true;
				this._allCosmeticsDict[this.nullItem.itemName] = this.nullItem;
				this._allCosmeticsItemIDsfromDisplayNamesDict[this.nullItem.displayName] = this.nullItem.itemName;
				for (int i = 0; i < 16; i++)
				{
					this.tryOnSet.items[i] = this.nullItem;
					this.tempUnlockedSet.items[i] = this.nullItem;
					this.activeMergedSet.items[i] = this.nullItem;
				}
				this.cosmeticsPages[0] = 0;
				this.cosmeticsPages[1] = 0;
				this.cosmeticsPages[2] = 0;
				this.cosmeticsPages[3] = 0;
				this.cosmeticsPages[4] = 0;
				this.cosmeticsPages[5] = 0;
				this.cosmeticsPages[6] = 0;
				this.cosmeticsPages[7] = 0;
				this.cosmeticsPages[8] = 0;
				this.cosmeticsPages[9] = 0;
				this.cosmeticsPages[10] = 0;
				this.itemLists[0] = this.unlockedHats;
				this.itemLists[1] = this.unlockedFaces;
				this.itemLists[2] = this.unlockedBadges;
				this.itemLists[3] = this.unlockedPaws;
				this.itemLists[4] = this.unlockedFurs;
				this.itemLists[5] = this.unlockedShirts;
				this.itemLists[6] = this.unlockedPants;
				this.itemLists[7] = this.unlockedArms;
				this.itemLists[8] = this.unlockedBacks;
				this.itemLists[9] = this.unlockedChests;
				this.itemLists[10] = this.unlockedTagFX;
				this.updateCosmeticsRetries = 0;
				this.maxUpdateCosmeticsRetries = 5;
				this.inventoryStringList.Clear();
				this.inventoryStringList.Add("Inventory");
				base.StartCoroutine(this.CheckCanGetDaily());
			}
		}

		public void Start()
		{
			PlayFabTitleDataCache.Instance.GetTitleData("BundleData", delegate(string data)
			{
				this.bundleList.FromJson(data);
			}, delegate(PlayFabError e)
			{
				Debug.LogError(string.Format("Error getting bundle data: {0}", e));
			}, false);
			this.anchorOverrides = GorillaTagger.Instance.offlineVRRig.GetComponent<VRRigAnchorOverrides>();
		}

		public void OnEnable()
		{
			GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
			if (SteamManager.Initialized && this._steamMicroTransactionAuthorizationResponse == null)
			{
				this._steamMicroTransactionAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(new Callback<MicroTxnAuthorizationResponse_t>.DispatchDelegate(this.ProcessSteamCallback));
			}
		}

		public void OnDisable()
		{
			Callback<MicroTxnAuthorizationResponse_t> steamMicroTransactionAuthorizationResponse = this._steamMicroTransactionAuthorizationResponse;
			if (steamMicroTransactionAuthorizationResponse != null)
			{
				steamMicroTransactionAuthorizationResponse.Unregister();
			}
			GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		}

		public void SliceUpdate()
		{
		}

		public static bool CompareCategoryToSavedCosmeticSlots(CosmeticsController.CosmeticCategory category, CosmeticsController.CosmeticSlots slot)
		{
			switch (category)
			{
			case CosmeticsController.CosmeticCategory.Hat:
				return slot == CosmeticsController.CosmeticSlots.Hat;
			case CosmeticsController.CosmeticCategory.Badge:
				return CosmeticsController.CosmeticSlots.Badge == slot;
			case CosmeticsController.CosmeticCategory.Face:
				return CosmeticsController.CosmeticSlots.Face == slot;
			case CosmeticsController.CosmeticCategory.Paw:
				return slot == CosmeticsController.CosmeticSlots.HandRight || slot == CosmeticsController.CosmeticSlots.HandLeft;
			case CosmeticsController.CosmeticCategory.Chest:
				return CosmeticsController.CosmeticSlots.Chest == slot;
			case CosmeticsController.CosmeticCategory.Fur:
				return CosmeticsController.CosmeticSlots.Fur == slot;
			case CosmeticsController.CosmeticCategory.Shirt:
				return CosmeticsController.CosmeticSlots.Shirt == slot;
			case CosmeticsController.CosmeticCategory.Back:
				return slot == CosmeticsController.CosmeticSlots.BackLeft || slot == CosmeticsController.CosmeticSlots.BackRight;
			case CosmeticsController.CosmeticCategory.Arms:
				return slot == CosmeticsController.CosmeticSlots.ArmLeft || slot == CosmeticsController.CosmeticSlots.ArmRight;
			case CosmeticsController.CosmeticCategory.Pants:
				return CosmeticsController.CosmeticSlots.Pants == slot;
			case CosmeticsController.CosmeticCategory.TagEffect:
				return CosmeticsController.CosmeticSlots.TagEffect == slot;
			default:
				return false;
			}
		}

		public static CosmeticsController.CosmeticSlots CategoryToNonTransferrableSlot(CosmeticsController.CosmeticCategory category)
		{
			switch (category)
			{
			case CosmeticsController.CosmeticCategory.Hat:
				return CosmeticsController.CosmeticSlots.Hat;
			case CosmeticsController.CosmeticCategory.Badge:
				return CosmeticsController.CosmeticSlots.Badge;
			case CosmeticsController.CosmeticCategory.Face:
				return CosmeticsController.CosmeticSlots.Face;
			case CosmeticsController.CosmeticCategory.Paw:
				return CosmeticsController.CosmeticSlots.HandRight;
			case CosmeticsController.CosmeticCategory.Chest:
				return CosmeticsController.CosmeticSlots.Chest;
			case CosmeticsController.CosmeticCategory.Fur:
				return CosmeticsController.CosmeticSlots.Fur;
			case CosmeticsController.CosmeticCategory.Shirt:
				return CosmeticsController.CosmeticSlots.Shirt;
			case CosmeticsController.CosmeticCategory.Back:
				return CosmeticsController.CosmeticSlots.Back;
			case CosmeticsController.CosmeticCategory.Arms:
				return CosmeticsController.CosmeticSlots.Arms;
			case CosmeticsController.CosmeticCategory.Pants:
				return CosmeticsController.CosmeticSlots.Pants;
			case CosmeticsController.CosmeticCategory.TagEffect:
				return CosmeticsController.CosmeticSlots.TagEffect;
			default:
				return CosmeticsController.CosmeticSlots.Count;
			}
		}

		private CosmeticsController.CosmeticSlots DropPositionToCosmeticSlot(BodyDockPositions.DropPositions pos)
		{
			switch (pos)
			{
			case BodyDockPositions.DropPositions.LeftArm:
				return CosmeticsController.CosmeticSlots.ArmLeft;
			case BodyDockPositions.DropPositions.RightArm:
				return CosmeticsController.CosmeticSlots.ArmRight;
			case BodyDockPositions.DropPositions.LeftArm | BodyDockPositions.DropPositions.RightArm:
				break;
			case BodyDockPositions.DropPositions.Chest:
				return CosmeticsController.CosmeticSlots.Chest;
			default:
				if (pos == BodyDockPositions.DropPositions.LeftBack)
				{
					return CosmeticsController.CosmeticSlots.BackLeft;
				}
				if (pos == BodyDockPositions.DropPositions.RightBack)
				{
					return CosmeticsController.CosmeticSlots.BackRight;
				}
				break;
			}
			return CosmeticsController.CosmeticSlots.Count;
		}

		private static BodyDockPositions.DropPositions CosmeticSlotToDropPosition(CosmeticsController.CosmeticSlots slot)
		{
			switch (slot)
			{
			case CosmeticsController.CosmeticSlots.ArmLeft:
				return BodyDockPositions.DropPositions.LeftArm;
			case CosmeticsController.CosmeticSlots.ArmRight:
				return BodyDockPositions.DropPositions.RightArm;
			case CosmeticsController.CosmeticSlots.BackLeft:
				return BodyDockPositions.DropPositions.LeftBack;
			case CosmeticsController.CosmeticSlots.BackRight:
				return BodyDockPositions.DropPositions.RightBack;
			case CosmeticsController.CosmeticSlots.Chest:
				return BodyDockPositions.DropPositions.Chest;
			}
			return BodyDockPositions.DropPositions.None;
		}

		public void AddItemCheckout(ItemCheckout newItemCheckout)
		{
			if (this.itemCheckouts.Contains(newItemCheckout))
			{
				return;
			}
			this.itemCheckouts.Add(newItemCheckout);
			this.UpdateShoppingCart();
			this.FormattedPurchaseText(this.finalLine, this.leftCheckoutPurchaseButtonString, this.rightCheckoutPurchaseButtonString, this.leftCheckoutPurchaseButtonOn, this.rightCheckoutPurchaseButtonOn);
			if (!this.itemToBuy.isNullItem)
			{
				this.RefreshItemToBuyPreview();
			}
		}

		public void RemoveItemCheckout(ItemCheckout checkoutToRemove)
		{
			this.itemCheckouts.Remove(checkoutToRemove);
		}

		public void AddFittingRoom(FittingRoom newFittingRoom)
		{
			if (this.fittingRooms.Contains(newFittingRoom))
			{
				return;
			}
			this.fittingRooms.Add(newFittingRoom);
			this.UpdateShoppingCart();
		}

		public void RemoveFittingRoom(FittingRoom fittingRoomToRemove)
		{
			this.fittingRooms.Remove(fittingRoomToRemove);
		}

		private void SaveItemPreference(CosmeticsController.CosmeticSlots slot, int slotIdx, CosmeticsController.CosmeticItem newItem)
		{
			PlayerPrefs.SetString(CosmeticsController.CosmeticSet.SlotPlayerPreferenceName(slot), newItem.itemName);
			PlayerPrefs.Save();
		}

		public void SaveCurrentItemPreferences()
		{
			for (int i = 0; i < 16; i++)
			{
				CosmeticsController.CosmeticSlots cosmeticSlots = (CosmeticsController.CosmeticSlots)i;
				this.SaveItemPreference(cosmeticSlots, i, this.currentWornSet.items[i]);
			}
		}

		private void ApplyCosmeticToSet(CosmeticsController.CosmeticSet set, CosmeticsController.CosmeticItem newItem, int slotIdx, CosmeticsController.CosmeticSlots slot, bool applyToPlayerPrefs, List<CosmeticsController.CosmeticSlots> appliedSlots)
		{
			CosmeticsController.CosmeticItem cosmeticItem = ((set.items[slotIdx].itemName == newItem.itemName) ? this.nullItem : newItem);
			set.items[slotIdx] = cosmeticItem;
			if (applyToPlayerPrefs)
			{
				this.SaveItemPreference(slot, slotIdx, cosmeticItem);
			}
			appliedSlots.Add(slot);
		}

		private void PrivApplyCosmeticItemToSet(CosmeticsController.CosmeticSet set, CosmeticsController.CosmeticItem newItem, bool isLeftHand, bool applyToPlayerPrefs, List<CosmeticsController.CosmeticSlots> appliedSlots)
		{
			if (newItem.isNullItem)
			{
				return;
			}
			if (CosmeticsController.CosmeticSet.IsHoldable(newItem))
			{
				BodyDockPositions.DockingResult dockingResult = GorillaTagger.Instance.offlineVRRig.GetComponent<BodyDockPositions>().ToggleWithHandedness(newItem.displayName, isLeftHand, newItem.bothHandsHoldable);
				foreach (BodyDockPositions.DropPositions dropPositions in dockingResult.positionsDisabled)
				{
					CosmeticsController.CosmeticSlots cosmeticSlots = this.DropPositionToCosmeticSlot(dropPositions);
					if (cosmeticSlots != CosmeticsController.CosmeticSlots.Count)
					{
						int num = (int)cosmeticSlots;
						set.items[num] = this.nullItem;
						if (applyToPlayerPrefs)
						{
							this.SaveItemPreference(cosmeticSlots, num, this.nullItem);
						}
					}
				}
				using (List<BodyDockPositions.DropPositions>.Enumerator enumerator = dockingResult.dockedPosition.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						BodyDockPositions.DropPositions dropPositions2 = enumerator.Current;
						if (dropPositions2 != BodyDockPositions.DropPositions.None)
						{
							CosmeticsController.CosmeticSlots cosmeticSlots2 = this.DropPositionToCosmeticSlot(dropPositions2);
							int num2 = (int)cosmeticSlots2;
							set.items[num2] = newItem;
							if (applyToPlayerPrefs)
							{
								this.SaveItemPreference(cosmeticSlots2, num2, newItem);
							}
							appliedSlots.Add(cosmeticSlots2);
						}
					}
					return;
				}
			}
			if (newItem.itemCategory == CosmeticsController.CosmeticCategory.Paw)
			{
				CosmeticsController.CosmeticSlots cosmeticSlots3 = (isLeftHand ? CosmeticsController.CosmeticSlots.HandLeft : CosmeticsController.CosmeticSlots.HandRight);
				int num3 = (int)cosmeticSlots3;
				this.ApplyCosmeticToSet(set, newItem, num3, cosmeticSlots3, applyToPlayerPrefs, appliedSlots);
				CosmeticsController.CosmeticSlots cosmeticSlots4 = CosmeticsController.CosmeticSet.OppositeSlot(cosmeticSlots3);
				int num4 = (int)cosmeticSlots4;
				if (newItem.bothHandsHoldable)
				{
					this.ApplyCosmeticToSet(set, this.nullItem, num4, cosmeticSlots4, applyToPlayerPrefs, appliedSlots);
					return;
				}
				if (set.items[num4].itemName == newItem.itemName)
				{
					this.ApplyCosmeticToSet(set, this.nullItem, num4, cosmeticSlots4, applyToPlayerPrefs, appliedSlots);
				}
				if (set.items[num4].bothHandsHoldable)
				{
					this.ApplyCosmeticToSet(set, this.nullItem, num4, cosmeticSlots4, applyToPlayerPrefs, appliedSlots);
					return;
				}
			}
			else
			{
				CosmeticsController.CosmeticSlots cosmeticSlots5 = CosmeticsController.CategoryToNonTransferrableSlot(newItem.itemCategory);
				int num5 = (int)cosmeticSlots5;
				this.ApplyCosmeticToSet(set, newItem, num5, cosmeticSlots5, applyToPlayerPrefs, appliedSlots);
			}
		}

		public void ApplyCosmeticItemToSet(CosmeticsController.CosmeticSet set, CosmeticsController.CosmeticItem newItem, bool isLeftHand, bool applyToPlayerPrefs)
		{
			this.ApplyCosmeticItemToSet(set, newItem, isLeftHand, applyToPlayerPrefs, CosmeticsController._g_default_outAppliedSlotsList_for_applyCosmeticItemToSet);
		}

		public void ApplyCosmeticItemToSet(CosmeticsController.CosmeticSet set, CosmeticsController.CosmeticItem newItem, bool isLeftHand, bool applyToPlayerPrefs, List<CosmeticsController.CosmeticSlots> outAppliedSlotsList)
		{
			outAppliedSlotsList.Clear();
			if (newItem.itemCategory == CosmeticsController.CosmeticCategory.Set)
			{
				bool flag = false;
				Dictionary<CosmeticsController.CosmeticItem, bool> dictionary = new Dictionary<CosmeticsController.CosmeticItem, bool>();
				foreach (string text in newItem.bundledItems)
				{
					CosmeticsController.CosmeticItem itemFromDict = this.GetItemFromDict(text);
					if (this.AnyMatch(set, itemFromDict))
					{
						flag = true;
						dictionary.Add(itemFromDict, true);
					}
					else
					{
						dictionary.Add(itemFromDict, false);
					}
				}
				using (Dictionary<CosmeticsController.CosmeticItem, bool>.Enumerator enumerator = dictionary.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<CosmeticsController.CosmeticItem, bool> keyValuePair = enumerator.Current;
						if (flag)
						{
							if (keyValuePair.Value)
							{
								this.PrivApplyCosmeticItemToSet(set, keyValuePair.Key, isLeftHand, applyToPlayerPrefs, outAppliedSlotsList);
							}
						}
						else
						{
							this.PrivApplyCosmeticItemToSet(set, keyValuePair.Key, isLeftHand, applyToPlayerPrefs, outAppliedSlotsList);
						}
					}
					return;
				}
			}
			this.PrivApplyCosmeticItemToSet(set, newItem, isLeftHand, applyToPlayerPrefs, outAppliedSlotsList);
		}

		public void RemoveCosmeticItemFromSet(CosmeticsController.CosmeticSet set, string itemName, bool applyToPlayerPrefs)
		{
			this.cachedSet.CopyItems(set);
			for (int i = 0; i < 16; i++)
			{
				if (set.items[i].displayName == itemName)
				{
					set.items[i] = this.nullItem;
					if (applyToPlayerPrefs)
					{
						this.SaveItemPreference((CosmeticsController.CosmeticSlots)i, i, this.nullItem);
					}
				}
			}
			VRRig offlineVRRig = GorillaTagger.Instance.offlineVRRig;
			BodyDockPositions component = offlineVRRig.GetComponent<BodyDockPositions>();
			set.ActivateCosmetics(this.cachedSet, offlineVRRig, component, offlineVRRig.cosmeticsObjectRegistry);
		}

		public void PressFittingRoomButton(FittingRoomButton pressedFittingRoomButton, bool isLeftHand)
		{
			BundleManager.instance._tryOnBundlesStand.ClearSelectedBundle();
			this.ApplyCosmeticItemToSet(this.tryOnSet, pressedFittingRoomButton.currentCosmeticItem, isLeftHand, false);
			this.UpdateShoppingCart();
			this.UpdateWornCosmetics(true);
		}

		public CosmeticsController.EWearingCosmeticSet CheckIfCosmeticSetMatchesItemSet(CosmeticsController.CosmeticSet set, string itemName)
		{
			CosmeticsController.EWearingCosmeticSet ewearingCosmeticSet = CosmeticsController.EWearingCosmeticSet.NotASet;
			CosmeticsController.CosmeticItem cosmeticItem = this.allCosmeticsDict[itemName];
			if (cosmeticItem.bundledItems.Length != 0)
			{
				foreach (string text in cosmeticItem.bundledItems)
				{
					if (this.AnyMatch(set, this.allCosmeticsDict[text]))
					{
						if (ewearingCosmeticSet == CosmeticsController.EWearingCosmeticSet.NotASet)
						{
							ewearingCosmeticSet = CosmeticsController.EWearingCosmeticSet.Complete;
						}
						else if (ewearingCosmeticSet == CosmeticsController.EWearingCosmeticSet.NotWearing)
						{
							ewearingCosmeticSet = CosmeticsController.EWearingCosmeticSet.Partial;
						}
					}
					else if (ewearingCosmeticSet == CosmeticsController.EWearingCosmeticSet.NotASet)
					{
						ewearingCosmeticSet = CosmeticsController.EWearingCosmeticSet.NotWearing;
					}
					else if (ewearingCosmeticSet == CosmeticsController.EWearingCosmeticSet.Complete)
					{
						ewearingCosmeticSet = CosmeticsController.EWearingCosmeticSet.Partial;
					}
				}
			}
			return ewearingCosmeticSet;
		}

		public void PressCosmeticStandButton(CosmeticStand pressedStand)
		{
			this.searchIndex = this.currentCart.IndexOf(pressedStand.thisCosmeticItem);
			if (this.searchIndex != -1)
			{
				GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.cart_item_remove, pressedStand.thisCosmeticItem);
				this.currentCart.RemoveAt(this.searchIndex);
				pressedStand.isOn = false;
				for (int i = 0; i < 16; i++)
				{
					if (pressedStand.thisCosmeticItem.itemName == this.tryOnSet.items[i].itemName)
					{
						this.tryOnSet.items[i] = this.nullItem;
					}
				}
			}
			else
			{
				GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.cart_item_add, pressedStand.thisCosmeticItem);
				this.currentCart.Insert(0, pressedStand.thisCosmeticItem);
				pressedStand.isOn = true;
				if (this.currentCart.Count > this.numFittingRoomButtons)
				{
					foreach (CosmeticStand cosmeticStand in this.cosmeticStands)
					{
						if (!(cosmeticStand == null) && cosmeticStand.thisCosmeticItem.itemName == this.currentCart[this.numFittingRoomButtons].itemName)
						{
							cosmeticStand.isOn = false;
							cosmeticStand.UpdateColor();
							break;
						}
					}
					this.currentCart.RemoveAt(this.numFittingRoomButtons);
				}
			}
			pressedStand.UpdateColor();
			this.UpdateShoppingCart();
		}

		public void PressWardrobeItemButton(CosmeticsController.CosmeticItem cosmeticItem, bool isLeftHand, bool isTempCosm)
		{
			if (cosmeticItem.isNullItem)
			{
				return;
			}
			CosmeticsController.CosmeticItem itemFromDict = this.GetItemFromDict(cosmeticItem.itemName);
			if (isTempCosm)
			{
				this.PressTemporaryWardrobeItemButton(itemFromDict, isLeftHand);
			}
			else
			{
				this.PressWardrobeItemButton(itemFromDict, isLeftHand);
			}
			this.UpdateWornCosmetics(true);
			Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
			if (onCosmeticsUpdated == null)
			{
				return;
			}
			onCosmeticsUpdated();
		}

		private void PressWardrobeItemButton(CosmeticsController.CosmeticItem item, bool isLeftHand)
		{
			List<CosmeticsController.CosmeticSlots> list = CollectionPool<List<CosmeticsController.CosmeticSlots>, CosmeticsController.CosmeticSlots>.Get();
			if (list.Capacity < 16)
			{
				list.Capacity = 16;
			}
			this.ApplyCosmeticItemToSet(this.currentWornSet, item, isLeftHand, true, list);
			foreach (CosmeticsController.CosmeticSlots cosmeticSlots in list)
			{
				this.tryOnSet.items[(int)cosmeticSlots] = this.nullItem;
			}
			CollectionPool<List<CosmeticsController.CosmeticSlots>, CosmeticsController.CosmeticSlots>.Release(list);
			this.UpdateShoppingCart();
		}

		private void PressTemporaryWardrobeItemButton(CosmeticsController.CosmeticItem item, bool isLeftHand)
		{
			this.ApplyCosmeticItemToSet(this.tempUnlockedSet, item, isLeftHand, false);
		}

		public void PressWardrobeFunctionButton(string function)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(function);
			if (num <= 2554875734U)
			{
				if (num <= 895779448U)
				{
					if (num != 292255708U)
					{
						if (num != 306900080U)
						{
							if (num == 895779448U)
							{
								if (function == "badge")
								{
									if (this.wardrobeType == 2)
									{
										return;
									}
									this.wardrobeType = 2;
								}
							}
						}
						else if (function == "left")
						{
							this.cosmeticsPages[this.wardrobeType] = this.cosmeticsPages[this.wardrobeType] - 1;
							if (this.cosmeticsPages[this.wardrobeType] < 0)
							{
								this.cosmeticsPages[this.wardrobeType] = (this.itemLists[this.wardrobeType].Count - 1) / 3;
							}
						}
					}
					else if (function == "face")
					{
						if (this.wardrobeType == 1)
						{
							return;
						}
						this.wardrobeType = 1;
					}
				}
				else if (num != 1538531746U)
				{
					if (num != 2028154341U)
					{
						if (num == 2554875734U)
						{
							if (function == "chest")
							{
								if (this.wardrobeType == 8)
								{
									return;
								}
								this.wardrobeType = 8;
							}
						}
					}
					else if (function == "right")
					{
						this.cosmeticsPages[this.wardrobeType] = this.cosmeticsPages[this.wardrobeType] + 1;
						if (this.cosmeticsPages[this.wardrobeType] > (this.itemLists[this.wardrobeType].Count - 1) / 3)
						{
							this.cosmeticsPages[this.wardrobeType] = 0;
						}
					}
				}
				else if (function == "back")
				{
					if (this.wardrobeType == 7)
					{
						return;
					}
					this.wardrobeType = 7;
				}
			}
			else if (num <= 3034286914U)
			{
				if (num != 2633735346U)
				{
					if (num != 2953262278U)
					{
						if (num == 3034286914U)
						{
							if (function == "fur")
							{
								if (this.wardrobeType == 4)
								{
									return;
								}
								this.wardrobeType = 4;
							}
						}
					}
					else if (function == "outfit")
					{
						if (this.wardrobeType == 5)
						{
							return;
						}
						this.wardrobeType = 5;
					}
				}
				else if (function == "arms")
				{
					if (this.wardrobeType == 6)
					{
						return;
					}
					this.wardrobeType = 6;
				}
			}
			else if (num <= 3300536096U)
			{
				if (num != 3081164502U)
				{
					if (num == 3300536096U)
					{
						if (function == "hand")
						{
							if (this.wardrobeType == 3)
							{
								return;
							}
							this.wardrobeType = 3;
						}
					}
				}
				else if (function == "tagEffect")
				{
					if (this.wardrobeType == 10)
					{
						return;
					}
					this.wardrobeType = 10;
				}
			}
			else if (num != 3568683773U)
			{
				if (num == 4072609730U)
				{
					if (function == "hat")
					{
						if (this.wardrobeType == 0)
						{
							return;
						}
						this.wardrobeType = 0;
					}
				}
			}
			else if (function == "reserved")
			{
				if (this.wardrobeType == 9)
				{
					return;
				}
				this.wardrobeType = 9;
			}
			this.UpdateWardrobeModelsAndButtons();
			Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
			if (onCosmeticsUpdated == null)
			{
				return;
			}
			onCosmeticsUpdated();
		}

		public void ClearCheckout(bool sendEvent)
		{
			if (sendEvent)
			{
				GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.checkout_cancel, this.currentCart);
			}
			this.itemToBuy = this.nullItem;
			this.RefreshItemToBuyPreview();
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
			this.ProcessPurchaseItemState(null, false);
		}

		public bool RemoveItemFromCart(CosmeticsController.CosmeticItem cosmeticItem)
		{
			this.searchIndex = this.currentCart.IndexOf(cosmeticItem);
			if (this.searchIndex != -1)
			{
				this.currentCart.RemoveAt(this.searchIndex);
				for (int i = 0; i < 16; i++)
				{
					if (cosmeticItem.itemName == this.tryOnSet.items[i].itemName)
					{
						this.tryOnSet.items[i] = this.nullItem;
					}
				}
				return true;
			}
			return false;
		}

		public void ClearCheckoutAndCart(bool sendEvent)
		{
			this.currentCart.Clear();
			this.tryOnSet.ClearSet(this.nullItem);
			this.ClearCheckout(sendEvent);
		}

		public void PressCheckoutCartButton(CheckoutCartButton pressedCheckoutCartButton, bool isLeftHand)
		{
			if (this.currentPurchaseItemStage != CosmeticsController.PurchaseItemStages.Buying)
			{
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
				this.tryOnSet.ClearSet(this.nullItem);
				if (this.itemToBuy.displayName == pressedCheckoutCartButton.currentCosmeticItem.displayName)
				{
					this.itemToBuy = this.nullItem;
					this.RefreshItemToBuyPreview();
				}
				else
				{
					this.itemToBuy = pressedCheckoutCartButton.currentCosmeticItem;
					this.checkoutCartButtonPressedWithLeft = isLeftHand;
					this.RefreshItemToBuyPreview();
				}
				this.ProcessPurchaseItemState(null, isLeftHand);
				this.UpdateShoppingCart();
			}
		}

		private void RefreshItemToBuyPreview()
		{
			if (this.itemToBuy.bundledItems != null && this.itemToBuy.bundledItems.Length != 0)
			{
				List<string> list = new List<string>();
				foreach (string text in this.itemToBuy.bundledItems)
				{
					this.tempItem = this.GetItemFromDict(text);
					list.Add(this.tempItem.displayName);
				}
				this.iterator = 0;
				while (this.iterator < this.itemCheckouts.Count)
				{
					if (!this.itemCheckouts[this.iterator].IsNull())
					{
						this.itemCheckouts[this.iterator].checkoutHeadModel.SetCosmeticActiveArray(list.ToArray(), new bool[list.Count]);
					}
					this.iterator++;
				}
			}
			else
			{
				this.iterator = 0;
				while (this.iterator < this.itemCheckouts.Count)
				{
					if (!this.itemCheckouts[this.iterator].IsNull())
					{
						this.itemCheckouts[this.iterator].checkoutHeadModel.SetCosmeticActive(this.itemToBuy.displayName, false);
					}
					this.iterator++;
				}
			}
			this.ApplyCosmeticItemToSet(this.tryOnSet, this.itemToBuy, this.checkoutCartButtonPressedWithLeft, false);
			this.UpdateWornCosmetics(true);
		}

		public void PressPurchaseItemButton(PurchaseItemButton pressedPurchaseItemButton, bool isLeftHand)
		{
			this.ProcessPurchaseItemState(pressedPurchaseItemButton.buttonSide, isLeftHand);
		}

		public void PurchaseBundle(StoreBundle bundleToPurchase)
		{
			if (bundleToPurchase.playfabBundleID != "NULL")
			{
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
				this.ProcessPurchaseItemState("left", false);
				this.buyingBundle = true;
				this.itemToPurchase = bundleToPurchase.playfabBundleID;
				this.SteamPurchase();
			}
		}

		public void PressEarlyAccessButton()
		{
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
			this.ProcessPurchaseItemState("left", false);
			this.buyingBundle = true;
			this.itemToPurchase = this.BundlePlayfabItemName;
			ATM_Manager.instance.shinyRocksCost = (float)this.BundleShinyRocks;
			this.SteamPurchase();
		}

		public void PressPurchaseBundleButton(string PlayFabItemName)
		{
			BundleManager.instance.BundlePurchaseButtonPressed(PlayFabItemName);
		}

		public void ProcessPurchaseItemState(string buttonSide, bool isLeftHand)
		{
			switch (this.currentPurchaseItemStage)
			{
			case CosmeticsController.PurchaseItemStages.Start:
				this.itemToBuy = this.nullItem;
				this.FormattedPurchaseText("SELECT AN ITEM FROM YOUR CART TO PURCHASE!", null, null, false, false);
				this.UpdateShoppingCart();
				return;
			case CosmeticsController.PurchaseItemStages.CheckoutButtonPressed:
				GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.checkout_start, this.currentCart);
				this.searchIndex = this.unlockedCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => this.itemToBuy.itemName == x.itemName);
				if (this.searchIndex > -1)
				{
					this.FormattedPurchaseText("YOU ALREADY OWN THIS ITEM!", "-", "-", true, true);
					this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.ItemOwned;
					return;
				}
				if (this.itemToBuy.cost <= this.currencyBalance)
				{
					this.FormattedPurchaseText("DO YOU WANT TO BUY THIS ITEM?", "NO!", "YES!", false, false);
					this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.ItemSelected;
					return;
				}
				this.FormattedPurchaseText("INSUFFICIENT SHINY ROCKS FOR THIS ITEM!", "-", "-", true, true);
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
				return;
			case CosmeticsController.PurchaseItemStages.ItemSelected:
				if (buttonSide == "right")
				{
					GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.item_select, this.itemToBuy);
					this.FormattedPurchaseText("ARE YOU REALLY SURE?", "YES! I NEED IT!", "LET ME THINK ABOUT IT", false, false);
					this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.FinalPurchaseAcknowledgement;
					return;
				}
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
				this.ProcessPurchaseItemState(null, isLeftHand);
				return;
			case CosmeticsController.PurchaseItemStages.ItemOwned:
			case CosmeticsController.PurchaseItemStages.Buying:
				break;
			case CosmeticsController.PurchaseItemStages.FinalPurchaseAcknowledgement:
				if (buttonSide == "left")
				{
					this.FormattedPurchaseText("PURCHASING ITEM...", "-", "-", true, true);
					this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Buying;
					this.isLastHandTouchedLeft = isLeftHand;
					this.PurchaseItem();
					return;
				}
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
				this.ProcessPurchaseItemState(null, isLeftHand);
				return;
			case CosmeticsController.PurchaseItemStages.Success:
			{
				this.FormattedPurchaseText("SUCCESS! ENJOY YOUR NEW ITEM!", "-", "-", true, true);
				VRRig offlineVRRig = GorillaTagger.Instance.offlineVRRig;
				offlineVRRig.concatStringOfCosmeticsAllowed += this.itemToBuy.itemName;
				CosmeticsController.CosmeticItem itemFromDict = this.GetItemFromDict(this.itemToBuy.itemName);
				if (itemFromDict.bundledItems != null)
				{
					foreach (string text in itemFromDict.bundledItems)
					{
						VRRig offlineVRRig2 = GorillaTagger.Instance.offlineVRRig;
						offlineVRRig2.concatStringOfCosmeticsAllowed += text;
					}
				}
				this.tryOnSet.ClearSet(this.nullItem);
				this.UpdateShoppingCart();
				this.ApplyCosmeticItemToSet(this.currentWornSet, itemFromDict, isLeftHand, true);
				this.UpdateShoppingCart();
				this.UpdateWornCosmetics(false);
				this.UpdateWardrobeModelsAndButtons();
				Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
				if (onCosmeticsUpdated == null)
				{
					return;
				}
				onCosmeticsUpdated();
				break;
			}
			case CosmeticsController.PurchaseItemStages.Failure:
				this.FormattedPurchaseText("ERROR IN PURCHASING ITEM! NO MONEY WAS SPENT. SELECT ANOTHER ITEM.", "-", "-", true, true);
				return;
			default:
				return;
			}
		}

		public void FormattedPurchaseText(string finalLineVar, string leftPurchaseButtonText = null, string rightPurchaseButtonText = null, bool leftButtonOn = false, bool rightButtonOn = false)
		{
			this.finalLine = finalLineVar;
			if (leftPurchaseButtonText != null)
			{
				this.leftCheckoutPurchaseButtonString = leftPurchaseButtonText;
				this.leftCheckoutPurchaseButtonOn = leftButtonOn;
			}
			if (rightPurchaseButtonText != null)
			{
				this.rightCheckoutPurchaseButtonString = rightPurchaseButtonText;
				this.rightCheckoutPurchaseButtonOn = rightButtonOn;
			}
			string text = string.Concat(new string[]
			{
				"SELECTION: ",
				this.GetItemDisplayName(this.itemToBuy),
				"\nITEM COST: ",
				this.itemToBuy.cost.ToString(),
				"\nYOU HAVE: ",
				this.currencyBalance.ToString(),
				"\n\n",
				this.finalLine
			});
			this.iterator = 0;
			while (this.iterator < this.itemCheckouts.Count)
			{
				if (!this.itemCheckouts[this.iterator].IsNull())
				{
					this.itemCheckouts[this.iterator].UpdatePurchaseText(text, leftPurchaseButtonText, rightPurchaseButtonText, leftButtonOn, rightButtonOn);
				}
				this.iterator++;
			}
		}

		public void PurchaseItem()
		{
			PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
			{
				ItemId = this.itemToBuy.itemName,
				Price = this.itemToBuy.cost,
				VirtualCurrency = this.currencyName,
				CatalogVersion = this.catalog
			}, delegate(PurchaseItemResult result)
			{
				if (result.Items.Count > 0)
				{
					foreach (ItemInstance itemInstance in result.Items)
					{
						CosmeticsController.CosmeticItem itemFromDict = this.GetItemFromDict(this.itemToBuy.itemName);
						if (itemFromDict.itemCategory == CosmeticsController.CosmeticCategory.Set)
						{
							this.UnlockItem(itemInstance.ItemId, false);
							foreach (string text in itemFromDict.bundledItems)
							{
								this.UnlockItem(text, false);
							}
						}
						else
						{
							this.UnlockItem(itemInstance.ItemId, false);
						}
					}
					this.UpdateMyCosmetics();
					if (NetworkSystem.Instance.InRoom)
					{
						base.StartCoroutine(this.CheckIfMyCosmeticsUpdated(this.itemToBuy.itemName));
					}
					this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Success;
					this.currencyBalance -= this.itemToBuy.cost;
					this.UpdateShoppingCart();
					this.ProcessPurchaseItemState(null, this.isLastHandTouchedLeft);
					return;
				}
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Failure;
				this.ProcessPurchaseItemState(null, false);
			}, delegate(PlayFabError error)
			{
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Failure;
				this.ProcessPurchaseItemState(null, false);
			}, null, null);
		}

		private void UnlockItem(string itemIdToUnlock, bool relock = false)
		{
			int num = this.allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => itemIdToUnlock == x.itemName);
			if (num > -1)
			{
				this.ModifyUnlockList(this.unlockedCosmetics, num, relock);
				if (relock)
				{
					this.concatStringCosmeticsAllowed.Replace(this.allCosmetics[num].itemName, string.Empty);
				}
				else
				{
					this.concatStringCosmeticsAllowed += this.allCosmetics[num].itemName;
				}
				switch (this.allCosmetics[num].itemCategory)
				{
				case CosmeticsController.CosmeticCategory.Hat:
					this.ModifyUnlockList(this.unlockedHats, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Badge:
					this.ModifyUnlockList(this.unlockedBadges, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Face:
					this.ModifyUnlockList(this.unlockedFaces, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Paw:
					if (!this.allCosmetics[num].isThrowable)
					{
						this.ModifyUnlockList(this.unlockedPaws, num, relock);
						return;
					}
					this.ModifyUnlockList(this.unlockedThrowables, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Chest:
					this.ModifyUnlockList(this.unlockedChests, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Fur:
					this.ModifyUnlockList(this.unlockedFurs, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Shirt:
					this.ModifyUnlockList(this.unlockedShirts, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Back:
					this.ModifyUnlockList(this.unlockedBacks, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Arms:
					this.ModifyUnlockList(this.unlockedArms, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Pants:
					this.ModifyUnlockList(this.unlockedPants, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.TagEffect:
					this.ModifyUnlockList(this.unlockedTagFX, num, relock);
					return;
				case CosmeticsController.CosmeticCategory.Count:
					break;
				case CosmeticsController.CosmeticCategory.Set:
					foreach (string text in this.allCosmetics[num].bundledItems)
					{
						this.UnlockItem(text, false);
					}
					break;
				default:
					return;
				}
			}
		}

		private void ModifyUnlockList(List<CosmeticsController.CosmeticItem> list, int index, bool relock)
		{
			if (!relock && !list.Contains(this.allCosmetics[index]))
			{
				list.Add(this.allCosmetics[index]);
				return;
			}
			if (relock && list.Contains(this.allCosmetics[index]))
			{
				list.Remove(this.allCosmetics[index]);
			}
		}

		private IEnumerator CheckIfMyCosmeticsUpdated(string itemToBuyID)
		{
			Debug.Log("Cosmetic updated check!");
			yield return new WaitForSeconds(1f);
			this.foundCosmetic = false;
			this.attempts = 0;
			while (!this.foundCosmetic && this.attempts < 10 && NetworkSystem.Instance.InRoom)
			{
				this.playerIDList.Clear();
				if (this.UseNewCosmeticsPath())
				{
					this.playerIDList.Add("Inventory");
					PlayFabClientAPI.GetSharedGroupData(new global::PlayFab.ClientModels.GetSharedGroupDataRequest
					{
						Keys = this.playerIDList,
						SharedGroupId = NetworkSystem.Instance.LocalPlayer.UserId + "Inventory"
					}, delegate(GetSharedGroupDataResult result)
					{
						this.attempts++;
						foreach (KeyValuePair<string, global::PlayFab.ClientModels.SharedGroupDataRecord> keyValuePair in result.Data)
						{
							if (keyValuePair.Value.Value.Contains(itemToBuyID))
							{
								PhotonNetwork.RaiseEvent(199, null, new RaiseEventOptions
								{
									Receivers = ReceiverGroup.Others
								}, SendOptions.SendReliable);
								this.foundCosmetic = true;
							}
						}
						if (this.foundCosmetic)
						{
							this.UpdateWornCosmetics(true);
						}
					}, delegate(PlayFabError error)
					{
						this.attempts++;
						this.ReauthOrBan(error);
					}, null, null);
					yield return new WaitForSeconds(1f);
				}
				else
				{
					this.playerIDList.Add(PhotonNetwork.LocalPlayer.ActorNumber.ToString());
					PlayFabClientAPI.GetSharedGroupData(new global::PlayFab.ClientModels.GetSharedGroupDataRequest
					{
						Keys = this.playerIDList,
						SharedGroupId = NetworkSystem.Instance.RoomName + Regex.Replace(NetworkSystem.Instance.CurrentRegion, "[^a-zA-Z0-9]", "").ToUpper()
					}, delegate(GetSharedGroupDataResult result)
					{
						this.attempts++;
						foreach (KeyValuePair<string, global::PlayFab.ClientModels.SharedGroupDataRecord> keyValuePair2 in result.Data)
						{
							if (keyValuePair2.Value.Value.Contains(itemToBuyID))
							{
								NetworkSystemRaiseEvent.RaiseEvent(199, null, NetworkSystemRaiseEvent.neoOthers, true);
								this.foundCosmetic = true;
							}
							else
							{
								Debug.Log("didnt find it, updating attempts and trying again in a bit. current attempt is " + this.attempts.ToString());
							}
						}
						if (this.foundCosmetic)
						{
							this.UpdateWornCosmetics(true);
						}
					}, delegate(PlayFabError error)
					{
						this.attempts++;
						if (error.Error == PlayFabErrorCode.NotAuthenticated)
						{
							PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
						}
						else if (error.Error == PlayFabErrorCode.AccountBanned)
						{
							GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
						}
						Debug.Log("Got error retrieving user data, on attempt " + this.attempts.ToString());
						Debug.Log(error.GenerateErrorReport());
					}, null, null);
					yield return new WaitForSeconds(1f);
				}
			}
			Debug.Log("done!");
			yield break;
		}

		public void UpdateWardrobeModelsAndButtons()
		{
			if (!CosmeticsV2Spawner_Dirty.allPartsInstantiated)
			{
				return;
			}
			foreach (WardrobeInstance wardrobeInstance in this.wardrobes)
			{
				wardrobeInstance.wardrobeItemButtons[0].currentCosmeticItem = ((this.cosmeticsPages[this.wardrobeType] * 3 < this.itemLists[this.wardrobeType].Count) ? this.itemLists[this.wardrobeType][this.cosmeticsPages[this.wardrobeType] * 3] : this.nullItem);
				wardrobeInstance.wardrobeItemButtons[1].currentCosmeticItem = ((this.cosmeticsPages[this.wardrobeType] * 3 + 1 < this.itemLists[this.wardrobeType].Count) ? this.itemLists[this.wardrobeType][this.cosmeticsPages[this.wardrobeType] * 3 + 1] : this.nullItem);
				wardrobeInstance.wardrobeItemButtons[2].currentCosmeticItem = ((this.cosmeticsPages[this.wardrobeType] * 3 + 2 < this.itemLists[this.wardrobeType].Count) ? this.itemLists[this.wardrobeType][this.cosmeticsPages[this.wardrobeType] * 3 + 2] : this.nullItem);
				this.iterator = 0;
				while (this.iterator < wardrobeInstance.wardrobeItemButtons.Length)
				{
					CosmeticsController.CosmeticItem currentCosmeticItem = wardrobeInstance.wardrobeItemButtons[this.iterator].currentCosmeticItem;
					wardrobeInstance.wardrobeItemButtons[this.iterator].isOn = !currentCosmeticItem.isNullItem && this.AnyMatch(this.currentWornSet, currentCosmeticItem);
					wardrobeInstance.wardrobeItemButtons[this.iterator].UpdateColor();
					this.iterator++;
				}
				wardrobeInstance.wardrobeItemButtons[0].controlledModel.SetCosmeticActive(wardrobeInstance.wardrobeItemButtons[0].currentCosmeticItem.displayName, false);
				wardrobeInstance.wardrobeItemButtons[1].controlledModel.SetCosmeticActive(wardrobeInstance.wardrobeItemButtons[1].currentCosmeticItem.displayName, false);
				wardrobeInstance.wardrobeItemButtons[2].controlledModel.SetCosmeticActive(wardrobeInstance.wardrobeItemButtons[2].currentCosmeticItem.displayName, false);
				wardrobeInstance.selfDoll.SetCosmeticActiveArray(this.currentWornSet.ToDisplayNameArray(), this.currentWornSet.ToOnRightSideArray());
			}
		}

		public int GetCategorySize(CosmeticsController.CosmeticCategory category)
		{
			int indexForCategory = this.GetIndexForCategory(category);
			if (indexForCategory != -1)
			{
				return this.itemLists[indexForCategory].Count;
			}
			return 0;
		}

		public CosmeticsController.CosmeticItem GetCosmetic(int category, int cosmeticIndex)
		{
			if (cosmeticIndex >= this.itemLists[category].Count || cosmeticIndex < 0)
			{
				return this.nullItem;
			}
			return this.itemLists[category][cosmeticIndex];
		}

		public CosmeticsController.CosmeticItem GetCosmetic(CosmeticsController.CosmeticCategory category, int cosmeticIndex)
		{
			return this.GetCosmetic(this.GetIndexForCategory(category), cosmeticIndex);
		}

		private int GetIndexForCategory(CosmeticsController.CosmeticCategory category)
		{
			switch (category)
			{
			case CosmeticsController.CosmeticCategory.Hat:
				return 0;
			case CosmeticsController.CosmeticCategory.Badge:
				return 2;
			case CosmeticsController.CosmeticCategory.Face:
				return 1;
			case CosmeticsController.CosmeticCategory.Paw:
				return 3;
			case CosmeticsController.CosmeticCategory.Chest:
				return 9;
			case CosmeticsController.CosmeticCategory.Fur:
				return 4;
			case CosmeticsController.CosmeticCategory.Shirt:
				return 5;
			case CosmeticsController.CosmeticCategory.Back:
				return 8;
			case CosmeticsController.CosmeticCategory.Arms:
				return 7;
			case CosmeticsController.CosmeticCategory.Pants:
				return 6;
			case CosmeticsController.CosmeticCategory.TagEffect:
				return 10;
			default:
				return 0;
			}
		}

		public bool IsCosmeticEquipped(CosmeticsController.CosmeticItem cosmetic)
		{
			return this.AnyMatch(this.currentWornSet, cosmetic);
		}

		public bool IsCosmeticEquipped(CosmeticsController.CosmeticItem cosmetic, bool tempSet)
		{
			if (!tempSet)
			{
				return this.IsCosmeticEquipped(cosmetic);
			}
			return this.IsTemporaryCosmeticEquipped(cosmetic);
		}

		public bool IsTemporaryCosmeticEquipped(CosmeticsController.CosmeticItem cosmetic)
		{
			return this.AnyMatch(this.tempUnlockedSet, cosmetic);
		}

		public CosmeticsController.CosmeticItem GetSlotItem(CosmeticsController.CosmeticSlots slot, bool checkOpposite = true, bool tempSet = false)
		{
			int num = (int)slot;
			if (checkOpposite)
			{
				num = (int)CosmeticsController.CosmeticSet.OppositeSlot(slot);
			}
			if (!tempSet)
			{
				return this.currentWornSet.items[num];
			}
			return this.tempUnlockedSet.items[num];
		}

		public string[] GetCurrentlyWornCosmetics(bool tempSet = false)
		{
			if (!tempSet)
			{
				return this.currentWornSet.ToDisplayNameArray();
			}
			return this.tempUnlockedSet.ToDisplayNameArray();
		}

		public bool[] GetCurrentRightEquippedSided(bool tempSet = false)
		{
			if (!tempSet)
			{
				return this.currentWornSet.ToOnRightSideArray();
			}
			return this.tempUnlockedSet.ToOnRightSideArray();
		}

		public void UpdateShoppingCart()
		{
			this.iterator = 0;
			while (this.iterator < this.itemCheckouts.Count)
			{
				if (!this.itemCheckouts[this.iterator].IsNull())
				{
					this.itemCheckouts[this.iterator].UpdateFromCart(this.currentCart, this.itemToBuy);
				}
				this.iterator++;
			}
			this.iterator = 0;
			while (this.iterator < this.fittingRooms.Count)
			{
				if (!this.fittingRooms[this.iterator].IsNull())
				{
					this.fittingRooms[this.iterator].UpdateFromCart(this.currentCart, this.tryOnSet);
				}
				this.iterator++;
			}
			if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
			{
				this.UpdateWardrobeModelsAndButtons();
			}
		}

		public void UpdateWornCosmetics(bool sync = false)
		{
			VRRig localRig = VRRig.LocalRig;
			this.activeMergedSet.MergeInSets(this.currentWornSet, this.tempUnlockedSet, (string id) => PlayerCosmeticsSystem.IsTemporaryCosmeticAllowed(localRig, id));
			GorillaTagger.Instance.offlineVRRig.LocalUpdateCosmeticsWithTryon(this.activeMergedSet, this.tryOnSet);
			if (sync && GorillaTagger.Instance.myVRRig != null)
			{
				if (this.isHidingCosmeticsFromRemotePlayers)
				{
					GorillaTagger.Instance.myVRRig.SendRPC("RPC_HideAllCosmetics", RpcTarget.All, Array.Empty<object>());
					return;
				}
				int[] array = this.activeMergedSet.ToPackedIDArray();
				int[] array2 = this.tryOnSet.ToPackedIDArray();
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others, new object[] { array, array2 });
			}
		}

		public CosmeticsController.CosmeticItem GetItemFromDict(string itemID)
		{
			if (!this.allCosmeticsDict.TryGetValue(itemID, out this.cosmeticItemVar))
			{
				return this.nullItem;
			}
			return this.cosmeticItemVar;
		}

		public string GetItemNameFromDisplayName(string displayName)
		{
			if (!this.allCosmeticsItemIDsfromDisplayNamesDict.TryGetValue(displayName, out this.returnString))
			{
				return "null";
			}
			return this.returnString;
		}

		public CosmeticSO GetCosmeticSOFromDisplayName(string displayName)
		{
			string itemNameFromDisplayName = this.GetItemNameFromDisplayName(displayName);
			if (itemNameFromDisplayName.Equals("null"))
			{
				return null;
			}
			AllCosmeticsArraySO allCosmeticsArraySO = this.v2_allCosmeticsInfoAssetRef.Asset as AllCosmeticsArraySO;
			if (allCosmeticsArraySO == null)
			{
				GTDev.LogWarning<string>("null AllCosmeticsArraySO", null);
				return null;
			}
			CosmeticSO cosmeticSO = allCosmeticsArraySO.SearchForCosmeticSO(itemNameFromDisplayName);
			if (cosmeticSO != null)
			{
				return cosmeticSO;
			}
			GTDev.Log<string>("Could not find cosmetic info for " + itemNameFromDisplayName, null);
			return null;
		}

		public CosmeticAnchorAntiIntersectOffsets GetClipOffsetsFromDisplayName(string displayName)
		{
			string itemNameFromDisplayName = this.GetItemNameFromDisplayName(displayName);
			if (itemNameFromDisplayName.Equals("null"))
			{
				return this.defaultClipOffsets;
			}
			AllCosmeticsArraySO allCosmeticsArraySO = this.v2_allCosmeticsInfoAssetRef.Asset as AllCosmeticsArraySO;
			if (allCosmeticsArraySO == null)
			{
				GTDev.LogWarning<string>("null AllCosmeticsArraySO", null);
				return this.defaultClipOffsets;
			}
			CosmeticSO cosmeticSO = allCosmeticsArraySO.SearchForCosmeticSO(itemNameFromDisplayName);
			if (cosmeticSO != null)
			{
				return cosmeticSO.info.anchorAntiIntersectOffsets;
			}
			GTDev.Log<string>("Could not find cosmetic info for " + itemNameFromDisplayName, null);
			return this.defaultClipOffsets;
		}

		public bool AnyMatch(CosmeticsController.CosmeticSet set, CosmeticsController.CosmeticItem item)
		{
			if (item.itemCategory != CosmeticsController.CosmeticCategory.Set)
			{
				return set.IsActive(item.displayName);
			}
			if (item.bundledItems.Length == 1)
			{
				return this.AnyMatch(set, this.GetItemFromDict(item.bundledItems[0]));
			}
			if (item.bundledItems.Length == 2)
			{
				return this.AnyMatch(set, this.GetItemFromDict(item.bundledItems[0])) || this.AnyMatch(set, this.GetItemFromDict(item.bundledItems[1]));
			}
			return item.bundledItems.Length >= 3 && (this.AnyMatch(set, this.GetItemFromDict(item.bundledItems[0])) || this.AnyMatch(set, this.GetItemFromDict(item.bundledItems[1])) || this.AnyMatch(set, this.GetItemFromDict(item.bundledItems[2])));
		}

		public void Initialize()
		{
			if (!base.gameObject.activeSelf || this.v2_isCosmeticPlayFabCatalogDataLoaded || this.v2_isGetCosmeticsPlayCatalogDataWaitingForCallback)
			{
				return;
			}
			if (this.v2_allCosmeticsInfoAssetRef_isLoaded)
			{
				this.GetCosmeticsPlayFabCatalogData();
				return;
			}
			this.v2_isGetCosmeticsPlayCatalogDataWaitingForCallback = true;
			this.V2_allCosmeticsInfoAssetRef_OnPostLoad = (Action)Delegate.Combine(this.V2_allCosmeticsInfoAssetRef_OnPostLoad, new Action(this.GetCosmeticsPlayFabCatalogData));
		}

		public void GetLastDailyLogin()
		{
			PlayFabClientAPI.GetUserReadOnlyData(new global::PlayFab.ClientModels.GetUserDataRequest(), delegate(GetUserDataResult result)
			{
				if (result.Data.TryGetValue("DailyLogin", out this.userDataRecord))
				{
					this.lastDailyLogin = this.userDataRecord.Value;
					return;
				}
				this.lastDailyLogin = "NONE";
				base.StartCoroutine(this.GetMyDaily());
			}, delegate(PlayFabError error)
			{
				Debug.Log("Got error getting read-only user data:");
				Debug.Log(error.GenerateErrorReport());
				this.lastDailyLogin = "FAILED";
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					return;
				}
				if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					Application.Quit();
					NetworkSystem.Instance.ReturnToSinglePlayer();
					Object.DestroyImmediate(PhotonNetworkController.Instance);
					Object.DestroyImmediate(GTPlayer.Instance);
					GameObject[] array = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
					for (int i = 0; i < array.Length; i++)
					{
						Object.Destroy(array[i]);
					}
				}
			}, null, null);
		}

		private IEnumerator CheckCanGetDaily()
		{
			while (!KIDManager.InitialisationComplete)
			{
				yield return new WaitForSeconds(1f);
			}
			for (;;)
			{
				if (GorillaComputer.instance != null && GorillaComputer.instance.startupMillis != 0L)
				{
					this.currentTime = new DateTime((GorillaComputer.instance.startupMillis + (long)(Time.realtimeSinceStartup * 1000f)) * 10000L);
					this.secondsUntilTomorrow = (int)(this.currentTime.AddDays(1.0).Date - this.currentTime).TotalSeconds;
					if (this.lastDailyLogin == null || this.lastDailyLogin == "")
					{
						this.GetLastDailyLogin();
					}
					else if (this.currentTime.ToString("o").Substring(0, 10) == this.lastDailyLogin)
					{
						this.checkedDaily = true;
						this.gotMyDaily = true;
					}
					else if (this.currentTime.ToString("o").Substring(0, 10) != this.lastDailyLogin)
					{
						this.checkedDaily = true;
						this.gotMyDaily = false;
						base.StartCoroutine(this.GetMyDaily());
					}
					else if (this.lastDailyLogin == "FAILED")
					{
						this.GetLastDailyLogin();
					}
					this.secondsToWaitToCheckDaily = (this.checkedDaily ? 60f : 10f);
					this.UpdateCurrencyBoards();
					yield return new WaitForSeconds(this.secondsToWaitToCheckDaily);
				}
				else
				{
					yield return new WaitForSeconds(1f);
				}
			}
			yield break;
		}

		private IEnumerator GetMyDaily()
		{
			yield return new WaitForSeconds(10f);
			GorillaServer.Instance.TryDistributeCurrency(delegate(ExecuteFunctionResult result)
			{
				this.GetCurrencyBalance();
				this.GetLastDailyLogin();
			}, delegate(PlayFabError error)
			{
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					return;
				}
				if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					Application.Quit();
					NetworkSystem.Instance.ReturnToSinglePlayer();
					Object.DestroyImmediate(PhotonNetworkController.Instance);
					Object.DestroyImmediate(GTPlayer.Instance);
					GameObject[] array = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
					for (int i = 0; i < array.Length; i++)
					{
						Object.Destroy(array[i]);
					}
				}
			});
			yield break;
		}

		public void GetCosmeticsPlayFabCatalogData()
		{
			this.v2_isGetCosmeticsPlayCatalogDataWaitingForCallback = false;
			if (!this.v2_allCosmeticsInfoAssetRef_isLoaded)
			{
				throw new Exception("Method `GetCosmeticsPlayFabCatalogData` was called before `v2_allCosmeticsInfoAssetRef` was loaded. Listen to callback `V2_allCosmeticsInfoAssetRef_OnPostLoad` or check `v2_allCosmeticsInfoAssetRef_isLoaded` before trying to get PlayFab catalog data.");
			}
			PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), delegate(GetUserInventoryResult result)
			{
				PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest
				{
					CatalogVersion = this.catalog
				}, delegate(GetCatalogItemsResult result2)
				{
					this.unlockedCosmetics.Clear();
					this.unlockedHats.Clear();
					this.unlockedBadges.Clear();
					this.unlockedFaces.Clear();
					this.unlockedPaws.Clear();
					this.unlockedFurs.Clear();
					this.unlockedShirts.Clear();
					this.unlockedPants.Clear();
					this.unlockedArms.Clear();
					this.unlockedBacks.Clear();
					this.unlockedChests.Clear();
					this.unlockedTagFX.Clear();
					this.unlockedThrowables.Clear();
					this.catalogItems = result2.Catalog;
					using (List<CatalogItem>.Enumerator enumerator = this.catalogItems.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							CatalogItem catalogItem = enumerator.Current;
							if (!BuilderSetManager.IsItemIDBuilderItem(catalogItem.ItemId))
							{
								this.searchIndex = this.allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => catalogItem.ItemId == x.itemName);
								if (this.searchIndex > -1)
								{
									this.tempStringArray = null;
									this.hasPrice = false;
									if (catalogItem.Bundle != null)
									{
										this.tempStringArray = catalogItem.Bundle.BundledItems.ToArray();
									}
									uint num;
									if (catalogItem.VirtualCurrencyPrices.TryGetValue(this.currencyName, out num))
									{
										this.hasPrice = true;
									}
									CosmeticsController.CosmeticItem cosmeticItem = this.allCosmetics[this.searchIndex];
									cosmeticItem.itemName = catalogItem.ItemId;
									cosmeticItem.displayName = catalogItem.DisplayName;
									cosmeticItem.cost = (int)num;
									cosmeticItem.bundledItems = this.tempStringArray;
									cosmeticItem.canTryOn = this.hasPrice;
									if (cosmeticItem.itemCategory == CosmeticsController.CosmeticCategory.Paw)
									{
										CosmeticInfoV2 cosmeticInfoV = this.v2_allCosmetics[this.searchIndex];
										cosmeticItem.isThrowable = cosmeticInfoV.isThrowable && !cosmeticInfoV.hasWardrobeParts;
									}
									if (cosmeticItem.displayName == null)
									{
										string text = "null";
										if (this.allCosmetics[this.searchIndex].itemPicture)
										{
											text = this.allCosmetics[this.searchIndex].itemPicture.name;
										}
										string debugCosmeticSOName = this.v2_allCosmetics[this.searchIndex].debugCosmeticSOName;
										Debug.LogError(string.Concat(new string[]
										{
											string.Format("Cosmetic encountered with a null displayName at index {0}! ", this.searchIndex),
											"Setting displayName to id: \"",
											this.allCosmetics[this.searchIndex].itemName,
											"\". iconName=\"",
											text,
											"\".cosmeticSOName=\"",
											debugCosmeticSOName,
											"\". "
										}));
										cosmeticItem.displayName = cosmeticItem.itemName;
									}
									this.V2_ConformCosmeticItemV1DisplayName(ref cosmeticItem);
									this._allCosmetics[this.searchIndex] = cosmeticItem;
									this._allCosmeticsDict[cosmeticItem.itemName] = cosmeticItem;
									this._allCosmeticsItemIDsfromDisplayNamesDict[cosmeticItem.displayName] = cosmeticItem.itemName;
									this._allCosmeticsItemIDsfromDisplayNamesDict[cosmeticItem.overrideDisplayName] = cosmeticItem.itemName;
								}
							}
						}
					}
					for (int i = this._allCosmetics.Count - 1; i > -1; i--)
					{
						this.tempItem = this._allCosmetics[i];
						if (this.tempItem.itemCategory == CosmeticsController.CosmeticCategory.Set && this.tempItem.canTryOn)
						{
							string[] array = this.tempItem.bundledItems;
							for (int j = 0; j < array.Length; j++)
							{
								string setItemName2 = array[j];
								this.searchIndex = this._allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => setItemName2 == x.itemName);
								if (this.searchIndex > -1)
								{
									this.tempItem = this._allCosmetics[this.searchIndex];
									this.tempItem.canTryOn = true;
									this._allCosmetics[this.searchIndex] = this.tempItem;
									this._allCosmeticsDict[this._allCosmetics[this.searchIndex].itemName] = this.tempItem;
									this._allCosmeticsItemIDsfromDisplayNamesDict[this._allCosmetics[this.searchIndex].displayName] = this.tempItem.itemName;
								}
							}
						}
					}
					foreach (KeyValuePair<string, StoreBundle> keyValuePair in BundleManager.instance.storeBundlesById)
					{
						string text2;
						StoreBundle storeBundle;
						keyValuePair.Deconstruct(out text2, out storeBundle);
						string text3 = text2;
						StoreBundle bundleData = storeBundle;
						int num2 = this._allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => bundleData.playfabBundleID == x.itemName);
						if (num2 > 0 && this._allCosmetics[num2].bundledItems != null)
						{
							string[] array = this._allCosmetics[num2].bundledItems;
							for (int j = 0; j < array.Length; j++)
							{
								string setItemName = array[j];
								this.searchIndex = this._allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => setItemName == x.itemName);
								if (this.searchIndex > -1)
								{
									this.tempItem = this._allCosmetics[this.searchIndex];
									this.tempItem.canTryOn = true;
									this._allCosmetics[this.searchIndex] = this.tempItem;
									this._allCosmeticsDict[this._allCosmetics[this.searchIndex].itemName] = this.tempItem;
									this._allCosmeticsItemIDsfromDisplayNamesDict[this._allCosmetics[this.searchIndex].displayName] = this.tempItem.itemName;
								}
							}
						}
						if (!bundleData.HasPrice)
						{
							num2 = this.catalogItems.FindIndex((CatalogItem ci) => ci.Bundle != null && ci.ItemId == bundleData.playfabBundleID);
							if (num2 > 0)
							{
								uint num3;
								if (this.catalogItems[num2].VirtualCurrencyPrices.TryGetValue("RM", out num3))
								{
									BundleManager.instance.storeBundlesById[text3].TryUpdatePrice(num3);
								}
								else
								{
									BundleManager.instance.storeBundlesById[text3].TryUpdatePrice(null);
								}
							}
						}
					}
					this.searchIndex = this._allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => "Slingshot" == x.itemName);
					if (this.searchIndex < 0)
					{
						throw new MissingReferenceException("CosmeticsController: Cannot find default slingshot! it is required for players that do not have another slingshot equipped and are playing Paintbrawl.");
					}
					this._allCosmeticsDict["Slingshot"] = this._allCosmetics[this.searchIndex];
					this._allCosmeticsItemIDsfromDisplayNamesDict[this._allCosmetics[this.searchIndex].displayName] = this._allCosmetics[this.searchIndex].itemName;
					this.allCosmeticsDict_isInitialized = true;
					this.allCosmeticsItemIDsfromDisplayNamesDict_isInitialized = true;
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					using (List<ItemInstance>.Enumerator enumerator3 = result.Inventory.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							ItemInstance item = enumerator3.Current;
							if (!BuilderSetManager.IsItemIDBuilderItem(item.ItemId))
							{
								if (item.ItemId == this.m_earlyAccessSupporterPackCosmeticSO.info.playFabID)
								{
									foreach (CosmeticSO cosmeticSO in this.m_earlyAccessSupporterPackCosmeticSO.info.setCosmetics)
									{
										CosmeticsController.CosmeticItem cosmeticItem2;
										if (this.allCosmeticsDict.TryGetValue(cosmeticSO.info.playFabID, out cosmeticItem2))
										{
											this.unlockedCosmetics.Add(cosmeticItem2);
										}
									}
								}
								BundleManager.instance.MarkBundleOwnedByPlayFabID(item.ItemId);
								if (!dictionary.ContainsKey(item.ItemId))
								{
									this.searchIndex = this.allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => item.ItemId == x.itemName);
									if (this.searchIndex > -1)
									{
										dictionary[item.ItemId] = item.ItemId;
										this.unlockedCosmetics.Add(this.allCosmetics[this.searchIndex]);
									}
								}
							}
						}
					}
					foreach (CosmeticsController.CosmeticItem cosmeticItem3 in this.unlockedCosmetics)
					{
						if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Hat && !this.unlockedHats.Contains(cosmeticItem3))
						{
							this.unlockedHats.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Face && !this.unlockedFaces.Contains(cosmeticItem3))
						{
							this.unlockedFaces.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Badge && !this.unlockedBadges.Contains(cosmeticItem3))
						{
							this.unlockedBadges.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Paw)
						{
							if (!cosmeticItem3.isThrowable && !this.unlockedPaws.Contains(cosmeticItem3))
							{
								this.unlockedPaws.Add(cosmeticItem3);
							}
							else if (cosmeticItem3.isThrowable && !this.unlockedThrowables.Contains(cosmeticItem3))
							{
								this.unlockedThrowables.Add(cosmeticItem3);
							}
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Fur && !this.unlockedFurs.Contains(cosmeticItem3))
						{
							this.unlockedFurs.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Shirt && !this.unlockedShirts.Contains(cosmeticItem3))
						{
							this.unlockedShirts.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Arms && !this.unlockedArms.Contains(cosmeticItem3))
						{
							this.unlockedArms.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Back && !this.unlockedBacks.Contains(cosmeticItem3))
						{
							this.unlockedBacks.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Chest && !this.unlockedChests.Contains(cosmeticItem3))
						{
							this.unlockedChests.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.Pants && !this.unlockedPants.Contains(cosmeticItem3))
						{
							this.unlockedPants.Add(cosmeticItem3);
						}
						else if (cosmeticItem3.itemCategory == CosmeticsController.CosmeticCategory.TagEffect && !this.unlockedTagFX.Contains(cosmeticItem3))
						{
							this.unlockedTagFX.Add(cosmeticItem3);
						}
						this.concatStringCosmeticsAllowed += cosmeticItem3.itemName;
					}
					BuilderSetManager.instance.OnGotInventoryItems(result, result2);
					this.currencyBalance = result.VirtualCurrency[this.currencyName];
					int num4;
					this.playedInBeta = result.VirtualCurrency.TryGetValue("TC", out num4) && num4 > 0;
					Action onGetCurrency = this.OnGetCurrency;
					if (onGetCurrency != null)
					{
						onGetCurrency();
					}
					BundleManager.instance.CheckIfBundlesOwned();
					StoreUpdater.instance.Initialize();
					this.currentWornSet.LoadFromPlayerPreferences(this);
					this.LoadSavedOutfits();
					if (!ATM_Manager.instance.alreadyBegan)
					{
						ATM_Manager.instance.SwitchToStage(ATM_Manager.ATMStages.Begin);
						ATM_Manager.instance.alreadyBegan = true;
					}
					this.ProcessPurchaseItemState(null, false);
					this.UpdateShoppingCart();
					this.UpdateCurrencyBoards();
					if (this.UseNewCosmeticsPath())
					{
						this.ConfirmIndividualCosmeticsSharedGroup(result);
					}
					Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
					if (onCosmeticsUpdated != null)
					{
						onCosmeticsUpdated();
					}
					this.v2_isCosmeticPlayFabCatalogDataLoaded = true;
					Action v2_OnGetCosmeticsPlayFabCatalogData_PostSuccess = this.V2_OnGetCosmeticsPlayFabCatalogData_PostSuccess;
					if (v2_OnGetCosmeticsPlayFabCatalogData_PostSuccess != null)
					{
						v2_OnGetCosmeticsPlayFabCatalogData_PostSuccess();
					}
					if (!CosmeticsV2Spawner_Dirty.startedAllPartsInstantiated && !CosmeticsV2Spawner_Dirty.allPartsInstantiated)
					{
						CosmeticsV2Spawner_Dirty.StartInstantiatingPrefabs();
					}
				}, delegate(PlayFabError error)
				{
					if (error.Error == PlayFabErrorCode.NotAuthenticated)
					{
						PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					}
					else if (error.Error == PlayFabErrorCode.AccountBanned)
					{
						Application.Quit();
						NetworkSystem.Instance.ReturnToSinglePlayer();
						Object.DestroyImmediate(PhotonNetworkController.Instance);
						Object.DestroyImmediate(GTPlayer.Instance);
						GameObject[] array2 = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
						for (int k = 0; k < array2.Length; k++)
						{
							Object.Destroy(array2[k]);
						}
					}
					if (!this.tryTwice)
					{
						this.tryTwice = true;
						this.GetCosmeticsPlayFabCatalogData();
					}
				}, null, null);
			}, delegate(PlayFabError error)
			{
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
				}
				else if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					Application.Quit();
					NetworkSystem.Instance.ReturnToSinglePlayer();
					Object.DestroyImmediate(PhotonNetworkController.Instance);
					Object.DestroyImmediate(GTPlayer.Instance);
					GameObject[] array3 = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
					for (int l = 0; l < array3.Length; l++)
					{
						Object.Destroy(array3[l]);
					}
				}
				if (!this.tryTwice)
				{
					this.tryTwice = true;
					this.GetCosmeticsPlayFabCatalogData();
				}
			}, null, null);
		}

		public void SteamPurchase()
		{
			if (string.IsNullOrEmpty(this.itemToPurchase))
			{
				Debug.Log("Unable to start steam purchase process. itemToPurchase is not set.");
				return;
			}
			Debug.Log(string.Format("attempting to purchase item through steam. Is this a bundle purchase: {0}", this.buyingBundle));
			PlayFabClientAPI.StartPurchase(this.GetStartPurchaseRequest(), new Action<StartPurchaseResult>(this.ProcessStartPurchaseResponse), new Action<PlayFabError>(this.ProcessSteamPurchaseError), null, null);
		}

		private StartPurchaseRequest GetStartPurchaseRequest()
		{
			return new StartPurchaseRequest
			{
				CatalogVersion = this.catalog,
				Items = new List<ItemPurchaseRequest>
				{
					new ItemPurchaseRequest
					{
						ItemId = this.itemToPurchase,
						Quantity = 1U,
						Annotation = "Purchased via in-game store"
					}
				}
			};
		}

		private void ProcessStartPurchaseResponse(StartPurchaseResult result)
		{
			Debug.Log("successfully started purchase. attempted to pay for purchase through steam");
			this.currentPurchaseID = result.OrderId;
			PlayFabClientAPI.PayForPurchase(CosmeticsController.GetPayForPurchaseRequest(this.currentPurchaseID), new Action<PayForPurchaseResult>(CosmeticsController.ProcessPayForPurchaseResult), new Action<PlayFabError>(this.ProcessSteamPurchaseError), null, null);
		}

		private static PayForPurchaseRequest GetPayForPurchaseRequest(string orderId)
		{
			return new PayForPurchaseRequest
			{
				OrderId = orderId,
				ProviderName = "Steam",
				Currency = "RM"
			};
		}

		private static void ProcessPayForPurchaseResult(PayForPurchaseResult result)
		{
			Debug.Log("succeeded on sending request for paying with steam! waiting for response");
		}

		private void ProcessSteamCallback(MicroTxnAuthorizationResponse_t callBackResponse)
		{
			Debug.Log("Steam has called back that the user has finished the payment interaction");
			if (callBackResponse.m_bAuthorized == 0)
			{
				Debug.Log("Steam has indicated that the payment was not authorised.");
			}
			if (this.buyingBundle)
			{
				PlayFabClientAPI.ConfirmPurchase(this.GetConfirmBundlePurchaseRequest(), delegate(ConfirmPurchaseResult _)
				{
					this.ProcessConfirmPurchaseSuccess();
				}, new Action<PlayFabError>(this.ProcessConfirmPurchaseError), null, null);
				return;
			}
			PlayFabClientAPI.ConfirmPurchase(this.GetConfirmATMPurchaseRequest(), delegate(ConfirmPurchaseResult _)
			{
				this.ProcessConfirmPurchaseSuccess();
			}, new Action<PlayFabError>(this.ProcessConfirmPurchaseError), null, null);
		}

		private ConfirmPurchaseRequest GetConfirmBundlePurchaseRequest()
		{
			return new ConfirmPurchaseRequest
			{
				OrderId = this.currentPurchaseID
			};
		}

		private ConfirmPurchaseRequest GetConfirmATMPurchaseRequest()
		{
			return new ConfirmPurchaseRequest
			{
				OrderId = this.currentPurchaseID,
				CustomTags = new Dictionary<string, string>
				{
					{
						"NexusCreatorId",
						ATM_Manager.instance.ValidatedCreatorCode
					},
					{
						"PlayerName",
						GorillaComputer.instance.savedName
					},
					{
						"Location",
						GorillaTagger.Instance.offlineVRRig.zoneEntity.currentZone.ToString()
					}
				}
			};
		}

		private void ProcessConfirmPurchaseSuccess()
		{
			if (this.buyingBundle)
			{
				this.buyingBundle = false;
				if (PhotonNetwork.InRoom)
				{
					object[] array = new object[0];
					NetworkSystemRaiseEvent.RaiseEvent(9, array, NetworkSystemRaiseEvent.newWeb, true);
				}
				base.StartCoroutine(this.CheckIfMyCosmeticsUpdated(this.BundlePlayfabItemName));
			}
			else
			{
				ATM_Manager.instance.SwitchToStage(ATM_Manager.ATMStages.Success);
			}
			this.GetCurrencyBalance();
			this.UpdateCurrencyBoards();
			this.GetCosmeticsPlayFabCatalogData();
			GorillaTagger.Instance.offlineVRRig.GetCosmeticsPlayFabCatalogData();
		}

		private void ProcessConfirmPurchaseError(PlayFabError error)
		{
			this.ProcessSteamPurchaseError(error);
			ATM_Manager.instance.SwitchToStage(ATM_Manager.ATMStages.Failure);
			this.UpdateCurrencyBoards();
		}

		private void ProcessSteamPurchaseError(PlayFabError error)
		{
			PlayFabErrorCode error2 = error.Error;
			if (error2 <= PlayFabErrorCode.PurchaseInitializationFailure)
			{
				if (error2 <= PlayFabErrorCode.FailedByPaymentProvider)
				{
					if (error2 == PlayFabErrorCode.AccountBanned)
					{
						PhotonNetwork.Disconnect();
						Object.DestroyImmediate(PhotonNetworkController.Instance);
						Object.DestroyImmediate(GTPlayer.Instance);
						GameObject[] array = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
						for (int i = 0; i < array.Length; i++)
						{
							Object.Destroy(array[i]);
						}
						Application.Quit();
						goto IL_01A2;
					}
					if (error2 != PlayFabErrorCode.FailedByPaymentProvider)
					{
						goto IL_0192;
					}
					Debug.Log(string.Format("Attempted to pay for order, but has been Failed by Steam with error: {0}", error));
					goto IL_01A2;
				}
				else
				{
					if (error2 == PlayFabErrorCode.InsufficientFunds)
					{
						Debug.Log(string.Format("Attempting to do purchase through steam, steam has returned insufficient funds: {0}", error));
						goto IL_01A2;
					}
					if (error2 == PlayFabErrorCode.InvalidPaymentProvider)
					{
						Debug.Log(string.Format("Attempted to connect to steam as payment provider, but received error: {0}", error));
						goto IL_01A2;
					}
					if (error2 != PlayFabErrorCode.PurchaseInitializationFailure)
					{
						goto IL_0192;
					}
				}
			}
			else if (error2 <= PlayFabErrorCode.InvalidPurchaseTransactionStatus)
			{
				if (error2 == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					goto IL_01A2;
				}
				if (error2 == PlayFabErrorCode.PurchaseDoesNotExist)
				{
					Debug.Log(string.Format("Attempting to confirm purchase for order {0} but received error: {1}", this.currentPurchaseID, error));
					goto IL_01A2;
				}
				if (error2 != PlayFabErrorCode.InvalidPurchaseTransactionStatus)
				{
					goto IL_0192;
				}
			}
			else
			{
				if (error2 == PlayFabErrorCode.InternalServerError)
				{
					Debug.Log(string.Format("PlayFab threw an internal server error: {0}", error));
					goto IL_01A2;
				}
				if (error2 == PlayFabErrorCode.StoreNotFound)
				{
					Debug.Log(string.Format("Attempted to load {0} from {1} but received an error: {2}", this.itemToPurchase, this.catalog, error));
					goto IL_01A2;
				}
				if (error2 != PlayFabErrorCode.DuplicatePurchaseTransactionId)
				{
					goto IL_0192;
				}
			}
			Debug.Log(string.Format("Attempted to pay for order {0}, however received an error: {1}", this.currentPurchaseID, error));
			goto IL_01A2;
			IL_0192:
			Debug.Log(string.Format("Steam purchase flow returned error: {0}", error));
			IL_01A2:
			ATM_Manager.instance.SwitchToStage(ATM_Manager.ATMStages.Failure);
		}

		public void UpdateCurrencyBoards()
		{
			this.FormattedPurchaseText(this.finalLine, null, null, false, false);
			this.iterator = 0;
			while (this.iterator < this.currencyBoards.Count)
			{
				if (this.currencyBoards[this.iterator].IsNotNull())
				{
					this.currencyBoards[this.iterator].UpdateCurrencyBoard(this.checkedDaily, this.gotMyDaily, this.currencyBalance, this.secondsUntilTomorrow);
				}
				this.iterator++;
			}
		}

		public void AddCurrencyBoard(CurrencyBoard newCurrencyBoard)
		{
			if (this.currencyBoards.Contains(newCurrencyBoard))
			{
				return;
			}
			this.currencyBoards.Add(newCurrencyBoard);
			newCurrencyBoard.UpdateCurrencyBoard(this.checkedDaily, this.gotMyDaily, this.currencyBalance, this.secondsUntilTomorrow);
		}

		public void RemoveCurrencyBoard(CurrencyBoard currencyBoardToRemove)
		{
			this.currencyBoards.Remove(currencyBoardToRemove);
		}

		public void GetCurrencyBalance()
		{
			PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), delegate(GetUserInventoryResult result)
			{
				this.currencyBalance = result.VirtualCurrency[this.currencyName];
				this.UpdateCurrencyBoards();
				Action onGetCurrency = this.OnGetCurrency;
				if (onGetCurrency == null)
				{
					return;
				}
				onGetCurrency();
			}, delegate(PlayFabError error)
			{
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					return;
				}
				if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					Application.Quit();
					NetworkSystem.Instance.ReturnToSinglePlayer();
					Object.DestroyImmediate(PhotonNetworkController.Instance);
					Object.DestroyImmediate(GTPlayer.Instance);
					GameObject[] array = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
					for (int i = 0; i < array.Length; i++)
					{
						Object.Destroy(array[i]);
					}
				}
			}, null, null);
		}

		public string GetItemDisplayName(CosmeticsController.CosmeticItem item)
		{
			if (item.overrideDisplayName != null && item.overrideDisplayName != "")
			{
				return item.overrideDisplayName;
			}
			return item.displayName;
		}

		public void UpdateMyCosmetics()
		{
			if (NetworkSystem.Instance.InRoom)
			{
				if (GorillaServer.Instance != null && GorillaServer.Instance.NewCosmeticsPathShouldSetSharedGroupData())
				{
					this.UpdateMyCosmeticsForRoom(true);
				}
				if (GorillaServer.Instance != null && GorillaServer.Instance.NewCosmeticsPathShouldSetRoomData())
				{
					this.UpdateMyCosmeticsForRoom(false);
					return;
				}
			}
			else if (GorillaServer.Instance != null && GorillaServer.Instance.NewCosmeticsPathShouldSetSharedGroupData())
			{
				this.UpdateMyCosmeticsNotInRoom();
			}
		}

		private void UpdateMyCosmeticsNotInRoom()
		{
			if (GorillaServer.Instance != null)
			{
				GorillaServer.Instance.UpdateUserCosmetics();
			}
		}

		private void UpdateMyCosmeticsForRoom(bool shouldSetSharedGroupData)
		{
			byte b = 9;
			if (shouldSetSharedGroupData)
			{
				b = 10;
			}
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			WebFlags webFlags = new WebFlags(1);
			raiseEventOptions.Flags = webFlags;
			object[] array = new object[0];
			PhotonNetwork.RaiseEvent(b, array, raiseEventOptions, SendOptions.SendReliable);
		}

		private void AlreadyOwnAllBundleButtons()
		{
			EarlyAccessButton[] array = this.earlyAccessButtons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AlreadyOwn();
			}
		}

		private bool UseNewCosmeticsPath()
		{
			return GorillaServer.Instance != null && GorillaServer.Instance.NewCosmeticsPathShouldReadSharedGroupData();
		}

		public void CheckCosmeticsSharedGroup()
		{
			this.updateCosmeticsRetries++;
			if (this.updateCosmeticsRetries < this.maxUpdateCosmeticsRetries)
			{
				base.StartCoroutine(this.WaitForNextCosmeticsAttempt());
			}
		}

		private IEnumerator WaitForNextCosmeticsAttempt()
		{
			int num = (int)Mathf.Pow(3f, (float)(this.updateCosmeticsRetries + 1));
			yield return new WaitForSeconds((float)num);
			this.ConfirmIndividualCosmeticsSharedGroup(this.latestInventory);
			yield break;
		}

		private void ConfirmIndividualCosmeticsSharedGroup(GetUserInventoryResult inventory)
		{
			Debug.Log("confirming individual cosmetics with shared group");
			this.latestInventory = inventory;
			if (PhotonNetwork.LocalPlayer.UserId == null)
			{
				base.StartCoroutine(this.WaitForNextCosmeticsAttempt());
				return;
			}
			PlayFabClientAPI.GetSharedGroupData(new global::PlayFab.ClientModels.GetSharedGroupDataRequest
			{
				Keys = this.inventoryStringList,
				SharedGroupId = PhotonNetwork.LocalPlayer.UserId + "Inventory"
			}, delegate(GetSharedGroupDataResult result)
			{
				bool flag = true;
				foreach (KeyValuePair<string, global::PlayFab.ClientModels.SharedGroupDataRecord> keyValuePair in result.Data)
				{
					if (keyValuePair.Key != "Inventory")
					{
						break;
					}
					foreach (ItemInstance itemInstance in inventory.Inventory)
					{
						if (itemInstance.CatalogVersion == CosmeticsController.instance.catalog && !keyValuePair.Value.Value.Contains(itemInstance.ItemId))
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag || result.Data.Count == 0)
				{
					this.UpdateMyCosmetics();
					return;
				}
				this.updateCosmeticsRetries = 0;
			}, delegate(PlayFabError error)
			{
				this.ReauthOrBan(error);
				this.CheckCosmeticsSharedGroup();
			}, null, null);
		}

		public void ReauthOrBan(PlayFabError error)
		{
			if (error.Error == PlayFabErrorCode.NotAuthenticated)
			{
				PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
				return;
			}
			if (error.Error == PlayFabErrorCode.AccountBanned)
			{
				Application.Quit();
				PhotonNetwork.Disconnect();
				Object.DestroyImmediate(PhotonNetworkController.Instance);
				Object.DestroyImmediate(GTPlayer.Instance);
				GameObject[] array = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
				for (int i = 0; i < array.Length; i++)
				{
					Object.Destroy(array[i]);
				}
			}
		}

		public void ProcessExternalUnlock(string itemID, bool autoEquip, bool isLeftHand)
		{
			this.UnlockItem(itemID, false);
			VRRig offlineVRRig = GorillaTagger.Instance.offlineVRRig;
			offlineVRRig.concatStringOfCosmeticsAllowed += itemID;
			this.UpdateMyCosmetics();
			if (autoEquip)
			{
				CosmeticsController.CosmeticItem itemFromDict = this.GetItemFromDict(itemID);
				GorillaTelemetry.PostShopEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.external_item_claim, itemFromDict);
				List<CosmeticsController.CosmeticSlots> list = CollectionPool<List<CosmeticsController.CosmeticSlots>, CosmeticsController.CosmeticSlots>.Get();
				if (list.Capacity < 16)
				{
					list.Capacity = 16;
				}
				this.ApplyCosmeticItemToSet(this.currentWornSet, itemFromDict, isLeftHand, true, list);
				foreach (CosmeticsController.CosmeticSlots cosmeticSlots in list)
				{
					this.tryOnSet.items[(int)cosmeticSlots] = this.nullItem;
				}
				CollectionPool<List<CosmeticsController.CosmeticSlots>, CosmeticsController.CosmeticSlots>.Release(list);
				this.UpdateShoppingCart();
				this.UpdateWornCosmetics(true);
				Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
				if (onCosmeticsUpdated == null)
				{
					return;
				}
				onCosmeticsUpdated();
			}
		}

		public void AddTempUnlockToWardrobe(string cosmeticID)
		{
			int num = this.allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => cosmeticID == x.itemName);
			if (num < 0)
			{
				return;
			}
			switch (this.allCosmetics[num].itemCategory)
			{
			case CosmeticsController.CosmeticCategory.Hat:
				this.ModifyUnlockList(this.unlockedHats, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Badge:
				this.ModifyUnlockList(this.unlockedBadges, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Face:
				this.ModifyUnlockList(this.unlockedFaces, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Paw:
				if (!this.allCosmetics[num].isThrowable)
				{
					this.ModifyUnlockList(this.unlockedPaws, num, false);
				}
				else
				{
					this.ModifyUnlockList(this.unlockedThrowables, num, false);
				}
				break;
			case CosmeticsController.CosmeticCategory.Chest:
				this.ModifyUnlockList(this.unlockedChests, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Fur:
				this.ModifyUnlockList(this.unlockedFurs, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Shirt:
				this.ModifyUnlockList(this.unlockedShirts, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Back:
				this.ModifyUnlockList(this.unlockedBacks, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Arms:
				this.ModifyUnlockList(this.unlockedArms, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Pants:
				this.ModifyUnlockList(this.unlockedPants, num, false);
				break;
			case CosmeticsController.CosmeticCategory.TagEffect:
				this.ModifyUnlockList(this.unlockedTagFX, num, false);
				break;
			case CosmeticsController.CosmeticCategory.Set:
				foreach (string text in this.allCosmetics[num].bundledItems)
				{
					this.AddTempUnlockToWardrobe(text);
				}
				break;
			}
			Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
			if (onCosmeticsUpdated == null)
			{
				return;
			}
			onCosmeticsUpdated();
		}

		public void RemoveTempUnlockFromWardrobe(string cosmeticID)
		{
			int num = this.allCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => cosmeticID == x.itemName);
			if (num < 0)
			{
				return;
			}
			switch (this.allCosmetics[num].itemCategory)
			{
			case CosmeticsController.CosmeticCategory.Hat:
				this.ModifyUnlockList(this.unlockedHats, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Badge:
				this.ModifyUnlockList(this.unlockedBadges, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Face:
				this.ModifyUnlockList(this.unlockedFaces, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Paw:
				if (!this.allCosmetics[num].isThrowable)
				{
					this.ModifyUnlockList(this.unlockedPaws, num, true);
				}
				else
				{
					this.ModifyUnlockList(this.unlockedThrowables, num, true);
				}
				break;
			case CosmeticsController.CosmeticCategory.Chest:
				this.ModifyUnlockList(this.unlockedChests, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Fur:
				this.ModifyUnlockList(this.unlockedFurs, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Shirt:
				this.ModifyUnlockList(this.unlockedShirts, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Back:
				this.ModifyUnlockList(this.unlockedBacks, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Arms:
				this.ModifyUnlockList(this.unlockedArms, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Pants:
				this.ModifyUnlockList(this.unlockedPants, num, true);
				break;
			case CosmeticsController.CosmeticCategory.TagEffect:
				this.ModifyUnlockList(this.unlockedTagFX, num, true);
				break;
			case CosmeticsController.CosmeticCategory.Set:
				foreach (string text in this.allCosmetics[num].bundledItems)
				{
					this.RemoveTempUnlockFromWardrobe(text);
				}
				break;
			}
			Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
			if (onCosmeticsUpdated == null)
			{
				return;
			}
			onCosmeticsUpdated();
		}

		public bool BuildValidationCheck()
		{
			if (this.m_earlyAccessSupporterPackCosmeticSO == null)
			{
				Debug.LogError("m_earlyAccessSupporterPackCosmeticSO is empty, everything will break!");
				return false;
			}
			return true;
		}

		public void SetHideCosmeticsFromRemotePlayers(bool hideCosmetics)
		{
			if (hideCosmetics == this.isHidingCosmeticsFromRemotePlayers)
			{
				return;
			}
			this.isHidingCosmeticsFromRemotePlayers = hideCosmetics;
			GorillaTagger.Instance.offlineVRRig.reliableState.SetIsDirty();
			this.UpdateWornCosmetics(true);
		}

		public bool ValidatePackedItems(int[] packed)
		{
			if (packed.Length == 0)
			{
				return true;
			}
			int num = 0;
			int num2 = packed[0];
			for (int i = 0; i < 16; i++)
			{
				if ((num2 & (1 << i)) != 0)
				{
					num++;
				}
			}
			return packed.Length == num + 1;
		}

		public static int SelectedOutfit
		{
			get
			{
				return CosmeticsController.selectedOutfit;
			}
		}

		public static bool CanScrollOutfits()
		{
			return CosmeticsController.loadedSavedOutfits && !CosmeticsController.saveOutfitInProgress;
		}

		public void PressWardrobeScrollOutfit(bool forward)
		{
			if (!CosmeticsController.CanScrollOutfits())
			{
				return;
			}
			this.savedOutfits[CosmeticsController.selectedOutfit].CopyItems(this.currentWornSet);
			this.SaveOutfitsToMothership();
			if (forward)
			{
				CosmeticsController.selectedOutfit = (CosmeticsController.selectedOutfit + 1) % this.outfitSystemConfig.maxOutfits;
			}
			else
			{
				CosmeticsController.selectedOutfit--;
				if (CosmeticsController.selectedOutfit < 0)
				{
					CosmeticsController.selectedOutfit = this.outfitSystemConfig.maxOutfits - 1;
				}
			}
			PlayerPrefs.SetInt(this.outfitSystemConfig.selectedOutfitPref, CosmeticsController.selectedOutfit);
			PlayerPrefs.Save();
			CosmeticsController.CosmeticSet cosmeticSet = this.savedOutfits[CosmeticsController.selectedOutfit];
			for (int i = 0; i < 16; i++)
			{
				this.currentWornSet.items[i] = cosmeticSet.items[i];
				if (!cosmeticSet.items[i].isNullItem)
				{
					this.tryOnSet.items[i] = this.nullItem;
				}
			}
			this.SaveCurrentItemPreferences();
			this.UpdateShoppingCart();
			this.UpdateWornCosmetics(true);
			this.UpdateWardrobeModelsAndButtons();
			Action onCosmeticsUpdated = this.OnCosmeticsUpdated;
			if (onCosmeticsUpdated == null)
			{
				return;
			}
			onCosmeticsUpdated();
		}

		private void LoadSavedOutfits()
		{
			if (CosmeticsController.loadedSavedOutfits || CosmeticsController.loadOutfitsInProgress)
			{
				return;
			}
			CosmeticsController.loadOutfitsInProgress = true;
			this.savedOutfits = new CosmeticsController.CosmeticSet[this.outfitSystemConfig.maxOutfits];
			if (!MothershipClientApiUnity.GetUserDataValue(this.outfitSystemConfig.mothershipKey, new Action<MothershipUserData>(this.GetSavedOutfitsSuccess), new Action<MothershipError, int>(this.GetSavedOutfitsFail), ""))
			{
				GTDev.LogError<string>("CosmeticsController LoadSavedOutfits GetUserDataValue failed", null);
				this.ClearOutfits();
				CosmeticsController.loadOutfitsInProgress = false;
				CosmeticsController.loadedSavedOutfits = true;
				Action onOutfitsUpdated = this.OnOutfitsUpdated;
				if (onOutfitsUpdated == null)
				{
					return;
				}
				onOutfitsUpdated();
			}
		}

		private void GetSavedOutfitsSuccess(MothershipUserData response)
		{
			if (response != null && response.value != null && response.value.Length > 0)
			{
				try
				{
					byte[] array = Convert.FromBase64String(response.value);
					this.outfitStringMothership = Encoding.UTF8.GetString(array);
					this.StringToOutfits(this.outfitStringMothership);
					goto IL_006E;
				}
				catch (Exception ex)
				{
					GTDev.LogError<string>("CosmeticsController GetSavedOutfitsSuccess error decoding " + ex.Message, null);
					this.ClearOutfits();
					goto IL_006E;
				}
			}
			this.ClearOutfits();
			IL_006E:
			this.GetSavedOutfitsComplete();
		}

		private void GetSavedOutfitsFail(MothershipError error, int status)
		{
			GTDev.LogError<string>(string.Format("CosmeticsController GetSavedOutfitsFail {0} {1}", status, error.Message), null);
			this.ClearOutfits();
			this.GetSavedOutfitsComplete();
		}

		private void GetSavedOutfitsComplete()
		{
			int num = PlayerPrefs.GetInt(this.outfitSystemConfig.selectedOutfitPref, 0);
			if (num < 0 || num >= this.outfitSystemConfig.maxOutfits)
			{
				num = 0;
			}
			else
			{
				CosmeticsController.CosmeticSet cosmeticSet = new CosmeticsController.CosmeticSet();
				cosmeticSet.LoadFromPlayerPreferences(this);
				if (cosmeticSet.HasAnyItems())
				{
					this.savedOutfits[num].CopyItems(cosmeticSet);
				}
			}
			CosmeticsController.selectedOutfit = num;
			this.currentWornSet.CopyItems(this.savedOutfits[CosmeticsController.selectedOutfit]);
			CosmeticsController.loadedSavedOutfits = true;
			CosmeticsController.loadOutfitsInProgress = false;
			Action onOutfitsUpdated = this.OnOutfitsUpdated;
			if (onOutfitsUpdated == null)
			{
				return;
			}
			onOutfitsUpdated();
		}

		private void SaveOutfitsToMothership()
		{
			if (!CosmeticsController.loadedSavedOutfits || CosmeticsController.saveOutfitInProgress)
			{
				return;
			}
			string mothershipKey = this.outfitSystemConfig.mothershipKey;
			this.outfitStringPendingSave = this.OutfitsToString();
			if (this.outfitStringPendingSave.Equals(this.outfitStringMothership))
			{
				return;
			}
			CosmeticsController.saveOutfitInProgress = true;
			if (!MothershipClientApiUnity.SetUserDataValue(mothershipKey, this.outfitStringPendingSave, new Action<SetUserDataResponse>(this.SaveOutfitsToMothershipSuccess), new Action<MothershipError, int>(this.SaveOutfitsToMothershipFail), ""))
			{
				GTDev.LogError<string>("CosmeticsController SaveOutfitToMothership SetUserDataValue failed", null);
				CosmeticsController.saveOutfitInProgress = false;
			}
		}

		private void SaveOutfitsToMothershipSuccess(SetUserDataResponse response)
		{
			this.outfitStringMothership = this.outfitStringPendingSave;
			CosmeticsController.saveOutfitInProgress = false;
			Action onOutfitsUpdated = this.OnOutfitsUpdated;
			if (onOutfitsUpdated != null)
			{
				onOutfitsUpdated();
			}
			response.Dispose();
		}

		private void SaveOutfitsToMothershipFail(MothershipError error, int status)
		{
			GTDev.LogError<string>(string.Format("CosmeticsController SaveOutfitsToMothershipFail {0} ", status) + error.Message, null);
			CosmeticsController.saveOutfitInProgress = false;
		}

		private string OutfitsToString()
		{
			if (!CosmeticsController.loadedSavedOutfits)
			{
				return string.Empty;
			}
			this.sb.Clear();
			for (int i = 0; i < this.savedOutfits.Length; i++)
			{
				CosmeticsController.CosmeticSet cosmeticSet = this.savedOutfits[i];
				for (int j = 0; j < cosmeticSet.items.Length; j++)
				{
					CosmeticsController.CosmeticItem cosmeticItem = cosmeticSet.items[j];
					if (cosmeticItem.isNullItem)
					{
						this.sb.Append("null");
					}
					else
					{
						this.sb.Append(string.IsNullOrEmpty(cosmeticItem.displayName) ? "null" : cosmeticItem.displayName);
					}
					if (j < cosmeticSet.items.Length - 1)
					{
						this.sb.Append(this.outfitSystemConfig.itemSeparator);
					}
				}
				if (i < this.savedOutfits.Length - 1)
				{
					this.sb.Append(this.outfitSystemConfig.outfitSeparator);
				}
			}
			return this.sb.ToString();
		}

		private void ClearOutfits()
		{
			for (int i = 0; i < this.savedOutfits.Length; i++)
			{
				this.savedOutfits[i] = new CosmeticsController.CosmeticSet();
				this.savedOutfits[i].ClearSet(this.nullItem);
			}
		}

		private void StringToOutfits(string response)
		{
			if (response.IsNullOrEmpty())
			{
				this.ClearOutfits();
				return;
			}
			try
			{
				string[] array = response.Split(this.outfitSystemConfig.outfitSeparator, StringSplitOptions.None);
				for (int i = 0; i < this.outfitSystemConfig.maxOutfits; i++)
				{
					this.savedOutfits[i] = new CosmeticsController.CosmeticSet();
					if (i >= array.Length)
					{
						this.savedOutfits[i].ClearSet(this.nullItem);
					}
					else
					{
						string text = array[i];
						if (text.IsNullOrEmpty())
						{
							this.savedOutfits[i].ClearSet(this.nullItem);
						}
						else
						{
							this.savedOutfits[i].ParseSetFromString(this, text);
						}
					}
				}
			}
			catch (Exception ex)
			{
				GTDev.LogError<string>("CosmeticsController StringToOutfit Error parsing " + ex.Message, null);
				this.ClearOutfits();
			}
		}

		[FormerlySerializedAs("v2AllCosmeticsInfoAssetRef")]
		[FormerlySerializedAs("newSysAllCosmeticsAssetRef")]
		[SerializeField]
		public GTAssetRef<AllCosmeticsArraySO> v2_allCosmeticsInfoAssetRef;

		private readonly Dictionary<string, CosmeticInfoV2> _allCosmeticsDictV2 = new Dictionary<string, CosmeticInfoV2>();

		public Action V2_allCosmeticsInfoAssetRef_OnPostLoad;

		public const int maximumTransferrableItems = 5;

		[OnEnterPlay_SetNull]
		public static volatile CosmeticsController instance;

		public Action V2_OnGetCosmeticsPlayFabCatalogData_PostSuccess;

		public Action OnGetCurrency;

		[FormerlySerializedAs("allCosmetics")]
		[SerializeField]
		private List<CosmeticsController.CosmeticItem> _allCosmetics;

		public Dictionary<string, CosmeticsController.CosmeticItem> _allCosmeticsDict = new Dictionary<string, CosmeticsController.CosmeticItem>(2048);

		public Dictionary<string, string> _allCosmeticsItemIDsfromDisplayNamesDict = new Dictionary<string, string>(2048);

		public CosmeticsController.CosmeticItem nullItem;

		public string catalog;

		private string[] tempStringArray;

		private CosmeticsController.CosmeticItem tempItem;

		private VRRigAnchorOverrides anchorOverrides;

		public List<CatalogItem> catalogItems;

		public bool tryTwice;

		public CustomMapCosmeticsData customMapCosmeticsData;

		[NonSerialized]
		public CosmeticsController.CosmeticSet tryOnSet = new CosmeticsController.CosmeticSet();

		public int numFittingRoomButtons = 12;

		public List<FittingRoom> fittingRooms = new List<FittingRoom>();

		public CosmeticStand[] cosmeticStands;

		public List<CosmeticsController.CosmeticItem> currentCart = new List<CosmeticsController.CosmeticItem>();

		public CosmeticsController.PurchaseItemStages currentPurchaseItemStage;

		public List<ItemCheckout> itemCheckouts = new List<ItemCheckout>();

		public CosmeticsController.CosmeticItem itemToBuy;

		private List<string> playerIDList = new List<string>();

		private List<string> inventoryStringList = new List<string>();

		private bool foundCosmetic;

		private int attempts;

		private string finalLine;

		private string leftCheckoutPurchaseButtonString;

		private string rightCheckoutPurchaseButtonString;

		private bool leftCheckoutPurchaseButtonOn;

		private bool rightCheckoutPurchaseButtonOn;

		private bool isLastHandTouchedLeft;

		private CosmeticsController.CosmeticSet cachedSet = new CosmeticsController.CosmeticSet();

		public readonly List<WardrobeInstance> wardrobes = new List<WardrobeInstance>();

		public List<CosmeticsController.CosmeticItem> unlockedCosmetics = new List<CosmeticsController.CosmeticItem>(2048);

		public List<CosmeticsController.CosmeticItem> unlockedHats = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedFaces = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedBadges = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedPaws = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedChests = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedFurs = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedShirts = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedPants = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedBacks = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedArms = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedTagFX = new List<CosmeticsController.CosmeticItem>(512);

		public List<CosmeticsController.CosmeticItem> unlockedThrowables = new List<CosmeticsController.CosmeticItem>(512);

		public int[] cosmeticsPages = new int[11];

		private List<CosmeticsController.CosmeticItem>[] itemLists = new List<CosmeticsController.CosmeticItem>[11];

		private int wardrobeType;

		[NonSerialized]
		public CosmeticsController.CosmeticSet currentWornSet = new CosmeticsController.CosmeticSet();

		[NonSerialized]
		public CosmeticsController.CosmeticSet tempUnlockedSet = new CosmeticsController.CosmeticSet();

		[NonSerialized]
		public CosmeticsController.CosmeticSet activeMergedSet = new CosmeticsController.CosmeticSet();

		public string concatStringCosmeticsAllowed = "";

		public Action OnCosmeticsUpdated;

		public int currencyBalance;

		public string currencyName;

		public List<CurrencyBoard> currencyBoards;

		public string itemToPurchase;

		public bool buyingBundle;

		public bool confirmedDidntPlayInBeta;

		public bool playedInBeta;

		public bool gotMyDaily;

		public bool checkedDaily;

		public string currentPurchaseID;

		public bool hasPrice;

		private int searchIndex;

		private int iterator;

		private CosmeticsController.CosmeticItem cosmeticItemVar;

		[SerializeField]
		private CosmeticSO m_earlyAccessSupporterPackCosmeticSO;

		public EarlyAccessButton[] earlyAccessButtons;

		private BundleList bundleList = new BundleList();

		public string BundleSkuName = "2024_i_lava_you_pack";

		public string BundlePlayfabItemName = "LSABG.";

		public int BundleShinyRocks = 10000;

		public DateTime currentTime;

		public string lastDailyLogin;

		public UserDataRecord userDataRecord;

		public int secondsUntilTomorrow;

		public float secondsToWaitToCheckDaily = 10f;

		private int updateCosmeticsRetries;

		private int maxUpdateCosmeticsRetries;

		private GetUserInventoryResult latestInventory;

		private string returnString;

		private bool checkoutCartButtonPressedWithLeft;

		private Callback<MicroTxnAuthorizationResponse_t> _steamMicroTransactionAuthorizationResponse;

		private static readonly List<CosmeticsController.CosmeticSlots> _g_default_outAppliedSlotsList_for_applyCosmeticItemToSet = new List<CosmeticsController.CosmeticSlots>(16);

		[SerializeField]
		private CosmeticOutfitSystemConfig outfitSystemConfig;

		private CosmeticsController.CosmeticSet[] savedOutfits;

		private string outfitStringMothership = string.Empty;

		private string outfitStringPendingSave = string.Empty;

		private static bool saveOutfitInProgress = false;

		private static bool loadOutfitsInProgress = false;

		private static bool loadedSavedOutfits = false;

		private static int selectedOutfit = 0;

		public Action OnOutfitsUpdated;

		private StringBuilder sb = new StringBuilder(256);

		public enum PurchaseItemStages
		{
			Start,
			CheckoutButtonPressed,
			ItemSelected,
			ItemOwned,
			FinalPurchaseAcknowledgement,
			Buying,
			Success,
			Failure
		}

		public enum CosmeticCategory
		{
			None,
			Hat,
			Badge,
			Face,
			Paw,
			Chest,
			Fur,
			Shirt,
			Back,
			Arms,
			Pants,
			TagEffect,
			Count,
			Set
		}

		public enum CosmeticSlots
		{
			Hat,
			Badge,
			Face,
			ArmLeft,
			ArmRight,
			BackLeft,
			BackRight,
			HandLeft,
			HandRight,
			Chest,
			Fur,
			Shirt,
			Pants,
			Back,
			Arms,
			TagEffect,
			Count
		}

		[Serializable]
		public class CosmeticSet
		{
			public event CosmeticsController.CosmeticSet.OnSetActivatedHandler onSetActivatedEvent;

			protected void OnSetActivated(CosmeticsController.CosmeticSet prevSet, CosmeticsController.CosmeticSet currentSet, NetPlayer netPlayer)
			{
				if (this.onSetActivatedEvent != null)
				{
					this.onSetActivatedEvent(prevSet, currentSet, netPlayer);
				}
			}

			public static CosmeticsController.CosmeticSet EmptySet
			{
				get
				{
					if (CosmeticsController.CosmeticSet._emptySet == null)
					{
						string[] array = new string[16];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = "NOTHING";
						}
						CosmeticsController.CosmeticSet._emptySet = new CosmeticsController.CosmeticSet(array, CosmeticsController.instance);
					}
					return CosmeticsController.CosmeticSet._emptySet;
				}
			}

			public CosmeticSet()
			{
				this.items = new CosmeticsController.CosmeticItem[16];
			}

			public CosmeticSet(string[] itemNames, CosmeticsController controller)
			{
				this.items = new CosmeticsController.CosmeticItem[16];
				for (int i = 0; i < itemNames.Length; i++)
				{
					string text = itemNames[i];
					string itemNameFromDisplayName = controller.GetItemNameFromDisplayName(text);
					this.items[i] = controller.GetItemFromDict(itemNameFromDisplayName);
				}
			}

			public CosmeticSet(int[] itemNamesPacked, CosmeticsController controller)
			{
				this.items = new CosmeticsController.CosmeticItem[16];
				int num = ((itemNamesPacked.Length != 0) ? itemNamesPacked[0] : 0);
				int num2 = 1;
				for (int i = 0; i < this.items.Length; i++)
				{
					if ((num & (1 << i)) != 0)
					{
						int num3 = itemNamesPacked[num2];
						CosmeticsController.CosmeticSet.nameScratchSpace[0] = (char)(65 + num3 % 26);
						CosmeticsController.CosmeticSet.nameScratchSpace[1] = (char)(65 + num3 / 26 % 26);
						CosmeticsController.CosmeticSet.nameScratchSpace[2] = (char)(65 + num3 / 676 % 26);
						CosmeticsController.CosmeticSet.nameScratchSpace[3] = (char)(65 + num3 / 17576 % 26);
						CosmeticsController.CosmeticSet.nameScratchSpace[4] = (char)(65 + num3 / 456976 % 26);
						CosmeticsController.CosmeticSet.nameScratchSpace[5] = '.';
						this.items[i] = controller.GetItemFromDict(new string(CosmeticsController.CosmeticSet.nameScratchSpace));
						num2++;
					}
					else
					{
						this.items[i] = controller.GetItemFromDict("null");
					}
				}
			}

			public void CopyItems(CosmeticsController.CosmeticSet other)
			{
				for (int i = 0; i < this.items.Length; i++)
				{
					this.items[i] = other.items[i];
				}
			}

			public void MergeSets(CosmeticsController.CosmeticSet tryOn, CosmeticsController.CosmeticSet current)
			{
				for (int i = 0; i < 16; i++)
				{
					if (tryOn == null)
					{
						this.items[i] = current.items[i];
					}
					else
					{
						this.items[i] = (tryOn.items[i].isNullItem ? current.items[i] : tryOn.items[i]);
					}
				}
			}

			public void MergeInSets(CosmeticsController.CosmeticSet playerPref, CosmeticsController.CosmeticSet tempOverrideSet, Predicate<string> predicate)
			{
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					bool flag = predicate(tempOverrideSet.items[i].itemName);
					this.items[i] = (flag ? tempOverrideSet.items[i] : playerPref.items[i]);
				}
			}

			public void ClearSet(CosmeticsController.CosmeticItem nullItem)
			{
				for (int i = 0; i < 16; i++)
				{
					this.items[i] = nullItem;
				}
			}

			public bool IsActive(string name)
			{
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					if (this.items[i].displayName == name)
					{
						return true;
					}
				}
				return false;
			}

			public bool HasItemOfCategory(CosmeticsController.CosmeticCategory category)
			{
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					if (!this.items[i].isNullItem && this.items[i].itemCategory == category)
					{
						return true;
					}
				}
				return false;
			}

			public bool HasItem(string name)
			{
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					if (!this.items[i].isNullItem && this.items[i].displayName == name)
					{
						return true;
					}
				}
				return false;
			}

			public bool HasAnyItems()
			{
				if (this.items == null || this.items.Length < 1)
				{
					return false;
				}
				for (int i = 0; i < this.items.Length; i++)
				{
					if (!this.items[i].isNullItem)
					{
						return true;
					}
				}
				return false;
			}

			public static bool IsSlotLeftHanded(CosmeticsController.CosmeticSlots slot)
			{
				return slot == CosmeticsController.CosmeticSlots.ArmLeft || slot == CosmeticsController.CosmeticSlots.BackLeft || slot == CosmeticsController.CosmeticSlots.HandLeft;
			}

			public static bool IsSlotRightHanded(CosmeticsController.CosmeticSlots slot)
			{
				return slot == CosmeticsController.CosmeticSlots.ArmRight || slot == CosmeticsController.CosmeticSlots.BackRight || slot == CosmeticsController.CosmeticSlots.HandRight;
			}

			public static bool IsHoldable(CosmeticsController.CosmeticItem item)
			{
				return item.isHoldable;
			}

			public static CosmeticsController.CosmeticSlots OppositeSlot(CosmeticsController.CosmeticSlots slot)
			{
				switch (slot)
				{
				case CosmeticsController.CosmeticSlots.Hat:
					return CosmeticsController.CosmeticSlots.Hat;
				case CosmeticsController.CosmeticSlots.Badge:
					return CosmeticsController.CosmeticSlots.Badge;
				case CosmeticsController.CosmeticSlots.Face:
					return CosmeticsController.CosmeticSlots.Face;
				case CosmeticsController.CosmeticSlots.ArmLeft:
					return CosmeticsController.CosmeticSlots.ArmRight;
				case CosmeticsController.CosmeticSlots.ArmRight:
					return CosmeticsController.CosmeticSlots.ArmLeft;
				case CosmeticsController.CosmeticSlots.BackLeft:
					return CosmeticsController.CosmeticSlots.BackRight;
				case CosmeticsController.CosmeticSlots.BackRight:
					return CosmeticsController.CosmeticSlots.BackLeft;
				case CosmeticsController.CosmeticSlots.HandLeft:
					return CosmeticsController.CosmeticSlots.HandRight;
				case CosmeticsController.CosmeticSlots.HandRight:
					return CosmeticsController.CosmeticSlots.HandLeft;
				case CosmeticsController.CosmeticSlots.Chest:
					return CosmeticsController.CosmeticSlots.Chest;
				case CosmeticsController.CosmeticSlots.Fur:
					return CosmeticsController.CosmeticSlots.Fur;
				case CosmeticsController.CosmeticSlots.Shirt:
					return CosmeticsController.CosmeticSlots.Shirt;
				case CosmeticsController.CosmeticSlots.Pants:
					return CosmeticsController.CosmeticSlots.Pants;
				case CosmeticsController.CosmeticSlots.Back:
					return CosmeticsController.CosmeticSlots.Back;
				case CosmeticsController.CosmeticSlots.Arms:
					return CosmeticsController.CosmeticSlots.Arms;
				case CosmeticsController.CosmeticSlots.TagEffect:
					return CosmeticsController.CosmeticSlots.TagEffect;
				default:
					return CosmeticsController.CosmeticSlots.Count;
				}
			}

			public static string SlotPlayerPreferenceName(CosmeticsController.CosmeticSlots slot)
			{
				return "slot_" + slot.ToString();
			}

			private void ActivateCosmetic(CosmeticsController.CosmeticSet prevSet, VRRig rig, int slotIndex, CosmeticItemRegistry cosmeticsObjectRegistry, BodyDockPositions bDock)
			{
				CosmeticsController.CosmeticItem cosmeticItem = prevSet.items[slotIndex];
				string itemNameFromDisplayName = CosmeticsController.instance.GetItemNameFromDisplayName(cosmeticItem.displayName);
				CosmeticsController.CosmeticItem cosmeticItem2 = this.items[slotIndex];
				string itemNameFromDisplayName2 = CosmeticsController.instance.GetItemNameFromDisplayName(cosmeticItem2.displayName);
				BodyDockPositions.DropPositions dropPositions = CosmeticsController.CosmeticSlotToDropPosition((CosmeticsController.CosmeticSlots)slotIndex);
				if (cosmeticItem2.itemCategory != CosmeticsController.CosmeticCategory.None && !CosmeticsController.CompareCategoryToSavedCosmeticSlots(cosmeticItem2.itemCategory, (CosmeticsController.CosmeticSlots)slotIndex))
				{
					return;
				}
				if (cosmeticItem2.isHoldable && dropPositions == BodyDockPositions.DropPositions.None)
				{
					return;
				}
				if (!(itemNameFromDisplayName == itemNameFromDisplayName2))
				{
					if (!cosmeticItem.isNullItem)
					{
						if (cosmeticItem.isHoldable)
						{
							bDock.TransferrableItemDisableAtPosition(dropPositions);
						}
						CosmeticItemInstance cosmeticItemInstance = cosmeticsObjectRegistry.Cosmetic(cosmeticItem.displayName);
						if (cosmeticItemInstance != null)
						{
							cosmeticItemInstance.DisableItem((CosmeticsController.CosmeticSlots)slotIndex);
						}
					}
					if (!cosmeticItem2.isNullItem)
					{
						if (cosmeticItem2.isHoldable)
						{
							bDock.TransferrableItemEnableAtPosition(cosmeticItem2.displayName, dropPositions);
						}
						CosmeticItemInstance cosmeticItemInstance2 = cosmeticsObjectRegistry.Cosmetic(cosmeticItem2.displayName);
						if (rig.IsItemAllowed(itemNameFromDisplayName2) && cosmeticItemInstance2 != null)
						{
							cosmeticItemInstance2.EnableItem((CosmeticsController.CosmeticSlots)slotIndex, rig);
						}
					}
					return;
				}
				if (cosmeticItem2.isNullItem)
				{
					return;
				}
				CosmeticItemInstance cosmeticItemInstance3 = cosmeticsObjectRegistry.Cosmetic(cosmeticItem2.displayName);
				if (cosmeticItemInstance3 != null)
				{
					if (!rig.IsItemAllowed(itemNameFromDisplayName2))
					{
						cosmeticItemInstance3.DisableItem((CosmeticsController.CosmeticSlots)slotIndex);
						return;
					}
					cosmeticItemInstance3.EnableItem((CosmeticsController.CosmeticSlots)slotIndex, rig);
				}
			}

			public void ActivateCosmetics(CosmeticsController.CosmeticSet prevSet, VRRig rig, BodyDockPositions bDock, CosmeticItemRegistry cosmeticsObjectRegistry)
			{
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					this.ActivateCosmetic(prevSet, rig, i, cosmeticsObjectRegistry, bDock);
				}
				this.OnSetActivated(prevSet, this, rig.creator);
			}

			public void DeactivateAllCosmetcs(BodyDockPositions bDock, CosmeticsController.CosmeticItem nullItem, CosmeticItemRegistry cosmeticObjectRegistry)
			{
				bDock.DisableAllTransferableItems();
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					CosmeticsController.CosmeticItem cosmeticItem = this.items[i];
					if (!cosmeticItem.isNullItem)
					{
						CosmeticsController.CosmeticSlots cosmeticSlots = (CosmeticsController.CosmeticSlots)i;
						CosmeticItemInstance cosmeticItemInstance = cosmeticObjectRegistry.Cosmetic(cosmeticItem.displayName);
						if (cosmeticItemInstance != null)
						{
							cosmeticItemInstance.DisableItem(cosmeticSlots);
						}
						this.items[i] = nullItem;
					}
				}
			}

			public void LoadFromPlayerPreferences(CosmeticsController controller)
			{
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					CosmeticsController.CosmeticSlots cosmeticSlots = (CosmeticsController.CosmeticSlots)i;
					string @string = PlayerPrefs.GetString(CosmeticsController.CosmeticSet.SlotPlayerPreferenceName(cosmeticSlots), "NOTHING");
					if (@string == "null" || @string == "NOTHING")
					{
						this.items[i] = controller.nullItem;
					}
					else
					{
						CosmeticsController.CosmeticItem item = controller.GetItemFromDict(@string);
						if (item.isNullItem)
						{
							Debug.Log("LoadFromPlayerPreferences: Could not find item stored in player prefs: \"" + @string + "\"");
							this.items[i] = controller.nullItem;
						}
						else if (!CosmeticsController.CompareCategoryToSavedCosmeticSlots(item.itemCategory, cosmeticSlots))
						{
							this.items[i] = controller.nullItem;
						}
						else if (controller.unlockedCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => item.itemName == x.itemName) >= 0)
						{
							this.items[i] = item;
						}
						else
						{
							this.items[i] = controller.nullItem;
						}
					}
				}
			}

			public void ParseSetFromString(CosmeticsController controller, string setString)
			{
				if (setString.IsNullOrEmpty())
				{
					this.ClearSet(controller.nullItem);
					GTDev.LogError<string>("CosmeticsController ParseSetFromString: null string", null);
					return;
				}
				int num = 16;
				char c = ',';
				if (controller.outfitSystemConfig != null)
				{
					c = controller.outfitSystemConfig.itemSeparator;
				}
				string[] array = setString.Split(c, num, StringSplitOptions.None);
				if (array == null || array.Length < num)
				{
					this.ClearSet(controller.nullItem);
					GTDev.LogError<string>(string.Format("CosmeticsController ParseSetFromString: wrong number of slots {0} {1}", array.Length, setString), null);
					return;
				}
				try
				{
					for (int i = 0; i < num; i++)
					{
						CosmeticsController.CosmeticSlots cosmeticSlots = (CosmeticsController.CosmeticSlots)i;
						string text = array[i];
						if (text.IsNullOrEmpty() || text == "null" || text == "NOTHING")
						{
							this.items[i] = controller.nullItem;
						}
						else
						{
							CosmeticsController.CosmeticItem item = controller.GetItemFromDict(text);
							if (item.isNullItem)
							{
								GTDev.Log<string>("CosmeticsController ParseSetFromString: Could not find item stored in player prefs: \"" + text + "\"", null);
								this.items[i] = controller.nullItem;
							}
							else if (!CosmeticsController.CompareCategoryToSavedCosmeticSlots(item.itemCategory, cosmeticSlots))
							{
								this.items[i] = controller.nullItem;
							}
							else if (controller.unlockedCosmetics.FindIndex((CosmeticsController.CosmeticItem x) => item.itemName == x.itemName) >= 0)
							{
								this.items[i] = item;
							}
							else
							{
								this.items[i] = controller.nullItem;
							}
						}
					}
				}
				catch (Exception ex)
				{
					this.ClearSet(controller.nullItem);
					GTDev.LogError<string>("CosmeticsController: Issue parsing saved outfit string: " + ex.Message, null);
				}
			}

			public string[] ToDisplayNameArray()
			{
				int num = 16;
				for (int i = 0; i < num; i++)
				{
					this.returnArray[i] = (string.IsNullOrEmpty(this.items[i].displayName) ? "null" : this.items[i].displayName);
				}
				return this.returnArray;
			}

			public int[] ToPackedIDArray()
			{
				int num = 0;
				int num2 = 0;
				int num3 = 16;
				for (int i = 0; i < num3; i++)
				{
					if (!this.items[i].isNullItem && this.items[i].itemName.Length == 6)
					{
						num |= 1 << i;
						num2++;
					}
				}
				if (num == 0)
				{
					return CosmeticsController.CosmeticSet.intArrays[0];
				}
				int[] array = CosmeticsController.CosmeticSet.intArrays[num2 + 1];
				array[0] = num;
				int num4 = 1;
				for (int j = 0; j < num3; j++)
				{
					if ((num & (1 << j)) != 0)
					{
						string itemName = this.items[j].itemName;
						array[num4] = (int)(itemName[0] - 'A' + '\u001a' * (itemName[1] - 'A' + '\u001a' * (itemName[2] - 'A' + '\u001a' * (itemName[3] - 'A' + '\u001a' * (itemName[4] - 'A')))));
						num4++;
					}
				}
				return array;
			}

			public string[] HoldableDisplayNames(bool leftHoldables)
			{
				int num = 16;
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					if (this.items[i].isHoldable && this.items[i].isHoldable && this.items[i].itemCategory != CosmeticsController.CosmeticCategory.Chest)
					{
						if (leftHoldables && BodyDockPositions.IsPositionLeft(CosmeticsController.CosmeticSlotToDropPosition((CosmeticsController.CosmeticSlots)i)))
						{
							num2++;
						}
						else if (!leftHoldables && !BodyDockPositions.IsPositionLeft(CosmeticsController.CosmeticSlotToDropPosition((CosmeticsController.CosmeticSlots)i)))
						{
							num2++;
						}
					}
				}
				if (num2 == 0)
				{
					return null;
				}
				int num3 = 0;
				string[] array = new string[num2];
				for (int j = 0; j < num; j++)
				{
					if (this.items[j].isHoldable)
					{
						if (leftHoldables && BodyDockPositions.IsPositionLeft(CosmeticsController.CosmeticSlotToDropPosition((CosmeticsController.CosmeticSlots)j)))
						{
							array[num3] = this.items[j].displayName;
							num3++;
						}
						else if (!leftHoldables && !BodyDockPositions.IsPositionLeft(CosmeticsController.CosmeticSlotToDropPosition((CosmeticsController.CosmeticSlots)j)))
						{
							array[num3] = this.items[j].displayName;
							num3++;
						}
					}
				}
				return array;
			}

			public bool[] ToOnRightSideArray()
			{
				int num = 16;
				bool[] array = new bool[num];
				for (int i = 0; i < num; i++)
				{
					if (this.items[i].isHoldable && this.items[i].itemCategory != CosmeticsController.CosmeticCategory.Chest)
					{
						array[i] = !BodyDockPositions.IsPositionLeft(CosmeticsController.CosmeticSlotToDropPosition((CosmeticsController.CosmeticSlots)i));
					}
					else
					{
						array[i] = false;
					}
				}
				return array;
			}

			public CosmeticsController.CosmeticItem[] items;

			public string[] returnArray = new string[16];

			private static int[][] intArrays = new int[][]
			{
				new int[0],
				new int[1],
				new int[2],
				new int[3],
				new int[4],
				new int[5],
				new int[6],
				new int[7],
				new int[8],
				new int[9],
				new int[10],
				new int[11],
				new int[12],
				new int[13],
				new int[14],
				new int[15],
				new int[16],
				new int[17],
				new int[18],
				new int[19],
				new int[20],
				new int[21]
			};

			private static CosmeticsController.CosmeticSet _emptySet;

			private static char[] nameScratchSpace = new char[6];

			public delegate void OnSetActivatedHandler(CosmeticsController.CosmeticSet prevSet, CosmeticsController.CosmeticSet currentSet, NetPlayer netPlayer);
		}

		[Serializable]
		public struct CosmeticItem
		{
			[Tooltip("Should match the spreadsheet item name.")]
			public string itemName;

			[Tooltip("Determines what wardrobe section the item will show up in.")]
			public CosmeticsController.CosmeticCategory itemCategory;

			[Tooltip("If this is a holdable item.")]
			public bool isHoldable;

			[Tooltip("If this is a throwable item and hidden on the wardrobe.")]
			public bool isThrowable;

			[Tooltip("Icon shown in the store menus & hunt watch.")]
			public Sprite itemPicture;

			public string displayName;

			public string itemPictureResourceString;

			[Tooltip("The name shown on the store checkout screen.")]
			public string overrideDisplayName;

			[DebugReadout]
			[NonSerialized]
			public int cost;

			[DebugReadout]
			[NonSerialized]
			public string[] bundledItems;

			[DebugReadout]
			[NonSerialized]
			public bool canTryOn;

			[Tooltip("Set to true if the item takes up both left and right wearable hand slots at the same time. Used for things like mittens/gloves.")]
			public bool bothHandsHoldable;

			public bool bLoadsFromResources;

			public bool bUsesMeshAtlas;

			public Vector3 rotationOffset;

			public Vector3 positionOffset;

			public string meshAtlasResourceString;

			public string meshResourceString;

			public string materialResourceString;

			[HideInInspector]
			public bool isNullItem;
		}

		[Serializable]
		public class IAPRequestBody
		{
			public string userID;

			public string nonce;

			public string platform;

			public string sku;

			public Dictionary<string, string> customTags;
		}

		public enum EWearingCosmeticSet
		{
			NotASet,
			NotWearing,
			Partial,
			Complete
		}
	}
}
