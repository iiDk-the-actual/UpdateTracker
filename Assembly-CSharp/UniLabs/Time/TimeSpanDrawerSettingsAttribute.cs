using System;
using System.Diagnostics;

namespace UniLabs.Time
{
	[Conditional("UNITY_EDITOR")]
	public class TimeSpanDrawerSettingsAttribute : Attribute
	{
		public TimeSpanDrawerSettingsAttribute()
		{
		}

		public TimeSpanDrawerSettingsAttribute(TimeUnit highestUnit, TimeUnit lowestUnit)
		{
			this.HighestUnit = highestUnit;
			this.LowestUnit = lowestUnit;
		}

		public TimeSpanDrawerSettingsAttribute(TimeUnit highestUnit, bool drawMilliseconds = false)
		{
			this.HighestUnit = highestUnit;
			this.LowestUnit = (drawMilliseconds ? TimeUnit.Milliseconds : TimeUnit.Seconds);
		}

		public TimeSpanDrawerSettingsAttribute(bool drawMilliseconds)
		{
			this.HighestUnit = TimeUnit.Days;
			this.LowestUnit = (drawMilliseconds ? TimeUnit.Milliseconds : TimeUnit.Seconds);
		}

		public TimeUnit HighestUnit = TimeUnit.Days;

		public TimeUnit LowestUnit = TimeUnit.Seconds;
	}
}
