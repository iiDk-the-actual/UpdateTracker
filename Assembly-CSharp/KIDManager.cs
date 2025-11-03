using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GorillaNetworking;
using KID.Model;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;

public class KIDManager : MonoBehaviour
{
	public static KIDManager Instance
	{
		get
		{
			return KIDManager._instance;
		}
	}

	public static bool InitialisationComplete { get; private set; } = false;

	public static bool InitialisationSuccessful { get; private set; } = false;

	public static TMPSession CurrentSession { get; private set; }

	public static SessionStatus PreviousStatus { get; private set; }

	public static GetRequirementsData _ageGateRequirements { get; private set; }

	public static bool KidTitleDataReady
	{
		get
		{
			return KIDManager._titleDataReady;
		}
	}

	public static bool KidEnabled
	{
		get
		{
			return KIDManager.KidTitleDataReady && KIDManager._useKid;
		}
	}

	public static bool KidEnabledAndReady
	{
		get
		{
			return KIDManager.KidEnabled && KIDManager.InitialisationSuccessful;
		}
	}

	public static bool HasSession
	{
		get
		{
			return KIDManager.CurrentSession != null && KIDManager.CurrentSession.SessionId != Guid.Empty;
		}
	}

	public static string PreviousStatusPlayerPrefRef
	{
		get
		{
			return "previous-status-" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
		}
	}

	public static bool HasOptedInToKID { get; private set; }

	private static string KIDSetupPlayerPref
	{
		get
		{
			return "KID-Setup-";
		}
	}

	public static string DbgLocale { get; set; }

	public static string DebugKIDLocalePlayerPrefRef
	{
		get
		{
			return KIDManager._debugKIDLocalePlayerPrefRef;
		}
	}

	public static string GetEmailForUserPlayerPrefRef
	{
		get
		{
			if (string.IsNullOrEmpty(KIDManager.parentEmailForUserPlayerPrefRef))
			{
				KIDManager.parentEmailForUserPlayerPrefRef = "k-id_EmailAddress" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
			}
			return KIDManager.parentEmailForUserPlayerPrefRef;
		}
	}

	public static string GetChallengedBeforePlayerPrefRef
	{
		get
		{
			return "k-id_ChallengedBefore" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
		}
	}

	private void Awake()
	{
		if (KIDManager._instance != null)
		{
			Debug.LogError("Trying to create new instance of [KIDManager], but one already exists. Destroying object [" + base.gameObject.name + "].");
			Object.Destroy(base.gameObject);
			return;
		}
		Debug.Log("[KID] INIT");
		KIDManager._instance = this;
		KIDManager.DbgLocale = PlayerPrefs.GetString(KIDManager._debugKIDLocalePlayerPrefRef, "");
	}

