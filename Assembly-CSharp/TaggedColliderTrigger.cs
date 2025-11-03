using System;
using UnityEngine;
using UnityEngine.Events;

public class TaggedColliderTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag(this.tag))
		{
			return;
		}
		if (this._sinceLastEnter.HasElapsed(this.enterHysteresis, true))
		{
			UnityEvent<Collider> unityEvent = this.onEnter;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag(this.tag))
		{
			return;
		}
		if (this._sinceLastExit.HasElapsed(this.exitHysteresis, true))
		{
			UnityEvent<Collider> unityEvent = this.onExit;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(other);
		}
	}

	public new UnityTag tag;

	public UnityEvent<Collider> onEnter = new UnityEvent<Collider>();

	public UnityEvent<Collider> onExit = new UnityEvent<Collider>();

	public float enterHysteresis = 0.125f;

	public float exitHysteresis = 0.125f;

	private TimeSince _sinceLastEnter;

	private TimeSince _sinceLastExit;
}
