using System;
using TMPro;
using UnityEngine;

public class CustomMapsTerminalControlButton : CustomMapsScreenTouchPoint
{
	public bool IsLocked
	{
		get
		{
			return this.isLocked;
		}
		set
		{
			this.isLocked = value;
		}
	}

	protected override void OnButtonPressedEvent()
	{
		GTDev.Log<string>("terminal control pressed", null);
		if (this.mapsTerminal == null)
		{
			return;
		}
		this.mapsTerminal.HandleTerminalControlButtonPressed();
	}

	public void LockTerminalControl()
	{
		if (this.IsLocked)
		{
			return;
		}
		this.IsLocked = true;
		this.PressButtonColourUpdate();
	}

	public void UnlockTerminalControl()
	{
		if (!this.IsLocked)
		{
			return;
		}
		this.IsLocked = false;
		this.PressButtonColourUpdate();
	}

	public override void PressButtonColourUpdate()
	{
		this.bttnText.fontSize = (this.isLocked ? this.lockedFontSize : this.unlockedFontSize);
		this.bttnText.text = (this.isLocked ? this.lockedText : this.unlockedText);
		this.bttnText.color = (this.isLocked ? this.lockedTextColor : this.unlockedTextColor);
		this.touchPointRenderer.color = (this.isLocked ? this.buttonColorSettings.PressedColor : this.buttonColorSettings.UnpressedColor);
	}

	[SerializeField]
	private TMP_Text bttnText;

	[SerializeField]
	private string unlockedText = "TERMINAL AVAILABLE";

	[SerializeField]
	private string lockedText = "TERMINAL UNAVAILABLE";

	[SerializeField]
	private float unlockedFontSize = 30f;

	[SerializeField]
	private float lockedFontSize = 30f;

	[SerializeField]
	private Color unlockedTextColor = Color.black;

	[SerializeField]
	private Color lockedTextColor = Color.white;

	private bool isLocked;

	[SerializeField]
	private CustomMapsTerminal mapsTerminal;
}
