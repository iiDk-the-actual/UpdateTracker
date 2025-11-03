using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class KIDUI_SetupScreen : MonoBehaviour
{
	private void Awake()
	{
		if (this._emailInputField == null)
		{
			Debug.LogErrorFormat("[KID::UI::Setup] Email Input Field is NULL", Array.Empty<object>());
			return;
		}
		if (this._confirmScreen == null)
		{
			Debug.LogErrorFormat("[KID::UI::Setup] Confirm Screen is NULL", Array.Empty<object>());
			return;
		}
		if (this._mainScreen == null)
		{
			Debug.LogErrorFormat("[KID::UI::Setup] Main Screen is NULL", Array.Empty<object>());
			return;
		}
	}

	private void OnEnable()
	{
		string @string = PlayerPrefs.GetString(KIDManager.GetEmailForUserPlayerPrefRef, "");
		this._emailInputField.text = @string;
		this._confirmButton.ResetButton();
		this.OnInputChanged(@string);
	}

	private void OnDisable()
	{
		if (this._keyboard == null)
		{
			return;
		}
		this._keyboard.active = false;
	}

	public void OnStartSetup()
	{
		base.gameObject.SetActive(true);
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_screen_shown",
			CustomTags = new string[]
			{
				"kid_setup",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string> { { "screen", "enter_email" } }
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
	}

	public void OnInputSelected()
	{
		Debug.LogFormat("[KID::UI::SETUP] Email Input Selected!", Array.Empty<object>());
	}

	public void OnInputChanged(string newVal)
	{
		bool flag = !string.IsNullOrEmpty(newVal);
		if (flag)
		{
			flag = Regex.IsMatch(newVal, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$");
		}
		this._confirmButton.interactable = flag;
	}

	public void OnSubmitEmailPressed()
	{
		PlayerPrefs.SetString(KIDManager.GetEmailForUserPlayerPrefRef, this._emailInputField.text);
		PlayerPrefs.Save();
		base.gameObject.SetActive(false);
		this._confirmScreen.OnEmailSubmitted(this._emailInputField.text);
	}

	public void OnBackPressed()
	{
		PlayerPrefs.SetString(KIDManager.GetEmailForUserPlayerPrefRef, this._emailInputField.text);
		PlayerPrefs.Save();
		base.gameObject.SetActive(false);
		this._mainScreen.ShowMainScreen(EMainScreenStatus.Previous);
	}

	[SerializeField]
	private TMP_InputField _emailInputField;

	[SerializeField]
	private KIDUIButton _confirmButton;

	[SerializeField]
	private KIDUI_ConfirmScreen _confirmScreen;

	[SerializeField]
	private KIDUI_MainScreen _mainScreen;

	[SerializeField]
	private TMP_Text _riftKeyboardMessage;

	private string _emailStr = string.Empty;

	private TouchScreenKeyboard _keyboard;
}
