using System;
using System.Collections.Generic;

namespace GorillaNetworking
{
	public class CacheImport
	{
		public string DeploymentId { get; set; }

		public Dictionary<string, Dictionary<string, string>> TitleData { get; set; }
	}
}
