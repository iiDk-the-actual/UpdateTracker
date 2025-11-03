using System;

public class CrittersAttachPoint : CrittersActor
{
	public override void ProcessRemote()
	{
	}

	public bool fixedOrientation = true;

	public CrittersAttachPoint.AnchoredLocationTypes anchorLocation;

	public bool isLeft;

	public enum AnchoredLocationTypes
	{
		Arm,
		Chest,
		Back
	}
}
