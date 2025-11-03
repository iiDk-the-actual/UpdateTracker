using System;
using UnityEngine;

public class TriggerEventNotifier : MonoBehaviour
{
	public event TriggerEventNotifier.TriggerEvent TriggerEnterEvent;

	public event TriggerEventNotifier.TriggerEvent TriggerExitEvent;

	private void OnTriggerEnter(Collider other)
	{
		TriggerEventNotifier.TriggerEvent triggerEnterEvent = this.TriggerEnterEvent;
		if (triggerEnterEvent == null)
		{
			return;
		}
		triggerEnterEvent(this, other);
	}

	private void OnTriggerExit(Collider other)
	{
		TriggerEventNotifier.TriggerEvent triggerExitEvent = this.TriggerExitEvent;
		if (triggerExitEvent == null)
		{
			return;
		}
		triggerExitEvent(this, other);
	}

	[HideInInspector]
	public int maskIndex;

	public delegate void TriggerEvent(TriggerEventNotifier notifier, Collider collider);
}
