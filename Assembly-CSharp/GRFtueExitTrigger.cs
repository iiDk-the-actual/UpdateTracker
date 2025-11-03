using System;
using UnityEngine;

public class GRFtueExitTrigger : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		this.startTime = Time.time;
		this.ftueObject.InterruptWaitingTimer();
		this.ftueObject.playerLight.GetComponentInChildren<Light>().intensity = 0.25f;
	}

	private void Update()
	{
		if (this.startTime > 0f && Time.time - this.startTime > this.delayTime)
		{
			this.ftueObject.ChangeState(GRFirstTimeUserExperience.TransitionState.Flicker);
			this.startTime = -1f;
		}
	}

	public GRFirstTimeUserExperience ftueObject;

	public float delayTime = 5f;

	private float startTime = -1f;
}
