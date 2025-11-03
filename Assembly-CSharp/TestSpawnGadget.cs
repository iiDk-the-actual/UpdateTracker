using System;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnGadget : MonoBehaviour
{
	public void Spawn(GameEntityManager gameEntityManager)
	{
		SIUpgradeSet siupgradeSet = default(SIUpgradeSet);
		foreach (TestSpawnGadget.SpawnTypeWithUpgrades spawnTypeWithUpgrades in this.testSpawnList)
		{
			if (!(spawnTypeWithUpgrades.prefab == null))
			{
				siupgradeSet.Clear();
				foreach (SIUpgradeType siupgradeType in spawnTypeWithUpgrades.upgrades)
				{
					siupgradeSet.Add(siupgradeType);
				}
				this.SpawnGadgetBatch(gameEntityManager, spawnTypeWithUpgrades.prefab, siupgradeSet);
			}
		}
		if (!this.spawnAllGadgets)
		{
			return;
		}
		siupgradeSet.Clear();
		foreach (GameEntity gameEntity in gameEntityManager.tempFactoryItems)
		{
			if (!this.skipEntityList.Contains(gameEntity))
			{
				this.SpawnGadgetBatch(gameEntityManager, gameEntity, siupgradeSet);
			}
		}
	}

	private void SpawnGadgetBatch(GameEntityManager gameEntityManager, GameEntity entityToSpawn, SIUpgradeSet upgrades)
	{
		for (int i = 0; i < this.spawnBatchSize; i++)
		{
			gameEntityManager.RequestCreateItem(entityToSpawn.gameObject.name.GetStaticHash(), base.transform.position + Random.insideUnitSphere, base.transform.rotation, (long)upgrades.GetBits() << 32);
		}
	}

	public int spawnBatchSize = 4;

	public List<TestSpawnGadget.SpawnTypeWithUpgrades> testSpawnList = new List<TestSpawnGadget.SpawnTypeWithUpgrades>();

	public bool spawnAllGadgets;

	public List<GameEntity> skipEntityList = new List<GameEntity>();

	[Serializable]
	public struct SpawnTypeWithUpgrades
	{
		public GameEntity prefab;

		public SIUpgradeType[] upgrades;
	}
}
