using System;

public interface IGameHittable
{
	bool IsHitValid(GameHitData hit);

	void OnHit(GameHitData hit);
}
