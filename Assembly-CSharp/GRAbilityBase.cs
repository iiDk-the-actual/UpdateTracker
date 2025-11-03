using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public class GRAbilityBase
{
	public virtual void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		this.root = root;
		this.anim = anim;
		this.agent = agent;
		this.head = head;
		this.audioSource = audioSource;
		this.lineOfSight = lineOfSight;
		this.rb = agent.GetComponent<Rigidbody>();
		this.entity = agent.GetComponent<GameEntity>();
		this.attributes = agent.GetComponent<GRAttributes>();
		this.walkableArea = NavMesh.GetAreaFromName("walkable");
	}

	public virtual void Start()
	{
		this.startTime = Time.timeAsDouble;
	}

	public virtual void Stop()
	{
	}

	public virtual bool IsDone()
	{
		return false;
	}

	public virtual void Think(float dt)
	{
	}

	public virtual void Update(float dt)
	{
		this.UpdateShared(dt);
	}

	public virtual void UpdateRemote(float dt)
	{
		this.UpdateShared(dt);
	}

	protected virtual void UpdateShared(float dt)
	{
	}

	protected virtual void PlayAnim(string animName, float blendTime, float speed)
	{
		if (this.anim != null && !string.IsNullOrEmpty(animName))
		{
			this.anim[animName].speed = speed;
			this.anim.CrossFade(animName, blendTime);
		}
	}

	protected GameAgent agent;

	protected GameEntity entity;

	protected Animation anim;

	protected Transform root;

	protected Transform head;

	protected AudioSource audioSource;

	protected GRSenseLineOfSight lineOfSight;

	protected Rigidbody rb;

	protected GRAttributes attributes;

	[ReadOnly]
	public double startTime;

	protected int walkableArea = -1;
}
