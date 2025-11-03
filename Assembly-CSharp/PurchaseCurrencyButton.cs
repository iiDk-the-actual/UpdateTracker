using System;
using System.Collections;
using UnityEngine;

public class PurchaseCurrencyButton : GorillaPressableButton
{
	public override void ButtonActivation()
	{
		base.ButtonActivation();
		ATM_Manager.instance.PressCurrencyPurchaseButton(this.purchaseCurrencySize);
		base.StartCoroutine(this.ButtonColorUpdate());
	}

	private IEnumerator ButtonColorUpdate()
	{
		this.buttonRenderer.sharedMaterial = this.pressedMaterial;
		yield return new WaitForSeconds(this.buttonFadeTime);
		this.buttonRenderer.sharedMaterial = this.unpressedMaterial;
		yield break;
	}

	public string purchaseCurrencySize;

	public float buttonFadeTime = 0.25f;
}
