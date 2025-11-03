using System;
using Newtonsoft.Json;
using UnityEngine;

namespace UniLabs.Time
{
	[JsonObject(MemberSerialization.OptIn)]
	[Serializable]
	public class UTimeSpanRange
	{
		public TimeSpan Start
		{
			get
			{
				return this._Start;
			}
			set
			{
				this._Start = value;
			}
		}

		public TimeSpan End
		{
			get
			{
				return this._End;
			}
			set
			{
				this._End = value;
			}
		}

		public TimeSpan Duration
		{
			get
			{
				return this.End - this.Start;
			}
		}

		public bool IsInRange(TimeSpan time)
		{
			return time >= this.Start && time <= this.End;
		}

		[JsonConstructor]
		public UTimeSpanRange()
		{
		}

		public UTimeSpanRange(TimeSpan start)
		{
			this._Start = start;
			this._End = start;
		}

		public UTimeSpanRange(TimeSpan start, TimeSpan end)
		{
			this._Start = start;
			this._End = end;
		}

		private void OnStartChanged()
		{
			if (this._Start.CompareTo(this._End) > 0)
			{
				this._End.TimeSpan = this._Start.TimeSpan;
			}
		}

		private void OnEndChanged()
		{
			if (this._End.CompareTo(this._Start) < 0)
			{
				this._Start.TimeSpan = this._End.TimeSpan;
			}
		}

		[JsonProperty("Start")]
		[SerializeField]
		private UTimeSpan _Start;

		[JsonProperty("End")]
		[SerializeField]
		private UTimeSpan _End;
	}
}
