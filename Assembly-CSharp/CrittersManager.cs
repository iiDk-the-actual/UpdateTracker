using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Critters.Scripts;
using Fusion;
using GorillaExtensions;
using GorillaNetworking;
using Photon.Pun;
using PlayFab;
using UnityEngine;
using Utilities;

[NetworkBehaviourWeaved(0)]
public class CrittersManager : NetworkComponent, IRequestableOwnershipGuardCallbacks, IBuildValidation, ITickSystemTick
{
	public static bool hasInstance { get; private set; }

	public bool TickRunning { get; set; }

	public bool allowGrabbingEntireBag
	{
		get
		{
			if (!NetworkSystem.Instance.SessionIsPrivate)
			{
				return (CrittersManager.AllowGrabbingFlags.EntireBag & this.publicRoomGrabbingFlags) > CrittersManager.AllowGrabbingFlags.None;
			}
			return (CrittersManager.AllowGrabbingFlags.EntireBag & this.privateRoomGrabbingFlags) > CrittersManager.AllowGrabbingFlags.None;
		}
	}

	public bool allowGrabbingOutOfHands
	{
		get
		{
			if (!NetworkSystem.Instance.SessionIsPrivate)
			{
				return (CrittersManager.AllowGrabbingFlags.OutOfHands & this.publicRoomGrabbingFlags) > CrittersManager.AllowGrabbingFlags.None;
			}
			return (CrittersManager.AllowGrabbingFlags.OutOfHands & this.privateRoomGrabbingFlags) > CrittersManager.AllowGrabbingFlags.None;
		}
	}

	public bool allowGrabbingFromBags
	{
		get
		{
			if (!NetworkSystem.Instance.SessionIsPrivate)
			{
				return (CrittersManager.AllowGrabbingFlags.FromBags & this.publicRoomGrabbingFlags) > CrittersManager.AllowGrabbingFlags.None;
			}
			return (CrittersManager.AllowGrabbingFlags.FromBags & this.privateRoomGrabbingFlags) > CrittersManager.AllowGrabbingFlags.None;
		}
	}

	public void LoadGrabSettings()
	{
		PlayFabTitleDataCache.Instance.GetTitleData("PublicCrittersGrabSettings", delegate(string data)
		{
			int num;
			if (int.TryParse(data, out num))
			{
				this.publicRoomGrabbingFlags = (CrittersManager.AllowGrabbingFlags)num;
			}
		}, delegate(PlayFabError e)
		{
		}, false);
		PlayFabTitleDataCache.Instance.GetTitleData("PrivateCrittersGrabSettings", delegate(string data)
		{
			int num2;
			if (int.TryParse(data, out num2))
			{
				this.privateRoomGrabbingFlags = (CrittersManager.AllowGrabbingFlags)num2;
			}
		}, delegate(PlayFabError e)
		{
		}, false);
	}

	public bool LocalInZone
	{
		get
		{
			return this.localInZone;
		}
	}

	public event Action<CrittersManager.CritterEvent, int, Vector3, Quaternion> OnCritterEventReceived;

	public bool BuildValidationCheck()
	{
		if (this.guard == null)
		{
			Debug.LogError("requestable owner guard missing", base.gameObject);
			return false;
		}
		if (this.crittersPool == null)
		{
			Debug.LogError("critters pool missing", base.gameObject);
			return false;
		}
		return true;
	}

	protected override void Start()
	{
		base.Start();
		CrittersManager.instance.LoadGrabSettings();
		CrittersManager.CheckInitialize();
	}

