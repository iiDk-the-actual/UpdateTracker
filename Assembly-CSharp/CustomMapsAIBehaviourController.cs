using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaGameModes;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GT_CustomMapSupportRuntime;
using UnityEngine;
using UnityEngine.AI;

public class CustomMapsAIBehaviourController : MonoBehaviour, IGameEntityComponent
{
	public GRPlayer TargetPlayer { get; private set; }

	private void Awake()
	{
		this.TargetPlayer = null;
		this.visibilityLayerMask = LayerMask.GetMask(new string[] { "Default", "Gorilla Object" });
		this.agent.onBehaviorStateChanged += this.OnNetworkBehaviourStateChanged;
	}

	private void OnDestroy()
	{
		this.agent.onBehaviorStateChanged -= this.OnNetworkBehaviourStateChanged;
	}

	public void SetTarget(GRPlayer newTarget)
	{
		if (newTarget.IsNull())
		{
			this.ClearTarget();
			return;
		}
		this.TargetPlayer = newTarget;
	}

	public void ClearTarget()
	{
		this.TargetPlayer = null;
	}

	private void Update()
	{
		this.OnThink();
		this.UpdateAnimators();
	}

	private void OnTriggerEnter(Collider collider)
	{
		CustomMapsBehaviourBase customMapsBehaviourBase = this.behaviourDict[this.currentBehaviour];
		if (customMapsBehaviourBase != null)
		{
			customMapsBehaviourBase.OnTriggerEnter(collider);
		}
	}

	private void InitAnimators()
	{
		this.animators = base.gameObject.GetComponentsInChildren<Animator>();
	}

	private void UpdateAnimators()
	{
		if (this.animators.IsNullOrEmpty<Animator>())
		{
			return;
		}
		float magnitude = this.agent.navAgent.velocity.magnitude;
		for (int i = 0; i < this.animators.Length; i++)
		{
			this.animators[i].SetFloat(CustomMapsAIBehaviourController.movementSpeedParamIndex, magnitude);
		}
	}

	public void PlayAnimation(string stateName, float blendTime = 0f)
	{
		for (int i = 0; i < this.animators.Length; i++)
		{
			this.animators[i].CrossFadeInFixedTime(stateName, blendTime);
		}
	}

	public bool IsAnimationPlaying(string stateName)
	{
		int num = 0;
		if (num >= this.animators.Length)
		{
			return false;
		}
		Animator animator = this.animators[num];
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		return (currentAnimatorStateInfo.IsName(stateName) && currentAnimatorStateInfo.normalizedTime < 1f) || animator.GetNextAnimatorStateInfo(0).IsName(stateName);
	}

	public void SetupBehaviours(AIAgent aiAgent)
	{
		this.allowTargetingTaggedPlayers = aiAgent.allowTargetingTaggedPlayers;
		for (int i = 0; i < aiAgent.agentBehaviours.Count; i++)
		{
			if (!this.usedBehaviours.Contains(aiAgent.agentBehaviours[i]))
			{
				switch (aiAgent.agentBehaviours[i])
				{
				case AgentBehaviours.Search:
					this.behaviourDict[AgentBehaviours.Search] = new CustomMapsSearchBehaviour(this, aiAgent);
					break;
				case AgentBehaviours.Chase:
					this.behaviourDict[AgentBehaviours.Chase] = new CustomMapsChaseBehaviour(this, aiAgent);
					break;
				case AgentBehaviours.Attack:
					this.behaviourDict[AgentBehaviours.Attack] = new CustomMapsAttackBehaviour(this, aiAgent);
					break;
				default:
					goto IL_00A1;
				}
				this.usedBehaviours.Add(aiAgent.agentBehaviours[i]);
			}
			IL_00A1:;
		}
	}

	public void StopMoving()
	{
		this.RequestDestination(base.transform.position);
	}

