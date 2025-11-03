using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizationTextSyncer : MonoBehaviour
{
	private void Start()
	{
		this.OnLanguageChanged();
	}

	private void OnEnable()
	{
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		if (LocalisationManager.Instance == null)
		{
			return;
		}
		this.OnLanguageChanged();
	}

	private void OnDisable()
	{
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.OnLanguageChanged));
	}

	private void OnDestroy()
	{
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.OnLanguageChanged));
	}

	private void OnLanguageChanged()
	{
		LocalisationFontPair localisationFontPair;
		LocalisationManager.GetFontAssetForCurrentLocale(out localisationFontPair);
		LocalisationFontPair localisationFontPair2;
		bool flag = this.TryGetFontDataOverride(out localisationFontPair2);
		if (!flag && !LocalisationManager.GetFontAssetForCurrentLocale(out localisationFontPair2))
		{
			return;
		}
		foreach (LocalizationTextSyncer.TextCompSyncData textCompSyncData in this._textComponentsToSync)
		{
			if (!(textCompSyncData.textComponent == null))
			{
				LocalisationFontPair localisationFontPair3;
				if (textCompSyncData.overrideLanguageSettings && textCompSyncData.GetOverrideForLanguage(out localisationFontPair3))
				{
					localisationFontPair2 = localisationFontPair3;
				}
				if (localisationFontPair2.fontAsset != null)
				{
					textCompSyncData.textComponent.font = localisationFontPair2.fontAsset;
				}
				else
				{
					textCompSyncData.textComponent.font = localisationFontPair.fontAsset;
				}
				if (flag)
				{
					textCompSyncData.textComponent.characterSpacing = localisationFontPair2.charSpacing;
					textCompSyncData.textComponent.lineSpacing = localisationFontPair2.lineSpacing;
					if (localisationFontPair2.fontSize != 0f)
					{
						textCompSyncData.textComponent.fontSize = (textCompSyncData.textComponent.fontSizeMax = localisationFontPair2.fontSize);
					}
				}
			}
		}
	}

	private bool TryGetFontDataOverride(out LocalisationFontPair fontDataOverride)
	{
		fontDataOverride = default(LocalisationFontPair);
		for (int i = 0; i < this._universalFontOverrides.Count; i++)
		{
			if (this._universalFontOverrides[i].ContainsLocale(LocalisationManager.CurrentLanguage))
			{
				fontDataOverride = this._universalFontOverrides[i];
				return true;
			}
		}
		return false;
	}

	[SerializeField]
	[Tooltip("List of all the Text Components - and optional overrides - that will be updated when langauge changes")]
	private List<LocalizationTextSyncer.TextCompSyncData> _textComponentsToSync = new List<LocalizationTextSyncer.TextCompSyncData>();

	[SerializeField]
	[Tooltip("List of optional overrides that will be applied to ALL Text Components on this object")]
	private List<LocalisationFontPair> _universalFontOverrides = new List<LocalisationFontPair>();

	[Serializable]
	public struct TextCompSyncData
	{
		public bool GetOverrideForLanguage(out LocalisationFontPair fontData)
		{
			fontData = default(LocalisationFontPair);
			for (int i = 0; i < this._fontOverrides.Count; i++)
			{
				if (this._fontOverrides[i].ContainsLocale(LocalisationManager.CurrentLanguage))
				{
					fontData = this._fontOverrides[i];
					return true;
				}
			}
			return false;
		}

		public TMP_Text textComponent;

		public bool overrideLanguageSettings;

		public List<LocalisationFontPair> _fontOverrides;
	}
}
