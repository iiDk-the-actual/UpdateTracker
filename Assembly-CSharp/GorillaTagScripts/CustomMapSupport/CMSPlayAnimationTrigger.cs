using System;
using System.Collections.Generic;
using GorillaExtensions;
using GT_CustomMapSupportRuntime;
using UnityEngine;

namespace GorillaTagScripts.CustomMapSupport
{
	public class CMSPlayAnimationTrigger : CMSTrigger
	{
		public override void CopyTriggerSettings(TriggerSettings settings)
		{
			if (settings.GetType() == typeof(PlayAnimationTriggerSettings))
			{
				PlayAnimationTriggerSettings playAnimationTriggerSettings = (PlayAnimationTriggerSettings)settings;
				this.animatedObjects = playAnimationTriggerSettings.animatedObjects;
				this.animationName = playAnimationTriggerSettings.animationName;
			}
			for (int i = this.animatedObjects.Count - 1; i >= 0; i--)
			{
				if (this.animatedObjects[i].IsNull())
				{
					this.animatedObjects.RemoveAt(i);
				}
			}
			base.CopyTriggerSettings(settings);
		}

		public override void Trigger(double triggerTime = -1.0, bool originatedLocally = false, bool ignoreTriggerCount = false)
		{
			base.Trigger(triggerTime, originatedLocally, ignoreTriggerCount);
			foreach (GameObject gameObject in this.animatedObjects)
			{
				Animator component = gameObject.GetComponent<Animator>();
				if (component.IsNotNull())
				{
					component.Play(this.animationName);
				}
			}
		}

		public List<GameObject> animatedObjects = new List<GameObject>();

		public string animationName = "";
	}
}
