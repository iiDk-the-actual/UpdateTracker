using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GorillaNetworking;
using UnityEngine;

internal class MockWarningServer : WarningsServer
{
	public static string ShownScreenPlayerPref
	{
		get
		{
			return "screen-shown-" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
		}
	}

	private void Awake()
	{
		if (WarningsServer.Instance == null)
		{
			WarningsServer.Instance = this;
			return;
		}
		Object.Destroy(this);
	}

	private PlayerAgeGateWarningStatus CreateWarningStatus(string header, string body, MockWarningServer.ButtonSetup? leftButtonSetup, MockWarningServer.ButtonSetup? rightButtonSetup, EImageVisibility showImage, Action leftButtonCallback, Action rightButtonCallback)
	{
		PlayerAgeGateWarningStatus playerAgeGateWarningStatus;
		playerAgeGateWarningStatus.header = header;
		playerAgeGateWarningStatus.body = body;
		playerAgeGateWarningStatus.leftButtonText = string.Empty;
		playerAgeGateWarningStatus.rightButtonText = string.Empty;
		playerAgeGateWarningStatus.leftButtonResult = WarningButtonResult.None;
		playerAgeGateWarningStatus.rightButtonResult = WarningButtonResult.None;
		playerAgeGateWarningStatus.showImage = showImage;
		playerAgeGateWarningStatus.onLeftButtonPressedAction = leftButtonCallback;
		playerAgeGateWarningStatus.onRightButtonPressedAction = rightButtonCallback;
		if (leftButtonSetup != null)
		{
			playerAgeGateWarningStatus.leftButtonText = leftButtonSetup.Value.buttonText;
			playerAgeGateWarningStatus.leftButtonResult = leftButtonSetup.Value.buttonResult;
		}
		if (rightButtonSetup != null)
		{
			playerAgeGateWarningStatus.rightButtonText = rightButtonSetup.Value.buttonText;
			playerAgeGateWarningStatus.rightButtonResult = rightButtonSetup.Value.buttonResult;
		}
		return playerAgeGateWarningStatus;
	}

