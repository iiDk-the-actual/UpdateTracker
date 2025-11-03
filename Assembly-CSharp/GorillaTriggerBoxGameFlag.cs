using System;
using Photon.Pun;
using UnityEngine;

public class GorillaTriggerBoxGameFlag : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		base.OnBoxTriggered();
		PhotonView.Get(Object.FindAnyObjectByType<GorillaGameManager>()).RPC(this.functionName, RpcTarget.MasterClient, null);
	}

	public string functionName;
}
