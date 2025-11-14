using System;
using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public class GREnemySummoner : MonoBehaviour, IGameEntityComponent, IGameEntitySerialize, IGameHittable, IGameEntityDebugComponent, IGameAgentComponent, IGRSummoningEntity
{
	private void Awake()
	{
		this.rigidBody = base.GetComponent<Rigidbody>();
		this.colliders = new List<Collider>(4);
		this.trackedEntities = new List<int>();
		base.GetComponentsInChildren<Collider>(this.colliders);
		this.agent = base.GetComponent<GameAgent>();
		this.entity = base.GetComponent<GameEntity>();
		if (this.armor != null)
		{
			this.armor.SetHp(0);
		}
		this.navAgent.updateRotation = false;
		this.behaviorStartTime = -1.0;
		this.agent.onBehaviorStateChanged += this.OnNetworkBehaviorStateChange;
		this.senseNearby.Setup(this.headTransform);
	}

	public void OnEntityInit()
	{
		this.abilityIdle.Setup(this.agent, this.anim, this.audioSource, null, null, null);
		this.abilityWander.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilityDie.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilitySummon.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilityKeepDistance.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilityMoveToTarget.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilityStagger.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilityInvestigate.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilityJump.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.abilityFlashed.Setup(this.agent, this.anim, this.audioSource, base.transform, null, null);
		this.SetBehavior(GREnemySummoner.Behavior.Idle, true);
		if (this.entity && this.entity.manager && this.entity.manager.ghostReactorManager && this.entity.manager.ghostReactorManager.reactor)
		{
			foreach (GRBonusEntry grbonusEntry in this.entity.manager.ghostReactorManager.reactor.GetCurrLevelGenConfig().enemyGlobalBonuses)
			{
				this.attributes.AddBonus(grbonusEntry);
			}
		}
		this.SetHP(this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax));
		this.navAgent.speed = (float)this.attributes.CalculateFinalValueForAttribute(GRAttributeType.PatrolSpeed);
		this.agent.navAgent.autoTraverseOffMeshLink = false;
		this.agent.onJumpRequested += this.OnAgentJumpRequested;
		if (this.attributes.CalculateFinalValueForAttribute(GRAttributeType.ArmorMax) > 0)
		{
			this.SetBodyState(GREnemySummoner.BodyState.Shell, true);
			return;
		}
		this.SetBodyState(GREnemySummoner.BodyState.Bones, true);
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private void OnDisable()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDestroy()
	{
		this.agent.onBehaviorStateChanged -= this.OnNetworkBehaviorStateChange;
	}

	private void OnAgentJumpRequested(Vector3 start, Vector3 end, float heightScale, float speedScale)
	{
		this.abilityJump.SetupJump(start, end, heightScale, speedScale);
		this.SetBehavior(GREnemySummoner.Behavior.Jump, false);
	}

	public void OnNetworkBehaviorStateChange(byte newState)
	{
		if (newState < 0 || newState >= 10)
		{
			return;
		}
		this.SetBehavior((GREnemySummoner.Behavior)newState, false);
	}

	public void SetHP(int hp)
	{
		this.hp = hp;
	}

	public bool TrySetBehavior(GREnemySummoner.Behavior newBehavior)
	{
		if (this.currBehavior == GREnemySummoner.Behavior.Jump && newBehavior == GREnemySummoner.Behavior.Stagger)
		{
			return false;
		}
		this.SetBehavior(newBehavior, false);
		return true;
	}

	public void SetBehavior(GREnemySummoner.Behavior newBehavior, bool force = false)
	{
		if (this.currBehavior == newBehavior && !force)
		{
			return;
		}
		switch (this.currBehavior)
		{
		case GREnemySummoner.Behavior.Idle:
			this.abilityIdle.Stop();
			break;
		case GREnemySummoner.Behavior.Wander:
			this.abilityWander.Stop();
			break;
		case GREnemySummoner.Behavior.Stagger:
			this.abilityStagger.Stop();
			break;
		case GREnemySummoner.Behavior.Destroyed:
			this.abilityDie.Stop();
			break;
		case GREnemySummoner.Behavior.Summon:
			this.abilitySummon.Stop();
			if (this.summonLight != null)
			{
				this.summonLight.gameObject.SetActive(false);
			}
			break;
		case GREnemySummoner.Behavior.KeepDistance:
			this.abilityKeepDistance.Stop();
			break;
		case GREnemySummoner.Behavior.MoveToTarget:
			this.abilityMoveToTarget.Stop();
			break;
		case GREnemySummoner.Behavior.Investigate:
			this.abilityInvestigate.Stop();
			break;
		case GREnemySummoner.Behavior.Jump:
			this.abilityJump.Stop();
			break;
		case GREnemySummoner.Behavior.Flashed:
			this.abilityFlashed.Stop();
			break;
		}
		this.currBehavior = newBehavior;
		this.behaviorStartTime = Time.timeAsDouble;
		switch (this.currBehavior)
		{
		case GREnemySummoner.Behavior.Idle:
			this.abilityIdle.Start();
			break;
		case GREnemySummoner.Behavior.Wander:
			this.abilityWander.Start();
			this.soundWander.Play(this.audioSource);
			break;
		case GREnemySummoner.Behavior.Stagger:
			this.abilityStagger.Start();
			break;
		case GREnemySummoner.Behavior.Destroyed:
			if (this.entity.IsAuthority())
			{
				this.entity.manager.RequestCreateItem(this.corePrefab.gameObject.name.GetStaticHash(), this.coreMarker.position, this.coreMarker.rotation, 0L);
			}
			this.abilityDie.Start();
			break;
		case GREnemySummoner.Behavior.Summon:
			if (this.summonLight != null)
			{
				this.summonLight.gameObject.SetActive(true);
			}
			this.lastSummonTime = Time.timeAsDouble;
			this.abilitySummon.SetLookAtTarget(this.GetPlayerTransform(this.agent.targetPlayer));
			this.abilitySummon.Start();
			break;
		case GREnemySummoner.Behavior.KeepDistance:
			this.abilityKeepDistance.SetTargetPlayer(this.agent.targetPlayer);
			this.abilityKeepDistance.Start();
			break;
		case GREnemySummoner.Behavior.MoveToTarget:
			this.abilityMoveToTarget.SetTarget(this.GetPlayerTransform(this.agent.targetPlayer));
			this.abilityMoveToTarget.Start();
			break;
		case GREnemySummoner.Behavior.Investigate:
			this.abilityInvestigate.Start();
			break;
		case GREnemySummoner.Behavior.Jump:
			this.abilityJump.Start();
			break;
		case GREnemySummoner.Behavior.Flashed:
			this.abilityFlashed.Start();
			break;
		}
		if (this.entity.IsAuthority())
		{
			this.agent.RequestBehaviorChange((byte)this.currBehavior);
		}
	}

	private void Update()
	{
		this.OnUpdate(Time.deltaTime);
	}

	public void OnEntityThink(float dt)
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		this.lastUpdateTime = Time.time;
		GREnemySummoner.tempRigs.Clear();
		GREnemySummoner.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GREnemySummoner.tempRigs);
		this.senseNearby.UpdateNearby(GREnemySummoner.tempRigs, this.senseLineOfSight);
		float num;
		VRRig vrrig = this.senseNearby.PickClosest(out num);
		this.agent.RequestTarget((vrrig == null) ? null : vrrig.OwningNetPlayer);
		switch (this.currBehavior)
		{
		case GREnemySummoner.Behavior.Idle:
			this.abilityIdle.Think(dt);
			this.ChooseNewBehavior();
			return;
		case GREnemySummoner.Behavior.Wander:
			this.abilityWander.Think(dt);
			this.ChooseNewBehavior();
			return;
		case GREnemySummoner.Behavior.Stagger:
		case GREnemySummoner.Behavior.Destroyed:
			break;
		case GREnemySummoner.Behavior.Summon:
			this.abilitySummon.Think(dt);
			if (this.abilitySummon.IsDone())
			{
				this.ChooseNewBehavior();
				return;
			}
			break;
		case GREnemySummoner.Behavior.KeepDistance:
			this.abilityKeepDistance.Think(dt);
			this.ChooseNewBehavior();
			return;
		case GREnemySummoner.Behavior.MoveToTarget:
			this.abilityMoveToTarget.Think(dt);
			this.ChooseNewBehavior();
			break;
		case GREnemySummoner.Behavior.Investigate:
			this.abilityInvestigate.Think(dt);
			this.ChooseNewBehavior();
			return;
		default:
			return;
		}
	}

	public bool CanSummon()
	{
		return !GhostReactorManager.AggroDisabled && (this.currBehavior != GREnemySummoner.Behavior.Summon || !this.abilitySummon.IsDone()) && Time.timeAsDouble - this.lastSummonTime >= (double)this.minSummonInterval && this.trackedEntities.Count < this.maxSimultaneousSummonedEntities;
	}

	public Transform GetPlayerTransform(NetPlayer targetPlayer)
	{
		if (targetPlayer != null)
		{
			GRPlayer grplayer = GRPlayer.Get(targetPlayer.ActorNumber);
			if (grplayer != null && grplayer.State == GRPlayer.GRPlayerState.Alive)
			{
				return grplayer.transform;
			}
		}
		return null;
	}

	private void ChooseNewBehavior()
	{
		float num = 0f;
		VRRig vrrig = this.senseNearby.PickClosest(out num);
		if (!GhostReactorManager.AggroDisabled && vrrig != null)
		{
			this.investigateLocation = null;
			float num2 = ((this.currBehavior == GREnemySummoner.Behavior.KeepDistance) ? (this.keepDistanceThreshold + 1f) : this.keepDistanceThreshold);
			if (num < num2 * num2)
			{
				this.SetBehavior(GREnemySummoner.Behavior.KeepDistance, false);
				return;
			}
			if (this.CanSummon())
			{
				this.SetBehavior(GREnemySummoner.Behavior.Summon, false);
				return;
			}
			float num3 = this.tooFarDistanceThreshold * this.tooFarDistanceThreshold;
			if (num > num3)
			{
				this.SetBehavior(GREnemySummoner.Behavior.MoveToTarget, false);
				return;
			}
			this.SetBehavior(GREnemySummoner.Behavior.Idle, false);
			return;
		}
		else
		{
			this.investigateLocation = AbilityHelperFunctions.GetLocationToInvestigate(base.transform.position, this.hearingRadius, this.investigateLocation);
			if (this.investigateLocation != null)
			{
				this.abilityInvestigate.SetTargetPos(this.investigateLocation.Value);
				this.SetBehavior(GREnemySummoner.Behavior.Investigate, false);
				return;
			}
			double num4 = Time.timeAsDouble - this.abilityIdle.startTime;
			if (this.currBehavior == GREnemySummoner.Behavior.Idle && num4 < (double)this.idleDuration)
			{
				this.SetBehavior(GREnemySummoner.Behavior.Idle, false);
				return;
			}
			this.SetBehavior(GREnemySummoner.Behavior.Wander, false);
			return;
		}
	}

	public void OnUpdate(float dt)
	{
		if (this.entity.IsAuthority())
		{
			this.OnUpdateAuthority(dt);
			return;
		}
		this.OnUpdateRemote(dt);
	}

	public void OnUpdateAuthority(float dt)
	{
		switch (this.currBehavior)
		{
		case GREnemySummoner.Behavior.Idle:
			this.abilityIdle.Update(dt);
			return;
		case GREnemySummoner.Behavior.Wander:
			this.abilityWander.Update(dt);
			return;
		case GREnemySummoner.Behavior.Stagger:
			this.abilityStagger.Update(dt);
			if (this.abilityStagger.IsDone())
			{
				this.SetBehavior(GREnemySummoner.Behavior.Wander, false);
				return;
			}
			break;
		case GREnemySummoner.Behavior.Destroyed:
			this.abilityDie.Update(dt);
			return;
		case GREnemySummoner.Behavior.Summon:
			this.abilitySummon.Update(dt);
			return;
		case GREnemySummoner.Behavior.KeepDistance:
			this.abilityKeepDistance.Update(dt);
			return;
		case GREnemySummoner.Behavior.MoveToTarget:
			this.abilityMoveToTarget.Update(dt);
			return;
		case GREnemySummoner.Behavior.Investigate:
			this.abilityInvestigate.Update(dt);
			return;
		case GREnemySummoner.Behavior.Jump:
			this.abilityJump.Update(dt);
			if (this.abilityJump.IsDone())
			{
				this.ChooseNewBehavior();
				return;
			}
			break;
		case GREnemySummoner.Behavior.Flashed:
			this.abilityFlashed.Update(dt);
			if (this.abilityFlashed.IsDone())
			{
				this.ChooseNewBehavior();
			}
			break;
		default:
			return;
		}
	}

	public void OnUpdateRemote(float dt)
	{
		switch (this.currBehavior)
		{
		case GREnemySummoner.Behavior.Wander:
			this.abilityWander.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.Stagger:
			this.abilityStagger.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.Destroyed:
			this.abilityDie.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.Summon:
			this.abilitySummon.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.KeepDistance:
			this.abilityKeepDistance.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.MoveToTarget:
			this.abilityMoveToTarget.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.Investigate:
			this.abilityInvestigate.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.Jump:
			this.abilityJump.UpdateRemote(dt);
			return;
		case GREnemySummoner.Behavior.Flashed:
			this.abilityFlashed.UpdateRemote(dt);
			return;
		default:
			return;
		}
	}

	public void OnGameEntitySerialize(BinaryWriter writer)
	{
		byte b = (byte)this.currBehavior;
		byte b2 = (byte)this.currBodyState;
		writer.Write(b);
		writer.Write(this.hp);
		writer.Write(b2);
	}

	public void OnGameEntityDeserialize(BinaryReader reader)
	{
		GREnemySummoner.Behavior behavior = (GREnemySummoner.Behavior)reader.ReadByte();
		int num = reader.ReadInt32();
		GREnemySummoner.BodyState bodyState = (GREnemySummoner.BodyState)reader.ReadByte();
		this.SetHP(num);
		this.SetBehavior(behavior, true);
		this.SetBodyState(bodyState, true);
	}

	public bool IsHitValid(GameHitData hit)
	{
		return true;
	}

	public void OnHit(GameHitData hit)
	{
		GameHitType hitTypeId = (GameHitType)hit.hitTypeId;
		GRTool gameComponent = this.entity.manager.GetGameComponent<GRTool>(hit.hitByEntityId);
		if (gameComponent != null)
		{
			switch (hitTypeId)
			{
			case GameHitType.Club:
				this.OnHitByClub(gameComponent, hit);
				return;
			case GameHitType.Flash:
				this.OnHitByFlash(gameComponent, hit);
				return;
			case GameHitType.Shield:
				this.OnHitByShield(gameComponent, hit);
				break;
			default:
				return;
			}
		}
	}

	public void OnHitByClub(GRTool tool, GameHitData hit)
	{
		if (this.currBehavior == GREnemySummoner.Behavior.Destroyed)
		{
			return;
		}
		if (this.currBodyState != GREnemySummoner.BodyState.Bones)
		{
			if (this.currBodyState == GREnemySummoner.BodyState.Shell && this.armor != null)
			{
				this.armor.PlayBlockFx(hit.hitEntityPosition);
			}
			return;
		}
		this.hp -= hit.hitAmount;
		if (this.hp <= 0)
		{
			this.abilityDie.SetInstigatingPlayerIndex(this.entity.GetLastHeldByPlayerForEntityID(hit.hitByEntityId));
			this.abilityDie.SetStaggerVelocity(hit.hitImpulse);
			this.SetBehavior(GREnemySummoner.Behavior.Destroyed, false);
			return;
		}
		this.abilityStagger.SetStaggerVelocity(hit.hitImpulse);
		this.TrySetBehavior(GREnemySummoner.Behavior.Stagger);
	}

	public void OnHitByFlash(GRTool tool, GameHitData hit)
	{
		this.abilityFlashed.SetStaggerVelocity(hit.hitImpulse);
		if (this.currBodyState == GREnemySummoner.BodyState.Shell)
		{
			this.hp -= hit.hitAmount;
			if (this.armor != null)
			{
				this.armor.SetHp(this.hp);
			}
			if (this.hp <= 0)
			{
				if (this.armor != null)
				{
					this.armor.PlayDestroyFx(this.armor.transform.position);
				}
				this.SetBodyState(GREnemySummoner.BodyState.Bones, false);
				if (tool.gameEntity.IsHeldByLocalPlayer())
				{
					PlayerGameEvents.MiscEvent("GRArmorBreak_" + base.name, 1);
				}
				if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.FlashDamage3))
				{
					this.armor.FragmentArmor();
				}
			}
			else
			{
				if (this.armor != null)
				{
					this.armor.PlayHitFx(this.armor.transform.position);
				}
				this.RefreshBody();
			}
		}
		GRToolFlash component = tool.GetComponent<GRToolFlash>();
		if (component != null)
		{
			this.abilityFlashed.SetStunTime(component.stunDuration);
		}
		this.TrySetBehavior(GREnemySummoner.Behavior.Flashed);
	}

	public void OnHitByShield(GRTool tool, GameHitData hit)
	{
		this.OnHitByClub(tool, hit);
	}

	private void OnTriggerEnter(Collider collider)
	{
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			GRPlayer component = attachedRigidbody.GetComponent<GRPlayer>();
			if (component != null && component.gamePlayer.IsLocal())
			{
				GhostReactorManager.Get(this.entity).RequestEnemyHitPlayer(GhostReactor.EnemyType.Phantom, this.entity.id, component, base.transform.position);
			}
			GRBreakable component2 = attachedRigidbody.GetComponent<GRBreakable>();
			GameHittable component3 = attachedRigidbody.GetComponent<GameHittable>();
			if (component2 != null && component3 != null)
			{
				GameHitData gameHitData = new GameHitData
				{
					hitTypeId = 0,
					hitEntityId = component3.gameEntity.id,
					hitByEntityId = this.entity.id,
					hitEntityPosition = component2.transform.position,
					hitImpulse = Vector3.zero,
					hitPosition = component2.transform.position
				};
				component3.RequestHit(gameHitData);
			}
		}
	}

	private void RefreshBody()
	{
		switch (this.currBodyState)
		{
		case GREnemySummoner.BodyState.Destroyed:
			this.armor.SetHp(0);
			return;
		case GREnemySummoner.BodyState.Bones:
			this.armor.SetHp(0);
			GREnemy.HideRenderers(this.bones, false);
			GREnemy.HideRenderers(this.always, false);
			GREnemy.HideObjects(this.bonesStateVisibleObjects, false);
			GREnemy.HideObjects(this.alwaysVisibleObjects, false);
			return;
		case GREnemySummoner.BodyState.Shell:
			this.armor.SetHp(this.hp);
			GREnemy.HideRenderers(this.bones, true);
			GREnemy.HideRenderers(this.always, false);
			GREnemy.HideObjects(this.bonesStateVisibleObjects, true);
			GREnemy.HideObjects(this.alwaysVisibleObjects, false);
			return;
		default:
			return;
		}
	}

	public void SetBodyState(GREnemySummoner.BodyState newBodyState, bool force = false)
	{
		if (this.currBodyState == newBodyState && !force)
		{
			return;
		}
		switch (this.currBodyState)
		{
		case GREnemySummoner.BodyState.Bones:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax);
			break;
		case GREnemySummoner.BodyState.Shell:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.ArmorMax);
			break;
		}
		this.currBodyState = newBodyState;
		switch (this.currBodyState)
		{
		case GREnemySummoner.BodyState.Destroyed:
			GhostReactorManager.Get(this.entity).ReportEnemyDeath();
			break;
		case GREnemySummoner.BodyState.Bones:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax);
			break;
		case GREnemySummoner.BodyState.Shell:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.ArmorMax);
			break;
		}
		this.RefreshBody();
		if (this.entity.IsAuthority())
		{
			this.agent.RequestStateChange((byte)newBodyState);
		}
	}

	public void GetDebugTextLines(out List<string> strings)
	{
		strings = new List<string>();
		strings.Add(string.Format("State: <color=\"yellow\">{0}<color=\"white\"> HP: <color=\"yellow\">{1}<color=\"white\">", this.currBehavior.ToString(), this.hp));
		strings.Add(string.Format("Nearby rigs: <color=\"yellow\">{0}<color=\"white\">", this.senseNearby.rigsNearby.Count));
		strings.Add(string.Format("Spawned entities: <color=\"yellow\">{0}<color=\"white\">", this.trackedEntities.Count));
	}

	public void AddTrackedEntity(GameEntity entityToTrack)
	{
		int netId = entityToTrack.GetNetId();
		this.trackedEntities.AddIfNew(netId);
	}

	public void RemoveTrackedEntity(GameEntity entityToRemove)
	{
		int netId = entityToRemove.GetNetId();
		if (this.trackedEntities.Contains(netId))
		{
			this.trackedEntities.Remove(netId);
		}
	}

	public void OnSummonedEntityInit(GameEntity entity)
	{
		this.AddTrackedEntity(entity);
	}

	public void OnSummonedEntityDestroy(GameEntity entity)
	{
		this.RemoveTrackedEntity(entity);
	}

	private GameEntity entity;

	private GameAgent agent;

	public GRArmorEnemy armor;

	public GRAttributes attributes;

	public Animation anim;

	public GRSenseNearby senseNearby;

	public GRSenseLineOfSight senseLineOfSight;

	public GRAbilityIdle abilityIdle;

	public GRAbilityWander abilityWander;

	public GRAbilityAttackJump abilityAttack;

	public GRAbilityStagger abilityStagger;

	public GRAbilityDie abilityDie;

	public GRAbilitySummon abilitySummon;

	public GRAbilityKeepDistance abilityKeepDistance;

	public GRAbilityMoveToTarget abilityMoveToTarget;

	public GRAbilityMoveToTarget abilityInvestigate;

	public GRAbilityJump abilityJump;

	public GRAbilityStagger abilityFlashed;

	public AbilitySound soundWander;

	public AbilitySound soundAttack;

	public GameLight summonLight;

	public List<Renderer> bones;

	public List<Renderer> always;

	public List<GameObject> bonesStateVisibleObjects;

	public List<GameObject> alwaysVisibleObjects;

	public Transform coreMarker;

	public GRCollectible corePrefab;

	public Transform headTransform;

	public float attackRange = 2f;

	public List<VRRig> rigsNearby;

	public NavMeshAgent navAgent;

	public AudioSource audioSource;

	public float idleDuration = 2f;

	public float keepDistanceThreshold = 3f;

	public float tooFarDistanceThreshold = 5f;

	public double lastSummonTime;

	public float minSummonInterval = 4f;

	public int maxSimultaneousSummonedEntities = 3;

	public float hearingRadius = 7f;

	[ReadOnly]
	public int hp;

	[ReadOnly]
	public GREnemySummoner.Behavior currBehavior;

	[ReadOnly]
	public double behaviorEndTime;

	[ReadOnly]
	public GREnemySummoner.BodyState currBodyState;

	[ReadOnly]
	public Vector3 searchPosition;

	[ReadOnly]
	public double behaviorStartTime;

	private Rigidbody rigidBody;

	private List<Collider> colliders;

	private List<int> trackedEntities;

	private Vector3? investigateLocation;

	private float lastUpdateTime;

	private static List<VRRig> tempRigs = new List<VRRig>(16);

	public enum Behavior
	{
		Idle,
		Wander,
		Stagger,
		Destroyed,
		Summon,
		KeepDistance,
		MoveToTarget,
		Investigate,
		Jump,
		Flashed,
		Count
	}

	public enum BodyState
	{
		Destroyed,
		Bones,
		Shell,
		Count
	}
}
