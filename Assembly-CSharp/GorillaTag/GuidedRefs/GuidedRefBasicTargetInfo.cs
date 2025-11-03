using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	[Serializable]
	public struct GuidedRefBasicTargetInfo
	{
		[SerializeField]
		public GuidedRefTargetIdSO targetId;

		[Tooltip("Used to filter down which relay the target can belong to. If null or empty then all parents with a GuidedRefRelayHub will be used.")]
		[SerializeField]
		public GuidedRefHubIdSO[] hubIds;

		[DebugOption]
		[SerializeField]
		public bool hackIgnoreDuplicateRegistration;
	}
}
