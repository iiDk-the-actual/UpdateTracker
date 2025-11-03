using System;
using Fusion;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Scripting;

[NetworkBehaviourWeaved(11)]
public class SecondLookSkeletonSynchValues : NetworkComponent
{
	[Networked]
	[NetworkedWeaved(0, 11)]
	public unsafe SkeletonNetData NetData
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing SecondLookSkeletonSynchValues.NetData. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(SkeletonNetData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing SecondLookSkeletonSynchValues.NetData. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(SkeletonNetData*)(this.Ptr + 0) = value;
		}
	}

	protected override void OnOwnerSwitched(NetPlayer newOwningPlayer)
	{
		base.OnOwnerSwitched(newOwningPlayer);
		if (newOwningPlayer.IsLocal)
		{
			this.mySkeleton.SetNodes();
			if (this.mySkeleton.currentState != this.currentState)
			{
				this.mySkeleton.ChangeState(this.currentState);
			}
		}
	}

	public override void WriteDataFusion()
	{
		this.NetData = new SkeletonNetData((int)this.currentState, this.position, this.rotation, this.currentNode, this.nextNode, this.angerPoint);
	}

	public override void ReadDataFusion()
	{
		this.currentState = (SecondLookSkeleton.GhostState)this.NetData.CurrentState;
		Vector3 vector = this.NetData.Position;
		(ref this.position).SetValueSafe(in vector);
		Quaternion quaternion = this.NetData.Rotation;
		(ref this.rotation).SetValueSafe(in quaternion);
		this.currentNode = this.NetData.CurrentNode;
		this.nextNode = this.NetData.NextNode;
		this.angerPoint = this.NetData.AngerPoint;
		if (this.mySkeleton.tapped && this.currentState != this.mySkeleton.currentState)
		{
			this.mySkeleton.ChangeState(this.currentState);
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!base.IsMine && !info.Sender.IsMasterClient)
		{
			return;
		}
		stream.SendNext(this.mySkeleton.currentState);
		stream.SendNext(this.mySkeleton.spookyGhost.transform.position);
		stream.SendNext(this.mySkeleton.spookyGhost.transform.rotation);
		stream.SendNext(this.currentNode);
		stream.SendNext(this.nextNode);
		stream.SendNext(this.angerPoint);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!base.IsMine && !info.Sender.IsMasterClient)
		{
			return;
		}
		this.currentState = (SecondLookSkeleton.GhostState)stream.ReceiveNext();
		Vector3 vector = (Vector3)stream.ReceiveNext();
		(ref this.position).SetValueSafe(in vector);
		Quaternion quaternion = (Quaternion)stream.ReceiveNext();
		(ref this.rotation).SetValueSafe(in quaternion);
		this.currentNode = (int)stream.ReceiveNext();
		this.nextNode = (int)stream.ReceiveNext();
		this.angerPoint = (int)stream.ReceiveNext();
		if (this.mySkeleton.tapped && this.currentState != this.mySkeleton.currentState)
		{
			this.mySkeleton.ChangeState(this.currentState);
		}
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public unsafe void RPC_RemoteActiveGhost(RpcInfo info = default(RpcInfo))
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
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void SecondLookSkeletonSynchValues::RPC_RemoteActiveGhost(Fusion.RpcInfo)", num);
							return;
						}
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 1);
							int num2 = 8;
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
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void SecondLookSkeletonSynchValues::RPC_RemoteActiveGhost(Fusion.RpcInfo)", base.Object, 7);
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		GorillaNot.IncrementRPCCall(info, "RPC_RemoteActiveGhost");
		if (!base.IsMine)
		{
			return;
		}
		this.mySkeleton.RemoteActivateGhost();
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	public unsafe void RPC_RemotePlayerSeen(RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void SecondLookSkeletonSynchValues::RPC_RemotePlayerSeen(Fusion.RpcInfo)", base.Object, 7);
				}
				else
				{
					int num = 8;
					if (!SimulationMessage.CanAllocateUserPayload(num))
					{
						NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void SecondLookSkeletonSynchValues::RPC_RemotePlayerSeen(Fusion.RpcInfo)", num);
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
		GorillaNot.IncrementRPCCall(info, "RPC_RemotePlayerSeen");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Source);
		if (!this.mySkeleton.playersSeen.Contains(player))
		{
			this.mySkeleton.RemotePlayerSeen(player);
		}
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public unsafe void RPC_RemotePlayerCaught(RpcInfo info = default(RpcInfo))
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
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void SecondLookSkeletonSynchValues::RPC_RemotePlayerCaught(Fusion.RpcInfo)", num);
							return;
						}
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 3);
							int num2 = 8;
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
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void SecondLookSkeletonSynchValues::RPC_RemotePlayerCaught(Fusion.RpcInfo)", base.Object, 7);
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		GorillaNot.IncrementRPCCall(info, "RPC_RemotePlayerCaught");
		if (!base.IsMine)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Source);
		if (this.mySkeleton.currentState == SecondLookSkeleton.GhostState.Chasing)
		{
			this.mySkeleton.RemotePlayerCaught(player);
		}
	}

	[PunRPC]
	public void RemoteActivateGhost(PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RemoteActivateGhost");
		if (!base.IsMine)
		{
			return;
		}
		this.mySkeleton.RemoteActivateGhost();
	}

	[PunRPC]
	public void RemotePlayerSeen(PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RemotePlayerSeen");
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		if (!this.mySkeleton.playersSeen.Contains(player))
		{
			this.mySkeleton.RemotePlayerSeen(player);
		}
	}

	[PunRPC]
	public void RemotePlayerCaught(PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "RemotePlayerCaught");
		if (!base.IsMine)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
		if (this.mySkeleton.currentState == SecondLookSkeleton.GhostState.Chasing)
		{
			this.mySkeleton.RemotePlayerCaught(player);
		}
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
	protected unsafe static void RPC_RemoteActiveGhost@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((SecondLookSkeletonSynchValues)behaviour).RPC_RemoteActiveGhost(rpcInfo);
	}

	[NetworkRpcWeavedInvoker(2, 7, 7)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_RemotePlayerSeen@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((SecondLookSkeletonSynchValues)behaviour).RPC_RemotePlayerSeen(rpcInfo);
	}

	[NetworkRpcWeavedInvoker(3, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_RemotePlayerCaught@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((SecondLookSkeletonSynchValues)behaviour).RPC_RemotePlayerCaught(rpcInfo);
	}

	public SecondLookSkeleton.GhostState currentState;

	public Vector3 position;

	public Quaternion rotation;

	public SecondLookSkeleton mySkeleton;

	public int currentNode;

	public int nextNode;

	public int angerPoint;

	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("NetData", 0, 11)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private SkeletonNetData _NetData;
}
