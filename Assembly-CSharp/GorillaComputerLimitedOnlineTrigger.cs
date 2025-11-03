using System;
using GorillaNetworking;

public class GorillaComputerLimitedOnlineTrigger : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		GorillaComputer.instance.SetLimitOnlineScreens(true);
	}

	public override void OnBoxExited()
	{
		GorillaComputer.instance.SetLimitOnlineScreens(false);
	}
}
