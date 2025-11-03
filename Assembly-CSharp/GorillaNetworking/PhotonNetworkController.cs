using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Fusion;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaTagScripts;
using Photon.Pun;
using PlayFab;
using UnityEngine;

namespace GorillaNetworking
{
	public class PhotonNetworkController : MonoBehaviour
	{
		public List<string> FriendIDList
		{
			get
			{
				return this.friendIDList;
			}
			set
			{
				this.friendIDList = value;
			}
		}

		public string StartLevel
		{
			get
			{
				return this.startLevel;
			}
			set
			{
				this.startLevel = value;
			}
		}

		public GTZone StartZone
		{
			get
			{
				return this.startZone;
			}
			set
			{
				this.startZone = value;
			}
		}

		public GTZone CurrentRoomZone
		{
			get
			{
				if (!(this.currentJoinTrigger != null))
				{
					return GTZone.none;
				}
				return this.currentJoinTrigger.zone;
			}
		}

		public GorillaGeoHideShowTrigger StartGeoTrigger
		{
			get
			{
				return this.startGeoTrigger;
			}
			set
			{
				this.startGeoTrigger = value;
			}
		}

		public void Awake()
		{
			if (PhotonNetworkController.Instance == null)
			{
				PhotonNetworkController.Instance = this;
			}
			else if (PhotonNetworkController.Instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			this.updatedName = false;
			this.playersInRegion = new int[this.serverRegions.Length];
			this.pingInRegion = new int[this.serverRegions.Length];
		}

		public void Start()
		{
			base.StartCoroutine(this.DisableOnStart());
			NetworkSystem.Instance.OnJoinedRoomEvent += this.OnJoinedRoom;
			NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnDisconnected;
			PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = true;
		}

		private IEnumerator DisableOnStart()
		{
			ZoneManagement.SetActiveZone(this.StartZone);
			yield break;
		}

		public void FixedUpdate()
		{
			this.headRightHandDistance = (GTPlayer.Instance.headCollider.transform.position - GTPlayer.Instance.GetControllerTransform(false).position).magnitude;
			this.headLeftHandDistance = (GTPlayer.Instance.headCollider.transform.position - GTPlayer.Instance.GetControllerTransform(true).position).magnitude;
			this.headQuat = GTPlayer.Instance.headCollider.transform.rotation;
			if (!this.disableAFKKick && Quaternion.Angle(this.headQuat, this.lastHeadQuat) <= 0.01f && Mathf.Abs(this.headRightHandDistance - this.lastHeadRightHandDistance) < 0.001f && Mathf.Abs(this.headLeftHandDistance - this.lastHeadLeftHandDistance) < 0.001f && this.pauseTime + this.disconnectTime < Time.realtimeSinceStartup)
			{
				this.pauseTime = Time.realtimeSinceStartup;
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
			else if (Quaternion.Angle(this.headQuat, this.lastHeadQuat) > 0.01f || Mathf.Abs(this.headRightHandDistance - this.lastHeadRightHandDistance) >= 0.001f || Mathf.Abs(this.headLeftHandDistance - this.lastHeadLeftHandDistance) >= 0.001f)
			{
				this.pauseTime = Time.realtimeSinceStartup;
			}
			this.lastHeadRightHandDistance = this.headRightHandDistance;
			this.lastHeadLeftHandDistance = this.headLeftHandDistance;
			this.lastHeadQuat = this.headQuat;
			if (this.deferredJoin && Time.time >= this.partyJoinDeferredUntilTimestamp)
			{
				if ((this.partyJoinDeferredUntilTimestamp != 0f || NetworkSystem.Instance.netState == NetSystemState.Idle) && this.currentJoinTrigger != null)
				{
					this.deferredJoin = false;
					this.partyJoinDeferredUntilTimestamp = 0f;
					if (this.currentJoinTrigger == this.privateTrigger)
					{
						this.AttemptToJoinSpecificRoom(this.customRoomID, FriendshipGroupDetection.Instance.IsInParty ? JoinType.JoinWithParty : JoinType.Solo);
						return;
					}
					this.AttemptToJoinPublicRoom(this.currentJoinTrigger, this.currentJoinType, null);
					return;
				}
				else if (NetworkSystem.Instance.netState != NetSystemState.PingRecon && NetworkSystem.Instance.netState != NetSystemState.Initialization)
				{
					this.deferredJoin = false;
					this.partyJoinDeferredUntilTimestamp = 0f;
				}
			}
		}

		public void DeferJoining(float duration)
		{
			this.partyJoinDeferredUntilTimestamp = Mathf.Max(this.partyJoinDeferredUntilTimestamp, Time.time + duration);
		}

		public void ClearDeferredJoin()
		{
			this.partyJoinDeferredUntilTimestamp = 0f;
			this.deferredJoin = false;
		}

		public void AttemptToJoinPublicRoom(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType = JoinType.Solo, List<ValueTuple<string, string>> additionalCustomProperties = null)
		{
			this.AttemptToJoinPublicRoomAsync(triggeredTrigger, roomJoinType, additionalCustomProperties);
		}

		private async void AttemptToJoinPublicRoomAsync(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType, List<ValueTuple<string, string>> additionalCustomProperties)
		{
			if ((!KIDManager.KidEnabledAndReady || KIDManager.CheckFeatureOptIn(EKIDFeatures.Multiplayer, null).Item2) && base.enabled)
			{
				if (NetworkSystem.Instance.netState != NetSystemState.Connecting && NetworkSystem.Instance.netState != NetSystemState.Disconnecting)
				{
					if (NetworkSystem.Instance.netState == NetSystemState.Initialization || NetworkSystem.Instance.netState == NetSystemState.PingRecon || Time.time < this.partyJoinDeferredUntilTimestamp)
					{
						this.currentJoinTrigger = triggeredTrigger;
						this.currentJoinType = roomJoinType;
						this.deferredJoin = true;
					}
					else
					{
						this.deferredJoin = false;
						string desiredGameMode = triggeredTrigger.GetFullDesiredGameModeString();
						if (NetworkSystem.Instance.InRoom)
						{
							if (NetworkSystem.Instance.SessionIsPrivate)
							{
								if (roomJoinType != JoinType.JoinWithNearby && roomJoinType != JoinType.ForceJoinWithParty)
								{
									return;
								}
							}
							else if (NetworkSystem.Instance.GameModeString.StartsWith(desiredGameMode))
							{
								return;
							}
						}
						if (roomJoinType == JoinType.JoinWithParty || roomJoinType == JoinType.ForceJoinWithParty)
						{
							await this.SendPartyFollowCommands();
						}
						this.currentJoinTrigger = triggeredTrigger;
						this.currentJoinType = roomJoinType;
						if (PlayFabClientAPI.IsClientLoggedIn())
						{
							this.playFabAuthenticator.SetDisplayName(NetworkSystem.Instance.GetMyNickName());
						}
						RoomConfig roomConfig = RoomConfig.AnyPublicConfig();
						if (this.currentJoinType == JoinType.JoinWithNearby || this.currentJoinType == JoinType.JoinWithElevator)
						{
							roomConfig.SetFriendIDs(this.FriendIDList);
						}
						else if (this.currentJoinType == JoinType.JoinWithParty || this.currentJoinType == JoinType.ForceJoinWithParty)
						{
							roomConfig.SetFriendIDs(FriendshipGroupDetection.Instance.PartyMemberIDs.ToList<string>());
						}
						Hashtable hashtable = new Hashtable
						{
							{ "gameMode", desiredGameMode },
							{ "platform", this.platformTag },
							{
								"queueName",
								GorillaComputer.instance.currentQueue
							},
							{
								"language",
								LocalisationManager.CurrentLanguage.ToString()
							}
						};
						if (additionalCustomProperties != null)
						{
							foreach (ValueTuple<string, string> valueTuple in additionalCustomProperties)
							{
								hashtable.Add(valueTuple.Item1, valueTuple.Item2);
							}
						}
						roomConfig.CustomProps = hashtable;
						roomConfig.MaxPlayers = this.currentJoinTrigger.GetRoomSize();
						Debug.Log(string.Format("AttemptToJoinPublicRoom: MaxPlayers: {0}", roomConfig.MaxPlayers));
						await NetworkSystem.Instance.ConnectToRoom(null, roomConfig, -1);
					}
				}
			}
		}

		public void AttemptToJoinRankedPublicRoom(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType = JoinType.Solo)
		{
			string text = RankedProgressionManager.Instance.GetRankedMatchmakingTier().ToString();
			string text2 = "PC";
			this.AttemptToJoinRankedPublicRoomAsync(triggeredTrigger, text, text2, roomJoinType);
		}

		private async void AttemptToJoinRankedPublicRoomAsync(GorillaNetworkJoinTrigger triggeredTrigger, string mmrTier, string platform, JoinType roomJoinType)
		{
			if ((!KIDManager.KidEnabledAndReady || KIDManager.CheckFeatureOptIn(EKIDFeatures.Multiplayer, null).Item2) && base.enabled)
			{
				if (NetworkSystem.Instance.netState != NetSystemState.Connecting && NetworkSystem.Instance.netState != NetSystemState.Disconnecting)
				{
					if (NetworkSystem.Instance.netState == NetSystemState.Initialization || NetworkSystem.Instance.netState == NetSystemState.PingRecon || Time.time < this.partyJoinDeferredUntilTimestamp)
					{
						this.currentJoinTrigger = triggeredTrigger;
						this.currentJoinType = roomJoinType;
						this.deferredJoin = true;
					}
					else
					{
						this.deferredJoin = false;
						string fullDesiredGameModeString = triggeredTrigger.GetFullDesiredGameModeString();
						if (!NetworkSystem.Instance.InRoom)
						{
							this.currentJoinTrigger = triggeredTrigger;
							this.currentJoinType = roomJoinType;
							if (PlayFabClientAPI.IsClientLoggedIn())
							{
								this.playFabAuthenticator.SetDisplayName(NetworkSystem.Instance.GetMyNickName());
							}
							RoomConfig roomConfig = RoomConfig.AnyPublicConfig();
							Hashtable hashtable = new Hashtable
							{
								{ "gameMode", fullDesiredGameModeString },
								{ "mmrTier", mmrTier },
								{ "platform", platform }
							};
							roomConfig.CustomProps = hashtable;
							roomConfig.MaxPlayers = this.currentJoinTrigger.GetRoomSize();
							await NetworkSystem.Instance.ConnectToRoom(null, roomConfig, -1);
						}
					}
				}
			}
		}

		private async Task SendPartyFollowCommands()
		{
			PhotonNetworkController.Instance.shuffler = Random.Range(0, 99).ToString().PadLeft(2, '0') + Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			RoomSystem.SendPartyFollowCommand(PhotonNetworkController.Instance.shuffler, PhotonNetworkController.Instance.keyStr);
			PhotonNetwork.SendAllOutgoingCommands();
			await Task.Delay(200);
		}

		public void AttemptToJoinSpecificRoom(string roomID, JoinType roomJoinType)
		{
			this.AttemptToJoinSpecificRoomAsync(roomID, roomJoinType, null);
		}

		public void AttemptToJoinSpecificRoomWithCallback(string roomID, JoinType roomJoinType, Action<NetJoinResult> callback)
		{
			this.AttemptToJoinSpecificRoomAsync(roomID, roomJoinType, callback);
		}

		public async Task AttemptToJoinSpecificRoomAsync(string roomID, JoinType roomJoinType, Action<NetJoinResult> callback)
		{
			TaskAwaiter<bool> taskAwaiter = KIDManager.UseKID().GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<bool> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (!taskAwaiter.GetResult() || KIDManager.HasPermissionToUseFeature(EKIDFeatures.Multiplayer))
			{
				if (NetworkSystem.Instance.netState == NetSystemState.Initialization || NetworkSystem.Instance.netState == NetSystemState.PingRecon)
				{
					this.deferredJoin = true;
					this.customRoomID = roomID;
					this.currentJoinType = roomJoinType;
					this.currentJoinTrigger = this.privateTrigger;
				}
				else if (NetworkSystem.Instance.netState == NetSystemState.Idle || NetworkSystem.Instance.netState == NetSystemState.InGame)
				{
					this.customRoomID = roomID;
					this.currentJoinType = roomJoinType;
					this.currentJoinTrigger = this.privateTrigger;
					this.deferredJoin = false;
					if (this.currentJoinType == JoinType.JoinWithParty || this.currentJoinType == JoinType.ForceJoinWithParty)
					{
						await this.SendPartyFollowCommands();
					}
					string fullDesiredGameModeString = this.currentJoinTrigger.GetFullDesiredGameModeString();
					Hashtable hashtable = new Hashtable
					{
						{ "gameMode", fullDesiredGameModeString },
						{ "platform", this.platformTag },
						{
							"queueName",
							GorillaComputer.instance.currentQueue
						}
					};
					RoomConfig roomConfig = new RoomConfig();
					roomConfig.createIfMissing = true;
					roomConfig.isJoinable = true;
					roomConfig.isPublic = false;
					roomConfig.MaxPlayers = RoomSystem.GetRoomSizeForCreate(this.currentJoinTrigger.networkZone);
					Debug.Log(string.Format("[AttemptToJoinSpecificRoomAsync] Room MaxPlayers = {0}", roomConfig.MaxPlayers));
					roomConfig.CustomProps = hashtable;
					if (roomJoinType == JoinType.FriendStationPublic)
					{
						roomConfig.isPublic = true;
					}
					if (PlayFabClientAPI.IsClientLoggedIn())
					{
						this.playFabAuthenticator.SetDisplayName(NetworkSystem.Instance.GetMyNickName());
					}
					Task<NetJoinResult> connectToRoomTask = NetworkSystem.Instance.ConnectToRoom(roomID, roomConfig, -1);
					if (callback != null)
					{
						await connectToRoomTask;
						Debug.Log("AttemptToJoinSpecificRoomAsync ConnectToRoom Result: " + connectToRoomTask.Result.ToString());
						callback(connectToRoomTask.Result);
					}
				}
			}
		}

		private void DisconnectCleanup()
		{
			if (ApplicationQuittingState.IsQuitting)
			{
				return;
			}
			if (GorillaParent.instance != null)
			{
				GorillaScoreboardSpawner[] componentsInChildren = GorillaParent.instance.GetComponentsInChildren<GorillaScoreboardSpawner>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].OnLeftRoom();
				}
			}
			this.attemptingToConnect = true;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.offlineVRRig)
			{
				if (skinnedMeshRenderer != null)
				{
					skinnedMeshRenderer.enabled = true;
				}
			}
			if (GorillaComputer.instance != null && !ApplicationQuittingState.IsQuitting)
			{
				this.UpdateTriggerScreens();
			}
			GTPlayer.Instance.maxJumpSpeed = 6.5f;
			GTPlayer.Instance.jumpMultiplier = 1.1f;
			GorillaNot.instance.currentMasterClient = null;
			GorillaTagger.Instance.offlineVRRig.huntComputer.SetActive(false);
			this.initialGameMode = "";
		}

