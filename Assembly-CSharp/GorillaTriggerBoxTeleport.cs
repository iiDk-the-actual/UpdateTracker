using System;
using UnityEngine;

public class GorillaTriggerBoxTeleport : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		this.cameraOffest.GetComponent<Rigidbody>().linearVelocity = new Vector3(0f, 0f, 0f);
		this.cameraOffest.transform.position = this.teleportLocation;
	}

	public Vector3 teleportLocation;

	public GameObject cameraOffest;
}
