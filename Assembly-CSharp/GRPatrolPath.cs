using System;
using System.Collections.Generic;
using UnityEngine;

public class GRPatrolPath : MonoBehaviour
{
	private void Awake()
	{
		this.patrolNodes = new List<Transform>(base.transform.childCount);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			this.patrolNodes.Add(base.transform.GetChild(i));
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (this.patrolNodes == null || base.transform.childCount != this.patrolNodes.Count)
		{
			this.patrolNodes = new List<Transform>(base.transform.childCount);
			for (int i = 0; i < base.transform.childCount; i++)
			{
				this.patrolNodes.Add(base.transform.GetChild(i));
			}
		}
		if (this.patrolNodes != null)
		{
			for (int j = 0; j < this.patrolNodes.Count; j++)
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawCube(this.patrolNodes[j].transform.position, Vector3.one * 0.5f);
				if (j < this.patrolNodes.Count - 1)
				{
					Gizmos.DrawLine(this.patrolNodes[j].transform.position, this.patrolNodes[j + 1].transform.position);
				}
			}
		}
	}

	[NonSerialized]
	public List<Transform> patrolNodes;

	public int index;
}
