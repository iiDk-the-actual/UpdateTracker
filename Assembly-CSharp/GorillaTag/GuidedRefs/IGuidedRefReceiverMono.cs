using System;

namespace GorillaTag.GuidedRefs
{
	public interface IGuidedRefReceiverMono : IGuidedRefMonoBehaviour, IGuidedRefObject
	{
		bool GuidedRefTryResolveReference(GuidedRefTryResolveInfo target);

		int GuidedRefsWaitingToResolveCount { get; set; }

		void OnAllGuidedRefsResolved();

		void OnGuidedRefTargetDestroyed(int fieldId);
	}
}
