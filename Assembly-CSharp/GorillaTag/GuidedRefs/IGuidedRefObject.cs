using System;

namespace GorillaTag.GuidedRefs
{
	public interface IGuidedRefObject
	{
		int GetInstanceID();

		void GuidedRefInitialize();
	}
}