	public void RequestDestination(Vector3 destination)
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		this.agent.RequestDestination(destination);
	}

	private void OnThink()
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		if (this.behaviourDict == null || this.behaviourDict.Count == 0)
		{
			return;
		}
		int num = -1;
		if (this.currentBehaviourIndex != -1 && this.behaviourDict[this.usedBehaviours[this.currentBehaviourIndex]].CanContinueExecuting())
		{
			num = this.currentBehaviourIndex;
		}
		else
		{
			for (int i = 0; i < this.usedBehaviours.Count; i++)
			{
				if (i != this.currentBehaviourIndex && this.behaviourDict[this.usedBehaviours[i]].CanExecute())
				{
					num = i;
					break;
				}
			}
		}
		if (num == -1)
		{
			return;
		}
		if (this.currentBehaviourIndex != num)
		{
			this.currentBehaviourIndex = num;
			this.currentBehaviour = this.usedBehaviours[num];
			this.agent.RequestBehaviorChange((byte)this.currentBehaviour);
		}
		this.behaviourDict[this.currentBehaviour].Execute();
	}

	private void OnNetworkBehaviourStateChanged(byte newstate)
	{
		if (newstate < 0 || newstate >= 3)
		{
			return;
		}
		if (!this.behaviourDict.ContainsKey((AgentBehaviours)newstate))
		{
			return;
		}
		if (this.currentBehaviour != (AgentBehaviours)newstate && this.behaviourDict.ContainsKey(this.currentBehaviour))
		{
			this.behaviourDict[this.currentBehaviour].ResetBehavior();
		}
		this.currentBehaviour = (AgentBehaviours)newstate;
		this.behaviourDict[this.currentBehaviour].NetExecute();
	}

	public void OnEntityInit()
	{
		bool flag = AISpawnManager.HasInstance && AISpawnManager.instance != null;
		if (!flag && MapSpawnManager.instance == null)
		{
			return;
		}
		this.entity.transform.parent = (flag ? AISpawnManager.instance.transform : MapSpawnManager.instance.transform);
		byte b;
		AIAgent.UnpackCreateData(this.entity.createData, out b, out this.luaAgentID);
		AIAgent aiagent;
		if (flag && AISpawnManager.instance.SpawnEnemy((int)b, out aiagent))
		{
			this.SetupNewEnemy(aiagent);
			return;
		}
		MapEntity mapEntity;
		if (!flag && MapSpawnManager.instance.SpawnEntity((int)b, out mapEntity))
		{
			this.SetupNewEnemy((AIAgent)mapEntity);
			return;
		}
		GTDev.LogError<string>("CustomMapsAIBehaviourController::OnEntityInit could not spawn enemy", null);
		Object.Destroy(base.gameObject);
	}

	private void SetupNewEnemy(AIAgent newEnemy)
	{
		newEnemy.gameObject.SetActive(true);
		newEnemy.transform.parent = this.entity.transform;
		newEnemy.transform.localPosition = Vector3.zero;
		newEnemy.transform.localRotation = Quaternion.identity;
		this.InitAnimators();
		NavMeshAgent component = this.entity.gameObject.GetComponent<NavMeshAgent>();
		if (component.IsNull())
		{
			GTDev.LogError<string>("nav mesh agent is null", null);
			Object.Destroy(base.gameObject);
			return;
		}
		component.agentTypeID = this.GetNavAgentType(newEnemy.navAgentType);
		component.speed = newEnemy.movementSpeed;
		component.angularSpeed = newEnemy.turnSpeed;
		component.acceleration = newEnemy.acceleration;
		this.SetupBehaviours(newEnemy);
	}

	private int GetNavAgentType(NavAgentType navType)
	{
		int settingsCount = NavMesh.GetSettingsCount();
		int num = NavMesh.GetSettingsByIndex(0).agentTypeID;
		for (int i = 0; i < settingsCount; i++)
		{
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(i);
			if (NavMesh.GetSettingsNameFromID(settingsByIndex.agentTypeID) == navType.ToString())
			{
				num = settingsByIndex.agentTypeID;
				break;
			}
		}
		return num;
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long newState)
	{
	}

	public GRPlayer FindBestTarget(Vector3 sourcePos, float maxRange, float maxRangeSq, float minDotVal)
	{
		float num = 0f;
		GRPlayer grplayer = null;
		this.tempRigs.Clear();
		this.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(this.tempRigs);
		Vector3 vector = base.transform.rotation * Vector3.forward;
		for (int i = 0; i < this.tempRigs.Count; i++)
		{
			GRPlayer component = this.tempRigs[i].GetComponent<GRPlayer>();
			Vector3 vector2;
			if (this.IsTargetInRange(sourcePos, component, maxRangeSq, out vector2))
			{
				float num2 = 0f;
				if (vector2.sqrMagnitude > 0f)
				{
					num2 = Mathf.Sqrt(vector2.magnitude);
				}
				float num3 = Vector3.Dot(vector2.normalized, vector);
				if (num3 >= minDotVal)
				{
					float num4 = Mathf.Lerp(0f, 0.5f, 1f - num2 / maxRange);
					float num5 = Mathf.Lerp(0f, 0.5f, (1f - minDotVal - (1f - num3)) / (1f - minDotVal));
					if (num4 + num5 > num && this.IsTargetVisible(sourcePos, component, maxRange))
					{
						num = num4 + num5;
						grplayer = component;
					}
				}
			}
		}
		return grplayer;
	}

	public bool IsTargetVisible(Vector3 startPos, GRPlayer target, float maxDist)
	{
		if (!this.IsTargetable(target))
		{
			return false;
		}
		int num = Physics.RaycastNonAlloc(new Ray(startPos, target.transform.position - startPos), CustomMapsAIBehaviourController.visibilityHits, Mathf.Min(Vector3.Distance(target.transform.position, startPos), maxDist), this.visibilityLayerMask.value, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			if (CustomMapsAIBehaviourController.visibilityHits[i].transform != base.transform && !CustomMapsAIBehaviourController.visibilityHits[i].transform.IsChildOf(base.transform))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsTargetInRange(Vector3 startPos, GRPlayer target, float maxRangeSq, out Vector3 toTarget)
	{
		toTarget = Vector3.zero;
		if (!this.IsTargetable(target))
		{
			return false;
		}
		Vector3 position = target.transform.position;
		toTarget = position - startPos;
		return toTarget.sqrMagnitude <= maxRangeSq;
	}

	public bool IsTargetable(GRPlayer potentialTarget)
	{
		if (potentialTarget.IsNull())
		{
			return false;
		}
		if (potentialTarget.State == GRPlayer.GRPlayerState.Ghost)
		{
			return false;
		}
		if (potentialTarget.MyRig.isLocal)
		{
			if (CustomMapManager.IsLocalPlayerInVirtualStump())
			{
				return false;
			}
		}
		else if (CustomMapManager.IsRemotePlayerInVirtualStump(potentialTarget.MyRig.OwningNetPlayer.UserId))
		{
			return false;
		}
		return this.allowTargetingTaggedPlayers || GameMode.ActiveGameMode.GameType() == GameModeType.Custom || !GameMode.LocalIsTagged(potentialTarget.MyRig.OwningNetPlayer);
	}

	private static readonly int movementSpeedParamIndex = Animator.StringToHash("MovementSpeed");

	public GameEntity entity;

	public GameAgent agent;

	public GRAttributes attributes;

	private Animator[] animators;

	public short luaAgentID;

	private List<VRRig> tempRigs = new List<VRRig>(10);

	private static RaycastHit[] visibilityHits = new RaycastHit[10];

	private LayerMask visibilityLayerMask;

	private bool allowTargetingTaggedPlayers;

	private Dictionary<AgentBehaviours, CustomMapsBehaviourBase> behaviourDict = new Dictionary<AgentBehaviours, CustomMapsBehaviourBase>(8);

	private List<AgentBehaviours> usedBehaviours = new List<AgentBehaviours>(8);

	private AgentBehaviours currentBehaviour;

	private int currentBehaviourIndex;

	private const int BEHAVIOUR_COUNT = 3;

	public enum CustomMapsAIBehaviour
	{
		Search,
		Chase,
		Attack
	}
}
