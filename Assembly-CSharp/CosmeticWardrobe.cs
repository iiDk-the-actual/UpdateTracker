using System;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class CosmeticWardrobe : MonoBehaviour
{
	public bool UseTemporarySet
	{
		get
		{
			return this.m_useTemporarySet;
		}
		set
		{
			bool flag = value != this.m_useTemporarySet;
			this.m_useTemporarySet = value;
			if (flag)
			{
				this.HandleCosmeticsUpdated();
			}
		}
	}

	private void Start()
	{
		for (int i = 0; i < this.cosmeticCategoryButtons.Length; i++)
		{
			if (this.cosmeticCategoryButtons[i].category == CosmeticWardrobe.selectedCategory)
			{
				CosmeticWardrobe.selectedCategoryIndex = i;
				break;
			}
		}
		for (int j = 0; j < this.cosmeticCollectionDisplays.Length; j++)
		{
			this.cosmeticCollectionDisplays[j].displayHead.transform.localScale = this.startingHeadSize;
		}
		if (GorillaTagger.Instance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.OnColorChanged += this.HandleLocalColorChanged;
			this.HandleLocalColorChanged(GorillaTagger.Instance.offlineVRRig.playerColor);
		}
		this.nextSelection.onPressed += this.HandlePressedNextSelection;
		this.prevSelection.onPressed += this.HandlePressedPrevSelection;
		for (int k = 0; k < this.cosmeticCollectionDisplays.Length; k++)
		{
			this.cosmeticCollectionDisplays[k].selectButton.onPressed += this.HandlePressedSelectCosmeticButton;
		}
		for (int l = 0; l < this.cosmeticCategoryButtons.Length; l++)
		{
			this.cosmeticCategoryButtons[l].button.onPressed += this.HandleChangeCategory;
			this.cosmeticCategoryButtons[l].slot1RemovedItem = CosmeticsController.instance.nullItem;
			this.cosmeticCategoryButtons[l].slot2RemovedItem = CosmeticsController.instance.nullItem;
		}
		CosmeticsController instance = CosmeticsController.instance;
		instance.OnCosmeticsUpdated = (Action)Delegate.Combine(instance.OnCosmeticsUpdated, new Action(this.HandleCosmeticsUpdated));
		CosmeticsController instance2 = CosmeticsController.instance;
		instance2.OnOutfitsUpdated = (Action)Delegate.Combine(instance2.OnOutfitsUpdated, new Action(this.UpdateOutfitButtons));
		CosmeticWardrobe.OnWardrobeUpdateCategories = (Action)Delegate.Combine(CosmeticWardrobe.OnWardrobeUpdateCategories, new Action(this.UpdateCategoryButtons));
		CosmeticWardrobe.OnWardrobeUpdateDisplays = (Action)Delegate.Combine(CosmeticWardrobe.OnWardrobeUpdateDisplays, new Action(this.UpdateCosmeticDisplays));
		this.previousOutfit.onPressed += this.HandlePressedPrevOutfitButton;
		this.nextOutfit.onPressed += this.HandlePressedNextOutfitButton;
		this.HandleCosmeticsUpdated();
	}

	private void OnDestroy()
	{
		if (GorillaTagger.Instance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.OnColorChanged -= this.HandleLocalColorChanged;
		}
		this.nextSelection.onPressed -= this.HandlePressedNextSelection;
		this.prevSelection.onPressed -= this.HandlePressedPrevSelection;
		for (int i = 0; i < this.cosmeticCollectionDisplays.Length; i++)
		{
			this.cosmeticCollectionDisplays[i].selectButton.onPressed -= this.HandlePressedSelectCosmeticButton;
		}
		for (int j = 0; j < this.cosmeticCategoryButtons.Length; j++)
		{
			this.cosmeticCategoryButtons[j].button.onPressed -= this.HandleChangeCategory;
		}
		CosmeticsController instance = CosmeticsController.instance;
		instance.OnCosmeticsUpdated = (Action)Delegate.Remove(instance.OnCosmeticsUpdated, new Action(this.HandleCosmeticsUpdated));
		CosmeticsController instance2 = CosmeticsController.instance;
		instance2.OnOutfitsUpdated = (Action)Delegate.Remove(instance2.OnOutfitsUpdated, new Action(this.UpdateOutfitButtons));
		CosmeticWardrobe.OnWardrobeUpdateCategories = (Action)Delegate.Remove(CosmeticWardrobe.OnWardrobeUpdateCategories, new Action(this.UpdateCategoryButtons));
		CosmeticWardrobe.OnWardrobeUpdateDisplays = (Action)Delegate.Remove(CosmeticWardrobe.OnWardrobeUpdateDisplays, new Action(this.UpdateCosmeticDisplays));
		this.previousOutfit.onPressed -= this.HandlePressedPrevOutfitButton;
		this.nextOutfit.onPressed -= this.HandlePressedNextOutfitButton;
	}

	private void HandlePressedNextSelection(GorillaPressableButton button, bool isLeft)
	{
		CosmeticWardrobe.startingDisplayIndex += this.cosmeticCollectionDisplays.Length;
		if (CosmeticWardrobe.startingDisplayIndex >= CosmeticsController.instance.GetCategorySize(CosmeticWardrobe.selectedCategory))
		{
			CosmeticWardrobe.startingDisplayIndex = 0;
		}
		Action onWardrobeUpdateDisplays = CosmeticWardrobe.OnWardrobeUpdateDisplays;
		if (onWardrobeUpdateDisplays == null)
		{
			return;
		}
		onWardrobeUpdateDisplays();
	}

	private void HandlePressedPrevSelection(GorillaPressableButton button, bool isLeft)
	{
		CosmeticWardrobe.startingDisplayIndex -= this.cosmeticCollectionDisplays.Length;
		if (CosmeticWardrobe.startingDisplayIndex < 0)
		{
			int categorySize = CosmeticsController.instance.GetCategorySize(CosmeticWardrobe.selectedCategory);
			int num;
			if (categorySize % this.cosmeticCollectionDisplays.Length == 0)
			{
				num = categorySize - this.cosmeticCollectionDisplays.Length;
			}
			else
			{
				num = categorySize / this.cosmeticCollectionDisplays.Length;
				num *= this.cosmeticCollectionDisplays.Length;
			}
			CosmeticWardrobe.startingDisplayIndex = num;
		}
		Action onWardrobeUpdateDisplays = CosmeticWardrobe.OnWardrobeUpdateDisplays;
		if (onWardrobeUpdateDisplays == null)
		{
			return;
		}
		onWardrobeUpdateDisplays();
	}

	private void HandlePressedSelectCosmeticButton(GorillaPressableButton button, bool isLeft)
	{
		int i = 0;
		while (i < this.cosmeticCollectionDisplays.Length)
		{
			if (this.cosmeticCollectionDisplays[i].selectButton == button)
			{
				CosmeticsController.instance.PressWardrobeItemButton(this.cosmeticCollectionDisplays[i].currentCosmeticItem, isLeft, this.m_useTemporarySet);
				if (isLeft)
				{
					this.cosmeticCategoryButtons[CosmeticWardrobe.selectedCategoryIndex].slot2RemovedItem = CosmeticsController.instance.nullItem;
					return;
				}
				this.cosmeticCategoryButtons[CosmeticWardrobe.selectedCategoryIndex].slot1RemovedItem = CosmeticsController.instance.nullItem;
				return;
			}
			else
			{
				i++;
			}
		}
	}

	private void HandleChangeCategory(GorillaPressableButton button, bool isLeft)
	{
		for (int i = 0; i < this.cosmeticCategoryButtons.Length; i++)
		{
			CosmeticWardrobe.CosmeticWardrobeCategory cosmeticWardrobeCategory = this.cosmeticCategoryButtons[i];
			if (cosmeticWardrobeCategory.button == button)
			{
				if (CosmeticWardrobe.selectedCategory == cosmeticWardrobeCategory.category)
				{
					CosmeticsController.CosmeticItem cosmeticItem = CosmeticsController.instance.nullItem;
					if (cosmeticWardrobeCategory.slot1 != CosmeticsController.CosmeticSlots.Count)
					{
						cosmeticItem = CosmeticsController.instance.GetSlotItem(cosmeticWardrobeCategory.slot1, true, this.m_useTemporarySet);
					}
					CosmeticsController.CosmeticItem cosmeticItem2 = CosmeticsController.instance.nullItem;
					if (cosmeticWardrobeCategory.slot2 != CosmeticsController.CosmeticSlots.Count)
					{
						cosmeticItem2 = CosmeticsController.instance.GetSlotItem(cosmeticWardrobeCategory.slot2, true, this.m_useTemporarySet);
					}
					bool flag = CosmeticWardrobe.selectedCategory == CosmeticsController.CosmeticCategory.Arms;
					if (!cosmeticItem.isNullItem || !cosmeticItem2.isNullItem)
					{
						if (!cosmeticItem.isNullItem)
						{
							cosmeticWardrobeCategory.slot1RemovedItem = cosmeticItem;
							CosmeticsController.instance.PressWardrobeItemButton(cosmeticItem, flag, this.m_useTemporarySet);
						}
						if (!cosmeticItem2.isNullItem)
						{
							cosmeticWardrobeCategory.slot2RemovedItem = cosmeticItem2;
							CosmeticsController.instance.PressWardrobeItemButton(cosmeticItem2, !flag, this.m_useTemporarySet);
						}
						Action onWardrobeUpdateDisplays = CosmeticWardrobe.OnWardrobeUpdateDisplays;
						if (onWardrobeUpdateDisplays != null)
						{
							onWardrobeUpdateDisplays();
						}
						Action onWardrobeUpdateCategories = CosmeticWardrobe.OnWardrobeUpdateCategories;
						if (onWardrobeUpdateCategories == null)
						{
							return;
						}
						onWardrobeUpdateCategories();
						return;
					}
					else if (!cosmeticWardrobeCategory.slot1RemovedItem.isNullItem || !cosmeticWardrobeCategory.slot2RemovedItem.isNullItem)
					{
						if (!cosmeticWardrobeCategory.slot1RemovedItem.isNullItem)
						{
							CosmeticsController.instance.PressWardrobeItemButton(cosmeticWardrobeCategory.slot1RemovedItem, flag, this.m_useTemporarySet);
							cosmeticWardrobeCategory.slot1RemovedItem = CosmeticsController.instance.nullItem;
						}
						if (!cosmeticWardrobeCategory.slot2RemovedItem.isNullItem)
						{
							CosmeticsController.instance.PressWardrobeItemButton(cosmeticWardrobeCategory.slot2RemovedItem, !flag, this.m_useTemporarySet);
							cosmeticWardrobeCategory.slot2RemovedItem = CosmeticsController.instance.nullItem;
						}
						Action onWardrobeUpdateDisplays2 = CosmeticWardrobe.OnWardrobeUpdateDisplays;
						if (onWardrobeUpdateDisplays2 != null)
						{
							onWardrobeUpdateDisplays2();
						}
						Action onWardrobeUpdateCategories2 = CosmeticWardrobe.OnWardrobeUpdateCategories;
						if (onWardrobeUpdateCategories2 == null)
						{
							return;
						}
						onWardrobeUpdateCategories2();
						return;
					}
				}
				else
				{
					CosmeticWardrobe.selectedCategory = cosmeticWardrobeCategory.category;
					CosmeticWardrobe.selectedCategoryIndex = i;
					CosmeticWardrobe.startingDisplayIndex = 0;
					Action onWardrobeUpdateDisplays3 = CosmeticWardrobe.OnWardrobeUpdateDisplays;
					if (onWardrobeUpdateDisplays3 != null)
					{
						onWardrobeUpdateDisplays3();
					}
					Action onWardrobeUpdateCategories3 = CosmeticWardrobe.OnWardrobeUpdateCategories;
					if (onWardrobeUpdateCategories3 == null)
					{
						return;
					}
					onWardrobeUpdateCategories3();
				}
				return;
			}
		}
	}

	private void HandleCosmeticsUpdated()
	{
		string[] currentlyWornCosmetics = CosmeticsController.instance.GetCurrentlyWornCosmetics(this.m_useTemporarySet);
		bool[] currentRightEquippedSided = CosmeticsController.instance.GetCurrentRightEquippedSided(this.m_useTemporarySet);
		this.currentEquippedDisplay.SetCosmeticActiveArray(currentlyWornCosmetics, currentRightEquippedSided);
		this.UpdateCategoryButtons();
		this.UpdateCosmeticDisplays();
		this.UpdateOutfitButtons();
	}

	private void HandleLocalColorChanged(Color newColor)
	{
		MeshRenderer component = this.currentEquippedDisplay.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.material.color = newColor;
		}
	}

	private void HandlePressedPrevOutfitButton(GorillaPressableButton button, bool isLeft)
	{
		CosmeticsController.instance.PressWardrobeScrollOutfit(false);
	}

	private void HandlePressedNextOutfitButton(GorillaPressableButton button, bool isLeft)
	{
		CosmeticsController.instance.PressWardrobeScrollOutfit(true);
	}

	private void UpdateCosmeticDisplays()
	{
		for (int i = 0; i < this.cosmeticCollectionDisplays.Length; i++)
		{
			CosmeticsController.CosmeticItem cosmetic = CosmeticsController.instance.GetCosmetic(CosmeticWardrobe.selectedCategory, CosmeticWardrobe.startingDisplayIndex + i);
			this.cosmeticCollectionDisplays[i].currentCosmeticItem = cosmetic;
			this.cosmeticCollectionDisplays[i].displayHead.SetCosmeticActive(cosmetic.displayName, false);
			this.cosmeticCollectionDisplays[i].selectButton.enabled = !cosmetic.isNullItem;
			this.cosmeticCollectionDisplays[i].selectButton.isOn = !cosmetic.isNullItem && CosmeticsController.instance.IsCosmeticEquipped(cosmetic, this.m_useTemporarySet);
			this.cosmeticCollectionDisplays[i].selectButton.UpdateColor();
		}
		int categorySize = CosmeticsController.instance.GetCategorySize(CosmeticWardrobe.selectedCategory);
		this.nextSelection.enabled = categorySize > this.cosmeticCollectionDisplays.Length;
		this.nextSelection.UpdateColor();
		this.prevSelection.enabled = categorySize > this.cosmeticCollectionDisplays.Length;
		this.prevSelection.UpdateColor();
	}

	private void UpdateCategoryButtons()
	{
		for (int i = 0; i < this.cosmeticCategoryButtons.Length; i++)
		{
			CosmeticWardrobe.CosmeticWardrobeCategory cosmeticWardrobeCategory = this.cosmeticCategoryButtons[i];
			if (cosmeticWardrobeCategory.slot1 != CosmeticsController.CosmeticSlots.Count)
			{
				CosmeticsController.CosmeticItem slotItem = CosmeticsController.instance.GetSlotItem(cosmeticWardrobeCategory.slot1, false, this.m_useTemporarySet);
				if (cosmeticWardrobeCategory.slot2 != CosmeticsController.CosmeticSlots.Count)
				{
					CosmeticsController.CosmeticItem slotItem2 = CosmeticsController.instance.GetSlotItem(cosmeticWardrobeCategory.slot2, false, this.m_useTemporarySet);
					if (slotItem.bothHandsHoldable)
					{
						cosmeticWardrobeCategory.button.SetIcon(slotItem.isNullItem ? null : slotItem.itemPicture);
					}
					else if (slotItem2.bothHandsHoldable)
					{
						cosmeticWardrobeCategory.button.SetIcon(slotItem2.isNullItem ? null : slotItem2.itemPicture);
					}
					else
					{
						cosmeticWardrobeCategory.button.SetDualIcon(slotItem.isNullItem ? null : slotItem.itemPicture, slotItem2.isNullItem ? null : slotItem2.itemPicture);
					}
				}
				else
				{
					cosmeticWardrobeCategory.button.SetIcon(slotItem.isNullItem ? null : slotItem.itemPicture);
				}
			}
			int categorySize = CosmeticsController.instance.GetCategorySize(cosmeticWardrobeCategory.category);
			cosmeticWardrobeCategory.button.enabled = categorySize > 0;
			cosmeticWardrobeCategory.button.isOn = CosmeticWardrobe.selectedCategory == cosmeticWardrobeCategory.category;
			cosmeticWardrobeCategory.button.UpdateColor();
		}
	}

	private void UpdateOutfitButtons()
	{
		bool flag = CosmeticsController.CanScrollOutfits();
		int num = CosmeticsController.SelectedOutfit + 1;
		this.nextOutfit.enabled = flag;
		this.previousOutfit.enabled = flag;
		this.nextOutfit.UpdateColor();
		this.previousOutfit.UpdateColor();
		this.outfitText.text = "Outfit #" + num.ToString();
		this.outfitTextOutline.text = "Outfit #" + num.ToString();
	}

	public bool WardrobeButtonsInitialized()
	{
		for (int i = 0; i < this.cosmeticCategoryButtons.Length; i++)
		{
			if (!this.cosmeticCategoryButtons[i].button.Initialized)
			{
				return false;
			}
		}
		for (int i = 0; i < this.cosmeticCollectionDisplays.Length; i++)
		{
			if (!this.cosmeticCollectionDisplays[i].selectButton.Initialized)
			{
				return false;
			}
		}
		return true;
	}

	[SerializeField]
	private CosmeticWardrobe.CosmeticWardrobeSelection[] cosmeticCollectionDisplays;

	[SerializeField]
	private CosmeticWardrobe.CosmeticWardrobeCategory[] cosmeticCategoryButtons;

	[SerializeField]
	private HeadModel currentEquippedDisplay;

	[SerializeField]
	private GorillaPressableButton nextSelection;

	[SerializeField]
	private GorillaPressableButton prevSelection;

	[SerializeField]
	private bool m_useTemporarySet;

	[SerializeField]
	private CosmeticButton previousOutfit;

	[SerializeField]
	private CosmeticButton nextOutfit;

	[SerializeField]
	private TMP_Text outfitText;

	[SerializeField]
	private TMP_Text outfitTextOutline;

	private static int selectedCategoryIndex = 0;

	private static CosmeticsController.CosmeticCategory selectedCategory = CosmeticsController.CosmeticCategory.Hat;

	private static int startingDisplayIndex = 0;

	private static int selectedOutfitIndex = 0;

	private static Action OnWardrobeUpdateCategories;

	private static Action OnWardrobeUpdateDisplays;

	public Vector3 startingHeadSize = new Vector3(0.25f, 0.25f, 0.25f);

	[Serializable]
	public class CosmeticWardrobeSelection
	{
		public HeadModel displayHead;

		public CosmeticButton selectButton;

		public CosmeticsController.CosmeticItem currentCosmeticItem;
	}

	[Serializable]
	public class CosmeticWardrobeCategory
	{
		public CosmeticCategoryButton button;

		public CosmeticsController.CosmeticCategory category;

		public CosmeticsController.CosmeticSlots slot1 = CosmeticsController.CosmeticSlots.Count;

		public CosmeticsController.CosmeticSlots slot2 = CosmeticsController.CosmeticSlots.Count;

		public CosmeticsController.CosmeticItem slot1RemovedItem;

		public CosmeticsController.CosmeticItem slot2RemovedItem;
	}
}
