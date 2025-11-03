using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Scripting;

public class FusionCallbackHandler : SimulationBehaviour, INetworkRunnerCallbacks, IPublicFacingInterface
{
	public void Setup(NetworkSystemFusion parentController)
	{
		this.parent = parentController;
		this.parent.runner.AddCallbacks(new INetworkRunnerCallbacks[] { this });
	}

	private void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		this.RemoveCallbacks();
	}

	private async void RemoveCallbacks()
	{
		await Task.Delay(500);
		this.parent.runner.RemoveCallbacks(new INetworkRunnerCallbacks[] { this });
	}

	public void OnConnectedToServer(NetworkRunner runner)
	{
		this.parent.OnJoinedSession();
	}

	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
		this.parent.OnJoinFailed(reason);
	}

	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
	}

	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
		this.parent.CustomAuthenticationResponse(data);
		Debug.Log("Received custom auth response:");
		foreach (KeyValuePair<string, object> keyValuePair in data)
		{
			Debug.Log(keyValuePair.Key + ":" + (keyValuePair.Value as string));
		}
	}

	public void OnDisconnectedFromServer(NetworkRunner runner)
	{
		this.parent.OnDisconnectedFromSession();
	}

	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
		this.parent.MigrateHost(runner, hostMigrationToken);
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		NetworkedInput input2 = NetInput.GetInput();
		input.Set<NetworkedInput>(input2);
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		this.parent.OnFusionPlayerJoined(player);
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		this.parent.OnFusionPlayerLeft(player);
	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
	{
	}

	public void OnSceneLoadDone(NetworkRunner runner)
	{
	}

	public void OnSceneLoadStart(NetworkRunner runner)
	{
	}

	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
		this.parent.OnRunnerShutDown();
	}

	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
	}

	[Rpc(Channel = RpcChannel.Reliable)]
	public unsafe static void RPC_OnEventRaisedReliable(NetworkRunner runner, byte eventCode, byte[] byteData, bool hasOps, byte[] netOptsData, RpcInfo info = default(RpcInfo))
	{
		if (NetworkBehaviourUtils.InvokeRpc)
		{
			NetworkBehaviourUtils.InvokeRpc = false;
		}
		else
		{
			if (runner == null)
			{
				throw new ArgumentNullException("runner");
			}
			if (runner.Stage != SimulationStages.Resimulate)
			{
				int num = 8;
				num += 4;
				num += (byteData.Length * 1 + 4 + 3) & -4;
				num += 4;
				num += (netOptsData.Length * 1 + 4 + 3) & -4;
				if (SimulationMessage.CanAllocateUserPayload(num))
				{
					if (runner.HasAnyActiveConnections())
					{
						SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
						byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
						*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void FusionCallbackHandler::RPC_OnEventRaisedReliable(Fusion.NetworkRunner,System.Byte,System.Byte[],System.Boolean,System.Byte[],Fusion.RpcInfo)"));
						int num2 = 8;
						ptr2[num2] = eventCode;
						num2 += (1 + 3) & -4;
						*(int*)(ptr2 + num2) = byteData.Length;
						num2 += 4;
						num2 = ((Native.CopyFromArray<byte>((void*)(ptr2 + num2), byteData) + 3) & -4) + num2;
						ReadWriteUtilsForWeaver.WriteBoolean((int*)(ptr2 + num2), hasOps);
						num2 += 4;
						*(int*)(ptr2 + num2) = netOptsData.Length;
						num2 += 4;
						num2 = ((Native.CopyFromArray<byte>((void*)(ptr2 + num2), netOptsData) + 3) & -4) + num2;
						ptr->Offset = num2 * 8;
						ptr->SetStatic();
						runner.SendRpc(ptr);
					}
					info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
					goto IL_0010;
				}
				NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void FusionCallbackHandler::RPC_OnEventRaisedReliable(Fusion.NetworkRunner,System.Byte,System.Byte[],System.Boolean,System.Byte[],Fusion.RpcInfo)", num);
			}
			return;
		}
		IL_0010:
		object obj = byteData.ByteDeserialize();
		NetEventOptions netEventOptions = null;
		if (hasOps)
		{
			netEventOptions = (NetEventOptions)netOptsData.ByteDeserialize();
		}
		if (!FusionCallbackHandler.CanRecieveEvent(runner, netEventOptions, info))
		{
			return;
		}
		NetworkSystem.Instance.RaiseEvent(eventCode, obj, info.Source.PlayerId);
	}

	[Rpc(Channel = RpcChannel.Unreliable)]
	public unsafe static void RPC_OnEventRaisedUnreliable(NetworkRunner runner, byte eventCode, byte[] byteData, bool hasOps, byte[] netOptsData, RpcInfo info = default(RpcInfo))
	{
		if (NetworkBehaviourUtils.InvokeRpc)
		{
			NetworkBehaviourUtils.InvokeRpc = false;
		}
		else
		{
			if (runner == null)
			{
				throw new ArgumentNullException("runner");
			}
			if (runner.Stage != SimulationStages.Resimulate)
			{
				int num = 8;
				num += 4;
				num += (byteData.Length * 1 + 4 + 3) & -4;
				num += 4;
				num += (netOptsData.Length * 1 + 4 + 3) & -4;
				if (SimulationMessage.CanAllocateUserPayload(num))
				{
					if (runner.HasAnyActiveConnections())
					{
						SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
						byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
						*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void FusionCallbackHandler::RPC_OnEventRaisedUnreliable(Fusion.NetworkRunner,System.Byte,System.Byte[],System.Boolean,System.Byte[],Fusion.RpcInfo)"));
						int num2 = 8;
						ptr2[num2] = eventCode;
						num2 += (1 + 3) & -4;
						*(int*)(ptr2 + num2) = byteData.Length;
						num2 += 4;
						num2 = ((Native.CopyFromArray<byte>((void*)(ptr2 + num2), byteData) + 3) & -4) + num2;
						ReadWriteUtilsForWeaver.WriteBoolean((int*)(ptr2 + num2), hasOps);
						num2 += 4;
						*(int*)(ptr2 + num2) = netOptsData.Length;
						num2 += 4;
						num2 = ((Native.CopyFromArray<byte>((void*)(ptr2 + num2), netOptsData) + 3) & -4) + num2;
						ptr->Offset = num2 * 8;
						ptr->SetUnreliable();
						ptr->SetStatic();
						runner.SendRpc(ptr);
					}
					info = RpcInfo.FromLocal(runner, RpcChannel.Unreliable, RpcHostMode.SourceIsServer);
					goto IL_0010;
				}
				NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void FusionCallbackHandler::RPC_OnEventRaisedUnreliable(Fusion.NetworkRunner,System.Byte,System.Byte[],System.Boolean,System.Byte[],Fusion.RpcInfo)", num);
			}
			return;
		}
		IL_0010:
		object obj = byteData.ByteDeserialize();
		NetEventOptions netEventOptions = null;
		if (hasOps)
		{
			netEventOptions = (NetEventOptions)netOptsData.ByteDeserialize();
		}
		if (!FusionCallbackHandler.CanRecieveEvent(runner, netEventOptions, info))
		{
			return;
		}
		NetworkSystem.Instance.RaiseEvent(eventCode, obj, info.Source.PlayerId);
	}

	private static bool CanRecieveEvent(NetworkRunner runner, NetEventOptions opts, RpcInfo info)
	{
		if (opts != null)
		{
			if (opts.Reciever != NetEventOptions.RecieverTarget.all)
			{
				if (opts.Reciever == NetEventOptions.RecieverTarget.master && !NetworkSystem.Instance.IsMasterClient)
				{
					return false;
				}
				if (info.Source == runner.LocalPlayer)
				{
					return false;
				}
			}
			if (opts.TargetActors != null && !opts.TargetActors.Contains(runner.LocalPlayer.PlayerId))
			{
				return false;
			}
		}
		return true;
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
	}

	[NetworkRpcStaticWeavedInvoker("System.Void FusionCallbackHandler::RPC_OnEventRaisedReliable(Fusion.NetworkRunner,System.Byte,System.Byte[],System.Boolean,System.Byte[],Fusion.RpcInfo)")]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_OnEventRaisedReliable@Invoker(NetworkRunner runner, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		byte b = ptr[num];
		num += (1 + 3) & -4;
		byte b2 = b;
		byte[] array = new byte[*(int*)(ptr + num)];
		num += 4;
		num = ((Native.CopyToArray<byte>(array, (void*)(ptr + num)) + 3) & -4) + num;
		bool flag = ReadWriteUtilsForWeaver.ReadBoolean((int*)(ptr + num));
		num += 4;
		bool flag2 = flag;
		byte[] array2 = new byte[*(int*)(ptr + num)];
		num += 4;
		num = ((Native.CopyToArray<byte>(array2, (void*)(ptr + num)) + 3) & -4) + num;
		RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
		NetworkBehaviourUtils.InvokeRpc = true;
		FusionCallbackHandler.RPC_OnEventRaisedReliable(runner, b2, array, flag2, array2, rpcInfo);
	}

	[NetworkRpcStaticWeavedInvoker("System.Void FusionCallbackHandler::RPC_OnEventRaisedUnreliable(Fusion.NetworkRunner,System.Byte,System.Byte[],System.Boolean,System.Byte[],Fusion.RpcInfo)")]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_OnEventRaisedUnreliable@Invoker(NetworkRunner runner, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		byte b = ptr[num];
		num += (1 + 3) & -4;
		byte b2 = b;
		byte[] array = new byte[*(int*)(ptr + num)];
		num += 4;
		num = ((Native.CopyToArray<byte>(array, (void*)(ptr + num)) + 3) & -4) + num;
		bool flag = ReadWriteUtilsForWeaver.ReadBoolean((int*)(ptr + num));
		num += 4;
		bool flag2 = flag;
		byte[] array2 = new byte[*(int*)(ptr + num)];
		num += 4;
		num = ((Native.CopyToArray<byte>(array2, (void*)(ptr + num)) + 3) & -4) + num;
		RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
		NetworkBehaviourUtils.InvokeRpc = true;
		FusionCallbackHandler.RPC_OnEventRaisedUnreliable(runner, b2, array, flag2, array2, rpcInfo);
	}

	private NetworkSystemFusion parent;
}
