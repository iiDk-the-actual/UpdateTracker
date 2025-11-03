using System;
using UnityEngine;

[Serializable]
public class GRAbilityAttackLatchOn : GRAbilityBase
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
		this.PlayAnim(this.animName, 0.1f, this.animSpeed);
		this.agent.navAgent.speed = this.tellMoveSpeed;
		this.startTime = Time.timeAsDouble;
		if (this.damageTrigger != null)
		{
			this.damageTrigger.SetActive(false);
		}
	}

	public override void Stop()
	{
		this.agent.transform.SetParent(null);
		this.agent.SetIsPathing(true, true);
		if (this.damageTrigger != null)
		{
			this.damageTrigger.SetActive(false);
		}
	}

	public override bool IsDone()
	{
		return Time.timeAsDouble - this.startTime >= (double)this.duration;
	}

	public override void Update(float dt)
	{
		this.UpdateNavSpeed();
		GameAgent.UpdateFacingTarget(this.root, this.agent.navAgent, this.target, this.maxTurnSpeed);
	}

	public override void UpdateRemote(float dt)
	{
		this.UpdateNavSpeed();
	}

	private void UpdateNavSpeed()
	{
		if (Time.timeAsDouble - this.startTime > (double)this.tellDuration)
		{
			this.agent.navAgent.velocity = this.agent.navAgent.velocity.normalized * this.attackMoveSpeed;
			this.agent.navAgent.speed = this.attackMoveSpeed;
			if (this.damageTrigger != null)
			{
				this.damageTrigger.SetActive(true);
			}
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
				this.agent.transform.SetParent(grplayer.attachEnemy);
				this.agent.transform.localPosition = Vector3.zero;
				this.agent.transform.localRotation = Quaternion.identity;
				this.agent.SetIsPathing(false, true);
			}
		}
	}

	public float duration;

	public float attackMoveSpeed;

	public float tellDuration;

	public float tellMoveSpeed;

	public string animName;

	public float animSpeed;

	public float maxTurnSpeed;

	public Transform target;

	public GameObject damageTrigger;
}
