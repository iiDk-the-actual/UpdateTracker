using System;
using UnityEngine;

public class CrittersGrabberSettings : CrittersActorSettings
{
	public override void UpdateActorSettings()
	{
		base.UpdateActorSettings();
		CrittersGrabber crittersGrabber = (CrittersGrabber)this.parentActor;
		crittersGrabber.grabPosition = this._grabPosition;
		crittersGrabber.grabDistance = this._grabDistance;
	}

	public Transform _grabPosition;

	public float _grabDistance;
}
