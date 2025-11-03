using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.Audio;
using Photon.Realtime;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSystemFusion : NetworkSystem
{
	public NetworkRunner runner { get; private set; }

	public override bool IsOnline
	{
		get
		{
			return this.runner != null && !this.runner.IsSinglePlayer;
		}
	}

	public override bool InRoom
	{
		get
		{
			return this.runner != null && this.runner.State != NetworkRunner.States.Shutdown && !this.runner.IsSinglePlayer && this.runner.IsConnectedToServer;
		}
	}

	public override string RoomName
	{
		get
		{
			SessionInfo sessionInfo = this.runner.SessionInfo;
			if (sessionInfo == null)
			{
				return null;
			}
			return sessionInfo.Name;
		}
	}

	public override string RoomStringStripped()
	{
		SessionInfo sessionInfo = this.runner.SessionInfo;
		NetworkSystem.reusableSB.Clear();
		NetworkSystem.reusableSB.AppendFormat("Room: '{0}' ", (sessionInfo.Name.Length < 20) ? sessionInfo.Name : sessionInfo.Name.Remove(20));
		NetworkSystem.reusableSB.AppendFormat("{0},{1} {3}/{2} players.", new object[]
		{
			sessionInfo.IsVisible ? "visible" : "hidden",
			sessionInfo.IsOpen ? "open" : "closed",
			sessionInfo.MaxPlayers,
			sessionInfo.PlayerCount
		});
		NetworkSystem.reusableSB.Append("\ncustomProps: {");
		NetworkSystem.reusableSB.AppendFormat("joinedGameMode={0}, ", (RoomSystem.RoomGameMode.Length < 50) ? RoomSystem.RoomGameMode : RoomSystem.RoomGameMode.Remove(50));
		IDictionary properties = sessionInfo.Properties;
		Debug.Log(RoomSystem.RoomGameMode.ToString());
		if (properties.Contains("gameMode"))
		{
			object obj = properties["gameMode"];
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
			SessionProperty sessionProperty;
			this.runner.SessionInfo.Properties.TryGetValue("gameMode", out sessionProperty);
			if (sessionProperty != null)
			{
				return (string)sessionProperty.PropertyValue;
			}
			return null;
		}
	}

	public override string CurrentRegion
	{
		get
		{
			SessionInfo sessionInfo = this.runner.SessionInfo;
			if (sessionInfo == null)
			{
				return null;
			}
			return sessionInfo.Region;
		}
	}

	public override bool SessionIsPrivate
	{
		get
		{
			NetworkRunner runner = this.runner;
			bool? flag;
			if (runner == null)
			{
				flag = null;
			}
			else
			{
				SessionInfo sessionInfo = runner.SessionInfo;
				flag = ((sessionInfo != null) ? new bool?(!sessionInfo.IsVisible) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}
	}

	public override int LocalPlayerID
	{
		get
		{
			return this.runner.LocalPlayer.PlayerId;
		}
	}

	public override string CurrentPhotonBackend
	{
		get
		{
			return "Fusion";
		}
	}

	public override double SimTime
	{
		get
		{
			return (double)this.runner.SimulationTime;
		}
	}

	public override float SimDeltaTime
	{
		get
		{
			return this.runner.DeltaTime;
		}
	}

	public override int SimTick
	{
		get
		{
			return this.runner.Tick.Raw;
		}
	}

	public override int TickRate
	{
		get
		{
			return this.runner.TickRate;
		}
	}

	public override int ServerTimestamp
	{
		get
		{
			return this.runner.Tick.Raw;
		}
	}

	public override int RoomPlayerCount
	{
		get
		{
			return this.runner.SessionInfo.PlayerCount;
		}
	}

	public override VoiceConnection VoiceConnection
	{
		get
		{
			return this.FusionVoice;
		}
	}

	public override bool IsMasterClient
	{
		get
		{
			NetworkRunner runner = this.runner;
			return runner == null || runner.IsSharedModeMasterClient;
		}
	}

	public override NetPlayer MasterClient
	{
		get
		{
			if (this.runner != null && this.runner.IsSharedModeMasterClient)
			{
				return base.GetPlayer(this.runner.LocalPlayer);
			}
			if (!(global::GorillaGameModes.GameMode.ActiveNetworkHandler != null))
			{
				return null;
			}
			return base.GetPlayer(global::GorillaGameModes.GameMode.ActiveNetworkHandler.Object.StateAuthority);
		}
	}

	public override async void Initialise()
	{
		base.Initialise();
		this.myObjectProvider = new CustomObjectProvider();
		base.netState = NetSystemState.Initialization;
		this.internalState = NetworkSystemFusion.InternalState.Idle;
		await this.ReturnToSinglePlayer();
		this.AwaitAuth();
		this.CreateRegionCrawler();
		GameModeSerializer.FusionGameModeOwnerChanged = (Action<NetPlayer>)Delegate.Combine(GameModeSerializer.FusionGameModeOwnerChanged, new Action<NetPlayer>(base.OnMasterClientSwitchedCallback));
		this.OnMasterClientSwitchedEvent += new Action<NetPlayer>(this.OnMasterSwitch);
		base.netState = NetSystemState.Idle;
		this.playerPool = new ObjectPool<FusionNetPlayer>(10);
		base.UpdatePlayers();
	}

	private void CreateRegionCrawler()
	{
		GameObject gameObject = new GameObject("[Network Crawler]");
		gameObject.transform.SetParent(base.transform);
		this.regionCrawler = gameObject.AddComponent<FusionRegionCrawler>();
	}

	private async Task AwaitAuth()
	{
		this.internalState = NetworkSystemFusion.InternalState.AwaitingAuth;
		while (this.cachedPlayfabAuth == null)
		{
			await Task.Yield();
		}
		this.internalState = NetworkSystemFusion.InternalState.Idle;
		base.netState = NetSystemState.Idle;
	}

	public override void FinishAuthenticating()
	{
		if (this.cachedPlayfabAuth != null)
		{
			Debug.Log("AUTHED");
			return;
		}
		Debug.LogError("Authentication Failed");
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
		else
		{
			base.netState = NetSystemState.Connecting;
			Utils.Log("Connecting to:" + (string.IsNullOrEmpty(roomName) ? "random room" : roomName));
			NetJoinResult netJoinResult2;
			if (!string.IsNullOrEmpty(roomName))
			{
				Task<NetJoinResult> makeOrJoinTask = this.MakeOrJoinRoom(roomName, opts);
				await makeOrJoinTask;
				netJoinResult2 = makeOrJoinTask.Result;
				makeOrJoinTask = null;
			}
			else
			{
				Task<NetJoinResult> makeOrJoinTask = this.JoinRandomPublicRoom(opts);
				await makeOrJoinTask;
				netJoinResult2 = makeOrJoinTask.Result;
				makeOrJoinTask = null;
			}
			if (netJoinResult2 == NetJoinResult.Failed_Full || netJoinResult2 == NetJoinResult.Failed_Other)
			{
				this.ResetSystem();
				netJoinResult = netJoinResult2;
			}
			else if (netJoinResult2 == NetJoinResult.AlreadyInRoom)
			{
				base.netState = NetSystemState.InGame;
				netJoinResult = netJoinResult2;
			}
			else
			{
				base.UpdatePlayers();
				base.netState = NetSystemState.InGame;
				Utils.Log("Connect to room result: " + netJoinResult2.ToString());
				netJoinResult = netJoinResult2;
			}
		}
		return netJoinResult;
	}

	private async Task<bool> Connect(global::Fusion.GameMode mode, string targetSessionName, RoomConfig opts)
	{
		if (this.runner != null)
		{
			bool goingBetweenRooms = this.InRoom && mode != global::Fusion.GameMode.Single;
			await this.CloseRunner(ShutdownReason.Ok);
			await Task.Yield();
			if (goingBetweenRooms)
			{
				base.SinglePlayerStarted();
				await Task.Yield();
			}
		}
		if (this.volatileNetObj)
		{
			Debug.LogError("Volatile net obj should not exist - destroying and recreating");
			Object.Destroy(this.volatileNetObj);
		}
		this.volatileNetObj = new GameObject("VolatileFusionObj");
		this.volatileNetObj.transform.parent = base.transform;
		this.runner = this.volatileNetObj.AddComponent<NetworkRunner>();
		this.internalRPCProvider = this.runner.AddBehaviour<FusionInternalRPCs>();
		this.callbackHandler = this.volatileNetObj.AddComponent<FusionCallbackHandler>();
		this.callbackHandler.Setup(this);
		this.AttachCallbackTargets();
		this.lastConnectAttempt_WasFull = false;
		this.internalState = NetworkSystemFusion.InternalState.ConnectingToRoom;
		Hashtable customProps = opts.CustomProps;
		Dictionary<string, SessionProperty> dictionary = ((customProps != null) ? customProps.ToPropDict() : null);
		this.myObjectProvider.SceneObjects = this.SceneObjectsToAttach;
		NetworkSceneManagerDefault networkSceneManagerDefault = this.volatileNetObj.AddComponent<NetworkSceneManagerDefault>();
		Task<global::Fusion.StartGameResult> startupTask = this.runner.StartGame(new StartGameArgs
		{
			IsVisible = new bool?(opts.isPublic),
			IsOpen = new bool?(opts.isJoinable),
			GameMode = mode,
			SessionName = targetSessionName,
			PlayerCount = new int?((int)opts.MaxPlayers),
			SceneManager = networkSceneManagerDefault,
			AuthValues = this.cachedPlayfabAuth,
			SessionProperties = dictionary,
			EnableClientSessionCreation = new bool?(opts.createIfMissing),
			ObjectProvider = this.myObjectProvider
		});
		await startupTask;
		Utils.Log("Startuptask finished : " + startupTask.Result.ToString());
		bool flag;
		if (!startupTask.Result.Ok)
		{
			base.CurrentRoom = null;
			flag = startupTask.Result.Ok;
		}
		else
		{
			if (this.cachedNetSceneObjects.Count > 0)
			{
				foreach (NetworkObject networkObject in this.cachedNetSceneObjects)
				{
					this.registrationQueue.Enqueue(networkObject);
				}
			}
			this.AttachSceneObjects(false);
			this.AddVoice();
			base.CurrentRoom = opts;
			if (this.IsTotalAuthority() || this.runner.IsSharedModeMasterClient)
			{
				opts.SetFusionOpts(this.runner);
			}
			this.SetMyNickName(GorillaComputer.instance.savedName);
			flag = startupTask.Result.Ok;
		}
		return flag;
	}

	private async Task<NetJoinResult> MakeOrJoinRoom(string roomName, RoomConfig opts)
	{
		int currentRegionIndex = 0;
		bool flag = false;
		opts.createIfMissing = false;
		Task<bool> connectTask;
		while (currentRegionIndex < this.regionNames.Length && !flag)
		{
			try
			{
				PhotonAppSettings.Global.AppSettings.FixedRegion = this.regionNames[currentRegionIndex];
				this.internalState = NetworkSystemFusion.InternalState.Searching_Joining;
				connectTask = this.Connect(global::Fusion.GameMode.Shared, roomName, opts);
				await connectTask;
				flag = connectTask.Result;
				if (!flag)
				{
					if (this.lastConnectAttempt_WasFull)
					{
						Utils.Log("Found room but it was full");
						break;
					}
					Utils.Log("Region incrimenting");
					currentRegionIndex++;
				}
				connectTask = null;
			}
			catch (Exception ex)
			{
				Debug.LogError("MakeOrJoinRoom - message: " + ex.Message + "\nStacktrace : " + ex.StackTrace);
				return NetJoinResult.Failed_Other;
			}
		}
		if (this.lastConnectAttempt_WasFull)
		{
			PhotonAppSettings.Global.AppSettings.FixedRegion = "";
			return NetJoinResult.Failed_Full;
		}
		if (flag)
		{
			return NetJoinResult.Success;
		}
		PhotonAppSettings.Global.AppSettings.FixedRegion = "";
		opts.createIfMissing = true;
		connectTask = this.Connect(global::Fusion.GameMode.Shared, roomName, opts);
		await connectTask;
		Utils.Log("made room?");
		if (!connectTask.Result)
		{
			Debug.LogError("NS-FUS] Failed to create private room");
			return NetJoinResult.Failed_Other;
		}
		while (!this.runner.SessionInfo.IsValid)
		{
			await Task.Yield();
		}
		return NetJoinResult.FallbackCreated;
	}

	private async Task<NetJoinResult> JoinRandomPublicRoom(RoomConfig opts)
	{
		bool shouldCreateIfNone = opts.createIfMissing;
		PhotonAppSettings.Global.AppSettings.FixedRegion = "";
		this.internalState = NetworkSystemFusion.InternalState.Searching_Joining;
		opts.createIfMissing = false;
		Task<bool> connectTask = this.Connect(global::Fusion.GameMode.Shared, null, opts);
		await connectTask;
		NetJoinResult netJoinResult;
		if (!connectTask.Result && shouldCreateIfNone)
		{
			opts.createIfMissing = shouldCreateIfNone;
			Task<bool> createTask = this.Connect(global::Fusion.GameMode.Shared, NetworkSystem.GetRandomRoomName(), opts);
			await createTask;
			if (!createTask.Result)
			{
				Debug.LogError("NS-FUS] Failed to create public room");
				netJoinResult = NetJoinResult.Failed_Other;
			}
			else
			{
				opts.SetFusionOpts(this.runner);
				netJoinResult = NetJoinResult.FallbackCreated;
			}
		}
		else
		{
			netJoinResult = NetJoinResult.Success;
		}
		return netJoinResult;
	}

	public override async Task JoinFriendsRoom(string userID, int actorIDToFollow, string keyToFollow, string shufflerToFollow)
	{
		bool foundFriend = false;
		float searchStartTime = Time.time;
		float timeToSpendSearching = 15f;
		Dictionary<string, global::PlayFab.ClientModels.SharedGroupDataRecord> dummyData = new Dictionary<string, global::PlayFab.ClientModels.SharedGroupDataRecord>();
		try
		{
			base.groupJoinInProgress = true;
			while (!foundFriend && searchStartTime + timeToSpendSearching > Time.time)
			{
				NetworkSystemFusion.<>c__DisplayClass61_0 CS$<>8__locals1 = new NetworkSystemFusion.<>c__DisplayClass61_0();
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
								NetPlayer player = this.GetPlayer(actorIDToFollow);
								if (this.InRoom && this.GetPlayer(actorIDToFollow) != null)
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
									NetJoinResult result2 = ConnectToRoomTask.Result;
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
		}
	}

	public override void JoinPubWithFriends()
	{
		throw new NotImplementedException();
	}

	public override async Task ReturnToSinglePlayer()
	{
		if (base.netState == NetSystemState.InGame || base.netState == NetSystemState.Initialization)
		{
			base.netState = NetSystemState.Disconnecting;
			Utils.Log("Returning to single player");
			if (this.runner)
			{
				await this.CloseRunner(ShutdownReason.Ok);
				await Task.Yield();
				Utils.Log("Connect in return to single player");
			}
			base.netState = NetSystemState.Idle;
			this.internalState = NetworkSystemFusion.InternalState.Idle;
			base.SinglePlayerStarted();
		}
	}

	private async Task CloseRunner(ShutdownReason reason = ShutdownReason.Ok)
	{
		this.internalState = NetworkSystemFusion.InternalState.Disconnecting;
		try
		{
			await this.runner.Shutdown(true, reason, false);
		}
		catch (Exception ex)
		{
			StackFrame frame = new StackTrace(ex, true).GetFrame(0);
			int fileLineNumber = frame.GetFileLineNumber();
			Debug.LogError(string.Concat(new string[]
			{
				ex.Message,
				" File:",
				frame.GetFileName(),
				" line: ",
				fileLineNumber.ToString()
			}));
		}
		if (Application.isPlaying)
		{
			Object.Destroy(this.volatileNetObj);
		}
		else
		{
			Object.DestroyImmediate(this.volatileNetObj);
		}
		this.internalState = NetworkSystemFusion.InternalState.Disconnected;
	}

	public async void MigrateHost(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
		Utils.Log("HOSTTEST : MigrateHostTriggered, returning to single player!");
		await this.ReturnToSinglePlayer();
	}

	public async void ResetSystem()
	{
		if (Application.isPlaying)
		{
			base.StopAllCoroutines();
			await this.Connect(global::Fusion.GameMode.Single, "--", RoomConfig.SPConfig());
			Utils.Log("Connect in return to single player");
			base.netState = NetSystemState.Idle;
			this.internalState = NetworkSystemFusion.InternalState.Idle;
		}
	}

	private void AddVoice()
	{
		this.SetupVoice();
	}

	private void SetupVoice()
	{
		Utils.Log("<color=orange>Adding Voice Stuff</color>");
		this.FusionVoice = this.volatileNetObj.AddComponent<VoiceConnection>();
		this.FusionVoice.LogLevel = this.VoiceSettings.LogLevel;
		this.FusionVoice.GlobalRecordersLogLevel = this.VoiceSettings.GlobalRecordersLogLevel;
		this.FusionVoice.GlobalSpeakersLogLevel = this.VoiceSettings.GlobalSpeakersLogLevel;
		this.FusionVoice.AutoCreateSpeakerIfNotFound = this.VoiceSettings.CreateSpeakerIfNotFound;
		Photon.Realtime.AppSettings appSettings = new Photon.Realtime.AppSettings();
		appSettings.AppIdFusion = PhotonAppSettings.Global.AppSettings.AppIdFusion;
		appSettings.AppIdVoice = PhotonAppSettings.Global.AppSettings.AppIdVoice;
		this.FusionVoice.Settings = appSettings;
		this.remoteVoiceAddedCallbacks.ForEach(delegate(Action<RemoteVoiceLink> callback)
		{
			this.FusionVoice.RemoteVoiceAdded += callback;
		});
		this.localRecorder = this.volatileNetObj.AddComponent<Recorder>();
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
		this.localRecorder.UserData = this.runner.UserId;
		this.FusionVoice.PrimaryRecorder = this.localRecorder;
		this.volatileNetObj.AddComponent<VoiceToLoudness>();
	}

	public override void AddRemoteVoiceAddedCallback(Action<RemoteVoiceLink> callback)
	{
		this.remoteVoiceAddedCallbacks.Add(callback);
	}

	private void AttachCallbackTargets()
	{
		this.runner.AddCallbacks(this.objectsThatNeedCallbacks.ToArray());
	}

	public void RegisterForNetworkCallbacks(INetworkRunnerCallbacks callbacks)
	{
		if (!this.objectsThatNeedCallbacks.Contains(callbacks))
		{
			this.objectsThatNeedCallbacks.Add(callbacks);
		}
		if (this.runner != null)
		{
			this.runner.AddCallbacks(new INetworkRunnerCallbacks[] { callbacks });
		}
	}

	private async void AttachSceneObjects(bool onlyCached = false)
	{
		if (!onlyCached)
		{
			this.SceneObjectsToAttach.ForEach(delegate(GameObject obj)
			{
				if (!this.cachedNetSceneObjects.Exists((NetworkObject o) => o.gameObject == obj.gameObject))
				{
					NetworkObject component = obj.GetComponent<NetworkObject>();
					if (component == null)
					{
						Debug.LogWarning("no network object on scene item - " + obj.name);
						return;
					}
					this.cachedNetSceneObjects.Add(component);
					this.registrationQueue.Enqueue(component);
				}
			});
		}
		await Task.Delay(5);
		this.ProcessRegistrationQueue();
	}

	public override void AttachObjectInGame(GameObject item)
	{
		base.AttachObjectInGame(item);
		NetworkObject component = item.GetComponent<NetworkObject>();
		if ((component != null && !this.cachedNetSceneObjects.Contains(component)) || !component.IsValid)
		{
			this.cachedNetSceneObjects.AddIfNew(component);
			this.registrationQueue.Enqueue(component);
			this.ProcessRegistrationQueue();
		}
	}

	private void ProcessRegistrationQueue()
	{
		if (this.isProcessingQueue)
		{
			Debug.LogError("Queue is still processing");
			return;
		}
		this.isProcessingQueue = true;
		List<NetworkObject> list = new List<NetworkObject>();
		SceneRef sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
		while (this.registrationQueue.Count > 0)
		{
			NetworkObject networkObject = this.registrationQueue.Dequeue();
			if (this.InRoom && !networkObject.IsValid && !networkObject.Id.IsValid && networkObject.Runner == null)
			{
				try
				{
					list.Add(networkObject);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					this.isProcessingQueue = false;
					this.runner.RegisterSceneObjects(sceneRef, list.ToArray(), default(NetworkSceneLoadId));
					this.ProcessRegistrationQueue();
					break;
				}
			}
		}
		this.runner.RegisterSceneObjects(sceneRef, list.ToArray(), default(NetworkSceneLoadId));
		this.isProcessingQueue = false;
	}

	public override GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isRoomObject = false)
	{
		Utils.Log("Net instantiate Fusion: " + prefab.name);
		try
		{
			return this.runner.Spawn(prefab, new Vector3?(position), new Quaternion?(rotation), new PlayerRef?(this.runner.LocalPlayer), null, (NetworkSpawnFlags)0).gameObject;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}
		return null;
	}

	public override GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, int playerAuthID, bool isRoomObject = false)
	{
		foreach (PlayerRef playerRef in this.runner.ActivePlayers)
		{
			if (playerRef.PlayerId == playerAuthID)
			{
				Utils.Log("Net instantiate Fusion: " + prefab.name);
				return this.runner.Spawn(prefab, new Vector3?(position), new Quaternion?(rotation), new PlayerRef?(playerRef), null, (NetworkSpawnFlags)0).gameObject;
			}
		}
		Debug.LogError(string.Format("Couldn't find player with ID: {0}, cancelling requested spawn...", playerAuthID));
		return null;
	}

	public override GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isRoomObject, byte group = 0, object[] data = null, NetworkRunner.OnBeforeSpawned callback = null)
	{
		Utils.Log("Net instantiate Fusion: " + prefab.name);
		return this.runner.Spawn(prefab, new Vector3?(position), new Quaternion?(rotation), new PlayerRef?(this.runner.LocalPlayer), callback, (NetworkSpawnFlags)0).gameObject;
	}

	public override void NetDestroy(GameObject instance)
	{
		NetworkObject networkObject;
		if (instance.TryGetComponent<NetworkObject>(out networkObject))
		{
			this.runner.Despawn(networkObject);
			return;
		}
		Object.Destroy(instance);
	}

	public override bool ShouldSpawnLocally(int playerID)
	{
		if (this.runner.GameMode == global::Fusion.GameMode.Shared)
		{
			return this.runner.LocalPlayer.PlayerId == playerID || (playerID == -1 && this.runner.IsSharedModeMasterClient);
		}
		return this.runner.GameMode != global::Fusion.GameMode.Client;
	}

	public override void CallRPC(MonoBehaviour component, NetworkSystem.RPC rpcMethod, bool sendToSelf = true)
	{
		Utils.Log(rpcMethod.GetDelegateName() + "RPC called!");
		foreach (PlayerRef playerRef in this.runner.ActivePlayers)
		{
			if (!sendToSelf)
			{
				playerRef != this.runner.LocalPlayer;
			}
		}
	}

	public override void CallRPC<T>(MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args, bool sendToSelf = true)
	{
		Utils.Log(rpcMethod.GetDelegateName() + "RPC called!");
		(ref args).SerializeToRPCData<T>();
		foreach (PlayerRef playerRef in this.runner.ActivePlayers)
		{
			if (!sendToSelf)
			{
				playerRef != this.runner.LocalPlayer;
			}
		}
	}

	public override void CallRPC(MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message, bool sendToSelf = true)
	{
		foreach (PlayerRef playerRef in this.runner.ActivePlayers)
		{
			if (!sendToSelf)
			{
				playerRef != this.runner.LocalPlayer;
			}
		}
	}

	public override void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod)
	{
		this.GetPlayerRef(targetPlayerID);
		Utils.Log(rpcMethod.GetDelegateName() + "RPC called!");
	}

	public override void CallRPC<T>(int targetPlayerID, MonoBehaviour component, NetworkSystem.RPC rpcMethod, RPCArgBuffer<T> args)
	{
		Utils.Log(rpcMethod.GetDelegateName() + "RPC called!");
		this.GetPlayerRef(targetPlayerID);
	}

	public override void CallRPC(int targetPlayerID, MonoBehaviour component, NetworkSystem.StringRPC rpcMethod, string message)
	{
		this.GetPlayerRef(targetPlayerID);
	}

	public override void NetRaiseEventReliable(byte eventCode, object data)
	{
		byte[] array = data.ByteSerialize();
		FusionCallbackHandler.RPC_OnEventRaisedReliable(this.runner, eventCode, array, false, null, default(RpcInfo));
	}

	public override void NetRaiseEventUnreliable(byte eventCode, object data)
	{
		byte[] array = data.ByteSerialize();
		FusionCallbackHandler.RPC_OnEventRaisedUnreliable(this.runner, eventCode, array, false, null, default(RpcInfo));
	}

	public override void NetRaiseEventReliable(byte eventCode, object data, NetEventOptions opts)
	{
		byte[] array = data.ByteSerialize();
		byte[] array2 = opts.ByteSerialize();
		FusionCallbackHandler.RPC_OnEventRaisedReliable(this.runner, eventCode, array, true, array2, default(RpcInfo));
	}

	public override void NetRaiseEventUnreliable(byte eventCode, object data, NetEventOptions opts)
	{
		byte[] array = data.ByteSerialize();
		byte[] array2 = opts.ByteSerialize();
		FusionCallbackHandler.RPC_OnEventRaisedUnreliable(this.runner, eventCode, array, true, array2, default(RpcInfo));
	}

	public override string GetRandomWeightedRegion()
	{
		throw new NotImplementedException();
	}

	public override async Task AwaitSceneReady()
	{
		while (this.runner.SceneManager.IsBusy)
		{
			await Task.Yield();
		}
		for (float counter = 0f; counter < 0.5f; counter += Time.deltaTime)
		{
			await Task.Yield();
		}
	}

	public void OnJoinedSession()
	{
	}

	public void OnJoinFailed(NetConnectFailedReason reason)
	{
		switch (reason)
		{
		case NetConnectFailedReason.Timeout:
		case NetConnectFailedReason.ServerRefused:
			break;
		case NetConnectFailedReason.ServerFull:
			this.lastConnectAttempt_WasFull = true;
			break;
		default:
			return;
		}
	}

	public void OnDisconnectedFromSession()
	{
		Utils.Log("On Disconnected");
		this.internalState = NetworkSystemFusion.InternalState.Disconnected;
		base.UpdatePlayers();
	}

	public void OnRunnerShutDown()
	{
		Utils.Log("Runner shutdown callback");
		if (this.internalState == NetworkSystemFusion.InternalState.Disconnecting)
		{
			this.internalState = NetworkSystemFusion.InternalState.Disconnected;
		}
	}

	public void OnFusionPlayerJoined(PlayerRef player)
	{
		this.AwaitJoiningPlayerClientReady(player);
	}

	private async Task AwaitJoiningPlayerClientReady(PlayerRef player)
	{
		base.UpdatePlayers();
		if (this.runner != null && player == this.runner.LocalPlayer && !this.runner.IsSinglePlayer)
		{
			Utils.Log("JoinedNetworkRoom");
			await Task.Delay(8);
			base.JoinedNetworkRoom();
		}
		if (this.runner != null && player == this.runner.LocalPlayer && this.runner.IsSinglePlayer)
		{
			base.SinglePlayerStarted();
		}
		await Task.Delay(200);
		NetPlayer joiningPlayer = base.GetPlayer(player);
		if (joiningPlayer == null)
		{
			Debug.LogError("Joining player doesnt have a NetPlayer somehow, this shouldnt happen");
		}
		while (joiningPlayer.NickName.IsNullOrEmpty())
		{
			await Task.Delay(1);
		}
		base.PlayerJoined(joiningPlayer);
	}

	public void OnFusionPlayerLeft(PlayerRef player)
	{
		if (this.IsTotalAuthority())
		{
			NetworkObject playerObject = this.runner.GetPlayerObject(player);
			if (playerObject != null)
			{
				Utils.Log("Destroying player object for leaving player!");
				this.NetDestroy(playerObject.gameObject);
			}
			else
			{
				Utils.Log("Player left without destroying an avatar for it somehow?");
			}
		}
		NetPlayer player2 = base.GetPlayer(player);
		if (player2 == null)
		{
			Debug.LogError("Joining player doesnt have a NetPlayer somehow, this shouldnt happen");
		}
		base.PlayerLeft(player2);
		base.UpdatePlayers();
	}

	protected override void UpdateNetPlayerList()
	{
		if (this.runner == null)
		{
			if (this.netPlayerCache.Count <= 1)
			{
				if (this.netPlayerCache.Exists((NetPlayer p) => p.IsLocal))
				{
					goto IL_0084;
				}
			}
			this.netPlayerCache.ForEach(delegate(NetPlayer p)
			{
				this.playerPool.Return((FusionNetPlayer)p);
			});
			this.netPlayerCache.Clear();
			this.netPlayerCache.Add(new FusionNetPlayer(default(PlayerRef)));
			return;
		}
		IL_0084:
		NetPlayer[] array;
		if (this.runner.IsSinglePlayer)
		{
			if (this.netPlayerCache.Count == 1 && this.netPlayerCache[0].IsLocal)
			{
				return;
			}
			bool flag = false;
			array = this.netPlayerCache.ToArray();
			if (this.netPlayerCache.Count > 0)
			{
				foreach (NetPlayer netPlayer in array)
				{
					if (((FusionNetPlayer)netPlayer).PlayerRef == this.runner.LocalPlayer)
					{
						flag = true;
					}
					else
					{
						this.playerPool.Return((FusionNetPlayer)netPlayer);
						this.netPlayerCache.Remove(netPlayer);
					}
				}
			}
			if (!flag)
			{
				FusionNetPlayer fusionNetPlayer = this.playerPool.Take();
				fusionNetPlayer.InitPlayer(this.runner.LocalPlayer);
				this.netPlayerCache.Add(fusionNetPlayer);
			}
		}
		foreach (PlayerRef playerRef in this.runner.ActivePlayers)
		{
			bool flag2 = false;
			for (int j = 0; j < this.netPlayerCache.Count; j++)
			{
				if (playerRef == ((FusionNetPlayer)this.netPlayerCache[j]).PlayerRef)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				FusionNetPlayer fusionNetPlayer2 = this.playerPool.Take();
				fusionNetPlayer2.InitPlayer(playerRef);
				this.netPlayerCache.Add(fusionNetPlayer2);
			}
		}
		array = this.netPlayerCache.ToArray();
		foreach (NetPlayer netPlayer2 in array)
		{
			bool flag3 = false;
			using (IEnumerator<PlayerRef> enumerator = this.runner.ActivePlayers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == ((FusionNetPlayer)netPlayer2).PlayerRef)
					{
						flag3 = true;
					}
				}
			}
			if (!flag3)
			{
				this.playerPool.Return((FusionNetPlayer)netPlayer2);
				this.netPlayerCache.Remove(netPlayer2);
			}
		}
	}

	public override void SetPlayerObject(GameObject playerInstance, int? owningPlayerID = null)
	{
		PlayerRef playerRef = this.runner.LocalPlayer;
		if (owningPlayerID != null)
		{
			playerRef = this.GetPlayerRef(owningPlayerID.Value);
		}
		this.runner.SetPlayerObject(playerRef, playerInstance.GetComponent<NetworkObject>());
	}

	private PlayerRef GetPlayerRef(int playerID)
	{
		if (this.runner == null)
		{
			Debug.LogWarning("There is no runner yet - returning default player ref");
			return default(PlayerRef);
		}
		foreach (PlayerRef playerRef in this.runner.ActivePlayers)
		{
			if (playerRef.PlayerId == playerID)
			{
				return playerRef;
			}
		}
		Debug.LogWarning(string.Format("GetPlayerRef - Couldn't find active player with ID #{0}", playerID));
		return default(PlayerRef);
	}

	public override NetPlayer GetLocalPlayer()
	{
		if (this.netPlayerCache.Count == 0 || this.netPlayerCache.Count != this.runner.SessionInfo.PlayerCount)
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
		Debug.LogError("Somehow there is no local NetPlayer. This shoulnd't happen.");
		return null;
	}

	public override NetPlayer GetPlayer(int PlayerID)
	{
		if (PlayerID == -1)
		{
			Debug.LogWarning("Attempting to get NetPlayer for local -1 ID.");
			return null;
		}
		foreach (NetPlayer netPlayer in this.netPlayerCache)
		{
			if (netPlayer.ActorNumber == PlayerID)
			{
				return netPlayer;
			}
		}
		if (this.netPlayerCache.Count == 0 || this.netPlayerCache.Count != this.runner.SessionInfo.PlayerCount)
		{
			base.UpdatePlayers();
			foreach (NetPlayer netPlayer2 in this.netPlayerCache)
			{
				if (netPlayer2.ActorNumber == PlayerID)
				{
					return netPlayer2;
				}
			}
		}
		Debug.LogError("Failed to find the player, before and after resyncing the player cache, this probably shoulnd't happen...");
		return null;
	}

	public override void SetMyNickName(string name)
	{
		if (!KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags) && !name.StartsWith("gorilla"))
		{
			Debug.Log("[KID] Trying to set custom nickname but that permission has been disallowed");
			if (this.InRoom && GorillaTagger.Instance.rigSerializer != null)
			{
				GorillaTagger.Instance.rigSerializer.nickName = "gorilla";
			}
			return;
		}
		PlayerPrefs.SetString("playerName", name);
		if (this.InRoom && GorillaTagger.Instance.rigSerializer != null)
		{
			GorillaTagger.Instance.rigSerializer.nickName = name;
		}
	}

	public override string GetMyNickName()
	{
		return PlayerPrefs.GetString("playerName");
	}

	public override string GetMyDefaultName()
	{
		return "gorilla" + Random.Range(0, 9999).ToString().PadLeft(4, '0');
	}

	public override string GetNickName(int playerID)
	{
		NetPlayer player = this.GetPlayer(playerID);
		return this.GetNickName(player);
	}

	public override string GetNickName(NetPlayer player)
	{
		if (player == null)
		{
			Debug.LogError("Cant get nick name as playerID doesnt have a NetPlayer...");
			return "";
		}
		RigContainer rigContainer;
		VRRigCache.Instance.TryGetVrrig(player, out rigContainer);
		if (!KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags))
		{
			return rigContainer.Rig.rigSerializer.defaultName.Value ?? "";
		}
		return rigContainer.Rig.rigSerializer.nickName.Value ?? "";
	}

	public override string GetMyUserID()
	{
		return this.runner.GetPlayerUserId(this.runner.LocalPlayer);
	}

	public override string GetUserID(int playerID)
	{
		if (this.runner == null)
		{
			return string.Empty;
		}
		return this.runner.GetPlayerUserId(this.GetPlayerRef(playerID));
	}

	public override string GetUserID(NetPlayer player)
	{
		if (this.runner == null)
		{
			return string.Empty;
		}
		return this.runner.GetPlayerUserId(((FusionNetPlayer)player).PlayerRef);
	}

	public override void SetMyTutorialComplete()
	{
		if (!(PlayerPrefs.GetString("didTutorial", "nope") == "done"))
		{
			PlayerPrefs.SetString("didTutorial", "done");
			PlayerPrefs.Save();
		}
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
			Debug.LogError("Player not found");
			return false;
		}
		RigContainer rigContainer;
		VRRigCache.Instance.TryGetVrrig(player, out rigContainer);
		if (rigContainer == null)
		{
			Debug.LogError("VRRig not found for player");
			return false;
		}
		if (rigContainer.Rig.rigSerializer == null)
		{
			Debug.LogWarning("Vr rig serializer is not set up on the rig yet");
			return false;
		}
		return rigContainer.Rig.rigSerializer.tutorialComplete;
	}

	public override int GlobalPlayerCount()
	{
		if (this.regionCrawler == null)
		{
			return 0;
		}
		return this.regionCrawler.PlayerCountGlobal;
	}

	public override int GetOwningPlayerID(GameObject obj)
	{
		NetworkObject networkObject;
		if (!obj.TryGetComponent<NetworkObject>(out networkObject))
		{
			return -1;
		}
		if (this.runner.GameMode == global::Fusion.GameMode.Shared)
		{
			return networkObject.StateAuthority.PlayerId;
		}
		return networkObject.InputAuthority.PlayerId;
	}

	public override bool IsObjectLocallyOwned(GameObject obj)
	{
		NetworkObject networkObject;
		if (!obj.TryGetComponent<NetworkObject>(out networkObject))
		{
			return false;
		}
		if (this.runner.GameMode == global::Fusion.GameMode.Shared)
		{
			return networkObject.StateAuthority == this.runner.LocalPlayer;
		}
		return networkObject.InputAuthority == this.runner.LocalPlayer;
	}

	public override bool IsTotalAuthority()
	{
		return this.runner.Mode == SimulationModes.Server || this.runner.Mode == SimulationModes.Host || this.runner.GameMode == global::Fusion.GameMode.Single || this.runner.IsSharedModeMasterClient;
	}

	public override bool ShouldWriteObjectData(GameObject obj)
	{
		NetworkObject networkObject;
		return obj.TryGetComponent<NetworkObject>(out networkObject) && networkObject.HasStateAuthority;
	}

	public override bool ShouldUpdateObject(GameObject obj)
	{
		NetworkObject networkObject;
		if (!obj.TryGetComponent<NetworkObject>(out networkObject))
		{
			return true;
		}
		if (this.IsTotalAuthority())
		{
			return true;
		}
		if (networkObject.InputAuthority.IsRealPlayer && !networkObject.InputAuthority.IsRealPlayer)
		{
			return networkObject.InputAuthority == this.runner.LocalPlayer;
		}
		return this.runner.IsSharedModeMasterClient;
	}

	public override bool IsObjectRoomObject(GameObject obj)
	{
		NetworkObject networkObject;
		if (obj.TryGetComponent<NetworkObject>(out networkObject))
		{
			Debug.LogWarning("Fusion currently automatically passes false for roomobject check.");
			return false;
		}
		return false;
	}

	private void OnMasterSwitch(NetPlayer player)
	{
		if (this.runner.IsSharedModeMasterClient)
		{
			Dictionary<string, SessionProperty> dictionary = new Dictionary<string, SessionProperty> { 
			{
				"MasterClient",
				base.LocalPlayer.ActorNumber
			} };
			this.runner.SessionInfo.UpdateCustomProperties(dictionary);
		}
	}

	private NetworkSystemFusion.InternalState internalState;

	private FusionInternalRPCs internalRPCProvider;

	private FusionCallbackHandler callbackHandler;

	private FusionRegionCrawler regionCrawler;

	private GameObject volatileNetObj;

	private global::Fusion.Photon.Realtime.AuthenticationValues cachedPlayfabAuth;

	private const string playerPropertiesPath = "P_FusionProperties";

	private bool lastConnectAttempt_WasFull;

	private VoiceConnection FusionVoice;

	private CustomObjectProvider myObjectProvider;

	private ObjectPool<FusionNetPlayer> playerPool;

	public List<NetworkObject> cachedNetSceneObjects = new List<NetworkObject>();

	private List<INetworkRunnerCallbacks> objectsThatNeedCallbacks = new List<INetworkRunnerCallbacks>();

	private Queue<NetworkObject> registrationQueue = new Queue<NetworkObject>();

	private bool isProcessingQueue;

	private enum InternalState
	{
		AwaitingAuth,
		Idle,
		Searching_Joining,
		Searching_Joined,
		Searching_JoinFailed,
		Searching_Disconnecting,
		Searching_Disconnected,
		ConnectingToRoom,
		ConnectedToRoom,
		JoinRoomFailed,
		Disconnecting,
		Disconnected,
		StateCheckFailed
	}
}
