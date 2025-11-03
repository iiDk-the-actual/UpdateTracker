using System;
using UnityEngine;

public class MonkeVoteProximityTrigger : GorillaTriggerBox
{
	public event Action OnEnter;

	public bool isPlayerNearby { get; private set; }

	public override void OnBoxTriggered()
	{
		this.isPlayerNearby = true;
		if (this.triggerTime + this.retriggerDelay < Time.unscaledTime)
		{
			this.triggerTime = Time.unscaledTime;
			Action onEnter = this.OnEnter;
			if (onEnter == null)
			{
				return;
			}
			onEnter();
		}
	}

	public override void OnBoxExited()
	{
		this.isPlayerNearby = false;
	}

	private float triggerTime = float.MinValue;

	private float retriggerDelay = 0.25f;
}
