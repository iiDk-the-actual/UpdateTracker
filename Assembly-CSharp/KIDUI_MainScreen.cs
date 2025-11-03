using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GorillaNetworking;
using KID.Model;
using UnityEngine;
using UnityEngine.Localization;

public class KIDUI_MainScreen : MonoBehaviour
{
	private void Awake()
	{
		KIDUI_MainScreen._featuresList.Clear();
		if (this._setupKidScreen == null)
		{
			Debug.LogErrorFormat("[KID::UI::Setup] Setup K-ID Screen is NULL", Array.Empty<object>());
			return;
		}
		if (this._initialised)
		{
			return;
		}
		this.InitialiseMainScreen();
	}

	private void OnEnable()
	{
		KIDManager.RegisterSessionUpdateCallback_AnyPermission(new Action(this.UpdatePermissionsAndFeaturesScreen));
		LocalisationManager.RegisterOnLanguageChanged(new Action(this.OnLanguageChanged));
		this.UpdatePermissionsAndFeaturesScreen();
	}

	private void OnDisable()
	{
		KIDManager.UnregisterSessionUpdateCallback_AnyPermission(new Action(this.UpdatePermissionsAndFeaturesScreen));
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance != null)
		{
			instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
		}
		LocalisationManager.UnregisterOnLanguageChanged(new Action(this.OnLanguageChanged));
	}

	private void OnDestroy()
	{
	}

	private void ConstructFeatureSettings()
	{
		for (int i = 0; i < this._displayOrder.Length; i++)
		{
			for (int j = 0; j < this._featureSetups.Count; j++)
			{
				if (this._featureSetups[j].linkedFeature == this._displayOrder[i])
				{
					this.CreateNewFeatureDisplay(this._featureSetups[j]);
					break;
				}
			}
		}
		this.UpdatePermissionsAndFeaturesScreen();
	}

	private void CreateNewFeatureDisplay(KIDUI_MainScreen.FeatureToggleSetup setup)
	{
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(setup.linkedFeature);
		if (permissionDataByFeature == null)
		{
			Debug.LogErrorFormat("[KID::UI::MAIN] Failed to retrieve permission data for feature; [" + setup.linkedFeature.ToString() + "]", Array.Empty<object>());
			return;
		}
		if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			return;
		}
		if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PLAYER)
		{
			if (permissionDataByFeature.Enabled)
			{
				return;
			}
			if (KIDManager.CheckFeatureOptIn(setup.linkedFeature, null).Item2)
			{
				return;
			}
		}
		if (setup.alwaysCheckFeatureSetting && KIDManager.CheckFeatureSettingEnabled(setup.linkedFeature))
		{
			return;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this._featurePrefab, this._featureRootTransform);
		KIDUIFeatureSetting component = gameObject.GetComponent<KIDUIFeatureSetting>();
		if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.GUARDIAN)
		{
			Debug.LogFormat(string.Format("[KID::UI::MAIN_SCREEN] Adding new Locked Feature:  {0} Is enabled: {1}", setup.linkedFeature.ToString(), permissionDataByFeature.Enabled), Array.Empty<object>());
			component.CreateNewFeatureSettingGuardianManaged(setup, permissionDataByFeature.Enabled);
			if (!KIDUI_MainScreen._featuresList.ContainsKey(setup.linkedFeature))
			{
				KIDUI_MainScreen._featuresList.Add(setup.linkedFeature, new List<KIDUIFeatureSetting>());
			}
			KIDUI_MainScreen._featuresList[setup.linkedFeature].Add(component);
			return;
		}
		if (setup.requiresToggle)
		{
			component.CreateNewFeatureSettingWithToggle(setup, false, setup.alwaysCheckFeatureSetting);
		}
		else
		{
			component.CreateNewFeatureSettingWithoutToggle(setup, setup.alwaysCheckFeatureSetting);
		}
		if (!KIDUI_MainScreen._featuresList.ContainsKey(setup.linkedFeature))
		{
			KIDUI_MainScreen._featuresList.Add(setup.linkedFeature, new List<KIDUIFeatureSetting>());
		}
		KIDUI_MainScreen._featuresList[setup.linkedFeature].Add(component);
		this.ConstructAdditionalSetup(setup.linkedFeature, gameObject);
	}

	private void ConstructAdditionalSetup(EKIDFeatures feature, GameObject featureObject)
	{
	}

	private void UpdatePermissionsAndFeaturesScreen()
	{
		int num = 0;
		Debug.LogFormat(string.Format("[KID::UI::MAIN] Updated Feature listings. To Update: [{0}]", KIDUI_MainScreen._featuresList.Count), Array.Empty<object>());
		foreach (KeyValuePair<EKIDFeatures, List<KIDUIFeatureSetting>> keyValuePair in KIDUI_MainScreen._featuresList)
		{
			for (int i = 0; i < keyValuePair.Value.Count; i++)
			{
				Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(keyValuePair.Key);
				if (permissionDataByFeature == null)
				{
					Debug.LogErrorFormat("[KID::UI::MAIN] Failed to find permission data for feature: [" + keyValuePair.Key.ToString() + "]", Array.Empty<object>());
				}
				else if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.GUARDIAN)
				{
					keyValuePair.Value[i].SetGuardianManagedState(permissionDataByFeature.Enabled);
				}
				else
				{
					bool flag = KIDManager.CheckFeatureOptIn(keyValuePair.Key, permissionDataByFeature).Item2;
					if (keyValuePair.Value[i].AlwaysCheckFeatureSetting)
					{
						flag = KIDManager.CheckFeatureSettingEnabled(keyValuePair.Key);
					}
					if (!keyValuePair.Value[i].GetHasToggle())
					{
						keyValuePair.Value[i].SetPlayerManagedState(permissionDataByFeature.Enabled, flag);
					}
				}
			}
		}
		int num2 = 0;
		foreach (KeyValuePair<EKIDFeatures, List<KIDUIFeatureSetting>> keyValuePair2 in KIDUI_MainScreen._featuresList)
		{
			for (int j = 0; j < keyValuePair2.Value.Count; j++)
			{
				num2++;
				Permission permissionDataByFeature2 = KIDManager.GetPermissionDataByFeature(keyValuePair2.Key);
				if (keyValuePair2.Value[j].GetFeatureToggleState() || permissionDataByFeature2.ManagedBy == Permission.ManagedByEnum.PLAYER)
				{
					num++;
				}
			}
		}
		if (num >= num2)
		{
			if (!this._initialised)
			{
				this._titleFeaturePermissions.SetActive(false);
				this._titleGameFeatures.SetActive(true);
			}
			this._hasAllPermissions = true;
			this._getPermissionsButton.gameObject.SetActive(false);
			this._gettingPermissionsButton.gameObject.SetActive(false);
			this._requestPermissionsButton.gameObject.SetActive(false);
			this._permissionsTip.SetActive(false);
			this.SetButtonContainersVisibility(EGetPermissionsStatus.RequestedPermission);
		}
	}

	private bool IsFeatureToggledOn(EKIDFeatures permissionFeature)
	{
		List<KIDUIFeatureSetting> list;
		if (!KIDUI_MainScreen._featuresList.TryGetValue(permissionFeature, out list))
		{
			return true;
		}
		KIDUIFeatureSetting kiduifeatureSetting = list.FirstOrDefault<KIDUIFeatureSetting>();
		if (kiduifeatureSetting == null)
		{
			Debug.LogErrorFormat(string.Format("[KID::UI::MAIN] Empty list for permission Name [{0}]", permissionFeature), Array.Empty<object>());
			return false;
		}
		return kiduifeatureSetting.GetFeatureToggleState();
	}

	public void InitialiseMainScreen()
	{
		if (this._initialised)
		{
			Debug.Log("[KID::MAIN_SCREEN] Already Initialised");
			return;
		}
		this.ConstructFeatureSettings();
		this._declinedStatus.SetActive(false);
		this._timeoutStatus.SetActive(false);
		this._pendingStatus.SetActive(false);
		this._updatedStatus.SetActive(false);
		this._setupRequiredStatus.SetActive(false);
		this._missingStatus.SetActive(false);
		this._fullPlayerControlStatus.SetActive(false);
		this._initialised = true;
	}

	public void ShowMainScreen(EMainScreenStatus showStatus, KIDUI_Controller.Metrics_ShowReason reason)
	{
		this.ShowMainScreen(showStatus);
		this._mainScreenOpenedReason = reason;
		string text = reason.ToString().Replace("_", "-").ToLower();
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "kid_game_settings",
			CustomTags = new string[]
			{
				"kid_setup",
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment,
				KIDTelemetry.Open_MetricActionCustomTag
			},
			BodyData = new Dictionary<string, string> { { "screen_shown_reason", text } }
		};
		foreach (Permission permission in KIDManager.GetAllPermissionsData())
		{
			telemetryData.BodyData.Add(KIDTelemetry.GetPermissionManagedByBodyData(permission.Name), permission.ManagedBy.ToString().ToLower());
			telemetryData.BodyData.Add(KIDTelemetry.GetPermissionEnabledBodyData(permission.Name), permission.Enabled.ToString().ToLower());
		}
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
	}

	public void ShowMainScreen(EMainScreenStatus showStatus)
	{
		KIDUI_MainScreen.ShownSettingsScreen = true;
		base.gameObject.SetActive(true);
		this.ConfigurePermissionsButtons();
		this.UpdateScreenStatus(showStatus, false);
	}

	public void UpdateScreenStatus(EMainScreenStatus showStatus, bool sendMetrics = false)
	{
		if (sendMetrics && showStatus == EMainScreenStatus.Updated)
		{
			string text = this._mainScreenOpenedReason.ToString().Replace("_", "-").ToLower();
			TelemetryData telemetryData = new TelemetryData
			{
				EventName = "kid_game_settings",
				CustomTags = new string[]
				{
					"kid_setup",
					KIDTelemetry.GameVersionCustomTag,
					KIDTelemetry.GameEnvironment,
					KIDTelemetry.Updated_MetricActionCustomTag
				},
				BodyData = new Dictionary<string, string> { { "screen_shown_reason", text } }
			};
			foreach (Permission permission in KIDManager.GetAllPermissionsData())
			{
				telemetryData.BodyData.Add(KIDTelemetry.GetPermissionManagedByBodyData(permission.Name), permission.ManagedBy.ToString().ToLower());
				telemetryData.BodyData.Add(KIDTelemetry.GetPermissionEnabledBodyData(permission.Name), permission.Enabled.ToString().ToLower());
			}
			GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		}
		GameObject activeStatusObject = this.GetActiveStatusObject();
		this._declinedStatus.SetActive(false);
		this._timeoutStatus.SetActive(false);
		this._pendingStatus.SetActive(false);
		this._updatedStatus.SetActive(false);
		this._setupRequiredStatus.SetActive(false);
		this._missingStatus.SetActive(false);
		this._fullPlayerControlStatus.SetActive(false);
		switch (showStatus)
		{
		default:
			if (!this._hasAllPermissions)
			{
				this._missingStatus.SetActive(true);
			}
			else if (this._hasAllPermissions)
			{
				this._fullPlayerControlStatus.SetActive(true);
			}
			else
			{
				this._screenStatus = showStatus;
			}
			break;
		case EMainScreenStatus.Declined:
			this._declinedStatus.SetActive(true);
			this._screenStatus = showStatus;
			break;
		case EMainScreenStatus.Pending:
			this._pendingStatus.SetActive(true);
			this._screenStatus = showStatus;
			break;
		case EMainScreenStatus.Timedout:
			this._timeoutStatus.SetActive(true);
			this._screenStatus = showStatus;
			break;
		case EMainScreenStatus.Setup:
			this._setupRequiredStatus.SetActive(true);
			this._screenStatus = showStatus;
			break;
		case EMainScreenStatus.Previous:
			if (activeStatusObject != null)
			{
				activeStatusObject.SetActive(true);
			}
			else
			{
				this._updatedStatus.SetActive(true);
			}
			break;
		case EMainScreenStatus.FullControl:
			this._fullPlayerControlStatus.SetActive(true);
			break;
		}
		this.SetButtonContainersVisibility(KIDUI_MainScreen.GetPermissionState());
	}

	public void HideMainScreen()
	{
		base.gameObject.SetActive(false);
	}

	public async void OnAskForPermission()
	{
		this._requestPermissionsButton.interactable = false;
		this._getPermissionsButton.interactable = false;
		this._gettingPermissionsButton.interactable = false;
		await this._animatedEllipsis.StartAnimation();
		bool missingPermissionsPostUpdate = await this.UpdateAndCheckForMissingPermissions();
		this._requestPermissionsButton.interactable = true;
		this._getPermissionsButton.interactable = true;
		this._gettingPermissionsButton.interactable = true;
		await this._animatedEllipsis.StopAnimation();
		if (missingPermissionsPostUpdate)
		{
			base.gameObject.SetActive(false);
			if (KIDManager.CurrentSession.IsDefault)
			{
				this._setupKidScreen.OnStartSetup();
			}
			else
			{
				List<string> list = new List<string>(this.CollectPermissionsToUpgrade());
				await this._sendUpgradeEmailScreen.SendUpgradeEmail(list);
				if (KIDManager.CurrentSession.ManagedBy == Session.ManagedByEnum.PLAYER)
				{
					this._setupKidScreen.OnStartSetup();
				}
				KIDManager.WaitForAndUpdateNewSession(true);
			}
		}
	}

	public void OnSaveAndExit()
	{
		if (KIDManager.CurrentSession == null)
		{
			Debug.LogError("[KID::KID_UI_MAINSCREEN] There is no session as such cannot opt into anything");
			KIDUI_Controller.Instance.CloseKIDScreens();
			return;
		}
		List<Permission> allPermissionsData = KIDManager.GetAllPermissionsData();
		for (int i = 0; i < allPermissionsData.Count; i++)
		{
			string name = allPermissionsData[i].Name;
			if (!(name == "multiplayer"))
			{
				if (!(name == "mods"))
				{
					if (!(name == "join-groups"))
					{
						if (!(name == "voice-chat"))
						{
							if (!(name == "custom-username"))
							{
								Debug.LogError("[KID::UI::MainScreen] Unhandled permission when saving and exiting: [" + allPermissionsData[i].Name + "]");
							}
							else
							{
								this.UpdateOptInSetting(allPermissionsData[i], EKIDFeatures.Custom_Nametags, delegate(bool b, Permission p, bool hasOptedInPreviously)
								{
									GorillaComputer.instance.SetNametagSetting(b, p.ManagedBy, hasOptedInPreviously);
								});
							}
						}
						else
						{
							this.UpdateOptInSetting(allPermissionsData[i], EKIDFeatures.Voice_Chat, delegate(bool b, Permission p, bool hasOptedInPreviously)
							{
								GorillaComputer.instance.KID_SetVoiceChatSettingOnStart(b, p.ManagedBy, hasOptedInPreviously);
							});
						}
					}
				}
				else
				{
					this.UpdateOptInSetting(allPermissionsData[i], EKIDFeatures.Mods, null);
				}
			}
			else
			{
				this.UpdateOptInSetting(allPermissionsData[i], EKIDFeatures.Multiplayer, null);
			}
		}
		KIDManager.SendOptInPermissions();
		if (this._screenStatus != EMainScreenStatus.None)
		{
			string text = this._mainScreenOpenedReason.ToString().Replace("_", "-").ToLower();
			TelemetryData telemetryData = new TelemetryData
			{
				EventName = "kid_game_settings",
				CustomTags = new string[]
				{
					"kid_setup",
					KIDTelemetry.GameVersionCustomTag,
					KIDTelemetry.GameEnvironment
				},
				BodyData = new Dictionary<string, string>
				{
					{ "screen_shown_reason", text },
					{
						"kid_status",
						this._screenStatus.ToString().ToLower()
					},
					{ "button_pressed", "save_and_continue" }
				}
			};
			GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		}
		else
		{
			Debug.LogError("[KID::UI::MAIN_SCREEN] Trying to close k-ID Main Screen, but screen status is set to [None] - Invalid status, will not submit analytics");
		}
		KIDUI_Controller.Instance.CloseKIDScreens();
	}

	public int GetFeatureListingCount()
	{
		int num = 0;
		foreach (List<KIDUIFeatureSetting> list in KIDUI_MainScreen._featuresList.Values)
		{
			num += list.Count;
		}
		return num;
	}

	private async Task<bool> UpdateAndCheckForMissingPermissions()
	{
		bool hasUpdated = false;
		bool wasSuccess = false;
		float cutOffDuration = Time.time + 15f;
		KIDManager.UpdateSession(delegate(bool success)
		{
			hasUpdated = true;
			wasSuccess = success;
		});
		do
		{
			await Task.Yield();
		}
		while (Time.time < cutOffDuration && !hasUpdated);
		this.UpdatePermissionsAndFeaturesScreen();
		if (wasSuccess)
		{
			bool flag = false;
			foreach (Permission permission in KIDManager.CurrentSession.GetAllPermissions())
			{
				if (permission.ManagedBy == Permission.ManagedByEnum.GUARDIAN && !permission.Enabled)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.UpdateScreenStatus(EMainScreenStatus.FullControl, false);
				return false;
			}
		}
		return true;
	}

	private void OnLanguageChanged()
	{
		foreach (KeyValuePair<EKIDFeatures, List<KIDUIFeatureSetting>> keyValuePair in KIDUI_MainScreen._featuresList)
		{
			List<KIDUIFeatureSetting> value = keyValuePair.Value;
			if (value != null)
			{
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i] != null)
					{
						value[i].RefreshTextOnLanguageChanged();
					}
				}
			}
		}
	}

	private void UpdateOptInSetting(Permission permissionData, EKIDFeatures feature, Action<bool, Permission, bool> onOptedIn)
	{
		bool item = KIDManager.CheckFeatureOptIn(feature, permissionData).Item2;
		bool flag = this.IsFeatureToggledOn(feature);
		Debug.Log(string.Format("[KID::UI::MainScreen] Update opt in for {0}. Has opted in: {1}. Toggled on: {2}", feature.ToString(), item, flag));
		KIDManager.SetFeatureOptIn(feature, flag);
		if (onOptedIn != null)
		{
			onOptedIn(flag, permissionData, item);
		}
	}

	public void OnConfirmedEmailAddress(string emailAddress)
	{
		this._emailAddress = emailAddress;
		Debug.LogFormat("[KID::UI::Main] Email has been confirmed: " + this._emailAddress, Array.Empty<object>());
	}

	private IEnumerable<string> CollectPermissionsToUpgrade()
	{
		return from permission in KIDManager.GetAllPermissionsData()
			where permission.ManagedBy == Permission.ManagedByEnum.GUARDIAN && !permission.Enabled
			select permission.Name;
	}

	private void ConfigurePermissionsButtons()
	{
		Debug.Log("[KID::MAIN_SCREEN] CONFIGURE BUTTONS");
		if (!this._getPermissionsButton.gameObject.activeSelf && !this._gettingPermissionsButton.gameObject.activeSelf)
		{
			Debug.Log("[KID::MAIN_SCREEN] CONFIGURE BUTTONS - GET PERMISSIONS IS DISABLED");
			return;
		}
		Debug.Log("[KID::MAIN_SCREEN] CONFIGURE BUTTONS - CHECK SESSION STATUS: Is Default: [" + KIDManager.CurrentSession.IsDefault.ToString() + "]");
		this.SetButtonContainersVisibility(KIDUI_MainScreen.GetPermissionState());
	}

	private void SetButtonContainersVisibility(EGetPermissionsStatus permissionStatus)
	{
		Debug.Log("[KID::MAIN_SCREEN] CONFIGURE BUTTONS - PERMISSION STATE: [" + permissionStatus.ToString() + "]");
		this._defaultButtonsContainer.SetActive(permissionStatus == EGetPermissionsStatus.GetPermission);
		this._permissionsRequestingButtonContainer.SetActive(permissionStatus == EGetPermissionsStatus.RequestingPermission);
		this._permissionsRequestedButtonContainer.SetActive(permissionStatus == EGetPermissionsStatus.RequestedPermission);
	}

	private GameObject GetActiveStatusObject()
	{
		foreach (GameObject gameObject in new List<GameObject> { this._declinedStatus, this._timeoutStatus, this._pendingStatus, this._updatedStatus, this._setupRequiredStatus, this._fullPlayerControlStatus })
		{
			if (gameObject.activeInHierarchy)
			{
				return gameObject;
			}
		}
		return null;
	}

	private static EGetPermissionsStatus GetPermissionState()
	{
		if (!KIDManager.CurrentSession.IsDefault)
		{
			Debug.Log("[KID::MAIN_SCREEN] CONFIGURE BUTTONS - SHOW REQUESTED");
			return EGetPermissionsStatus.RequestedPermission;
		}
		if (PlayerPrefs.GetInt(KIDManager.GetChallengedBeforePlayerPrefRef, 0) == 0)
		{
			Debug.Log("[KID::MAIN_SCREEN] CONFIGURE BUTTONS - SHOW DEFAULT");
			return EGetPermissionsStatus.GetPermission;
		}
		Debug.Log("[KID::MAIN_SCREEN] CONFIGURE BUTTONS - SHOW SWAPPED DEFAULT");
		return EGetPermissionsStatus.RequestingPermission;
	}

	private void OnFeatureToggleChanged(EKIDFeatures feature)
	{
		switch (feature)
		{
		case EKIDFeatures.Multiplayer:
			this.OnMultiplayerToggled();
			return;
		case EKIDFeatures.Custom_Nametags:
			this.OnCustomNametagsToggled();
			return;
		case EKIDFeatures.Voice_Chat:
			this.OnVoiceChatToggled();
			return;
		case EKIDFeatures.Mods:
			this.OnModToggleChanged();
			return;
		case EKIDFeatures.Groups:
			this.OnGroupToggleChanged();
			return;
		default:
			Debug.LogErrorFormat("[KID::UI::MAIN_SCREEN] Toggle NOT YET IMPLEMENTED for Feature: " + feature.ToString() + ".", Array.Empty<object>());
			return;
		}
	}

	private void OnMultiplayerToggled()
	{
		Debug.LogErrorFormat("[KID::UI::MAIN_SCREEN] MULTIPLAYER Toggle NOT YET IMPLEMENTED.", Array.Empty<object>());
	}

	private void OnVoiceChatToggled()
	{
		Debug.LogErrorFormat("[KID::UI::MAIN_SCREEN] VOICE CHAT Toggle NOT YET IMPLEMENTED.", Array.Empty<object>());
	}

	private void OnGroupToggleChanged()
	{
		Debug.LogErrorFormat("[KID::UI::MAIN_SCREEN] GROUPS Toggle NOT YET IMPLEMENTED.", Array.Empty<object>());
	}

	private void OnModToggleChanged()
	{
		Debug.LogErrorFormat("[KID::UI::MAIN_SCREEN] MODS Toggle NOT YET IMPLEMENTED.", Array.Empty<object>());
	}

	private void OnCustomNametagsToggled()
	{
		Debug.LogErrorFormat("[KID::UI::MAIN_SCREEN] CUSTOM USERNAMES Toggle NOT YET IMPLEMENTED.", Array.Empty<object>());
	}

	public const string OPT_IN_SUFFIX = "-opt-in";

	public static bool ShownSettingsScreen = false;

	[SerializeField]
	private GameObject _kidScreensGroup;

	[SerializeField]
	private KIDUI_SetupScreen _setupKidScreen;

	[SerializeField]
	private KIDUI_SendUpgradeEmailScreen _sendUpgradeEmailScreen;

	[SerializeField]
	private KIDUI_AnimatedEllipsis _animatedEllipsis;

	[Header("Permission Request Buttons")]
	[SerializeField]
	private KIDUIButton _getPermissionsButton;

	[SerializeField]
	private KIDUIButton _gettingPermissionsButton;

	[SerializeField]
	private KIDUIButton _requestPermissionsButton;

	[SerializeField]
	private GameObject _defaultButtonsContainer;

	[SerializeField]
	private GameObject _permissionsRequestingButtonContainer;

	[SerializeField]
	private GameObject _permissionsRequestedButtonContainer;

	private bool _hasAllPermissions;

	[Header("Dynamic Feature Settings Setup")]
	[SerializeField]
	private GameObject _featurePrefab;

	[SerializeField]
	private Transform _featureRootTransform;

	[SerializeField]
	private EKIDFeatures[] _displayOrder = new EKIDFeatures[4];

	[SerializeField]
	private List<KIDUI_MainScreen.FeatureToggleSetup> _featureSetups = new List<KIDUI_MainScreen.FeatureToggleSetup>();

	[Header("Additional Feature-Specific Setup")]
	[SerializeField]
	private GameObject _voiceChatLabel;

	[Header("Hide Permissions Tip")]
	[SerializeField]
	private GameObject _permissionsTip;

	[Header("Titles")]
	[SerializeField]
	private GameObject _titleFeaturePermissions;

	[SerializeField]
	private GameObject _titleGameFeatures;

	[Header("Game Status Setup")]
	[SerializeField]
	private GameObject _missingStatus;

	[SerializeField]
	private GameObject _updatedStatus;

	[SerializeField]
	private GameObject _declinedStatus;

	[SerializeField]
	private GameObject _pendingStatus;

	[SerializeField]
	private GameObject _timeoutStatus;

	[SerializeField]
	private GameObject _setupRequiredStatus;

	[SerializeField]
	private GameObject _fullPlayerControlStatus;

	private string _emailAddress;

	private bool _multiplayerEnabled;

	private bool _customNameEnabled;

	private bool _voiceChatEnabled;

	private bool _initialised;

	private KIDUI_Controller.Metrics_ShowReason _mainScreenOpenedReason;

	private EMainScreenStatus _screenStatus;

	private GameObject _eventSystemObj;

	private static Dictionary<EKIDFeatures, List<KIDUIFeatureSetting>> _featuresList = new Dictionary<EKIDFeatures, List<KIDUIFeatureSetting>>();

	[Serializable]
	public struct FeatureToggleSetup
	{
		public EKIDFeatures linkedFeature;

		public string permissionName;

		public LocalizedString featureName;

		public bool requiresToggle;

		public bool alwaysCheckFeatureSetting;

		public LocalizedString enabledText;

		public LocalizedString disabledText;
	}
}
