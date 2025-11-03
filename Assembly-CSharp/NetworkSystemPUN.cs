using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Fusion;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.Audio;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

[RequireComponent(typeof(PUNCallbackNotifier))]
public class NetworkSystemPUN : NetworkSystem
{
	public override NetPlayer[] AllNetPlayers
	{
		get
		{
			return this.m_allNetPlayers;
		}
	}

	public override NetPlayer[] PlayerListOthers
	{
		get
		{
			return this.m_otherNetPlayers;
		}
	}

	public override VoiceConnection VoiceConnection
	{
		get
		{
			return this.punVoice;
		}
	}

	private int lowestPingRegionIndex
	{
		get
		{
			int num = 9999;
			int num2 = -1;
			for (int i = 0; i < this.regionData.Length; i++)
			{
				if (this.regionData[i].pingToRegion < num)
				{
					num = this.regionData[i].pingToRegion;
					num2 = i;
				}
			}
			return num2;
		}
	}

	private NetworkSystemPUN.InternalState internalState
	{
		get
		{
			return this.currentState;
		}
		set
		{
			this.currentState = value;
		}
	}

	public override string CurrentPhotonBackend
	{
		get
		{
			return "PUN";
		}
	}

	public override bool IsOnline
	{
		get
		{
			return this.InRoom;
		}
	}

	public override bool InRoom
	{
		get
		{
			return PhotonNetwork.InRoom;
		}
	}

	public override string RoomName
	{
		get
		{
			Room currentRoom = PhotonNetwork.CurrentRoom;
			return ((currentRoom != null) ? currentRoom.Name : null) ?? string.Empty;
		}
	}

	public override string RoomStringStripped()
	{
		Room currentRoom = PhotonNetwork.CurrentRoom;
		NetworkSystem.reusableSB.Clear();
		NetworkSystem.reusableSB.AppendFormat("Room: '{0}' ", (currentRoom.Name.Length < 20) ? currentRoom.Name : currentRoom.Name.Remove(20));
		NetworkSystem.reusableSB.AppendFormat("{0},{1} {3}/{2} players.", new object[]
		{
			currentRoom.IsVisible ? "visible" : "hidden",
			currentRoom.IsOpen ? "open" : "closed",
			currentRoom.MaxPlayers,
			currentRoom.PlayerCount
		});
		NetworkSystem.reusableSB.Append("\ncustomProps: {");
		NetworkSystem.reusableSB.AppendFormat("joinedGameMode={0}, ", (RoomSystem.RoomGameMode.Length < 50) ? RoomSystem.RoomGameMode : RoomSystem.RoomGameMode.Remove(50));
		IDictionary customProperties = currentRoom.CustomProperties;
		if (customProperties.Contains("gameMode"))
		{
			object obj = customProperties["gameMode"];
			if (obj == null)
			{
				NetworkSystem.reusableSB.AppendFormat("gameMode=null}", Array.Empty<object>());
			}
			else
			{
				string text = obj as string;
				if (text != null)
				{
					NetworkSystem.reusableSB.AppendFormat("gameMode={0}", (text.Length < 50) ? text : text.Remove(50));
				}
			}
		}
		NetworkSystem.reusableSB.Append("}");
		Debug.Log(NetworkSystem.reusableSB.ToString());
		return NetworkSystem.reusableSB.ToString();
	}

