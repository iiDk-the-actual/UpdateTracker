using System;
using System.Collections.Generic;
using System.Timers;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.Cosmetics;
using GorillaTagScripts;
using Photon.Pun;
using TagEffects;
using UnityEngine;

internal class RoomSystem : MonoBehaviour
{
	internal static void DeserializeLaunchProjectile(object[] projectileData, PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		GorillaNot.IncrementRPCCall(info, "LaunchSlingshotProjectile");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			return;
		}
		byte b = Convert.ToByte(projectileData[5]);
		byte b2 = Convert.ToByte(projectileData[6]);
		byte b3 = Convert.ToByte(projectileData[7]);
		byte b4 = Convert.ToByte(projectileData[8]);
		Color32 color = new Color32(b, b2, b3, b4);
		Vector3 vector = (Vector3)projectileData[0];
		Vector3 vector2 = (Vector3)projectileData[1];
		float num = 10000f;
		if ((in vector).IsValid(in num))
		{
			float num2 = 10000f;
			if ((in vector2).IsValid(in num2) && float.IsFinite((float)b) && float.IsFinite((float)b2) && float.IsFinite((float)b3) && float.IsFinite((float)b4))
			{
				RoomSystem.ProjectileSource projectileSource = (RoomSystem.ProjectileSource)Convert.ToInt32(projectileData[2]);
				int num3 = Convert.ToInt32(projectileData[3]);
				bool flag = Convert.ToBoolean(projectileData[4]);
				VRRig rig = rigContainer.Rig;
				if (rig.isOfflineVRRig || rig.IsPositionInRange(vector, 4f))
				{
					RoomSystem.launchProjectile.targetRig = rig;
					RoomSystem.launchProjectile.position = vector;
					RoomSystem.launchProjectile.velocity = vector2;
					RoomSystem.launchProjectile.overridecolour = flag;
					RoomSystem.launchProjectile.colour = color;
					RoomSystem.launchProjectile.projectileIndex = num3;
					RoomSystem.launchProjectile.projectileSource = projectileSource;
					RoomSystem.launchProjectile.messageInfo = info;
					FXSystem.PlayFXForRig(FXType.Projectile, RoomSystem.launchProjectile, info);
				}
				return;
			}
		}
		GorillaNot.instance.SendReport("invalid projectile state", player.UserId, player.NickName);
	}

	internal static void SendLaunchProjectile(Vector3 position, Vector3 velocity, RoomSystem.ProjectileSource projectileSource, int projectileCount, bool randomColour, byte r, byte g, byte b, byte a)
	{
		if (!RoomSystem.JoinedRoom)
		{
			return;
		}
		RoomSystem.projectileSendData[0] = position;
		RoomSystem.projectileSendData[1] = velocity;
		RoomSystem.projectileSendData[2] = projectileSource;
		RoomSystem.projectileSendData[3] = projectileCount;
		RoomSystem.projectileSendData[4] = randomColour;
		RoomSystem.projectileSendData[5] = r;
		RoomSystem.projectileSendData[6] = g;
		RoomSystem.projectileSendData[7] = b;
		RoomSystem.projectileSendData[8] = a;
		byte b2 = 0;
		object obj = RoomSystem.projectileSendData;
		RoomSystem.SendEvent(in b2, in obj, in NetworkSystemRaiseEvent.neoOthers, false);
	}

	internal static void ImpactEffect(VRRig targetRig, Vector3 position, float r, float g, float b, float a, int projectileCount, PhotonMessageInfoWrapped info = default(PhotonMessageInfoWrapped))
	{
		RoomSystem.impactEffect.targetRig = targetRig;
		RoomSystem.impactEffect.position = position;
		RoomSystem.impactEffect.colour = new Color(r, g, b, a);
		RoomSystem.impactEffect.projectileIndex = projectileCount;
		FXSystem.PlayFXForRig(FXType.Impact, RoomSystem.impactEffect, info);
	}

	internal static void DeserializeImpactEffect(object[] impactData, PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		GorillaNot.IncrementRPCCall(info, "SpawnSlingshotPlayerImpactEffect");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || rigContainer.Rig.projectileWeapon.IsNull())
		{
			return;
		}
		float num = Convert.ToSingle(impactData[1]);
		float num2 = Convert.ToSingle(impactData[2]);
		float num3 = Convert.ToSingle(impactData[3]);
		float num4 = Convert.ToSingle(impactData[4]);
		Vector3 vector = (Vector3)impactData[0];
		float num5 = 10000f;
		if (!(in vector).IsValid(in num5) || !float.IsFinite(num) || !float.IsFinite(num2) || !float.IsFinite(num3) || !float.IsFinite(num4))
		{
			GorillaNot.instance.SendReport("invalid impact state", player.UserId, player.NickName);
			return;
		}
		int num6 = Convert.ToInt32(impactData[5]);
		RoomSystem.ImpactEffect(rigContainer.Rig, vector, num, num2, num3, num4, num6, info);
	}

	internal static void SendImpactEffect(Vector3 position, float r, float g, float b, float a, int projectileCount)
	{
		RoomSystem.ImpactEffect(VRRigCache.Instance.localRig.Rig, position, r, g, b, a, projectileCount, default(PhotonMessageInfoWrapped));
		if (RoomSystem.joinedRoom)
		{
			RoomSystem.impactSendData[0] = position;
			RoomSystem.impactSendData[1] = r;
			RoomSystem.impactSendData[2] = g;
			RoomSystem.impactSendData[3] = b;
			RoomSystem.impactSendData[4] = a;
			RoomSystem.impactSendData[5] = projectileCount;
			byte b2 = 1;
			object obj = RoomSystem.impactSendData;
			RoomSystem.SendEvent(in b2, in obj, in NetworkSystemRaiseEvent.neoOthers, false);
		}
	}

	private void Awake()
	{
		base.transform.SetParent(null, true);
		Object.DontDestroyOnLoad(this);
		RoomSystem.playerImpactEffectPrefab = this.roomSettings.PlayerImpactEffect;
		RoomSystem.callbackInstance = this;
		RoomSystem.disconnectTimer.Interval = (double)(this.roomSettings.PausedDCTimer * 1000);
		RoomSystem.playerEffectDictionary.Clear();
		foreach (RoomSystem.PlayerEffectConfig playerEffectConfig in this.roomSettings.PlayerEffects)
		{
			RoomSystem.playerEffectDictionary.Add(playerEffectConfig.type, playerEffectConfig);
		}
		this.roomSettings.ResyncNetworkTimeTimer.callback = new Action(PhotonNetwork.FetchServerTimestamp);
	}

	private void Start()
	{
		List<PhotonView> list = new List<PhotonView>(20);
		foreach (PhotonView photonView in PhotonNetwork.PhotonViewCollection)
		{
			if (photonView.IsRoomView)
			{
				list.Add(photonView);
			}
		}
		RoomSystem.sceneViews = list.ToArray();
		NetworkSystem.Instance.OnRaiseEvent += RoomSystem.OnEvent;
		NetworkSystem.Instance.OnPlayerLeft += this.OnPlayerLeftRoom;
		NetworkSystem.Instance.OnPlayerJoined += this.OnPlayerEnteredRoom;
		NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
		NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnLeftRoom;
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			RoomSystem.disconnectTimer.Stop();
			return;
		}
		if (RoomSystem.JoinedRoom)
		{
			RoomSystem.disconnectTimer.Start();
		}
	}

	private void OnJoinedRoom()
	{
		RoomSystem.joinedRoom = true;
		foreach (NetPlayer netPlayer in NetworkSystem.Instance.AllNetPlayers)
		{
			RoomSystem.netPlayersInRoom.Add(netPlayer);
		}
		PlayerCosmeticsSystem.UpdatePlayerCosmetics(RoomSystem.netPlayersInRoom);
		RoomSystem.roomGameMode = NetworkSystem.Instance.GameModeString;
		RoomSystem.WasRoomPrivate = NetworkSystem.Instance.SessionIsPrivate;
		RoomSystem.IsVStumpRoom = NetworkSystem.Instance.RoomName.StartsWith(GorillaComputer.instance.VStumpRoomPrepend);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			for (int j = 0; j < this.prefabsToInstantiateByPath.Length; j++)
			{
				this.prefabsInstantiated.Add(NetworkSystem.Instance.NetInstantiate(this.prefabsToInstantiate[j], Vector3.zero, Quaternion.identity, true));
			}
		}
		try
		{
			RoomSystem.m_roomSizeOnJoin = PhotonNetwork.CurrentRoom.MaxPlayers;
			this.roomSettings.ExpectedUsersTimer.Start();
			this.roomSettings.ResyncNetworkTimeTimer.Start();
			DelegateListProcessor joinedRoomEvent = RoomSystem.JoinedRoomEvent;
			if (joinedRoomEvent != null)
			{
				joinedRoomEvent.InvokeSafe();
			}
			this.roomSettings.ResyncNetworkTimeTimer.OnTimedEvent();
		}
		catch (Exception)
		{
			Debug.LogError("RoomSystem failed invoking event");
		}
	}

	private void OnPlayerEnteredRoom(NetPlayer newPlayer)
	{
		if (newPlayer.IsLocal)
		{
			return;
		}
		if (!RoomSystem.netPlayersInRoom.Contains(newPlayer))
		{
			RoomSystem.netPlayersInRoom.Add(newPlayer);
		}
		PlayerCosmeticsSystem.UpdatePlayerCosmetics(newPlayer);
		try
		{
			DelegateListProcessor<NetPlayer> playerJoinedEvent = RoomSystem.PlayerJoinedEvent;
			if (playerJoinedEvent != null)
			{
				playerJoinedEvent.InvokeSafe(in newPlayer);
			}
			DelegateListProcessor playersChangedEvent = RoomSystem.PlayersChangedEvent;
			if (playersChangedEvent != null)
			{
				playersChangedEvent.InvokeSafe();
			}
		}
		catch (Exception)
		{
			Debug.LogError("RoomSystem failed invoking event");
		}
	}

	private void OnLeftRoom()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		RoomSystem.joinedRoom = false;
		RoomSystem.netPlayersInRoom.Clear();
		RoomSystem.roomGameMode = "";
		PlayerCosmeticsSystem.StaticReset();
		int actorNumber = NetworkSystem.Instance.LocalPlayer.ActorNumber;
		for (int i = 0; i < RoomSystem.sceneViews.Length; i++)
		{
			RoomSystem.sceneViews[i].ControllerActorNr = actorNumber;
			RoomSystem.sceneViews[i].OwnerActorNr = actorNumber;
		}
		this.roomSettings.StatusEffectLimiter.Reset();
		this.roomSettings.SoundEffectLimiter.Reset();
		this.roomSettings.SoundEffectOtherLimiter.Reset();
		this.roomSettings.PlayerEffectLimiter.Reset();
		try
		{
			RoomSystem.m_roomSizeOnJoin = 0;
			this.roomSettings.ExpectedUsersTimer.Stop();
			this.roomSettings.ResyncNetworkTimeTimer.Stop();
			DelegateListProcessor leftRoomEvent = RoomSystem.LeftRoomEvent;
			if (leftRoomEvent != null)
			{
				leftRoomEvent.InvokeSafe();
			}
		}
		catch (Exception)
		{
			Debug.LogError("RoomSystem failed invoking event");
		}
		GC.Collect(0);
	}

	private void OnPlayerLeftRoom(NetPlayer netPlayer)
	{
		if (netPlayer == null)
		{
			Debug.LogError("Player how left doesnt have a reference somehow");
		}
		foreach (NetPlayer netPlayer2 in RoomSystem.netPlayersInRoom)
		{
			if (netPlayer2 == netPlayer)
			{
				RoomSystem.netPlayersInRoom.Remove(netPlayer2);
				break;
			}
		}
		RoomSystem.netPlayersInRoom.Remove(netPlayer);
		try
		{
			DelegateListProcessor<NetPlayer> playerLeftEvent = RoomSystem.PlayerLeftEvent;
			if (playerLeftEvent != null)
			{
				playerLeftEvent.InvokeSafe(in netPlayer);
			}
			DelegateListProcessor playersChangedEvent = RoomSystem.PlayersChangedEvent;
			if (playersChangedEvent != null)
			{
				playersChangedEvent.InvokeSafe();
			}
		}
		catch (Exception)
		{
			Debug.LogError("RoomSystem failed invoking event");
		}
	}

	private static bool UseRoomSizeOverride { get; set; }

	public static byte RoomSizeOverride { get; set; }

	public static List<NetPlayer> PlayersInRoom
	{
		get
		{
			return RoomSystem.netPlayersInRoom;
		}
	}

	public static string RoomGameMode
	{
		get
		{
			return RoomSystem.roomGameMode;
		}
	}

	public static bool JoinedRoom
	{
		get
		{
			return NetworkSystem.Instance.InRoom && RoomSystem.joinedRoom;
		}
	}

	public static bool AmITheHost
	{
		get
		{
			return NetworkSystem.Instance.IsMasterClient || !NetworkSystem.Instance.InRoom;
		}
	}

	public static bool IsVStumpRoom { get; private set; }

	public static bool WasRoomPrivate { get; private set; }

	static RoomSystem()
	{
		RoomSystem.disconnectTimer.Elapsed += RoomSystem.TimerDC;
		RoomSystem.disconnectTimer.AutoReset = false;
		RoomSystem.StaticLoad();
	}

	[OnEnterPlay_Run]
	private static void StaticLoad()
	{
		RoomSystem.netEventCallbacks[0] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializeLaunchProjectile);
		RoomSystem.netEventCallbacks[1] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializeImpactEffect);
		RoomSystem.netEventCallbacks[4] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.SearchForNearby);
		RoomSystem.netEventCallbacks[7] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.SearchForParty);
		RoomSystem.netEventCallbacks[10] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.SearchForElevator);
		RoomSystem.netEventCallbacks[11] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.SearchForShuttle);
		RoomSystem.netEventCallbacks[2] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializeStatusEffect);
		RoomSystem.netEventCallbacks[3] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializeSoundEffect);
		RoomSystem.netEventCallbacks[5] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializeReportTouch);
		RoomSystem.netEventCallbacks[8] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializePlayerLaunched);
		RoomSystem.netEventCallbacks[6] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializePlayerEffect);
		RoomSystem.netEventCallbacks[9] = new Action<object[], PhotonMessageInfoWrapped>(RoomSystem.DeserializePlayerHit);
		RoomSystem.soundEffectCallback = new Action<RoomSystem.SoundEffect, NetPlayer>(RoomSystem.OnPlaySoundEffect);
		RoomSystem.statusEffectCallback = new Action<RoomSystem.StatusEffects>(RoomSystem.OnStatusEffect);
	}

	private static void TimerDC(object sender, ElapsedEventArgs args)
	{
		RoomSystem.disconnectTimer.Stop();
		if (!RoomSystem.joinedRoom)
		{
			return;
		}
		PhotonNetwork.Disconnect();
		PhotonNetwork.SendAllOutgoingCommands();
	}

	public static byte GetRoomSize(string gameMode = "")
	{
		if (RoomSystem.joinedRoom)
		{
			if (RoomSystem.m_roomSizeOnJoin > 10)
			{
				return 10;
			}
			return RoomSystem.m_roomSizeOnJoin;
		}
		else
		{
			if (RoomSystem.UseRoomSizeOverride)
			{
				return RoomSystem.RoomSizeOverride;
			}
			return 10;
		}
	}

	public static byte GetRoomSizeForCreate(string gameMode = "")
	{
		if (RoomSystem.UseRoomSizeOverride)
		{
			return RoomSystem.RoomSizeOverride;
		}
		return 10;
	}

	public static void OverrideRoomSize(byte roomSize)
	{
		if (roomSize < 1)
		{
			roomSize = 1;
		}
		if (roomSize > 10)
		{
			roomSize = 10;
		}
		if (roomSize == 10)
		{
			RoomSystem.UseRoomSizeOverride = false;
			RoomSystem.RoomSizeOverride = 10;
			return;
		}
		RoomSystem.UseRoomSizeOverride = true;
		RoomSystem.RoomSizeOverride = roomSize;
	}

	public static byte GetOverridenRoomSize()
	{
		if (RoomSystem.UseRoomSizeOverride)
		{
			return RoomSystem.RoomSizeOverride;
		}
		return 10;
	}

	public static void ClearOverridenRoomSize()
	{
		RoomSystem.UseRoomSizeOverride = false;
		RoomSystem.RoomSizeOverride = 10;
	}

	public static void MakeRoomMultiplayer(byte roomSize)
	{
		if (!RoomSystem.joinedRoom || RoomSystem.m_roomSizeOnJoin > 1)
		{
			return;
		}
		if (roomSize > 10)
		{
			roomSize = 10;
		}
		RoomSystem.m_roomSizeOnJoin = roomSize;
		PhotonNetwork.CurrentRoom.MaxPlayers = roomSize;
	}

	internal static void SendEvent(in byte code, in object evData, in NetPlayer target, bool reliable)
	{
		NetworkSystemRaiseEvent.neoTarget.TargetActors[0] = target.ActorNumber;
		RoomSystem.SendEvent(in code, in evData, in NetworkSystemRaiseEvent.neoTarget, reliable);
	}

	internal static void SendEvent(in byte code, in object evData, in NetEventOptions neo, bool reliable)
	{
		RoomSystem.sendEventData[0] = NetworkSystem.Instance.ServerTimestamp;
		RoomSystem.sendEventData[1] = code;
		RoomSystem.sendEventData[2] = evData;
		NetworkSystemRaiseEvent.RaiseEvent(3, RoomSystem.sendEventData, neo, reliable);
	}

	private static void OnEvent(EventData data)
	{
		RoomSystem.OnEvent(data.Code, data.CustomData, data.Sender);
	}

	private static void OnEvent(byte code, object data, int source)
	{
		NetPlayer netPlayer;
		if (code != 3 || !Utils.PlayerInRoom(source, out netPlayer))
		{
			return;
		}
		try
		{
			object[] array = (object[])data;
			int num = Convert.ToInt32(array[0]);
			byte b = Convert.ToByte(array[1]);
			object[] array2 = null;
			if (array.Length > 2)
			{
				object obj = array[2];
				array2 = ((obj == null) ? null : ((object[])obj));
			}
			PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(netPlayer.ActorNumber, num);
			Action<object[], PhotonMessageInfoWrapped> action;
			if (RoomSystem.netEventCallbacks.TryGetValue(b, out action))
			{
				action(array2, photonMessageInfoWrapped);
			}
		}
		catch
		{
		}
	}

	internal static void SearchForNearby(object[] shuffleData, PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		GorillaNot.IncrementRPCCall(info, "JoinPubWithNearby");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 23, NetworkSystem.Instance.SimTime))
		{
			return;
		}
		string text = (string)shuffleData[0];
		string text2 = (string)shuffleData[1];
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups);
		if (!GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(NetworkSystem.Instance.LocalPlayer.UserId))
		{
			GorillaNot.instance.SendReport("possible kick attempt", player.UserId, player.NickName);
			return;
		}
		if (!flag || !RoomSystem.WasRoomPrivate)
		{
			return;
		}
		PhotonNetworkController.Instance.AttemptToFollowIntoPub(player.UserId, player.ActorNumber, text2, text, JoinType.FollowingNearby);
	}

	internal static void SearchForParty(object[] shuffleData, PhotonMessageInfoWrapped info)
	{
		GorillaNot.IncrementRPCCall(info, "PARTY_JOIN");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer) || !FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 23, NetworkSystem.Instance.SimTime))
		{
			return;
		}
		string text = (string)shuffleData[0];
		string text2 = (string)shuffleData[1];
		if (!FriendshipGroupDetection.Instance.IsInMyGroup(info.Sender.UserId))
		{
			GorillaNot.instance.SendReport("possible kick attempt", info.Sender.UserId, info.Sender.NickName);
			return;
		}
		if (PlayFabAuthenticator.instance.GetSafety())
		{
			return;
		}
		PhotonNetworkController.Instance.AttemptToFollowIntoPub(info.Sender.UserId, info.Sender.ActorNumber, text2, text, JoinType.FollowingParty);
	}

	internal static void SearchForElevator(object[] shuffleData, PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		GorillaNot.IncrementRPCCall(info, "JoinPubWithElevator");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 23, NetworkSystem.Instance.SimTime))
		{
			return;
		}
		string text = (string)shuffleData[0];
		string text2 = (string)shuffleData[1];
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups);
		if (GRElevatorManager.ValidElevatorNetworking(info.Sender.ActorNumber) && GRElevatorManager.ValidElevatorNetworking(NetworkSystem.Instance.LocalPlayer.ActorNumber))
		{
			if (!flag)
			{
				GRElevatorManager.JoinPublicRoom();
				return;
			}
			PhotonNetworkController.Instance.AttemptToFollowIntoPub(player.UserId, player.ActorNumber, text2, text, JoinType.JoinWithElevator);
		}
	}

	internal static void SearchForShuttle(object[] shuffleData, PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		GorillaNot.IncrementRPCCall(info, "JoinPubWithElevator");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 23, NetworkSystem.Instance.SimTime))
		{
			return;
		}
		string text = (string)shuffleData[0];
		string text2 = (string)shuffleData[1];
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups);
		bool flag2 = GRElevatorManager.ValidShuttleNetworking(info.Sender.ActorNumber);
		bool flag3 = GRElevatorManager.ValidShuttleNetworking(NetworkSystem.Instance.LocalPlayer.ActorNumber);
		if (flag2 && flag3)
		{
			if (!flag)
			{
				GRElevatorManager.JoinPublicRoom();
				return;
			}
			PhotonNetworkController.Instance.AttemptToFollowIntoPub(player.UserId, player.ActorNumber, text2, text, JoinType.JoinWithElevator);
		}
	}

	internal static void SendNearbyFollowCommand(GorillaFriendCollider friendCollider, string shuffler, string keyStr)
	{
		RoomSystem.groupJoinSendData[0] = shuffler;
		RoomSystem.groupJoinSendData[1] = keyStr;
		NetEventOptions netEventOptions = new NetEventOptions
		{
			TargetActors = new int[1]
		};
		foreach (NetPlayer netPlayer in RoomSystem.PlayersInRoom)
		{
			if (friendCollider.playerIDsCurrentlyTouching.Contains(netPlayer.UserId) && netPlayer != NetworkSystem.Instance.LocalPlayer)
			{
				netEventOptions.TargetActors[0] = netPlayer.ActorNumber;
				byte b = 4;
				object obj = RoomSystem.groupJoinSendData;
				RoomSystem.SendEvent(in b, in obj, in netEventOptions, false);
			}
		}
	}

	internal static void SendPartyFollowCommand(string shuffler, string keyStr)
	{
		RoomSystem.groupJoinSendData[0] = shuffler;
		RoomSystem.groupJoinSendData[1] = keyStr;
		NetEventOptions netEventOptions = new NetEventOptions
		{
			TargetActors = new int[1]
		};
		foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
		{
			if (vrrig.IsLocalPartyMember && vrrig.creator != NetworkSystem.Instance.LocalPlayer)
			{
				netEventOptions.TargetActors[0] = vrrig.creator.ActorNumber;
				Debug.Log(string.Format("SendGroupFollowCommand - sendEvent to {0} from {1}, shuffler {2} key {3}", new object[]
				{
					vrrig.creator.NickName,
					NetworkSystem.Instance.LocalPlayer.UserId,
					RoomSystem.groupJoinSendData[0],
					RoomSystem.groupJoinSendData[1]
				}));
				byte b = 7;
				object obj = RoomSystem.groupJoinSendData;
				RoomSystem.SendEvent(in b, in obj, in netEventOptions, false);
			}
		}
	}

	internal static void SendElevatorFollowCommand(string shuffler, string keyStr, GorillaFriendCollider sourceFriendCollider, GorillaFriendCollider targetFriendCollider)
	{
		RoomSystem.SendGroupJoinFollowCommand(10, shuffler, keyStr, sourceFriendCollider, targetFriendCollider);
	}

	internal static void SendShuttleFollowCommand(string shuffler, string keyStr, GorillaFriendCollider sourceFriendCollider, GorillaFriendCollider targetFriendCollider)
	{
		RoomSystem.SendGroupJoinFollowCommand(11, shuffler, keyStr, sourceFriendCollider, targetFriendCollider);
	}

	internal static void SendGroupJoinFollowCommand(byte eventType, string shuffler, string keyStr, GorillaFriendCollider sourceFriendCollider, GorillaFriendCollider targetFriendCollider)
	{
		RoomSystem.groupJoinSendData[0] = shuffler;
		RoomSystem.groupJoinSendData[1] = keyStr;
		NetEventOptions netEventOptions = new NetEventOptions
		{
			TargetActors = new int[1]
		};
		foreach (NetPlayer netPlayer in RoomSystem.PlayersInRoom)
		{
			if (sourceFriendCollider.playerIDsCurrentlyTouching.Contains(netPlayer.UserId) || (targetFriendCollider.playerIDsCurrentlyTouching.Contains(netPlayer.UserId) && netPlayer != NetworkSystem.Instance.LocalPlayer))
			{
				netEventOptions.TargetActors[0] = netPlayer.ActorNumber;
				Debug.Log(string.Format("SendElevatorFollowCommand - sendEvent to {0} from {1}, shuffler {2} key {3}", new object[]
				{
					netPlayer.NickName,
					NetworkSystem.Instance.LocalPlayer.UserId,
					RoomSystem.groupJoinSendData[0],
					RoomSystem.groupJoinSendData[1]
				}));
				object obj = RoomSystem.groupJoinSendData;
				RoomSystem.SendEvent(in eventType, in obj, in netEventOptions, false);
			}
		}
	}

	private static void DeserializeReportTouch(object[] data, PhotonMessageInfoWrapped info)
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		NetPlayer netPlayer = (NetPlayer)data[0];
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		Action<NetPlayer, NetPlayer> action = RoomSystem.playerTouchedCallback;
		if (action == null)
		{
			return;
		}
		action(netPlayer, player);
	}

	internal static void SendReportTouch(NetPlayer touchedNetPlayer)
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			RoomSystem.reportTouchSendData[0] = touchedNetPlayer;
			byte b = 5;
			object obj = RoomSystem.reportTouchSendData;
			RoomSystem.SendEvent(in b, in obj, in NetworkSystemRaiseEvent.neoMaster, false);
			return;
		}
		Action<NetPlayer, NetPlayer> action = RoomSystem.playerTouchedCallback;
		if (action == null)
		{
			return;
		}
		action(touchedNetPlayer, NetworkSystem.Instance.LocalPlayer);
	}

	internal static void LaunchPlayer(NetPlayer player, Vector3 velocity)
	{
		RoomSystem.reportTouchSendData[0] = velocity;
		byte b = 8;
		object obj = RoomSystem.reportTouchSendData;
		RoomSystem.SendEvent(in b, in obj, in player, false);
	}

	private static void DeserializePlayerLaunched(object[] data, PhotonMessageInfoWrapped info)
	{
		GorillaNot.IncrementRPCCall(info, "DeserializePlayerLaunched");
		GorillaGameManager activeGameMode = GameMode.ActiveGameMode;
		if (activeGameMode != null && activeGameMode.GameType() == GameModeType.Guardian && info.Sender == NetworkSystem.Instance.MasterClient)
		{
			object obj = data[0];
			if (obj is Vector3)
			{
				Vector3 vector = (Vector3)obj;
				float num = 10000f;
				if ((in vector).IsValid(in num) && vector.magnitude <= 20f && RoomSystem.playerLaunchedCallLimiter.CheckCallTime(Time.time))
				{
					GTPlayer.Instance.DoLaunch(vector);
					return;
				}
			}
		}
	}

	internal static void HitPlayer(NetPlayer player, Vector3 direction, float strength)
	{
		RoomSystem.reportHitSendData[0] = direction;
		RoomSystem.reportHitSendData[1] = strength;
		RoomSystem.reportHitSendData[2] = player.ActorNumber;
		byte b = 9;
		object obj = RoomSystem.reportHitSendData;
		RoomSystem.SendEvent(in b, in obj, in NetworkSystemRaiseEvent.neoOthers, false);
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			rigContainer.Rig.DisableHitWithKnockBack();
		}
	}

	private static void DeserializePlayerHit(object[] data, PhotonMessageInfoWrapped info)
	{
		object obj = data[0];
		if (obj is Vector3)
		{
			Vector3 vector = (Vector3)obj;
			obj = data[1];
			if (obj is float)
			{
				float num = (float)obj;
				obj = data[2];
				if (obj is int)
				{
					int num2 = (int)obj;
					float num3 = 10000f;
					RigContainer rigContainer;
					if ((in vector).IsValid(in num3) && VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer) && FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 20, info.SentServerTime))
					{
						float num4 = num.ClampSafe(0f, 10f);
						GorillaNot.IncrementRPCCall(info, "DeserializePlayerHit");
						if (num2 == NetworkSystem.Instance.LocalPlayer.ActorNumber)
						{
							CosmeticEffectsOnPlayers.CosmeticEffect cosmeticEffect;
							CosmeticEffectsOnPlayers.CosmeticEffect cosmeticEffect2;
							if (GorillaTagger.Instance.offlineVRRig.TemporaryCosmeticEffects.TryGetValue(CosmeticEffectsOnPlayers.EFFECTTYPE.TagWithKnockback, out cosmeticEffect))
							{
								if (!cosmeticEffect.IsGameModeAllowed())
								{
									return;
								}
								float num5 = (num4 * cosmeticEffect.knockbackStrength * cosmeticEffect.knockbackStrengthMultiplier).ClampSafe(cosmeticEffect.minKnockbackStrength, cosmeticEffect.maxKnockbackStrength);
								if (cosmeticEffect.applyScaleToKnockbackStrength)
								{
									num5 *= GTPlayer.Instance.scale;
								}
								GTPlayer.Instance.ApplyKnockback(vector.normalized, num5, cosmeticEffect.forceOffTheGround);
							}
							else if (GorillaTagger.Instance.offlineVRRig.TemporaryCosmeticEffects.TryGetValue(CosmeticEffectsOnPlayers.EFFECTTYPE.InstantKnockback, out cosmeticEffect2))
							{
								if (!cosmeticEffect2.IsGameModeAllowed())
								{
									return;
								}
								float num6 = (num4 * cosmeticEffect2.knockbackStrength * cosmeticEffect2.knockbackStrengthMultiplier).ClampSafe(cosmeticEffect2.minKnockbackStrength, cosmeticEffect2.maxKnockbackStrength);
								if (cosmeticEffect.applyScaleToKnockbackStrength)
								{
									num6 *= GTPlayer.Instance.scale;
								}
								GTPlayer.Instance.ApplyKnockback(vector.normalized, num6, cosmeticEffect2.forceOffTheGround);
							}
						}
						NetPlayer player = NetworkSystem.Instance.GetPlayer(num2);
						RigContainer rigContainer2;
						if (player != null && VRRigCache.Instance.TryGetVrrig(player, out rigContainer2))
						{
							rigContainer2.Rig.DisableHitWithKnockBack();
						}
						return;
					}
				}
			}
		}
	}

	private static void SetSlowedTime()
	{
		if (GorillaTagger.Instance.currentStatus != GorillaTagger.StatusEffect.Slowed)
		{
			GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
		}
		GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Slowed, GorillaTagger.Instance.slowCooldown);
		GorillaTagger.Instance.offlineVRRig.PlayTaggedEffect();
	}

	private static void SetTaggedTime()
	{
		GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Frozen, GorillaTagger.Instance.tagCooldown);
		GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
		GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
		GorillaTagger.Instance.offlineVRRig.PlayTaggedEffect();
	}

	private static void SetFrozenTime()
	{
		GorillaFreezeTagManager gorillaFreezeTagManager = GameMode.ActiveGameMode as GorillaFreezeTagManager;
		if (gorillaFreezeTagManager != null)
		{
			GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Slowed, gorillaFreezeTagManager.freezeDuration);
			GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			GorillaTagger.Instance.offlineVRRig.PlayTaggedEffect();
		}
	}

	private static void SetJoinedTaggedTime()
	{
		GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
		GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
	}

	private static void SetUntaggedTime()
	{
		GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.None, 0f);
		GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
		GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
	}

	private static void OnStatusEffect(RoomSystem.StatusEffects status)
	{
		switch (status)
		{
		case RoomSystem.StatusEffects.TaggedTime:
			RoomSystem.SetTaggedTime();
			return;
		case RoomSystem.StatusEffects.JoinedTaggedTime:
			RoomSystem.SetJoinedTaggedTime();
			return;
		case RoomSystem.StatusEffects.SetSlowedTime:
			RoomSystem.SetSlowedTime();
			return;
		case RoomSystem.StatusEffects.UnTagged:
			RoomSystem.SetUntaggedTime();
			return;
		case RoomSystem.StatusEffects.FrozenTime:
			RoomSystem.SetFrozenTime();
			return;
		default:
			return;
		}
	}

	private static void DeserializeStatusEffect(object[] data, PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		GorillaNot.IncrementRPCCall(info, "DeserializeStatusEffect");
		if (!player.IsMasterClient)
		{
			GorillaNot.instance.SendReport("invalid status", player.UserId, player.NickName);
			return;
		}
		if (!RoomSystem.callbackInstance.roomSettings.StatusEffectLimiter.CheckCallServerTime(info.SentServerTime))
		{
			return;
		}
		RoomSystem.StatusEffects statusEffects = (RoomSystem.StatusEffects)Convert.ToInt32(data[0]);
		Action<RoomSystem.StatusEffects> action = RoomSystem.statusEffectCallback;
		if (action == null)
		{
			return;
		}
		action(statusEffects);
	}

	internal static void SendStatusEffectAll(RoomSystem.StatusEffects status)
	{
		Action<RoomSystem.StatusEffects> action = RoomSystem.statusEffectCallback;
		if (action != null)
		{
			action(status);
		}
		if (!RoomSystem.joinedRoom)
		{
			return;
		}
		RoomSystem.statusSendData[0] = (int)status;
		byte b = 2;
		object obj = RoomSystem.statusSendData;
		RoomSystem.SendEvent(in b, in obj, in NetworkSystemRaiseEvent.neoOthers, false);
	}

	internal static void SendStatusEffectToPlayer(RoomSystem.StatusEffects status, NetPlayer target)
	{
		if (!target.IsLocal)
		{
			RoomSystem.statusSendData[0] = (int)status;
			byte b = 2;
			object obj = RoomSystem.statusSendData;
			RoomSystem.SendEvent(in b, in obj, in target, false);
			return;
		}
		Action<RoomSystem.StatusEffects> action = RoomSystem.statusEffectCallback;
		if (action == null)
		{
			return;
		}
		action(status);
	}

	internal static void PlaySoundEffect(int soundIndex, float soundVolume, bool stopCurrentAudio)
	{
		VRRigCache.Instance.localRig.Rig.PlayTagSoundLocal(soundIndex, soundVolume, stopCurrentAudio);
	}

	internal static void PlaySoundEffect(int soundIndex, float soundVolume, bool stopCurrentAudio, NetPlayer target)
	{
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(target, out rigContainer))
		{
			rigContainer.Rig.PlayTagSoundLocal(soundIndex, soundVolume, stopCurrentAudio);
		}
	}

	private static void OnPlaySoundEffect(RoomSystem.SoundEffect sound, NetPlayer target)
	{
		if (target.IsLocal)
		{
			RoomSystem.PlaySoundEffect(sound.id, sound.volume, sound.stopCurrentAudio);
			return;
		}
		RoomSystem.PlaySoundEffect(sound.id, sound.volume, sound.stopCurrentAudio, target);
	}

	private static void DeserializeSoundEffect(object[] data, PhotonMessageInfoWrapped info)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.senderID);
		GorillaNot.IncrementRPCCall(info, "DeserializeSoundEffect");
		if (!player.Equals(NetworkSystem.Instance.MasterClient))
		{
			GorillaNot.instance.SendReport("invalid sound effect", player.UserId, player.NickName);
			return;
		}
		RoomSystem.SoundEffect soundEffect;
		soundEffect.id = Convert.ToInt32(data[0]);
		soundEffect.volume = Convert.ToSingle(data[1]);
		soundEffect.stopCurrentAudio = Convert.ToBoolean(data[2]);
		if (!float.IsFinite(soundEffect.volume))
		{
			return;
		}
		NetPlayer netPlayer;
		if (data.Length > 3)
		{
			if (!RoomSystem.callbackInstance.roomSettings.SoundEffectOtherLimiter.CheckCallServerTime(info.SentServerTime))
			{
				return;
			}
			int num = Convert.ToInt32(data[3]);
			netPlayer = NetworkSystem.Instance.GetPlayer(num);
		}
		else
		{
			if (!RoomSystem.callbackInstance.roomSettings.SoundEffectLimiter.CheckCallServerTime(info.SentServerTime))
			{
				return;
			}
			netPlayer = NetworkSystem.Instance.LocalPlayer;
		}
		RoomSystem.soundEffectCallback(soundEffect, netPlayer);
	}

	internal static void SendSoundEffectAll(int soundIndex, float soundVolume, bool stopCurrentAudio = false)
	{
		RoomSystem.SendSoundEffectAll(new RoomSystem.SoundEffect(soundIndex, soundVolume, stopCurrentAudio));
	}

	internal static void SendSoundEffectAll(RoomSystem.SoundEffect sound)
	{
		Action<RoomSystem.SoundEffect, NetPlayer> action = RoomSystem.soundEffectCallback;
		if (action != null)
		{
			action(sound, NetworkSystem.Instance.LocalPlayer);
		}
		if (!RoomSystem.joinedRoom)
		{
			return;
		}
		RoomSystem.soundSendData[0] = sound.id;
		RoomSystem.soundSendData[1] = sound.volume;
		RoomSystem.soundSendData[2] = sound.stopCurrentAudio;
		byte b = 3;
		object obj = RoomSystem.soundSendData;
		RoomSystem.SendEvent(in b, in obj, in NetworkSystemRaiseEvent.neoOthers, false);
	}

	internal static void SendSoundEffectToPlayer(int soundIndex, float soundVolume, NetPlayer player, bool stopCurrentAudio = false)
	{
		RoomSystem.SendSoundEffectToPlayer(new RoomSystem.SoundEffect(soundIndex, soundVolume, stopCurrentAudio), player);
	}

	internal static void SendSoundEffectToPlayer(RoomSystem.SoundEffect sound, NetPlayer player)
	{
		if (player.IsLocal)
		{
			Action<RoomSystem.SoundEffect, NetPlayer> action = RoomSystem.soundEffectCallback;
			if (action == null)
			{
				return;
			}
			action(sound, player);
			return;
		}
		else
		{
			if (!RoomSystem.joinedRoom)
			{
				return;
			}
			RoomSystem.soundSendData[0] = sound.id;
			RoomSystem.soundSendData[1] = sound.volume;
			RoomSystem.soundSendData[2] = sound.stopCurrentAudio;
			byte b = 3;
			object obj = RoomSystem.soundSendData;
			RoomSystem.SendEvent(in b, in obj, in player, false);
			return;
		}
	}

	internal static void SendSoundEffectOnOther(int soundIndex, float soundvolume, NetPlayer target, bool stopCurrentAudio = false)
	{
		RoomSystem.SendSoundEffectOnOther(new RoomSystem.SoundEffect(soundIndex, soundvolume, stopCurrentAudio), target);
	}

	internal static void SendSoundEffectOnOther(RoomSystem.SoundEffect sound, NetPlayer target)
	{
		Action<RoomSystem.SoundEffect, NetPlayer> action = RoomSystem.soundEffectCallback;
		if (action != null)
		{
			action(sound, target);
		}
		if (!RoomSystem.joinedRoom)
		{
			return;
		}
		RoomSystem.sendSoundDataOther[0] = sound.id;
		RoomSystem.sendSoundDataOther[1] = sound.volume;
		RoomSystem.sendSoundDataOther[2] = sound.stopCurrentAudio;
		RoomSystem.sendSoundDataOther[3] = target.ActorNumber;
		byte b = 3;
		object obj = RoomSystem.sendSoundDataOther;
		RoomSystem.SendEvent(in b, in obj, in NetworkSystemRaiseEvent.neoOthers, false);
	}

	internal static void OnPlayerEffect(PlayerEffect effect, NetPlayer target)
	{
		if (target == null)
		{
			return;
		}
		RoomSystem.PlayerEffectConfig playerEffectConfig;
		RigContainer rigContainer;
		if (RoomSystem.playerEffectDictionary.TryGetValue(effect, out playerEffectConfig) && VRRigCache.Instance.TryGetVrrig(target, out rigContainer) && rigContainer != null && rigContainer.Rig != null && playerEffectConfig.tagEffectPack != null)
		{
			TagEffectsLibrary.PlayEffect(rigContainer.Rig.transform, false, rigContainer.Rig.scaleFactor, target.IsLocal ? TagEffectsLibrary.EffectType.FIRST_PERSON : TagEffectsLibrary.EffectType.THIRD_PERSON, playerEffectConfig.tagEffectPack, playerEffectConfig.tagEffectPack, rigContainer.Rig.transform.rotation);
		}
	}

	private static void DeserializePlayerEffect(object[] data, PhotonMessageInfoWrapped info)
	{
		GorillaNot.IncrementRPCCall(info, "DeserializePlayerEffect");
		if (!RoomSystem.callbackInstance.roomSettings.PlayerEffectLimiter.CheckCallServerTime(info.SentServerTime))
		{
			return;
		}
		int num = Convert.ToInt32(data[0]);
		PlayerEffect playerEffect = (PlayerEffect)Convert.ToInt32(data[1]);
		NetPlayer player = NetworkSystem.Instance.GetPlayer(num);
		RoomSystem.OnPlayerEffect(playerEffect, player);
	}

	internal static void SendPlayerEffect(PlayerEffect effect, NetPlayer target)
	{
		RoomSystem.OnPlayerEffect(effect, target);
		if (!RoomSystem.joinedRoom)
		{
			return;
		}
		RoomSystem.playerEffectData[0] = target.ActorNumber;
		RoomSystem.playerEffectData[1] = effect;
		byte b = 6;
		object obj = RoomSystem.playerEffectData;
		RoomSystem.SendEvent(in b, in obj, in NetworkSystemRaiseEvent.neoOthers, false);
	}

	private static RoomSystem.ImpactFxContainer impactEffect = new RoomSystem.ImpactFxContainer();

	private static RoomSystem.LaunchProjectileContainer launchProjectile = new RoomSystem.LaunchProjectileContainer();

	public static GameObject playerImpactEffectPrefab = null;

	private static readonly object[] projectileSendData = new object[9];

	private static readonly object[] impactSendData = new object[6];

	private static readonly List<int> hashValues = new List<int>(2);

	[SerializeField]
	private RoomSystemSettings roomSettings;

	[SerializeField]
	private string[] prefabsToInstantiateByPath;

	[SerializeField]
	private GameObject[] prefabsToInstantiate;

	private List<GameObject> prefabsInstantiated = new List<GameObject>();

	public static Dictionary<PlayerEffect, RoomSystem.PlayerEffectConfig> playerEffectDictionary = new Dictionary<PlayerEffect, RoomSystem.PlayerEffectConfig>();

	[OnEnterPlay_SetNull]
	private static RoomSystem callbackInstance;

	private static byte m_roomSizeOnJoin;

	[OnEnterPlay_Clear]
	private static List<NetPlayer> netPlayersInRoom = new List<NetPlayer>(10);

	[OnEnterPlay_Set("")]
	private static string roomGameMode = "";

	[OnEnterPlay_Set(false)]
	private static bool joinedRoom = false;

	[OnEnterPlay_SetNull]
	private static PhotonView[] sceneViews;

	[OnEnterPlay_SetNew]
	public static DelegateListProcessor LeftRoomEvent = new DelegateListProcessor();

	[OnEnterPlay_SetNew]
	public static DelegateListProcessor JoinedRoomEvent = new DelegateListProcessor();

	[OnEnterPlay_SetNew]
	public static DelegateListProcessor<NetPlayer> PlayerJoinedEvent = new DelegateListProcessor<NetPlayer>();

	[OnEnterPlay_SetNew]
	public static DelegateListProcessor<NetPlayer> PlayerLeftEvent = new DelegateListProcessor<NetPlayer>();

	[OnEnterPlay_SetNew]
	public static DelegateListProcessor PlayersChangedEvent = new DelegateListProcessor();

	private static Timer disconnectTimer = new Timer();

	[OnExitPlay_Clear]
	internal static readonly Dictionary<byte, Action<object[], PhotonMessageInfoWrapped>> netEventCallbacks = new Dictionary<byte, Action<object[], PhotonMessageInfoWrapped>>(10);

	private static readonly object[] sendEventData = new object[3];

	private static readonly object[] groupJoinSendData = new object[2];

	private static readonly object[] reportTouchSendData = new object[1];

	private static readonly object[] reportHitSendData = new object[3];

	[OnExitPlay_SetNull]
	public static Action<NetPlayer, NetPlayer> playerTouchedCallback;

	private static CallLimiter playerLaunchedCallLimiter = new CallLimiter(3, 15f, 0.5f);

	private static CallLimiter hitPlayerCallLimiter = new CallLimiter(10, 2f, 0.5f);

	private static object[] statusSendData = new object[1];

	public static Action<RoomSystem.StatusEffects> statusEffectCallback;

	private static object[] soundSendData = new object[3];

	private static object[] sendSoundDataOther = new object[4];

	public static Action<RoomSystem.SoundEffect, NetPlayer> soundEffectCallback;

	private static object[] playerEffectData = new object[2];

	private class ImpactFxContainer : IFXContext
	{
		public FXSystemSettings settings
		{
			get
			{
				return this.targetRig.fxSettings;
			}
		}

		public virtual void OnPlayFX()
		{
			NetPlayer creator = this.targetRig.creator;
			ProjectileTracker.ProjectileInfo projectileInfo;
			if (this.targetRig.isOfflineVRRig)
			{
				projectileInfo = ProjectileTracker.GetLocalProjectile(this.projectileIndex);
			}
			else
			{
				ValueTuple<bool, ProjectileTracker.ProjectileInfo> andRemoveRemotePlayerProjectile = ProjectileTracker.GetAndRemoveRemotePlayerProjectile(creator, this.projectileIndex);
				if (!andRemoveRemotePlayerProjectile.Item1)
				{
					return;
				}
				projectileInfo = andRemoveRemotePlayerProjectile.Item2;
			}
			SlingshotProjectile projectileInstance = projectileInfo.projectileInstance;
			GameObject gameObject = (projectileInfo.hasImpactOverride ? projectileInstance.playerImpactEffectPrefab : RoomSystem.playerImpactEffectPrefab);
			GameObject gameObject2 = ObjectPools.instance.Instantiate(gameObject, this.position, true);
			gameObject2.transform.localScale = Vector3.one * this.targetRig.scaleFactor;
			GorillaColorizableBase gorillaColorizableBase;
			if (gameObject2.TryGetComponent<GorillaColorizableBase>(out gorillaColorizableBase))
			{
				gorillaColorizableBase.SetColor(this.colour);
			}
			SurfaceImpactFX component = gameObject2.GetComponent<SurfaceImpactFX>();
			if (component != null)
			{
				component.SetScale(projectileInstance.transform.localScale.x * projectileInstance.impactEffectScaleMultiplier);
			}
			SoundBankPlayer component2 = gameObject2.GetComponent<SoundBankPlayer>();
			if (component2 != null && !component2.playOnEnable)
			{
				component2.Play(projectileInstance.impactSoundVolumeOverride, projectileInstance.impactSoundPitchOverride);
			}
			if (projectileInstance.gameObject.activeSelf && projectileInstance.projectileOwner == creator)
			{
				projectileInstance.Deactivate();
			}
		}

		public VRRig targetRig;

		public Vector3 position;

		public Color colour;

		public int projectileIndex;
	}

	private class LaunchProjectileContainer : RoomSystem.ImpactFxContainer
	{
		public override void OnPlayFX()
		{
			GameObject gameObject = null;
			SlingshotProjectile slingshotProjectile = null;
			try
			{
				switch (this.projectileSource)
				{
				case RoomSystem.ProjectileSource.ProjectileWeapon:
					if (this.targetRig.projectileWeapon.IsNotNull() && this.targetRig.projectileWeapon.IsNotNull())
					{
						this.velocity = this.targetRig.ClampVelocityRelativeToPlayerSafe(this.velocity, 70f, 100f);
						SlingshotProjectile slingshotProjectile2 = this.targetRig.projectileWeapon.LaunchNetworkedProjectile(this.position, this.velocity, this.projectileSource, this.projectileIndex, this.targetRig.scaleFactor, this.overridecolour, this.colour, this.messageInfo);
						if (slingshotProjectile2.IsNotNull())
						{
							ProjectileTracker.AddRemotePlayerProjectile(this.messageInfo.Sender, slingshotProjectile2, this.projectileIndex, this.messageInfo.SentServerTime, this.velocity, this.position, this.targetRig.scaleFactor);
						}
					}
					return;
				case RoomSystem.ProjectileSource.LeftHand:
					this.tempThrowableGO = this.targetRig.myBodyDockPositions.GetLeftHandThrowable();
					break;
				case RoomSystem.ProjectileSource.RightHand:
					this.tempThrowableGO = this.targetRig.myBodyDockPositions.GetRightHandThrowable();
					break;
				default:
					return;
				}
				if (!this.tempThrowableGO.IsNull() && this.tempThrowableGO.TryGetComponent<SnowballThrowable>(out this.tempThrowableRef) && !(this.tempThrowableRef is GrowingSnowballThrowable))
				{
					this.velocity = this.targetRig.ClampVelocityRelativeToPlayerSafe(this.velocity, 50f, 100f);
					int projectileHash = this.tempThrowableRef.ProjectileHash;
					gameObject = ObjectPools.instance.Instantiate(projectileHash, true);
					slingshotProjectile = gameObject.GetComponent<SlingshotProjectile>();
					ProjectileTracker.AddRemotePlayerProjectile(this.targetRig.creator, slingshotProjectile, this.projectileIndex, this.messageInfo.SentServerTime, this.velocity, this.position, this.targetRig.scaleFactor);
					slingshotProjectile.Launch(this.position, this.velocity, this.messageInfo.Sender, false, false, this.projectileIndex, this.targetRig.scaleFactor, this.overridecolour, this.colour);
				}
			}
			catch
			{
				GorillaNot.instance.SendReport("throwable error", this.messageInfo.Sender.UserId, this.messageInfo.Sender.NickName);
				if (slingshotProjectile != null && slingshotProjectile)
				{
					slingshotProjectile.transform.position = Vector3.zero;
					slingshotProjectile.Deactivate();
				}
				else if (gameObject.IsNotNull())
				{
					ObjectPools.instance.Destroy(gameObject);
				}
			}
		}

		public Vector3 velocity;

		public RoomSystem.ProjectileSource projectileSource;

		public bool overridecolour;

		public PhotonMessageInfoWrapped messageInfo;

		private GameObject tempThrowableGO;

		private SnowballThrowable tempThrowableRef;
	}

	internal enum ProjectileSource
	{
		ProjectileWeapon,
		LeftHand,
		RightHand
	}

	private struct Events
	{
		public const byte PROJECTILE = 0;

		public const byte IMPACT = 1;

		public const byte STATUS_EFFECT = 2;

		public const byte SOUND_EFFECT = 3;

		public const byte NEARBY_JOIN = 4;

		public const byte PLAYER_TOUCHED = 5;

		public const byte PLAYER_EFFECT = 6;

		public const byte PARTY_JOIN = 7;

		public const byte PLAYER_LAUNCHED = 8;

		public const byte PLAYER_HIT = 9;

		public const byte ELEVATOR_JOIN = 10;

		public const byte SHUTTLE_JOIN = 11;
	}

	public enum StatusEffects
	{
		TaggedTime,
		JoinedTaggedTime,
		SetSlowedTime,
		UnTagged,
		FrozenTime
	}

	public struct SoundEffect
	{
		public SoundEffect(int soundID, float soundVolume, bool _stopCurrentAudio)
		{
			this.id = soundID;
			this.volume = soundVolume;
			this.volume = soundVolume;
			this.stopCurrentAudio = _stopCurrentAudio;
		}

		public int id;

		public float volume;

		public bool stopCurrentAudio;
	}

	[Serializable]
	public struct PlayerEffectConfig
	{
		public PlayerEffect type;

		public TagEffectPack tagEffectPack;
	}
}
