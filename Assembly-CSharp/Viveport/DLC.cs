using System;
using System.Text;
using AOT;
using Viveport.Internal;

namespace Viveport
{
	public class DLC
	{
		[MonoPInvokeCallback(typeof(StatusCallback))]
		private static void IsDlcReadyIl2cppCallback(int errorCode)
		{
			DLC.isDlcReadyIl2cppCallback(errorCode);
		}

		public static int IsDlcReady(StatusCallback callback)
		{
			if (callback == null)
			{
				throw new InvalidOperationException("callback == null");
			}
			DLC.isDlcReadyIl2cppCallback = new StatusCallback(callback.Invoke);
			Api.InternalStatusCallbacks.Add(new StatusCallback(DLC.IsDlcReadyIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				return DLC.IsReady_64(new StatusCallback(DLC.IsDlcReadyIl2cppCallback));
			}
			return DLC.IsReady(new StatusCallback(DLC.IsDlcReadyIl2cppCallback));
		}

		public static int GetCount()
		{
			if (IntPtr.Size == 8)
			{
				return DLC.GetCount_64();
			}
			return DLC.GetCount();
		}

		public static bool GetIsAvailable(int index, out string appId, out bool isAvailable)
		{
			StringBuilder stringBuilder = new StringBuilder(37);
			bool flag;
			if (IntPtr.Size == 8)
			{
				flag = DLC.GetIsAvailable_64(index, stringBuilder, out isAvailable);
			}
			else
			{
				flag = DLC.GetIsAvailable(index, stringBuilder, out isAvailable);
			}
			appId = stringBuilder.ToString();
			return flag;
		}

		public static int IsSubscribed()
		{
			if (IntPtr.Size == 8)
			{
				return DLC.IsSubscribed_64();
			}
			return DLC.IsSubscribed();
		}

		private static StatusCallback isDlcReadyIl2cppCallback;

		private const int AppIdLength = 37;
	}
}
