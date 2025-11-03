using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GorillaNetworking;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

public class GorillaTagCompetitiveServerApi : MonoBehaviour
{
	private void Awake()
	{
		if (GorillaTagCompetitiveServerApi.Instance)
		{
			GTDev.LogError<string>("Duplicate GorillaTagCompetitiveServerApi detected. Destroying self.", base.gameObject, null);
			Object.Destroy(this);
			return;
		}
		GorillaTagCompetitiveServerApi.Instance = this;
	}

	public void RequestGetRankInformation(List<string> playfabs, Action<GorillaTagCompetitiveServerApi.RankedModeProgressionData> callback)
	{
		if (!MothershipClientContext.IsClientLoggedIn())
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestGetRankInformation Client Not Logged into Mothership", null);
			return;
		}
		if (this.GetRankInformationInProgress)
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestGetRankInformation already in progress", null);
			return;
		}
		this.GetRankInformationInProgress = true;
		string text = "PC";
		base.StartCoroutine(this.GetRankInformation(new GorillaTagCompetitiveServerApi.RankedModeProgressionRequestData
		{
			mothershipId = MothershipClientContext.MothershipId,
			mothershipToken = MothershipClientContext.Token,
			mothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			platform = text,
			playfabIds = playfabs
		}, callback));
	}

	private IEnumerator GetRankInformation(GorillaTagCompetitiveServerApi.RankedModeProgressionRequestData data, Action<GorillaTagCompetitiveServerApi.RankedModeProgressionData> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.MmrApiBaseUrl + "/api/GetTier", "GET");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			GTDev.Log<string>("GetRankInformation Success: raw response: " + request.downloadHandler.text, null);
			this.OnCompleteGetRankInformation(request.downloadHandler.text, callback);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode > 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
			else
			{
				this.OnCompleteGetRankInformation(null, callback);
			}
		}
		if (retry)
		{
			if (this.GetRankInformationRetryCount < this.MAX_SERVER_RETRIES)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.GetRankInformationRetryCount + 1));
				this.GetRankInformationRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.GetRankInformationInProgress = false;
				this.RequestGetRankInformation(data.playfabIds, callback);
			}
			else
			{
				this.GetRankInformationRetryCount = 0;
				this.OnCompleteGetRankInformation(null, callback);
			}
		}
		yield break;
	}

	private void OnCompleteGetRankInformation([CanBeNull] string response, Action<GorillaTagCompetitiveServerApi.RankedModeProgressionData> callback)
	{
		this.GetRankInformationInProgress = false;
		this.GetRankInformationRetryCount = 0;
		if (response.IsNullOrEmpty())
		{
			return;
		}
		string text = "{ \"playerData\": " + response + " }";
		GorillaTagCompetitiveServerApi.RankedModeProgressionData rankedModeProgressionData;
		try
		{
			rankedModeProgressionData = JsonUtility.FromJson<GorillaTagCompetitiveServerApi.RankedModeProgressionData>(text);
		}
		catch (ArgumentException ex)
		{
			Debug.LogException(ex);
			Debug.LogError("[GT/GorillaTagCompetitiveServerApi]  ERROR!!!  OnCompleteGetRankInformation: Encountered ArgumentException above while trying to parse json string:\n" + text);
			return;
		}
		catch (Exception ex2)
		{
			Debug.LogException(ex2);
			Debug.LogError("[GT/GorillaTagCompetitiveServerApi]  ERROR!!!  OnCompleteGetRankInformation: Encountered exception above while trying to parse json string:\n" + text);
			return;
		}
		if (callback != null)
		{
			callback(rankedModeProgressionData);
		}
	}

	public void RequestCreateMatchId(Action<string> callback)
	{
		if (!MothershipClientContext.IsClientLoggedIn())
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestCreateMatchId Client Not Logged into Mothership", null);
			return;
		}
		if (this.CreateMatchIdInProgress)
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestCreateMatchId already in progress", null);
			return;
		}
		string text = "PC";
		this.CreateMatchIdInProgress = true;
		base.StartCoroutine(this.CreateMatchId(new GorillaTagCompetitiveServerApi.RankedModeRequestDataPlatformed
		{
			mothershipId = MothershipClientContext.MothershipId,
			mothershipToken = MothershipClientContext.Token,
			mothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			platform = text
		}, callback));
	}

	private IEnumerator CreateMatchId(GorillaTagCompetitiveServerApi.RankedModeRequestDataPlatformed data, Action<string> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.MmrApiBaseUrl + "/api/CreateMatchId", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			GTDev.Log<string>("CreateMatchId Success: raw response: " + request.downloadHandler.text, null);
			this.OnCompleteCreateMatchId(request.downloadHandler.text, callback);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode > 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
			else
			{
				this.OnCompleteCreateMatchId(request.downloadHandler.text, callback);
			}
		}
		if (retry)
		{
			if (this.CreateMatchIdRetryCount < this.MAX_SERVER_RETRIES)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.CreateMatchIdRetryCount + 1));
				this.CreateMatchIdRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.CreateMatchIdInProgress = false;
				this.RequestCreateMatchId(callback);
			}
			else
			{
				this.CreateMatchIdRetryCount = 0;
				this.OnCompleteCreateMatchId(null, callback);
			}
		}
		yield break;
	}

	private void OnCompleteCreateMatchId([CanBeNull] string response, Action<string> callback)
	{
		this.CreateMatchIdInProgress = false;
		this.CreateMatchIdRetryCount = 0;
		if (response.IsNullOrEmpty())
		{
			return;
		}
		if (callback != null)
		{
			callback(response);
		}
	}

	public void RequestValidateMatchJoin(string matchId, Action<bool> callback)
	{
		if (!MothershipClientContext.IsClientLoggedIn())
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestValidateMatchJoin Client Not Logged into Mothership", null);
			return;
		}
		if (this.ValidateMatchJoinInProgress)
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestValidateMatchJoin already in progress", null);
			return;
		}
		string text = "PC";
		this.ValidateMatchJoinInProgress = true;
		base.StartCoroutine(this.ValidateMatchJoin(new GorillaTagCompetitiveServerApi.RankedModeRequestDataWithMatchId
		{
			mothershipId = MothershipClientContext.MothershipId,
			mothershipToken = MothershipClientContext.Token,
			mothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			platform = text,
			matchId = matchId
		}, callback));
	}

	private IEnumerator ValidateMatchJoin(GorillaTagCompetitiveServerApi.RankedModeRequestDataWithMatchId data, Action<bool> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.MmrApiBaseUrl + "/api/ValidateMatchJoin", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			GTDev.Log<string>("ValidateMatchJoin Success: raw response: " + request.downloadHandler.text, null);
			this.OnCompleteValidateMatchJoin(request.downloadHandler.text, callback);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode > 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
			else
			{
				this.OnCompleteValidateMatchJoin(request.downloadHandler.text, callback);
			}
		}
		if (retry)
		{
			if (this.ValidateMatchJoinRetryCount < this.MAX_SERVER_RETRIES)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.ValidateMatchJoinRetryCount + 1));
				this.ValidateMatchJoinRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.ValidateMatchJoinInProgress = false;
				this.RequestValidateMatchJoin(data.matchId, callback);
			}
			else
			{
				this.ValidateMatchJoinRetryCount = 0;
				this.OnCompleteValidateMatchJoin(null, callback);
			}
		}
		yield break;
	}

	private void OnCompleteValidateMatchJoin([CanBeNull] string response, Action<bool> callback)
	{
		this.ValidateMatchJoinInProgress = false;
		this.ValidateMatchJoinRetryCount = 0;
		if (response.IsNullOrEmpty())
		{
			return;
		}
		GorillaTagCompetitiveServerApi.RankedModeValidateMatchJoinResponseData rankedModeValidateMatchJoinResponseData = JsonUtility.FromJson<GorillaTagCompetitiveServerApi.RankedModeValidateMatchJoinResponseData>(response);
		if (callback != null)
		{
			callback(rankedModeValidateMatchJoinResponseData.validJoin);
		}
	}

	public void RequestSubmitMatchScores(string matchId, List<RankedMultiplayerScore.PlayerScore> finalScores)
	{
		List<GorillaTagCompetitiveServerApi.RankedModePlayerScore> list = new List<GorillaTagCompetitiveServerApi.RankedModePlayerScore>();
		foreach (RankedMultiplayerScore.PlayerScore playerScore in finalScores)
		{
			NetPlayer player = NetworkSystem.Instance.GetPlayer(playerScore.PlayerId);
			list.Add(new GorillaTagCompetitiveServerApi.RankedModePlayerScore
			{
				playfabId = player.UserId,
				gameScore = playerScore.GameScore
			});
		}
		this.RequestSubmitMatchScores(matchId, list);
	}

	private void RequestSubmitMatchScores(string matchId, List<GorillaTagCompetitiveServerApi.RankedModePlayerScore> playerScores)
	{
		if (!MothershipClientContext.IsClientLoggedIn())
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestSubmitMatchScores Client Not Logged into Mothership", null);
			return;
		}
		if (this.SubmitMatchScoresInProgress)
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestSubmitMatchScores already in progress", null);
			return;
		}
		this.SubmitMatchScoresInProgress = true;
		base.StartCoroutine(this.SubmitMatchScores(new GorillaTagCompetitiveServerApi.RankedModeSubmitMatchScoresRequestData
		{
			mothershipId = MothershipClientContext.MothershipId,
			mothershipToken = MothershipClientContext.Token,
			mothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			matchId = matchId,
			playfabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
			playerScores = playerScores
		}));
	}

	private IEnumerator SubmitMatchScores(GorillaTagCompetitiveServerApi.RankedModeSubmitMatchScoresRequestData data)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.MmrApiBaseUrl + "/api/SubmitMatchScores", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			GTDev.Log<string>("SubmitMatchScores Success: raw response: " + request.downloadHandler.text, null);
			this.OnCompleteSubmitMatchScores(request.downloadHandler.text);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode > 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
			else
			{
				this.OnCompleteSubmitMatchScores(request.downloadHandler.text);
			}
		}
		if (retry)
		{
			if (this.SubmitMatchScoresRetryCount < this.MAX_SERVER_RETRIES)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.SubmitMatchScoresRetryCount + 1));
				this.SubmitMatchScoresRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.SubmitMatchScoresInProgress = false;
				this.RequestSubmitMatchScores(data.matchId, data.playerScores);
			}
			else
			{
				this.SubmitMatchScoresRetryCount = 0;
				this.OnCompleteSubmitMatchScores(null);
			}
		}
		yield break;
	}

	private void OnCompleteSubmitMatchScores([CanBeNull] string response)
	{
		this.SubmitMatchScoresInProgress = false;
		this.SubmitMatchScoresRetryCount = 0;
	}

	public void RequestSetEloValue(float desiredElo, Action callback)
	{
		if (!MothershipClientContext.IsClientLoggedIn())
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestSetEloValue Client Not Logged into Mothership", null);
			return;
		}
		if (this.SetEloValueInProgress)
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestSetEloValue already in progress", null);
			return;
		}
		string text = "PC";
		this.SetEloValueInProgress = true;
		base.StartCoroutine(this.SetEloValue(new GorillaTagCompetitiveServerApi.RankedModeSetEloValueRequestData
		{
			mothershipId = MothershipClientContext.MothershipId,
			mothershipToken = MothershipClientContext.Token,
			mothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			platform = text,
			elo = desiredElo
		}, callback));
	}

	private IEnumerator SetEloValue(GorillaTagCompetitiveServerApi.RankedModeSetEloValueRequestData data, Action callback)
	{
		GTDev.LogWarning<string>("SetEloValue is for internal use only (Is Beta)", null);
		yield break;
	}

	private void OnCompleteSetEloValue([CanBeNull] string response, Action callback)
	{
		this.SetEloValueInProgress = false;
		this.SetEloValueRetryCount = 0;
		if (response != null && callback != null)
		{
			callback();
		}
	}

	public void RequestPingRoom(string matchId, Action callback)
	{
		if (!MothershipClientContext.IsClientLoggedIn())
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestPingRoom Client Not Logged into Mothership", null);
			return;
		}
		if (this.SetEloValueInProgress)
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestPingRoom already in progress", null);
			return;
		}
		string text = "PC";
		this.PingMatchInProgress = true;
		base.StartCoroutine(this.PingRoom(new GorillaTagCompetitiveServerApi.RankedModeRequestDataWithMatchId
		{
			mothershipId = MothershipClientContext.MothershipId,
			mothershipToken = MothershipClientContext.Token,
			mothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			platform = text,
			matchId = matchId
		}, callback));
	}

	private IEnumerator PingRoom(GorillaTagCompetitiveServerApi.RankedModeRequestDataWithMatchId data, Action callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.MmrApiBaseUrl + "/api/PingRoom", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			GTDev.Log<string>("PingRoom Success: raw response: " + request.downloadHandler.text, null);
			this.OnCompletePingRoom(request.downloadHandler.text, callback);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode > 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
			else
			{
				this.OnCompletePingRoom(request.downloadHandler.text, callback);
			}
		}
		if (retry)
		{
			if (this.PingMatchRetryCount < this.MAX_SERVER_RETRIES)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.PingMatchRetryCount + 1));
				this.ValidateMatchJoinRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.PingMatchInProgress = false;
				this.RequestPingRoom(data.matchId, callback);
			}
			else
			{
				this.PingMatchRetryCount = 0;
				this.OnCompletePingRoom(null, callback);
			}
		}
		yield break;
	}

	private void OnCompletePingRoom([CanBeNull] string response, Action callback)
	{
		GTDev.Log<string>("PingRoom complete", null);
		this.PingMatchInProgress = false;
		this.PingMatchRetryCount = 0;
		if (response != null && callback != null)
		{
			callback();
		}
	}

	public void RequestUnlockCompetitiveQueue(bool unlocked, Action callback)
	{
		if (!MothershipClientContext.IsClientLoggedIn())
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestUnlockCompetitiveQueue Client Not Logged into Mothership", null);
			return;
		}
		if (this.UnlockCompetitiveQueueInProgress)
		{
			GTDev.LogWarning<string>("GorillaTagCompetitiveServerApi RequestUnlockCompetitiveQueue already in progress", null);
			return;
		}
		string text = "PC";
		this.UnlockCompetitiveQueueInProgress = true;
		base.StartCoroutine(this.UnlockCompetitiveQueue(new GorillaTagCompetitiveServerApi.RankedModeUnlockCompetitiveQueueRequestData
		{
			mothershipId = MothershipClientContext.MothershipId,
			mothershipToken = MothershipClientContext.Token,
			mothershipEnvId = MothershipClientApiUnity.EnvironmentId,
			platform = text,
			unlocked = unlocked
		}, callback));
	}

	private IEnumerator UnlockCompetitiveQueue(GorillaTagCompetitiveServerApi.RankedModeUnlockCompetitiveQueueRequestData data, Action callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.MmrApiBaseUrl + "/api/UnlockCompetitiveQueue", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			GTDev.Log<string>("UnlockCompetitiveQueue Success: raw response: " + request.downloadHandler.text, null);
			this.OnCompleteUnlockCompetitiveQueue(request.downloadHandler.text, callback);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode > 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
			else
			{
				this.OnCompleteUnlockCompetitiveQueue(request.downloadHandler.text, callback);
			}
		}
		if (retry)
		{
			if (this.UnlockCompetitiveQueueRetryCount < this.MAX_SERVER_RETRIES)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.UnlockCompetitiveQueueRetryCount + 1));
				this.ValidateMatchJoinRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.UnlockCompetitiveQueueInProgress = false;
				this.RequestUnlockCompetitiveQueue(data.unlocked, callback);
			}
			else
			{
				this.UnlockCompetitiveQueueRetryCount = 0;
				this.OnCompleteUnlockCompetitiveQueue(null, callback);
			}
		}
		yield break;
	}

	private void OnCompleteUnlockCompetitiveQueue([CanBeNull] string response, Action callback)
	{
		GTDev.Log<string>("UnlockCompetitiveQueue complete", null);
		this.UnlockCompetitiveQueueInProgress = false;
		this.UnlockCompetitiveQueueRetryCount = 0;
		if (response != null && callback != null)
		{
			callback();
		}
	}

	public static GorillaTagCompetitiveServerApi Instance;

	public int MAX_SERVER_RETRIES = 3;

	private bool GetRankInformationInProgress;

	private int GetRankInformationRetryCount;

	private bool CreateMatchIdInProgress;

	private int CreateMatchIdRetryCount;

	private bool ValidateMatchJoinInProgress;

	private int ValidateMatchJoinRetryCount;

	private bool SubmitMatchScoresInProgress;

	private int SubmitMatchScoresRetryCount;

	private bool SetEloValueInProgress;

	private int SetEloValueRetryCount;

	private bool PingMatchInProgress;

	private int PingMatchRetryCount;

	private bool UnlockCompetitiveQueueInProgress;

	private int UnlockCompetitiveQueueRetryCount;

	public enum EPlatformType
	{
		PC,
		Quest,
		NumPlatforms
	}

	[Serializable]
	public class RankedModeRequestDataBase
	{
		public string mothershipId;

		public string mothershipToken;

		public string mothershipEnvId;
	}

	[Serializable]
	public class RankedModeRequestDataPlatformed : GorillaTagCompetitiveServerApi.RankedModeRequestDataBase
	{
		public string platform;
	}

	[Serializable]
	public class RankedModeProgressionRequestData : GorillaTagCompetitiveServerApi.RankedModeRequestDataPlatformed
	{
		public List<string> playfabIds;
	}

	[Serializable]
	public class RankedModeProgressionPlatformData
	{
		public string platform;

		public float elo;

		public int majorTier;

		public int minorTier;

		public float rankProgress;
	}

	[Serializable]
	public class RankedModePlayerProgressionData
	{
		public string playfabID;

		public GorillaTagCompetitiveServerApi.RankedModeProgressionPlatformData[] platformData = new GorillaTagCompetitiveServerApi.RankedModeProgressionPlatformData[2];
	}

	[Serializable]
	public class RankedModeProgressionData
	{
		public List<GorillaTagCompetitiveServerApi.RankedModePlayerProgressionData> playerData;
	}

	[Serializable]
	public class RankedModeRequestDataWithMatchId : GorillaTagCompetitiveServerApi.RankedModeRequestDataPlatformed
	{
		public string matchId;
	}

	[Serializable]
	public class RankedModeValidateMatchJoinResponseData
	{
		public bool validJoin;
	}

	[Serializable]
	public class RankedModePlayerScore
	{
		public string playfabId;

		public float gameScore;
	}

	[Serializable]
	public class RankedModeSubmitMatchScoresRequestData : GorillaTagCompetitiveServerApi.RankedModeRequestDataBase
	{
		public string matchId;

		public string playfabId;

		public List<GorillaTagCompetitiveServerApi.RankedModePlayerScore> playerScores;
	}

	[Serializable]
	public class RankedModeSetEloValueRequestData : GorillaTagCompetitiveServerApi.RankedModeRequestDataPlatformed
	{
		public float elo;
	}

	[Serializable]
	public class RankedModeUnlockCompetitiveQueueRequestData : GorillaTagCompetitiveServerApi.RankedModeRequestDataPlatformed
	{
		public bool unlocked;
	}
}
