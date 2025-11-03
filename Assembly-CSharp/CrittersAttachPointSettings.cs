using System;

public class CrittersAttachPointSettings : CrittersActorSettings
{
	public override void UpdateActorSettings()
	{
		base.UpdateActorSettings();
		CrittersAttachPoint crittersAttachPoint = (CrittersAttachPoint)this.parentActor;
		crittersAttachPoint.anchorLocation = this.anchoredLocation;
		crittersAttachPoint.rb.isKinematic = true;
		crittersAttachPoint.isLeft = this.isLeft;
	}

	public bool isLeft;

	public CrittersAttachPoint.AnchoredLocationTypes anchoredLocation;
}
