using System;
using UnityEngine;

public class GorillaSetZoneTrigger : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		ZoneManagement.SetActiveZones(this.zones);
	}

	[SerializeField]
	private GTZone[] zones;
}
