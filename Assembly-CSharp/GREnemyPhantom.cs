using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public class GREnemyPhantom : MonoBehaviour, IGameEntityComponent, IGameEntitySerialize, IGameAgentComponent, IGameEntityDebugComponent
{
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
		this.agent.onBodyStateChanged += this.OnNetworkBodyStateChange;
		this.agent.onBehaviorStateChanged += this.OnNetworkBehaviorStateChange;
		this.senseNearby.Setup(this.headTransform);
	}

	public void OnEntityInit()
	{
		this.abilityMine.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityIdle.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityRage.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityAlert.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityChase.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityReturn.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityAttack.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityInvestigate.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		this.abilityJump.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, this.senseLineOfSight);
		int num = (int)this.entity.createData;
		this.Setup((long)num);
		if (this.entity && this.entity.manager && this.entity.manager.ghostReactorManager && this.entity.manager.ghostReactorManager.reactor)
		{
			foreach (GRBonusEntry grbonusEntry in this.entity.manager.ghostReactorManager.reactor.GetCurrLevelGenConfig().enemyGlobalBonuses)
			{
				this.attributes.AddBonus(grbonusEntry);
			}
		}
		this.navAgent.speed = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.PatrolSpeed);
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
		this.agent.onBodyStateChanged -= this.OnNetworkBodyStateChange;
		this.agent.onBehaviorStateChanged -= this.OnNetworkBehaviorStateChange;
	}

	private void Setup(long createData)
	{
		this.SetPatrolPath(createData);
		if (this.patrolPath != null && this.patrolPath.patrolNodes.Count > 0)
		{
			this.nextPatrolNode = 0;
			this.target = this.patrolPath.patrolNodes[0];
			this.idleLocation = this.target;
			this.SetBehavior(GREnemyPhantom.Behavior.Return, true);
		}
		else
		{
			this.SetBehavior(GREnemyPhantom.Behavior.Mine, true);
		}
		this.SetBodyState(GREnemyPhantom.BodyState.Bones, true);
		if (this.attackLight != null)
		{
			this.attackLight.gameObject.SetActive(false);
		}
		if (this.negativeLight != null)
		{
			this.negativeLight.gameObject.SetActive(false);
		}
		GREnemyPhantom.Hide(this.bones, false);
		GREnemyPhantom.Hide(this.always, false);
	}

	private void OnAgentJumpRequested(Vector3 start, Vector3 end)
	{
		this.abilityJump.SetupJump(start, end);
		this.SetBehavior(GREnemyPhantom.Behavior.Jump, false);
	}

	public void OnNetworkBehaviorStateChange(byte newState)
	{
		if (newState < 0 || newState >= 9)
		{
			return;
		}
		this.SetBehavior((GREnemyPhantom.Behavior)newState, false);
	}

	public void OnNetworkBodyStateChange(byte newState)
	{
		if (newState < 0 || newState >= 2)
		{
			return;
		}
		this.SetBodyState((GREnemyPhantom.BodyState)newState, false);
	}

	public void SetPatrolPath(long createData)
	{
		GRPatrolPath grpatrolPath = GhostReactorManager.Get(this.entity).reactor.GetPatrolPath(createData);
		this.patrolPath = grpatrolPath;
	}

	public void SetNextPatrolNode(int nextPatrolNode)
	{
		this.nextPatrolNode = nextPatrolNode;
	}

	public void SetHP(int hp)
	{
		this.hp = hp;
	}

	public void SetBehavior(GREnemyPhantom.Behavior newBehavior, bool force = false)
	{
		if (this.currBehavior == newBehavior && !force)
		{
			return;
		}
		this.lastStateChange = PhotonNetwork.Time;
		switch (this.currBehavior)
		{
		case GREnemyPhantom.Behavior.Mine:
			this.abilityMine.Stop();
			break;
		case GREnemyPhantom.Behavior.Idle:
			this.abilityIdle.Stop();
			break;
		case GREnemyPhantom.Behavior.Alert:
			this.abilityAlert.Stop();
			break;
		case GREnemyPhantom.Behavior.Return:
			this.abilityReturn.Stop();
			break;
		case GREnemyPhantom.Behavior.Rage:
			this.abilityRage.Stop();
			break;
		case GREnemyPhantom.Behavior.Chase:
			this.abilityChase.Stop();
			if (this.negativeLight != null)
			{
				this.negativeLight.gameObject.SetActive(false);
			}
			break;
		case GREnemyPhantom.Behavior.Attack:
			this.abilityAttack.Stop();
			if (this.attackLight != null)
			{
				this.attackLight.gameObject.SetActive(false);
			}
			break;
		case GREnemyPhantom.Behavior.Investigate:
			this.abilityInvestigate.Stop();
			break;
		case GREnemyPhantom.Behavior.Jump:
			this.abilityJump.Stop();
			break;
		}
		this.currBehavior = newBehavior;
		this.behaviorStartTime = Time.timeAsDouble;
		switch (this.currBehavior)
		{
		case GREnemyPhantom.Behavior.Mine:
			this.abilityMine.Start();
			break;
		case GREnemyPhantom.Behavior.Idle:
			this.abilityIdle.Start();
			break;
		case GREnemyPhantom.Behavior.Alert:
			this.abilityAlert.Start();
			this.soundAlert.Play(this.audioSource);
			break;
		case GREnemyPhantom.Behavior.Return:
			this.abilityReturn.Start();
			this.soundReturn.Play(this.audioSource);
			this.abilityReturn.SetTarget(this.idleLocation);
			break;
		case GREnemyPhantom.Behavior.Rage:
			this.abilityRage.Start();
			this.soundRage.Play(this.audioSource);
			break;
		case GREnemyPhantom.Behavior.Chase:
			this.abilityChase.Start();
			this.soundChase.Play(this.audioSource);
			this.abilityChase.SetTargetPlayer(this.agent.targetPlayer);
			this.investigateLocation = null;
			if (this.negativeLight != null)
			{
				this.negativeLight.gameObject.SetActive(true);
			}
			break;
		case GREnemyPhantom.Behavior.Attack:
			this.abilityAttack.Start();
			this.abilityAttack.SetTargetPlayer(this.agent.targetPlayer);
			this.investigateLocation = null;
			this.soundAttack.Play(this.audioSource);
			if (this.attackLight != null)
			{
				this.attackLight.gameObject.SetActive(true);
			}
			break;
		case GREnemyPhantom.Behavior.Investigate:
			this.abilityInvestigate.Start();
			break;
		case GREnemyPhantom.Behavior.Jump:
			this.abilityJump.Start();
			break;
		}
		this.RefreshBody();
		if (this.entity.IsAuthority())
		{
			this.agent.RequestBehaviorChange((byte)this.currBehavior);
		}
	}

	public void SetBodyState(GREnemyPhantom.BodyState newBodyState, bool force = false)
	{
		if (this.currBodyState == newBodyState && !force)
		{
			return;
		}
		if (this.currBodyState == GREnemyPhantom.BodyState.Bones)
		{
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax);
		}
		this.currBodyState = newBodyState;
		if (this.currBodyState == GREnemyPhantom.BodyState.Bones)
		{
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax);
		}
		this.RefreshBody();
		if (this.entity.IsAuthority())
		{
			this.agent.RequestStateChange((byte)newBodyState);
		}
	}

	private void RefreshBody()
	{
		GREnemyPhantom.BodyState bodyState = this.currBodyState;
		if (bodyState == GREnemyPhantom.BodyState.Destroyed)
		{
			this.armor.SetHp(0);
			return;
		}
		if (bodyState != GREnemyPhantom.BodyState.Bones)
		{
			return;
		}
		this.armor.SetHp(0);
	}

	private void Update()
	{
		this.OnUpdate(Time.deltaTime);
	}

	private void ChooseNewBehavior()
	{
		if (!GhostReactorManager.AggroDisabled && this.senseNearby.IsAnyoneNearby())
		{
			this.investigateLocation = null;
			this.SetBehavior(GREnemyPhantom.Behavior.Alert, false);
			return;
		}
		this.investigateLocation = AbilityHelperFunctions.GetLocationToInvestigate(base.transform.position, this.hearingRadius, this.investigateLocation);
		if (this.investigateLocation != null)
		{
			this.abilityInvestigate.SetTargetPos(this.investigateLocation.Value);
			this.SetBehavior(GREnemyPhantom.Behavior.Investigate, false);
			return;
		}
		if (this.currBehavior == GREnemyPhantom.Behavior.Investigate)
		{
			if (this.idleLocation != null)
			{
				this.SetBehavior(GREnemyPhantom.Behavior.Return, false);
				return;
			}
			this.SetBehavior(GREnemyPhantom.Behavior.Idle, false);
		}
	}

	public void OnEntityThink(float dt)
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		GREnemyPhantom.tempRigs.Clear();
		GREnemyPhantom.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GREnemyPhantom.tempRigs);
		this.senseNearby.UpdateNearby(GREnemyPhantom.tempRigs, this.senseLineOfSight);
		float num;
		VRRig vrrig = this.senseNearby.PickClosest(out num);
		this.agent.RequestTarget((vrrig == null) ? null : vrrig.OwningNetPlayer);
		switch (this.currBehavior)
		{
		case GREnemyPhantom.Behavior.Mine:
			this.ChooseNewBehavior();
			return;
		case GREnemyPhantom.Behavior.Idle:
			this.ChooseNewBehavior();
			return;
		case GREnemyPhantom.Behavior.Alert:
		case GREnemyPhantom.Behavior.Rage:
		case GREnemyPhantom.Behavior.Attack:
			break;
		case GREnemyPhantom.Behavior.Return:
			this.abilityReturn.SetTarget(this.idleLocation);
			this.abilityReturn.Think(dt);
			this.ChooseNewBehavior();
			return;
		case GREnemyPhantom.Behavior.Chase:
			if (this.agent.targetPlayer != null)
			{
				this.abilityChase.SetTargetPlayer(this.agent.targetPlayer);
			}
			this.abilityChase.Think(dt);
			return;
		case GREnemyPhantom.Behavior.Investigate:
			this.abilityInvestigate.Think(dt);
			this.ChooseNewBehavior();
			break;
		default:
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
		case GREnemyPhantom.Behavior.Mine:
			this.abilityMine.Update(dt);
			if (this.idleLocation != null)
			{
				GameAgent.UpdateFacingDir(base.transform, this.agent.navAgent, this.idleLocation.forward, 180f);
				return;
			}
			break;
		case GREnemyPhantom.Behavior.Idle:
			this.abilityIdle.Update(dt);
			return;
		case GREnemyPhantom.Behavior.Alert:
			this.UpdateAlert(dt);
			return;
		case GREnemyPhantom.Behavior.Return:
			this.abilityReturn.Update(dt);
			if (this.abilityReturn.IsDone())
			{
				this.SetBehavior(GREnemyPhantom.Behavior.Mine, false);
				return;
			}
			break;
		case GREnemyPhantom.Behavior.Rage:
			this.abilityRage.Update(dt);
			if (this.abilityRage.IsDone())
			{
				this.SetBehavior(GREnemyPhantom.Behavior.Chase, false);
				return;
			}
			break;
		case GREnemyPhantom.Behavior.Chase:
		{
			this.abilityChase.Update(dt);
			if (this.abilityChase.IsDone())
			{
				this.SetBehavior(GREnemyPhantom.Behavior.Return, false);
				return;
			}
			GRPlayer grplayer = GRPlayer.Get(this.agent.targetPlayer);
			if (grplayer != null)
			{
				float num = this.attackRange * this.attackRange;
				if ((grplayer.transform.position - base.transform.position).sqrMagnitude < num)
				{
					this.SetBehavior(GREnemyPhantom.Behavior.Attack, false);
					return;
				}
			}
			break;
		}
		case GREnemyPhantom.Behavior.Attack:
			this.abilityAttack.Update(dt);
			if (this.abilityAttack.IsDone())
			{
				this.SetBehavior(GREnemyPhantom.Behavior.Chase, false);
				return;
			}
			break;
		case GREnemyPhantom.Behavior.Investigate:
			this.abilityInvestigate.Update(dt);
			return;
		case GREnemyPhantom.Behavior.Jump:
			this.abilityJump.Update(dt);
			if (this.abilityJump.IsDone())
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
		case GREnemyPhantom.Behavior.Return:
			this.abilityReturn.UpdateRemote(dt);
			return;
		case GREnemyPhantom.Behavior.Rage:
			break;
		case GREnemyPhantom.Behavior.Chase:
			this.abilityChase.UpdateRemote(dt);
			return;
		case GREnemyPhantom.Behavior.Attack:
			this.abilityAttack.UpdateRemote(dt);
			return;
		case GREnemyPhantom.Behavior.Investigate:
			this.abilityInvestigate.UpdateRemote(dt);
			return;
		case GREnemyPhantom.Behavior.Jump:
			this.abilityJump.UpdateRemote(dt);
			break;
		default:
			return;
		}
	}

	public void UpdateAlert(float dt)
	{
		this.abilityAlert.SetTargetPlayer(this.agent.targetPlayer);
		this.abilityAlert.Update(dt);
		double timeAsDouble = Time.timeAsDouble;
		if (!this.senseNearby.IsAnyoneNearby())
		{
			this.SetBehavior(GREnemyPhantom.Behavior.Return, false);
			return;
		}
		float num;
		if (this.abilityAlert.IsDone() && this.senseNearby.PickClosest(out num) != null)
		{
			this.SetBehavior(GREnemyPhantom.Behavior.Rage, false);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (this.currBodyState == GREnemyPhantom.BodyState.Destroyed)
		{
			return;
		}
		if (this.currBehavior != GREnemyPhantom.Behavior.Attack)
		{
			return;
		}
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
	}

	public void OnGameEntitySerialize(BinaryWriter writer)
	{
		byte b = (byte)this.currBehavior;
		byte b2 = (byte)this.currBodyState;
		byte b3 = (byte)this.nextPatrolNode;
		writer.Write(b);
		writer.Write(b2);
		writer.Write(this.hp);
		writer.Write(b3);
	}

	public void OnGameEntityDeserialize(BinaryReader reader)
	{
		GREnemyPhantom.Behavior behavior = (GREnemyPhantom.Behavior)reader.ReadByte();
		GREnemyPhantom.BodyState bodyState = (GREnemyPhantom.BodyState)reader.ReadByte();
		int num = reader.ReadInt32();
		byte b = reader.ReadByte();
		this.SetPatrolPath(this.entity.createData);
		this.SetNextPatrolNode((int)b);
		this.SetHP(num);
		this.SetBehavior(behavior, true);
		this.SetBodyState(bodyState, true);
	}

	public GameEntity entity;

	public GameAgent agent;

	public GRArmorEnemy armor;

	public GRAttributes attributes;

	public Animation anim;

	public GRSenseNearby senseNearby;

	public GRSenseLineOfSight senseLineOfSight;

	public GRAbilityIdle abilityMine;

	public AbilitySound soundMine;

	public GRAbilityIdle abilityIdle;

	public GRAbilityWatch abilityRage;

	public AbilitySound soundRage;

	public GRAbilityWatch abilityAlert;

	public AbilitySound soundAlert;

	public GRAbilityChase abilityChase;

	public AbilitySound soundChase;

	public GRAbilityMoveToTarget abilityReturn;

	public AbilitySound soundReturn;

	public GRAbilityAttackLatchOn abilityAttack;

	public AbilitySound soundAttack;

	public GRAbilityMoveToTarget abilityInvestigate;

	public GRAbilityJump abilityJump;

	public List<Renderer> bones;

	public List<Renderer> always;

	public Transform coreMarker;

	public GRCollectible corePrefab;

	public Transform headTransform;

	public float attackRange = 2f;

	public float hearingRadius = 7f;

	public List<VRRig> rigsNearby;

	public GameLight attackLight;

	public GameLight negativeLight;

	[ReadOnly]
	[SerializeField]
	private GRPatrolPath patrolPath;

	private Transform idleLocation;

	public NavMeshAgent navAgent;

	public AudioSource audioSource;

	public double lastStateChange;

	private Vector3? investigateLocation;

	private Transform target;

	[ReadOnly]
	public int hp;

	[ReadOnly]
	public GREnemyPhantom.Behavior currBehavior;

	[ReadOnly]
	public double behaviorEndTime;

	[ReadOnly]
	public GREnemyPhantom.BodyState currBodyState;

	[ReadOnly]
	public int nextPatrolNode;

	[ReadOnly]
	public Vector3 searchPosition;

	[ReadOnly]
	public double behaviorStartTime;

	private Rigidbody rigidBody;

	private List<Collider> colliders;

	private static List<VRRig> tempRigs = new List<VRRig>(16);

	public enum Behavior
	{
		Mine,
		Idle,
		Alert,
		Return,
		Rage,
		Chase,
		Attack,
		Investigate,
		Jump,
		Count
	}

	public enum BodyState
	{
		Destroyed,
		Bones,
		Count
	}
}
