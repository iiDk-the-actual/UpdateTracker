using System;
using GorillaNetworking;
using Photon.Pun;

public class GroupJoinButton : GorillaPressableButton
{
	public override void ButtonActivation()
	{
		base.ButtonActivation();
		if (this.inPrivate)
		{
			GorillaComputer.instance.OnGroupJoinButtonPress(this.gameModeIndex, this.friendCollider);
		}
	}

	public void Update()
	{
		this.inPrivate = PhotonNetwork.InRoom && !PhotonNetwork.CurrentRoom.IsVisible;
		if (!this.inPrivate)
		{
			this.isOn = true;
		}
	}

	public int gameModeIndex;

	public GorillaFriendCollider friendCollider;

	public bool inPrivate;
}
