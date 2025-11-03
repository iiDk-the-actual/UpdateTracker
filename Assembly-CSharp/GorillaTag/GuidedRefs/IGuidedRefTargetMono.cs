using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	public interface IGuidedRefTargetMono : IGuidedRefMonoBehaviour, IGuidedRefObject
	{
		GuidedRefBasicTargetInfo GRefTargetInfo { get; set; }

		Object GuidedRefTargetObject { get; }
	}
}
