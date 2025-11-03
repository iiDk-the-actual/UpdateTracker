using System;
using GorillaExtensions;
using GorillaGameModes;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class CustomMapsAttackBehaviour : CustomMapsBehaviourBase
{
	public CustomMapsAttackBehaviour(CustomMapsAIBehaviourController AIController, AIAgent agentSettings)
	{
		this.attackType = agentSettings.attackType;
		this.attackDist = agentSettings.attackDist;
		this.attackDistSq = this.attackDist * this.attackDist;
		this.stopMovingToAttack = agentSettings.stopMovingToAttack;
		this.useColliders = agentSettings.useColliders;
		this.damageDelayAfterPlayingAnimation = agentSettings.damageDelayAfterPlayingAnim;
		this.damageAmount = agentSettings.damageAmount;
		this.attackAnimName = agentSettings.attackAnimName;
		this.sightOffset = agentSettings.sightOffset;
		this.sightFOV = agentSettings.sightFOV;
		this.sightMinDot = Mathf.Cos(this.sightFOV / 2f * 0.017453292f);
		this.controller = AIController;
		this.animBlendTime = agentSettings.animBlendTime;
		this.turnSpeed = agentSettings.turnSpeed * 10f;
		this.timeBetweenAttacks = agentSettings.timeBetweenAttacks;
		this.controller.attributes.AddAttribute(GRAttributeType.PlayerDamage, this.damageAmount);
		this.state = CustomMapsAttackBehaviour.State.Idle;
	}

	public override bool CanExecute()
	{
		return !this.controller.IsNull() && !this.controller.TargetPlayer.IsNull() && this.IsTargetInAttackRange(null) && this.IsTargetVisible();
	}

	private bool IsTargetVisible()
	{
		Vector3 vector = this.controller.transform.position + this.controller.transform.TransformVector(this.sightOffset);
		return this.controller.IsTargetVisible(vector, this.controller.TargetPlayer, this.attackDist);
	}

	private bool IsTargetInAttackRange(GRPlayer target = null)
	{
		if (target.IsNull() && this.controller.TargetPlayer.IsNull())
		{
			return false;
		}
		if (target.IsNotNull())
		{
			Vector3 vector;
			return this.controller.IsTargetInRange(this.controller.transform.position, target, this.attackDistSq, out vector);
		}
		Vector3 vector2;
		return this.controller.IsTargetInRange(this.controller.transform.position, this.controller.TargetPlayer, this.attackDistSq, out vector2);
	}

	public override bool CanContinueExecuting()
	{
		if (this.state != CustomMapsAttackBehaviour.State.Idle && this.controller.IsAnimationPlaying(this.attackAnimName))
		{
			return true;
		}
		if (this.controller.IsNull() || this.controller.TargetPlayer.IsNull())
		{
			return false;
		}
		if (!this.controller.IsTargetable(this.controller.TargetPlayer))
		{
			this.controller.ClearTarget();
			return false;
		}
		return this.CanExecute();
	}

	public override void Execute()
	{
		if (this.controller.IsNull())
		{
			return;
		}
		if (this.stopMovingToAttack)
		{
			this.controller.StopMoving();
		}
		this.FaceTarget();
		this.controller.agent.RequestBehaviorChange(2);
	}

	public override void NetExecute()
	{
		if (this.controller.IsNull())
		{
			return;
		}
		if (this.state == CustomMapsAttackBehaviour.State.Attacking && !this.useColliders && this.startTime > this.lastAttackTime && Time.time > this.startTime + this.damageDelayAfterPlayingAnimation)
		{
			this.TriggerAttack(null);
		}
		if (this.controller.IsAnimationPlaying(this.attackAnimName))
		{
			return;
		}
		CustomMapsAttackBehaviour.State state = this.state;
		if (state != CustomMapsAttackBehaviour.State.Idle)
		{
			if (state != CustomMapsAttackBehaviour.State.Attacking)
			{
				return;
			}
			if (Time.time < this.startTime + this.timeBetweenAttacks)
			{
				this.state = CustomMapsAttackBehaviour.State.Idle;
				return;
			}
			this.startTime = Time.time;
			this.controller.PlayAnimation(this.attackAnimName, this.animBlendTime);
			return;
		}
		else
		{
			if (Time.time < this.startTime + this.timeBetweenAttacks)
			{
				return;
			}
			this.startTime = Time.time;
			this.state = CustomMapsAttackBehaviour.State.Attacking;
			this.controller.PlayAnimation(this.attackAnimName, this.animBlendTime);
			return;
		}
	}

	public override void ResetBehavior()
	{
		this.state = CustomMapsAttackBehaviour.State.Idle;
	}

	private void FaceTarget()
	{
		if (this.controller.TargetPlayer.IsNull())
		{
			return;
		}
		GameAgent.UpdateFacingTarget(this.controller.transform, this.controller.agent.navAgent, this.controller.TargetPlayer.transform, this.turnSpeed);
	}

	public override void OnTriggerEnter(Collider otherCollider)
	{
		if (!this.useColliders)
		{
			return;
		}
		if (Time.time < this.lastAttackTime + this.timeBetweenAttacks || this.state != CustomMapsAttackBehaviour.State.Attacking)
		{
			return;
		}
		GRPlayer componentInParent = otherCollider.GetComponentInParent<GRPlayer>();
		if (componentInParent.IsNull())
		{
			return;
		}
		if (componentInParent.MyRig.IsNotNull() && !componentInParent.MyRig.isLocal)
		{
			return;
		}
		if (componentInParent.State == GRPlayer.GRPlayerState.Ghost)
		{
			return;
		}
		this.TriggerAttack(componentInParent);
	}

	private void TriggerAttack(GRPlayer targetPlayer = null)
	{
		this.lastAttackTime = Time.time;
		GRPlayer grplayer = ((targetPlayer != null) ? targetPlayer : (this.controller.entity.IsAuthority() ? this.controller.TargetPlayer : null));
		if (!this.controller.entity.IsAuthority() && grplayer == null)
		{
			Vector3 vector = this.controller.transform.position + this.controller.transform.TransformVector(this.sightOffset);
			grplayer = this.controller.FindBestTarget(vector, this.attackDist, this.attackDistSq, this.sightMinDot);
		}
		if (grplayer == null)
		{
			return;
		}
		if (!grplayer.MyRig.isLocal)
		{
			return;
		}
		if (this.controller.entity.IsAuthority() && !this.IsTargetInAttackRange(grplayer))
		{
			return;
		}
		switch (this.attackType)
		{
		case AttackType.Tag:
			if (GameMode.ActiveGameMode.GameType() != GameModeType.Custom)
			{
				GameMode.ReportHit();
				return;
			}
			CustomGameMode.TaggedByAI(this.controller.entity, grplayer.MyRig.OwningNetPlayer.ActorNumber);
			return;
		case AttackType.UseGT:
			CustomMapsGameManager.instance.OnPlayerHit(this.controller.entity.id, grplayer, this.controller.transform.position);
			return;
		case AttackType.UseLuau:
			CustomGameMode.OnPlayerHit(this.controller.entity, grplayer.MyRig.OwningNetPlayer.ActorNumber, this.damageAmount);
			return;
		default:
			return;
		}
	}

	private CustomMapsAIBehaviourController controller;

	private CustomMapsAttackBehaviour.State state;

	private AttackType attackType;

	private float attackDist;

	private float attackDistSq;

	private bool stopMovingToAttack;

	private bool useColliders;

	private float damageAmount;

	private Vector3 sightOffset;

	private float sightFOV;

	private float sightMinDot;

	private string attackAnimName;

	private float timeBetweenAttacks;

	private float damageDelayAfterPlayingAnimation;

	private float animBlendTime;

	private float startTime;

	private float turnSpeed;

	private float lastAttackTime;

	private enum State
	{
		Idle,
		Attacking
	}
}
