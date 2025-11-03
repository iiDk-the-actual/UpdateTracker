using System;
using System.Collections.Generic;
using Fusion;
using GorillaTag.Rendering;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[NetworkBehaviourWeaved(3)]
public class AngryBeeSwarm : NetworkComponent
{
	public bool isDormant
	{
		get
		{
			return this.currentState == AngryBeeSwarm.ChaseState.Dormant;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AngryBeeSwarm.instance = this;
		this.targetPlayer = null;
		this.currentState = AngryBeeSwarm.ChaseState.Dormant;
		this.grabTimestamp = -this.minGrabCooldown;
		RoomSystem.JoinedRoomEvent += new Action(this.OnJoinedRoom);
	}

	private void InitializeSwarm()
	{
		if (NetworkSystem.Instance.InRoom && base.IsMine)
		{
			this.beeAnimator.transform.localPosition = Vector3.zero;
			this.lastSpeedIncreased = 0f;
			this.currentSpeed = 0f;
		}
	}

	private void LateUpdate()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			this.currentState = AngryBeeSwarm.ChaseState.Dormant;
			this.UpdateState();
			return;
		}
		if (base.IsMine)
		{
			AngryBeeSwarm.ChaseState chaseState = this.currentState;
			switch (chaseState)
			{
			case AngryBeeSwarm.ChaseState.Dormant:
				if (Application.isEditor && Keyboard.current[Key.Space].wasPressedThisFrame)
				{
					this.currentState = AngryBeeSwarm.ChaseState.InitialEmerge;
				}
				break;
			case AngryBeeSwarm.ChaseState.InitialEmerge:
				if (Time.time > this.emergeStartedTimestamp + this.totalTimeToEmerge)
				{
					this.currentState = AngryBeeSwarm.ChaseState.Chasing;
				}
				break;
			case (AngryBeeSwarm.ChaseState)3:
				break;
			case AngryBeeSwarm.ChaseState.Chasing:
				if (this.followTarget == null || this.targetPlayer == null || Time.time > this.NextRefreshClosestPlayerTimestamp)
				{
					this.ChooseClosestTarget();
					if (this.followTarget != null)
					{
						this.BoredToDeathAtTimestamp = -1f;
					}
					else if (this.BoredToDeathAtTimestamp < 0f)
					{
						this.BoredToDeathAtTimestamp = Time.time + this.boredAfterDuration;
					}
				}
				if (this.BoredToDeathAtTimestamp >= 0f && Time.time > this.BoredToDeathAtTimestamp)
				{
					this.currentState = AngryBeeSwarm.ChaseState.Dormant;
				}
				else if (!(this.followTarget == null) && (this.followTarget.position - this.beeAnimator.transform.position).magnitude < this.catchDistance)
				{
					float num = ZoneShaderSettings.GetWaterY() + this.PlayerMinHeightAboveWater;
					if (this.followTarget.position.y > num)
					{
						this.currentState = AngryBeeSwarm.ChaseState.Grabbing;
					}
				}
				break;
			default:
				if (chaseState == AngryBeeSwarm.ChaseState.Grabbing)
				{
					if (Time.time > this.grabTimestamp + this.grabDuration)
					{
						this.currentState = AngryBeeSwarm.ChaseState.Dormant;
					}
				}
				break;
			}
		}
		if (this.lastState != this.currentState)
		{
			this.OnChangeState(this.currentState);
			this.lastState = this.currentState;
		}
		this.UpdateState();
	}

	public void UpdateState()
	{
		AngryBeeSwarm.ChaseState chaseState = this.currentState;
		switch (chaseState)
		{
		case AngryBeeSwarm.ChaseState.Dormant:
		case (AngryBeeSwarm.ChaseState)3:
			break;
		case AngryBeeSwarm.ChaseState.InitialEmerge:
			if (NetworkSystem.Instance.InRoom)
			{
				this.SwarmEmergeUpdateShared();
				return;
			}
			break;
		case AngryBeeSwarm.ChaseState.Chasing:
			if (NetworkSystem.Instance.InRoom)
			{
				if (base.IsMine)
				{
					this.ChaseHost();
				}
				this.MoveBodyShared();
				return;
			}
			break;
		default:
			if (chaseState != AngryBeeSwarm.ChaseState.Grabbing)
			{
				return;
			}
			if (NetworkSystem.Instance.InRoom)
			{
				if (this.targetPlayer == NetworkSystem.Instance.LocalPlayer)
				{
					this.RiseGrabbedLocalPlayer();
				}
				this.GrabBodyShared();
			}
			break;
		}
	}

	public void Emerge(Vector3 fromPosition, Vector3 toPosition)
	{
		base.transform.position = fromPosition;
		this.emergeFromPosition = fromPosition;
		this.emergeToPosition = toPosition;
		this.currentState = AngryBeeSwarm.ChaseState.InitialEmerge;
		this.emergeStartedTimestamp = Time.time;
	}

	private void OnChangeState(AngryBeeSwarm.ChaseState newState)
	{
		switch (newState)
		{
		case AngryBeeSwarm.ChaseState.Dormant:
			if (this.beeAnimator.gameObject.activeSelf)
			{
				this.beeAnimator.gameObject.SetActive(false);
			}
			if (base.IsMine)
			{
				this.targetPlayer = null;
				base.transform.position = new Vector3(0f, -9999f, 0f);
				this.InitializeSwarm();
			}
			this.SetInitialRotations();
			return;
		case AngryBeeSwarm.ChaseState.InitialEmerge:
			this.emergeStartedTimestamp = Time.time;
			if (!this.beeAnimator.gameObject.activeSelf)
			{
				this.beeAnimator.gameObject.SetActive(true);
			}
			this.beeAnimator.SetEmergeFraction(0f);
			if (base.IsMine)
			{
				this.currentSpeed = 0f;
				this.ChooseClosestTarget();
			}
			this.SetInitialRotations();
			return;
		case (AngryBeeSwarm.ChaseState)3:
			break;
		case AngryBeeSwarm.ChaseState.Chasing:
			if (!this.beeAnimator.gameObject.activeSelf)
			{
				this.beeAnimator.gameObject.SetActive(true);
			}
			this.beeAnimator.SetEmergeFraction(1f);
			this.ResetPath();
			this.NextRefreshClosestPlayerTimestamp = Time.time + this.RefreshClosestPlayerInterval;
			this.BoredToDeathAtTimestamp = -1f;
			return;
		default:
		{
			if (newState != AngryBeeSwarm.ChaseState.Grabbing)
			{
				return;
			}
			if (!this.beeAnimator.gameObject.activeSelf)
			{
				this.beeAnimator.gameObject.SetActive(true);
			}
			this.grabTimestamp = Time.time;
			this.beeAnimator.transform.localPosition = this.ghostOffsetGrabbingLocal;
			VRRig vrrig = GorillaGameManager.StaticFindRigForPlayer(this.targetPlayer);
			if (vrrig != null)
			{
				this.followTarget = vrrig.transform;
			}
			break;
		}
		}
	}

	private void ChooseClosestTarget()
	{
		float num = Mathf.Lerp(this.initialRangeLimit, this.finalRangeLimit, (Time.time + this.totalTimeToEmerge - this.emergeStartedTimestamp) / this.rangeLimitBlendDuration);
		float num2 = num * num;
		VRRig vrrig = null;
		float num3 = ZoneShaderSettings.GetWaterY() + this.PlayerMinHeightAboveWater;
		foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
		{
			if (vrrig2.head != null && !(vrrig2.head.rigTarget == null) && vrrig2.head.rigTarget.position.y > num3)
			{
				float sqrMagnitude = (base.transform.position - vrrig2.head.rigTarget.transform.position).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					vrrig = vrrig2;
				}
			}
		}
		if (vrrig != null)
		{
			this.targetPlayer = vrrig.creator;
			this.followTarget = vrrig.head.rigTarget;
			NavMeshHit navMeshHit;
			this.targetIsOnNavMesh = NavMesh.SamplePosition(this.followTarget.position, out navMeshHit, 5f, 1);
		}
		else
		{
			this.targetPlayer = null;
			this.followTarget = null;
		}
		this.NextRefreshClosestPlayerTimestamp = Time.time + this.RefreshClosestPlayerInterval;
	}

	private void SetInitialRotations()
	{
		this.beeAnimator.transform.localPosition = Vector3.zero;
	}

	private void SwarmEmergeUpdateShared()
	{
		if (Time.time < this.emergeStartedTimestamp + this.totalTimeToEmerge)
		{
			float num = (Time.time - this.emergeStartedTimestamp) / this.totalTimeToEmerge;
			if (base.IsMine)
			{
				base.transform.position = Vector3.Lerp(this.emergeFromPosition, this.emergeToPosition, (Time.time - this.emergeStartedTimestamp) / this.totalTimeToEmerge);
			}
			this.beeAnimator.SetEmergeFraction(num);
		}
	}

	private void RiseGrabbedLocalPlayer()
	{
		if (Time.time > this.grabTimestamp + this.minGrabCooldown)
		{
			this.grabTimestamp = Time.time;
			GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Frozen, GorillaTagger.Instance.tagCooldown);
			GorillaTagger.Instance.StartVibration(true, this.hapticStrength, this.hapticDuration);
			GorillaTagger.Instance.StartVibration(false, this.hapticStrength, this.hapticDuration);
		}
		if (Time.time < this.grabTimestamp + this.grabDuration)
		{
			GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.up * this.grabSpeed;
			EquipmentInteractor.instance.ForceStopClimbing();
		}
	}

	public void UpdateFollowPath(Vector3 destination, float currentSpeed)
	{
		if (this.path == null)
		{
			this.GetNewPath(destination);
		}
		this.pathPoints[this.pathPoints.Count - 1] = destination;
		Vector3 vector = this.pathPoints[this.currentPathPointIdx];
		base.transform.position = Vector3.MoveTowards(base.transform.position, vector, currentSpeed * Time.deltaTime);
		Vector3 eulerAngles = Quaternion.LookRotation(vector - base.transform.position).eulerAngles;
		if (Mathf.Abs(eulerAngles.x) > 45f)
		{
			eulerAngles.x = 0f;
		}
		base.transform.rotation = Quaternion.Euler(eulerAngles);
		if (this.currentPathPointIdx + 1 < this.pathPoints.Count && (base.transform.position - vector).sqrMagnitude < 0.1f)
		{
			if (this.nextPathTimestamp <= Time.time)
			{
				this.GetNewPath(destination);
				return;
			}
			this.currentPathPointIdx++;
		}
	}

	private void GetNewPath(Vector3 destination)
	{
		this.path = new NavMeshPath();
		NavMeshHit navMeshHit;
		NavMesh.SamplePosition(base.transform.position, out navMeshHit, 5f, 1);
		NavMeshHit navMeshHit2;
		this.targetIsOnNavMesh = NavMesh.SamplePosition(destination, out navMeshHit2, 5f, 1);
		NavMesh.CalculatePath(navMeshHit.position, navMeshHit2.position, -1, this.path);
		this.pathPoints = new List<Vector3>();
		foreach (Vector3 vector in this.path.corners)
		{
			this.pathPoints.Add(vector + Vector3.up * this.heightAboveNavmesh);
		}
		this.pathPoints.Add(destination);
		this.currentPathPointIdx = 0;
		this.nextPathTimestamp = Time.time + 2f;
	}

	public void ResetPath()
	{
		this.path = null;
	}

	private void ChaseHost()
	{
		if (this.followTarget != null)
		{
			if (Time.time > this.lastSpeedIncreased + this.velocityIncreaseInterval)
			{
				this.lastSpeedIncreased = Time.time;
				this.currentSpeed += this.velocityStep;
			}
			float num = ZoneShaderSettings.GetWaterY() + this.MinHeightAboveWater;
			Vector3 position = this.followTarget.position;
			if (position.y < num)
			{
				position.y = num;
			}
			if (this.targetIsOnNavMesh)
			{
				this.UpdateFollowPath(position, this.currentSpeed);
				return;
			}
			base.transform.position = Vector3.MoveTowards(base.transform.position, position, this.currentSpeed * Time.deltaTime);
		}
	}

	private void MoveBodyShared()
	{
		this.noisyOffset = new Vector3(Mathf.PerlinNoise(Time.time, 0f) - 0.5f, Mathf.PerlinNoise(Time.time, 10f) - 0.5f, Mathf.PerlinNoise(Time.time, 20f) - 0.5f);
		this.beeAnimator.transform.localPosition = this.noisyOffset;
	}

	private void GrabBodyShared()
	{
		if (this.followTarget != null)
		{
			base.transform.rotation = this.followTarget.rotation;
			base.transform.position = this.followTarget.position;
		}
	}

	[Networked]
	[NetworkedWeaved(0, 3)]
	public unsafe BeeSwarmData Data
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AngryBeeSwarm.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(BeeSwarmData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing AngryBeeSwarm.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(BeeSwarmData*)(this.Ptr + 0) = value;
		}
	}

	public override void WriteDataFusion()
	{
		this.Data = new BeeSwarmData(this.targetPlayer.ActorNumber, (int)this.currentState, this.currentSpeed);
	}

	public override void ReadDataFusion()
	{
		this.targetPlayer = NetworkSystem.Instance.GetPlayer(this.Data.TargetActorNumber);
		this.currentState = (AngryBeeSwarm.ChaseState)this.Data.CurrentState;
		if (float.IsFinite(this.Data.CurrentSpeed))
		{
			this.currentSpeed = this.Data.CurrentSpeed;
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == null || !info.Sender.Equals(PhotonNetwork.MasterClient))
		{
			return;
		}
		NetPlayer netPlayer = this.targetPlayer;
		stream.SendNext((netPlayer != null) ? netPlayer.ActorNumber : (-1));
		stream.SendNext(this.currentState);
		stream.SendNext(this.currentSpeed);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender != PhotonNetwork.MasterClient)
		{
			return;
		}
		int num = (int)stream.ReceiveNext();
		this.targetPlayer = NetworkSystem.Instance.GetPlayer(num);
		this.currentState = (AngryBeeSwarm.ChaseState)stream.ReceiveNext();
		float num2 = (float)stream.ReceiveNext();
		if (float.IsFinite(num2))
		{
			this.currentSpeed = num2;
		}
	}

	public override void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		base.OnOwnerChange(newOwner, previousOwner);
		if (newOwner == PhotonNetwork.LocalPlayer)
		{
			this.OnChangeState(this.currentState);
		}
	}

	public void OnJoinedRoom()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.InitializeSwarm();
		}
	}

	private void TestEmerge()
	{
		this.Emerge(this.testEmergeFrom.transform.position, this.testEmergeTo.transform.position);
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.Data = this._Data;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._Data = this.Data;
	}

	public static AngryBeeSwarm instance;

	public float heightAboveNavmesh = 0.5f;

	public Transform followTarget;

	[SerializeField]
	private float velocityStep = 1f;

	private float currentSpeed;

	[SerializeField]
	private float velocityIncreaseInterval = 20f;

	public Vector3 noisyOffset;

	public Vector3 ghostOffsetGrabbingLocal;

	private float emergeStartedTimestamp;

	private float grabTimestamp;

	private float lastSpeedIncreased;

	[SerializeField]
	private float totalTimeToEmerge;

	[SerializeField]
	private float catchDistance;

	[SerializeField]
	private float grabDuration;

	[SerializeField]
	private float grabSpeed = 1f;

	[SerializeField]
	private float minGrabCooldown;

	[SerializeField]
	private float initialRangeLimit;

	[SerializeField]
	private float finalRangeLimit;

	[SerializeField]
	private float rangeLimitBlendDuration;

	[SerializeField]
	private float boredAfterDuration;

	public NetPlayer targetPlayer;

	public AngryBeeAnimator beeAnimator;

	public AngryBeeSwarm.ChaseState currentState;

	public AngryBeeSwarm.ChaseState lastState;

	public NetPlayer grabbedPlayer;

	private bool targetIsOnNavMesh;

	private const float navMeshSampleRange = 5f;

	[Tooltip("Haptic vibration when chased by lucy")]
	public float hapticStrength = 1f;

	public float hapticDuration = 1.5f;

	public float MinHeightAboveWater = 0.5f;

	public float PlayerMinHeightAboveWater = 0.5f;

	public float RefreshClosestPlayerInterval = 1f;

	private float NextRefreshClosestPlayerTimestamp = 1f;

	private float BoredToDeathAtTimestamp = -1f;

	[SerializeField]
	private Transform testEmergeFrom;

	[SerializeField]
	private Transform testEmergeTo;

	private Vector3 emergeFromPosition;

	private Vector3 emergeToPosition;

	private NavMeshPath path;

	public List<Vector3> pathPoints;

	public int currentPathPointIdx;

	private float nextPathTimestamp;

	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("Data", 0, 3)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private BeeSwarmData _Data;

	public enum ChaseState
	{
		Dormant = 1,
		InitialEmerge,
		Chasing = 4,
		Grabbing = 8
	}
}
