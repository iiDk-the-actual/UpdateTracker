using System;
using System.Collections.Generic;
using GorillaLocomotion;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(NetworkView))]
public class MonkeyeAI : MonoBehaviour, IGorillaSliceableSimple
{
	private string UserIdFromRig(VRRig rig)
	{
		if (rig == null)
		{
			return "";
		}
		if (!NetworkSystem.Instance.InRoom)
		{
			if (rig == GorillaTagger.Instance.offlineVRRig)
			{
				return "-1";
			}
			Debug.Log("Not in a room but not targeting offline rig");
			return null;
		}
		else
		{
			if (rig == GorillaTagger.Instance.offlineVRRig)
			{
				return NetworkSystem.Instance.LocalPlayer.UserId;
			}
			if (rig.creator == null)
			{
				return "";
			}
			return rig.creator.UserId;
		}
	}

	private VRRig GetRig(string userId)
	{
		if (userId == "")
		{
			return null;
		}
		if (NetworkSystem.Instance.InRoom || !(userId != "-1"))
		{
			foreach (VRRig vrrig in this.GetValidChoosableRigs())
			{
				if (!(vrrig == null))
				{
					NetPlayer creator = vrrig.creator;
					if (creator != null && userId == creator.UserId)
					{
						return vrrig;
					}
				}
			}
			return null;
		}
		if (userId == "-1 " && GorillaTagger.Instance != null)
		{
			return GorillaTagger.Instance.offlineVRRig;
		}
		return null;
	}

	private float Distance2D(Vector3 a, Vector3 b)
	{
		Vector2 vector = new Vector2(a.x, a.z);
		Vector2 vector2 = new Vector2(b.x, b.z);
		return Vector2.Distance(vector, vector2);
	}

	private Transform PickRandomPatrolPoint()
	{
		int num;
		do
		{
			num = Random.Range(0, this.patrolPts.Count);
		}
		while (num == this.patrolIdx);
		this.patrolIdx = num;
		return this.patrolPts[num];
	}

	private void PickNewPath(bool pathFinished = false)
	{
		if (this.calculatingPath)
		{
			return;
		}
		this.currentWaypoint = 0;
		switch (this.replState.state)
		{
		case MonkeyeAI_ReplState.EStates.Patrolling:
			if (this.patrolCount == this.maxPatrols)
			{
				this.SetState(MonkeyeAI_ReplState.EStates.Patrolling);
				this.targetPosition = this.PickRandomPatrolPoint().position;
				this.patrolCount = 0;
			}
			else
			{
				this.targetPosition = this.PickRandomPatrolPoint().position;
				this.patrolCount++;
			}
			break;
		case MonkeyeAI_ReplState.EStates.Chasing:
			if (!this.lockedOn)
			{
				Vector3 position = base.transform.position;
				VRRig vrrig;
				if (this.ClosestPlayer(in position, out vrrig) && vrrig != this.targetRig)
				{
					this.SetTargetPlayer(vrrig);
				}
			}
			if (this.targetRig == null)
			{
				this.SetState(MonkeyeAI_ReplState.EStates.Patrolling);
				this.targetPosition = this.sleepPt.position;
			}
			else
			{
				this.targetPosition = this.targetRig.transform.position;
			}
			break;
		case MonkeyeAI_ReplState.EStates.ReturnToSleepPt:
			this.targetPosition = this.sleepPt.position;
			break;
		}
		this.calculatingPath = true;
		this.seeker.StartPath(base.transform.position, this.targetPosition, new OnPathDelegate(this.OnPathComplete));
	}

