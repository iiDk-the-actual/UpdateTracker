using System;
using UnityEngine;

public class ZoneBasedObject : MonoBehaviour
{
	public bool IsLocalPlayerInZone()
	{
		GTZone[] array = this.zones;
		for (int i = 0; i < array.Length; i++)
		{
			if (ZoneManagement.IsInZone(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static ZoneBasedObject SelectRandomEligible(ZoneBasedObject[] objects, string overrideChoice = "")
	{
		if (overrideChoice != "")
		{
			foreach (ZoneBasedObject zoneBasedObject in objects)
			{
				if (zoneBasedObject.gameObject.name == overrideChoice)
				{
					return zoneBasedObject;
				}
			}
		}
		ZoneBasedObject zoneBasedObject2 = null;
		int num = 0;
		foreach (ZoneBasedObject zoneBasedObject3 in objects)
		{
			if (zoneBasedObject3.gameObject.activeInHierarchy)
			{
				GTZone[] array = zoneBasedObject3.zones;
				for (int j = 0; j < array.Length; j++)
				{
					if (ZoneManagement.IsInZone(array[j]))
					{
						if (Random.Range(0, num) == 0)
						{
							zoneBasedObject2 = zoneBasedObject3;
						}
						num++;
						break;
					}
				}
			}
		}
		return zoneBasedObject2;
	}

	public GTZone[] zones;
}
