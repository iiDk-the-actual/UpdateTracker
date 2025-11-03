using System;
using System.Collections.Generic;
using UnityEngine;

public class ZoneBasedGameObjectActivator : MonoBehaviour
{
	private void OnEnable()
	{
		ZoneManagement.OnZoneChange += this.ZoneManagement_OnZoneChange;
	}

	private void OnDisable()
	{
		ZoneManagement.OnZoneChange -= this.ZoneManagement_OnZoneChange;
	}

	private void ZoneManagement_OnZoneChange(ZoneData[] zoneData)
	{
		HashSet<GTZone> hashSet = new HashSet<GTZone>(this.zones);
		bool flag = false;
		for (int i = 0; i < zoneData.Length; i++)
		{
			flag |= zoneData[i].active && hashSet.Contains(zoneData[i].zone);
		}
		for (int j = 0; j < this.gameObjects.Length; j++)
		{
			this.gameObjects[j].SetActive(flag);
		}
	}

	[SerializeField]
	private GTZone[] zones;

	[SerializeField]
	private GameObject[] gameObjects;
}
