using System;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticStand : GorillaPressableButton
{
	public void InitializeCosmetic()
	{
		this.thisCosmeticItem = CosmeticsController.instance.allCosmetics.Find((CosmeticsController.CosmeticItem x) => this.thisCosmeticName == x.displayName || this.thisCosmeticName == x.overrideDisplayName || this.thisCosmeticName == x.itemName);
		if (this.slotPriceText != null)
		{
			this.slotPriceText.text = this.thisCosmeticItem.itemCategory.ToString().ToUpper() + " " + this.thisCosmeticItem.cost.ToString();
		}
	}

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		CosmeticsController.instance.PressCosmeticStandButton(this);
	}

	public CosmeticsController.CosmeticItem thisCosmeticItem;

	public string thisCosmeticName;

	public HeadModel thisHeadModel;

	public Text slotPriceText;

	public Text addToCartText;

	[Tooltip("If this is true then this cosmetic stand should have already been updated when the 'Update Cosmetic Stands' button was pressed in the CosmeticsController inspector.")]
	public bool skipMe;
}
