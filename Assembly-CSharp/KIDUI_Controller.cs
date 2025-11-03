using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GorillaNetworking;
using KID.Model;
using UnityEngine;

public class KIDUI_Controller : MonoBehaviour
{
	public static KIDUI_Controller Instance
	{
		get
		{
			return KIDUI_Controller._instance;
		}
	}

	public static bool IsKIDUIActive
	{
		get
		{
			return !(KIDUI_Controller.Instance == null) && KIDUI_Controller.Instance._isKidUIActive;
		}
	}

	private static string EtagOnCloseBlackScreenPlayerPrefRef
	{
		get
		{
			if (string.IsNullOrEmpty(KIDUI_Controller.etagOnCloseBlackScreenPlayerPrefStr))
			{
				KIDUI_Controller.etagOnCloseBlackScreenPlayerPrefStr = "closeBlackScreen-" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
			}
			return KIDUI_Controller.etagOnCloseBlackScreenPlayerPrefStr;
		}
	}

	private void Awake()
	{
		KIDUI_Controller._instance = this;
		Debug.LogFormat("[KID::UI::CONTROLLER] Controller Initialised", Array.Empty<object>());
	}

	private void OnDestroy()
	{
		KIDManager.onEmailResultReceived = (KIDManager.OnEmailResultReceived)Delegate.Remove(KIDManager.onEmailResultReceived, new KIDManager.OnEmailResultReceived(this.NotifyOfEmailResult));
	}

	public async Task StartKIDScreens(CancellationToken cancellationToken)
	{
		Debug.LogFormat("[KID::UI::CONTROLLER] Starting k-ID Screens", Array.Empty<object>());
		bool flag = await this.ShouldShowKIDScreen(cancellationToken);
		if (!cancellationToken.IsCancellationRequested)
		{
			if (!flag)
			{
				Debug.LogFormat("[KID::UI::CONTROLLER] Should NOT Show k-ID Screens", Array.Empty<object>());
			}
			else
			{
				PrivateUIRoom.ForceStartOverlay();
				Debug.LogFormat("[KID::UI::CONTROLLER] Showing k-ID Screens", Array.Empty<object>());
				while (HandRayController.Instance == null)
				{
					await Task.Yield();
				}
				HandRayController.Instance.EnableHandRays();
				PrivateUIRoom.AddUI(base.transform);
				EMainScreenStatus screenStatusFromSession = this.GetScreenStatusFromSession();
				this._mainKIDScreen.ShowMainScreen(screenStatusFromSession, this._showReason);
				this._isKidUIActive = true;
				KIDManager.onEmailResultReceived = (KIDManager.OnEmailResultReceived)Delegate.Combine(KIDManager.onEmailResultReceived, new KIDManager.OnEmailResultReceived(this.NotifyOfEmailResult));
			}
		}
	}

