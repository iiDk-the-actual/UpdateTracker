using System;

namespace UniLabs.Time
{
	public static class TimeUnitExtensions
	{
		public static string ToShortString(this TimeUnit timeUnit)
		{
			string text;
			switch (timeUnit)
			{
			case TimeUnit.None:
				text = "";
				break;
			case TimeUnit.Milliseconds:
				text = "ms";
				break;
			case TimeUnit.Seconds:
				text = "s";
				break;
			case TimeUnit.Minutes:
				text = "m";
				break;
			case TimeUnit.Hours:
				text = "h";
				break;
			case TimeUnit.Days:
				text = "D";
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return text;
		}

		public static string ToSeparatorString(this TimeUnit timeUnit)
		{
			string text;
			switch (timeUnit)
			{
			case TimeUnit.None:
				text = "";
				break;
			case TimeUnit.Milliseconds:
				text = "";
				break;
			case TimeUnit.Seconds:
				text = ".";
				break;
			case TimeUnit.Minutes:
				text = ":";
				break;
			case TimeUnit.Hours:
				text = ":";
				break;
			case TimeUnit.Days:
				text = ".";
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return text;
		}

		public static double GetUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit)
		{
			int num;
			switch (timeUnit)
			{
			case TimeUnit.None:
				num = 0;
				break;
			case TimeUnit.Milliseconds:
				num = timeSpan.Milliseconds;
				break;
			case TimeUnit.Seconds:
				num = timeSpan.Seconds;
				break;
			case TimeUnit.Minutes:
				num = timeSpan.Minutes;
				break;
			case TimeUnit.Hours:
				num = timeSpan.Hours;
				break;
			case TimeUnit.Days:
				num = timeSpan.Days;
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return (double)num;
		}

		public static TimeSpan WithUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit, double value)
		{
			TimeSpan timeSpan2;
			switch (timeUnit)
			{
			case TimeUnit.None:
				timeSpan2 = timeSpan;
				break;
			case TimeUnit.Milliseconds:
				timeSpan2 = timeSpan.Add(TimeSpan.FromMilliseconds(value - (double)timeSpan.Milliseconds));
				break;
			case TimeUnit.Seconds:
				timeSpan2 = timeSpan.Add(TimeSpan.FromSeconds(value - (double)timeSpan.Seconds));
				break;
			case TimeUnit.Minutes:
				timeSpan2 = timeSpan.Add(TimeSpan.FromMinutes(value - (double)timeSpan.Minutes));
				break;
			case TimeUnit.Hours:
				timeSpan2 = timeSpan.Add(TimeSpan.FromHours(value - (double)timeSpan.Hours));
				break;
			case TimeUnit.Days:
				timeSpan2 = timeSpan.Add(TimeSpan.FromDays(value - (double)timeSpan.Days));
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return timeSpan2;
		}

		public static double GetLowestUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit)
		{
			double num;
			switch (timeUnit)
			{
			case TimeUnit.None:
				num = 0.0;
				break;
			case TimeUnit.Milliseconds:
				num = (double)timeSpan.Milliseconds;
				break;
			case TimeUnit.Seconds:
				num = new TimeSpan(0, 0, 0, timeSpan.Seconds, timeSpan.Milliseconds).TotalSeconds;
				break;
			case TimeUnit.Minutes:
				num = new TimeSpan(0, 0, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds).TotalMinutes;
				break;
			case TimeUnit.Hours:
				num = new TimeSpan(0, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds).TotalHours;
				break;
			case TimeUnit.Days:
				num = timeSpan.TotalDays;
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return num;
		}

		public static TimeSpan WithLowestUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit, double value)
		{
			TimeSpan timeSpan2;
			switch (timeUnit)
			{
			case TimeUnit.None:
				timeSpan2 = timeSpan;
				break;
			case TimeUnit.Milliseconds:
				timeSpan2 = new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, (int)value);
				break;
			case TimeUnit.Seconds:
				timeSpan2 = new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, 0).Add(TimeSpan.FromSeconds(value));
				break;
			case TimeUnit.Minutes:
				timeSpan2 = new TimeSpan(timeSpan.Days, timeSpan.Hours, 0, 0).Add(TimeSpan.FromMinutes(value));
				break;
			case TimeUnit.Hours:
				timeSpan2 = new TimeSpan(timeSpan.Days, 0, 0, 0).Add(TimeSpan.FromHours(value));
				break;
			case TimeUnit.Days:
				timeSpan2 = TimeSpan.FromDays(value);
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return timeSpan2;
		}

