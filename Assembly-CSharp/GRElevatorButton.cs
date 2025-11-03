using System;
using UnityEngine;

public class GRElevatorButton : MonoBehaviour
{
	private void Awake()
	{
		if (this.disableDelayed == null)
		{
			this.disableDelayed = this.buttonLit.GetComponent<DisableGameObjectDelayed>();
		}
		if (this.tempLight)
		{
			this.disableDelayed.enabled = false;
			return;
		}
		this.disableDelayed.delayTime = this.litUpTime;
	}

	public void Pressed()
	{
		this.buttonLit.SetActive(true);
	}

	public void Depressed()
	{
		this.buttonLit.SetActive(false);
	}

	public GRElevator.ButtonType buttonType;

	public GameObject buttonLit;

	public float litUpTime;

	public DisableGameObjectDelayed disableDelayed;

	public bool tempLight;
}
