using System;
using System.Collections.Generic;
using UnityEngine;

public class GRDebugUpgradeKiosk : MonoBehaviour
{
	public void Init(GhostReactorManager grManager, GhostReactor reactor)
	{
		this.grManager = grManager;
		this.reactor = reactor;
	}

	private void Start()
	{
	}

	public void OnButtonSpawnClub()
	{
		this.OnButtonSpawnEntity("GhostReactorToolClub", this.toolSpawnNode);
	}

	public void OnButtonSpawnCollector()
	{
		this.OnButtonSpawnEntity("GhostReactorToolCollector", this.toolSpawnNode);
	}

	public void OnButtonSpawnLantern()
	{
		this.OnButtonSpawnEntity("GhostReactorToolLantern", this.toolSpawnNode);
	}

	public void OnButtonSpawnFlash()
	{
		this.OnButtonSpawnEntity("GhostReactorToolFlash", this.toolSpawnNode);
	}

	public void OnButtonSpawnShieldGun()
	{
		this.OnButtonSpawnEntity("GhostReactorToolShieldGun", this.toolSpawnNode);
	}

	public void OnButtonSpawnRevive()
	{
		this.OnButtonSpawnEntity("GhostReactorToolRevive", this.toolSpawnNode);
	}

	public void OnButtonSpawnDirectionalShield()
	{
		this.OnButtonSpawnEntity("GhostReactorToolDirectionalShield", this.toolSpawnNode);
	}

	public void OnButtonKillAllEnemies()
	{
		this.KillAllEnemies();
	}

	public void OnButtonSpawnPest()
	{
		this.OnButtonSpawnEntity("GhostReactorEnemyPest", this.enemySpawnNode);
	}

	public void OnButtonSpawnChaser()
	{
		this.OnButtonSpawnEntity("GhostReactorEnemyChaser", this.enemySpawnNode);
	}

	public void OnButtonSpawnPhantom()
	{
		this.OnButtonSpawnEntity("GhostReactorEnemyPhantom", this.enemySpawnNode);
	}

	public void OnButtonSpawnRanged()
	{
		this.OnButtonSpawnEntity("GhostReactorEnemyRanged", this.enemySpawnNode);
	}

	public void OnButtonSpawnSummoner()
	{
		this.OnButtonSpawnEntity("GhostReactorEnemySummoner", this.enemySpawnNode);
	}

	public void OnButtonSpawnIceRanged()
	{
		this.OnButtonSpawnEntity("GhostReactorEnemyRangedIce", this.enemySpawnNode);
	}

	public void OnButtonSpawnUpgEff1()
	{
		this.OnButtonSpawnEntity("GRUPowerEff1", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgEff2()
	{
		this.OnButtonSpawnEntity("GRUPowerEff2", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgEff3()
	{
		this.OnButtonSpawnEntity("GRUPowerEff3", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgBatonDmg1()
	{
		this.OnButtonSpawnEntity("GRUBatonDamage1", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgBatonDmg2()
	{
		this.OnButtonSpawnEntity("GRUBatonDamage2", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgBatonDmg3()
	{
		this.OnButtonSpawnEntity("GRUBatonDamage3", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgEfficiency1()
	{
		this.OnButtonSpawnEntity("GRUPowerEff1", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgEfficiency2()
	{
		this.OnButtonSpawnEntity("GRUPowerEff2", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnUpgEfficiency3()
	{
		this.OnButtonSpawnEntity("GRUPowerEff3", this.upgradeSpawnNode);
	}

	public void OnButtonSpawnChaosSeed()
	{
		this.OnButtonSpawnEntity("GhostReactorCollectibleSentientCore", this.enemySpawnNode);
	}

	public void OnButtonSpawnEntity(string entityName, Transform location)
	{
		if (location == null)
		{
			return;
		}
		Debug.Log("GRDebugUpgradeKiosk attempting to spawn " + entityName);
		int staticHash = entityName.GetStaticHash();
		GameEntityId gameEntityId = this.grManager.gameEntityManager.RequestCreateItem(staticHash, location.position, Quaternion.identity, 0L);
		GameAgent component = this.grManager.gameEntityManager.GetGameEntity(gameEntityId).gameObject.GetComponent<GameAgent>();
		if (component != null)
		{
			if (entityName.Contains("enemy", StringComparison.OrdinalIgnoreCase))
			{
				GhostReactorManager.entityDebugEnabled = true;
			}
			this.spawnedEntities.Add(gameEntityId);
			component.ApplyDestination(location.position);
			return;
		}
		Debug.Log("GRDebugUpgradeKiosk failed to spawn " + entityName);
	}

	public void KillAllEnemies()
	{
		foreach (GameEntityId gameEntityId in this.spawnedEntities)
		{
			this.grManager.gameEntityManager.RequestDestroyItem(gameEntityId);
		}
		this.spawnedEntities.Clear();
	}

	public Transform upgradeSpawnNode;

	public Transform toolSpawnNode;

	public Transform enemySpawnNode;

	private GhostReactorManager grManager;

	private GhostReactor reactor;

	private List<GameEntityId> spawnedEntities = new List<GameEntityId>();
}