	private async void Start()
	{
		TaskAwaiter<bool> taskAwaiter = KIDManager.UseKID().GetAwaiter();
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			TaskAwaiter<bool> taskAwaiter2;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<bool>);
		}
		KIDManager._useKid = taskAwaiter.GetResult();
		TaskAwaiter<int> taskAwaiter3 = KIDManager.CheckKIDPhase().GetAwaiter();
		if (!taskAwaiter3.IsCompleted)
		{
			await taskAwaiter3;
			TaskAwaiter<int> taskAwaiter4;
			taskAwaiter3 = taskAwaiter4;
			taskAwaiter4 = default(TaskAwaiter<int>);
		}
		KIDManager._kIDPhase = taskAwaiter3.GetResult();
		TaskAwaiter<DateTime?> taskAwaiter5 = KIDManager.CheckKIDNewPlayerDateTime().GetAwaiter();
		if (!taskAwaiter5.IsCompleted)
		{
			await taskAwaiter5;
			TaskAwaiter<DateTime?> taskAwaiter6;
			taskAwaiter5 = taskAwaiter6;
			taskAwaiter6 = default(TaskAwaiter<DateTime?>);
		}
		KIDManager._kIDNewPlayerDateTime = taskAwaiter5.GetResult();
		KIDManager._titleDataReady = true;
	}

	private void OnDestroy()
	{
		KIDManager._requestCancellationSource.Cancel();
	}

	public static string GetActiveAccountStatusNiceString()
	{
		switch (KIDManager.GetActiveAccountStatus())
		{
		case AgeStatusType.DIGITALMINOR:
			return "Digital Minor";
		case AgeStatusType.DIGITALYOUTH:
			return "Digital Youth";
		case AgeStatusType.LEGALADULT:
			return "Legal Adult";
		default:
			return "UNKNOWN";
		}
	}

	public static AgeStatusType GetActiveAccountStatus()
	{
		if (KIDManager.CurrentSession != null)
		{
			return KIDManager.CurrentSession.AgeStatus;
		}
		if (!PlayFabAuthenticator.instance.GetSafety())
		{
			return AgeStatusType.LEGALADULT;
		}
		return AgeStatusType.DIGITALMINOR;
	}

	public static List<Permission> GetAllPermissionsData()
	{
		if (KIDManager.CurrentSession == null)
		{
			Debug.LogError("[KID::MANAGER] There is no current session. Unless the age-gate has not yet finished there should always be a session even if it is the default session");
			return new List<Permission>();
		}
		return KIDManager.CurrentSession.GetAllPermissions();
	}

	public static bool TryGetAgeStatusTypeFromAge(int age, out AgeStatusType ageType)
	{
		if (KIDManager._ageGateRequirements == null)
		{
			Debug.LogError("[KID::MANAGER] [_ageGateRequirements] is not set - need to Get AgeGate Requirements first");
			ageType = AgeStatusType.DIGITALMINOR;
			return false;
		}
		if (age < KIDManager._ageGateRequirements.AgeGateRequirements.DigitalConsentAge)
		{
			ageType = AgeStatusType.DIGITALMINOR;
			return true;
		}
		if (age < KIDManager._ageGateRequirements.AgeGateRequirements.CivilAge)
		{
			ageType = AgeStatusType.DIGITALYOUTH;
			return true;
		}
		ageType = AgeStatusType.LEGALADULT;
		return true;
	}

	[return: TupleElementNames(new string[] { "requiresOptIn", "hasOptedInPreviously" })]
	public static ValueTuple<bool, bool> CheckFeatureOptIn(EKIDFeatures feature, Permission permissionData = null)
	{
		if (permissionData == null)
		{
			permissionData = KIDManager.GetPermissionDataByFeature(feature);
			if (permissionData == null)
			{
				Debug.LogError("[KID::MANAGER] Unable to retrieve permission data for feature [" + feature.ToStandardisedString() + "]");
				return new ValueTuple<bool, bool>(false, false);
			}
		}
		if (permissionData.ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		bool flag = true;
		if (KIDManager.CurrentSession != null)
		{
			flag = KIDManager.CurrentSession.HasOptedInToPermission(feature);
		}
		if (permissionData.ManagedBy == Permission.ManagedByEnum.GUARDIAN)
		{
			return new ValueTuple<bool, bool>(false, flag);
		}
		if (permissionData.ManagedBy == Permission.ManagedByEnum.PLAYER && permissionData.Enabled)
		{
			return new ValueTuple<bool, bool>(false, true);
		}
		return new ValueTuple<bool, bool>(true, flag);
	}

	public static void SetFeatureOptIn(EKIDFeatures feature, bool optedIn)
	{
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(feature);
		if (permissionDataByFeature == null)
		{
			Debug.LogErrorFormat("[KID] Trying to set Feature Opt in for feature [" + feature.ToStandardisedString() + "] but permission data could not be found. Assumed is opt-in", Array.Empty<object>());
			return;
		}
		if (KIDManager.CurrentSession == null)
		{
			Debug.Log("[KID::MANAGER] CurrentSession is null, cannot set feature opt-in. Returning.");
			return;
		}
		switch (permissionDataByFeature.ManagedBy)
		{
		case Permission.ManagedByEnum.PLAYER:
			KIDManager.CurrentSession.OptInToPermission(feature, optedIn);
			return;
		case Permission.ManagedByEnum.GUARDIAN:
			KIDManager.CurrentSession.OptInToPermission(feature, permissionDataByFeature.Enabled);
			return;
		case Permission.ManagedByEnum.PROHIBITED:
			KIDManager.CurrentSession.OptInToPermission(feature, false);
			return;
		default:
			return;
		}
	}

	public static bool CheckFeatureSettingEnabled(EKIDFeatures feature)
	{
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(feature);
		if (permissionDataByFeature == null)
		{
			Debug.LogError("[KID::MANAGER] Unable to permissions for feature [" + feature.ToStandardisedString() + "]");
			return false;
		}
		if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			return false;
		}
		bool item = KIDManager.CheckFeatureOptIn(feature, null).Item2;
		switch (feature)
		{
		case EKIDFeatures.Multiplayer:
		case EKIDFeatures.Mods:
			return item;
		case EKIDFeatures.Custom_Nametags:
			return item && GorillaComputer.instance.NametagsEnabled;
		case EKIDFeatures.Voice_Chat:
			return item && GorillaComputer.instance.CheckVoiceChatEnabled();
		case EKIDFeatures.Groups:
			return permissionDataByFeature.ManagedBy != Permission.ManagedByEnum.GUARDIAN || permissionDataByFeature.Enabled;
		default:
			Debug.LogError("[KID::MANAGER] Tried finding feature setting for [" + feature.ToStandardisedString() + "] but failed.");
			return false;
		}
	}

	private static async Task<GetPlayerData_Data> TryGetPlayerData(bool forceRefresh)
	{
		return await KIDManager.Server_GetPlayerData(forceRefresh, null);
	}

	private static async Task<GetRequirementsData> TryGetRequirements()
	{
		return await KIDManager.Server_GetRequirements();
	}

	private static async Task<VerifyAgeData> TryVerifyAgeResponse()
	{
		PlayerPlatform playerPlatform = PlayerPlatform.Steam;
		VerifyAgeRequest verifyAgeRequest = new VerifyAgeRequest();
		verifyAgeRequest.Age = new int?(KIDAgeGate.UserAge);
		verifyAgeRequest.Platform = new PlayerPlatform?(playerPlatform);
		Debug.Log(string.Format("[KID::MANAGER] Sending verify age request for age: [{0}]", KIDAgeGate.UserAge));
		return await KIDManager.Server_VerifyAge(verifyAgeRequest, null);
	}

	[return: TupleElementNames(new string[] { "success", "exception" })]
	private static async Task<ValueTuple<bool, string>> TrySendChallengeEmailRequest()
	{
		do
		{
			await Task.Yield();
		}
		while (string.IsNullOrEmpty(KIDManager._emailAddress));
		ValueTuple<bool, string> valueTuple = await KIDManager.Server_SendChallengeEmail(new SendChallengeEmailRequest
		{
			Email = KIDManager._emailAddress,
			Locale = (string.IsNullOrEmpty(KIDManager.DbgLocale) ? CultureInfo.CurrentCulture.Name : KIDManager.DbgLocale)
		});
		bool item = valueTuple.Item1;
		string item2 = valueTuple.Item2;
		if (item)
		{
			KIDManager.OnEmailResultReceived onEmailResultReceived = KIDManager.onEmailResultReceived;
			if (onEmailResultReceived != null)
			{
				onEmailResultReceived(true);
			}
		}
		else
		{
			KIDManager.OnEmailResultReceived onEmailResultReceived2 = KIDManager.onEmailResultReceived;
			if (onEmailResultReceived2 != null)
			{
				onEmailResultReceived2(false);
			}
		}
		return new ValueTuple<bool, string>(item, item2);
	}

	private static async Task<bool> TrySendOptInPermissions()
	{
		string[] optedInPermissions = KIDManager.CurrentSession.GetOptedInPermissions();
		bool flag;
		if (optedInPermissions == null)
		{
			Debug.LogError("[KID::MANAGER::OptInRefactor] Tried to set opt-in permissions but no permissions were provided");
			flag = false;
		}
		else
		{
			Debug.Log("[KID::MANAGER::OptInRefactor] Setting Opt-in Permissions: " + string.Join(", ", optedInPermissions));
			flag = await KIDManager.Server_SetOptInPermissions(new SetOptInPermissionsRequest
			{
				OptInPermissions = optedInPermissions
			}, null);
		}
		return flag;
	}

	public static async Task<ValueTuple<bool, string>> TrySendUpgradeSessionChallengeEmail()
	{
		ValueTuple<bool, string> valueTuple = await KIDManager.Server_SendChallengeEmail(new SendChallengeEmailRequest());
		bool item = valueTuple.Item1;
		string item2 = valueTuple.Item2;
		return new ValueTuple<bool, string>(item, item2);
	}

	public static async Task<bool> TrySetHasConfirmedStatus()
	{
		return await KIDManager.Server_SetConfirmedStatus();
	}

	public static async Task<UpgradeSessionData> TryUpgradeSession(List<string> requestedPermissions)
	{
		global::UpgradeSessionRequest upgradeSessionRequest = new global::UpgradeSessionRequest();
		upgradeSessionRequest.Permissions = requestedPermissions.Select((string name) => new RequestedPermission(name)).ToList<RequestedPermission>();
		UpgradeSessionData upgradeSessionData = await KIDManager.Server_UpgradeSession(upgradeSessionRequest);
		UpgradeSessionData upgradeSessionData2;
		if (upgradeSessionData == null)
		{
			Debug.LogError("[KID::MANAGER] Failed to upgrade session. Data is null.");
			upgradeSessionData2 = null;
		}
		else
		{
			KIDManager.UpdatePermissions(upgradeSessionData.session);
			upgradeSessionData2 = upgradeSessionData;
		}
		return upgradeSessionData2;
	}

	public static async Task<AttemptAgeUpdateData> TryAttemptAgeUpdate(int age)
	{
		PlayerPlatform playerPlatform = PlayerPlatform.Steam;
		AttemptAgeUpdateRequest attemptAgeUpdateRequest = new AttemptAgeUpdateRequest();
		attemptAgeUpdateRequest.Age = age;
		attemptAgeUpdateRequest.Platform = playerPlatform;
		Debug.Log(string.Format("[KID::MANAGER] Sending age update request for age: [{0}]", age));
		return await KIDManager.Server_AttemptAgeUpdate(attemptAgeUpdateRequest, null);
	}

	public static async Task<bool> TryAppealAge(string email, int newAge)
	{
		string text = (string.IsNullOrEmpty(KIDManager.DbgLocale) ? CultureInfo.CurrentCulture.Name : KIDManager.DbgLocale);
		AppealAgeRequest appealAgeRequest = new AppealAgeRequest();
		appealAgeRequest.Age = newAge;
		appealAgeRequest.Email = email;
		appealAgeRequest.Locale = text;
		Debug.Log(string.Format("[KID::MANAGER] Sending age appeal request for age: [{0}] at email [{1}]", newAge, email));
		return await KIDManager.Server_AppealAge(appealAgeRequest, null);
	}

	public static async Task UpdateSession(Action<bool> getDataCompleted = null)
	{
		GetPlayerData_Data getPlayerData_Data = await KIDManager.TryGetPlayerData(true);
		if (getPlayerData_Data == null)
		{
			if (getDataCompleted != null)
			{
				getDataCompleted(false);
			}
			Debug.LogError("[KID::MANAGER] Failed to retrieve session");
		}
		else if (getPlayerData_Data.responseType == GetSessionResponseType.ERROR)
		{
			if (getDataCompleted != null)
			{
				getDataCompleted(false);
			}
			Debug.LogError("[KID::MANAGER] Failed to get session. Resulted in error. Cannot update session");
		}
		else
		{
			if (getDataCompleted != null)
			{
				getDataCompleted(true);
			}
			KIDManager.UpdatePermissions(getPlayerData_Data.session);
		}
	}

	private static async Task<bool> CheckWarningScreensOptedIn()
	{
		bool flag;
		if (GorillaServer.Instance.CheckOptedInKID())
		{
			Debug.Log("[KID::MANAGER] PHASE ONE (A) -- IN PROGRESS - User has already opted in to k-ID, skipping warning screens");
			flag = true;
		}
		else
		{
			Debug.Log("[KID::MANAGER] CHECK WARNING SCREENS - Force Starting Overlay");
			PrivateUIRoom.ForceStartOverlay();
			WarningButtonResult warningButtonResult = await WarningScreens.StartWarningScreen(KIDManager._requestCancellationSource.Token);
			if (warningButtonResult == WarningButtonResult.None)
			{
				if (KIDManager._requestCancellationSource.IsCancellationRequested)
				{
					flag = false;
				}
				else
				{
					Debug.Log("[KID::MANAGER] PHASE ONE (A) -- IN PROGRESS - User not shown any warning screen and has not opted in yet. Is Eligible: [" + (GorillaServer.Instance.CheckIsInKIDOptInCohort() | GorillaServer.Instance.CheckIsInKIDRequiredCohort()).ToString() + "].");
					flag = false;
				}
			}
			else if (warningButtonResult == WarningButtonResult.CloseWarning)
			{
				if (KIDManager._requestCancellationSource.IsCancellationRequested)
				{
					flag = false;
				}
				else
				{
					Debug.Log("[KID::MANAGER] PHASE ONE (A) -- IN PROGRESS - User cancelled the warning screen. Skipping k-ID Opt-in.");
					flag = false;
				}
			}
			else
			{
				if (warningButtonResult == WarningButtonResult.OptIn)
				{
					Debug.Log("[KID::MANAGER] PHASE ONE (A) -- IN PROGRESS - User has newly opted in to k-ID");
					TaskAwaiter<bool> taskAwaiter = KIDManager.Server_OptIn().GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<bool> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult())
					{
						Debug.LogError("[KID::MANAGER] PHASE ONE (A) -- FAILURE - Opting in to k-ID failed!");
						return false;
					}
					if (CosmeticsController.instance != null)
					{
						CosmeticsController.instance.GetCurrencyBalance();
					}
					await WarningScreens.StartOptInFollowUpScreen(KIDManager._requestCancellationSource.Token);
				}
				flag = true;
			}
		}
		return flag;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitialiseBootFlow()
	{
		Debug.Log("[KID::MANAGER] PHASE ZERO -- START -- Checking K-ID Flag");
		if (PlayerPrefs.GetInt(KIDManager.KIDSetupPlayerPref, 0) != 0)
		{
			return;
		}
		Debug.Log("[KID::MANAGER] INITIALISE BOOT FLOW - Force Starting Overlay");
		PrivateUIRoom.ForceStartOverlay();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static async void InitialiseKID()
	{
		bool snapTurnDisabled = false;
		float? cachedTapHapticsStrength = null;
		object obj = null;
		int num = 0;
		try
		{
			Debug.Log("[KID::MANAGER] PHASE ZERO -- START - Initialising k-ID System - " + Application.version);
			TaskAwaiter<bool> taskAwaiter = KIDManager.WaitForAuthentication().GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<bool> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			bool flag = !taskAwaiter.GetResult();
			UGCPermissionManager.UsePlayFabSafety();
			if (flag)
			{
				Debug.Log("[KID::MANAGER] Wait for auth failed. Skipping age gate.");
			}
			else if (!KIDManager._useKid)
			{
				Debug.Log("[KID::MANAGER] Kid disabled. Skipping age gate.");
			}
			else
			{
				Debug.Log("[KID::MANAGER] PlayFab has logged in, starting k-ID initialisation flow");
				Debug.Log("[KID::MANAGER] PHASE ZERO -- COMPLETE");
				GorillaSnapTurn.DisableSnapTurn();
				snapTurnDisabled = true;
				if (GorillaTagger.Instance != null)
				{
					cachedTapHapticsStrength = new float?(GorillaTagger.Instance.tapHapticStrength);
					GorillaTagger.Instance.tapHapticStrength = 0f;
				}
				Debug.Log("[KID::MANAGER] PHASE ONE -- START - Initialising k-ID System");
				GetPlayerData_Data newSessionData = await KIDManager.TryGetPlayerData(true);
				if (!KIDManager._requestCancellationSource.IsCancellationRequested)
				{
					if (newSessionData == null)
					{
						Debug.LogError("[KID::MANAGER] [newSessionData] returned NULL. Something went wrong, we should always get a [GetPlayerData_Data]. Disabling k-ID");
					}
					else if (newSessionData.responseType == GetSessionResponseType.ERROR)
					{
						Debug.LogError("[KID::MANAGER] Failed to retrieve Player Data, response type: [" + newSessionData.responseType.ToString() + "]. Unable to proceed. Will default to Using Safeties");
						Debug.Log(string.Format("[KID::MANAGER] Safeties is: [{0}", PlayFabAuthenticator.instance.GetSafety()));
					}
					else
					{
						KIDManager.HasOptedInToKID = newSessionData.responseType != GetSessionResponseType.NOT_FOUND;
						bool flag2 = await KIDManager.CheckWarningScreensOptedIn();
						if (!KIDManager._requestCancellationSource.IsCancellationRequested)
						{
							if (!flag2)
							{
								Debug.Log("[KID] Kid Not opted into. Aborting k-ID setup.");
							}
							else
							{
								Debug.Log("[KID::MANAGER] PHASE ONE -- COMPLETE");
								Debug.Log("[KID::MANAGER] PHASE TWO -- START - Get Player Data from Server");
								KIDManager.PreviousStatus = (SessionStatus)PlayerPrefs.GetInt(KIDManager.PreviousStatusPlayerPrefRef, 0);
								TMPSession newSession = newSessionData.session;
								TMPSession session = newSessionData.session;
								if (session != null)
								{
									AgeStatusType ageStatus = session.AgeStatus;
								}
								Debug.Log("[KID::MANAGER] PHASE TWO -- IN PROGRESS - Getting Age-Gate Configuration Data");
								KIDManager._ageGateRequirements = await KIDManager.TryGetRequirements();
								KIDAgeGate.SetAgeGateConfig(KIDManager._ageGateRequirements);
								string text = ((KIDManager._ageGateRequirements == null) ? "UNSUCCESSFULLY [_ageGateRequirements] is null" : ((KIDManager._ageGateRequirements.AgeGateRequirements == null) ? "UNSUCCESSFULLY [AgeGateRequirements] is NULL" : "SUCCESSFULLY"));
								Debug.Log("[KID::MANAGER] PHASE TWO -- IN PROGRESS - Age-Gate configuration Completed: " + text);
								SessionStatus? sessionStatus = newSessionData.status;
								SessionStatus sessionStatus2 = SessionStatus.PROHIBITED;
								if (!((sessionStatus.GetValueOrDefault() == sessionStatus2) & (sessionStatus != null)))
								{
									sessionStatus = newSessionData.status;
									sessionStatus2 = SessionStatus.PENDING_AGE_APPEAL;
									if (!((sessionStatus.GetValueOrDefault() == sessionStatus2) & (sessionStatus != null)))
									{
										Debug.Log("[KID::MANAGER] PHASE TWO -- COMPLETE");
										Debug.Log("[KID::MANAGER] PHASE THREE -- START - Check for Age-Gate");
										TMPSession session2 = newSessionData.session;
										bool flag3;
										if (session2 == null)
										{
											flag3 = true;
										}
										else
										{
											AgeStatusType ageStatus2 = session2.AgeStatus;
											flag3 = false;
										}
										if (flag3)
										{
											PrivateUIRoom.ForceStartOverlay();
											Debug.Log("[KID::MANAGER] PHASE THREE -- IN PROGRESS - Age-gate required");
											ValueTuple<AgeStatusType, TMPSession> valueTuple = await KIDManager.AgeGateFlow(newSessionData);
											AgeStatusType item = valueTuple.Item1;
											TMPSession item2 = valueTuple.Item2;
											if (KIDManager._requestCancellationSource.IsCancellationRequested)
											{
												goto IL_0885;
											}
											newSession = item2;
										}
										Debug.Log("[KID::MANAGER] PHASE THREE -- COMPLETE");
										Debug.Log("[KID::MANAGER] PHASE FOUR -- START - Legal Agreements Processes");
										if (LegalAgreements.instance != null)
										{
											Debug.Log("[KID::MANAGER] Start legal agreements");
											await LegalAgreements.instance.StartLegalAgreements();
											if (KIDManager._requestCancellationSource.IsCancellationRequested)
											{
												goto IL_0885;
											}
										}
										Debug.Log("[KID::MANAGER] PHASE FOUR -- COMPLETE");
										Debug.Log("[KID::MANAGER] PHASE FIVE -- START - Update Permissions");
										if (!KIDManager.UpdatePermissions(newSession))
										{
											string text2 = "[KID::MANAGER] PHASE FIVE -- FAILURE - Failed to update permissions but will continue.\nSession was:\n";
											TMPSession tmpsession = newSession;
											Debug.LogError(text2 + ((tmpsession != null) ? tmpsession.ToString() : null));
											Debug.Log(string.Format("[KID::MANAGER] Safeties is: [{0}", PlayFabAuthenticator.instance.GetSafety()));
											goto IL_0885;
										}
										if (KIDManager.CurrentSession == null)
										{
											Debug.LogError("[KID::MANAGER] PHASE FIVE -- FAILURE -- CurrentSession is NULL, should at least have a default session!");
											Debug.Log(string.Format("[KID::MANAGER] Safeties is: [{0}", PlayFabAuthenticator.instance.GetSafety()));
											goto IL_0885;
										}
										if (KIDManager.CurrentSession.IsDefault)
										{
											KIDManager.WaitForAndUpdateNewSession(true);
										}
										if (!KIDManager._requestCancellationSource.IsCancellationRequested)
										{
											UGCPermissionManager.UseKID();
											Debug.Log("[KID::MANAGER] PHASE FIVE -- COMPLETE");
											Debug.Log("[KID::MANAGER] PHASE SIX -- START - Check for K-ID Screens");
											await KIDUI_Controller.Instance.StartKIDScreens(KIDManager._requestCancellationSource.Token);
											while (!KIDManager._requestCancellationSource.IsCancellationRequested)
											{
												await Task.Yield();
												if (!KIDUI_Controller.IsKIDUIActive)
												{
													Debug.Log("[KID::MANAGER] PHASE SIX --  COMPLETE");
													if (KIDManager._requestCancellationSource.IsCancellationRequested)
													{
														break;
													}
													Debug.Log("[KID::MANAGER] PHASE SEVEN -- START - Finalise setup");
													if (KIDManager.CurrentSession == null)
													{
														Debug.LogError("[KID::MANAGER] PHASE SEVEN -- FAILURE -- CurrentSession is NULL, should at least have a default session!");
														Debug.Log(string.Format("[KID::MANAGER] Safeties is: [{0}", PlayFabAuthenticator.instance.GetSafety()));
														break;
													}
													if (!newSessionData.HasConfirmedSetup)
													{
														await KIDMessagingController.StartKIDConfirmationScreen(KIDManager._requestCancellationSource.Token);
													}
													PlayerPrefs.SetInt(KIDManager.PreviousStatusPlayerPrefRef, (int)KIDManager.PreviousStatus);
													PlayerPrefs.Save();
													KIDManager.InitialisationSuccessful = true;
													newSessionData = null;
													newSession = null;
													goto IL_089A;
												}
											}
											goto IL_0885;
										}
										goto IL_0885;
									}
								}
								PrivateUIRoom.ForceStartOverlay();
								Debug.Log("[KID::MANAGER] User is [" + newSessionData.status.ToString() + "] from playing Gorilla Tag. Skipping to Age-Appeal flow");
								KIDUI_AgeAppealController.Instance.StartAgeAppealScreens(newSessionData.status);
							}
						}
					}
				}
			}
			IL_0885:
			num = 1;
		}
		catch (object obj)
		{
		}
		IL_089A:
		KIDManager.InitialisationComplete = true;
		if (!KIDManager.InitialisationSuccessful)
		{
			Debug.Log("[KID::MANAGER] k-ID Initialisation has FAILED.");
			if (cachedTapHapticsStrength != null)
			{
				Debug.Log("[KID::MANAGER] Enable back haptics when we're done with the k-ID setup");
				GorillaTagger.Instance.tapHapticStrength = cachedTapHapticsStrength.Value;
			}
			if (snapTurnDisabled)
			{
				Debug.Log("[KID::MANAGER] Reverting Snap Turning to PlayerPref settings");
				GorillaSnapTurn.LoadSettingsFromCache();
			}
			if (LegalAgreements.instance != null)
			{
				Debug.Log("[KID::MANAGER] Start legal agreements");
				await LegalAgreements.instance.StartLegalAgreements();
			}
			Debug.Log("[KID::MANAGER] Stop forced overlay");
			PrivateUIRoom.StopForcedOverlay();
		}
		object obj2 = obj;
		if (obj2 != null)
		{
			Exception ex = obj2 as Exception;
			if (ex == null)
			{
				throw obj2;
			}
			ExceptionDispatchInfo.Capture(ex).Throw();
		}
		if (num != 1)
		{
			obj = null;
			UGCPermissionManager.UseKID();
			bool flag4 = KIDManager.CurrentSession == null && PlayFabAuthenticator.instance.GetSafety();
			Debug.Log(string.Format("[KID::MANAGER] Safeties enabled status: [{0}", flag4));
			if (cachedTapHapticsStrength != null)
			{
				Debug.Log("[KID::MANAGER] Enable back haptics when we're done with the k-ID setup");
				GorillaTagger.Instance.tapHapticStrength = cachedTapHapticsStrength.Value;
			}
			if (snapTurnDisabled)
			{
				Debug.Log("[KID::MANAGER] Reverting Snap Turning to PlayerPref settings");
				GorillaSnapTurn.LoadSettingsFromCache();
			}
			Debug.Log("[KID::MANAGER] Stop forced overlay");
			PrivateUIRoom.StopForcedOverlay();
			Debug.Log("[KID::MANAGER] PHASE SEVEN -- COMPLETE");
			Debug.Log("[KID::MANAGER] K-ID Has been Initialised and is ready!");
		}
	}

	private static bool UpdatePermissions(TMPSession newSession)
	{
		Debug.Log("[KID::MANAGER] Updating Permissions to reflect session.");
		if (newSession == null || !newSession.IsValidSession)
		{
			Debug.LogError("[KID::MANAGER] A NULL or Invalid Session was received!");
			return false;
		}
		KIDManager.CurrentSession = newSession;
		if (KIDUI_Controller.IsKIDUIActive)
		{
			KIDManager.PreviousStatus = KIDManager.CurrentSession.SessionStatus;
			PlayerPrefs.SetInt(KIDManager.PreviousStatusPlayerPrefRef, (int)KIDManager.PreviousStatus);
			PlayerPrefs.Save();
		}
		if (!KIDManager.CurrentSession.IsDefault)
		{
			PlayerPrefs.SetInt(KIDManager.KIDSetupPlayerPref, 1);
			PlayerPrefs.Save();
		}
		KIDManager.OnSessionUpdated();
		if (KIDUI_Controller.Instance)
		{
			KIDUI_Controller.Instance.UpdateScreenStatus();
		}
		return true;
	}

	private static void ClearSession()
	{
		KIDManager.CurrentSession = null;
		KIDManager.DeleteStoredPermissions();
	}

	private static void DeleteStoredPermissions()
	{
	}

	public static CancellationTokenSource ResetCancellationToken()
	{
		KIDManager._requestCancellationSource.Dispose();
		KIDManager._requestCancellationSource = new CancellationTokenSource();
		return KIDManager._requestCancellationSource;
	}

	public static Permission GetPermissionDataByFeature(EKIDFeatures feature)
	{
		if (KIDManager.CurrentSession == null)
		{
			if (!PlayFabAuthenticator.instance.GetSafety())
			{
				return new Permission(feature.ToStandardisedString(), true, Permission.ManagedByEnum.PLAYER);
			}
			return new Permission(feature.ToStandardisedString(), false, Permission.ManagedByEnum.GUARDIAN);
		}
		else
		{
			Permission permission;
			if (!KIDManager.CurrentSession.TryGetPermission(feature, out permission))
			{
				Debug.LogError("[KID::MANAGER] Failed to retreive permission from session for [" + feature.ToStandardisedString() + "]. Assuming disabled permission");
				return new Permission(feature.ToStandardisedString(), false, Permission.ManagedByEnum.GUARDIAN);
			}
			return permission;
		}
	}

	public static void CancelToken()
	{
		KIDManager._requestCancellationSource.Cancel();
	}

	public static async Task<bool> UseKID()
	{
		bool flag;
		if (KIDManager._titleDataReady)
		{
			Debug.Log(string.Format("[KID::MANAGER] K-ID Title Data already retrieved, returning _useKid = [{0}]", KIDManager._useKid));
			flag = KIDManager._useKid;
		}
		else
		{
			int state = 0;
			bool isEnabled = false;
			PlayFabTitleDataCache.Instance.GetTitleData("KIDData", delegate(string res)
			{
				state = 1;
				isEnabled = KIDManager.GetIsEnabled(res);
				Debug.Log(string.Format("[KID::MANAGER::UseKID] K-ID Enabled status retrieved from Title Data: [{0}]", isEnabled));
			}, delegate(PlayFabError err)
			{
				state = -1;
				Debug.LogError("[KID_MANAGER::UseKID] Something went wrong trying to get title data for key: [KIDData]. Error:\n" + err.ErrorMessage);
			}, false);
			do
			{
				await Task.Yield();
			}
			while (state == 0);
			if (PlayFabAuthenticator.instance.postAuthSetSafety & isEnabled)
			{
				PlayFabAuthenticator.instance.DefaultSafetiesByAgeCategory();
			}
			flag = isEnabled;
		}
		return flag;
	}

	public static async Task<int> CheckKIDPhase()
	{
		int num;
		if (KIDManager._titleDataReady)
		{
			num = KIDManager._kIDPhase;
		}
		else
		{
			int state = 0;
			int phase = 0;
			PlayFabTitleDataCache.Instance.GetTitleData("KIDData", delegate(string res)
			{
				state = 1;
				phase = KIDManager.GetPhase(res);
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
			num = phase;
		}
		return num;
	}

	public static async Task<DateTime?> CheckKIDNewPlayerDateTime()
	{
		DateTime? dateTime;
		if (KIDManager._titleDataReady)
		{
			dateTime = KIDManager._kIDNewPlayerDateTime;
		}
		else
		{
			int state = 0;
			DateTime? newPlayerDateTime = null;
			PlayFabTitleDataCache.Instance.GetTitleData("KIDData", delegate(string res)
			{
				state = 1;
				newPlayerDateTime = KIDManager.GetNewPlayerDateTime(res);
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
			dateTime = newPlayerDateTime;
		}
		return dateTime;
	}

	private static bool GetIsEnabled(string jsonTxt)
	{
		KIDTitleData kidtitleData = JsonConvert.DeserializeObject<KIDTitleData>(jsonTxt);
		if (kidtitleData == null)
		{
			Debug.LogError("[KID_MANAGER] Failed to parse json to [KIDTitleData]. Json: \n" + jsonTxt);
			return false;
		}
		bool flag;
		if (!bool.TryParse(kidtitleData.KIDEnabled, out flag))
		{
			Debug.LogError("[KID_MANAGER] Failed to parse 'KIDEnabled': [KIDEnabled] to bool.");
			return false;
		}
		return flag;
	}

	private static int GetPhase(string jsonTxt)
	{
		KIDTitleData kidtitleData = JsonConvert.DeserializeObject<KIDTitleData>(jsonTxt);
		if (kidtitleData == null)
		{
			Debug.LogError("[KID_MANAGER] Failed to parse json to [KIDTitleData]. Json: \n" + jsonTxt);
			return 0;
		}
		return kidtitleData.KIDPhase;
	}

	private static DateTime? GetNewPlayerDateTime(string jsonTxt)
	{
		KIDTitleData kidtitleData = JsonConvert.DeserializeObject<KIDTitleData>(jsonTxt);
		if (kidtitleData == null)
		{
			Debug.LogError("[KID_MANAGER] Failed to parse json to [KIDTitleData]. Json: \n" + jsonTxt);
			return null;
		}
		DateTime dateTime;
		if (!DateTime.TryParse(kidtitleData.KIDNewPlayerIsoTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime))
		{
			Debug.LogError("[KID_MANAGER] Failed to parse 'KIDNewPlayerIsoTimestamp': [KIDNewPlayerIsoTimestamp] to DateTime.");
			return null;
		}
		return new DateTime?(dateTime);
	}

	public static bool IsAdult()
	{
		return KIDManager.CurrentSession.IsValidSession && KIDManager.CurrentSession.AgeStatus == AgeStatusType.LEGALADULT;
	}

	public static bool HasAllPermissions()
	{
		List<Permission> allPermissions = KIDManager.CurrentSession.GetAllPermissions();
		for (int i = 0; i < allPermissions.Count; i++)
		{
			if (allPermissions[i].ManagedBy == Permission.ManagedByEnum.GUARDIAN || !allPermissions[i].Enabled)
			{
				return false;
			}
		}
		return true;
	}

	public static async Task<bool> SetKIDOptIn()
	{
		return await KIDManager.Server_OptIn();
	}

	[return: TupleElementNames(new string[] { "success", "message" })]
	public static async Task<ValueTuple<bool, string>> SetAndSendEmail(string email)
	{
		KIDManager._emailAddress = email;
		return await KIDManager.TrySendChallengeEmailRequest();
	}

	public static async Task<bool> SendOptInPermissions()
	{
		return await KIDManager.TrySendOptInPermissions();
	}

	public static bool HasPermissionToUseFeature(EKIDFeatures feature)
	{
		if (!KIDManager.KidEnabledAndReady)
		{
			return !PlayFabAuthenticator.instance.GetSafety();
		}
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(feature);
		return (permissionDataByFeature.Enabled || permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PLAYER) && permissionDataByFeature.ManagedBy != Permission.ManagedByEnum.PROHIBITED;
	}

	private static async Task<bool> WaitForAuthentication()
	{
		Debug.Log("[KID] Starting Age-Gate process.");
		while (!PlayFabClientAPI.IsClientLoggedIn())
		{
			bool flag;
			if (KIDManager._requestCancellationSource.IsCancellationRequested)
			{
				flag = false;
			}
			else
			{
				if (!PlayFabAuthenticator.instance || !PlayFabAuthenticator.instance.loginFailed)
				{
					await Task.Yield();
					continue;
				}
				flag = false;
			}
			return flag;
		}
		Debug.Log("[KID] Initialisation - PlayFab has signed in. Continuing.");
		while (!GorillaServer.Instance.FeatureFlagsReady)
		{
			if (KIDManager._requestCancellationSource.IsCancellationRequested)
			{
				return false;
			}
			await Task.Yield();
		}
		Debug.Log("[KID] Initialisation - Feature Flags ready. Continuing.");
		while (!KIDManager._titleDataReady)
		{
			if (KIDManager._requestCancellationSource.IsCancellationRequested)
			{
				return false;
			}
			await Task.Yield();
		}
		Debug.Log("[KID] Initialisation - K-ID Title Data loaded in. Continuing.");
		return true;
	}

	[return: TupleElementNames(new string[] { "ageStatus", "resp" })]
	private static async Task<ValueTuple<AgeStatusType, TMPSession>> AgeGateFlow(GetPlayerData_Data newPlayerData)
	{
		TMPSession tmpsession = newPlayerData.session;
		TMPSession session = newPlayerData.session;
		AgeStatusType? ageStatusType = ((session != null) ? new AgeStatusType?(session.AgeStatus) : null);
		if (newPlayerData.AgeStatus == null)
		{
			Debug.Log("[KID::MANAGER] PHASE THREE (A) -- IN PROGRESS - Age not set, must process age gate");
			VerifyAgeData verifyAgeData = await KIDManager.ProcessAgeGate();
			Debug.Log("[KID::MANAGER] PHASE THREE (A) -- IN PROGRESS - Age Gate Completed");
			if (verifyAgeData == null)
			{
				Debug.Log("[KID::MANAGER] Verify Response returned NULL, this could happen if a Prohibited response was received and the age-gate exited, or the game shut down");
				return new ValueTuple<AgeStatusType, TMPSession>(AgeStatusType.DIGITALMINOR, null);
			}
			tmpsession = verifyAgeData.Session;
			ageStatusType = new AgeStatusType?(tmpsession.AgeStatus);
			if (tmpsession.IsDefault)
			{
				Debug.Log("[KID::MANAGER] PHASE THREE (A) -- IN PROGRESS - Age Gate completed - Default session received");
			}
		}
		if (ageStatusType == null)
		{
			Debug.LogError("[KID::MANAGER] PHASE THREE (A) -- FAILURE - Age Gate completed, but age status is null. Defaulting to MINOR");
			ageStatusType = new AgeStatusType?(AgeStatusType.DIGITALMINOR);
		}
		return new ValueTuple<AgeStatusType, TMPSession>(ageStatusType.Value, tmpsession);
	}

	private static async Task<VerifyAgeData> ProcessAgeGate()
	{
		Debug.Log("[KID::MANAGER] PHASE THREE (B) -- IN PROGRESS - Beginning Age-Gate");
		await KIDAgeGate.BeginAgeGate();
		VerifyAgeData verifyAgeData;
		if (KIDManager._requestCancellationSource.IsCancellationRequested)
		{
			verifyAgeData = null;
		}
		else
		{
			Debug.Log("[KID::MANAGER] PHASE THREE (B) -- IN PROGRESS - Age-Gate completed");
			Debug.Log("[KID::MANAGER] PHASE THREE (C) -- IN PROGRESS - Trying to verify Age Response");
			VerifyAgeData verifyResponse = await KIDManager.TryVerifyAgeResponse();
			if (KIDManager._requestCancellationSource.IsCancellationRequested)
			{
				verifyAgeData = null;
			}
			else
			{
				Debug.Log("[KID::MANAGER] PHASE THREE (C) -- IN PROGRESS - Verify Age Response completed");
				if (verifyResponse.Status == SessionStatus.PROHIBITED || verifyResponse.Status == SessionStatus.PENDING_AGE_APPEAL)
				{
					KIDUI_AgeAppealController.Instance.StartAgeAppealScreens(new SessionStatus?(verifyResponse.Status));
					GetPlayerData_Data getPlayerData_Data = await KIDManager.TryGetPlayerData(true);
					for (;;)
					{
						SessionStatus? sessionStatus = getPlayerData_Data.status;
						SessionStatus sessionStatus2 = SessionStatus.PROHIBITED;
						if (!((sessionStatus.GetValueOrDefault() == sessionStatus2) & (sessionStatus != null)))
						{
							sessionStatus = getPlayerData_Data.status;
							sessionStatus2 = SessionStatus.PENDING_AGE_APPEAL;
							if (!((sessionStatus.GetValueOrDefault() == sessionStatus2) & (sessionStatus != null)))
							{
								break;
							}
						}
						await Task.Delay(30000);
						getPlayerData_Data = await KIDManager.TryGetPlayerData(true);
					}
					verifyAgeData = verifyResponse;
				}
				else
				{
					verifyAgeData = verifyResponse;
				}
			}
		}
		return verifyAgeData;
	}

	public static string GetOptInKey(EKIDFeatures feature)
	{
		return feature.ToStandardisedString() + "-opt-in-" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
	}

	private static async Task<GetPlayerData_Data> Server_GetPlayerData(bool forceRefresh, Action failureCallback)
	{
		string text = string.Format("sessionRefresh={0}", forceRefresh ? "true" : "false");
		ValueTuple<long, GetPlayerDataResponse, string> valueTuple = await KIDManager.KIDServerWebRequest<GetPlayerDataResponse, KIDRequestData>("GetPlayerData", "GET", null, text, 3, null);
		long item = valueTuple.Item1;
		GetPlayerDataResponse item2 = valueTuple.Item2;
		GetSessionResponseType getSessionResponseType = GetSessionResponseType.ERROR;
		if (item != 200L)
		{
			if (item != 204L)
			{
				if (item == 404L)
				{
					getSessionResponseType = GetSessionResponseType.LOST;
				}
			}
			else
			{
				getSessionResponseType = GetSessionResponseType.NOT_FOUND;
			}
		}
		else
		{
			getSessionResponseType = GetSessionResponseType.OK;
		}
		GetPlayerData_Data getPlayerData_Data = new GetPlayerData_Data(getSessionResponseType, item2);
		if (item < 200L || item >= 300L)
		{
			if (failureCallback != null)
			{
				failureCallback();
			}
		}
		return getPlayerData_Data;
	}

	private static async Task<bool> Server_SetConfirmedStatus()
	{
		long num = await KIDManager.KIDServerWebRequestNoResponse<KIDRequestData>("SetConfirmedStatus", "POST", null, 2, null);
		bool flag;
		if (num == 200L)
		{
			flag = true;
		}
		else
		{
			Debug.LogError(string.Format("[KID::SERVER_ROUTER] SetConfirmedStatus request failed. Code: {0}", num));
			flag = false;
		}
		return flag;
	}

	private static async Task<UpgradeSessionData> Server_UpgradeSession(global::UpgradeSessionRequest request)
	{
		ValueTuple<long, global::UpgradeSessionResponse, string> valueTuple = await KIDManager.KIDServerWebRequest<global::UpgradeSessionResponse, global::UpgradeSessionRequest>("UpgradeSession", "POST", request, null, 2, null);
		long item = valueTuple.Item1;
		global::UpgradeSessionResponse item2 = valueTuple.Item2;
		if (item != 200L)
		{
			Debug.LogError(string.Format("[KID::SERVER_ROUTER] Upgrade session request failed. Code: {0}", item));
		}
		UpgradeSessionData upgradeSessionData;
		if (item2 == null)
		{
			Debug.LogError("[KID::SERVER_ROUTER] Upgrade session response is NULL. This is unexpected.");
			upgradeSessionData = null;
		}
		else
		{
			upgradeSessionData = new UpgradeSessionData(item2);
		}
		return upgradeSessionData;
	}

	private static async Task<VerifyAgeData> Server_VerifyAge(VerifyAgeRequest request, Action failureCallback)
	{
		ValueTuple<long, VerifyAgeResponse, string> valueTuple = await KIDManager.KIDServerWebRequest<VerifyAgeResponse, VerifyAgeRequest>("VerifyAge", "POST", request, null, 2, null);
		long item = valueTuple.Item1;
		VerifyAgeData verifyAgeData = new VerifyAgeData(valueTuple.Item2, request.Age);
		if (item < 200L || item >= 300L)
		{
			if (failureCallback != null)
			{
				failureCallback();
			}
		}
		return verifyAgeData;
	}

	private static async Task<AttemptAgeUpdateData> Server_AttemptAgeUpdate(AttemptAgeUpdateRequest request, Action failureCallback)
	{
		ValueTuple<long, AttemptAgeUpdateResponse, string> valueTuple = await KIDManager.KIDServerWebRequest<AttemptAgeUpdateResponse, AttemptAgeUpdateRequest>("AttemptAgeUpdate", "POST", request, null, 2, null);
		long item = valueTuple.Item1;
		AttemptAgeUpdateResponse item2 = valueTuple.Item2;
		if (item != 200L)
		{
			Debug.LogError(string.Format("[KID::SERVER_ROUTER] Attempt age update request failed. Code: {0}", item));
		}
		return new AttemptAgeUpdateData(item2.Status);
	}

	private static async Task<bool> Server_AppealAge(AppealAgeRequest request, Action failureCallback)
	{
		bool success = false;
		long num = await KIDManager.KIDServerWebRequestNoResponse<AppealAgeRequest>("AppealAge", "POST", request, 2, null);
		if (num == 200L)
		{
			success = true;
		}
		else
		{
			Debug.LogError(string.Format("[KID::SERVER_ROUTER] Appeal age request failed. Code: {0}", num));
		}
		return success;
	}

	private static async Task<ValueTuple<bool, string>> Server_SendChallengeEmail(SendChallengeEmailRequest request)
	{
		bool success = false;
		ValueTuple<long, object, string> valueTuple = await KIDManager.KIDServerWebRequest<object, SendChallengeEmailRequest>("SendChallengeEmail", "POST", request, null, 2, null);
		long item = valueTuple.Item1;
		string item2 = valueTuple.Item3;
		ValueTuple<bool, string> valueTuple2;
		if (item >= 200L && item < 300L)
		{
			success = true;
			valueTuple2 = new ValueTuple<bool, string>(success, string.Empty);
		}
		else
		{
			Debug.Log(string.Format("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Send challenge email request failed. Code: [{0}] ErrorMessage: [{1}]", item, item2));
			string text = "Oops, something went wrong.";
			ErrorContent errorContent = new ErrorContent
			{
				Error = "Unhandled",
				Message = "This error is unhandled"
			};
			try
			{
				errorContent = JsonConvert.DeserializeObject<ErrorContent>(item2);
			}
			catch (Exception)
			{
				Debug.LogError("Could not deserialize error message");
			}
			if (item <= 403L)
			{
				if (item != 400L)
				{
					if (item == 403L)
					{
						text = "This account has been banned. Please contact Customer Support.";
						Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Account is banned for player ID: [" + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + "]");
					}
				}
				else if (errorContent.Error.ToLower().Contains("BadRequest-InvalidEmail".ToLower()))
				{
					text = "This email doesn't seem right. Please check and try again.";
					Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Invalid email format: [" + request.Email + "]");
				}
				else if (errorContent.Error.ToLower().Contains("BadRequest-PlayerDataNotFound".ToLower()))
				{
					text = "Something went wrong. Please reboot the game and try again.";
					Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Player data not found for player ID: [" + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + "]");
				}
				else if (errorContent.Error.ToLower().Contains("BadRequest-ChallengeNotFound".ToLower()))
				{
					text = "Couldn't find your challenge. If this keeps happening, contact Customer Support.";
					Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Challenge not found for player ID: [" + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + "]");
				}
			}
			else if (item != 429L)
			{
				if (item == 500L)
				{
					if (errorContent.Error.ToLower().Contains("InternalServerError-FailedToRetrievePlayerData".ToLower()))
					{
						text = "We couldn't find your player data. Please reboot the game and try again.";
						Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Failed to retrieve player data for player ID: [" + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + "]");
					}
					else if (errorContent.Error.ToLower().Contains("InternalServerError-UnhandledException".ToLower()))
					{
						text = "Something went wrong. Please reboot the game and try again.";
						Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Unhandled exception for player ID: [" + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + "]");
					}
					else if (errorContent.Error.ToLower().Contains("InternalServerError-SendEmail".ToLower()))
					{
						text = "Something went wrong while sending the email. Please reboot the game and try again.";
						Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Failed to send email for player ID: [" + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + "]");
					}
				}
			}
			else
			{
				text = "You've sent too many! Please wait a moment and try again.";
				Debug.LogError("[KID::SERVER_ROUTER::Server_SendChallengeEmail] Too many requests for player ID: [" + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + "]");
			}
			valueTuple2 = new ValueTuple<bool, string>(success, text);
		}
		return valueTuple2;
	}

	private static async Task<bool> Server_SetOptInPermissions(SetOptInPermissionsRequest request, Action failureCallback)
	{
		bool success = false;
		Debug.Log("[KID::SERVER_ROUTER::OptInRefactor] Setting opt-in permissions with request: " + JsonConvert.SerializeObject(request));
		long num = await KIDManager.KIDServerWebRequestNoResponse<SetOptInPermissionsRequest>("SetOptInPermissions", "POST", request, 2, null);
		if (num >= 200L && num < 300L)
		{
			success = true;
		}
		else
		{
			Debug.LogError(string.Format("[KID::SERVER_ROUTER] SetOptInPermissions request failed. Code: {0}", num));
			if (failureCallback != null)
			{
				failureCallback();
			}
		}
		Debug.Log(string.Format("[KID::SERVER_ROUTER::OptInRefactor] SetOptInPermissions request completed with success: {0} - code: {1}", success, num));
		return success;
	}

	private static async Task<bool> Server_OptIn()
	{
		long num = await KIDManager.KIDServerWebRequestNoResponse<KIDRequestData>("OptIn", "POST", null, 2, null);
		bool flag;
		if (num == 200L)
		{
			flag = true;
		}
		else
		{
			Debug.LogError(string.Format("[KID::SERVER_ROUTER] Opt in request failed. Code: {0}", num));
			flag = false;
		}
		return flag;
	}

	private static async Task<GetRequirementsData> Server_GetRequirements()
	{
		ValueTuple<long, GetAgeGateRequirementsResponse, string> valueTuple = await KIDManager.KIDServerWebRequest<GetAgeGateRequirementsResponse, KIDRequestData>("GetRequirements", "GET", null, null, 3, null);
		long item = valueTuple.Item1;
		GetAgeGateRequirementsResponse item2 = valueTuple.Item2;
		GetRequirementsData getRequirementsData = new GetRequirementsData
		{
			AgeGateRequirements = item2
		};
		GetRequirementsData getRequirementsData2;
		if (item == 200L)
		{
			getRequirementsData2 = getRequirementsData;
		}
		else
		{
			Debug.LogError(string.Format("[KID::SERVER_ROUTER] Get Age-gate Requirements FAILED. Code: {0}", item));
			getRequirementsData2 = getRequirementsData;
		}
		return getRequirementsData2;
	}

	[return: TupleElementNames(new string[] { "code", "responseModel", "errorMessage" })]
	private static async Task<ValueTuple<long, T, string>> KIDServerWebRequest<T, Q>(string endpoint, string operationType, Q requestData, string queryParams = null, int maxRetries = 2, Func<long, bool> responseCodeIsRetryable = null) where T : class where Q : KIDRequestData
	{
		int retryCount = 0;
		string URL = "/api/" + endpoint;
		if (!string.IsNullOrEmpty(queryParams))
		{
			URL = URL + "?" + queryParams;
		}
		Debug.Log("[KID::MANAGER::SERVER_ROUTER] URL: " + URL);
		ValueTuple<long, T, string> valueTuple;
		for (;;)
		{
			using (UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.KidApiBaseUrl + URL, operationType))
			{
				byte[] array = Array.Empty<byte>();
				string json = "";
				if (requestData != null)
				{
					json = JsonConvert.SerializeObject(requestData);
					array = Encoding.UTF8.GetBytes(json);
				}
				request.uploadHandler = new UploadHandlerRaw(array);
				request.downloadHandler = new DownloadHandlerBuffer();
				request.SetRequestHeader("Content-Type", "application/json");
				request.SetRequestHeader("X-Authorization", PlayFabSettings.staticPlayer.ClientSessionTicket);
				request.SetRequestHeader("X-PlayerId", PlayFabSettings.staticPlayer.PlayFabId);
				request.SetRequestHeader("X-Mothership-Token", MothershipClientContext.Token);
				request.SetRequestHeader("X-Mothership-Player-Id", MothershipClientContext.MothershipId);
				request.SetRequestHeader("X-Mothership-Env-Id", MothershipClientApiUnity.EnvironmentId);
				request.SetRequestHeader("X-Mothership-Deployment-Id", MothershipClientApiUnity.DeploymentId);
				if (!PlayFabAuthenticatorSettings.KidApiBaseUrl.Contains("gtag-cf.com"))
				{
					request.SetRequestHeader("CF-IPCountry", RegionInfo.CurrentRegion.TwoLetterISORegionName);
				}
				request.timeout = 15;
				UnityWebRequest unityWebRequest = await request.SendWebRequest();
				if (unityWebRequest.result == UnityWebRequest.Result.Success)
				{
					if (typeof(T) == typeof(object))
					{
						valueTuple = new ValueTuple<long, T, string>(unityWebRequest.responseCode, default(T), unityWebRequest.error);
						break;
					}
					try
					{
						T t = JsonConvert.DeserializeObject<T>(unityWebRequest.downloadHandler.text);
						valueTuple = new ValueTuple<long, T, string>(unityWebRequest.responseCode, t, unityWebRequest.error);
						break;
					}
					catch (Exception)
					{
						Debug.LogError("[KID::SERVER_ROUTER] Failed to convert to class type [T] via JSON:\n[" + unityWebRequest.downloadHandler.text + "]");
						valueTuple = new ValueTuple<long, T, string>(unityWebRequest.responseCode, default(T), unityWebRequest.error);
						break;
					}
				}
				bool flag = request.result != UnityWebRequest.Result.ProtocolError;
				if (!flag)
				{
					bool flag2;
					if (responseCodeIsRetryable != null)
					{
						flag2 = responseCodeIsRetryable(request.responseCode);
					}
					else
					{
						long responseCode = request.responseCode;
						if (responseCode >= 500L)
						{
							if (responseCode >= 600L)
							{
								goto IL_035E;
							}
						}
						else if (responseCode != 408L && responseCode != 429L)
						{
							goto IL_035E;
						}
						bool flag3 = true;
						goto IL_0361;
						IL_035E:
						flag3 = false;
						IL_0361:
						flag2 = flag3;
					}
					flag = flag2;
				}
				if (flag)
				{
					if (retryCount < maxRetries)
					{
						float num = Random.Range(0.5f, Mathf.Pow(2f, (float)(++retryCount)));
						Debug.LogWarning(string.Concat(new string[] { "[KID::SERVER_ROUTER] Tried sending request [", operationType, " - ", endpoint, "] but it failed:\n", unityWebRequest.error, "\n\nRequest:\n", json }));
						Debug.LogWarning(string.Format("[KID::SERVER_ROUTER] Retrying {0}... Retry attempt #{1}, waiting for {2} seconds", endpoint, retryCount, num));
						await Task.Delay(TimeSpan.FromSeconds((double)num));
						continue;
					}
					Debug.LogError(string.Concat(new string[] { "[KID::SERVER_ROUTER] Tried sending request [", operationType, " - ", endpoint, "] but it failed:\n", unityWebRequest.error, "\n\nRequest:\n", json }));
					Debug.LogError("[KID::SERVER_ROUTER] Maximum retries attempted. Please check your network connection.");
				}
				if (request.result == UnityWebRequest.Result.ProtocolError)
				{
					Debug.LogError(string.Format("[KID::SERVER_ROUTER] HTTP {0} ERROR: {1}\nMessage: {2}", request.responseCode, request.error, request.downloadHandler.text));
				}
				else if (request.result == UnityWebRequest.Result.ConnectionError)
				{
					Debug.LogError("[KID::SERVER_ROUTER] NETWORK ERROR: " + request.error + "\nMessage: " + request.downloadHandler.text);
					if (KIDUI_Controller.Instance != null)
					{
						KIDMessagingController.ShowConnectionErrorScreen();
					}
				}
				else
				{
					Debug.LogError("[KID::SERVER_ROUTER] ERROR: " + request.error + "\nMessage: " + request.downloadHandler.text);
				}
				valueTuple = new ValueTuple<long, T, string>(unityWebRequest.responseCode, default(T), unityWebRequest.downloadHandler.text);
			}
			break;
		}
		return valueTuple;
	}

	private static async Task<long> KIDServerWebRequestNoResponse<Q>(string endpoint, string operationType, Q requestData, int maxRetries = 2, Func<long, bool> responseCodeIsRetryable = null) where Q : KIDRequestData
	{
		TaskAwaiter<ValueTuple<long, object, string>> taskAwaiter = KIDManager.KIDServerWebRequest<object, Q>(endpoint, operationType, requestData, null, maxRetries, responseCodeIsRetryable).GetAwaiter();
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			TaskAwaiter<ValueTuple<long, object, string>> taskAwaiter2;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<ValueTuple<long, object, string>>);
		}
		return taskAwaiter.GetResult().Item1;
	}

	public static void RegisterSessionUpdateCallback_AnyPermission(Action callback)
	{
		Debug.Log("[KID] Successfully registered a new callback to SessionUpdate which monitors any permission change");
		KIDManager._onSessionUpdated_AnyPermission = (Action)Delegate.Combine(KIDManager._onSessionUpdated_AnyPermission, callback);
	}

	public static void UnregisterSessionUpdateCallback_AnyPermission(Action callback)
	{
		Debug.Log("[KID] Successfully unregistered a new callback to SessionUpdate which monitors any permission change");
		KIDManager._onSessionUpdated_AnyPermission = (Action)Delegate.Remove(KIDManager._onSessionUpdated_AnyPermission, callback);
	}

	public static void RegisterSessionUpdatedCallback_VoiceChat(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully registered a new callback to SessionUpdate which monitors the Voice Chat permission");
		KIDManager._onSessionUpdated_VoiceChat = (Action<bool, Permission.ManagedByEnum>)Delegate.Combine(KIDManager._onSessionUpdated_VoiceChat, callback);
	}

	public static void UnregisterSessionUpdatedCallback_VoiceChat(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully unregistered a callback to SessionUpdate which monitors the Voice Chat permission");
		KIDManager._onSessionUpdated_VoiceChat = (Action<bool, Permission.ManagedByEnum>)Delegate.Remove(KIDManager._onSessionUpdated_VoiceChat, callback);
	}

	public static void RegisterSessionUpdatedCallback_CustomUsernames(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully registered a new callback to SessionUpdate which monitors the Custom Usernames permission");
		KIDManager._onSessionUpdated_CustomUsernames = (Action<bool, Permission.ManagedByEnum>)Delegate.Combine(KIDManager._onSessionUpdated_CustomUsernames, callback);
	}

	public static void UnregisterSessionUpdatedCallback_CustomUsernames(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully unregistered a callback to SessionUpdate which monitors the Custom Usernames permission");
		KIDManager._onSessionUpdated_CustomUsernames = (Action<bool, Permission.ManagedByEnum>)Delegate.Remove(KIDManager._onSessionUpdated_CustomUsernames, callback);
	}

	public static void RegisterSessionUpdatedCallback_PrivateRooms(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully registered a new callback to SessionUpdate which monitors the Private Rooms permission");
		KIDManager._onSessionUpdated_PrivateRooms = (Action<bool, Permission.ManagedByEnum>)Delegate.Combine(KIDManager._onSessionUpdated_PrivateRooms, callback);
	}

	public static void UnregisterSessionUpdatedCallback_PrivateRooms(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully unregistered a callback to SessionUpdate which monitors the Private Rooms permission");
		KIDManager._onSessionUpdated_PrivateRooms = (Action<bool, Permission.ManagedByEnum>)Delegate.Remove(KIDManager._onSessionUpdated_PrivateRooms, callback);
	}

	public static void RegisterSessionUpdatedCallback_Multiplayer(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully registered a new callback to SessionUpdate which monitors the Multiplayer permission");
		KIDManager._onSessionUpdated_Multiplayer = (Action<bool, Permission.ManagedByEnum>)Delegate.Combine(KIDManager._onSessionUpdated_Multiplayer, callback);
	}

	public static void UnregisterSessionUpdatedCallback_Multiplayer(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully unregistered a callback to SessionUpdate which monitors the Multiplayer permission");
		KIDManager._onSessionUpdated_Multiplayer = (Action<bool, Permission.ManagedByEnum>)Delegate.Remove(KIDManager._onSessionUpdated_Multiplayer, callback);
	}

	public static void RegisterSessionUpdatedCallback_UGC(Action<bool, Permission.ManagedByEnum> callback)
	{
		Debug.Log("[KID] Successfully registered a new callback to SessionUpdate which monitors the UGC permission");
		KIDManager._onSessionUpdated_UGC = (Action<bool, Permission.ManagedByEnum>)Delegate.Combine(KIDManager._onSessionUpdated_UGC, callback);
	}

	public static async Task<bool> WaitForAndUpdateNewSession(bool forceRefresh)
	{
		bool flag;
		if (KIDManager._isUpdatingNewSession)
		{
			Debug.LogError("[KID::MANAGER] Trying to UpdateNewSession, but is already running, or state was not reset. Will not start again");
			flag = false;
		}
		else
		{
			KIDManager._isUpdatingNewSession = true;
			Debug.Log(string.Format("[KID::MANAGER] UpdateNewSession -- START - Starting Update New Session async Loop. Max duration: [{0:#} minutes", 10f));
			float updateTimeout = Time.time + 600f;
			GetPlayerData_Data getPlayerData_Data = await KIDManager.TryGetPlayerData(forceRefresh);
			TMPSession tmpsession = ((getPlayerData_Data != null) ? getPlayerData_Data.session : null);
			bool flag2 = KIDManager.HasSessionChanged(tmpsession);
			while (Time.time < updateTimeout && (tmpsession == null || tmpsession.Age == 0 || !flag2))
			{
				await Task.Delay(30000);
				if (KIDManager._requestCancellationSource.IsCancellationRequested)
				{
					Debug.Log("[KID::MANAGER] UpdateNewSession -- CANCELLED - CancellationTokenSource was cancelled, aborting session Update");
					KIDManager._isUpdatingNewSession = false;
					return false;
				}
				Debug.Log("[KID::MANAGER] UpdateNewSession -- LOOP - Trying to get Player Data");
				getPlayerData_Data = await KIDManager.TryGetPlayerData(forceRefresh);
				tmpsession = ((getPlayerData_Data != null) ? getPlayerData_Data.session : null);
				flag2 = KIDManager.HasSessionChanged(tmpsession);
				if (flag2)
				{
					Debug.Log("[KID::MANAGER] UpdateNewSession -- SUCCESS - Valid, updated session has been found");
					break;
				}
				if (getPlayerData_Data == null)
				{
					Debug.LogError("[KID::MANAGER] UpdateNewSession -- LOOP - Tried getting Player Data but returned NULL");
				}
				else if (getPlayerData_Data.responseType == GetSessionResponseType.ERROR)
				{
					Debug.LogError("[KID::MANAGER] UpdateNewSession -- LOOP - Tried getting a new Session but playerData returned with ERROR");
				}
				else if (tmpsession == null)
				{
					Debug.LogError("[KID::MANAGER] UpdateNewSession -- LOOP - Found Player Data, but SESSION was NULL");
				}
			}
			KIDManager._isUpdatingNewSession = false;
			if (getPlayerData_Data == null || getPlayerData_Data.responseType != GetSessionResponseType.OK || tmpsession == null)
			{
				Debug.Log("[KID::MANAGER] UpdateNewSession -- FAILED - Was unable to get new session in time");
				flag = false;
			}
			else
			{
				flag = KIDManager.UpdatePermissions(tmpsession);
			}
		}
		return flag;
	}

	private static bool HasSessionChanged(TMPSession newSession)
	{
		if (newSession == null)
		{
			return false;
		}
		if (KIDManager.CurrentSession == null)
		{
			return true;
		}
		if (!newSession.IsValidSession)
		{
			return false;
		}
		if (newSession.IsDefault)
		{
			Debug.LogError(string.Format("[KID::MANAGER] DEBUG - New Session Is Default! Age: [{0}]", newSession.Age));
			return false;
		}
		return KIDManager.CurrentSession.IsDefault || !newSession.Etag.Equals(KIDManager.CurrentSession.Etag);
	}

	private static void OnSessionUpdated()
	{
		Action onSessionUpdated_AnyPermission = KIDManager._onSessionUpdated_AnyPermission;
		if (onSessionUpdated_AnyPermission != null)
		{
			onSessionUpdated_AnyPermission();
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		List<Permission> allPermissionsData = KIDManager.GetAllPermissionsData();
		int count = allPermissionsData.Count;
		for (int i = 0; i < count; i++)
		{
			Permission permission = allPermissionsData[i];
			string name = permission.Name;
			if (!(name == "voice-chat"))
			{
				if (!(name == "custom-username"))
				{
					if (!(name == "join-groups"))
					{
						if (!(name == "multiplayer"))
						{
							if (!(name == "mods"))
							{
								Debug.Log("[KID] Tried updating permission with name [" + permission.Name + "] but did not match any of the set cases. Unable to process");
							}
							else if (KIDManager.HasPermissionChanged(permission))
							{
								Action<bool, Permission.ManagedByEnum> onSessionUpdated_UGC = KIDManager._onSessionUpdated_UGC;
								if (onSessionUpdated_UGC != null)
								{
									onSessionUpdated_UGC(permission.Enabled, permission.ManagedBy);
								}
								KIDManager._previousPermissionSettings[permission.Name] = permission;
							}
						}
						else
						{
							if (KIDManager.HasPermissionChanged(permission))
							{
								Action<bool, Permission.ManagedByEnum> onSessionUpdated_Multiplayer = KIDManager._onSessionUpdated_Multiplayer;
								if (onSessionUpdated_Multiplayer != null)
								{
									onSessionUpdated_Multiplayer(permission.Enabled, permission.ManagedBy);
								}
								KIDManager._previousPermissionSettings[permission.Name] = permission;
							}
							bool enabled = permission.Enabled;
						}
					}
					else
					{
						if (KIDManager.HasPermissionChanged(permission))
						{
							Action<bool, Permission.ManagedByEnum> onSessionUpdated_PrivateRooms = KIDManager._onSessionUpdated_PrivateRooms;
							if (onSessionUpdated_PrivateRooms != null)
							{
								onSessionUpdated_PrivateRooms(permission.Enabled, permission.ManagedBy);
							}
							KIDManager._previousPermissionSettings[permission.Name] = permission;
						}
						flag2 = permission.Enabled;
					}
				}
				else
				{
					if (KIDManager.HasPermissionChanged(permission))
					{
						Action<bool, Permission.ManagedByEnum> onSessionUpdated_CustomUsernames = KIDManager._onSessionUpdated_CustomUsernames;
						if (onSessionUpdated_CustomUsernames != null)
						{
							onSessionUpdated_CustomUsernames(permission.Enabled, permission.ManagedBy);
						}
						KIDManager._previousPermissionSettings[permission.Name] = permission;
					}
					flag3 = permission.Enabled;
				}
			}
			else
			{
				if (KIDManager.HasPermissionChanged(permission))
				{
					Action<bool, Permission.ManagedByEnum> onSessionUpdated_VoiceChat = KIDManager._onSessionUpdated_VoiceChat;
					if (onSessionUpdated_VoiceChat != null)
					{
						onSessionUpdated_VoiceChat(permission.Enabled, permission.ManagedBy);
					}
					KIDManager._previousPermissionSettings[permission.Name] = permission;
				}
				flag = permission.Enabled;
			}
		}
		GorillaTelemetry.PostKidEvent(flag2, flag, flag3, KIDManager.CurrentSession.AgeStatus, GTKidEventType.permission_update);
	}

	private static bool HasPermissionChanged(Permission newValue)
	{
		Permission permission;
		if (KIDManager._previousPermissionSettings.TryGetValue(newValue.Name, out permission))
		{
			return permission.Enabled != newValue.Enabled || permission.ManagedBy != newValue.ManagedBy;
		}
		KIDManager._previousPermissionSettings.Add(newValue.Name, newValue);
		return true;
	}

	public const string MULTIPLAYER_PERMISSION_NAME = "multiplayer";

	public const string UGC_PERMISSION_NAME = "mods";

	public const string PRIVATE_ROOM_PERMISSION_NAME = "join-groups";

	public const string VOICE_CHAT_PERMISSION_NAME = "voice-chat";

	public const string CUSTOM_USERNAME_PERMISSION_NAME = "custom-username";

	public const string PREVIOUS_STATUS_PREF_KEY_PREFIX = "previous-status-";

	public const string KID_DATA_KEY = "KIDData";

	private const string KID_EMAIL_KEY = "k-id_EmailAddress";

	private const int SECONDS_BETWEEN_UPDATE_ATTEMPTS = 30;

	private const string KID_SETUP_FLAG = "KID-Setup-";

	[OnEnterPlay_SetNull]
	private static KIDManager _instance;

	private static string _emailAddress;

	private static CancellationTokenSource _requestCancellationSource = new CancellationTokenSource();

	private static bool _titleDataReady = false;

	private static bool _useKid = false;

	private static int _kIDPhase = 0;

	private static DateTime? _kIDNewPlayerDateTime = null;

	private static string _debugKIDLocalePlayerPrefRef = "KID_SPOOF_LOCALE";

	private static string parentEmailForUserPlayerPrefRef;

	[OnEnterPlay_SetNull]
	private static Action _sessionUpdatedCallback = null;

	[OnEnterPlay_SetNull]
	private static Action _onKIDInitialisationComplete = null;

	public static KIDManager.OnEmailResultReceived onEmailResultReceived;

	private const string KID_GET_SESSION = "GetPlayerData";

	private const string KID_VERIFY_AGE = "VerifyAge";

	private const string KID_UPGRADE_SESSION = "UpgradeSession";

	private const string KID_SEND_CHALLENGE_EMAIL = "SendChallengeEmail";

	private const string KID_ATTEMPT_AGE_UPDATE = "AttemptAgeUpdate";

	private const string KID_APPEAL_AGE = "AppealAge";

	private const string KID_OPT_IN = "OptIn";

	private const string KID_GET_REQUIREMENTS = "GetRequirements";

	private const string KID_SET_CONFIRMED_STATUS = "SetConfirmedStatus";

	private const string KID_SET_OPT_IN_PERMISSIONS = "SetOptInPermissions";

	private const string KID_FORCE_REFRESH = "sessionRefresh";

	private const int MAX_RETRIES_FOR_CRITICAL_KID_SERVER_REQUESTS = 3;

	private const int MAX_RETRIES_FOR_NORMAL_KID_SERVER_REQUESTS = 2;

	public const string KID_PERMISSION__VOICE_CHAT = "voice-chat";

	public const string KID_PERMISSION__CUSTOM_NAMES = "custom-username";

	public const string KID_PERMISSION__PRIVATE_ROOMS = "join-groups";

	public const string KID_PERMISSION__MULTIPLAYER = "multiplayer";

	public const string KID_PERMISSION__UGC = "mods";

	private const float MAX_SESSION_UPDATE_TIME = 600f;

	private const int TIME_BETWEEN_SESSION_UPDATE_ATTEMPTS = 30;

	[OnEnterPlay_SetNull]
	private static Action _onSessionUpdated_AnyPermission;

	[OnEnterPlay_SetNull]
	private static Action<bool, Permission.ManagedByEnum> _onSessionUpdated_VoiceChat;

	[OnEnterPlay_SetNull]
	private static Action<bool, Permission.ManagedByEnum> _onSessionUpdated_CustomUsernames;

	[OnEnterPlay_SetNull]
	private static Action<bool, Permission.ManagedByEnum> _onSessionUpdated_PrivateRooms;

	[OnEnterPlay_SetNull]
	private static Action<bool, Permission.ManagedByEnum> _onSessionUpdated_Multiplayer;

	[OnEnterPlay_SetNull]
	private static Action<bool, Permission.ManagedByEnum> _onSessionUpdated_UGC;

	private static bool _isUpdatingNewSession = false;

	[OnEnterPlay_SetNull]
	private static Dictionary<string, Permission> _previousPermissionSettings = new Dictionary<string, Permission>();

	public delegate void OnEmailResultReceived(bool result);
}
