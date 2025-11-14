using System;
using UnityEngine;

public class PlayerPrefFlagButton : GorillaPressableButton
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.isOn = PlayerPrefFlags.Check(this.flag);
		this.UpdateColor();
	}

	public override void ButtonActivation()
	{
		PlayerPrefFlagButton.ButtonMode buttonMode = this.mode;
		if (buttonMode == PlayerPrefFlagButton.ButtonMode.SET_VALUE)
		{
			PlayerPrefFlags.Set(this.flag, this.value);
			this.isOn = this.value;
			this.UpdateColor();
			return;
		}
		if (buttonMode != PlayerPrefFlagButton.ButtonMode.TOGGLE)
		{
			return;
		}
		this.isOn = PlayerPrefFlags.Flip(this.flag);
		this.UpdateColor();
	}

	[SerializeField]
	private PlayerPrefFlags.Flag flag;

	[SerializeField]
	private PlayerPrefFlagButton.ButtonMode mode;

	[SerializeField]
	private bool value;

	private enum ButtonMode
	{
		SET_VALUE,
		TOGGLE
	}
}
