using System;
using System.Collections.Generic;
using System.IO;
using CjLib;
using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class GREnemyRanged : MonoBehaviour, IGameEntityComponent, IGameEntitySerialize, IGameHittable, IGameAgentComponent, IGameProjectileLauncher, IGameEntityDebugComponent
{
	private bool IsMoving()
	{
		return this.navAgent.velocity.sqrMagnitude > 0f;
	}

	private void SoftResetThrowableHead()
	{
		this.headRemoved = false;
		this.spitterHeadOnShoulders.SetActive(true);
		this.spitterHeadOnShouldersVFX.SetActive(false);
		this.spitterHeadInHand.SetActive(false);
		this.spitterHeadInHandLight.SetActive(false);
		this.spitterHeadInHandVFX.SetActive(false);
		this.headLightReset = true;
		this.spitterLightTurnOffTime = Time.timeAsDouble + this.spitterLightTurnOffDelay;
	}

	private void ForceResetThrowableHead()
	{
		this.headRemoved = false;
		this.headLightReset = false;
		this.spitterHeadOnShoulders.SetActive(true);
		this.spitterHeadOnShouldersLight.SetActive(false);
		this.spitterHeadOnShouldersVFX.SetActive(false);
		this.spitterHeadInHand.SetActive(false);
		this.spitterHeadInHandLight.SetActive(false);
		this.spitterHeadInHandVFX.SetActive(false);
	}

	private void ForceHeadToDeadState()
	{
		this.headRemoved = false;
		this.headLightReset = false;
		this.spitterHeadOnShoulders.SetActive(true);
		this.spitterHeadOnShouldersLight.SetActive(false);
		this.spitterHeadOnShouldersVFX.SetActive(false);
		this.spitterHeadInHand.SetActive(false);
		this.spitterHeadInHandLight.SetActive(false);
		this.spitterHeadInHandVFX.SetActive(false);
	}

	private void EnableVFXForShoulderHead()
	{
		this.headLightReset = false;
		this.spitterHeadOnShoulders.SetActive(true);
		this.spitterHeadOnShouldersLight.SetActive(true);
		this.spitterHeadOnShouldersVFX.SetActive(true);
		this.spitterHeadInHand.SetActive(false);
		this.spitterHeadInHandLight.SetActive(false);
		this.spitterHeadInHandVFX.SetActive(false);
	}

	private void EnableVFXForHeadInHand()
	{
		this.headLightReset = false;
		this.spitterHeadOnShoulders.SetActive(false);
		this.spitterHeadOnShouldersLight.SetActive(false);
		this.spitterHeadOnShouldersVFX.SetActive(false);
		this.spitterHeadInHand.SetActive(true);
		this.spitterHeadInHandLight.SetActive(true);
		this.spitterHeadInHandVFX.SetActive(true);
	}

	private void DisableHeadInHand()
	{
		this.headLightReset = false;
		this.spitterHeadInHand.SetActive(false);
	}

	private void DisableHeadOnShoulderAndHeadInHand()
	{
		this.headLightReset = false;
		this.headRemoved = false;
		this.spitterHeadOnShoulders.SetActive(false);
		this.spitterHeadOnShouldersLight.SetActive(false);
		this.spitterHeadOnShouldersVFX.SetActive(false);
		this.spitterHeadInHand.SetActive(false);
		this.spitterHeadInHandLight.SetActive(false);
		this.spitterHeadInHandVFX.SetActive(false);
	}

	private void Awake()
	{
		this.rigidBody = base.GetComponent<Rigidbody>();
		this.colliders = new List<Collider>(4);
		base.GetComponentsInChildren<Collider>(this.colliders);
		this.visibilityLayerMask = LayerMask.GetMask(new string[] { "Default" });
		if (this.armor != null)
		{
			this.armor.SetHp(0);
		}
		this.navAgent.updateRotation = false;
		this.agent.onBodyStateChanged += this.OnNetworkBodyStateChange;
		this.agent.onBehaviorStateChanged += this.OnNetworkBehaviorStateChange;
	}

	public void OnEntityInit()
	{
		this.abilityStagger.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, null);
		this.abilityInvestigate.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, null);
		this.abilityPatrol.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, null);
		this.abilityFlashed.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, null);
		this.abilityKeepDistance.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, null);
		this.abilityJump.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, null);
		this.Setup(this.entity.createData);
		if (this.entity && this.entity.manager && this.entity.manager.ghostReactorManager && this.entity.manager.ghostReactorManager.reactor)
		{
			foreach (GRBonusEntry grbonusEntry in this.entity.manager.ghostReactorManager.reactor.GetCurrLevelGenConfig().enemyGlobalBonuses)
			{
				this.attributes.AddBonus(grbonusEntry);
			}
		}
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
		this.DestroyProjectile();
	}

	public void Setup(long entityCreateData)
	{
		this.SetPatrolPath(entityCreateData);
		if (this.abilityPatrol.HasValidPatrolPath())
		{
			this.SetBehavior(GREnemyRanged.Behavior.Patrol, true);
		}
		else
		{
			this.SetBehavior(GREnemyRanged.Behavior.Idle, true);
		}
		if (this.attributes.CalculateFinalValueForAttribute(GRAttributeType.ArmorMax) > 0)
		{
			this.SetBodyState(GREnemyRanged.BodyState.Shell, true);
		}
		else
		{
			this.SetBodyState(GREnemyRanged.BodyState.Bones, true);
		}
		this.abilityDie.Setup(this.agent, this.anim, this.audioSource, base.transform, this.headTransform, null);
	}

	private void OnAgentJumpRequested(Vector3 start, Vector3 end, float heightScale, float speedScale)
	{
		this.abilityJump.SetupJump(start, end, heightScale, speedScale);
		this.SetBehavior(GREnemyRanged.Behavior.Jump, false);
	}

	public void OnNetworkBehaviorStateChange(byte newState)
	{
		if (newState < 0 || newState >= 11)
		{
			return;
		}
		this.SetBehavior((GREnemyRanged.Behavior)newState, false);
	}

	public void OnNetworkBodyStateChange(byte newState)
	{
		if (newState < 0 || newState >= 3)
		{
			return;
		}
		this.SetBodyState((GREnemyRanged.BodyState)newState, false);
	}

	public void SetPatrolPath(long entityCreateData)
	{
		this.abilityPatrol.SetPatrolPath(GhostReactorManager.Get(this.entity).reactor.GetPatrolPath(entityCreateData));
	}

	public void SetHP(int hp)
	{
		this.hp = hp;
	}

	public bool TrySetBehavior(GREnemyRanged.Behavior newBehavior)
	{
		if (this.currBehavior == GREnemyRanged.Behavior.Jump && newBehavior == GREnemyRanged.Behavior.Stagger)
		{
			return false;
		}
		this.SetBehavior(newBehavior, false);
		return true;
	}

	public void SetBehavior(GREnemyRanged.Behavior newBehavior, bool force = false)
	{
		if (this.currBehavior == newBehavior && !force)
		{
			return;
		}
		switch (this.currBehavior)
		{
		case GREnemyRanged.Behavior.Patrol:
			this.abilityPatrol.Stop();
			break;
		case GREnemyRanged.Behavior.Stagger:
			this.abilityStagger.Stop();
			break;
		case GREnemyRanged.Behavior.Dying:
			this.abilityDie.Stop();
			break;
		case GREnemyRanged.Behavior.SeekRangedAttackPosition:
			if (newBehavior != GREnemyRanged.Behavior.RangedAttack)
			{
				this.SoftResetThrowableHead();
			}
			break;
		case GREnemyRanged.Behavior.RangedAttack:
			if (newBehavior != GREnemyRanged.Behavior.RangedAttackCooldown)
			{
				this.ForceResetThrowableHead();
			}
			break;
		case GREnemyRanged.Behavior.RangedAttackCooldown:
			this.ForceResetThrowableHead();
			this.abilityKeepDistance.Stop();
			break;
		case GREnemyRanged.Behavior.Flashed:
			this.abilityFlashed.Stop();
			break;
		case GREnemyRanged.Behavior.Investigate:
			this.abilityInvestigate.Stop();
			break;
		case GREnemyRanged.Behavior.Jump:
			this.abilityJump.Stop();
			break;
		}
		this.currBehavior = newBehavior;
		switch (this.currBehavior)
		{
		case GREnemyRanged.Behavior.Idle:
			this.targetPlayer = null;
			this.PlayAnim("GREnemyRangedIdleSearch", 0.1f, 1f);
			break;
		case GREnemyRanged.Behavior.Patrol:
			this.targetPlayer = null;
			this.abilityPatrol.Start();
			break;
		case GREnemyRanged.Behavior.Search:
			this.targetPlayer = null;
			this.PlayAnim("GREnemyRangedWalk", 0.1f, 1f);
			this.navAgent.speed = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.PatrolSpeed);
			this.lastMoving = false;
			break;
		case GREnemyRanged.Behavior.Stagger:
			this.abilityStagger.Start();
			break;
		case GREnemyRanged.Behavior.Dying:
			this.abilityDie.Start();
			if (this.entity.IsAuthority())
			{
				this.entity.manager.RequestCreateItem(this.corePrefab.gameObject.name.GetStaticHash(), this.coreMarker.position, this.coreMarker.rotation, 0L);
			}
			break;
		case GREnemyRanged.Behavior.SeekRangedAttackPosition:
			this.PlayAnim("GREnemyRangedWalk", 0.1f, 1f);
			this.navAgent.speed = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.ChaseSpeed);
			this.EnableVFXForShoulderHead();
			this.chaseAbilitySound.Play(this.audioSecondarySource);
			break;
		case GREnemyRanged.Behavior.RangedAttack:
			this.PlayAnim("GREnemyRangedAttack01", 0.1f, 1f);
			this.navAgent.speed = 0f;
			this.navAgent.velocity = Vector3.zero;
			this.headRemovaltime = PhotonNetwork.Time + (double)this.headRemovalFrame;
			this.attackAbilitySound.Play(this.audioSource);
			break;
		case GREnemyRanged.Behavior.RangedAttackCooldown:
			this.lastMoving = true;
			this.abilityKeepDistance.SetTargetPlayer(this.targetPlayer);
			this.abilityKeepDistance.Start();
			break;
		case GREnemyRanged.Behavior.Flashed:
			this.abilityFlashed.Start();
			break;
		case GREnemyRanged.Behavior.Investigate:
			this.abilityInvestigate.Start();
			break;
		case GREnemyRanged.Behavior.Jump:
			this.abilityJump.Start();
			break;
		}
		this.RefreshBody();
		if (this.entity.IsAuthority())
		{
			this.agent.RequestBehaviorChange((byte)this.currBehavior);
		}
	}

	private void PlayAnim(string animName, float blendTime, float speed)
	{
		if (this.anim != null)
		{
			this.anim[animName].speed = speed;
			this.anim.CrossFade(animName, blendTime);
		}
	}

	public void SetBodyState(GREnemyRanged.BodyState newBodyState, bool force = false)
	{
		if (this.currBodyState == newBodyState && !force)
		{
			return;
		}
		switch (this.currBodyState)
		{
		case GREnemyRanged.BodyState.Destroyed:
		{
			this.ForceResetThrowableHead();
			for (int i = 0; i < this.colliders.Count; i++)
			{
				this.colliders[i].enabled = true;
			}
			break;
		}
		case GREnemyRanged.BodyState.Bones:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax);
			break;
		case GREnemyRanged.BodyState.Shell:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.ArmorMax);
			break;
		}
		this.currBodyState = newBodyState;
		switch (this.currBodyState)
		{
		case GREnemyRanged.BodyState.Destroyed:
			this.DisableHeadOnShoulderAndHeadInHand();
			GhostReactorManager.Get(this.entity).ReportEnemyDeath();
			break;
		case GREnemyRanged.BodyState.Bones:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.HPMax);
			break;
		case GREnemyRanged.BodyState.Shell:
			this.hp = this.attributes.CalculateFinalValueForAttribute(GRAttributeType.ArmorMax);
			break;
		}
		this.RefreshBody();
		if (this.entity.IsAuthority())
		{
			this.agent.RequestStateChange((byte)newBodyState);
		}
	}

	private void RefreshBody()
	{
		switch (this.currBodyState)
		{
		case GREnemyRanged.BodyState.Destroyed:
			this.armor.SetHp(0);
			GREnemy.HideRenderers(this.bones, true);
			GREnemy.HideRenderers(this.always, true);
			this.DisableHeadOnShoulderAndHeadInHand();
			return;
		case GREnemyRanged.BodyState.Bones:
			this.armor.SetHp(0);
			GREnemy.HideRenderers(this.bones, false);
			GREnemy.HideRenderers(this.always, false);
			return;
		case GREnemyRanged.BodyState.Shell:
			this.armor.SetHp(this.hp);
			GREnemy.HideRenderers(this.bones, true);
			GREnemy.HideRenderers(this.always, false);
			return;
		default:
			return;
		}
	}

	private void Update()
	{
		if (this.entity.IsAuthority())
		{
			this.OnUpdateAuthority(Time.deltaTime);
		}
		else
		{
			this.OnUpdateRemote(Time.deltaTime);
		}
		this.UpdateShared();
	}

	public void OnEntityThink(float dt)
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		if (!GhostReactorManager.AggroDisabled)
		{
			GREnemyRanged.Behavior behavior = this.currBehavior;
			if (behavior > GREnemyRanged.Behavior.Search)
			{
				if (behavior == GREnemyRanged.Behavior.RangedAttackCooldown)
				{
					this.abilityKeepDistance.Think(dt);
					this.UpdateTarget();
					return;
				}
				if (behavior != GREnemyRanged.Behavior.Investigate)
				{
					return;
				}
			}
			this.UpdateTarget();
		}
	}

	private void UpdateTarget()
	{
		float num = float.MaxValue;
		this.bestTargetPlayer = null;
		this.bestTargetNetPlayer = null;
		GREnemyRanged.tempRigs.Clear();
		GREnemyRanged.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GREnemyRanged.tempRigs);
		Vector3 position = base.transform.position;
		Vector3 vector = base.transform.rotation * Vector3.forward;
		float num2 = this.sightDist * this.sightDist;
		float num3 = Mathf.Cos(this.sightFOV * 0.017453292f);
		for (int i = 0; i < GREnemyRanged.tempRigs.Count; i++)
		{
			VRRig vrrig = GREnemyRanged.tempRigs[i];
			GRPlayer component = vrrig.GetComponent<GRPlayer>();
			if (component.State != GRPlayer.GRPlayerState.Ghost)
			{
				Vector3 position2 = vrrig.transform.position;
				Vector3 vector2 = position2 - position;
				float sqrMagnitude = vector2.sqrMagnitude;
				if (sqrMagnitude <= num2)
				{
					float num4 = 0f;
					if (sqrMagnitude > 0f)
					{
						num4 = Mathf.Sqrt(sqrMagnitude);
						if (Vector3.Dot(vector2 / num4, vector) < num3)
						{
							goto IL_016F;
						}
					}
					if (num4 < num && Physics.RaycastNonAlloc(new Ray(this.headTransform.position, position2 - this.headTransform.position), GREnemyChaser.visibilityHits, num4, this.visibilityLayerMask.value, QueryTriggerInteraction.Ignore) < 1)
					{
						num = num4;
						this.bestTargetPlayer = component;
						this.bestTargetNetPlayer = vrrig.OwningNetPlayer;
						this.lastSeenTargetTime = Time.timeAsDouble;
						this.lastSeenTargetPosition = position2;
					}
				}
			}
			IL_016F:;
		}
	}

	private void ChooseNewBehavior()
	{
		if (this.bestTargetPlayer != null && Time.timeAsDouble - this.lastSeenTargetTime < (double)this.sightLostFollowStopTime)
		{
			this.targetPlayer = this.bestTargetNetPlayer;
			this.lastSeenTargetTime = Time.timeAsDouble;
			this.investigateLocation = null;
			this.SetBehavior(GREnemyRanged.Behavior.SeekRangedAttackPosition, false);
			return;
		}
		if (Time.timeAsDouble - this.lastSeenTargetTime < (double)this.searchTime)
		{
			this.SetBehavior(GREnemyRanged.Behavior.Search, false);
			return;
		}
		this.investigateLocation = AbilityHelperFunctions.GetLocationToInvestigate(base.transform.position, this.hearingRadius, this.investigateLocation);
		if (this.investigateLocation != null)
		{
			this.abilityInvestigate.SetTargetPos(this.investigateLocation.Value);
			this.SetBehavior(GREnemyRanged.Behavior.Investigate, false);
			return;
		}
		if (this.abilityPatrol.HasValidPatrolPath())
		{
			this.SetBehavior(GREnemyRanged.Behavior.Patrol, false);
			return;
		}
		this.SetBehavior(GREnemyRanged.Behavior.Idle, false);
	}

	public void OnUpdateAuthority(float dt)
	{
		switch (this.currBehavior)
		{
		case GREnemyRanged.Behavior.Idle:
			this.ChooseNewBehavior();
			break;
		case GREnemyRanged.Behavior.Patrol:
			this.abilityPatrol.Update(dt);
			this.ChooseNewBehavior();
			break;
		case GREnemyRanged.Behavior.Search:
			this.UpdateSearch();
			this.ChooseNewBehavior();
			break;
		case GREnemyRanged.Behavior.Stagger:
			this.abilityStagger.Update(dt);
			if (this.abilityStagger.IsDone())
			{
				if (this.targetPlayer == null)
				{
					this.SetBehavior(GREnemyRanged.Behavior.Search, false);
				}
				else
				{
					this.SetBehavior(GREnemyRanged.Behavior.SeekRangedAttackPosition, false);
				}
			}
			break;
		case GREnemyRanged.Behavior.Dying:
			this.abilityDie.Update(dt);
			break;
		case GREnemyRanged.Behavior.SeekRangedAttackPosition:
			if (this.targetPlayer != null)
			{
				GRPlayer grplayer = GRPlayer.Get(this.targetPlayer.ActorNumber);
				if (grplayer != null && grplayer.State == GRPlayer.GRPlayerState.Alive)
				{
					Vector3 position = grplayer.transform.position;
					Vector3 position2 = base.transform.position;
					float magnitude = (position - position2).magnitude;
					if (magnitude > this.loseSightDist)
					{
						this.ChooseNewBehavior();
					}
					else
					{
						float num = Vector3.Distance(position, this.headTransform.position);
						bool flag = false;
						if (num < this.sightDist)
						{
							flag = Physics.RaycastNonAlloc(new Ray(this.headTransform.position, position - this.headTransform.position), GREnemyChaser.visibilityHits, num, this.visibilityLayerMask.value, QueryTriggerInteraction.Ignore) < 1;
						}
						if (flag)
						{
							this.lastSeenTargetPosition = position;
							this.lastSeenTargetTime = Time.timeAsDouble;
						}
						if (Time.timeAsDouble - this.lastSeenTargetTime < (double)this.sightLostFollowStopTime)
						{
							this.searchPosition = position;
							this.agent.RequestDestination(this.lastSeenTargetPosition);
							if (flag)
							{
								this.rangedTargetPosition = position;
								Vector3 vector = Vector3.up * 0.4f;
								this.rangedTargetPosition += vector;
								if (magnitude < this.rangedAttackDistMax)
								{
									this.behaviorEndTime = Time.timeAsDouble + (double)this.rangedAttackChargeTime;
									this.SetBehavior(GREnemyRanged.Behavior.RangedAttack, false);
									GhostReactorManager.Get(this.entity).RequestFireProjectile(this.entity.id, this.rangedProjectileFirePoint.position, this.rangedTargetPosition, PhotonNetwork.Time + (double)this.rangedAttackChargeTime);
								}
							}
						}
						else
						{
							this.ChooseNewBehavior();
						}
					}
				}
			}
			break;
		case GREnemyRanged.Behavior.RangedAttack:
			if (Time.timeAsDouble > this.behaviorEndTime)
			{
				if (this.targetPlayer != null)
				{
					GRPlayer grplayer2 = GRPlayer.Get(this.targetPlayer.ActorNumber);
					if (grplayer2 != null && grplayer2.State == GRPlayer.GRPlayerState.Alive)
					{
						this.rangedTargetPosition = grplayer2.transform.position;
					}
				}
				this.SetBehavior(GREnemyRanged.Behavior.RangedAttackCooldown, false);
				this.behaviorEndTime = Time.timeAsDouble + (double)this.rangedAttackRecoverTime;
			}
			break;
		case GREnemyRanged.Behavior.RangedAttackCooldown:
			if (Time.timeAsDouble > this.behaviorEndTime)
			{
				this.SetBehavior(GREnemyRanged.Behavior.SeekRangedAttackPosition, false);
				this.behaviorEndTime = Time.timeAsDouble;
			}
			else
			{
				this.abilityKeepDistance.Update(dt);
			}
			break;
		case GREnemyRanged.Behavior.Flashed:
			this.abilityFlashed.Update(dt);
			if (this.abilityFlashed.IsDone())
			{
				if (this.targetPlayer == null)
				{
					this.SetBehavior(GREnemyRanged.Behavior.Search, false);
				}
				else
				{
					this.SetBehavior(GREnemyRanged.Behavior.SeekRangedAttackPosition, false);
				}
			}
			break;
		case GREnemyRanged.Behavior.Investigate:
			this.abilityInvestigate.Update(dt);
			if (GhostReactorManager.noiseDebugEnabled)
			{
				DebugUtil.DrawLine(base.transform.position, this.abilityInvestigate.GetTargetPos(), Color.green, true);
			}
			this.ChooseNewBehavior();
			break;
		case GREnemyRanged.Behavior.Jump:
			this.abilityJump.Update(dt);
			if (this.abilityJump.IsDone())
			{
				this.ChooseNewBehavior();
			}
			break;
		}
		GameAgent.UpdateFacing(base.transform, this.navAgent, this.targetPlayer, this.turnSpeed);
	}

	public void OnUpdateRemote(float dt)
	{
		switch (this.currBehavior)
		{
		case GREnemyRanged.Behavior.Patrol:
			this.abilityPatrol.UpdateRemote(dt);
			return;
		case GREnemyRanged.Behavior.Search:
		case GREnemyRanged.Behavior.SeekRangedAttackPosition:
		case GREnemyRanged.Behavior.RangedAttack:
			break;
		case GREnemyRanged.Behavior.Stagger:
			this.abilityStagger.UpdateRemote(dt);
			return;
		case GREnemyRanged.Behavior.Dying:
			this.abilityDie.UpdateRemote(dt);
			return;
		case GREnemyRanged.Behavior.RangedAttackCooldown:
			this.abilityKeepDistance.Update(dt);
			return;
		case GREnemyRanged.Behavior.Flashed:
			this.abilityFlashed.UpdateRemote(dt);
			return;
		case GREnemyRanged.Behavior.Investigate:
			this.abilityInvestigate.UpdateRemote(dt);
			if (GhostReactorManager.noiseDebugEnabled)
			{
				DebugUtil.DrawLine(base.transform.position, this.abilityInvestigate.GetTargetPos(), Color.green, true);
				return;
			}
			break;
		case GREnemyRanged.Behavior.Jump:
			this.abilityJump.UpdateRemote(dt);
			break;
		default:
			return;
		}
	}

	public void UpdateShared()
	{
		if (this.rangedAttackQueued)
		{
			if (!this.headRemoved && this.currBehavior == GREnemyRanged.Behavior.RangedAttack && PhotonNetwork.Time >= this.headRemovaltime)
			{
				this.headRemoved = true;
				this.EnableVFXForHeadInHand();
			}
			if (PhotonNetwork.Time > this.queuedFiringTime)
			{
				this.rangedAttackQueued = false;
				this.FireRangedAttack(this.queuedFiringPosition, this.queuedTargetPosition);
			}
		}
		if (this.headLightReset && Time.timeAsDouble > this.spitterLightTurnOffTime)
		{
			this.spitterHeadOnShouldersLight.SetActive(false);
			this.headLightReset = false;
		}
	}

	public void UpdateSearch()
	{
		Vector3 vector = this.searchPosition - base.transform.position;
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
		if (vector2.sqrMagnitude < 0.15f)
		{
			Vector3 vector3 = this.lastSeenTargetPosition - this.searchPosition;
			vector3.y = 0f;
			this.searchPosition = this.lastSeenTargetPosition + vector3;
		}
		if (this.IsMoving())
		{
			if (!this.lastMoving)
			{
				this.PlayAnim("GREnemyRangedWalk", 0.1f, 1f);
				this.lastMoving = true;
			}
		}
		else if (this.lastMoving)
		{
			this.PlayAnim("GREnemyRangedWalk", 0.1f, 1f);
			this.lastMoving = false;
		}
		this.agent.RequestDestination(this.searchPosition);
		if (Time.timeAsDouble - this.lastSeenTargetTime > (double)this.searchTime)
		{
			this.ChooseNewBehavior();
		}
	}

	public void OnHitByClub(GRTool tool, GameHitData hit)
	{
		if (this.currBodyState != GREnemyRanged.BodyState.Bones)
		{
			if (this.currBodyState == GREnemyRanged.BodyState.Shell && this.armor != null)
			{
				this.armor.PlayBlockFx(hit.hitEntityPosition);
			}
			return;
		}
		this.hp -= hit.hitAmount;
		this.audioSource.PlayOneShot(this.damagedSound, this.damagedSoundVolume);
		if (this.fxDamaged != null)
		{
			this.fxDamaged.SetActive(false);
			this.fxDamaged.SetActive(true);
		}
		if (this.hp <= 0)
		{
			this.abilityDie.SetInstigatingPlayerIndex(this.entity.GetLastHeldByPlayerForEntityID(hit.hitByEntityId));
			this.SetBodyState(GREnemyRanged.BodyState.Destroyed, false);
			this.SetBehavior(GREnemyRanged.Behavior.Dying, false);
			return;
		}
		this.lastSeenTargetPosition = tool.transform.position;
		this.lastSeenTargetTime = Time.timeAsDouble;
		Vector3 vector = this.lastSeenTargetPosition - base.transform.position;
		vector.y = 0f;
		this.searchPosition = this.lastSeenTargetPosition + vector.normalized * 1.5f;
		this.abilityStagger.SetStaggerVelocity(hit.hitImpulse);
		this.TrySetBehavior(GREnemyRanged.Behavior.Stagger);
	}

	public void OnHitByFlash(GRTool tool, GameHitData hit)
	{
		if (this.currBodyState == GREnemyRanged.BodyState.Shell)
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
				this.SetBodyState(GREnemyRanged.BodyState.Bones, false);
				if (tool.gameEntity.IsHeldByLocalPlayer())
				{
					PlayerGameEvents.MiscEvent("GRArmorBreak_" + base.name, 1);
				}
				if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.FlashDamage3))
				{
					this.armor.FragmentArmor();
				}
			}
			else if (tool != null)
			{
				if (this.armor != null)
				{
					this.armor.PlayHitFx(this.armor.transform.position);
				}
				this.lastSeenTargetPosition = tool.transform.position;
				this.lastSeenTargetTime = Time.timeAsDouble;
				Vector3 vector = this.lastSeenTargetPosition - base.transform.position;
				vector.y = 0f;
				this.searchPosition = this.lastSeenTargetPosition + vector.normalized * 1.5f;
				this.SetBehavior(GREnemyRanged.Behavior.Search, false);
				this.RefreshBody();
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
		this.SetBehavior(GREnemyRanged.Behavior.Flashed, false);
	}

	public void OnHitByShield(GRTool tool, GameHitData hit)
	{
		this.OnHitByClub(tool, hit);
	}

	public void OnGameEntitySerialize(BinaryWriter writer)
	{
		byte b = (byte)this.currBehavior;
		byte b2 = (byte)this.currBodyState;
		byte b3 = (byte)this.abilityPatrol.nextPatrolNode;
		int num = ((this.targetPlayer == null) ? (-1) : this.targetPlayer.ActorNumber);
		writer.Write(b);
		writer.Write(b2);
		writer.Write(this.hp);
		writer.Write(b3);
		writer.Write(num);
	}

	public void OnGameEntityDeserialize(BinaryReader reader)
	{
		GREnemyRanged.Behavior behavior = (GREnemyRanged.Behavior)reader.ReadByte();
		GREnemyRanged.BodyState bodyState = (GREnemyRanged.BodyState)reader.ReadByte();
		int num = reader.ReadInt32();
		byte b = reader.ReadByte();
		int num2 = reader.ReadInt32();
		this.SetPatrolPath((long)((int)this.entity.createData));
		this.abilityPatrol.SetNextPatrolNode((int)b);
		this.SetHP(num);
		this.SetBehavior(behavior, true);
		this.SetBodyState(bodyState, true);
		this.targetPlayer = NetworkSystem.Instance.GetPlayer(num2);
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

	public void RequestRangedAttack(Vector3 firingPosition, Vector3 targetPosition, double fireTime)
	{
		this.rangedAttackQueued = true;
		this.queuedFiringTime = fireTime;
		this.queuedFiringPosition = firingPosition;
		this.queuedTargetPosition = targetPosition;
	}

	private void DestroyProjectile()
	{
		if (this.entity.IsAuthority() && this.rangedProjectileInstance != null)
		{
			GameEntity component = this.rangedProjectileInstance.GetComponent<GameEntity>();
			if (component != null)
			{
				component.manager.RequestDestroyItem(component.id);
			}
		}
	}

	private void FireRangedAttack(Vector3 launchPosition, Vector3 targetPosition)
	{
		if (!this.entity.IsAuthority())
		{
			return;
		}
		this.DisableHeadInHand();
		this.DestroyProjectile();
		Vector3 vector;
		if (GREnemyRanged.CalculateLaunchDirection(launchPosition, targetPosition, this.projectileSpeed, out vector))
		{
			this.entity.manager.RequestCreateItem(this.rangedProjectilePrefab.name.GetStaticHash(), launchPosition, Quaternion.LookRotation(vector, Vector3.up), (long)this.entity.GetNetId());
		}
	}

	public static bool CalculateLaunchDirection(Vector3 startPos, Vector3 targetPos, float speed, out Vector3 direction)
	{
		direction = Vector3.zero;
		Vector3 vector = targetPos - startPos;
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
		float magnitude = vector2.magnitude;
		Vector3 normalized = vector2.normalized;
		float y = vector.y;
		float num = 9.8f;
		float num2 = speed * speed;
		float num3 = num2 * num2 - num * (num * magnitude * magnitude + 2f * y * num2);
		if (num3 < 0f)
		{
			return false;
		}
		int num4 = 0;
		float num5 = Mathf.Sqrt(num3);
		float num6 = (num2 + num5) / (num * magnitude);
		float num7 = (num2 - num5) / (num * magnitude);
		float num8 = num2 / (num6 * num6 + 1f);
		float num9 = num2 / (num7 * num7 + 1f);
		float num10 = ((num4 != 0) ? Mathf.Min(num8, num9) : Mathf.Max(num8, num9));
		float num11 = ((num4 != 0) ? ((num8 < num9) ? Mathf.Sign(num6) : Mathf.Sign(num7)) : ((num8 > num9) ? Mathf.Sign(num6) : Mathf.Sign(num7)));
		float num12 = Mathf.Sqrt(num10);
		float num13 = Mathf.Sqrt(Mathf.Abs(num2 - num10));
		direction = (normalized * num12 + new Vector3(0f, num13 * num11, 0f)).normalized;
		return true;
	}

	public void OnProjectileInit(GRRangedEnemyProjectile projectile)
	{
		this.rangedProjectileInstance = projectile.gameObject;
	}

	public void OnProjectileHit(GRRangedEnemyProjectile projectile, Collision collision)
	{
	}

	public void GetDebugTextLines(out List<string> strings)
	{
		strings = new List<string>();
		strings.Add(string.Format("State: <color=\"yellow\">{0}<color=\"white\"> HP: <color=\"yellow\">{1}<color=\"white\">", this.currBehavior.ToString(), this.hp));
		strings.Add(string.Format("speed: <color=\"yellow\">{0}<color=\"white\"> patrol node:<color=\"yellow\">{1}/{2}<color=\"white\">", this.navAgent.speed, this.abilityPatrol.nextPatrolNode, (this.abilityPatrol.GetPatrolPath() != null) ? this.abilityPatrol.GetPatrolPath().patrolNodes.Count : 0));
		if (this.targetPlayer != null)
		{
			GRPlayer grplayer = GRPlayer.Get(this.targetPlayer.ActorNumber);
			if (grplayer != null)
			{
				float magnitude = (grplayer.transform.position - base.transform.position).magnitude;
				strings.Add(string.Format("TargetDis: <color=\"yellow\">{0}<color=\"white\"> ", magnitude));
			}
		}
	}

	public GameEntity entity;

	public GameAgent agent;

	public GRArmorEnemy armor;

	public GameHittable hittable;

	public GRAttributes attributes;

	public Animation anim;

	public GRAbilityStagger abilityStagger;

	public GRAbilityDie abilityDie;

	public GRAbilityMoveToTarget abilityInvestigate;

	public GRAbilityPatrol abilityPatrol;

	public GRAbilityFlashed abilityFlashed;

	public GRAbilityKeepDistance abilityKeepDistance;

	public GRAbilityJump abilityJump;

	public List<Renderer> bones;

	public List<Renderer> always;

	public Transform coreMarker;

	public GRCollectible corePrefab;

	public Transform headTransform;

	public float sightDist;

	public float loseSightDist;

	public float sightFOV;

	public float sightLostFollowStopTime = 0.5f;

	public float searchTime = 5f;

	public float hearingRadius = 5f;

	public float turnSpeed = 540f;

	public Color chaseColor = Color.red;

	public AbilitySound attackAbilitySound;

	public AbilitySound chaseAbilitySound;

	public float rangedAttackDistMin = 6f;

	public float rangedAttackDistMax = 8f;

	public float rangedAttackChargeTime = 0.5f;

	public float rangedAttackRecoverTime = 2f;

	public float projectileSpeed = 5f;

	public float projectileHitRadius = 1f;

	public GameObject rangedProjectilePrefab;

	public Transform rangedProjectileFirePoint;

	[ReadOnly]
	[SerializeField]
	private GRPatrolPath patrolPath;

	public NavMeshAgent navAgent;

	public AudioSource audioSource;

	public AudioSource audioSecondarySource;

	public AudioClip damagedSound;

	public float damagedSoundVolume;

	public GameObject fxDamaged;

	public bool lastMoving;

	private Vector3? investigateLocation;

	public bool debugLog;

	public GameObject spitterHeadOnShoulders;

	public GameObject spitterHeadOnShouldersLight;

	public GameObject spitterHeadOnShouldersVFX;

	public GameObject spitterHeadInHand;

	public GameObject spitterHeadInHandLight;

	public GameObject spitterHeadInHandVFX;

	public double spitterLightTurnOffDelay = 0.75;

	private bool headLightReset;

	private double spitterLightTurnOffTime;

	[FormerlySerializedAs("headRemovalInterval")]
	public float headRemovalFrame = 0.23333333f;

	private double headRemovaltime;

	private bool headRemoved;

	private Transform target;

	[ReadOnly]
	public int hp;

	[ReadOnly]
	public GREnemyRanged.Behavior currBehavior;

	[ReadOnly]
	public double behaviorEndTime;

	[ReadOnly]
	public GREnemyRanged.BodyState currBodyState;

	[ReadOnly]
	public int nextPatrolNode;

	[ReadOnly]
	public NetPlayer targetPlayer;

	[ReadOnly]
	public Vector3 lastSeenTargetPosition;

	[ReadOnly]
	public double lastSeenTargetTime;

	[ReadOnly]
	public Vector3 searchPosition;

	[ReadOnly]
	public Vector3 rangedFiringPosition;

	[ReadOnly]
	public Vector3 rangedTargetPosition;

	[ReadOnly]
	private GRPlayer bestTargetPlayer;

	[ReadOnly]
	private NetPlayer bestTargetNetPlayer;

	private bool rangedAttackQueued;

	private double queuedFiringTime;

	private Vector3 queuedFiringPosition;

	private Vector3 queuedTargetPosition;

	private GameObject rangedProjectileInstance;

	private bool projectileHasImpacted;

	private double projectileImpactTime;

	private Rigidbody rigidBody;

	private List<Collider> colliders;

	private LayerMask visibilityLayerMask;

	private Color defaultColor;

	private float lastHitPlayerTime;

	private float minTimeBetweenHits = 0.5f;

	private static List<VRRig> tempRigs = new List<VRRig>(16);

	public enum Behavior
	{
		Idle,
		Patrol,
		Search,
		Stagger,
		Dying,
		SeekRangedAttackPosition,
		RangedAttack,
		RangedAttackCooldown,
		Flashed,
		Investigate,
		Jump,
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
