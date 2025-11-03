using System;
using UnityEngine;

[Serializable]
public class GRAbilityGrabbed : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.idleAbility.Setup(agent, anim, audioSource, root, head, lineOfSight);
	}

	public override void Start()
	{
		base.Start();
		this.agent.SetIsPathing(false, true);
		this.idleAbility.Start();
	}

	public override void Stop()
	{
		this.idleAbility.Stop();
		this.agent.SetIsPathing(true, true);
	}

	public override bool IsDone()
	{
		return this.idleAbility.IsDone();
	}

	public override void Update(float dt)
	{
		this.idleAbility.Update(dt);
	}

	public GRAbilityIdle idleAbility;
}
