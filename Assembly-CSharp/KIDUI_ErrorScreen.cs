using System;
using TMPro;
using UnityEngine;

public class KIDUI_ErrorScreen : MonoBehaviour
{
	public void ShowErrorScreen(string title, string email, string errorMessage)
	{
		this._titleTxt.text = title;
		this._emailTxt.text = email;
		this._errorTxt.text = errorMessage;
		base.gameObject.SetActive(true);
	}

	public void OnClose()
	{
		base.gameObject.SetActive(false);
		this._mainScreen.ShowMainScreen(EMainScreenStatus.None);
	}

	public void OnQuitGame()
	{
		Application.Quit();
	}

	public void OnBack()
	{
		base.gameObject.SetActive(false);
		this._setupScreen.OnStartSetup();
	}

	[SerializeField]
	private TMP_Text _titleTxt;

	[SerializeField]
	private TMP_Text _emailTxt;

	[SerializeField]
	private TMP_Text _errorTxt;

	[SerializeField]
	private KIDUI_MainScreen _mainScreen;

	[SerializeField]
	private KIDUI_SetupScreen _setupScreen;
}
