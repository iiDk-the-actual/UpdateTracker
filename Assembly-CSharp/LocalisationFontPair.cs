using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public struct LocalisationFontPair
{
	public bool ContainsLocale(Locale locale)
	{
		int count = this.locales.Count;
		for (int i = 0; i < this.locales.Count; i++)
		{
			if (!(this.locales[i] == null) && this.locales[i].Identifier.Code == locale.Identifier.Code)
			{
				return true;
			}
		}
		return false;
	}

	public List<Locale> locales;

	public TMP_FontAsset fontAsset;

	public Font legacyFontAsset;

	public float charSpacing;

	public float lineSpacing;

	public float fontSize;
}
