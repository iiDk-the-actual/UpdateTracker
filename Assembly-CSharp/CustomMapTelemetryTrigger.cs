using System;
using GorillaLocomotion;
using GorillaNetworking;
using UnityEngine;

public class CustomMapTelemetryTrigger : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		if (other == GTPlayer.Instance.headCollider && CustomMapTelemetry.IsActive)
		{
			CustomMapTelemetry.EndMapTracking();
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other == GTPlayer.Instance.headCollider && GorillaComputer.instance.IsPlayerInVirtualStump() && !CustomMapTelemetry.IsActive)
		{
			CustomMapTelemetry.StartMapTracking();
		}
	}
}
