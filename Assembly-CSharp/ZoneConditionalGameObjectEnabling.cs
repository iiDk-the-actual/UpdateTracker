using System;
using UnityEngine;

public class ZoneConditionalGameObjectEnabling : MonoBehaviour
{
	private void Start()
	{
		this.OnZoneChanged();
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
	}

	private void OnDestroy()
	{
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
	}

	private void OnZoneChanged()
	{
		if (this.invisibleWhileLoaded)
		{
			if (this.gameObjects != null)
			{
				for (int i = 0; i < this.gameObjects.Length; i++)
				{
					this.gameObjects[i].SetActive(!ZoneManagement.IsInZone(this.zone));
				}
				return;
			}
		}
		else if (this.gameObjects != null)
		{
			for (int j = 0; j < this.gameObjects.Length; j++)
			{
				this.gameObjects[j].SetActive(ZoneManagement.IsInZone(this.zone));
			}
		}
	}

	[SerializeField]
	private GTZone zone;

	[SerializeField]
	private bool invisibleWhileLoaded;

	[SerializeField]
	private GameObject[] gameObjects;
}
