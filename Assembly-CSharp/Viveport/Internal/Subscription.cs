using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal
{
	internal class Subscription
	{
		static Subscription()
		{
			Api.LoadLibraryManually("viveport_api");
		}

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_IsReady")]
		internal static extern void IsReady(StatusCallback2 IsReadyCallback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_IsReady")]
		internal static extern void IsReady_64(StatusCallback2 IsReadyCallback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_IsWindowsSubscriber")]
		internal static extern bool IsWindowsSubscriber();

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_IsWindowsSubscriber")]
		internal static extern bool IsWindowsSubscriber_64();

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_IsAndroidSubscriber")]
		internal static extern bool IsAndroidSubscriber();

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_IsAndroidSubscriber")]
		internal static extern bool IsAndroidSubscriber_64();

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_GetTransactionType")]
		internal static extern ESubscriptionTransactionType GetTransactionType();

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportSubscription_GetTransactionType")]
		internal static extern ESubscriptionTransactionType GetTransactionType_64();
	}
}
