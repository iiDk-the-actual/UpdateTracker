using System;
using System.Collections.Generic;
using UnityEngine;

public class BSPZoneData : MonoBehaviour
{
	public int Priority
	{
		get
		{
			return this.priority;
		}
	}

	public string ZoneName
	{
		get
		{
			return base.gameObject.name;
		}
	}

	[SerializeField]
	private int priority;

	[NonSerialized]
	public List<BoxCollider> boxList;
}