	public static void InitializeCrittersManager()
	{
		if (CrittersManager.hasInstance)
		{
			return;
		}
		CrittersManager.hasInstance = true;
		CrittersManager.instance = global::UnityEngine.Object.FindAnyObjectByType<CrittersManager>();
		CrittersManager.instance.crittersActors = new List<CrittersActor>();
		CrittersManager.instance.crittersPawns = new List<CrittersPawn>();
		CrittersManager.instance.awareOfActors = new Dictionary<CrittersPawn, List<CrittersActor>>();
		CrittersManager.instance.despawnableActors = new List<CrittersActor>();
		CrittersManager.instance.newlyDisabledActors = new List<CrittersActor>();
		CrittersManager.instance.rigActorSetups = new List<CrittersRigActorSetup>();
		CrittersManager.instance.rigSetupByRig = new Dictionary<VRRig, CrittersRigActorSetup>();
		CrittersManager.instance.updatesToSend = new List<int>();
		CrittersManager.instance.objList = new List<object>();
		CrittersManager.instance.lowPriorityPawnsToProcess = new List<CrittersActor>();
		CrittersManager.instance.actorSpawners = global::UnityEngine.Object.FindObjectsByType<CrittersActorSpawner>(FindObjectsSortMode.None).ToList<CrittersActorSpawner>();
		CrittersManager.instance._spawnRegions = CrittersRegion.Regions;
		CrittersManager.instance.poolCounts = new Dictionary<CrittersActor.CrittersActorType, int>();
		CrittersManager.instance.despawnDecayValue = new Dictionary<CrittersActor.CrittersActorType, float>();
		CrittersManager.instance.actorTypes = (CrittersActor.CrittersActorType[])Enum.GetValues(typeof(CrittersActor.CrittersActorType));
		CrittersManager.instance.poolIndexDict = new Dictionary<CrittersActor.CrittersActorType, int>();
		for (int i = 0; i < CrittersManager.instance.actorTypes.Length; i++)
		{
			CrittersManager.instance.poolCounts[CrittersManager.instance.actorTypes[i]] = 0;
			CrittersManager.instance.despawnDecayValue[CrittersManager.instance.actorTypes[i]] = 0f;
		}
		CrittersManager.instance.PopulatePools();
		List<CrittersRigActorSetup> list = global::UnityEngine.Object.FindObjectsByType<CrittersRigActorSetup>(FindObjectsSortMode.None).ToList<CrittersRigActorSetup>();
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].enabled)
			{
				CrittersManager.RegisterRigActorSetup(list[j]);
			}
		}
		CrittersActorGrabber[] array = global::UnityEngine.Object.FindObjectsByType<CrittersActorGrabber>(FindObjectsSortMode.None);
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k].isLeft)
			{
				CrittersManager._leftGrabber = array[k];
			}
			else
			{
				CrittersManager._rightGrabber = array[k];
			}
		}
		if (CrittersManager.instance.guard.IsNotNull())
		{
			CrittersManager.instance.guard.AddCallbackTarget(CrittersManager.instance);
		}
		RoomSystem.JoinedRoomEvent += new Action(CrittersManager.instance.JoinedRoomEvent);
		RoomSystem.LeftRoomEvent += new Action(CrittersManager.instance.LeftRoomEvent);
	}

	private new void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
		TickSystem<object>.AddTickCallback(this);
	}

	private new void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
		TickSystem<object>.RemoveTickCallback(this);
	}

	private void ResetRoom()
	{
		this.lastSpawnTime = 0.0;
		for (int i = 0; i < this.allActors.Count; i++)
		{
			CrittersActor crittersActor = this.allActors[i];
			if (crittersActor.gameObject.activeSelf)
			{
				if (this.persistentActors.Contains(this.allActors[i]))
				{
					this.allActors[i].Initialize();
				}
				else
				{
					crittersActor.gameObject.SetActive(false);
				}
			}
		}
		for (int j = 0; j < this.actorSpawners.Count; j++)
		{
			this.actorSpawners[j].DoReset();
		}
	}

	public void Tick()
	{
		this.HandleZonesAndOwnership();
		if (this.localInZone)
		{
			this.ProcessSpawning();
			this.ProcessActorBinLocations();
			this.ProcessRigSetups();
			this.ProcessCritterAwareness();
			this.ProcessDespawningIdles();
			this.ProcessActors();
		}
		this.ProcessNewlyDisabledActors();
	}

	public void ProcessRigSetups()
	{
		if (!this.LocalAuthority())
		{
			return;
		}
		this.objList.Clear();
		for (int i = 0; i < this.rigActorSetups.Count; i++)
		{
			this.rigActorSetups[i].CheckUpdate(ref this.objList, false);
		}
		if (this.objList.Count > 0 && NetworkSystem.Instance.InRoom)
		{
			CrittersManager.instance.SendRPC("RemoteUpdatePlayerCrittersActorData", RpcTarget.Others, new object[] { this.objList.ToArray() });
		}
	}

	private void ProcessCritterAwareness()
	{
		if (!this.LocalAuthority())
		{
			return;
		}
		int num = 0;
		this.lowPriorityPawnsToProcess.Clear();
		int i = 0;
		while (i < this.crittersPawns.Count)
		{
			CrittersPawn crittersPawn = this.crittersPawns[i];
			if (!this.awareOfActors.ContainsKey(crittersPawn))
			{
				this.awareOfActors[crittersPawn] = new List<CrittersActor>();
			}
			else
			{
				this.awareOfActors[crittersPawn].Clear();
			}
			this.nearbyActors.Clear();
			int num2 = this.actorBinIndices[crittersPawn];
			if (this.priorityBins[num2])
			{
				goto IL_00D9;
			}
			if (i >= this.lowPriorityIndex && num < this.lowPriorityActorsPerFrame)
			{
				this.lowPriorityPawnsToProcess.Add(this.crittersPawns[i]);
				num++;
				this.lowPriorityIndex++;
				if (this.lowPriorityIndex >= this.crittersPawns.Count)
				{
					this.lowPriorityIndex = 0;
					goto IL_00D9;
				}
				goto IL_00D9;
			}
			IL_01C4:
			i++;
			continue;
			IL_00D9:
			int num3 = Mathf.FloorToInt((float)(num2 / this.binXCount));
			int num4 = num2 % this.binXCount;
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					if (num3 + j < this.binXCount && num3 + j >= 0 && num4 + k < this.binZCount && num4 + k >= 0)
					{
						this.nearbyActors.AddRange(this.actorBins[num4 + k + (num3 + j) * this.binXCount]);
					}
				}
			}
			for (int l = 0; l < this.nearbyActors.Count; l++)
			{
				if (this.crittersPawns[i].AwareOfActor(this.nearbyActors[l]))
				{
					this.awareOfActors[this.crittersPawns[i]].Add(this.nearbyActors[l]);
				}
			}
			goto IL_01C4;
		}
	}

	private void ProcessSpawning()
	{
		if (!this.LocalAuthority())
		{
			return;
		}
		if (this.lastSpawnTime + this.spawnDelay <= (NetworkSystem.Instance.InRoom ? PhotonNetwork.Time : ((double)Time.time)))
		{
			int nextSpawnRegion = this.GetNextSpawnRegion();
			if (nextSpawnRegion >= 0)
			{
				this.SpawnCritter(nextSpawnRegion);
			}
			else
			{
				this.lastSpawnTime = (NetworkSystem.Instance.InRoom ? PhotonNetwork.Time : ((double)Time.time));
			}
		}
		if (this.spawnerIndex >= this.actorSpawners.Count)
		{
			this.spawnerIndex = 0;
		}
		if (this.actorSpawners.Count == 0)
		{
			return;
		}
		this.actorSpawners[this.spawnerIndex].ProcessLocal();
		this.spawnerIndex++;
	}

	private int GetNextSpawnRegion()
	{
		for (int i = 1; i <= this._spawnRegions.Count; i++)
		{
			int num = (this._currentRegionIndex + i) % this._spawnRegions.Count;
			CrittersRegion crittersRegion = this._spawnRegions[num];
			if (crittersRegion.CritterCount < crittersRegion.maxCritters)
			{
				this._currentRegionIndex = num;
				return this._currentRegionIndex;
			}
		}
		return -1;
	}

	private void ProcessActorBinLocations()
	{
		if (this.LocalAuthority())
		{
			for (int i = 0; i < this.actorBins.Length; i++)
			{
				this.actorBins[i].Clear();
				this.priorityBins[i] = false;
			}
			for (int j = this.crittersActors.Count - 1; j >= 0; j--)
			{
				CrittersActor crittersActor = this.crittersActors[j];
				if (crittersActor == null)
				{
					this.crittersActors.RemoveAt(j);
				}
				else
				{
					Transform transform = crittersActor.transform;
					int num = Mathf.Clamp(Mathf.FloorToInt((transform.position.x - this.binDimensionXMin) / this.individualBinSide), 0, this.binXCount - 1);
					int num2 = Mathf.Clamp(Mathf.FloorToInt((transform.position.z - this.binDimensionZMin) / this.individualBinSide), 0, this.binZCount - 1);
					int num3 = num + num2 * this.binXCount;
					if (this.actorBinIndices.ContainsKey(crittersActor))
					{
						this.actorBinIndices[crittersActor] = num3;
					}
					else
					{
						this.actorBinIndices.Add(crittersActor, num3);
					}
					this.actorBins[num3].Add(crittersActor);
				}
			}
			for (int k = 0; k < RoomSystem.PlayersInRoom.Count; k++)
			{
				RigContainer rigContainer;
				if (VRRigCache.Instance.TryGetVrrig(RoomSystem.PlayersInRoom[k], out rigContainer))
				{
					Transform transform2 = rigContainer.Rig.transform;
					float num4 = (transform2.position.x - this.binDimensionXMin) / this.individualBinSide;
					float num5 = (transform2.position.z - this.binDimensionZMin) / this.individualBinSide;
					int num6 = Mathf.FloorToInt(num4);
					int num7 = Mathf.FloorToInt(num5);
					int num8 = ((num4 % 1f > 0.5f) ? 1 : (-1));
					int num9 = ((num5 % 1f > 0.5f) ? 1 : (-1));
					if (num6 < 0 || num6 >= this.binXCount || num7 < 0 || num7 >= this.binZCount)
					{
						return;
					}
					int num10 = num6 + num7 * this.binXCount;
					this.priorityBins[num10] = true;
					num8 = Mathf.Clamp(num6 + num8, 0, this.binXCount - 1);
					num9 = Mathf.Clamp(num7 + num9, 0, this.binZCount - 1);
					this.priorityBins[num8 + num7 * this.binXCount] = true;
					this.priorityBins[num6 + num9 * this.binXCount] = true;
					this.priorityBins[num8 + num9 * this.binXCount] = true;
				}
			}
		}
	}

	private void ProcessDespawningIdles()
	{
		for (int i = 0; i < this.actorTypes.Length; i++)
		{
			this.despawnDecayValue[this.actorTypes[i]] = Mathf.Lerp(this.despawnDecayValue[this.actorTypes[i]], (float)this.despawnThreshold, 1f - Mathf.Exp(-this.decayRate * (Time.realtimeSinceStartup - Time.deltaTime)));
		}
		if (!this.LocalAuthority())
		{
			return;
		}
		if (this.despawnableActors.Count == 0)
		{
			return;
		}
		int j = 0;
		while (j <= this.lowPriorityActorsPerFrame)
		{
			this.despawnIndex++;
			if (this.despawnIndex >= this.despawnableActors.Count)
			{
				this.despawnIndex = 0;
			}
			j++;
			CrittersActor crittersActor = this.despawnableActors[this.despawnIndex];
			if (this.despawnDecayValue[crittersActor.crittersActorType] >= (float)this.despawnThreshold && crittersActor.ShouldDespawn())
			{
				this.DespawnActor(crittersActor);
			}
		}
	}

	public void DespawnActor(CrittersActor actor)
	{
		int actorId = actor.actorId;
		if (!this.updatesToSend.Contains(actorId))
		{
			this.updatesToSend.Add(actorId);
		}
		actor.gameObject.SetActive(false);
	}

	public void IncrementPoolCount(CrittersActor.CrittersActorType type)
	{
		int num;
		if (!this.poolCounts.TryGetValue(type, out num))
		{
			this.poolCounts[type] = 1;
		}
		else
		{
			this.poolCounts[type] = this.poolCounts[type] + 1;
		}
		float num2;
		if (!this.despawnDecayValue.TryGetValue(type, out num2))
		{
			this.despawnDecayValue[type] = 1f;
			return;
		}
		this.despawnDecayValue[type] = this.despawnDecayValue[type] + 1f;
	}

	public void DecrementPoolCount(CrittersActor.CrittersActorType type)
	{
		int num;
		if (this.poolCounts.TryGetValue(type, out num))
		{
			this.poolCounts[type] = Mathf.Max(0, num - 1);
			return;
		}
		this.poolCounts[type] = 0;
	}

	private void ProcessActors()
	{
		if (this.LocalAuthority())
		{
			for (int i = this.crittersActors.Count - 1; i >= 0; i--)
			{
				if (this.crittersActors[i].crittersActorType != CrittersActor.CrittersActorType.Creature || this.priorityBins[this.actorBinIndices[this.crittersActors[i]]] || this.lowPriorityPawnsToProcess.Contains(this.crittersActors[i]))
				{
					int actorId = this.crittersActors[i].actorId;
					if (this.crittersActors[i].ProcessLocal() && !this.updatesToSend.Contains(actorId))
					{
						this.updatesToSend.Add(actorId);
					}
				}
			}
			return;
		}
		for (int j = 0; j < this.crittersActors.Count; j++)
		{
			this.crittersActors[j].ProcessRemote();
		}
	}

	private void ProcessNewlyDisabledActors()
	{
		for (int i = 0; i < this.newlyDisabledActors.Count; i++)
		{
			CrittersActor crittersActor = this.newlyDisabledActors[i];
			if (CrittersManager.instance.crittersActors.Contains(crittersActor))
			{
				CrittersManager.instance.crittersActors.Remove(crittersActor);
			}
			if (crittersActor.despawnWhenIdle && CrittersManager.instance.despawnableActors.Contains(crittersActor))
			{
				CrittersManager.instance.despawnableActors.Remove(crittersActor);
			}
			CrittersManager.instance.DecrementPoolCount(crittersActor.crittersActorType);
			crittersActor.SetTransformToDefaultParent(true);
		}
		this.newlyDisabledActors.Clear();
	}

	public static void RegisterCritter(CrittersPawn crittersPawn)
	{
		CrittersManager.CheckInitialize();
		if (!CrittersManager.instance.crittersPawns.Contains(crittersPawn))
		{
			CrittersManager.instance.crittersPawns.Add(crittersPawn);
		}
	}

	public static void RegisterRigActorSetup(CrittersRigActorSetup setup)
	{
		CrittersManager.CheckInitialize();
		if (!CrittersManager.instance.rigActorSetups.Contains(setup))
		{
			CrittersManager.instance.rigActorSetups.Add(setup);
		}
		CrittersManager.instance.rigSetupByRig.AddOrUpdate(setup.myRig, setup);
	}

	public static void DeregisterCritter(CrittersPawn crittersPawn)
	{
		CrittersManager.CheckInitialize();
		CrittersManager.instance.SetCritterRegion(crittersPawn, 0);
		if (CrittersManager.instance.crittersPawns.Contains(crittersPawn))
		{
			CrittersManager.instance.crittersPawns.Remove(crittersPawn);
		}
	}

	public static void RegisterActor(CrittersActor actor)
	{
		CrittersManager.CheckInitialize();
		if (!CrittersManager.instance.crittersActors.Contains(actor))
		{
			CrittersManager.instance.crittersActors.Add(actor);
		}
		if (actor.despawnWhenIdle && !CrittersManager.instance.despawnableActors.Contains(actor))
		{
			CrittersManager.instance.despawnableActors.Add(actor);
		}
		if (CrittersManager.instance.newlyDisabledActors.Contains(actor))
		{
			CrittersManager.instance.newlyDisabledActors.Remove(actor);
		}
		CrittersManager.instance.IncrementPoolCount(actor.crittersActorType);
	}

	public static void DeregisterActor(CrittersActor actor)
	{
		CrittersManager.CheckInitialize();
		if (!CrittersManager.instance.newlyDisabledActors.Contains(actor))
		{
			CrittersManager.instance.newlyDisabledActors.Add(actor);
		}
	}

	public static void CheckInitialize()
	{
		if (!CrittersManager.hasInstance)
		{
			CrittersManager.InitializeCrittersManager();
		}
	}

	public static bool CritterAwareOfAny(CrittersPawn creature)
	{
		return CrittersManager.instance.awareOfActors[creature].Count > 0;
	}

	public static bool AnyFoodNearby(CrittersPawn creature)
	{
		List<CrittersActor> list = CrittersManager.instance.awareOfActors[creature];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].crittersActorType == CrittersActor.CrittersActorType.Food)
			{
				return true;
			}
		}
		return false;
	}

	public static CrittersFood ClosestFood(CrittersPawn creature)
	{
		float num = float.MaxValue;
		CrittersFood crittersFood = null;
		List<CrittersActor> list = CrittersManager.instance.awareOfActors[creature];
		for (int i = 0; i < list.Count; i++)
		{
			CrittersActor crittersActor = list[i];
			if (crittersActor.crittersActorType == CrittersActor.CrittersActorType.Food)
			{
				CrittersFood crittersFood2 = (CrittersFood)crittersActor;
				float sqrMagnitude = (creature.transform.position - crittersFood2.food.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					crittersFood = crittersFood2;
					num = sqrMagnitude;
				}
			}
		}
		return crittersFood;
	}

	public static void PlayHaptics(AudioClip clip, float strength, bool isLeftHand)
	{
		(isLeftHand ? CrittersManager._leftGrabber : CrittersManager._rightGrabber).PlayHaptics(clip, strength);
	}

	public static void StopHaptics(bool isLeftHand)
	{
		(isLeftHand ? CrittersManager._leftGrabber : CrittersManager._rightGrabber).StopHaptics();
	}

	public CrittersPawn SpawnCritter(int regionIndex = -1)
	{
		CrittersRegion crittersRegion = ((regionIndex >= 0 && regionIndex < this._spawnRegions.Count) ? this._spawnRegions[regionIndex] : null);
		int randomCritterType = this.creatureIndex.GetRandomCritterType(crittersRegion);
		if (randomCritterType < 0)
		{
			return null;
		}
		Vector3 vector = (crittersRegion ? crittersRegion.GetSpawnPoint() : this._spawnRegions[0].transform.position);
		Quaternion quaternion = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
		CrittersPawn crittersPawn = this.SpawnCritter(randomCritterType, vector, quaternion);
		this.SetCritterRegion(crittersPawn, crittersRegion);
		this.lastSpawnTime = (NetworkSystem.Instance.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		return crittersPawn;
	}

	public CrittersPawn SpawnCritter(int critterType, Vector3 position, Quaternion rotation)
	{
		CrittersPawn crittersPawn = (CrittersPawn)this.SpawnActor(CrittersActor.CrittersActorType.Creature, -1);
		crittersPawn.SetTemplate(critterType);
		crittersPawn.currentState = CrittersPawn.CreatureState.Idle;
		crittersPawn.MoveActor(position, Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f), false, true, true);
		crittersPawn.SetImpulseVelocity(Vector3.zero, Vector3.zero);
		crittersPawn.SetState(CrittersPawn.CreatureState.Spawning);
		if (NetworkSystem.Instance.InRoom && this.LocalAuthority())
		{
			base.SendRPC("RemoteSpawnCreature", RpcTarget.Others, new object[]
			{
				crittersPawn.actorId,
				crittersPawn.regionId,
				crittersPawn.visuals.Appearance.WriteToRPCData()
			});
		}
		return crittersPawn;
	}

	public void DespawnCritter(CrittersPawn crittersPawn)
	{
		this.DeactivateActor(crittersPawn);
	}

	public void QueueDespawnAllCritters()
	{
		if (!this.LocalAuthority())
		{
			return;
		}
		foreach (CrittersPawn crittersPawn in this.crittersPawns)
		{
			crittersPawn.SetState(CrittersPawn.CreatureState.Despawning);
		}
	}

	private void SetCritterRegion(CrittersPawn critter, CrittersRegion region)
	{
		this.SetCritterRegion(critter, region ? region.ID : 0);
	}

	private void SetCritterRegion(CrittersPawn critter, int regionId)
	{
		if (critter.regionId != 0)
		{
			CrittersRegion.RemoveCritterFromRegion(critter);
		}
		if (regionId != 0)
		{
			CrittersRegion.AddCritterToRegion(critter, regionId);
		}
		critter.regionId = regionId;
	}

	public void DeactivateActor(CrittersActor actor)
	{
		actor.gameObject.SetActive(false);
	}

	private void CamCapture()
	{
		Camera component = base.GetComponent<Camera>();
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = component.targetTexture;
		component.Render();
		Texture2D texture2D = new Texture2D(component.targetTexture.width, component.targetTexture.height);
		texture2D.ReadPixels(new Rect(0f, 0f, (float)component.targetTexture.width, (float)component.targetTexture.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		texture2D.EncodeToPNG();
		global::UnityEngine.Object.Destroy(texture2D);
	}

	private IEnumerator RemoteDataInitialization(NetPlayer player, int actorNumber)
	{
		List<object> nonPlayerActorObjList = new List<object>();
		List<object> playerActorObjList = new List<object>();
		int worldActorDataCount = 0;
		int playerActorDataCount = 0;
		int num;
		for (int i = 0; i < this.allActors.Count; i = num + 1)
		{
			if (!NetworkSystem.Instance.InRoom || !this.LocalAuthority())
			{
				this.RemoveInitializingPlayer(actorNumber);
				yield break;
			}
			if (this.allActors[i].isOnPlayer)
			{
				num = playerActorDataCount;
				playerActorDataCount = num + 1;
				this.allActors[i].AddPlayerCrittersActorDataToList(ref playerActorObjList);
			}
			if (playerActorDataCount >= this.actorsPerInitializationCall || (i == this.allActors.Count - 1 && playerActorDataCount > 0))
			{
				if (!player.InRoom || player.ActorNumber != actorNumber)
				{
					this.RemoveInitializingPlayer(actorNumber);
					yield break;
				}
				if (NetworkSystem.Instance.InRoom && this.LocalAuthority())
				{
					base.SendRPC("RemoteUpdatePlayerCrittersActorData", player, new object[] { playerActorObjList.ToArray() });
				}
				playerActorObjList.Clear();
				playerActorDataCount = 0;
				yield return new WaitForSeconds(this.actorsInitializationCallCooldown);
			}
			num = i;
		}
		if (!player.InRoom || player.ActorNumber != actorNumber)
		{
			this.RemoveInitializingPlayer(actorNumber);
			yield break;
		}
		if (NetworkSystem.Instance.InRoom && this.LocalAuthority() && playerActorDataCount > 0)
		{
			base.SendRPC("RemoteUpdatePlayerCrittersActorData", player, new object[] { playerActorObjList.ToArray() });
		}
		for (int i = 0; i < this.allActors.Count; i = num + 1)
		{
			if (!player.InRoom || player.ActorNumber != actorNumber)
			{
				this.RemoveInitializingPlayer(actorNumber);
				yield break;
			}
			if (!NetworkSystem.Instance.InRoom || !this.LocalAuthority())
			{
				this.RemoveInitializingPlayer(actorNumber);
				yield break;
			}
			CrittersActor crittersActor = this.allActors[i];
			if (crittersActor.gameObject.activeSelf)
			{
				num = worldActorDataCount;
				worldActorDataCount = num + 1;
				if (crittersActor.parentActorId == -1)
				{
					crittersActor.UpdateImpulses(false, false);
					crittersActor.UpdateImpulseVelocity();
				}
				crittersActor.AddActorDataToList(ref nonPlayerActorObjList);
				if (worldActorDataCount >= this.actorsPerInitializationCall)
				{
					if (!player.InRoom || player.ActorNumber != actorNumber)
					{
						this.RemoveInitializingPlayer(actorNumber);
						yield break;
					}
					if (!NetworkSystem.Instance.InRoom || !this.LocalAuthority())
					{
						this.RemoveInitializingPlayer(actorNumber);
						yield break;
					}
					base.SendRPC("RemoteUpdateCritterData", player, new object[] { nonPlayerActorObjList.ToArray() });
					nonPlayerActorObjList.Clear();
					worldActorDataCount = 0;
					yield return new WaitForSeconds(this.actorsInitializationCallCooldown);
				}
			}
			num = i;
		}
		if (NetworkSystem.Instance.InRoom && this.LocalAuthority() && worldActorDataCount > 0)
		{
			base.SendRPC("RemoteUpdateCritterData", player, new object[] { nonPlayerActorObjList.ToArray() });
		}
		this.RemoveInitializingPlayer(actorNumber);
		yield break;
	}

	private IEnumerator DelayedInitialization(NetPlayer player, List<object> nonPlayerActorObjList)
	{
		yield return new WaitForSeconds(30f);
		base.SendRPC("RemoteUpdateCritterData", player, new object[] { nonPlayerActorObjList.ToArray() });
		yield break;
	}

	public void RemoveInitializingPlayer(int actorNumber)
	{
		if (this.updatingPlayers.Contains(actorNumber))
		{
			this.updatingPlayers.Remove(actorNumber);
		}
	}

	private void JoinedRoomEvent()
	{
		if (this.localInZone && !this.LocalAuthority())
		{
			this.ResetRoom();
		}
		this.hasNewlyInitialized = false;
	}

	private void LeftRoomEvent()
	{
		this.guard.TransferOwnership(NetworkSystem.Instance.LocalPlayer, "");
		if (this.LocalInZone)
		{
			this.ResetRoom();
		}
	}

	[PunRPC]
	public void RequestDataInitialization(PhotonMessageInfo info)
	{
		if (!NetworkSystem.Instance.InRoom || !this.LocalAuthority())
		{
			return;
		}
		if (this.updatingPlayers == null)
		{
			this.updatingPlayers = new List<int>();
		}
		if (this.updatingPlayers.Contains(info.Sender.ActorNumber))
		{
			return;
		}
		this.updatingPlayers.Add(info.Sender.ActorNumber);
		base.StartCoroutine(this.RemoteDataInitialization(info.Sender, info.Sender.ActorNumber));
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.SenderIsOwner(info))
		{
			this.OwnerSentError(info);
			return;
		}
		if (!this.localInZone)
		{
			return;
		}
		int num;
		if (!CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num))
		{
			return;
		}
		if (num > this.actorsPerInitializationCall)
		{
			return;
		}
		int num2 = 0;
		while (num2 < num && this.UpdateActorByType(stream))
		{
			num2++;
		}
	}

	public bool UpdateActorByType(PhotonStream stream)
	{
		int num;
		CrittersActor crittersActor;
		return CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num) && num >= 0 && num < this.universalActorId && this.actorById.TryGetValue(num, out crittersActor) && crittersActor.UpdateSpecificActor(stream);
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!ZoneManagement.IsInZone(GTZone.critters))
		{
			return;
		}
		using (GTProfiler.BeginSample("WriteDataPUNCrittersManager"))
		{
			int num = Mathf.Min(this.updatesToSend.Count, this.actorsPerInitializationCall);
			stream.SendNext(num);
			for (int i = 0; i < num; i++)
			{
				this.allActors[this.updatesToSend[i]].SendDataByCrittersActorType(stream);
			}
			this.updatesToSend.RemoveRange(0, num);
		}
	}

	[PunRPC]
	public void RemoteCritterActorReleased(int releasedActorID, bool keepWorldPosition, Quaternion rotation, Vector3 position, Vector3 velocity, Vector3 angularVelocity, PhotonMessageInfo info)
	{
		if (!this.LocalAuthority())
		{
			return;
		}
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			return;
		}
		if ((in rotation).IsValid())
		{
			float num = 10000f;
			if ((in position).IsValid(in num))
			{
				float num2 = 10000f;
				if ((in velocity).IsValid(in num2))
				{
					float num3 = 10000f;
					if ((in angularVelocity).IsValid(in num3))
					{
						this.CheckValidRemoteActorRelease(releasedActorID, keepWorldPosition, rotation, position, velocity, angularVelocity, info);
						return;
					}
				}
			}
		}
	}

	[PunRPC]
	public void RemoteSpawnCreature(int actorID, int regionId, object[] spawnData, PhotonMessageInfo info)
	{
		if (!this.SenderIsOwner(info))
		{
			this.OwnerSentError(info);
			return;
		}
		if (!this.localInZone)
		{
			return;
		}
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			return;
		}
		if (!CritterAppearance.ValidateData(spawnData))
		{
			return;
		}
		CrittersActor crittersActor;
		if (this.actorById.TryGetValue(actorID, out crittersActor))
		{
			CrittersPawn crittersPawn = (CrittersPawn)crittersActor;
			this.SetCritterRegion(crittersPawn, regionId);
			crittersPawn.SetSpawnData(spawnData);
		}
	}

	[PunRPC]
	public void RemoteCrittersActorGrabbedby(int grabbedActorID, int grabberActorID, Quaternion offsetRotation, Vector3 offsetPosition, bool isGrabDisabled, PhotonMessageInfo info)
	{
		if (!this.LocalAuthority())
		{
			return;
		}
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			return;
		}
		if ((in offsetRotation).IsValid())
		{
			float num = 10000f;
			if ((in offsetPosition).IsValid(in num))
			{
				this.CheckValidRemoteActorGrab(grabbedActorID, grabberActorID, offsetRotation, offsetPosition, isGrabDisabled, info);
				return;
			}
		}
	}

	[PunRPC]
	public void RemoteUpdatePlayerCrittersActorData(object[] data, PhotonMessageInfo info)
	{
		if (!this.SenderIsOwner(info))
		{
			this.OwnerSentError(info);
			return;
		}
		if (!this.localInZone)
		{
			return;
		}
		if (data == null)
		{
			return;
		}
		CrittersActor crittersActor;
		for (int i = 0; i < data.Length; i += crittersActor.UpdatePlayerCrittersActorFromRPC(data, i))
		{
			int num;
			if (!CrittersManager.ValidateDataType<int>(data[i], out num))
			{
				return;
			}
			if (!this.actorById.TryGetValue(num, out crittersActor))
			{
				return;
			}
		}
	}

	[PunRPC]
	public void RemoteUpdateCritterData(object[] data, PhotonMessageInfo info)
	{
		if (!this.SenderIsOwner(info))
		{
			this.OwnerSentError(info);
			return;
		}
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			return;
		}
		if (!this.localInZone)
		{
			return;
		}
		if (data == null)
		{
			return;
		}
		CrittersActor crittersActor;
		for (int i = 0; i < data.Length; i += crittersActor.UpdateFromRPC(data, i))
		{
			int num;
			if (!CrittersManager.ValidateDataType<int>(data[i], out num))
			{
				return;
			}
			if (!this.actorById.TryGetValue(num, out crittersActor))
			{
				return;
			}
		}
	}

	public CrittersActor SpawnActor(CrittersActor.CrittersActorType type, int subObjectIndex = -1)
	{
		List<CrittersActor> list;
		if (!this.actorPools.TryGetValue(type, out list))
		{
			return null;
		}
		int num = this.poolIndexDict[type];
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[(i + num) % list.Count].gameObject.activeSelf)
			{
				num = (i + num) % list.Count;
				this.poolIndexDict[type] = num + 1;
				list[num].subObjectIndex = subObjectIndex;
				list[num].gameObject.SetActive(true);
				return list[num];
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			CrittersActor crittersActor = list[j];
			int num2 = this.actorBinIndices[crittersActor];
			if (!this.priorityBins[num2])
			{
				list[j].gameObject.SetActive(false);
				list[j].subObjectIndex = subObjectIndex;
				list[j].gameObject.SetActive(true);
				return list[j];
			}
		}
		return null;
	}

	public override void WriteDataFusion()
	{
	}

	public override void ReadDataFusion()
	{
	}

	public void PopulatePools()
	{
		this.binDimensionXMin = this.crittersRange.position.x - this.crittersRange.localScale.x / 2f;
		this.binDimensionZMin = this.crittersRange.position.z - this.crittersRange.localScale.z / 2f;
		this.xLength = this.crittersRange.localScale.x;
		this.zLength = this.crittersRange.localScale.z;
		float num = this.xLength * this.zLength / (float)this.totalBinsApproximate;
		this.individualBinSide = Mathf.Sqrt(num);
		this.binXCount = Mathf.CeilToInt(this.xLength / this.individualBinSide);
		this.binZCount = Mathf.CeilToInt(this.zLength / this.individualBinSide);
		int num2 = this.binXCount * this.binZCount;
		this.actorBins = new List<CrittersActor>[num2];
		for (int i = 0; i < num2; i++)
		{
			this.actorBins[i] = new List<CrittersActor>();
		}
		this.priorityBins = new bool[num2];
		this.actorBinIndices = new Dictionary<CrittersActor, int>();
		this.nearbyActors = new List<CrittersActor>();
		this.allActors = new List<CrittersActor>();
		this.actorPools = new Dictionary<CrittersActor.CrittersActorType, List<CrittersActor>>();
		this.actorPools.Add(CrittersActor.CrittersActorType.Bag, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.Cage, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.Food, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.Creature, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.LoudNoise, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.Grabber, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.FoodSpawner, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.AttachPoint, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.StunBomb, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.BodyAttachPoint, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.NoiseMaker, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.StickyTrap, new List<CrittersActor>());
		this.actorPools.Add(CrittersActor.CrittersActorType.StickyGoo, new List<CrittersActor>());
		this.actorById = new Dictionary<int, CrittersActor>();
		this.universalActorId = 0;
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = base.transform;
		this.poolParent = gameObject.transform;
		this.poolParent.name = "Critter Actors Pool Parent";
		List<CrittersActor> list;
		this.actorPools.TryGetValue(CrittersActor.CrittersActorType.Food, out list);
		this.persistentActors = global::UnityEngine.Object.FindObjectsByType<CrittersActor>(FindObjectsSortMode.InstanceID).ToList<CrittersActor>();
		this.persistentActors.Sort((CrittersActor x, CrittersActor y) => x.transform.position.magnitude.CompareTo(y.transform.position.magnitude));
		this.persistentActors.Sort((CrittersActor x, CrittersActor y) => x.gameObject.name.CompareTo(y.gameObject.name));
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.bagPrefab, CrittersActor.CrittersActorType.Bag, gameObject.transform, 80, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.cagePrefab, CrittersActor.CrittersActorType.Cage, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.foodPrefab, CrittersActor.CrittersActorType.Food, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.creaturePrefab, CrittersActor.CrittersActorType.Creature, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.noisePrefab, CrittersActor.CrittersActorType.LoudNoise, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.grabberPrefab, CrittersActor.CrittersActorType.Grabber, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.foodSpawnerPrefab, CrittersActor.CrittersActorType.FoodSpawner, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.bodyAttachPointPrefab, CrittersActor.CrittersActorType.BodyAttachPoint, gameObject.transform, 40, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, null, CrittersActor.CrittersActorType.AttachPoint, gameObject.transform, 0, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.stunBombPrefab, CrittersActor.CrittersActorType.StunBomb, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.noiseMakerPrefab, CrittersActor.CrittersActorType.NoiseMaker, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.stickyTrapPrefab, CrittersActor.CrittersActorType.StickyTrap, gameObject.transform, this.poolCount, this.persistentActors);
		this.UpdatePool<CrittersActor>(ref this.actorPools, this.stickyGooPrefab, CrittersActor.CrittersActorType.StickyGoo, gameObject.transform, this.poolCount, this.persistentActors);
	}

	public void UpdatePool<T>(ref Dictionary<CrittersActor.CrittersActorType, List<T>> dict, GameObject prefab, CrittersActor.CrittersActorType crittersActorType, Transform parent, int poolAmount, List<CrittersActor> sceneActors) where T : CrittersActor
	{
		int num = 0;
		for (int i = 0; i < sceneActors.Count; i++)
		{
			if (sceneActors[i].crittersActorType == crittersActorType)
			{
				dict[crittersActorType].Add((T)((object)sceneActors[i]));
				sceneActors[i].actorId = this.universalActorId;
				this.actorById.Add(this.universalActorId, sceneActors[i]);
				this.allActors.Add(sceneActors[i]);
				this.universalActorId++;
				num++;
				if (sceneActors[i].enabled)
				{
					if (crittersActorType == CrittersActor.CrittersActorType.Creature)
					{
						CrittersManager.RegisterCritter(sceneActors[i] as CrittersPawn);
					}
					else
					{
						CrittersManager.RegisterActor(sceneActors[i]);
					}
				}
			}
		}
		for (int j = 0; j < poolAmount - num; j++)
		{
			GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(prefab);
			gameObject.transform.parent = parent;
			gameObject.name += j.ToString();
			gameObject.SetActive(false);
			T component = gameObject.GetComponent<T>();
			dict[crittersActorType].Add(component);
			component.actorId = this.universalActorId;
			component.SetDefaultParent(parent);
			this.actorById.Add(this.universalActorId, component);
			this.allActors.Add(component);
			this.universalActorId++;
		}
		this.poolIndexDict[crittersActorType] = 0;
	}

	public void TriggerEvent(CrittersManager.CritterEvent eventType, int sourceActor, Vector3 position, Quaternion rotation)
	{
		Action<CrittersManager.CritterEvent, int, Vector3, Quaternion> onCritterEventReceived = this.OnCritterEventReceived;
		if (onCritterEventReceived != null)
		{
			onCritterEventReceived(eventType, sourceActor, position, rotation);
		}
		if (!this.LocalAuthority() || !NetworkSystem.Instance.InRoom)
		{
			return;
		}
		base.SendRPC("RemoteReceivedCritterEvent", RpcTarget.Others, new object[] { eventType, sourceActor, position, rotation });
	}

	public void TriggerEvent(CrittersManager.CritterEvent eventType, int sourceActor, Vector3 position)
	{
		this.TriggerEvent(eventType, sourceActor, position, Quaternion.identity);
	}

	[PunRPC]
	public void RemoteReceivedCritterEvent(CrittersManager.CritterEvent eventType, int sourceActor, Vector3 position, Quaternion rotation, PhotonMessageInfo info)
	{
		if (!this.localInZone)
		{
			return;
		}
		if (!this.SenderIsOwner(info))
		{
			this.OwnerSentError(info);
			return;
		}
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			return;
		}
		float num = 10000f;
		if (!(in position).IsValid(in num) || !(in rotation).IsValid())
		{
			return;
		}
		if (!this.critterEventCallLimit.CheckCallTime(Time.time))
		{
			return;
		}
		Action<CrittersManager.CritterEvent, int, Vector3, Quaternion> onCritterEventReceived = this.OnCritterEventReceived;
		if (onCritterEventReceived == null)
		{
			return;
		}
		onCritterEventReceived(eventType, sourceActor, position, rotation);
	}

	public static bool ValidateDataType<T>(object obj, out T dataAsType)
	{
		if (obj is T)
		{
			dataAsType = (T)((object)obj);
			return true;
		}
		dataAsType = default(T);
		return false;
	}

	public void CheckValidRemoteActorRelease(int releasedActorID, bool keepWorldPosition, Quaternion rotation, Vector3 position, Vector3 velocity, Vector3 angularVelocity, PhotonMessageInfo info)
	{
		CrittersActor crittersActor;
		if (!this.actorById.TryGetValue(releasedActorID, out crittersActor))
		{
			return;
		}
		CrittersActor crittersActor2 = this.TopLevelCritterGrabber(crittersActor);
		(ref rotation).SetValueSafe(in rotation);
		(ref position).SetValueSafe(in position);
		(ref velocity).SetValueSafe(in velocity);
		(ref angularVelocity).SetValueSafe(in angularVelocity);
		if (crittersActor2 != null && crittersActor2 is CrittersGrabber && crittersActor2.isOnPlayer && crittersActor2.rigPlayerId == info.Sender.ActorNumber)
		{
			crittersActor.Released(keepWorldPosition, rotation, position, velocity, angularVelocity);
		}
	}

	private void CheckValidRemoteActorGrab(int actorBeingGrabbedActorID, int grabbingActorID, Quaternion offsetRotation, Vector3 offsetPosition, bool isGrabDisabled, PhotonMessageInfo info)
	{
		CrittersActor crittersActor;
		CrittersActor crittersActor2;
		if (!this.actorById.TryGetValue(actorBeingGrabbedActorID, out crittersActor) || !this.actorById.TryGetValue(grabbingActorID, out crittersActor2))
		{
			return;
		}
		(ref offsetRotation).SetValueSafe(in offsetRotation);
		(ref offsetPosition).SetValueSafe(in offsetPosition);
		if ((crittersActor.transform.position - crittersActor2.transform.position).magnitude > this.maxGrabDistance || offsetPosition.magnitude > this.maxGrabDistance)
		{
			return;
		}
		if (((crittersActor2.crittersActorType == CrittersActor.CrittersActorType.Grabber && crittersActor2.isOnPlayer && crittersActor2.rigPlayerId == info.Sender.ActorNumber) || crittersActor2.crittersActorType != CrittersActor.CrittersActorType.Grabber) && crittersActor.AllowGrabbingActor(crittersActor2))
		{
			crittersActor.GrabbedBy(crittersActor2, true, offsetRotation, offsetPosition, isGrabDisabled);
		}
	}

	private CrittersActor TopLevelCritterGrabber(CrittersActor baseActor)
	{
		CrittersActor crittersActor = null;
		this.actorById.TryGetValue(baseActor.parentActorId, out crittersActor);
		while (crittersActor != null && crittersActor.parentActorId != -1)
		{
			this.actorById.TryGetValue(crittersActor.parentActorId, out crittersActor);
		}
		return crittersActor;
	}

	public static CapsuleCollider DuplicateCapsuleCollider(Transform targetTransform, CapsuleCollider sourceCollider)
	{
		if (sourceCollider == null)
		{
			return null;
		}
		CapsuleCollider capsuleCollider = new GameObject().AddComponent<CapsuleCollider>();
		capsuleCollider.transform.rotation = sourceCollider.transform.rotation;
		capsuleCollider.transform.position = sourceCollider.transform.position;
		capsuleCollider.transform.localScale = sourceCollider.transform.lossyScale;
		capsuleCollider.radius = sourceCollider.radius;
		capsuleCollider.height = sourceCollider.height;
		capsuleCollider.center = sourceCollider.center;
		capsuleCollider.gameObject.layer = targetTransform.gameObject.layer;
		capsuleCollider.transform.SetParent(targetTransform.transform);
		return capsuleCollider;
	}

	private void HandleZonesAndOwnership()
	{
		bool flag = this.localInZone;
		this.localInZone = ZoneManagement.IsInZone(GTZone.critters);
		this.CheckOwnership();
		if (!this.LocalAuthority() && this.localInZone && NetworkSystem.Instance.InRoom && this.guard.actualOwner != null && (!this.hasNewlyInitialized || !flag) && Time.time > this.lastRequest + this.initRequestCooldown)
		{
			this.lastRequest = Time.time;
			this.hasNewlyInitialized = true;
			base.SendRPC("RequestDataInitialization", this.guard.actualOwner, Array.Empty<object>());
		}
		if (flag && !this.localInZone)
		{
			this.ResetRoom();
			this.poolParent.gameObject.SetActive(false);
			this.crittersPool.poolParent.gameObject.SetActive(false);
		}
		if (!flag && this.localInZone)
		{
			this.poolParent.gameObject.SetActive(true);
			this.crittersPool.poolParent.gameObject.SetActive(true);
		}
	}

	private void CheckOwnership()
	{
		if (!PhotonNetwork.InRoom && base.IsMine)
		{
			if (this.guard.actualOwner == null || !this.guard.actualOwner.Equals(NetworkSystem.Instance.LocalPlayer))
			{
				this.guard.SetOwnership(NetworkSystem.Instance.LocalPlayer, false, false);
			}
			return;
		}
		if (this.allRigs == null && !VRRigCache.isInitialized)
		{
			return;
		}
		if (this.allRigs == null)
		{
			this.allRigs = new List<VRRig>(VRRigCache.Instance.GetAllRigs());
		}
		if (!this.LocalAuthority())
		{
			return;
		}
		if (this.localInZone)
		{
			return;
		}
		int num = int.MaxValue;
		NetPlayer netPlayer = null;
		for (int i = 0; i < this.allRigs.Count; i++)
		{
			NetPlayer creator = this.allRigs[i].creator;
			if (creator != null && this.allRigs[i].zoneEntity.currentZone == GTZone.critters && creator.ActorNumber < num)
			{
				netPlayer = creator;
				num = creator.ActorNumber;
			}
		}
		if (netPlayer == null)
		{
			return;
		}
		this.guard.TransferOwnership(netPlayer, "");
	}

	public bool LocalAuthority()
	{
		return !NetworkSystem.Instance.InRoom || (!(this.guard == null) && ((this.guard.actualOwner != null && this.guard.isTrulyMine) || (base.Owner != null && base.Owner.IsLocal) || this.guard.currentState == NetworkingState.IsOwner));
	}

	private bool SenderIsOwner(PhotonMessageInfo info)
	{
		return (this.guard.actualOwner != null || base.Owner != null) && info.Sender != null && !this.LocalAuthority() && ((this.guard.actualOwner != null && this.guard.actualOwner.ActorNumber == info.Sender.ActorNumber) || (base.Owner != null && base.Owner.ActorNumber == info.Sender.ActorNumber));
	}

	private void OwnerSentError(PhotonMessageInfo info)
	{
		NetPlayer owner = base.Owner;
	}

	public void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer)
	{
		NetPlayer localPlayer = NetworkSystem.Instance.LocalPlayer;
	}

	public bool OnOwnershipRequest(NetPlayer fromPlayer)
	{
		return false;
	}

	public void OnMyOwnerLeft()
	{
	}

	public bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer)
	{
		return false;
	}

	public void OnMyCreatorLeft()
	{
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

	public CritterIndex creatureIndex;

	public static volatile CrittersManager instance;

	public LayerMask movementLayers;

	public LayerMask objectLayers;

	public LayerMask containerLayer;

	[ReadOnly]
	public List<CrittersActor> crittersActors;

	[ReadOnly]
	public List<CrittersActor> allActors;

	[ReadOnly]
	public List<CrittersPawn> crittersPawns;

	[ReadOnly]
	public List<CrittersActor> despawnableActors;

	[ReadOnly]
	public List<CrittersActor> newlyDisabledActors;

	[ReadOnly]
	public List<CrittersRigActorSetup> rigActorSetups;

	[ReadOnly]
	public List<CrittersActorSpawner> actorSpawners;

	[NonSerialized]
	private List<CrittersActor> persistentActors = new List<CrittersActor>();

	public Dictionary<int, CrittersActor> actorById;

	public Dictionary<CrittersPawn, List<CrittersActor>> awareOfActors;

	public Dictionary<VRRig, CrittersRigActorSetup> rigSetupByRig;

	private int allActorsCount;

	public bool intialized;

	private List<int> updatesToSend;

	public int actorsPerInitializationCall = 5;

	public float actorsInitializationCallCooldown = 0.2f;

	public Transform poolParent;

	public List<object> objList;

	public double spawnDelay;

	private double lastSpawnTime;

	public float softJointGracePeriod = 0.1f;

	private List<CrittersRegion> _spawnRegions;

	private int _currentRegionIndex = -1;

	private static CrittersActorGrabber _rightGrabber;

	private static CrittersActorGrabber _leftGrabber;

	public float springForce = 1000f;

	public float springAngularForce = 100f;

	public float damperForce = 10f;

	public float damperAngularForce = 1f;

	public float lightMass = 0.05f;

	public float heavyMass = 2f;

	public float overlapDistanceMax = 0.01f;

	public float fastThrowThreshold = 3f;

	public float fastThrowMultiplier = 1.5f;

	private Dictionary<CrittersActor.CrittersActorType, int> poolIndexDict;

	public CrittersManager.AllowGrabbingFlags privateRoomGrabbingFlags;

	public CrittersManager.AllowGrabbingFlags publicRoomGrabbingFlags;

	public float MaxAttachSpeed = 0.04f;

	private float binDimensionXMin;

	private float binDimensionZMin;

	public Transform crittersRange;

	public int totalBinsApproximate = 400;

	private float xLength;

	private float zLength;

	private int binXCount;

	private int binZCount;

	private float individualBinSide;

	private List<CrittersActor>[] actorBins;

	private bool[] priorityBins;

	private Dictionary<CrittersActor, int> actorBinIndices;

	private List<CrittersActor> nearbyActors;

	private List<NetPlayer> playersToUpdate;

	public CrittersPool crittersPool;

	private int lowPriorityActorsPerFrame = 5;

	private int lowPriorityIndex;

	private int spawnerIndex;

	private int despawnIndex;

	private List<CrittersActor> lowPriorityPawnsToProcess;

	private Dictionary<CrittersActor.CrittersActorType, float> despawnDecayValue;

	public float decayRate = 60f;

	private CrittersActor.CrittersActorType[] actorTypes;

	public float maxGrabDistance = 25f;

	public RequestableOwnershipGuard guard;

	private List<VRRig> allRigs;

	private bool localInZone;

	private List<int> updatingPlayers;

	private bool hasNewlyInitialized;

	private float initRequestCooldown = 10f;

	private float lastRequest;

	public int poolCount = 100;

	public int despawnThreshold = 20;

	private Dictionary<CrittersActor.CrittersActorType, int> poolCounts;

	private Dictionary<CrittersActor.CrittersActorType, List<CrittersActor>> actorPools;

	public GameObject foodPrefab;

	public GameObject creaturePrefab;

	public GameObject noisePrefab;

	public GameObject grabberPrefab;

	public GameObject cagePrefab;

	public GameObject foodSpawnerPrefab;

	public GameObject stunBombPrefab;

	public GameObject bodyAttachPointPrefab;

	public GameObject bagPrefab;

	public GameObject noiseMakerPrefab;

	public GameObject stickyTrapPrefab;

	public GameObject stickyGooPrefab;

	public int universalActorId;

	public int rigActorId;

	private CallLimiter critterEventCallLimit = new CallLimiter(10, 0.5f, 0.5f);

	[Flags]
	public enum AllowGrabbingFlags
	{
		None = 0,
		OutOfHands = 1,
		FromBags = 2,
		EntireBag = 4
	}

	public enum CritterEvent
	{
		StunExplosion,
		NoiseMakerTriggered,
		StickyDeployed,
		StickyTriggered
	}
}
