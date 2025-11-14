using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using GorillaExtensions;
using GorillaTag;
using Liv.Lck;
using Liv.Lck.Core;
using Liv.Lck.Core.Cosmetics;
using Liv.Lck.DependencyInjection;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public class LckEntitlementsNetworked : NetworkComponent
{
	public static bool LckEntitlementsEnabled { get; private set; }

	public override void WriteDataFusion()
	{
	}

	public override void ReadDataFusion()
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void Awake()
	{
		base.Awake();
		if (this.m_rigNetworkController.IsNull())
		{
			this.m_rigNetworkController = base.GetComponentInParent<VRRigSerializer>();
		}
		if (this.m_rigNetworkController.IsNull())
		{
			Debug.LogError("LCK: Unable to find VRRigSerializer");
			return;
		}
		ListProcessor<InAction<RigContainer, PhotonMessageInfoWrapped>> succesfullSpawnEvent = this.m_rigNetworkController.SuccesfullSpawnEvent;
		InAction<RigContainer, PhotonMessageInfoWrapped> inAction = new InAction<RigContainer, PhotonMessageInfoWrapped>(this.OnSuccessfulSpawn);
		succesfullSpawnEvent.Add(in inAction);
	}

	internal override void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		this.InitializeFeatureAsync();
	}

	private async Task InitializeFeatureAsync()
	{
		this._currentState = LckEntitlementsNetworked.FeatureState.Checking;
		bool flag = await this._featureFlagManager.IsEnabledAsync();
		if (!(this == null))
		{
			if (flag)
			{
				this._currentState = LckEntitlementsNetworked.FeatureState.Enabled;
				LckEntitlementsNetworked.LckEntitlementsEnabled = true;
				base.OnEnable();
				this.ProcessSpawnQueue();
			}
			else
			{
				this._currentState = LckEntitlementsNetworked.FeatureState.Disabled;
				LckEntitlementsNetworked.LckEntitlementsEnabled = false;
				this._queuedSpawns.Clear();
			}
		}
	}

	private void OnSuccessfulSpawn(in RigContainer rig, in PhotonMessageInfoWrapped info)
	{
		switch (this._currentState)
		{
		case LckEntitlementsNetworked.FeatureState.Checking:
			this._queuedSpawns.Enqueue(new LckEntitlementsNetworked.QueuedSpawn(rig, info));
			return;
		case LckEntitlementsNetworked.FeatureState.Enabled:
			this.ProcessSpawn(in rig, in info);
			break;
		case LckEntitlementsNetworked.FeatureState.Disabled:
			break;
		default:
			return;
		}
	}

	private void ProcessSpawn(in RigContainer rig, in PhotonMessageInfoWrapped info)
	{
		if (this._lckCosmeticsCoordinator == null)
		{
			Debug.LogError("LCK: ILckCosmeticsCoordinator has not been injected. Entitlement checks will fail.");
			return;
		}
		if (base.IsLocallyOwned)
		{
			base.StartCoroutine(this.AnnouncePlayerPresenceForSession());
			base.StartCoroutine(this.GetAllOtherPlayerCosmeticsForSession());
			return;
		}
		base.StartCoroutine(this.GetNewPlayerCosmeticsForSession());
	}

	private void ProcessSpawnQueue()
	{
		while (this._queuedSpawns.Count > 0)
		{
			LckEntitlementsNetworked.QueuedSpawn queuedSpawn = this._queuedSpawns.Dequeue();
			this.ProcessSpawn(in queuedSpawn.Rig, in queuedSpawn.Info);
		}
	}

	private IEnumerator AnnouncePlayerPresenceForSession()
	{
		if (PhotonNetwork.CurrentRoom == null)
		{
			Debug.LogError("LCK: Called AnnouncePlayerPresenceForSession() but no room was found. Player not announced.");
			yield break;
		}
		string localUserId = this.m_rigNetworkController.VRRig.OwningNetPlayer.UserId;
		string uniqueUserSessionId = PhotonNetwork.CurrentRoom.Name;
		Debug.Log(string.Concat(new string[] { "LCK: Announcing Presence for local player with UserId: ", localUserId, " + Session ID: ", uniqueUserSessionId, "." }));
		int num;
		for (int attempt = 1; attempt <= 2; attempt = num + 1)
		{
			LckEntitlementsNetworked.<>c__DisplayClass23_0 CS$<>8__locals1 = new LckEntitlementsNetworked.<>c__DisplayClass23_0();
			CS$<>8__locals1.announcementAsync = this._lckCosmeticsCoordinator.AnnouncePlayerPresenceForSessionAsync(localUserId, uniqueUserSessionId);
			yield return new WaitUntil(() => CS$<>8__locals1.announcementAsync.IsCompleted);
			if (!CS$<>8__locals1.announcementAsync.IsFaulted && CS$<>8__locals1.announcementAsync.Result.IsOk)
			{
				Debug.Log("LCK: Successfully set session entitlement.");
				yield break;
			}
			string text = (CS$<>8__locals1.announcementAsync.IsFaulted ? CS$<>8__locals1.announcementAsync.Exception.ToString() : CS$<>8__locals1.announcementAsync.Result.Message.ToString());
			Debug.LogError(string.Format("LCK: Error setting session entitlement (Attempt {0}/{1}): {2}", attempt, 2, text));
			CS$<>8__locals1 = null;
			num = attempt;
		}
		Debug.LogError("LCK: All attempts to set session entitlement failed.");
		yield break;
	}

	private IEnumerator GetNewPlayerCosmeticsForSession()
	{
		string[] array = new string[] { this.m_rigNetworkController.VRRig.OwningNetPlayer.UserId };
		yield return this.GetCosmeticsForPlayersCoroutine(array, "GetNewPlayerCosmeticsForSession");
		yield break;
	}

	private IEnumerator GetAllOtherPlayerCosmeticsForSession()
	{
		IEnumerable<string> enumerable = NetworkSystem.Instance.PlayerListOthers.Select((NetPlayer p) => p.UserId);
		yield return this.GetCosmeticsForPlayersCoroutine(enumerable, "GetAllOtherPlayerCosmeticsForSession");
		yield break;
	}

	private IEnumerator GetCosmeticsForPlayersCoroutine(IEnumerable<string> playerUserIds, string methodNameForLogging)
	{
		List<string> userIdList = ((playerUserIds != null) ? playerUserIds.ToList<string>() : null) ?? new List<string>();
		if (userIdList.Count == 0)
		{
			yield break;
		}
		if (PhotonNetwork.CurrentRoom == null)
		{
			Debug.LogError("LCK: Called " + methodNameForLogging + " but no room was found.");
			yield break;
		}
		string sessionId = PhotonNetwork.CurrentRoom.Name;
		Debug.Log(string.Concat(new string[]
		{
			"LCK: Calling ",
			methodNameForLogging,
			" for session: ",
			sessionId,
			" for players: ",
			string.Join(", ", userIdList),
			"."
		}));
		int num;
		for (int attempt = 1; attempt <= 2; attempt = num + 1)
		{
			LckEntitlementsNetworked.<>c__DisplayClass26_0 CS$<>8__locals1 = new LckEntitlementsNetworked.<>c__DisplayClass26_0();
			CS$<>8__locals1.getUserCosmeticsTask = this._lckCosmeticsCoordinator.GetUserCosmeticsForSessionAsync(userIdList, sessionId);
			yield return new WaitUntil(() => CS$<>8__locals1.getUserCosmeticsTask.IsCompleted);
			if (!CS$<>8__locals1.getUserCosmeticsTask.IsFaulted && CS$<>8__locals1.getUserCosmeticsTask.Result.IsOk)
			{
				Debug.Log("LCK: Successfully called " + methodNameForLogging + " endpoint.");
				yield break;
			}
			string text = (CS$<>8__locals1.getUserCosmeticsTask.IsFaulted ? CS$<>8__locals1.getUserCosmeticsTask.Exception.ToString() : CS$<>8__locals1.getUserCosmeticsTask.Result.Message.ToString());
			Debug.LogError(string.Format("LCK: Error in {0} (Attempt {1}/{2}): {3}", new object[] { methodNameForLogging, attempt, 2, text }));
			CS$<>8__locals1 = null;
			num = attempt;
		}
		Debug.LogError("LCK: All attempts to call " + methodNameForLogging + " failed.");
		yield break;
	}

	private void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		if (this.m_rigNetworkController != null && this.m_rigNetworkController.SuccesfullSpawnEvent != null)
		{
			ListProcessor<InAction<RigContainer, PhotonMessageInfoWrapped>> succesfullSpawnEvent = this.m_rigNetworkController.SuccesfullSpawnEvent;
			InAction<RigContainer, PhotonMessageInfoWrapped> inAction = new InAction<RigContainer, PhotonMessageInfoWrapped>(this.OnSuccessfulSpawn);
			succesfullSpawnEvent.Remove(in inAction);
		}
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
	}

	[Header("Configuration")]
	[SerializeField]
	private VRRigSerializer m_rigNetworkController;

	[InjectLck]
	private ILckCore _lckCore;

	[InjectLck]
	private ILckCosmeticsCoordinator _lckCosmeticsCoordinator;

	[InjectLck]
	private ILckCosmeticsFeatureFlagManager _featureFlagManager;

	private const int MAX_ATTEMPTS = 2;

	private LckEntitlementsNetworked.FeatureState _currentState;

	private readonly Queue<LckEntitlementsNetworked.QueuedSpawn> _queuedSpawns = new Queue<LckEntitlementsNetworked.QueuedSpawn>();

	private enum FeatureState
	{
		Checking,
		Enabled,
		Disabled
	}

	private readonly struct QueuedSpawn
	{
		public QueuedSpawn(RigContainer rig, PhotonMessageInfoWrapped info)
		{
			this.Rig = rig;
			this.Info = info;
		}

		public readonly RigContainer Rig;

		public readonly PhotonMessageInfoWrapped Info;
	}
}
