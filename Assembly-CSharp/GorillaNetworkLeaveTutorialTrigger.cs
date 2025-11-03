using System;

public class GorillaNetworkLeaveTutorialTrigger : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		base.OnBoxTriggered();
		NetworkSystem.Instance.SetMyTutorialComplete();
	}
}
