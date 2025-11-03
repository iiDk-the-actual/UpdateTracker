using System;
using Unity.XR.CoreUtils;
using UnityEngine;

[Serializable]
public class GRAbilityIdle : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.animLoops = 0;
	}

	public override void Start()
	{
		base.Start();
		this.PlayAnim(this.animName, 0.3f, this.animSpeed);
		this.animLoops = 0;
		this.events.Reset();
	}

	public override bool IsDone()
	{
		return (double)this.duration > 0.0 && Time.timeAsDouble >= this.startTime + (double)this.duration;
	}

	protected override void UpdateShared(float dt)
	{
		float num = (float)(Time.timeAsDouble - this.startTime);
		if (this.anim != null && this.anim[this.animName] != null)
		{
			if ((int)this.anim[this.animName].normalizedTime > this.animLoops)
			{
				this.events.Reset();
				this.animLoops = (int)this.anim[this.animName].normalizedTime;
			}
			num = this.anim[this.animName].time - this.anim[this.animName].length * (float)this.animLoops;
		}
		this.events.TryPlay(num, this.audioSource);
	}

	public float duration;

	public string animName;

	public float animSpeed;

	public GameAbilityEvents events;

	[ReadOnly]
	public int animLoops;
}
