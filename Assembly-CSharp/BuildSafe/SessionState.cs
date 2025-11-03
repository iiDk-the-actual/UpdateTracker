using System;

namespace BuildSafe
{
	public class SessionState
	{
		public string this[string key]
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public static readonly SessionState Shared = new SessionState();
	}
}
