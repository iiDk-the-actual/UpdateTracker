using System;
using GorillaTagScripts.VirtualStumpCustomMaps.UI;

public class CustomMapsKeyToggleButton : CustomMapsKeyButton
{
	public override void PressButtonColourUpdate()
	{
	}

	public void SetButtonStatus(bool newIsPressed)
	{
		if (this.isPressed == newIsPressed)
		{
			return;
		}
		this.isPressed = newIsPressed;
		this.propBlock.SetColor("_BaseColor", this.isPressed ? this.ButtonColorSettings.PressedColor : this.ButtonColorSettings.UnpressedColor);
		this.propBlock.SetColor("_Color", this.isPressed ? this.ButtonColorSettings.PressedColor : this.ButtonColorSettings.UnpressedColor);
		this.ButtonRenderer.SetPropertyBlock(this.propBlock);
	}

	private bool isPressed;
}
