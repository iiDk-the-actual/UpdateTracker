using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class KIDUI_AgeAppealEmailScreen : MonoBehaviour
{
	public void ShowAgeAppealEmailScreen(bool receivedChallenge, int newAge)
	{
		this.newAgeToAppeal = newAge;
		base.gameObject.SetActive(true);
		this.hasChallenge = receivedChallenge;
		this._enterEmailText.text = (this.hasChallenge ? this.PARENT_EMAIL_DESCRIPTION : this.VERIFY_AGE_EMAIL_DESCRIPTION);
		if (this._parentPermissionNotice)
		{
			this._parentPermissionNotice.SetActive(this.hasChallenge);
		}
		this.OnInputChanged(this._emailText.text);
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_age_appeal_enter_email",
			CustomTags = new string[]
			{
				"kid_age_appeal",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string> { 
			{
				"email_type",
				this.hasChallenge ? "under_dac" : "over_dac"
			} }
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
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

	public void OnConfirmPressed()
	{
		if (string.IsNullOrEmpty(this._emailText.text))
		{
			Debug.LogError("[KID::UI::APPEAL_AGE_EMAIL] Age Appeal Email Text is empty");
			return;
		}
		this._confirmationScreen.ShowAgeAppealConfirmationScreen(this.hasChallenge, this.newAgeToAppeal, this._emailText.text);
		base.gameObject.SetActive(false);
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
	private KIDUIButton _confirmButton;

	[SerializeField]
	private KIDUI_AgeAppealEmailConfirmation _confirmationScreen;

	[SerializeField]
	private TMP_Text _enterEmailText;

	[SerializeField]
	private TMP_InputField _emailText;

	[SerializeField]
	private GameObject _parentPermissionNotice;

	private string PARENT_EMAIL_DESCRIPTION = "Enter your parent or guardian's email address below.";

	private string VERIFY_AGE_EMAIL_DESCRIPTION = "Enter your email address below";

	private bool hasChallenge = true;

	private int newAgeToAppeal;
}
