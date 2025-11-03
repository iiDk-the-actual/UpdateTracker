using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using GorillaNetworking;
using GorillaTag;
using Photon.Realtime;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using Steamworks;
using UnityEngine;

public abstract class NetworkSystem : MonoBehaviour
{
	public bool groupJoinInProgress { get; protected set; }

	public NetSystemState netState
	{
		get
		{
			return this.testState;
		}
		protected set
		{
			Debug.Log("netstate set to:" + value.ToString());
			this.testState = value;
		}
	}

	public NetPlayer LocalPlayer
	{
		get
		{
			return this.netPlayerCache.Find((NetPlayer p) => p.IsLocal);
		}
	}

	public virtual bool IsMasterClient { get; }

	public virtual NetPlayer MasterClient
	{
		get
		{
			return this.netPlayerCache.Find((NetPlayer p) => p.IsMasterClient);
		}
	}

	public Recorder LocalRecorder
	{
		get
		{
			return this.localRecorder;
		}
	}

	public Speaker LocalSpeaker
	{
		get
		{
			return this.localSpeaker;
		}
	}

	protected void JoinedNetworkRoom()
	{
		VRRigCache.Instance.OnJoinedRoom();
		DelegateListProcessor onJoinedRoomEvent = this.OnJoinedRoomEvent;
		if (onJoinedRoomEvent == null)
		{
			return;
		}
		onJoinedRoomEvent.InvokeSafe();
	}

	internal void MultiplayerStarted()
	{
		DelegateListProcessor onMultiplayerStarted = this.OnMultiplayerStarted;
		if (onMultiplayerStarted == null)
		{
			return;
		}
		onMultiplayerStarted.InvokeSafe();
	}

	protected void SinglePlayerStarted()
	{
		try
		{
			DelegateListProcessor onReturnedToSinglePlayer = this.OnReturnedToSinglePlayer;
			if (onReturnedToSinglePlayer != null)
			{
				onReturnedToSinglePlayer.InvokeSafe();
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		VRRigCache.Instance.OnLeftRoom();
	}

	protected void PlayerJoined(NetPlayer netPlayer)
	{
		if (this.IsOnline)
		{
			VRRigCache.Instance.OnPlayerEnteredRoom(netPlayer);
			DelegateListProcessor<NetPlayer> onPlayerJoined = this.OnPlayerJoined;
			if (onPlayerJoined == null)
			{
				return;
			}
			onPlayerJoined.InvokeSafe(in netPlayer);
		}
	}

	protected void PlayerLeft(NetPlayer netPlayer)
	{
		try
		{
			DelegateListProcessor<NetPlayer> onPlayerLeft = this.OnPlayerLeft;
			if (onPlayerLeft != null)
			{
				onPlayerLeft.InvokeSafe(in netPlayer);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		VRRigCache.Instance.OnPlayerLeftRoom(netPlayer);
	}

	protected void OnMasterClientSwitchedCallback(NetPlayer nMaster)
	{
		DelegateListProcessor<NetPlayer> onMasterClientSwitchedEvent = this.OnMasterClientSwitchedEvent;
		if (onMasterClientSwitchedEvent == null)
		{
			return;
		}
		onMasterClientSwitchedEvent.InvokeSafe(in nMaster);
	}

	public event Action<byte, object, int> OnRaiseEvent;

	internal void RaiseEvent(byte eventCode, object data, int source)
	{
		Action<byte, object, int> onRaiseEvent = this.OnRaiseEvent;
		if (onRaiseEvent == null)
		{
			return;
		}
		onRaiseEvent(eventCode, data, source);
	}

	public event Action<Dictionary<string, object>> OnCustomAuthenticationResponse;

	internal void CustomAuthenticationResponse(Dictionary<string, object> response)
	{
		Action<Dictionary<string, object>> onCustomAuthenticationResponse = this.OnCustomAuthenticationResponse;
		if (onCustomAuthenticationResponse == null)
		{
			return;
		}
		onCustomAuthenticationResponse(response);
	}

	public virtual void Initialise()
	{
		Debug.Log("INITIALISING NETWORKSYSTEMS");
		if (NetworkSystem.Instance)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		NetworkSystem.Instance = this;
		NetCrossoverUtils.Prewarm();
	}

	protected virtual void Update()
	{
	}

	public void RegisterSceneNetworkItem(GameObject item)
	{
		if (!this.SceneObjectsToAttach.Contains(item))
		{
			this.SceneObjectsToAttach.Add(item);
		}
	}

	public virtual void AttachObjectInGame(GameObject item)
	{
		this.RegisterSceneNetworkItem(item);
	}

	public virtual void DetatchSceneObjectInGame(GameObject item)
	{
	}

	public virtual AuthenticationValues GetAuthenticationValues()
	{
		Debug.LogWarning("NetworkSystem.GetAuthenticationValues should be overridden");
		return new AuthenticationValues();
	}

	public virtual void SetAuthenticationValues(AuthenticationValues authValues)
	{
		Debug.LogWarning("NetworkSystem.SetAuthenticationValues should be overridden");
	}

	public abstract void FinishAuthenticating();

	public abstract Task<NetJoinResult> ConnectToRoom(string roomName, RoomConfig opts, int regionIndex = -1);

	public abstract Task JoinFriendsRoom(string userID, int actorID, string keyToFollow, string shufflerToFollow);

	public abstract Task ReturnToSinglePlayer();

	public abstract void JoinPubWithFriends();

	public bool WrongVersion
	{
		get
		{
			return this.isWrongVersion;
		}
	}

	public void SetWrongVersion()
	{
		this.isWrongVersion = true;
	}

	public GameObject NetInstantiate(GameObject prefab, bool isRoomObject = false)
	{
		return this.NetInstantiate(prefab, Vector3.zero, Quaternion.identity, false);
	}

	public GameObject NetInstantiate(GameObject prefab, Vector3 position, bool isRoomObject = false)
	{
		return this.NetInstantiate(prefab, position, Quaternion.identity, false);
	}

	public abstract GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isRoomObject = false);

	public abstract GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, int playerAuthID, bool isRoomObject = false);

	public abstract GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isRoomObject, byte group = 0, object[] data = null, NetworkRunner.OnBeforeSpawned callback = null);

	public abstract void SetPlayerObject(GameObject playerInstance, int? owningPlayerID = null);

	public abstract void NetDestroy(GameObject instance);

	public abstract void CallRPC(MonoBehaviour component, NetworkSystem.RPC rpcMethod, bool sendToSelf = true);

	public abstract void CallRPC<T>(MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args, bool sendToSelf = true) where T : struct;

	public abstract void CallRPC(MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message, bool sendToSelf = true);

	public abstract void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod);

