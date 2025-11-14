using System;
using CjLib;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GRAbilityJump : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		this.isActive = false;
	}

	public void SetupJump(Vector3 start, Vector3 end, float heightScale = 1f, float speedScale = 1f)
	{
		this.elapsedTime = 0f;
		this.startPos = start;
		this.endPos = end;
		float magnitude = (this.endPos - this.startPos).magnitude;
		this.controlPoint = (this.startPos + this.endPos) / 2f + new Vector3(0f, magnitude * heightScale, 0f);
		this.jumpTime = magnitude / (this.jumpSpeed * speedScale);
	}

	public void SetupJumpFromLinkData(OffMeshLinkData linkData)
	{
		if ((this.root.position - linkData.startPos).sqrMagnitude < (this.root.position - linkData.endPos).sqrMagnitude)
		{
			this.SetupJump(linkData.startPos, linkData.endPos, 1f, 1f);
			return;
		}
		this.SetupJump(linkData.endPos, linkData.startPos, 1f, 1f);
	}

	public override void Start()
	{
		base.Start();
		this.elapsedTime = 0f;
		this.isActive = true;
		this.PlayAnim(this.animationData.animName, 0.05f, this.animationData.speed);
		this.agent.navAgent.isStopped = true;
		this.agent.SetDisableNetworkSync(true);
		this.agent.pauseEntityThink = true;
		this.soundJump.Play(this.audioSource);
	}

	public override void Stop()
	{
		base.Stop();
		this.agent.navAgent.Warp(this.endPos);
		this.agent.navAgent.CompleteOffMeshLink();
		this.agent.navAgent.isStopped = false;
		this.isActive = false;
		this.agent.SetDisableNetworkSync(false);
		this.agent.pauseEntityThink = false;
	}

	public override bool IsDone()
	{
		return this.elapsedTime >= this.jumpTime;
	}

	public bool IsActive()
	{
		return this.isActive;
	}

	protected override void UpdateShared(float dt)
	{
		if (GhostReactorManager.entityDebugEnabled)
		{
			DebugUtil.DrawLine(this.startPos, this.controlPoint, Color.green, true);
			DebugUtil.DrawLine(this.endPos, this.controlPoint, Color.green, true);
		}
		float num = ((this.jumpTime > 0f) ? Math.Clamp(this.elapsedTime / this.jumpTime, 0f, 1f) : 1f);
		Vector3 vector = GRAbilityJump.EvaluateQuadratic(this.startPos, this.controlPoint, this.endPos, num);
		this.root.position = vector;
		if (this.rb != null)
		{
			this.rb.position = vector;
		}
		this.elapsedTime += dt;
	}

	public static Vector3 EvaluateQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		Vector3 vector = Vector3.Lerp(p0, p1, t);
		Vector3 vector2 = Vector3.Lerp(p1, p2, t);
		return Vector3.Lerp(vector, vector2, t);
	}

	private Vector3 startPos;

	private Vector3 endPos;

	private Vector3 controlPoint;

	[ReadOnly]
	public float jumpTime;

	[ReadOnly]
	public float elapsedTime;

	private bool isActive;

	public AnimationData animationData;

	public float jumpSpeed = 3f;

	public AbilitySound soundJump;
}
