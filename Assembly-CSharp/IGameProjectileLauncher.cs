using System;
using UnityEngine;

public interface IGameProjectileLauncher
{
	void OnProjectileInit(GRRangedEnemyProjectile projectile)
	{
	}

	void OnProjectileHit(GRRangedEnemyProjectile projectile, Collision collision)
	{
	}
}
