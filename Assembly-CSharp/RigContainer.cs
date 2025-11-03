using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using GorillaTag.Audio;
using Newtonsoft.Json;
using Photon.Voice.PUN;
using PlayFab;
using PlayFab.CloudScriptModels;
using UnityEngine;

[RequireComponent(typeof(VRRig), typeof(VRRigReliableState))]
public class RigContainer : MonoBehaviour
{
	public bool Initialized { get; private set; }

	public VRRig Rig
	{
		get
		{
			return this.vrrig;
		}
	}

	public VRRigReliableState ReliableState
	{
		get
		{
			return this.reliableState;
		}
	}

	public Transform SpeakerHead
	{
		get
		{
			return this.speakerHead;
		}
	}

	public AudioSource ReplacementVoiceSource
	{
		get
		{
			return this.replacementVoiceSource;
		}
	}

	public List<LoudSpeakerNetwork> LoudSpeakerNetworks
	{
		get
		{
			return this.loudSpeakerNetworks;
		}
	}

	public LCKSocialCameraFollower LCKCoconutCamera
	{
		get
		{
			return this.m_lckCoconutCamera;
		}
	}

	public PhotonVoiceView Voice
	{
		get
		{
			return this.voiceView;
		}
		set
		{
			if (value == this.voiceView)
			{
				return;
			}
			if (this.voiceView != null)
			{
				this.voiceView.SpeakerInUse.enabled = false;
			}
			this.voiceView = value;
			this.RefreshVoiceChat();
		}
	}

	public NetworkView netView
	{
		get
		{
			return this.vrrig.netView;
		}
	}

	public int CachedNetViewID
	{
		get
		{
			return this.m_cachedNetViewID;
		}
	}

	public bool Muted
	{
		get
		{
			return !this.enableVoice;
		}
		set
		{
			this.enableVoice = !value;
			this.RefreshVoiceChat();
		}
	}

	public NetPlayer Creator
	{
		get
		{
			return this.vrrig.creator;
		}
		set
		{
			if (this.vrrig.isOfflineVRRig || (this.vrrig.creator != null && this.vrrig.creator.InRoom))
			{
				return;
			}
			this.vrrig.creator = value;
		}
	}

	public bool ForceMute
	{
		get
		{
			return this.forceMute;
		}
		set
		{
			this.forceMute = value;
			this.RefreshVoiceChat();
		}
	}

	public SphereCollider HeadCollider
	{
		get
		{
			return this.headCollider;
		}
	}

	public CapsuleCollider BodyCollider
	{
		get
		{
			return this.bodyCollider;
		}
	}

	public VRRigEvents RigEvents
	{
		get
		{
			return this.rigEvents;
		}
	}

	public bool GetIsPlayerAutoMuted()
	{
		return this.bPlayerAutoMuted;
	}

