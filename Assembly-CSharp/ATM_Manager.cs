using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GorillaNetworking;
using GorillaNetworking.Store;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ATM_Manager : MonoBehaviour
{
	public string ValidatedCreatorCode { get; set; }

	public ATM_Manager.ATMStages CurrentATMStage
	{
		get
		{
			return this.currentATMStage;
		}
	}

	public void Awake()
	{
		if (ATM_Manager.instance)
		{
			Object.Destroy(this);
		}
		else
		{
			ATM_Manager.instance = this;
		}
		string text = "CREATOR CODE: ";
		string text2;
		if (!LocalisationManager.TryGetKeyForCurrentLocale("ATM_CREATOR_CODE", out text2, text))
		{
			Debug.LogError("[LOCALIZATION::ATM_MANAGER] Failed to get key for [ATM_CREATOR_CODE]");
		}
		foreach (ATM_UI atm_UI in this.atmUIs)
		{
			atm_UI.creatorCodeTitle.text = text2;
		}
		this.SwitchToStage(ATM_Manager.ATMStages.Unavailable);
		this.smallDisplays = new List<CreatorCodeSmallDisplay>();
	}

	public void Start()
	{
		Debug.Log("ATM COUNT: " + this.atmUIs.Count.ToString());
		Debug.Log("SMALL DISPLAY COUNT: " + this.smallDisplays.Count.ToString());
		GameEvents.OnGorrillaATMKeyButtonPressedEvent.AddListener(new UnityAction<GorillaATMKeyBindings>(this.PressButton));
		this.currentCreatorCode = "";
		if (PlayerPrefs.HasKey("CodeUsedTime"))
		{
			this.codeFirstUsedTime = PlayerPrefs.GetString("CodeUsedTime");
			DateTime dateTime = DateTime.Parse(this.codeFirstUsedTime);
			if ((DateTime.Now - dateTime).TotalDays > 14.0)
			{
				PlayerPrefs.SetString("CreatorCode", "");
			}
			else
			{
				this.currentCreatorCode = PlayerPrefs.GetString("CreatorCode", "");
				this.initialCode = this.currentCreatorCode;
				Debug.Log("Initial code: " + this.initialCode);
				if (string.IsNullOrEmpty(this.currentCreatorCode))
				{
					this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Empty;
				}
				else
				{
					this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Unchecked;
				}
				foreach (CreatorCodeSmallDisplay creatorCodeSmallDisplay in this.smallDisplays)
				{
					creatorCodeSmallDisplay.SetCode(this.currentCreatorCode);
				}
			}
		}
		foreach (ATM_UI atm_UI in this.atmUIs)
		{
			atm_UI.creatorCodeField.text = this.currentCreatorCode;
		}
	}

	private void OnEnable()
	{
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		this.SwitchToStage(this.currentATMStage);
	}

	private void OnDisable()
	{
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.OnLanguageChanged));
	}

	private void OnLanguageChanged()
	{
		this.SwitchToStage(this.currentATMStage);
	}

	public void PressButton(GorillaATMKeyBindings buttonPressed)
	{
		if (this.currentATMStage == ATM_Manager.ATMStages.Confirm && this.creatorCodeStatus != ATM_Manager.CreatorCodeStatus.Validating)
		{
			string text = "CREATOR CODE: ";
			string text2;
			LocalisationManager.TryGetKeyForCurrentLocale("ATM_CREATOR_CODE", out text2, text);
			foreach (ATM_UI atm_UI in this.atmUIs)
			{
				atm_UI.creatorCodeTitle.text = text2;
			}
			if (buttonPressed == GorillaATMKeyBindings.delete)
			{
				if (this.currentCreatorCode.Length > 0)
				{
					this.currentCreatorCode = this.currentCreatorCode.Substring(0, this.currentCreatorCode.Length - 1);
					if (this.currentCreatorCode.Length == 0)
					{
						this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Empty;
						this.ValidatedCreatorCode = "";
						foreach (CreatorCodeSmallDisplay creatorCodeSmallDisplay in this.smallDisplays)
						{
							creatorCodeSmallDisplay.SetCode("");
						}
						PlayerPrefs.SetString("CreatorCode", "");
						PlayerPrefs.Save();
					}
					else
					{
						this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Unchecked;
					}
				}
			}
			else if (this.currentCreatorCode.Length < 10)
			{
				string text3 = this.currentCreatorCode;
				string text4;
				if (buttonPressed >= GorillaATMKeyBindings.delete)
				{
					text4 = buttonPressed.ToString();
				}
				else
				{
					int num = (int)buttonPressed;
					text4 = num.ToString();
				}
				this.currentCreatorCode = text3 + text4;
				this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Unchecked;
			}
			foreach (ATM_UI atm_UI2 in this.atmUIs)
			{
				atm_UI2.creatorCodeField.text = this.currentCreatorCode;
			}
		}
	}

	public void ProcessATMState(string currencyButton)
	{
		switch (this.currentATMStage)
		{
		case ATM_Manager.ATMStages.Unavailable:
		case ATM_Manager.ATMStages.Purchasing:
			break;
		case ATM_Manager.ATMStages.Begin:
			this.SwitchToStage(ATM_Manager.ATMStages.Menu);
			return;
		case ATM_Manager.ATMStages.Menu:
			if (PlayFabAuthenticator.instance.GetSafety())
			{
				if (currencyButton == "one")
				{
					this.SwitchToStage(ATM_Manager.ATMStages.Balance);
					return;
				}
				if (!(currencyButton == "four"))
				{
					return;
				}
				this.SwitchToStage(ATM_Manager.ATMStages.Begin);
				return;
			}
			else
			{
				if (currencyButton == "one")
				{
					this.SwitchToStage(ATM_Manager.ATMStages.Balance);
					return;
				}
				if (currencyButton == "two")
				{
					this.SwitchToStage(ATM_Manager.ATMStages.Choose);
					return;
				}
				if (!(currencyButton == "back"))
				{
					return;
				}
				this.SwitchToStage(ATM_Manager.ATMStages.Begin);
				return;
			}
			break;
		case ATM_Manager.ATMStages.Balance:
			if (currencyButton == "back")
			{
				this.SwitchToStage(ATM_Manager.ATMStages.Menu);
				return;
			}
			break;
		case ATM_Manager.ATMStages.Choose:
			if (currencyButton == "one")
			{
				this.numShinyRocksToBuy = 1000;
				this.shinyRocksCost = 4.99f;
				CosmeticsController.instance.itemToPurchase = "1000SHINYROCKS";
				CosmeticsController.instance.buyingBundle = false;
				this.SwitchToStage(ATM_Manager.ATMStages.Confirm);
				return;
			}
			if (currencyButton == "two")
			{
				this.numShinyRocksToBuy = 2200;
				this.shinyRocksCost = 9.99f;
				CosmeticsController.instance.itemToPurchase = "2200SHINYROCKS";
				CosmeticsController.instance.buyingBundle = false;
				this.SwitchToStage(ATM_Manager.ATMStages.Confirm);
				return;
			}
			if (currencyButton == "three")
			{
				this.numShinyRocksToBuy = 5000;
				this.shinyRocksCost = 19.99f;
				CosmeticsController.instance.itemToPurchase = "5000SHINYROCKS";
				CosmeticsController.instance.buyingBundle = false;
				this.SwitchToStage(ATM_Manager.ATMStages.Confirm);
				return;
			}
			if (currencyButton == "four")
			{
				this.numShinyRocksToBuy = 11000;
				this.shinyRocksCost = 39.99f;
				CosmeticsController.instance.itemToPurchase = "11000SHINYROCKS";
				CosmeticsController.instance.buyingBundle = false;
				this.SwitchToStage(ATM_Manager.ATMStages.Confirm);
				return;
			}
			if (!(currencyButton == "back"))
			{
				return;
			}
			this.SwitchToStage(ATM_Manager.ATMStages.Menu);
			return;
		case ATM_Manager.ATMStages.Confirm:
			if (!(currencyButton == "one"))
			{
				if (!(currencyButton == "back"))
				{
					return;
				}
				this.SwitchToStage(ATM_Manager.ATMStages.Choose);
				return;
			}
			else
			{
				if (this.creatorCodeStatus == ATM_Manager.CreatorCodeStatus.Empty)
				{
					CosmeticsController.instance.SteamPurchase();
					this.SwitchToStage(ATM_Manager.ATMStages.Purchasing);
					return;
				}
				base.StartCoroutine(this.CheckValidationCoroutine());
				return;
			}
			break;
		default:
			this.SwitchToStage(ATM_Manager.ATMStages.Menu);
			break;
		}
	}

	public void AddATM(ATM_UI newATM)
	{
		this.atmUIs.Add(newATM);
		newATM.creatorCodeField.text = this.currentCreatorCode;
		this.SwitchToStage(this.currentATMStage);
	}

	public void RemoveATM(ATM_UI atmToRemove)
	{
		this.atmUIs.Remove(atmToRemove);
	}

	public void SetTemporaryCreatorCode(string creatorCode, bool onlyIfEmpty = true, Action<bool> OnComplete = null)
	{
		if (onlyIfEmpty && (this.creatorCodeStatus != ATM_Manager.CreatorCodeStatus.Empty || !this.currentCreatorCode.IsNullOrEmpty()))
		{
			Action<bool> onComplete = OnComplete;
			if (onComplete == null)
			{
				return;
			}
			onComplete(false);
			return;
		}
		else
		{
			string text = "^[a-zA-Z0-9]+$";
			if (creatorCode.Length <= 10 && Regex.IsMatch(creatorCode, text))
			{
				NexusManager.instance.VerifyCreatorCode(creatorCode, delegate(Member member)
				{
					if (this.currentATMStage > ATM_Manager.ATMStages.Confirm)
					{
						Action<bool> onComplete3 = OnComplete;
						if (onComplete3 == null)
						{
							return;
						}
						onComplete3(false);
						return;
					}
					else if (onlyIfEmpty && (this.creatorCodeStatus != ATM_Manager.CreatorCodeStatus.Empty || !this.currentCreatorCode.IsNullOrEmpty()))
					{
						Action<bool> onComplete4 = OnComplete;
						if (onComplete4 == null)
						{
							return;
						}
						onComplete4(false);
						return;
					}
					else
					{
						this.temporaryOverrideCode = creatorCode;
						this.currentCreatorCode = creatorCode;
						this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Unchecked;
						foreach (CreatorCodeSmallDisplay creatorCodeSmallDisplay in this.smallDisplays)
						{
							creatorCodeSmallDisplay.SetCode(this.currentCreatorCode);
						}
						foreach (ATM_UI atm_UI in this.atmUIs)
						{
							atm_UI.creatorCodeField.text = this.currentCreatorCode;
						}
						Action<bool> onComplete5 = OnComplete;
						if (onComplete5 == null)
						{
							return;
						}
						onComplete5(true);
						return;
					}
				}, delegate
				{
					Action<bool> onComplete6 = OnComplete;
					if (onComplete6 == null)
					{
						return;
					}
					onComplete6(false);
				});
				return;
			}
			Action<bool> onComplete2 = OnComplete;
			if (onComplete2 == null)
			{
				return;
			}
			onComplete2(false);
			return;
		}
	}

	public void ResetTemporaryCreatorCode()
	{
		if (this.creatorCodeStatus == ATM_Manager.CreatorCodeStatus.Unchecked && this.currentCreatorCode.Equals(this.temporaryOverrideCode))
		{
			this.currentCreatorCode = "";
			this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Empty;
			foreach (CreatorCodeSmallDisplay creatorCodeSmallDisplay in this.smallDisplays)
			{
				creatorCodeSmallDisplay.SetCode("");
			}
			foreach (ATM_UI atm_UI in this.atmUIs)
			{
				atm_UI.creatorCodeField.text = this.currentCreatorCode;
			}
		}
		this.temporaryOverrideCode = "";
	}

	private void ResetCreatorCode()
	{
		Debug.Log("Resetting creator code");
		string text = "CREATOR CODE: ";
		string text2;
		LocalisationManager.TryGetKeyForCurrentLocale("ATM_CREATOR_CODE", out text2, text);
		foreach (ATM_UI atm_UI in this.atmUIs)
		{
			atm_UI.creatorCodeTitle.text = text2;
		}
		foreach (CreatorCodeSmallDisplay creatorCodeSmallDisplay in this.smallDisplays)
		{
			creatorCodeSmallDisplay.SetCode("");
		}
		this.currentCreatorCode = "";
		this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Empty;
		this.supportedMember = default(Member);
		this.ValidatedCreatorCode = "";
		PlayerPrefs.SetString("CreatorCode", "");
		PlayerPrefs.Save();
		foreach (ATM_UI atm_UI2 in this.atmUIs)
		{
			atm_UI2.creatorCodeField.text = this.currentCreatorCode;
		}
	}

	private IEnumerator CheckValidationCoroutine()
	{
		foreach (ATM_UI atm_UI in this.atmUIs)
		{
			atm_UI.creatorCodeTitle.text = "CREATOR CODE: VALIDATING";
			string text;
			LocalisationManager.TryGetKeyForCurrentLocale("ATM_CREATOR_CODE_VALIDATING", out text, atm_UI.atmText.text);
			atm_UI.creatorCodeTitle.text = text;
		}
		this.VerifyCreatorCode();
		while (this.creatorCodeStatus == ATM_Manager.CreatorCodeStatus.Validating)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (this.creatorCodeStatus == ATM_Manager.CreatorCodeStatus.Valid)
		{
			foreach (ATM_UI atm_UI2 in this.atmUIs)
			{
				atm_UI2.creatorCodeTitle.text = "CREATOR CODE: VALID";
				string text2;
				LocalisationManager.TryGetKeyForCurrentLocale("ATM_CREATOR_CODE_VALID", out text2, atm_UI2.atmText.text);
				atm_UI2.creatorCodeTitle.text = text2;
			}
			this.SwitchToStage(ATM_Manager.ATMStages.Purchasing);
			CosmeticsController.instance.SteamPurchase();
		}
		yield break;
	}

	public void SwitchToStage(ATM_Manager.ATMStages newStage)
	{
		this.currentATMStage = newStage;
		foreach (ATM_UI atm_UI in this.atmUIs)
		{
			if (atm_UI.atmText)
			{
				string text = "";
				string text2 = "";
				string text3 = "";
				string text4 = "";
				string text5 = "";
				switch (newStage)
				{
				case ATM_Manager.ATMStages.Unavailable:
					atm_UI.atmText.text = "ATM NOT AVAILABLE! PLEASE TRY AGAIN LATER!";
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_NOT_AVAILABLE", out text, atm_UI.atmText.text);
					atm_UI.atmText.text = text;
					atm_UI.ATM_RightColumnButtonText[0].text = "";
					atm_UI.ATM_RightColumnArrowText[0].enabled = false;
					atm_UI.ATM_RightColumnButtonText[1].text = "";
					atm_UI.ATM_RightColumnArrowText[1].enabled = false;
					atm_UI.ATM_RightColumnButtonText[2].text = "";
					atm_UI.ATM_RightColumnArrowText[2].enabled = false;
					atm_UI.ATM_RightColumnButtonText[3].text = "";
					atm_UI.ATM_RightColumnArrowText[3].enabled = false;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				case ATM_Manager.ATMStages.Begin:
					atm_UI.atmText.text = "WELCOME! PRESS ANY BUTTON TO BEGIN.";
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_STARTUP", out text, atm_UI.atmText.text);
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_BEGIN", out text5, "BEGIN");
					atm_UI.atmText.text = text;
					atm_UI.ATM_RightColumnButtonText[0].text = "";
					atm_UI.ATM_RightColumnArrowText[0].enabled = false;
					atm_UI.ATM_RightColumnButtonText[1].text = "";
					atm_UI.ATM_RightColumnArrowText[1].enabled = false;
					atm_UI.ATM_RightColumnButtonText[2].text = "";
					atm_UI.ATM_RightColumnArrowText[2].enabled = false;
					atm_UI.ATM_RightColumnButtonText[3].text = text5;
					atm_UI.ATM_RightColumnArrowText[3].enabled = true;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				case ATM_Manager.ATMStages.Menu:
					if (PlayFabAuthenticator.instance.GetSafety())
					{
						atm_UI.atmText.text = "CHECK YOUR BALANCE.";
						LocalisationManager.TryGetKeyForCurrentLocale("ATM_CHECK_YOUR_BALANCE", out text, atm_UI.atmText.text);
						LocalisationManager.TryGetKeyForCurrentLocale("ATM_BALANCE", out text2, atm_UI.atmText.text);
						atm_UI.atmText.text = text;
						atm_UI.ATM_RightColumnButtonText[0].text = text2;
						atm_UI.ATM_RightColumnArrowText[0].enabled = true;
						atm_UI.ATM_RightColumnButtonText[1].text = "";
						atm_UI.ATM_RightColumnArrowText[1].enabled = false;
						atm_UI.ATM_RightColumnButtonText[2].text = "";
						atm_UI.ATM_RightColumnArrowText[2].enabled = false;
						atm_UI.ATM_RightColumnButtonText[3].text = "";
						atm_UI.ATM_RightColumnArrowText[3].enabled = false;
						atm_UI.creatorCodeObject.SetActive(false);
					}
					else
					{
						atm_UI.atmText.text = "CHECK YOUR BALANCE OR PURCHASE MORE SHINY ROCKS.";
						LocalisationManager.TryGetKeyForCurrentLocale("ATM_MAIN_SCREEN", out text, atm_UI.atmText.text);
						LocalisationManager.TryGetKeyForCurrentLocale("ATM_BALANCE", out text2, atm_UI.atmText.text);
						LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASE", out text3, atm_UI.atmText.text);
						atm_UI.atmText.text = text;
						atm_UI.ATM_RightColumnButtonText[0].text = text2;
						atm_UI.ATM_RightColumnArrowText[0].enabled = true;
						atm_UI.ATM_RightColumnButtonText[1].text = text3;
						atm_UI.ATM_RightColumnArrowText[1].enabled = true;
						atm_UI.ATM_RightColumnButtonText[2].text = "";
						atm_UI.ATM_RightColumnArrowText[2].enabled = false;
						atm_UI.ATM_RightColumnButtonText[3].text = "";
						atm_UI.ATM_RightColumnArrowText[3].enabled = false;
						atm_UI.creatorCodeObject.SetActive(false);
					}
					break;
				case ATM_Manager.ATMStages.Balance:
					atm_UI.atmText.text = "CURRENT BALANCE:\n\n" + CosmeticsController.instance.CurrencyBalance.ToString();
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_CURRENT_BALANCE", out text, atm_UI.atmText.text);
					atm_UI.atmText.text = text + "\n\n" + CosmeticsController.instance.CurrencyBalance.ToString();
					atm_UI.ATM_RightColumnButtonText[0].text = "";
					atm_UI.ATM_RightColumnArrowText[0].enabled = false;
					atm_UI.ATM_RightColumnButtonText[1].text = "";
					atm_UI.ATM_RightColumnArrowText[1].enabled = false;
					atm_UI.ATM_RightColumnButtonText[2].text = "";
					atm_UI.ATM_RightColumnArrowText[2].enabled = false;
					atm_UI.ATM_RightColumnButtonText[3].text = "";
					atm_UI.ATM_RightColumnArrowText[3].enabled = false;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				case ATM_Manager.ATMStages.Choose:
				{
					string text6 = "{numShinyRocksToBuy} - {currencySymbol}{shinyRocksCost}";
					string text7 = "{numShinyRocksToBuy} - {currencySymbol}{shinyRocksCost}\r\n({discount}% BONUS!";
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASE_OPTION_FIRST", out text2, text6);
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASE_OPTION_SECOND", out text3, text7);
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASE_OPTION_SECOND", out text4, text7);
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASE_OPTION_SECOND", out text5, text7);
					text2 = text2.Replace("{numShinyRocksToBuy}", "1000").Replace("{currencySymbol}", "$").Replace("{shinyRocksCost}", "4.99");
					text3 = text3.Replace("{numShinyRocksToBuy}", "2200").Replace("{currencySymbol}", "$").Replace("{shinyRocksCost}", "9.99")
						.Replace("{discount}", "10");
					text4 = text4.Replace("{numShinyRocksToBuy}", "5000").Replace("{currencySymbol}", "$").Replace("{shinyRocksCost}", "19.99")
						.Replace("{discount}", "25");
					text5 = text5.Replace("{numShinyRocksToBuy}", "11000").Replace("{currencySymbol}", "$").Replace("{shinyRocksCost}", "39.99")
						.Replace("{discount}", "37");
					atm_UI.atmText.text = "CHOOSE AN AMOUNT OF SHINY ROCKS TO PURCHASE.";
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_CHOOSE_PURCHASE", out text, atm_UI.atmText.text);
					atm_UI.atmText.text = text;
					atm_UI.ATM_RightColumnButtonText[0].text = text2;
					atm_UI.ATM_RightColumnArrowText[0].enabled = true;
					atm_UI.ATM_RightColumnButtonText[1].text = text3;
					atm_UI.ATM_RightColumnArrowText[1].enabled = true;
					atm_UI.ATM_RightColumnButtonText[2].text = text4;
					atm_UI.ATM_RightColumnArrowText[2].enabled = true;
					atm_UI.ATM_RightColumnButtonText[3].text = text5;
					atm_UI.ATM_RightColumnArrowText[3].enabled = true;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				}
				case ATM_Manager.ATMStages.Confirm:
					atm_UI.atmText.text = string.Concat(new string[]
					{
						"YOU HAVE CHOSEN TO PURCHASE ",
						this.numShinyRocksToBuy.ToString(),
						" SHINY ROCKS FOR $",
						this.shinyRocksCost.ToString(),
						". CONFIRM TO LAUNCH A STEAM WINDOW TO COMPLETE YOUR PURCHASE."
					});
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASE_CONFIRMATION_STEAM", out text, atm_UI.atmText.text);
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_CONFIRM", out text2, "CONFIRM");
					text = text.Replace("{numShinyRocksToBuy}", this.numShinyRocksToBuy.ToString());
					text = text.Replace("{currencySymbol}", "$");
					text = text.Replace("{shinyRocksCost}", this.shinyRocksCost.ToString());
					atm_UI.atmText.text = text;
					atm_UI.ATM_RightColumnButtonText[0].text = text2;
					atm_UI.ATM_RightColumnArrowText[0].enabled = true;
					atm_UI.ATM_RightColumnButtonText[1].text = "";
					atm_UI.ATM_RightColumnArrowText[1].enabled = false;
					atm_UI.ATM_RightColumnButtonText[2].text = "";
					atm_UI.ATM_RightColumnArrowText[2].enabled = false;
					atm_UI.ATM_RightColumnButtonText[3].text = "";
					atm_UI.ATM_RightColumnArrowText[3].enabled = false;
					atm_UI.creatorCodeObject.SetActive(true);
					break;
				case ATM_Manager.ATMStages.Purchasing:
					atm_UI.atmText.text = "PURCHASING IN STEAM...";
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASING", out text, atm_UI.atmText.text);
					atm_UI.atmText.text = text;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				case ATM_Manager.ATMStages.Success:
					atm_UI.atmText.text = "SUCCESS! NEW SHINY ROCKS BALANCE: " + (CosmeticsController.instance.CurrencyBalance + this.numShinyRocksToBuy).ToString();
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_SUCCESS_NEW_BALANCE", out text, atm_UI.atmText.text);
					atm_UI.atmText.text = text + (CosmeticsController.instance.CurrencyBalance + this.numShinyRocksToBuy).ToString();
					if (this.creatorCodeStatus == ATM_Manager.CreatorCodeStatus.Valid)
					{
						string name = this.supportedMember.name;
						if (!string.IsNullOrEmpty(name))
						{
							TMP_Text atmText = atm_UI.atmText;
							atmText.text = atmText.text + "\n\nTHIS PURCHASE SUPPORTED\n" + name + "!";
							foreach (CreatorCodeSmallDisplay creatorCodeSmallDisplay in this.smallDisplays)
							{
								creatorCodeSmallDisplay.SuccessfulPurchase(name);
							}
						}
					}
					atm_UI.ATM_RightColumnButtonText[0].text = "";
					atm_UI.ATM_RightColumnArrowText[0].enabled = false;
					atm_UI.ATM_RightColumnButtonText[1].text = "";
					atm_UI.ATM_RightColumnArrowText[1].enabled = false;
					atm_UI.ATM_RightColumnButtonText[2].text = "";
					atm_UI.ATM_RightColumnArrowText[2].enabled = false;
					atm_UI.ATM_RightColumnButtonText[3].text = "";
					atm_UI.ATM_RightColumnArrowText[3].enabled = false;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				case ATM_Manager.ATMStages.Failure:
					atm_UI.atmText.text = "PURCHASE CANCELLED. NO FUNDS WERE SPENT.";
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASE_CANCELLED", out text, atm_UI.atmText.text);
					atm_UI.atmText.text = text;
					atm_UI.ATM_RightColumnButtonText[0].text = "";
					atm_UI.ATM_RightColumnArrowText[0].enabled = false;
					atm_UI.ATM_RightColumnButtonText[1].text = "";
					atm_UI.ATM_RightColumnArrowText[1].enabled = false;
					atm_UI.ATM_RightColumnButtonText[2].text = "";
					atm_UI.ATM_RightColumnArrowText[2].enabled = false;
					atm_UI.ATM_RightColumnButtonText[3].text = "";
					atm_UI.ATM_RightColumnArrowText[3].enabled = false;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				case ATM_Manager.ATMStages.SafeAccount:
					atm_UI.atmText.text = "Out Of Order.";
					LocalisationManager.TryGetKeyForCurrentLocale("ATM_PURCHASING_DISABLED_OUT_OF_ORDER", out text, atm_UI.atmText.text);
					atm_UI.atmText.text = text;
					atm_UI.ATM_RightColumnButtonText[0].text = "";
					atm_UI.ATM_RightColumnArrowText[0].enabled = false;
					atm_UI.ATM_RightColumnButtonText[1].text = "";
					atm_UI.ATM_RightColumnArrowText[1].enabled = false;
					atm_UI.ATM_RightColumnButtonText[2].text = "";
					atm_UI.ATM_RightColumnArrowText[2].enabled = false;
					atm_UI.ATM_RightColumnButtonText[3].text = "";
					atm_UI.ATM_RightColumnArrowText[3].enabled = false;
					atm_UI.creatorCodeObject.SetActive(false);
					break;
				}
			}
		}
	}

	public void SetATMText(string newText)
	{
		foreach (ATM_UI atm_UI in this.atmUIs)
		{
			atm_UI.atmText.text = newText;
		}
	}

	public void PressCurrencyPurchaseButton(string currencyPurchaseSize)
	{
		this.ProcessATMState(currencyPurchaseSize);
	}

	public void VerifyCreatorCode()
	{
		this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Validating;
		NexusManager.instance.VerifyCreatorCode(this.currentCreatorCode, new Action<Member>(this.OnCreatorCodeSucess), new Action(this.OnCreatorCodeFailure));
	}

	private void OnCreatorCodeSucess(Member member)
	{
		this.creatorCodeStatus = ATM_Manager.CreatorCodeStatus.Valid;
		this.supportedMember = member;
		this.ValidatedCreatorCode = this.currentCreatorCode;
		foreach (CreatorCodeSmallDisplay creatorCodeSmallDisplay in this.smallDisplays)
		{
			creatorCodeSmallDisplay.SetCode(this.ValidatedCreatorCode);
		}
		PlayerPrefs.SetString("CreatorCode", this.ValidatedCreatorCode);
		if (this.initialCode != this.ValidatedCreatorCode)
		{
			PlayerPrefs.SetString("CodeUsedTime", DateTime.Now.ToString());
		}
		PlayerPrefs.Save();
		Debug.Log("ATM CODE SUCCESS: " + this.supportedMember.name);
	}

	private void OnCreatorCodeFailure()
	{
		this.supportedMember = default(Member);
		this.ResetCreatorCode();
		foreach (ATM_UI atm_UI in this.atmUIs)
		{
			atm_UI.creatorCodeTitle.text = "CREATOR CODE: INVALID";
			string text;
			LocalisationManager.TryGetKeyForCurrentLocale("ATM_CREATOR_CODE_INVALID", out text, atm_UI.atmText.text);
			atm_UI.creatorCodeTitle.text = text;
		}
		Debug.Log("ATM CODE FAILURE");
	}

	public void LeaveSystemMenu()
	{
	}

	private const string ATM_STARTUP_KEY = "ATM_STARTUP";

	private const string ATM_SCREEN_KEY = "ATM_SCREEN";

	private const string ATM_NOT_AVAILABLE_KEY = "ATM_NOT_AVAILABLE";

	private const string ATM_BEGIN_KEY = "ATM_BEGIN";

	private const string ATM_MAIN_SCREEN_KEY = "ATM_MAIN_SCREEN";

	private const string ATM_CHECK_YOUR_BALANCE_KEY = "ATM_CHECK_YOUR_BALANCE";

	private const string ATM_PURCHASING_DISABLED_OUT_OF_ORDER_KEY = "ATM_PURCHASING_DISABLED_OUT_OF_ORDER";

	private const string ATM_CURRENT_BALANCE_KEY = "ATM_CURRENT_BALANCE";

	private const string ATM_MODDED_CLIENT_KEY = "ATM_MODDED_CLIENT";

	private const string ATM_CHOOSE_PURCHASE_KEY = "ATM_CHOOSE_PURCHASE";

	private const string ATM_PURCHASE_CONFIRMATION_KEY = "ATM_PURCHASE_CONFIRMATION";

	private const string ATM_PURCHASE_CONFIRMATION_STEAM_KEY = "ATM_PURCHASE_CONFIRMATION_STEAM";

	private const string ATM_PURCHASING_KEY = "ATM_PURCHASING";

	private const string ATM_SUCCESS_NEW_BALANCE_KEY = "ATM_SUCCESS_NEW_BALANCE";

	private const string ATM_PURCHASE_CANCELLED_KEY = "ATM_PURCHASE_CANCELLED";

	private const string ATM_LOCKED_KEY = "ATM_LOCKED";

	private const string ATM_RETURN_KEY = "ATM_RETURN";

	private const string ATM_BACK_KEY = "ATM_BACK";

	private const string ATM_CONFIRM_KEY = "ATM_CONFIRM";

	private const string ATM_IAP_NOT_AVAILABLE_KEY = "ATM_IAP_NOT_AVAILABLE";

	private const string ATM_BALANCE_KEY = "ATM_BALANCE";

	private const string ATM_PURCHASE_KEY = "ATM_PURCHASE";

	private const string ATM_CREATOR_CODE_KEY = "ATM_CREATOR_CODE";

	private const string ATM_CREATOR_CODE_VALIDATING_KEY = "ATM_CREATOR_CODE_VALIDATING";

	private const string ATM_CREATOR_CODE_VALID_KEY = "ATM_CREATOR_CODE_VALID";

	private const string ATM_CREATOR_CODE_INVALID_KEY = "ATM_CREATOR_CODE_INVALID";

	private const string ATM_PURCHASE_OPTION_FIRST_KEY = "ATM_PURCHASE_OPTION_FIRST";

	private const string ATM_PURCHASE_OPTION_SECOND_KEY = "ATM_PURCHASE_OPTION_SECOND";

	private const string ATM_PURCHASE_OPTION_THIRD_KEY = "ATM_PURCHASE_OPTION_THIRD";

	private const string ATM_PURCHASE_OPTION_FOURTH_KEY = "ATM_PURCHASE_OPTION_FOURTH";

	[OnEnterPlay_SetNull]
	public static volatile ATM_Manager instance;

	private const int MAX_CODE_LENGTH = 10;

	public List<ATM_UI> atmUIs = new List<ATM_UI>();

	[HideInInspector]
	public List<CreatorCodeSmallDisplay> smallDisplays;

	private string currentCreatorCode;

	private string codeFirstUsedTime;

	private string initialCode;

	private string temporaryOverrideCode;

	private ATM_Manager.CreatorCodeStatus creatorCodeStatus;

	private ATM_Manager.ATMStages currentATMStage;

	public int numShinyRocksToBuy;

	public float shinyRocksCost;

	private Member supportedMember;

	public bool alreadyBegan;

	public enum CreatorCodeStatus
	{
		Empty,
		Unchecked,
		Validating,
		Valid
	}

	public enum ATMStages
	{
		Unavailable,
		Begin,
		Menu,
		Balance,
		Choose,
		Confirm,
		Purchasing,
		Success,
		Failure,
		SafeAccount
	}
}
