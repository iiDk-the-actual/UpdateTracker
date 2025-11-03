using System;
using GorillaTag;
using UnityEngine;

[GTStripGameObjectFromBuild("!GT_AUTOMATED_PERF_TEST")]
public class PerfTestGorillaSlot : MonoBehaviour
{
	private void Start()
	{
		this.localStartPosition = base.transform.localPosition;
	}

	public PerfTestGorillaSlot.SlotType slotType;

	public Vector3 localStartPosition;

	public enum SlotType
	{
		VR_PLAYER,
		DUMMY
	}
}
