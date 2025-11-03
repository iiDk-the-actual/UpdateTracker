using System;
using UnityEngine;

[Serializable]
public class CritterConfiguration
{
	public CritterConfiguration()
	{
		this.animalType = CritterConfiguration.AnimalType.UNKNOWN;
	}

	public int GetIndex()
	{
		return CrittersManager.instance.creatureIndex.critterTypes.IndexOf(this);
	}

	private bool RegionMatches(CrittersRegion region)
	{
		return !region || (region.Biome & this.biome) > (CrittersBiome)0;
	}

	private bool SpawnCriteriaMatches()
	{
		return !this.spawnCriteria || this.spawnCriteria.CanSpawn();
	}

	public bool CanSpawn()
	{
		return this.SpawnCriteriaMatches();
	}

	public bool CanSpawn(CrittersRegion region)
	{
		return this.RegionMatches(region) && this.SpawnCriteriaMatches();
	}

	public bool DateConditionsMet(DateTime utcDate)
	{
		return !this.dateLimit || this.dateLimit.MatchesDate(utcDate);
	}

	public bool ShouldDespawn()
	{
		return !this.SpawnCriteriaMatches();
	}

	public void ApplyToCreature(CrittersPawn crittersPawn)
	{
		this.behaviour.ApplyToCritter(crittersPawn);
		if (CrittersManager.instance.LocalAuthority())
		{
			this.ApplyVisualsTo(crittersPawn, true);
			return;
		}
		this.ApplyVisualsTo(crittersPawn, false);
	}

	private void ApplyVisualsTo(CrittersPawn critter, bool generateAppearance = true)
	{
		this.ApplyVisualsTo(critter.visuals, generateAppearance);
	}

	public void ApplyVisualsTo(CritterVisuals visuals, bool generateAppearance = true)
	{
		visuals.critterType = this.GetIndex();
		visuals.ApplyMesh(CritterIndex.GetMesh(this.animalType));
		visuals.ApplyMaterial(this.critterMat);
		if (generateAppearance)
		{
			visuals.SetAppearance(this.GenerateAppearance());
		}
	}

	public CritterAppearance GenerateAppearance()
	{
		string text = "";
		if (Random.value <= this.behaviour.GetTemplateValue<float>("hatChance"))
		{
			GameObject[] templateValue = this.behaviour.GetTemplateValue<GameObject[]>("hats");
			if (!templateValue.IsNullOrEmpty<GameObject>())
			{
				text = templateValue[Random.Range(0, templateValue.Length)].name;
			}
		}
		float templateValue2 = this.behaviour.GetTemplateValue<float>("minSize");
		float templateValue3 = this.behaviour.GetTemplateValue<float>("maxSize");
		float num = Random.Range(templateValue2, templateValue3);
		return new CritterAppearance(text, num);
	}

	public override string ToString()
	{
		return string.Format("{0} B:{1} C:{2}", this.critterName, this.behaviour, this.spawnCriteria);
	}

	[Tooltip("Basic internal description of critter.  Could be role, purpose, player experience, etc.")]
	public string internalDescription;

	public string critterName = "UNNAMED CRITTER";

	public CritterConfiguration.AnimalType animalType;

	public CritterTemplate behaviour;

	public CritterSpawnCriteria spawnCriteria;

	public RealWorldDateTimeWindow dateLimit;

	public CrittersBiome biome = CrittersBiome.Any;

	public float spawnWeight = 1f;

	public Material critterMat;

	public enum AnimalType
	{
		Raccoon,
		Cat,
		Bird,
		Goblin,
		Egg,
		UNKNOWN = -1
	}
}
