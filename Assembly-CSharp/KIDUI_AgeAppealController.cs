using System;
using System.Collections.Generic;
using KID.Model;
using UnityEngine;

public class KIDUI_AgeAppealController : MonoBehaviour
{
	public static KIDUI_AgeAppealController Instance
	{
		get
		{
			return KIDUI_AgeAppealController._instance;
		}
	}

	private void Awake()
	{
		KIDUI_AgeAppealController._instance = this;
		Debug.LogFormat("[KID::UI::AGEAPPEALCONTROLLER] Controller Initialised", Array.Empty<object>());
	}

	public void StartAgeAppealScreens(SessionStatus? sessionStatus)
	{
		Debug.LogFormat("[KID::UI::AGEAPPEALCONTROLLER] Showing k-ID Age Appeal Screens", Array.Empty<object>());
		HandRayController.Instance.EnableHandRays();
		PrivateUIRoom.AddUI(base.transform);
		this._firstAgeAppealScreen.ShowRestrictedAccessScreen(sessionStatus);
		AgeStatusType ageStatusType;
		if (KIDManager.TryGetAgeStatusTypeFromAge(KIDAgeGate.UserAge, out ageStatusType))
		{
			TelemetryData telemetryData = new TelemetryData
			{
				EventName = "kid_age_appeal",
				CustomTags = new string[]
				{
					"kid_age_appeal",
					KIDTelemetry.GameVersionCustomTag,
					KIDTelemetry.GameEnvironment
				},
				BodyData = new Dictionary<string, string> { 
				{
					"submitted_age",
					ageStatusType.ToString()
				} }
			};
			GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		}
	}

	public void CloseKIDScreens()
	{
		PrivateUIRoom.RemoveUI(base.transform);
		HandRayController.Instance.DisableHandRays();
		this._firstAgeAppealScreen.gameObject.SetActive(false);
		Object.DestroyImmediate(base.gameObject);
	}

	public void StartTooYoungToPlayScreen()
	{
		Debug.LogFormat("[KID::UI::AGEAPPEALCONTROLLER] Showing k-ID Too Young to Play Screen", Array.Empty<object>());
		HandRayController.Instance.EnableHandRays();
		PrivateUIRoom.AddUI(base.transform);
		this._tooYoungToPlayScreen.ShowTooYoungToPlayScreen();
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_screen_shown",
			CustomTags = new string[]
			{
				"kid_age_appeal",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string> { { "screen", "blocked" } }
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
	}

	public void OnQuitGamePressed()
	{
		Application.Quit();
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

	private static KIDUI_AgeAppealController _instance;

	[SerializeField]
	private KIDUI_RestrictedAccessScreen _firstAgeAppealScreen;

	[SerializeField]
	private KIDUI_TooYoungToPlay _tooYoungToPlayScreen;
}
