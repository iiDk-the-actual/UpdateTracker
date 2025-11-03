using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnRegion<TItem, TRegion> : MonoBehaviour where TItem : Object where TRegion : SpawnRegion<TItem, TRegion>
{
	public static List<TRegion> Regions
	{
		get
		{
			return SpawnRegion<TItem, TRegion>._regions;
		}
	}

	public int MaxItems { get; private set; } = 10;

	private bool HasSpawnOrigins
	{
		get
		{
			Transform[] array = this.spawnOrigins;
			return array != null && array.Length != 0;
		}
	}

	public List<TItem> Items
	{
		get
		{
			return this._items;
		}
	}

	public int ItemCount
	{
		get
		{
			return this._items.Count;
		}
	}

	public int ID { get; private set; }

	private void OnEnable()
	{
		Transform[] array = this.spawnOrigins;
		this._useSpawnOrigins = array != null && array.Length != 0;
		this._testAgainstGeo = !this._useSpawnOrigins && this.geoTestPoint;
		if (this._testAgainstGeo && this._hitTestBuffer == null)
		{
			this._hitTestBuffer = new RaycastHit[20];
		}
		SpawnRegion<TItem, TRegion>.RegisterRegion((TRegion)((object)this));
	}

	private void OnDisable()
	{
		SpawnRegion<TItem, TRegion>.UnregisterRegion((TRegion)((object)this));
		foreach (TItem titem in this._items)
		{
			if (titem)
			{
				SpawnRegion<TItem, TRegion>._itemRegionLookup.Remove(titem);
			}
		}
		this._items.Clear();
	}

	private static void RegisterRegion(TRegion region)
	{
		SpawnRegion<TItem, TRegion>._regionLookup[region.ID] = region;
		SpawnRegion<TItem, TRegion>._regions.Add(region);
	}

	private static void UnregisterRegion(TRegion region)
	{
		SpawnRegion<TItem, TRegion>._regionLookup.Remove(region.ID);
		SpawnRegion<TItem, TRegion>._regions.Remove(region);
	}

	public static void AddItemToRegion(TItem item, int regionId)
	{
		TRegion tregion;
		if (SpawnRegion<TItem, TRegion>._regionLookup.TryGetValue(regionId, out tregion))
		{
			tregion.AddItem(item);
			return;
		}
		GTDev.LogError<string>(string.Format("Attempted to add item to non-existing region {0}.", regionId), null);
	}

	public static void RemoveItemFromRegion(TItem item)
	{
		int num;
		if (SpawnRegion<TItem, TRegion>._itemRegionLookup.TryGetValue(item, out num))
		{
			TRegion tregion;
			if (SpawnRegion<TItem, TRegion>._regionLookup.TryGetValue(num, out tregion))
			{
				tregion.RemoveItem(item);
				return;
			}
			GTDev.LogError<string>(string.Format("Couldn't find region with id {0}", num), null);
		}
	}

	public void AddItem(TItem item)
	{
		this._items.Add(item);
		SpawnRegion<TItem, TRegion>._itemRegionLookup[item] = this.ID;
	}

	public void RemoveItem(TItem item)
	{
		this._items.Remove(item);
		SpawnRegion<TItem, TRegion>._itemRegionLookup.Remove(item);
	}

	[return: TupleElementNames(new string[] { "isOnGround", "position", "normal" })]
	public ValueTuple<bool, Vector3, Vector3> GetSpawnPointWithNormal(int maxTries = 5)
	{
		for (int i = 0; i < maxTries; i++)
		{
			RaycastHit raycastHit;
			if (this.TryGetSpawnPoint(out raycastHit))
			{
				return new ValueTuple<bool, Vector3, Vector3>(true, raycastHit.point, raycastHit.normal);
			}
		}
		GTDev.LogError<string>(string.Format("Failed {0} times to find ground point in region {1} in scene {2}.", maxTries, this.ID, base.gameObject.scene.name), this, null);
		float num = this._scale / 2f;
		Vector3 vector = base.transform.TransformPoint(new Vector3(Random.Range(-num, num), num, Random.Range(-num, num)));
		return new ValueTuple<bool, Vector3, Vector3>(false, vector, Vector3.up);
	}

	private bool TryGetSpawnPoint(out RaycastHit spawnPoint)
	{
		float num = base.transform.lossyScale.y * this._scale;
		if (this._useSpawnOrigins)
		{
			Transform transform = this.spawnOrigins[Random.Range(0, this.spawnOrigins.Length)];
			Vector3 vector = transform.position;
			if (this.TryGetSpawnPoint(vector, Random.onUnitSphere, Mathf.Max(num, 100f), out spawnPoint))
			{
				return spawnPoint.normal.y > 0f || this.TryGetSpawnPoint(spawnPoint.point, Vector3.down, num, out spawnPoint);
			}
			Debug.LogError(string.Format("Unable to find spawn point originating from {0} [P:{1} N:{2}].", vector, spawnPoint.point, spawnPoint.normal), transform);
			spawnPoint = default(RaycastHit);
			return false;
		}
		else
		{
			float num2 = this._scale / 2f;
			Vector3 vector = base.transform.TransformPoint(new Vector3(Random.Range(-num2, num2), num2, Random.Range(-num2, num2)));
			if (this._testAgainstGeo && this.IsInsideGeo(vector))
			{
				spawnPoint = default(RaycastHit);
				return false;
			}
			return this.TryGetSpawnPoint(vector, Vector3.down, num, out spawnPoint);
		}
	}

	private bool TryGetSpawnPoint(Vector3 origin, Vector3 direction, float distance, out RaycastHit spawnPoint)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, direction, out raycastHit, distance, -1, QueryTriggerInteraction.Ignore))
		{
			Debug.DrawLine(origin, raycastHit.point, Color.green, 5f);
			spawnPoint = raycastHit;
			return true;
		}
		Debug.DrawLine(origin, origin + direction * distance, Color.red, 5f);
		spawnPoint = default(RaycastHit);
		return false;
	}

	private bool IsInsideGeo(Vector3 point)
	{
		Vector3 position = this.geoTestPoint.position;
		Vector3 vector = position - point;
		int num;
		int num2;
		for (;;)
		{
			num = Physics.RaycastNonAlloc(point, vector, this._hitTestBuffer, vector.magnitude, -1, QueryTriggerInteraction.Ignore);
			num2 = Physics.RaycastNonAlloc(position, -vector, this._hitTestBuffer, vector.magnitude, -1, QueryTriggerInteraction.Ignore);
			if (num < this._hitTestBuffer.Length && num2 < this._hitTestBuffer.Length)
			{
				break;
			}
			GTDev.LogError<string>(string.Format("{0} hit test buffer not large enough in {1} - expanding.", this, base.gameObject.scene.name), this, null);
			this._hitTestBuffer = new RaycastHit[this._hitTestBuffer.Length * 2];
		}
		bool flag = (num + num2) % 2 != 0;
		Debug.DrawLine(point, position, flag ? Color.red : Color.green, 5f);
		return flag;
	}

	private static List<TRegion> _regions = new List<TRegion>();

	private static Dictionary<int, TRegion> _regionLookup = new Dictionary<int, TRegion>();

	private static Dictionary<TItem, int> _itemRegionLookup = new Dictionary<TItem, int>();

	[SerializeField]
	private float _scale = 10f;

	[SerializeField]
	[Tooltip("If set, spawn points will be created via raycasts from one of these points.")]
	private Transform[] spawnOrigins;

	[Tooltip("If set, all spawn points will be tested against this transform to see if they're inside geo.  Ignored if spawn origins are configured.")]
	private Transform geoTestPoint;

	private List<TItem> _items = new List<TItem>();

	private bool _useSpawnOrigins;

	private bool _testAgainstGeo;

	private RaycastHit[] _hitTestBuffer;
}
