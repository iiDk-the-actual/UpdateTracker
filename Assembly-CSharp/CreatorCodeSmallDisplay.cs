using System;
using UnityEngine;
using UnityEngine.UI;

public class CreatorCodeSmallDisplay : MonoBehaviour
{
	private void Awake()
	{
		this.codeText.text = "CREATOR CODE: <NONE>";
		ATM_Manager.instance.smallDisplays.Add(this);
	}

	public void SetCode(string code)
	{
		if (code == "")
		{
			this.codeText.text = "CREATOR CODE: <NONE>";
			return;
		}
		this.codeText.text = "CREATOR CODE: " + code;
	}

	public void SuccessfulPurchase(string memberName)
	{
		if (!string.IsNullOrWhiteSpace(memberName))
		{
			this.codeText.text = "SUPPORTED: " + memberName + "!";
		}
	}

	public Text codeText;

	private const string CreatorCode = "CREATOR CODE: ";

	private const string CreatorSupported = "SUPPORTED: ";

	private const string NoCreator = "<NONE>";
}
