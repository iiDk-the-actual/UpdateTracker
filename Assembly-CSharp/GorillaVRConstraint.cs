using System;
using UnityEngine;

public class GorillaVRConstraint : MonoBehaviourTick
{
	public override void Tick()
	{
		if (NetworkSystem.Instance.WrongVersion)
		{
			this.isConstrained = true;
		}
		if (this.isConstrained && Time.realtimeSinceStartup > this.angle)
		{
			GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
		}
	}

	public bool isConstrained;

	public float angle = 3600f;
}
