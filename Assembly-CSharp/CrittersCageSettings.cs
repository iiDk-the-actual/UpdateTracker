using System;
using UnityEngine;

public class CrittersCageSettings : CrittersActorSettings
{
	public override void UpdateActorSettings()
	{
		base.UpdateActorSettings();
		CrittersCage crittersCage = (CrittersCage)this.parentActor;
		crittersCage.cagePosition = this.cagePoint;
		crittersCage.grabPosition = this.grabPoint;
	}

	public Transform cagePoint;

	public Transform grabPoint;
}
