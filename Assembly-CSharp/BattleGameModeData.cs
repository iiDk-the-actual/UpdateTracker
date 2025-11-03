using System;
using Fusion;
using UnityEngine;
using UnityEngine.Scripting;

[NetworkBehaviourWeaved(31)]
public class BattleGameModeData : FusionGameModeData
{
	[Networked]
	[NetworkedWeaved(0, 31)]
	private unsafe PaintbrawlData PaintbrawlData
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing BattleGameModeData.PaintbrawlData. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(PaintbrawlData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing BattleGameModeData.PaintbrawlData. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(PaintbrawlData*)(this.Ptr + 0) = value;
		}
	}

	public override object Data
	{
		get
		{
			return this.PaintbrawlData;
		}
		set
		{
			this.PaintbrawlData = (PaintbrawlData)value;
		}
	}

	public override void Spawned()
	{
		this.serializer = base.GetComponent<GameModeSerializer>();
		this.battleTarget = (GorillaPaintbrawlManager)this.serializer.GameModeInstance;
	}

	[Rpc]
	public unsafe void RPC_ReportSlinshotHit(int taggedPlayerID, Vector3 hitLocation, int projectileCount, RpcInfo rpcInfo = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 7) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void BattleGameModeData::RPC_ReportSlinshotHit(System.Int32,UnityEngine.Vector3,System.Int32,Fusion.RpcInfo)", base.Object, 7);
				}
				else
				{
					int num = 8;
					num += 4;
					num += 12;
					num += 4;
					if (!SimulationMessage.CanAllocateUserPayload(num))
					{
						NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void BattleGameModeData::RPC_ReportSlinshotHit(System.Int32,UnityEngine.Vector3,System.Int32,Fusion.RpcInfo)", num);
					}
					else
					{
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 1);
							int num2 = 8;
							*(int*)(ptr2 + num2) = taggedPlayerID;
							num2 += 4;
							*(Vector3*)(ptr2 + num2) = hitLocation;
							num2 += 12;
							*(int*)(ptr2 + num2) = projectileCount;
							num2 += 4;
							ptr->Offset = num2 * 8;
							base.Runner.SendRpc(ptr);
						}
						if ((localAuthorityMask & 7) != 0)
						{
							rpcInfo = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0012;
						}
					}
				}
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(rpcInfo);
		GorillaNot.IncrementRPCCall(photonMessageInfoWrapped, "RPC_ReportSlinshotHit");
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(taggedPlayerID);
		this.battleTarget.ReportSlingshotHit(player, hitLocation, projectileCount, photonMessageInfoWrapped);
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.PaintbrawlData = this._PaintbrawlData;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._PaintbrawlData = this.PaintbrawlData;
	}

	[NetworkRpcWeavedInvoker(1, 7, 7)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_ReportSlinshotHit@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		int num2 = *(int*)(ptr + num);
		num += 4;
		int num3 = num2;
		Vector3 vector = *(Vector3*)(ptr + num);
		num += 12;
		Vector3 vector2 = vector;
		int num4 = *(int*)(ptr + num);
		num += 4;
		int num5 = num4;
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((BattleGameModeData)behaviour).RPC_ReportSlinshotHit(num3, vector2, num5, rpcInfo);
	}

	[WeaverGenerated]
	[DefaultForProperty("PaintbrawlData", 0, 31)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private PaintbrawlData _PaintbrawlData;

	private GorillaPaintbrawlManager battleTarget;

	private GameModeSerializer serializer;
}
