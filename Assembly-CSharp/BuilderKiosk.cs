using System;
using System.Collections;
using System.Collections.Generic;
using GameObjectScheduling;
using GorillaNetworking;
using GorillaTagScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class BuilderKiosk : MonoBehaviour
{
	private void Awake()
	{
		BuilderKiosk.nullItem = new BuilderSetManager.BuilderSetStoreItem
		{
			displayName = "NOTHING",
			playfabID = "NULL",
			isNullItem = true
		};
	}

	private void Start()
	{
		this._puchaseTextLocStr = this._puchaseTextLoc.StringReference;
		this._itemNameVar = this._puchaseTextLocStr["item-name"] as StringVariable;
		this._itemCostVar = this._puchaseTextLocStr["item-cost"] as IntVariable;
		this._currencyBalanceVar = this._puchaseTextLocStr["currency-balance"] as IntVariable;
		this._finalLineVar = this._puchaseTextLocStr["final-line-index"] as IntVariable;
		this.purchaseParticles.Stop();
		BuilderSetManager.instance.OnOwnedSetsUpdated.AddListener(new UnityAction(this.OnOwnedSetsUpdated));
		CosmeticsController instance = CosmeticsController.instance;
		instance.OnGetCurrency = (Action)Delegate.Combine(instance.OnGetCurrency, new Action(this.OnUpdateCurrencyBalance));
		this.leftPurchaseButton.onPressed += this.PressLeftPurchaseItemButton;
		this.rightPurchaseButton.onPressed += this.PressRightPurchaseItemButton;
		BuilderTable builderTable;
		if (BuilderTable.TryGetBuilderTableForZone(GTZone.monkeBlocks, out builderTable))
		{
			builderTable.OnTableConfigurationUpdated.AddListener(new UnityAction(this.UpdateCountdown));
		}
		this.UpdateCountdown();
		this.availableItems.Clear();
		if (this.isMiniKiosk)
		{
			this.availableItems.Add(this.pieceSetForSale);
		}
		else
		{
			this.availableItems.AddRange(BuilderSetManager.instance.GetPermanentSetsForSale());
			this.availableItems.AddRange(BuilderSetManager.instance.GetSeasonalSetsForSale());
		}
		if (!this.isMiniKiosk)
		{
			this.SetupSetButtons();
		}
		if (this.availableItems.Count <= 0 || !BuilderSetManager.instance.pulledStoreItems)
		{
			this.itemToBuy = BuilderKiosk.nullItem;
			return;
		}
		this.hasInitFromPlayfab = true;
		if (this.pieceSetForSale != null)
		{
			this.itemToBuy = BuilderSetManager.instance.GetStoreItemFromSetID(this.pieceSetForSale.GetIntIdentifier());
			this.UpdateLabels();
			this.UpdateDiorama();
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
			this.ProcessPurchaseItemState(null, false);
			return;
		}
		this.itemToBuy = BuilderKiosk.nullItem;
		this.UpdateLabels();
		this.UpdateDiorama();
		this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
		this.ProcessPurchaseItemState(null, false);
	}

	private void UpdateCountdown()
	{
		if (!this.useTitleCountDown)
		{
			return;
		}
		if (!string.IsNullOrEmpty(BuilderTable.nextUpdateOverride) && !BuilderTable.nextUpdateOverride.Equals(this.countdownOverride))
		{
			this.countdownOverride = BuilderTable.nextUpdateOverride;
			CountdownTextDate countdown = this.countdownText.Countdown;
			countdown.CountdownTo = this.countdownOverride;
			this.countdownText.Countdown = countdown;
		}
	}

	private void SetupSetButtons()
	{
		this.setsPerPage = this.setButtons.Length;
		this.totalPages = this.availableItems.Count / this.setsPerPage;
		if (this.availableItems.Count % this.setsPerPage > 0)
		{
			this.totalPages++;
		}
		this.previousPageButton.gameObject.SetActive(this.totalPages > 1);
		this.nextPageButton.gameObject.SetActive(this.totalPages > 1);
		this.previousPageButton.myTmpText.enabled = this.totalPages > 1;
		this.nextPageButton.myTmpText.enabled = this.totalPages > 1;
		this.previousPageButton.onPressButton.AddListener(new UnityAction(this.OnPreviousPageClicked));
		this.nextPageButton.onPressButton.AddListener(new UnityAction(this.OnNextPageClicked));
		GorillaPressableButton[] array = this.setButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].onPressed += this.OnSetButtonPressed;
		}
		this.UpdateLabels();
	}

	private void OnDestroy()
	{
		if (this.leftPurchaseButton != null)
		{
			this.leftPurchaseButton.onPressed -= this.PressLeftPurchaseItemButton;
		}
		if (this.rightPurchaseButton != null)
		{
			this.rightPurchaseButton.onPressed -= this.PressRightPurchaseItemButton;
		}
		if (BuilderSetManager.instance != null)
		{
			BuilderSetManager.instance.OnOwnedSetsUpdated.RemoveListener(new UnityAction(this.OnOwnedSetsUpdated));
		}
		if (CosmeticsController.instance != null)
		{
			CosmeticsController instance = CosmeticsController.instance;
			instance.OnGetCurrency = (Action)Delegate.Remove(instance.OnGetCurrency, new Action(this.OnUpdateCurrencyBalance));
		}
		if (!this.isMiniKiosk)
		{
			foreach (GorillaPressableButton gorillaPressableButton in this.setButtons)
			{
				if (gorillaPressableButton != null)
				{
					gorillaPressableButton.onPressed -= this.OnSetButtonPressed;
				}
			}
			if (this.previousPageButton != null)
			{
				this.previousPageButton.onPressButton.RemoveListener(new UnityAction(this.OnPreviousPageClicked));
			}
			if (this.nextPageButton != null)
			{
				this.nextPageButton.onPressButton.RemoveListener(new UnityAction(this.OnNextPageClicked));
			}
		}
		if (this.currentDiorama != null)
		{
			Object.Destroy(this.currentDiorama);
			this.currentDiorama = null;
		}
		if (this.nextDiorama != null)
		{
			Object.Destroy(this.nextDiorama);
			this.nextDiorama = null;
		}
		BuilderTable builderTable;
		if (BuilderTable.TryGetBuilderTableForZone(GTZone.monkeBlocks, out builderTable))
		{
			builderTable.OnTableConfigurationUpdated.RemoveListener(new UnityAction(this.UpdateCountdown));
		}
	}

	private void OnOwnedSetsUpdated()
	{
		if (this.hasInitFromPlayfab || !BuilderSetManager.instance.pulledStoreItems)
		{
			if (this.currentPurchaseItemStage == CosmeticsController.PurchaseItemStages.Start || this.currentPurchaseItemStage == CosmeticsController.PurchaseItemStages.CheckoutButtonPressed)
			{
				this.ProcessPurchaseItemState(null, false);
			}
			return;
		}
		this.hasInitFromPlayfab = true;
		this.availableItems.Clear();
		if (this.isMiniKiosk)
		{
			this.availableItems.Add(this.pieceSetForSale);
		}
		else
		{
			this.availableItems.AddRange(BuilderSetManager.instance.GetPermanentSetsForSale());
			this.availableItems.AddRange(BuilderSetManager.instance.GetSeasonalSetsForSale());
		}
		if (this.pieceSetForSale != null)
		{
			this.itemToBuy = BuilderSetManager.instance.GetStoreItemFromSetID(this.pieceSetForSale.GetIntIdentifier());
			this.UpdateLabels();
			this.UpdateDiorama();
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
			this.ProcessPurchaseItemState(null, false);
			return;
		}
		this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
		this.ProcessPurchaseItemState(null, false);
	}

	private void OnSetButtonPressed(GorillaPressableButton button, bool isLeft)
	{
		if (this.currentPurchaseItemStage != CosmeticsController.PurchaseItemStages.Buying && !this.animating)
		{
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
			int num = 0;
			for (int i = 0; i < this.setButtons.Length; i++)
			{
				if (button.Equals(this.setButtons[i]))
				{
					num = i;
					break;
				}
			}
			int num2 = this.pageIndex * this.setsPerPage + num;
			if (num2 < this.availableItems.Count)
			{
				BuilderPieceSet builderPieceSet = this.availableItems[num2];
				if (builderPieceSet.SetName.Equals(this.itemToBuy.displayName))
				{
					this.itemToBuy = BuilderKiosk.nullItem;
					this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
				}
				else
				{
					this.itemToBuy = BuilderSetManager.instance.GetStoreItemFromSetID(builderPieceSet.GetIntIdentifier());
					this.UpdateLabels();
					this.UpdateDiorama();
				}
				this.ProcessPurchaseItemState(null, isLeft);
			}
		}
	}

	private void OnPreviousPageClicked()
	{
		int num = Mathf.Clamp(this.pageIndex - 1, 0, this.totalPages - 1);
		if (num != this.pageIndex)
		{
			this.pageIndex = num;
			this.UpdateLabels();
		}
	}

	private void OnNextPageClicked()
	{
		int num = Mathf.Clamp(this.pageIndex + 1, 0, this.totalPages - 1);
		if (num != this.pageIndex)
		{
			this.pageIndex = num;
			this.UpdateLabels();
		}
	}

	private void UpdateLabels()
	{
		if (this.isMiniKiosk)
		{
			return;
		}
		for (int i = 0; i < this.setButtons.Length; i++)
		{
			int num = this.pageIndex * this.setsPerPage + i;
			if (num < this.availableItems.Count && this.availableItems[num] != null)
			{
				if (!this.setButtons[i].gameObject.activeSelf)
				{
					this.setButtons[i].gameObject.SetActive(true);
					this.setButtons[i].myTmpText.gameObject.SetActive(true);
				}
				if (this.setButtons[i].myTmpText.text != this.availableItems[num].SetName.ToUpper())
				{
					this.setButtons[i].myTmpText.text = this.availableItems[num].SetName.ToUpper();
				}
				bool flag = !this.itemToBuy.isNullItem && this.availableItems[num].playfabID == this.itemToBuy.playfabID;
				if (flag != this.setButtons[i].isOn || !this.setButtons[i].enabled)
				{
					this.setButtons[i].isOn = flag;
					this.setButtons[i].buttonRenderer.material = (flag ? this.setButtons[i].pressedMaterial : this.setButtons[i].unpressedMaterial);
				}
				this.setButtons[i].enabled = true;
			}
			else
			{
				if (this.setButtons[i].gameObject.activeSelf)
				{
					this.setButtons[i].gameObject.SetActive(false);
					this.setButtons[i].myTmpText.gameObject.SetActive(false);
				}
				if (this.setButtons[i].isOn || this.setButtons[i].enabled)
				{
					this.setButtons[i].isOn = false;
					this.setButtons[i].enabled = false;
				}
			}
		}
		bool flag2 = this.pageIndex > 0 && this.totalPages > 1;
		bool flag3 = this.pageIndex < this.totalPages - 1 && this.totalPages > 1;
		if (this.previousPageButton.myTmpText.enabled != flag2)
		{
			this.previousPageButton.myTmpText.enabled = flag2;
		}
		if (this.nextPageButton.myTmpText.enabled != flag3)
		{
			this.nextPageButton.myTmpText.enabled = flag3;
		}
	}

	public void UpdateDiorama()
	{
		if (this.isMiniKiosk)
		{
			return;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.itemToBuy.isNullItem)
		{
			this.countdownText.gameObject.SetActive(false);
		}
		else
		{
			this.countdownText.gameObject.SetActive(BuilderSetManager.instance.IsSetSeasonal(this.itemToBuy.playfabID));
		}
		if (this.animating)
		{
			base.StopCoroutine(this.PlaySwapAnimation());
			if (this.currentDiorama != null)
			{
				Object.Destroy(this.currentDiorama);
				this.currentDiorama = null;
			}
			this.currentDiorama = this.nextDiorama;
			this.nextDiorama = null;
		}
		this.animating = true;
		if (this.nextDiorama != null)
		{
			Object.Destroy(this.nextDiorama);
			this.nextDiorama = null;
		}
		if (!this.itemToBuy.isNullItem && this.itemToBuy.displayModel != null)
		{
			this.nextDiorama = Object.Instantiate<GameObject>(this.itemToBuy.displayModel, this.nextItemDisplayPos);
		}
		else
		{
			this.nextDiorama = Object.Instantiate<GameObject>(this.emptyDisplay, this.nextItemDisplayPos);
		}
		this.itemDisplayAnimation.Rewind();
		if (this.currentDiorama != null)
		{
			this.currentDiorama.transform.SetParent(this.itemDisplayPos, false);
		}
		base.StartCoroutine(this.PlaySwapAnimation());
	}

	private IEnumerator PlaySwapAnimation()
	{
		this.itemDisplayAnimation.Play();
		yield return new WaitForSeconds(this.itemDisplayAnimation.clip.length);
		if (this.currentDiorama != null)
		{
			Object.Destroy(this.currentDiorama);
			this.currentDiorama = null;
		}
		this.currentDiorama = this.nextDiorama;
		this.nextDiorama = null;
		this.animating = false;
		yield break;
	}

	public void PressLeftPurchaseItemButton(GorillaPressableButton pressedPurchaseItemButton, bool isLeftHand)
	{
		if (this.currentPurchaseItemStage != CosmeticsController.PurchaseItemStages.Start && !this.animating)
		{
			this.ProcessPurchaseItemState("left", isLeftHand);
		}
	}

	public void PressRightPurchaseItemButton(GorillaPressableButton pressedPurchaseItemButton, bool isLeftHand)
	{
		if (this.currentPurchaseItemStage != CosmeticsController.PurchaseItemStages.Start && !this.animating)
		{
			this.ProcessPurchaseItemState("right", isLeftHand);
		}
	}

	public void OnUpdateCurrencyBalance()
	{
		if (this.currentPurchaseItemStage == CosmeticsController.PurchaseItemStages.Start || this.currentPurchaseItemStage == CosmeticsController.PurchaseItemStages.CheckoutButtonPressed || this.currentPurchaseItemStage == CosmeticsController.PurchaseItemStages.ItemOwned)
		{
			this.ProcessPurchaseItemState(null, false);
		}
	}

	public void ClearCheckout()
	{
		GorillaTelemetry.PostBuilderKioskEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.checkout_cancel, this.itemToBuy);
		this.itemToBuy = BuilderKiosk.nullItem;
		this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
	}

	public void ProcessPurchaseItemState(string buttonSide, bool isLeftHand)
	{
		switch (this.currentPurchaseItemStage)
		{
		case CosmeticsController.PurchaseItemStages.Start:
			this.itemToBuy = BuilderKiosk.nullItem;
			this.FormattedPurchaseText(0);
			this.leftPurchaseButton.myTmpText.text = "-";
			this.rightPurchaseButton.myTmpText.text = "-";
			this.UpdateLabels();
			this.UpdateDiorama();
			return;
		case CosmeticsController.PurchaseItemStages.CheckoutButtonPressed:
			if (this.availableItems.Count > 1)
			{
				GorillaTelemetry.PostBuilderKioskEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.checkout_start, this.itemToBuy);
			}
			if (BuilderSetManager.instance.IsPieceSetOwnedLocally(this.itemToBuy.setID))
			{
				this.FormattedPurchaseText(1);
				this.leftPurchaseButton.myTmpText.text = "-";
				this.rightPurchaseButton.myTmpText.text = "-";
				this.leftPurchaseButton.buttonRenderer.material = this.leftPurchaseButton.pressedMaterial;
				this.rightPurchaseButton.buttonRenderer.material = this.rightPurchaseButton.pressedMaterial;
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.ItemOwned;
				return;
			}
			if ((ulong)this.itemToBuy.cost <= (ulong)((long)CosmeticsController.instance.currencyBalance))
			{
				this.FormattedPurchaseText(2);
				string text;
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_PURCHASE_BUTTON_WANT_TO_BUY_CANCEL", out text, "NO!");
				string text2;
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_PURCHASE_BUTTON_WANT_TO_BUY_CONFIRM", out text2, "YES!");
				this.leftPurchaseButton.myTmpText.text = text;
				this.rightPurchaseButton.myTmpText.text = text2;
				this.leftPurchaseButton.buttonRenderer.material = this.leftPurchaseButton.unpressedMaterial;
				this.rightPurchaseButton.buttonRenderer.material = this.rightPurchaseButton.unpressedMaterial;
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.ItemSelected;
				return;
			}
			this.FormattedPurchaseText(3);
			this.leftPurchaseButton.myTmpText.text = "-";
			this.rightPurchaseButton.myTmpText.text = "-";
			this.leftPurchaseButton.buttonRenderer.material = this.leftPurchaseButton.pressedMaterial;
			this.rightPurchaseButton.buttonRenderer.material = this.rightPurchaseButton.pressedMaterial;
			if (!this.isMiniKiosk)
			{
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Start;
				return;
			}
			break;
		case CosmeticsController.PurchaseItemStages.ItemSelected:
			if (buttonSide == "right")
			{
				GorillaTelemetry.PostBuilderKioskEvent(GorillaTagger.Instance.offlineVRRig, GTShopEventType.item_select, this.itemToBuy);
				this.FormattedPurchaseText(4);
				string text3;
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_PURCHASE_BUTTON_CONFIRMATION_CANCEL", out text3, "LET ME THINK ABOUT IT");
				string text4;
				LocalisationManager.TryGetKeyForCurrentLocale("MONKE_BLOCKS_PURCHASE_BUTTON_CONFIRMATION_CONFIRM", out text4, "YES! I NEED IT!");
				this.leftPurchaseButton.myTmpText.text = text4;
				this.rightPurchaseButton.myTmpText.text = text3;
				this.leftPurchaseButton.buttonRenderer.material = this.leftPurchaseButton.unpressedMaterial;
				this.rightPurchaseButton.buttonRenderer.material = this.rightPurchaseButton.unpressedMaterial;
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.FinalPurchaseAcknowledgement;
				return;
			}
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
			this.ProcessPurchaseItemState(null, isLeftHand);
			return;
		case CosmeticsController.PurchaseItemStages.ItemOwned:
		case CosmeticsController.PurchaseItemStages.Buying:
			break;
		case CosmeticsController.PurchaseItemStages.FinalPurchaseAcknowledgement:
			if (buttonSide == "left")
			{
				this.FormattedPurchaseText(5);
				this.leftPurchaseButton.myTmpText.text = "-";
				this.rightPurchaseButton.myTmpText.text = "-";
				this.leftPurchaseButton.buttonRenderer.material = this.leftPurchaseButton.pressedMaterial;
				this.rightPurchaseButton.buttonRenderer.material = this.rightPurchaseButton.pressedMaterial;
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Buying;
				this.isLastHandTouchedLeft = isLeftHand;
				this.PurchaseItem();
				return;
			}
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.CheckoutButtonPressed;
			this.ProcessPurchaseItemState(null, isLeftHand);
			return;
		case CosmeticsController.PurchaseItemStages.Success:
		{
			this.FormattedPurchaseText(7);
			this.audioSource.GTPlayOneShot(this.purchaseSetAudioClip, 1f);
			this.purchaseParticles.Play();
			VRRig offlineVRRig = GorillaTagger.Instance.offlineVRRig;
			offlineVRRig.concatStringOfCosmeticsAllowed += this.itemToBuy.playfabID;
			this.leftPurchaseButton.myTmpText.text = "-";
			this.rightPurchaseButton.myTmpText.text = "-";
			this.leftPurchaseButton.buttonRenderer.material = this.leftPurchaseButton.pressedMaterial;
			this.rightPurchaseButton.buttonRenderer.material = this.rightPurchaseButton.pressedMaterial;
			break;
		}
		case CosmeticsController.PurchaseItemStages.Failure:
			this.FormattedPurchaseText(6);
			this.leftPurchaseButton.myTmpText.text = "-";
			this.rightPurchaseButton.myTmpText.text = "-";
			this.leftPurchaseButton.buttonRenderer.material = this.leftPurchaseButton.pressedMaterial;
			this.rightPurchaseButton.buttonRenderer.material = this.rightPurchaseButton.pressedMaterial;
			return;
		default:
			return;
		}
	}

	public void FormattedPurchaseText(int finalLineVar)
	{
		if (this._itemNameVar == null || this._itemCostVar == null || this._currencyBalanceVar == null || this._finalLineVar == null)
		{
			Debug.LogError("[LOCALIZATION::BUILDER_KIOSK] One of the dynamic variables is NULL and cannot update the [purchaseText] screen");
			return;
		}
		this._itemNameVar.Value = this.itemToBuy.displayName.ToUpper();
		this._itemCostVar.Value = (int)this.itemToBuy.cost;
		this._currencyBalanceVar.Value = CosmeticsController.instance.currencyBalance;
		this._finalLineVar.Value = finalLineVar;
	}

	public void PurchaseItem()
	{
		BuilderSetManager.instance.TryPurchaseItem(this.itemToBuy.setID, delegate(bool result)
		{
			if (result)
			{
				this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Success;
				CosmeticsController.instance.currencyBalance -= (int)this.itemToBuy.cost;
				this.ProcessPurchaseItemState(null, this.isLastHandTouchedLeft);
				return;
			}
			this.currentPurchaseItemStage = CosmeticsController.PurchaseItemStages.Failure;
			this.ProcessPurchaseItemState(null, false);
		});
	}

	private const string MONKE_BLOCKS_PURCHASE_BUTTON_WANT_TO_BUY_CONFIRM_KEY = "MONKE_BLOCKS_PURCHASE_BUTTON_WANT_TO_BUY_CONFIRM";

	private const string MONKE_BLOCKS_PURCHASE_BUTTON_WANT_TO_BUY_CANCEL_KEY = "MONKE_BLOCKS_PURCHASE_BUTTON_WANT_TO_BUY_CANCEL";

	private const string MONKE_BLOCKS_PURCHASE_BUTTON_CONFIRMATION_CONFIRM_KEY = "MONKE_BLOCKS_PURCHASE_BUTTON_CONFIRMATION_CONFIRM";

	private const string MONKE_BLOCKS_PURCHASE_BUTTON_CONFIRMATION_CANCEL_KEY = "MONKE_BLOCKS_PURCHASE_BUTTON_CONFIRMATION_CANCEL";

	public BuilderPieceSet pieceSetForSale;

	public GorillaPressableButton leftPurchaseButton;

	public GorillaPressableButton rightPurchaseButton;

	public TMP_Text purchaseText;

	[SerializeField]
	private bool isMiniKiosk;

	[SerializeField]
	private bool useTitleCountDown = true;

	[Header("Buttons")]
	[SerializeField]
	private GorillaPressableButton[] setButtons;

	[SerializeField]
	private GorillaPressableButton previousPageButton;

	[SerializeField]
	private GorillaPressableButton nextPageButton;

	private BuilderPieceSet currentSet;

	private int pageIndex;

	private int setsPerPage = 3;

	private int totalPages = 1;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip purchaseSetAudioClip;

	[SerializeField]
	private ParticleSystem purchaseParticles;

	[SerializeField]
	private GameObject emptyDisplay;

	private List<BuilderPieceSet> availableItems = new List<BuilderPieceSet>(10);

	internal CosmeticsController.PurchaseItemStages currentPurchaseItemStage;

	private bool hasInitFromPlayfab;

	internal BuilderSetManager.BuilderSetStoreItem itemToBuy;

	public static BuilderSetManager.BuilderSetStoreItem nullItem;

	private GameObject currentDiorama;

	private GameObject nextDiorama;

	private bool animating;

	[SerializeField]
	private Transform itemDisplayPos;

	[SerializeField]
	private Transform nextItemDisplayPos;

	[SerializeField]
	private Animation itemDisplayAnimation;

	[SerializeField]
	private CountdownText countdownText;

	private string countdownOverride = string.Empty;

	private bool isLastHandTouchedLeft;

	private string finalLine;

	[Header("Localization")]
	[SerializeField]
	private LocalizedText _puchaseTextLoc;

	private LocalizedString _puchaseTextLocStr;

	private StringVariable _itemNameVar;

	private IntVariable _finalLineVar;

	private IntVariable _itemCostVar;

	private IntVariable _currencyBalanceVar;
}
