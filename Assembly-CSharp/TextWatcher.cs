using System;
using UnityEngine;
using UnityEngine.UI;

public class TextWatcher : MonoBehaviour
{
	private void Start()
	{
		this.myText = base.GetComponent<Text>();
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

	private Text myText;
}
