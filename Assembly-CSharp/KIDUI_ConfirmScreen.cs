using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using UnityEngine;

public class KIDUI_ConfirmScreen : MonoBehaviour
{
	private void Awake()
	{
		if (this._emailToConfirmTxt == null)
		{
			Debug.LogErrorFormat("[KID::UI::Setup] Email To Confirm Field is NULL", Array.Empty<object>());
			return;
		}
		if (this._setupScreen == null)
		{
			Debug.LogErrorFormat("[KID::UI::Setup] Setup K-ID Screen is NULL", Array.Empty<object>());
			return;
		}
		if (this._mainScreen == null)
		{
			Debug.LogErrorFormat("[KID::UI::Setup] Main Screen is NULL", Array.Empty<object>());
			return;
		}
		this._cancellationTokenSource = new CancellationTokenSource();
	}

	private void OnEnable()
	{
		this._confirmButton.interactable = true;
		this._backButton.interactable = true;
	}

	public void OnEmailSubmitted(string emailAddress)
	{
		this._submittedEmailAddress = emailAddress;
		this._emailToConfirmTxt.text = this._submittedEmailAddress;
		base.gameObject.SetActive(true);
	}

	public void OnConfirmPressed()
	{
		KIDUI_ConfirmScreen.<OnConfirmPressed>d__16 <OnConfirmPressed>d__;
		<OnConfirmPressed>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
		<OnConfirmPressed>d__.<>4__this = this;
		<OnConfirmPressed>d__.<>1__state = -1;
		<OnConfirmPressed>d__.<>t__builder.Start<KIDUI_ConfirmScreen.<OnConfirmPressed>d__16>(ref <OnConfirmPressed>d__);
	}

	public async void OnBackPressed()
	{
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_email_confirm",
			CustomTags = new string[]
			{
				"kid_setup",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string> { { "button_pressed", "go_back" } }
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		this._cancellationTokenSource.Cancel();
		await this._animatedEllipsis.StopAnimation();
		base.gameObject.SetActive(false);
		this._setupScreen.OnStartSetup();
	}

	public void NotifyOfResult(bool success)
	{
		this._hasCompletedSendEmailRequest = true;
		this._emailRequestResult = success;
	}

	private async void ShowErrorScreen(string errorMessage)
	{
		Debug.LogErrorFormat("[KID::UI::Setup] K-ID Confirmation Failed - Failed to send email", Array.Empty<object>());
		this._cancellationTokenSource.Cancel();
		await this._animatedEllipsis.StopAnimation();
		base.gameObject.SetActive(false);
		this._errorScreen.ShowErrorScreen("Confirmation Error", this._submittedEmailAddress, errorMessage);
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
	private TMP_Text _emailToConfirmTxt;

	[SerializeField]
	private KIDUI_MainScreen _mainScreen;

	[SerializeField]
	private KIDUI_SetupScreen _setupScreen;

	[SerializeField]
	private KIDUI_ErrorScreen _errorScreen;

	[SerializeField]
	private KIDUI_EmailSuccess _successScreen;

	[SerializeField]
	private KIDUI_AnimatedEllipsis _animatedEllipsis;

	[SerializeField]
	private KIDUIButton _confirmButton;

	[SerializeField]
	private KIDUIButton _backButton;

	[SerializeField]
	private int _minimumDelay = 1000;

	private string _submittedEmailAddress;

	private CancellationTokenSource _cancellationTokenSource;

	private bool _hasCompletedSendEmailRequest;

	private bool _emailRequestResult;
}