	private void Awake()
	{
		this.lazerFx = base.GetComponent<Monkeye_LazerFX>();
		this.animController = base.GetComponent<Animator>();
		this.layerBase = this.animController.GetLayerIndex("Base_Layer");
		this.layerForward = this.animController.GetLayerIndex("MoveFwdAddPose");
		this.layerLeft = this.animController.GetLayerIndex("TurnLAddPose");
		this.layerRight = this.animController.GetLayerIndex("TurnRAddPose");
		this.seeker = base.GetComponent<Seeker>();
		this.renderer = this.portalFx.GetComponent<Renderer>();
		this.portalMatPropBlock = new MaterialPropertyBlock();
		this.monkEyeMatPropBlock = new MaterialPropertyBlock();
		this.layerMask = UnityLayer.Default.ToLayerMask() | UnityLayer.GorillaObject.ToLayerMask();
		this.SetDefaultAttackState();
		this.SetState(MonkeyeAI_ReplState.EStates.Sleeping);
		this.replStateRequestableOwnershipGaurd = this.replState.GetComponent<RequestableOwnershipGuard>();
		this.myRequestableOwnershipGaurd = base.GetComponent<RequestableOwnershipGuard>();
		if (this.monkEyeColor.a != 0f || this.monkEyeEyeColorNormal.a != 0f)
		{
			if (this.monkEyeColor.a != 0f)
			{
				this.monkEyeMatPropBlock.SetVector(MonkeyeAI.ColorShaderProp, this.monkEyeColor);
			}
			if (this.monkEyeEyeColorNormal.a != 0f)
			{
				this.monkEyeMatPropBlock.SetVector(MonkeyeAI.EyeColorShaderProp, this.monkEyeEyeColorNormal);
			}
			this.skinnedMeshRenderer.SetPropertyBlock(this.monkEyeMatPropBlock);
		}
		base.InvokeRepeating("AntiOverlapAssurance", 0.2f, 0.5f);
	}

	private void Start()
	{
		NetworkSystem.Instance.RegisterSceneNetworkItem(base.gameObject);
	}

	private void OnPathComplete(Path path_)
	{
		this.path = path_;
		this.currentWaypoint = 0;
		if (this.path.vectorPath.Count < 1)
		{
			base.transform.position = this.sleepPt.position;
			base.transform.rotation = this.sleepPt.rotation;
			this.path = null;
		}
		this.calculatingPath = false;
	}

	private void FollowPath()
	{
		if (this.path == null || this.currentWaypoint >= this.path.vectorPath.Count || this.currentWaypoint < 0)
		{
			this.PickNewPath(false);
			if (this.path == null)
			{
				return;
			}
		}
		if (this.Distance2D(base.transform.position, this.path.vectorPath[this.currentWaypoint]) < 0.01f)
		{
			if (this.currentWaypoint + 1 == this.path.vectorPath.Count)
			{
				this.PickNewPath(true);
				return;
			}
			this.currentWaypoint++;
		}
		Vector3 normalized = (this.path.vectorPath[this.currentWaypoint] - base.transform.position).normalized;
		normalized.y = 0f;
		if (this.animController.GetCurrentAnimatorStateInfo(0).IsName("Move"))
		{
			Vector3 vector = normalized * this.speed;
			base.transform.position += vector * this.deltaTime;
		}
		Mathf.Clamp01(Vector3.Dot(base.transform.forward, normalized) / 1.5707964f);
		if (Mathf.Sign(Vector3.Cross(base.transform.forward, normalized).y) > 0f)
		{
			this.animController.SetLayerWeight(this.layerRight, 0f);
		}
		else
		{
			this.animController.SetLayerWeight(this.layerLeft, 0f);
		}
		this.animController.SetLayerWeight(this.layerForward, 0f);
		Vector3 vector2 = Vector3.RotateTowards(base.transform.forward, normalized, this.rotationSpeed * this.deltaTime, 0f);
		base.transform.rotation = Quaternion.LookRotation(vector2);
	}

	private bool PlayerNear(VRRig rig, float dist, out float playerDist)
	{
		if (rig == null)
		{
			playerDist = float.PositiveInfinity;
			return false;
		}
		playerDist = this.Distance2D(rig.transform.position, base.transform.position);
		return playerDist < dist && Physics.RaycastNonAlloc(new Ray(base.transform.position, rig.transform.position - base.transform.position), this.rayResults, playerDist, this.layerMask) <= 0;
	}

	private void Sleeping()
	{
		this.audioSource.volume = Mathf.Min(this.sleepLoopVolume, this.audioSource.volume + this.deltaTime / this.sleepDuration);
		if (this.audioSource.volume == this.sleepLoopVolume)
		{
			this.SetState(MonkeyeAI_ReplState.EStates.Patrolling);
			this.PickNewPath(false);
		}
	}

