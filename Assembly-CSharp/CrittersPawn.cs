using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class CrittersPawn : CrittersActor, IEyeScannable
{
	public override void Initialize()
	{
		base.Initialize();
		this.rB = base.GetComponentInChildren<Rigidbody>();
		this.soundsHeard = new Dictionary<int, CrittersActor>();
		base.transform.eulerAngles = new Vector3(0f, Random.value * 360f, 0f);
		this.raycastHits = new RaycastHit[20];
		this.wasSomethingInTheWay = false;
		this._spawnAnimationDuration = this.spawnInHeighMovement.keys.Last<Keyframe>().time;
		this._despawnAnimationDuration = this.despawnInHeighMovement.keys.Last<Keyframe>().time;
	}

	private void InitializeTemplateValues()
	{
		this.sensoryRange *= this.sensoryRange;
		this.autoSeeFoodDistance *= this.autoSeeFoodDistance;
		this.currentSleepiness = Random.value * this.tiredThreshold;
		this.currentHunger = Random.value * this.hungryThreshold;
		this.currentFear = 0f;
		this.currentStruggle = 0f;
		this.currentAttraction = 0f;
	}

	public float JumpVelocityForDistanceAtAngle(float horizontalDistance, float angle)
	{
		return Mathf.Min(this.maxJumpVel, Mathf.Sqrt(horizontalDistance * Physics.gravity.magnitude / Mathf.Sin(2f * angle)));
	}

	public override void OnEnable()
	{
		base.OnEnable();
		CrittersManager.RegisterCritter(this);
		this.lifeTimeStart = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		EyeScannerMono.Register(this);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		CrittersManager.DeregisterCritter(this);
		if (this.currentOngoingStateFX.IsNotNull())
		{
			this.currentOngoingStateFX.SetActive(false);
			this.currentOngoingStateFX = null;
		}
		EyeScannerMono.Unregister(this);
	}

	private float GetAdditiveJumpDelay()
	{
		if (this.currentState == CrittersPawn.CreatureState.Running)
		{
			return 0f;
		}
		return Mathf.Max(0f, this.jumpCooldown * Random.value * this.jumpVariabilityTime);
	}

	public void LocalJump(float maxVel, float jumpAngle)
	{
		maxVel *= this.slowSpeedMod;
		this.lastImpulsePosition = base.transform.position;
		this.lastImpulseVelocity = base.transform.forward * (Mathf.Sin(0.017453292f * jumpAngle) * maxVel) + Vector3.up * (Mathf.Cos(0.017453292f * jumpAngle) * maxVel);
		this.lastImpulseTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		this.lastImpulseTime += (double)this.GetAdditiveJumpDelay();
		this.lastImpulseQuaternion = base.transform.rotation;
		this.rB.linearVelocity = this.lastImpulseVelocity;
		this.rb.angularVelocity = Vector3.zero;
	}

	private bool CanSeeActor(Vector3 actorPosition)
	{
		Vector3 vector = actorPosition - base.transform.position;
		return vector.sqrMagnitude < this.autoSeeFoodDistance || (vector.sqrMagnitude < this.sensoryRange && Vector3.Angle(base.transform.forward, vector) < this.visionConeAngle);
	}

	private bool IsGrabPossible(CrittersGrabber actor)
	{
		return actor.grabbing && (base.transform.position - actor.grabPosition.position).magnitude < actor.grabDistance;
	}

	private bool WithinCaptureDistance(CrittersCage actor)
	{
		return (this.bodyCollider.bounds.center - actor.grabPosition.position).magnitude < actor.grabDistance;
	}

	public bool AwareOfActor(CrittersActor actor)
	{
		CrittersActor.CrittersActorType crittersActorType = actor.crittersActorType;
		switch (crittersActorType)
		{
		case CrittersActor.CrittersActorType.Creature:
			return this.CanSeeActor(actor.transform.position);
		case CrittersActor.CrittersActorType.Food:
			return ((CrittersFood)actor).currentFood > 0f && this.CanSeeActor(((CrittersFood)actor).food.transform.position);
		case CrittersActor.CrittersActorType.LoudNoise:
			return (actor.transform.position - base.transform.position).sqrMagnitude < this.sensoryRange;
		case CrittersActor.CrittersActorType.BrightLight:
			return this.CanSeeActor(actor.transform.position);
		case CrittersActor.CrittersActorType.Darkness:
		case CrittersActor.CrittersActorType.HidingArea:
		case CrittersActor.CrittersActorType.Disappear:
		case CrittersActor.CrittersActorType.Spawn:
		case CrittersActor.CrittersActorType.Player:
		case CrittersActor.CrittersActorType.AttachPoint:
			break;
		case CrittersActor.CrittersActorType.Grabber:
			return this.CanSeeActor(actor.transform.position);
		case CrittersActor.CrittersActorType.Cage:
			return this.CanSeeActor(actor.transform.position);
		case CrittersActor.CrittersActorType.FoodSpawner:
			return this.CanSeeActor(actor.transform.position);
		case CrittersActor.CrittersActorType.StunBomb:
			return this.CanSeeActor(actor.transform.position);
		default:
			if (crittersActorType == CrittersActor.CrittersActorType.StickyGoo)
			{
				return ((CrittersStickyGoo)actor).CanAffect(base.transform.position);
			}
			break;
		}
		return false;
	}

	public override bool ProcessLocal()
	{
		CrittersPawn.CreatureUpdateData creatureUpdateData = new CrittersPawn.CreatureUpdateData(this);
		bool flag = base.ProcessLocal();
		if (!this.isEnabled)
		{
			return flag;
		}
		this.wasSomethingInTheWay = false;
		this.UpdateMoodSourceData();
		this.StuckCheck();
		switch (this.currentState)
		{
		case CrittersPawn.CreatureState.Idle:
			this.IdleStateUpdate();
			this.DespawnCheck();
			break;
		case CrittersPawn.CreatureState.Eating:
			this.EatingStateUpdate();
			this.DespawnCheck();
			break;
		case CrittersPawn.CreatureState.AttractedTo:
			this.AttractedStateUpdate();
			this.DespawnCheck();
			break;
		case CrittersPawn.CreatureState.Running:
			this.RunningStateUpdate();
			this.DespawnCheck();
			break;
		case CrittersPawn.CreatureState.Grabbed:
			this.GrabbedStateUpdate();
			break;
		case CrittersPawn.CreatureState.Sleeping:
			this.SleepingStateUpdate();
			this.DespawnCheck();
			break;
		case CrittersPawn.CreatureState.SeekingFood:
			this.SeekingFoodStateUpdate();
			this.DespawnCheck();
			break;
		case CrittersPawn.CreatureState.Captured:
			this.CapturedStateUpdate();
			break;
		case CrittersPawn.CreatureState.Stunned:
			this.StunnedStateUpdate();
			break;
		case CrittersPawn.CreatureState.WaitingToDespawn:
			this.WaitingToDespawnStateUpdate();
			break;
		case CrittersPawn.CreatureState.Despawning:
			this.DespawningStateUpdate();
			break;
		case CrittersPawn.CreatureState.Spawning:
			this.SpawningStateUpdate();
			break;
		}
		this.UpdateStateAnim();
		this.updatedSinceLastFrame = flag || this.updatedSinceLastFrame || !creatureUpdateData.SameData(this);
		return this.updatedSinceLastFrame;
	}

	private void StuckCheck()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (this._nextStuckCheck > (double)realtimeSinceStartup)
		{
			return;
		}
		this._nextStuckCheck = (double)(realtimeSinceStartup + 1f);
		if (!this.canJump && this.rb.IsSleeping())
		{
			this.canJump = true;
		}
		if (base.transform.position.y < this.killHeight)
		{
			this.SetState(CrittersPawn.CreatureState.Despawning);
		}
	}

	private void DespawnCheck()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (this._nextDespawnCheck > (double)realtimeSinceStartup)
		{
			return;
		}
		this._nextDespawnCheck = (double)(realtimeSinceStartup + 1f);
		bool flag;
		if (this.lifeTime <= 0.0)
		{
			flag = this.creatureConfiguration != null && !this.creatureConfiguration.ShouldDespawn();
		}
		else
		{
			flag = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)) - this.lifeTimeStart > this.lifeTime;
		}
		if (flag)
		{
			this.SetState(CrittersPawn.CreatureState.WaitingToDespawn);
			this.spawningStartingPosition = base.gameObject.transform.position;
			this.despawnStartTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		}
	}

	public void SetTemplate(int templateIndex)
	{
		this.TemplateIndex = templateIndex;
		this.UpdateTemplate();
	}

	private void UpdateTemplate()
	{
		if (this.TemplateIndex != this.LastTemplateIndex)
		{
			this.creatureConfiguration = CrittersManager.instance.creatureIndex[this.TemplateIndex];
			if (this.creatureConfiguration != null)
			{
				this.creatureConfiguration.ApplyToCreature(this);
				this.InitializeAttractors();
			}
			this.LastTemplateIndex = this.TemplateIndex;
			this.InitializeTemplateValues();
		}
		if (this.OnDataChange != null)
		{
			this.OnDataChange();
		}
	}

	private void InitializeAttractors()
	{
		this.attractedToTypes = new Dictionary<CrittersActor.CrittersActorType, float>();
		this.afraidOfTypes = new Dictionary<CrittersActor.CrittersActorType, float>();
		if (this.attractedToList != null)
		{
			for (int i = 0; i < this.attractedToList.Count; i++)
			{
				this.attractedToTypes.Add(this.attractedToList[i].type, this.attractedToList[i].multiplier);
			}
		}
		if (this.afraidOfList != null)
		{
			for (int j = 0; j < this.afraidOfList.Count; j++)
			{
				this.afraidOfTypes.Add(this.afraidOfList[j].type, this.afraidOfList[j].multiplier);
			}
		}
	}

	public override void ProcessRemote()
	{
		this.UpdateTemplate();
		base.ProcessRemote();
		this.UpdateStateAnim();
	}

	public void SetState(CrittersPawn.CreatureState newState)
	{
		if (this.currentState == newState)
		{
			return;
		}
		if (this.currentState == CrittersPawn.CreatureState.Captured)
		{
			base.transform.localScale = Vector3.one;
		}
		this.ClearOngoingStateFX();
		this.currentState = newState;
		if (newState != CrittersPawn.CreatureState.Despawning)
		{
			if (newState == CrittersPawn.CreatureState.Spawning && CrittersManager.instance.LocalAuthority())
			{
				this.spawningStartingPosition = base.gameObject.transform.position;
				this.spawnStartTime = (double)(PhotonNetwork.InRoom ? ((float)PhotonNetwork.Time) : Time.time);
			}
		}
		else if (CrittersManager.instance.LocalAuthority())
		{
			this.spawningStartingPosition = base.gameObject.transform.position;
			this.despawnStartTime = (double)(PhotonNetwork.InRoom ? ((float)PhotonNetwork.Time) : Time.time);
		}
		this.StartOngoingStateFX(newState);
		GameObject valueOrDefault = this.StartStateFX.GetValueOrDefault(this.currentState);
		if (valueOrDefault.IsNotNull())
		{
			GameObject pooled = CrittersPool.GetPooled(valueOrDefault);
			if (pooled != null)
			{
				pooled.transform.position = base.transform.position;
			}
		}
		this.currentAnimTime = 0f;
		CrittersAnim crittersAnim;
		if (this.stateAnim.TryGetValue(this.currentState, out crittersAnim))
		{
			this.currentAnim = crittersAnim;
		}
		else
		{
			this.currentAnim = null;
			this.animTarget.localPosition = Vector3.zero;
			this.animTarget.localScale = Vector3.one;
		}
		if (this.OnDataChange != null)
		{
			this.OnDataChange();
		}
	}

	private void ClearOngoingStateFX()
	{
		if (this.currentOngoingStateFX.IsNotNull())
		{
			CrittersPool.Return(this.currentOngoingStateFX);
			this.currentOngoingStateFX = null;
		}
	}

	private void StartOngoingStateFX(CrittersPawn.CreatureState state)
	{
		GameObject valueOrDefault = this.OngoingStateFX.GetValueOrDefault(state);
		if (valueOrDefault.IsNotNull())
		{
			this.currentOngoingStateFX = CrittersPool.GetPooled(valueOrDefault);
			if (this.currentOngoingStateFX.IsNotNull())
			{
				this.currentOngoingStateFX.transform.SetParent(base.transform, false);
				this.currentOngoingStateFX.transform.localPosition = Vector3.zero;
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void UpdateStateColor()
	{
		switch (this.currentState)
		{
		case CrittersPawn.CreatureState.Idle:
			this.debugStateIndicator.material.color = this.debugColorIdle;
			return;
		case CrittersPawn.CreatureState.Eating:
			this.debugStateIndicator.material.color = this.debugColorEating;
			return;
		case CrittersPawn.CreatureState.AttractedTo:
			this.debugStateIndicator.material.color = this.debugColorAttracted;
			return;
		case CrittersPawn.CreatureState.Running:
			this.debugStateIndicator.material.color = this.debugColorScared;
			return;
		case CrittersPawn.CreatureState.Grabbed:
			this.debugStateIndicator.material.color = this.debugColorCaught;
			return;
		case CrittersPawn.CreatureState.Sleeping:
			this.debugStateIndicator.material.color = this.debugColorSleeping;
			return;
		case CrittersPawn.CreatureState.SeekingFood:
			this.debugStateIndicator.material.color = this.debugColorSeekingFood;
			return;
		case CrittersPawn.CreatureState.Captured:
			this.debugStateIndicator.material.color = this.debugColorCaged;
			return;
		case CrittersPawn.CreatureState.Stunned:
			this.debugStateIndicator.material.color = this.debugColorStunned;
			return;
		default:
			this.debugStateIndicator.material.color = new Color(1f, 0f, 1f);
			return;
		}
	}

	public void UpdateStateAnim()
	{
		if (this.currentAnim != null)
		{
			this.currentAnimTime += Time.deltaTime * this.currentAnim.playSpeed;
			this.currentAnimTime %= 1f;
			float num = this.currentAnim.squashAmount.Evaluate(this.currentAnimTime);
			float num2 = this.currentAnim.forwardOffset.Evaluate(this.currentAnimTime);
			float num3 = this.currentAnim.horizontalOffset.Evaluate(this.currentAnimTime);
			float num4 = this.currentAnim.verticalOffset.Evaluate(this.currentAnimTime);
			this.animTarget.localPosition = new Vector3(num3, num4, num2);
			float num5 = 1f - num;
			num5 *= 0.5f;
			num5 += 1f;
			this.animTarget.localScale = new Vector3(num5, num, num5);
		}
	}

	public void IdleStateUpdate()
	{
		if (this.AboveFearThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Running);
			return;
		}
		if (this.AboveAttractedThreshold() && (!this.AboveHungryThreshold() || !CrittersManager.AnyFoodNearby(this)))
		{
			this.SetState(CrittersPawn.CreatureState.AttractedTo);
			return;
		}
		if (this.AboveHungryThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.SeekingFood);
			return;
		}
		if (this.AboveSleepyThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Sleeping);
			return;
		}
		if (this.CanJump())
		{
			this.RandomJump();
		}
	}

	public void EatingStateUpdate()
	{
		if (this.AboveFearThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Running);
			return;
		}
		if (this.BelowNotHungryThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Idle);
			return;
		}
		if (!this.withinEatingRadius || this.eatingTarget.IsNull() || this.eatingTarget.currentFood <= 0f)
		{
			this.SetState(CrittersPawn.CreatureState.SeekingFood);
		}
	}

	public void SleepingStateUpdate()
	{
		if (this.AboveFearThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Running);
			return;
		}
		if (this.BelowNotSleepyThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Idle);
		}
	}

	public void AttractedStateUpdate()
	{
		if (this.AboveFearThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Running);
			return;
		}
		if (this.BelowUnAttractedThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Idle);
			return;
		}
		if (this.CanJump())
		{
			if (this.AboveHungryThreshold() && CrittersManager.AnyFoodNearby(this))
			{
				this.SetState(CrittersPawn.CreatureState.SeekingFood);
				return;
			}
			if (CrittersManager.instance.awareOfActors[this].Contains(this.attractionTarget))
			{
				this.lastSeenAttractionPosition = this.attractionTarget.transform.position;
			}
			this.JumpTowards(this.lastSeenAttractionPosition);
		}
	}

	public void RunningStateUpdate()
	{
		if (this.CanJump())
		{
			if (CrittersManager.instance.awareOfActors[this].Contains(this.fearTarget))
			{
				this.lastSeenFearPosition = this.fearTarget.transform.position;
			}
			this.JumpAwayFrom(this.lastSeenFearPosition);
		}
		if (this.BelowNotAfraidThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Idle);
		}
	}

	public void SeekingFoodStateUpdate()
	{
		if (this.AboveFearThreshold())
		{
			this.SetState(CrittersPawn.CreatureState.Running);
			return;
		}
		if (this.CanJump())
		{
			if (CrittersManager.CritterAwareOfAny(this))
			{
				this.eatingTarget = CrittersManager.ClosestFood(this);
				if (this.eatingTarget != null)
				{
					this.withinEatingRadius = (this.eatingTarget.food.transform.position - base.transform.position).sqrMagnitude < this.eatingRadiusMaxSquared;
					if (!this.withinEatingRadius)
					{
						this.JumpTowards(this.eatingTarget.food.transform.position);
						return;
					}
					base.transform.forward = (this.eatingTarget.food.transform.position - base.transform.position).X_Z().normalized;
					this.SetState(CrittersPawn.CreatureState.Eating);
					this.debugStateIndicator.material.color = this.debugColorEating;
					return;
				}
				else
				{
					if (this.AboveAttractedThreshold())
					{
						this.SetState(CrittersPawn.CreatureState.AttractedTo);
						return;
					}
					this.RandomJump();
					return;
				}
			}
			else
			{
				this.RandomJump();
			}
		}
	}

	public void GrabbedStateUpdate()
	{
		if (this.currentState == CrittersPawn.CreatureState.Grabbed && this.grabbedTarget != null)
		{
			if (this.currentStruggle >= this.escapeThreshold || !this.grabbedTarget.grabbing)
			{
				this.Released(true, default(Quaternion), default(Vector3), default(Vector3), default(Vector3));
				return;
			}
		}
		else if (this.grabbedTarget == null)
		{
			this.Released(true, default(Quaternion), default(Vector3), default(Vector3), default(Vector3));
		}
	}

	protected override void HandleRemoteReleased()
	{
		base.HandleRemoteReleased();
		if (this.cageTarget.IsNotNull())
		{
			this.fearTarget = this.cageTarget;
			this.cageTarget.SetHasCritter(false);
			this.cageTarget = null;
		}
		if (this.grabbedTarget.IsNotNull())
		{
			this.fearTarget = this.grabbedTarget;
			this.grabbedTarget = null;
			if (this.OnReleasedFX)
			{
				CrittersPool.GetPooled(this.OnReleasedFX).transform.position = base.transform.position;
			}
		}
	}

	public override void Released(bool keepWorldPosition, Quaternion rotation = default(Quaternion), Vector3 position = default(Vector3), Vector3 impulse = default(Vector3), Vector3 impulseRotation = default(Vector3))
	{
		base.Released(keepWorldPosition, rotation, position, impulse, impulseRotation);
		if (this.currentState != CrittersPawn.CreatureState.Grabbed && this.currentState != CrittersPawn.CreatureState.Captured)
		{
			return;
		}
		if (this.grabbedTarget.IsNotNull() && this.grabbedTarget.grabbedActors.Contains(this))
		{
			this.grabbedTarget.grabbedActors.Remove(this);
		}
		if (this.currentState == CrittersPawn.CreatureState.Grabbed)
		{
			this.fearTarget = this.grabbedTarget;
			this.grabbedTarget = null;
			if (this.OnReleasedFX)
			{
				CrittersPool.GetPooled(this.OnReleasedFX).transform.position = base.transform.position;
			}
		}
		else if (this.currentState == CrittersPawn.CreatureState.Captured)
		{
			base.transform.localScale = Vector3.one;
			this.fearTarget = this.cageTarget;
			this.cageTarget.SetHasCritter(false);
			this.cageTarget = null;
		}
		if (this.struggleGainedPerSecond > 0f)
		{
			this.currentFear = this.maxFear;
			this.SetState(CrittersPawn.CreatureState.Running);
			this.lastSeenFearPosition = this.fearTarget.transform.position;
			return;
		}
		this.currentFear = 0f;
		this.SetState(CrittersPawn.CreatureState.Idle);
	}

	public void CapturedStateUpdate()
	{
		if (this.cageTarget.IsNull())
		{
			this.cageTarget = (CrittersCage)CrittersManager.instance.actorById[this.actorIdTarget];
			this.cageTarget.SetHasCritter(false);
		}
		if (this.cageTarget.inReleasingPosition && this.cageTarget.heldByPlayer)
		{
			this.Released(true, default(Quaternion), default(Vector3), default(Vector3), default(Vector3));
		}
	}

	public void StunnedStateUpdate()
	{
		this.remainingStunnedTime = Mathf.Max(0f, this.remainingStunnedTime - Time.deltaTime);
		if (this.remainingStunnedTime <= 0f)
		{
			this.currentFear = this.maxFear;
			this.SetState(CrittersPawn.CreatureState.Running);
		}
	}

	public void WaitingToDespawnStateUpdate()
	{
		if (Mathf.FloorToInt(this.rb.linearVelocity.magnitude * 10f) == 0)
		{
			this.SetState(CrittersPawn.CreatureState.Despawning);
		}
	}

	public void DespawningStateUpdate()
	{
		this._despawnAnimTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)) - this.despawnStartTime;
		if (this._despawnAnimTime >= (double)this._despawnAnimationDuration)
		{
			base.gameObject.SetActive(false);
			this.TemplateIndex = -1;
		}
	}

	public void SpawningStateUpdate()
	{
		this._spawnAnimTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)) - this.spawnStartTime;
		base.MoveActor(this.spawningStartingPosition + new Vector3(0f, this.spawnInHeighMovement.Evaluate(Mathf.Clamp((float)this._spawnAnimTime, 0f, this._spawnAnimationDuration)), 0f), base.transform.rotation, false, true, true);
		if (this._spawnAnimTime >= (double)this._spawnAnimationDuration)
		{
			this.SetState(CrittersPawn.CreatureState.Idle);
		}
	}

	public void UpdateMoodSourceData()
	{
		this.UpdateHunger();
		this.UpdateFearAndAttraction();
		this.UpdateSleepiness();
		this.UpdateStruggle();
		this.UpdateSlowed();
		this.UpdateGrabbed();
		this.UpdateCaged();
	}

	public void UpdateHunger()
	{
		if (this.currentState == CrittersPawn.CreatureState.Eating && !this.eatingTarget.IsNull())
		{
			this.eatingTarget.Feed(this.hungerLostPerSecond * Time.deltaTime);
			this.currentHunger = Mathf.Max(0f, this.currentHunger - this.hungerLostPerSecond * Time.deltaTime);
			return;
		}
		this.currentHunger = Mathf.Min(this.maxHunger, this.currentHunger + this.hungerGainedPerSecond * Time.deltaTime);
	}

	public void UpdateFearAndAttraction()
	{
		if (this.currentState == CrittersPawn.CreatureState.Spawning)
		{
			return;
		}
		this.currentFear = Mathf.Max(0f, this.currentFear - this.fearLostPerSecond * Time.deltaTime);
		this.currentAttraction = Mathf.Max(0f, this.currentAttraction - this.attractionLostPerSecond * Time.deltaTime);
		for (int i = 0; i < CrittersManager.instance.awareOfActors[this].Count; i++)
		{
			CrittersActor crittersActor = CrittersManager.instance.awareOfActors[this][i];
			float num;
			float num2;
			if (this.afraidOfTypes != null && this.afraidOfTypes.TryGetValue(crittersActor.crittersActorType, out num))
			{
				crittersActor.CalculateFear(this, num);
			}
			else if (this.attractedToTypes != null && this.attractedToTypes.TryGetValue(crittersActor.crittersActorType, out num2))
			{
				crittersActor.CalculateAttraction(this, num2);
			}
		}
	}

	public void IncreaseFear(float fearAmount, CrittersActor actor)
	{
		if (fearAmount > 0f)
		{
			this.currentFear += fearAmount;
			this.currentFear = Mathf.Min(this.maxFear, this.currentFear);
			this.fearTarget = actor;
			this.lastSeenFearPosition = this.fearTarget.transform.position;
		}
	}

	public void IncreaseAttraction(float attractionAmount, CrittersActor actor)
	{
		if (attractionAmount > 0f)
		{
			this.currentAttraction += attractionAmount;
			this.currentAttraction = Mathf.Min(this.maxAttraction, this.currentAttraction);
			this.attractionTarget = actor;
			this.lastSeenAttractionPosition = this.attractionTarget.transform.position;
		}
	}

	public void UpdateSleepiness()
	{
		if (this.currentState == CrittersPawn.CreatureState.Sleeping)
		{
			this.currentSleepiness = Mathf.Max(0f, this.currentSleepiness - Time.deltaTime * this.sleepinessLostPerSecond);
			return;
		}
		this.currentSleepiness = Mathf.Min(this.maxSleepiness, this.currentSleepiness + Time.deltaTime * this.sleepinessGainedPerSecond);
	}

	public void UpdateStruggle()
	{
		if (this.currentState == CrittersPawn.CreatureState.Grabbed)
		{
			this.currentStruggle = Mathf.Clamp(this.currentStruggle + this.struggleGainedPerSecond * Time.deltaTime, 0f, this.maxStruggle);
			return;
		}
		this.currentStruggle = Mathf.Max(0f, this.currentStruggle - this.struggleLostPerSecond * Time.deltaTime);
	}

	private void UpdateSlowed()
	{
		if (this.remainingSlowedTime > 0f)
		{
			this.remainingSlowedTime -= Time.deltaTime;
			if (this.remainingSlowedTime < 0f)
			{
				this.slowSpeedMod = 1f;
				return;
			}
		}
		else if (this.currentState != CrittersPawn.CreatureState.Captured && this.currentState != CrittersPawn.CreatureState.Despawning && this.currentState != CrittersPawn.CreatureState.Grabbed && this.currentState != CrittersPawn.CreatureState.WaitingToDespawn && this.currentState != CrittersPawn.CreatureState.Spawning)
		{
			for (int i = 0; i < CrittersManager.instance.awareOfActors[this].Count; i++)
			{
				CrittersActor crittersActor = CrittersManager.instance.awareOfActors[this][i];
				if (crittersActor.crittersActorType == CrittersActor.CrittersActorType.StickyGoo)
				{
					CrittersStickyGoo crittersStickyGoo = crittersActor as CrittersStickyGoo;
					this.slowSpeedMod = crittersStickyGoo.slowModifier;
					this.remainingSlowedTime = crittersStickyGoo.slowDuration;
					crittersStickyGoo.EffectApplied(this);
				}
			}
		}
	}

	public void UpdateGrabbed()
	{
		if (this.currentState == CrittersPawn.CreatureState.Grabbed || this.currentState == CrittersPawn.CreatureState.Captured)
		{
			return;
		}
		for (int i = 0; i < CrittersManager.instance.awareOfActors[this].Count; i++)
		{
			CrittersActor crittersActor = CrittersManager.instance.awareOfActors[this][i];
			if (crittersActor.crittersActorType == CrittersActor.CrittersActorType.Grabber && !crittersActor.isOnPlayer && this.IsGrabPossible((CrittersGrabber)crittersActor))
			{
				this.GrabbedBy(crittersActor, true, default(Quaternion), default(Vector3), false);
			}
		}
	}

	public void UpdateCaged()
	{
		if (this.currentState == CrittersPawn.CreatureState.Captured)
		{
			return;
		}
		for (int i = 0; i < CrittersManager.instance.awareOfActors[this].Count; i++)
		{
			CrittersActor crittersActor = CrittersManager.instance.awareOfActors[this][i];
			CrittersCage crittersCage = crittersActor as CrittersCage;
			if (crittersActor.crittersActorType == CrittersActor.CrittersActorType.Cage && crittersCage.IsNotNull() && crittersCage.CanCatch && this.WithinCaptureDistance(crittersCage))
			{
				this.GrabbedBy(crittersActor, true, crittersCage.cagePosition.localRotation, crittersCage.cagePosition.localPosition, false);
			}
		}
	}

	public void RandomJump()
	{
		for (int i = 0; i < 5; i++)
		{
			base.transform.eulerAngles = new Vector3(0f, 360f * Random.value, 0f);
			if (!this.SomethingInTheWay(default(Vector3)))
			{
				break;
			}
		}
		this.LocalJump(this.maxJumpVel, 45f);
	}

	public void JumpTowards(Vector3 targetPos)
	{
		if (this.SomethingInTheWay((targetPos - base.transform.position).X_Z()))
		{
			this.RandomJump();
			return;
		}
		base.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(targetPos - base.transform.position, Vector3.up), Vector3.up);
		this.LocalJump(this.JumpVelocityForDistanceAtAngle(Vector3.ProjectOnPlane(targetPos - base.transform.position, Vector3.up).magnitude * this.fudge, 45f), 45f);
	}

	public void JumpAwayFrom(Vector3 targetPos)
	{
		Vector3 vector = (base.transform.position - targetPos).X_Z();
		if (vector == Vector3.zero)
		{
			vector = base.transform.forward;
		}
		Vector3 vector2 = Quaternion.Euler(0f, (float)Random.Range(-30, 30), 0f) * vector;
		if (this.SomethingInTheWay(vector2))
		{
			this.RandomJump();
			return;
		}
		base.transform.rotation = Quaternion.LookRotation(vector2, Vector3.up);
		this.LocalJump(this.maxJumpVel, 45f);
	}

	public bool SomethingInTheWay(Vector3 direction = default(Vector3))
	{
		if (direction == default(Vector3))
		{
			direction = base.transform.forward;
		}
		bool flag = Physics.RaycastNonAlloc(this.bodyCollider.bounds.center, direction, this.raycastHits, this.obstacleSeeDistance, CrittersManager.instance.movementLayers) > 0;
		this.wasSomethingInTheWay = this.wasSomethingInTheWay || flag;
		return flag;
	}

	public override bool CanBeGrabbed(CrittersActor grabbedBy)
	{
		return this.currentState != CrittersPawn.CreatureState.Captured && base.CanBeGrabbed(grabbedBy);
	}

	public override void GrabbedBy(CrittersActor grabbingActor, bool positionOverride = false, Quaternion localRotation = default(Quaternion), Vector3 localOffset = default(Vector3), bool disableGrabbing = false)
	{
		CrittersActor.CrittersActorType crittersActorType = grabbingActor.crittersActorType;
		if (crittersActorType == CrittersActor.CrittersActorType.Grabber)
		{
			this.SetState(CrittersPawn.CreatureState.Grabbed);
			this.grabbedTarget = (CrittersGrabber)grabbingActor;
			this.actorIdTarget = this.grabbedTarget.actorId;
			base.GrabbedBy(grabbingActor, positionOverride, localRotation, localOffset, disableGrabbing);
			return;
		}
		if (crittersActorType != CrittersActor.CrittersActorType.Cage)
		{
			return;
		}
		this.SetState(CrittersPawn.CreatureState.Captured);
		this.cageTarget = (CrittersCage)grabbingActor;
		this.cageTarget.SetHasCritter(true);
		this.actorIdTarget = this.cageTarget.actorId;
		if (CrittersManager.instance.LocalAuthority())
		{
			base.transform.localScale = this.cageTarget.critterScale;
		}
		base.GrabbedBy(grabbingActor, positionOverride, localRotation, localOffset, disableGrabbing);
	}

	protected override void RemoteGrabbedBy(CrittersActor grabbingActor)
	{
		base.RemoteGrabbedBy(grabbingActor);
		CrittersActor.CrittersActorType crittersActorType = grabbingActor.crittersActorType;
		if (crittersActorType != CrittersActor.CrittersActorType.Grabber)
		{
			if (crittersActorType == CrittersActor.CrittersActorType.Cage)
			{
				this.cageTarget = (CrittersCage)grabbingActor;
				this.cageTarget.SetHasCritter(true);
				this.actorIdTarget = this.cageTarget.actorId;
				if (CrittersManager.instance.LocalAuthority())
				{
					base.transform.localScale = this.cageTarget.critterScale;
					return;
				}
			}
		}
		else
		{
			this.grabbedTarget = (CrittersGrabber)grabbingActor;
			this.actorIdTarget = this.grabbedTarget.actorId;
		}
	}

	public void Stunned(float duration)
	{
		if (this.currentState == CrittersPawn.CreatureState.Captured || this.currentState == CrittersPawn.CreatureState.Grabbed || this.currentState == CrittersPawn.CreatureState.Despawning || this.currentState == CrittersPawn.CreatureState.WaitingToDespawn)
		{
			return;
		}
		this.remainingStunnedTime = duration;
		this.SetState(CrittersPawn.CreatureState.Stunned);
		this.updatedSinceLastFrame = true;
	}

	public bool AboveFearThreshold()
	{
		return this.currentFear >= this.scaredThreshold;
	}

	public bool BelowNotAfraidThreshold()
	{
		return this.currentFear < this.calmThreshold;
	}

	public bool AboveAttractedThreshold()
	{
		return this.currentAttraction >= this.attractedThreshold;
	}

	public bool BelowUnAttractedThreshold()
	{
		return this.currentAttraction < this.unattractedThreshold;
	}

	public bool AboveHungryThreshold()
	{
		return this.currentHunger >= this.hungryThreshold;
	}

	public bool BelowNotHungryThreshold()
	{
		return this.currentHunger < this.satiatedThreshold;
	}

	public bool AboveSleepyThreshold()
	{
		return this.currentSleepiness >= this.tiredThreshold;
	}

	public bool BelowNotSleepyThreshold()
	{
		return this.currentSleepiness < this.awakeThreshold;
	}

	public bool CanJump()
	{
		if (!this.canJump)
		{
			return false;
		}
		float num;
		if (this.currentState == CrittersPawn.CreatureState.Running)
		{
			num = this.scaredJumpCooldown;
		}
		else
		{
			num = this.jumpCooldown;
		}
		float num2 = (PhotonNetwork.InRoom ? ((float)PhotonNetwork.Time) : Time.time);
		if (this.lastImpulseTime > (double)(num2 + this.jumpCooldown + this.jumpVariabilityTime))
		{
			this.lastImpulseTime = (double)(num2 + this.GetAdditiveJumpDelay());
		}
		return (double)num2 > this.lastImpulseTime + (double)num;
	}

	public void OnCollisionEnter(Collision collision)
	{
		this.canJump = true;
	}

	public void OnCollisionExit(Collision collision)
	{
		this.canJump = false;
	}

	public void SetVelocity(Vector3 linearVelocity)
	{
		this.rb.linearVelocity = linearVelocity;
	}

	public override int AddActorDataToList(ref List<object> objList)
	{
		base.AddActorDataToList(ref objList);
		objList.Add(Mathf.FloorToInt(this.currentFear));
		objList.Add(Mathf.FloorToInt(this.currentHunger));
		objList.Add(Mathf.FloorToInt(this.currentSleepiness));
		objList.Add(Mathf.FloorToInt(this.currentStruggle));
		objList.Add(this.currentState);
		objList.Add(this.actorIdTarget);
		objList.Add(this.lifeTimeStart);
		objList.Add(this.TemplateIndex);
		objList.Add(Mathf.FloorToInt(this.remainingStunnedTime));
		objList.Add(this.spawnStartTime);
		objList.Add(this.despawnStartTime);
		objList.AddRange(this.visuals.Appearance.WriteToRPCData());
		return this.TotalActorDataLength();
	}

	public override int TotalActorDataLength()
	{
		return base.BaseActorDataLength() + 11 + CritterAppearance.DataLength();
	}

	public override int UpdateFromRPC(object[] data, int startingIndex)
	{
		startingIndex += base.UpdateFromRPC(data, startingIndex);
		int num;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex], out num))
		{
			return this.TotalActorDataLength();
		}
		int num2;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 1], out num2))
		{
			return this.TotalActorDataLength();
		}
		int num3;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 2], out num3))
		{
			return this.TotalActorDataLength();
		}
		int num4;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 3], out num4))
		{
			return this.TotalActorDataLength();
		}
		int num5;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 4], out num5))
		{
			return this.TotalActorDataLength();
		}
		if (!Enum.IsDefined(typeof(CrittersPawn.CreatureState), (CrittersPawn.CreatureState)num5))
		{
			return this.TotalActorDataLength();
		}
		int num6;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 5], out num6))
		{
			return this.TotalActorDataLength();
		}
		double num7;
		if (!CrittersManager.ValidateDataType<double>(data[startingIndex + 6], out num7))
		{
			return this.TotalActorDataLength();
		}
		int num8;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 7], out num8))
		{
			return this.TotalActorDataLength();
		}
		int num9;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 8], out num9))
		{
			return this.TotalActorDataLength();
		}
		double num10;
		if (!CrittersManager.ValidateDataType<double>(data[startingIndex + 9], out num10))
		{
			return this.TotalActorDataLength();
		}
		double num11;
		if (!CrittersManager.ValidateDataType<double>(data[startingIndex + 10], out num11))
		{
			return this.TotalActorDataLength();
		}
		this.currentFear = (float)num;
		this.currentHunger = (float)num2;
		this.currentSleepiness = (float)num3;
		this.currentStruggle = (float)num4;
		this.SetState((CrittersPawn.CreatureState)num5);
		this.actorIdTarget = num6;
		this.lifeTimeStart = num7.GetFinite();
		this.TemplateIndex = num8;
		this.remainingStunnedTime = (float)num9;
		this.spawnStartTime = num10.GetFinite();
		this.despawnStartTime = num11.GetFinite();
		CrittersActor crittersActor = null;
		CrittersPawn.CreatureState creatureState = this.currentState;
		if (creatureState != CrittersPawn.CreatureState.Grabbed)
		{
			if (creatureState != CrittersPawn.CreatureState.Captured)
			{
				this.grabbedTarget = null;
				this.cageTarget = null;
			}
			else
			{
				if (CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor))
				{
					this.cageTarget = (CrittersCage)crittersActor;
					if (this.cageTarget != null)
					{
						base.transform.localScale = this.cageTarget.critterScale;
					}
				}
				this.grabbedTarget = null;
			}
		}
		else
		{
			if (CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor))
			{
				this.grabbedTarget = (CrittersGrabber)crittersActor;
			}
			this.cageTarget = null;
		}
		this.UpdateTemplate();
		this.visuals.SetAppearance(CritterAppearance.ReadFromRPCData(RuntimeHelpers.GetSubArray<object>(data, Range.StartAt(startingIndex + 11))));
		return this.TotalActorDataLength();
	}

	public override bool UpdateSpecificActor(PhotonStream stream)
	{
		int num;
		int num2;
		int num3;
		int num4;
		int num5;
		int num6;
		double num7;
		int num8;
		int num9;
		double num10;
		double num11;
		if (!(base.UpdateSpecificActor(stream) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num2) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num3) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num4) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num5) & Enum.IsDefined(typeof(CrittersPawn.CreatureState), (CrittersPawn.CreatureState)num5) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num6) & CrittersManager.ValidateDataType<double>(stream.ReceiveNext(), out num7) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num8) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num9) & CrittersManager.ValidateDataType<double>(stream.ReceiveNext(), out num10) & CrittersManager.ValidateDataType<double>(stream.ReceiveNext(), out num11)))
		{
			return false;
		}
		this.currentFear = (float)num;
		this.currentHunger = (float)num2;
		this.currentSleepiness = (float)num3;
		this.currentStruggle = (float)num4;
		this.SetState((CrittersPawn.CreatureState)num5);
		this.actorIdTarget = num6;
		this.lifeTimeStart = num7;
		this.TemplateIndex = num8;
		this.remainingStunnedTime = (float)num9;
		this.spawnStartTime = num10;
		this.despawnStartTime = num11;
		this.UpdateTemplate();
		CrittersActor crittersActor = null;
		CrittersPawn.CreatureState creatureState = this.currentState;
		if (creatureState != CrittersPawn.CreatureState.Grabbed)
		{
			if (creatureState != CrittersPawn.CreatureState.Captured)
			{
				this.grabbedTarget = null;
				this.cageTarget = null;
			}
			else
			{
				if (CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor))
				{
					this.cageTarget = (CrittersCage)crittersActor;
					if (this.cageTarget != null)
					{
						base.transform.localScale = this.cageTarget.critterScale;
					}
				}
				this.grabbedTarget = null;
			}
		}
		else
		{
			if (CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor))
			{
				this.grabbedTarget = (CrittersGrabber)crittersActor;
			}
			this.cageTarget = null;
		}
		return true;
	}

	public override void SendDataByCrittersActorType(PhotonStream stream)
	{
		base.SendDataByCrittersActorType(stream);
		stream.SendNext(Mathf.FloorToInt(this.currentFear));
		stream.SendNext(Mathf.FloorToInt(this.currentHunger));
		stream.SendNext(Mathf.FloorToInt(this.currentSleepiness));
		stream.SendNext(Mathf.FloorToInt(this.currentStruggle));
		stream.SendNext(this.currentState);
		stream.SendNext(this.actorIdTarget);
		stream.SendNext(this.lifeTimeStart);
		stream.SendNext(this.TemplateIndex);
		stream.SendNext(Mathf.FloorToInt(this.remainingStunnedTime));
		stream.SendNext(this.spawnStartTime);
		stream.SendNext(this.despawnStartTime);
	}

	public void SetConfiguration(CritterConfiguration getRandomConfiguration)
	{
		throw new NotImplementedException();
	}

	public void SetSpawnData(object[] spawnData)
	{
		this.visuals.SetAppearance(CritterAppearance.ReadFromRPCData(spawnData));
	}

	int IEyeScannable.scannableId
	{
		get
		{
			return base.gameObject.GetInstanceID();
		}
	}

	Vector3 IEyeScannable.Position
	{
		get
		{
			return this.bodyCollider.bounds.center;
		}
	}

	Bounds IEyeScannable.Bounds
	{
		get
		{
			return this.bodyCollider.bounds;
		}
	}

	IList<KeyValueStringPair> IEyeScannable.Entries
	{
		get
		{
			return this.BuildEyeScannerData();
		}
	}

	private IList<KeyValueStringPair> BuildEyeScannerData()
	{
		this.eyeScanData[0] = new KeyValueStringPair("Name", this.creatureConfiguration.critterName);
		this.eyeScanData[1] = new KeyValueStringPair("Type", this.creatureConfiguration.animalType.ToString());
		this.eyeScanData[2] = new KeyValueStringPair("Temperament", this.creatureConfiguration.behaviour.temperament);
		this.eyeScanData[3] = new KeyValueStringPair("Habitat", this.creatureConfiguration.biome.GetHabitatDescription());
		this.eyeScanData[4] = new KeyValueStringPair("Size", this.visuals.Appearance.size.ToString("0.00"));
		this.eyeScanData[5] = new KeyValueStringPair("State", this.GetCurrentStateName());
		return this.eyeScanData;
	}

	private string GetCurrentStateName()
	{
		string text;
		switch (this.currentState)
		{
		case CrittersPawn.CreatureState.Idle:
			text = "Adventuring";
			break;
		case CrittersPawn.CreatureState.Eating:
			text = "Eating";
			break;
		case CrittersPawn.CreatureState.AttractedTo:
			text = "Curious";
			break;
		case CrittersPawn.CreatureState.Running:
			text = "Scared";
			break;
		case CrittersPawn.CreatureState.Grabbed:
			text = ((this.struggleGainedPerSecond > 0f) ? "Struggling" : "Happy");
			break;
		case CrittersPawn.CreatureState.Sleeping:
			text = "Sleeping";
			break;
		case CrittersPawn.CreatureState.SeekingFood:
			text = "Foraging";
			break;
		case CrittersPawn.CreatureState.Captured:
			text = "Captured";
			break;
		case CrittersPawn.CreatureState.Stunned:
			text = "Stunned";
			break;
		default:
			text = "Contemplating Life";
			break;
		}
		string text2 = text;
		if (this.slowSpeedMod < 1f)
		{
			text2 = "Slowed, " + text2;
		}
		return text2;
	}

	public event Action OnDataChange;

	[NonSerialized]
	public CritterConfiguration creatureConfiguration;

	public Collider bodyCollider;

	[HideInInspector]
	[NonSerialized]
	public float maxJumpVel;

	[HideInInspector]
	[NonSerialized]
	public float jumpCooldown;

	[HideInInspector]
	[NonSerialized]
	public float scaredJumpCooldown;

	[HideInInspector]
	[NonSerialized]
	public float jumpVariabilityTime;

	[HideInInspector]
	[NonSerialized]
	public float visionConeAngle;

	[HideInInspector]
	[NonSerialized]
	public float sensoryRange;

	[HideInInspector]
	[NonSerialized]
	public float maxHunger;

	[HideInInspector]
	[NonSerialized]
	public float hungryThreshold;

	[HideInInspector]
	[NonSerialized]
	public float satiatedThreshold;

	[HideInInspector]
	[NonSerialized]
	public float hungerLostPerSecond;

	[HideInInspector]
	[NonSerialized]
	public float hungerGainedPerSecond;

	[HideInInspector]
	[NonSerialized]
	public float maxFear;

	[HideInInspector]
	[NonSerialized]
	public float scaredThreshold;

	[HideInInspector]
	[NonSerialized]
	public float calmThreshold;

	[HideInInspector]
	[NonSerialized]
	public float fearLostPerSecond;

	[NonSerialized]
	public float maxAttraction;

	[NonSerialized]
	public float attractedThreshold;

	[NonSerialized]
	public float unattractedThreshold;

	[NonSerialized]
	public float attractionLostPerSecond;

	[HideInInspector]
	[NonSerialized]
	public float maxSleepiness;

	[HideInInspector]
	[NonSerialized]
	public float tiredThreshold;

	[HideInInspector]
	[NonSerialized]
	public float awakeThreshold;

	[HideInInspector]
	[NonSerialized]
	public float sleepinessGainedPerSecond;

	[HideInInspector]
	[NonSerialized]
	public float sleepinessLostPerSecond;

	[HideInInspector]
	[NonSerialized]
	public float maxStruggle;

	[HideInInspector]
	[NonSerialized]
	public float escapeThreshold;

	[HideInInspector]
	[NonSerialized]
	public float catchableThreshold;

	[HideInInspector]
	[NonSerialized]
	public float struggleGainedPerSecond;

	[HideInInspector]
	[NonSerialized]
	public float struggleLostPerSecond;

	public List<crittersAttractorStruct> attractedToList;

	public List<crittersAttractorStruct> afraidOfList;

	public Dictionary<CrittersActor.CrittersActorType, float> afraidOfTypes;

	public Dictionary<CrittersActor.CrittersActorType, float> attractedToTypes;

	private Rigidbody rB;

	[NonSerialized]
	public CrittersPawn.CreatureState currentState;

	[NonSerialized]
	public float currentHunger;

	[NonSerialized]
	public float currentFear;

	[NonSerialized]
	public float currentAttraction;

	[NonSerialized]
	public float currentSleepiness;

	[NonSerialized]
	public float currentStruggle;

	public double lifeTime = 10.0;

	public double lifeTimeStart;

	private CrittersFood eatingTarget;

	private CrittersActor fearTarget;

	private CrittersActor attractionTarget;

	private Vector3 lastSeenFearPosition;

	private Vector3 lastSeenAttractionPosition;

	private CrittersGrabber grabbedTarget;

	private CrittersCage cageTarget;

	private int actorIdTarget;

	[FormerlySerializedAs("eatingRadiusMax")]
	public float eatingRadiusMaxSquared;

	private bool withinEatingRadius;

	public Transform animTarget;

	public MeshRenderer myRenderer;

	public float autoSeeFoodDistance;

	public Dictionary<int, CrittersActor> soundsHeard;

	public float fudge = 1.1f;

	public float obstacleSeeDistance = 0.25f;

	private RaycastHit[] raycastHits;

	private bool canJump;

	private bool wasSomethingInTheWay;

	public Transform hat;

	private int LastTemplateIndex = -1;

	private int TemplateIndex = -1;

	private double _nextDespawnCheck;

	private double _nextStuckCheck;

	public float killHeight = -500f;

	private float remainingStunnedTime;

	private float remainingSlowedTime;

	private float slowSpeedMod = 1f;

	[Header("Visuals")]
	public CritterVisuals visuals;

	[HideInInspector]
	public Dictionary<CrittersPawn.CreatureState, GameObject> StartStateFX = new Dictionary<CrittersPawn.CreatureState, GameObject>();

	[HideInInspector]
	public Dictionary<CrittersPawn.CreatureState, GameObject> OngoingStateFX = new Dictionary<CrittersPawn.CreatureState, GameObject>();

	[NonSerialized]
	public GameObject OnReleasedFX;

	private GameObject currentOngoingStateFX;

	[HideInInspector]
	public Dictionary<CrittersPawn.CreatureState, CrittersAnim> stateAnim = new Dictionary<CrittersPawn.CreatureState, CrittersAnim>();

	private CrittersAnim currentAnim;

	private float currentAnimTime;

	public AudioClip grabbedHaptics;

	public float grabbedHapticsStrength;

	public AnimationCurve spawnInHeighMovement;

	public AnimationCurve despawnInHeighMovement;

	private Vector3 spawningStartingPosition;

	private double spawnStartTime;

	private double despawnStartTime;

	private float _spawnAnimationDuration;

	private float _despawnAnimationDuration;

	private double _spawnAnimTime;

	private double _despawnAnimTime;

	public MeshRenderer debugStateIndicator;

	public Color debugColorIdle;

	public Color debugColorSeekingFood;

	public Color debugColorEating;

	public Color debugColorScared;

	public Color debugColorSleeping;

	public Color debugColorCaught;

	public Color debugColorCaged;

	public Color debugColorStunned;

	public Color debugColorAttracted;

	[NonSerialized]
	public int regionId;

	private KeyValueStringPair[] eyeScanData = new KeyValueStringPair[6];

	public enum CreatureState
	{
		Idle,
		Eating,
		AttractedTo,
		Running,
		Grabbed,
		Sleeping,
		SeekingFood,
		Captured,
		Stunned,
		WaitingToDespawn,
		Despawning,
		Spawning
	}

	internal struct CreatureUpdateData
	{
		internal CreatureUpdateData(CrittersPawn creature)
		{
			this.lastImpulseTime = creature.lastImpulseTime;
			this.state = creature.currentState;
		}

		internal bool SameData(CrittersPawn creature)
		{
			return this.lastImpulseTime == creature.lastImpulseTime && this.state == creature.currentState;
		}

		private double lastImpulseTime;

		private CrittersPawn.CreatureState state;
	}
}
