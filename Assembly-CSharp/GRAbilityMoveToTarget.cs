using System;
using UnityEngine;

[Serializable]
public class GRAbilityMoveToTarget : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.target = null;
		this.targetPos = agent.transform.position;
	}

	public override void Start()
	{
		base.Start();
		this.PlayAnim(this.animName, 0.3f, this.animSpeed);
		if (this.attributes && this.moveSpeed == 0f)
		{
			this.moveSpeed = this.attributes.CalculateFinalFloatValueForAttribute(GRAttributeType.PatrolSpeed);
		}
		this.agent.navAgent.speed = this.moveSpeed;
		this.targetPos = this.agent.transform.position;
		this.movementSound.Play(null);
	}

	public override void Stop()
	{
		base.Stop();
		this.movementSound.Stop();
	}

	public override bool IsDone()
	{
		return (this.targetPos - this.root.position).sqrMagnitude < 0.25f;
	}

	protected override void UpdateShared(float dt)
	{
		if (this.target != null)
		{
			this.targetPos = this.target.position;
			this.agent.RequestDestination(this.targetPos);
		}
		Transform transform = ((this.lookAtTarget != null) ? this.lookAtTarget : this.target);
		GameAgent.UpdateFacingTarget(this.root, this.agent.navAgent, transform, this.maxTurnSpeed);
	}

	public void SetTarget(Transform transform)
	{
		this.target = transform;
	}

	public void SetTargetPos(Vector3 targetPos)
	{
		this.targetPos = targetPos;
		this.agent.RequestDestination(targetPos);
	}

	public Vector3 GetTargetPos()
	{
		return this.targetPos;
	}

	public void SetLookAtTarget(Transform transform)
	{
		this.lookAtTarget = transform;
	}

	public float moveSpeed;

	public string animName;

	public float animSpeed = 1f;

	public float maxTurnSpeed = 360f;

	public AbilitySound movementSound;

	private Vector3 targetPos;

	private Transform target;

	private Transform lookAtTarget;
}
