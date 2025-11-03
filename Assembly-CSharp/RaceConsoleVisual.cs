using System;
using UnityEngine;

public class RaceConsoleVisual : MonoBehaviour
{
	public void ShowRaceInProgress(int laps)
	{
		this.button1.sharedMaterial = this.inactiveButton;
		this.button3.sharedMaterial = this.inactiveButton;
		this.button5.sharedMaterial = this.inactiveButton;
		this.button1.transform.localPosition = Vector3.zero;
		this.button3.transform.localPosition = Vector3.zero;
		this.button5.transform.localPosition = Vector3.zero;
		switch (laps)
		{
		default:
			this.button1.sharedMaterial = this.selectedButton;
			this.button1.transform.localPosition = this.buttonPressedOffset;
			return;
		case 3:
			this.button3.sharedMaterial = this.selectedButton;
			this.button3.transform.localPosition = this.buttonPressedOffset;
			return;
		case 5:
			this.button5.sharedMaterial = this.selectedButton;
			this.button5.transform.localPosition = this.buttonPressedOffset;
			return;
		}
	}

	public void ShowCanStartRace()
	{
		this.button1.transform.localPosition = Vector3.zero;
		this.button3.transform.localPosition = Vector3.zero;
		this.button5.transform.localPosition = Vector3.zero;
		this.button1.sharedMaterial = this.pressableButton;
		this.button3.sharedMaterial = this.pressableButton;
		this.button5.sharedMaterial = this.pressableButton;
	}

	[SerializeField]
	private MeshRenderer button1;

	[SerializeField]
	private MeshRenderer button3;

	[SerializeField]
	private MeshRenderer button5;

	[SerializeField]
	private Vector3 buttonPressedOffset;

	[SerializeField]
	private Material pressableButton;

	[SerializeField]
	private Material selectedButton;

	[SerializeField]
	private Material inactiveButton;
}
