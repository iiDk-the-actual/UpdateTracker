using System;
using KID.Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

public class KIDUIFeatureSetting : MonoBehaviour
{
	public bool AlwaysCheckFeatureSetting { get; private set; }

	public void CreateNewFeatureSettingGuardianManaged(KIDUI_MainScreen.FeatureToggleSetup feature, bool isEnabled)
	{
		this.CreateNewFeatureSettingWithoutToggle(feature, false);
		this._guardianManagedEnabled.SetActive(isEnabled);
		this._guardianManagedLocked.SetActive(!isEnabled);
	}

	public KIDUIToggle CreateNewFeatureSettingWithToggle(KIDUI_MainScreen.FeatureToggleSetup feature, bool initialState = false, bool alwaysCheckFeatureSetting = false)
	{
		this.SetFeatureData(feature, alwaysCheckFeatureSetting, true);
		this._featureToggle.SetValue(initialState);
		KIDUIToggle featureToggle = this._featureToggle;
		if (featureToggle != null)
		{
			featureToggle.RegisterOnChangeEvent(new Action(this.SetFeatureName));
		}
		return this._featureToggle;
	}

	public void CreateNewFeatureSettingWithoutToggle(KIDUI_MainScreen.FeatureToggleSetup feature, bool alwaysCheckFeatureSetting = false)
	{
		this.SetFeatureData(feature, alwaysCheckFeatureSetting, false);
	}

	private void SetFeatureData(KIDUI_MainScreen.FeatureToggleSetup feature, bool alwaysCheckFeatureSetting, bool featureToggleEnabled)
	{
		string text;
		if (!LocalisationManager.TryGetTranslationForCurrentLocaleWithLocString(feature.enabledText, out text, "ON", null))
		{
			Debug.LogError(string.Format("[LOCALIZATION::FEATURE_SETTING] Failed to get key for  k-ID Feature [{0}]\n[{1}]", feature.featureName, feature.enabledText), this);
		}
		this._enabledTextStr = text;
		if (!LocalisationManager.TryGetTranslationForCurrentLocaleWithLocString(feature.disabledText, out text, "OFF", null))
		{
			Debug.LogError(string.Format("[LOCALIZATION::FEATURE_SETTING] Failed to get key for  k-ID Feature [{0}]\n[{1}]", feature.featureName, feature.disabledText), this);
		}
		this._disabledTextStr = text;
		this._hasToggle = featureToggleEnabled;
		this._featureType = feature.linkedFeature;
		if (!LocalisationManager.TryGetTranslationForCurrentLocaleWithLocString(feature.featureName, out text, feature.permissionName, null))
		{
			Debug.LogError(string.Format("[LOCALIZATION::FeatureSetting] Failed to get key for k-ID Feature [{0}]\n[{1}]", feature.featureName, feature.disabledText), this);
		}
		this._featureName = text;
		this.SetFeatureName();
		GameObject gameObject = base.gameObject;
		string name = gameObject.name;
		string text2 = "_";
		LocalizedString featureName = feature.featureName;
		gameObject.name = name + text2 + ((featureName != null) ? featureName.ToString() : null);
		this._permissionName = feature.permissionName;
		this._featureToggle.gameObject.SetActive(featureToggleEnabled);
		this.AlwaysCheckFeatureSetting = alwaysCheckFeatureSetting;
		this._feature = feature;
	}

	public void RefreshTextOnLanguageChanged()
	{
		string text;
		if (!LocalisationManager.TryGetTranslationForCurrentLocaleWithLocString(this._feature.enabledText, out text, "ON", null))
		{
			Debug.LogError(string.Format("[LOCALIZATION::FeatureSetting] Failed to get key for Game Mode [{0}]", this._feature.enabledText));
		}
		this._enabledTextStr = text;
		Debug.Log("[KIDUIFeatureSetting::Language] Refreshed enabled text: " + this._enabledTextStr);
		if (!LocalisationManager.TryGetTranslationForCurrentLocaleWithLocString(this._feature.disabledText, out text, "OFF", null))
		{
			Debug.LogError(string.Format("[LOCALIZATION::FeatureSetting] Failed to get key for Game Mode [{0}]", this._feature.disabledText));
		}
		this._disabledTextStr = text;
		Debug.Log("[KIDUIFeatureSetting::Language] Refreshed disabled text: " + this._disabledTextStr);
		if (!LocalisationManager.TryGetTranslationForCurrentLocaleWithLocString(this._feature.featureName, out text, this._feature.permissionName, null))
		{
			Debug.LogError(string.Format("[LOCALIZATION::FeatureSetting] Failed to get key for Game Mode [{0}]", this._feature.disabledText));
		}
		this._featureName = text;
		Debug.Log("[KIDUIFeatureSetting::Language] Refreshed feature name text: " + this._featureName);
		this.SetFeatureName();
	}

