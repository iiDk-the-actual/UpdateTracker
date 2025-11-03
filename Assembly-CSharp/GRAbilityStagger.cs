using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GRAbilityStagger : GRAbilityBase
{
	public void SetStunTime(float time)
	{
		this.stunTime = time;
	}

	public void SetStaggerVelocity(Vector3 vel)
	{
		float magnitude = vel.magnitude;
		if (magnitude > 0f)
		{
			Vector3 vector = vel / magnitude;
			vector.y = 0f;
			vel = vector * magnitude;
		}
		this.staggerMovement.InitFromVelocityAndDuration(vel, this.duration);
	}

	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.staggerMovement.Setup(root);
		this.staggerMovement.interpolationType = GRAbilityInterpolatedMovement.InterpType.EaseOut;
	}

	public override void Start()
	{
		base.Start();
		if (this.animData.Count > 0)
		{
			this.lastAnimIndex = AbilityHelperFunctions.RandomRangeUnique(0, this.animData.Count, this.lastAnimIndex);
			this.duration = this.animData[this.lastAnimIndex].duration + this.stunTime;
			this.PlayAnim(this.animData[this.lastAnimIndex].animName, 0.1f, this.animData[this.lastAnimIndex].speed);
			this.animNameString = this.animData[this.lastAnimIndex].animName;
		}
		else
		{
			this.duration = 0.5f + this.stunTime;
		}
		this.agent.SetIsPathing(false, true);
		this.agent.SetDisableNetworkSync(true);
		this.staggerMovement.InitFromVelocityAndDuration(this.staggerMovement.velocity, this.duration);
		this.staggerMovement.Start();
	}

	public override void Stop()
	{
		this.agent.SetIsPathing(true, true);
		this.agent.SetDisableNetworkSync(false);
	}

	public override bool IsDone()
	{
		return this.staggerMovement.IsDone();
	}

	protected override void UpdateShared(float dt)
	{
		this.staggerMovement.Update(dt);
	}

	public string GetAnimName()
	{
		return this.animNameString;
	}

	private float duration;

	public List<AnimationData> animData;

	private int lastAnimIndex = -1;

	private string animNameString;

	public GRAbilityInterpolatedMovement staggerMovement;

	private float stunTime;
}
