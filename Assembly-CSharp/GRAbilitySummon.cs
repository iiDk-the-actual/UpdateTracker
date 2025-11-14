using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GRAbilitySummon : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
	}

	public override void Start()
	{
		base.Start();
		this.lastAnimIndex = AbilityHelperFunctions.RandomRangeUnique(0, this.animData.Count, this.lastAnimIndex);
		this.duration = this.animData[this.lastAnimIndex].duration;
		this.chargeTime = this.animData[this.lastAnimIndex].eventTime;
		this.PlayAnim(this.animData[this.lastAnimIndex].animName, 0.1f, this.animSpeed);
		this.state = GRAbilitySummon.State.Charge;
		this.summonSound.Play(this.audioSource);
		this.spawnedCount = 0;
		this.agent.navAgent.isStopped = true;
		this.agent.navAgent.speed = 1f;
		if (this.fxStartSummon != null)
		{
			this.fxStartSummon.SetActive(false);
			this.fxStartSummon.SetActive(true);
		}
	}

	public override void Stop()
	{
		this.lookAtTarget = null;
		this.agent.navAgent.isStopped = false;
	}

	public void SetLookAtTarget(Transform transform)
	{
		this.lookAtTarget = transform;
	}

	public override void Think(float dt)
	{
		this.UpdateState(dt);
	}

	protected override void UpdateShared(float dt)
	{
		if (this.lookAtTarget != null)
		{
			GameAgent.UpdateFacingTarget(this.root, this.agent.navAgent, this.lookAtTarget, 360f);
		}
	}

	private void UpdateState(float dt)
	{
		double num = Time.timeAsDouble - this.startTime;
		switch (this.state)
		{
		case GRAbilitySummon.State.Charge:
			if (num > (double)this.chargeTime)
			{
				this.SetState(GRAbilitySummon.State.Spawn);
				return;
			}
			break;
		case GRAbilitySummon.State.Spawn:
			if (!this.spawned)
			{
				this.spawned = this.DoSpawn();
			}
			if (this.spawned && num > (double)this.duration)
			{
				this.SetState(GRAbilitySummon.State.Done);
				this.spawned = false;
			}
			break;
		case GRAbilitySummon.State.Done:
			break;
		default:
			return;
		}
	}

	private void SetState(GRAbilitySummon.State newState)
	{
		GRAbilitySummon.State state = this.state;
		this.state = newState;
		switch (newState)
		{
		default:
			return;
		}
	}

	private Vector3? GetSpawnLocation()
	{
		Vector3 position = this.root.position;
		float num = Random.Range(-this.summonConeAngle / 2f, this.summonConeAngle / 2f);
		int i = 0;
		while (i < 5)
		{
			Vector3 vector = Quaternion.Euler(0f, num, 0f) * this.root.forward;
			Vector3 vector2 = position + vector * this.desiredSpawnDistance;
			NavMeshHit navMeshHit;
			if (NavMesh.Raycast(position, vector2, out navMeshHit, this.walkableArea))
			{
				if (navMeshHit.distance < this.minSpawnDistance)
				{
					num += 15f;
					if (num > this.summonConeAngle / 2f)
					{
						this.summonConeAngle = -this.summonConeAngle / 2f;
					}
					i++;
					continue;
				}
				vector2 = navMeshHit.position + Vector3.up * this.spawnHeight;
			}
			return new Vector3?(vector2);
		}
		return null;
	}

	private bool DoSpawn()
	{
		Vector3? spawnLocation = this.GetSpawnLocation();
		if (spawnLocation != null)
		{
			if (this.entity.IsAuthority())
			{
				Quaternion identity = Quaternion.identity;
				GhostReactorManager.Get(this.entity).gameEntityManager.RequestCreateItem(this.entityPrefabToSpawn.name.GetStaticHash(), spawnLocation.Value, identity, (long)this.entity.GetNetId());
				this.spawnedCount++;
			}
			if (this.audioSource != null)
			{
				this.audioSource.PlayOneShot(this.summonSpawnAudioClip);
			}
			if (this.fxOnSpawn != null)
			{
				this.fxOnSpawn.SetActive(false);
				this.fxOnSpawn.SetActive(true);
			}
			return true;
		}
		return false;
	}

	public override bool IsDone()
	{
		return this.state == GRAbilitySummon.State.Done;
	}

	private int lastAnimIndex = -1;

	public GameEntity entityPrefabToSpawn;

	public List<AnimationData> animData;

	private float animSpeed = 1f;

	public float chargeTime = 3f;

	public float duration = 3f;

	public float desiredSpawnDistance = 3f;

	public float minSpawnDistance = 1f;

	public float spawnHeight = 1f;

	public float summonConeAngle = 120f;

	private bool spawned;

	public AudioClip summonSpawnAudioClip;

	public GameObject fxStartSummon;

	public GameObject fxOnSpawn;

	public AbilitySound summonSound;

	private int spawnedCount;

	public Transform lookAtTarget;

	private GRAbilitySummon.State state;

	private enum State
	{
		Charge,
		Spawn,
		Done
	}
}
