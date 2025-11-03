using System;
using GorillaExtensions;
using GT_CustomMapSupportRuntime;
using UnityEngine;
using UnityEngine.AI;

public class CustomMapsChaseBehaviour : CustomMapsBehaviourBase
{
	public CustomMapsChaseBehaviour(CustomMapsAIBehaviourController AIController, AIAgent agentSettings)
	{
		this.sightOffset = agentSettings.sightOffset;
		this.rememberLoseSightPos = agentSettings.rememberLoseSightPosition;
		this.loseSightDist = agentSettings.loseSightDist;
		this.loseSightDistSq = this.loseSightDist * this.loseSightDist;
		this.stopDistSq = agentSettings.stopDist * agentSettings.stopDist;
		this.controller = AIController;
	}

	public override bool CanExecute()
	{
		return !this.controller.IsNull() && !this.controller.TargetPlayer.IsNull();
	}

	public override bool CanContinueExecuting()
	{
		if (!this.CanExecute())
		{
			return false;
		}
		bool flag;
		if (this.IsTargetInChaseRange(out flag))
		{
			return !flag;
		}
		if (!this.controller.IsTargetable(this.controller.TargetPlayer))
		{
			this.controller.StopMoving();
		}
		this.controller.ClearTarget();
		return false;
	}

	public override void Execute()
	{
		bool flag;
		if (!this.IsTargetInChaseRange(out flag))
		{
			this.controller.ClearTarget();
			this.isChasing = false;
			if (!this.rememberLoseSightPos)
			{
				this.controller.StopMoving();
			}
			return;
		}
		if (!this.IsTargetVisible())
		{
			this.controller.ClearTarget();
			this.isChasing = false;
			if (!this.rememberLoseSightPos)
			{
				this.controller.StopMoving();
			}
			return;
		}
		if (flag && this.isChasing)
		{
			this.isChasing = false;
			this.controller.StopMoving();
			return;
		}
		this.isChasing = true;
		this.controller.RequestDestination(this.controller.TargetPlayer.transform.position);
	}

	private bool IsTargetVisible()
	{
		Vector3 vector = this.controller.transform.position + this.controller.transform.TransformVector(this.sightOffset);
		return this.controller.IsTargetVisible(vector, this.controller.TargetPlayer, this.loseSightDist);
	}

	private bool IsTargetInChaseRange(out bool withinStopDist)
	{
		withinStopDist = false;
		Vector3 vector;
		if (!this.controller.IsTargetInRange(this.controller.transform.position, this.controller.TargetPlayer, this.loseSightDistSq, out vector))
		{
			return false;
		}
		if (vector.sqrMagnitude < this.stopDistSq)
		{
			withinStopDist = true;
		}
		return true;
	}

	public override void NetExecute()
	{
	}

	public override void ResetBehavior()
	{
		this.isChasing = false;
	}

	public override void OnTriggerEnter(Collider otherCollider)
	{
	}

	private NavMeshAgent navMeshAgent;

	private CustomMapsAIBehaviourController controller;

	private float loseSightDist;

	private float loseSightDistSq;

	private Vector3 sightOffset;

	private bool rememberLoseSightPos;

	private float stopDistSq;

	private bool isChasing;
}
