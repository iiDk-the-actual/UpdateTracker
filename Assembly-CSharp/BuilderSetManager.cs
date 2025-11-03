using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

public class BuilderSetManager : MonoBehaviour
{
	public static bool hasInstance { get; private set; }

	public string GetStarterSetsConcat()
	{
		if (BuilderSetManager.concatStarterSets.Length > 0)
		{
			return BuilderSetManager.concatStarterSets;
		}
		BuilderSetManager.concatStarterSets = string.Empty;
		foreach (BuilderPieceSet builderPieceSet in this._starterPieceSets)
		{
			BuilderSetManager.concatStarterSets += builderPieceSet.playfabID;
		}
		return BuilderSetManager.concatStarterSets;
	}

	public string GetAllSetsConcat()
	{
		if (BuilderSetManager.concatAllSets.Length > 0)
		{
			return BuilderSetManager.concatAllSets;
		}
		BuilderSetManager.concatAllSets = string.Empty;
		foreach (BuilderPieceSet builderPieceSet in this._allPieceSets)
		{
			BuilderSetManager.concatAllSets += builderPieceSet.playfabID;
		}
		return BuilderSetManager.concatAllSets;
	}

	public void Awake()
	{
		if (BuilderSetManager.instance == null)
		{
			BuilderSetManager.instance = this;
			BuilderSetManager.hasInstance = true;
		}
		else if (BuilderSetManager.instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		this.Init();
		if (this.monitor == null)
		{
			this.monitor = base.StartCoroutine(this.MonitorTime());
		}
	}

	private void Init()
	{
		this.InitPieceDictionary();
		this.catalog = "DLC";
		this.currencyName = "SR";
		this.pulledStoreItems = false;
		BuilderSetManager._setIdToStoreItem = new Dictionary<int, BuilderSetManager.BuilderSetStoreItem>(this._allPieceSets.Count);
		BuilderSetManager._setIdToStoreItem.Clear();
		BuilderSetManager.pieceSetInfos = new List<BuilderSetManager.BuilderPieceSetInfo>(this._allPieceSets.Count * 45);
		BuilderSetManager.pieceSetInfoMap = new Dictionary<int, int>(this._allPieceSets.Count * 45);
		this.livePieceSets = new List<BuilderPieceSet>(this._allPieceSets.Count);
		this.scheduledPieceSets = new List<BuilderPieceSet>(this._allPieceSets.Count);
		this.displayGroups = new List<BuilderPieceSet.BuilderDisplayGroup>(this._allPieceSets.Count * 2);
		this.displayGroupMap = new Dictionary<int, int>(this._allPieceSets.Count * 2);
		this.liveDisplayGroups = new List<BuilderPieceSet.BuilderDisplayGroup>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>(5);
		foreach (BuilderPieceSet builderPieceSet in this._allPieceSets)
		{
			dictionary.Clear();
			int num = 0;
			BuilderSetManager.BuilderSetStoreItem builderSetStoreItem = new BuilderSetManager.BuilderSetStoreItem
			{
				displayName = builderPieceSet.SetName,
				playfabID = builderPieceSet.playfabID,
				setID = builderPieceSet.GetIntIdentifier(),
				cost = 0U,
				setRef = builderPieceSet,
				displayModel = builderPieceSet.displayModel,
				isNullItem = false
			};
			BuilderSetManager._setIdToStoreItem.TryAdd(builderPieceSet.GetIntIdentifier(), builderSetStoreItem);
			int num2 = -1;
			if (!string.IsNullOrEmpty(builderPieceSet.materialId))
			{
				num2 = builderPieceSet.materialId.GetHashCode();
			}
			for (int i = 0; i < builderPieceSet.subsets.Count; i++)
			{
				BuilderPieceSet.BuilderPieceSubset builderPieceSubset = builderPieceSet.subsets[i];
				if (!builderPieceSet.setName.Equals("HIDDEN"))
				{
					string text = builderPieceSet.subsets[i].GetShelfButtonName();
					if (text.IsNullOrEmpty())
					{
						text = builderPieceSet.setName;
					}
					text = text.ToUpper();
					int num3;
					if (dictionary.TryGetValue(text, out num3))
					{
						int num4;
						this.displayGroupMap.TryGetValue(num3, out num4);
						BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup = this.displayGroups[num4];
						builderDisplayGroup.pieceSubsets.Add(builderPieceSet.subsets[i]);
						this.displayGroups[num4] = builderDisplayGroup;
					}
					else
					{
						string groupUniqueID = this.GetGroupUniqueID(builderPieceSet.playfabID, num);
						num++;
						BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup2 = new BuilderPieceSet.BuilderDisplayGroup(text, builderPieceSet.materialId, builderPieceSet.GetIntIdentifier(), groupUniqueID);
						builderDisplayGroup2.pieceSubsets.Add(builderPieceSet.subsets[i]);
						dictionary.Add(text, builderDisplayGroup2.GetDisplayGroupIdentifier());
						this.displayGroupMap.Add(builderDisplayGroup2.GetDisplayGroupIdentifier(), this.displayGroups.Count);
						this.displayGroups.Add(builderDisplayGroup2);
						if (!builderPieceSet.isScheduled)
						{
							this.liveDisplayGroups.Add(builderDisplayGroup2);
						}
					}
				}
				for (int j = 0; j < builderPieceSubset.pieceInfos.Count; j++)
				{
					BuilderPiece piecePrefab = builderPieceSubset.pieceInfos[j].piecePrefab;
					piecePrefab == null;
					int staticHash = piecePrefab.name.GetStaticHash();
					int num5 = num2;
					if (piecePrefab.materialOptions == null)
					{
						num5 = -1;
						this.AddPieceToInfoMap(staticHash, num5, builderPieceSet.GetIntIdentifier());
					}
					else if (builderPieceSubset.pieceInfos[j].overrideSetMaterial)
					{
						if (builderPieceSubset.pieceInfos[j].pieceMaterialTypes.Length == 0)
						{
							Debug.LogErrorFormat("Material List for piece {0} in set {1} is empty", new object[] { piecePrefab.name, builderPieceSet.SetName });
						}
						foreach (string text2 in builderPieceSubset.pieceInfos[j].pieceMaterialTypes)
						{
							if (string.IsNullOrEmpty(text2))
							{
								Debug.LogErrorFormat("Material List Entry for piece {0} in set {1} is empty", new object[] { piecePrefab.name, builderPieceSet.SetName });
							}
							else
							{
								num5 = text2.GetHashCode();
								this.AddPieceToInfoMap(staticHash, num5, builderPieceSet.GetIntIdentifier());
							}
						}
					}
					else
					{
						Material material;
						int num6;
						piecePrefab.materialOptions.GetMaterialFromType(num2, out material, out num6);
						if (material == null)
						{
							num5 = -1;
						}
						this.AddPieceToInfoMap(staticHash, num5, builderPieceSet.GetIntIdentifier());
					}
				}
			}
			if (!builderPieceSet.isScheduled)
			{
				this.livePieceSets.Add(builderPieceSet);
			}
			else
			{
				this.scheduledPieceSets.Add(builderPieceSet);
			}
		}
		this._unlockedPieceSets = new List<BuilderPieceSet>(this._allPieceSets.Count);
		this._unlockedPieceSets.AddRange(this._starterPieceSets);
	}

	private string GetGroupUniqueID(string setPlayfabID, int groupNumber)
	{
		return setPlayfabID.Trim('.') + ((char)(65 + groupNumber)).ToString();
	}

	public void InitPieceDictionary()
	{
		if (this.hasPieceDictionary)
		{
			return;
		}
		BuilderSetManager.pieceTypes = new List<int>(256);
		BuilderSetManager.pieceList = new List<BuilderPiece>(256);
		BuilderSetManager.pieceTypeToIndex = new Dictionary<int, int>(256);
		int num = 0;
		for (int i = 0; i < this._allPieceSets.Count; i++)
		{
			BuilderPieceSet builderPieceSet = this._allPieceSets[i];
			if (!(builderPieceSet == null))
			{
				for (int j = 0; j < builderPieceSet.subsets.Count; j++)
				{
					BuilderPieceSet.BuilderPieceSubset builderPieceSubset = builderPieceSet.subsets[j];
					if (!(builderPieceSet == null))
					{
						for (int k = 0; k < builderPieceSubset.pieceInfos.Count; k++)
						{
							BuilderPieceSet.PieceInfo pieceInfo = builderPieceSubset.pieceInfos[k];
							if (!(pieceInfo.piecePrefab == null))
							{
								int staticHash = pieceInfo.piecePrefab.name.GetStaticHash();
								if (!BuilderSetManager.pieceTypeToIndex.ContainsKey(staticHash))
								{
									BuilderSetManager.pieceList.Add(pieceInfo.piecePrefab);
									BuilderSetManager.pieceTypes.Add(staticHash);
									BuilderSetManager.pieceTypeToIndex.Add(staticHash, num);
									num++;
								}
							}
						}
					}
				}
			}
		}
		this.hasPieceDictionary = true;
	}

	public BuilderPiece GetPiecePrefab(int pieceType)
	{
		int num;
		if (BuilderSetManager.pieceTypeToIndex.TryGetValue(pieceType, out num))
		{
			return BuilderSetManager.pieceList[num];
		}
		Debug.LogErrorFormat("No Prefab found for type {0}", new object[] { pieceType });
		return null;
	}

	private void OnEnable()
	{
		if (this.monitor == null && this.scheduledPieceSets.Count > 0)
		{
			this.monitor = base.StartCoroutine(this.MonitorTime());
		}
	}

	private void OnDisable()
	{
		if (this.monitor != null)
		{
			base.StopCoroutine(this.monitor);
		}
		this.monitor = null;
	}

	private IEnumerator MonitorTime()
	{
		while (GorillaComputer.instance == null || GorillaComputer.instance.startupMillis == 0L)
		{
			yield return null;
		}
		while (this.scheduledPieceSets.Count > 0)
		{
			bool flag = false;
			for (int i = this.scheduledPieceSets.Count - 1; i >= 0; i--)
			{
				BuilderPieceSet builderPieceSet = this.scheduledPieceSets[i];
				if (GorillaComputer.instance.GetServerTime() > builderPieceSet.GetScheduleDateTime())
				{
					flag = true;
					this.livePieceSets.Add(builderPieceSet);
					this.scheduledPieceSets.RemoveAt(i);
					int intIdentifier = builderPieceSet.GetIntIdentifier();
					foreach (BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup in this.displayGroups)
					{
						if (builderDisplayGroup != null && builderDisplayGroup.setID == intIdentifier && !this.liveDisplayGroups.Contains(builderDisplayGroup))
						{
							this.liveDisplayGroups.Add(builderDisplayGroup);
						}
					}
				}
			}
			if (flag)
			{
				this.OnLiveSetsUpdated.Invoke();
			}
			yield return new WaitForSeconds(60f);
		}
		this.monitor = null;
		yield break;
	}

	private void AddPieceToInfoMap(int pieceType, int pieceMaterial, int setID)
	{
		int num;
		if (BuilderSetManager.pieceSetInfoMap.TryGetValue(HashCode.Combine<int, int>(pieceType, pieceMaterial), out num))
		{
			BuilderSetManager.BuilderPieceSetInfo builderPieceSetInfo = BuilderSetManager.pieceSetInfos[num];
			if (!builderPieceSetInfo.setIds.Contains(setID))
			{
				builderPieceSetInfo.setIds.Add(setID);
			}
			BuilderSetManager.pieceSetInfos[num] = builderPieceSetInfo;
			return;
		}
		BuilderSetManager.BuilderPieceSetInfo builderPieceSetInfo2 = new BuilderSetManager.BuilderPieceSetInfo
		{
			pieceType = pieceType,
			materialType = pieceMaterial,
			setIds = new List<int> { setID }
		};
		BuilderSetManager.pieceSetInfoMap.Add(HashCode.Combine<int, int>(pieceType, pieceMaterial), BuilderSetManager.pieceSetInfos.Count);
		BuilderSetManager.pieceSetInfos.Add(builderPieceSetInfo2);
	}

	public static bool IsItemIDBuilderItem(string playfabID)
	{
		return BuilderSetManager.instance.GetAllSetsConcat().Contains(playfabID);
	}

	public void OnGotInventoryItems(GetUserInventoryResult inventoryResult, GetCatalogItemsResult catalogResult)
	{
		CosmeticsController cosmeticsController = CosmeticsController.instance;
		cosmeticsController.concatStringCosmeticsAllowed += this.GetStarterSetsConcat();
		this._unlockedPieceSets.Clear();
		this._unlockedPieceSets.AddRange(this._starterPieceSets);
		foreach (CatalogItem catalogItem in catalogResult.Catalog)
		{
			BuilderSetManager.BuilderSetStoreItem builderSetStoreItem;
			if (BuilderSetManager.IsItemIDBuilderItem(catalogItem.ItemId) && BuilderSetManager._setIdToStoreItem.TryGetValue(catalogItem.ItemId.GetStaticHash(), out builderSetStoreItem))
			{
				bool flag = false;
				uint num = 0U;
				if (catalogItem.VirtualCurrencyPrices.TryGetValue(this.currencyName, out num))
				{
					flag = true;
				}
				builderSetStoreItem.playfabID = catalogItem.ItemId;
				builderSetStoreItem.cost = num;
				builderSetStoreItem.hasPrice = flag;
				BuilderSetManager._setIdToStoreItem[builderSetStoreItem.setRef.GetIntIdentifier()] = builderSetStoreItem;
			}
		}
		foreach (ItemInstance itemInstance in inventoryResult.Inventory)
		{
			if (BuilderSetManager.IsItemIDBuilderItem(itemInstance.ItemId))
			{
				BuilderSetManager.BuilderSetStoreItem builderSetStoreItem2;
				if (BuilderSetManager._setIdToStoreItem.TryGetValue(itemInstance.ItemId.GetStaticHash(), out builderSetStoreItem2))
				{
					Debug.LogFormat("BuilderSetManager: Unlocking Inventory Item {0}", new object[] { itemInstance.ItemId });
					this._unlockedPieceSets.Add(builderSetStoreItem2.setRef);
					CosmeticsController cosmeticsController2 = CosmeticsController.instance;
					cosmeticsController2.concatStringCosmeticsAllowed += itemInstance.ItemId;
				}
				else
				{
					Debug.Log("BuilderSetManager: No store item found with id" + itemInstance.ItemId);
				}
			}
		}
		this.pulledStoreItems = true;
		UnityEvent onOwnedSetsUpdated = this.OnOwnedSetsUpdated;
		if (onOwnedSetsUpdated == null)
		{
			return;
		}
		onOwnedSetsUpdated.Invoke();
	}

	public BuilderSetManager.BuilderSetStoreItem GetStoreItemFromSetID(int setID)
	{
		return BuilderSetManager._setIdToStoreItem.GetValueOrDefault(setID, BuilderKiosk.nullItem);
	}

	public BuilderPieceSet GetPieceSetFromID(int setID)
	{
		BuilderSetManager.BuilderSetStoreItem builderSetStoreItem;
		if (BuilderSetManager._setIdToStoreItem.TryGetValue(setID, out builderSetStoreItem))
		{
			return builderSetStoreItem.setRef;
		}
		return null;
	}

	public BuilderPieceSet.BuilderDisplayGroup GetDisplayGroupFromIndex(int groupID)
	{
		int num;
		if (this.displayGroupMap.TryGetValue(groupID, out num))
		{
			return this.displayGroups[num];
		}
		return null;
	}

	public List<BuilderPieceSet> GetAllPieceSets()
	{
		return this._allPieceSets;
	}

	public List<BuilderPieceSet> GetLivePieceSets()
	{
		return this.livePieceSets;
	}

	public List<BuilderPieceSet.BuilderDisplayGroup> GetLiveDisplayGroups()
	{
		return this.liveDisplayGroups;
	}

	public List<BuilderPieceSet> GetUnlockedPieceSets()
	{
		return this._unlockedPieceSets;
	}

	public List<BuilderPieceSet> GetPermanentSetsForSale()
	{
		return this._setsAlwaysForSale;
	}

	public List<BuilderPieceSet> GetSeasonalSetsForSale()
	{
		return this._seasonalSetsForSale;
	}

	public bool IsSetSeasonal(string playfabID)
	{
		return !this._seasonalSetsForSale.IsNullOrEmpty<BuilderPieceSet>() && this._seasonalSetsForSale.FindIndex((BuilderPieceSet x) => x.playfabID.Equals(playfabID)) >= 0;
	}

	public bool DoesPlayerOwnDisplayGroup(Player player, int groupID)
	{
		if (player == null)
		{
			return false;
		}
		int num;
		if (!this.displayGroupMap.TryGetValue(groupID, out num))
		{
			return false;
		}
		if (num < 0 || num >= this.displayGroups.Count)
		{
			return false;
		}
		BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup = this.displayGroups[num];
		return builderDisplayGroup != null && this.DoesPlayerOwnPieceSet(player, builderDisplayGroup.setID);
	}

	public bool DoesPlayerOwnPieceSet(Player player, int setID)
	{
		BuilderPieceSet pieceSetFromID = this.GetPieceSetFromID(setID);
		if (pieceSetFromID == null)
		{
			return false;
		}
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			bool flag = rigContainer.Rig.IsItemAllowed(pieceSetFromID.playfabID);
			Debug.LogFormat("BuilderSetManager: does player {0} own set {1} {2}", new object[] { player.ActorNumber, pieceSetFromID.SetName, flag });
			return flag;
		}
		Debug.LogFormat("BuilderSetManager: could not get rig for player {0}", new object[] { player.ActorNumber });
		return false;
	}

