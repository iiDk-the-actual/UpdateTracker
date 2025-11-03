using System;
using System.Collections.Generic;
using UnityEngine;

public class CrittersRegion : MonoBehaviour
{
	public static List<CrittersRegion> Regions
	{
		get
		{
			return CrittersRegion._regions;
		}
	}

	public int CritterCount
	{
		get
		{
			return this._critters.Count;
		}
	}

	public int ID { get; private set; }

	private void OnEnable()
	{
		CrittersRegion.RegisterRegion(this);
	}

	private void OnDisable()
	{
		CrittersRegion.UnregisterRegion(this);
	}

	private static void RegisterRegion(CrittersRegion region)
	{
		CrittersRegion._regionLookup[region.ID] = region;
		CrittersRegion._regions.Add(region);
	}

	private static void UnregisterRegion(CrittersRegion region)
	{
		CrittersRegion._regionLookup.Remove(region.ID);
		CrittersRegion._regions.Remove(region);
	}

	public static void AddCritterToRegion(CrittersPawn critter, int regionId)
	{
		CrittersRegion crittersRegion;
		if (CrittersRegion._regionLookup.TryGetValue(regionId, out crittersRegion))
		{
			crittersRegion.AddCritter(critter);
			return;
		}
		GTDev.LogError<string>(string.Format("Attempted to add critter to non-existing region {0}.", regionId), null);
	}

	public static void RemoveCritterFromRegion(CrittersPawn critter)
	{
		CrittersRegion crittersRegion;
		if (CrittersRegion._regionLookup.TryGetValue(critter.regionId, out crittersRegion))
		{
			crittersRegion.RemoveCritter(critter);
			return;
		}
		GTDev.LogError<string>(string.Format("Couldn't find region with id {0}", critter.regionId), null);
	}

	public void AddCritter(CrittersPawn pawn)
	{
		this._critters.Add(pawn);
	}

	public void RemoveCritter(CrittersPawn pawn)
	{
		this._critters.Remove(pawn);
	}

	public Vector3 GetSpawnPoint()
	{
		float num = this.scale / 2f;
		float num2 = base.transform.lossyScale.y * this.scale;
		Vector3 vector = base.transform.TransformPoint(new Vector3(Random.Range(-num, num), num, Random.Range(-num, num)));
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, -base.transform.up, out raycastHit, num2, -1, QueryTriggerInteraction.Ignore))
		{
			Debug.DrawLine(vector, raycastHit.point, Color.green, 5f);
			return raycastHit.point;
		}
		Debug.DrawLine(vector, vector - base.transform.up * num2, Color.red, 5f);
		return vector;
	}

	private static List<CrittersRegion> _regions = new List<CrittersRegion>();

	private static Dictionary<int, CrittersRegion> _regionLookup = new Dictionary<int, CrittersRegion>();

	public CrittersBiome Biome = CrittersBiome.Any;

	public int maxCritters = 10;

	public float scale = 10f;

	public List<CrittersPawn> _critters = new List<CrittersPawn>();
}
