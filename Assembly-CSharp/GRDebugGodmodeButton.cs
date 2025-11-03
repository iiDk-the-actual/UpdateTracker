using System;

public class GRDebugGodmodeButton : GorillaPressableReleaseButton
{
	private void Awake()
	{
		base.gameObject.SetActive(false);
	}

	public void OnPressedButton()
	{
	}

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		this.UpdateColor();
	}

	public override void ButtonDeactivation()
	{
		base.ButtonDeactivation();
		this.UpdateColor();
	}
}
