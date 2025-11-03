using System;
using UnityEngine;

public class GRSummonedEntity : MonoBehaviour, IGameEntityComponent
{
	private void Awake()
	{
		this.entity = base.GetComponent<GameEntity>();
	}

	public void OnEntityInit()
	{
		this.summonerNetID = (int)this.entity.createData;
		if (this.summonerNetID != 0)
		{
			this.summoner = this.FindSummoner();
			if (this.summoner != null)
			{
				this.summoner.OnSummonedEntityInit(this.entity);
			}
		}
	}

	public int GetSummonerNetID()
	{
		return this.summonerNetID;
	}

	public void OnEntityDestroy()
	{
		if (this.summoner != null)
		{
			this.summoner.OnSummonedEntityDestroy(this.entity);
		}
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private IGRSummoningEntity FindSummoner()
	{
		if (this.summonerNetID != 0)
		{
			GameEntityManager gameEntityManager = GhostReactorManager.Get(this.entity).gameEntityManager;
			GameEntityId entityIdFromNetId = gameEntityManager.GetEntityIdFromNetId(this.summonerNetID);
			GameEntity gameEntity = gameEntityManager.GetGameEntity(entityIdFromNetId);
			if (gameEntity != null)
			{
				return gameEntity.GetComponent<IGRSummoningEntity>();
			}
		}
		return null;
	}

	private int summonerNetID;

	private GameEntity entity;

	private IGRSummoningEntity summoner;
}
