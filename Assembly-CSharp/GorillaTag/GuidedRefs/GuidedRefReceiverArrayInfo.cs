using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	[Serializable]
	public struct GuidedRefReceiverArrayInfo
	{
		public GuidedRefReceiverArrayInfo(bool useRecommendedDefaults)
		{
			this.resolveModes = (useRecommendedDefaults ? (GRef.EResolveModes.Runtime | GRef.EResolveModes.SceneProcessing) : GRef.EResolveModes.None);
			this.targets = Array.Empty<GuidedRefTargetIdSO>();
			this.hubId = null;
			this.fieldId = 0;
			this.resolveCount = 0;
		}

		[Tooltip("Controls whether the array should be overridden by the guided refs.")]
		[SerializeField]
		public GRef.EResolveModes resolveModes;

		[Tooltip("(Required) Used to filter down which relay the target can belong to. Only one GuidedRefRelayHub will be used.")]
		[SerializeField]
		public GuidedRefHubIdSO hubId;

		[SerializeField]
		public GuidedRefTargetIdSO[] targets;

		[NonSerialized]
		public int fieldId;

		[NonSerialized]
		public int resolveCount;
	}
}
