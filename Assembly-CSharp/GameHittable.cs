using System;
using System.Collections.Generic;
using UnityEngine;

public class GameHittable : MonoBehaviour
{
	private void Awake()
	{
		this.components = new List<IGameHittable>(1);
		base.GetComponentsInChildren<IGameHittable>(this.components);
	}

	public void RequestHit(GameHitData hitData)
	{
		hitData.hitEntityId = this.gameEntity.id;
		this.gameEntity.manager.RequestHit(hitData);
	}

	public void ApplyHit(GameHitData hitData)
	{
		for (int i = 0; i < this.components.Count; i++)
		{
			this.components[i].OnHit(hitData);
		}
		GameHitter component = this.gameEntity.manager.GetGameEntity(hitData.hitByEntityId).GetComponent<GameHitter>();
		if (component != null)
		{
			component.ApplyHit(hitData);
		}
	}

	public bool IsHitValid(GameHitData hitData)
	{
		for (int i = 0; i < this.components.Count; i++)
		{
			if (!this.components[i].IsHitValid(hitData))
			{
				return false;
			}
		}
		return true;
	}

	public GameEntity gameEntity;

	private List<IGameHittable> components;
}
