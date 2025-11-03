using System;
using GorillaNetworking;
using PlayFab;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public class PlayFabTitleDataTextDisplay : MonoBehaviour, IBuildValidation
{
	public string playFabKeyValue
	{
		get
		{
			return this.playfabKey;
		}
	}

	private void Start()
	{
		if (this.textBox != null)
		{
			this.textBox.color = this.defaultTextColor;
		}
		else
		{
			Debug.LogError("The TextBox is null on this PlayFabTitleDataTextDisplay component");
		}
		PlayFabTitleDataCache.Instance.OnTitleDataUpdate.AddListener(new UnityAction<string>(this.OnNewTitleDataAdded));
		PlayFabTitleDataCache.Instance.GetTitleData(this.playfabKey, new Action<string>(this.OnTitleDataRequestComplete), new Action<PlayFabError>(this.OnPlayFabError), false);
		if (!this._hasRegisteredCallback)
		{
			LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		}
	}

	private void OnEnable()
	{
		if (LocalisationManager.Instance == null)
		{
			return;
		}
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		this._hasRegisteredCallback = true;
	}

	private void OnDisable()
	{
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		this._hasRegisteredCallback = false;
	}

	private void OnPlayFabError(PlayFabError error)
	{
		if (this.textBox != null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"PlayFabTitleDataTextDisplay: PlayFab error retrieving title data for key ",
				this.playfabKey,
				" displayed ",
				this.fallbackText,
				": ",
				error.GenerateErrorReport()
			}));
			if (this._fallbackLocalizedText == null || this._fallbackLocalizedText.IsEmpty)
			{
				this.textBox.text = this.fallbackText;
				return;
			}
			string text;
			if (!LocalisationManager.TryGetTranslationForCurrentLocaleWithLocString(this._fallbackLocalizedText, out text, this.fallbackText, null))
			{
				Debug.LogError("[LOCALIZATION::PLAYFAB_TITLEDATA_TEXT_DISPLAY] Failed to get key for PlayFab Title Data Text [_fallbackLocalizedText]");
			}
			this.textBox.text = text;
		}
	}

	private void OnLanguageChanged()
	{
		if (string.IsNullOrEmpty(this._cachedText))
		{
			Debug.LogError("[LOCALIZATION::PLAY_FAB_TITLE_DATA_TEXT_DISPLAY] [_cachedText] is not set yet, is this being called before title data has been obtained?");
			return;
		}
		PlayFabTitleDataCache.Instance.GetTitleData(this.playfabKey, new Action<string>(this.OnTitleDataRequestComplete), new Action<PlayFabError>(this.OnPlayFabError), false);
	}

	private void OnTitleDataRequestComplete(string titleDataResult)
	{
		if (this.textBox != null)
		{
			this._cachedText = titleDataResult;
			string text = titleDataResult.Replace("\\r", "\r").Replace("\\n", "\n");
			if (text[0] == '"' && text[text.Length - 1] == '"')
			{
				text = text.Substring(1, text.Length - 2);
			}
			this.textBox.text = text;
			Debug.Log("PlayFabTitleDataTextDisplay: text: " + text);
		}
	}

	private void OnNewTitleDataAdded(string key)
	{
		if (key == this.playfabKey && this.textBox != null)
		{
			this.textBox.color = this.newUpdateColor;
		}
	}

	private void OnDestroy()
	{
		PlayFabTitleDataCache.Instance.OnTitleDataUpdate.RemoveListener(new UnityAction<string>(this.OnNewTitleDataAdded));
	}

	public bool BuildValidationCheck()
	{
		if (this.textBox == null)
		{
			Debug.LogError("text reference is null! sign text will be broken");
			return false;
		}
		return true;
	}

	public void ChangeTitleDataAtRuntime(string newTitleDataKey)
	{
		this.playfabKey = newTitleDataKey;
		if (this.textBox != null)
		{
			this.textBox.color = this.defaultTextColor;
		}
		else
		{
			Debug.LogError("The TextBox is null on this PlayFabTitleDataTextDisplay component");
		}
		PlayFabTitleDataCache.Instance.OnTitleDataUpdate.AddListener(new UnityAction<string>(this.OnNewTitleDataAdded));
		PlayFabTitleDataCache.Instance.GetTitleData(this.playfabKey, new Action<string>(this.OnTitleDataRequestComplete), new Action<PlayFabError>(this.OnPlayFabError), false);
	}

	[SerializeField]
	private TextMeshPro textBox;

	[SerializeField]
	private Color newUpdateColor = Color.magenta;

	[SerializeField]
	private Color defaultTextColor = Color.white;

	[Tooltip("PlayFab Title Data key from where to pull display text")]
	[SerializeField]
	private string playfabKey;

	[Tooltip("Text to display when error occurs during fetch")]
	[TextArea(3, 5)]
	[SerializeField]
	private string fallbackText;

	[SerializeField]
	private LocalizedString _fallbackLocalizedText;

	private bool _hasRegisteredCallback;

	private string _cachedText = string.Empty;
}
