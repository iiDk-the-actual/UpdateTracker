using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using GorillaExtensions;
using JetBrains.Annotations;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GorillaNetworking
{
	public class PlayFabAuthenticator : MonoBehaviour
	{
		public GorillaComputer gorillaComputer
		{
			get
			{
				return GorillaComputer.instance;
			}
		}

		public bool IsReturningPlayer { get; private set; }

		public bool postAuthSetSafety { get; private set; }

		private void Awake()
		{
			if (PlayFabAuthenticator.instance == null)
			{
				PlayFabAuthenticator.instance = this;
			}
			else if (PlayFabAuthenticator.instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			if (PlayFabAuthenticator.instance.photonAuthenticator == null)
			{
				PlayFabAuthenticator.instance.photonAuthenticator = PlayFabAuthenticator.instance.gameObject.GetOrAddComponent<PhotonAuthenticator>();
			}
			this.platform = ScriptableObject.CreateInstance<PlatformTagJoin>();
			PlayFabSettings.CompressApiData = false;
			new byte[1];
			if (this.screenDebugMode)
			{
				this.debugText.text = "";
			}
			Debug.Log("doing steam thing");
			if (PlayFabAuthenticator.instance.steamAuthenticator == null)
			{
				PlayFabAuthenticator.instance.steamAuthenticator = PlayFabAuthenticator.instance.gameObject.GetOrAddComponent<SteamAuthenticator>();
			}
			this.platform.PlatformTag = "Steam";
			PlayFabSettings.TitleId = PlayFabAuthenticatorSettings.TitleId;
			PlayFabSettings.DisableFocusTimeCollection = true;
			this.BeginLoginFlow();
		}

		public void BeginLoginFlow()
		{
			if (!MothershipClientApiUnity.IsEnabled())
			{
				this.AuthenticateWithPlayFab();
				return;
			}
			if (PlayFabAuthenticator.instance.mothershipAuthenticator == null)
			{
				PlayFabAuthenticator.instance.mothershipAuthenticator = MothershipAuthenticator.Instance ?? PlayFabAuthenticator.instance.gameObject.GetOrAddComponent<MothershipAuthenticator>();
				MothershipAuthenticator mothershipAuthenticator = PlayFabAuthenticator.instance.mothershipAuthenticator;
				mothershipAuthenticator.OnLoginSuccess = (Action)Delegate.Combine(mothershipAuthenticator.OnLoginSuccess, new Action(delegate
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
				}));
				MothershipAuthenticator mothershipAuthenticator2 = PlayFabAuthenticator.instance.mothershipAuthenticator;
				mothershipAuthenticator2.OnLoginAttemptFailure = (Action<int>)Delegate.Combine(mothershipAuthenticator2.OnLoginAttemptFailure, new Action<int>(delegate(int attempts)
				{
					if (attempts == 1)
					{
						PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
					}
				}));
				PlayFabAuthenticator.instance.mothershipAuthenticator.BeginLoginFlow();
			}
		}

		private void Start()
		{
		}

		private void OnEnable()
		{
			NetworkSystem.Instance.OnCustomAuthenticationResponse += this.OnCustomAuthenticationResponse;
		}

		private void OnDisable()
		{
			NetworkSystem.Instance.OnCustomAuthenticationResponse -= this.OnCustomAuthenticationResponse;
			SteamAuthTicket steamAuthTicket = this.steamAuthTicketForPhoton;
			if (steamAuthTicket != null)
			{
				steamAuthTicket.Dispose();
			}
			SteamAuthTicket steamAuthTicket2 = this.steamAuthTicketForPlayFab;
			if (steamAuthTicket2 == null)
			{
				return;
			}
			steamAuthTicket2.Dispose();
		}

		public void RefreshSteamAuthTicketForPhoton(Action<string> successCallback, Action<EResult> failureCallback)
		{
			SteamAuthTicket steamAuthTicket = this.steamAuthTicketForPhoton;
			if (steamAuthTicket != null)
			{
				steamAuthTicket.Dispose();
			}
			this.steamAuthTicketForPhoton = this.steamAuthenticator.GetAuthTicketForWebApi(this.steamAuthIdForPhoton, successCallback, failureCallback);
		}

		private void OnCustomAuthenticationResponse(Dictionary<string, object> response)
		{
			SteamAuthTicket steamAuthTicket = this.steamAuthTicketForPhoton;
			if (steamAuthTicket != null)
			{
				steamAuthTicket.Dispose();
			}
			object obj;
			if (response.TryGetValue("SteamAuthIdForPhoton", out obj))
			{
				string text = obj as string;
				if (text != null)
				{
					this.steamAuthIdForPhoton = text;
					return;
				}
			}
			this.steamAuthIdForPhoton = null;
		}

		public void AuthenticateWithPlayFab()
		{
			Debug.Log("authenticating with playFab!");
			GorillaServer gorillaServer = GorillaServer.Instance;
			if (gorillaServer != null && gorillaServer.FeatureFlagsReady)
			{
				if (KIDManager.KidEnabled)
				{
					Debug.Log("[KID] Is Enabled - Enabling safeties by platform and age category");
					this.DefaultSafetiesByAgeCategory();
				}
			}
			else
			{
				this.postAuthSetSafety = true;
			}
			if (SteamManager.Initialized)
			{
				this.userID = SteamUser.GetSteamID().ToString();
				Debug.Log("trying to auth with steam");
				this.steamAuthTicketForPlayFab = this.steamAuthenticator.GetAuthTicket(delegate(string ticket)
				{
					Debug.Log("Got steam auth session ticket!");
					PlayFabClientAPI.LoginWithSteam(new LoginWithSteamRequest
					{
						CreateAccount = new bool?(true),
						SteamTicket = ticket
					}, new Action<LoginResult>(this.OnLoginWithSteamResponse), new Action<PlayFabError>(this.OnPlayFabError), null, null);
				}, delegate(EResult result)
				{
					base.StartCoroutine(this.DisplayGeneralFailureMessageOnGorillaComputerAfter1Frame());
				});
				return;
			}
			base.StartCoroutine(this.DisplayGeneralFailureMessageOnGorillaComputerAfter1Frame());
		}

		private IEnumerator VerifyKidAuthenticated(DateTime accountCreationDateTime)
		{
			Task<DateTime?> getNewPlayerDateTimeTask = KIDManager.CheckKIDNewPlayerDateTime();
			yield return new WaitUntil(() => getNewPlayerDateTimeTask.IsCompleted);
			DateTime? result = getNewPlayerDateTimeTask.Result;
			if (result != null && KIDManager.KidEnabled)
			{
				this.IsReturningPlayer = accountCreationDateTime < result;
			}
			yield break;
		}

		private IEnumerator DisplayGeneralFailureMessageOnGorillaComputerAfter1Frame()
		{
			yield return null;
			if (this.gorillaComputer != null)
			{
				this.gorillaComputer.GeneralFailureMessage("UNABLE TO AUTHENTICATE YOUR STEAM ACCOUNT! PLEASE MAKE SURE STEAM IS RUNNING AND YOU ARE LAUNCHING THE GAME DIRECTLY FROM STEAM.");
				this.gorillaComputer.screenText.Text = "UNABLE TO AUTHENTICATE YOUR STEAM ACCOUNT! PLEASE MAKE SURE STEAM IS RUNNING AND YOU ARE LAUNCHING THE GAME DIRECTLY FROM STEAM.";
				Debug.Log("Couldn't authenticate steam account");
			}
			else
			{
				Debug.LogError("PlayFabAuthenticator: gorillaComputer is null, so could not set GeneralFailureMessage notifying user that the steam account could not be authenticated.", this);
			}
			yield break;
		}

		private void OnLoginWithSteamResponse(LoginResult obj)
		{
			this._playFabPlayerIdCache = obj.PlayFabId;
			this._sessionTicket = obj.SessionTicket;
			base.StartCoroutine(this.CachePlayFabId(new PlayFabAuthenticator.CachePlayFabIdRequest
			{
				Platform = this.platform.ToString(),
				SessionTicket = this._sessionTicket,
				PlayFabId = this._playFabPlayerIdCache,
				TitleId = PlayFabSettings.TitleId,
				MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
				MothershipToken = MothershipClientContext.Token,
				MothershipId = MothershipClientContext.MothershipId
			}, new Action<PlayFabAuthenticator.CachePlayFabIdResponse>(this.OnCachePlayFabIdRequest)));
		}

		private void OnCachePlayFabIdRequest([CanBeNull] PlayFabAuthenticator.CachePlayFabIdResponse response)
		{
			if (response != null)
			{
				this.steamAuthIdForPhoton = response.SteamAuthIdForPhoton;
				DateTime dateTime;
				if (DateTime.TryParse(response.AccountCreationIsoTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime))
				{
					base.StartCoroutine(this.VerifyKidAuthenticated(dateTime));
				}
				Debug.Log("Successfully cached PlayFab Id.  Continuing!");
				this.AdvanceLogin();
				return;
			}
			Debug.LogError("Could not cache PlayFab Id.  Cannot continue.");
		}

		private void AdvanceLogin()
		{
			this.LogMessage("PlayFab authenticated ... Getting Nonce");
			this.RefreshSteamAuthTicketForPhoton(delegate(string ticket)
			{
				this._nonce = ticket;
				Debug.Log("Got nonce!  Authenticating...");
				this.AuthenticateWithPhoton();
			}, delegate(EResult result)
			{
				Debug.LogWarning("Failed to get nonce!");
				this.AuthenticateWithPhoton();
			});
		}

		private void AuthenticateWithPhoton()
		{
			this.photonAuthenticator.SetCustomAuthenticationParameters(new Dictionary<string, object>
			{
				{
					"AppId",
					PlayFabSettings.TitleId
				},
				{
					"AppVersion",
					NetworkSystemConfig.AppVersion ?? "-1"
				},
				{ "Ticket", this._sessionTicket },
				{ "Nonce", this._nonce },
				{
					"MothershipEnvId",
					MothershipClientApiUnity.EnvironmentId
				},
				{
					"MothershipToken",
					MothershipClientContext.Token
				}
			});
			this.GetPlayerDisplayName(this._playFabPlayerIdCache);
			GorillaServer.Instance.AddOrRemoveDLCOwnership(delegate(ExecuteFunctionResult result)
			{
				Debug.Log("got results! updating!");
				if (GorillaTagger.Instance != null)
				{
					GorillaTagger.Instance.offlineVRRig.GetCosmeticsPlayFabCatalogData();
				}
			}, delegate(PlayFabError error)
			{
				Debug.Log("Got error retrieving user data:");
				Debug.Log(error.GenerateErrorReport());
				if (GorillaTagger.Instance != null)
				{
					GorillaTagger.Instance.offlineVRRig.GetCosmeticsPlayFabCatalogData();
				}
			});
			if (CosmeticsController.instance != null)
			{
				Debug.Log("initializing cosmetics");
				CosmeticsController.instance.Initialize();
			}
			if (this.gorillaComputer != null)
			{
				this.gorillaComputer.OnConnectedToMasterStuff();
			}
			else
			{
				base.StartCoroutine(this.ComputerOnConnectedToMaster());
			}
			if (RankedProgressionManager.Instance != null)
			{
				RankedProgressionManager.Instance.LoadStats();
			}
			if (PhotonNetworkController.Instance != null)
			{
				Debug.Log("Finish authenticating");
				NetworkSystem.Instance.FinishAuthenticating();
			}
		}

		private IEnumerator ComputerOnConnectedToMaster()
		{
			WaitForEndOfFrame frameYield = new WaitForEndOfFrame();
			while (this.gorillaComputer == null)
			{
				yield return frameYield;
			}
			this.gorillaComputer.OnConnectedToMasterStuff();
			yield break;
		}

		private void OnPlayFabError(PlayFabError obj)
		{
			this.LogMessage(obj.ErrorMessage);
			Debug.Log("OnPlayFabError(): " + obj.ErrorMessage);
			this.loginFailed = true;
			if (obj.ErrorMessage == "The account making this request is currently banned")
			{
				using (Dictionary<string, List<string>>.Enumerator enumerator = obj.ErrorDetails.GetEnumerator())
				{
					if (!enumerator.MoveNext())
					{
						return;
					}
					KeyValuePair<string, List<string>> keyValuePair = enumerator.Current;
					if (keyValuePair.Value[0] != "Indefinite")
					{
						this.gorillaComputer.GeneralFailureMessage("YOUR ACCOUNT HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + keyValuePair.Key + "\nHOURS LEFT: " + ((int)((DateTime.Parse(keyValuePair.Value[0]) - DateTime.UtcNow).TotalHours + 1.0)).ToString());
						return;
					}
					this.gorillaComputer.GeneralFailureMessage("YOUR ACCOUNT HAS BEEN BANNED INDEFINITELY.\nREASON: " + keyValuePair.Key);
					return;
				}
			}
			if (obj.ErrorMessage == "The IP making this request is currently banned")
			{
				using (Dictionary<string, List<string>>.Enumerator enumerator = obj.ErrorDetails.GetEnumerator())
				{
					if (!enumerator.MoveNext())
					{
						return;
					}
					KeyValuePair<string, List<string>> keyValuePair2 = enumerator.Current;
					if (keyValuePair2.Value[0] != "Indefinite")
					{
						this.gorillaComputer.GeneralFailureMessage("THIS IP HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + keyValuePair2.Key + "\nHOURS LEFT: " + ((int)((DateTime.Parse(keyValuePair2.Value[0]) - DateTime.UtcNow).TotalHours + 1.0)).ToString());
						return;
					}
					this.gorillaComputer.GeneralFailureMessage("THIS IP HAS BEEN BANNED INDEFINITELY.\nREASON: " + keyValuePair2.Key);
					return;
				}
			}
			if (this.gorillaComputer != null)
			{
				this.gorillaComputer.GeneralFailureMessage(this.gorillaComputer.unableToConnect);
			}
		}

		private void LogMessage(string message)
		{
		}

		private void GetPlayerDisplayName(string playFabId)
		{
			GetPlayerProfileRequest getPlayerProfileRequest = new GetPlayerProfileRequest();
			getPlayerProfileRequest.PlayFabId = playFabId;
			getPlayerProfileRequest.ProfileConstraints = new PlayerProfileViewConstraints
			{
				ShowDisplayName = true
			};
			PlayFabClientAPI.GetPlayerProfile(getPlayerProfileRequest, delegate(GetPlayerProfileResult result)
			{
				this._displayName = result.PlayerProfile.DisplayName;
			}, delegate(PlayFabError error)
			{
				Debug.LogError(error.GenerateErrorReport());
			}, null, null);
		}

		public void SetDisplayName(string playerName)
		{
			if (this._displayName == null || (this._displayName.Length > 4 && this._displayName.Substring(0, this._displayName.Length - 4) != playerName))
			{
				UpdateUserTitleDisplayNameRequest updateUserTitleDisplayNameRequest = new UpdateUserTitleDisplayNameRequest();
				updateUserTitleDisplayNameRequest.DisplayName = playerName;
				PlayFabClientAPI.UpdateUserTitleDisplayName(updateUserTitleDisplayNameRequest, delegate(UpdateUserTitleDisplayNameResult result)
				{
					this._displayName = playerName;
				}, delegate(PlayFabError error)
				{
					Debug.LogError(error.GenerateErrorReport());
				}, null, null);
			}
		}

		public void ScreenDebug(string debugString)
		{
			Debug.Log(debugString);
			if (this.screenDebugMode)
			{
				Text text = this.debugText;
				text.text = text.text + debugString + "\n";
			}
		}

		public void ScreenDebugClear()
		{
			this.debugText.text = "";
		}

		public IEnumerator PlayfabAuthenticate(PlayFabAuthenticator.PlayfabAuthRequestData data, Action<PlayFabAuthenticator.PlayfabAuthResponseData> callback)
		{
			UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.AuthApiBaseUrl + "/api/PlayFabAuthentication", "POST");
			byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
			bool retry = false;
			request.uploadHandler = new UploadHandlerRaw(bytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			request.timeout = 15;
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
			{
				PlayFabAuthenticator.PlayfabAuthResponseData playfabAuthResponseData = JsonUtility.FromJson<PlayFabAuthenticator.PlayfabAuthResponseData>(request.downloadHandler.text);
				callback(playfabAuthResponseData);
			}
			else
			{
				if (request.responseCode == 403L)
				{
					Debug.LogError(string.Format("HTTP {0}: {1}, with body: {2}", request.responseCode, request.error, request.downloadHandler.text));
					PlayFabAuthenticator.BanInfo banInfo = JsonUtility.FromJson<PlayFabAuthenticator.BanInfo>(request.downloadHandler.text);
					this.ShowBanMessage(banInfo);
					callback(null);
				}
				if (request.result == UnityWebRequest.Result.ProtocolError && request.responseCode != 400L)
				{
					retry = true;
					Debug.LogError(string.Format("HTTP {0} error: {1} message:{2}", request.responseCode, request.error, request.downloadHandler.text));
				}
				else if (request.result == UnityWebRequest.Result.ConnectionError)
				{
					retry = true;
					Debug.LogError("NETWORK ERROR: " + request.error + "\nMessage: " + request.downloadHandler.text);
				}
				else
				{
					Debug.LogError("HTTP ERROR: " + request.error + "\nMessage: " + request.downloadHandler.text);
					retry = true;
				}
			}
			if (retry)
			{
				if (this.playFabAuthRetryCount < this.playFabMaxRetries)
				{
					int num = (int)Mathf.Pow(2f, (float)(this.playFabAuthRetryCount + 1));
					Debug.LogWarning(string.Format("Retrying PlayFab auth... Retry attempt #{0}, waiting for {1} seconds", this.playFabAuthRetryCount + 1, num));
					this.playFabAuthRetryCount++;
					yield return new WaitForSeconds((float)num);
				}
				else
				{
					Debug.LogError("Maximum retries attempted. Please check your network connection.");
					callback(null);
					this.ShowPlayFabAuthErrorMessage(request.downloadHandler.text);
				}
			}
			yield break;
		}

		private void ShowPlayFabAuthErrorMessage(string errorJson)
		{
			try
			{
				PlayFabAuthenticator.ErrorInfo errorInfo = JsonUtility.FromJson<PlayFabAuthenticator.ErrorInfo>(errorJson);
				this.gorillaComputer.GeneralFailureMessage("UNABLE TO AUTHENTICATE WITH PLAYFAB.\nREASON: " + errorInfo.Message);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("Failed to show PlayFab auth error message: {0}", ex));
			}
		}

		private void ShowBanMessage(PlayFabAuthenticator.BanInfo banInfo)
		{
			try
			{
				if (banInfo.BanExpirationTime != null && banInfo.BanMessage != null)
				{
					if (banInfo.BanExpirationTime != "Indefinite")
					{
						this.gorillaComputer.GeneralFailureMessage("YOUR ACCOUNT HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + banInfo.BanMessage + "\nHOURS LEFT: " + ((int)((DateTime.Parse(banInfo.BanExpirationTime) - DateTime.UtcNow).TotalHours + 1.0)).ToString());
					}
					else
					{
						this.gorillaComputer.GeneralFailureMessage("YOUR ACCOUNT HAS BEEN BANNED INDEFINITELY.\nREASON: " + banInfo.BanMessage);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("Failed to show ban message: {0}", ex));
			}
		}

		public IEnumerator CachePlayFabId(PlayFabAuthenticator.CachePlayFabIdRequest data, Action<PlayFabAuthenticator.CachePlayFabIdResponse> callback)
		{
			Debug.Log("Trying to cache playfab Id");
			UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.AuthApiBaseUrl + "/api/CachePlayFabId", "POST");
			byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
			bool retry = false;
			request.uploadHandler = new UploadHandlerRaw(bytes);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			request.timeout = 15;
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
			{
				if (request.responseCode == 200L)
				{
					PlayFabAuthenticator.CachePlayFabIdResponse cachePlayFabIdResponse = JsonUtility.FromJson<PlayFabAuthenticator.CachePlayFabIdResponse>(request.downloadHandler.text);
					callback(cachePlayFabIdResponse);
				}
			}
			else if (request.result == UnityWebRequest.Result.ProtocolError && request.responseCode != 400L)
			{
				retry = true;
				Debug.LogError(string.Format("HTTP {0} error: {1}", request.responseCode, request.error));
			}
			else
			{
				retry = request.result != UnityWebRequest.Result.ConnectionError || true;
			}
			if (retry)
			{
				if (this.playFabCacheRetryCount < this.playFabCacheMaxRetries)
				{
					int num = (int)Mathf.Pow(2f, (float)(this.playFabCacheRetryCount + 1));
					Debug.LogWarning(string.Format("Retrying PlayFab auth... Retry attempt #{0}, waiting for {1} seconds", this.playFabCacheRetryCount + 1, num));
					this.playFabCacheRetryCount++;
					yield return new WaitForSeconds((float)num);
					base.StartCoroutine(this.CachePlayFabId(new PlayFabAuthenticator.CachePlayFabIdRequest
					{
						Platform = this.platform.ToString(),
						SessionTicket = this._sessionTicket,
						PlayFabId = this._playFabPlayerIdCache,
						TitleId = PlayFabSettings.TitleId,
						MothershipEnvId = MothershipClientApiUnity.EnvironmentId,
						MothershipToken = MothershipClientContext.Token,
						MothershipId = MothershipClientContext.MothershipId
					}, new Action<PlayFabAuthenticator.CachePlayFabIdResponse>(this.OnCachePlayFabIdRequest)));
				}
				else
				{
					Debug.LogError("Maximum retries attempted. Please check your network connection.");
					callback(null);
					this.ShowPlayFabAuthErrorMessage(request.downloadHandler.text);
				}
			}
			yield break;
		}

		public void DefaultSafetiesByAgeCategory()
		{
			Debug.Log("[KID::PLAYFAB_AUTHENTICATOR] Defaulting Safety Settings to Disabled because age category data unavailable on this platform");
			this.SetSafety(false, true, false);
		}

		public void SetSafety(bool isSafety, bool isAutoSet, bool setPlayfab = false)
		{
			this.postAuthSetSafety = false;
			Action<bool> onSafetyUpdate = this.OnSafetyUpdate;
			if (onSafetyUpdate != null)
			{
				onSafetyUpdate(isSafety);
			}
			Debug.Log("[KID] Setting safety to: [" + isSafety.ToString() + "]");
			this.isSafeAccount = isSafety;
			this.safetyType = PlayFabAuthenticator.SafetyType.None;
			if (!isSafety)
			{
				if (isAutoSet)
				{
					PlayerPrefs.SetInt("autoSafety", 0);
				}
				else
				{
					PlayerPrefs.SetInt("optSafety", 0);
				}
				PlayerPrefs.Save();
				return;
			}
			if (isAutoSet)
			{
				PlayerPrefs.SetInt("autoSafety", 1);
				this.safetyType = PlayFabAuthenticator.SafetyType.Auto;
				return;
			}
			PlayerPrefs.SetInt("optSafety", 1);
			this.safetyType = PlayFabAuthenticator.SafetyType.OptIn;
		}

		public string GetPlayFabSessionTicket()
		{
			return this._sessionTicket;
		}

		public string GetPlayFabPlayerId()
		{
			return this._playFabPlayerIdCache;
		}

		public bool GetSafety()
		{
			return this.isSafeAccount;
		}

		public PlayFabAuthenticator.SafetyType GetSafetyType()
		{
			return this.safetyType;
		}

		public string GetUserID()
		{
			return this.userID;
		}

		public static volatile PlayFabAuthenticator instance;

		private string _playFabPlayerIdCache;

		private string _sessionTicket;

		private string _displayName;

		private string _nonce;

		public string userID;

		private string userToken;

		public PlatformTagJoin platform;

		private bool isSafeAccount;

		public Action<bool> OnSafetyUpdate;

		private PlayFabAuthenticator.SafetyType safetyType;

		private byte[] m_Ticket;

		private uint m_pcbTicket;

		public Text debugText;

		public bool screenDebugMode;

		public bool loginFailed;

		[FormerlySerializedAs("loginDisplayID")]
		public GameObject emptyObject;

		private int playFabAuthRetryCount;

		private int playFabMaxRetries = 5;

		private int playFabCacheRetryCount;

		private int playFabCacheMaxRetries = 5;

		public MetaAuthenticator metaAuthenticator;

		public SteamAuthenticator steamAuthenticator;

		public MothershipAuthenticator mothershipAuthenticator;

		public PhotonAuthenticator photonAuthenticator;

		[SerializeField]
		private bool dbg_isReturningPlayer;

		private SteamAuthTicket steamAuthTicketForPlayFab;

		private SteamAuthTicket steamAuthTicketForPhoton;

		private string steamAuthIdForPhoton;

		public enum SafetyType
		{
			None,
			Auto,
			OptIn
		}

		[Serializable]
		public class CachePlayFabIdRequest
		{
			public string Platform;

			public string SessionTicket;

			public string PlayFabId;

			public string TitleId;

			public string MothershipEnvId;

			public string MothershipToken;

			public string MothershipId;
		}

		[Serializable]
		public class PlayfabAuthRequestData
		{
			public string AppId;

			public string Nonce;

			public string OculusId;

			public string Platform;

			public string AgeCategory;

			public string MothershipEnvId;

			public string MothershipToken;

			public string MothershipId;
		}

		[Serializable]
		public class PlayfabAuthResponseData
		{
			public string SessionTicket;

			public string EntityToken;

			public string PlayFabId;

			public string EntityId;

			public string EntityType;

			public string AccountCreationIsoTimestamp;
		}

		[Serializable]
		public class CachePlayFabIdResponse
		{
			public string PlayFabId;

			public string SteamAuthIdForPhoton;

			public string AccountCreationIsoTimestamp;
		}

		private class ErrorInfo
		{
			public string Message;

			public string Error;
		}

		private class BanInfo
		{
			public string BanMessage;

			public string BanExpirationTime;
		}
	}
}
