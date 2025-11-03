using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GorillaNetworking;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine;
using UnityEngine.Events;

public class KIDMessagingController : MonoBehaviour
{
	private static string HasShownConfirmationScreenPlayerPref
	{
		get
		{
			return "hasShownKIDConfirmationScreen-" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
		}
	}

	public void OnConfirmPressed()
	{
		this._closeMessageBox = true;
	}

	private void Awake()
	{
		if (KIDMessagingController.instance != null)
		{
			Debug.LogError("[KID::MESSAGING_CONTROLLER] Trying to start a new [KIDMessagingController] but one already exists");
			Object.Destroy(this);
			return;
		}
		KIDMessagingController.instance = this;
	}

	private bool ShouldShowConfirmationScreen()
	{
		return !KIDManager.CurrentSession.IsDefault;
	}

	private async Task StartKIDConfirmationScreenInternal(CancellationToken token)
	{
		if (this.messageBox == null)
		{
			Debug.LogError("[KID::MESSAGING_CONTROLLER] Trying to show confirmation screen but [messageBox] is null");
		}
		else
		{
			string text = await KIDMessagingController.GetSetupConfirmationMessage();
			if (string.IsNullOrEmpty(text))
			{
				text = "k-ID setup is now complete. Thanks and have fun in Gorilla World!";
			}
			string text2;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_SETUP_CONFIRMATION_TITLE", out text2, "Thank you"))
			{
				Debug.LogError("[LOCALIZATION::KID_MESSAGING_CONTROLLER] Failed to get key for k-ID localization [KID_SETUP_CONFIRMATION_TITLE]");
			}
			this.messageBox.Header = text2;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_SETUP_CONFIRMATION_BODY", out text2, text))
			{
				Debug.LogError("[LOCALIZATION::KID_MESSAGING_CONTROLLER] Failed to get key for k-ID localization [KID_SETUP_CONFIRMATION_BODY]");
			}
			this.messageBox.Body = text2;
			this.messageBox.LeftButton = string.Empty;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_SETUP_CONFIRMATION_BUTTON", out text2, "Continue"))
			{
				Debug.LogError("[LOCALIZATION::KID_MESSAGING_CONTROLLER] Failed to get key for k-ID localization [KID_SETUP_CONFIRMATION_BUTTON]");
			}
			this.messageBox.RightButton = text2;
			this.messageBox.gameObject.SetActive(true);
			HandRayController.Instance.EnableHandRays();
			PrivateUIRoom.AddUI(base.transform);
			while (!token.IsCancellationRequested)
			{
				await Task.Yield();
				if (this._closeMessageBox)
				{
					PrivateUIRoom.RemoveUI(base.transform);
					HandRayController.Instance.DisableHandRays();
					this.messageBox.gameObject.SetActive(false);
					await KIDManager.TrySetHasConfirmedStatus();
					break;
				}
			}
		}
	}

	public void OnDisable()
	{
		KIDAudioManager kidaudioManager = KIDAudioManager.Instance;
		if (kidaudioManager == null)
		{
			return;
		}
		kidaudioManager.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
	}

	public static async Task StartKIDConfirmationScreen(CancellationToken token)
	{
		KIDMessagingController kidmessagingController = KIDMessagingController.instance;
		if (kidmessagingController == null || kidmessagingController.ShouldShowConfirmationScreen())
		{
			await KIDMessagingController.instance.StartKIDConfirmationScreenInternal(token);
			TelemetryData telemetryData = new TelemetryData
			{
				EventName = "kid_screen_shown",
				CustomTags = new string[]
				{
					"kid_setup",
					KIDTelemetry.GameVersionCustomTag,
					KIDTelemetry.GameEnvironment
				},
				BodyData = new Dictionary<string, string>
				{
					{ "screen", "setup_complete" },
					{
						"saw_game_settings",
						KIDUI_MainScreen.ShownSettingsScreen.ToString().ToLower() ?? ""
					}
				}
			};
			GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		}
	}

