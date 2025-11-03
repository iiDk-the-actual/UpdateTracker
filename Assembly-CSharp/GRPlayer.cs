using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

public class GRPlayer : MonoBehaviourTick
{
	public GRPlayer.GRPlayerState State
	{
		get
		{
			return this.state;
		}
	}

	public int Juice
	{
		get
		{
			return this.playerJuice;
		}
	}

	public int ShiftCreditCapIncreases { get; set; }

	public int ShiftCreditCapIncreasesMax { get; set; }

	public int ShiftCredits
	{
		get
		{
			return this.shiftCreditCache;
		}
	}

	public bool HasXRayVision()
	{
		return this.xRayVisionRefCount > 0;
	}

	public int MaxHp
	{
		get
		{
			return this.maxHp;
		}
	}

	public int MaxShieldHp
	{
		get
		{
			return this.maxShieldHp;
		}
	}

	public int Hp
	{
		get
		{
			return this.hp;
		}
	}

	public int ShieldHp
	{
		get
		{
			return this.shieldHp;
		}
	}

	public int ShieldFlags
	{
		get
		{
			return this.shieldFlags;
		}
	}

	public bool InStealthMode
	{
		get
		{
			return this.inStealthMode;
		}
	}

	public VRRig MyRig
	{
		get
		{
			return this.vrRig;
		}
	}

	public float ShiftPlayTime
	{
		get
		{
			return this.shiftPlayTime;
		}
		set
		{
			this.shiftPlayTime = value;
		}
	}

	public int LastShiftCut
	{
		get
		{
			return this.lastShiftCut;
		}
		set
		{
			this.lastShiftCut = value;
		}
	}

	public GRPlayer.ProgressionData CurrentProgression
	{
		get
		{
			return this.currentProgression;
		}
		set
		{
			this.currentProgression = value;
		}
	}

	private void Awake()
	{
		this.vrRig = base.GetComponent<VRRig>();
		this.lowHealthVisualPropertyBlock = new MaterialPropertyBlock();
		this.damageEffects = GTPlayer.Instance.mainCamera.GetComponent<GRPlayerDamageEffects>();
		this.lowHealthTintPropertyId = Shader.PropertyToID("_TintColor");
		this.isEmployee = false;
		this.SetHp(this.maxHp);
		this.SetShieldHp(0);
		this.state = GRPlayer.GRPlayerState.Alive;
		this.RefreshDamageVignetteVisual();
		this.shieldHeadVisual.gameObject.SetActive(false);
		this.shieldBodyVisual.gameObject.SetActive(false);
		this.shieldGameLight = this.shieldBodyVisual.gameObject.GetComponentInChildren<GameLight>(true);
		this.requestCollectItemLimiter = new CallLimiter(25, 1f, 0.5f);
		this.requestChargeToolLimiter = new CallLimiter(25, 1f, 0.5f);
		this.requestDepositCurrencyLimiter = new CallLimiter(25, 1f, 0.5f);
		this.requestShiftStartLimiter = new CallLimiter(25, 1f, 0.5f);
		this.requestToolPurchaseStationLimiter = new CallLimiter(25, 1f, 0.5f);
		this.applyEnemyHitLimiter = new CallLimiter(25, 1f, 0.5f);
		this.reportLocalHitLimiter = new CallLimiter(25, 1f, 0.5f);
		this.reportBreakableBrokenLimiter = new CallLimiter(25, 1f, 0.5f);
		this.playerStateChangeLimiter = new CallLimiter(25, 1f, 0.5f);
		this.promotionBotLimiter = new CallLimiter(25, 1f, 0.5f);
		this.progressionBroadcastLimiter = new CallLimiter(25, 1f, 0.5f);
		this.scoreboardPageLimiter = new CallLimiter(25, 1f, 0.5f);
		this.fireShieldLimiter = new CallLimiter(25, 1f, 0.5f);
		this.shuttleData = new GRPlayer.ShuttleData();
		this.lastLeftWithBadgeAttachedTime = -10000.0;
	}

	private void Start()
	{
		if (this.gamePlayer != null && this.gamePlayer.IsLocal())
		{
			this.LoadMyProgression();
			ProgressionManager.Instance.OnGetShiftCredit += this.OnShiftCreditChanged;
			ProgressionManager.Instance.OnGetShiftCreditCapData += this.OnShiftCreditCapChanged;
			this.soak = new GhostReactorSoak();
			this.soak.Setup(this);
		}
		else
		{
			this.currentProgression = new GRPlayer.ProgressionData
			{
				points = 0,
				redeemedPoints = 0
			};
		}
		if (ProgressionManager.Instance != null)
		{
			ProgressionManager.Instance.OnGetShiftCredit += this.OnShiftCreditChanged;
			ProgressionManager.Instance.OnGetShiftCreditCapData += this.OnShiftCreditCapChanged;
		}
	}

	private new void OnDisable()
	{
		this.Reset();
	}

	public void Reset()
	{
		this.SetHp(this.maxHp);
		this.SetShieldHp(0);
		this.state = GRPlayer.GRPlayerState.Alive;
		this.RefreshDamageVignetteVisual();
		this.RefreshPlayerVisuals();
		for (int i = 0; i < 8; i++)
		{
			this.synchronizedSessionStats[i] = 0f;
		}
	}

	private void SetHp(int newHp)
	{
		this.hp = Mathf.Max(newHp, 0);
	}

	private void SetShieldHp(int newShieldHp)
	{
		this.shieldHp = Mathf.Max(newShieldHp, 0);
	}

	public void OnShiftCreditCapChanged(string targetMothershipId, int newCap, int newCapMax)
	{
		if (this.mothershipId != null && targetMothershipId == this.mothershipId)
		{
			if (this.gamePlayer.IsLocal() && (newCap != this.ShiftCreditCapIncreases || newCapMax != this.ShiftCreditCapIncreasesMax) && GhostReactor.instance != null)
			{
				GhostReactor.instance.grManager.RefreshShiftCredit();
			}
			this.ShiftCreditCapIncreases = newCap;
			this.ShiftCreditCapIncreasesMax = newCapMax;
		}
	}

