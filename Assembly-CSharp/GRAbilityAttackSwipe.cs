using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GRAbilityAttackSwipe : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.target = null;
		if (this.damageTrigger != null)
		{
			this.damageTrigger.SetActive(false);
		}
	}

	public override void Start()
	{
		base.Start();
		if (this.animData.Count > 0)
		{
			this.lastAnimIndex = AbilityHelperFunctions.RandomRangeUnique(0, this.animData.Count, this.lastAnimIndex);
			this.duration = this.animData[this.lastAnimIndex].duration;
			this.PlayAnim(this.animData[this.lastAnimIndex].animName, 0.1f, this.animData[this.lastAnimIndex].speed);
			this.animNameString = this.animData[this.lastAnimIndex].animName;
		}
		else
		{
			this.duration = 0.5f;
		}
		this.soundAttack.soundSelectMode = AbilitySound.SoundSelectMode.Random;
		this.soundAttack.Play(null);
		this.agent.SetIsPathing(false, true);
		this.agent.SetDisableNetworkSync(true);
		if (this.damageTrigger != null)
		{
			this.damageTrigger.SetActive(false);
		}
		this.state = GRAbilityAttackSwipe.State.Tell;
	}

	public override void Stop()
	{
		this.agent.SetIsPathing(true, true);
		this.agent.SetDisableNetworkSync(false);
		if (this.damageTrigger != null)
		{
			this.damageTrigger.SetActive(false);
		}
	}

	public override bool IsDone()
	{
		return this.state == GRAbilityAttackSwipe.State.Done;
	}

	protected override void UpdateShared(float dt)
	{
		float num = (float)(Time.timeAsDouble - this.startTime);
		switch (this.state)
		{
		case GRAbilityAttackSwipe.State.Tell:
			this.targetPos = this.root.position + this.root.transform.forward;
			if (this.target != null)
			{
				this.targetPos = this.target.position;
			}
			GameAgent.UpdateFacingTarget(this.root, this.agent.navAgent, this.target, this.maxTurnSpeed);
			if (num > this.tellDuration)
			{
				this.state = GRAbilityAttackSwipe.State.Attack;
				if (this.damageTrigger != null)
				{
					this.damageTrigger.SetActive(true);
				}
				this.initialPos = this.root.position;
				this.initialVel = (this.targetPos - this.initialPos).normalized * this.attackMoveSpeed;
				return;
			}
			break;
		case GRAbilityAttackSwipe.State.Attack:
		{
			float num2 = num - this.tellDuration;
			Vector3 vector = this.initialPos + this.initialVel * num2;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(vector, out navMeshHit, 0.5f, this.walkableArea))
			{
				vector = navMeshHit.position;
				if (NavMesh.Raycast(this.initialPos, vector, out navMeshHit, this.walkableArea))
				{
					vector = navMeshHit.position;
				}
				this.root.position = vector;
			}
			if (num > this.tellDuration + this.attackDuration)
			{
				if (this.damageTrigger != null)
				{
					this.damageTrigger.SetActive(false);
				}
				this.state = GRAbilityAttackSwipe.State.FollowThrough;
				return;
			}
			break;
		}
		case GRAbilityAttackSwipe.State.FollowThrough:
			if (num >= this.duration)
			{
				this.state = GRAbilityAttackSwipe.State.Done;
			}
			break;
		default:
			return;
		}
	}

	public void SetTargetPlayer(NetPlayer targetPlayer)
	{
		this.target = null;
		if (targetPlayer != null)
		{
			GRPlayer grplayer = GRPlayer.Get(targetPlayer.ActorNumber);
			if (grplayer != null && grplayer.State == GRPlayer.GRPlayerState.Alive)
			{
				this.target = grplayer.transform;
			}
		}
	}

	public string GetAnimName()
	{
		return this.animNameString;
	}

	public float duration;

	public float tellDuration;

	public float attackDuration;

	public float attackMoveSpeed;

	public List<AnimationData> animData;

	public AbilitySound soundAttack;

	private GRAbilityAttackSwipe.State state;

	public float maxTurnSpeed;

	public GameObject damageTrigger;

	private Transform target;

	private string animNameString;

	private int lastAnimIndex = -1;

	public Vector3 targetPos;

	public Vector3 initialPos;

	public Vector3 initialVel;

	private enum State
	{
		Tell,
		Attack,
		FollowThrough,
		Done
	}
}
