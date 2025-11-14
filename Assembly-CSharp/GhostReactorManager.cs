using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fusion;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public class GhostReactorManager : NetworkComponent, IGameEntityZoneComponent
{
	protected override void Awake()
	{
		base.Awake();
		this.noiseEventManager = base.GetComponent<GRNoiseEventManager>();
	}

	internal override void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
	}

	internal override void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
	}

	public bool IsAuthority()
	{
		return this.gameEntityManager.IsAuthority();
	}

	private bool IsAuthorityPlayer(NetPlayer player)
	{
		return this.gameEntityManager.IsAuthorityPlayer(player);
	}

	private bool IsAuthorityPlayer(Player player)
	{
		return this.gameEntityManager.IsAuthorityPlayer(player);
	}

	private Player GetAuthorityPlayer()
	{
		return this.gameEntityManager.GetAuthorityPlayer();
	}

	public bool IsZoneActive()
	{
		return this.gameEntityManager.IsZoneActive();
	}

	public bool IsPositionInZone(Vector3 pos)
	{
		return this.gameEntityManager.IsPositionInZone(pos);
	}

	public bool IsValidClientRPC(Player sender)
	{
		return this.gameEntityManager.IsValidClientRPC(sender);
	}

	public bool IsValidClientRPC(Player sender, int entityNetId)
	{
		return this.gameEntityManager.IsValidClientRPC(sender, entityNetId);
	}

	public bool IsValidClientRPC(Player sender, int entityNetId, Vector3 pos)
	{
		return this.gameEntityManager.IsValidClientRPC(sender, entityNetId, pos);
	}

	public bool IsValidClientRPC(Player sender, Vector3 pos)
	{
		return this.gameEntityManager.IsValidClientRPC(sender, pos);
	}

	public bool IsValidAuthorityRPC(Player sender)
	{
		return this.gameEntityManager.IsValidAuthorityRPC(sender);
	}

	public bool IsValidAuthorityRPC(Player sender, int entityNetId)
	{
		return this.gameEntityManager.IsValidAuthorityRPC(sender, entityNetId);
	}

	public bool IsValidAuthorityRPC(Player sender, int entityNetId, Vector3 pos)
	{
		return this.gameEntityManager.IsValidAuthorityRPC(sender, entityNetId, pos);
	}

	public bool IsValidAuthorityRPC(Player sender, Vector3 pos)
	{
		return this.gameEntityManager.IsValidAuthorityRPC(sender, pos);
	}

	public static GhostReactorManager Get(GameEntity gameEntity)
	{
		if (gameEntity == null || gameEntity.manager == null)
		{
			return null;
		}
		return gameEntity.manager.ghostReactorManager;
	}

	public void RefreshShiftCredit()
	{
	}

	[PunRPC]
	public void RefreshShiftCreditRPC(PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		if (this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.RefreshShiftCredit))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull())
		{
			return;
		}
		if (grplayer.mothershipId.IsNullOrEmpty())
		{
			return;
		}
		ProgressionManager.Instance.GetShiftCredit(grplayer.mothershipId);
	}

	public void SendMothershipId()
	{
		string mothershipId = MothershipClientContext.MothershipId;
	}

	[PunRPC]
	public void SendMothershipIdRPC(string mothershipId, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		if (this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.SendMothershipId))
		{
			return;
		}
		if (mothershipId.Length > 40)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull())
		{
			return;
		}
		if (!grplayer.mothershipId.IsNullOrEmpty())
		{
			return;
		}
		if (grplayer.mothershipId.IsNullOrEmpty())
		{
			grplayer.mothershipId = mothershipId.Trim();
			ProgressionManager.Instance.GetShiftCredit(grplayer.mothershipId);
		}
	}

	public void RequestCollectItem(GameEntityId collectibleEntityId, GameEntityId collectorEntityId)
	{
		this.photonView.RPC("RequestCollectItemRPC", this.GetAuthorityPlayer(), new object[]
		{
			this.gameEntityManager.GetNetIdFromEntityId(collectibleEntityId),
			this.gameEntityManager.GetNetIdFromEntityId(collectorEntityId)
		});
	}

	public void RequestDepositCollectible(GameEntityId collectibleEntityId)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		GameEntity gameEntity = this.gameEntityManager.GetGameEntity(collectibleEntityId);
		if (gameEntity != null)
		{
			this.photonView.RPC("ApplyCollectItemRPC", RpcTarget.All, new object[]
			{
				this.gameEntityManager.GetNetIdFromEntityId(collectibleEntityId),
				-1,
				gameEntity.lastHeldByActorNumber
			});
		}
	}

	[PunRPC]
	public void RequestCollectItemRPC(int collectibleEntityNetId, int collectorEntityNetId, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender, collectibleEntityNetId))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull() || !grplayer.requestCollectItemLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		if (!this.gameEntityManager.IsValidNetId(collectorEntityNetId) || !this.gameEntityManager.IsEntityNearEntity(collectibleEntityNetId, collectorEntityNetId, 16f))
		{
			return;
		}
		if (true)
		{
			this.photonView.RPC("ApplyCollectItemRPC", RpcTarget.All, new object[]
			{
				collectibleEntityNetId,
				collectorEntityNetId,
				info.Sender.ActorNumber
			});
		}
	}

	[PunRPC]
	public void ApplyCollectItemRPC(int collectibleEntityNetId, int collectorEntityNetId, int collectingPlayerActorNumber, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender, collectibleEntityNetId) || this.reactor == null || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyCollectItem))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(collectingPlayerActorNumber);
		if (grplayer == null)
		{
			return;
		}
		if (true)
		{
			GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(collectibleEntityNetId);
			GameEntity gameEntity = this.gameEntityManager.GetGameEntity(entityIdFromNetId);
			if (gameEntity == null)
			{
				return;
			}
			GRCollectible component = gameEntity.GetComponent<GRCollectible>();
			if (component == null)
			{
				return;
			}
			GameEntityId entityIdFromNetId2 = this.gameEntityManager.GetEntityIdFromNetId(collectorEntityNetId);
			GameEntity gameEntity2 = this.gameEntityManager.GetGameEntity(entityIdFromNetId2);
			if (gameEntity2 != null)
			{
				GRToolCollector component2 = gameEntity2.GetComponent<GRToolCollector>();
				if (component2 != null && component2.tool != null)
				{
					component2.PerformCollection(component);
				}
			}
			else
			{
				ProgressionManager.Instance.DepositCore(component.type);
				this.ReportCoreCollection(grplayer, component.type);
				int count = this.reactor.vrRigs.Count;
				int num = component.energyValue / 4;
				for (int i = 0; i < count; i++)
				{
					GRPlayer.Get(this.reactor.vrRigs[i]).IncrementCoresCollectedGroup(num);
				}
				grplayer.IncrementCoresCollectedPlayer(num);
			}
			if (gameEntity != null && component != null)
			{
				component.InvokeOnCollected();
			}
			this.gameEntityManager.DestroyItemLocal(entityIdFromNetId);
		}
	}

	public void RequestApplySeedExtractorState(int coreCount, int coresProcessedByOverdrive, int researchPoints, float coreProcessingPercentage, float overdriveSupply)
	{
		this.photonView.RPC("RequestApplySeedExtractorStateRPC", this.GetAuthorityPlayer(), new object[] { coreCount, coresProcessedByOverdrive, researchPoints, coreProcessingPercentage, overdriveSupply });
	}

	[PunRPC]
	public void RequestApplySeedExtractorStateRPC(int coreCount, int coresProcessedByOverdrive, int researchPoints, float coreProcessingPercentage, float overdriveSupply, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.SeedExtractorAction) || coreCount < 0 || coresProcessedByOverdrive < 0 || researchPoints < 0 || !float.IsFinite(coreProcessingPercentage) || !float.IsFinite(overdriveSupply))
		{
			return;
		}
		if (info.Sender.ActorNumber != this.reactor.seedExtractor.CurrentPlayerActorNumber)
		{
			return;
		}
		this.photonView.RPC("ApplySeedExtractorStateRPC", RpcTarget.All, new object[]
		{
			info.Sender.ActorNumber,
			coreCount,
			coresProcessedByOverdrive,
			researchPoints,
			coreProcessingPercentage,
			overdriveSupply
		});
	}

	[PunRPC]
	public void ApplySeedExtractorStateRPC(int playerActorNumber, int coreCount, int coresProcessedByOverdrive, int researchPoints, float coreProcessingPercentage, float overdriveSupply, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.SeedExtractorAction) || coreCount < 0 || coresProcessedByOverdrive < 0 || researchPoints < 0 || !float.IsFinite(coreProcessingPercentage) || !float.IsFinite(overdriveSupply))
		{
			return;
		}
		if (this.reactor != null && this.reactor.seedExtractor != null)
		{
			this.reactor.seedExtractor.ApplyState(playerActorNumber, coreCount, coresProcessedByOverdrive, researchPoints, coreProcessingPercentage, overdriveSupply);
		}
	}

	public void RequestDistillCollectible(GameEntityId collectibleEntityId, Player sender)
	{
		if (!this.IsValidAuthorityRPC(sender))
		{
			return;
		}
		GameEntity gameEntity = this.gameEntityManager.GetGameEntity(collectibleEntityId);
		if (gameEntity != null)
		{
			this.photonView.RPC("DistillItemRPC", RpcTarget.All, new object[]
			{
				this.gameEntityManager.GetNetIdFromEntityId(collectibleEntityId),
				gameEntity.lastHeldByActorNumber
			});
		}
	}

	[PunRPC]
	public void DistillItemRPC(int collectibleEntityNetId, int collectingPlayerActorNumber, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender, collectibleEntityNetId) || this.reactor == null || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.DistillItem))
		{
			return;
		}
		if (GRPlayer.Get(collectingPlayerActorNumber) == null)
		{
			return;
		}
		if (true)
		{
			GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(collectibleEntityNetId);
			GameEntity gameEntity = this.gameEntityManager.GetGameEntity(entityIdFromNetId);
			if (gameEntity == null)
			{
				return;
			}
			GRCollectible component = gameEntity.GetComponent<GRCollectible>();
			if (component == null)
			{
				return;
			}
			Debug.LogWarning("Warning - NOT IMPLEMENTED - Return validating inserting core for distillery.");
			if (gameEntity != null && component != null)
			{
				component.InvokeOnCollected();
			}
			this.gameEntityManager.DestroyItemLocal(entityIdFromNetId);
		}
	}

	public void RequestChargeTool(GameEntityId collectorEntityId, GameEntityId targetToolId, int targetEnergyDelta = 0, bool useCollectorEnergy = true)
	{
		this.photonView.RPC("RequestChargeToolRPC", this.GetAuthorityPlayer(), new object[]
		{
			this.gameEntityManager.GetNetIdFromEntityId(collectorEntityId),
			this.gameEntityManager.GetNetIdFromEntityId(targetToolId),
			targetEnergyDelta,
			useCollectorEnergy
		});
	}

	[PunRPC]
	public void RequestChargeToolRPC(int collectorEntityNetId, int targetToolNetId, int targetEnergyDelta, bool useCollectorEnergy, PhotonMessageInfo info)
	{
		GamePlayer gamePlayer;
		if (!this.IsValidAuthorityRPC(info.Sender) || !this.gameEntityManager.IsValidNetId(collectorEntityNetId) || !this.gameEntityManager.IsValidNetId(targetToolNetId) || !this.gameEntityManager.IsEntityNearEntity(collectorEntityNetId, targetToolNetId, 16f) || !GamePlayer.TryGetGamePlayer(info.Sender.ActorNumber, out gamePlayer) || !this.gameEntityManager.IsPlayerHandNearEntity(gamePlayer, collectorEntityNetId, false, true, 16f) || !this.gameEntityManager.IsPlayerHandNearEntity(gamePlayer, targetToolNetId, false, true, 16f))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull() || !grplayer.requestChargeToolLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		if (true)
		{
			this.photonView.RPC("ApplyChargeToolRPC", RpcTarget.All, new object[] { collectorEntityNetId, targetToolNetId, targetEnergyDelta, useCollectorEnergy, info.Sender });
		}
	}

	[PunRPC]
	public void ApplyChargeToolRPC(int collectorEntityNetId, int targetToolNetId, int targetEnergyDelta, bool useCollectorEnergy, Player collectingPlayer, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyChargeTool) || !this.gameEntityManager.IsValidNetId(collectorEntityNetId) || !this.gameEntityManager.IsValidNetId(targetToolNetId))
		{
			return;
		}
		if (true)
		{
			GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(collectorEntityNetId);
			GameEntity gameEntity = this.gameEntityManager.GetGameEntity(entityIdFromNetId);
			GameEntityId entityIdFromNetId2 = this.gameEntityManager.GetEntityIdFromNetId(targetToolNetId);
			GameEntity gameEntity2 = this.gameEntityManager.GetGameEntity(entityIdFromNetId2);
			if (gameEntity != null && gameEntity2 != null)
			{
				GRToolCollector component = gameEntity.GetComponent<GRToolCollector>();
				GRTool component2 = gameEntity2.GetComponent<GRTool>();
				if (component != null && component.tool != null && component2 != null)
				{
					int num = ((targetEnergyDelta != 0) ? targetEnergyDelta : 100);
					int num2 = Mathf.Max(component2.GetEnergyMax() - component2.energy, 0);
					int num3;
					if (!useCollectorEnergy)
					{
						num3 = Mathf.Min(num, num2);
						Debug.Log(string.Format("Apply SelfCharge {0}", num3));
					}
					else
					{
						num3 = Mathf.Min(Mathf.Min(component.tool.energy, num), num2);
					}
					if (num3 > 0)
					{
						if (useCollectorEnergy)
						{
							component.tool.SetEnergy(component.tool.energy - num3);
						}
						component2.RefillEnergy(num3, entityIdFromNetId);
						component.PlayChargeEffect(component2);
					}
				}
			}
		}
	}

	public void RequestDepositCurrency(GameEntityId collectorEntityId)
	{
		this.photonView.RPC("RequestDepositCurrencyRPC", this.GetAuthorityPlayer(), new object[] { this.gameEntityManager.GetNetIdFromEntityId(collectorEntityId) });
	}

	[PunRPC]
	public void RequestDepositCurrencyRPC(int collectorEntityNetId, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender, collectorEntityNetId))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull() || !grplayer.requestDepositCurrencyLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(collectorEntityNetId);
		this.gameEntityManager.GetGameEntity(entityIdFromNetId);
		GamePlayer gamePlayer;
		if (GamePlayer.TryGetGamePlayer(info.Sender.ActorNumber, out gamePlayer) && this.gameEntityManager.IsPlayerHandNearEntity(gamePlayer, collectorEntityNetId, false, true, 16f) && (grplayer.transform.position - this.reactor.currencyDepositor.transform.position).magnitude < 16f)
		{
			this.photonView.RPC("ApplyDepositCurrencyRPC", RpcTarget.All, new object[]
			{
				collectorEntityNetId,
				info.Sender.ActorNumber
			});
		}
	}

	[PunRPC]
	public void ApplyDepositCurrencyRPC(int collectorEntityNetId, int targetPlayerActorNumber, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender, collectorEntityNetId) || this.reactor == null || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyDepositCurrency))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(targetPlayerActorNumber);
		if (grplayer == null)
		{
			return;
		}
		if (true)
		{
			GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(collectorEntityNetId);
			GameEntity gameEntity = this.gameEntityManager.GetGameEntity(entityIdFromNetId);
			if (gameEntity != null)
			{
				GRToolCollector component = gameEntity.GetComponent<GRToolCollector>();
				if (component != null && component.tool != null)
				{
					int energy = component.tool.energy;
					int energyDepositPerUse = component.energyDepositPerUse;
					if (energy >= energyDepositPerUse)
					{
						this.ReportCoreCollection(grplayer, ProgressionManager.CoreType.Core);
						int count = this.reactor.vrRigs.Count;
						int num = energyDepositPerUse / 4;
						for (int i = 0; i < count; i++)
						{
							GRPlayer.Get(this.reactor.vrRigs[i]).IncrementCoresCollectedGroup(num);
						}
						grplayer.IncrementCoresCollectedPlayer(num);
						int num2 = energy - energyDepositPerUse;
						component.tool.SetEnergy(num2);
						this.reactor.RefreshScoreboards();
						ProgressionManager.Instance.DepositCore(ProgressionManager.CoreType.Core);
						component.PlayChargeEffect(this.reactor.currencyDepositor);
					}
				}
			}
		}
	}

	public void RequestEnemyHitPlayer(GhostReactor.EnemyType type, GameEntityId hitByEntityId, GRPlayer player, Vector3 hitPosition)
	{
		this.photonView.RPC("ApplyEnemyHitPlayerRPC", RpcTarget.All, new object[]
		{
			type,
			this.gameEntityManager.GetNetIdFromEntityId(hitByEntityId),
			hitPosition
		});
	}

	[PunRPC]
	private void ApplyEnemyHitPlayerRPC(GhostReactor.EnemyType type, int entityNetId, Vector3 hitPosition, PhotonMessageInfo info)
	{
		if (!this.gameEntityManager.IsValidNetId(entityNetId))
		{
			return;
		}
		GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(entityNetId);
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer == null || !grplayer.applyEnemyHitLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.OnEnemyHitPlayerInternal(type, entityIdFromNetId, grplayer, hitPosition);
	}

	private void OnEnemyHitPlayerInternal(GhostReactor.EnemyType type, GameEntityId entityId, GRPlayer player, Vector3 hitPosition)
	{
		if (type == GhostReactor.EnemyType.Chaser || type == GhostReactor.EnemyType.Phantom || type == GhostReactor.EnemyType.Ranged || type == GhostReactor.EnemyType.CustomMapsEnemy)
		{
			player.OnPlayerHit(hitPosition, this, entityId);
			GameHitter component = this.gameEntityManager.GetGameEntity(entityId).GetComponent<GameHitter>();
			if (component != null)
			{
				component.ApplyHitToPlayer(player, hitPosition);
			}
		}
	}

	public void ReportLocalPlayerHit()
	{
		base.GetView.RPC("ReportLocalPlayerHitRPC", RpcTarget.All, Array.Empty<object>());
	}

	[PunRPC]
	private void ReportLocalPlayerHitRPC(PhotonMessageInfo info)
	{
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer == null || !grplayer.reportLocalHitLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		grplayer.ChangePlayerState(GRPlayer.GRPlayerState.Ghost, this);
	}

	public void RequestPlayerRevive(GRReviveStation reviveStation, GRPlayer player)
	{
		if ((NetworkSystem.Instance.InRoom && this.IsAuthority()) || !NetworkSystem.Instance.InRoom)
		{
			base.GetView.RPC("ApplyPlayerRevivedRPC", RpcTarget.All, new object[]
			{
				reviveStation.Index,
				player.gamePlayer.rig.OwningNetPlayer.ActorNumber
			});
		}
	}

	[PunRPC]
	private void ApplyPlayerRevivedRPC(int reviveStationIndex, int playerActorNumber, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyPlayerRevived))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(playerActorNumber);
		if (grplayer == null)
		{
			return;
		}
		if (reviveStationIndex < 0 || reviveStationIndex >= this.reactor.reviveStations.Count)
		{
			return;
		}
		GRReviveStation grreviveStation = this.reactor.reviveStations[reviveStationIndex];
		if (grreviveStation == null)
		{
			return;
		}
		grreviveStation.RevivePlayer(grplayer);
	}

	public void RequestPlayerStateChange(GRPlayer player, GRPlayer.GRPlayerState newState)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			base.GetView.RPC("PlayerStateChangeRPC", RpcTarget.All, new object[]
			{
				PhotonNetwork.LocalPlayer.ActorNumber,
				player.gamePlayer.rig.OwningNetPlayer.ActorNumber,
				(int)newState
			});
			return;
		}
		player.ChangePlayerState(newState, this);
	}

	[PunRPC]
	private void PlayerStateChangeRPC(int playerResponsibleNumber, int playerActorNumber, int newState, PhotonMessageInfo info)
	{
		bool flag = this.IsValidClientRPC(info.Sender);
		bool flag2 = newState == 1 && info.Sender.ActorNumber == playerActorNumber;
		bool flag3 = newState == 0 && flag;
		if (!flag2 && !flag3)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(playerActorNumber);
		GRPlayer grplayer2 = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer == null || grplayer2.IsNull() || !grplayer2.playerStateChangeLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		if (newState == 0 && playerResponsibleNumber != playerActorNumber)
		{
			GRPlayer grplayer3 = GRPlayer.Get(playerResponsibleNumber);
			if (grplayer3 != null && grplayer3 != grplayer)
			{
				grplayer3.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.Assists, 1f);
			}
		}
		grplayer.ChangePlayerState((GRPlayer.GRPlayerState)newState, this);
	}

	public void RequestGrantPlayerShield(GRPlayer player, int shieldHp, int shieldFlags)
	{
		base.GetView.RPC("RequestGrantPlayerShieldRPC", this.GetAuthorityPlayer(), new object[]
		{
			PhotonNetwork.LocalPlayer.ActorNumber,
			player.gamePlayer.rig.OwningNetPlayer.ActorNumber,
			shieldHp,
			shieldFlags
		});
	}

	[PunRPC]
	private void RequestGrantPlayerShieldRPC(int shieldingPlayer, int playerToGrantShieldActorNumber, int shieldHp, int shieldFlags, PhotonMessageInfo info)
	{
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		GRPlayer grplayer2 = GRPlayer.Get(playerToGrantShieldActorNumber);
		if (!this.IsValidAuthorityRPC(info.Sender) || grplayer.IsNull() || !grplayer.fireShieldLimiter.CheckCallTime(Time.unscaledTime) || grplayer2.IsNull() || !grplayer2.CanActivateShield(shieldHp))
		{
			return;
		}
		base.GetView.RPC("ApplyGrantPlayerShieldRPC", RpcTarget.All, new object[] { shieldingPlayer, playerToGrantShieldActorNumber, shieldHp, shieldFlags });
	}

	[PunRPC]
	private void ApplyGrantPlayerShieldRPC(int shieldingPlayer, int playerToGrantShieldActorNumber, int shieldHp, int shieldFlags, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.GrantPlayerShield))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(playerToGrantShieldActorNumber);
		if (grplayer == null)
		{
			return;
		}
		if (grplayer.TryActivateShield(shieldHp, shieldFlags))
		{
			GRPlayer grplayer2 = GRPlayer.Get(shieldingPlayer);
			if (grplayer2 != null)
			{
				grplayer2.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.Assists, 1f);
			}
		}
	}

	public void RequestFireProjectile(GameEntityId entityId, Vector3 firingPosition, Vector3 targetPosition, double networkTime)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		if ((NetworkSystem.Instance.InRoom && base.IsMine) || !NetworkSystem.Instance.InRoom)
		{
			base.GetView.RPC("RequestFireProjectileRPC", RpcTarget.All, new object[]
			{
				this.gameEntityManager.GetNetIdFromEntityId(entityId),
				firingPosition,
				targetPosition,
				networkTime
			});
		}
	}

	[PunRPC]
	private void RequestFireProjectileRPC(int entityNetId, Vector3 firingPosition, Vector3 targetPosition, double networkTime, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender, entityNetId, targetPosition) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.RequestFireProjectile) || !this.gameEntityManager.IsEntityNearPosition(entityNetId, firingPosition, 16f))
		{
			return;
		}
		GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(entityNetId);
		this.OnRequestFireProjectileInternal(entityIdFromNetId, firingPosition, targetPosition, networkTime);
	}

	private void OnRequestFireProjectileInternal(GameEntityId entityId, Vector3 firingPosition, Vector3 targetPosition, double networkTime)
	{
		GREnemyRanged gameComponent = this.gameEntityManager.GetGameComponent<GREnemyRanged>(entityId);
		if (gameComponent != null)
		{
			gameComponent.RequestRangedAttack(firingPosition, targetPosition, networkTime);
		}
		GRHazardTower gameComponent2 = this.gameEntityManager.GetGameComponent<GRHazardTower>(entityId);
		if (gameComponent2 != null)
		{
			gameComponent2.OnFire(firingPosition, targetPosition, networkTime);
		}
	}

	[PunRPC]
	public void BroadcastHandprint(Vector3 pos, Quaternion orient, PhotonMessageInfo info)
	{
		if (this.reactor == null)
		{
			return;
		}
		float num = 10000f;
		if (!(in pos).IsValid(in num) || !(in orient).IsValid())
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender);
		if (grplayer == null)
		{
			return;
		}
		if (!GameEntityManager.IsPlayerHandNearPosition(grplayer.gamePlayer, pos, false, true, 3f))
		{
			return;
		}
		if (info.Sender.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber && Time.time - this.LastHandprintTime <= 0.25f)
		{
			return;
		}
		this.LastHandprintTime = Time.time;
		this.reactor.AddHandprint(pos, orient);
	}

	public void OnAbilityDie(GameEntity entity)
	{
		if (this.reactor == null)
		{
			return;
		}
		this.reactor.OnAbilityDie(entity);
	}

	public void RequestShiftStartAuthority(bool isFirstShift)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		if (!shiftManager.ShiftActive)
		{
			double time = PhotonNetwork.Time;
			SRand srand = new SRand(Mathf.FloorToInt(Time.time * 100f));
			int num = srand.NextInt(0, int.MaxValue);
			string text = Guid.NewGuid().ToString();
			this.photonView.RPC("ApplyShiftStartRPC", RpcTarget.All, new object[] { time, num, text, isFirstShift });
			shiftManager.RequestState(GhostReactorShiftManager.State.ShiftActive);
			ProgressionManager.Instance.StartOfShift(text, shiftManager.shiftRewardCoresForMothership, this.reactor.vrRigs.Count, this.reactor.GetDepthLevel());
		}
	}

	[PunRPC]
	public void ApplyShiftStartRPC(double shiftStartTime, int randomSeed, string gameIdGuid, bool isFirstShift, PhotonMessageInfo info)
	{
		if (double.IsNaN(shiftStartTime) || !this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyShiftStart))
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		int num = Math.Clamp(this.reactor.NumActivePlayers, 0, this.reactor.difficultyScalingPerPlayer.Count - 1);
		this.reactor.difficultyScalingForCurrentFloor = 1f;
		if (this.reactor.difficultyScalingPerPlayer.Count > 0)
		{
			this.reactor.difficultyScalingForCurrentFloor = this.reactor.difficultyScalingPerPlayer[num];
		}
		double num2 = PhotonNetwork.Time - shiftStartTime;
		if (num2 < 0.0 || num2 > 10.0)
		{
			return;
		}
		levelGenerator.Generate(randomSeed);
		if (this.gameEntityManager.IsAuthority())
		{
			if (this.activeSpawnSectionEntitiesCoroutine != null)
			{
				base.StopCoroutine(this.activeSpawnSectionEntitiesCoroutine);
			}
			this.activeSpawnSectionEntitiesCoroutine = base.StartCoroutine(this.SpawnSectionEntitiesCoroutine(this.reactor.difficultyScalingForCurrentFloor));
		}
		shiftManager.shiftStats.ResetShiftStats();
		shiftManager.ResetJudgment();
		shiftManager.RefreshShiftStatsDisplay();
		shiftManager.OnShiftStarted(gameIdGuid, shiftStartTime, true, isFirstShift);
		this.reactor.ClearAllHandprints();
		this.reactor.ClearAllRespawns();
	}

	private IEnumerator SpawnSectionEntitiesCoroutine(float respawnCount)
	{
		int initialFrameCount = Time.frameCount;
		while (initialFrameCount == Time.frameCount)
		{
			yield return this.spawnSectionEntitiesWait;
		}
		if (this.gameEntityManager.IsAuthority())
		{
			this.reactor.levelGenerator.SpawnEntitiesInEachSection(respawnCount);
		}
		yield break;
	}

	public void RequestShiftEnd()
	{
		if (!this.IsAuthority())
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		if (!shiftManager.ShiftActive)
		{
			return;
		}
		GhostReactorManager.tempEntitiesToDestroy.Clear();
		List<GameEntity> gameEntities = this.gameEntityManager.GetGameEntities();
		for (int i = 0; i < gameEntities.Count; i++)
		{
			GameEntity gameEntity = gameEntities[i];
			if (gameEntity != null && !this.ShouldEntitySurviveShift(gameEntity))
			{
				GhostReactorManager.tempEntitiesToDestroy.Add(gameEntity.id);
			}
		}
		this.gameEntityManager.RequestDestroyItems(GhostReactorManager.tempEntitiesToDestroy);
		this.photonView.RPC("ApplyShiftEndRPC", RpcTarget.Others, new object[] { PhotonNetwork.Time });
		levelGenerator.ClearLevelSections();
		shiftManager.OnShiftEnded(PhotonNetwork.Time, true, ZoneClearReason.JoinZone);
		shiftManager.CalculateShiftTotal();
		shiftManager.RevealJudgment(Mathf.FloorToInt((float)shiftManager.shiftStats.GetShiftStat(GRShiftStatType.EnemyDeaths) / 5f));
		shiftManager.RequestState(GhostReactorShiftManager.State.PostShift);
	}

	[PunRPC]
	public void ApplyShiftEndRPC(double networkedTime, PhotonMessageInfo info)
	{
		if (!double.IsFinite(networkedTime) || !this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyShiftEnd))
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		if (!shiftManager.ShiftActive)
		{
			return;
		}
		this.reactor.ClearAllRespawns();
		levelGenerator.ClearLevelSections();
		shiftManager.OnShiftEnded(networkedTime, true, ZoneClearReason.JoinZone);
		shiftManager.CalculateShiftTotal();
		shiftManager.RevealJudgment(Mathf.FloorToInt((float)shiftManager.shiftStats.GetShiftStat(GRShiftStatType.EnemyDeaths) / 5f));
	}

	private bool ShouldEntitySurviveShift(GameEntity gameEntity)
	{
		if (gameEntity == null)
		{
			return true;
		}
		if (this.reactor == null)
		{
			return false;
		}
		if (gameEntity.GetComponent<GREnemyChaser>() != null || gameEntity.GetComponent<GREnemyRanged>() != null || gameEntity.GetComponent<GREnemyPhantom>() != null || gameEntity.GetComponent<GREnemyPest>() != null)
		{
			return false;
		}
		if (gameEntity.GetComponent<GRBreakable>() != null || gameEntity.GetComponent<GRCollectibleDispenser>() != null || gameEntity.GetComponent<GRMetalEnergyGate>() != null || gameEntity.GetComponent<GRBarrierSpectral>() != null || gameEntity.GetComponent<GRSconce>() != null)
		{
			return false;
		}
		Collider safeZoneLimit = this.reactor.safeZoneLimit;
		Vector3 position = gameEntity.gameObject.transform.position;
		return safeZoneLimit.bounds.Contains(position) || gameEntity.GetComponent<GRBadge>() != null;
	}

	public void ReportEnemyDeath()
	{
		if (this.reactor == null)
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		shiftManager.shiftStats.IncrementShiftStat(GRShiftStatType.EnemyDeaths);
		shiftManager.RefreshShiftStatsDisplay();
		PlayerGameEvents.MiscEvent("GRKillEnemy", 1);
	}

	public void ReportCoreCollection(GRPlayer player, ProgressionManager.CoreType type)
	{
		Debug.Log("GhostReactorManager ReportCoreCollection");
		if (player == null)
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		if (type == ProgressionManager.CoreType.ChaosSeed)
		{
			shiftManager.shiftStats.IncrementShiftStat(GRShiftStatType.SentientCoresCollected);
		}
		else if (type == ProgressionManager.CoreType.SuperCore)
		{
			shiftManager.shiftStats.IncrementShiftStat(GRShiftStatType.CoresCollected);
			shiftManager.shiftStats.IncrementShiftStat(GRShiftStatType.CoresCollected);
			shiftManager.shiftStats.IncrementShiftStat(GRShiftStatType.CoresCollected);
			player.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.CoresDeposited, 3f);
			int count = this.reactor.vrRigs.Count;
			for (int i = 0; i < count; i++)
			{
				GRPlayer grplayer = GRPlayer.Get(this.reactor.vrRigs[i]);
				if (grplayer != null)
				{
					grplayer.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.EarnedCredits, 15f);
				}
			}
		}
		else
		{
			shiftManager.shiftStats.IncrementShiftStat(GRShiftStatType.CoresCollected);
			player.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.CoresDeposited, 1f);
			int count2 = this.reactor.vrRigs.Count;
			for (int j = 0; j < count2; j++)
			{
				GRPlayer grplayer2 = GRPlayer.Get(this.reactor.vrRigs[j]);
				if (grplayer2 != null)
				{
					grplayer2.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.EarnedCredits, 5f);
				}
			}
		}
		shiftManager.RefreshShiftStatsDisplay();
		PlayerGameEvents.MiscEvent("GRCollectCore", 1);
	}

	public void ReportPlayerDeath(GRPlayer player)
	{
		if (this.reactor == null || player == null || this.reactor.zone == GTZone.customMaps)
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		shiftManager.shiftStats.IncrementShiftStat(GRShiftStatType.PlayerDeaths);
		shiftManager.RefreshShiftStatsDisplay();
		player.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.Deaths, 1f);
	}

	public void PromotionBotActivePlayerRequest(int state)
	{
		this.photonView.RPC("PromotionBotActivePlayerRequestRPC", this.GetAuthorityPlayer(), new object[] { state });
	}

	[PunRPC]
	public void PromotionBotActivePlayerRequestRPC(int state, PhotonMessageInfo info)
	{
		if (this.reactor == null)
		{
			return;
		}
		if (!this.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull() || !grplayer.promotionBotLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		GRUIPromotionBot promotionBot = this.reactor.promotionBot;
		if (promotionBot == null)
		{
			return;
		}
		if (state == 6)
		{
			if (promotionBot.currentPlayerActorNumber != -1)
			{
				return;
			}
			state = 1;
		}
		int actorNumber = info.Sender.ActorNumber;
		this.photonView.RPC("PromotionBotActivePlayerResponseRPC", RpcTarget.Others, new object[] { actorNumber, state });
		promotionBot.SetActivePlayerStateChange(actorNumber, state);
	}

	[PunRPC]
	public void PromotionBotActivePlayerResponseRPC(int actorNumber, int state, PhotonMessageInfo info)
	{
		if (this.reactor == null)
		{
			return;
		}
		GRUIPromotionBot promotionBot = this.reactor.promotionBot;
		if (GRPlayer.Get(info.Sender.ActorNumber) == null || promotionBot == null || !this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.PromotionBotResponse))
		{
			return;
		}
		promotionBot.SetActivePlayerStateChange(actorNumber, state);
	}

	[PunRPC]
	public void BroadcastScoreboardPage(int scoreboardPage, PhotonMessageInfo info)
	{
		if (this.reactor == null)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer == null || !grplayer.scoreboardPageLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		if (GRUIScoreboard.ValidPage((GRUIScoreboard.ScoreboardScreen)scoreboardPage))
		{
			GhostReactor.instance.UpdateScoreboardScreen((GRUIScoreboard.ScoreboardScreen)scoreboardPage);
		}
	}

	[PunRPC]
	public void BroadcastStartingProgression(int points, int redeemedPoints, double shiftJoinedTime, PhotonMessageInfo info)
	{
		if (double.IsNaN(shiftJoinedTime) || double.IsInfinity(shiftJoinedTime))
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer == null || !grplayer.progressionBroadcastLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		grplayer.SetProgressionData(points, redeemedPoints, false);
		grplayer.shiftJoinTime = Math.Clamp(shiftJoinedTime, PhotonNetwork.Time - 10.0, PhotonNetwork.Time);
	}

	public void RequestPlayerAction(GhostReactorManager.GRPlayerAction playerAction)
	{
		this.photonView.RPC("RequestPlayerActionRPC", this.GetAuthorityPlayer(), new object[]
		{
			(int)playerAction,
			0,
			0
		});
	}

	public void RequestPlayerAction(GhostReactorManager.GRPlayerAction playerAction, int param0)
	{
		this.photonView.RPC("RequestPlayerActionRPC", this.GetAuthorityPlayer(), new object[]
		{
			(int)playerAction,
			param0,
			0
		});
	}

	public void RequestPlayerAction(GhostReactorManager.GRPlayerAction playerAction, int param0, int param1)
	{
		this.photonView.RPC("RequestPlayerActionRPC", this.GetAuthorityPlayer(), new object[]
		{
			(int)playerAction,
			param0,
			param1
		});
	}

	public bool VerifyShuttleInteractability(GRPlayer player, int shuttleIdx, bool ignoreOwnership = false)
	{
		if (GRElevatorManager._instance == null)
		{
			return false;
		}
		GRShuttle shuttleById = GRElevatorManager._instance.GetShuttleById(shuttleIdx);
		return !(shuttleById == null) && shuttleById.IsShuttleInteractableByPlayer(player, ignoreOwnership);
	}

	[PunRPC]
	public void RequestPlayerActionRPC(int playerAction, int param0, int param1, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull() || !grplayer.requestShiftStartLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		bool flag = false;
		switch (playerAction)
		{
		case 1:
			flag = !shiftManager.ShiftActive && shiftManager.authorizedToDelveDeeper;
			if (flag)
			{
				int num = this.reactor.GetDepthLevel() + 1;
				this.reactor.depthConfigIndex = this.reactor.PickLevelConfigForDepth(num);
				param0 = num;
				param1 = this.reactor.depthConfigIndex;
			}
			break;
		case 2:
			flag = true;
			break;
		case 3:
			flag = this.VerifyShuttleInteractability(grplayer, param0, true);
			param1 = info.Sender.ActorNumber;
			break;
		case 4:
			flag = this.VerifyShuttleInteractability(grplayer, param0, false);
			param1 = info.Sender.ActorNumber;
			break;
		case 5:
			flag = this.VerifyShuttleInteractability(grplayer, param0, false);
			param1 = info.Sender.ActorNumber;
			break;
		case 6:
			flag = this.VerifyShuttleInteractability(grplayer, param0, false);
			param1 = info.Sender.ActorNumber;
			break;
		case 7:
			flag = this.VerifyShuttleInteractability(grplayer, param0, false);
			param1 = info.Sender.ActorNumber;
			break;
		case 8:
			flag = this.VerifyShuttleInteractability(grplayer, param0, false);
			param1 = info.Sender.ActorNumber;
			break;
		case 9:
			flag = true;
			param0 = Mathf.Clamp(param0, 0, 1);
			param1 = info.Sender.ActorNumber;
			break;
		case 10:
			flag = true;
			param0 = Mathf.Clamp(param0, 0, 3);
			param1 = info.Sender.ActorNumber;
			break;
		case 11:
			flag = param0 == info.Sender.ActorNumber || this.IsAuthorityPlayer(info.Sender);
			if (this.reactor.seedExtractor.StationOpen && this.reactor.seedExtractor.CurrentPlayerActorNumber != info.Sender.ActorNumber)
			{
				playerAction = 13;
			}
			break;
		case 12:
			flag = this.IsAuthorityPlayer(info.Sender);
			break;
		case 13:
			flag = this.IsAuthorityPlayer(info.Sender);
			break;
		case 14:
		{
			GameEntity gameEntityFromNetId = this.gameEntityManager.GetGameEntityFromNetId(param1);
			if (this.IsAuthorityPlayer(info.Sender) && gameEntityFromNetId != null && gameEntityFromNetId.lastHeldByActorNumber == param0)
			{
				flag = true;
			}
			break;
		}
		case 15:
		{
			int num2 = param1;
			GameEntity gameEntityFromNetId2 = this.gameEntityManager.GetGameEntityFromNetId(num2);
			if (gameEntityFromNetId2 != null && this.reactor.seedExtractor.ValidateSeedDepositSucceeded(param0, param1))
			{
				this.gameEntityManager.RequestDestroyItem(gameEntityFromNetId2.id);
				flag = true;
			}
			break;
		}
		case 16:
			flag = info.Sender.ActorNumber == param0;
			break;
		}
		if (flag)
		{
			this.photonView.RPC("ApplyPlayerActionRPC", RpcTarget.All, new object[] { playerAction, param0, param1 });
		}
	}

	[PunRPC]
	public void ApplyPlayerActionRPC(int playerAction, int param0, int param1, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyShiftStart))
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		this.gameEntityManager.IsAuthorityPlayer(info.Sender);
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull() || !grplayer.requestShiftStartLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		switch (playerAction)
		{
		case 1:
			this.reactor.SetNextDelveDepth(param0, param1);
			return;
		case 2:
			this.reactor.shiftManager.SetState((GhostReactorShiftManager.State)param0, false);
			return;
		case 3:
		{
			GRPlayer grplayer2 = GRPlayer.Get(param1);
			if (grplayer2 == null)
			{
				return;
			}
			if (!this.VerifyShuttleInteractability(grplayer2, param0, true))
			{
				return;
			}
			GRShuttle shuttleById = GRElevatorManager._instance.GetShuttleById(param0);
			if (shuttleById != null)
			{
				shuttleById.OnOpenDoor();
				return;
			}
			break;
		}
		case 4:
		{
			GRPlayer grplayer3 = GRPlayer.Get(param1);
			if (grplayer3 == null)
			{
				return;
			}
			if (!this.VerifyShuttleInteractability(grplayer3, param0, false))
			{
				return;
			}
			GRShuttle shuttleById2 = GRElevatorManager._instance.GetShuttleById(param0);
			if (shuttleById2 != null)
			{
				shuttleById2.OnCloseDoor();
				return;
			}
			break;
		}
		case 5:
		{
			GRPlayer grplayer4 = GRPlayer.Get(param1);
			if (grplayer4 == null)
			{
				return;
			}
			if (!this.VerifyShuttleInteractability(grplayer4, param0, false))
			{
				return;
			}
			GRShuttle shuttleById3 = GRElevatorManager._instance.GetShuttleById(param0);
			if (shuttleById3 != null)
			{
				shuttleById3.OnLaunch();
				return;
			}
			break;
		}
		case 6:
		{
			GRPlayer grplayer5 = GRPlayer.Get(param1);
			if (grplayer5 == null)
			{
				return;
			}
			if (!this.VerifyShuttleInteractability(grplayer5, param0, false))
			{
				return;
			}
			GRShuttle shuttleById4 = GRElevatorManager._instance.GetShuttleById(param0);
			if (shuttleById4 != null)
			{
				shuttleById4.OnArrive();
				return;
			}
			break;
		}
		case 7:
		{
			GRPlayer grplayer6 = GRPlayer.Get(param1);
			if (grplayer6 == null)
			{
				return;
			}
			if (!this.VerifyShuttleInteractability(grplayer6, param0, false))
			{
				return;
			}
			GRShuttle shuttleById5 = GRElevatorManager._instance.GetShuttleById(param0);
			if (shuttleById5 != null)
			{
				shuttleById5.OnTargetLevelUp();
				return;
			}
			break;
		}
		case 8:
		{
			GRPlayer grplayer7 = GRPlayer.Get(param1);
			if (grplayer7 == null)
			{
				return;
			}
			if (!this.VerifyShuttleInteractability(grplayer7, param0, false))
			{
				return;
			}
			GRShuttle shuttleById6 = GRElevatorManager._instance.GetShuttleById(param0);
			if (shuttleById6 != null)
			{
				shuttleById6.OnTargetLevelDown();
				return;
			}
			break;
		}
		case 9:
		{
			GRPlayer grplayer8 = GRPlayer.Get(param1);
			if (grplayer8 != null)
			{
				param0 = Mathf.Clamp(param0, 0, 1);
				grplayer8.dropPodLevel = param0;
				this.reactor.RefreshBays();
				grplayer8.RefreshShuttles();
				return;
			}
			break;
		}
		case 10:
		{
			GRPlayer grplayer9 = GRPlayer.Get(param1);
			if (grplayer9 != null)
			{
				param0 = Mathf.Clamp(param0, 0, 3);
				grplayer9.dropPodChasisLevel = param0;
				this.reactor.RefreshBays();
				grplayer9.RefreshShuttles();
				return;
			}
			break;
		}
		case 11:
			this.reactor.seedExtractor.CardSwipeSuccess();
			this.reactor.seedExtractor.OpenStation(param0);
			return;
		case 12:
			this.reactor.seedExtractor.CloseStation();
			return;
		case 13:
			this.reactor.seedExtractor.CardSwipeFail();
			return;
		case 14:
			this.reactor.seedExtractor.TryDepositSeed(param0, param1);
			return;
		case 15:
			this.reactor.seedExtractor.SeedDepositSucceeded(param0, param1);
			return;
		case 16:
			this.reactor.seedExtractor.SeedDepositFailed(param0, param1);
			break;
		default:
			return;
		}
	}

	public GRToolUpgradePurchaseStationFull GetToolUpgradeStationFullForIndex(int idx)
	{
		if (this.reactor == null || this.reactor.toolUpgradePurchaseStationsFull == null || idx < 0 || idx >= this.reactor.toolUpgradePurchaseStationsFull.Count)
		{
			return null;
		}
		return this.reactor.toolUpgradePurchaseStationsFull[idx];
	}

	public int GetIndexForToolUpgradeStationFull(GRToolUpgradePurchaseStationFull station)
	{
		if (this.reactor == null || this.reactor.toolUpgradePurchaseStationsFull == null)
		{
			return -1;
		}
		return this.reactor.toolUpgradePurchaseStationsFull.IndexOf(station);
	}

	public void RequestNetworkShelfAndItemChange(GRToolUpgradePurchaseStationFull station, int shelf, int item)
	{
		int indexForToolUpgradeStationFull = this.GetIndexForToolUpgradeStationFull(station);
		if (indexForToolUpgradeStationFull == -1)
		{
			return;
		}
		this.photonView.RPC("ToolPurchaseV2_RPC", RpcTarget.Others, new object[]
		{
			GhostReactorManager.ToolPurchaseActionV2.SelectShelfAndItem,
			PhotonNetwork.LocalPlayer.ActorNumber,
			indexForToolUpgradeStationFull,
			shelf,
			item
		});
	}

	private void SelectToolShelfAndItemRPCRouted(int stationIndex, int shelf, int item, PhotonMessageInfo info)
	{
		GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = this.GetToolUpgradeStationFullForIndex(stationIndex);
		if (toolUpgradeStationFullForIndex == null)
		{
			return;
		}
		if (toolUpgradeStationFullForIndex.currentActivePlayerActorNumber == info.Sender.ActorNumber)
		{
			toolUpgradeStationFullForIndex.SetSelectedShelfAndItem(shelf, item, true);
		}
	}

	public void RequestPurchaseToolOrUpgrade(GRToolUpgradePurchaseStationFull station, int shelf, int item)
	{
		int indexForToolUpgradeStationFull = this.GetIndexForToolUpgradeStationFull(station);
		if (indexForToolUpgradeStationFull == -1)
		{
			return;
		}
		this.photonView.RPC("ToolPurchaseV2_RPC", this.GetAuthorityPlayer(), new object[]
		{
			GhostReactorManager.ToolPurchaseActionV2.RequestPurchaseAuthority,
			PhotonNetwork.LocalPlayer.ActorNumber,
			indexForToolUpgradeStationFull,
			shelf,
			item
		});
	}

	public void RequestPurchaseRPCRoutedAuthority(int stationIndex, int shelf, int item, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = this.GetToolUpgradeStationFullForIndex(stationIndex);
		if (toolUpgradeStationFullForIndex == null)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull())
		{
			return;
		}
		if (toolUpgradeStationFullForIndex.currentActivePlayerActorNumber != info.Sender.ActorNumber)
		{
			return;
		}
		ValueTuple<bool, bool> valueTuple = toolUpgradeStationFullForIndex.TryPurchaseAuthority(grplayer, shelf, item);
		bool item2 = valueTuple.Item1;
		if (!valueTuple.Item2)
		{
			return;
		}
		if (item2)
		{
			this.photonView.RPC("ToolPurchaseV2_RPC", RpcTarget.Others, new object[]
			{
				GhostReactorManager.ToolPurchaseActionV2.NotifyPurchaseSuccess,
				info.Sender.ActorNumber,
				stationIndex,
				shelf,
				item
			});
		}
		else
		{
			this.photonView.RPC("ToolPurchaseV2_RPC", RpcTarget.Others, new object[]
			{
				GhostReactorManager.ToolPurchaseActionV2.NotifyPurchaseFail,
				info.Sender.ActorNumber,
				stationIndex,
				shelf,
				item
			});
		}
		toolUpgradeStationFullForIndex.ToolPurchaseResponseLocal(grplayer, shelf, item, item2);
	}

	public void NotifyPurchaseToolOrUpgradeRPCRouted(int actorNumber, int stationIndex, int shelf, int item, bool success, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender))
		{
			return;
		}
		GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = this.GetToolUpgradeStationFullForIndex(stationIndex);
		if (toolUpgradeStationFullForIndex == null)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(actorNumber);
		if (grplayer != null)
		{
			toolUpgradeStationFullForIndex.ToolPurchaseResponseLocal(grplayer, shelf, item, success);
		}
	}

	public void RequestStationExclusivity(GRToolUpgradePurchaseStationFull station)
	{
		int indexForToolUpgradeStationFull = this.GetIndexForToolUpgradeStationFull(station);
		if (indexForToolUpgradeStationFull == -1)
		{
			return;
		}
		this.photonView.RPC("ToolPurchaseV2_RPC", this.GetAuthorityPlayer(), new object[]
		{
			GhostReactorManager.ToolPurchaseActionV2.RequestStationExclusivityAuthority,
			PhotonNetwork.LocalPlayer.ActorNumber,
			indexForToolUpgradeStationFull,
			0,
			0
		});
	}

	public void SetActivePlayerAuthority(GRToolUpgradePurchaseStationFull station, int actorNumber)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		int indexForToolUpgradeStationFull = this.GetIndexForToolUpgradeStationFull(station);
		if (indexForToolUpgradeStationFull == -1)
		{
			return;
		}
		station.SetActivePlayer(actorNumber);
		this.photonView.RPC("ToolPurchaseV2_RPC", RpcTarget.Others, new object[]
		{
			GhostReactorManager.ToolPurchaseActionV2.SetToolStationActivePlayer,
			PhotonNetwork.LocalPlayer.ActorNumber,
			indexForToolUpgradeStationFull,
			station.currentActivePlayerActorNumber,
			0
		});
	}

	public void RequestStationExclusivityRPCRoutedAuthority(int stationIndex, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = this.GetToolUpgradeStationFullForIndex(stationIndex);
		if (toolUpgradeStationFullForIndex == null)
		{
			return;
		}
		if (toolUpgradeStationFullForIndex.currentActivePlayerActorNumber != -1)
		{
			return;
		}
		this.SetActivePlayerAuthority(toolUpgradeStationFullForIndex, info.Sender.ActorNumber);
	}

	public void SetToolStationActivePlayerRPCRouted(int stationIndex, int activeOwner, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender))
		{
			return;
		}
		GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = this.GetToolUpgradeStationFullForIndex(stationIndex);
		if (toolUpgradeStationFullForIndex == null)
		{
			return;
		}
		toolUpgradeStationFullForIndex.SetActivePlayer(activeOwner);
	}

	public void BroadcastHandleAndSelectionWheelPosition(GRToolUpgradePurchaseStationFull station, int handlePos, int wheelPos)
	{
		int indexForToolUpgradeStationFull = this.GetIndexForToolUpgradeStationFull(station);
		if (indexForToolUpgradeStationFull == -1)
		{
			return;
		}
		if (NetworkSystem.Instance.LocalPlayer.ActorNumber != station.currentActivePlayerActorNumber)
		{
			return;
		}
		this.photonView.RPC("ToolPurchaseV2_RPC", RpcTarget.Others, new object[]
		{
			GhostReactorManager.ToolPurchaseActionV2.SetHandleAndSelectionWheelPosition,
			PhotonNetwork.LocalPlayer.ActorNumber,
			indexForToolUpgradeStationFull,
			handlePos,
			wheelPos
		});
	}

	public void SetHandleAndSelectionWheelPositionRPCRouted(int stationIndex, int handlePos, int wheelPos, PhotonMessageInfo info)
	{
		GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = this.GetToolUpgradeStationFullForIndex(stationIndex);
		if (toolUpgradeStationFullForIndex == null)
		{
			return;
		}
		if (info.Sender.ActorNumber != toolUpgradeStationFullForIndex.currentActivePlayerActorNumber)
		{
			return;
		}
		toolUpgradeStationFullForIndex.SetHandleAndSelectionWheelPositionRemote(handlePos, wheelPos);
	}

	public void RequestHackToolStation()
	{
	}

	[PunRPC]
	public void ToolPurchaseV2_RPC(GhostReactorManager.ToolPurchaseActionV2 command, int initiatorID, int stationIndex, int param1, int param2, PhotonMessageInfo info)
	{
		if (this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ToolUpgradeStationAction))
		{
			return;
		}
		switch (command)
		{
		case GhostReactorManager.ToolPurchaseActionV2.RequestPurchaseAuthority:
			this.RequestPurchaseRPCRoutedAuthority(stationIndex, param1, param2, info);
			return;
		case GhostReactorManager.ToolPurchaseActionV2.SelectShelfAndItem:
			this.SelectToolShelfAndItemRPCRouted(stationIndex, param1, param2, info);
			return;
		case GhostReactorManager.ToolPurchaseActionV2.NotifyPurchaseFail:
			this.NotifyPurchaseToolOrUpgradeRPCRouted(initiatorID, stationIndex, param1, param2, false, info);
			return;
		case GhostReactorManager.ToolPurchaseActionV2.NotifyPurchaseSuccess:
			this.NotifyPurchaseToolOrUpgradeRPCRouted(initiatorID, stationIndex, param1, param2, true, info);
			return;
		case GhostReactorManager.ToolPurchaseActionV2.RequestStationExclusivityAuthority:
			this.RequestStationExclusivityRPCRoutedAuthority(stationIndex, info);
			return;
		case GhostReactorManager.ToolPurchaseActionV2.SetToolStationActivePlayer:
			this.SetToolStationActivePlayerRPCRouted(stationIndex, param1, info);
			return;
		case GhostReactorManager.ToolPurchaseActionV2.SetHandleAndSelectionWheelPosition:
			this.SetHandleAndSelectionWheelPositionRPCRouted(stationIndex, param1, param2, info);
			break;
		case GhostReactorManager.ToolPurchaseActionV2.SetToolStationHackedDebug:
			break;
		default:
			return;
		}
	}

	public void ToolPurchaseStationRequest(int stationIndex, GhostReactorManager.ToolPurchaseStationAction action)
	{
		this.photonView.RPC("ToolPurchaseStationRequestRPC", this.GetAuthorityPlayer(), new object[] { stationIndex, action });
	}

	[PunRPC]
	public void ToolPurchaseStationRequestRPC(int stationIndex, GhostReactorManager.ToolPurchaseStationAction action, PhotonMessageInfo info)
	{
		if (this.reactor == null)
		{
			return;
		}
		List<GRToolPurchaseStation> toolPurchasingStations = this.reactor.toolPurchasingStations;
		if (!this.IsValidAuthorityRPC(info.Sender) || stationIndex < 0 || stationIndex >= toolPurchasingStations.Count)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer.IsNull() || !grplayer.requestToolPurchaseStationLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		GRToolPurchaseStation grtoolPurchaseStation = toolPurchasingStations[stationIndex];
		if (grtoolPurchaseStation == null)
		{
			return;
		}
		switch (action)
		{
		case GhostReactorManager.ToolPurchaseStationAction.ShiftLeft:
			grtoolPurchaseStation.ShiftLeftAuthority();
			this.photonView.RPC("ToolPurchaseStationResponseRPC", RpcTarget.Others, new object[]
			{
				stationIndex,
				GhostReactorManager.ToolPurchaseStationResponse.SelectionUpdate,
				grtoolPurchaseStation.ActiveEntryIndex,
				0
			});
			this.ToolPurchaseResponseLocal(stationIndex, GhostReactorManager.ToolPurchaseStationResponse.SelectionUpdate, grtoolPurchaseStation.ActiveEntryIndex, 0);
			return;
		case GhostReactorManager.ToolPurchaseStationAction.ShiftRight:
			grtoolPurchaseStation.ShiftRightAuthority();
			this.photonView.RPC("ToolPurchaseStationResponseRPC", RpcTarget.Others, new object[]
			{
				stationIndex,
				GhostReactorManager.ToolPurchaseStationResponse.SelectionUpdate,
				grtoolPurchaseStation.ActiveEntryIndex,
				0
			});
			this.ToolPurchaseResponseLocal(stationIndex, GhostReactorManager.ToolPurchaseStationResponse.SelectionUpdate, grtoolPurchaseStation.ActiveEntryIndex, 0);
			return;
		case GhostReactorManager.ToolPurchaseStationAction.TryPurchase:
		{
			bool flag = false;
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetNetPlayerByID(info.Sender.ActorNumber), out rigContainer))
			{
				GRPlayer component = rigContainer.Rig.GetComponent<GRPlayer>();
				int num;
				if (component != null && grtoolPurchaseStation.TryPurchaseAuthority(component, out num))
				{
					this.photonView.RPC("ToolPurchaseStationResponseRPC", RpcTarget.Others, new object[]
					{
						stationIndex,
						GhostReactorManager.ToolPurchaseStationResponse.PurchaseSucceeded,
						info.Sender.ActorNumber,
						num
					});
					this.ToolPurchaseResponseLocal(stationIndex, GhostReactorManager.ToolPurchaseStationResponse.PurchaseSucceeded, info.Sender.ActorNumber, num);
					flag = true;
				}
			}
			if (!flag)
			{
				this.photonView.RPC("ToolPurchaseStationResponseRPC", RpcTarget.Others, new object[]
				{
					stationIndex,
					GhostReactorManager.ToolPurchaseStationResponse.PurchaseFailed,
					info.Sender.ActorNumber,
					0
				});
				this.ToolPurchaseResponseLocal(stationIndex, GhostReactorManager.ToolPurchaseStationResponse.PurchaseFailed, info.Sender.ActorNumber, 0);
			}
			return;
		}
		default:
			return;
		}
	}

	[PunRPC]
	public void ToolPurchaseStationResponseRPC(int stationIndex, GhostReactorManager.ToolPurchaseStationResponse responseType, int dataA, int dataB, PhotonMessageInfo info)
	{
		if (this.reactor == null)
		{
			return;
		}
		List<GRToolPurchaseStation> toolPurchasingStations = this.reactor.toolPurchasingStations;
		if (!this.IsValidClientRPC(info.Sender) || stationIndex < 0 || stationIndex >= toolPurchasingStations.Count || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ToolPurchaseResponse))
		{
			return;
		}
		this.ToolPurchaseResponseLocal(stationIndex, responseType, dataA, dataB);
	}

	private void ToolPurchaseResponseLocal(int stationIndex, GhostReactorManager.ToolPurchaseStationResponse responseType, int dataA, int dataB)
	{
		if (this.reactor == null)
		{
			return;
		}
		List<GRToolPurchaseStation> toolPurchasingStations = this.reactor.toolPurchasingStations;
		if (stationIndex < 0 || stationIndex >= toolPurchasingStations.Count)
		{
			return;
		}
		GRToolPurchaseStation grtoolPurchaseStation = toolPurchasingStations[stationIndex];
		if (grtoolPurchaseStation == null)
		{
			return;
		}
		switch (responseType)
		{
		case GhostReactorManager.ToolPurchaseStationResponse.SelectionUpdate:
			grtoolPurchaseStation.OnSelectionUpdate(dataA);
			return;
		case GhostReactorManager.ToolPurchaseStationResponse.PurchaseSucceeded:
		{
			grtoolPurchaseStation.OnPurchaseSucceeded();
			GRPlayer grplayer = GRPlayer.Get(dataA);
			if (grplayer != null)
			{
				grplayer.IncrementCoresSpentPlayer(dataB);
				grplayer.AddItemPurchased(grtoolPurchaseStation.GetCurrentToolName());
				grplayer.SubtractShiftCredit(dataB);
				return;
			}
			break;
		}
		case GhostReactorManager.ToolPurchaseStationResponse.PurchaseFailed:
			grtoolPurchaseStation.OnPurchaseFailed();
			break;
		default:
			return;
		}
	}

	public void ToolUpgradeStationRequestUpgrade(GRToolProgressionManager.ToolParts UpgradeID, int entityNetId)
	{
		this.photonView.RPC("ToolUpgradeStationRequestUpgradeRPC", this.GetAuthorityPlayer(), new object[] { UpgradeID, entityNetId });
	}

	public void ToolSnapRequestUpgrade(int upgradeNetID, GRToolProgressionManager.ToolParts UpgradeID, int entityNetId)
	{
		this.photonView.RPC("ToolSnapRequestUpgradeRPC", this.GetAuthorityPlayer(), new object[] { upgradeNetID, UpgradeID, entityNetId });
	}

	[PunRPC]
	public void ToolSnapRequestUpgradeRPC(int upgradeNetID, GRToolProgressionManager.ToolParts UpgradeID, int entityNetId, PhotonMessageInfo info)
	{
		if (this.reactor == null)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
		if (grplayer == null)
		{
			return;
		}
		if (this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ToolUpgradeStationAction))
		{
			return;
		}
		if (!this.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		GameEntity gameEntity = this.gameEntityManager.GetGameEntity(this.gameEntityManager.GetEntityIdFromNetId(entityNetId));
		if (gameEntity != null)
		{
			Object component = gameEntity.GetComponent<GRTool>();
			GameEntity gameEntity2 = this.gameEntityManager.GetGameEntity(this.gameEntityManager.GetEntityIdFromNetId(upgradeNetID));
			if (component != null && gameEntity2 != null && GameEntityManager.IsPlayerHandNearPosition(grplayer.gamePlayer, gameEntity2.transform.position, false, true, 16f) && GameEntityManager.IsPlayerHandNearPosition(grplayer.gamePlayer, gameEntity2.transform.position, false, true, 16f))
			{
				this.photonView.RPC("UpgradeToolRemoteRPC", RpcTarget.All, new object[]
				{
					UpgradeID,
					entityNetId,
					false,
					info.Sender.ActorNumber
				});
				this.gameEntityManager.RequestDestroyItem(gameEntity2.id);
			}
		}
	}

	public void ToolUpgradeStationRequestUpgradeRPC(GRToolProgressionManager.ToolParts UpgradeID, int entityNetId, PhotonMessageInfo info)
	{
	}

	[PunRPC]
	public void UpgradeToolRemoteRPC(GRToolProgressionManager.ToolParts UpgradeID, int entityNetId, bool applyCost, int playerNetId, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender))
		{
			return;
		}
		if (applyCost)
		{
			GRPlayer grplayer = GRPlayer.Get(info.Sender.ActorNumber);
			int num;
			if (grplayer != null && this.reactor.toolProgression.GetShiftCreditCost(UpgradeID, out num))
			{
				grplayer.SubtractShiftCredit(num);
			}
		}
		GameEntity gameEntity = this.gameEntityManager.GetGameEntity(this.gameEntityManager.GetEntityIdFromNetId(entityNetId));
		if (gameEntity != null)
		{
			GRTool component = gameEntity.GetComponent<GRTool>();
			if (component != null)
			{
				component.UpgradeTool(UpgradeID);
			}
		}
	}

	private bool DoesUserHaveResearchUnlocked(int UserID, string ResearchID)
	{
		return true;
	}

	public void ToolPlacedInUpgradeStation(GameEntity entity)
	{
		this.photonView.RPC("PlacedToolInUpgradeStationRPC", RpcTarget.All, new object[] { this.gameEntityManager.GetNetIdFromEntityId(entity.id) });
	}

	public void PlacedToolInUpgradeStationRPC(int entityNetId, PhotonMessageInfo info)
	{
	}

	public void UpgradeToolAtToolStation()
	{
		this.photonView.RPC("UpgradeToolAtToolStationRPC", RpcTarget.All, Array.Empty<object>());
	}

	public void UpgradeToolAtToolStationRPC(PhotonMessageInfo info)
	{
	}

	public void LocalEjectToolInUpgradeStation()
	{
	}

	public void EntityEnteredDropZone(GameEntity entity)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		if (this.reactor == null)
		{
			return;
		}
		GRUIStationEmployeeBadges employeeBadges = this.reactor.employeeBadges;
		long num = BitPackUtils.PackWorldPosForNetwork(entity.transform.position);
		int num2 = BitPackUtils.PackQuaternionForNetwork(entity.transform.rotation);
		if (entity.gameObject.GetComponent<GRBadge>() != null)
		{
			GRUIEmployeeBadgeDispenser gruiemployeeBadgeDispenser = employeeBadges.badgeDispensers[entity.gameObject.GetComponent<GRBadge>().dispenserIndex];
			if (gruiemployeeBadgeDispenser != null)
			{
				num = BitPackUtils.PackWorldPosForNetwork(gruiemployeeBadgeDispenser.GetSpawnPosition());
				num2 = BitPackUtils.PackQuaternionForNetwork(gruiemployeeBadgeDispenser.GetSpawnRotation());
			}
		}
		this.photonView.RPC("EntityEnteredDropZoneRPC", RpcTarget.All, new object[]
		{
			this.gameEntityManager.GetNetIdFromEntityId(entity.id),
			num,
			num2
		});
	}

	[PunRPC]
	public void EntityEnteredDropZoneRPC(int entityNetId, long position, int rotation, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender, entityNetId) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.EntityEnteredDropZone))
		{
			return;
		}
		GorillaNot.IncrementRPCCall(info, "EntityEnteredDropZoneRPC");
		Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(position);
		float num = 10000f;
		if (!(in vector).IsValid(in num))
		{
			return;
		}
		Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(rotation);
		if (!(in quaternion).IsValid())
		{
			return;
		}
		if (!this.IsPositionInZone(vector))
		{
			return;
		}
		if ((vector - this.reactor.dropZone.transform.position).magnitude > 5f)
		{
			return;
		}
		this.LocalEntityEnteredDropZone(this.gameEntityManager.GetEntityIdFromNetId(entityNetId), vector, quaternion);
	}

	private void LocalEntityEnteredDropZone(GameEntityId entityId, Vector3 position, Quaternion rotation)
	{
		if (this.reactor == null)
		{
			return;
		}
		GRDropZone dropZone = this.reactor.dropZone;
		Vector3 vector = dropZone.GetRepelDirectionWorld() * GhostReactor.DROP_ZONE_REPEL;
		GameEntity gameEntity = this.gameEntityManager.GetGameEntity(entityId);
		GamePlayer gamePlayer;
		if (gameEntity.heldByActorNumber >= 0 && GamePlayer.TryGetGamePlayer(gameEntity.heldByActorNumber, out gamePlayer))
		{
			int num = gamePlayer.FindHandIndex(entityId);
			gamePlayer.ClearGrabbedIfHeld(entityId);
			if (gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
			{
				GamePlayerLocal.instance.gamePlayer.ClearGrabbed(num);
				GamePlayerLocal.instance.ClearGrabbed(num);
			}
			gameEntity.heldByActorNumber = -1;
			gameEntity.heldByHandIndex = -1;
			Action onReleased = gameEntity.OnReleased;
			if (onReleased != null)
			{
				onReleased();
			}
		}
		gameEntity.transform.SetParent(null);
		gameEntity.transform.SetLocalPositionAndRotation(position, rotation);
		if (!(gameEntity.gameObject.GetComponent<GRBadge>() != null))
		{
			Rigidbody component = gameEntity.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.isKinematic = false;
				component.position = position;
				component.rotation = rotation;
				component.linearVelocity = vector;
				component.angularVelocity = Vector3.zero;
			}
		}
		dropZone.PlayEffect();
	}

	public void RequestRecycleScanItem(GameEntityId gameEntityId)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		int netIdFromEntityId = this.gameEntityManager.GetNetIdFromEntityId(gameEntityId);
		if (netIdFromEntityId == -1)
		{
			return;
		}
		base.SendRPC("ApplyRecycleScanItemRPC", RpcTarget.All, new object[] { netIdFromEntityId });
	}

	[PunRPC]
	public void ApplyRecycleScanItemRPC(int netId, PhotonMessageInfo info)
	{
		if (!this.IsZoneActive() || !this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplRecycleScanItem))
		{
			return;
		}
		GameEntityId entityIdFromNetId = this.gameEntityManager.GetEntityIdFromNetId(netId);
		this.reactor.recycler.ScanItem(entityIdFromNetId);
	}

	public void RequestRecycleItem(int lastHeldActorNumber, GameEntityId toolId, GRTool.GRToolType toolType)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		if (this.gameEntityManager == null)
		{
			return;
		}
		int netIdFromEntityId = this.gameEntityManager.GetNetIdFromEntityId(toolId);
		if (netIdFromEntityId == -1)
		{
			return;
		}
		base.SendRPC("ApplyRecycleItemRPC", RpcTarget.All, new object[] { lastHeldActorNumber, netIdFromEntityId, toolType });
	}

	[PunRPC]
	public void ApplyRecycleItemRPC(int lastHeldActorNumber, int toolNetId, GRTool.GRToolType toolType, PhotonMessageInfo info)
	{
		if (!this.IsZoneActive() || !this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplyRecycleItem) || !this.gameEntityManager.IsEntityNearPosition(toolNetId, this.reactor.recycler.transform.position, 16f))
		{
			return;
		}
		int count = this.reactor.vrRigs.Count;
		Mathf.FloorToInt((float)this.reactor.recycler.GetRecycleValue(toolType) / (float)count);
		ProgressionManager.Instance.RecycleTool(toolType, this.reactor.vrRigs.Count);
		this.reactor.RefreshScoreboards();
		this.reactor.recycler.RecycleItem();
		this.gameEntityManager.DestroyItemLocal(this.gameEntityManager.GetEntityIdFromNetId(toolNetId));
	}

	public void RequestSentientCorePerformJump(GameEntity entity, Vector3 startPos, Vector3 normal, Vector3 direction, float waitTime)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		int netIdFromEntityId = this.gameEntityManager.GetNetIdFromEntityId(entity.id);
		double num = PhotonNetwork.Time + (double)waitTime;
		base.SendRPC("SentientCorePerformJumpRPC", RpcTarget.All, new object[] { netIdFromEntityId, startPos, normal, direction, num });
	}

	[PunRPC]
	public void SentientCorePerformJumpRPC(int entityNetId, Vector3 startPosition, Vector3 surfaceNormal, Vector3 jumpDirection, double jumpStartTime, PhotonMessageInfo info)
	{
		if (this.IsValidClientRPC(info.Sender, entityNetId, startPosition) && !this.m_RpcSpamChecks.IsSpamming(GhostReactorManager.RPC.ApplySentientCoreDestination))
		{
			float num = 10000f;
			if ((in startPosition).IsValid(in num))
			{
				float num2 = 10000f;
				if ((in surfaceNormal).IsValid(in num2))
				{
					float num3 = 10000f;
					if ((in jumpDirection).IsValid(in num3) && double.IsFinite(jumpStartTime) && PhotonNetwork.Time - jumpStartTime <= 5.0 && this.gameEntityManager.IsEntityNearPosition(entityNetId, startPosition, 16f))
					{
						GameEntity gameEntity = this.gameEntityManager.GetGameEntity(this.gameEntityManager.GetEntityIdFromNetId(entityNetId));
						if (gameEntity == null)
						{
							return;
						}
						GRSentientCore component = gameEntity.GetComponent<GRSentientCore>();
						if (component == null)
						{
							return;
						}
						component.PerformJump(startPosition, surfaceNormal, jumpDirection, jumpStartTime);
						return;
					}
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
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected void OnNewPlayerEnteredGhostReactor()
	{
		if (this.reactor == null)
		{
			return;
		}
		this.reactor.VRRigRefresh();
	}

	public void OnEntityZoneClear(GTZone zoneId)
	{
	}

	public void OnZoneCreate()
	{
		if (this.reactor == null)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (this.reactor.zone == GTZone.customMaps)
		{
			return;
		}
		int num = this.reactor.PickLevelConfigForDepth(grplayer.shuttleData.targetLevel);
		this.reactor.SetNextDelveDepth(grplayer.shuttleData.targetLevel, num);
		this.reactor.DelveToNextDepth();
		if (this.reactor.shiftManager != null)
		{
			this.reactor.shiftManager.SetState(GhostReactorShiftManager.State.WaitingForConnect, true);
		}
	}

	public void OnZoneInit()
	{
		if (this.reactor == null)
		{
			return;
		}
		if (this.reactor.zone == GTZone.customMaps)
		{
			return;
		}
		this.reactor.VRRigRefresh();
		if (this.reactor.employeeTerminal != null)
		{
			this.reactor.employeeTerminal.Setup();
		}
		if (GRPlayer.Get(NetworkSystem.Instance.LocalPlayer.ActorNumber) != null)
		{
			this.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SetPodLevel, this.reactor.toolProgression.GetDropPodLevel());
			this.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SetPodChassisLevel, this.reactor.toolProgression.GetDropPodChasisLevel());
		}
	}

	public void OnZoneClear(ZoneClearReason reason)
	{
		if (this.reactor == null)
		{
			return;
		}
		GRPlayer component = GamePlayerLocal.instance.gamePlayer.GetComponent<GRPlayer>();
		if (component != null)
		{
			GRBadge badge = component.badge;
			if (badge != null && badge.IsAttachedToPlayer())
			{
				component.lastLeftWithBadgeAttachedTime = Time.timeAsDouble;
			}
			component.SendGameEndedTelemetry(false, reason);
		}
		if (this.reactor.levelGenerator != null)
		{
			this.reactor.levelGenerator.ClearLevelSections();
		}
		if (this.reactor.shiftManager != null)
		{
			this.reactor.shiftManager.OnShiftEnded(0.0, false, reason);
		}
		GRPlayer grplayer = GRPlayer.Get(NetworkSystem.Instance.LocalPlayer.ActorNumber);
		if (grplayer != null)
		{
			grplayer.SetGooParticleSystemEnabled(false, false);
			grplayer.SetGooParticleSystemEnabled(true, false);
		}
	}

	public bool IsZoneReady()
	{
		return this.reactor != null;
	}

	public bool ShouldClearZone()
	{
		return true;
	}

	public void OnCreateGameEntity(GameEntity entity)
	{
	}

	public void SerializeZoneData(BinaryWriter writer)
	{
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		GRUIPromotionBot promotionBot = this.reactor.promotionBot;
		GRUIScoreboard[] array = this.reactor.scoreboards.ToArray();
		writer.Write(this.reactor.depthLevel);
		writer.Write(this.reactor.depthConfigIndex);
		writer.Write(this.reactor.difficultyScalingForCurrentFloor);
		if (shiftManager != null)
		{
			writer.Write(shiftManager.ShiftActive);
			writer.Write(shiftManager.ShiftStartNetworkTime);
			shiftManager.shiftStats.Serialize(writer);
			writer.Write(shiftManager.ShiftId);
			writer.Write(shiftManager.stateStartTime);
			writer.Write((byte)shiftManager.GetState());
			writer.Write(levelGenerator.seed);
		}
		if (promotionBot != null)
		{
			writer.Write(promotionBot.GetCurrentPlayerActorNumber());
			writer.Write((int)promotionBot.currentState);
		}
		for (int i = 0; i < array.Length; i++)
		{
			writer.Write((int)array[i].currentScreen);
		}
		List<GRToolPurchaseStation> toolPurchasingStations = this.reactor.toolPurchasingStations;
		writer.Write(toolPurchasingStations.Count);
		for (int j = 0; j < toolPurchasingStations.Count; j++)
		{
			writer.Write(toolPurchasingStations[j].ActiveEntryIndex);
		}
		List<GRToolUpgradePurchaseStationFull> toolUpgradePurchaseStationsFull = this.reactor.toolUpgradePurchaseStationsFull;
		writer.Write(toolUpgradePurchaseStationsFull.Count);
		for (int k = 0; k < toolUpgradePurchaseStationsFull.Count; k++)
		{
			writer.Write(toolUpgradePurchaseStationsFull[k].SelectedShelf);
			writer.Write(toolUpgradePurchaseStationsFull[k].SelectedItem);
			writer.Write(toolUpgradePurchaseStationsFull[k].currentActivePlayerActorNumber);
		}
		List<GhostReactor.EntityTypeRespawnTracker> respawnQueue = this.reactor.respawnQueue;
		writer.Write(this.reactor.respawnQueue.Count);
		for (int l = 0; l < respawnQueue.Count; l++)
		{
			writer.Write(respawnQueue[l].entityTypeID);
			writer.Write(respawnQueue[l].entityCreateData);
			writer.Write(respawnQueue[l].entityNextRespawnTime);
		}
		bool flag = false;
		writer.Write(flag);
	}

	public void DeserializeZoneData(BinaryReader reader)
	{
		GhostReactorShiftManager shiftManager = this.reactor.shiftManager;
		GhostReactorLevelGenerator levelGenerator = this.reactor.levelGenerator;
		GRUIPromotionBot promotionBot = this.reactor.promotionBot;
		GRUIScoreboard[] array = this.reactor.scoreboards.ToArray();
		int num = reader.ReadInt32();
		this.reactor.depthLevel = num;
		int num2 = reader.ReadInt32();
		this.reactor.depthConfigIndex = num2;
		float num3 = reader.ReadSingle();
		this.reactor.difficultyScalingForCurrentFloor = num3;
		if (shiftManager != null)
		{
			bool flag = reader.ReadBoolean();
			double num4 = reader.ReadDouble();
			shiftManager.shiftStats.Deserialize(reader);
			shiftManager.RefreshShiftStatsDisplay();
			string text = reader.ReadString();
			shiftManager.SetShiftId(text);
			shiftManager.stateStartTime = reader.ReadDouble();
			GhostReactorShiftManager.State state = (GhostReactorShiftManager.State)reader.ReadByte();
			shiftManager.SetState(state, true);
			int num5 = reader.ReadInt32();
			if (flag)
			{
				levelGenerator.Generate(num5);
				shiftManager.OnShiftStarted(text, num4, false, true);
				this.reactor.ClearAllHandprints();
			}
		}
		if (promotionBot != null)
		{
			int num6 = reader.ReadInt32();
			int num7 = reader.ReadInt32();
			promotionBot.SetActivePlayerStateChange(num6, num7);
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i].currentScreen = (GRUIScoreboard.ScoreboardScreen)reader.ReadInt32();
		}
		this.reactor.RefreshScoreboards();
		this.reactor.RefreshDepth();
		List<GRToolPurchaseStation> toolPurchasingStations = this.reactor.toolPurchasingStations;
		int num8 = reader.ReadInt32();
		for (int j = 0; j < num8; j++)
		{
			int num9 = reader.ReadInt32();
			if (j < toolPurchasingStations.Count && toolPurchasingStations[j] != null)
			{
				toolPurchasingStations[j].OnSelectionUpdate(num9);
			}
		}
		List<GRToolUpgradePurchaseStationFull> toolUpgradePurchaseStationsFull = this.reactor.toolUpgradePurchaseStationsFull;
		int num10 = reader.ReadInt32();
		for (int k = 0; k < num10; k++)
		{
			int num11 = reader.ReadInt32();
			int num12 = reader.ReadInt32();
			int num13 = reader.ReadInt32();
			if (k < toolUpgradePurchaseStationsFull.Count && toolUpgradePurchaseStationsFull[k] != null)
			{
				toolUpgradePurchaseStationsFull[k].SetSelectedShelfAndItem(num11, num12, true);
				toolUpgradePurchaseStationsFull[k].SetActivePlayer(num13);
			}
		}
		List<GhostReactor.EntityTypeRespawnTracker> respawnQueue = this.reactor.respawnQueue;
		respawnQueue.Clear();
		int num14 = reader.ReadInt32();
		for (int l = 0; l < num14; l++)
		{
			respawnQueue.Add(new GhostReactor.EntityTypeRespawnTracker
			{
				entityTypeID = reader.ReadInt32(),
				entityCreateData = reader.ReadInt64(),
				entityNextRespawnTime = reader.ReadSingle()
			});
		}
		reader.ReadBoolean();
		this.reactor.VRRigRefresh();
	}

	public long ProcessMigratedGameEntityCreateData(GameEntity entity, long createData)
	{
		return createData;
	}

	public bool ValidateMigratedGameEntity(int netId, int entityTypeId, Vector3 position, Quaternion rotation, long createData, int actorNr)
	{
		return false;
	}

	public void SerializeZoneEntityData(BinaryWriter writer, GameEntity entity)
	{
	}

	public void DeserializeZoneEntityData(BinaryReader reader, GameEntity entity)
	{
	}

	public void OnTapLocal(bool isLeftHand, Vector3 pos, Quaternion rot, GorillaSurfaceOverride surfaceOverride, Vector3 handVelocity)
	{
		if (this.reactor != null)
		{
			this.reactor.OnTapLocal(isLeftHand, pos, rot, surfaceOverride);
		}
		if (this.IsAuthority())
		{
			float num = Math.Clamp(handVelocity.magnitude / 8f, 0f, 1f);
			if (num > 0.25f)
			{
				GRNoiseEventManager.instance.AddNoiseEvent(pos, num, 1f);
			}
		}
	}

	public void OnSharedTap(VRRig rig, Vector3 tapPos, float handTapSpeed)
	{
		if (this.IsAuthority())
		{
			float num = Math.Clamp(handTapSpeed / 8f, 0f, 1f);
			if (num > 0.25f)
			{
				GRNoiseEventManager.instance.AddNoiseEvent(tapPos, num, 1f);
			}
		}
	}

	public void SerializeZonePlayerData(BinaryWriter writer, int actorNumber)
	{
		GRPlayer grplayer = GRPlayer.Get(actorNumber);
		grplayer.SerializeNetworkState(writer, grplayer.gamePlayer.rig.OwningNetPlayer);
	}

	public void DeserializeZonePlayerData(BinaryReader reader, int actorNumber)
	{
		GRPlayer grplayer = GRPlayer.Get(actorNumber);
		GRPlayer.DeserializeNetworkStateAndBurn(reader, grplayer, this);
	}

	public bool DebugIsToolStationHacked()
	{
		return false;
	}

	public static bool AggroDisabled
	{
		get
		{
			return false;
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

	private const string EVENT_CORE_COLLECTED = "GRCollectCore";

	private const string EVENT_ENEMY_KILLED = "GRKillEnemy";

	public const string EVENT_BREAKABLE_BROKEN = "GRSmashBreakable";

	public const string EVENT_ENEMY_ARMOR_BREAK = "GRArmorBreak";

	public const string NETWORK_ROOM_GR_DEPTH = "ghostReactorDepth";

	public const int GHOSTREACTOR_ZONE_ID = 5;

	public const GTZone GT_ZONE_GHOSTREACTOR = GTZone.ghostReactor;

	public GameEntityManager gameEntityManager;

	public GameAgentManager gameAgentManager;

	public GRNoiseEventManager noiseEventManager;

	public PhotonView photonView;

	public GhostReactor reactor;

	public CallLimitersList<CallLimiter, GhostReactorManager.RPC> m_RpcSpamChecks = new CallLimitersList<CallLimiter, GhostReactorManager.RPC>();

	private const float HandprintThrottleTime = 0.25f;

	private float LastHandprintTime;

	private Coroutine activeSpawnSectionEntitiesCoroutine;

	private WaitForSeconds spawnSectionEntitiesWait = new WaitForSeconds(0.1f);

	private static List<GameEntityId> tempEntitiesToDestroy = new List<GameEntityId>();

	public GRToolUpgradeStation upgradeStation;

	public static bool entityDebugEnabled = false;

	public static bool noiseDebugEnabled = false;

	public static bool bayUnlockEnabled = false;

	public enum RPC
	{
		ApplyCollectItem,
		ApplyChargeTool,
		ApplyDepositCurrency,
		ApplyPlayerRevived,
		GrantPlayerShield,
		RequestFireProjectile,
		ApplyShiftStart,
		ApplyShiftEnd,
		ToolPurchaseResponse,
		ApplyBreakableBroken,
		EntityEnteredDropZone,
		PromotionBotResponse,
		DistillItem,
		ApplySentientCoreDestination,
		Handprint,
		ApplyRecycleItem,
		ApplRecycleScanItem,
		SeedExtractorAction,
		ToolUpgradeStationAction,
		SendMothershipId,
		RefreshShiftCredit
	}

	public enum GRPlayerAction
	{
		ButtonShiftStart,
		DelveDeeper,
		DelveState,
		ShuttleOpen,
		ShuttleClose,
		ShuttleLaunch,
		ShuttleArrive,
		ShuttleTargetLevelUp,
		ShuttleTargetLevelDown,
		SetPodLevel,
		SetPodChassisLevel,
		SeedExtractorOpenStation,
		SeedExtractorCloseStation,
		SeedExtractorCardSwipeFail,
		SeedExtractorTryDepositSeed,
		SeedExtractorDepositSeedSucceeded,
		SeedExtractorDepositSeedFailed,
		DEBUG_ResetDepth,
		DEBUG_DelveDeeper,
		DEBUG_DelveShallower
	}

	public enum ToolPurchaseActionV2
	{
		RequestPurchaseAuthority,
		SelectShelfAndItem,
		NotifyPurchaseFail,
		NotifyPurchaseSuccess,
		RequestStationExclusivityAuthority,
		SetToolStationActivePlayer,
		SetHandleAndSelectionWheelPosition,
		SetToolStationHackedDebug
	}

	public enum ToolPurchaseStationAction
	{
		ShiftLeft,
		ShiftRight,
		TryPurchase
	}

	public enum ToolPurchaseStationResponse
	{
		SelectionUpdate,
		PurchaseSucceeded,
		PurchaseFailed
	}
}