		public static double GetHighestUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit)
		{
			double num;
			switch (timeUnit)
			{
			case TimeUnit.None:
				num = 0.0;
				break;
			case TimeUnit.Milliseconds:
				num = timeSpan.TotalMilliseconds;
				break;
			case TimeUnit.Seconds:
				num = new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds).TotalSeconds;
				break;
			case TimeUnit.Minutes:
				num = new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, 0).TotalMinutes;
				break;
			case TimeUnit.Hours:
				num = new TimeSpan(timeSpan.Days, timeSpan.Hours, 0, 0).TotalHours;
				break;
			case TimeUnit.Days:
				num = (double)timeSpan.Days;
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return num;
		}

		public static TimeSpan WithHighestUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit, double value)
		{
			TimeSpan timeSpan2;
			switch (timeUnit)
			{
			case TimeUnit.None:
				timeSpan2 = timeSpan;
				break;
			case TimeUnit.Milliseconds:
				timeSpan2 = TimeSpan.FromMilliseconds(value);
				break;
			case TimeUnit.Seconds:
				timeSpan2 = new TimeSpan(0, 0, 0, 0, timeSpan.Milliseconds).Add(TimeSpan.FromSeconds(value));
				break;
			case TimeUnit.Minutes:
				timeSpan2 = new TimeSpan(0, 0, 0, timeSpan.Seconds, timeSpan.Milliseconds).Add(TimeSpan.FromMinutes(value));
				break;
			case TimeUnit.Hours:
				timeSpan2 = new TimeSpan(0, 0, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds).Add(TimeSpan.FromHours(value));
				break;
			case TimeUnit.Days:
				timeSpan2 = new TimeSpan(0, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds).Add(TimeSpan.FromDays(value));
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return timeSpan2;
		}

		public static double GetSingleUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit)
		{
			double num;
			switch (timeUnit)
			{
			case TimeUnit.Milliseconds:
				num = timeSpan.TotalMilliseconds;
				break;
			case TimeUnit.Seconds:
				num = timeSpan.TotalSeconds;
				break;
			case TimeUnit.Minutes:
				num = timeSpan.TotalMinutes;
				break;
			case TimeUnit.Hours:
				num = timeSpan.TotalHours;
				break;
			case TimeUnit.Days:
				num = timeSpan.TotalDays;
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return num;
		}

		public static TimeSpan FromSingleUnitValue(this TimeSpan timeSpan, TimeUnit timeUnit, double value)
		{
			TimeSpan timeSpan2;
			switch (timeUnit)
			{
			case TimeUnit.None:
				timeSpan2 = TimeSpan.Zero;
				break;
			case TimeUnit.Milliseconds:
				timeSpan2 = TimeSpan.FromMilliseconds(value);
				break;
			case TimeUnit.Seconds:
				timeSpan2 = TimeSpan.FromSeconds(value);
				break;
			case TimeUnit.Minutes:
				timeSpan2 = TimeSpan.FromMinutes(value);
				break;
			case TimeUnit.Hours:
				timeSpan2 = TimeSpan.FromHours(value);
				break;
			case TimeUnit.Days:
				timeSpan2 = TimeSpan.FromDays(value);
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return timeSpan2;
		}

		public static TimeSpan SnapToUnit(this TimeSpan timeSpan, TimeUnit timeUnit)
		{
			TimeSpan timeSpan2;
			switch (timeUnit)
			{
			case TimeUnit.None:
				timeSpan2 = timeSpan;
				break;
			case TimeUnit.Milliseconds:
				timeSpan2 = timeSpan;
				break;
			case TimeUnit.Seconds:
				timeSpan2 = new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
				break;
			case TimeUnit.Minutes:
				timeSpan2 = new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, 0);
				break;
			case TimeUnit.Hours:
				timeSpan2 = new TimeSpan(timeSpan.Days, timeSpan.Hours, 0, 0);
				break;
			case TimeUnit.Days:
				timeSpan2 = new TimeSpan(timeSpan.Days, 0, 0, 0);
				break;
			default:
				throw new ArgumentOutOfRangeException("timeUnit", timeUnit, null);
			}
			return timeSpan2;
		}

		public delegate TimeSpan WithUnitValueDelegate(TimeSpan timeSpan, TimeUnit timeUnit, double value);

		public delegate double GetUnitValueDelegate(TimeSpan timeSpan, TimeUnit timeUnit);
	}
}
