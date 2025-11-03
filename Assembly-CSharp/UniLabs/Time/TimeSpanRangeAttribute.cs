using System;
using System.Diagnostics;

namespace UniLabs.Time
{
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class TimeSpanRangeAttribute : Attribute
	{
		public TimeSpanRangeAttribute(string maxGetter, bool inline = false, TimeUnit snappingUnit = TimeUnit.Seconds)
		{
			this.MaxGetter = maxGetter;
			this.SnappingUnit = snappingUnit;
			this.Inline = inline;
		}

		public TimeSpanRangeAttribute(string minGetter, string maxGetter, bool inline = false, TimeUnit snappingUnit = TimeUnit.Seconds)
		{
			this.MinGetter = minGetter;
			this.MaxGetter = maxGetter;
			this.SnappingUnit = snappingUnit;
			this.Inline = inline;
		}

		public string MinGetter;

		public string MaxGetter;

		public TimeUnit SnappingUnit;

		public bool Inline;

		public string DisableMinMaxIf;
	}
}
