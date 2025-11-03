using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	public abstract class BaseGuidedRefTargetMono : MonoBehaviour, IGuidedRefTargetMono, IGuidedRefMonoBehaviour, IGuidedRefObject
	{
		protected virtual void Awake()
		{
			((IGuidedRefObject)this).GuidedRefInitialize();
		}

		protected virtual void OnDestroy()
		{
			GuidedRefHub.UnregisterTarget<BaseGuidedRefTargetMono>(this, true);
		}

		GuidedRefBasicTargetInfo IGuidedRefTargetMono.GRefTargetInfo
		{
			get
			{
				return this.guidedRefTargetInfo;
			}
			set
			{
				this.guidedRefTargetInfo = value;
			}
		}

		Object IGuidedRefTargetMono.GuidedRefTargetObject
		{
			get
			{
				return this;
			}
		}

		void IGuidedRefObject.GuidedRefInitialize()
		{
			GuidedRefHub.RegisterTarget<BaseGuidedRefTargetMono>(this, this.guidedRefTargetInfo.hubIds, this);
		}

		Transform IGuidedRefMonoBehaviour.get_transform()
		{
			return base.transform;
		}

		int IGuidedRefObject.GetInstanceID()
		{
			return base.GetInstanceID();
		}

		public GuidedRefBasicTargetInfo guidedRefTargetInfo;
	}
}
