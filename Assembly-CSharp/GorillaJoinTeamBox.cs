using System;
using Photon.Pun;
using UnityEngine;

public class GorillaJoinTeamBox : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		base.OnBoxTriggered();
		if (GameObject.FindGameObjectWithTag("GorillaGameManager").GetComponent<GorillaGameManager>() != null)
		{
			bool inRoom = PhotonNetwork.InRoom;
		}
	}

	public bool joinRedTeam;
}
