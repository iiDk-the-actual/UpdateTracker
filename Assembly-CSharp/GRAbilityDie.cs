using System;
using System.Collections.Generic;
using GorillaTagScripts.GhostReactor;
using UnityEngine;

[Serializable]
public class GRAbilityDie : GRAbilityBase
{
	public override void Setup(GameAgent agent, Animation anim, AudioSource audioSource, Transform root, Transform head, GRSenseLineOfSight lineOfSight)
	{
		base.Setup(agent, anim, audioSource, root, head, lineOfSight);
		if (this.disableAllCollidersWhenDead)
		{
			agent.GetComponentsInChildren<Collider>(this.disableCollidersWhenDead);
		}
		if (this.disableAllRenderersWhenDead)
		{
			agent.GetComponentsInChildren<Renderer>(this.hideWhenDead);
		}
		GRAbilityDie.Disable(this.disableCollidersWhenDead, false);
		this.staggerMovement.Setup(root);
	}

	public override void Start()
	{
		base.Start();
		if (this.animData.Count > 0)
		{
			int num = Random.Range(0, this.animData.Count);
			this.delayDeath = this.animData[num].duration;
			this.staggerMovement.InitFromVelocityAndDuration(this.staggerMovement.velocity, this.delayDeath);
			this.PlayAnim(this.animData[num].animName, 0.1f, this.animData[num].speed);
		}
		this.agent.SetIsPathing(false, true);
		this.agent.SetDisableNetworkSync(true);
		this.isDead = false;
		if (this.doKnockback)
		{
			this.staggerMovement.Start();
		}
		this.soundDeath.soundSelectMode = AbilitySound.SoundSelectMode.Random;
		this.soundOnHide.soundSelectMode = AbilitySound.SoundSelectMode.Random;
		this.soundDeath.Play(null);
		GRAbilityDie.Disable(this.disableCollidersWhenDead, true);
	}

	public override void Stop()
	{
		this.staggerMovement.Stop();
		this.agent.SetIsPathing(true, true);
		this.agent.SetDisableNetworkSync(false);
		GRAbilityDie.Hide(this.hideWhenDead, false);
		GRAbilityDie.Disable(this.disableCollidersWhenDead, false);
	}

	public void SetStaggerVelocity(Vector3 vel)
	{
		float magnitude = vel.magnitude;
		if (magnitude > 0f)
		{
			Vector3 vector = vel / magnitude;
			vector.y = 0f;
			vel = vector * magnitude;
		}
		this.staggerMovement.InitFromVelocityAndDuration(vel, this.delayDeath);
	}

	public void SetInstigatingPlayerIndex(int actorNumber)
	{
		this.instigatingActorNumber = actorNumber;
	}

	private void Die()
	{
		this.soundOnHide.Play(null);
		if (this.fxDeath != null)
		{
			this.fxDeath.SetActive(false);
			this.fxDeath.SetActive(true);
		}
		GRAbilityDie.Hide(this.hideWhenDead, true);
		GRAbilityDie.Disable(this.disableCollidersWhenDead, true);
		GameEntity entity = this.agent.entity;
		GameEntity gameEntity;
		if (this.lootTable != null && entity.IsAuthority() && this.lootTable.TryForRandomItem(entity, out gameEntity, 0))
		{
			Transform transform = this.lootSpawnMarker;
			if (transform == null)
			{
				transform = this.agent.transform;
			}
			Vector3 position = transform.position;
			if (transform == null)
			{
				position.y += 0.33f;
			}
			entity.manager.RequestCreateItem(gameEntity.gameObject.name.GetStaticHash(), position, transform.rotation, 0L);
		}
	}

	public void DestroySelf()
	{
		GameEntity entity = this.agent.entity;
		GRPlayer grplayer = GRPlayer.Get(this.instigatingActorNumber);
		if (grplayer != null)
		{
			grplayer.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.Kills, 1f);
		}
		GREnemyType? enemyType = entity.GetEnemyType();
		if (enemyType != null)
		{
			GREnemyType valueOrDefault = enemyType.GetValueOrDefault();
			GhostReactor.instance.shiftManager.shiftStats.IncrementEnemyKills(valueOrDefault);
		}
		if (entity.IsAuthority())
		{
			entity.manager.RequestDestroyItem(entity.id);
		}
	}

	public override bool IsDone()
	{
		return false;
	}

	protected override void UpdateShared(float dt)
	{
		if (this.startTime >= 0.0)
		{
			if (this.doKnockback)
			{
				this.staggerMovement.Update(dt);
			}
			double num = Time.timeAsDouble - this.startTime;
			if (!this.isDead && num > (double)this.delayDeath)
			{
				this.isDead = true;
				this.Die();
				return;
			}
			if (this.isDead && num > (double)(this.delayDeath + this.destroyDelay))
			{
				GhostReactorManager.Get(this.entity).OnAbilityDie(this.entity);
				this.DestroySelf();
				this.startTime = -1.0;
			}
		}
	}

	public static void Hide(List<Renderer> renderers, bool hide)
	{
		if (renderers == null)
		{
			return;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].enabled = !hide;
			}
		}
	}

	public static void Disable(List<Collider> colliders, bool disable)
	{
		if (colliders == null)
		{
			return;
		}
		for (int i = 0; i < colliders.Count; i++)
		{
			if (colliders[i] != null)
			{
				colliders[i].enabled = !disable;
			}
		}
	}

	public float delayDeath;

	public List<Renderer> hideWhenDead;

	public List<Collider> disableCollidersWhenDead;

	public bool disableAllCollidersWhenDead;

	public bool disableAllRenderersWhenDead;

	public GameObject fxDeath;

	public AbilitySound soundDeath;

	public AbilitySound soundOnHide;

	public float destroyDelay = 3f;

	public bool doKnockback = true;

	public GRBreakableItemSpawnConfig lootTable;

	public Transform lootSpawnMarker;

	public List<AnimationData> animData;

	private int instigatingActorNumber;

	private bool isDead;

	public GRAbilityInterpolatedMovement staggerMovement;
}