	private bool ClosestPlayer(in Vector3 myPos, out VRRig outRig)
	{
		float num = float.MaxValue;
		outRig = null;
		foreach (VRRig vrrig in this.GetValidChoosableRigs())
		{
			float num2 = 0f;
			if (this.PlayerNear(vrrig, this.chaseDistance, out num2) && num2 < num)
			{
				num = num2;
				outRig = vrrig;
			}
		}
		return num != float.MaxValue;
	}

	private bool CheckForChase()
	{
		foreach (VRRig vrrig in this.GetValidChoosableRigs())
		{
			float num = 0f;
			if (this.PlayerNear(vrrig, this.wakeDistance, out num))
			{
				this.SetTargetPlayer(vrrig);
				this.SetState(MonkeyeAI_ReplState.EStates.Chasing);
				this.PickNewPath(false);
				return true;
			}
		}
		return false;
	}

	public void SetChasePlayer(VRRig rig)
	{
		if (!this.GetValidChoosableRigs().Contains(rig))
		{
			return;
		}
		this.SetTargetPlayer(rig);
		this.lockedOn = true;
		this.SetState(MonkeyeAI_ReplState.EStates.Chasing);
		this.PickNewPath(false);
	}

	public void SetSleep()
	{
		if (this.replState.state == MonkeyeAI_ReplState.EStates.Patrolling || this.replState.state == MonkeyeAI_ReplState.EStates.Chasing)
		{
			this.SetState(MonkeyeAI_ReplState.EStates.Sleeping);
		}
	}

	private void Patrolling()
	{
		this.audioSource.volume = Mathf.Min(this.patrolLoopVolume, this.audioSource.volume + this.deltaTime / this.patrolLoopFadeInTime);
		if (this.path == null)
		{
			this.PickNewPath(false);
		}
		if (this.audioSource.volume == this.patrolLoopVolume)
		{
			this.CheckForChase();
		}
	}

	private void Chasing()
	{
		this.audioSource.volume = Mathf.Min(this.chaseLoopVolume, this.audioSource.volume + this.deltaTime / this.chaseLoopFadeInTime);
		this.PickNewPath(false);
		if (this.targetRig == null)
		{
			this.SetState(MonkeyeAI_ReplState.EStates.Patrolling);
			return;
		}
		if (this.Distance2D(base.transform.position, this.targetRig.transform.position) < this.attackDistance)
		{
			this.SetState(MonkeyeAI_ReplState.EStates.BeginAttack);
			return;
		}
	}

	private void ReturnToSleepPt()
	{
		if (this.path == null)
		{
			this.PickNewPath(false);
		}
		if (this.CheckForChase())
		{
			this.SetState(MonkeyeAI_ReplState.EStates.Chasing);
			return;
		}
		if (this.Distance2D(base.transform.position, this.sleepPt.position) < 0.01f)
		{
			this.SetState(MonkeyeAI_ReplState.EStates.Sleeping);
		}
	}

