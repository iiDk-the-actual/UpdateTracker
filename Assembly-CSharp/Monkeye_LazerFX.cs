using System;
using UnityEngine;

public class Monkeye_LazerFX : MonoBehaviour
{
	private void Awake()
	{
		base.enabled = false;
	}

	public void EnableLazer(Transform[] eyes_, VRRig rig_)
	{
		if (rig_ == this.rig)
		{
			return;
		}
		this.eyeBones = eyes_;
		this.rig = rig_;
		base.enabled = true;
		LineRenderer[] array = this.lines;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].positionCount = 2;
		}
	}

	public void DisableLazer()
	{
		if (base.enabled)
		{
			base.enabled = false;
			LineRenderer[] array = this.lines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].positionCount = 0;
			}
		}
	}

	private void Update()
	{
		for (int i = 0; i < this.lines.Length; i++)
		{
			this.lines[i].SetPosition(0, this.eyeBones[i].transform.position);
			this.lines[i].SetPosition(1, this.rig.transform.position);
		}
	}

	private Transform[] eyeBones;

	private VRRig rig;

	public LineRenderer[] lines;
}
