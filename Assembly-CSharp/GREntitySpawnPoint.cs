using System;
using UnityEngine;

public class GREntitySpawnPoint : MonoBehaviour
{
	public GhostReactorSpawnConfig.SpawnPointType spawnPointType;

	public GameEntity entity;

	public GRPatrolPath patrolPath;

	public bool applyScale;
}
