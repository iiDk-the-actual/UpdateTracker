using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	public class GuidedRefTargetMonoTransform : MonoBehaviour, IGuidedRefTargetMono, IGuidedRefMonoBehaviour, IGuidedRefObject
	{
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

		public Object GuidedRefTargetObject
		{
			get
			{
				return base.transform;
			}
		}

		protected void Awake()
		{
			((IGuidedRefObject)this).GuidedRefInitialize();
		}

		protected void OnDestroy()
		{
			GuidedRefHub.UnregisterTarget<GuidedRefTargetMonoTransform>(this, true);
		}

		void IGuidedRefObject.GuidedRefInitialize()
		{
			GuidedRefHub.RegisterTarget<GuidedRefTargetMonoTransform>(this, this.guidedRefTargetInfo.hubIds, this);
		}

		Transform IGuidedRefMonoBehaviour.get_transform()
		{
			return base.transform;
		}

		int IGuidedRefObject.GetInstanceID()
		{
			return base.GetInstanceID();
		}

		[SerializeField]
		private GuidedRefBasicTargetInfo guidedRefTargetInfo;
	}
}
