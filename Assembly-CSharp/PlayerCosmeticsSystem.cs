using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GorillaExtensions;
using GorillaNetworking;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

internal class PlayerCosmeticsSystem : MonoBehaviour, ITickSystemPre
{
	bool ITickSystemPre.PreTickRunning { get; set; }

	private void Awake()
	{
		if (PlayerCosmeticsSystem.instance == null)
		{
			PlayerCosmeticsSystem.instance = this;
			base.transform.SetParent(null, true);
			Object.DontDestroyOnLoad(this);
			this.inventory = new List<string>();
			this.inventory.Add("Inventory");
			NetworkSystem.Instance.OnRaiseEvent += this.OnNetEvent;
			return;
		}
		Object.Destroy(this);
	}

	private void Start()
	{
		this.playerLookUpCooldown = Mathf.Max(this.playerLookUpCooldown, 3f);
		PlayFabTitleDataCache.Instance.GetTitleData("EnableTempCosmeticUnlocks", delegate(string data)
		{
			bool flag;
			if (bool.TryParse(data, out flag))
			{
				PlayerCosmeticsSystem.TempUnlocksEnabled = flag;
				return;
			}
			Debug.LogError("PlayerCosmeticsSystem: error parsing EnableTempCosmeticUnlocks data");
		}, delegate(PlayFabError error)
		{
		}, false);
	}

	private void OnDestroy()
	{
		if (PlayerCosmeticsSystem.instance == this)
		{
			PlayerCosmeticsSystem.instance = null;
		}
	}

	private void LookUpPlayerCosmetics(bool wait = false)
	{
		if (!this.isLookingUp)
		{
			TickSystem<object>.AddPreTickCallback(this);
			if (wait)
			{
				this.startSearchingTime = Time.time;
				return;
			}
			this.startSearchingTime = float.MinValue;
		}
	}

