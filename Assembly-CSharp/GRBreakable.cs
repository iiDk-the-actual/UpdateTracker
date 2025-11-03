using System;
using System.Collections.Generic;
using UnityEngine;

public class GRBreakable : MonoBehaviour, IGameHittable
{
	public bool BrokenLocal
	{
		get
		{
			return this.brokenLocal;
		}
	}

	private void OnEnable()
	{
		this.gameEntity.OnStateChanged += this.OnEntityStateChanged;
	}

	private void OnDisable()
	{
		if (this.gameEntity != null)
		{
			this.gameEntity.OnStateChanged -= this.OnEntityStateChanged;
		}
	}

	private void OnEntityStateChanged(long prevState, long nextState)
	{
		GRBreakable.BreakableState breakableState = (GRBreakable.BreakableState)nextState;
		if (breakableState == GRBreakable.BreakableState.Broken)
		{
			this.BreakLocal();
			return;
		}
		if (breakableState == GRBreakable.BreakableState.Unbroken)
		{
			this.RestoreLocal();
		}
	}

	public void BreakLocal()
	{
		if (!this.brokenLocal)
		{
			this.brokenLocal = true;
			if (this.breakableCollider != null)
			{
				this.breakableCollider.enabled = false;
			}
			for (int i = 0; i < this.disableWhenBroken.Count; i++)
			{
				this.disableWhenBroken[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < this.enableWhenBroken.Count; j++)
			{
				this.enableWhenBroken[j].gameObject.SetActive(true);
			}
			if (this.audioSource != null)
			{
				this.audioSource.PlayOneShot(this.breakSound, this.breakSoundVolume);
			}
			GameEntity gameEntity;
			if (this.gameEntity.IsAuthority() && this.holdsRandomItem && this.itemSpawnProbability.TryForRandomItem(this.gameEntity, out gameEntity, 0))
			{
				this.gameEntity.manager.RequestCreateItem(gameEntity.gameObject.name.GetStaticHash(), this.itemSpawnLocation.position, this.itemSpawnLocation.rotation, 0L);
			}
		}
	}

	public void RestoreLocal()
	{
		if (this.brokenLocal)
		{
			this.brokenLocal = false;
			if (this.breakableCollider != null)
			{
				this.breakableCollider.enabled = true;
			}
			for (int i = 0; i < this.disableWhenBroken.Count; i++)
			{
				this.disableWhenBroken[i].gameObject.SetActive(true);
			}
			for (int j = 0; j < this.enableWhenBroken.Count; j++)
			{
				this.enableWhenBroken[j].gameObject.SetActive(false);
			}
		}
	}

	public bool IsHitValid(GameHitData hit)
	{
		return !this.brokenLocal && hit.hitTypeId == 0;
	}

	public void OnHit(GameHitData hit)
	{
		if (hit.hitTypeId == 0 && (int)this.gameEntity.GetState() != 1)
		{
			this.gameEntity.RequestState(this.gameEntity.id, 1L);
			GameEntity gameEntity = this.gameEntity.manager.GetGameEntity(hit.hitByEntityId);
			if (gameEntity != null && gameEntity.IsHeldByLocalPlayer())
			{
				PlayerGameEvents.MiscEvent("GRSmashBreakable", 1);
			}
		}
	}

	public GameEntity gameEntity;

	public List<Transform> enableWhenBroken;

	public List<Transform> disableWhenBroken;

	public Collider breakableCollider;

	public bool holdsRandomItem = true;

	public Transform itemSpawnLocation;

	public GRBreakableItemSpawnConfig itemSpawnProbability;

	public AudioSource audioSource;

	public AudioClip breakSound;

	public float breakSoundVolume;

	private bool brokenLocal;

	public enum BreakableState
	{
		Unbroken,
		Broken
	}
}
