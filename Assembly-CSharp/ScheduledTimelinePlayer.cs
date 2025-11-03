using System;
using UnityEngine;
using UnityEngine.Playables;

public class ScheduledTimelinePlayer : MonoBehaviour
{
	protected void OnEnable()
	{
		this.scheduledEventID = BetterDayNightManager.RegisterScheduledEvent(this.eventHour, new Action(this.HandleScheduledEvent));
	}

	protected void OnDisable()
	{
		BetterDayNightManager.UnregisterScheduledEvent(this.scheduledEventID);
	}

	private void HandleScheduledEvent()
	{
		this.timeline.Play();
	}

	public PlayableDirector timeline;

	public int eventHour = 7;

	private int scheduledEventID;
}
