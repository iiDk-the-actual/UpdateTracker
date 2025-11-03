using System;

public interface IGRSummoningEntity
{
	void OnSummonedEntityInit(GameEntity entity);

	void OnSummonedEntityDestroy(GameEntity entity);
}
