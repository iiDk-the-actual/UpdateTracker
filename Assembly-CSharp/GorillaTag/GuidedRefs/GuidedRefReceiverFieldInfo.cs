using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	[Serializable]
	public struct GuidedRefReceiverFieldInfo
	{
		public GuidedRefReceiverFieldInfo(bool useRecommendedDefaults)
		{
			this.resolveModes = (useRecommendedDefaults ? (GRef.EResolveModes.Runtime | GRef.EResolveModes.SceneProcessing) : GRef.EResolveModes.None);
			this.targetId = null;
			this.hubId = null;
			this.fieldId = 0;
		}

		[SerializeField]
		public GRef.EResolveModes resolveModes;

		[SerializeField]
		public GuidedRefTargetIdSO targetId;

		[Tooltip("(Required) Used to filter down which relay the target can belong to. Only one GuidedRefRelayHub will be used.")]
		[SerializeField]
		public GuidedRefHubIdSO hubId;

		[NonSerialized]
		public int fieldId;
	}
}
