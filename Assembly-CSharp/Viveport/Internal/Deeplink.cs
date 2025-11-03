using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Viveport.Internal
{
	internal class Deeplink
	{
		static Deeplink()
		{
			Api.LoadLibraryManually("viveport_api");
		}

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_IsReady")]
		internal static extern void IsReady(StatusCallback IsReadyCallback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_IsReady")]
		internal static extern void IsReady_64(StatusCallback IsReadyCallback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToApp")]
		internal static extern void GoToApp(StatusCallback2 GoToAppCallback, string ViveportId, string LaunchData);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToApp")]
		internal static extern void GoToApp_64(StatusCallback2 GoToAppCallback, string ViveportId, string LaunchData);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToAppWithBranchName")]
		internal static extern void GoToApp(StatusCallback2 GoToAppCallback, string ViveportId, string LaunchData, string branchName);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToAppWithBranchName")]
		internal static extern void GoToApp_64(StatusCallback2 GoToAppCallback, string ViveportId, string LaunchData, string branchName);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToStore")]
		internal static extern void GoToStore(StatusCallback2 GetSessionTokenCallback, string ViveportId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToStore")]
		internal static extern void GoToStore_64(StatusCallback2 GetSessionTokenCallback, string ViveportId);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToAppOrGoToStore")]
		internal static extern void GoToAppOrGoToStore(StatusCallback2 GoToAppCallback, string ViveportId, string LaunchData);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GoToAppOrGoToStore")]
		internal static extern void GoToAppOrGoToStore_64(StatusCallback2 GoToAppCallback, string ViveportId, string LaunchData);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GetAppLaunchData")]
		internal static extern int GetAppLaunchData(StringBuilder userId, int size);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportDeeplink_GetAppLaunchData")]
		internal static extern int GetAppLaunchData_64(StringBuilder userId, int size);
	}
}