	public void UpdateAutomuteLevel(string autoMuteLevel)
	{
		if (autoMuteLevel.Equals("LOW", StringComparison.OrdinalIgnoreCase))
		{
			this.playerChatQuality = 1;
		}
		else if (autoMuteLevel.Equals("HIGH", StringComparison.OrdinalIgnoreCase))
		{
			this.playerChatQuality = 0;
		}
		else if (autoMuteLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
		{
			this.playerChatQuality = 2;
		}
		else
		{
			this.playerChatQuality = 2;
		}
		this.RefreshVoiceChat();
	}

	private void Awake()
	{
		this.loudSpeakerNetworks = new List<LoudSpeakerNetwork>();
	}

	private void Start()
	{
		if (this.Rig.isOfflineVRRig)
		{
			this.vrrig.creator = NetworkSystem.Instance.LocalPlayer;
			RoomSystem.JoinedRoomEvent += new Action(this.OnMultiPlayerStarted);
			RoomSystem.LeftRoomEvent += new Action(this.OnReturnedToSinglePlayer);
		}
		else
		{
			this.rigEvents.enableEvent += this.RigPostEnable;
		}
		this.Rig.rigContainer = this;
	}

	private void RigPostEnable(RigContainer _)
	{
		this.vrrig.UpdateName();
	}

	private void OnMultiPlayerStarted()
	{
		if (this.Rig.isOfflineVRRig)
		{
			this.vrrig.creator = NetworkSystem.Instance.GetLocalPlayer();
		}
	}

	private void OnReturnedToSinglePlayer()
	{
		if (this.Rig.isOfflineVRRig)
		{
			RigContainer.CancelAutomuteRequest();
		}
	}

	private void OnDisable()
	{
		this.Initialized = false;
		this.enableVoice = true;
		this.voiceView = null;
		base.gameObject.transform.localPosition = Vector3.zero;
		base.gameObject.transform.localRotation = Quaternion.identity;
		this.vrrig.syncPos = base.gameObject.transform.position;
		this.vrrig.syncRotation = base.gameObject.transform.rotation;
		this.forceMute = false;
	}

	internal void InitializeNetwork(NetworkView netView, PhotonVoiceView voiceView, VRRigSerializer vrRigSerializer)
	{
		if (!netView || !voiceView)
		{
			return;
		}
		this.InitializeNetwork_Shared(netView, vrRigSerializer);
		this.Voice = voiceView;
		this.vrrig.voiceAudio = voiceView.SpeakerInUse.GetComponent<AudioSource>();
	}

	private void InitializeNetwork_Shared(NetworkView netView, VRRigSerializer vrRigSerializer)
	{
		if (this.vrrig.netView)
		{
			GorillaNot.instance.SendReport("inappropriate tag data being sent creating multiple vrrigs", this.Creator.UserId, this.Creator.NickName);
			if (this.vrrig.netView.IsMine)
			{
				NetworkSystem.Instance.NetDestroy(this.vrrig.gameObject);
			}
			else
			{
				this.vrrig.netView.gameObject.SetActive(false);
			}
		}
		this.vrrig.netView = netView;
		this.vrrig.rigSerializer = vrRigSerializer;
		this.vrrig.OwningNetPlayer = NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(vrRigSerializer.gameObject));
		this.m_cachedNetViewID = netView.ViewID;
		if (!this.Initialized)
		{
			this.vrrig.NetInitialize();
			if (GorillaGameManager.instance != null && NetworkSystem.Instance.IsMasterClient)
			{
				int owningPlayerID = NetworkSystem.Instance.GetOwningPlayerID(vrRigSerializer.gameObject);
				bool playerTutorialCompletion = NetworkSystem.Instance.GetPlayerTutorialCompletion(owningPlayerID);
				GorillaGameManager.instance.NewVRRig(netView.Owner, netView.ViewID, playerTutorialCompletion);
			}
			bool isLocal = this.vrrig.OwningNetPlayer.IsLocal;
			if (this.vrrig.InitializedCosmetics)
			{
				netView.SendRPC("RPC_RequestCosmetics", netView.Owner, Array.Empty<object>());
			}
		}
		this.Initialized = true;
		if (!this.vrrig.isOfflineVRRig)
		{
			base.StartCoroutine(RigContainer.QueueAutomute(this.Creator));
		}
	}

	private static IEnumerator QueueAutomute(NetPlayer player)
	{
		RigContainer.playersToCheckAutomute.Add(player);
		if (!RigContainer.automuteQueued)
		{
			RigContainer.automuteQueued = true;
			yield return new WaitForSecondsRealtime(1f);
			while (RigContainer.waitingForAutomuteCallback)
			{
				yield return null;
			}
			RigContainer.automuteQueued = false;
			RigContainer.RequestAutomuteSettings();
		}
		yield break;
	}

