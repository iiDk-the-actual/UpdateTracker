using System;
using UnityEngine;

public class SIAutoPressButtonOnAwake : MonoBehaviour
{
	private void Awake()
	{
		this.button = base.GetComponent<SITouchscreenButton>();
		this.terminalParent = this.button.GetComponentInParent<SICombinedTerminal>();
	}

	private void OnEnable()
	{
		if (this.button == null)
		{
			return;
		}
		this.awakeTime = Time.time;
		this.buttonPressed = false;
	}

	private void Update()
	{
		if (this.buttonPressed || Time.time < this.awakeTime + this.delay)
		{
			return;
		}
		if (this.terminalParent.activePlayer.ActorNr == SIPlayer.LocalPlayer.ActorNr)
		{
			this.button.PressButton();
		}
		this.buttonPressed = true;
	}

	private SICombinedTerminal terminalParent;

	private SITouchscreenButton button;

	private float awakeTime;

	private bool buttonPressed;

	public float delay = 2f;
}
