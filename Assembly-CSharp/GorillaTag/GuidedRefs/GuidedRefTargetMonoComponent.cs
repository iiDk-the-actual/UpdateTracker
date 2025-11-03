using System;
using UnityEngine;

namespace GorillaTag.GuidedRefs
{
	public class GuidedRefTargetMonoComponent : MonoBehaviour, IGuidedRefTargetMono, IGuidedRefMonoBehaviour, IGuidedRefObject
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
				return this.targetComponent;
			}
		}

		protected void Awake()
		{
			((IGuidedRefObject)this).GuidedRefInitialize();
		}

		protected void OnDestroy()
		{
			GuidedRefHub.UnregisterTarget<GuidedRefTargetMonoComponent>(this, true);
		}

		void IGuidedRefObject.GuidedRefInitialize()
		{
			GuidedRefHub.RegisterTarget<GuidedRefTargetMonoComponent>(this, this.guidedRefTargetInfo.hubIds, this);
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
		private Component targetComponent;

		[SerializeField]
		private GuidedRefBasicTargetInfo guidedRefTargetInfo;
	}
}
