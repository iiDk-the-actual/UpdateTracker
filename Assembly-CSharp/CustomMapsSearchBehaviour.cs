using System;
using GorillaExtensions;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class CustomMapsSearchBehaviour : CustomMapsBehaviourBase
{
	public CustomMapsSearchBehaviour(CustomMapsAIBehaviourController AIcontroller, AIAgent agentSettings)
	{
		this.sightOffset = agentSettings.sightOffset;
		this.sightDist = agentSettings.sightDist;
		this.sightDistSq = this.sightDist * this.sightDist;
		this.sightFOV = agentSettings.sightFOV;
		this.sightMinDot = Mathf.Cos(this.sightFOV / 2f * 0.017453292f);
		this.controller = AIcontroller;
	}

	public override bool CanExecute()
	{
		return !this.controller.IsNull();
	}

	public override bool CanContinueExecuting()
	{
		return this.CanExecute() && this.controller.TargetPlayer == null;
	}

	public override void Execute()
	{
		if (Time.time < this.lastSearchTime + 0.1f)
		{
			return;
		}
		this.lastSearchTime = Time.time;
		Vector3 vector = this.controller.transform.position + this.controller.transform.TransformVector(this.sightOffset);
		this.controller.SetTarget(this.controller.FindBestTarget(vector, this.sightDist, this.sightDistSq, this.sightMinDot));
	}

	public override void NetExecute()
	{
	}

	public override void ResetBehavior()
	{
	}

	public override void OnTriggerEnter(Collider otherCollider)
	{
	}

	private const float SEARCH_COOLDOWN = 0.1f;

	private CustomMapsAIBehaviourController controller;

	private float sightDist;

	private float sightDistSq;

	private Vector3 sightOffset;

	private float sightFOV;

	private float sightMinDot;

	private float lastSearchTime;
}
