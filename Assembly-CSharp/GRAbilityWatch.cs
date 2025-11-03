using System;
using Unity.XR.CoreUtils;
using UnityEngine;

[Serializable]
public class GRAbilityWatch : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.target = null;
	}

	public override void Start()
	{
		base.Start();
		this.PlayAnim(this.animName, 0.1f, this.animSpeed);
		this.endTime = -1.0;
		if (this.duration > 0f)
		{
			this.endTime = Time.timeAsDouble + (double)this.duration;
		}
		this.agent.navAgent.isStopped = true;
	}

	public override void Stop()
	{
		this.agent.navAgent.isStopped = false;
	}

	public override bool IsDone()
	{
		return this.endTime > 0.0 && Time.timeAsDouble >= this.endTime;
	}

	protected override void UpdateShared(float dt)
	{
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

	public string animName;

	public float animSpeed;

	public float maxTurnSpeed;

	private Transform target;

	[ReadOnly]
	public double endTime;
}