	public void PreTick()
	{
		if (PlayerCosmeticsSystem.playersToLookUp.Count < 1)
		{
			TickSystem<object>.RemovePreTickCallback(this);
			this.startSearchingTime = float.MinValue;
			this.isLookingUp = false;
			return;
		}
		this.isLookingUp = true;
		if (this.startSearchingTime + this.playerLookUpCooldown > Time.time)
		{
			return;
		}
		if (GorillaServer.Instance.NewCosmeticsPathShouldReadSharedGroupData())
		{
			this.NewCosmeticsPath();
			return;
		}
		PlayerCosmeticsSystem.playerIDsList.Clear();
		while (PlayerCosmeticsSystem.playersToLookUp.Count > 0)
		{
			NetPlayer netPlayer = PlayerCosmeticsSystem.playersToLookUp.Dequeue();
			string text = netPlayer.ActorNumber.ToString();
			if (netPlayer.InRoom() && !PlayerCosmeticsSystem.playerIDsList.Contains(text))
			{
				if (PlayerCosmeticsSystem.playerIDsList.Count == 0)
				{
					int actorNumber = netPlayer.ActorNumber;
				}
				PlayerCosmeticsSystem.playerIDsList.Add(text);
				PlayerCosmeticsSystem.playersWaiting.AddSortedUnique(netPlayer.ActorNumber);
			}
		}
		if (PlayerCosmeticsSystem.playerIDsList.Count > 0)
		{
			global::PlayFab.ClientModels.GetSharedGroupDataRequest getSharedGroupDataRequest = new global::PlayFab.ClientModels.GetSharedGroupDataRequest();
			getSharedGroupDataRequest.Keys = PlayerCosmeticsSystem.playerIDsList;
			getSharedGroupDataRequest.SharedGroupId = NetworkSystem.Instance.RoomName + Regex.Replace(NetworkSystem.Instance.CurrentRegion, "[^a-zA-Z0-9]", "").ToUpper();
			PlayFabClientAPI.GetSharedGroupData(getSharedGroupDataRequest, new Action<GetSharedGroupDataResult>(this.OnGetsharedGroupData), delegate(PlayFabError error)
			{
				Debug.Log(error.GenerateErrorReport());
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					return;
				}
				if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
				}
			}, null, null);
		}
		this.isLookingUp = false;
	}

	private void NewCosmeticsPath()
	{
		if (this.isLookingUpNew)
		{
			return;
		}
		base.StartCoroutine(this.NewCosmeticsPathCoroutine());
	}

	private IEnumerator NewCosmeticsPathCoroutine()
	{
		this.isLookingUpNew = true;
		NetPlayer player = null;
		PlayerCosmeticsSystem.playerIDsList.Clear();
		PlayerCosmeticsSystem.playerActorNumberList.Clear();
		while (PlayerCosmeticsSystem.playersToLookUp.Count > 0)
		{
			player = PlayerCosmeticsSystem.playersToLookUp.Dequeue();
			string text = player.ActorNumber.ToString();
			if (player.InRoom() && !PlayerCosmeticsSystem.playerIDsList.Contains(text))
			{
				PlayerCosmeticsSystem.playerIDsList.Add(player.UserId);
				PlayerCosmeticsSystem.playerActorNumberList.Add(player.ActorNumber);
			}
		}
		int num;
		for (int i = 0; i < PlayerCosmeticsSystem.playerIDsList.Count; i = num + 1)
		{
			int j = i;
			global::PlayFab.ClientModels.GetSharedGroupDataRequest getSharedGroupDataRequest = new global::PlayFab.ClientModels.GetSharedGroupDataRequest();
			getSharedGroupDataRequest.Keys = this.inventory;
			getSharedGroupDataRequest.SharedGroupId = PlayerCosmeticsSystem.playerIDsList[j] + "Inventory";
			PlayFabClientAPI.GetSharedGroupData(getSharedGroupDataRequest, delegate(GetSharedGroupDataResult result)
			{
				if (!NetworkSystem.Instance.InRoom)
				{
					PlayerCosmeticsSystem.playersWaiting.Clear();
					return;
				}
				foreach (KeyValuePair<string, global::PlayFab.ClientModels.SharedGroupDataRecord> keyValuePair in result.Data)
				{
					if (!(keyValuePair.Key != "Inventory") && Utils.PlayerInRoom(PlayerCosmeticsSystem.playerActorNumberList[j]))
					{
						this.tempCosmetics = keyValuePair.Value.Value;
						IUserCosmeticsCallback userCosmeticsCallback;
						if (!PlayerCosmeticsSystem.userCosmeticCallback.TryGetValue(PlayerCosmeticsSystem.playerActorNumberList[j], out userCosmeticsCallback))
						{
							PlayerCosmeticsSystem.userCosmeticsWaiting[PlayerCosmeticsSystem.playerActorNumberList[j]] = this.tempCosmetics;
						}
						else
						{
							userCosmeticsCallback.PendingUpdate = false;
							if (!userCosmeticsCallback.OnGetUserCosmetics(this.tempCosmetics))
							{
								PlayerCosmeticsSystem.playersToLookUp.Enqueue(player);
								userCosmeticsCallback.PendingUpdate = true;
							}
						}
					}
				}
			}, delegate(PlayFabError error)
			{
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					return;
				}
				if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
				}
			}, null, null);
			yield return new WaitForSeconds(this.getSharedGroupDataCooldown);
			num = i;
		}
		this.isLookingUpNew = false;
		yield break;
	}

	private void UpdatePlayersWaitingAndDoLookup(bool retrying)
	{
		if (PlayerCosmeticsSystem.playersWaiting.Count > 0)
		{
			for (int i = PlayerCosmeticsSystem.playersWaiting.Count - 1; i >= 0; i--)
			{
				int num = PlayerCosmeticsSystem.playersWaiting[i];
				if (!Utils.PlayerInRoom(num))
				{
					PlayerCosmeticsSystem.playersWaiting.RemoveAt(i);
				}
				else
				{
					PlayerCosmeticsSystem.playersToLookUp.Enqueue(NetworkSystem.Instance.GetPlayer(num));
					retrying = true;
				}
			}
		}
		if (retrying)
		{
			this.LookUpPlayerCosmetics(true);
		}
	}

	private void OnGetsharedGroupData(GetSharedGroupDataResult result)
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			PlayerCosmeticsSystem.playersWaiting.Clear();
			return;
		}
		bool flag = false;
		foreach (KeyValuePair<string, global::PlayFab.ClientModels.SharedGroupDataRecord> keyValuePair in result.Data)
		{
			this.playerTemp = null;
			int num;
			if (int.TryParse(keyValuePair.Key, out num))
			{
				if (!Utils.PlayerInRoom(num))
				{
					PlayerCosmeticsSystem.playersWaiting.Remove(num);
				}
				else
				{
					PlayerCosmeticsSystem.playersWaiting.Remove(num);
					this.playerTemp = NetworkSystem.Instance.GetPlayer(num);
					this.tempCosmetics = keyValuePair.Value.Value;
					IUserCosmeticsCallback userCosmeticsCallback;
					if (!PlayerCosmeticsSystem.userCosmeticCallback.TryGetValue(num, out userCosmeticsCallback))
					{
						PlayerCosmeticsSystem.userCosmeticsWaiting[num] = this.tempCosmetics;
					}
					else
					{
						userCosmeticsCallback.PendingUpdate = false;
						if (!userCosmeticsCallback.OnGetUserCosmetics(this.tempCosmetics))
						{
							Debug.Log("retrying cosmetics for " + this.playerTemp.ToStringFull());
							PlayerCosmeticsSystem.playersToLookUp.Enqueue(this.playerTemp);
							flag = true;
							userCosmeticsCallback.PendingUpdate = true;
						}
					}
				}
			}
		}
		this.UpdatePlayersWaitingAndDoLookup(flag);
	}

	private void OnNetEvent(byte code, object data, int source)
	{
		if (code != 199 || source < 0)
		{
			return;
		}
		Debug.Log("OnNetEvent in Cosmetics called!!");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(source);
		GorillaNot.IncrementRPCCall(new PhotonMessageInfoWrapped(source, NetworkSystem.Instance.ServerTimestamp), "UpdatePlayerCosmetics");
		PlayerCosmeticsSystem.UpdatePlayerCosmetics(player);
	}

	private static bool nullInstance
	{
		get
		{
			return PlayerCosmeticsSystem.instance == null || !PlayerCosmeticsSystem.instance;
		}
	}

	public static bool TempUnlocksEnabled { get; private set; } = false;

	public static string[] TempUnlockCosmeticString { get; private set; } = Array.Empty<string>();

	public static void RegisterCosmeticCallback(int playerID, IUserCosmeticsCallback callback)
	{
		PlayerCosmeticsSystem.userCosmeticCallback[playerID] = callback;
		string text;
		if (PlayerCosmeticsSystem.userCosmeticsWaiting.TryGetValue(playerID, out text))
		{
			callback.PendingUpdate = false;
			callback.OnGetUserCosmetics(text);
			PlayerCosmeticsSystem.userCosmeticsWaiting.Remove(playerID);
		}
	}

	public static void RemoveCosmeticCallback(int playerID)
	{
		if (PlayerCosmeticsSystem.userCosmeticCallback.ContainsKey(playerID))
		{
			PlayerCosmeticsSystem.userCosmeticCallback.Remove(playerID);
		}
	}

	public static void UpdatePlayerCosmetics(NetPlayer player)
	{
		if (player == null || player.IsLocal)
		{
			return;
		}
		PlayerCosmeticsSystem.playersToLookUp.Enqueue(player);
		IUserCosmeticsCallback userCosmeticsCallback;
		if (PlayerCosmeticsSystem.userCosmeticCallback.TryGetValue(player.ActorNumber, out userCosmeticsCallback))
		{
			userCosmeticsCallback.PendingUpdate = true;
		}
		if (!PlayerCosmeticsSystem.nullInstance)
		{
			PlayerCosmeticsSystem.instance.LookUpPlayerCosmetics(true);
		}
	}

	public static void UpdatePlayerCosmetics(List<NetPlayer> players)
	{
		foreach (NetPlayer netPlayer in players)
		{
			if (netPlayer != null && !netPlayer.IsLocal)
			{
				PlayerCosmeticsSystem.playersToLookUp.Enqueue(netPlayer);
				IUserCosmeticsCallback userCosmeticsCallback;
				if (PlayerCosmeticsSystem.userCosmeticCallback.TryGetValue(netPlayer.ActorNumber, out userCosmeticsCallback))
				{
					userCosmeticsCallback.PendingUpdate = true;
				}
			}
		}
		if (!PlayerCosmeticsSystem.nullInstance)
		{
			PlayerCosmeticsSystem.instance.LookUpPlayerCosmetics(false);
		}
	}

	public static void SetRigTryOn(bool inTryon, RigContainer rigRefg)
	{
		VRRig rig = rigRefg.Rig;
		rig.inTryOnRoom = inTryon;
		if (inTryon)
		{
			if (PlayerCosmeticsSystem.sinceLastTryOnEvent.HasElapsed(0.5f, true))
			{
				GorillaTelemetry.PostShopEvent(rig, GTShopEventType.item_try_on, rig.tryOnSet.items);
			}
		}
		else if (rig.isOfflineVRRig)
		{
			rig.tryOnSet.ClearSet(CosmeticsController.instance.nullItem);
			CosmeticsController.instance.ClearCheckout(false);
			CosmeticsController.instance.UpdateShoppingCart();
			CosmeticsController.instance.UpdateWornCosmetics(true);
			rig.myBodyDockPositions.RefreshTransferrableItems();
			return;
		}
		rig.LocalUpdateCosmeticsWithTryon(rig.cosmeticSet, rig.tryOnSet);
		rig.myBodyDockPositions.RefreshTransferrableItems();
	}

	public static void UnlockTemporaryCosmeticsForPlayer(RigContainer rigRef)
	{
		PlayerCosmeticsSystem.UnlockTemporaryCosmeticsForPlayer(rigRef, PlayerCosmeticsSystem.TempUnlockCosmeticString);
	}

	public static void UnlockTemporaryCosmeticsForPlayer(RigContainer rigRef, IReadOnlyList<string> cosmeticIds)
	{
		if (cosmeticIds == null)
		{
			Debug.LogError("PlayerCosmeticsSystem failed to unlock temporary cosmetics, cosmetic IDs are null");
			return;
		}
		VRRig rig = rigRef.Rig;
		foreach (string text in cosmeticIds)
		{
			if (!rig.concatStringOfCosmeticsAllowed.Contains(text) && rig.TemporaryCosmetics.Add(text) && rig.isOfflineVRRig)
			{
				CosmeticsController.instance.AddTempUnlockToWardrobe(text);
			}
		}
		if (rig.isOfflineVRRig)
		{
			CosmeticsController.instance.UpdateWornCosmetics(true);
			return;
		}
		rig.RefreshCosmetics();
	}

	public static void LockTemporaryCosmeticsForPlayer(RigContainer rigRef)
	{
		PlayerCosmeticsSystem.LockTemporaryCosmeticsForPlayer(rigRef, PlayerCosmeticsSystem.TempUnlockCosmeticString);
	}

	public static void LockTemporaryCosmeticsForPlayer(RigContainer rigRef, IReadOnlyList<string> cosmeticIds)
	{
		if (cosmeticIds == null)
		{
			Debug.LogError("PlayerCosmeticsSystem failed to unlock temporary cosmetics, cosmetic IDs are null");
			return;
		}
		VRRig rig = rigRef.Rig;
		foreach (string text in cosmeticIds)
		{
			if (rig.TemporaryCosmetics.Remove(text) && rig.isOfflineVRRig && !rig.concatStringOfCosmeticsAllowed.Contains(text))
			{
				CosmeticsController.instance.RemoveTempUnlockFromWardrobe(text);
			}
		}
		if (rig.isOfflineVRRig)
		{
			CosmeticsController.instance.UpdateWornCosmetics(true);
			return;
		}
		rig.RefreshCosmetics();
	}

	internal static void UnlockTemporaryCosmeticsGlobal(IReadOnlyList<string> cosmeticIds)
	{
		int count = cosmeticIds.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerCosmeticsSystem.UnlockTemporaryCosmeticGlobal(cosmeticIds[i]);
		}
	}

	internal static void UnlockTemporaryCosmeticGlobal(string cosmeticId)
	{
		int num = 0;
		if (PlayerCosmeticsSystem.k_tempUnlockedCosmetics.ContainsKey(cosmeticId))
		{
			num = PlayerCosmeticsSystem.k_tempUnlockedCosmetics[cosmeticId];
		}
		num++;
		PlayerCosmeticsSystem.k_tempUnlockedCosmetics[cosmeticId] = num;
	}

	internal static void LockTemporaryCosmeticsGlobal(IReadOnlyList<string> cosmeticIds)
	{
		int count = cosmeticIds.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerCosmeticsSystem.LockTemporaryCosmeticGlobal(cosmeticIds[i]);
		}
	}

	internal static void LockTemporaryCosmeticGlobal(string cosmeticId)
	{
		if (!PlayerCosmeticsSystem.k_tempUnlockedCosmetics.ContainsKey(cosmeticId))
		{
			Debug.LogError("PlayerCosmeticsSystem: Unable to lock cosmetic, ID:-" + cosmeticId + " not found!");
			return;
		}
		int num = PlayerCosmeticsSystem.k_tempUnlockedCosmetics[cosmeticId];
		num--;
		PlayerCosmeticsSystem.k_tempUnlockedCosmetics[cosmeticId] = num;
	}

	public static bool IsTemporaryCosmeticAllowed(VRRig rigRef, string cosmeticId)
	{
		int num;
		return rigRef.TemporaryCosmetics.Contains(cosmeticId) || (PlayerCosmeticsSystem.k_tempUnlockedCosmetics.TryGetValue(cosmeticId, out num) && num > 0);
	}

	public static void StaticReset()
	{
		PlayerCosmeticsSystem.playersToLookUp.Clear();
		PlayerCosmeticsSystem.userCosmeticCallback.Clear();
		PlayerCosmeticsSystem.userCosmeticsWaiting.Clear();
		PlayerCosmeticsSystem.playerIDsList.Clear();
		PlayerCosmeticsSystem.playersWaiting.Clear();
	}

	public float playerLookUpCooldown = 3f;

	public float getSharedGroupDataCooldown = 0.1f;

	private float startSearchingTime = float.MinValue;

	private bool isLookingUp;

	private bool isLookingUpNew;

	private string tempCosmetics;

	private NetPlayer playerTemp;

	private RigContainer tempRC;

	private List<string> inventory;

	private static PlayerCosmeticsSystem instance;

	private static Queue<NetPlayer> playersToLookUp = new Queue<NetPlayer>(10);

	private static Dictionary<int, IUserCosmeticsCallback> userCosmeticCallback = new Dictionary<int, IUserCosmeticsCallback>(10);

	private static Dictionary<int, string> userCosmeticsWaiting = new Dictionary<int, string>(5);

	private static List<string> playerIDsList = new List<string>(10);

	private static List<int> playerActorNumberList = new List<int>(10);

	private static List<int> playersWaiting = new List<int>();

	private static TimeSince sinceLastTryOnEvent = 0f;

	private static readonly Dictionary<string, int> k_tempUnlockedCosmetics = new Dictionary<string, int>(20);
}