	public abstract void CallRPC<T>(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args) where T : struct;

	public abstract void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message);

	public static string GetRandomRoomName()
	{
		string text = "";
		for (int i = 0; i < 4; i++)
		{
			text += "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Substring(Random.Range(0, "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Length), 1);
		}
		if (GorillaComputer.instance.IsPlayerInVirtualStump())
		{
			text = GorillaComputer.instance.VStumpRoomPrepend + text;
		}
		if (GorillaComputer.instance.CheckAutoBanListForName(text))
		{
			return text;
		}
		return NetworkSystem.GetRandomRoomName();
	}

	public abstract string GetRandomWeightedRegion();

	protected async Task RefreshNonce()
	{
		Debug.Log("Refreshing Nonce Token.");
		this.nonceRefreshed = false;
		PlayFabAuthenticator.instance.RefreshSteamAuthTicketForPhoton(new Action<string>(this.GetSteamAuthTicketSuccessCallback), new Action<EResult>(this.GetSteamAuthTicketFailureCallback));
		while (!this.nonceRefreshed)
		{
			await Task.Yield();
		}
		Debug.Log("New Nonce Token acquired");
	}

	private void GetSteamAuthTicketSuccessCallback(string ticket)
	{
		AuthenticationValues authenticationValues = this.GetAuthenticationValues();
		Dictionary<string, object> dictionary = ((authenticationValues != null) ? authenticationValues.AuthPostData : null) as Dictionary<string, object>;
		if (dictionary != null)
		{
			dictionary["Nonce"] = ticket;
			authenticationValues.SetAuthPostData(dictionary);
			this.SetAuthenticationValues(authenticationValues);
			this.nonceRefreshed = true;
		}
	}

	private void GetSteamAuthTicketFailureCallback(EResult result)
	{
		base.StartCoroutine(this.ReGetNonce());
	}

	private IEnumerator ReGetNonce()
	{
		yield return new WaitForSeconds(3f);
		PlayFabAuthenticator.instance.RefreshSteamAuthTicketForPhoton(new Action<string>(this.GetSteamAuthTicketSuccessCallback), new Action<EResult>(this.GetSteamAuthTicketFailureCallback));
		yield return null;
		yield break;
	}

	public void BroadcastMyRoom(bool create, string key, string shuffler)
	{
		string text = NetworkSystem.ShuffleRoomName(NetworkSystem.Instance.RoomName, shuffler.Substring(2, 8), true) + "|" + NetworkSystem.ShuffleRoomName("ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Substring(NetworkSystem.Instance.currentRegionIndex, 1), shuffler.Substring(0, 2), true);
		Debug.Log(string.Format("Broadcasting room {0} region {1}({2}). Create: {3} key: {4} (shuffler {5}) shuffled: {6}", new object[]
		{
			NetworkSystem.Instance.RoomName,
			NetworkSystem.Instance.currentRegionIndex,
			NetworkSystem.Instance.regionNames[NetworkSystem.Instance.currentRegionIndex],
			create,
			key,
			shuffler,
			text
		}));
		GorillaServer instance = GorillaServer.Instance;
		BroadcastMyRoomRequest broadcastMyRoomRequest = new BroadcastMyRoomRequest();
		broadcastMyRoomRequest.KeyToFollow = key;
		broadcastMyRoomRequest.RoomToJoin = text;
		broadcastMyRoomRequest.Set = create;
		instance.BroadcastMyRoom(broadcastMyRoomRequest, delegate(ExecuteFunctionResult result)
		{
		}, delegate(PlayFabError error)
		{
		});
	}

	public bool InstantCheckGroupData(string userID, string keyToFollow)
	{
		bool success = false;
		global::PlayFab.ClientModels.GetSharedGroupDataRequest getSharedGroupDataRequest = new global::PlayFab.ClientModels.GetSharedGroupDataRequest();
		getSharedGroupDataRequest.Keys = new List<string> { keyToFollow };
		getSharedGroupDataRequest.SharedGroupId = userID;
		PlayFabClientAPI.GetSharedGroupData(getSharedGroupDataRequest, delegate(GetSharedGroupDataResult result)
		{
			Debug.Log("Get Shared Group Data returned a success");
			Debug.Log(result.Data.ToStringFull());
			if (result.Data.Count > 0)
			{
				success = true;
				return;
			}
			Debug.Log("RESULT returned but no DATA");
		}, delegate(PlayFabError error)
		{
			Debug.Log("ERROR - no group data found");
		}, null, null);
		return success;
	}

	public NetPlayer GetNetPlayerByID(int playerActorNumber)
	{
		return this.netPlayerCache.Find((NetPlayer a) => a.ActorNumber == playerActorNumber);
	}

	public virtual void NetRaiseEventReliable(byte eventCode, object data)
	{
	}

	public virtual void NetRaiseEventUnreliable(byte eventCode, object data)
	{
	}

	public virtual void NetRaiseEventReliable(byte eventCode, object data, NetEventOptions options)
	{
	}

	public virtual void NetRaiseEventUnreliable(byte eventCode, object data, NetEventOptions options)
	{
	}

	public static string ShuffleRoomName(string room, string shuffle, bool encode)
	{
		NetworkSystem.shuffleStringBuilder.Clear();
		int num;
		if (!int.TryParse(shuffle, out num))
		{
			Debug.Log("Shuffle room failed");
			return "";
		}
		for (int i = 0; i < room.Length; i++)
		{
			int num2 = int.Parse(shuffle.Substring(i * 2 % (shuffle.Length - 1), 2));
			int num3 = NetworkSystem.mod("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".IndexOf(room[i]) + (encode ? num2 : (-num2)), "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Length);
			NetworkSystem.shuffleStringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"[num3]);
		}
		return NetworkSystem.shuffleStringBuilder.ToString();
	}

	public static int mod(int x, int m)
	{
		return (x % m + m) % m;
	}

	public abstract Task AwaitSceneReady();

	public abstract string CurrentPhotonBackend { get; }

	public abstract NetPlayer GetLocalPlayer();

	public abstract NetPlayer GetPlayer(int PlayerID);

	public NetPlayer GetPlayer(Player punPlayer)
	{
		if (punPlayer == null)
		{
			return null;
		}
		NetPlayer netPlayer = this.FindPlayer(punPlayer);
		if (netPlayer == null)
		{
			this.UpdatePlayers();
			netPlayer = this.FindPlayer(punPlayer);
			if (netPlayer == null)
			{
				Debug.LogError(string.Format("There is no NetPlayer with this ID currently in game. Passed ID: {0} nickname {1}", punPlayer.ActorNumber, punPlayer.NickName));
				return null;
			}
		}
		return netPlayer;
	}

	private NetPlayer FindPlayer(Player punPlayer)
	{
		for (int i = 0; i < this.netPlayerCache.Count; i++)
		{
			if (this.netPlayerCache[i].GetPlayerRef() == punPlayer)
			{
				return this.netPlayerCache[i];
			}
		}
		return null;
	}

	public NetPlayer GetPlayer(PlayerRef playerRef)
	{
		return null;
	}

	public abstract void SetMyNickName(string name);

	public abstract string GetMyNickName();

	public abstract string GetMyDefaultName();

	public abstract string GetNickName(int playerID);

	public abstract string GetNickName(NetPlayer player);

	public abstract string GetMyUserID();

	public abstract string GetUserID(int playerID);

	public abstract string GetUserID(NetPlayer player);

	public abstract void SetMyTutorialComplete();

	public abstract bool GetMyTutorialCompletion();

	public abstract bool GetPlayerTutorialCompletion(int playerID);

	public void AddVoiceSettings(SO_NetworkVoiceSettings settings)
	{
		this.VoiceSettings = settings;
	}

	public abstract void AddRemoteVoiceAddedCallback(Action<RemoteVoiceLink> callback);

	public abstract VoiceConnection VoiceConnection { get; }

	public abstract bool IsOnline { get; }

	public abstract bool InRoom { get; }

	public abstract string RoomName { get; }

	public abstract string RoomStringStripped();

	public string RoomString()
	{
		return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", new object[]
		{
			this.RoomName,
			this.CurrentRoom.isPublic ? "visible" : "hidden",
			this.CurrentRoom.isJoinable ? "open" : "closed",
			this.CurrentRoom.MaxPlayers,
			this.RoomPlayerCount,
			this.CurrentRoom.CustomProps.ToStringFull()
		});
	}

	public abstract string GameModeString { get; }

	public abstract string CurrentRegion { get; }

	public abstract bool SessionIsPrivate { get; }

	public abstract int LocalPlayerID { get; }

	public virtual NetPlayer[] AllNetPlayers
	{
		get
		{
			return this.netPlayerCache.ToArray();
		}
	}

	public virtual NetPlayer[] PlayerListOthers
	{
		get
		{
			return this.netPlayerCache.FindAll((NetPlayer p) => !p.IsLocal).ToArray();
		}
	}

	protected abstract void UpdateNetPlayerList();

	public void UpdatePlayers()
	{
		this.UpdateNetPlayerList();
	}

	public abstract double SimTime { get; }

	public abstract float SimDeltaTime { get; }

	public abstract int SimTick { get; }

	public abstract int TickRate { get; }

	public abstract int ServerTimestamp { get; }

	public abstract int RoomPlayerCount { get; }

	public abstract int GlobalPlayerCount();

	public RoomConfig CurrentRoom { get; protected set; }

	public abstract bool IsObjectLocallyOwned(GameObject obj);

	public abstract bool IsObjectRoomObject(GameObject obj);

	public abstract bool ShouldUpdateObject(GameObject obj);

	public abstract bool ShouldWriteObjectData(GameObject obj);

	public abstract int GetOwningPlayerID(GameObject obj);

	public abstract bool ShouldSpawnLocally(int playerID);

	public abstract bool IsTotalAuthority();

	public static NetworkSystem Instance;

	public NetworkSystemConfig config;

	public bool changingSceneManually;

	public string[] regionNames;

	public int currentRegionIndex;

	private bool nonceRefreshed;

	protected bool isWrongVersion;

	private NetSystemState testState;

	protected List<NetPlayer> netPlayerCache = new List<NetPlayer>();

	protected Recorder localRecorder;

	protected Speaker localSpeaker;

	public List<GameObject> SceneObjectsToAttach = new List<GameObject>();

	protected SO_NetworkVoiceSettings VoiceSettings;

	protected List<Action<RemoteVoiceLink>> remoteVoiceAddedCallbacks = new List<Action<RemoteVoiceLink>>();

	public DelegateListProcessor OnJoinedRoomEvent = new DelegateListProcessor();

	public DelegateListProcessor OnMultiplayerStarted = new DelegateListProcessor();

	public DelegateListProcessor OnReturnedToSinglePlayer = new DelegateListProcessor();

	public DelegateListProcessor<NetPlayer> OnPlayerJoined = new DelegateListProcessor<NetPlayer>();

	public DelegateListProcessor<NetPlayer> OnPlayerLeft = new DelegateListProcessor<NetPlayer>();

	internal DelegateListProcessor<NetPlayer> OnMasterClientSwitchedEvent = new DelegateListProcessor<NetPlayer>();

	protected static readonly byte[] EmptyArgs = new byte[0];

	public const string roomCharacters = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";

	public const string shuffleCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

	private static StringBuilder shuffleStringBuilder = new StringBuilder(4);

	protected static StringBuilder reusableSB = new StringBuilder();

	public delegate void RPC(byte[] data);

	public delegate void StringRPC(string message);

	public delegate void StaticRPC(byte[] data);

	public delegate void StaticRPCPlaceholder(byte[] args);
}
