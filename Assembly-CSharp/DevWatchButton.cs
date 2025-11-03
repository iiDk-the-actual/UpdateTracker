using System;
using UnityEngine;
using UnityEngine.Events;

public class DevWatchButton : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		this.SearchEvent.Invoke();
	}

	public UnityEvent SearchEvent = new UnityEvent();
}