	public bool DoesAnyPlayerInRoomOwnPieceSet(int setID)
	{
		BuilderPieceSet pieceSetFromID = this.GetPieceSetFromID(setID);
		if (pieceSetFromID == null)
		{
			return false;
		}
		if (this.GetStarterSetsConcat().Contains(pieceSetFromID.setName))
		{
			return true;
		}
		foreach (NetPlayer netPlayer in RoomSystem.PlayersInRoom)
		{
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(netPlayer, out rigContainer) && rigContainer.Rig.IsItemAllowed(pieceSetFromID.playfabID))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPieceOwnedByRoom(int pieceType, int materialType)
	{
		int num;
		if (BuilderSetManager.pieceSetInfoMap.TryGetValue(HashCode.Combine<int, int>(pieceType, materialType), out num))
		{
			foreach (int num2 in BuilderSetManager.pieceSetInfos[num].setIds)
			{
				if (this.DoesAnyPlayerInRoomOwnPieceSet(num2))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool IsPieceOwnedLocally(int pieceType, int materialType)
	{
		int num;
		if (BuilderSetManager.pieceSetInfoMap.TryGetValue(HashCode.Combine<int, int>(pieceType, materialType), out num))
		{
			foreach (int num2 in BuilderSetManager.pieceSetInfos[num].setIds)
			{
				if (this.IsPieceSetOwnedLocally(num2))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool IsPieceSetOwnedLocally(int setID)
	{
		return this._unlockedPieceSets.FindIndex((BuilderPieceSet x) => setID == x.GetIntIdentifier()) >= 0;
	}

	public void UnlockSet(int setID)
	{
		int num = this._allPieceSets.FindIndex((BuilderPieceSet x) => setID == x.GetIntIdentifier());
		if (num >= 0 && !this._unlockedPieceSets.Contains(this._allPieceSets[num]))
		{
			Debug.Log("BuilderSetManager: unlocking set " + this._allPieceSets[num].SetName);
			this._unlockedPieceSets.Add(this._allPieceSets[num]);
		}
		UnityEvent onOwnedSetsUpdated = this.OnOwnedSetsUpdated;
		if (onOwnedSetsUpdated == null)
		{
			return;
		}
		onOwnedSetsUpdated.Invoke();
	}

	public void TryPurchaseItem(int setID, Action<bool> resultCallback)
	{
		BuilderSetManager.BuilderSetStoreItem storeItem;
		if (!BuilderSetManager._setIdToStoreItem.TryGetValue(setID, out storeItem))
		{
			Debug.Log("BuilderSetManager: no store Item for set " + setID.ToString());
			Action<bool> resultCallback2 = resultCallback;
			if (resultCallback2 == null)
			{
				return;
			}
			resultCallback2(false);
			return;
		}
		else
		{
			if (!this.IsPieceSetOwnedLocally(setID))
			{
				PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
				{
					ItemId = storeItem.playfabID,
					Price = (int)storeItem.cost,
					VirtualCurrency = this.currencyName,
					CatalogVersion = this.catalog
				}, delegate(PurchaseItemResult result)
				{
					if (result.Items.Count > 0)
					{
						foreach (ItemInstance itemInstance in result.Items)
						{
							Debug.Log("BuilderSetManager: unlocking set " + itemInstance.ItemId);
							this.UnlockSet(itemInstance.ItemId.GetStaticHash());
						}
						CosmeticsController.instance.UpdateMyCosmetics();
						if (PhotonNetwork.InRoom)
						{
							this.StartCoroutine(this.CheckIfMyCosmeticsUpdated(storeItem.playfabID));
						}
						Action<bool> resultCallback4 = resultCallback;
						if (resultCallback4 == null)
						{
							return;
						}
						resultCallback4(true);
						return;
					}
					else
					{
						Debug.Log("BuilderSetManager: no items purchased ");
						Action<bool> resultCallback5 = resultCallback;
						if (resultCallback5 == null)
						{
							return;
						}
						resultCallback5(false);
						return;
					}
				}, delegate(PlayFabError error)
				{
					Debug.LogErrorFormat("BuilderSetManager: purchase {0} Error {1}", new object[] { setID, error.ErrorMessage });
					Action<bool> resultCallback6 = resultCallback;
					if (resultCallback6 == null)
					{
						return;
					}
					resultCallback6(false);
				}, null, null);
				return;
			}
			Debug.Log("BuilderSetManager: set already owned " + setID.ToString());
			Action<bool> resultCallback3 = resultCallback;
			if (resultCallback3 == null)
			{
				return;
			}
			resultCallback3(false);
			return;
		}
	}

	private IEnumerator CheckIfMyCosmeticsUpdated(string itemToBuyID)
	{
		yield return new WaitForSeconds(1f);
		this.foundCosmetic = false;
		this.attempts = 0;
		while (!this.foundCosmetic && this.attempts < 10 && PhotonNetwork.InRoom)
		{
			this.playerIDList.Clear();
			if (GorillaServer.Instance != null && GorillaServer.Instance.NewCosmeticsPath())
			{
				this.playerIDList.Add("Inventory");
				PlayFabClientAPI.GetSharedGroupData(new global::PlayFab.ClientModels.GetSharedGroupDataRequest
				{
					Keys = this.playerIDList,
					SharedGroupId = PhotonNetwork.LocalPlayer.UserId + "Inventory"
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
					bool flag = this.foundCosmetic;
				}, delegate(PlayFabError error)
				{
					this.attempts++;
					CosmeticsController.instance.ReauthOrBan(error);
				}, null, null);
				yield return new WaitForSeconds(1f);
			}
			else
			{
				this.playerIDList.Add(PhotonNetwork.LocalPlayer.ActorNumber.ToString());
				PlayFabClientAPI.GetSharedGroupData(new global::PlayFab.ClientModels.GetSharedGroupDataRequest
				{
					Keys = this.playerIDList,
					SharedGroupId = PhotonNetwork.CurrentRoom.Name + Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper()
				}, delegate(GetSharedGroupDataResult result)
				{
					this.attempts++;
					foreach (KeyValuePair<string, global::PlayFab.ClientModels.SharedGroupDataRecord> keyValuePair2 in result.Data)
					{
						if (keyValuePair2.Value.Value.Contains(itemToBuyID))
						{
							Debug.Log("BuilderSetManager: found it! updating others cosmetic!");
							PhotonNetwork.RaiseEvent(199, null, new RaiseEventOptions
							{
								Receivers = ReceiverGroup.Others
							}, SendOptions.SendReliable);
							this.foundCosmetic = true;
						}
						else
						{
							Debug.Log("BuilderSetManager: didnt find it, updating attempts and trying again in a bit. current attempt is " + this.attempts.ToString());
						}
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
					Debug.Log("BuilderSetManager: Got error retrieving user data, on attempt " + this.attempts.ToString());
					Debug.Log(error.GenerateErrorReport());
				}, null, null);
				yield return new WaitForSeconds(1f);
			}
		}
		Debug.Log("BuilderSetManager: done!");
		yield break;
	}

	private const string preLog = "[GT/MonkeBlocks/BuilderSetManager]  ";

	private const string preErr = "[GT/MonkeBlocks/BuilderSetManager]  ERROR!!!  ";

	private const string preErrBeta = "[GT/MonkeBlocks/BuilderSetManager]  ERROR!!!  (beta only log)  ";

	[SerializeField]
	private List<BuilderPieceSet> _allPieceSets;

	[SerializeField]
	private List<BuilderPieceSet> _starterPieceSets;

	[SerializeField]
	private List<BuilderPieceSet> _setsAlwaysForSale;

	[SerializeField]
	private List<BuilderPieceSet> _seasonalSetsForSale;

	private List<BuilderPieceSet> livePieceSets;

	private List<BuilderPieceSet> scheduledPieceSets;

	private List<BuilderPieceSet.BuilderDisplayGroup> liveDisplayGroups;

	private Coroutine monitor;

	private List<BuilderSetManager.BuilderSetStoreItem> _allStoreItems;

	private List<BuilderPieceSet> _unlockedPieceSets;

	private static Dictionary<int, BuilderSetManager.BuilderSetStoreItem> _setIdToStoreItem;

	private static List<BuilderSetManager.BuilderPieceSetInfo> pieceSetInfos;

	private static Dictionary<int, int> pieceSetInfoMap;

	private List<BuilderPieceSet.BuilderDisplayGroup> displayGroups;

	private Dictionary<int, int> displayGroupMap;

	[OnEnterPlay_SetNull]
	public static volatile BuilderSetManager instance;

	[HideInInspector]
	public string catalog;

	[HideInInspector]
	public string currencyName;

	private string[] tempStringArray;

	[HideInInspector]
	public UnityEvent OnLiveSetsUpdated;

	[HideInInspector]
	public UnityEvent OnOwnedSetsUpdated;

	[HideInInspector]
	public bool pulledStoreItems;

	[OnEnterPlay_Set("")]
	private static string concatStarterSets = string.Empty;

	[OnEnterPlay_Set("")]
	private static string concatAllSets = string.Empty;

	private bool foundCosmetic;

	private int attempts;

	private List<string> playerIDList = new List<string>();

	private static List<int> pieceTypes;

	[HideInInspector]
	public static List<BuilderPiece> pieceList;

	private static Dictionary<int, int> pieceTypeToIndex;

	private bool hasPieceDictionary;

	[Serializable]
	public struct BuilderSetStoreItem
	{
		public string displayName;

		public string playfabID;

		public int setID;

		public uint cost;

		public bool hasPrice;

		public BuilderPieceSet setRef;

		public GameObject displayModel;

		[NonSerialized]
		public bool isNullItem;
	}

	[Serializable]
	public struct BuilderPieceSetInfo
	{
		public int pieceType;

		public int materialType;

		public List<int> setIds;
	}
}