	public override async Task<PlayerAgeGateWarningStatus?> FetchPlayerData(CancellationToken token)
	{
		int num = await KIDManager.CheckKIDPhase();
		PlayerAgeGateWarningStatus? playerAgeGateWarningStatus;
		if (token.IsCancellationRequested)
		{
			playerAgeGateWarningStatus = null;
		}
		else
		{
			bool flag = GorillaServer.Instance.CheckIsInKIDOptInCohort();
			bool flag2 = GorillaServer.Instance.CheckIsInKIDRequiredCohort();
			if (!this.ShouldShowWarningScreen(num, flag))
			{
				playerAgeGateWarningStatus = new PlayerAgeGateWarningStatus?(this.CreateWarningStatus("", "", null, null, EImageVisibility.None, null, null));
			}
			else
			{
				Debug.Log(string.Format("[KID::WARNING_SERVER] Phase Is: [{0}]", num));
				PlayerAgeGateWarningStatus playerAgeGateWarningStatus2;
				switch (num)
				{
				case 1:
				{
					MockWarningServer.ButtonSetup buttonSetup = new MockWarningServer.ButtonSetup("Continue", WarningButtonResult.CloseWarning);
					playerAgeGateWarningStatus2 = this.CreateWarningStatus("IMPORTANT NEWS", "We're working to make Gorilla Tag a better, more age-appropriate experience in our next update. To learn more, please check out our Discord.", null, new MockWarningServer.ButtonSetup?(buttonSetup), EImageVisibility.None, null, null);
					break;
				}
				case 2:
					if (flag)
					{
						MockWarningServer.ButtonSetup buttonSetup2 = new MockWarningServer.ButtonSetup("Do This Later", WarningButtonResult.CloseWarning);
						MockWarningServer.ButtonSetup buttonSetup3 = new MockWarningServer.ButtonSetup("Opt-In", WarningButtonResult.OptIn);
						playerAgeGateWarningStatus2 = this.CreateWarningStatus("IMPORTANT NEWS", "We have partnered with k-ID to create a better, more age-appropriate experience. Opt-in early and get 500 Shiny Rocks as our way of saying \"Thanks!\"", new MockWarningServer.ButtonSetup?(buttonSetup2), new MockWarningServer.ButtonSetup?(buttonSetup3), EImageVisibility.AfterBody, delegate
						{
							TelemetryData telemetryData2 = new TelemetryData
							{
								EventName = "kid_phase2_incohort",
								CustomTags = new string[]
								{
									"kid_warning_screen",
									"kid_phase_2",
									KIDTelemetry.GameVersionCustomTag,
									KIDTelemetry.GameEnvironment
								},
								BodyData = new Dictionary<string, string> { { "opt_in_choice", "skip" } }
							};
							GorillaTelemetry.EnqueueTelemetryEvent(telemetryData2.EventName, telemetryData2.BodyData, telemetryData2.CustomTags);
						}, delegate
						{
							TelemetryData telemetryData3 = new TelemetryData
							{
								EventName = "kid_phase2_incohort",
								CustomTags = new string[]
								{
									"kid_warning_screen",
									"kid_phase_2",
									KIDTelemetry.GameVersionCustomTag,
									KIDTelemetry.GameEnvironment
								},
								BodyData = new Dictionary<string, string> { { "opt_in_choice", "sign_up" } }
							};
							GorillaTelemetry.EnqueueTelemetryEvent(telemetryData3.EventName, telemetryData3.BodyData, telemetryData3.CustomTags);
						});
					}
					else
					{
						MockWarningServer.ButtonSetup buttonSetup4 = new MockWarningServer.ButtonSetup("Continue", WarningButtonResult.CloseWarning);
						playerAgeGateWarningStatus2 = this.CreateWarningStatus("IMPORTANT NEWS", "We're working to make Gorilla Tag a better, more age-appropriate experience in the coming days. To learn more, please check out our Discord.", null, new MockWarningServer.ButtonSetup?(buttonSetup4), EImageVisibility.None, null, null);
						TelemetryData telemetryData = new TelemetryData
						{
							EventName = "kid_screen_shown",
							CustomTags = new string[]
							{
								"kid_warning_screen",
								"kid_phase_2",
								KIDTelemetry.GameVersionCustomTag,
								KIDTelemetry.GameEnvironment
							},
							BodyData = new Dictionary<string, string> { { "screen", "phase2_nocohort" } }
						};
						GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
					}
					break;
				case 3:
					if (flag2)
					{
						string text;
						if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_CONTINUE", out text, "Continue"))
						{
							Debug.LogError("[LOCALIZATION::ATM_MANAGER] Failed to get key for [KID_WARNING_CONTINUE]");
						}
						MockWarningServer.ButtonSetup buttonSetup5 = new MockWarningServer.ButtonSetup(text, WarningButtonResult.OptIn);
						string text2;
						if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_TITLE", out text2, "IMPORTANT NEWS"))
						{
							Debug.LogError("[LOCALIZATION::ATM_MANAGER] Failed to get key for [KID_WARNING_TITLE]");
						}
						string text3;
						if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_PHASE_THREE_IN_COHORT", out text3, "We have partnered with k-ID to create a better, more age-appropriate experience. Confirm your age and get 500 Shiny Rocks as our way of saying \"Thanks!\""))
						{
							Debug.LogError("[LOCALIZATION::ATM_MANAGER] Failed to get key for [KID_WARNING_PHASE_THREE_IN_COHORT]");
						}
						playerAgeGateWarningStatus2 = this.CreateWarningStatus(text2, text3, new MockWarningServer.ButtonSetup?(buttonSetup5), null, EImageVisibility.AfterBody, delegate
						{
							TelemetryData telemetryData4 = new TelemetryData
							{
								EventName = "kid_screen_shown",
								CustomTags = new string[]
								{
									"kid_warning_screen",
									"kid_phase_3",
									KIDTelemetry.GameVersionCustomTag,
									KIDTelemetry.GameEnvironment
								},
								BodyData = new Dictionary<string, string> { { "screen", "phase3_required" } }
							};
							GorillaTelemetry.EnqueueTelemetryEvent(telemetryData4.EventName, telemetryData4.BodyData, telemetryData4.CustomTags);
						}, null);
					}
					else
					{
						MockWarningServer.ButtonSetup buttonSetup6 = new MockWarningServer.ButtonSetup("Do This Later", WarningButtonResult.CloseWarning);
						MockWarningServer.ButtonSetup buttonSetup7 = new MockWarningServer.ButtonSetup("Opt-In", WarningButtonResult.OptIn);
						playerAgeGateWarningStatus2 = this.CreateWarningStatus("IMPORTANT NEWS", "We have partnered with k-ID to create a better, more age-appropriate experience. Opt-in early and get 500 Shiny Rocks as our way of saying \"Thanks!\"", new MockWarningServer.ButtonSetup?(buttonSetup6), new MockWarningServer.ButtonSetup?(buttonSetup7), EImageVisibility.AfterBody, delegate
						{
							TelemetryData telemetryData5 = new TelemetryData
							{
								EventName = "kid_phase3_optional",
								CustomTags = new string[]
								{
									"kid_warning_screen",
									"kid_phase_3",
									KIDTelemetry.GameVersionCustomTag,
									KIDTelemetry.GameEnvironment
								},
								BodyData = new Dictionary<string, string> { { "opt_in_choice", "skip" } }
							};
							GorillaTelemetry.EnqueueTelemetryEvent(telemetryData5.EventName, telemetryData5.BodyData, telemetryData5.CustomTags);
						}, delegate
						{
							TelemetryData telemetryData6 = new TelemetryData
							{
								EventName = "kid_phase3_optional",
								CustomTags = new string[]
								{
									"kid_warning_screen",
									"kid_phase_3",
									KIDTelemetry.GameVersionCustomTag,
									KIDTelemetry.GameEnvironment
								},
								BodyData = new Dictionary<string, string> { { "opt_in_choice", "sign_up" } }
							};
							GorillaTelemetry.EnqueueTelemetryEvent(telemetryData6.EventName, telemetryData6.BodyData, telemetryData6.CustomTags);
						});
					}
					break;
				case 4:
					if (PlayFabAuthenticator.instance.IsReturningPlayer)
					{
						string text4;
						if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_CONTINUE", out text4, "Continue"))
						{
							Debug.LogError("[LOCALIZATION::ATM_MANAGER] Failed to get key for [KID_WARNING_CONTINUE]");
						}
						MockWarningServer.ButtonSetup buttonSetup8 = new MockWarningServer.ButtonSetup(text4, WarningButtonResult.OptIn);
						string text5;
						if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_TITLE", out text5, "IMPORTANT NEWS"))
						{
							Debug.LogError("[LOCALIZATION::ATM_MANAGER] Failed to get key for [KID_WARNING_TITLE]");
						}
						string text6;
						if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_PHASE_FOUR_RETURNING_PLAYER", out text6, "We have partnered with k-ID to create a better, more age-appropriate experience. Confirm your age and get 100 Shiny Rocks as our way of saying \"Thanks!\""))
						{
							Debug.LogError("[LOCALIZATION::ATM_MANAGER] Failed to get key for [KID_WARNING_PHASE_FOUR_RETURNING_PLAYER]");
						}
						playerAgeGateWarningStatus2 = this.CreateWarningStatus(text5, text6, null, new MockWarningServer.ButtonSetup?(buttonSetup8), EImageVisibility.AfterBody, delegate
						{
							TelemetryData telemetryData7 = new TelemetryData
							{
								EventName = "kid_screen_shown",
								CustomTags = new string[]
								{
									"kid_warning_screen",
									"kid_phase_4",
									KIDTelemetry.GameVersionCustomTag,
									KIDTelemetry.GameEnvironment
								},
								BodyData = new Dictionary<string, string> { { "screen", "phase4" } }
							};
							GorillaTelemetry.EnqueueTelemetryEvent(telemetryData7.EventName, telemetryData7.BodyData, telemetryData7.CustomTags);
						}, null);
					}
					else
					{
						playerAgeGateWarningStatus2 = this.CreateWarningStatus("", "", null, null, EImageVisibility.None, null, null);
					}
					break;
				default:
					return new PlayerAgeGateWarningStatus?(this.CreateWarningStatus("", "", null, null, EImageVisibility.None, null, null));
				}
				PlayerPrefs.SetInt(string.Format("phase-{0}-{1}", num, MockWarningServer.ShownScreenPlayerPref), 1);
				PlayerPrefs.Save();
				playerAgeGateWarningStatus = new PlayerAgeGateWarningStatus?(playerAgeGateWarningStatus2);
			}
		}
		return playerAgeGateWarningStatus;
	}

	public override async Task<PlayerAgeGateWarningStatus?> GetOptInFollowUpMessage(CancellationToken token)
	{
		int num = await KIDManager.CheckKIDPhase();
		PlayerAgeGateWarningStatus? playerAgeGateWarningStatus;
		if (token.IsCancellationRequested)
		{
			playerAgeGateWarningStatus = null;
		}
		else
		{
			PlayerAgeGateWarningStatus? playerAgeGateWarningStatus2 = null;
			switch (num)
			{
			case 2:
			{
				MockWarningServer.ButtonSetup buttonSetup = new MockWarningServer.ButtonSetup("Yay!", WarningButtonResult.CloseWarning);
				playerAgeGateWarningStatus2 = new PlayerAgeGateWarningStatus?(this.CreateWarningStatus("", "Your shiny rocks have been granted!", null, new MockWarningServer.ButtonSetup?(buttonSetup), EImageVisibility.BeforeBody, null, null));
				break;
			}
			case 3:
			{
				string text;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_FOLLOW_UP_YAY", out text, "Yay!"))
				{
					Debug.LogWarning("[KID::WARNING_SERVER] Missing localisation key: KID_WARNING_FOLLOW_UP_YAY");
				}
				MockWarningServer.ButtonSetup buttonSetup2 = new MockWarningServer.ButtonSetup(text, WarningButtonResult.CloseWarning);
				if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_OPT_IN_FOLLOW_MESSAGE", out text, "Your shiny rocks have been granted!"))
				{
					Debug.LogWarning("[KID::WARNING_SERVER] Missing localisation key: KID_WARNING_OPT_IN_FOLLOW_MESSAGE");
				}
				playerAgeGateWarningStatus2 = new PlayerAgeGateWarningStatus?(this.CreateWarningStatus("", text, null, new MockWarningServer.ButtonSetup?(buttonSetup2), EImageVisibility.BeforeBody, null, null));
				break;
			}
			case 4:
				if (PlayFabAuthenticator.instance.IsReturningPlayer)
				{
					string text;
					if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_FOLLOW_UP_YAY", out text, "Yay!"))
					{
						Debug.LogWarning("[KID::WARNING_SERVER] Missing localisation key: KID_WARNING_FOLLOW_UP_YAY");
					}
					MockWarningServer.ButtonSetup buttonSetup3 = new MockWarningServer.ButtonSetup(text, WarningButtonResult.CloseWarning);
					if (!LocalisationManager.TryGetKeyForCurrentLocale("KID_WARNING_OPT_IN_FOLLOW_MESSAGE", out text, "Your shiny rocks have been granted!"))
					{
						Debug.LogWarning("[KID::WARNING_SERVER] Missing localisation key: KID_WARNING_OPT_IN_FOLLOW_MESSAGE");
					}
					playerAgeGateWarningStatus2 = new PlayerAgeGateWarningStatus?(this.CreateWarningStatus("", text, null, new MockWarningServer.ButtonSetup?(buttonSetup3), EImageVisibility.BeforeBody, null, null));
				}
				break;
			}
			playerAgeGateWarningStatus = playerAgeGateWarningStatus2;
		}
		return playerAgeGateWarningStatus;
	}

	private bool ShouldShowWarningScreen(int phase, bool inOptInCohort)
	{
		if (PlayerPrefs.GetInt(string.Format("phase-{0}-{1}", phase, MockWarningServer.ShownScreenPlayerPref), 0) == 0)
		{
			return true;
		}
		switch (phase)
		{
		default:
			return false;
		case 2:
			return inOptInCohort;
		case 3:
		case 4:
			return true;
		}
	}

	private const string SHOWN_SCREEN_PREFIX = "screen-shown-";

	private const string KID_WARNING_TITLE_KEY = "KID_WARNING_TITLE";

	private const string KID_WARNING_CONTINUE_KEY = "KID_WARNING_CONTINUE";

	private const string KID_WARNING_PHASE_THREE_IN_COHORT_KEY = "KID_WARNING_PHASE_THREE_IN_COHORT";

	private const string KID_WARNING_PHASE_FOUR_RETURNING_PLAYER_KEY = "KID_WARNING_PHASE_FOUR_RETURNING_PLAYER";

	private const string KID_WARNING_OPT_IN_FOLLOW_MESSAGE_KEY = "KID_WARNING_OPT_IN_FOLLOW_MESSAGE";

	private const string KID_WARNING_FOLLOW_UP_YAY_KEY = "KID_WARNING_FOLLOW_UP_YAY";

	public struct ButtonSetup
	{
		public ButtonSetup(string txt, WarningButtonResult result)
		{
			this.buttonText = txt;
			this.buttonResult = result;
		}

		public string buttonText;

		public WarningButtonResult buttonResult;
	}
}
