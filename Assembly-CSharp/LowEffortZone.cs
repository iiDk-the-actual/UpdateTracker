using System;
using UnityEngine;

public class LowEffortZone : GorillaTriggerBox
{
	private void Awake()
	{
		if (this.triggerOnAwake)
		{
			this.OnBoxTriggered();
		}
	}

	public override void OnBoxTriggered()
	{
		for (int i = 0; i < this.objectsToEnable.Length; i++)
		{
			if (this.objectsToEnable[i] != null)
			{
				this.objectsToEnable[i].SetActive(true);
			}
		}
		for (int j = 0; j < this.objectsToDisable.Length; j++)
		{
			if (this.objectsToDisable[j] != null)
			{
				this.objectsToDisable[j].SetActive(false);
			}
		}
	}

	public GameObject[] objectsToEnable;

	public GameObject[] objectsToDisable;

	public bool triggerOnAwake;
}
