using System;
using UnityEngine;

public class GRShiftSirenLight : MonoBehaviourTick
{
	public override void Tick()
	{
		if (this.shiftManager == null)
		{
			this.shiftManager = GhostReactor.instance.shiftManager;
			return;
		}
		if (this.redLight.activeSelf != this.shiftManager.ShiftActive)
		{
			this.redLight.SetActive(this.shiftManager.ShiftActive);
		}
		if (this.greenLight.activeSelf == this.shiftManager.ShiftActive)
		{
			this.greenLight.SetActive(!this.shiftManager.ShiftActive);
		}
		if (this.readyRoomLight != null)
		{
			this.readyRoomLight.intensity = (this.shiftManager.ShiftActive ? this.dimLight : this.brightLight);
		}
		if (this.shiftManager.ShiftActive)
		{
			this.redLightParent.localEulerAngles = new Vector3(0f, Time.time * this.rotationRate, 0f);
			return;
		}
		this.greenLightParent.localEulerAngles = new Vector3(0f, Time.time * this.rotationRate, 0f);
	}

	public float rotationRate = 1.25f;

	public Transform greenLightParent;

	public Transform redLightParent;

	public GameObject redLight;

	public GameObject greenLight;

	public GhostReactorShiftManager shiftManager;

	public float dimLight;

	public float brightLight;

	public Light readyRoomLight;
}
