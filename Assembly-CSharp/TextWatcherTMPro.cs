using System;
using TMPro;
using UnityEngine;

public class TextWatcherTMPro : MonoBehaviour
{
	private void Start()
	{
		this.myText = base.GetComponent<TextMeshPro>();
		this.textToCopy.AddCallback(new Action<string>(this.OnTextChanged), true);
	}

	private void OnDestroy()
	{
		this.textToCopy.RemoveCallback(new Action<string>(this.OnTextChanged));
	}

	private void OnTextChanged(string newText)
	{
		this.myText.text = newText;
	}

	public WatchableStringSO textToCopy;

	private TextMeshPro myText;
}
