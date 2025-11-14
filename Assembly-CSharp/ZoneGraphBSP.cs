using System;
using UnityEngine;

public class ZoneGraphBSP : MonoBehaviour
{
	public static ZoneGraphBSP Instance { get; private set; }

	private void Awake()
	{
		if (ZoneGraphBSP.Instance == null)
		{
			ZoneGraphBSP.Instance = this;
			return;
		}
		Object.Destroy(this);
	}

	public void Preprocess()
	{
		BoxCollider[] componentsInChildren = base.GetComponentsInChildren<BoxCollider>(true);
		if (componentsInChildren != null)
		{
			foreach (BoxCollider boxCollider in componentsInChildren)
			{
				if (boxCollider.transform.GetComponent<ZoneDef>() != null)
				{
					Object.Destroy(boxCollider);
				}
				else
				{
					Object.Destroy(boxCollider.gameObject);
				}
			}
		}
	}

	public void CompileBSP()
	{
		ZoneDef[] componentsInChildren = base.gameObject.GetComponentsInChildren<ZoneDef>();
		this.bspTree = BSPTreeBuilder.BuildTree(componentsInChildren);
		if (this.bspTree != null && this.bspTree.nodes != null)
		{
			Debug.Log(string.Format("BSP Tree compiled with {0} zones, {1} nodes", componentsInChildren.Length, this.bspTree.nodes.Length));
			return;
		}
		Debug.Log("BSP Tree compilation failed - no zones found");
	}

	public ZoneDef FindZoneAtPoint(Vector3 worldPoint)
	{
		SerializableBSPTree serializableBSPTree = this.bspTree;
		if (serializableBSPTree == null)
		{
			return null;
		}
		return serializableBSPTree.FindZone(worldPoint);
	}

	public bool IsPointInAnyZone(Vector3 worldPoint)
	{
		return this.FindZoneAtPoint(worldPoint) != null;
	}

	public bool HasCompiledTree()
	{
		return this.bspTree != null && this.bspTree.nodes != null && this.bspTree.nodes.Length != 0;
	}

	public SerializableBSPTree GetBSPTree()
	{
		return this.bspTree;
	}

	[SerializeField]
	private SerializableBSPTree bspTree;
}
