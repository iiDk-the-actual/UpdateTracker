using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KID.Model;
using TMPro;
using UnityEngine;

public class KIDAgeGate : MonoBehaviour
{
	public static int UserAge
	{
		get
		{
			return KIDAgeGate._ageValue;
		}
	}

	public static bool DisplayedScreen { get; private set; }

	private void Awake()
	{
		if (KIDAgeGate._activeReference != null)
		{
			Debug.LogError("[KID::Age_Gate] Age Gate already exists, this is a duplicate, deleting the new one");
			Object.DestroyImmediate(base.gameObject);
			return;
		}
		KIDAgeGate._activeReference = this;
	}

	private async void Start()
	{
	}

	private void OnDestroy()
	{
		this.requestCancellationSource.Cancel();
	}

	public static async Task BeginAgeGate()
	{
		if (KIDAgeGate._activeReference == null)
		{
			Debug.LogError("[KID::Age_Gate] Unable to start Age Gate. No active reference assigned. Has it initialised yet?");
			do
			{
				await Task.Yield();
			}
			while (KIDAgeGate._activeReference == null);
		}
		await KIDAgeGate._activeReference.StartAgeGate();
	}

	private async Task StartAgeGate()
	{
		await this.InitialiseAgeGate();
	}

	private async Task InitialiseAgeGate()
	{
		Debug.Log("[KID] Initialising Age-Gate");
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_screen_shown",
			CustomTags = new string[]
			{
				KIDTelemetry.Open_MetricActionCustomTag,
				"kid_age_gate",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string> { { "screen", "age_gate" } }
		};
		TelemetryData telemetryData2 = telemetryData;
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData2.EventName, telemetryData2.BodyData, telemetryData2.CustomTags);
		for (;;)
		{
			KIDAgeGate.DisplayedScreen = true;
			this._ageSlider.ControllerActive = true;
			PrivateUIRoom.AddUI(this._uiParent.transform);
			HandRayController.Instance.EnableHandRays();
			await this.ProcessAgeGate();
			this._ageSlider.ControllerActive = false;
			KIDAudioManager instance = KIDAudioManager.Instance;
			if (instance != null)
			{
				instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
			}
			PrivateUIRoom.RemoveUI(this._uiParent.transform);
			if (this.requestCancellationSource.IsCancellationRequested)
			{
				break;
			}
			AgeStatusType ageStatusType;
			if (KIDManager.TryGetAgeStatusTypeFromAge(KIDAgeGate.UserAge, out ageStatusType))
			{
				telemetryData = new TelemetryData
				{
					EventName = "kid_age_gate",
					CustomTags = new string[]
					{
						KIDTelemetry.Closed_MetricActionCustomTag,
						"kid_age_gate",
						KIDTelemetry.GameVersionCustomTag,
						KIDTelemetry.GameEnvironment
					},
					BodyData = new Dictionary<string, string> { 
					{
						"age_declared",
						ageStatusType.ToString()
					} }
				};
				telemetryData2 = telemetryData;
				GorillaTelemetry.EnqueueTelemetryEvent(telemetryData2.EventName, telemetryData2.BodyData, telemetryData2.CustomTags);
			}
			this._confirmationUIManager.Reset(KIDAgeGate._ageValue);
			PrivateUIRoom.AddUI(this._confirmationUI.transform);
			bool flag = await this.ProcessAgeGateConfirmation();
			telemetryData = new TelemetryData
			{
				EventName = "kid_age_gate_confirm",
				CustomTags = new string[]
				{
					"kid_age_gate",
					KIDTelemetry.GameVersionCustomTag,
					KIDTelemetry.GameEnvironment
				},
				BodyData = new Dictionary<string, string> { 
				{
					"button_pressed",
					flag ? "confirm" : "go_back"
				} }
			};
			telemetryData2 = telemetryData;
			GorillaTelemetry.EnqueueTelemetryEvent(telemetryData2.EventName, telemetryData2.BodyData, telemetryData2.CustomTags);
			KIDAudioManager instance2 = KIDAudioManager.Instance;
			if (instance2 != null)
			{
				instance2.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
			}
			PrivateUIRoom.RemoveUI(this._confirmationUI.transform);
			HandRayController.Instance.DisableHandRays();
			if (flag)
			{
				goto Block_6;
			}
		}
		return;
		Block_6:
		await this.OnAgeGateCompleted();
		Debug.Log("[KID] Age Gate Complete");
	}

	private async Task ProcessAgeGate()
	{
		Debug.Log("[KID] Waiting for Age Confirmation");
		await this.WaitForAgeChoice();
	}

	private async Task<bool> ProcessAgeGateConfirmation()
	{
		while (this._confirmationUIManager.Result == KidAgeConfirmationResult.None)
		{
			if (this.requestCancellationSource.IsCancellationRequested)
			{
				return false;
			}
			await Task.Yield();
		}
		return this._confirmationUIManager.Result == KidAgeConfirmationResult.Confirm;
	}

	private async Task WaitForAgeChoice()
	{
		KIDAgeGate._hasChosenAge = false;
		while (!this.requestCancellationSource.IsCancellationRequested)
		{
			await Task.Yield();
			if (KIDAgeGate._hasChosenAge)
			{
				KIDAgeGate._ageValue = this._ageSlider.CurrentAge;
				string ageString = this._ageSlider.GetAgeString();
				this._confirmationAgeText.text = "You entered " + ageString + "\n\nPlease be sure to enter your real age so we can customize your experience!";
				return;
			}
		}
	}

	public static void OnConfirmAgePressed(int currentAge)
	{
		KIDAgeGate._hasChosenAge = true;
	}

	private async Task OnAgeGateCompleted()
	{
		this.FinaliseAgeGateAndContinue();
	}

	private void FinaliseAgeGateAndContinue()
	{
		if (this.requestCancellationSource.IsCancellationRequested)
		{
			return;
		}
		Debug.Log("[KID::AGE_GATE] Age gate completed");
		Object.Destroy(base.gameObject);
	}

	private void QuitGame()
	{
		Debug.Log("[KID] QUIT PRESSED");
		Application.Quit();
	}

	private async void AppealAge()
	{
		Debug.Log("[KID] APPEAL PRESSED");
		if (!KIDManager.InitialisationComplete)
		{
			Debug.LogError("[KID] [KIDManager] has not been Initialised yet. Unable to start appeals flow. Will wait until ready");
			do
			{
				await Task.Yield();
			}
			while (!KIDManager.InitialisationComplete);
		}
		if (KIDManager.InitialisationSuccessful)
		{
			string text = "VERIFY AGE";
			string text2 = "GETTING ONE TIME PASSCODE. PLEASE WAIT.\n\nGIVE IT TO A PARENT/GUARDIAN TO ENTER IT AT: k-id.com/code";
			string empty = string.Empty;
			this._pregameMessageReference.ShowMessage(text, text2, empty, new Action(this.RefreshChallengeStatus), 0.25f, 0f);
		}
		Debug.LogError("[KID::AGE_GATE] TODO: Refactor Age-Appeal flow");
	}

	private void AppealRejected()
	{
		Debug.Log("[KID] APPEAL REJECTED");
		string text = "UNDER AGE";
		string text2 = "Your VR platform requires a certain minimum age to play Gorilla Tag. Unfortunately, due to those age requirements, we cannot allow you to play Gorilla Tag at this time.\n\nIf you incorrectly submitted your age, please appeal.";
		string text3 = "Hold any face button to appeal";
		this._pregameMessageReference.ShowMessage(text, text2, text3, new Action(this.AppealAge), 0.25f, 0f);
	}

	private void RefreshChallengeStatus()
	{
	}

	public static void SetAgeGateConfig(GetRequirementsData response)
	{
		KIDAgeGate._ageGateConfig = response;
	}

	public void OnWhyAgeGateButtonPressed()
	{
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_screen_shown",
			CustomTags = new string[]
			{
				"kid_age_gate",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string> { { "screen", "why_age_gate" } }
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		this._uiParent.SetActive(false);
		PrivateUIRoom.AddUI(this._whyAgeGateScreen.transform);
		this._whyAgeGateScreen.SetActive(true);
	}

	public void OnWhyAgeGateButtonBackPressed()
	{
		this._uiParent.SetActive(true);
		PrivateUIRoom.RemoveUI(this._whyAgeGateScreen.transform);
		this._whyAgeGateScreen.SetActive(false);
	}

	public void OnLearnMoreAboutKIDPressed()
	{
		this._metrics_LearnMorePressed = true;
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_screen_shown",
			CustomTags = new string[]
			{
				"kid_age_gate",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, string> { { "screen", "learn_more_url" } }
		};
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		Application.OpenURL("https://whyagegate.com/");
	}

	private const string LEARN_MORE_URL = "https://whyagegate.com/";

	private const string DEFAULT_AGE_VALUE_STRING = "SET AGE";

	private const int MINIMUM_PLATFORM_AGE = 13;

	[Header("Age Gate Settings")]
	[SerializeField]
	private PreGameMessage _pregameMessageReference;

	[SerializeField]
	private KIDUI_AgeDiscrepancyScreen _ageDiscrepancyScreen;

	[SerializeField]
	private GameObject _uiParent;

	[SerializeField]
	private AgeSliderWithProgressBar _ageSlider;

	[SerializeField]
	private GameObject _confirmationUI;

	[SerializeField]
	private KIDAgeGateConfirmation _confirmationUIManager;

	[SerializeField]
	private TMP_Text _confirmationAgeText;

	[SerializeField]
	private GameObject _whyAgeGateScreen;

	private const string strBlockAccessTitle = "UNDER AGE";

	private const string strBlockAccessMessage = "Your VR platform requires a certain minimum age to play Gorilla Tag. Unfortunately, due to those age requirements, we cannot allow you to play Gorilla Tag at this time.\n\nIf you incorrectly submitted your age, please appeal.";

	private const string strBlockAccessConfirm = "Hold any face button to appeal";

	private const string strVerifyAgeTitle = "VERIFY AGE";

	private const string strVerifyAgeMessage = "GETTING ONE TIME PASSCODE. PLEASE WAIT.\n\nGIVE IT TO A PARENT/GUARDIAN TO ENTER IT AT: k-id.com/code";

	private const string strDiscrepancyMessage = "You entered {0} for your age,\nbut your Meta account says you should be {1}. You could be logged into the wrong Meta account on this device.\n\nWe will use the lowest age ({2})\nif you Continue.";

	private static KIDAgeGate _activeReference;

	private static GetRequirementsData _ageGateConfig;

	private static int _ageValue;

	private CancellationTokenSource requestCancellationSource = new CancellationTokenSource();

	private static bool _hasChosenAge;

	private bool _metrics_LearnMorePressed;
}
