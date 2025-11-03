using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;
using UnityEngine.Scripting;

[NetworkBehaviourWeaved(0)]
public class FusionPlayerProperties : NetworkBehaviour
{
	[Capacity(10)]
	private NetworkDictionary<PlayerRef, FusionPlayerProperties.PlayerInfo> netPlayerAttributes
	{
		get
		{
			return default(NetworkDictionary<PlayerRef, FusionPlayerProperties.PlayerInfo>);
		}
	}

	public FusionPlayerProperties.PlayerInfo PlayerProperties
	{
		get
		{
			return this.netPlayerAttributes[base.Runner.LocalPlayer];
		}
	}

	private void OnAttributesChanged()
	{
		FusionPlayerProperties.PlayerAttributeOnChanged playerAttributeOnChanged = this.playerAttributeOnChanged;
		if (playerAttributeOnChanged == null)
		{
			return;
		}
		playerAttributeOnChanged();
	}

	[Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true, TickAligned = true)]
	public unsafe void RPC_UpdatePlayerAttributes(FusionPlayerProperties.PlayerInfo newInfo, RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void FusionPlayerProperties::RPC_UpdatePlayerAttributes(FusionPlayerProperties/PlayerInfo,Fusion.RpcInfo)", base.Object, 7);
				}
				else
				{
					int num = 8;
					num += 960;
					if (!SimulationMessage.CanAllocateUserPayload(num))
					{
						NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void FusionPlayerProperties::RPC_UpdatePlayerAttributes(FusionPlayerProperties/PlayerInfo,Fusion.RpcInfo)", num);
					}
					else
					{
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 1);
							int num2 = 8;
							*(FusionPlayerProperties.PlayerInfo*)(ptr2 + num2) = newInfo;
							num2 += 960;
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
		Debug.Log("Update Player attributes triggered");
		PlayerRef source = info.Source;
		if (this.netPlayerAttributes.ContainsKey(source))
		{
			Debug.Log("Current nickname is " + this.netPlayerAttributes[source].NickName.ToString());
			Debug.Log("Sent nickname is " + newInfo.NickName.ToString());
			if (this.netPlayerAttributes[source].Equals(newInfo))
			{
				Debug.Log("Info is already correct for this user. Shouldnt have received an RPC in this case.");
				return;
			}
		}
		this.netPlayerAttributes.Set(source, newInfo);
	}

	public override void Spawned()
	{
		Debug.Log("Player props SPAWNED!");
		if (base.Runner.Mode == SimulationModes.Client)
		{
			Debug.Log("SET Player Properties manager!");
		}
	}

	public string GetDisplayName(PlayerRef player)
	{
		return this.netPlayerAttributes[player].NickName.Value;
	}

	public string GetLocalDisplayName()
	{
		return this.netPlayerAttributes[base.Runner.LocalPlayer].NickName.Value;
	}

	public bool GetProperty(PlayerRef player, string propertyName, out string propertyValue)
	{
		NetworkString<_32> networkString;
		if (this.netPlayerAttributes[player].properties.TryGet(propertyName, out networkString))
		{
			propertyValue = networkString.Value;
			return true;
		}
		propertyValue = null;
		return false;
	}

	public bool PlayerHasEntry(PlayerRef player)
	{
		return this.netPlayerAttributes.ContainsKey(player);
	}

	public void RemovePlayerEntry(PlayerRef player)
	{
		if (base.Object.HasStateAuthority)
		{
			string value = this.netPlayerAttributes[player].NickName.Value;
			this.netPlayerAttributes.Remove(player);
			Debug.Log("Removed " + value + "player properties as they just left.");
		}
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
	}

	[NetworkRpcWeavedInvoker(1, 7, 7)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_UpdatePlayerAttributes@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		FusionPlayerProperties.PlayerInfo playerInfo = *(FusionPlayerProperties.PlayerInfo*)(ptr + num);
		num += 960;
		FusionPlayerProperties.PlayerInfo playerInfo2 = playerInfo;
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((FusionPlayerProperties)behaviour).RPC_UpdatePlayerAttributes(playerInfo2, rpcInfo);
	}

	public FusionPlayerProperties.PlayerAttributeOnChanged playerAttributeOnChanged;

	[NetworkStructWeaved(240)]
	[StructLayout(LayoutKind.Explicit, Size = 960)]
	public struct PlayerInfo : INetworkStruct
	{
		[Networked]
		[NetworkedWeaved(0, 33)]
		public unsafe NetworkString<_32> NickName
		{
			readonly get
			{
				return *(NetworkString<_32>*)Native.ReferenceToPointer<FixedStorage@33>(ref this._NickName);
			}
			set
			{
				*(NetworkString<_32>*)Native.ReferenceToPointer<FixedStorage@33>(ref this._NickName) = value;
			}
		}

		[Networked]
		[NetworkedWeavedDictionary(3, 33, 33, typeof(ReaderWriter@Fusion_NetworkString), typeof(ReaderWriter@Fusion_NetworkString))]
		[NetworkedWeaved(33, 207)]
		public unsafe NetworkDictionary<NetworkString<_32>, NetworkString<_32>> properties
		{
			get
			{
				return new NetworkDictionary<NetworkString<_32>, NetworkString<_32>>((int*)Native.ReferenceToPointer<FixedStorage@207>(ref this._properties), 3, ReaderWriter@Fusion_NetworkString.GetInstance(), ReaderWriter@Fusion_NetworkString.GetInstance());
			}
		}

		[FixedBufferProperty(typeof(NetworkString<_32>), typeof(UnityValueSurrogate@ReaderWriter@Fusion_NetworkString), 0, order = -2147483647)]
		[WeaverGenerated]
		[SerializeField]
		[FieldOffset(0)]
		private FixedStorage@33 _NickName;

		[FixedBufferProperty(typeof(NetworkDictionary<NetworkString<_32>, NetworkString<_32>>), typeof(UnityDictionarySurrogate@ReaderWriter@Fusion_NetworkString`1<Fusion__32>@ReaderWriter@Fusion_NetworkString), 3, order = -2147483647)]
		[WeaverGenerated]
		[SerializeField]
		[FieldOffset(132)]
		private FixedStorage@207 _properties;
	}

	public delegate void PlayerAttributeOnChanged();
}
