using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal
{
	internal class UserStats
	{
		static UserStats()
		{
			Api.LoadLibraryManually("viveport_api");
		}

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_IsReady")]
		internal static extern int IsReady(StatusCallback IsReadyCallback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_IsReady")]
		internal static extern int IsReady_64(StatusCallback IsReadyCallback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_DownloadStats")]
		internal static extern int DownloadStats(StatusCallback downloadStatsCallback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_DownloadStats")]
		internal static extern int DownloadStats_64(StatusCallback downloadStatsCallback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetStat0")]
		internal static extern int GetStat(string pchName, ref int pnData);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetStat0")]
		internal static extern int GetStat_64(string pchName, ref int pnData);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetStat")]
		internal static extern int GetStat(string pchName, ref float pfData);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetStat")]
		internal static extern int GetStat_64(string pchName, ref float pfData);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_SetStat0")]
		internal static extern int SetStat(string pchName, int nData);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_SetStat0")]
		internal static extern int SetStat_64(string pchName, int nData);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_SetStat")]
		internal static extern int SetStat(string pchName, float fData);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_SetStat")]
		internal static extern int SetStat_64(string pchName, float fData);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_UploadStats")]
		internal static extern int UploadStats(StatusCallback uploadStatsCallback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_UploadStats")]
		internal static extern int UploadStats_64(StatusCallback uploadStatsCallback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetAchievement")]
		internal static extern int GetAchievement(string pchName, ref int pbAchieved);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetAchievement")]
		internal static extern int GetAchievement_64(string pchName, ref int pbAchieved);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetAchievementUnlockTime")]
		internal static extern int GetAchievementUnlockTime(string pchName, ref int punUnlockTime);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetAchievementUnlockTime")]
		internal static extern int GetAchievementUnlockTime_64(string pchName, ref int punUnlockTime);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_SetAchievement")]
		internal static extern int SetAchievement(string pchName);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_SetAchievement")]
		internal static extern int SetAchievement_64(string pchName);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_ClearAchievement")]
		internal static extern int ClearAchievement(string pchName);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_ClearAchievement")]
		internal static extern int ClearAchievement_64(string pchName);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_DownloadLeaderboardScores")]
		internal static extern int DownloadLeaderboardScores(StatusCallback downloadLeaderboardScoresCB, string pchLeaderboardName, ELeaderboardDataRequest eLeaderboardDataRequest, ELeaderboardDataTimeRange eLeaderboardDataTimeRange, int nRangeStart, int nRangeEnd);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_DownloadLeaderboardScores")]
		internal static extern int DownloadLeaderboardScores_64(StatusCallback downloadLeaderboardScoresCB, string pchLeaderboardName, ELeaderboardDataRequest eLeaderboardDataRequest, ELeaderboardDataTimeRange eLeaderboardDataTimeRange, int nRangeStart, int nRangeEnd);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_UploadLeaderboardScore")]
		internal static extern int UploadLeaderboardScore(StatusCallback uploadLeaderboardScoreCB, string pchLeaderboardName, int nScore);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_UploadLeaderboardScore")]
		internal static extern int UploadLeaderboardScore_64(StatusCallback uploadLeaderboardScoreCB, string pchLeaderboardName, int nScore);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardScore")]
		internal static extern int GetLeaderboardScore(int index, ref LeaderboardEntry_t pLeaderboardEntry);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardScore")]
		internal static extern int GetLeaderboardScore_64(int index, ref LeaderboardEntry_t pLeaderboardEntry);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardScoreCount")]
		internal static extern int GetLeaderboardScoreCount();

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardScoreCount")]
		internal static extern int GetLeaderboardScoreCount_64();

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardSortMethod")]
		internal static extern ELeaderboardSortMethod GetLeaderboardSortMethod();

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardSortMethod")]
		internal static extern ELeaderboardSortMethod GetLeaderboardSortMethod_64();

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardDisplayType")]
		internal static extern ELeaderboardDisplayType GetLeaderboardDisplayType();

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportUserStats_GetLeaderboardDisplayType")]
		internal static extern ELeaderboardDisplayType GetLeaderboardDisplayType_64();
	}
}
