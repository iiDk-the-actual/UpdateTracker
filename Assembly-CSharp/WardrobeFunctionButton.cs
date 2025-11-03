using System;
using System.Collections;
using GorillaNetworking;
using UnityEngine;

public class WardrobeFunctionButton : GorillaPressableButton
{
	public override void ButtonActivation()
	{
		base.ButtonActivation();
		CosmeticsController.instance.PressWardrobeFunctionButton(this.function);
		base.StartCoroutine(this.ButtonColorUpdate());
	}

	public override void UpdateColor()
	{
	}

	private IEnumerator ButtonColorUpdate()
	{
		this.buttonRenderer.material = this.pressedMaterial;
		yield return new WaitForSeconds(this.buttonFadeTime);
		this.buttonRenderer.material = this.unpressedMaterial;
		yield break;
	}

	public string function;

	public float buttonFadeTime = 0.25f;
}
