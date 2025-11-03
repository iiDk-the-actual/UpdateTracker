using System;
using GorillaExtensions;
using TMPro;
using UnityEngine;

public class CustomMapsScreenButton : CustomMapsScreenTouchPoint
{
	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.isToggle)
		{
			this.SetButtonActive(this.isActive);
			return;
		}
		this.isActive = false;
	}

	public void SetButtonText(string text)
	{
		if (this.bttnText.IsNull())
		{
			return;
		}
		this.bttnText.text = text;
	}

	public void SetButtonActive(bool active)
	{
		this.isActive = active;
		this.touchPointRenderer.color = (this.isActive ? this.buttonColorSettings.PressedColor : this.buttonColorSettings.UnpressedColor);
	}

	public override void PressButtonColourUpdate()
	{
		if (!this.isToggle)
		{
			base.PressButtonColourUpdate();
			return;
		}
	}

	protected override void OnButtonPressedEvent()
	{
		this.isActive = !this.isActive;
	}

	[SerializeField]
	private TMP_Text bttnText;

	[SerializeField]
	private bool isToggle;

	private bool isActive;
}
