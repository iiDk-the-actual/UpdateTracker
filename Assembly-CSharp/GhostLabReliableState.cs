using System;
using Fusion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Scripting;

[NetworkBehaviourWeaved(11)]
public class GhostLabReliableState : NetworkComponent
{
	[Networked]
	[NetworkedWeaved(0, 11)]
	private unsafe GhostLabData NetData
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GhostLabReliableState.NetData. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(GhostLabData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GhostLabReliableState.NetData. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(GhostLabData*)(this.Ptr + 0) = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.singleDoorOpen = new bool[this.singleDoorCount];
	}

	public override void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		base.OnOwnerChange(newOwner, previousOwner);
		Player localPlayer = PhotonNetwork.LocalPlayer;
	}

	public override void WriteDataFusion()
	{
		this.NetData = new GhostLabData((int)this.doorState, this.singleDoorOpen);
	}

	public override void ReadDataFusion()
	{
		this.doorState = (GhostLab.EntranceDoorsState)this.NetData.DoorState;
		for (int i = 0; i < this.singleDoorCount; i++)
		{
			this.singleDoorOpen[i] = this.NetData.OpenDoors[i];
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!base.IsMine && !info.Sender.IsMasterClient)
		{
			return;
		}
		stream.SendNext(this.doorState);
		for (int i = 0; i < this.singleDoorOpen.Length; i++)
		{
			stream.SendNext(this.singleDoorOpen[i]);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!base.IsMine && !info.Sender.IsMasterClient)
		{
			return;
		}
		this.doorState = (GhostLab.EntranceDoorsState)stream.ReceiveNext();
		for (int i = 0; i < this.singleDoorOpen.Length; i++)
		{
			this.singleDoorOpen[i] = (bool)stream.ReceiveNext();
		}
	}

	public void UpdateEntranceDoorsState(GhostLab.EntranceDoorsState newState)
	{
		if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.IsMasterClient)
		{
			this.doorState = newState;
			return;
		}
		if (NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.IsMasterClient)
		{
			base.SendRPC("RemoteEntranceDoorState", RpcTarget.MasterClient, new object[] { newState });
		}
	}

	public void UpdateSingleDoorState(int singleDoorIndex)
	{
		if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.IsMasterClient)
		{
			this.singleDoorOpen[singleDoorIndex] = !this.singleDoorOpen[singleDoorIndex];
			return;
		}
		if (NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.IsMasterClient)
		{
			base.SendRPC("RemoteSingleDoorState", RpcTarget.MasterClient, new object[] { singleDoorIndex });
		}
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public unsafe void RPC_RemoteEntranceDoorState(GhostLab.EntranceDoorsState newState, RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) != 0)
				{
					if ((localAuthorityMask & 1) != 1)
					{
						int num = 8;
						num += 4;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GhostLabReliableState::RPC_RemoteEntranceDoorState(GhostLab/EntranceDoorsState,Fusion.RpcInfo)", num);
							return;
						}
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 1);
							int num2 = 8;
							*(GhostLab.EntranceDoorsState*)(ptr2 + num2) = newState;
							num2 += 4;
							ptr->Offset = num2 * 8;
							base.Runner.SendRpc(ptr);
						}
						if ((localAuthorityMask & 1) == 0)
						{
							return;
						}
					}
					info = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
					goto IL_0012;
				}
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void GhostLabReliableState::RPC_RemoteEntranceDoorState(GhostLab/EntranceDoorsState,Fusion.RpcInfo)", base.Object, 7);
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		GorillaNot.IncrementRPCCall(info, "RPC_RemoteEntranceDoorState");
		if (!base.IsMine)
		{
			return;
		}
		this.doorState = newState;
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public unsafe void RPC_RemoteSingleDoorState(int doorIndex, RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) != 0)
				{
					if ((localAuthorityMask & 1) != 1)
					{
						int num = 8;
						num += 4;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GhostLabReliableState::RPC_RemoteSingleDoorState(System.Int32,Fusion.RpcInfo)", num);
							return;
						}
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 2);
							int num2 = 8;
							*(int*)(ptr2 + num2) = doorIndex;
							num2 += 4;
							ptr->Offset = num2 * 8;
							base.Runner.SendRpc(ptr);
						}
						if ((localAuthorityMask & 1) == 0)
						{
							return;
						}
					}
					info = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
					goto IL_0012;
				}
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void GhostLabReliableState::RPC_RemoteSingleDoorState(System.Int32,Fusion.RpcInfo)", base.Object, 7);
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		GorillaNot.IncrementRPCCall(info, "RPC_RemoteSingleDoorState");
		if (!base.IsMine)
		{
			return;
		}
		if (doorIndex >= this.singleDoorCount)
		{
			return;
		}
		this.singleDoorOpen[doorIndex] = !this.singleDoorOpen[doorIndex];
	}

	[PunRPC]
	public void RemoteEntranceDoorState(GhostLab.EntranceDoorsState newState, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RemoteEntranceDoorState");
		if (!base.IsMine)
		{
			return;
		}
		this.doorState = newState;
	}

	[PunRPC]
	public void RemoteSingleDoorState(int doorIndex, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RemoteSingleDoorState");
		if (!base.IsMine)
		{
			return;
		}
		if (doorIndex >= this.singleDoorCount)
		{
			return;
		}
		this.singleDoorOpen[doorIndex] = !this.singleDoorOpen[doorIndex];
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.NetData = this._NetData;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._NetData = this.NetData;
	}

	[NetworkRpcWeavedInvoker(1, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_RemoteEntranceDoorState@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		GhostLab.EntranceDoorsState entranceDoorsState = *(GhostLab.EntranceDoorsState*)(ptr + num);
		num += 4;
		GhostLab.EntranceDoorsState entranceDoorsState2 = entranceDoorsState;
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((GhostLabReliableState)behaviour).RPC_RemoteEntranceDoorState(entranceDoorsState2, rpcInfo);
	}

	[NetworkRpcWeavedInvoker(2, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_RemoteSingleDoorState@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		int num2 = *(int*)(ptr + num);
		num += 4;
		int num3 = num2;
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((GhostLabReliableState)behaviour).RPC_RemoteSingleDoorState(num3, rpcInfo);
	}

	public GhostLab.EntranceDoorsState doorState;

	public int singleDoorCount;

	public bool[] singleDoorOpen;

	[WeaverGenerated]
	[DefaultForProperty("NetData", 0, 11)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private GhostLabData _NetData;
}
