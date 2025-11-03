using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	public abstract class GuidedRefIdBaseSO : ScriptableObject, IGuidedRefObject
	{
		public virtual void GuidedRefInitialize()
		{
		}

		int IGuidedRefObject.GetInstanceID()
		{
			return base.GetInstanceID();
		}
	}
}
