using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Events;

[DisallowMultipleComponent]
public class LocalizedText : LocalizeStringEvent
{
	public bool HasFontOverrides()
	{
		return this._localisationFontsOverrides.Count > 0;
	}

	private TextComponentLegacySupportStore TextComponent
	{
		get
		{
			if (!this._textComponent.IsValid)
			{
				this._textComponent = new TextComponentLegacySupportStore(base.transform);
			}
			return this._textComponent;
		}
	}

	private void Awake()
	{
		this._textComponent = new TextComponentLegacySupportStore(base.transform);
		base.OnUpdateString = new UnityEventString();
		base.OnUpdateString.AddListener(delegate(string val)
		{
			this.OnLocaleChanged(val);
		});
		if (!this.TextComponent.IsValid)
		{
			base.gameObject.AddComponent<TMP_Text>();
			this._textComponent = new TextComponentLegacySupportStore(base.transform);
		}
	}

	protected override async void UpdateString(string value)
	{
		if (LocalisationManager.ApplicationRunning && !LocalisationManager.IsReady)
		{
			await Task.Yield();
		}
		base.UpdateString(value);
	}

	private async void OnLocaleChanged(string newText)
	{
		if (LocalisationManager.ApplicationRunning && !LocalisationManager.IsReady)
		{
			await Task.Yield();
		}
		LocalisationFontPair localisationFontPair;
		if (this.GetLocalizedFonts(out localisationFontPair))
		{
			if (localisationFontPair.fontAsset == null)
			{
				LocalisationFontPair localisationFontPair2;
				if (LocalisationManager.GetFontAssetForCurrentLocale(out localisationFontPair2))
				{
					this.TextComponent.SetFont(localisationFontPair2.fontAsset, localisationFontPair2.legacyFontAsset);
				}
			}
			else
			{
				this.TextComponent.SetFont(localisationFontPair.fontAsset, localisationFontPair.legacyFontAsset);
			}
			if (localisationFontPair.fontSize != 0f && this.HasFontOverrides())
			{
				this.TextComponent.SetFontSize(localisationFontPair.fontSize);
			}
		}
		else
		{
			float time = Time.time;
		}
		if (this.HasFontOverrides())
		{
			this.TextComponent.SetCharSpacing(localisationFontPair.charSpacing);
		}
		this.TextComponent.SetText(newText);
	}

	private bool GetLocalizedFonts(out LocalisationFontPair fontData)
	{
		fontData = default(LocalisationFontPair);
		if (!this.HasFontOverrides())
		{
			return LocalisationManager.GetFontAssetForCurrentLocale(out fontData);
		}
		for (int i = 0; i < this._localisationFontsOverrides.Count; i++)
		{
			if (this._localisationFontsOverrides[i].ContainsLocale(LocalisationManager.CurrentLanguage))
			{
				fontData = new LocalisationFontPair
				{
					fontAsset = this._localisationFontsOverrides[i].fontAsset,
					legacyFontAsset = this._localisationFontsOverrides[i].legacyFontAsset,
					charSpacing = this._localisationFontsOverrides[i].charSpacing
				};
				return true;
			}
		}
		return LocalisationManager.GetFontAssetForCurrentLocale(out fontData);
	}

	[SerializeField]
	private bool _isLocalized;

	[SerializeField]
	private bool _isNewKey;

	[SerializeField]
	private string _newKeyName;

	[SerializeField]
	private ELocale _previewLocale;

	[SerializeField]
	private List<LocalisationFontPair> _localisationFontsOverrides = new List<LocalisationFontPair>();

	private static List<ELocale> _cachedELocalesList = new List<ELocale>();

	private TextComponentLegacySupportStore _textComponent;
}
