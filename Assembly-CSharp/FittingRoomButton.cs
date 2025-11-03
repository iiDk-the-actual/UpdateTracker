using System;
using GorillaExtensions;
using GorillaNetworking;
using UnityEngine;

public class FittingRoomButton : GorillaPressableButton
{
	public override void Start()
	{
		this.currentCosmeticItem = CosmeticsController.instance.nullItem;
	}

	public override void UpdateColor()
	{
		if (this.currentCosmeticItem.itemName == "null")
		{
			if (this.buttonRenderer.IsNotNull())
			{
				this.buttonRenderer.material = this.unpressedMaterial;
			}
			if (this.myText.IsNotNull())
			{
				this.myText.text = this.noCosmeticText;
			}
			if (this.myTmpText.IsNotNull())
			{
				this.myTmpText.text = this.noCosmeticText;
			}
			if (this.myTmpText2.IsNotNull())
			{
				this.myTmpText2.text = this.noCosmeticText;
				return;
			}
		}
		else
		{
			if (this.isOn)
			{
				if (this.buttonRenderer.IsNotNull())
				{
					this.buttonRenderer.material = this.pressedMaterial;
				}
				this.SetOnText(this.myText.IsNotNull(), this.myTmpText.IsNotNull(), this.myTmpText2.IsNotNull());
				return;
			}
			if (this.buttonRenderer.IsNotNull())
			{
				this.buttonRenderer.material = this.unpressedMaterial;
			}
			this.SetOffText(this.myText.IsNotNull(), this.myTmpText.IsNotNull(), this.myTmpText2.IsNotNull());
		}
	}

	public override void ButtonActivationWithHand(bool isLeftHand)
	{
		base.ButtonActivationWithHand(isLeftHand);
		CosmeticsController.instance.PressFittingRoomButton(this, isLeftHand);
	}

	public void SetItem(CosmeticsController.CosmeticItem item, bool isInTryOnSet)
	{
		this.currentCosmeticItem = item;
		if (this.currentCosmeticSprite.IsNotNull())
		{
			this.currentCosmeticSprite.sprite = this.currentCosmeticItem.itemPicture;
		}
		this.isOn = isInTryOnSet;
		this.UpdateColor();
	}

	public void ClearItem()
	{
		if (this.currentCosmeticItem.isNullItem)
		{
			return;
		}
		this.currentCosmeticItem = CosmeticsController.instance.nullItem;
		if (this.currentCosmeticSprite.IsNotNull())
		{
			this.currentCosmeticSprite.sprite = this.blankSprite;
		}
		this.isOn = false;
		this.UpdateColor();
	}

	public CosmeticsController.CosmeticItem currentCosmeticItem;

	[SerializeField]
	private SpriteRenderer currentCosmeticSprite;

	[SerializeField]
	private Sprite blankSprite;

	public string noCosmeticText;
}