	public void OnShiftCreditChanged(string targetMothershipId, int newShiftCredits)
	{
		if (this.mothershipId != null && targetMothershipId == this.mothershipId)
		{
			int num = this.shiftCreditCache;
			this.shiftCreditCache = newShiftCredits;
			if (GhostReactor.instance != null && this.gamePlayer.IsLocal() && num != newShiftCredits && GhostReactor.instance != null)
			{
				if (GhostReactor.instance.promotionBot != null)
				{
					GhostReactor.instance.promotionBot.Refresh();
				}
				if (GhostReactor.instance.grManager != null)
				{
					GhostReactor.instance.grManager.RefreshShiftCredit();
				}
			}
		}
		if (GhostReactor.instance != null)
		{
			GhostReactor.instance.RefreshScoreboards();
		}
	}

	public void OnShiftCreditCapData(string targetMothershipId, int shiftCreditCapNumberOfIncreases, int shiftCreditMaxNumberOfIncreases)
	{
		if (this.mothershipId != null)
		{
			targetMothershipId == this.mothershipId;
		}
	}

	public void SubtractShiftCredit(int shiftCreditDelta)
	{
		if (this.gamePlayer.IsLocal())
		{
			ProgressionManager.Instance.SubtractShiftCredit(shiftCreditDelta);
		}
	}

	public void OnPlayerHit(Vector3 hitPosition, GhostReactorManager manager, GameEntityId hitByEntityId)
	{
		GameEntity gameEntity = manager.gameEntityManager.GetGameEntity(hitByEntityId);
		int num = 1;
		if (this.State == GRPlayer.GRPlayerState.Alive)
		{
			if (this.shieldHp > 0)
			{
				if (gameEntity != null)
				{
					GRAttributes component = gameEntity.GetComponent<GRAttributes>();
					if (component != null)
					{
						num = component.CalculateFinalValueForAttribute(GRAttributeType.PlayerShieldDamage);
					}
				}
				this.SetShieldHp(this.shieldHp - num);
				if (this.shieldHp > 0)
				{
					if (this.shieldDamagedSound != null)
					{
						this.audioSource.PlayOneShot(this.shieldDamagedSound, this.shieldDamagedVolume);
					}
					this.shieldDamagedEffect.Play();
				}
				else
				{
					if (this.shieldDestroyedSound != null)
					{
						this.audioSource.PlayOneShot(this.shieldDestroyedSound, this.shieldDestroyedVolume);
					}
					this.shieldDestroyedEffect.Play();
				}
				this.RefreshPlayerVisuals();
				return;
			}
			if (gameEntity != null)
			{
				GRAttributes component2 = gameEntity.GetComponent<GRAttributes>();
				if (component2 != null)
				{
					num = component2.CalculateFinalValueForAttribute(GRAttributeType.PlayerDamage);
				}
			}
			this.PlayHitFx(hitPosition);
			this.SetHp(this.hp - num);
			this.RefreshDamageVignetteVisual();
			if (this.hp <= 0)
			{
				this.ChangePlayerState(GRPlayer.GRPlayerState.Ghost, manager);
			}
		}
	}

	public void OnPlayerRevive(GhostReactorManager manager)
	{
		this.SetHp(this.maxHp);
		this.RefreshDamageVignetteVisual();
		this.ChangePlayerState(GRPlayer.GRPlayerState.Alive, manager);
	}

