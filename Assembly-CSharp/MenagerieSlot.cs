using System;
using TMPro;
using UnityEngine;

public class MenagerieSlot : MonoBehaviour
{
	private void Reset()
	{
		this.critterMountPoint = base.transform;
	}

	public Transform critterMountPoint;

	public TMP_Text label;

	public MenagerieCritter critter;
}
