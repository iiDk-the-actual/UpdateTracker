using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostReactorBreakableItemSpawnConfig", menuName = "ScriptableObjects/GhostReactorBreakableItemSpawnConfig")]
public class GRBreakableItemSpawnConfig : ScriptableObject
{
	public bool TryForRandomItem(GameEntity spawnFromEntity, out GameEntity entity, int sanity = 0)
	{
		GRBreakableItemSpawnConfig @override = this.GetOverride(spawnFromEntity);
		if (sanity <= 5 && @override != null)
		{
			return @override.TryForRandomItem(spawnFromEntity, out entity, sanity + 1);
		}
		if (sanity > 5)
		{
			Debug.LogError("Circular override loop");
		}
		if (Random.Range(0f, 1f) < this.spawnAnythingProbability)
		{
			float num = Random.Range(0f, this.precomputedItemTotalWeight);
			float num2 = 0f;
			for (int i = 0; i < this.perItemProbabilities.Count; i++)
			{
				num2 += this.perItemProbabilities[i].probability;
				if (num2 > num || i == this.perItemProbabilities.Count - 1)
				{
					entity = this.perItemProbabilities[i].entity;
					return true;
				}
			}
		}
		entity = null;
		return false;
	}

	public bool TryForRandomItem(GhostReactor reactor, ref SRand srand, out GameEntity entity, int sanity = 0)
	{
		GRBreakableItemSpawnConfig @override = this.GetOverride(reactor);
		if (sanity <= 5 && @override != null)
		{
			return @override.TryForRandomItem(reactor, ref srand, out entity, sanity + 1);
		}
		if (sanity > 5)
		{
			Debug.LogError("Circular override loop");
		}
		if (srand.NextFloat(0f, 1f) < this.spawnAnythingProbability)
		{
			float num = srand.NextFloat(0f, this.precomputedItemTotalWeight);
			float num2 = 0f;
			for (int i = 0; i < this.perItemProbabilities.Count; i++)
			{
				num2 += this.perItemProbabilities[i].probability;
				if (num2 > num || i == this.perItemProbabilities.Count - 1)
				{
					entity = this.perItemProbabilities[i].entity;
					return true;
				}
			}
		}
		entity = null;
		return false;
	}

	private GRBreakableItemSpawnConfig GetOverride(GameEntity entity)
	{
		GhostReactorManager ghostReactorManager = GhostReactorManager.Get(entity);
		if (ghostReactorManager == null)
		{
			return null;
		}
		return this.GetOverride(ghostReactorManager.reactor);
	}

	private GRBreakableItemSpawnConfig GetOverride(GhostReactor reactor)
	{
		if (reactor == null)
		{
			return null;
		}
		GhostReactorLevelGenConfig currLevelGenConfig = reactor.GetCurrLevelGenConfig();
		if (currLevelGenConfig == null || currLevelGenConfig.dropTableOverrides == null)
		{
			return null;
		}
		return currLevelGenConfig.dropTableOverrides.GetOverride(this);
	}

	private void OnValidate()
	{
		this.precomputedItemTotalWeight = 0f;
		for (int i = 0; i < this.perItemProbabilities.Count; i++)
		{
			this.precomputedItemTotalWeight += this.perItemProbabilities[i].probability;
		}
	}

	[SerializeField]
	[Range(0f, 1f)]
	public float spawnAnythingProbability = 0.2f;

	public List<GRBreakableItemSpawnConfig.ItemProbability> perItemProbabilities = new List<GRBreakableItemSpawnConfig.ItemProbability>();

	[SerializeField]
	[ReadOnly]
	private float precomputedItemTotalWeight;

	[Serializable]
	public struct ItemProbability
	{
		public GameEntity entity;

		public float probability;
	}
}
