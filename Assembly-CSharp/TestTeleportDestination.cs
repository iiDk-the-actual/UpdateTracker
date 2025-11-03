using System;
using GorillaTag;
using UnityEngine;

[GTStripGameObjectFromBuild("!GT_AUTOMATED_PERF_TEST")]
public class TestTeleportDestination : MonoBehaviour
{
	private void OnDrawGizmosSelected()
	{
		Debug.DrawRay(base.transform.position, base.transform.forward * 2f, Color.magenta);
	}

	public GTZone[] zones;

	public GameObject teleportTransform;
}
