using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GorillaNetworking;
using PlayFab;
using UnityEngine;
using UnityEngine.UI;

public class LegalAgreementBodyText : MonoBehaviour
{
	private void Awake()
	{
		this.textCollection.Add(this.textBox);
	}

	public void SetText(string text)
	{
		text = Regex.Unescape(text);
		string[] array = text.Split(new string[]
		{
			Environment.NewLine,
			"\\r\\n",
			"\n"
		}, StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			Text text2;
			if (i >= this.textCollection.Count)
			{
				text2 = Object.Instantiate<Text>(this.textBox, base.transform);
				this.textCollection.Add(text2);
			}
			else
			{
				text2 = this.textCollection[i];
			}
			text2.text = array[i];
		}
	}

	public void ClearText()
	{
		foreach (Text text in this.textCollection)
		{
			text.text = string.Empty;
		}
		this.state = LegalAgreementBodyText.State.Ready;
	}

	public async Task<bool> UpdateTextFromPlayFabTitleData(string key, string version)
	{
		string text = key + "_" + version;
		this.state = LegalAgreementBodyText.State.Loading;
		PlayFabTitleDataCache.Instance.GetTitleData(text, new Action<string>(this.OnTitleDataReceived), new Action<PlayFabError>(this.OnPlayFabError), false);
		while (this.state == LegalAgreementBodyText.State.Loading)
		{
			await Task.Yield();
		}
		bool flag;
		if (this.cachedText != null)
		{
			this.SetText(this.cachedText.Substring(1, this.cachedText.Length - 2));
			flag = true;
		}
		else
		{
			flag = false;
		}
		return flag;
	}

	private void OnPlayFabError(PlayFabError obj)
	{
		Debug.LogError("ERROR: " + obj.ErrorMessage);
		this.state = LegalAgreementBodyText.State.Error;
	}

	private void OnTitleDataReceived(string text)
	{
		this.cachedText = text;
		this.state = LegalAgreementBodyText.State.Ready;
	}

	public float Height
	{
		get
		{
			return this.rectTransform.rect.height;
		}
	}

	[SerializeField]
	private Text textBox;

	[SerializeField]
	private TextAsset textAsset;

	[SerializeField]
	private RectTransform rectTransform;

	private List<Text> textCollection = new List<Text>();

	private string cachedText;

	private LegalAgreementBodyText.State state;

	private enum State
	{
		Ready,
		Loading,
		Error
	}
}