	private void UpdateClientState()
	{
		if (this.wasConnectedToRoom && !NetworkSystem.Instance.InRoom)
		{
			this.SetDefaultState();
			return;
		}
		if (ColliderEnabledManager.instance != null && !this.replState.floorEnabled)
		{
			if (!NetworkSystem.Instance.InRoom)
			{
				if (this.replState.userId == "-1")
				{
					ColliderEnabledManager.instance.DisableFloorForFrame();
				}
			}
			else if (this.replState.userId == NetworkSystem.Instance.LocalPlayer.UserId)
			{
				ColliderEnabledManager.instance.DisableFloorForFrame();
			}
		}
		if (this.portalFx.activeSelf != this.replState.portalEnabled)
		{
			this.portalFx.SetActive(this.replState.portalEnabled);
		}
		this.portalFx.transform.position = new Vector3(this.replState.attackPos.x, this.portalFx.transform.position.y, this.replState.attackPos.z);
		this.replState.timer -= this.deltaTime;
		if (this.replState.timer < 0f)
		{
			this.replState.timer = 0f;
		}
		VRRig rig = this.GetRig(this.replState.userId);
		if (this.replState.state >= MonkeyeAI_ReplState.EStates.BeginAttack)
		{
			if (rig == null)
			{
				this.lazerFx.DisableLazer();
			}
			else if (this.replState.state < MonkeyeAI_ReplState.EStates.DropPlayer)
			{
				this.lazerFx.EnableLazer(this.eyeBones, rig);
			}
			else
			{
				this.lazerFx.DisableLazer();
			}
		}
		else
		{
			this.lazerFx.DisableLazer();
		}
		if (this.replState.portalEnabled)
		{
			this.portalColor.a = this.replState.alpha;
			this.portalMatPropBlock.SetVector(MonkeyeAI.tintColorShaderProp, this.portalColor);
			this.renderer.SetPropertyBlock(this.portalMatPropBlock);
		}
		if (GorillaTagger.Instance.offlineVRRig == rig && this.replState.freezePlayer)
		{
			GTPlayer.Instance.SetMaximumSlipThisFrame();
			Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
			Vector3 linearVelocity = rigidbody.linearVelocity;
			rigidbody.linearVelocity = new Vector3(linearVelocity.x * this.deltaTime * 4f, Mathf.Min(linearVelocity.y, 0f), linearVelocity.x * this.deltaTime * 4f);
		}
		if (!this.replState.IsMine)
		{
			this.SetClientState(this.replState.state);
		}
	}

	private void SetDefaultState()
	{
		this.SetState(MonkeyeAI_ReplState.EStates.Sleeping);
		this.SetDefaultAttackState();
	}

	private void SetDefaultAttackState()
	{
		this.replState.floorEnabled = true;
		this.replState.timer = 0f;
		this.replState.userId = "";
		this.replState.attackPos = base.transform.position;
		this.replState.portalEnabled = false;
		this.replState.freezePlayer = false;
		this.replState.alpha = 0f;
	}

	private void ExitAttackState()
	{
		this.SetDefaultAttackState();
		this.SetState(MonkeyeAI_ReplState.EStates.Patrolling);
	}

	private void BeginAttack()
	{
		this.path = null;
		this.replState.freezePlayer = true;
		if (this.replState.timer <= 0f)
		{
			if (this.audioSource.isActiveAndEnabled)
			{
				this.audioSource.GTPlayOneShot(this.attackSound, this.attackVolume);
			}
			this.replState.timer = this.openFloorTime;
			this.replState.portalEnabled = true;
			this.SetState(MonkeyeAI_ReplState.EStates.OpenFloor);
		}
	}

	private void OpenFloor()
	{
		this.replState.alpha = Mathf.Lerp(0f, 1f, 1f - Mathf.Clamp01(this.replState.timer / this.openFloorTime));
		if (this.replState.timer <= 0f)
		{
			this.replState.timer = this.dropPlayerTime;
			this.replState.floorEnabled = false;
			this.SetState(MonkeyeAI_ReplState.EStates.DropPlayer);
		}
	}

	private void DropPlayer()
	{
		if (this.replState.timer <= 0f)
		{
			this.replState.timer = this.dropPlayerTime;
			this.replState.floorEnabled = true;
			this.SetState(MonkeyeAI_ReplState.EStates.CloseFloor);
		}
	}

	private void CloseFloor()
	{
		if (this.replState.timer <= 0f)
		{
			this.ExitAttackState();
		}
	}

	private void ValidateChasingRig()
	{
		if (this.targetRig == null)
		{
			this.SetTargetPlayer(null);
			return;
		}
		bool flag = false;
		foreach (VRRig vrrig in this.GetValidChoosableRigs())
		{
			if (vrrig == this.targetRig)
			{
				flag = true;
				this.SetTargetPlayer(vrrig);
				break;
			}
		}
		if (!flag)
		{
			this.SetTargetPlayer(null);
		}
	}