	public void UnregisterOnToggleChangeEvent(Action action)
	{
		this._featureToggle.UnregisterOnChangeEvent(action);
	}

	public void RegisterToggleOnEvent(Action action)
	{
		this._featureToggle.RegisterToggleOnEvent(action);
	}

	public void UnregisterToggleOnEvent(Action action)
	{
		this._featureToggle.UnregisterToggleOnEvent(action);
	}

	public void RegisterToggleOffEvent(Action action)
	{
		this._featureToggle.RegisterToggleOffEvent(action);
	}

	public void UnregisterToggleOffEvent(Action action)
	{
		this._featureToggle.UnregisterToggleOffEvent(action);
	}

	public bool GetFeatureToggleState()
	{
		if (this._hasToggle)
		{
			return this._featureToggle.IsOn;
		}
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(this._featureType);
		if (permissionDataByFeature.ManagedBy != Permission.ManagedByEnum.GUARDIAN)
		{
			Debug.LogError("[KID::FeatureSetting] GetToggleState: feature has no toggle AND is not managed by Guardian");
		}
		return permissionDataByFeature.Enabled;
	}

	public bool GetHasToggle()
	{
		return this._hasToggle;
	}

	public void SetFeatureSettingVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
	}

	public void SetFeatureToggle(bool enableToggle)
	{
		this._featureToggle.interactable = enableToggle;
	}

	public void SetGuardianManagedState(bool isEnabled)
	{
		this._featureToggle.gameObject.SetActive(false);
		this._guardianManagedEnabled.SetActive(isEnabled);
		this._guardianManagedLocked.SetActive(!isEnabled);
		this.SetupGuardianManagedClickHandlers();
		this.SetFeatureName();
	}

	public void SetPlayerManagedState(bool isInteractable, bool isOptedIn)
	{
		this._featureToggle.gameObject.SetActive(true);
		this._guardianManagedEnabled.SetActive(false);
		this._guardianManagedLocked.SetActive(false);
		this._featureToggle.interactable = isInteractable;
		this._featureToggle.SetValue(isOptedIn);
	}

	private void SetFeatureName()
	{
		string text = (this.GetFeatureToggleState() ? ("<b>(" + this._enabledTextStr + ")</b>") : ("<b>(" + this._disabledTextStr + ")</b>"));
		this._featureNameTxt.text = "<b>" + this._featureName + "</b>";
		this._featureStatusTxt.text = text ?? "";
	}

	private void SetupGuardianManagedClickHandlers()
	{
		this.AddDeniedSoundHandler(this._guardianManagedEnabled);
		this.AddDeniedSoundHandler(this._guardianManagedLocked);
	}

	private void AddDeniedSoundHandler(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		EventTrigger component = obj.GetComponent<EventTrigger>();
		if (component != null)
		{
			Object.DestroyImmediate(component);
		}
		EventTrigger eventTrigger = obj.AddComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener(delegate(BaseEventData data)
		{
			Debug.Log("[KIDUIFeatureSetting] Guardian-managed feature clicked - playing denied sound");
			KIDAudioManager instance = KIDAudioManager.Instance;
			if (instance == null)
			{
				return;
			}
			instance.PlaySound(KIDAudioManager.KIDSoundType.Denied);
		});
		eventTrigger.triggers.Add(entry);
		this.EnsureRaycastTarget(obj);
	}

	private void EnsureRaycastTarget(GameObject obj)
	{
		Graphic component = obj.GetComponent<Graphic>();
		if (component != null)
		{
			component.raycastTarget = true;
			return;
		}
		Image image = obj.GetComponent<Image>();
		if (image == null)
		{
			image = obj.AddComponent<Image>();
		}
		image.color = new Color(0f, 0f, 0f, 0f);
		image.raycastTarget = true;
	}

	[SerializeField]
	private TMP_Text _featureNameTxt;

	[SerializeField]
	private TMP_Text _featureStatusTxt;

	[SerializeField]
	private KIDUIToggle _featureToggle;

	[SerializeField]
	private GameObject _tickIcon;

	[SerializeField]
	private GameObject _crossIcon;

	[SerializeField]
	private GameObject _guardianManagedLocked;

	[SerializeField]
	private GameObject _guardianManagedEnabled;

	private bool _hasToggle;

	private string _featureName;

	private string _permissionName;

	private string _enabledTextStr;

	private string _disabledTextStr;

	private EKIDFeatures _featureType;

	private Action<EKIDFeatures> _onChangeCallback;

	private KIDUI_MainScreen.FeatureToggleSetup _feature;
}
