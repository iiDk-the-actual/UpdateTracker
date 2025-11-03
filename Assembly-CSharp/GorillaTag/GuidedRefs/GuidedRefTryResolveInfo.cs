using System;
using UnityEngine.Serialization;

namespace GorillaTag.GuidedRefs
{
	public struct GuidedRefTryResolveInfo
	{
		public int fieldId;

		public int index;

		[FormerlySerializedAs("target")]
		public IGuidedRefTargetMono targetMono;
	}
}