	public void SetState(MonkeyeAI_ReplState.EStates state_)
	{
		if (this.replState.IsMine)
		{
			this.replState.state = state_;
		}
		this.animController.SetInteger(MonkeyeAI.animStateID, (int)this.replState.state);
		switch (this.replState.state)
		{
		case MonkeyeAI_ReplState.EStates.Sleeping:
			this.setEyeColor(this.monkEyeEyeColorNormal);
			this.lockedOn = false;
			this.audioSource.clip = this.sleepLoopSound;
			this.audioSource.volume = 0f;
			if (this.audioSource.isActiveAndEnabled)
			{
				this.audioSource.GTPlay();
				return;
			}
			break;
		case MonkeyeAI_ReplState.EStates.Patrolling:
			this.setEyeColor(this.monkEyeEyeColorNormal);
			this.lockedOn = false;
			this.audioSource.clip = this.patrolLoopSound;
			this.audioSource.loop = true;
			this.audioSource.volume = 0f;
			if (this.audioSource.isActiveAndEnabled)
			{
				this.audioSource.GTPlay();
			}
			this.patrolCount = 0;
			return;
		case MonkeyeAI_ReplState.EStates.Chasing:
			this.setEyeColor(this.monkEyeEyeColorNormal);
			this.audioSource.loop = true;
			this.audioSource.volume = 0f;
			this.audioSource.clip = this.chaseLoopSound;
			if (this.audioSource.isActiveAndEnabled)
			{
				this.audioSource.GTPlay();
				return;
			}
			break;
		case MonkeyeAI_ReplState.EStates.ReturnToSleepPt:
		case MonkeyeAI_ReplState.EStates.GoToSleep:
			break;
		case MonkeyeAI_ReplState.EStates.BeginAttack:
			this.setEyeColor(this.monkEyeEyeColorAttacking);
			if (this.replState.IsMine)
			{
				this.replState.attackPos = ((this.targetRig != null) ? this.targetRig.transform.position : base.transform.position);
				this.replState.timer = this.beginAttackTime;
			}
			break;
		default:
			return;
		}
	}

	public void SetClientState(MonkeyeAI_ReplState.EStates state_)
	{
		this.animController.SetInteger(MonkeyeAI.animStateID, (int)this.replState.state);
		if (this.previousState != this.replState.state)
		{
			this.previousState = this.replState.state;
			switch (this.replState.state)
			{
			case MonkeyeAI_ReplState.EStates.Sleeping:
				this.setEyeColor(this.monkEyeEyeColorNormal);
				this.lockedOn = false;
				this.audioSource.clip = this.sleepLoopSound;
				this.audioSource.volume = Mathf.Min(this.sleepLoopVolume, this.audioSource.volume + this.deltaTime / this.sleepDuration);
				if (this.audioSource.isActiveAndEnabled)
				{
					this.audioSource.GTPlay();
				}
				break;
			case MonkeyeAI_ReplState.EStates.Patrolling:
				this.setEyeColor(this.monkEyeEyeColorNormal);
				this.lockedOn = false;
				this.audioSource.clip = this.patrolLoopSound;
				this.audioSource.loop = true;
				this.audioSource.volume = Mathf.Min(this.patrolLoopVolume, this.audioSource.volume + this.deltaTime / this.patrolLoopFadeInTime);
				if (this.audioSource.isActiveAndEnabled)
				{
					this.audioSource.GTPlay();
				}
				this.patrolCount = 0;
				break;
			case MonkeyeAI_ReplState.EStates.Chasing:
				this.setEyeColor(this.monkEyeEyeColorNormal);
				this.audioSource.loop = true;
				this.audioSource.volume = Mathf.Min(this.chaseLoopVolume, this.audioSource.volume + this.deltaTime / this.chaseLoopFadeInTime);
				this.audioSource.clip = this.chaseLoopSound;
				if (this.audioSource.isActiveAndEnabled)
				{
					this.audioSource.GTPlay();
				}
				break;
			case MonkeyeAI_ReplState.EStates.BeginAttack:
				this.setEyeColor(this.monkEyeEyeColorAttacking);
				break;
			}
		}
		switch (this.replState.state)
		{
		case MonkeyeAI_ReplState.EStates.Sleeping:
			this.audioSource.volume = Mathf.Min(this.sleepLoopVolume, this.audioSource.volume + this.deltaTime / this.sleepDuration);
			return;
		case MonkeyeAI_ReplState.EStates.Patrolling:
			this.audioSource.volume = Mathf.Min(this.patrolLoopVolume, this.audioSource.volume + this.deltaTime / this.patrolLoopFadeInTime);
			return;
		case MonkeyeAI_ReplState.EStates.Chasing:
			this.audioSource.volume = Mathf.Min(this.chaseLoopVolume, this.audioSource.volume + this.deltaTime / this.chaseLoopFadeInTime);
			return;
		default:
			return;
		}
	}

