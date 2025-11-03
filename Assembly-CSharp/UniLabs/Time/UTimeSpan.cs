using System;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace UniLabs.Time
{
	[JsonObject(MemberSerialization.OptIn)]
	[Serializable]
	public class UTimeSpan : ISerializationCallbackReceiver, IComparable<UTimeSpan>, IComparable<TimeSpan>
	{
		[JsonProperty("TimeSpan")]
		public TimeSpan TimeSpan { get; set; }

		[JsonConstructor]
		public UTimeSpan()
		{
			this.TimeSpan = TimeSpan.Zero;
		}

		public UTimeSpan(TimeSpan timeSpan)
		{
			this.TimeSpan = timeSpan;
		}

		public UTimeSpan(long ticks)
			: this(new TimeSpan(ticks))
		{
		}

		public UTimeSpan(int hours, int minutes, int seconds)
			: this(new TimeSpan(hours, minutes, seconds))
		{
		}

		public UTimeSpan(int days, int hours, int minutes, int seconds)
			: this(new TimeSpan(days, hours, minutes, seconds))
		{
		}

		public UTimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
			: this(new TimeSpan(days, hours, minutes, seconds, milliseconds))
		{
		}

		public static implicit operator TimeSpan(UTimeSpan uTimeSpan)
		{
			if (uTimeSpan == null)
			{
				return TimeSpan.Zero;
			}
			return uTimeSpan.TimeSpan;
		}

		public static implicit operator UTimeSpan(TimeSpan timeSpan)
		{
			return new UTimeSpan(timeSpan);
		}

		public int CompareTo(TimeSpan other)
		{
			return this.TimeSpan.CompareTo(other);
		}

		public int CompareTo(UTimeSpan other)
		{
			if (this == other)
			{
				return 0;
			}
			if (other == null)
			{
				return 1;
			}
			return this.TimeSpan.CompareTo(other.TimeSpan);
		}

		protected bool Equals(UTimeSpan other)
		{
			return this.TimeSpan.Equals(other.TimeSpan);
		}

		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((UTimeSpan)obj)));
		}

		public override int GetHashCode()
		{
			return this.TimeSpan.GetHashCode();
		}

		public void OnAfterDeserialize()
		{
			TimeSpan timeSpan;
			this.TimeSpan = (TimeSpan.TryParse(this._TimeSpan, CultureInfo.InvariantCulture, out timeSpan) ? timeSpan : TimeSpan.Zero);
		}

		public void OnBeforeSerialize()
		{
			this._TimeSpan = this.TimeSpan.ToString();
		}

		[OnSerializing]
		internal void OnSerializingMethod(StreamingContext context)
		{
			this.OnBeforeSerialize();
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			this.OnAfterDeserialize();
		}

		[HideInInspector]
		[SerializeField]
		private string _TimeSpan;
	}
}
