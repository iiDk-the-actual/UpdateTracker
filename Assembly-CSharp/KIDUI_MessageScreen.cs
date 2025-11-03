using System;
using TMPro;
using UnityEngine;

public class KIDUI_MessageScreen : MonoBehaviour
{
	public void Show(string errorMessage)
	{
		base.gameObject.SetActive(true);
		if (errorMessage != null && errorMessage.Length > 0)
		{
			this._errorTxt.text = errorMessage;
		}
	}

	public void OnClose()
	{
		base.gameObject.SetActive(false);
		this._mainScreen.ShowMainScreen(EMainScreenStatus.Pending);
	}

	public void OnDisable()
	{
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance == null)
		{
			return;
		}
		instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
	}

	[SerializeField]
	private KIDUI_MainScreen _mainScreen;

	[SerializeField]
	private TMP_Text _errorTxt;
}
