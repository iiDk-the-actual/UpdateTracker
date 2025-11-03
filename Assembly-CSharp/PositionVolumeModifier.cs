using System;
using UnityEngine;

public class PositionVolumeModifier : MonoBehaviour
{
	public void OnTriggerStay(Collider other)
	{
		this.audioToMod.isModified = true;
	}

	public TimeOfDayDependentAudio audioToMod;
}
