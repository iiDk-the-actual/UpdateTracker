using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTag
{
	public static class GTTime
	{
		public static TimeZoneInfo timeZoneInfoLA { get; private set; }

		static GTTime()
		{
			GTTime._Init();
		}

		[RuntimeInitializeOnLoadMethod]
		private static void _Init()
		{
			try
			{
				GTTime.timeZoneInfoLA = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
			}
			catch
			{
				try
				{
					GTTime.timeZoneInfoLA = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
				}
				catch
				{
					Debug.LogError("[GTTime]  ERROR!!!  Constructor: Could not get United States Pacific Time Zone (Los Angeles) so UTC will be used instead.");
					GTTime.timeZoneInfoLA = TimeZoneInfo.Utc;
				}
			}
		}

		public static bool usingServerTime { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long GetServerStartupTimeAsMilliseconds()
		{
			return GorillaComputer.instance.startupMillis;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long GetDeviceStartupTimeAsMilliseconds()
		{
			return (long)(TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds - Time.realtimeSinceStartupAsDouble * 1000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long GetStartupTimeAsMilliseconds()
		{
			GTTime.usingServerTime = true;
			long num = 0L;
			if (GorillaComputer.hasInstance)
			{
				num = GTTime.GetServerStartupTimeAsMilliseconds();
			}
			if (num == 0L)
			{
				GTTime.usingServerTime = false;
				num = GTTime.GetDeviceStartupTimeAsMilliseconds();
			}
			return num;
		}

		public static long TimeAsMilliseconds()
		{
			return GTTime.GetStartupTimeAsMilliseconds() + (long)(Time.realtimeSinceStartupAsDouble * 1000.0);
		}

		public static double TimeAsDouble()
		{
			return (double)GTTime.GetStartupTimeAsMilliseconds() / 1000.0 + Time.realtimeSinceStartupAsDouble;
		}

		public static DateTime GetAAxiomDateTime()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GTTime.timeZoneInfoLA);
		}

		public static string GetAAxiomDateTimeAsStringForDisplay()
		{
			return GTTime.GetAAxiomDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
		}

		public static string GetAAxiomDateTimeAsStringForFilename()
		{
			return GTTime.GetAAxiomDateTime().ToString("yyyy-MM-dd_HH-mm-ss-fff");
		}

		public static long GetAAxiomDateTimeAsHumanReadableLong()
		{
			return long.Parse(GTTime.GetAAxiomDateTime().ToString("yyyyMMddHHmmssfff00"));
		}

		public static DateTime ConvertDateTimeHumanReadableLongToDateTime(long humanReadableLong)
		{
			return DateTime.ParseExact(humanReadableLong.ToString(), "yyyyMMddHHmmssfff'00'", CultureInfo.InvariantCulture);
		}

		private const string preLog = "[GTTime]  ";

		private const string preErr = "[GTTime]  ERROR!!!  ";
	}
}
