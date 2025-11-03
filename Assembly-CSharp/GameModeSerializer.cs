using System;
using Fusion;
using GorillaExtensions;
using GorillaGameModes;
using GorillaTag;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Scripting;

[NetworkBehaviourWeaved(1)]
internal class GameModeSerializer : GorillaSerializerMasterOnly, IStateAuthorityChanged, IPublicFacingInterface
{
	[Networked]
	[NetworkedWeaved(0, 1)]
	private unsafe int gameModeKeyInt
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GameModeSerializer.gameModeKeyInt. Networked properties can only be accessed when Spawned() has been called.");
			}
			return this.Ptr[0];
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GameModeSerializer.gameModeKeyInt. Networked properties can only be accessed when Spawned() has been called.");
			}
			this.Ptr[0] = value;
		}
	}

	public GorillaGameManager GameModeInstance
	{
		get
		{
			return this.gameModeInstance;
		}
	}

	protected override bool OnSpawnSetupCheck(PhotonMessageInfoWrapped wrappedInfo, out GameObject outTargetObject, out Type outTargetType)
	{
		outTargetObject = null;
		outTargetType = null;
		NetPlayer player = NetworkSystem.Instance.GetPlayer(wrappedInfo.senderID);
		if (player != null)
		{
			GorillaNot.IncrementRPCCall(wrappedInfo, "OnSpawnSetupCheck");
		}
		GameModeSerializer activeNetworkHandler = global::GorillaGameModes.GameMode.ActiveNetworkHandler;
		if (player != null && player.InRoom)
		{
			if (!player.IsMasterClient)
			{
				GTDev.LogError<string>("SPAWN FAIL NOT MASTER :" + player.UserId + player.NickName, null);
				GorillaNot.instance.SendReport("trying to inappropriately create game managers", player.UserId, player.NickName);
				return false;
			}
			if (!this.netView.IsRoomView)
			{
				GTDev.LogError<string>("SPAWN FAIL ROOM VIEW" + player.UserId + player.NickName, null);
				GorillaNot.instance.SendReport("creating game manager as player object", player.UserId, player.NickName);
				return false;
			}
			if (activeNetworkHandler.IsNotNull() && activeNetworkHandler != this)
			{
				GTDev.LogError<string>("DUPLICATE CHECK" + player.UserId + player.NickName, null);
				GorillaNot.instance.SendReport("trying to create multiple game managers", player.UserId, player.NickName);
				return false;
			}
		}
		else if ((activeNetworkHandler.IsNotNull() && activeNetworkHandler != this) || !this.netView.IsRoomView)
		{
			GTDev.LogError<string>("ACTIVE HANDLER CHECK FAIL" + ((player != null) ? player.UserId : null) + ((player != null) ? player.NickName : null), null);
			GTDev.LogError<string>("existing game manager! destroying newly created manager", null);
			return false;
		}
		object[] instantiationData = wrappedInfo.punInfo.photonView.InstantiationData;
		if (instantiationData != null && instantiationData.Length >= 1)
		{
			object obj = instantiationData[0];
			if (obj is int)
			{
				int num = (int)obj;
				this.gameModeKey = (GameModeType)num;
				this.gameModeInstance = global::GorillaGameModes.GameMode.GetGameModeInstance(this.gameModeKey);
				if (this.gameModeInstance.IsNull() || !this.gameModeInstance.ValidGameMode())
				{
					return false;
				}
				this.serializeTarget = this.gameModeInstance;
				base.transform.parent = VRRigCache.Instance.NetworkParent;
				return true;
			}
		}
		GTDev.LogError<string>("missing instantiation data", null);
		return false;
	}

	internal void Init(int gameModeType)
	{
		Debug.Log("<color=red>Init called</color>");
		this.gameModeKeyInt = gameModeType;
	}

	protected override void OnSuccesfullySpawned(PhotonMessageInfoWrapped info)
	{
		this.netView.GetView.AddCallbackTarget(this);
		global::GorillaGameModes.GameMode.SetupGameModeRemote(this);
	}

	protected override void OnBeforeDespawn()
	{
		global::GorillaGameModes.GameMode.RemoveNetworkLink(this);
	}

	protected override void OnFailedSpawn()
	{
	}

	[PunRPC]
	internal void RPC_ReportTag(int taggedPlayer, PhotonMessageInfo info)
	{
		this.ReportTag(NetworkSystem.Instance.GetPlayer(taggedPlayer), new PhotonMessageInfoWrapped(info));
	}

	[PunRPC]
	internal void RPC_ReportHit(PhotonMessageInfo info)
	{
		this.ReportHit(new PhotonMessageInfoWrapped(info));
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	internal unsafe void RPC_ReportTag(int taggedPlayer, RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void GameModeSerializer::RPC_ReportTag(System.Int32,Fusion.RpcInfo)", base.Object, 7);
				}
				else
				{
					int num = 8;
					num += 4;
					if (!SimulationMessage.CanAllocateUserPayload(num))
					{
						NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GameModeSerializer::RPC_ReportTag(System.Int32,Fusion.RpcInfo)", num);
					}
					else
					{
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 1);
							int num2 = 8;
							*(int*)(ptr2 + num2) = taggedPlayer;
							num2 += 4;
							ptr->Offset = num2 * 8;
							base.Runner.SendRpc(ptr);
						}
						if ((localAuthorityMask & 7) != 0)
						{
							info = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0012;
						}
					}
				}
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		this.ReportTag(NetworkSystem.Instance.GetPlayer(taggedPlayer), new PhotonMessageInfoWrapped(info));
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	internal unsafe void RPC_ReportHit(RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void GameModeSerializer::RPC_ReportHit(Fusion.RpcInfo)", base.Object, 7);
				}
				else
				{
					int num = 8;
					if (!SimulationMessage.CanAllocateUserPayload(num))
					{
						NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GameModeSerializer::RPC_ReportHit(Fusion.RpcInfo)", num);
					}
					else
					{
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 2);
							int num2 = 8;
							ptr->Offset = num2 * 8;
							base.Runner.SendRpc(ptr);
						}
						if ((localAuthorityMask & 7) != 0)
						{
							info = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0012;
						}
					}
				}
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		this.ReportHit(new PhotonMessageInfoWrapped(info));
	}

	private void ReportTag(NetPlayer taggedPlayer, PhotonMessageInfoWrapped info)
	{
		GorillaNot.IncrementRPCCall(info, "ReportTag");
		NetPlayer sender = info.Sender;
		this.gameModeInstance.ReportTag(taggedPlayer, sender);
	}

	private void ReportHit(PhotonMessageInfoWrapped info)
	{
		GorillaNot.IncrementRPCCall(info, "ReportContactWithLavaRPC");
		bool flag = ZoneManagement.instance.IsZoneActive(GTZone.customMaps);
		bool flag2 = false;
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			InfectionLavaController instance = InfectionLavaController.Instance;
			flag2 = instance != null && instance.LavaCurrentlyActivated && (instance.SurfaceCenter - rigContainer.Rig.syncPos).sqrMagnitude < 2500f && instance.LavaPlane.GetDistanceToPoint(rigContainer.Rig.syncPos) < 5f;
		}
		if (flag || flag2)
		{
			this.GameModeInstance.HitPlayer(info.Sender);
		}
	}

	[PunRPC]
	internal void RPC_BroadcastRoundComplete(PhotonMessageInfo info)
	{
		this.BroadcastRoundComplete(info);
	}

	private void BroadcastRoundComplete(PhotonMessageInfoWrapped info)
	{
		GorillaNot.IncrementRPCCall(info, "BroadcastRoundComplete");
		if (info.Sender.IsMasterClient)
		{
			this.gameModeInstance.HandleRoundComplete();
		}
	}

	[PunRPC]
	internal void RPC_BroadcastTag(int taggedPlayer, int taggingPlayer, PhotonMessageInfo info)
	{
		this.BroadcastTag(NetworkSystem.Instance.GetPlayer(taggedPlayer), NetworkSystem.Instance.GetPlayer(taggingPlayer), info);
	}

	private void BroadcastTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "BroadcastTag");
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		if (taggedPlayer == null || taggingPlayer == null)
		{
			return;
		}
		if (!this.broadcastTagCallLimit.CheckCallTime(Time.time))
		{
			return;
		}
		this.gameModeInstance.HandleTagBroadcast(taggedPlayer, taggingPlayer);
	}

	protected override void FusionDataRPC(string method, NetPlayer targetPlayer, params object[] parameters)
	{
		Debug.Log(this.gameModeData.GetType().Name);
	}

	protected override void FusionDataRPC(string method, RpcTarget target, params object[] parameters)
	{
		base.FusionDataRPC(method, target, parameters);
	}

	void IStateAuthorityChanged.StateAuthorityChanged()
	{
		GameModeSerializer.FusionGameModeOwnerChanged(NetworkSystem.Instance.GetPlayer(base.Object.StateAuthority));
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.gameModeKeyInt = this._gameModeKeyInt;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._gameModeKeyInt = this.gameModeKeyInt;
	}

	[NetworkRpcWeavedInvoker(1, 7, 7)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_ReportTag@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		int num2 = *(int*)(ptr + num);
		num += 4;
		int num3 = num2;
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((GameModeSerializer)behaviour).RPC_ReportTag(num3, rpcInfo);
	}

	[NetworkRpcWeavedInvoker(2, 7, 7)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_ReportHit@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((GameModeSerializer)behaviour).RPC_ReportHit(rpcInfo);
	}

	[WeaverGenerated]
	[DefaultForProperty("gameModeKeyInt", 0, 1)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private int _gameModeKeyInt;

	private GameModeType gameModeKey;

	private GorillaGameManager gameModeInstance;

	private FusionGameModeData gameModeData;

	private Type currentGameDataType;

	private CallLimiter broadcastTagCallLimit = new CallLimiter(12, 5f, 0.5f);

	public static Action<NetPlayer> FusionGameModeOwnerChanged;
}