	private static void RequestAutomuteSettings()
	{
		if (RigContainer.playersToCheckAutomute.Count == 0)
		{
			return;
		}
		RigContainer.waitingForAutomuteCallback = true;
		RigContainer.playersToCheckAutomute.RemoveAll((NetPlayer player) => player == null);
		RigContainer.requestedAutomutePlayers = new List<NetPlayer>(RigContainer.playersToCheckAutomute);
		RigContainer.playersToCheckAutomute.Clear();
		string[] array = RigContainer.requestedAutomutePlayers.Select((NetPlayer x) => x.UserId).ToArray<string>();
		foreach (NetPlayer netPlayer in RigContainer.requestedAutomutePlayers)
		{
		}
		ExecuteFunctionRequest executeFunctionRequest = new ExecuteFunctionRequest();
		executeFunctionRequest.Entity = new EntityKey
		{
			Id = PlayFabSettings.staticPlayer.EntityId,
			Type = PlayFabSettings.staticPlayer.EntityType
		};
		executeFunctionRequest.FunctionName = "ShouldUserAutomutePlayer";
		executeFunctionRequest.FunctionParameter = string.Join(",", array);
		PlayFabCloudScriptAPI.ExecuteFunction(executeFunctionRequest, delegate(ExecuteFunctionResult result)
		{
			Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.FunctionResult.ToString());
			if (dictionary == null)
			{
				using (List<NetPlayer>.Enumerator enumerator2 = RigContainer.requestedAutomutePlayers.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						NetPlayer netPlayer2 = enumerator2.Current;
						if (netPlayer2 != null)
						{
							RigContainer.ReceiveAutomuteSettings(netPlayer2, "none");
						}
					}
					goto IL_00A6;
				}
			}
			foreach (NetPlayer netPlayer3 in RigContainer.requestedAutomutePlayers)
			{
				if (netPlayer3 != null)
				{
					string text;
					if (dictionary.TryGetValue(netPlayer3.UserId, out text))
					{
						RigContainer.ReceiveAutomuteSettings(netPlayer3, text);
					}
					else
					{
						RigContainer.ReceiveAutomuteSettings(netPlayer3, "none");
					}
				}
			}
			IL_00A6:
			RigContainer.requestedAutomutePlayers.Clear();
			RigContainer.waitingForAutomuteCallback = false;
		}, delegate(PlayFabError error)
		{
			foreach (NetPlayer netPlayer4 in RigContainer.requestedAutomutePlayers)
			{
				RigContainer.ReceiveAutomuteSettings(netPlayer4, "ERROR");
			}
			RigContainer.requestedAutomutePlayers.Clear();
			RigContainer.waitingForAutomuteCallback = false;
		}, null, null);
	}

	private static void CancelAutomuteRequest()
	{
		RigContainer.playersToCheckAutomute.Clear();
		RigContainer.automuteQueued = false;
		if (RigContainer.requestedAutomutePlayers != null)
		{
			RigContainer.requestedAutomutePlayers.Clear();
		}
		RigContainer.waitingForAutomuteCallback = false;
	}

	private static void ReceiveAutomuteSettings(NetPlayer player, string score)
	{
		RigContainer rigContainer;
		VRRigCache.Instance.TryGetVrrig(player, out rigContainer);
		if (rigContainer != null)
		{
			rigContainer.UpdateAutomuteLevel(score);
		}
	}

	private void ProcessAutomute()
	{
		int @int = PlayerPrefs.GetInt("autoMute", 1);
		this.bPlayerAutoMuted = !this.hasManualMute && this.playerChatQuality < @int;
	}

	public void RefreshVoiceChat()
	{
		if (this.Voice == null)
		{
			return;
		}
		this.ProcessAutomute();
		this.Voice.SpeakerInUse.enabled = !this.forceMute && this.enableVoice && !this.bPlayerAutoMuted && GorillaComputer.instance.voiceChatOn == "TRUE";
		this.replacementVoiceSource.mute = this.forceMute || !this.enableVoice || this.bPlayerAutoMuted || GorillaComputer.instance.voiceChatOn == "OFF";
	}

	public void AddLoudSpeakerNetwork(LoudSpeakerNetwork network)
	{
		if (this.loudSpeakerNetworks.Contains(network))
		{
			return;
		}
		this.loudSpeakerNetworks.Add(network);
	}

	public void RemoveLoudSpeakerNetwork(LoudSpeakerNetwork network)
	{
		this.loudSpeakerNetworks.Remove(network);
	}

	public static void RefreshAllRigVoices()
	{
		RigContainer.staticTempRC = null;
		if (!NetworkSystem.Instance.InRoom || VRRigCache.Instance == null)
		{
			return;
		}
		foreach (NetPlayer netPlayer in NetworkSystem.Instance.AllNetPlayers)
		{
			if (VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer.staticTempRC))
			{
				RigContainer.staticTempRC.RefreshVoiceChat();
			}
		}
	}

	[SerializeField]
	private VRRig vrrig;

	[SerializeField]
	private VRRigReliableState reliableState;

	[SerializeField]
	private Transform speakerHead;

	[SerializeField]
	private AudioSource replacementVoiceSource;

	private List<LoudSpeakerNetwork> loudSpeakerNetworks;

	[SerializeField]
	private LCKSocialCameraFollower m_lckCoconutCamera;

	private PhotonVoiceView voiceView;

	private int m_cachedNetViewID;

	private bool enableVoice = true;

	private bool forceMute;

	[SerializeField]
	private SphereCollider headCollider;

	[SerializeField]
	private CapsuleCollider bodyCollider;

	[SerializeField]
	private VRRigEvents rigEvents;

	public bool hasManualMute;

	private bool bPlayerAutoMuted;

	public int playerChatQuality = 2;

	private static List<NetPlayer> playersToCheckAutomute = new List<NetPlayer>();

	private static bool automuteQueued = false;

	private static List<NetPlayer> requestedAutomutePlayers;

	private static bool waitingForAutomuteCallback = false;

	private static RigContainer staticTempRC;
}
