using System;
using UnityEngine.Events;

public class GorillaTriggerBoxEvent : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		if (this.onBoxTriggered != null)
		{
			this.onBoxTriggered.Invoke();
		}
	}

	public UnityEvent onBoxTriggered;
}
