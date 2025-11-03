using System;
using System.Collections.Generic;
using GorillaNetworking;
using UnityEngine;

public class ServerTimeEvent : TimeEvent
{
	private void Awake()
	{
		this.eventTimes = new HashSet<ServerTimeEvent.EventTime>(this.times);
	}

	private void Update()
	{
		if (GorillaComputer.instance == null || Time.time - this.lastQueryTime < this.queryTime)
		{
			return;
		}
		ServerTimeEvent.EventTime eventTime = new ServerTimeEvent.EventTime(GorillaComputer.instance.GetServerTime().Hour, GorillaComputer.instance.GetServerTime().Minute);
		bool flag = this.eventTimes.Contains(eventTime);
		if (!this._ongoing && flag)
		{
			base.StartEvent();
		}
		if (this._ongoing && !flag)
		{
			base.StopEvent();
		}
		this.lastQueryTime = Time.time;
	}

	[SerializeField]
	private ServerTimeEvent.EventTime[] times;

	[SerializeField]
	private float queryTime = 60f;

	private float lastQueryTime;

	private HashSet<ServerTimeEvent.EventTime> eventTimes;

	[Serializable]
	public struct EventTime
	{
		public EventTime(int h, int m)
		{
			this.hour = h;
			this.minute = m;
		}

		public int hour;

		public int minute;
	}
}
