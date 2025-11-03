using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Fusion;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public abstract class GorillaGameManager : MonoBehaviourPunCallbacks, ITickSystemTick, IWrappedSerializable, INetworkStruct
{
	public static string GameModeEnumToName(GameModeType gameMode)
	{
		return gameMode.ToString();
	}

	public static event GorillaGameManager.OnTouchDelegate OnTouch;

	public static GorillaGameManager instance
	{
		get
		{
			return global::GorillaGameModes.GameMode.ActiveGameMode;
		}
	}

	bool ITickSystemTick.TickRunning { get; set; }

	public virtual void Awake()
	{
	}

	private new void OnEnable()
	{
	}

	private new void OnDisable()
	{
	}

	public virtual void Tick()
	{
		if (this.lastCheck + this.checkCooldown < Time.time)
		{
			this.lastCheck = Time.time;
			if (NetworkSystem.Instance.IsMasterClient && !this.ValidGameMode())
			{
				global::GorillaGameModes.GameMode.ChangeGameFromProperty();
				return;
			}
			this.InfrequentUpdate();
		}
	}

	public virtual void InfrequentUpdate()
	{
		global::GorillaGameModes.GameMode.RefreshPlayers();
		this.currentNetPlayerArray = NetworkSystem.Instance.AllNetPlayers;
	}

	public virtual string GameModeName()
	{
		if (this._gameModeName == null)
		{
			this._gameModeName = this.GameType().ToString().ToUpper();
		}
		return this._gameModeName;
	}

	public virtual string GameModeNameRoomLabel()
	{
		string text;
		if (!LocalisationManager.TryGetKeyForCurrentLocale("GAME_MODE_NONE_ROOM_LABEL", out text, "(NONE GAME)"))
		{
			Debug.LogError("[LOCALIZATION::GORILLA_GAME_MANAGER] Failed to get key for Game Mode [GAME_MODE_NONE_ROOM_LABEL]");
		}
		return text;
	}

	public virtual void LocalTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer, bool bodyHit, bool leftHand)
	{
	}

	public virtual void ReportTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
	}

	public virtual void HitPlayer(NetPlayer player)
	{
	}

	public virtual bool CanAffectPlayer(NetPlayer player, bool thisFrame)
	{
		return false;
	}

	public virtual void HandleHandTap(NetPlayer tappingPlayer, Tappable hitTappable, bool leftHand, Vector3 handVelocity, Vector3 tapSurfaceNormal)
	{
	}

	public virtual bool CanJoinFrienship(NetPlayer player)
	{
		return true;
	}

	public virtual bool CanPlayerParticipate(NetPlayer player)
	{
		return true;
	}

	public virtual void HandleRoundComplete()
	{
		PlayerGameEvents.GameModeCompleteRound();
	}

	public virtual void HandleTagBroadcast(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
	}

	public virtual void HandleTagBroadcast(NetPlayer taggedPlayer, NetPlayer taggingPlayer, double tagTime)
	{
	}

	public virtual void NewVRRig(NetPlayer player, int vrrigPhotonViewID, bool didTutorial)
	{
	}

	public virtual bool LocalCanTag(NetPlayer myPlayer, NetPlayer otherPlayer)
	{
		return false;
	}

	public virtual bool LocalIsTagged(NetPlayer player)
	{
		return false;
	}

	public virtual VRRig FindPlayerVRRig(NetPlayer player)
	{
		RigContainer rigContainer;
		if (player != null && VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			return rigContainer.Rig;
		}
		return null;
	}

	public static VRRig StaticFindRigForPlayer(NetPlayer player)
	{
		VRRig vrrig = null;
		RigContainer rigContainer;
		if (GorillaGameManager.instance != null)
		{
			vrrig = GorillaGameManager.instance.FindPlayerVRRig(player);
		}
		else if (VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			vrrig = rigContainer.Rig;
		}
		return vrrig;
	}

	public virtual float[] LocalPlayerSpeed()
	{
		this.playerSpeed[0] = this.slowJumpLimit;
		this.playerSpeed[1] = this.slowJumpMultiplier;
		return this.playerSpeed;
	}

	public virtual void UpdatePlayerAppearance(VRRig rig)
	{
		ScienceExperimentManager instance = ScienceExperimentManager.instance;
		int num;
		if (instance != null && instance.GetMaterialIfPlayerInGame(rig.creator.ActorNumber, out num))
		{
			rig.ChangeMaterialLocal(num);
			return;
		}
		int num2 = this.MyMatIndex(rig.creator);
		rig.ChangeMaterialLocal(num2);
	}

	public virtual int MyMatIndex(NetPlayer forPlayer)
	{
		return 0;
	}

	public virtual int SpecialHandFX(NetPlayer player, RigContainer rigContainer)
	{
		return -1;
	}

	public virtual bool ValidGameMode()
	{
		return NetworkSystem.Instance.InRoom && ((NetworkSystem.Instance.SessionIsPrivate && RoomSystem.IsVStumpRoom) || NetworkSystem.Instance.GameModeString.Contains(this.GameTypeName()));
	}

	public static void OnInstanceReady(Action action)
	{
		GorillaParent.OnReplicatedClientReady(delegate
		{
			if (GorillaGameManager.instance)
			{
				action();
				return;
			}
			GorillaGameManager.onInstanceReady = (Action)Delegate.Combine(GorillaGameManager.onInstanceReady, action);
		});
	}

	public static void ReplicatedClientReady()
	{
		GorillaGameManager.replicatedClientReady = true;
	}

	public static void OnReplicatedClientReady(Action action)
	{
		if (GorillaGameManager.replicatedClientReady)
		{
			action();
			return;
		}
		GorillaGameManager.onReplicatedClientReady = (Action)Delegate.Combine(GorillaGameManager.onReplicatedClientReady, action);
	}

	internal GameModeSerializer Serializer
	{
		get
		{
			return this.serializer;
		}
	}

	internal virtual void NetworkLinkSetup(GameModeSerializer netSerializer)
	{
		this.serializer = netSerializer;
	}

	internal virtual void NetworkLinkDestroyed(GameModeSerializer netSerializer)
	{
		if (this.serializer == netSerializer)
		{
			this.serializer = null;
		}
	}

	public abstract GameModeType GameType();

	public string GameTypeName()
	{
		return this.GameType().ToString();
	}

	public abstract void AddFusionDataBehaviour(NetworkObject behaviour);

	public abstract void OnSerializeRead(object newData);

	public abstract object OnSerializeWrite();

	public abstract void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info);

	public abstract void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info);

	public virtual void ResetGame()
	{
	}

	public virtual void StartPlaying()
	{
		TickSystem<object>.AddTickCallback(this);
		NetworkSystem.Instance.OnPlayerJoined += this.OnPlayerEnteredRoom;
		NetworkSystem.Instance.OnPlayerLeft += this.OnPlayerLeftRoom;
		NetworkSystem.Instance.OnMasterClientSwitchedEvent += this.OnMasterClientSwitched;
		this.currentNetPlayerArray = NetworkSystem.Instance.AllNetPlayers;
		GorillaTelemetry.PostGameModeEvent(GTGameModeEventType.game_mode_start, this.GameType());
	}

	public virtual void StopPlaying()
	{
		TickSystem<object>.RemoveTickCallback(this);
		NetworkSystem.Instance.OnPlayerJoined -= this.OnPlayerEnteredRoom;
		NetworkSystem.Instance.OnPlayerLeft -= this.OnPlayerLeftRoom;
		NetworkSystem.Instance.OnMasterClientSwitchedEvent -= this.OnMasterClientSwitched;
		this.lastCheck = 0f;
	}

	public new virtual void OnMasterClientSwitched(Player newMaster)
	{
	}

	public new virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	public new virtual void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public virtual void OnPlayerLeftRoom(NetPlayer otherPlayer)
	{
		this.currentNetPlayerArray = NetworkSystem.Instance.AllNetPlayers;
		if (this.lastTaggedActorNr.ContainsKey(otherPlayer.ActorNumber))
		{
			this.lastTaggedActorNr.Remove(otherPlayer.ActorNumber);
		}
	}

	public virtual void OnPlayerEnteredRoom(NetPlayer newPlayer)
	{
		this.currentNetPlayerArray = NetworkSystem.Instance.AllNetPlayers;
	}

	public virtual void OnMasterClientSwitched(NetPlayer newMaster)
	{
	}

	internal static void ForceStopGame_DisconnectAndDestroy()
	{
		Application.Quit();
		NetworkSystem instance = NetworkSystem.Instance;
		if (instance != null)
		{
			instance.ReturnToSinglePlayer();
		}
		Object.DestroyImmediate(PhotonNetworkController.Instance);
		Object.DestroyImmediate(GTPlayer.Instance);
		GameObject[] array = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
	}

	public void AddLastTagged(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		if (this.lastTaggedActorNr.ContainsKey(taggedPlayer.ActorNumber))
		{
			this.lastTaggedActorNr[taggedPlayer.ActorNumber] = taggingPlayer.ActorNumber;
			return;
		}
		this.lastTaggedActorNr.Add(taggedPlayer.ActorNumber, taggingPlayer.ActorNumber);
	}

	public void WriteLastTagged(PhotonStream stream)
	{
		stream.SendNext(this.lastTaggedActorNr.Count);
		foreach (KeyValuePair<int, int> keyValuePair in this.lastTaggedActorNr)
		{
			stream.SendNext(keyValuePair.Key);
			stream.SendNext(keyValuePair.Value);
		}
	}

	public void ReadLastTagged(PhotonStream stream)
	{
		this.lastTaggedActorNr.Clear();
		int num = Mathf.Min((int)stream.ReceiveNext(), 10);
		for (int i = 0; i < num; i++)
		{
			this.lastTaggedActorNr.Add((int)stream.ReceiveNext(), (int)stream.ReceiveNext());
		}
	}

	protected const string GAME_MODE_NONE_KEY = "GAME_MODE_NONE";

	protected const string GAME_MODE_CASUAL_ROOM_LABEL_KEY = "GAME_MODE_CASUAL_ROOM_LABEL";

	protected const string GAME_MODE_INFECTION_ROOM_LABEL_KEY = "GAME_MODE_INFECTION_ROOM_LABEL";

	protected const string GAME_MODE_HUNT_ROOM_LABEL_KEY = "GAME_MODE_HUNT_ROOM_LABEL";

	protected const string GAME_MODE_PAINTBRAWL_ROOM_LABEL_KEY = "GAME_MODE_PAINTBRAWL_ROOM_LABEL";

	protected const string GAME_MODE_SUPER_INFECTION_ROOM_LABEL_KEY = "GAME_MODE_SUPER_INFECTION_ROOM_LABEL";

	protected const string GAME_MODE_NONE_ROOM_LABEL_KEY = "GAME_MODE_NONE_ROOM_LABEL";

	protected const string GAME_MODE_CUSTOM_ROOM_LABEL_KEY = "GAME_MODE_CUSTOM_ROOM_LABEL";

	protected const string GAME_MODE_GHOST_ROOM_LABEL_KEY = "GAME_MODE_GHOST_ROOM_LABEL";

	protected const string GAME_MODE_AMBUSH_ROOM_LABEL_KEY = "GAME_MODE_AMBUSH_ROOM_LABEL";

	protected const string GAME_MODE_FREEZE_TAG_ROOM_LABEL_KEY = "GAME_MODE_FREEZE_TAG_ROOM_LABEL";

	protected const string GAME_MODE_GUARDIAN_ROOM_LABEL_KEY = "GAME_MODE_GUARDIAN_ROOM_LABEL";

	protected const string GAME_MODE_PROP_HUNT_ROOM_LABEL_KEY = "GAME_MODE_PROP_HUNT_ROOM_LABEL";

	protected const string GAME_MODE_COMP_INF_ROOM_LABEL_KEY = "GAME_MODE_COMP_INF_ROOM_LABEL";

	public const int k_defaultMatIndex = 0;

	public float fastJumpLimit;

	public float fastJumpMultiplier;

	public float slowJumpLimit;

	public float slowJumpMultiplier;

	public float lastCheck;

	public float checkCooldown = 3f;

	public float tagDistanceThreshold = 4f;

	private NetPlayer outPlayer;

	private int outInt;

	private VRRig tempRig;

	public NetPlayer[] currentNetPlayerArray;

	public float[] playerSpeed = new float[2];

	public Dictionary<int, int> lastTaggedActorNr = new Dictionary<int, int>();

	private string _gameModeName;

	private static Action onInstanceReady;

	private static bool replicatedClientReady;

	private static Action onReplicatedClientReady;

	private GameModeSerializer serializer;

	public delegate void OnTouchDelegate(NetPlayer taggedPlayer, NetPlayer taggingPlayer);
}