	public void CloseKIDScreens()
	{
		this.SaveEtagOnCloseScreen();
		this._isKidUIActive = false;
		this._mainKIDScreen.HideMainScreen();
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance != null)
		{
			instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
		}
		PrivateUIRoom.RemoveUI(base.transform);
		HandRayController.Instance.DisableHandRays();
		Object.DestroyImmediate(base.gameObject);
		KIDManager.onEmailResultReceived = (KIDManager.OnEmailResultReceived)Delegate.Remove(KIDManager.onEmailResultReceived, new KIDManager.OnEmailResultReceived(this.NotifyOfEmailResult));
	}

	public void UpdateScreenStatus()
	{
		EMainScreenStatus screenStatusFromSession = this.GetScreenStatusFromSession();
		KIDUI_MainScreen mainKIDScreen = this._mainKIDScreen;
		if (mainKIDScreen == null)
		{
			return;
		}
		mainKIDScreen.UpdateScreenStatus(screenStatusFromSession, true);
	}

	public void NotifyOfEmailResult(bool success)
	{
		if (this._confirmScreen == null)
		{
			Debug.LogError("[KID::UI_CONTROLLER] _confirmScreen has not been set yet and is NULL. Cannot inform of result");
			return;
		}
		if (success)
		{
			PlayerPrefs.SetInt(KIDManager.GetChallengedBeforePlayerPrefRef, 1);
			PlayerPrefs.Save();
		}
		Debug.Log("[KID::UI_CONTROLLER] Notifying user about email result. Showing confirm screen.");
		this._confirmScreen.NotifyOfResult(success);
	}

	private EMainScreenStatus GetScreenStatusFromSession()
	{
		EMainScreenStatus emainScreenStatus;
		switch (KIDManager.CurrentSession.SessionStatus)
		{
		case SessionStatus.PASS:
			if (this.ShouldShowScreenOnPermissionChange())
			{
				emainScreenStatus = EMainScreenStatus.Updated;
			}
			else if (KIDManager.PreviousStatus == SessionStatus.CHALLENGE_SESSION_UPGRADE)
			{
				emainScreenStatus = EMainScreenStatus.Declined;
			}
			else
			{
				emainScreenStatus = EMainScreenStatus.Missing;
			}
			break;
		case SessionStatus.PROHIBITED:
			Debug.LogError("[KID::KIDUI_CONTROLLER] Status is PROHIBITED but is trying to show k-ID screens");
			emainScreenStatus = EMainScreenStatus.Declined;
			break;
		case SessionStatus.CHALLENGE:
		case SessionStatus.CHALLENGE_SESSION_UPGRADE:
		case SessionStatus.PENDING_AGE_APPEAL:
			if (string.IsNullOrEmpty(PlayerPrefs.GetString(KIDManager.GetEmailForUserPlayerPrefRef, "")))
			{
				emainScreenStatus = EMainScreenStatus.Setup;
			}
			else
			{
				emainScreenStatus = EMainScreenStatus.Pending;
			}
			break;
		default:
			Debug.LogError("[KID::KIDUI_CONTROLLER] Unknown status");
			emainScreenStatus = EMainScreenStatus.None;
			break;
		}
		return emainScreenStatus;
	}

	private async Task<bool> ShouldShowKIDScreen(CancellationToken cancellationToken)
	{
		bool flag;
		if (KIDManager.CurrentSession == null)
		{
			this._showReason = KIDUI_Controller.Metrics_ShowReason.No_Session;
			flag = true;
		}
		else
		{
			if (!KIDManager.CurrentSession.IsValidSession)
			{
				while (!KIDManager.CurrentSession.IsValidSession)
				{
					Debug.Log("[KID::UI::CONTROLLER] K-ID Session not found yet");
					await Task.Delay(100, cancellationToken);
				}
			}
			Debug.Log("[KID::UI::CONTROLLER] K-ID Session has been found and is proceeding ");
			if (KIDManager.HasAllPermissions())
			{
				flag = false;
			}
			else
			{
				for (int i = 0; i < this._inaccessibleSettings.Count; i++)
				{
					Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(this._inaccessibleSettings[i]);
					if (permissionDataByFeature == null)
					{
						Debug.LogErrorFormat(string.Format("[KID::UI::CONTROLLER] Failed to get Permission with name [{0}]", this._inaccessibleSettings[i]), Array.Empty<object>());
						return true;
					}
					if (permissionDataByFeature.ManagedBy != Permission.ManagedByEnum.PROHIBITED && !KIDManager.CheckFeatureSettingEnabled(this._inaccessibleSettings[i]))
					{
						this._showReason = KIDUI_Controller.Metrics_ShowReason.Inaccessible;
						if (KIDManager.CurrentSession.IsDefault)
						{
							this._showReason = KIDUI_Controller.Metrics_ShowReason.Default_Session;
						}
						return true;
					}
				}
				List<Permission> allPermissionsData = KIDManager.GetAllPermissionsData();
				for (int j = 0; j < allPermissionsData.Count; j++)
				{
					if (allPermissionsData[j].ManagedBy == Permission.ManagedByEnum.GUARDIAN && !allPermissionsData[j].Enabled)
					{
						this._showReason = KIDUI_Controller.Metrics_ShowReason.Guardian_Disabled;
						if (KIDManager.CurrentSession.IsDefault)
						{
							this._showReason = KIDUI_Controller.Metrics_ShowReason.Default_Session;
						}
						return true;
					}
				}
				this._mainKIDScreen.InitialiseMainScreen();
				if (this._mainKIDScreen.GetFeatureListingCount() == 0)
				{
					Debug.Log("[KID::CONTROLLER] Nothing to show on k-ID UI. Skipping");
					flag = false;
				}
				else if (this.ShouldShowScreenOnPermissionChange())
				{
					this._showReason = KIDUI_Controller.Metrics_ShowReason.Permissions_Changed;
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
		}
		return flag;
	}

	private bool ShouldShowScreenOnPermissionChange()
	{
		this._lastEtagOnClose = this.GetLastBlackScreenEtag();
		string lastEtagOnClose = this._lastEtagOnClose;
		TMPSession currentSession = KIDManager.CurrentSession;
		return lastEtagOnClose != (((currentSession != null) ? currentSession.Etag : null) ?? string.Empty);
	}

	private string GetLastBlackScreenEtag()
	{
		return PlayerPrefs.GetString(KIDUI_Controller.EtagOnCloseBlackScreenPlayerPrefRef, "");
	}

	private void SaveEtagOnCloseScreen()
	{
		if (KIDManager.CurrentSession == null)
		{
			Debug.Log("[KID::MANAGER] Trying to save Pre-Game Screen ETAG, but [CurrentSession] is null");
			return;
		}
		PlayerPrefs.SetString(KIDUI_Controller.EtagOnCloseBlackScreenPlayerPrefRef, KIDManager.CurrentSession.Etag);
		PlayerPrefs.Save();
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

	private const string CLOSE_BLACK_SCREEN_ETAG_PLAYER_PREF_PREFIX = "closeBlackScreen-";

	private const string FIRST_TIME_POST_CHANGE_PLAYER_PREF = "hasShownFirstTimePostChange-";

	private static KIDUI_Controller _instance;

	[SerializeField]
	private KIDUI_MainScreen _mainKIDScreen;

	[SerializeField]
	private KIDUI_ConfirmScreen _confirmScreen;

	[SerializeField]
	private List<string> _PermissionsWithToggles = new List<string>();

	[SerializeField]
	private List<EKIDFeatures> _inaccessibleSettings = new List<EKIDFeatures>
	{
		EKIDFeatures.Multiplayer,
		EKIDFeatures.Mods
	};

	private KIDUI_Controller.Metrics_ShowReason _showReason;

	private bool _isKidUIActive;

	private static string etagOnCloseBlackScreenPlayerPrefStr;

	private string _lastEtagOnClose;

	public enum Metrics_ShowReason
	{
		None,
		Inaccessible,
		Guardian_Disabled,
		Permissions_Changed,
		Default_Session,
		No_Session
	}
}
