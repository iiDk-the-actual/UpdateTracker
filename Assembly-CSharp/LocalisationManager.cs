using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LocalisationManager : MonoBehaviour
{
	public static LocalisationManager Instance
	{
		get
		{
			return LocalisationManager._instance;
		}
	}

	public static bool IsReady
	{
		get
		{
			return LocalisationManager.Instance != null && LocalisationManager._localeTablePairs.Count != 0;
		}
	}

	public static bool LanguageSet
	{
		get
		{
			return PlayerPrefs.GetInt("has-set-language", 0) == 1;
		}
	}

	public static Locale CurrentLanguage
	{
		get
		{
			return LocalizationSettings.SelectedLocale;
		}
	}

	private static string LanugageSetPlayerPrefKey
	{
		get
		{
			return "selected-locale";
		}
	}

	public static bool ApplicationRunning
	{
		get
		{
			return Application.isPlaying && !ApplicationQuittingState.IsQuitting;
		}
	}

	private void Awake()
	{
		if (LocalisationManager._instance != null)
		{
			Object.DestroyImmediate(this);
			return;
		}
		LocalisationManager._instance = this;
		Object.DontDestroyOnLoad(this);
		LocalisationManager._localisationFontDict.Clear();
		for (int i = 0; i < this._localisationFonts.Count; i++)
		{
			for (int j = 0; j < this._localisationFonts[i].locales.Count; j++)
			{
				if (!(this._localisationFonts[i].locales[j] == null) && !LocalisationManager._localisationFontDict.ContainsKey(this._localisationFonts[i].locales[j].Identifier.Code) && !(this._localisationFonts[i].fontAsset == null))
				{
					this._localisationFonts[i].fontAsset == null;
					LocalisationManager._localisationFontDict.Add(this._localisationFonts[i].locales[j].Identifier.Code, this._localisationFonts[i]);
					Debug.Log("[LOCALIZATION::MANAGER] Added new Locale-Font pair to Dictionary: [" + this._localisationFonts[i].locales[j].LocaleName + "]");
				}
			}
		}
		LocalisationManager._requestCancellationSource = new CancellationTokenSource();
	}

	private async void Start()
	{
		this.TryUpdateLanguage(LocalisationManager._initLocale, false);
		if (!LocalisationManager.LanguageSet)
		{
			HandRayController.Instance.EnableHandRays();
			PrivateUIRoom.AddUI(LocalisationUI.GetUITransform());
		}
	}

	private void OnDestroy()
	{
		LocalisationManager._requestCancellationSource.Cancel();
		LocalisationManager._onLanguageChanged = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void InitialiseLocTables()
	{
		CultureInfo.CurrentCulture = new CultureInfo("en");
		LocalisationManager.CacheLocTables();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitialiseLanguage()
	{
		LocalisationManager._hasInitialised = false;
		string @string = PlayerPrefs.GetString(LocalisationManager.LanugageSetPlayerPrefKey, "");
		Locale locale = null;
		if (!string.IsNullOrEmpty(@string) && LocalisationManager.LanguageSet)
		{
			LocalisationManager.LoadPreviousLanguage(@string, out locale);
		}
		else
		{
			LocalisationManager.DefaultLocaleFallback(out locale);
		}
		MothershipClientApiUnity.SetLanguage(locale.Identifier.Code);
		LocalisationManager._initLocale = locale;
		LocalisationManager._hasInitialised = true;
	}

	private static void CacheLocTables()
	{
		LocalisationManager._localeTablePairs.Clear();
		float time = Time.time;
		foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
		{
			AsyncOperationHandle<IList<StringTable>> allTables = LocalizationSettings.StringDatabase.GetAllTables(locale);
			allTables.WaitForCompletion();
			IList<StringTable> result = allTables.Result;
			if (result.Count != 0)
			{
				int count = result.Count;
				LocalisationManager._localeTablePairs.Add(locale.Identifier.Code, result[0]);
			}
		}
	}

	public void OnLanguageButtonPressed(string langCode, bool saveLanguage)
	{
		Locale locale;
		if (!LocalisationManager.TryGetLocaleFromCode(langCode, out locale))
		{
			return;
		}
		this.TryUpdateLanguage(locale, saveLanguage);
	}

	private void ReconstructBindings()
	{
		int num = 1;
		LocalisationManager._localeDisplayBinding.Clear();
		foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
		{
			LocalisationManager._localeDisplayBinding.Add(num, locale);
			num++;
		}
	}

	private static void LoadPreviousLanguage(string languageCode, out Locale result)
	{
		if (!LocalisationManager.TryGetLocaleFromCode(languageCode, out result))
		{
			LocalisationManager.DefaultLocaleFallback(out result);
			return;
		}
		PlayerPrefs.SetString(LocalisationManager.LanugageSetPlayerPrefKey, result.Identifier.Code);
		PlayerPrefs.SetInt("has-set-language", 1);
		PlayerPrefs.Save();
	}

	private static void DefaultLocaleFallback(out Locale result)
	{
		if (LocalisationManager.SysLangToLoc(Application.systemLanguage, out result))
		{
			PlayerPrefs.SetString(LocalisationManager.LanugageSetPlayerPrefKey, result.Identifier.Code);
			PlayerPrefs.SetInt("has-set-language", 1);
			PlayerPrefs.Save();
		}
	}

	private static bool SysLangToLoc(SystemLanguage sysLanguage, out Locale language)
	{
		if (sysLanguage <= SystemLanguage.French)
		{
			if (sysLanguage == SystemLanguage.English)
			{
				language = LocalizationSettings.Instance.GetAvailableLocales().GetLocale("en");
				return language != null;
			}
			if (sysLanguage == SystemLanguage.French)
			{
				language = LocalizationSettings.Instance.GetAvailableLocales().GetLocale("fr");
				return language != null;
			}
		}
		else
		{
			if (sysLanguage == SystemLanguage.German)
			{
				language = LocalizationSettings.Instance.GetAvailableLocales().GetLocale("de");
				return language != null;
			}
			if (sysLanguage == SystemLanguage.Spanish)
			{
				language = LocalizationSettings.Instance.GetAvailableLocales().GetLocale("es");
				return language != null;
			}
		}
		language = LocalizationSettings.Instance.GetAvailableLocales().GetLocale("en");
		language == null;
		return false;
	}

	private void TryUpdateLanguage(Locale newLocale, bool saveLanguage = true)
	{
		if (this._updateLangCoroutine != null)
		{
			base.StopCoroutine(this._updateLangCoroutine);
		}
		this._updateLangCoroutine = base.StartCoroutine(this.UpdateLanguage(newLocale, saveLanguage));
	}

	private IEnumerator UpdateLanguage(Locale newLocale, bool saveLanguage)
	{
		if (!this._cachedHasInitialised)
		{
			yield return LocalizationSettings.InitializationOperation;
		}
		this._cachedHasInitialised = true;
		if (LocalisationManager.CurrentLanguage.Identifier.Code == newLocale.Identifier.Code)
		{
			yield break;
		}
		TelemetryData telemetryData = new TelemetryData
		{
			EventName = "language_changed",
			CustomTags = new string[] { LocalizationTelemetry.GameVersionCustomTag },
			BodyData = new Dictionary<string, string>
			{
				{
					"starting_language",
					LocalisationManager.CurrentLanguage.Identifier.Code
				},
				{
					"new_language",
					newLocale.Identifier.Code
				}
			}
		};
		MothershipClientApiUnity.SetLanguage(newLocale.Identifier.Code);
		GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		LocalizationSettings.SelectedLocale = newLocale;
		UnityEvent languageEvent = GameEvents.LanguageEvent;
		if (languageEvent != null)
		{
			languageEvent.Invoke();
		}
		Action onLanguageChanged = LocalisationManager._onLanguageChanged;
		if (onLanguageChanged != null)
		{
			onLanguageChanged();
		}
		if (!saveLanguage)
		{
			yield break;
		}
		LocalisationManager.OnSaveLanguage();
		yield break;
	}

	public static bool TryGetLocaleFromCode(string code, out Locale result)
	{
		result = LocalizationSettings.AvailableLocales.GetLocale(code);
		return result != null;
	}

	public static void RegisterOnLanguageChanged(Action callback)
	{
		LocalisationManager._onLanguageChanged = (Action)Delegate.Combine(LocalisationManager._onLanguageChanged, callback);
	}

	public static void UnregisterOnLanguageChanged(Action callback)
	{
		LocalisationManager._onLanguageChanged = (Action)Delegate.Remove(LocalisationManager._onLanguageChanged, callback);
	}

	public static bool GetFontAssetForCurrentLocale(out LocalisationFontPair result)
	{
		result = default(LocalisationFontPair);
		if (LocalisationManager.Instance == null)
		{
			bool applicationRunning = LocalisationManager.ApplicationRunning;
			return false;
		}
		if (!LocalisationManager._localisationFontDict.ContainsKey(LocalisationManager.CurrentLanguage.Identifier.Code))
		{
			float time = Time.time;
			return false;
		}
		result = LocalisationManager._localisationFontDict[LocalisationManager.CurrentLanguage.Identifier.Code];
		return true;
	}

	public static void OnSaveLanguage()
	{
		PlayerPrefs.SetString(LocalisationManager.LanugageSetPlayerPrefKey, LocalisationManager.CurrentLanguage.Identifier.Code);
		PlayerPrefs.SetInt("has-set-language", 1);
		PlayerPrefs.Save();
	}

	public static bool TryGetLocaleBinding(int binding, out Locale loc)
	{
		loc = null;
		if (LocalisationManager.Instance == null)
		{
			return false;
		}
		if (LocalisationManager._localeDisplayBinding.Count != LocalizationSettings.AvailableLocales.Locales.Count)
		{
			LocalisationManager.Instance.ReconstructBindings();
		}
		return LocalisationManager._localeDisplayBinding.TryGetValue(binding, out loc);
	}

	public static Dictionary<int, Locale> GetAllBindings()
	{
		if (LocalisationManager._localeDisplayBinding.Count != LocalizationSettings.AvailableLocales.Locales.Count)
		{
			LocalisationManager.Instance.ReconstructBindings();
		}
		return LocalisationManager._localeDisplayBinding;
	}

	public static bool TryGetKeyForCurrentLocale(string key, out string result, string defaultResult = "")
	{
		result = defaultResult;
		if (LocalisationManager._localeTablePairs.Count == 0)
		{
			return false;
		}
		StringTable stringTable;
		if (!LocalisationManager._localeTablePairs.TryGetValue(LocalisationManager.CurrentLanguage.Identifier.Code, out stringTable))
		{
			return false;
		}
		TableEntry entry = stringTable.GetEntry(key);
		if (entry == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(entry.LocalizedValue))
		{
			result = defaultResult;
			return true;
		}
		result = entry.LocalizedValue;
		return true;
	}

	public static bool TryGetKeyForEnglishString(string englishString, out string result)
	{
		result = "";
		if (LocalisationManager._localeTablePairs.Count == 0)
		{
			return false;
		}
		StringTable stringTable;
		if (!LocalisationManager._localeTablePairs.TryGetValue("en", out stringTable))
		{
			return false;
		}
		foreach (StringTableEntry stringTableEntry in stringTable.Values)
		{
			if (!englishString.Contains(stringTableEntry.LocalizedValue))
			{
				result = stringTableEntry.LocalizedValue;
				return true;
			}
		}
		return false;
	}

	public static bool TryGetTranslationForCurrentLocaleWithLocString(LocalizedString key, out string result, string defaultResult = "", Object context = null)
	{
		result = defaultResult;
		key.TableReference;
		StringTable table = LocalizationSettings.StringDatabase.GetTable(key.TableReference, null);
		if (table == null)
		{
			return false;
		}
		TableEntry entryFromReference = table.GetEntryFromReference(key.TableEntryReference);
		if (entryFromReference == null)
		{
			return false;
		}
		result = entryFromReference.LocalizedValue;
		return true;
	}

	public static string LocaleToFriendlyString(Locale locale = null, bool forceEnglishChars = false)
	{
		if (locale == null)
		{
			locale = LocalisationManager.CurrentLanguage;
		}
		string code = locale.Identifier.Code;
		if (code == "en")
		{
			return "English";
		}
		if (code == "fr")
		{
			return "Français";
		}
		if (code == "de")
		{
			return "Deutsch";
		}
		if (code == "es")
		{
			return "Español";
		}
		if (!(code == "ja"))
		{
			return "English";
		}
		if (forceEnglishChars)
		{
			return "Nihongo";
		}
		return "日本語";
	}

	public static string LocaleDisplayNameToFriendlyString(string locTextName, bool forceEnglishChar = false)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(locTextName);
		if (num > 2645429922U)
		{
			if (num <= 3560075306U)
			{
				if (num != 3159852254U)
				{
					if (num != 3560075306U)
					{
						goto IL_0103;
					}
					if (!(locTextName == "JAPANESE"))
					{
						goto IL_0103;
					}
					goto IL_0121;
				}
				else if (!(locTextName == "ESPAÑOL"))
				{
					goto IL_0103;
				}
			}
			else if (num != 3567715190U)
			{
				if (num != 3825731007U)
				{
					if (num != 4169853379U)
					{
						goto IL_0103;
					}
					if (!(locTextName == "ESPANOL"))
					{
						goto IL_0103;
					}
				}
				else
				{
					if (!(locTextName == "NIHONGO"))
					{
						goto IL_0103;
					}
					goto IL_0121;
				}
			}
			else
			{
				if (!(locTextName == "FRANÇAIS"))
				{
					goto IL_0103;
				}
				goto IL_010F;
			}
			return "Español";
		}
		if (num <= 1409693518U)
		{
			if (num != 1157811451U)
			{
				if (num == 1409693518U)
				{
					if (locTextName == "日本語")
					{
						goto IL_0121;
					}
				}
			}
			else if (locTextName == "ENGLISH")
			{
				return "English";
			}
		}
		else if (num != 2572742563U)
		{
			if (num == 2645429922U)
			{
				if (locTextName == "FRANCAIS")
				{
					goto IL_010F;
				}
			}
		}
		else if (locTextName == "DEUTSCH")
		{
			return "Deutsch";
		}
		IL_0103:
		return "English";
		IL_010F:
		return "Français";
		IL_0121:
		if (forceEnglishChar)
		{
			return "Nihongo";
		}
		return "日本語";
	}

	public const string ENGLISH_IDENTIFIER = "en";

	public const string FRENCH_IDENTIFIER = "fr";

	public const string GERMAN_IDENTIFIER = "de";

	public const string ITALIAN_IDENTIFIER = "it";

	public const string SPANISH_IDENTIFIER = "es";

	public const string JAPENESE_IDENTIFIER = "ja";

	private static LocalisationManager _instance;

	[SerializeField]
	private List<LocalisationFontPair> _localisationFonts = new List<LocalisationFontPair>();

	private bool _cachedHasInitialised;

	private static bool _hasInitialised = false;

	private const string LANGUAGE_SET_PLAYER_PREF = "has-set-language";

	private const string LOC_SYSTEM_PLAYER_PREF = "selected-locale";

	private static Locale _initLocale;

	private static Action _onLanguageChanged;

	private Coroutine _updateLangCoroutine;

	private static CancellationTokenSource _requestCancellationSource;

	private static Dictionary<int, Locale> _localeDisplayBinding = new Dictionary<int, Locale>();

	private static Dictionary<string, StringTable> _localeTablePairs = new Dictionary<string, StringTable>();

	private static Dictionary<string, LocalisationFontPair> _localisationFontDict = new Dictionary<string, LocalisationFontPair>();
}