		public void OnJoinedRoom()
		{
			if (NetworkSystem.Instance.GameModeString.IsNullOrEmpty())
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
			this.initialGameMode = NetworkSystem.Instance.GameModeString;
			if (NetworkSystem.Instance.SessionIsPrivate)
			{
				this.currentJoinTrigger = this.privateTrigger;
				PhotonNetworkController.Instance.UpdateTriggerScreens();
			}
			else if (this.currentJoinType != JoinType.FollowingParty)
			{
				bool flag = false;
				for (int i = 0; i < GorillaComputer.instance.allowedMapsToJoin.Length; i++)
				{
					if (NetworkSystem.Instance.GameModeString.StartsWith(GorillaComputer.instance.allowedMapsToJoin[i]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					GorillaComputer.instance.roomNotAllowed = true;
					NetworkSystem.Instance.ReturnToSinglePlayer();
					return;
				}
			}
			NetworkSystem.Instance.SetMyTutorialComplete();
			VRRigCache.Instance.InstantiateNetworkObject();
			if (NetworkSystem.Instance.IsMasterClient)
			{
				global::GorillaGameModes.GameMode.LoadGameModeFromProperty(this.initialGameMode);
			}
			GorillaComputer.instance.roomFull = false;
			GorillaComputer.instance.roomNotAllowed = false;
			if (this.currentJoinType == JoinType.JoinWithParty || this.currentJoinType == JoinType.JoinWithNearby || this.currentJoinType == JoinType.ForceJoinWithParty || this.currentJoinType == JoinType.JoinWithElevator)
			{
				this.keyToFollow = NetworkSystem.Instance.LocalPlayer.UserId + this.keyStr;
				NetworkSystem.Instance.BroadcastMyRoom(true, this.keyToFollow, this.shuffler);
			}
			GorillaNot.instance.currentMasterClient = null;
			this.UpdateCurrentJoinTrigger();
			this.UpdateTriggerScreens();
			NetworkSystem.Instance.MultiplayerStarted();
		}

		public void RegisterJoinTrigger(GorillaNetworkJoinTrigger trigger)
		{
			this.allJoinTriggers.Add(trigger);
		}

		private void UpdateCurrentJoinTrigger()
		{
			GorillaNetworkJoinTrigger joinTriggerFromFullGameModeString = GorillaComputer.instance.GetJoinTriggerFromFullGameModeString(NetworkSystem.Instance.GameModeString);
			if (joinTriggerFromFullGameModeString != null)
			{
				this.currentJoinTrigger = joinTriggerFromFullGameModeString;
				return;
			}
			if (NetworkSystem.Instance.SessionIsPrivate)
			{
				if (this.currentJoinTrigger != this.privateTrigger)
				{
					Debug.LogError("IN a private game but private trigger isnt current");
					return;
				}
			}
			else
			{
				Debug.LogError("Not in private room and unabel tp update jointrigger.");
			}
		}

		public void UpdateTriggerScreens()
		{
			foreach (GorillaNetworkJoinTrigger gorillaNetworkJoinTrigger in this.allJoinTriggers)
			{
				gorillaNetworkJoinTrigger.UpdateUI();
			}
		}

		public void AttemptToFollowIntoPub(string userIDToFollow, int actorNumberToFollow, string newKeyStr, string shufflerStr, JoinType joinType)
		{
			this.friendToFollow = userIDToFollow;
			this.keyToFollow = userIDToFollow + newKeyStr;
			this.shuffler = shufflerStr;
			this.currentJoinType = joinType;
			this.ClearDeferredJoin();
			if (NetworkSystem.Instance.InRoom)
			{
				NetworkSystem.Instance.JoinFriendsRoom(this.friendToFollow, actorNumberToFollow, this.keyToFollow, this.shuffler);
			}
		}

		public void OnDisconnected()
		{
			this.DisconnectCleanup();
		}

		public void OnApplicationQuit()
		{
			if (PhotonNetwork.IsConnected)
			{
				PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion != "dev";
			}
		}

		private string ReturnRoomName()
		{
			if (this.isPrivate)
			{
				return this.customRoomID;
			}
			return this.RandomRoomName();
		}

		private string RandomRoomName()
		{
			string text = "";
			for (int i = 0; i < 4; i++)
			{
				text += "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Substring(Random.Range(0, "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789".Length), 1);
			}
			if (GorillaComputer.instance.CheckAutoBanListForName(text))
			{
				return text;
			}
			return this.RandomRoomName();
		}

		private string GetRegionWithLowestPing()
		{
			int num = 10000;
			int num2 = 0;
			for (int i = 0; i < this.serverRegions.Length; i++)
			{
				Debug.Log("ping in region " + this.serverRegions[i] + " is " + this.pingInRegion[i].ToString());
				if (this.pingInRegion[i] < num && this.pingInRegion[i] > 0)
				{
					num = this.pingInRegion[i];
					num2 = i;
				}
			}
			return this.serverRegions[num2];
		}

		public int TotalUsers()
		{
			int num = 0;
			foreach (int num2 in this.playersInRegion)
			{
				num += num2;
			}
			return num;
		}

		public string CurrentState()
		{
			if (NetworkSystem.Instance == null)
			{
				Debug.Log("Null netsys!!!");
			}
			return NetworkSystem.Instance.netState.ToString();
		}

		private void OnApplicationPause(bool pause)
		{
			if (pause)
			{
				this.timeWhenApplicationPaused = new DateTime?(DateTime.Now);
				return;
			}
			if ((DateTime.Now - (this.timeWhenApplicationPaused ?? DateTime.Now)).TotalSeconds > (double)this.disconnectTime)
			{
				this.timeWhenApplicationPaused = null;
				NetworkSystem instance = NetworkSystem.Instance;
				if (instance != null)
				{
					instance.ReturnToSinglePlayer();
				}
			}
			if (NetworkSystem.Instance != null && !NetworkSystem.Instance.InRoom && NetworkSystem.Instance.netState == NetSystemState.InGame)
			{
				NetworkSystem instance2 = NetworkSystem.Instance;
				if (instance2 == null)
				{
					return;
				}
				instance2.ReturnToSinglePlayer();
			}
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus && NetworkSystem.Instance != null && !NetworkSystem.Instance.InRoom && NetworkSystem.Instance.netState == NetSystemState.InGame)
			{
				NetworkSystem instance = NetworkSystem.Instance;
				if (instance == null)
				{
					return;
				}
				instance.ReturnToSinglePlayer();
			}
		}

		[OnEnterPlay_SetNull]
		public static volatile PhotonNetworkController Instance;

		public int incrementCounter;

		public PlayFabAuthenticator playFabAuthenticator;

		public string[] serverRegions;

		public bool isPrivate;

		public string customRoomID;

		public GameObject playerOffset;

		public SkinnedMeshRenderer[] offlineVRRig;

		public bool attemptingToConnect;

		private int currentRegionIndex;

		public string currentGameType;

		public bool roomCosmeticsInitialized;

		public GameObject photonVoiceObjectPrefab;

		public Dictionary<string, bool> playerCosmeticsLookup = new Dictionary<string, bool>();

		private float lastHeadRightHandDistance;

		private float lastHeadLeftHandDistance;

		private float pauseTime;

		private float disconnectTime = 120f;

		public bool disableAFKKick;

		private float headRightHandDistance;

		private float headLeftHandDistance;

		private Quaternion headQuat;

		private Quaternion lastHeadQuat;

		public GameObject[] disableOnStartup;

		public GameObject[] enableOnStartup;

		public bool updatedName;

		private int[] playersInRegion;

		private int[] pingInRegion;

		private List<string> friendIDList = new List<string>();

		private JoinType currentJoinType;

		private string friendToFollow;

		private string keyToFollow;

		public string shuffler;

		public string keyStr;

		private string platformTag = "OTHER";

		private string startLevel;

		[SerializeField]
		private GTZone startZone;

		private GorillaGeoHideShowTrigger startGeoTrigger;

		public GorillaNetworkJoinTrigger privateTrigger;

		internal string initialGameMode = "";

		public GorillaNetworkJoinTrigger currentJoinTrigger;

		public string autoJoinRoom;

		public string autoJoinGameMode;

		private bool deferredJoin;

		private float partyJoinDeferredUntilTimestamp;

		private DateTime? timeWhenApplicationPaused;

		[NetworkPrefab]
		[SerializeField]
		private NetworkObject testPlayerPrefab;

		private List<GorillaNetworkJoinTrigger> allJoinTriggers = new List<GorillaNetworkJoinTrigger>();
	}
}