	public override string GameModeString
	{
		get
		{
			object obj;
			PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out obj);
			if (obj != null)
			{
				return obj.ToString();
			}
			return null;
		}
	}

	public override string CurrentRegion
	{
		get
		{
			return PhotonNetwork.CloudRegion;
		}
	}

	public override bool SessionIsPrivate
	{
		get
		{
			Room currentRoom = PhotonNetwork.CurrentRoom;
			return currentRoom != null && !currentRoom.IsVisible;
		}
	}

	public override int LocalPlayerID
	{
		get
		{
			return PhotonNetwork.LocalPlayer.ActorNumber;
		}
	}

	public override int ServerTimestamp
	{
		get
		{
			return PhotonNetwork.ServerTimestamp;
		}
	}

	public override double SimTime
	{
		get
		{
			return PhotonNetwork.Time;
		}
	}

	public override float SimDeltaTime
	{
		get
		{
			return Time.deltaTime;
		}
	}

	public override int SimTick
	{
		get
		{
			return PhotonNetwork.ServerTimestamp;
		}
	}

	public override int TickRate
	{
		get
		{
			return PhotonNetwork.SerializationRate;
		}
	}

	public override int RoomPlayerCount
	{
		get
		{
			return (int)PhotonNetwork.CurrentRoom.PlayerCount;
		}
	}

	public override bool IsMasterClient
	{
		get
		{
			return PhotonNetwork.IsMasterClient;
		}
	}

	public override async void Initialise()
	{
		base.Initialise();
		base.netState = NetSystemState.Initialization;
		PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = NetworkSystemConfig.AppVersion;
		PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
		PhotonNetwork.EnableCloseConnection = false;
		PhotonNetwork.AutomaticallySyncScene = false;
		string playerName = PlayerPrefs.GetString("playerName", "gorilla" + Random.Range(0, 9999).ToString().PadLeft(4, '0'));
		this.playerPool = new ObjectPool<PunNetPlayer>(10);
		base.UpdatePlayers();
		await this.CacheRegionInfo();
		base.UpdatePlayers();
		this.SetMyNickName(playerName);
	}

	private async Task CacheRegionInfo()
	{
		if (!this.isWrongVersion)
		{
			this.regionData = new NetworkRegionInfo[this.regionNames.Length];
			for (int i = 0; i < this.regionData.Length; i++)
			{
				this.regionData[i] = new NetworkRegionInfo();
			}
			int tryingRegionIndex = 0;
			TaskAwaiter<bool> taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.Authenticated }).GetAwaiter();
			TaskAwaiter<bool> taskAwaiter2;
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (taskAwaiter.GetResult())
			{
				base.netState = NetSystemState.PingRecon;
				while (tryingRegionIndex < this.regionNames.Length)
				{
					this.internalState = NetworkSystemPUN.InternalState.ConnectingToMaster;
					PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = this.regionNames[tryingRegionIndex];
					this.currentRegionIndex = tryingRegionIndex;
					PhotonNetwork.ConnectUsingSettings();
					taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.ConnectedToMaster }).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult())
					{
						return;
					}
					this.regionData[this.currentRegionIndex].playersInRegion = PhotonNetwork.CountOfPlayers;
					this.regionData[this.currentRegionIndex].pingToRegion = PhotonNetwork.GetPing();
					Utils.Log("Ping for " + PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion.ToString() + " is " + PhotonNetwork.GetPing().ToString());
					this.internalState = NetworkSystemPUN.InternalState.PingGathering;
					PhotonNetwork.Disconnect();
					taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.Internal_Disconnected }).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult())
					{
						return;
					}
					tryingRegionIndex++;
				}
				this.internalState = NetworkSystemPUN.InternalState.Idle;
				base.netState = NetSystemState.Idle;
			}
		}
	}

	public override AuthenticationValues GetAuthenticationValues()
	{
		return PhotonNetwork.AuthValues;
	}

	public override void SetAuthenticationValues(AuthenticationValues authValues)
	{
		PhotonNetwork.AuthValues = authValues;
	}

	public override void FinishAuthenticating()
	{
		this.internalState = NetworkSystemPUN.InternalState.Authenticated;
	}

	private async Task WaitForState(CancellationToken ct, params NetworkSystemPUN.InternalState[] desiredStates)
	{
		float timeoutTime = Time.time + 10f;
		while (!desiredStates.Contains(this.internalState))
		{
			if (ct.IsCancellationRequested)
			{
				string text = "";
				foreach (NetworkSystemPUN.InternalState internalState in desiredStates)
				{
					text += string.Format("- {0}", internalState);
				}
				Debug.LogError("Got cancelation token while waiting for states " + text);
				this.internalState = NetworkSystemPUN.InternalState.StateCheckFailed;
				break;
			}
			if (timeoutTime < Time.time)
			{
				string text2 = "";
				foreach (NetworkSystemPUN.InternalState internalState2 in desiredStates)
				{
					text2 += string.Format("- {0}", internalState2);
				}
				Debug.LogError("Got stuck waiting for states " + text2);
				this.internalState = NetworkSystemPUN.InternalState.StateCheckFailed;
				break;
			}
			await Task.Yield();
		}
	}

	private Task<bool> WaitForStateCheck(params NetworkSystemPUN.InternalState[] desiredStates)
	{
		NetworkSystemPUN.<WaitForStateCheck>d__59 <WaitForStateCheck>d__;
		<WaitForStateCheck>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
		<WaitForStateCheck>d__.<>4__this = this;
		<WaitForStateCheck>d__.desiredStates = desiredStates;
		<WaitForStateCheck>d__.<>1__state = -1;
		<WaitForStateCheck>d__.<>t__builder.Start<NetworkSystemPUN.<WaitForStateCheck>d__59>(ref <WaitForStateCheck>d__);
		return <WaitForStateCheck>d__.<>t__builder.Task;
	}

	private async Task<NetJoinResult> MakeOrFindRoom(string roomName, RoomConfig opts, int regionIndex = -1)
	{
		if (this.InRoom)
		{
			await this.InternalDisconnect();
		}
		this.currentRegionIndex = 0;
		bool flag = ((regionIndex >= 0) ? (await this.TryJoinRoomInRegion(roomName, opts, regionIndex)) : (await this.TryJoinRoom(roomName, opts)));
		NetJoinResult netJoinResult;
		if (this.internalState == NetworkSystemPUN.InternalState.Searching_JoinFailed_Full)
		{
			netJoinResult = NetJoinResult.Failed_Full;
		}
		else if (!flag)
		{
			netJoinResult = await this.TryCreateRoom(roomName, opts);
		}
		else
		{
			netJoinResult = NetJoinResult.Success;
		}
		return netJoinResult;
	}

	private async Task<bool> TryJoinRoom(string roomName, RoomConfig opts)
	{
		while (this.currentRegionIndex < this.regionNames.Length)
		{
			TaskAwaiter<bool> taskAwaiter = this.TryJoinRoomInRegion(roomName, opts, this.currentRegionIndex).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<bool> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (taskAwaiter.GetResult())
			{
				return true;
			}
			this.currentRegionIndex++;
		}
		return false;
	}

	private async Task<bool> TryJoinRoomInRegion(string roomName, RoomConfig opts, int regionIndex)
	{
		this.internalState = NetworkSystemPUN.InternalState.ConnectingToMaster;
		string text = this.regionNames[regionIndex];
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = text;
		this.currentRegionIndex = regionIndex;
		this.UpdateZoneInfo(opts.isPublic, null);
		PhotonNetwork.ConnectUsingSettings();
		TaskAwaiter<bool> taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.ConnectedToMaster }).GetAwaiter();
		TaskAwaiter<bool> taskAwaiter2;
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<bool>);
		}
		bool flag;
		if (!taskAwaiter.GetResult())
		{
			flag = false;
		}
		else
		{
			this.internalState = NetworkSystemPUN.InternalState.Searching_Joining;
			PhotonNetwork.JoinRoom(roomName, null);
			taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[]
			{
				NetworkSystemPUN.InternalState.Searching_Joined,
				NetworkSystemPUN.InternalState.Searching_JoinFailed,
				NetworkSystemPUN.InternalState.Searching_JoinFailed_Full
			}).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (!taskAwaiter.GetResult())
			{
				flag = false;
			}
			else if (this.internalState == NetworkSystemPUN.InternalState.Searching_JoinFailed_Full)
			{
				flag = false;
			}
			else
			{
				bool foundRoom = this.internalState == NetworkSystemPUN.InternalState.Searching_Joined;
				if (!foundRoom)
				{
					PhotonNetwork.Disconnect();
					this.internalState = NetworkSystemPUN.InternalState.Searching_Disconnecting;
					taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.Searching_Disconnected }).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult())
					{
						return false;
					}
				}
				flag = foundRoom;
			}
		}
		return flag;
	}

	private async Task<NetJoinResult> TryCreateRoom(string roomName, RoomConfig opts)
	{
		Debug.Log("returning to best region to create room");
		this.internalState = NetworkSystemPUN.InternalState.ConnectingToMaster;
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = this.regionNames[this.lowestPingRegionIndex];
		this.currentRegionIndex = this.lowestPingRegionIndex;
		this.UpdateZoneInfo(opts.isPublic, null);
		PhotonNetwork.ConnectUsingSettings();
		TaskAwaiter<bool> taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.ConnectedToMaster }).GetAwaiter();
		TaskAwaiter<bool> taskAwaiter2;
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<bool>);
		}
		NetJoinResult netJoinResult;
		if (!taskAwaiter.GetResult())
		{
			netJoinResult = NetJoinResult.Failed_Other;
		}
		else
		{
			this.internalState = NetworkSystemPUN.InternalState.Searching_Creating;
			PhotonNetwork.CreateRoom(roomName, opts.ToPUNOpts(), null, null);
			taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[]
			{
				NetworkSystemPUN.InternalState.Searching_Created,
				NetworkSystemPUN.InternalState.Searching_CreateFailed
			}).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (!taskAwaiter.GetResult())
			{
				netJoinResult = NetJoinResult.Failed_Other;
			}
			else if (this.internalState == NetworkSystemPUN.InternalState.Searching_CreateFailed)
			{
				netJoinResult = NetJoinResult.Failed_Other;
			}
			else
			{
				netJoinResult = NetJoinResult.FallbackCreated;
			}
		}
		return netJoinResult;
	}

	private async Task<NetJoinResult> JoinRandomPublicRoom(RoomConfig opts)
	{
		if (this.InRoom)
		{
			await this.InternalDisconnect();
		}
		this.internalState = NetworkSystemPUN.InternalState.ConnectingToMaster;
		object obj;
		if (!this.firstRoomJoin && opts.CustomProps.TryGetValue("gameMode", out obj) && !obj.ToString().StartsWith("city"))
		{
			this.firstRoomJoin = true;
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = this.regionNames[this.lowestPingRegionIndex];
			this.currentRegionIndex = this.lowestPingRegionIndex;
		}
		else if (!opts.IsJoiningWithFriends)
		{
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = this.GetRandomWeightedRegion();
			this.currentRegionIndex = Array.IndexOf<string>(this.regionNames, PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion);
		}
		this.UpdateZoneInfo(true, PhotonNetworkController.Instance.currentJoinTrigger.zone.GetName<GTZone>());
		PhotonNetwork.ConnectUsingSettings();
		TaskAwaiter<bool> taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.ConnectedToMaster }).GetAwaiter();
		TaskAwaiter<bool> taskAwaiter2;
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<bool>);
		}
		NetJoinResult netJoinResult;
		if (!taskAwaiter.GetResult())
		{
			netJoinResult = NetJoinResult.Failed_Other;
		}
		else
		{
			this.internalState = NetworkSystemPUN.InternalState.Searching_Joining;
			if (opts.IsJoiningWithFriends)
			{
				PhotonNetwork.JoinRandomRoom(opts.CustomProps, opts.MaxPlayers, MatchmakingMode.RandomMatching, null, null, opts.joinFriendIDs.ToArray<string>());
			}
			else
			{
				PhotonNetwork.JoinRandomRoom(opts.CustomProps, opts.MaxPlayers, MatchmakingMode.FillRoom, null, null, null);
			}
			taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[]
			{
				NetworkSystemPUN.InternalState.Searching_Joined,
				NetworkSystemPUN.InternalState.Searching_JoinFailed
			}).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (!taskAwaiter.GetResult())
			{
				netJoinResult = NetJoinResult.Failed_Other;
			}
			else if (this.internalState == NetworkSystemPUN.InternalState.Searching_JoinFailed)
			{
				this.internalState = NetworkSystemPUN.InternalState.Searching_Creating;
				if (opts.IsJoiningWithFriends)
				{
					PhotonNetwork.CreateRoom(NetworkSystem.GetRandomRoomName(), opts.ToPUNOpts(), null, opts.joinFriendIDs);
				}
				else
				{
					PhotonNetwork.CreateRoom(NetworkSystem.GetRandomRoomName(), opts.ToPUNOpts(), null, null);
				}
				taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[]
				{
					NetworkSystemPUN.InternalState.Searching_Created,
					NetworkSystemPUN.InternalState.Searching_CreateFailed
				}).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<bool>);
				}
				if (!taskAwaiter.GetResult())
				{
					netJoinResult = NetJoinResult.Failed_Other;
				}
				else if (this.internalState == NetworkSystemPUN.InternalState.Searching_CreateFailed)
				{
					netJoinResult = NetJoinResult.Failed_Other;
				}
				else
				{
					netJoinResult = NetJoinResult.FallbackCreated;
				}
			}
			else
			{
				netJoinResult = NetJoinResult.Success;
			}
		}
		return netJoinResult;
	}

	public override async Task<NetJoinResult> ConnectToRoom(string roomName, RoomConfig opts, int regionIndex = -1)
	{
		NetJoinResult netJoinResult;
		if (this.isWrongVersion)
		{
			netJoinResult = NetJoinResult.Failed_Other;
		}
		else if (base.netState != NetSystemState.Idle && base.netState != NetSystemState.InGame)
		{
			netJoinResult = NetJoinResult.Failed_Other;
		}
		else if (this.InRoom && roomName == this.RoomName)
		{
			netJoinResult = NetJoinResult.AlreadyInRoom;
		}
		else if (this.roomTask != null && !this.roomTask.IsCompleted)
		{
			netJoinResult = NetJoinResult.Failed_Other;
		}
		else
		{
			base.netState = NetSystemState.Connecting;
			NetJoinResult netJoinResult2;
			if (roomName != null)
			{
				this.roomTask = this.MakeOrFindRoom(roomName, opts, regionIndex);
				netJoinResult2 = await this.roomTask;
				this.roomTask = null;
			}
			else
			{
				this.roomTask = this.JoinRandomPublicRoom(opts);
				netJoinResult2 = await this.roomTask;
				this.roomTask = null;
			}
			if (netJoinResult2 == NetJoinResult.Failed_Full)
			{
				GorillaComputer.instance.roomFull = true;
				GorillaComputer.instance.UpdateScreen();
				this.ResetSystem();
				this.roomTask = null;
				netJoinResult = netJoinResult2;
			}
			else if (netJoinResult2 == NetJoinResult.Failed_Other)
			{
				this.ResetSystem();
				this.roomTask = null;
				netJoinResult = netJoinResult2;
			}
			else if (netJoinResult2 == NetJoinResult.AlreadyInRoom)
			{
				base.netState = NetSystemState.InGame;
				this.roomTask = null;
				netJoinResult = netJoinResult2;
			}
			else if (!this.InRoom)
			{
				GTDev.LogError<string>("NetworkSystem: room joined success but we have disconnected", null);
				netJoinResult = NetJoinResult.Failed_Other;
			}
			else
			{
				base.netState = NetSystemState.InGame;
				base.PlayerJoined(base.LocalPlayer);
				this.localRecorder.StartRecording();
				netJoinResult = netJoinResult2;
			}
		}
		return netJoinResult;
	}

	public override async Task JoinFriendsRoom(string userID, int actorIDToFollow, string keyToFollow, string shufflerToFollow)
	{
		bool foundFriend = false;
		float searchStartTime = Time.time;
		float timeToSpendSearching = 15f;
		Dictionary<string, global::PlayFab.ClientModels.SharedGroupDataRecord> dummyData = new Dictionary<string, global::PlayFab.ClientModels.SharedGroupDataRecord>();
		bool failedToJoinFriend = false;
		try
		{
			base.groupJoinInProgress = true;
			while (!foundFriend && searchStartTime + timeToSpendSearching > Time.time)
			{
				NetworkSystemPUN.<>c__DisplayClass66_0 CS$<>8__locals1 = new NetworkSystemPUN.<>c__DisplayClass66_0();
				CS$<>8__locals1.data = dummyData;
				CS$<>8__locals1.callbackFinished = false;
				PlayFabClientAPI.GetSharedGroupData(new global::PlayFab.ClientModels.GetSharedGroupDataRequest
				{
					Keys = new List<string> { keyToFollow },
					SharedGroupId = userID
				}, delegate(GetSharedGroupDataResult result)
				{
					CS$<>8__locals1.data = result.Data;
					Debug.Log(string.Format("Got friend follow data, {0} entries", CS$<>8__locals1.data.Count));
					CS$<>8__locals1.callbackFinished = true;
				}, delegate(PlayFabError error)
				{
					Debug.Log(string.Format("GetSharedGroupData returns error: {0}", error));
					CS$<>8__locals1.callbackFinished = true;
				}, null, null);
				while (!CS$<>8__locals1.callbackFinished)
				{
					await Task.Yield();
				}
				foreach (KeyValuePair<string, global::PlayFab.ClientModels.SharedGroupDataRecord> keyValuePair in CS$<>8__locals1.data)
				{
					if (keyValuePair.Key == keyToFollow)
					{
						string[] array = keyValuePair.Value.Value.Split("|", StringSplitOptions.None);
						if (array.Length == 2)
						{
							string roomID = NetworkSystem.ShuffleRoomName(array[0], shufflerToFollow.Substring(2, 8), false);
							int regionIndex = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".IndexOf(NetworkSystem.ShuffleRoomName(array[1], shufflerToFollow.Substring(0, 2), false));
							if (regionIndex >= 0 && regionIndex < NetworkSystem.Instance.regionNames.Length)
							{
								foundFriend = true;
								Player player;
								if (this.InRoom && PhotonNetwork.CurrentRoom.Players.TryGetValue(actorIDToFollow, out player) && player != null)
								{
									GorillaNot.instance.SendReport("possible kick attempt", player.UserId, player.NickName);
								}
								else if (this.RoomName != roomID)
								{
									await this.ReturnToSinglePlayer();
									Task<NetJoinResult> ConnectToRoomTask = this.ConnectToRoom(roomID, new RoomConfig
									{
										createIfMissing = false,
										isPublic = true,
										isJoinable = true
									}, regionIndex);
									await ConnectToRoomTask;
									failedToJoinFriend = ConnectToRoomTask.Result > NetJoinResult.Success;
									ConnectToRoomTask = null;
								}
								roomID = null;
							}
						}
					}
				}
				Dictionary<string, global::PlayFab.ClientModels.SharedGroupDataRecord>.Enumerator enumerator = default(Dictionary<string, global::PlayFab.ClientModels.SharedGroupDataRecord>.Enumerator);
				await Task.Delay(500);
				CS$<>8__locals1 = null;
			}
		}
		finally
		{
			base.groupJoinInProgress = false;
			if (failedToJoinFriend)
			{
				FriendshipGroupDetection.Instance.OnFailedToFollowParty();
			}
		}
	}

	public override void JoinPubWithFriends()
	{
		throw new NotImplementedException();
	}

	public override string GetRandomWeightedRegion()
	{
		float value = Random.value;
		int num = 0;
		for (int i = 0; i < this.regionData.Length; i++)
		{
			num += this.regionData[i].playersInRegion;
		}
		float num2 = 0f;
		int num3 = -1;
		while (num2 < value && num3 < this.regionData.Length - 1)
		{
			num3++;
			num2 += (float)this.regionData[num3].playersInRegion / (float)num;
		}
		return this.regionNames[num3];
	}

	public override async Task ReturnToSinglePlayer()
	{
		if (base.netState == NetSystemState.InGame || base.netState == NetSystemState.Connecting)
		{
			base.netState = NetSystemState.Disconnecting;
			this._taskCancelTokens.ForEach(delegate(CancellationTokenSource cts)
			{
				cts.Cancel();
				cts.Dispose();
			});
			this._taskCancelTokens.Clear();
			await this.InternalDisconnect();
			base.netState = NetSystemState.Idle;
		}
	}

	private async Task InternalDisconnect()
	{
		this.internalState = NetworkSystemPUN.InternalState.Internal_Disconnecting;
		PhotonNetwork.Disconnect();
		TaskAwaiter<bool> taskAwaiter = this.WaitForStateCheck(new NetworkSystemPUN.InternalState[] { NetworkSystemPUN.InternalState.Internal_Disconnected }).GetAwaiter();
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			TaskAwaiter<bool> taskAwaiter2;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<bool>);
		}
		if (!taskAwaiter.GetResult())
		{
			Debug.LogError("Failed to achieve internal disconnected state");
		}
		Object.Destroy(this.VoiceNetworkObject);
		base.UpdatePlayers();
		base.SinglePlayerStarted();
	}

	private void AddVoice()
	{
		this.SetupVoice();
	}

	private void SetupVoice()
	{
		try
		{
			this.punVoice = PhotonVoiceNetwork.Instance;
			this.VoiceNetworkObject = this.punVoice.gameObject;
			this.VoiceNetworkObject.name = "VoiceNetworkObject";
			this.VoiceNetworkObject.transform.parent = base.transform;
			this.VoiceNetworkObject.transform.localPosition = Vector3.zero;
			this.punVoice.LogLevel = this.VoiceSettings.LogLevel;
			this.punVoice.GlobalRecordersLogLevel = this.VoiceSettings.GlobalRecordersLogLevel;
			this.punVoice.GlobalSpeakersLogLevel = this.VoiceSettings.GlobalSpeakersLogLevel;
			this.punVoice.AutoConnectAndJoin = this.VoiceSettings.AutoConnectAndJoin;
			this.punVoice.AutoLeaveAndDisconnect = this.VoiceSettings.AutoLeaveAndDisconnect;
			this.punVoice.WorkInOfflineMode = this.VoiceSettings.WorkInOfflineMode;
			this.punVoice.AutoCreateSpeakerIfNotFound = this.VoiceSettings.CreateSpeakerIfNotFound;
			AppSettings appSettings = new AppSettings();
			appSettings.AppIdRealtime = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
			appSettings.AppIdVoice = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice;
			this.punVoice.Settings = appSettings;
			this.remoteVoiceAddedCallbacks.ForEach(delegate(Action<RemoteVoiceLink> callback)
			{
				this.punVoice.RemoteVoiceAdded += callback;
			});
			this.localRecorder = this.VoiceNetworkObject.GetComponent<GTRecorder>();
			if (this.localRecorder == null)
			{
				this.localRecorder = this.VoiceNetworkObject.AddComponent<GTRecorder>();
				if (VRRigCache.Instance != null && VRRigCache.Instance.localRig != null)
				{
					LoudSpeakerActivator[] componentsInChildren = VRRigCache.Instance.localRig.GetComponentsInChildren<LoudSpeakerActivator>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].SetRecorder((GTRecorder)this.localRecorder);
					}
				}
			}
			this.localRecorder.LogLevel = this.VoiceSettings.LogLevel;
			this.localRecorder.RecordOnlyWhenEnabled = this.VoiceSettings.RecordOnlyWhenEnabled;
			this.localRecorder.RecordOnlyWhenJoined = this.VoiceSettings.RecordOnlyWhenJoined;
			this.localRecorder.StopRecordingWhenPaused = this.VoiceSettings.StopRecordingWhenPaused;
			this.localRecorder.TransmitEnabled = this.VoiceSettings.TransmitEnabled;
			this.localRecorder.AutoStart = this.VoiceSettings.AutoStart;
			this.localRecorder.Encrypt = this.VoiceSettings.Encrypt;
			this.localRecorder.FrameDuration = this.VoiceSettings.FrameDuration;
			this.localRecorder.SamplingRate = this.VoiceSettings.SamplingRate;
			this.localRecorder.InterestGroup = this.VoiceSettings.InterestGroup;
			this.localRecorder.SourceType = this.VoiceSettings.InputSourceType;
			this.localRecorder.MicrophoneType = this.VoiceSettings.MicrophoneType;
			this.localRecorder.UseMicrophoneTypeFallback = this.VoiceSettings.UseFallback;
			this.localRecorder.VoiceDetection = this.VoiceSettings.Detect;
			this.localRecorder.VoiceDetectionThreshold = this.VoiceSettings.Threshold;
			this.localRecorder.Bitrate = this.VoiceSettings.Bitrate;
			this.localRecorder.VoiceDetectionDelayMs = this.VoiceSettings.Delay;
			this.localRecorder.DebugEchoMode = this.VoiceSettings.DebugEcho;
			this.punVoice.PrimaryRecorder = this.localRecorder;
			this.VoiceNetworkObject.AddComponent<VoiceToLoudness>();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception was thrown when trying to setup photon voice, please check microphone permissions.:/n" + ex.ToString());
		}
	}

	public override void AddRemoteVoiceAddedCallback(Action<RemoteVoiceLink> callback)
	{
		this.remoteVoiceAddedCallbacks.Add(callback);
	}

	public override GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isRoomObject = false)
	{
		if (PhotonNetwork.CurrentRoom == null)
		{
			return null;
		}
		if (isRoomObject)
		{
			return PhotonNetwork.InstantiateRoomObject(prefab.name, position, rotation, 0, null);
		}
		return PhotonNetwork.Instantiate(prefab.name, position, rotation, 0, null);
	}

	public override GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, int playerAuthID, bool isRoomObject = false)
	{
		return this.NetInstantiate(prefab, position, rotation, isRoomObject);
	}

	public override GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isRoomObject, byte group = 0, object[] data = null, NetworkRunner.OnBeforeSpawned callback = null)
	{
		if (PhotonNetwork.CurrentRoom == null)
		{
			return null;
		}
		if (isRoomObject)
		{
			return PhotonNetwork.InstantiateRoomObject(prefab.name, position, rotation, group, data);
		}
		return PhotonNetwork.Instantiate(prefab.name, position, rotation, group, data);
	}

	public override void NetDestroy(GameObject instance)
	{
		PhotonView photonView;
		if (instance.TryGetComponent<PhotonView>(out photonView) && photonView.AmOwner)
		{
			PhotonNetwork.Destroy(instance);
			return;
		}
		Object.Destroy(instance);
	}

	public override void SetPlayerObject(GameObject playerInstance, int? owningPlayerID = null)
	{
	}

	public override void CallRPC(MonoBehaviour component, NetworkSystem.RPC rpcMethod, bool sendToSelf = true)
	{
		RpcTarget rpcTarget = (sendToSelf ? RpcTarget.All : RpcTarget.Others);
		PhotonView.Get(component).RPC(rpcMethod.Method.Name, rpcTarget, new object[] { NetworkSystem.EmptyArgs });
	}

	public override void CallRPC<T>(MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args, bool sendToSelf = true)
	{
		RpcTarget rpcTarget = (sendToSelf ? RpcTarget.All : RpcTarget.Others);
		(ref args).SerializeToRPCData<T>();
		PhotonView.Get(component).RPC(rpcMethod.Method.Name, rpcTarget, new object[] { args.Data });
	}

	public override void CallRPC(MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message, bool sendToSelf = true)
	{
		RpcTarget rpcTarget = (sendToSelf ? RpcTarget.All : RpcTarget.Others);
		PhotonView.Get(component).RPC(rpcMethod.Method.Name, rpcTarget, new object[] { message });
	}

	public override void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod)
	{
		Player player = PhotonNetwork.CurrentRoom.GetPlayer(targetPlayerID, false);
		PhotonView.Get(component).RPC(rpcMethod.Method.Name, player, new object[] { NetworkSystem.EmptyArgs });
	}

	public override void CallRPC<T>(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args)
	{
		Player player = PhotonNetwork.CurrentRoom.GetPlayer(targetPlayerID, false);
		(ref args).SerializeToRPCData<T>();
		PhotonView.Get(component).RPC(rpcMethod.Method.Name, player, new object[] { args.Data });
	}

	public override void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message)
	{
		Player player = PhotonNetwork.CurrentRoom.GetPlayer(targetPlayerID, false);
		PhotonView.Get(component).RPC(rpcMethod.Method.Name, player, new object[] { message });
	}

	public override async Task AwaitSceneReady()
	{
		while (PhotonNetwork.LevelLoadingProgress < 1f)
		{
			await Task.Yield();
		}
	}

	public override NetPlayer GetLocalPlayer()
	{
		if (this.netPlayerCache.Count == 0)
		{
			base.UpdatePlayers();
		}
		foreach (NetPlayer netPlayer in this.netPlayerCache)
		{
			if (netPlayer.IsLocal)
			{
				return netPlayer;
			}
		}
		Debug.LogError("Somehow no local net players found. This shouldn't happen");
		return null;
	}

	public override NetPlayer GetPlayer(int PlayerID)
	{
		if (this.InRoom && !PhotonNetwork.CurrentRoom.Players.ContainsKey(PlayerID))
		{
			return null;
		}
		foreach (NetPlayer netPlayer in this.netPlayerCache)
		{
			if (netPlayer.ActorNumber == PlayerID)
			{
				return netPlayer;
			}
		}
		base.UpdatePlayers();
		foreach (NetPlayer netPlayer2 in this.netPlayerCache)
		{
			if (netPlayer2.ActorNumber == PlayerID)
			{
				return netPlayer2;
			}
		}
		GTDev.LogWarning<string>("There is no NetPlayer with this ID currently in game. Passed ID: " + PlayerID.ToString(), null);
		return null;
	}

	public override void SetMyNickName(string id)
	{
		if (!KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags) && !id.StartsWith("gorilla"))
		{
			Debug.Log("[KID] Trying to set custom nickname but that permission has been disallowed");
			PhotonNetwork.LocalPlayer.NickName = "gorilla";
			return;
		}
		PlayerPrefs.SetString("playerName", id);
		PhotonNetwork.LocalPlayer.NickName = id;
	}

	public override string GetMyNickName()
	{
		return PhotonNetwork.LocalPlayer.NickName;
	}

	public override string GetMyDefaultName()
	{
		return PhotonNetwork.LocalPlayer.DefaultName;
	}

	public override string GetNickName(int playerID)
	{
		NetPlayer player = this.GetPlayer(playerID);
		if (player != null)
		{
			return player.NickName;
		}
		return null;
	}

	public override string GetNickName(NetPlayer player)
	{
		return player.NickName;
	}

	public override void SetMyTutorialComplete()
	{
		bool flag = PlayerPrefs.GetString("didTutorial", "nope") == "done";
		if (!flag)
		{
			PlayerPrefs.SetString("didTutorial", "done");
			PlayerPrefs.Save();
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add("didTutorial", flag);
		PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
	}

	public override bool GetMyTutorialCompletion()
	{
		return PlayerPrefs.GetString("didTutorial", "nope") == "done";
	}

	public override bool GetPlayerTutorialCompletion(int playerID)
	{
		NetPlayer player = this.GetPlayer(playerID);
		if (player == null)
		{
			return false;
		}
		Player player2 = PhotonNetwork.CurrentRoom.GetPlayer(player.ActorNumber, false);
		if (player2 == null)
		{
			return false;
		}
		object obj;
		if (player2.CustomProperties.TryGetValue("didTutorial", out obj))
		{
			bool flag;
			bool flag2;
			if (obj is bool)
			{
				flag = (bool)obj;
				flag2 = 1 == 0;
			}
			else
			{
				flag2 = true;
			}
			return flag2 || flag;
		}
		return false;
	}

	public override string GetMyUserID()
	{
		return PhotonNetwork.LocalPlayer.UserId;
	}

	public override string GetUserID(int playerID)
	{
		NetPlayer player = this.GetPlayer(playerID);
		if (player != null)
		{
			return player.UserId;
		}
		return null;
	}

	public override string GetUserID(NetPlayer netPlayer)
	{
		Player playerRef = ((PunNetPlayer)netPlayer).PlayerRef;
		if (playerRef != null)
		{
			return playerRef.UserId;
		}
		return null;
	}

	public override int GlobalPlayerCount()
	{
		int num = 0;
		foreach (NetworkRegionInfo networkRegionInfo in this.regionData)
		{
			num += networkRegionInfo.playersInRegion;
		}
		return num;
	}

	public override bool IsObjectLocallyOwned(GameObject obj)
	{
		PhotonView photonView;
		return !this.IsOnline || !obj.TryGetComponent<PhotonView>(out photonView) || photonView.IsMine;
	}

	protected override void UpdateNetPlayerList()
	{
		if (!this.IsOnline)
		{
			bool flag = false;
			PunNetPlayer punNetPlayer = null;
			if (this.netPlayerCache.Count > 0)
			{
				for (int i = 0; i < this.netPlayerCache.Count; i++)
				{
					NetPlayer netPlayer = this.netPlayerCache[i];
					if (netPlayer.IsLocal)
					{
						punNetPlayer = (PunNetPlayer)netPlayer;
						flag = true;
					}
					else
					{
						this.playerPool.Return((PunNetPlayer)netPlayer);
					}
				}
				this.netPlayerCache.Clear();
			}
			if (!flag)
			{
				punNetPlayer = this.playerPool.Take();
				punNetPlayer.InitPlayer(PhotonNetwork.LocalPlayer);
			}
			this.netPlayerCache.Add(punNetPlayer);
		}
		else
		{
			Dictionary<int, Player>.ValueCollection values = PhotonNetwork.CurrentRoom.Players.Values;
			foreach (Player player in values)
			{
				bool flag2 = false;
				for (int j = 0; j < this.netPlayerCache.Count; j++)
				{
					if (player == ((PunNetPlayer)this.netPlayerCache[j]).PlayerRef)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					PunNetPlayer punNetPlayer2 = this.playerPool.Take();
					punNetPlayer2.InitPlayer(player);
					this.netPlayerCache.Add(punNetPlayer2);
				}
			}
			for (int k = 0; k < this.netPlayerCache.Count; k++)
			{
				PunNetPlayer punNetPlayer3 = (PunNetPlayer)this.netPlayerCache[k];
				bool flag3 = false;
				using (Dictionary<int, Player>.ValueCollection.Enumerator enumerator = values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == punNetPlayer3.PlayerRef)
						{
							flag3 = true;
							break;
						}
					}
				}
				if (!flag3)
				{
					this.playerPool.Return(punNetPlayer3);
					this.netPlayerCache.Remove(punNetPlayer3);
				}
			}
		}
		this.m_allNetPlayers = this.netPlayerCache.ToArray();
		this.m_otherNetPlayers = new NetPlayer[this.m_allNetPlayers.Length - 1];
		int num = 0;
		for (int l = 0; l < this.m_allNetPlayers.Length; l++)
		{
			NetPlayer netPlayer2 = this.m_allNetPlayers[l];
			if (netPlayer2.IsLocal)
			{
				num++;
			}
			else
			{
				int num2 = l - num;
				if (num2 == this.m_otherNetPlayers.Length)
				{
					break;
				}
				this.m_otherNetPlayers[num2] = netPlayer2;
			}
		}
	}

	public override bool IsObjectRoomObject(GameObject obj)
	{
		PhotonView component = obj.GetComponent<PhotonView>();
		if (component == null)
		{
			Debug.LogError("No photonview found on this Object, this shouldn't happen");
			return false;
		}
		return component.IsRoomView;
	}

	public override bool ShouldUpdateObject(GameObject obj)
	{
		return this.IsObjectLocallyOwned(obj);
	}

	public override bool ShouldWriteObjectData(GameObject obj)
	{
		return this.IsObjectLocallyOwned(obj);
	}

	public override int GetOwningPlayerID(GameObject obj)
	{
		PhotonView photonView;
		if (obj.TryGetComponent<PhotonView>(out photonView) && photonView.Owner != null)
		{
			return photonView.Owner.ActorNumber;
		}
		return -1;
	}

	public override bool ShouldSpawnLocally(int playerID)
	{
		return this.LocalPlayerID == playerID || (playerID == -1 && PhotonNetwork.MasterClient.IsLocal);
	}

	public override bool IsTotalAuthority()
	{
		return false;
	}

	public void OnConnectedtoMaster()
	{
		if (this.internalState == NetworkSystemPUN.InternalState.ConnectingToMaster)
		{
			this.internalState = NetworkSystemPUN.InternalState.ConnectedToMaster;
		}
		base.UpdatePlayers();
	}

	public void OnJoinedRoom()
	{
		if (this.internalState == NetworkSystemPUN.InternalState.Searching_Joining)
		{
			this.internalState = NetworkSystemPUN.InternalState.Searching_Joined;
		}
		else if (this.internalState == NetworkSystemPUN.InternalState.Searching_Creating)
		{
			this.internalState = NetworkSystemPUN.InternalState.Searching_Created;
		}
		this.AddVoice();
		base.UpdatePlayers();
		base.JoinedNetworkRoom();
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.Log("onJoinRoomFailed " + returnCode.ToString() + message);
		if (this.internalState == NetworkSystemPUN.InternalState.Searching_Joining)
		{
			if (returnCode == 32765)
			{
				this.internalState = NetworkSystemPUN.InternalState.Searching_JoinFailed_Full;
				return;
			}
			this.internalState = NetworkSystemPUN.InternalState.Searching_JoinFailed;
		}
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
		if (this.internalState == NetworkSystemPUN.InternalState.Searching_Creating)
		{
			this.internalState = NetworkSystemPUN.InternalState.Searching_CreateFailed;
		}
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
		base.UpdatePlayers();
		NetPlayer player = base.GetPlayer(newPlayer);
		base.PlayerJoined(player);
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		NetPlayer player = base.GetPlayer(otherPlayer);
		base.UpdatePlayers();
		base.PlayerLeft(player);
	}

	public async void OnDisconnected(DisconnectCause cause)
	{
		if (!ApplicationQuittingState.IsQuitting)
		{
			await base.RefreshNonce();
			if (this.internalState == NetworkSystemPUN.InternalState.Searching_Disconnecting)
			{
				this.internalState = NetworkSystemPUN.InternalState.Searching_Disconnected;
			}
			else if (this.internalState == NetworkSystemPUN.InternalState.PingGathering)
			{
				this.internalState = NetworkSystemPUN.InternalState.Internal_Disconnected;
			}
			else if (this.internalState == NetworkSystemPUN.InternalState.Internal_Disconnecting)
			{
				this.internalState = NetworkSystemPUN.InternalState.Internal_Disconnected;
			}
			else
			{
				base.UpdatePlayers();
				base.SinglePlayerStarted();
			}
		}
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		base.OnMasterClientSwitchedCallback(newMasterClient);
	}

	private ValueTuple<CancellationTokenSource, CancellationToken> GetCancellationToken()
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		this._taskCancelTokens.Add(cancellationTokenSource);
		return new ValueTuple<CancellationTokenSource, CancellationToken>(cancellationTokenSource, token);
	}

	public void ResetSystem()
	{
		if (this.VoiceNetworkObject)
		{
			Object.Destroy(this.VoiceNetworkObject);
		}
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = this.regionNames[this.lowestPingRegionIndex];
		this.currentRegionIndex = this.lowestPingRegionIndex;
		PhotonNetwork.Disconnect();
		this._taskCancelTokens.ForEach(delegate(CancellationTokenSource token)
		{
			token.Cancel();
			token.Dispose();
		});
		this._taskCancelTokens.Clear();
		this.internalState = NetworkSystemPUN.InternalState.Idle;
		base.netState = NetSystemState.Idle;
	}

	private void UpdateZoneInfo(bool roomIsPublic, string zoneName = null)
	{
		AuthenticationValues authenticationValues = this.GetAuthenticationValues();
		Dictionary<string, object> dictionary = ((authenticationValues != null) ? authenticationValues.AuthPostData : null) as Dictionary<string, object>;
		if (dictionary != null)
		{
			dictionary["Zone"] = ((zoneName != null) ? zoneName : ((ZoneManagement.instance.activeZones.Count > 0) ? ZoneManagement.instance.activeZones.First<GTZone>().GetName<GTZone>() : ""));
			dictionary["SubZone"] = GTSubZone.none.GetName<GTSubZone>();
			dictionary["IsPublic"] = roomIsPublic;
			authenticationValues.SetAuthPostData(dictionary);
			this.SetAuthenticationValues(authenticationValues);
		}
	}

	private NetworkRegionInfo[] regionData;

	private Task<NetJoinResult> roomTask;

	private ObjectPool<PunNetPlayer> playerPool;

	private NetPlayer[] m_allNetPlayers = new NetPlayer[0];

	private NetPlayer[] m_otherNetPlayers = new NetPlayer[0];

	private List<CancellationTokenSource> _taskCancelTokens = new List<CancellationTokenSource>();

	private PhotonVoiceNetwork punVoice;

	private GameObject VoiceNetworkObject;

	private NetworkSystemPUN.InternalState currentState;

	private bool firstRoomJoin;

	private enum InternalState
	{
		AwaitingAuth,
		Authenticated,
		PingGathering,
		StateCheckFailed,
		ConnectingToMaster,
		ConnectedToMaster,
		Idle,
		Internal_Disconnecting,
		Internal_Disconnected,
		Searching_Connecting,
		Searching_Connected,
		Searching_Joining,
		Searching_Joined,
		Searching_JoinFailed,
		Searching_JoinFailed_Full,
		Searching_Creating,
		Searching_Created,
		Searching_CreateFailed,
		Searching_Disconnecting,
		Searching_Disconnected
	}
}
