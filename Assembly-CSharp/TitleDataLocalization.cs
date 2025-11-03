using System;
using UnityEngine;

[Serializable]
public class TitleDataLocalization
{
	public string GetLocalizedText()
	{
		Debug.Log("TODO: JH - Review localization method");
		string code = LocalisationManager.CurrentLanguage.Identifier.Code;
		if (!(code == "en"))
		{
			if (code == "fr")
			{
				return this.French;
			}
			if (code == "es")
			{
				return this.Spanish;
			}
			if (code == "it")
			{
				return this.Italian;
			}
			if (code == "de")
			{
				return this.German;
			}
			if (code == "ja")
			{
				return this.Japanese;
			}
		}
		return this.English;
	}

	public string English;

	public string French;

	public string German;

	public string Spanish;

	public string Italian;

	public string Japanese;
}
