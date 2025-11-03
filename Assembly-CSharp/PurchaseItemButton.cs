using System;
using System.Collections;
using GorillaNetworking;
using UnityEngine;

public class PurchaseItemButton : GorillaPressableButton
{
	public override void ButtonActivationWithHand(bool isLeftHand)
	{
		base.ButtonActivation();
		CosmeticsController.instance.PressPurchaseItemButton(this, isLeftHand);
		base.StartCoroutine(this.ButtonColorUpdate());
	}

	private IEnumerator ButtonColorUpdate()
	{
		Debug.Log("did this happen?");
		this.buttonRenderer.material = this.pressedMaterial;
		yield return new WaitForSeconds(this.debounceTime);
		this.buttonRenderer.material = (this.isOn ? this.pressedMaterial : this.unpressedMaterial);
		yield break;
	}

	public string buttonSide;
}
