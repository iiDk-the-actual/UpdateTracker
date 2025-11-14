using System;
using System.Collections.Generic;
using Fusion;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public class GameAgentManager : NetworkComponent, ITickSystemTick
{
	public bool TickRunning { get; set; }

	protected override void Awake()
	{
		this.agents = new List<GameAgent>(128);
		this.netIdsForDestination = new List<int>();
		this.destinationsForDestination = new List<Vector3>();
		this.netIdsForState = new List<int>();
		this.statesForState = new List<byte>();
		this.netIdsForBehavior = new List<int>();
		this.behaviorsForBehavior = new List<byte>();
		this.nextAgentIndexUpdate = 0;
		this.nextAgentIndexThink = 0;
	}

	private new void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		TickSystem<object>.AddCallbackTarget(this);
	}

	private new void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		TickSystem<object>.RemoveCallbackTarget(this);
	}

	public static GameAgentManager Get(GameEntity gameEntity)
	{
		if (!(gameEntity == null) && !(gameEntity.manager == null))
		{
			return gameEntity.manager.gameAgentManager;
		}
		return null;
	}

	public List<GameAgent> GetAgents()
	{
		return this.agents;
	}

	public int GetGameAgentCount()
	{
		return this.agents.Count;
	}

	public void AddGameAgent(GameAgent gameAgent)
	{
		this.agents.Add(gameAgent);
	}

	public void RemoveGameAgent(GameAgent gameAgent)
	{
		this.agents.Remove(gameAgent);
	}

	public GameAgent GetGameAgent(GameEntityId id)
	{
		return this.entityManager.GetGameEntity(id).GetComponent<GameAgent>();
	}

	public void Tick()
	{
		if (this.IsAuthority())
		{
			int num = Mathf.Min(1, this.agents.Count);
			for (int i = 0; i < num; i++)
			{
				if (this.nextAgentIndexThink >= this.agents.Count)
				{
					this.nextAgentIndexThink = 0;
				}
				this.agents[this.nextAgentIndexThink].OnThink(Time.deltaTime);
				this.nextAgentIndexThink++;
			}
		}
		for (int j = 0; j < this.agents.Count; j++)
		{
			if (this.agents[j] != null)
			{
				this.agents[j].OnUpdate();
			}
		}
		if (this.IsAuthority())
		{
			if (this.netIdsForDestination.Count > 0 && Time.time > this.lastDestinationSentTime + this.destinationCooldown)
			{
				this.lastDestinationSentTime = Time.time;
				base.SendRPC("ApplyDestinationRPC", RpcTarget.All, new object[]
				{
					this.netIdsForDestination.ToArray(),
					this.destinationsForDestination.ToArray()
				});
				this.netIdsForDestination.Clear();
				this.destinationsForDestination.Clear();
			}
			if (this.netIdsForState.Count > 0 && Time.time > this.lastStateSentTime + this.stateCooldown)
			{
				this.lastStateSentTime = Time.time;
				base.SendRPC("ApplyStateRPC", RpcTarget.All, new object[]
				{
					this.netIdsForState.ToArray(),
					this.statesForState.ToArray()
				});
				this.netIdsForState.Clear();
				this.statesForState.Clear();
			}
			if (this.netIdsForBehavior.Count > 0 && Time.time > this.lastBehaviorSentTime + this.behaviorCooldown)
			{
				this.lastBehaviorSentTime = Time.time;
				base.SendRPC("ApplyBehaviorRPC", RpcTarget.All, new object[]
				{
					this.netIdsForBehavior.ToArray(),
					this.behaviorsForBehavior.ToArray()
				});
				this.netIdsForBehavior.Clear();
				this.behaviorsForBehavior.Clear();
			}
		}
	}

	public bool IsAuthority()
	{
		return this.entityManager.IsAuthority();
	}

	public bool IsAuthorityPlayer(NetPlayer player)
	{
		return this.entityManager.IsAuthorityPlayer(player);
	}

	public bool IsAuthorityPlayer(Player player)
	{
		return this.entityManager.IsAuthorityPlayer(player);
	}

	public Player GetAuthorityPlayer()
	{
		return this.entityManager.GetAuthorityPlayer();
	}

	public bool IsZoneActive()
	{
		return this.entityManager.IsZoneActive();
	}

	public bool IsPositionInZone(Vector3 pos)
	{
		return this.entityManager.IsPositionInZone(pos);
	}

	public bool IsValidClientRPC(Player sender)
	{
		return this.entityManager.IsValidClientRPC(sender);
	}

	public bool IsValidClientRPC(Player sender, int entityNetId)
	{
		return this.entityManager.IsValidClientRPC(sender, entityNetId);
	}

	public bool IsValidClientRPC(Player sender, int entityNetId, Vector3 pos)
	{
		return this.entityManager.IsValidClientRPC(sender, entityNetId, pos);
	}

	public bool IsValidClientRPC(Player sender, Vector3 pos)
	{
		return this.entityManager.IsValidClientRPC(sender, pos);
	}

	public bool IsValidAuthorityRPC(Player sender)
	{
		return this.entityManager.IsValidAuthorityRPC(sender);
	}

	public bool IsValidAuthorityRPC(Player sender, int entityNetId)
	{
		return this.entityManager.IsValidAuthorityRPC(sender, entityNetId);
	}

	public bool IsValidAuthorityRPC(Player sender, int entityNetId, Vector3 pos)
	{
		return this.entityManager.IsValidAuthorityRPC(sender, entityNetId, pos);
	}

	public bool IsValidAuthorityRPC(Player sender, Vector3 pos)
	{
		return this.entityManager.IsValidAuthorityRPC(sender, pos);
	}

	public void RequestDestination(GameAgent agent, Vector3 dest)
	{
		if (!this.IsAuthority())
		{
			Debug.LogError("RequestDestination should only be called from the master client");
			return;
		}
		int netIdFromEntityId = this.entityManager.GetNetIdFromEntityId(agent.entity.id);
		if (this.netIdsForDestination.Contains(netIdFromEntityId))
		{
			this.destinationsForDestination[this.netIdsForDestination.IndexOf(netIdFromEntityId)] = dest;
			return;
		}
		this.netIdsForDestination.Add(netIdFromEntityId);
		this.destinationsForDestination.Add(dest);
	}

	[PunRPC]
	public void ApplyDestinationRPC(int[] netEntityId, Vector3[] dest, PhotonMessageInfo info)
	{
		if (!this.IsZoneActive() || this.m_RpcSpamChecks.IsSpamming(GameAgentManager.RPC.ApplyDestination))
		{
			return;
		}
		if (netEntityId == null || dest == null || netEntityId.Length != dest.Length)
		{
			return;
		}
		int i = 0;
		while (i < netEntityId.Length)
		{
			if (this.IsValidClientRPC(info.Sender, netEntityId[i], dest[i]))
			{
				int num = i;
				float num2 = 10000f;
				if ((in dest[num]).IsValid(in num2))
				{
					i++;
					continue;
				}
			}
			return;
		}
		for (int j = 0; j < netEntityId.Length; j++)
		{
			GameEntity gameEntity = this.entityManager.GetGameEntity(this.entityManager.GetEntityIdFromNetId(netEntityId[j]));
			if (gameEntity == null)
			{
				return;
			}
			GameAgent component = gameEntity.GetComponent<GameAgent>();
			if (component == null)
			{
				return;
			}
			component.ApplyDestination(dest[j]);
		}
	}

	public void RequestState(GameAgent agent, byte state)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		int netIdFromEntityId = this.entityManager.GetNetIdFromEntityId(agent.entity.id);
		if (this.netIdsForState.Contains(netIdFromEntityId))
		{
			this.statesForState[this.netIdsForState.IndexOf(netIdFromEntityId)] = state;
			return;
		}
		this.netIdsForState.Add(netIdFromEntityId);
		this.statesForState.Add(state);
	}

	[PunRPC]
	public void ApplyStateRPC(int[] netEntityId, byte[] state, PhotonMessageInfo info)
	{
		if (netEntityId == null || state == null || netEntityId.Length != state.Length || this.m_RpcSpamChecks.IsSpamming(GameAgentManager.RPC.ApplyState))
		{
			return;
		}
		for (int i = 0; i < netEntityId.Length; i++)
		{
			if (!this.IsValidClientRPC(info.Sender, netEntityId[i]))
			{
				return;
			}
			GameEntity gameEntity = this.entityManager.GetGameEntity(this.entityManager.GetEntityIdFromNetId(netEntityId[i]));
			if (gameEntity == null)
			{
				return;
			}
			GameAgent component = gameEntity.GetComponent<GameAgent>();
			if (component == null)
			{
				return;
			}
			component.OnBodyStateChanged(state[i]);
		}
	}

	public void RequestBehavior(GameAgent agent, byte behavior)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		int netIdFromEntityId = this.entityManager.GetNetIdFromEntityId(agent.entity.id);
		if (this.netIdsForBehavior.Contains(netIdFromEntityId))
		{
			this.behaviorsForBehavior[this.netIdsForBehavior.IndexOf(netIdFromEntityId)] = behavior;
			return;
		}
		this.netIdsForBehavior.Add(netIdFromEntityId);
		this.behaviorsForBehavior.Add(behavior);
	}

	[PunRPC]
	public void ApplyBehaviorRPC(int[] netEntityId, byte[] behavior, PhotonMessageInfo info)
	{
		if (netEntityId == null || behavior == null || netEntityId.Length != behavior.Length || this.m_RpcSpamChecks.IsSpamming(GameAgentManager.RPC.ApplyBehaviour))
		{
			return;
		}
		for (int i = 0; i < netEntityId.Length; i++)
		{
			if (!this.IsValidClientRPC(info.Sender, netEntityId[i]))
			{
				return;
			}
			GameEntity gameEntity = this.entityManager.GetGameEntity(this.entityManager.GetEntityIdFromNetId(netEntityId[i]));
			if (gameEntity == null)
			{
				return;
			}
			GameAgent component = gameEntity.GetComponent<GameAgent>();
			if (component != null)
			{
				component.OnBehaviorStateChanged(behavior[i]);
			}
		}
	}

	public void RequestTarget(GameAgent agent, NetPlayer player)
	{
		if (player == agent.targetPlayer)
		{
			return;
		}
		if (!this.IsAuthority())
		{
			return;
		}
		if (agent == null)
		{
			return;
		}
		agent.targetPlayer = player;
		base.SendRPC("ApplyTargetRPC", RpcTarget.Others, new object[]
		{
			this.entityManager.GetNetIdFromEntityId(agent.entity.id),
			(player == null) ? null : player.GetPlayerRef()
		});
	}

	[PunRPC]
	public void ApplyTargetRPC(int agentNetId, Player player, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender, agentNetId) || this.m_RpcSpamChecks.IsSpamming(GameAgentManager.RPC.ApplyTarget) || player == null)
		{
			return;
		}
		GameEntity gameEntity = this.entityManager.GetGameEntity(this.entityManager.GetEntityIdFromNetId(agentNetId));
		if (gameEntity == null)
		{
			return;
		}
		GameAgent component = gameEntity.GetComponent<GameAgent>();
		if (component == null)
		{
			return;
		}
		component.targetPlayer = NetPlayer.Get(player);
	}

	public void RequestJump(GameAgent agent, Vector3 start, Vector3 end, float heightScale, float speedScale)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		if (agent == null)
		{
			return;
		}
		agent.OnJumpRequested(start, end, heightScale, speedScale);
		base.SendRPC("ApplyJumpRPC", RpcTarget.Others, new object[]
		{
			this.entityManager.GetNetIdFromEntityId(agent.entity.id),
			start,
			end,
			heightScale,
			speedScale
		});
	}

	[PunRPC]
	public void ApplyJumpRPC(int agentNetId, Vector3 start, Vector3 end, float heightScale, float speedScale, PhotonMessageInfo info)
	{
		if (this.IsValidClientRPC(info.Sender, agentNetId) && !this.m_RpcSpamChecks.IsSpamming(GameAgentManager.RPC.ApplyTarget))
		{
			float num = 10000f;
			if ((in start).IsValid(in num))
			{
				float num2 = 10000f;
				if ((in end).IsValid(in num2) && this.entityManager.IsPositionInZone(start) && this.entityManager.IsPositionInZone(end) && this.entityManager.IsEntityNearPosition(agentNetId, start, 16f) && heightScale <= 5f && speedScale <= 5f)
				{
					if ((end - start).sqrMagnitude > 625f)
					{
						return;
					}
					GameEntity gameEntity = this.entityManager.GetGameEntity(this.entityManager.GetEntityIdFromNetId(agentNetId));
					if (gameEntity == null)
					{
						return;
					}
					GameAgent component = gameEntity.GetComponent<GameAgent>();
					if (component == null)
					{
						return;
					}
					component.OnJumpRequested(start, end, heightScale, speedScale);
					return;
				}
			}
		}
	}

	public override void WriteDataFusion()
	{
	}

	public override void ReadDataFusion()
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		int num = Mathf.Min(4, this.agents.Count);
		stream.SendNext(num);
		for (int i = 0; i < num; i++)
		{
			if (this.nextAgentIndexUpdate >= this.agents.Count)
			{
				this.nextAgentIndexUpdate = 0;
			}
			stream.SendNext(this.entityManager.GetNetIdFromEntityId(this.agents[this.nextAgentIndexUpdate].entity.id));
			long num2 = BitPackUtils.PackWorldPosForNetwork(this.agents[this.nextAgentIndexUpdate].transform.position);
			stream.SendNext(num2);
			int num3 = BitPackUtils.PackQuaternionForNetwork(this.agents[this.nextAgentIndexUpdate].transform.rotation);
			stream.SendNext(num3);
			this.nextAgentIndexUpdate++;
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender))
		{
			return;
		}
		int num = (int)stream.ReceiveNext();
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)stream.ReceiveNext();
			Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork((long)stream.ReceiveNext());
			Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork((int)stream.ReceiveNext());
			if (this.IsPositionInZone(vector) && this.entityManager.IsValidNetId(num2))
			{
				GameEntityId entityIdFromNetId = this.entityManager.GetEntityIdFromNetId(num2);
				GameAgent gameAgent = this.GetGameAgent(entityIdFromNetId);
				if (gameAgent != null)
				{
					gameAgent.ApplyNetworkUpdate(vector, quaternion);
				}
			}
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

	public const float MAX_JUMP_DISTANCE = 25f;

	public GameEntityManager entityManager;

	public PhotonView photonView;

	private List<GameAgent> agents;

	private float lastDestinationSentTime;

	private float destinationCooldown;

	private List<int> netIdsForDestination;

	private List<Vector3> destinationsForDestination;

	private List<int> netIdsForState;

	private List<byte> statesForState;

	private float lastStateSentTime;

	private float stateCooldown;

	private List<int> netIdsForBehavior;

	private List<byte> behaviorsForBehavior;

	private float lastBehaviorSentTime;

	private float behaviorCooldown = 0.25f;

	private const int MAX_UPDATES_PER_FRAME = 4;

	private int nextAgentIndexUpdate;

	private const int MAX_THINK_PER_FRAME = 1;

	private int nextAgentIndexThink;

	public CallLimitersList<CallLimiter, GameAgentManager.RPC> m_RpcSpamChecks = new CallLimitersList<CallLimiter, GameAgentManager.RPC>();

	public enum RPC
	{
		ApplyDestination,
		ApplyState,
		ApplyBehaviour,
		ApplyImpact,
		ApplyTarget
	}
}