	private void setEyeColor(Color c)
	{
		if (c.a != 0f)
		{
			this.monkEyeMatPropBlock.SetVector(MonkeyeAI.EyeColorShaderProp, c);
			this.skinnedMeshRenderer.SetPropertyBlock(this.monkEyeMatPropBlock);
		}
	}

	public List<VRRig> GetValidChoosableRigs()
	{
		this.validRigs.Clear();
		foreach (VRRig vrrig in this.playerCollection.containedRigs)
		{
			if ((NetworkSystem.Instance.InRoom || vrrig.isOfflineVRRig) && !(vrrig == null))
			{
				this.validRigs.Add(vrrig);
			}
		}
		return this.validRigs;
	}

	public void SliceUpdate()
	{
		this.wasConnectedToRoom = NetworkSystem.Instance.InRoom;
		this.deltaTime = Time.time - this.lastTime;
		this.lastTime = Time.time;
		this.UpdateClientState();
		if (NetworkSystem.Instance.InRoom && !this.replState.IsMine)
		{
			this.path = null;
			return;
		}
		if (!this.playerCollection.gameObject.activeInHierarchy)
		{
			NetPlayer netPlayer = null;
			float num = float.PositiveInfinity;
			foreach (VRRig vrrig in this.playersInRoomCollection.containedRigs)
			{
				if (!(vrrig == null))
				{
					float num2 = Vector3.Distance(base.transform.position, vrrig.transform.position);
					if (num2 < num)
					{
						netPlayer = vrrig.creator;
						num = num2;
					}
				}
			}
			if (num > 6f)
			{
				return;
			}
			this.path = null;
			if (netPlayer == null)
			{
				return;
			}
			this.replStateRequestableOwnershipGaurd.TransferOwnership(netPlayer, "");
			this.myRequestableOwnershipGaurd.TransferOwnership(netPlayer, "");
			return;
		}
		else
		{
			this.ValidateChasingRig();
			switch (this.replState.state)
			{
			case MonkeyeAI_ReplState.EStates.Sleeping:
				this.Sleeping();
				break;
			case MonkeyeAI_ReplState.EStates.Patrolling:
				this.Patrolling();
				break;
			case MonkeyeAI_ReplState.EStates.Chasing:
				this.Chasing();
				break;
			case MonkeyeAI_ReplState.EStates.ReturnToSleepPt:
				this.ReturnToSleepPt();
				break;
			case MonkeyeAI_ReplState.EStates.BeginAttack:
				this.BeginAttack();
				break;
			case MonkeyeAI_ReplState.EStates.OpenFloor:
				this.OpenFloor();
				break;
			case MonkeyeAI_ReplState.EStates.DropPlayer:
				this.DropPlayer();
				break;
			case MonkeyeAI_ReplState.EStates.CloseFloor:
				this.CloseFloor();
				break;
			}
			if (this.path == null)
			{
				return;
			}
			this.FollowPath();
			this.velocity = base.transform.position - this.prevPosition;
			this.prevPosition = base.transform.position;
			return;
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	private void AntiOverlapAssurance()
	{
		try
		{
			if ((!NetworkSystem.Instance.InRoom || this.replState.IsMine) && this.playerCollection.gameObject.activeInHierarchy)
			{
				foreach (MonkeyeAI monkeyeAI in this.playerCollection.monkeyeAis)
				{
					if (!(monkeyeAI == this) && Vector3.Distance(base.transform.position, monkeyeAI.transform.position) < this.overlapRadius && (double)Vector3.Dot(base.transform.forward, monkeyeAI.transform.forward) > 0.2)
					{
						MonkeyeAI_ReplState.EStates state = this.replState.state;
						if (state != MonkeyeAI_ReplState.EStates.Patrolling)
						{
							if (state == MonkeyeAI_ReplState.EStates.Chasing)
							{
								if (monkeyeAI.replState.state == MonkeyeAI_ReplState.EStates.Chasing)
								{
									this.SetState(MonkeyeAI_ReplState.EStates.Patrolling);
								}
							}
						}
						else
						{
							this.PickNewPath(false);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex, this);
		}
	}

	private void SetTargetPlayer([CanBeNull] VRRig rig)
	{
		if (rig == null)
		{
			this.replState.userId = "";
			this.replState.freezePlayer = false;
			this.replState.floorEnabled = true;
			this.replState.portalEnabled = false;
			this.targetRig = null;
			return;
		}
		this.replState.userId = this.UserIdFromRig(rig);
		this.targetRig = rig;
	}

	public List<Transform> patrolPts;

	public Transform sleepPt;

	private int patrolIdx = -1;

	private int patrolCount;

	private Vector3 targetPosition;

	private MaterialPropertyBlock portalMatPropBlock;

	private MaterialPropertyBlock monkEyeMatPropBlock;

	private Renderer renderer;

	private AIDestinationSetter aiDest;

	private AIPath aiPath;

	private AILerp aiLerp;

	private Seeker seeker;

	private Path path;

	private int currentWaypoint;

	private bool calculatingPath;

	private Monkeye_LazerFX lazerFx;

	private Animator animController;

	private RaycastHit[] rayResults = new RaycastHit[1];

	private LayerMask layerMask;

	private bool wasConnectedToRoom;

	public SkinnedMeshRenderer skinnedMeshRenderer;

	public MazePlayerCollection playerCollection;

	public PlayerCollection playersInRoomCollection;

	private List<VRRig> validRigs = new List<VRRig>();

	public GameObject portalFx;

	public Transform[] eyeBones;

	public float speed = 0.1f;

	public float rotationSpeed = 1f;

	public float wakeDistance = 1f;

	public float chaseDistance = 3f;

	public float sleepDuration = 3f;

	public float attackDistance = 0.1f;

	public float beginAttackTime = 1f;

	public float openFloorTime = 3f;

	public float dropPlayerTime = 1f;

	public float closeFloorTime = 1f;

	public Color portalColor;

	public Color gorillaPortalColor;

	public Color monkEyeColor;

	public Color monkEyeEyeColorNormal;

	public Color monkEyeEyeColorAttacking;

	public int maxPatrols = 4;

	private VRRig targetRig;

	private float deltaTime;

	private float lastTime;

	public MonkeyeAI_ReplState replState;

	private MonkeyeAI_ReplState.EStates previousState;

	private RequestableOwnershipGuard replStateRequestableOwnershipGaurd;

	private RequestableOwnershipGuard myRequestableOwnershipGaurd;

	private int layerBase;

	private int layerForward = 1;

	private int layerLeft = 2;

	private int layerRight = 3;

	private static readonly int EmissionColorShaderProp = ShaderProps._EmissionColor;

	private static readonly int ColorShaderProp = ShaderProps._BaseColor;

	private static readonly int EyeColorShaderProp = ShaderProps._GChannelColor;

	private static readonly int tintColorShaderProp = ShaderProps._TintColor;

	private static readonly int animStateID = Animator.StringToHash("state");

	private Vector3 prevPosition;

	private Vector3 velocity;

	public AudioSource audioSource;

	public AudioClip sleepLoopSound;

	public float sleepLoopVolume = 0.5f;

	[FormerlySerializedAs("moveLoopSound")]
	public AudioClip patrolLoopSound;

	public float patrolLoopVolume = 0.5f;

	public float patrolLoopFadeInTime = 1f;

	public AudioClip chaseLoopSound;

	public float chaseLoopVolume = 0.5f;

	public float chaseLoopFadeInTime = 0.05f;

	public AudioClip attackSound;

	public float attackVolume = 0.5f;

	public float overlapRadius;

	private bool lockedOn;
}
