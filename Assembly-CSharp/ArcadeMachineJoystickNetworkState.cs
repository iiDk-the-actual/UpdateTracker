using System;
using Fusion;
using Photon.Pun;

[NetworkBehaviourWeaved(0)]
public class ArcadeMachineJoystickNetworkState : NetworkComponent
{
	private new void Awake()
	{
		this.joystick = base.GetComponent<ArcadeMachineJoystick>();
	}

	public override void ReadDataFusion()
	{
		throw new NotImplementedException();
	}

	public override void WriteDataFusion()
	{
		throw new NotImplementedException();
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
	}

	private ArcadeMachineJoystick joystick;
}
