using System;
using System.Text;
using AOT;
using Viveport.Internal;

namespace Viveport
{
	public class User
	{
		[MonoPInvokeCallback(typeof(StatusCallback))]
		private static void IsReadyIl2cppCallback(int errorCode)
		{
			User.isReadyIl2cppCallback(errorCode);
		}

		public static int IsReady(StatusCallback callback)
		{
			if (callback == null)
			{
				throw new InvalidOperationException("callback == null");
			}
			User.isReadyIl2cppCallback = new StatusCallback(callback.Invoke);
			Api.InternalStatusCallbacks.Add(new StatusCallback(User.IsReadyIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				return User.IsReady_64(new StatusCallback(User.IsReadyIl2cppCallback));
			}
			return User.IsReady(new StatusCallback(User.IsReadyIl2cppCallback));
		}

		public static string GetUserId()
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (IntPtr.Size == 8)
			{
				User.GetUserID_64(stringBuilder, 256);
			}
			else
			{
				User.GetUserID(stringBuilder, 256);
			}
			return stringBuilder.ToString();
		}

		public static string GetUserName()
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			if (IntPtr.Size == 8)
			{
				User.GetUserName_64(stringBuilder, 256);
			}
			else
			{
				User.GetUserName(stringBuilder, 256);
			}
			return stringBuilder.ToString();
		}

		public static string GetUserAvatarUrl()
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			if (IntPtr.Size == 8)
			{
				User.GetUserAvatarUrl_64(stringBuilder, 512);
			}
			else
			{
				User.GetUserAvatarUrl(stringBuilder, 512);
			}
			return stringBuilder.ToString();
		}

		private static StatusCallback isReadyIl2cppCallback;

		private const int MaxIdLength = 256;

		private const int MaxNameLength = 256;

		private const int MaxUrlLength = 512;
	}
}
