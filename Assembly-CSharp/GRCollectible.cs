using System;
using UnityEngine;

public class GRCollectible : MonoBehaviour, IGameEntityComponent
{
	private void Awake()
	{
	}

	public void OnEntityInit()
	{
		GameEntityManager manager = this.entity.manager;
		GameEntity gameEntity = manager.GetGameEntity(manager.GetEntityIdFromNetId((int)this.entity.createData));
		if (gameEntity != null)
		{
			GRCollectibleDispenser component = gameEntity.GetComponent<GRCollectibleDispenser>();
			if (component != null)
			{
				component.GetSpawnedCollectible(this);
			}
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	public void InvokeOnCollected()
	{
		Action onCollected = this.OnCollected;
		if (onCollected == null)
		{
			return;
		}
		onCollected();
	}

	public GameEntity entity;

	public int energyValue = 100;

	public ProgressionManager.CoreType type;

	public Action OnCollected;
}
