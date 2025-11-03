using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Fusion;
using GorillaTag.Rendering;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;

public class GhostReactor : MonoBehaviourTick, IBuildValidation
{
	public static GhostReactor Get(GameEntity gameEntity)
	{
		GhostReactorManager ghostReactorManager = GhostReactorManager.Get(gameEntity);
		if (ghostReactorManager == null)
		{
			return null;
		}
		return ghostReactorManager.reactor;
	}

	private void Awake()
	{
		GhostReactor.instance = this;
		this.reviveStations = new List<GRReviveStation>();
		base.GetComponentsInChildren<GRReviveStation>(this.reviveStations);
		for (int i = 0; i < this.reviveStations.Count; i++)
		{
			this.reviveStations[i].Init(this, i);
		}
		this.vrRigs = new List<VRRig>();
		for (int j = 0; j < this.itemPurchaseStands.Count; j++)
		{
			if (this.itemPurchaseStands[j] == null)
			{
				Debug.LogErrorFormat("Null Item Purchase Stand {0}", new object[] { j });
			}
			else
			{
				this.itemPurchaseStands[j].Setup(j);
			}
		}
		for (int k = 0; k < this.toolPurchasingStations.Count; k++)
		{
			if (this.toolPurchasingStations[k] == null)
			{
				Debug.LogErrorFormat("Null Tool Purchasing Station {0}", new object[] { k });
			}
			else
			{
				this.toolPurchasingStations[k].PurchaseStationId = k;
			}
		}
		if (this.promotionBot != null)
		{
			this.promotionBot.Init(this);
		}
		this.randomGenerator = new SRand(Random.Range(0, int.MaxValue));
		this.handPrintMPB = new MaterialPropertyBlock();
		this.handPrintMPB.SetFloatArray("_HandPrintData", new float[1024]);
		this.bays = new List<GRBay>(32);
		base.GetComponentsInChildren<GRBay>(false, this.bays);
		this.storeDisplays = new List<GRUIStoreDisplay>();
		base.GetComponentsInChildren<GRUIStoreDisplay>(false, this.storeDisplays);
	}

