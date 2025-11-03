using System;
using Photon.Pun;

public class GorillaThrowingRock : GorillaThrowable, IPunInstantiateMagicCallback
{
	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
	}

	public float bonkSpeedMin = 1f;

	public float bonkSpeedMax = 5f;

	public VRRig hitRig;
}
