using System;
using AOT;
using Viveport.Internal;

namespace Viveport
{
	public class Subscription
	{
		[MonoPInvokeCallback(typeof(StatusCallback2))]
		private static void IsReadyIl2cppCallback(int errorCode, string message)
		{
			Subscription.isReadyIl2cppCallback(errorCode, message);
		}

		public static void IsReady(StatusCallback2 callback)
		{
			if (callback == null)
			{
				throw new InvalidOperationException("callback == null");
			}
			Subscription.isReadyIl2cppCallback = new StatusCallback2(callback.Invoke);
			Api.InternalStatusCallback2s.Add(new StatusCallback2(Subscription.IsReadyIl2cppCallback));
			if (IntPtr.Size == 8)
			{
				Subscription.IsReady_64(new StatusCallback2(Subscription.IsReadyIl2cppCallback));
				return;
			}
			Subscription.IsReady(new StatusCallback2(Subscription.IsReadyIl2cppCallback));
		}

		public static SubscriptionStatus GetUserStatus()
		{
			SubscriptionStatus subscriptionStatus = new SubscriptionStatus();
			if (IntPtr.Size == 8)
			{
				if (Subscription.IsWindowsSubscriber_64())
				{
					subscriptionStatus.Platforms.Add(SubscriptionStatus.Platform.Windows);
				}
				if (Subscription.IsAndroidSubscriber_64())
				{
					subscriptionStatus.Platforms.Add(SubscriptionStatus.Platform.Android);
				}
				switch (Subscription.GetTransactionType_64())
				{
				case ESubscriptionTransactionType.UNKNOWN:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Unknown;
					break;
				case ESubscriptionTransactionType.PAID:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Paid;
					break;
				case ESubscriptionTransactionType.REDEEM:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Redeem;
					break;
				case ESubscriptionTransactionType.FREEE_TRIAL:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.FreeTrial;
					break;
				default:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Unknown;
					break;
				}
			}
			else
			{
				if (Subscription.IsWindowsSubscriber())
				{
					subscriptionStatus.Platforms.Add(SubscriptionStatus.Platform.Windows);
				}
				if (Subscription.IsAndroidSubscriber())
				{
					subscriptionStatus.Platforms.Add(SubscriptionStatus.Platform.Android);
				}
				switch (Subscription.GetTransactionType())
				{
				case ESubscriptionTransactionType.UNKNOWN:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Unknown;
					break;
				case ESubscriptionTransactionType.PAID:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Paid;
					break;
				case ESubscriptionTransactionType.REDEEM:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Redeem;
					break;
				case ESubscriptionTransactionType.FREEE_TRIAL:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.FreeTrial;
					break;
				default:
					subscriptionStatus.Type = SubscriptionStatus.TransactionType.Unknown;
					break;
				}
			}
			return subscriptionStatus;
		}

		private static StatusCallback2 isReadyIl2cppCallback;
	}
}
