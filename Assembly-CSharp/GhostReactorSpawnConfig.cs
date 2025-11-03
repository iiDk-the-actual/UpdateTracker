using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostReactorSpawnConfig", menuName = "ScriptableObjects/GhostReactorSpawnConfig")]
public class GhostReactorSpawnConfig : ScriptableObject
{
	public List<GhostReactorSpawnConfig.EntitySpawnGroup> entitySpawnGroups;

	public enum SpawnPointType
	{
		Enemy,
		Collectible,
		Barrier,
		HazardLiquid,
		Phantom,
		Pest,
		Crate,
		Tool,
		ChaosSeed,
		HazardTower,
		SpawnPointTypeCount
	}

	[Serializable]
	public struct EntitySpawnGroup
	{
		public GhostReactorSpawnConfig.SpawnPointType spawnPointType;

		public GameEntity entity;

		public GRBreakableItemSpawnConfig randomEntity;

		public int spawnCount;
	}
}
