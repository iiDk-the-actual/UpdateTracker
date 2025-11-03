using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public class GREnemyPest : MonoBehaviour, IGameEntityComponent, IGameEntitySerialize, IGameHittable, IGameAgentComponent, IGameEntityDebugComponent, ITickSystemTick
{
	public bool TickRunning { get; set; }

	private void Awake()
	{
		this.rigidBody = base.GetComponent<Rigidbody>();
		this.colliders = new List<Collider>(4);
		base.GetComponentsInChildren<Collider>(this.colliders);
		if (this.armor != null)
		{
			this.armor.SetHp(0);
		}
		this.navAgent.updateRotation = false;
		this.behaviorStartTime = -1.0;
		this.agent.onBehaviorStateChanged += this.OnNetworkBehaviorStateChange;
		this.senseNearby.Setup(this.headTransform);
		GameEntity gameEntity = this.entity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.entity;
		gameEntity2.OnReleased = (Action)Delegate.Combine(gameEntity2.OnReleased, new Action(this.OnReleased));
		base.Invoke("PlaySpawnAudio", 0.1f);
	}

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	private void PlaySpawnAudio()
	{
		this.spawnSound.Play(null);
	}

	public void OnEntityInit()
	{
		this.abilityIdle.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityChase.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityAttack.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityWander.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityDie.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityGrabbed.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityThrown.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityStagger.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityFlashed.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityInvestigate.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityJump.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.SetBehavior(GREnemyPest.Behavior.Wander, false);
		if (this.entity && this.entity.manager && this.entity.manager.ghostReactorManager && this.entity.manager.ghostReactorManager.reactor)
		{
			foreach (GRBonusEntry grbonusEntry in this.entity.manager.ghostReactorManager.reactor.GetCurrLevelGenConfig().enemyGlobalBonuses)
			{
				this.attributes.AddBonus(grbonusEntry);
			}
		}
		this.navAgent.speed = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.PatrolSpeed);
		this.SetHP(this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax));
		this.agent.navAgent.autoTraverseOffMeshLink = false;
		this.agent.onJumpRequested += this.OnAgentJumpRequested;
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private void OnDestroy()
	{
		this.agent.onBehaviorStateChanged -= this.OnNetworkBehaviorStateChange;
	}

	private void OnAgentJumpRequested(Vector3 start, Vector3 end)
	{
		this.abilityJump.SetupJump(start, end);
		this.SetBehavior(GREnemyPest.Behavior.Jump, false);
	}

	public void OnNetworkBehaviorStateChange(byte newState)
	{
		if (newState < 0 || newState >= 11)
		{
			return;
		}
		this.SetBehavior((GREnemyPest.Behavior)newState, false);
	}

	public void SetHP(int hp)
	{
		this.hp = hp;
	}

	public bool TrySetBehavior(GREnemyPest.Behavior newBehavior)
	{
		if (this.currBehavior == GREnemyPest.Behavior.Jump && newBehavior == GREnemyPest.Behavior.Stagger)
		{
			return false;
		}
		this.SetBehavior(newBehavior, false);
		return true;
	}

	public void SetBehavior(GREnemyPest.Behavior newBehavior, bool force = false)
	{
		if (this.currBehavior == newBehavior && !force)
		{
			return;
		}
		switch (this.currBehavior)
		{
		case GREnemyPest.Behavior.Idle:
			this.abilityIdle.Stop();
			break;
		case GREnemyPest.Behavior.Wander:
			this.abilityWander.Stop();
			break;
		case GREnemyPest.Behavior.Chase:
			this.abilityChase.Stop();
			break;
		case GREnemyPest.Behavior.Attack:
			this.abilityAttack.Stop();
			break;
		case GREnemyPest.Behavior.Stagger:
			this.abilityStagger.Stop();
			break;
		case GREnemyPest.Behavior.Grabbed:
			this.abilityGrabbed.Stop();
			break;
		case GREnemyPest.Behavior.Thrown:
			this.abilityThrown.Stop();
			break;
		case GREnemyPest.Behavior.Destroyed:
			this.abilityDie.Stop();
			break;
		case GREnemyPest.Behavior.Investigate:
			this.abilityInvestigate.Stop();
			break;
		case GREnemyPest.Behavior.Jump:
			this.abilityJump.Stop();
			break;
		case GREnemyPest.Behavior.Flashed:
			this.abilityFlashed.Stop();
			break;
		}
		this.currBehavior = newBehavior;
		this.behaviorStartTime = Time.timeAsDouble;
		switch (this.currBehavior)
		{
		case GREnemyPest.Behavior.Idle:
			this.abilityIdle.Start();
			break;
		case GREnemyPest.Behavior.Wander:
			this.abilityWander.Start();
			break;
		case GREnemyPest.Behavior.Chase:
			this.abilityChase.Start();
			this.abilityChase.SetTargetPlayer(this.agent.targetPlayer);
			break;
		case GREnemyPest.Behavior.Attack:
			this.abilityAttack.Start();
			this.abilityAttack.SetTargetPlayer(this.agent.targetPlayer);
			break;
		case GREnemyPest.Behavior.Stagger:
			this.abilityStagger.Start();
			break;
		case GREnemyPest.Behavior.Grabbed:
			this.abilityGrabbed.Start();
			break;
		case GREnemyPest.Behavior.Thrown:
			this.abilityThrown.Start();
			break;
		case GREnemyPest.Behavior.Destroyed:
			this.abilityDie.Start();
			break;
		case GREnemyPest.Behavior.Investigate:
			this.abilityInvestigate.Start();
			break;
		case GREnemyPest.Behavior.Jump:
			this.abilityJump.Start();
			break;
		case GREnemyPest.Behavior.Flashed:
			this.abilityFlashed.Start();
			break;
		}
		if (this.entity.IsAuthority())
		{
			this.agent.RequestBehaviorChange((byte)this.currBehavior);
		}
	}

	private void OnGrabbed()
	{
		if (this.currBehavior == GREnemyPest.Behavior.Destroyed)
		{
			return;
		}
		this.SetBehavior(GREnemyPest.Behavior.Grabbed, false);
	}

	private void OnReleased()
	{
		if (this.currBehavior == GREnemyPest.Behavior.Destroyed)
		{
			return;
		}
		this.SetBehavior(GREnemyPest.Behavior.Thrown, false);
	}

	public void Tick()
	{
		this.OnUpdate(Time.deltaTime);
	}

	public void OnEntityThink(float dt)
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		GREnemyPest.tempRigs.Clear();
		GREnemyPest.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GREnemyPest.tempRigs);
		this.senseNearby.UpdateNearby(GREnemyPest.tempRigs, this.senseLineOfSight);
		float num;
		VRRig vrrig = this.senseNearby.PickClosest(out num);
		this.agent.RequestTarget((vrrig == null) ? null : vrrig.OwningNetPlayer);
		GREnemyPest.Behavior behavior = this.currBehavior;
		switch (behavior)
		{
		case GREnemyPest.Behavior.Idle:
			this.ChooseNewBehavior();
			return;
		case GREnemyPest.Behavior.Wander:
			this.abilityWander.Think(dt);
			this.ChooseNewBehavior();
			return;
		case GREnemyPest.Behavior.Chase:
			if (this.agent.targetPlayer != null)
			{
				this.abilityChase.SetTargetPlayer(this.agent.targetPlayer);
			}
			this.abilityChase.Think(dt);
			return;
		default:
			if (behavior != GREnemyPest.Behavior.Investigate)
			{
				return;
			}
			this.abilityInvestigate.Think(dt);
			this.ChooseNewBehavior();
			return;
		}
	}

	private void ChooseNewBehavior()
	{
		if (!GhostReactorManager.AggroDisabled && this.senseNearby.IsAnyoneNearby())
		{
			this.investigateLocation = null;
			this.SetBehavior(GREnemyPest.Behavior.Chase, false);
			return;
		}
		this.investigateLocation = AbilityHelperFunctions.GetLocationToInvestigate(base.transform.position, this.hearingRadius, this.investigateLocation);
		if (this.investigateLocation != null)
		{
			this.abilityInvestigate.SetTargetPos(this.investigateLocation.Value);
			this.SetBehavior(GREnemyPest.Behavior.Investigate, false);
			return;
		}
		this.SetBehavior(GREnemyPest.Behavior.Wander, false);
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
		case GREnemyPest.Behavior.Idle:
			this.abilityIdle.Update(dt);
			return;
		case GREnemyPest.Behavior.Wander:
			this.abilityWander.Update(dt);
			return;
		case GREnemyPest.Behavior.Chase:
		{
			this.abilityChase.Update(dt);
			if (this.abilityChase.IsDone())
			{
				this.SetBehavior(GREnemyPest.Behavior.Wander, false);
				return;
			}
			GRPlayer grplayer = GRPlayer.Get(this.agent.targetPlayer);
			if (grplayer != null)
			{
				float num = this.attackRange * this.attackRange;
				if ((grplayer.transform.position - base.transform.position).sqrMagnitude < num)
				{
					this.SetBehavior(GREnemyPest.Behavior.Attack, false);
					return;
				}
			}
			break;
		}
		case GREnemyPest.Behavior.Attack:
			this.abilityAttack.Update(dt);
			if (this.abilityAttack.IsDone())
			{
				this.SetBehavior(GREnemyPest.Behavior.Chase, false);
				return;
			}
			break;
		case GREnemyPest.Behavior.Stagger:
			this.abilityStagger.Update(dt);
			if (this.abilityStagger.IsDone())
			{
				this.SetBehavior(GREnemyPest.Behavior.Wander, false);
				return;
			}
			break;
		case GREnemyPest.Behavior.Grabbed:
			break;
		case GREnemyPest.Behavior.Thrown:
			if (this.abilityThrown.IsDone())
			{
				this.SetBehavior(GREnemyPest.Behavior.Wander, false);
				return;
			}
			break;
		case GREnemyPest.Behavior.Destroyed:
			this.abilityDie.Update(dt);
			return;
		case GREnemyPest.Behavior.Investigate:
			this.abilityInvestigate.Update(dt);
			return;
		case GREnemyPest.Behavior.Jump:
			this.abilityJump.Update(dt);
			if (this.abilityJump.IsDone())
			{
				this.ChooseNewBehavior();
				return;
			}
			break;
		case GREnemyPest.Behavior.Flashed:
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
		case GREnemyPest.Behavior.Wander:
			this.abilityWander.UpdateRemote(dt);
			return;
		case GREnemyPest.Behavior.Chase:
			this.abilityChase.UpdateRemote(dt);
			return;
		case GREnemyPest.Behavior.Attack:
			this.abilityAttack.UpdateRemote(dt);
			return;
		case GREnemyPest.Behavior.Stagger:
			this.abilityStagger.UpdateRemote(dt);
			return;
		case GREnemyPest.Behavior.Grabbed:
		case GREnemyPest.Behavior.Thrown:
			break;
		case GREnemyPest.Behavior.Destroyed:
			this.abilityDie.UpdateRemote(dt);
			return;
		case GREnemyPest.Behavior.Investigate:
			this.abilityInvestigate.UpdateRemote(dt);
			return;
		case GREnemyPest.Behavior.Jump:
			this.abilityJump.UpdateRemote(dt);
			return;
		case GREnemyPest.Behavior.Flashed:
			this.abilityFlashed.UpdateRemote(dt);
			break;
		default:
			return;
		}
	}

	public void OnGameEntitySerialize(BinaryWriter writer)
	{
		byte b = (byte)this.currBehavior;
		writer.Write(b);
		writer.Write(this.hp);
	}

	public void OnGameEntityDeserialize(BinaryReader reader)
	{
		GREnemyPest.Behavior behavior = (GREnemyPest.Behavior)reader.ReadByte();
		int num = reader.ReadInt32();
		this.SetHP(num);
		this.SetBehavior(behavior, true);
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
				this.OnHitByClub(hit);
				return;
			case GameHitType.Flash:
				this.OnHitByFlash(gameComponent, hit);
				return;
			case GameHitType.Shield:
				this.OnHitByShield(hit);
				break;
			default:
				return;
			}
		}
	}

	public void OnHitByClub(GameHitData hit)
	{
		if (this.currBehavior == GREnemyPest.Behavior.Destroyed)
		{
			return;
		}
		this.hp -= hit.hitAmount;
		if (this.hp <= 0)
		{
			this.abilityDie.SetInstigatingPlayerIndex(this.entity.GetLastHeldByPlayerForEntityID(hit.hitByEntityId));
			this.SetBehavior(GREnemyPest.Behavior.Destroyed, false);
			return;
		}
		this.abilityStagger.SetStaggerVelocity(hit.hitImpulse);
		this.TrySetBehavior(GREnemyPest.Behavior.Stagger);
	}

	public void OnHitByFlash(GRTool tool, GameHitData hit)
	{
		this.abilityFlashed.SetStaggerVelocity(hit.hitImpulse);
		GRToolFlash component = tool.GetComponent<GRToolFlash>();
		if (component != null)
		{
			this.abilityFlashed.SetStunTime(component.stunDuration);
		}
		this.TrySetBehavior(GREnemyPest.Behavior.Flashed);
	}

	public void OnHitByShield(GameHitData hit)
	{
		this.OnHitByClub(hit);
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (this.currBehavior != GREnemyPest.Behavior.Attack)
		{
			return;
		}
		GRShieldCollider component = collider.GetComponent<GRShieldCollider>();
		if (component != null)
		{
			Vector3 vector = this.abilityAttack.targetPos - this.abilityAttack.initialPos;
			GameHittable component2 = base.GetComponent<GameHittable>();
			component.BlockHittable(base.transform.position, vector, component2);
			return;
		}
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			GRPlayer component3 = attachedRigidbody.GetComponent<GRPlayer>();
			if (component3 != null && component3.gamePlayer.IsLocal() && Time.time > this.lastHitPlayerTime + this.minTimeBetweenHits)
			{
				if (this.tryHitPlayerCoroutine != null)
				{
					base.StopCoroutine(this.tryHitPlayerCoroutine);
				}
				this.tryHitPlayerCoroutine = base.StartCoroutine(this.TryHitPlayer(component3));
			}
			GRBreakable component4 = attachedRigidbody.GetComponent<GRBreakable>();
			GameHittable component5 = attachedRigidbody.GetComponent<GameHittable>();
			if (component4 != null && component5 != null)
			{
				GameHitData gameHitData = new GameHitData
				{
					hitTypeId = 0,
					hitEntityId = component5.gameEntity.id,
					hitByEntityId = this.entity.id,
					hitEntityPosition = component4.transform.position,
					hitImpulse = Vector3.zero,
					hitPosition = component4.transform.position
				};
				component5.RequestHit(gameHitData);
			}
		}
	}

	private IEnumerator TryHitPlayer(GRPlayer player)
	{
		yield return new WaitForUpdate();
		if (this.currBehavior == GREnemyPest.Behavior.Attack && player != null && player.gamePlayer.IsLocal() && Time.time > this.lastHitPlayerTime + this.minTimeBetweenHits)
		{
			this.lastHitPlayerTime = Time.time;
			GhostReactorManager.Get(this.entity).RequestEnemyHitPlayer(GhostReactor.EnemyType.Chaser, this.entity.id, player, base.transform.position);
		}
		yield break;
	}

	public static void Hide(List<Renderer> renderers, bool hide)
	{
		if (renderers == null)
		{
			return;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].enabled = !hide;
			}
		}
	}

	public void GetDebugTextLines(out List<string> strings)
	{
		strings = new List<string>();
		strings.Add(string.Format("State: <color=\"yellow\">{0}<color=\"white\"> HP: <color=\"yellow\">{1}<color=\"white\">", this.currBehavior.ToString(), this.hp));
		float magnitude = (GRSenseNearby.GetRigTestLocation(VRRig.LocalRig) - base.transform.position).magnitude;
		bool flag = GRSenseLineOfSight.HasGeoLineOfSight(this.headTransform.position, GRSenseNearby.GetRigTestLocation(VRRig.LocalRig), this.senseLineOfSight.sightDist, this.senseLineOfSight.visibilityMask);
		strings.Add(string.Format("player rig dis: {0} has los: {1}", magnitude, flag));
	}

	public GameEntity entity;

	public GameAgent agent;

	public GRArmorEnemy armor;

	public GRAttributes attributes;

	public Animation anim;

	public GRSenseNearby senseNearby;

	public GRSenseLineOfSight senseLineOfSight;

	public GRAbilityIdle abilityIdle;

	public GRAbilityChase abilityChase;

	public GRAbilityWander abilityWander;

	public GRAbilityAttackJump abilityAttack;

	public GRAbilityStagger abilityStagger;

	public GRAbilityStagger abilityFlashed;

	public GRAbilityDie abilityDie;

	public GRAbilityGrabbed abilityGrabbed;

	public GRAbilityThrown abilityThrown;

	public AbilitySound spawnSound;

	public GRAbilityMoveToTarget abilityInvestigate;

	public GRAbilityJump abilityJump;

	public List<Renderer> bones;

	public List<Renderer> always;

	public Transform coreMarker;

	public GRCollectible corePrefab;

	public Transform headTransform;

	public float attackRange = 2f;

	public List<VRRig> rigsNearby;

	public NavMeshAgent navAgent;

	public AudioSource audioSource;

	public float hearingRadius = 5f;

	private Vector3? investigateLocation;

	[ReadOnly]
	public int hp;

	[ReadOnly]
	public GREnemyPest.Behavior currBehavior;

	[ReadOnly]
	public double behaviorEndTime;

	[ReadOnly]
	public GREnemyPest.BodyState currBodyState;

	[ReadOnly]
	public int nextPatrolNode;

	[ReadOnly]
	public Vector3 searchPosition;

	[ReadOnly]
	public double behaviorStartTime;

	private Rigidbody rigidBody;

	private List<Collider> colliders;

	private float lastHitPlayerTime;

	private float minTimeBetweenHits = 0.5f;

	private static List<VRRig> tempRigs = new List<VRRig>(16);

	private Coroutine tryHitPlayerCoroutine;

	public enum Behavior
	{
		Idle,
		Wander,
		Chase,
		Attack,
		Stagger,
		Grabbed,
		Thrown,
		Destroyed,
		Investigate,
		Jump,
		Flashed,
		Count
	}

	public enum BodyState
	{
		Destroyed,
		Bones,
		Count
	}
}
