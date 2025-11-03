using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(5555)]
public class ZoneGraph : MonoBehaviour
{
	public static ZoneGraph Instance
	{
		get
		{
			return ZoneGraph.gGraph;
		}
	}

	public static ZoneDef ColliderToZoneDef(BoxCollider collider)
	{
		if (!(collider == null))
		{
			return ZoneGraph.gGraph._colliderToZoneDef[collider];
		}
		return null;
	}

	public static ZoneNode ColliderToNode(BoxCollider collider)
	{
		if (!(collider == null))
		{
			return ZoneGraph.gGraph._colliderToNode[collider];
		}
		return ZoneNode.Null;
	}

	private void Awake()
	{
		if (ZoneGraph.gGraph != null && ZoneGraph.gGraph != this)
		{
			Object.Destroy(this);
		}
		else
		{
			ZoneGraph.gGraph = this;
		}
		this.CompileColliderMaps(this._zoneDefs);
	}

	public void CheckCompiledMaps()
	{
		if (this._compiledGraph)
		{
			return;
		}
		this.CompileColliderMaps(this._zoneDefs);
	}

	private void CompileColliderMaps(ZoneDef[] zones)
	{
		foreach (ZoneDef zoneDef in zones)
		{
			for (int j = 0; j < zoneDef.colliders.Length; j++)
			{
				BoxCollider boxCollider = zoneDef.colliders[j];
				if (!(boxCollider == null))
				{
					this._colliderToZoneDef[boxCollider] = zoneDef;
				}
			}
		}
		for (int k = 0; k < this._colliders.Length; k++)
		{
			BoxCollider boxCollider2 = this._colliders[k];
			if (!(boxCollider2 == null))
			{
				this._colliderToNode[boxCollider2] = this._nodes[k];
			}
		}
		this._compiledGraph = true;
	}

	public static int Compare(ZoneDef x, ZoneDef y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return 1;
		}
		if (y == null)
		{
			return -1;
		}
		int num = (int)x.zoneId;
		int num2 = num.CompareTo((int)y.zoneId);
		if (num2 == 0)
		{
			num = (int)x.subZoneId;
			num2 = num.CompareTo((int)y.subZoneId);
		}
		return num2;
	}

	public static void Register(ZoneEntity entity)
	{
		if (ZoneGraph.gGraph == null)
		{
			ZoneGraph.gGraph = Object.FindFirstObjectByType<ZoneGraph>();
		}
		if (!ZoneGraph.gGraph._entityList.Contains(entity))
		{
			ZoneGraph.gGraph._entityList.Add(entity);
		}
	}

	public static void Unregister(ZoneEntity entity)
	{
		ZoneGraph.gGraph._entityList.Remove(entity);
	}

	[SerializeField]
	private ZoneDef[] _zoneDefs = new ZoneDef[0];

	[SerializeField]
	private BoxCollider[] _colliders = new BoxCollider[0];

	[SerializeField]
	private ZoneNode[] _nodes = new ZoneNode[0];

	[Space]
	[NonSerialized]
	private Dictionary<BoxCollider, ZoneDef> _colliderToZoneDef = new Dictionary<BoxCollider, ZoneDef>(64);

	[Space]
	[NonSerialized]
	private Dictionary<BoxCollider, ZoneNode> _colliderToNode = new Dictionary<BoxCollider, ZoneNode>(64);

	[Space]
	[NonSerialized]
	private List<ZoneEntity> _entityList = new List<ZoneEntity>(16);

	private static ZoneGraph gGraph;

	private bool _compiledGraph;
}
