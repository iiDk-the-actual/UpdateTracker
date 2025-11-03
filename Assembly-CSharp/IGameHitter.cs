using System;
using UnityEngine;

public interface IGameHitter
{
	void OnSuccessfulHit(GameHitData hit);

	void OnSuccessfulHitPlayer(GRPlayer player, Vector3 hitPosition)
	{
	}
}
