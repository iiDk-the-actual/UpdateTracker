using System;
using System.Collections.Generic;

namespace Viveport
{
	public class SubscriptionStatus
	{
		public List<SubscriptionStatus.Platform> Platforms { get; set; }

		public SubscriptionStatus.TransactionType Type { get; set; }

		public SubscriptionStatus()
		{
			this.Platforms = new List<SubscriptionStatus.Platform>();
			this.Type = SubscriptionStatus.TransactionType.Unknown;
		}

		public enum Platform
		{
			Windows,
			Android
		}

		public enum TransactionType
		{
			Unknown,
			Paid,
			Redeem,
			FreeTrial
		}
	}
}
