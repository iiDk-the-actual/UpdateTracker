using System;
using System.Collections.Generic;
using GorillaNetworking;
using UnityEngine;

public class CritterIndex : ScriptableObject
{
	public CritterConfiguration this[int index]
	{
		get
		{
			if (index < 0 || index >= this.critterTypes.Count)
			{
				return null;
			}
			return this.critterTypes[index];
		}
	}

	private void OnEnable()
	{
		CritterIndex._instance = this;
	}

	public static Mesh GetMesh(CritterConfiguration.AnimalType animalType)
	{
		if (animalType < CritterConfiguration.AnimalType.Raccoon || animalType >= (CritterConfiguration.AnimalType)CritterIndex._instance.animalMeshes.Count)
		{
			return null;
		}
		return CritterIndex._instance.animalMeshes[(int)animalType].mesh;
	}

	public int GetRandomCritterType(CrittersRegion region = null)
	{
		return this.critterTypes.IndexOf(this.GetRandomConfiguration(region));
	}

	public CritterConfiguration GetRandomConfiguration(CrittersRegion region = null)
	{
		WeightedList<CritterConfiguration> validCritterTypes = this.GetValidCritterTypes(region);
		if (validCritterTypes.Count == 0)
		{
			return null;
		}
		return validCritterTypes.GetRandomItem();
	}

	public static DateTime GetCritterDateTime()
	{
		if (!GorillaComputer.instance)
		{
			return DateTime.UtcNow;
		}
		return GorillaComputer.instance.GetServerTime();
	}

	private WeightedList<CritterConfiguration> GetValidCritterTypes(CrittersRegion region = null)
	{
		this._currentConfigs.Clear();
		DateTime critterDateTime = CritterIndex.GetCritterDateTime();
		foreach (CritterConfiguration critterConfiguration in this.critterTypes)
		{
			if (critterConfiguration.DateConditionsMet(critterDateTime) && critterConfiguration.CanSpawn(region))
			{
				this._currentConfigs.Add(critterConfiguration, critterConfiguration.spawnWeight);
			}
		}
		return this._currentConfigs;
	}

	public List<CritterIndex.AnimalTypeMeshEntry> animalMeshes = new List<CritterIndex.AnimalTypeMeshEntry>();

	public List<CritterConfiguration> critterTypes;

	private WeightedList<CritterConfiguration> _currentConfigs = new WeightedList<CritterConfiguration>();

	private static CritterIndex _instance;

	[Serializable]
	public class AnimalTypeMeshEntry
	{
		public CritterConfiguration.AnimalType animalType;

		public Mesh mesh;
	}
}