	private new void OnEnable()
	{
		base.OnEnable();
		if (this.zone == GTZone.customMaps)
		{
			return;
		}
		GTDev.Log<string>(string.Format("GhostReactor::OnEnable getting manager for zone {0}", this.zone), null);
		GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this.zone);
		if (managerForZone == null)
		{
			Debug.LogErrorFormat("No GameEntityManager found for zone {0}", new object[] { this.zone });
			return;
		}
		this.grManager = managerForZone.ghostReactorManager;
		if (this.grManager == null)
		{
			Debug.LogErrorFormat("No GhostReactorManager found for zone {0}", new object[] { this.zone });
			return;
		}
		this.grManager.reactor = this;
		this.grManager.gameEntityManager.zoneLimit = this.zoneLimit;
		if (GameLightingManager.instance != null && this.zone != GTZone.customMaps)
		{
			GameLightingManager.instance.ZoneEnableCustomDynamicLighting(true);
		}
		VRRigCache.OnRigActivated += this.OnVRRigsChanged;
		VRRigCache.OnRigDeactivated += this.OnVRRigsChanged;
		VRRigCache.OnRigNameChanged += this.OnVRRigsChanged;
		if (NetworkSystem.Instance != null)
		{
			NetworkSystem.Instance.OnMultiplayerStarted += this.OnLocalPlayerConnectedToRoom;
		}
		for (int i = 0; i < this.toolPurchasingStations.Count; i++)
		{
			this.toolPurchasingStations[i].Init(this.grManager, this);
		}
		if (this.debugUpgradeKiosk != null)
		{
			this.debugUpgradeKiosk.Init(this.grManager, this);
		}
		if (this.currencyDepositor != null)
		{
			this.currencyDepositor.Init(this);
		}
		if (this.distillery != null)
		{
			this.distillery.Init(this);
		}
		if (this.seedExtractor != null)
		{
			this.seedExtractor.Init(this.toolProgression, this);
		}
		if (this.levelGenerator != null)
		{
			this.levelGenerator.Init(this);
		}
		if (this.employeeBadges != null)
		{
			this.employeeBadges.Init(this);
		}
		if (this.toolProgression != null)
		{
			this.toolProgression.Init(this);
			this.toolProgression.OnProgressionUpdated += this.OnProgressionUpdated;
		}
		if (this.shiftManager != null)
		{
			this.shiftManager.Init(this.grManager);
		}
		for (int j = 0; j < this.toolUpgradePurchaseStationsFull.Count; j++)
		{
			this.toolUpgradePurchaseStationsFull[j].Init(this.toolProgression, this);
		}
		GRElevatorManager._instance.InitShuttles(this);
		if (this.recycler != null)
		{
			this.recycler.Init(this);
		}
		if (this.zoneShaderSettings != null)
		{
			this.zoneShaderSettings.BecomeActiveInstance(true);
		}
		for (int k = 0; k < this.bays.Count; k++)
		{
			this.bays[k].Setup(this);
		}
		for (int l = 0; l < this.storeDisplays.Count; l++)
		{
			this.storeDisplays[l].Setup(-1, this);
		}
		this.RefreshDepth();
	}

	public void EnableGhostReactorForVirtualStump()
	{
		GhostReactor.instance = this;
		this.RefreshReviveStations(false);
		this.OnEnable();
	}

	public void RefreshReviveStations(bool searchScene = false)
	{
		this.reviveStations = new List<GRReviveStation>();
		base.GetComponentsInChildren<GRReviveStation>(this.reviveStations);
		if (searchScene)
		{
			this.reviveStations.AddRange(Object.FindObjectsByType<GRReviveStation>(FindObjectsInactive.Include, FindObjectsSortMode.None));
		}
		for (int i = 0; i < this.reviveStations.Count; i++)
		{
			this.reviveStations[i].Init(this, i);
		}
	}

	private new void OnDisable()
	{
		base.OnDisable();
		if (this.zone == GTZone.customMaps)
		{
			return;
		}
		GameLightingManager.instance.ZoneEnableCustomDynamicLighting(false);
		VRRigCache.OnRigActivated -= this.OnVRRigsChanged;
		VRRigCache.OnRigDeactivated -= this.OnVRRigsChanged;
		VRRigCache.OnRigNameChanged -= this.OnVRRigsChanged;
		if (this.toolProgression != null)
		{
			this.toolProgression.OnProgressionUpdated -= this.OnProgressionUpdated;
		}
		if (NetworkSystem.Instance != null)
		{
			NetworkSystem.Instance.OnMultiplayerStarted -= this.OnLocalPlayerConnectedToRoom;
		}
	}

	private void OnProgressionUpdated()
	{
		if (this.toolProgression != null)
		{
			this.UpdateLocalPlayerFromProgression();
		}
	}

	public void UpdateLocalPlayerFromProgression()
	{
		GRPlayer local = GRPlayer.GetLocal();
		if (local != null)
		{
			int dropPodLevel = this.toolProgression.GetDropPodLevel();
			if (local.dropPodLevel != dropPodLevel)
			{
				local.dropPodLevel = dropPodLevel;
				Debug.LogFormat("Drop Pod UpdateLocalPlayerFromProgression Level {0} {1} {2}", new object[]
				{
					this.grManager.IsZoneActive(),
					local.dropPodLevel,
					local.dropPodChasisLevel
				});
				if (this.grManager.IsZoneActive())
				{
					this.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SetPodLevel, dropPodLevel);
				}
			}
			int dropPodChasisLevel = this.toolProgression.GetDropPodChasisLevel();
			if (local.dropPodChasisLevel != dropPodChasisLevel)
			{
				local.dropPodChasisLevel = dropPodChasisLevel;
				Debug.LogFormat("Drop Pod UpdateLocalPlayerFromProgression Level {0} {1} {2}", new object[]
				{
					this.grManager.IsZoneActive(),
					local.dropPodLevel,
					local.dropPodChasisLevel
				});
				if (this.grManager.IsZoneActive())
				{
					this.grManager.RequestPlayerAction(GhostReactorManager.GRPlayerAction.SetPodChassisLevel, dropPodChasisLevel);
				}
			}
			if (local.badge)
			{
				local.badge.RefreshText(PhotonNetwork.LocalPlayer);
			}
			this.RefreshStore();
		}
	}

	public GRPatrolPath GetPatrolPath(long createData)
	{
		if (this.levelGenerator == null)
		{
			return null;
		}
		return this.levelGenerator.GetPatrolPath(createData);
	}

	public override void Tick()
	{
		if (this.grManager == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (this.grManager.gameEntityManager.IsAuthority())
		{
			if (Time.timeAsDouble - this.lastCollectibleDispenserUpdateTime > (double)this.collectibleDispenserUpdateFrequency)
			{
				this.lastCollectibleDispenserUpdateTime = Time.timeAsDouble;
				for (int i = 0; i < this.collectibleDispensers.Count; i++)
				{
					if (this.collectibleDispensers[i] != null && this.collectibleDispensers[i].ReadyToDispenseNewCollectible)
					{
						this.collectibleDispensers[i].RequestDispenseCollectible();
					}
				}
			}
			if (this.sleepableEntities.Count > 0)
			{
				this.sentientCoreUpdateIndex = Mathf.Max(0, this.sentientCoreUpdateIndex % this.sleepableEntities.Count);
				if (this.sentientCoreUpdateIndex < this.sleepableEntities.Count)
				{
					IGRSleepableEntity igrsleepableEntity = this.sleepableEntities[this.sentientCoreUpdateIndex];
					float num = igrsleepableEntity.WakeUpRadius * igrsleepableEntity.WakeUpRadius;
					float num2 = (igrsleepableEntity.WakeUpRadius + 0.5f) * (igrsleepableEntity.WakeUpRadius + 0.5f);
					bool flag = false;
					bool flag2 = false;
					for (int j = 0; j < this.vrRigs.Count; j++)
					{
						GRPlayer component = this.vrRigs[j].GetComponent<GRPlayer>();
						if (!(component == null) && component.State != GRPlayer.GRPlayerState.Ghost)
						{
							float sqrMagnitude = (igrsleepableEntity.Position - this.vrRigs[j].bodyTransform.position).sqrMagnitude;
							if (sqrMagnitude < num2)
							{
								flag = true;
							}
							if (sqrMagnitude < num)
							{
								flag2 = true;
								break;
							}
						}
					}
					bool flag3 = igrsleepableEntity.IsSleeping();
					if (flag3 && flag2)
					{
						igrsleepableEntity.WakeUp();
					}
					else if (!flag3 && !flag)
					{
						igrsleepableEntity.Sleep();
					}
					this.sentientCoreUpdateIndex++;
				}
			}
		}
		bool flag4 = false;
		foreach (GhostReactor.EntityTypeRespawnTracker entityTypeRespawnTracker in this.respawnQueue)
		{
			entityTypeRespawnTracker.entityNextRespawnTime -= Time.deltaTime;
			if (entityTypeRespawnTracker.entityNextRespawnTime < 0f)
			{
				entityTypeRespawnTracker.entityNextRespawnTime = 0f;
				flag4 = true;
				if (this.grManager.gameEntityManager.IsAuthority())
				{
					this.levelGenerator.RespawnEntity(entityTypeRespawnTracker.entityTypeID, entityTypeRespawnTracker.entityCreateData);
				}
			}
		}
		if (flag4)
		{
			this.respawnQueue.RemoveAll((GhostReactor.EntityTypeRespawnTracker e) => e.entityNextRespawnTime <= 0f);
		}
		this.UpdateHandprints(Time.deltaTime);
	}

	private void OnLocalPlayerConnectedToRoom()
	{
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (grplayer != null)
		{
			grplayer.Reset();
		}
		if (this.shiftManager != null)
		{
			this.shiftManager.shiftStats.ResetShiftStats();
			this.shiftManager.RefreshShiftStatsDisplay();
		}
	}

	private void OnVRRigsChanged(RigContainer container)
	{
		this.VRRigRefresh();
	}

	public void VRRigRefresh()
	{
		if (this.isRefreshing)
		{
			return;
		}
		this.isRefreshing = true;
		this.vrRigs.Clear();
		this.vrRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(this.vrRigs);
		this.vrRigs.Sort(delegate(VRRig a, VRRig b)
		{
			if (a == null || a.OwningNetPlayer == null)
			{
				return 1;
			}
			if (b == null || b.OwningNetPlayer == null)
			{
				return -1;
			}
			return a.OwningNetPlayer.ActorNumber.CompareTo(b.OwningNetPlayer.ActorNumber);
		});
		if (this.promotionBot != null)
		{
			this.promotionBot.Refresh();
		}
		this.RefreshScoreboards();
		this.RefreshDepth();
		this.RefreshStore();
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (grplayer != null && this.vrRigs.Count > grplayer.maxNumberOfPlayersInShift)
		{
			grplayer.maxNumberOfPlayersInShift = this.vrRigs.Count;
		}
		this.isRefreshing = false;
	}

	public void UpdateScoreboardScreen(GRUIScoreboard.ScoreboardScreen newScreen)
	{
		for (int i = 0; i < this.scoreboards.Count; i++)
		{
			this.scoreboards[i].SwitchToScreen(newScreen);
		}
		this.RefreshScoreboards();
	}

	public void RefreshScoreboards()
	{
		for (int i = 0; i < this.scoreboards.Count; i++)
		{
			if (!(this.scoreboards[i] == null))
			{
				this.scoreboards[i].Refresh(this.vrRigs);
				if (this.shiftManager != null)
				{
					if (this.shiftManager.ShiftActive)
					{
						this.scoreboards[i].total.text = "-AWAITING SHIFT END-";
					}
					else if (this.shiftManager.ShiftTotalEarned < 0)
					{
						this.scoreboards[i].total.text = "-SHIFT NOT ACTIVE-";
					}
					else
					{
						this.scoreboards[i].total.text = this.shiftManager.ShiftTotalEarned.ToString();
					}
				}
			}
		}
	}

	public int GetItemCost(int entityTypeId)
	{
		int num;
		if (!this.grManager.gameEntityManager.PriceLookup(entityTypeId, out num))
		{
			return 100;
		}
		return num;
	}

	public void UpdateRemoteScoreboardScreen(GRUIScoreboard.ScoreboardScreen scoreboardPage)
	{
		GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this.zone);
		if (managerForZone != null && managerForZone.ghostReactorManager != null)
		{
			managerForZone.ghostReactorManager.photonView.RPC("BroadcastScoreboardPage", RpcTarget.Others, new object[] { scoreboardPage });
		}
	}

	public void SetNextDelveDepth(int newLevel, int newDepthConfigIndex)
	{
		this.depthLevel = newLevel;
		this.depthLevel = Mathf.Clamp(this.depthLevel, 0, this.levelGenerator.depthConfigs.Count);
		if (this.depthLevel >= 0 && this.zone == GTZone.ghostReactorDrill && PhotonNetwork.InRoom && !NetworkSystem.Instance.SessionIsPrivate && this.grManager.IsAuthority())
		{
			int joinDepthSectionFromLevel = GhostReactor.GetJoinDepthSectionFromLevel(this.depthLevel);
			Hashtable hashtable = new Hashtable { 
			{
				"ghostReactorDepth",
				joinDepthSectionFromLevel.ToString()
			} };
			Debug.LogFormat("GR Room Param Set {0} {1}", new object[]
			{
				"ghostReactorDepth",
				hashtable["ghostReactorDepth"]
			});
			PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable, null, null);
		}
		this.depthConfigIndex = newDepthConfigIndex;
	}

	public static int GetJoinDepthSectionFromLevel(int depthLevel)
	{
		if (depthLevel < 4)
		{
			return 0;
		}
		if (depthLevel < 10)
		{
			return 1;
		}
		if (depthLevel < 15)
		{
			return 2;
		}
		if (depthLevel < 20)
		{
			return 3;
		}
		if (depthLevel < 25)
		{
			return 5;
		}
		return 6;
	}

	public void DelveToNextDepth()
	{
		if (this.shiftManager != null)
		{
			this.shiftManager.authorizedToDelveDeeper = false;
		}
		this.RefreshDepth();
	}

	public int PickLevelConfigForDepth(int depthLevel)
	{
		if (this.zone == GTZone.customMaps)
		{
			return 0;
		}
		GhostReactorLevelDepthConfig depthLevelConfig = this.GetDepthLevelConfig(depthLevel);
		int num = 0;
		for (int i = 0; i < depthLevelConfig.options.Count; i++)
		{
			num += depthLevelConfig.options[i].weight;
		}
		int num2 = Random.Range(0, num + 1);
		for (int j = 0; j < depthLevelConfig.options.Count; j++)
		{
			if (depthLevelConfig.options[j].weight >= num2)
			{
				return j;
			}
			num2 -= depthLevelConfig.options[j].weight;
		}
		return 0;
	}

	public void RefreshDepth()
	{
		if (this.shiftManager != null)
		{
			this.shiftManager.RefreshDepthDisplay();
		}
		this.RefreshBays();
	}

	public int GetDepthLevel()
	{
		return this.depthLevel;
	}

	public int GetDepthConfigIndex()
	{
		return this.depthConfigIndex;
	}

	public GhostReactorLevelDepthConfig GetDepthLevelConfig(int level)
	{
		if (this.levelGenerator == null)
		{
			return null;
		}
		level = Mathf.Clamp(level, 0, this.levelGenerator.depthConfigs.Count - 1);
		return this.levelGenerator.depthConfigs[level];
	}

	public GhostReactorLevelGenConfig GetCurrLevelGenConfig()
	{
		if (this.levelGenerator == null)
		{
			return null;
		}
		int num = this.GetDepthLevel();
		num = Mathf.Clamp(num, 0, this.levelGenerator.depthConfigs.Count - 1);
		this.depthConfigIndex = Mathf.Clamp(this.depthConfigIndex, 0, this.levelGenerator.depthConfigs[num].options.Count - 1);
		return this.levelGenerator.depthConfigs[num].options[this.depthConfigIndex].levelConfig;
	}

	public void RefreshStore()
	{
		for (int i = 0; i < this.storeDisplays.Count; i++)
		{
			this.storeDisplays[i].Setup(PhotonNetwork.LocalPlayer.ActorNumber, this);
		}
	}

	public void RefreshBays()
	{
		for (int i = 0; i < this.bays.Count; i++)
		{
			this.bays[i].Refresh();
		}
	}

	public void UpdateHandprints(float deltaTime)
	{
		int num = this.handPrintData.Count - 1000;
		if (num > 0)
		{
			this.handPrintData.RemoveRange(0, num);
			this.handPrintLocations.RemoveRange(0, num);
		}
		float time = Time.time;
		int i = this.handPrintData.Count - 1;
		while (i >= 0)
		{
			this.handPrintData[i] = this.handPrintData[i] - deltaTime;
			if (i + this.handPrintCombineTestDelta >= this.handPrintData.Count)
			{
				goto IL_013E;
			}
			if (this.handPrintData[i + this.handPrintCombineTestDelta] <= this.handPrintFadeTime - 3f)
			{
				Matrix4x4 matrix4x = this.handPrintLocations[i];
				Matrix4x4 matrix4x2 = this.handPrintLocations[i + this.handPrintCombineTestDelta];
				Vector3 vector = new Vector3(matrix4x.m03 - matrix4x2.m03, matrix4x.m13 - matrix4x2.m13, matrix4x.m23 - matrix4x2.m23);
				if (vector.sqrMagnitude < this.handPrintScale * this.handPrintScale)
				{
					List<float> list = this.handPrintData;
					int num2 = i;
					list[num2] -= deltaTime * (float)this.handPrintData.Count * 50f;
					goto IL_013E;
				}
				goto IL_013E;
			}
			IL_0169:
			i--;
			continue;
			IL_013E:
			if (this.handPrintData[i] < 0f)
			{
				this.handPrintData.RemoveAt(i);
				this.handPrintLocations.RemoveAt(i);
				goto IL_0169;
			}
			goto IL_0169;
		}
		if (this.handPrintData.Count > 0)
		{
			this.handPrintCombineTestDelta = (this.handPrintCombineTestDelta + 1) % this.handPrintData.Count;
			if (this.handPrintCombineTestDelta == 0)
			{
				this.handPrintCombineTestDelta = 1;
			}
		}
		else
		{
			this.handPrintCombineTestDelta = 1;
		}
		if (this.handPrintMaterial != null)
		{
			this.handPrintMaterial.SetFloat("_FadeDuration", this.handPrintFadeTime);
			this.handPrintMaterial.enableInstancing = true;
		}
		int num3 = Mathf.Min(Math.Min(1000, 1023), this.handPrintLocations.Count);
		if (num3 > 0)
		{
			this.handPrintMPB.Clear();
			this.handPrintMPB.SetFloatArray("_HandPrintData", this.handPrintData.GetRange(0, num3));
			this.handPrintMPB.SetFloat("_FadeDuration", this.handPrintFadeTime);
			RenderParams renderParams = new RenderParams(this.handPrintMaterial)
			{
				shadowCastingMode = ShadowCastingMode.Off,
				receiveShadows = false,
				layer = base.gameObject.layer,
				matProps = this.handPrintMPB,
				worldBounds = new Bounds(Vector3.zero, Vector3.one * 2000f)
			};
			Graphics.RenderMeshInstanced<Matrix4x4>(in renderParams, this.handPrintMesh, 0, this.handPrintLocations.GetRange(0, num3), -1, 0);
		}
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (grplayer != null)
		{
			if (Time.time - this.handPrintTimeLeft >= this.handPrintInkTime)
			{
				grplayer.SetGooParticleSystemEnabled(true, false);
			}
			if (Time.time - this.handPrintTimeRight >= this.handPrintInkTime)
			{
				grplayer.SetGooParticleSystemEnabled(false, false);
			}
		}
	}

	public void OnTapLocal(bool isLeftHand, Vector3 pos, Quaternion orient, GorillaSurfaceOverride surfaceOverride)
	{
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (grplayer == null)
		{
			return;
		}
		if (!(surfaceOverride != null) || surfaceOverride.overrideIndex != 79)
		{
			float num = (isLeftHand ? this.handPrintTimeLeft : this.handPrintTimeRight);
			if (Time.time - num < this.handPrintInkTime && (Time.time < this.lastBroadcastHandTapTime || Time.time > this.lastBroadcastHandTapTime + this.broadcastHandTapDelay))
			{
				GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this.zone);
				if (managerForZone != null && managerForZone.ghostReactorManager != null)
				{
					managerForZone.ghostReactorManager.photonView.RPC("BroadcastHandprint", RpcTarget.All, new object[] { pos, orient });
				}
				this.lastBroadcastHandTapTime = Time.time;
			}
			return;
		}
		grplayer.SetGooParticleSystemEnabled(isLeftHand, true);
		if (isLeftHand)
		{
			this.handPrintTimeLeft = Time.time;
			return;
		}
		this.handPrintTimeRight = Time.time;
	}

	public void AddHandprint(Vector3 pos, Quaternion orient)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x.SetTRS(pos, orient * Quaternion.Euler(90f, 0f, 180f), Vector3.one * this.handPrintScale);
		this.handPrintLocations.Add(matrix4x);
		this.handPrintData.Add(this.handPrintFadeTime);
	}

	public void ClearAllHandprints()
	{
		this.handPrintData.Clear();
		this.handPrintLocations.Clear();
	}

	public int NumActivePlayers
	{
		get
		{
			return this.vrRigs.Count;
		}
	}

	public void OnAbilityDie(GameEntity entity)
	{
		GhostReactor.EnemyEntityCreateData enemyEntityCreateData = GhostReactor.EnemyEntityCreateData.Unpack(entity.createData);
		if (enemyEntityCreateData.respawnCount == 0)
		{
			return;
		}
		GhostReactor.EntityTypeRespawnTracker entityTypeRespawnTracker = new GhostReactor.EntityTypeRespawnTracker();
		entityTypeRespawnTracker.entityTypeID = entity.typeId;
		entityTypeRespawnTracker.entityCreateData = enemyEntityCreateData.Pack();
		entityTypeRespawnTracker.entityNextRespawnTime = this.respawnTime;
		this.respawnQueue.Add(entityTypeRespawnTracker);
	}

	public void ClearAllRespawns()
	{
		this.respawnQueue.Clear();
	}

	bool IBuildValidation.BuildValidationCheck()
	{
		return true;
	}

	public static GhostReactor instance;

	public GTZone zone;

	public Transform restartMarker;

	public PhotonView photonView;

	public AudioSource entryRoomAudio;

	public AudioClip entryRoomDeathSound;

	public BoxCollider zoneLimit;

	public BoxCollider safeZoneLimit;

	public List<GhostReactor.TempEnemySpawnInfo> tempSpawnEnemies;

	public GameEntity overrideEnemySpawn;

	public List<GameEntity> tempSpawnItems;

	public Transform tempSpawnItemsMarker;

	public List<GRUIBuyItem> itemPurchaseStands;

	public List<GRToolPurchaseStation> toolPurchasingStations;

	public GRDebugUpgradeKiosk debugUpgradeKiosk;

	public List<GRUIScoreboard> scoreboards;

	public List<GRCollectibleDispenser> collectibleDispensers = new List<GRCollectibleDispenser>();

	public List<IGRSleepableEntity> sleepableEntities = new List<IGRSleepableEntity>();

	private List<GRBay> bays;

	private List<GRUIStoreDisplay> storeDisplays;

	public GRUIStationEmployeeBadges employeeBadges;

	public GRUIEmployeeTerminal employeeTerminal;

	public GhostReactorShiftManager shiftManager;

	public GhostReactorLevelGenerator levelGenerator;

	public GRCurrencyDepositor currencyDepositor;

	public GRSeedExtractor seedExtractor;

	public GRDistillery distillery;

	public GRToolProgressionManager toolProgression;

	public GRToolUpgradeStation upgradeStation;

	public List<GRToolUpgradePurchaseStationFull> toolUpgradePurchaseStationsFull;

	public GRRecycler recycler;

	public List<GhostReactor.EntityTypeRespawnTracker> respawnQueue = new List<GhostReactor.EntityTypeRespawnTracker>();

	public List<float> difficultyScalingPerPlayer = new List<float>(10);

	public float respawnTime = 10f;

	public float respawnMinDistToPlayer = 8f;

	public float difficultyScalingForCurrentFloor = 1f;

	public LayerMask envLayerMask;

	public Material handPrintMaterial;

	public Mesh handPrintMesh;

	public float handPrintScale;

	public float handPrintInkTime = 30f;

	public float handPrintFadeTime = 600f;

	private const int handPrintMaxCount = 1000;

	private List<Matrix4x4> handPrintLocations = new List<Matrix4x4>(1000);

	private List<float> handPrintData = new List<float>(1000);

	private MaterialPropertyBlock handPrintMPB;

	[ReadOnly]
	public List<GRReviveStation> reviveStations;

	public List<GRVendingMachine> vendingMachines;

	public List<VRRig> vrRigs;

	private float collectibleDispenserUpdateFrequency = 3f;

	private double lastCollectibleDispenserUpdateTime = -10.0;

	private int sentientCoreUpdateIndex;

	private SRand randomGenerator;

	[ReadOnly]
	public int depthLevel;

	[ReadOnly]
	public int depthConfigIndex;

	public Dictionary<int, double> playerProgressionData;

	public GRDropZone dropZone;

	public static float DROP_ZONE_REPEL = 2.25f;

	public ZoneShaderSettings zoneShaderSettings;

	public GRUIPromotionBot promotionBot;

	private bool isRefreshing;

	public GhostReactorManager grManager;

	private float handPrintTimeLeft = -1000f;

	private float handPrintTimeRight = -1000f;

	private int handPrintCombineTestDelta = 1;

	private float lastBroadcastHandTapTime;

	private float broadcastHandTapDelay = 0.3f;

	[Serializable]
	public class TempEnemySpawnInfo
	{
		public GameEntity prefab;

		public Transform spawnMarker;

		public int patrolPath;
	}

	public class EntityTypeRespawnTracker
	{
		public int entityTypeID;

		public long entityCreateData;

		public float entityNextRespawnTime;
	}

	public enum EntityGroupTypes
	{
		EnemyChaser,
		EnemyChaserArmored,
		EnemyRanged,
		EnemyRangedArmored,
		CollectibleFlower,
		BarrierEnergyCostGate,
		BarrierSpectralWall,
		HazardSpectralLiquid
	}

	public enum EnemyType
	{
		Chaser,
		Ranged,
		Phantom,
		Environment,
		CustomMapsEnemy
	}

	public struct EnemyEntityCreateData
	{
		private static long PackData(int value, int nbits, int shift)
		{
			return ((long)value & (long)((1 << nbits) - 1)) << shift;
		}

		private static int UnpackData(long createData, int nbits, int shift)
		{
			return (int)((createData >> shift) & (long)((1 << nbits) - 1));
		}

		public static GhostReactor.EnemyEntityCreateData Unpack(long bits)
		{
			return new GhostReactor.EnemyEntityCreateData
			{
				respawnCount = GhostReactor.EnemyEntityCreateData.UnpackData(bits, 8, 16),
				sectionIndex = GhostReactor.EnemyEntityCreateData.UnpackData(bits, 8, 8),
				patrolIndex = GhostReactor.EnemyEntityCreateData.UnpackData(bits, 8, 0)
			};
		}

		public long Pack()
		{
			return GhostReactor.EnemyEntityCreateData.PackData(this.respawnCount, 8, 16) | GhostReactor.EnemyEntityCreateData.PackData(this.sectionIndex, 8, 8) | GhostReactor.EnemyEntityCreateData.PackData(this.patrolIndex, 8, 0);
		}

		public int respawnCount;

		public int sectionIndex;

		public int patrolIndex;
	}

	public struct ToolEntityCreateData
	{
		private static long PackData(int value, int nbits, int shift)
		{
			return ((long)value & (long)((1 << nbits) - 1)) << shift;
		}

		private static int UnpackData(long createData, int nbits, int shift)
		{
			return (int)((createData >> shift) & (long)((1 << nbits) - 1));
		}

		public static GhostReactor.ToolEntityCreateData Unpack(long bits)
		{
			GhostReactor.ToolEntityCreateData toolEntityCreateData = default(GhostReactor.ToolEntityCreateData);
			toolEntityCreateData.stationIndex = GhostReactor.ToolEntityCreateData.UnpackData(bits, 8, 0) - 1;
			int num = GhostReactor.ToolEntityCreateData.UnpackData(bits, 8, 8);
			toolEntityCreateData.decayTime = 5f * (float)num;
			return toolEntityCreateData;
		}

		public long Pack()
		{
			long num = GhostReactor.ToolEntityCreateData.PackData(this.stationIndex + 1, 8, 0);
			GhostReactor.ToolEntityCreateData.PackData((int)(this.decayTime / 5f), 8, 8);
			return num;
		}

		public int stationIndex;

		public float decayTime;
	}
}
