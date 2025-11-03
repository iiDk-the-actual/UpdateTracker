using System;
using System.Collections.Generic;

namespace GorillaNetworking
{
	[Serializable]
	internal class FeatureFlagData
	{
		public string name;

		public int value;

		public string valueType;

		public List<string> alwaysOnForUsers;
	}
}
