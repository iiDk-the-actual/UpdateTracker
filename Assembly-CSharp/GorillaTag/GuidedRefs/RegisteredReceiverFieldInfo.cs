using System;
using UnityEngine.Serialization;

namespace GorillaTag.GuidedRefs
{
	public struct RegisteredReceiverFieldInfo
	{
		[FormerlySerializedAs("receiver")]
		public IGuidedRefReceiverMono receiverMono;

		public int fieldId;

		public int index;
	}
}