	private static async Task<string> GetSetupConfirmationMessage()
	{
		int state = 0;
		string bodyText = string.Empty;
		PlayFabTitleDataCache.Instance.GetTitleData("KIDData", delegate(string res)
		{
			state = 1;
			bodyText = KIDMessagingController.GetConfirmMessageFromTitleDataJson(res);
		}, delegate(PlayFabError err)
		{
			state = -1;
			Debug.LogError("[KID_MANAGER] Something went wrong trying to get title data for key: [KIDData]. Error:\n" + err.ErrorMessage);
		}, false);
		do
		{
			await Task.Yield();
		}
		while (state == 0);
		return bodyText;
	}

	private static string GetConfirmMessageFromTitleDataJson(string jsonTxt)
	{
		if (string.IsNullOrEmpty(jsonTxt))
		{
			Debug.LogError("[KID_MANAGER] Cannot get Confirmation Message. JSON is null or empty!");
			return null;
		}
		KIDMessagingTitleData kidmessagingTitleData = JsonConvert.DeserializeObject<KIDMessagingTitleData>(jsonTxt);
		if (kidmessagingTitleData == null)
		{
			Debug.LogError("[KID_MANAGER] Failed to parse json to [KIDMessagingTitleData]. Json: \n" + jsonTxt);
			return null;
		}
		if (string.IsNullOrEmpty(kidmessagingTitleData.KIDSetupConfirmation))
		{
			Debug.LogError("[KID_MANAGER] Failed to parse json to [KIDMessagingTitleData] - [KIDSetupConfirmation] is null or empty. Json: \n" + jsonTxt);
			return null;
		}
		return kidmessagingTitleData.KIDSetupConfirmation;
	}

	public static void ShowConnectionErrorScreen()
	{
		if (KIDMessagingController.instance == null || KIDMessagingController.instance.messageBox == null)
		{
			Debug.LogError("[KID::MESSAGING_CONTROLLER] No message box");
			return;
		}
		KIDMessagingController.instance._closeMessageBox = false;
		KIDMessagingController.instance.messageBox.Header = "Connection Error";
		KIDMessagingController.instance.messageBox.Body = "Unable to connect to the internet. Please restart the game and try again.";
		KIDMessagingController.instance.messageBox.RightButton = "Quit";
		KIDMessagingController.instance.messageBox.ShowQuitButtonAsPrimary();
		KIDMessagingController.instance.messageBox.RightButtonCallback.RemoveAllListeners();
		KIDMessagingController.instance.messageBox.RightButtonCallback.AddListener(new UnityAction(Application.Quit));
		KIDMessagingController.instance.messageBox.gameObject.SetActive(true);
		HandRayController.Instance.EnableHandRays();
		PrivateUIRoom.AddUI(KIDMessagingController.instance.transform);
	}

	private const string SHOWN_CONFIRMATION_SCREEN_PREFIX = "hasShownKIDConfirmationScreen-";

	private const string CONFIRMATION_HEADER = "Thank you";

	private const string CONFIRMATION_BODY = "k-ID setup is now complete. Thanks and have fun in Gorilla World!";

	private const string CONFIRMATION_BUTTON = "Continue";

	private const string KID_SETUP_CONFIRMATION_TITLE_KEY = "KID_SETUP_CONFIRMATION_TITLE";

	private const string KID_SETUP_CONFIRMATION_BODY_KEY = "KID_SETUP_CONFIRMATION_BODY";

	private const string KID_SETUP_CONFIRMATION_BUTTON_KEY = "KID_SETUP_CONFIRMATION_BUTTON";

	private static KIDMessagingController instance;

	[SerializeField]
	private MessageBox messageBox;

	private const string CONNECTION_ERROR_HEADER = "Connection Error";

	private const string CONNECTION_ERROR_BODY = "Unable to connect to the internet. Please restart the game and try again.";

	private const string CONNECTION_ERROR_BUTTON = "Quit";

	private bool _closeMessageBox;
}
