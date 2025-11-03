using System;

namespace GorillaTagScripts.CustomMapSupport
{
	public class CMSLuau : CMSTrigger
	{
		public override void Trigger(double triggerTime = -1.0, bool originatedLocally = false, bool ignoreTriggerCount = false)
		{
			base.Trigger(triggerTime, originatedLocally, ignoreTriggerCount);
			if (originatedLocally)
			{
				LuauVm.touchEventsQueue.Enqueue(base.gameObject);
			}
		}
	}
}
