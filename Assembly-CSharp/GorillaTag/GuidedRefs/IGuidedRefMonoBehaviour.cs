using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	public interface IGuidedRefMonoBehaviour : IGuidedRefObject
	{
		Transform transform { get; }
	}
}
