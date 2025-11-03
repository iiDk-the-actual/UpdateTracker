using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaNetworking
{
	public class TitleVotingExample : MonoBehaviour
	{
		public async void Start()
		{
			await this.WaitForSessionToken();
			this.FetchPollsAndVote();
		}

		public void Update()
		{
		}

		private async Task WaitForSessionToken()
		{
			while (!PlayFabAuthenticator.instance || PlayFabAuthenticator.instance.GetPlayFabPlayerId().IsNullOrEmpty() || PlayFabAuthenticator.instance.GetPlayFabSessionTicket().IsNullOrEmpty() || PlayFabAuthenticator.instance.userID.IsNullOrEmpty())
			{
				await Task.Yield();
				await Task.Delay(1000);
			}
		}

		public void FetchPollsAndVote()
		{
			base.StartCoroutine(this.DoFetchPolls(new TitleVotingExample.FetchPollsRequest
			{
				TitleId = PlayFabAuthenticatorSettings.TitleId,
				PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
				PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
				IncludeInactive = this.includeInactive
			}, new Action<List<TitleVotingExample.FetchPollsResponse>>(this.OnFetchPollsResponse)));
		}

		private void GetNonceForVotingCallback([CanBeNull] Message<UserProof> message)
		{
			if (message != null)
			{
				UserProof data = message.Data;
				this.Nonce = ((data != null) ? data.ToString() : null);
			}
			base.StartCoroutine(this.DoVote(new TitleVotingExample.VoteRequest
			{
				PollId = this.PollId,
				TitleId = PlayFabAuthenticatorSettings.TitleId,
				PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
				OculusId = PlayFabAuthenticator.instance.userID,
				UserPlatform = PlayFabAuthenticator.instance.platform.ToString(),
				UserNonce = this.Nonce,
				PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
				OptionIndex = this.Option,
				IsPrediction = this.isPrediction
			}, new Action<TitleVotingExample.VoteResponse>(this.OnVoteSuccess)));
		}

		public void Vote()
		{
			this.GetNonceForVotingCallback(null);
		}

		private IEnumerator DoFetchPolls(TitleVotingExample.FetchPollsRequest data, Action<List<TitleVotingExample.FetchPollsResponse>> callback)
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
				List<TitleVotingExample.FetchPollsResponse> list = JsonConvert.DeserializeObject<List<TitleVotingExample.FetchPollsResponse>>(request.downloadHandler.text);
				callback(list);
			}
			else
			{
				Debug.LogError(string.Format("FetchPolls Error: {0} -- raw response: ", request.responseCode) + request.downloadHandler.text);
				long responseCode = request.responseCode;
				if (responseCode >= 500L && responseCode < 600L)
				{
					retry = true;
					Debug.LogError(string.Format("HTTP {0} error: {1}", request.responseCode, request.error));
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
					Debug.LogWarning(string.Format("Retrying Title Voting FetchPolls... Retry attempt #{0}, waiting for {1} seconds", this.fetchPollsRetryCount + 1, num));
					this.fetchPollsRetryCount++;
					yield return new WaitForSeconds((float)num);
					this.FetchPollsAndVote();
				}
				else
				{
					Debug.LogError("Maximum FetchPolls retries attempted. Please check your network connection.");
					this.fetchPollsRetryCount = 0;
					callback(null);
				}
			}
			yield break;
		}

		private IEnumerator DoVote(TitleVotingExample.VoteRequest data, Action<TitleVotingExample.VoteResponse> callback)
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
				TitleVotingExample.VoteResponse voteResponse = JsonConvert.DeserializeObject<TitleVotingExample.VoteResponse>(request.downloadHandler.text);
				callback(voteResponse);
			}
			else
			{
				Debug.LogError(string.Format("Vote Error: {0} -- raw response: ", request.responseCode) + request.downloadHandler.text);
				long responseCode = request.responseCode;
				if (responseCode >= 500L && responseCode < 600L)
				{
					retry = true;
					Debug.LogError(string.Format("HTTP {0} error: {1}", request.responseCode, request.error));
				}
				else if (request.responseCode == 409L)
				{
					Debug.LogWarning("User already voted on this poll!");
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
					Debug.LogWarning(string.Format("Retrying Voting... Retry attempt #{0}, waiting for {1} seconds", this.voteRetryCount + 1, num));
					this.voteRetryCount++;
					yield return new WaitForSeconds((float)num);
					this.Vote();
				}
				else
				{
					Debug.LogError("Maximum Vote retries attempted. Please check your network connection.");
					this.voteRetryCount = 0;
					callback(null);
				}
			}
			yield break;
		}

		private void OnFetchPollsResponse([CanBeNull] List<TitleVotingExample.FetchPollsResponse> response)
		{
			if (response != null)
			{
				Debug.Log("Got polls: " + JsonConvert.SerializeObject(response));
				this.Vote();
				return;
			}
			Debug.LogError("Error: Could not fetch polls!");
		}

		private void OnVoteSuccess([CanBeNull] TitleVotingExample.VoteResponse response)
		{
			if (response != null)
			{
				Debug.Log("Voted! " + JsonConvert.SerializeObject(response));
				return;
			}
			Debug.LogError("Error: Could not vote!");
		}

		private string Nonce = "";

		private int PollId = 5;

		private bool includeInactive = true;

		private int Option;

		private bool isPrediction;

		private int fetchPollsRetryCount;

		private int voteRetryCount;

		private int maxRetriesOnFail = 3;

		[Serializable]
		private class FetchPollsRequest
		{
			public string TitleId;

			public string PlayFabId;

			public string PlayFabTicket;

			public bool IncludeInactive;
		}

		[Serializable]
		private class FetchPollsResponse
		{
			public int PollId;

			public string Question;

			public List<string> VoteOptions;

			public List<int> VoteCount;

			public List<int> PredictionCount;

			public DateTime StartTime;

			public DateTime EndTime;
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
		private class VoteResponse
		{
			public int PollId { get; set; }

			public string TitleId { get; set; }

			public List<string> VoteOptions { get; set; }

			public List<int> VoteCount { get; set; }

			public List<int> PredictionCount { get; set; }
		}
	}
}
