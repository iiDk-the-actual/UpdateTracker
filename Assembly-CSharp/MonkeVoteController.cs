using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GorillaNetworking;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;
using UnityEngine.Networking;

public class MonkeVoteController : MonoBehaviour, IGorillaSliceableSimple
{
	public static MonkeVoteController instance { get; private set; }

	public event Action OnPollsUpdated;

	public event Action OnVoteAccepted;

	public event Action OnVoteFailed;

	public event Action OnCurrentPollEnded;

	public void Awake()
	{
		if (MonkeVoteController.instance == null)
		{
			MonkeVoteController.instance = this;
			return;
		}
		Object.Destroy(this);
	}

	public void SliceUpdate()
	{
		if (this.isCurrentPollActive && !this.hasCurrentPollCompleted && this.currentPollCompletionTime < DateTime.UtcNow)
		{
			GTDev.Log<string>("Active vote poll completed.", null);
			this.hasCurrentPollCompleted = true;
			Action onCurrentPollEnded = this.OnCurrentPollEnded;
			if (onCurrentPollEnded == null)
			{
				return;
			}
			onCurrentPollEnded();
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public async void RequestPolls()
	{
		if (!this.isFetchingPoll && (!this.hasPoll || (this.isCurrentPollActive && this.hasCurrentPollCompleted)))
		{
			this.isFetchingPoll = true;
			await this.WaitForSessionToken();
			this.FetchPolls();
		}
		else
		{
			Action onPollsUpdated = this.OnPollsUpdated;
			if (onPollsUpdated != null)
			{
				onPollsUpdated();
			}
		}
	}

	private async Task WaitForSessionToken()
	{
		while (!PlayFabAuthenticator.instance || PlayFabAuthenticator.instance.GetPlayFabPlayerId().IsNullOrEmpty() || PlayFabAuthenticator.instance.GetPlayFabSessionTicket().IsNullOrEmpty() || PlayFabAuthenticator.instance.userID.IsNullOrEmpty())
		{
			await Task.Yield();
			await Task.Delay(1000);
		}
	}

	private void FetchPolls()
	{
		base.StartCoroutine(this.DoFetchPolls(new MonkeVoteController.FetchPollsRequest
		{
			TitleId = PlayFabAuthenticatorSettings.TitleId,
			PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
			PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
			IncludeInactive = this.includeInactive
		}, new Action<List<MonkeVoteController.FetchPollsResponse>>(this.OnFetchPollsResponse)));
	}

	private IEnumerator DoFetchPolls(MonkeVoteController.FetchPollsRequest data, Action<List<MonkeVoteController.FetchPollsResponse>> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.VotingApiBaseUrl + "/api/FetchPoll", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			List<MonkeVoteController.FetchPollsResponse> list = JsonConvert.DeserializeObject<List<MonkeVoteController.FetchPollsResponse>>(request.downloadHandler.text);
			callback(list);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode >= 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
		}
		if (retry)
		{
			if (this.fetchPollsRetryCount < this.maxRetriesOnFail)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.fetchPollsRetryCount + 1));
				this.fetchPollsRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.FetchPolls();
			}
			else
			{
				GTDev.LogError<string>("Maximum FetchPolls retries attempted. Please check your network connection.", null);
				this.fetchPollsRetryCount = 0;
				callback(null);
			}
		}
		yield break;
	}

	private void OnFetchPollsResponse([CanBeNull] List<MonkeVoteController.FetchPollsResponse> response)
	{
		this.isFetchingPoll = false;
		this.hasPoll = false;
		this.lastPollData = null;
		this.currentPollData = null;
		this.isCurrentPollActive = false;
		this.hasCurrentPollCompleted = false;
		if (response != null)
		{
			DateTime minValue = DateTime.MinValue;
			using (List<MonkeVoteController.FetchPollsResponse>.Enumerator enumerator = response.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MonkeVoteController.FetchPollsResponse fetchPollsResponse = enumerator.Current;
					if (fetchPollsResponse.isActive)
					{
						this.hasPoll = true;
						this.currentPollData = fetchPollsResponse;
						if (this.currentPollData.EndTime > DateTime.UtcNow)
						{
							this.isCurrentPollActive = true;
							this.hasCurrentPollCompleted = false;
							this.currentPollCompletionTime = this.currentPollData.EndTime;
							this.currentPollCompletionTime = this.currentPollCompletionTime.AddMinutes(1.0);
						}
					}
					if (!fetchPollsResponse.isActive && fetchPollsResponse.EndTime > minValue && fetchPollsResponse.EndTime < DateTime.UtcNow)
					{
						this.lastPollData = fetchPollsResponse;
					}
				}
				goto IL_0106;
			}
		}
		GTDev.LogError<string>("Error: Could not fetch polls!", null);
		IL_0106:
		Action onPollsUpdated = this.OnPollsUpdated;
		if (onPollsUpdated == null)
		{
			return;
		}
		onPollsUpdated();
	}

	public void Vote(int pollId, int option, bool isPrediction)
	{
		if (!this.hasPoll)
		{
			return;
		}
		if (this.isSendingVote)
		{
			return;
		}
		this.isSendingVote = true;
		this.pollId = pollId;
		this.option = option;
		this.isPrediction = isPrediction;
		this.SendVote();
	}

	private void SendVote()
	{
		this.GetNonceForVotingCallback(null);
	}

	private void GetNonceForVotingCallback([CanBeNull] Message<UserProof> message)
	{
		if (message != null)
		{
			UserProof data = message.Data;
			this.Nonce = ((data != null) ? data.Value : null);
		}
		base.StartCoroutine(this.DoVote(new MonkeVoteController.VoteRequest
		{
			PollId = this.pollId,
			TitleId = PlayFabAuthenticatorSettings.TitleId,
			PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
			OculusId = PlayFabAuthenticator.instance.userID,
			UserPlatform = PlayFabAuthenticator.instance.platform.ToString(),
			UserNonce = this.Nonce,
			PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
			OptionIndex = this.option,
			IsPrediction = this.isPrediction
		}, new Action<MonkeVoteController.VoteResponse>(this.OnVoteSuccess)));
	}

	private IEnumerator DoVote(MonkeVoteController.VoteRequest data, Action<MonkeVoteController.VoteResponse> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.VotingApiBaseUrl + "/api/Vote", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
		bool retry = false;
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			MonkeVoteController.VoteResponse voteResponse = JsonConvert.DeserializeObject<MonkeVoteController.VoteResponse>(request.downloadHandler.text);
			callback(voteResponse);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode >= 500L && responseCode < 600L)
			{
				retry = true;
			}
			else if (request.responseCode == 429L)
			{
				GTDev.LogWarning<string>("User already voted on this poll!", null);
				callback(null);
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				retry = true;
			}
		}
		if (retry)
		{
			if (this.voteRetryCount < this.maxRetriesOnFail)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.voteRetryCount + 1));
				this.voteRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.SendVote();
			}
			else
			{
				GTDev.LogError<string>("Maximum Vote retries attempted. Please check your network connection.", null);
				this.voteRetryCount = 0;
				callback(null);
			}
		}
		else
		{
			this.isSendingVote = false;
		}
		yield break;
	}

	private void OnVoteSuccess([CanBeNull] MonkeVoteController.VoteResponse response)
	{
		this.isSendingVote = false;
		if (response != null)
		{
			this.lastVoteData = response;
			Action onVoteAccepted = this.OnVoteAccepted;
			if (onVoteAccepted == null)
			{
				return;
			}
			onVoteAccepted();
			return;
		}
		else
		{
			Action onVoteFailed = this.OnVoteFailed;
			if (onVoteFailed == null)
			{
				return;
			}
			onVoteFailed();
			return;
		}
	}

	public MonkeVoteController.FetchPollsResponse GetLastPollData()
	{
		return this.lastPollData;
	}

	public MonkeVoteController.FetchPollsResponse GetCurrentPollData()
	{
		return this.currentPollData;
	}

	public MonkeVoteController.VoteResponse GetVoteData()
	{
		return this.lastVoteData;
	}

	public int GetLastVotePollId()
	{
		return this.pollId;
	}

	public int GetLastVoteSelectedOption()
	{
		return this.option;
	}

	public bool GetLastVoteWasPrediction()
	{
		return this.isPrediction;
	}

	public DateTime GetCurrentPollCompletionTime()
	{
		return this.currentPollCompletionTime;
	}

	private string Nonce = "";

	private bool includeInactive = true;

	private int fetchPollsRetryCount;

	private int maxRetriesOnFail = 3;

	private int voteRetryCount;

	private MonkeVoteController.FetchPollsResponse lastPollData;

	private MonkeVoteController.FetchPollsResponse currentPollData;

	private MonkeVoteController.VoteResponse lastVoteData;

	private bool isFetchingPoll;

	private bool hasPoll;

	private bool isCurrentPollActive;

	private bool hasCurrentPollCompleted;

	private DateTime currentPollCompletionTime;

	private bool isSendingVote;

	private int pollId = -1;

	private int option;

	private bool isPrediction;

	[Serializable]
	private class FetchPollsRequest
	{
		public string TitleId;

		public string PlayFabId;

		public string PlayFabTicket;

		public bool IncludeInactive;
	}

	[Serializable]
	public class FetchPollsResponse
	{
		public int PollId;

		public string Question;

		public List<string> VoteOptions;

		public List<int> VoteCount;

		public List<int> PredictionCount;

		public DateTime StartTime;

		public DateTime EndTime;

		public bool isActive;
	}

	[Serializable]
	private class VoteRequest
	{
		public int PollId;

		public string TitleId;

		public string PlayFabId;

		public string OculusId;

		public string UserNonce;

		public string UserPlatform;

		public int OptionIndex;

		public bool IsPrediction;

		public string PlayFabTicket;
	}

	[Serializable]
	public class VoteResponse
	{
		public int PollId { get; set; }

		public string TitleId { get; set; }

		public List<string> VoteOptions { get; set; }

		public List<int> VoteCount { get; set; }

		public List<int> PredictionCount { get; set; }
	}
}
