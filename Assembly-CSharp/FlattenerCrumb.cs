using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

public class FlattenerCrumb : MonoBehaviour
{
	private void OnDisable()
	{
		for (int i = this.flattenerList.Count - 1; i >= 0; i--)
		{
			this.flattenerList[i].CrumbDisabled();
		}
	}

	public void AddFlattenerReference(ObjectHierarchyFlattener flattener)
	{
		this.flattenerList.AddIfNew(flattener);
	}

	[DebugReadout]
	private List<ObjectHierarchyFlattener> flattenerList = new List<ObjectHierarchyFlattener>();
}
