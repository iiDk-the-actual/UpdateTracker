using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CrittersGrabber : CrittersActor
{
	public override void ProcessRemote()
	{
		if (this.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			this.UpdateAverageSpeed();
		}
	}

	public override bool ProcessLocal()
	{
		if (this.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			this.UpdateAverageSpeed();
		}
		return base.ProcessLocal();
	}

	public Transform grabPosition;

	public bool grabbing;

	public float grabDistance;

	public List<CrittersActor> grabbedActors = new List<CrittersActor>();

	public bool isLeft;
}
