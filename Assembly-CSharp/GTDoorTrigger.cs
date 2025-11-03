using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class GTDoorTrigger : MonoBehaviour
{
	public int overlapCount
	{
		get
		{
			return this.overlappingColliders.Count;
		}
	}

	public bool TriggeredThisFrame
	{
		get
		{
			return this.lastTriggeredFrame == Time.frameCount;
		}
	}

	public void ValidateOverlappingColliders()
	{
		for (int i = this.overlappingColliders.Count - 1; i >= 0; i--)
		{
			if (this.overlappingColliders[i] == null || !this.overlappingColliders[i].gameObject.activeInHierarchy || !this.overlappingColliders[i].enabled)
			{
				this.overlappingColliders.RemoveAt(i);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!this.overlappingColliders.Contains(other))
		{
			this.overlappingColliders.Add(other);
		}
		this.lastTriggeredFrame = Time.frameCount;
		this.TriggeredEvent.Invoke();
		if (this.timeline != null && (this.timeline.time == 0.0 || this.timeline.time >= this.timeline.duration))
		{
			this.timeline.Play();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		this.overlappingColliders.Remove(other);
	}

	[Tooltip("Optional timeline to play to animate the thing getting activated, play sound, particles, etc...")]
	public PlayableDirector timeline;

	private int lastTriggeredFrame = -1;

	private List<Collider> overlappingColliders = new List<Collider>(20);

	internal UnityEvent TriggeredEvent = new UnityEvent();
}
