using System;
using UnityEngine;
using UnityEngine.Events;

public class TimeEvent : MonoBehaviour
{
	protected void StartEvent()
	{
		this._ongoing = true;
		UnityEvent unityEvent = this.onEventStart;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	protected void StopEvent()
	{
		this._ongoing = false;
		UnityEvent unityEvent = this.onEventStop;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	public UnityEvent onEventStart;

	public UnityEvent onEventStop;

	[SerializeField]
	protected bool _ongoing;
}
