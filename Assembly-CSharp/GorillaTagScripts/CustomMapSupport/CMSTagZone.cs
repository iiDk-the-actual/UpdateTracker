using System;
using GorillaGameModes;

namespace GorillaTagScripts.CustomMapSupport
{
	public class CMSTagZone : CMSTrigger
	{
		public override void Trigger(double triggerTime = -1.0, bool originatedLocally = false, bool ignoreTriggerCount = false)
		{
			base.Trigger(triggerTime, originatedLocally, ignoreTriggerCount);
			if (originatedLocally)
			{
				GameMode.ReportHit();
			}
		}
	}
}
