using System;
using UnityEngine.UI;

public class BuilderKioskButton : GorillaPressableButton
{
	public override void Start()
	{
		this.currentPieceSet = BuilderKiosk.nullItem;
	}

	public override void UpdateColor()
	{
		if (this.currentPieceSet.isNullItem)
		{
			this.buttonRenderer.material = this.unpressedMaterial;
			this.myText.text = "";
			return;
		}
		base.UpdateColor();
	}

	public override void ButtonActivationWithHand(bool isLeftHand)
	{
		base.ButtonActivation();
	}

	public BuilderSetManager.BuilderSetStoreItem currentPieceSet;

	public BuilderKiosk kiosk;

	public Text setNameText;
}
