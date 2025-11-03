using System;
using UnityEngine;

[Serializable]
public class GRAbilityAttackJump : GRAbilityBase
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
		this.startTime = Time.timeAsDouble;
		if (this.damageTrigger != null)
		{
			this.damageTrigger.SetActive(false);
		}
		this.agent.SetIsPathing(false, true);
		this.agent.SetDisableNetworkSync(true);
		this.state = GRAbilityAttackJump.State.Tell;
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
		return Time.timeAsDouble - this.startTime >= (double)this.duration;
	}

	protected override void UpdateShared(float dt)
	{
		double num = (double)((float)Time.timeAsDouble) - this.startTime;
		switch (this.state)
		{
		case GRAbilityAttackJump.State.Tell:
			if (num > (double)this.jumpTime)
			{
				this.targetPos = this.agent.transform.position + this.agent.transform.forward * 0.5f;
				if (this.target != null)
				{
					Vector3 vector = this.target.transform.position - this.agent.transform.position;
					this.targetPos = this.agent.transform.position + vector * this.jumpLengthScale;
					this.targetPos.y = this.target.transform.position.y;
				}
				float num2 = this.attackLandTime - this.jumpTime;
				num2 = Mathf.Max(0.1f, num2);
				this.initialPos = this.agent.transform.position;
				Vector3 vector2 = this.targetPos - this.initialPos;
				float y = vector2.y;
				vector2.y = 0f;
				float num3 = num2;
				float num4 = 0f;
				if (num3 > 0f)
				{
					Vector3 gravity = Physics.gravity;
					num4 = (y - 0.5f * gravity.y * num3 * num3) / num3;
				}
				this.initialVel = vector2 / num2;
				this.initialVel.y = num4;
				if (this.damageTrigger != null)
				{
					this.damageTrigger.SetActive(true);
				}
				this.PlayAnim(this.jumpAnimName, 0.1f, this.animSpeed);
				this.jumpSound.Play(null);
				this.state = GRAbilityAttackJump.State.Jump;
			}
			break;
		case GRAbilityAttackJump.State.Jump:
		{
			float num5 = (float)(num - (double)this.jumpTime);
			Vector3 vector3 = this.initialPos + this.initialVel * num5 + 0.5f * Physics.gravity * num5 * num5;
			this.root.position = vector3;
			if (num > (double)this.attackLandTime)
			{
				if (this.damageTrigger != null)
				{
					this.damageTrigger.SetActive(false);
				}
				if (this.doReturnPhase)
				{
					float num6 = this.attackReturnTime - this.attackLandTime;
					num6 = Mathf.Max(0.1f, num6);
					Vector3 vector4 = this.initialPos;
					this.initialPos = this.agent.transform.position;
					this.initialVel = (vector4 - this.initialPos) / num6;
					this.state = GRAbilityAttackJump.State.Return;
				}
				else
				{
					this.state = GRAbilityAttackJump.State.Done;
				}
			}
			break;
		}
		case GRAbilityAttackJump.State.Return:
		{
			float num7 = (float)(num - (double)this.attackLandTime);
			Vector3 vector5 = this.initialPos + this.initialVel * num7;
			this.root.position = vector5;
			if (num > (double)this.attackReturnTime)
			{
				this.state = GRAbilityAttackJump.State.Done;
			}
			break;
		}
		}
		GameAgent.UpdateFacingTarget(this.root, this.agent.navAgent, this.target, this.maxTurnSpeed);
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

	public float duration;

	public float attackMoveSpeed;

	public float jumpTime;

	public float attackLandTime;

	public float attackReturnTime;

	public bool doReturnPhase = true;

	public float jumpLengthScale = 1f;

	public string animName;

	public float animSpeed;

	public float maxTurnSpeed;

	public string jumpAnimName;

	public AbilitySound jumpSound;

	public GameObject damageTrigger;

	private Transform target;

	private GRAbilityAttackJump.State state;

	public Vector3 targetPos;

	public Vector3 initialPos;

	public Vector3 initialVel;

	private enum State
	{
		Tell,
		Jump,
		Return,
		Done
	}
}
