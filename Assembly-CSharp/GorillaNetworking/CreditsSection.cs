using System;
using System.Collections.Generic;

namespace GorillaNetworking
{
	[Serializable]
	internal class CreditsSection
	{
		public string Title { get; set; }

		public List<string> Entries { get; set; }
	}
}
