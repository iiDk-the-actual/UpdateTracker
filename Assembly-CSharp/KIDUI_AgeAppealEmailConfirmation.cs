using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class KIDUI_AgeAppealEmailConfirmation : MonoBehaviour
{
	private void OnEnable()
	{
		KIDManager.onEmailResultReceived = (KIDManager.OnEmailResultReceived)Delegate.Combine(KIDManager.onEmailResultReceived, new KIDManager.OnEmailResultReceived(this.NotifyOfEmailResult));
	}

	private void OnDisable()
	{
		KIDManager.onEmailResultReceived = (KIDManager.OnEmailResultReceived)Delegate.Remove(KIDManager.onEmailResultReceived, new KIDManager.OnEmailResultReceived(this.NotifyOfEmailResult));
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance == null)
		{
			return;
		}
		instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
	}

	public void ShowAgeAppealConfirmationScreen(bool hasChallenge, int newAge, string emailToConfirm)
	{
		this.hasChallenge = hasChallenge;
		this.newAgeToAppeal = newAge;
		this._confirmText.text = (this.hasChallenge ? this.CONFIRM_PARENT_EMAIL : this.CONFIRM_YOUR_EMAIL);
		this._emailText.text = emailToConfirm;
		base.gameObject.SetActive(true);
	}

	public void OnConfirmPressed()
	{
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_age_appeal_confirm_email",
			CustomTags = new string[]
			{
				"kid_age_appeal",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string>
			{
				{
					"email_type",
					this.hasChallenge ? "under_dac" : "over_dac"
				},
				{ "button_pressed", "confirm" }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		if (this.hasChallenge)
		{
			this.StartAgeAppealChallengeEmail();
			return;
		}
		this.StartAgeAppealEmail();
	}

	public void OnBackPressed()
	{
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_age_appeal_confirm_email",
			CustomTags = new string[]
			{
				"kid_age_appeal",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string>
			{
				{
					"email_type",
					this.hasChallenge ? "under_dac" : "over_dac"
				},
				{ "button_pressed", "go_back" }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		base.gameObject.SetActive(false);
		this._ageAppealEmailScreen.ShowAgeAppealEmailScreen(this.hasChallenge, this.newAgeToAppeal);
	}

	private void StartAgeAppealChallengeEmail()
	{
		KIDUI_AgeAppealEmailConfirmation.<StartAgeAppealChallengeEmail>d__16 <StartAgeAppealChallengeEmail>d__;
		<StartAgeAppealChallengeEmail>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
		<StartAgeAppealChallengeEmail>d__.<>4__this = this;
		<StartAgeAppealChallengeEmail>d__.<>1__state = -1;
		<StartAgeAppealChallengeEmail>d__.<>t__builder.Start<KIDUI_AgeAppealEmailConfirmation.<StartAgeAppealChallengeEmail>d__16>(ref <StartAgeAppealChallengeEmail>d__);
	}

	private async Task StartAgeAppealEmail()
	{
		TaskAwaiter<bool> taskAwaiter = KIDManager.TryAppealAge(this._emailText.text, this.newAgeToAppeal).GetAwaiter();
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			TaskAwaiter<bool> taskAwaiter2;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<bool>);
		}
		if (!taskAwaiter.GetResult())
		{
			base.gameObject.SetActive(false);
			this._errorScreen.ShowAgeAppealEmailErrorScreen(this.hasChallenge, this.newAgeToAppeal, this._emailText.text);
		}
		else
		{
			Debug.Log("[KID::UI::APPEAL_AGE_EMAIL] Age appeal succesful for [" + this._emailText.text + "]. Proceeding tu Success screen");
			base.gameObject.SetActive(false);
			this._successScreen.ShowSuccessScreenAppeal(this._emailText.text);
		}
	}

	private void NotifyOfEmailResult(bool success)
	{
		if (this._successScreen == null)
		{
			Debug.LogError("[KID::AGE_APPEAL_EMAIL] _successScreen has not been set yet and is NULL. Cannot inform of result");
			return;
		}
		this._hasCompletedSendEmailRequest = true;
		if (success)
		{
			base.gameObject.SetActive(false);
			this._successScreen.ShowSuccessScreenAppeal(this._emailText.text);
			return;
		}
	}

	private void ShowErrorScreen()
	{
		Debug.LogErrorFormat("[KID::UI::Setup] K-ID Confirmation Failed - Failed to send email", Array.Empty<object>());
		base.gameObject.SetActive(false);
		this._errorScreen.ShowAgeAppealEmailErrorScreen(this.hasChallenge, this.newAgeToAppeal, this._emailText.text);
	}

	[SerializeField]
	private TMP_Text _confirmText;

	[SerializeField]
	private TMP_Text _emailText;

	private string CONFIRM_PARENT_EMAIL = "Please confirm your parent or guardian's email address.";

	private string CONFIRM_YOUR_EMAIL = "Please confirm your email address.";

	private bool hasChallenge = true;

	private int newAgeToAppeal;

	private bool _hasCompletedSendEmailRequest;

	[SerializeField]
	private KIDUI_EmailSuccess _successScreen;

	[SerializeField]
	private KIDUI_AgeAppealEmailError _errorScreen;

	[SerializeField]
	private KIDUI_AgeAppealEmailScreen _ageAppealEmailScreen;

	[SerializeField]
	private int _minimumDelay = 1000;
}
