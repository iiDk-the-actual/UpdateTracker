using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public class GameAgent : MonoBehaviour, IGameEntityComponent
{
	public event GameAgent.StateChangedEvent onBodyStateChanged;

	public event GameAgent.StateChangedEvent onBehaviorStateChanged;

	public event GameAgent.NavigationLinkReachedEvent onReachedNavigationLink;

	public event GameAgent.JumpRequestedEvent onJumpRequested;

	public event GameAgent.NavigationFailedEvent onNavigationFailed;

	public GameAgentManager GetGameAgentManager()
	{
		return this.entity.manager.gameAgentManager;
	}

	private void Awake()
	{
		this.agentComponents = new List<IGameAgentComponent>(1);
		base.GetComponentsInChildren<IGameAgentComponent>(this.agentComponents);
	}

	public void OnEntityInit()
	{
		this.GetGameAgentManager().AddGameAgent(this);
	}

	public void OnEntityDestroy()
	{
		this.GetGameAgentManager().RemoveGameAgent(this);
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	public void OnBehaviorStateChanged(byte newState)
	{
		GameAgent.StateChangedEvent stateChangedEvent = this.onBehaviorStateChanged;
		if (stateChangedEvent == null)
		{
			return;
		}
		stateChangedEvent(newState);
	}

	public void OnBodyStateChanged(byte newState)
	{
		GameAgent.StateChangedEvent stateChangedEvent = this.onBodyStateChanged;
		if (stateChangedEvent == null)
		{
			return;
		}
		stateChangedEvent(newState);
	}

	public void OnThink(float deltaTime)
	{
		if (!this.pauseEntityThink)
		{
			for (int i = 0; i < this.agentComponents.Count; i++)
			{
				this.agentComponents[i].OnEntityThink(deltaTime);
			}
		}
	}

	public void OnUpdate()
	{
		if (this.navAgent.isOnNavMesh)
		{
			this.lastPosOnNavMesh = this.navAgent.transform.position;
		}
		if (!this.navAgent.autoTraverseOffMeshLink && !this.wasOnOffMeshNavLink && this.navAgent.isOnOffMeshLink)
		{
			if (this.entity.IsAuthority())
			{
				if ((this.navAgent.transform.position - this.navAgent.currentOffMeshLinkData.startPos).sqrMagnitude < (this.navAgent.transform.position - this.navAgent.currentOffMeshLinkData.endPos).sqrMagnitude)
				{
					this.GetGameAgentManager().RequestJump(this, this.navAgent.transform.position, this.navAgent.currentOffMeshLinkData.endPos, 1f, 1f);
				}
				else
				{
					this.GetGameAgentManager().RequestJump(this, this.navAgent.transform.position, this.navAgent.currentOffMeshLinkData.startPos, 1f, 1f);
				}
			}
			GameAgent.NavigationLinkReachedEvent navigationLinkReachedEvent = this.onReachedNavigationLink;
			if (navigationLinkReachedEvent != null)
			{
				navigationLinkReachedEvent(this.navAgent.currentOffMeshLinkData);
			}
		}
		this.wasOnOffMeshNavLink = this.navAgent.isOnOffMeshLink;
		if (!this.hasNotifiedNavigationFailure && !this.navAgent.pathPending && (this.navAgent.pathStatus == NavMeshPathStatus.PathPartial || this.navAgent.pathStatus == NavMeshPathStatus.PathInvalid))
		{
			GameAgent.NavigationFailedEvent navigationFailedEvent = this.onNavigationFailed;
			if (navigationFailedEvent != null)
			{
				navigationFailedEvent(this.navAgent.pathStatus, this.navAgent.destination, this.navAgent.remainingDistance);
			}
			this.hasNotifiedNavigationFailure = true;
		}
	}

	public void OnJumpRequested(Vector3 start, Vector3 end, float heightScale, float speedScale)
	{
		GameAgent.JumpRequestedEvent jumpRequestedEvent = this.onJumpRequested;
		if (jumpRequestedEvent == null)
		{
			return;
		}
		jumpRequestedEvent(start, end, heightScale, speedScale);
	}

	public bool IsOnNavMesh()
	{
		return this.navAgent != null && this.navAgent.isOnNavMesh;
	}

	public Vector3 GetLastPosOnNavMesh()
	{
		return this.lastPosOnNavMesh;
	}

	public void RequestDestination(Vector3 dest)
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		if (!this.IsOnNavMesh())
		{
			dest = this.lastPosOnNavMesh;
		}
		if (Vector3.Distance(this.lastRequestedDest, dest) < 0.5f)
		{
			return;
		}
		this.lastRequestedDest = dest;
		if (this.entity.IsAuthority())
		{
			this.GetGameAgentManager().RequestDestination(this, dest);
		}
	}

	public void RequestBehaviorChange(byte behavior)
	{
		this.GetGameAgentManager().RequestBehavior(this, behavior);
	}

	public void RequestStateChange(byte state)
	{
		this.GetGameAgentManager().RequestState(this, state);
	}

	public void RequestTarget(NetPlayer targetPlayer)
	{
		this.GetGameAgentManager().RequestTarget(this, targetPlayer);
	}

	public void ApplyDestination(Vector3 dest)
	{
		NavMeshHit navMeshHit;
		if (!NavMesh.SamplePosition(dest, out navMeshHit, 1.5f, -1))
		{
			return;
		}
		dest = navMeshHit.position;
		this.lastReceivedDest = dest;
		this.hasNotifiedNavigationFailure = false;
		if (this.navAgent.isOnNavMesh)
		{
			this.navAgent.destination = dest;
		}
	}

	public void SetDisableNetworkSync(bool disable)
	{
		this.disableNetworkSync = disable;
	}

	public void SetIsPathing(bool isPathing, bool ignoreRigiBody = false)
	{
		this.navAgent.enabled = isPathing;
		if (!ignoreRigiBody && this.rigidBody != null)
		{
			this.rigidBody.isKinematic = isPathing;
		}
	}

	public void SetSpeed(float speed)
	{
		this.navAgent.speed = speed;
	}

	public void ApplyNetworkUpdate(Vector3 position, Quaternion rotation)
	{
		if (this.disableNetworkSync)
		{
			return;
		}
		if ((base.transform.position - position).sqrMagnitude > this.networkPositionCorrectionDist * this.networkPositionCorrectionDist)
		{
			this.navAgent.Warp(position);
			this.navAgent.destination = this.lastReceivedDest;
		}
		base.transform.rotation = rotation;
		if (this.rigidBody != null)
		{
			this.rigidBody.rotation = rotation;
		}
	}

	public static void UpdateFacing(Transform transform, NavMeshAgent navAgent, NetPlayer targetPlayer, float turnspeed = 3600f)
	{
		Transform transform2 = null;
		Vector3 forward = transform.forward;
		if (targetPlayer != null)
		{
			GRPlayer grplayer = GRPlayer.Get(targetPlayer.ActorNumber);
			if (grplayer != null && grplayer.State == GRPlayer.GRPlayerState.Alive)
			{
				transform2 = grplayer.transform;
			}
		}
		GameAgent.UpdateFacingTarget(transform, navAgent, transform2, turnspeed);
	}

	public static void UpdateFacingTarget(Transform transform, NavMeshAgent navAgent, Transform target, float turnspeed = 3600f)
	{
		Vector3 vector = transform.forward;
		if (target != null)
		{
			Vector3 position = target.position;
			Vector3 position2 = transform.position;
			Vector3 vector2 = position - position2;
			vector2.y = 0f;
			float magnitude = vector2.magnitude;
			if (magnitude > 0f)
			{
				vector = vector2 / magnitude;
			}
		}
		else
		{
			Vector3 desiredVelocity = navAgent.desiredVelocity;
			desiredVelocity.y = 0f;
			float magnitude2 = desiredVelocity.magnitude;
			if (magnitude2 > 0f)
			{
				vector = desiredVelocity / magnitude2;
			}
		}
		Quaternion quaternion = Quaternion.LookRotation(vector);
		if (navAgent.speed > 0f)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, Mathf.Clamp(turnspeed * navAgent.speed / Quaternion.Angle(transform.rotation, quaternion) * Time.deltaTime, 0f, 1f));
			return;
		}
		transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, Mathf.Clamp(turnspeed / Quaternion.Angle(transform.rotation, quaternion) * Time.deltaTime, 0f, 1f));
	}

	public static void UpdateFacingForward(Transform transform, NavMeshAgent navAgent, float turnspeed = 3600f)
	{
		Vector3 desiredVelocity = navAgent.desiredVelocity;
		desiredVelocity.y = 0f;
		float magnitude = desiredVelocity.magnitude;
		if (magnitude <= 0f)
		{
			return;
		}
		Vector3 vector = desiredVelocity / magnitude;
		GameAgent.UpdateFacingDir(transform, navAgent, vector, turnspeed);
	}

	public static void UpdateFacingPos(Transform transform, NavMeshAgent navAgent, Vector3 facingPos, float turnspeed = 3600f)
	{
		Vector3 vector = facingPos - transform.position;
		vector.y = 0f;
		vector.Normalize();
		GameAgent.UpdateFacingDir(transform, navAgent, vector, turnspeed);
	}

	public static void UpdateFacingDir(Transform transform, NavMeshAgent navAgent, Vector3 facingDir, float turnspeed = 3600f)
	{
		Quaternion quaternion = Quaternion.LookRotation(facingDir);
		transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, Mathf.Clamp(turnspeed * navAgent.speed / Quaternion.Angle(transform.rotation, quaternion) * Time.deltaTime, 0f, 1f));
	}

	public GameEntity entity;

	public NavMeshAgent navAgent;

	public Rigidbody rigidBody;

	public float networkPositionCorrectionDist = 2.5f;

	[ReadOnly]
	public NetPlayer targetPlayer;

	private bool disableNetworkSync;

	private Vector3 lastPosOnNavMesh;

	private Vector3 lastRequestedDest;

	private Vector3 lastReceivedDest;

	private bool hasNotifiedNavigationFailure;

	private List<IGameAgentComponent> agentComponents;

	private bool wasOnOffMeshNavLink;

	[ReadOnly]
	public bool pauseEntityThink;

	public delegate void StateChangedEvent(byte newState);

	public delegate void NavigationLinkReachedEvent(OffMeshLinkData linkData);

	public delegate void JumpRequestedEvent(Vector3 start, Vector3 end, float heightScale, float speedScale);

	public delegate void NavigationFailedEvent(NavMeshPathStatus status, Vector3 destination, float remainingDistance);
}