	public void ChangePlayerState(GRPlayer.GRPlayerState newState, GhostReactorManager manager)
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			newState = GRPlayer.GRPlayerState.Alive;
		}
		if (this.state == newState)
		{
			return;
		}
		this.state = newState;
		GRPlayer.GRPlayerState grplayerState = this.state;
		if (grplayerState != GRPlayer.GRPlayerState.Alive)
		{
			if (grplayerState == GRPlayer.GRPlayerState.Ghost)
			{
				this.SetHp(0);
				this.SetShieldHp(0);
				this.RefreshDamageVignetteVisual();
				if (this.playerTurnedGhostEffect != null)
				{
					this.playerTurnedGhostEffect.Play();
				}
				this.playerTurnedGhostSoundBank.Play();
				manager.ReportPlayerDeath(this);
				this.IncrementDeaths(1);
			}
		}
		else
		{
			this.SetHp(this.maxHp);
			this.RefreshDamageVignetteVisual();
			this.IncrementRevives(1);
			if (this.playerRevivedEffect != null)
			{
				this.playerRevivedEffect.Play();
			}
			if (this.audioSource != null && this.playerRevivedSound != null)
			{
				this.audioSource.PlayOneShot(this.playerRevivedSound, this.playerRevivedVolume);
			}
		}
		this.RefreshPlayerVisuals();
		if (this.vrRig.isLocal)
		{
			this.vrRigs.Clear();
			VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
			for (int i = 0; i < this.vrRigs.Count; i++)
			{
				this.vrRigs[i].GetComponent<GRPlayer>().RefreshPlayerVisuals();
			}
		}
	}

	public void RefreshPlayerVisuals()
	{
		this.RefreshDamageVignetteVisual();
		GRPlayer.GRPlayerState grplayerState = this.state;
		if (grplayerState == GRPlayer.GRPlayerState.Alive)
		{
			this.gamePlayer.DisableGrabbing(false);
			if (this.badge != null)
			{
				this.badge.UnHide();
			}
			this.vrRig.ChangeMaterialLocal(0);
			this.vrRig.bodyRenderer.SetGameModeBodyType(GorillaBodyType.Default);
			this.vrRig.SetInvisibleToLocalPlayer(false);
			if (this.vrRig.isLocal)
			{
				CosmeticsController.instance.SetHideCosmeticsFromRemotePlayers(false);
				GameLightingManager.instance.SetDesaturateAndTintEnabled(false, Color.black);
				Color color = Color.black;
				GhostReactor instance = GhostReactor.instance;
				if (instance != null && instance.zone != GTZone.customMaps)
				{
					color = instance.GetCurrLevelGenConfig().ambientLight;
				}
				GameLightingManager.instance.SetAmbientLightDynamic(color);
			}
			if (this.shieldHp > 0)
			{
				this.shieldHeadVisual.gameObject.SetActive(true);
				this.shieldBodyVisual.gameObject.SetActive(true);
				Color color2 = this.shieldColorNormal;
				if ((this.shieldFlags & 1) != 0)
				{
					color2 = this.shieldColorLight;
				}
				else if ((this.shieldFlags & 2) != 0)
				{
					color2 = this.shieldColorStealth;
				}
				else if ((this.shieldFlags & 4) != 0)
				{
					color2 = this.shieldColorHeal;
				}
				Renderer component = this.shieldBodyVisual.GetComponent<Renderer>();
				if (component != null)
				{
					component.material.SetColor("_BaseColor", color2);
				}
				Renderer component2 = this.shieldHeadVisual.GetComponent<Renderer>();
				if (component2 != null)
				{
					component2.material.SetColor("_BaseColor", color2);
				}
			}
			else
			{
				this.shieldHeadVisual.gameObject.SetActive(false);
				this.shieldBodyVisual.gameObject.SetActive(false);
			}
			this.shieldGameLight.gameObject.SetActive((this.shieldFlags & 1) != 0);
			return;
		}
		if (grplayerState != GRPlayer.GRPlayerState.Ghost)
		{
			return;
		}
		if (this.vrRig.isLocal)
		{
			this.gamePlayer.RequestDropAllSnapped();
		}
		this.gamePlayer.DisableGrabbing(true);
		this.shieldHeadVisual.gameObject.SetActive(false);
		this.shieldBodyVisual.gameObject.SetActive(false);
		this.shieldGameLight.gameObject.SetActive(false);
		if (this.badge != null)
		{
			this.badge.Hide();
		}
		if (this.vrRig.isLocal)
		{
			GamePlayerLocal.instance.OnUpdateInteract();
			this.vrRig.bodyRenderer.SetGameModeBodyType(GorillaBodyType.Skeleton);
			this.vrRig.ChangeMaterialLocal(13);
			this.vrRig.SetInvisibleToLocalPlayer(false);
			CosmeticsController.instance.SetHideCosmeticsFromRemotePlayers(true);
			GameLightingManager.instance.SetDesaturateAndTintEnabled(true, this.deathTintColor);
			GameLightingManager.instance.SetAmbientLightDynamic(this.deathAmbientLightColor);
			return;
		}
		if (VRRigCache.Instance.localRig.GetComponent<GRPlayer>().State == GRPlayer.GRPlayerState.Ghost)
		{
			this.vrRig.ChangeMaterialLocal(13);
			this.vrRig.bodyRenderer.SetGameModeBodyType(GorillaBodyType.Skeleton);
			this.vrRig.SetInvisibleToLocalPlayer(false);
			return;
		}
		this.vrRig.bodyRenderer.SetGameModeBodyType(GorillaBodyType.Invisible);
		this.vrRig.SetInvisibleToLocalPlayer(true);
	}

	public static GRPlayer Get(int actorNumber)
	{
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(actorNumber, out gamePlayer))
		{
			return null;
		}
		return gamePlayer.GetComponent<GRPlayer>();
	}

	public static GRPlayer Get(NetPlayer player)
	{
		if (player == null)
		{
			return null;
		}
		return GRPlayer.Get(player.ActorNumber);
	}

	public static GRPlayer Get(VRRig vrRig)
	{
		if (!(vrRig != null))
		{
			return null;
		}
		return vrRig.GetComponent<GRPlayer>();
	}

	public static GRPlayer GetLocal()
	{
		return GRPlayer.Get(VRRig.LocalRig);
	}

	public void AttachBadge(GRBadge grBadge)
	{
		this.badge = grBadge;
		this.badge.transform.SetParent(this.badgeBodyAnchor);
		this.badge.GetComponent<Rigidbody>().isKinematic = true;
		this.badge.StartRetracting();
	}

	public bool CanActivateShield(int shieldHitPoints)
	{
		return this.state == GRPlayer.GRPlayerState.Alive && shieldHitPoints > 0;
	}

	public bool TryActivateShield(int shieldHitpoints, int shieldFlags)
	{
		if (this.state == GRPlayer.GRPlayerState.Alive)
		{
			if (this.shieldHp <= 0 && this.shieldActivatedSound != null)
			{
				this.audioSource.PlayOneShot(this.shieldActivatedSound, this.shieldActivatedVolume);
			}
			this.SetShieldHp(Mathf.Min(shieldHitpoints, this.maxShieldHp));
			this.shieldFlags = shieldFlags;
			this.inStealthMode = (shieldFlags & 2) != 0;
			if (this.inStealthMode)
			{
				if (this.damageEffects.stealthModeVisualRenderer != null)
				{
					this.damageEffects.stealthModeVisualRenderer.gameObject.SetActive(true);
				}
				this.shieldStealthModeEndTime = Time.timeAsDouble + (double)this.shieldStealthModeDuration;
			}
			if ((shieldFlags & 4) != 0)
			{
				this.SetHp(this.maxHp);
			}
			this.RefreshPlayerVisuals();
			return true;
		}
		return false;
	}

	public void ClearStealthMode()
	{
		this.inStealthMode = false;
		if (this.damageEffects.stealthModeVisualRenderer != null)
		{
			this.damageEffects.stealthModeVisualRenderer.gameObject.SetActive(false);
		}
	}

	public void SerializeNetworkState(BinaryWriter writer, NetPlayer player)
	{
		writer.Write((byte)this.state);
		writer.Write(this.hp);
		writer.Write(this.shieldHp);
		writer.Write(this.shiftJoinTime);
		writer.Write(this.isEmployee ? 1 : 0);
		writer.Write(this.CurrentProgression.points);
		writer.Write(this.CurrentProgression.redeemedPoints);
		writer.Write(this.dropPodLevel);
		writer.Write(this.dropPodChasisLevel);
		for (int i = 0; i < 8; i++)
		{
			writer.Write(this.synchronizedSessionStats[i]);
		}
	}

	public static void DeserializeNetworkStateAndBurn(BinaryReader reader, GRPlayer player, GhostReactorManager grManager)
	{
		GRPlayer.GRPlayerState grplayerState = (GRPlayer.GRPlayerState)reader.ReadByte();
		int num = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		double num3 = reader.ReadDouble();
		bool flag = reader.ReadByte() > 0;
		int num4 = reader.ReadInt32();
		int num5 = reader.ReadInt32();
		int num6 = reader.ReadInt32();
		int num7 = reader.ReadInt32();
		for (int i = 0; i < 8; i++)
		{
			player.synchronizedSessionStats[i] = reader.ReadSingle();
		}
		if (player != null)
		{
			player.SetHp(num);
			player.SetShieldHp(num2);
			player.isEmployee = flag;
			player.ChangePlayerState(grplayerState, grManager);
			player.RefreshPlayerVisuals();
			if (!player.gamePlayer.IsLocal())
			{
				player.SetProgressionData(num4, num5, false);
				player.dropPodLevel = num6;
				player.dropPodChasisLevel = num7;
			}
			if (double.IsNaN(num3) || double.IsInfinity(num3))
			{
				player.shiftJoinTime = PhotonNetwork.Time;
			}
			else
			{
				player.shiftJoinTime = Math.Min(num3, PhotonNetwork.Time);
			}
		}
		if (grManager != null)
		{
			grManager.SendMothershipId();
		}
	}

	public void PlayHitFx(Vector3 attackLocation)
	{
		if (this.playerDamageAudioSource != null)
		{
			this.playerDamageAudioSource.PlayOneShot(this.playerDamageSound, this.playerDamageVolume);
		}
		if (this.bodyCenter != null)
		{
			Vector3 vector = attackLocation - this.bodyCenter.position;
			vector.y = 0f;
			Vector3 vector2 = vector.normalized * this.playerDamageOffsetDist;
			if (this.playerDamageEffect != null)
			{
				this.playerDamageEffect.transform.position = this.bodyCenter.position + vector2;
				this.playerDamageEffect.Play();
			}
			if (this.vrRig.isLocal)
			{
				Vector3 normalized = Vector3.ProjectOnPlane(GTPlayer.Instance.mainCamera.transform.forward, Vector3.up).normalized;
				vector = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
				float num = Vector3.SignedAngle(normalized, vector, Vector3.up);
				this.damageEffects.radialDamageEffect.transform.localRotation = Quaternion.Euler(0f, 0f, -num);
				this.damageEffects.radialDamageEffect.Play();
			}
		}
		if (this.gamePlayer == GamePlayerLocal.instance.gamePlayer)
		{
			GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength, 0.5f);
			GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, 0.5f);
		}
	}

	public void SendGameStartedTelemetry(float timeIntoShift, bool wasPlayerInAtStart, int currentFloor)
	{
		this.vrRigs.Clear();
		VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
		string titleNameFromLevel = GhostReactorProgression.GetTitleNameFromLevel(GhostReactorProgression.GetTitleLevel(this.CurrentProgression.redeemedPoints));
		GorillaTelemetry.GhostReactorShiftStart(this.gameId, this.ShiftCredits, timeIntoShift, wasPlayerInAtStart, this.vrRigs.Count + 1, currentFloor, titleNameFromLevel);
		this.wasPlayerInAtShiftStart = wasPlayerInAtStart;
		this.ResetGameTelemetryTracking();
	}

	public void SendGameEndedTelemetry(bool isShiftActuallyEnding, ZoneClearReason zoneClearReason)
	{
		this.vrRigs.Clear();
		VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
		GorillaTelemetry.GhostReactorGameEnd(this.gameId, this.ShiftCredits, this.totalCoresCollectedByPlayer, this.totalCoresCollectedByGroup, this.totalCoresSpentByPlayer, this.totalCoresSpentByGroup, this.totalGatesUnlocked, this.totalDeaths, this.totalItemsPurchased, this.lastShiftCut, isShiftActuallyEnding, this.timeIntoShiftAtJoin, (float)(PhotonNetwork.Time - (double)this.gameStartTime), this.wasPlayerInAtShiftStart, zoneClearReason, this.maxNumberOfPlayersInShift, this.vrRigs.Count + 1, this.totalItemTypesHeldThisShift, this.totalRevives, this.numShiftsPlayed);
		this.isFirstShift = true;
	}

	public void SendFloorStartedTelemetry(float timeIntoShift, bool wasPlayerInAtStart, int currentFloor, string floorPreset, string floorModifier)
	{
		this.vrRigs.Clear();
		VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
		string titleNameFromLevel = GhostReactorProgression.GetTitleNameFromLevel(GhostReactorProgression.GetTitleLevel(this.CurrentProgression.redeemedPoints));
		GorillaTelemetry.GhostReactorFloorStart(this.gameId, this.ShiftCredits, timeIntoShift, wasPlayerInAtStart, this.vrRigs.Count + 1, titleNameFromLevel, currentFloor, floorPreset, floorModifier);
		this.wasPlayerInAtShiftStart = wasPlayerInAtStart;
	}

	public void SendFloorEndedTelemetry(bool isShiftActuallyEnding, float shiftStartTime, ZoneClearReason zoneClearReason, int currentFloor, string floorPreset, string floorModifier, bool objectivesCompleted, string section, int xpGained)
	{
		this.vrRigs.Clear();
		VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
		GorillaTelemetry.GhostReactorFloorComplete(this.gameId, this.ShiftCredits, this.coresCollectedByPlayer, this.coresCollectedByGroup, this.coresSpentByPlayer, this.coresSpentByGroup, this.gatesUnlocked, this.deaths, this.itemsPurchased, this.lastShiftCut, isShiftActuallyEnding, this.timeIntoShiftAtJoin, (float)(PhotonNetwork.Time - (double)(this.timeIntoShiftAtJoin + shiftStartTime)), this.wasPlayerInAtShiftStart, zoneClearReason, this.maxNumberOfPlayersInShift, this.vrRigs.Count + 1, this.itemTypesHeldThisShift, this.revives, currentFloor, floorPreset, floorModifier, this.sentientCoresCollected, objectivesCompleted, section, xpGained);
	}

	public void SendToolPurchasedTelemetry(string toolName, int toolLevel, int coresSpent, int shinyRocksSpent)
	{
		int num = -1;
		string text = "";
		GhostReactor instance = GhostReactor.instance;
		if (instance != null && instance.zone != GTZone.customMaps)
		{
			num = instance.GetDepthLevel();
			text = instance.GetCurrLevelGenConfig().name;
		}
		GorillaTelemetry.GhostReactorToolPurchased(this.gameId, toolName, toolLevel, coresSpent, shinyRocksSpent, num, text);
	}

	public void SendRankUpTelemetry(string newRank)
	{
		int num = -1;
		string text = "";
		GhostReactor instance = GhostReactor.instance;
		if (instance != null && instance.zone != GTZone.customMaps)
		{
			num = instance.GetDepthLevel();
			text = instance.GetCurrLevelGenConfig().name;
		}
		GorillaTelemetry.GhostReactorRankUp(this.gameId, newRank, num, text);
	}

	public void SendToolUpgradeTelemetry(string upgradeType, string toolName, int newLevel, int juiceSpent, int griftSpent, int coresSpent)
	{
		int num = -1;
		string text = "";
		GhostReactor instance = GhostReactor.instance;
		if (instance != null && instance.zone != GTZone.customMaps)
		{
			num = instance.GetDepthLevel();
			text = instance.GetCurrLevelGenConfig().name;
		}
		GorillaTelemetry.GhostReactorToolUpgrade(this.gameId, upgradeType, toolName, newLevel, juiceSpent, griftSpent, coresSpent, num, text);
	}

	public void SendSeedDepositedTelemetry(string unlockTime, int seedsInQueue)
	{
		int num = -1;
		string text = "";
		GhostReactor instance = GhostReactor.instance;
		if (instance != null && instance.zone != GTZone.customMaps)
		{
			num = instance.GetDepthLevel();
			text = instance.GetCurrLevelGenConfig().name;
		}
		GorillaTelemetry.GhostReactorChaosSeedStart(this.gameId, unlockTime, seedsInQueue, num, text);
	}

	public void SendJuiceCollectedTelemetry(int juiceCollected, int coresProcessedByOverdrive)
	{
		GorillaTelemetry.GhostReactorChaosJuiceCollected(this.gameId, juiceCollected, coresProcessedByOverdrive);
	}

	public void SendOverdrivePurchasedTelemetry(int shinyRocksUsed, int seedsInQueue)
	{
		int num = -1;
		string text = "";
		GhostReactor instance = GhostReactor.instance;
		if (instance != null && instance.zone != GTZone.customMaps)
		{
			num = instance.GetDepthLevel();
			text = instance.GetCurrLevelGenConfig().name;
		}
		GorillaTelemetry.GhostReactorOverdrivePurchased(this.gameId, shinyRocksUsed, seedsInQueue, num, text);
	}

	public void SendPodUpgradeTelemetry(string toolName, int level, int shinyRocksSpent, int juiceSpent)
	{
		GorillaTelemetry.GhostReactorPodUpgradePurchased(this.gameId, toolName, level, shinyRocksSpent, juiceSpent);
	}

	public void SendCreditsRefilledTelemetry(int shinyRocksSpent, int finalCredits)
	{
		int num = -1;
		string text = "";
		GhostReactor instance = GhostReactor.instance;
		if (instance != null && instance.zone != GTZone.customMaps)
		{
			num = instance.GetDepthLevel();
			text = instance.GetCurrLevelGenConfig().name;
		}
		GorillaTelemetry.GhostReactorCreditsRefillPurchased(this.gameId, shinyRocksSpent, finalCredits, num, text);
	}

	public void ResetTelemetryTracking(string newGameId, float timeSinceShiftStart)
	{
		this.gameId = newGameId;
		this.coresCollectedByPlayer = 0;
		this.coresCollectedByGroup = 0;
		this.gatesUnlocked = 0;
		this.deaths = 0;
		this.caughtByAnomaly = false;
		this.itemsPurchased = new List<string>();
		this.levelsUnlocked = new List<string>();
		this.sentientCoresCollected = 0;
		this.vrRigs.Clear();
		VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
		this.maxNumberOfPlayersInShift = this.vrRigs.Count + 1;
		this.timeIntoShiftAtJoin = timeSinceShiftStart;
		this.itemsHeldThisShift.Clear();
		this.itemTypesHeldThisShift.Clear();
	}

	public void ResetGameTelemetryTracking()
	{
		this.totalCoresCollectedByPlayer = 0;
		this.totalCoresCollectedByGroup = 0;
		this.totalGatesUnlocked = 0;
		this.totalDeaths = 0;
		this.totalItemsPurchased = new List<string>();
		this.vrRigs.Clear();
		VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
		this.maxNumberOfPlayersIngame = this.vrRigs.Count + 1;
		this.totalItemsHeldThisShift.Clear();
		this.totalItemTypesHeldThisShift.Clear();
		this.numShiftsPlayed = 0;
		this.isFirstShift = false;
	}

	public void IncrementCoresCollectedPlayer(int coreValue)
	{
		this.totalCoresCollectedByPlayer += coreValue;
		this.coresCollectedByPlayer += coreValue;
	}

	public void IncrementCoresCollectedGroup(int coreValue)
	{
		this.totalCoresCollectedByGroup += coreValue;
		this.coresCollectedByGroup += coreValue;
	}

	public void IncrementCoresSpentPlayer(int coreValue)
	{
		this.totalCoresSpentByPlayer += coreValue;
		this.coresSpentByPlayer += coreValue;
	}

	public void IncrementCoresSpentGroup(int coreValue)
	{
		this.totalCoresSpentByGroup += coreValue;
		this.coresSpentByGroup += coreValue;
	}

	public void IncrementChaosSeedsCollected(int numSeeds)
	{
		this.sentientCoresCollected += numSeeds;
	}

	public void IncrementGatesUnlocked(int numGatesUnlocked)
	{
		this.gatesUnlocked += numGatesUnlocked;
		this.totalGatesUnlocked += numGatesUnlocked;
	}

	public void IncrementDeaths(int numDeaths)
	{
		this.deaths += numDeaths;
		this.totalDeaths += numDeaths;
	}

	public void IncrementRevives(int numRevives)
	{
		this.revives += numRevives;
		this.totalRevives += numRevives;
	}

	public void IncrementShiftsPlayed(int numShifts)
	{
		this.numShiftsPlayed += numShifts;
	}

	public void AddItemPurchased(string newItemPurchased)
	{
		this.itemsPurchased.Add(newItemPurchased);
		this.totalItemsPurchased.Add(newItemPurchased);
	}

	public void GrabbedItem(GameEntityId id, string itemName)
	{
		if (this.itemsHeldThisShift.Contains(id))
		{
			return;
		}
		this.itemsHeldThisShift.Add(id);
		if (this.itemTypesHeldThisShift.ContainsKey(itemName))
		{
			this.itemTypesHeldThisShift[itemName] = this.itemTypesHeldThisShift[itemName] + 1;
		}
		else
		{
			this.itemTypesHeldThisShift[itemName] = 1;
		}
		if (this.totalItemsHeldThisShift.Contains(id))
		{
			return;
		}
		this.totalItemsHeldThisShift.Add(id);
		if (this.totalItemTypesHeldThisShift.ContainsKey(itemName))
		{
			this.totalItemTypesHeldThisShift[itemName] = this.totalItemTypesHeldThisShift[itemName] + 1;
			return;
		}
		this.totalItemTypesHeldThisShift[itemName] = 1;
	}

	public GRShuttle GetAssignedShuttle(bool isOnDrillovator)
	{
		GhostReactor instance = GhostReactor.instance;
		GRShuttle drillShuttleForPlayer = GRElevatorManager._instance.GetDrillShuttleForPlayer(this.gamePlayer.rig.OwningNetPlayer.ActorNumber);
		GRShuttle stagingShuttleForPlayer = GRElevatorManager._instance.GetStagingShuttleForPlayer(this.gamePlayer.rig.OwningNetPlayer.ActorNumber);
		if (!isOnDrillovator)
		{
			return stagingShuttleForPlayer;
		}
		return drillShuttleForPlayer;
	}

	public void RefreshShuttles()
	{
		GRShuttle grshuttle = this.GetAssignedShuttle(true);
		if (grshuttle != null)
		{
			grshuttle.Refresh();
		}
		grshuttle = this.GetAssignedShuttle(false);
		if (grshuttle != null)
		{
			grshuttle.Refresh();
		}
	}

	public static GRPlayer GetFromUserId(string userId)
	{
		GRPlayer.tempRigs.Clear();
		GRPlayer.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GRPlayer.tempRigs);
		for (int i = 0; i < GRPlayer.tempRigs.Count; i++)
		{
			if (GRPlayer.tempRigs[i].OwningNetPlayer != null && GRPlayer.tempRigs[i].OwningNetPlayer.UserId == userId)
			{
				return GRPlayer.Get(GRPlayer.tempRigs[i].OwningNetPlayer);
			}
		}
		return null;
	}

	[ContextMenu("Refresh Damage Vignette Visual")]
	public void RefreshDamageVignetteVisual()
	{
		if (this.vrRig.isLocal && this.currentHealthVisualValue != this.hp)
		{
			this.currentHealthVisualValue = this.hp;
			if (this.hp <= this.damageOverlayMaxHp && this.hp > 0)
			{
				if (this.lowHeathVisualCoroutine != null)
				{
					base.StopCoroutine(this.lowHeathVisualCoroutine);
				}
				this.damageEffects.lowHealthVisualRenderer.gameObject.SetActive(true);
				this.lowHeathVisualCoroutine = base.StartCoroutine(this.LowHeathVisualCoroutine());
				return;
			}
			this.damageEffects.lowHealthVisualRenderer.gameObject.SetActive(false);
		}
	}

	private IEnumerator LowHeathVisualCoroutine()
	{
		int index = this.hp - 1;
		if (index >= 0 && index < this.damageOverlayValues.Count)
		{
			float startTime = Time.time;
			while (Time.time - startTime < this.damageOverlayValues[index].effectDuration)
			{
				float num = Mathf.Clamp01((Time.time - startTime) / this.damageOverlayValues[index].effectDuration);
				float num2 = this.damageOverlayValues[index].effectCurve.Evaluate(num);
				Color tint = this.damageOverlayValues[index].tint;
				tint.a *= num2;
				this.damageEffects.lowHealthVisualRenderer.GetPropertyBlock(this.lowHealthVisualPropertyBlock);
				this.lowHealthVisualPropertyBlock.SetColor(this.lowHealthTintPropertyId, tint);
				this.damageEffects.lowHealthVisualRenderer.SetPropertyBlock(this.lowHealthVisualPropertyBlock);
				yield return null;
			}
		}
		yield break;
	}

	public void SetGooParticleSystemEnabled(bool bIsLeftHand, bool newEnableState)
	{
		if (this.vrRig != null)
		{
			this.vrRig.SetGooParticleSystemStatus(bIsLeftHand, newEnableState);
		}
	}

	public void SetAsFrozen(float duration)
	{
		if (GorillaTagger.Instance.currentStatus != GorillaTagger.StatusEffect.Frozen)
		{
			this.freezeDuration = duration;
			if (this.gamePlayer.rig.OwningNetPlayer.IsLocal)
			{
				GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Frozen, duration);
				GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
				GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
				GorillaTagger.Instance.offlineVRRig.PlayTaggedEffect();
				if (this.damageEffects.frozenVisualRenderer != null)
				{
					this.damageEffects.frozenVisualRenderer.gameObject.SetActive(true);
				}
				this.playerDamageAudioSource.PlayOneShot(this.playerFrozenSound, 1f);
			}
			this.gamePlayer.rig.UpdateFrozenEffect(true);
			base.Invoke("RemoveFrozen", duration);
		}
	}

	public void RemoveFrozen()
	{
		this.gamePlayer.rig.UpdateFrozenEffect(false);
		this.freezeDuration = 0f;
		if (this.damageEffects.frozenVisualRenderer != null)
		{
			this.damageEffects.frozenVisualRenderer.gameObject.SetActive(false);
		}
	}

	public override void Tick()
	{
		if (this.lastPlayerPosition != Vector3.zero)
		{
			Vector3 position = this.vrRig.transform.position;
			float magnitude = (this.lastPlayerPosition - position).magnitude;
			this.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.DistanceTraveled, magnitude);
		}
		this.lastPlayerPosition = this.vrRig.transform.position;
		if (this.freezeDuration > 0f)
		{
			this.gamePlayer.rig.UpdateFrozen(Time.deltaTime, this.freezeDuration);
		}
		if (this.inStealthMode && Time.timeAsDouble > this.shieldStealthModeEndTime)
		{
			this.ClearStealthMode();
		}
		GRShuttle.UpdateGRPlayerShuttle(this);
		if (this.soak != null && this.soak.IsSoaking())
		{
			this.soak.OnUpdate();
		}
	}

	public void SetSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat stat, float amt)
	{
		this.synchronizedSessionStats[(int)stat] = amt;
	}

	public void IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat stat, float amt)
	{
		this.synchronizedSessionStats[(int)stat] += amt;
	}

	public void ResetSynchronizedSessionStats()
	{
		for (int i = 0; i < 8; i++)
		{
			this.synchronizedSessionStats[i] = 0f;
		}
	}

	public bool IsDropPodUnlocked()
	{
		return this.dropPodLevel > 0;
	}

	public int GetMaxDropFloor()
	{
		switch (this.dropPodChasisLevel + this.dropPodLevel)
		{
		case 0:
			return 1;
		case 1:
			return 5;
		case 2:
			return 10;
		case 3:
			return 15;
		case 4:
			return 20;
		default:
			return 0;
		}
	}

	public void CollectShiftCut()
	{
		this.SetProgressionData(this.currentProgression.points + this.LastShiftCut, this.currentProgression.redeemedPoints, true);
	}

	public bool AttemptPromotion()
	{
		ValueTuple<int, int, int, int> gradePointDetails = GhostReactorProgression.GetGradePointDetails(this.CurrentProgression.redeemedPoints);
		int item = gradePointDetails.Item3;
		int item2 = gradePointDetails.Item4;
		if (item - item2 < this.CurrentProgression.points - this.CurrentProgression.redeemedPoints)
		{
			this.SetProgressionData(this.currentProgression.points, this.currentProgression.points, false);
			return true;
		}
		return false;
	}

	public void SetProgressionData(int _points, int _redeemedPoints, bool saveProgression = false)
	{
		if (_points < 0 || _redeemedPoints < 0)
		{
			return;
		}
		this.currentProgression = new GRPlayer.ProgressionData
		{
			points = _points,
			redeemedPoints = _redeemedPoints
		};
		if (this.gamePlayer.IsLocal() && saveProgression)
		{
			this.SaveMyProgression();
		}
	}

	public void LoadMyProgression()
	{
		GhostReactorProgression.instance.GetStartingProgression(this);
	}

	public void SaveMyProgression()
	{
		GhostReactorProgression.instance.SetProgression(this.LastShiftCut, this);
	}

	public const int MAX_CURRENCY = 500;

	public GamePlayer gamePlayer;

	private GRPlayer.GRPlayerState state;

	private int shiftCreditCache;

	public int startingShiftCreditCache;

	public int playerJuice;

	public double shiftJoinTime;

	public bool isEmployee;

	public AudioSource audioSource;

	[Header("Hit / Revive Effects")]
	public ParticleSystem playerTurnedGhostEffect;

	public SoundBankPlayer playerTurnedGhostSoundBank;

	public ParticleSystem playerRevivedEffect;

	public AudioClip playerRevivedSound;

	public float playerRevivedVolume = 1f;

	public AudioSource playerDamageAudioSource;

	public Transform bodyCenter;

	public ParticleSystem playerDamageEffect;

	public float playerDamageVolume = 1f;

	public AudioClip playerDamageSound;

	public float playerDamageOffsetDist = 0.25f;

	[ColorUsage(true, true)]
	[SerializeField]
	private Color deathTintColor;

	[ColorUsage(true, true)]
	[SerializeField]
	private Color deathAmbientLightColor;

	public GameLight shieldGameLight;

	[Header("Attach")]
	public Transform attachEnemy;

	[Header("Shield")]
	public Transform shieldHeadVisual;

	public Transform shieldBodyVisual;

	public AudioClip shieldActivatedSound;

	public float shieldActivatedVolume = 0.5f;

	public ParticleSystem shieldDamagedEffect;

	public AudioClip shieldDamagedSound;

	public float shieldDamagedVolume = 0.5f;

	public ParticleSystem shieldDestroyedEffect;

	public AudioClip shieldDestroyedSound;

	public float shieldDestroyedVolume = 0.5f;

	public float shieldStealthModeDuration = 20f;

	private double shieldStealthModeEndTime;

	public Color shieldColorNormal = new Color(0.42352942f, 0.25490198f, 1f, 0.45490196f);

	public Color shieldColorLight = new Color(1f, 1f, 1f, 0.5f);

	public Color shieldColorStealth = new Color(1f, 0.2f, 0f, 0.5f);

	public Color shieldColorHeal = new Color(0f, 1f, 1f, 0.5f);

	public int xRayVisionRefCount;

	[Header("Badge")]
	public Transform badgeBodyAnchor;

	[SerializeField]
	private Transform badgeBodyStringAttach;

	[NonSerialized]
	public double lastLeftWithBadgeAttachedTime;

	[Header("Health")]
	[SerializeField]
	private int maxHp = 1;

	[SerializeField]
	private int maxShieldHp = 1;

	public string mothershipId;

	private int hp;

	private int shieldHp;

	private int shieldFlags;

	private bool inStealthMode;

	[Header("Damage Vignette")]
	[SerializeField]
	[Tooltip("First entry is 1 hp, second entry is 2 hp, etc.")]
	private List<GRPlayer.DamageOverlayValues> damageOverlayValues = new List<GRPlayer.DamageOverlayValues>();

	[SerializeField]
	private int damageOverlayMaxHp = 1;

	[HideInInspector]
	public GRBadge badge;

	public CallLimiter requestCollectItemLimiter;

	public CallLimiter requestChargeToolLimiter;

	public CallLimiter requestDepositCurrencyLimiter;

	public CallLimiter requestShiftStartLimiter;

	public CallLimiter requestToolPurchaseStationLimiter;

	public CallLimiter applyEnemyHitLimiter;

	public CallLimiter reportLocalHitLimiter;

	public CallLimiter reportBreakableBrokenLimiter;

	public CallLimiter playerStateChangeLimiter;

	public CallLimiter promotionBotLimiter;

	public CallLimiter progressionBroadcastLimiter;

	public CallLimiter scoreboardPageLimiter;

	public CallLimiter fireShieldLimiter;

	private VRRig vrRig;

	private List<VRRig> vrRigs = new List<VRRig>();

	private string gameId;

	public int coresCollectedByPlayer;

	public int coresCollectedByGroup;

	public int coresSpentByPlayer;

	public int coresSpentByGroup;

	public int gatesUnlocked;

	public int deaths;

	public bool caughtByAnomaly;

	public List<string> itemsPurchased;

	public List<string> levelsUnlocked;

	public float timeIntoShiftAtJoin;

	public bool wasPlayerInAtShiftStart;

	public int sentientCoresCollected;

	public int maxNumberOfPlayersInShift;

	public int revives;

	public float[] synchronizedSessionStats = new float[8];

	private HashSet<GameEntityId> itemsHeldThisShift = new HashSet<GameEntityId>();

	private Dictionary<string, int> itemTypesHeldThisShift = new Dictionary<string, int>();

	public int totalCoresCollectedByPlayer;

	public int totalCoresCollectedByGroup;

	public int totalCoresSpentByPlayer;

	public int totalCoresSpentByGroup;

	public int totalGatesUnlocked;

	public int totalDeaths;

	public List<string> totalItemsPurchased;

	public float timeIntoGameAtJoin;

	public bool wasPlayerInAtGameStart;

	public int maxNumberOfPlayersIngame;

	public int totalRevives;

	public int numShiftsPlayed;

	public float gameStartTime;

	public bool isFirstShift = true;

	private HashSet<GameEntityId> totalItemsHeldThisShift = new HashSet<GameEntityId>();

	private Dictionary<string, int> totalItemTypesHeldThisShift = new Dictionary<string, int>();

	private GRPlayerDamageEffects damageEffects;

	private MaterialPropertyBlock lowHealthVisualPropertyBlock;

	private int lowHealthTintPropertyId;

	private int currentHealthVisualValue;

	private Coroutine lowHeathVisualCoroutine;

	public AudioClip playerFrozenSound;

	public GRPlayer.ShuttleData shuttleData;

	private GRPlayer.ProgressionData currentProgression;

	private float shiftPlayTime;

	private int lastShiftCut;

	private GhostReactorSoak soak;

	private static List<VRRig> tempRigs = new List<VRRig>(32);

	private float freezeDuration;

	private Vector3 lastPlayerPosition = Vector3.zero;

	public int dropPodLevel;

	public int dropPodChasisLevel;

	public enum GRPlayerState
	{
		Alive,
		Ghost,
		Shielded
	}

	public enum GRPlayerShieldFlags
	{
		Light = 1,
		Stealth,
		Heal = 4
	}

	public enum SynchronizedSessionStat
	{
		CoresDeposited,
		EarnedCredits,
		SpentCredits,
		DistanceTraveled,
		Deaths,
		Kills,
		Assists,
		TimeChaosExposure,
		Count
	}

	[Serializable]
	private struct DamageOverlayValues
	{
		public Color tint;

		public float effectDuration;

		public AnimationCurve effectCurve;
	}

	public enum ShuttleState
	{
		Idle,
		Moving,
		WaitForLeaveRoom,
		JoinRoom,
		WaitForLeadPlayer,
		Teleport,
		TeleportToMyShuttleSafety,
		PostTeleport
	}

	public class ShuttleData
	{
		public string ownerUserId;

		public int currShuttleId;

		public int targetShuttleId;

		public int targetLevel;

		public GRPlayer.ShuttleState state;

		public double stateStartTime;
	}

	[Serializable]
	public struct ProgressionData
	{
		public int points;

		public int redeemedPoints;
	}

	[Serializable]
	public struct ProgressionLevels
	{
		public int tierId;

		public string tierName;

		public int grades;

		public int pointsPerGrade;
	}
}
