using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class LocalisationUI : MonoBehaviour
{
	public static LocalisationUI Instance
	{
		get
		{
			return LocalisationUI._instance;
		}
	}

	private void Awake()
	{
		if (LocalisationUI._instance != null)
		{
			Object.DestroyImmediate(this);
			return;
		}
		LocalisationUI._instance = this;
	}

	private void Start()
	{
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		this.ConstructLocalisationUI();
	}

	private void OnEnable()
	{
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		this.CheckSelectedLanguage();
	}

	private void OnDisable()
	{
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.OnLanguageChanged));
	}

	public void OnLanguageButtonPressed(KIDUIButton objRef, int languageIndex)
	{
		if (objRef != this._activeButton)
		{
			KIDUIButton activeButton = this._activeButton;
			if (activeButton != null)
			{
				activeButton.SetBorderImage(this._inactiveSprite);
			}
			objRef.SetBorderImage(this._activeSprite);
			this._activeButton = objRef;
		}
		Locale locale;
		if (!LocalisationManager.TryGetLocaleBinding(languageIndex, out locale))
		{
			return;
		}
		LocalisationManager.Instance.OnLanguageButtonPressed(locale.Identifier.Code, false);
	}

	public void OnContinueButtonPressed()
	{
		HandRayController.Instance.DisableHandRays();
		PrivateUIRoom.RemoveUI(LocalisationUI.GetUITransform());
		LocalisationManager.OnSaveLanguage();
	}

	private void ConstructLocalisationUI()
	{
		using (Dictionary<int, Locale>.Enumerator enumerator = LocalisationManager.GetAllBindings().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, Locale> item = enumerator.Current;
				KIDUIButton newButton = Object.Instantiate<KIDUIButton>(this._languageButtonPrefab, this._languageButtonGridTransform);
				bool flag = LocalisationManager.CurrentLanguage.Identifier.Code.ToLower() != "ja";
				newButton.SetText(LocalisationManager.LocaleToFriendlyString(item.Value, flag).ToUpper());
				newButton.onClick.AddListener(delegate
				{
					this.OnLanguageButtonPressed(newButton, item.Key);
				});
				this._languageButtons.Add(newButton);
			}
		}
	}

	private void CheckSelectedLanguage()
	{
		KIDUIButton kiduibutton = null;
		for (int i = 0; i < this._languageButtons.Count; i++)
		{
			bool flag = LocalisationManager.CurrentLanguage.Identifier.Code.ToLower() != "ja";
			if (!(this._languageButtons[i].GetText() != LocalisationManager.LocaleToFriendlyString(LocalisationManager.CurrentLanguage, flag).ToUpper()))
			{
				kiduibutton = this._languageButtons[i];
				break;
			}
		}
		if (kiduibutton == null)
		{
			return;
		}
		if (this._activeButton != null)
		{
			this._activeButton.SetBorderImage(this._inactiveSprite);
		}
		kiduibutton.SetBorderImage(this._activeSprite);
		this._activeButton = kiduibutton;
	}

	private void OnLanguageChanged()
	{
		for (int i = 0; i < this._languageButtons.Count; i++)
		{
			bool flag = LocalisationManager.CurrentLanguage.Identifier.Code.ToLower() != "ja";
			this._languageButtons[i].SetText(LocalisationManager.LocaleDisplayNameToFriendlyString(this._languageButtons[i].GetText(), flag).ToUpper());
			if (!(LocalisationManager.CurrentLanguage.Identifier.Code == "ja"))
			{
				this._languageButtons[i].SetFont(this._defaultFont);
			}
			else
			{
				this._languageButtons[i].SetFont(this._japaneseFont);
			}
		}
	}

	public static Transform GetUITransform()
	{
		if (LocalisationUI.Instance == null)
		{
			return null;
		}
		if (LocalisationUI.Instance._uiTransform == null)
		{
			LocalisationUI.Instance._uiTransform = LocalisationUI.Instance.transform.GetChild(0);
		}
		return LocalisationUI.Instance._uiTransform;
	}

	private static LocalisationUI _instance;

	[Header("Text Components")]
	[SerializeField]
	private TMP_Text _titleTxt;

	[SerializeField]
	private TMP_Text _confirmBtnTxt;

	[Header("UI Setup")]
	[SerializeField]
	private KIDUIButton _languageButtonPrefab;

	[SerializeField]
	private Transform _languageButtonGridTransform;

	[SerializeField]
	private Sprite _activeSprite;

	[SerializeField]
	private Sprite _inactiveSprite;

	[SerializeField]
	private TMP_FontAsset _defaultFont;

	[SerializeField]
	private TMP_FontAsset _japaneseFont;

	private Transform _uiTransform;

	private KIDUIButton _activeButton;

	private List<KIDUIButton> _languageButtons = new List<KIDUIButton>();
}
