using System;
using GorillaNetworking;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(500)]
public class SIPurchaseTerminal : MonoBehaviour, ITouchScreenStation
{
	public SIScreenRegion ScreenRegion
	{
		get
		{
			return this.screenRegion;
		}
	}

	private void OnEnable()
	{
		if (CosmeticsController.hasInstance)
		{
			this.DelayedOnEnable();
			return;
		}
		CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs = (Action)Delegate.Combine(CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs, new Action(this.DelayedOnEnable));
	}

	private void DelayedOnEnable()
	{
		CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs = (Action)Delegate.Remove(CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs, new Action(this.DelayedOnEnable));
		CosmeticsController instance = CosmeticsController.instance;
		instance.OnGetCurrency = (Action)Delegate.Combine(instance.OnGetCurrency, new Action(this.OnUpdateCurrencyBalance));
		this.OnUpdateCurrencyBalance();
		this.PopupBackgroundScreen.SetActive(false);
		this.ConfirmPurchasePopupScreen.SetActive(false);
		this.PendingPurchasePopupScreen.SetActive(false);
		this.PurchaseCompletePopupScreen.SetActive(false);
		this.InsufficientFundsPopupScreen.SetActive(false);
		this.UnableToCompletePurchasePopupScreen.SetActive(false);
		this.UpdateState(SIPurchaseTerminal.PurchaseTerminalState.PurchaseAmountSelection, true);
		this.purchaseSize = 1;
		this.UpdatePurchaseAmount();
	}

	private void OnDisable()
	{
		CosmeticsController instance = CosmeticsController.instance;
		instance.OnGetCurrency = (Action)Delegate.Remove(instance.OnGetCurrency, new Action(this.OnUpdateCurrencyBalance));
	}

	public void UpdateCurrentTechPoints()
	{
		this.PurchaseAmountCurrentTechPointsCount.text = SIPlayer.LocalPlayer.CurrentProgression.resourceArray[0].ToString();
	}

	private void OnUpdateCurrencyBalance()
	{
		this.PurchaseAmountCurrentShinyRockCount.text = CosmeticsController.instance.currencyBalance.ToString().ToUpperInvariant();
	}

	public void AddButton(SITouchscreenButton button, bool isPopupButton = false)
	{
	}

