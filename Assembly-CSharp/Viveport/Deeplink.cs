using System;
using System.Text;
using AOT;
using Viveport.Internal;

namespace Viveport
{
	public class Deeplink
	{
		[MonoPInvokeCallback(typeof(StatusCallback))]
		private static void IsReadyIl2cppCallback(int errorCode)
		{
			Deeplink.isReadyIl2cppCallback(errorCode);
		}

		public static void IsReady(StatusCallback callback)
		{
			if (callback == null)
			{
				throw new InvalidOperationException("callback == null");
			}
			Deeplink.isReadyIl2cppCallback = new StatusCallback(callback.Invoke);
			Api.InternalStatusCallbacks.Add(new StatusCallback(Deeplink.IsReadyIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				Deeplink.IsReady_64(new StatusCallback(Deeplink.IsReadyIl2cppCallback));
				return;
			}
			Deeplink.IsReady(new StatusCallback(Deeplink.IsReadyIl2cppCallback));
		}

		[MonoPInvokeCallback(typeof(StatusCallback2))]
		private static void GoToAppIl2cppCallback(int errorCode, string message)
		{
			Deeplink.goToAppIl2cppCallback(errorCode, message);
		}

		public static void GoToApp(StatusCallback2 callback, string viveportId, string launchData)
		{
			if (callback == null || string.IsNullOrEmpty(viveportId))
			{
				throw new InvalidOperationException("callback == null || string.IsNullOrEmpty(viveportId)");
			}
			Deeplink.goToAppIl2cppCallback = new StatusCallback2(callback.Invoke);
			Api.InternalStatusCallback2s.Add(new StatusCallback2(Deeplink.GoToAppIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				Deeplink.GoToApp_64(new StatusCallback2(Deeplink.GoToAppIl2cppCallback), viveportId, launchData);
				return;
			}
			Deeplink.GoToApp(new StatusCallback2(Deeplink.GoToAppIl2cppCallback), viveportId, launchData);
		}

		[MonoPInvokeCallback(typeof(StatusCallback2))]
		private static void GoToAppWithBranchNameIl2cppCallback(int errorCode, string message)
		{
			Deeplink.goToAppWithBranchNameIl2cppCallback(errorCode, message);
		}

		public static void GoToApp(StatusCallback2 callback, string viveportId, string launchData, string branchName)
		{
			if (callback == null || string.IsNullOrEmpty(viveportId))
			{
				throw new InvalidOperationException("callback == null || string.IsNullOrEmpty(viveportId)");
			}
			Deeplink.goToAppWithBranchNameIl2cppCallback = new StatusCallback2(callback.Invoke);
			Api.InternalStatusCallback2s.Add(new StatusCallback2(Deeplink.GoToAppWithBranchNameIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				Deeplink.GoToApp_64(new StatusCallback2(Deeplink.GoToAppWithBranchNameIl2cppCallback), viveportId, launchData, branchName);
				return;
			}
			Deeplink.GoToApp(new StatusCallback2(Deeplink.GoToAppWithBranchNameIl2cppCallback), viveportId, launchData, branchName);
		}

		[MonoPInvokeCallback(typeof(StatusCallback2))]
		private static void GoToStoreIl2cppCallback(int errorCode, string message)
		{
			Deeplink.goToStoreIl2cppCallback(errorCode, message);
		}

		public static void GoToStore(StatusCallback2 callback, string viveportId = "")
		{
			if (callback == null)
			{
				throw new InvalidOperationException("callback == null || string.IsNullOrEmpty(viveportId)");
			}
			Deeplink.goToStoreIl2cppCallback = new StatusCallback2(callback.Invoke);
			Api.InternalStatusCallback2s.Add(new StatusCallback2(Deeplink.GoToStoreIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				Deeplink.GoToStore_64(new StatusCallback2(Deeplink.GoToStoreIl2cppCallback), viveportId);
				return;
			}
			Deeplink.GoToStore(new StatusCallback2(Deeplink.GoToStoreIl2cppCallback), viveportId);
		}

		[MonoPInvokeCallback(typeof(StatusCallback2))]
		private static void GoToAppOrGoToStoreIl2cppCallback(int errorCode, string message)
		{
			Deeplink.goToAppOrGoToStoreIl2cppCallback(errorCode, message);
		}

		public static void GoToAppOrGoToStore(StatusCallback2 callback, string viveportId, string launchData)
		{
			if (callback == null || string.IsNullOrEmpty(viveportId))
			{
				throw new InvalidOperationException("callback == null || string.IsNullOrEmpty(viveportId)");
			}
			Deeplink.goToAppOrGoToStoreIl2cppCallback = new StatusCallback2(callback.Invoke);
			Api.InternalStatusCallback2s.Add(new StatusCallback2(Deeplink.GoToAppOrGoToStoreIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				Deeplink.GoToAppOrGoToStore_64(new StatusCallback2(Deeplink.GoToAppOrGoToStoreIl2cppCallback), viveportId, launchData);
				return;
			}
			Deeplink.GoToAppOrGoToStore(new StatusCallback2(Deeplink.GoToAppOrGoToStoreIl2cppCallback), viveportId, launchData);
		}

		public static string GetAppLaunchData()
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (IntPtr.Size == 8)
			{
				Deeplink.GetAppLaunchData_64(stringBuilder, 256);
			}
			else
			{
				Deeplink.GetAppLaunchData(stringBuilder, 256);
			}
			return stringBuilder.ToString();
		}

		private const int MaxIdLength = 256;

		private static StatusCallback isReadyIl2cppCallback;

		private static StatusCallback2 goToAppIl2cppCallback;

		private static StatusCallback2 goToAppWithBranchNameIl2cppCallback;

		private static StatusCallback2 goToStoreIl2cppCallback;

		private static StatusCallback2 goToAppOrGoToStoreIl2cppCallback;
	}
}
