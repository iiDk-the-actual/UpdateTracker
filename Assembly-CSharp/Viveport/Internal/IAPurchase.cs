using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal
{
	internal class IAPurchase
	{
		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_IsReady")]
		public static extern void IsReady(IAPurchaseCallback callback, string pchAppKey);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_IsReady")]
		public static extern void IsReady_64(IAPurchaseCallback callback, string pchAppKey);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Request")]
		public static extern void Request(IAPurchaseCallback callback, string pchPrice);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Request")]
		public static extern void Request_64(IAPurchaseCallback callback, string pchPrice);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_RequestWithUserData")]
		public static extern void Request(IAPurchaseCallback callback, string pchPrice, string pchUserData);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_RequestWithUserData")]
		public static extern void Request_64(IAPurchaseCallback callback, string pchPrice, string pchUserData);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Purchase")]
		public static extern void Purchase(IAPurchaseCallback callback, string pchPurchaseId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Purchase")]
		public static extern void Purchase_64(IAPurchaseCallback callback, string pchPurchaseId);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Query")]
		public static extern void Query(IAPurchaseCallback callback, string pchPurchaseId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Query")]
		public static extern void Query_64(IAPurchaseCallback callback, string pchPurchaseId);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_QueryList")]
		public static extern void Query(IAPurchaseCallback callback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_QueryList")]
		public static extern void Query_64(IAPurchaseCallback callback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, EntryPoint = "IViveportIAPurchase_GetBalance")]
		public static extern void GetBalance(IAPurchaseCallback callback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "IViveportIAPurchase_GetBalance")]
		public static extern void GetBalance_64(IAPurchaseCallback callback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_RequestSubscription")]
		public static extern void RequestSubscription(IAPurchaseCallback callback, string pchPrice, string pchFreeTrialType, int nFreeTrialValue, string pchChargePeriodType, int nChargePeriodValue, int nNumberOfChargePeriod, string pchPlanId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_RequestSubscription")]
		public static extern void RequestSubscription_64(IAPurchaseCallback callback, string pchPrice, string pchFreeTrialType, int nFreeTrialValue, string pchChargePeriodType, int nChargePeriodValue, int nNumberOfChargePeriod, string pchPlanId);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_RequestSubscriptionWithPlanID")]
		public static extern void RequestSubscriptionWithPlanID(IAPurchaseCallback callback, string pchPlanId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_RequestSubscriptionWithPlanID")]
		public static extern void RequestSubscriptionWithPlanID_64(IAPurchaseCallback callback, string pchPlanId);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Subscribe")]
		public static extern void Subscribe(IAPurchaseCallback callback, string pchSubscriptionId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_Subscribe")]
		public static extern void Subscribe_64(IAPurchaseCallback callback, string pchSubscriptionId);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_QuerySubscription")]
		public static extern void QuerySubscription(IAPurchaseCallback callback, string pchSubscriptionId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_QuerySubscription")]
		public static extern void QuerySubscription_64(IAPurchaseCallback callback, string pchSubscriptionId);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_QuerySubscriptionList")]
		public static extern void QuerySubscriptionList(IAPurchaseCallback callback);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_QuerySubscriptionList")]
		public static extern void QuerySubscriptionList_64(IAPurchaseCallback callback);

		[DllImport("viveport_api", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_CancelSubscription")]
		public static extern void CancelSubscription(IAPurchaseCallback callback, string pchSubscriptionId);

		[DllImport("viveport_api64", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "IViveportIAPurchase_CancelSubscription")]
		public static extern void CancelSubscription_64(IAPurchaseCallback callback, string pchSubscriptionId);
	}
}
