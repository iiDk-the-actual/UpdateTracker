using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public struct LocalizedStringContainer
{
	public string GetName()
	{
		string localizedString = this.StringReference.GetLocalizedString();
		string text = ((localizedString != null) ? localizedString.ToUpper() : null);
		if (string.IsNullOrEmpty(text) || text.ToLower().Contains("no translation found"))
		{
			return this.FallbackName;
		}
		return text;
	}

	[SerializeField]
	private LocalizedString StringReference;

	[SerializeField]
	private string FallbackName;
}
