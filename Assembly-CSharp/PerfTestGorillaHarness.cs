using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

[GTStripGameObjectFromBuild("!GT_AUTOMATED_PERF_TEST")]
public class PerfTestGorillaHarness : MonoBehaviour
{
	private void Awake()
	{
		foreach (PerfTestGorillaSlot perfTestGorillaSlot in base.GetComponentsInChildren<PerfTestGorillaSlot>())
		{
			if (perfTestGorillaSlot.slotType == PerfTestGorillaSlot.SlotType.VR_PLAYER)
			{
				this._vrSlot = perfTestGorillaSlot;
			}
			else
			{
				this.dummySlots.Add(perfTestGorillaSlot);
			}
		}
	}

	private void Update()
	{
		if (!this._isRecording)
		{
			return;
		}
		foreach (PerfTestGorillaSlot perfTestGorillaSlot in this.dummySlots)
		{
			float num = perfTestGorillaSlot.localStartPosition.y + Mathf.Sin(Time.time * this.bounceSpeed) * this.bounceAmplitude;
			perfTestGorillaSlot.transform.localPosition = new Vector3(perfTestGorillaSlot.localStartPosition.x, num, perfTestGorillaSlot.localStartPosition.z);
		}
	}

	public void StartRecording()
	{
		this._isRecording = true;
	}

	public void StopRecording()
	{
		foreach (PerfTestGorillaSlot perfTestGorillaSlot in this.dummySlots)
		{
			perfTestGorillaSlot.transform.localPosition = perfTestGorillaSlot.localStartPosition;
		}
		this._isRecording = false;
	}

	public PerfTestGorillaSlot _vrSlot;

	public List<PerfTestGorillaSlot> dummySlots = new List<PerfTestGorillaSlot>(9);

	private bool _isRecording;

	private float _nextRandomMoveTime;

	private float bounceSpeed = 5f;

	private float bounceAmplitude = 0.5f;
}
