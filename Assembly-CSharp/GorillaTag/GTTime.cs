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
			if (GTTime._isInitialized)
			{
				return;
			}
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
					TimeZoneInfo timeZoneInfo;
					if (GTTime._TryCreateCustomPST(out timeZoneInfo))
					{
						GTTime.timeZoneInfoLA = timeZoneInfo;
						Debug.Log("[GTTime]  _Init: Could not get US Pacific Time Zone, so using manual created Pacific time zone instead.");
					}
					else
					{
						Debug.LogError("[GTTime]  ERROR!!!  _Init: Could not get US Pacific Time Zone and manual Pacific time zone creation failed. Using UTC instead.");
						GTTime.timeZoneInfoLA = TimeZoneInfo.Utc;
					}
				}
			}
			finally
			{
				GTTime._isInitialized = true;
			}
		}

		private static bool _TryCreateCustomPST(out TimeZoneInfo out_tz)
		{
			TimeZoneInfo.AdjustmentRule[] array = new TimeZoneInfo.AdjustmentRule[] { TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2007, 1, 1), DateTime.MaxValue.Date, TimeSpan.FromHours(1.0), TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 2, DayOfWeek.Sunday), TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 11, 1, DayOfWeek.Sunday)) };
			bool flag;
			try
			{
				out_tz = TimeZoneInfo.CreateCustomTimeZone("Custom/America_Los_Angeles", TimeSpan.FromHours(-8.0), "(UTC-08:00) Pacific Time (US & Canada)", "Pacific Standard Time", "Pacific Daylight Time", array, false);
				flag = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("[GTTime]  ERROR!!!  _TryCreateCustomPST: Encountered exception: " + ex.Message);
				out_tz = null;
				flag = false;
			}
			return flag;
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

		private static bool _isInitialized;
	}
}
