using System;
using UnityEngine;

public class GRDebugFtueResetButton : GorillaPressableReleaseButton
{
	private void Awake()
	{
		if (!this.availableOnLive)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void OnPressedButton()
	{
		PlayerPrefs.SetString("spawnInWrongStump", "flagged");
		PlayerPrefs.Save();
	}

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		this.isOn = true;
		this.UpdateColor();
	}

	public override void ButtonDeactivation()
	{
		base.ButtonDeactivation();
		this.isOn = false;
		this.UpdateColor();
	}

	public bool availableOnLive;
}
