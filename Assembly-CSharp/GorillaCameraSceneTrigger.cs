using System;
using UnityEngine;

public class GorillaCameraSceneTrigger : MonoBehaviour
{
	public void ChangeScene(GorillaCameraTriggerIndex triggerLeft)
	{
		if (triggerLeft == this.currentSceneTrigger || this.currentSceneTrigger == null)
		{
			if (this.mostRecentSceneTrigger != this.currentSceneTrigger)
			{
				this.sceneCamera.SetSceneCamera(this.mostRecentSceneTrigger.sceneTriggerIndex);
				this.currentSceneTrigger = this.mostRecentSceneTrigger;
				return;
			}
			this.currentSceneTrigger = null;
		}
	}

	public GorillaSceneCamera sceneCamera;

	public GorillaCameraTriggerIndex currentSceneTrigger;

	public GorillaCameraTriggerIndex mostRecentSceneTrigger;
}
