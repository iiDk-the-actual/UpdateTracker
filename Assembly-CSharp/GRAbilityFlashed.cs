using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GRAbilityFlashed : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
	}

	public void SetStunTime(float time)
	{
		this.stunTime = time;
	}

	public override void Start()
	{
		base.Start();
		if (this.flashAnimations.Count > 0)
		{
			this.flashAnimationIndex = AbilityHelperFunctions.RandomRangeUnique(0, this.flashAnimations.Count, this.flashAnimationIndex);
			this.PlayAnim(this.flashAnimations[this.flashAnimationIndex].animName, 0.1f, this.flashAnimations[this.flashAnimationIndex].speed);
			this.behaviorEndTime = Time.timeAsDouble + (double)this.flashAnimations[this.flashAnimationIndex].duration + (double)this.stunTime;
		}
		else
		{
			this.PlayAnim("GREnemyFlashReaction01", 0.1f, 1f);
			this.behaviorEndTime = Time.timeAsDouble + 0.5 + (double)this.stunTime;
		}
		this.agent.SetIsPathing(false, true);
		this.agent.SetDisableNetworkSync(true);
	}

	public override void Stop()
	{
		this.agent.SetIsPathing(true, true);
		this.agent.SetDisableNetworkSync(false);
	}

	public override bool IsDone()
	{
		return Time.timeAsDouble >= this.behaviorEndTime;
	}

	public List<AnimationData> flashAnimations;

	private int flashAnimationIndex;

	private double behaviorEndTime;

	private float stunTime;
}
