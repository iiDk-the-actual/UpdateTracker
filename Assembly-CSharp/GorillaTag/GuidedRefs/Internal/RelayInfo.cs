using System;
using System.Collections.Generic;

namespace GorillaTag.GuidedRefs.Internal
{
	public class RelayInfo
	{
		[NonSerialized]
		public IGuidedRefTargetMono targetMono;

		[NonSerialized]
		public List<RegisteredReceiverFieldInfo> registeredFields;

		[NonSerialized]
		public List<RegisteredReceiverFieldInfo> resolvedFields;
	}
}