	public void TouchscreenButtonPressed(SITouchscreenButton.SITouchscreenButtonType buttonType, int data, int actorNr)
	{
		switch (this.currentState)
		{
		case SIPurchaseTerminal.PurchaseTerminalState.PurchaseAmountSelection:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Purchase)
			{
				this.SelectPurchase();
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Next)
			{
				this.IncreasePurchase();
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.DecreasePurcahse();
				return;
			}
			break;
		case SIPurchaseTerminal.PurchaseTerminalState.ConfirmPurchasePopup:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Confirm)
			{
				this.ConfirmPurchase();
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Cancel)
			{
				this.ReturnToBaseScreen();
				return;
			}
			break;
		case SIPurchaseTerminal.PurchaseTerminalState.PendingPurchasePopup:
			break;
		case SIPurchaseTerminal.PurchaseTerminalState.PurchaseCompletePopup:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Confirm)
			{
				this.ReturnToBaseScreen();
				return;
			}
			break;
		case SIPurchaseTerminal.PurchaseTerminalState.InsufficientFundsPopup:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.ReturnToBaseScreen();
				return;
			}
			break;
		case SIPurchaseTerminal.PurchaseTerminalState.UnableToCompletePurchasePopup:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.ReturnToBaseScreen();
			}
			break;
		default:
			return;
		}
	}

	private void IncreasePurchase()
	{
		this.purchaseSize = Math.Min(this.purchaseSize + 1, this.maxPurchaseSize);
		this.UpdatePurchaseAmount();
	}

	private void DecreasePurcahse()
	{
		this.purchaseSize = Math.Max(this.purchaseSize - 1, this.minPurchaseSize);
		this.UpdatePurchaseAmount();
	}

	private void UpdatePurchaseAmount()
	{
		this.PurchaseAmountShinyRockCount.text = (this.purchaseSize * this.costPerTechPoint).ToString().ToUpperInvariant();
		this.PurchaseAmountTechPointCount.text = this.purchaseSize.ToString().ToUpperInvariant();
		this.ConfirmPurchaseShinyRockCount.text = (this.purchaseSize * this.costPerTechPoint).ToString().ToUpperInvariant();
		this.ConfirmPurchaseTechPointCount.text = this.purchaseSize.ToString().ToUpperInvariant();
		this.PurchasedTechPointCount.text = this.purchaseSize.ToString().ToUpperInvariant();
	}

	private void SelectPurchase()
	{
		this.UpdateState(SIPurchaseTerminal.PurchaseTerminalState.ConfirmPurchasePopup, false);
	}

	private void ConfirmPurchase()
	{
		int num = this.purchaseSize * this.costPerTechPoint;
		if (CosmeticsController.instance.currencyBalance < num)
		{
			this.UpdateState(SIPurchaseTerminal.PurchaseTerminalState.InsufficientFundsPopup, false);
			return;
		}
		this.UpdateState(SIPurchaseTerminal.PurchaseTerminalState.PendingPurchasePopup, false);
		ProgressionManager.Instance.PurchaseTechPoints(this.purchaseSize, delegate
		{
			SIProgression.Instance.SendPurchaseTechPointsData(this.purchaseSize);
			this.UpdateState(SIPurchaseTerminal.PurchaseTerminalState.PurchaseCompletePopup, false);
			ProgressionManager.Instance.RefreshUserInventory();
		}, delegate(string error)
		{
			Debug.LogError("[SIPurchaseTerminal] PurchaseTechPoints failed: " + error);
			this.UpdateState(SIPurchaseTerminal.PurchaseTerminalState.UnableToCompletePurchasePopup, false);
		});
	}

	private void ReturnToBaseScreen()
	{
		this.UpdateState(SIPurchaseTerminal.PurchaseTerminalState.PurchaseAmountSelection, false);
	}

	private void UpdateState(SIPurchaseTerminal.PurchaseTerminalState newState, bool forceUpdate = false)
	{
		if (!forceUpdate && this.currentState == newState)
		{
			return;
		}
		this.SetScreenVisibility(this.currentState, false);
		this.currentState = newState;
		this.SetScreenVisibility(this.currentState, true);
	}

	private void SetScreenVisibility(SIPurchaseTerminal.PurchaseTerminalState screenState, bool isEnabled)
	{
		switch (screenState)
		{
		case SIPurchaseTerminal.PurchaseTerminalState.ConfirmPurchasePopup:
			this.PopupBackgroundScreen.SetActive(isEnabled);
			this.ConfirmPurchasePopupScreen.SetActive(isEnabled);
			return;
		case SIPurchaseTerminal.PurchaseTerminalState.PendingPurchasePopup:
			this.PopupBackgroundScreen.SetActive(isEnabled);
			this.PendingPurchasePopupScreen.SetActive(isEnabled);
			return;
		case SIPurchaseTerminal.PurchaseTerminalState.PurchaseCompletePopup:
			this.PopupBackgroundScreen.SetActive(isEnabled);
			this.PurchaseCompletePopupScreen.SetActive(isEnabled);
			return;
		case SIPurchaseTerminal.PurchaseTerminalState.InsufficientFundsPopup:
			this.PopupBackgroundScreen.SetActive(isEnabled);
			this.InsufficientFundsPopupScreen.SetActive(isEnabled);
			return;
		case SIPurchaseTerminal.PurchaseTerminalState.UnableToCompletePurchasePopup:
			this.PopupBackgroundScreen.SetActive(isEnabled);
			this.UnableToCompletePurchasePopupScreen.SetActive(isEnabled);
			return;
		default:
			return;
		}
	}

	GameObject ITouchScreenStation.get_gameObject()
	{
		return base.gameObject;
	}

	private SIPurchaseTerminal.PurchaseTerminalState currentState;

	[SerializeField]
	private SIScreenRegion screenRegion;

	[SerializeField]
	private GameObject PopupBackgroundScreen;

	[SerializeField]
	private GameObject ConfirmPurchasePopupScreen;

	[SerializeField]
	private GameObject PurchaseCompletePopupScreen;

	[SerializeField]
	private GameObject PendingPurchasePopupScreen;

	[SerializeField]
	private GameObject InsufficientFundsPopupScreen;

	[SerializeField]
	private GameObject UnableToCompletePurchasePopupScreen;

	[SerializeField]
	private TextMeshProUGUI PurchaseAmountShinyRockCount;

	[SerializeField]
	private TextMeshProUGUI PurchaseAmountTechPointCount;

	[SerializeField]
	private TextMeshProUGUI PurchaseAmountCurrentShinyRockCount;

	[SerializeField]
	private TextMeshProUGUI PurchaseAmountCurrentTechPointsCount;

	[SerializeField]
	private TextMeshProUGUI ConfirmPurchaseShinyRockCount;

	[SerializeField]
	private TextMeshProUGUI ConfirmPurchaseTechPointCount;

	[SerializeField]
	private TextMeshProUGUI PurchasedTechPointCount;

	[SerializeField]
	private int maxPurchaseSize = 10;

	[SerializeField]
	private int minPurchaseSize = 1;

	[SerializeField]
	private int costPerTechPoint = 100;

	private int purchaseSize = 1;

	public enum PurchaseTerminalState
	{
		PurchaseAmountSelection,
		ConfirmPurchasePopup,
		PendingPurchasePopup,
		PurchaseCompletePopup,
		InsufficientFundsPopup,
		UnableToCompletePurchasePopup
	}
}
