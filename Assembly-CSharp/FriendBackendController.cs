using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using GorillaNetworking;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class FriendBackendController : MonoBehaviour
{
	public event Action<bool> OnGetFriendsComplete;

	public event Action<bool> OnSetPrivacyStateComplete;

	public event Action<NetPlayer, bool> OnAddFriendComplete;

	public event Action<FriendBackendController.Friend, bool> OnRemoveFriendComplete;

	public List<FriendBackendController.Friend> FriendsList
	{
		get
		{
			return this.lastFriendsList;
		}
	}

	public FriendBackendController.PrivacyState MyPrivacyState
	{
		get
		{
			return this.lastPrivacyState;
		}
	}

	public void GetFriends()
	{
		if (!this.getFriendsInProgress)
		{
			this.getFriendsInProgress = true;
			this.GetFriendsInternal();
		}
	}

	public void SetPrivacyState(FriendBackendController.PrivacyState state)
	{
		if (!this.setPrivacyStateInProgress)
		{
			this.setPrivacyStateInProgress = true;
			this.setPrivacyStateState = state;
			this.SetPrivacyStateInternal();
			return;
		}
		this.setPrivacyStateQueue.Enqueue(state);
	}

	public void AddFriend(NetPlayer target)
	{
		if (target == null)
		{
			return;
		}
		int hashCode = target.UserId.GetHashCode();
		if (!this.addFriendInProgress)
		{
			this.addFriendInProgress = true;
			this.addFriendTargetIdHash = hashCode;
			this.addFriendTargetPlayer = target;
			this.AddFriendInternal();
			return;
		}
		if (hashCode != this.addFriendTargetIdHash && !this.addFriendRequestQueue.Contains(new ValueTuple<int, NetPlayer>(hashCode, target)))
		{
			this.addFriendRequestQueue.Enqueue(new ValueTuple<int, NetPlayer>(hashCode, target));
		}
	}

	public void RemoveFriend(FriendBackendController.Friend target)
	{
		if (target == null)
		{
			return;
		}
		int hashCode = target.Presence.FriendLinkId.GetHashCode();
		if (!this.removeFriendInProgress)
		{
			this.removeFriendInProgress = true;
			this.removeFriendTargetIdHash = hashCode;
			this.removeFriendTarget = target;
			this.RemoveFriendInternal();
			return;
		}
		if (hashCode != this.addFriendTargetIdHash && !this.removeFriendRequestQueue.Contains(new ValueTuple<int, FriendBackendController.Friend>(hashCode, target)))
		{
			this.removeFriendRequestQueue.Enqueue(new ValueTuple<int, FriendBackendController.Friend>(hashCode, target));
		}
	}

	private void Awake()
	{
		if (FriendBackendController.Instance == null)
		{
			FriendBackendController.Instance = this;
			return;
		}
		Object.Destroy(this);
	}

	private void GetFriendsInternal()
	{
		base.StartCoroutine(this.SendGetFriendsRequest(new FriendBackendController.GetFriendsRequest
		{
			PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
			PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
			MothershipId = ""
		}, new Action<FriendBackendController.GetFriendsResponse>(this.GetFriendsComplete)));
	}

	private IEnumerator SendGetFriendsRequest(FriendBackendController.GetFriendsRequest data, Action<FriendBackendController.GetFriendsResponse> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.FriendApiBaseUrl + "/api/GetFriendsV2", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		bool flag = false;
		if (request.result == UnityWebRequest.Result.Success)
		{
			FriendBackendController.GetFriendsResponse getFriendsResponse = JsonConvert.DeserializeObject<FriendBackendController.GetFriendsResponse>(request.downloadHandler.text);
			callback(getFriendsResponse);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode >= 500L && responseCode < 600L)
			{
				flag = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (this.getFriendsRetryCount < this.maxRetriesOnFail)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.getFriendsRetryCount + 1));
				this.getFriendsRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.GetFriendsInternal();
			}
			else
			{
				GTDev.LogError<string>("Maximum GetFriends retries attempted. Please check your network connection.", null);
				this.getFriendsRetryCount = 0;
				callback(null);
			}
		}
		else
		{
			this.getFriendsInProgress = false;
		}
		yield break;
	}

	private void GetFriendsComplete([CanBeNull] FriendBackendController.GetFriendsResponse response)
	{
		this.getFriendsInProgress = false;
		if (response != null)
		{
			this.lastGetFriendsResponse = response;
			if (this.lastGetFriendsResponse.Result != null)
			{
				this.lastPrivacyState = this.lastGetFriendsResponse.Result.MyPrivacyState;
				if (this.lastGetFriendsResponse.Result.Friends != null)
				{
					this.lastFriendsList.Clear();
					foreach (FriendBackendController.Friend friend in this.lastGetFriendsResponse.Result.Friends)
					{
						this.lastFriendsList.Add(friend);
					}
				}
			}
			Action<bool> onGetFriendsComplete = this.OnGetFriendsComplete;
			if (onGetFriendsComplete == null)
			{
				return;
			}
			onGetFriendsComplete(true);
			return;
		}
		else
		{
			Action<bool> onGetFriendsComplete2 = this.OnGetFriendsComplete;
			if (onGetFriendsComplete2 == null)
			{
				return;
			}
			onGetFriendsComplete2(false);
			return;
		}
	}

	public void CreateTestFriends()
	{
		Debug.Log("Adding test friends");
		for (int i = 0; i < 15; i++)
		{
			FriendBackendController.FriendPresence friendPresence = new FriendBackendController.FriendPresence();
			friendPresence.FriendLinkId = i.ToString();
			friendPresence.UserName = i.ToString();
			friendPresence.RoomId = i.ToString();
			friendPresence.Zone = "TreeHouse";
			friendPresence.Region = "Jungle";
			friendPresence.IsPublic = new bool?(true);
			FriendBackendController.Friend friend = new FriendBackendController.Friend();
			friend.Presence = friendPresence;
			friend.Created = DateTime.Now;
			this.FriendsList.Add(friend);
		}
	}

	private void SetPrivacyStateInternal()
	{
		base.StartCoroutine(this.SendSetPrivacyStateRequest(new FriendBackendController.SetPrivacyStateRequest
		{
			PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
			PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
			PrivacyState = this.setPrivacyStateState.ToString()
		}, new Action<FriendBackendController.SetPrivacyStateResponse>(this.SetPrivacyStateComplete)));
	}

	private IEnumerator SendSetPrivacyStateRequest(FriendBackendController.SetPrivacyStateRequest data, Action<FriendBackendController.SetPrivacyStateResponse> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.FriendApiBaseUrl + "/api/SetPrivacyState", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		bool flag = false;
		if (request.result == UnityWebRequest.Result.Success)
		{
			FriendBackendController.SetPrivacyStateResponse setPrivacyStateResponse = JsonConvert.DeserializeObject<FriendBackendController.SetPrivacyStateResponse>(request.downloadHandler.text);
			callback(setPrivacyStateResponse);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode >= 500L && responseCode < 600L)
			{
				flag = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (this.setPrivacyStateRetryCount < this.maxRetriesOnFail)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.setPrivacyStateRetryCount + 1));
				this.setPrivacyStateRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.SetPrivacyStateInternal();
			}
			else
			{
				GTDev.LogError<string>("Maximum SetPrivacyState retries attempted. Please check your network connection.", null);
				this.setPrivacyStateRetryCount = 0;
				callback(null);
			}
		}
		else
		{
			this.setPrivacyStateInProgress = false;
		}
		yield break;
	}

	private void SetPrivacyStateComplete([CanBeNull] FriendBackendController.SetPrivacyStateResponse response)
	{
		this.setPrivacyStateInProgress = false;
		if (response != null)
		{
			this.lastPrivacyStateResponse = response;
			Action<bool> onSetPrivacyStateComplete = this.OnSetPrivacyStateComplete;
			if (onSetPrivacyStateComplete != null)
			{
				onSetPrivacyStateComplete(true);
			}
		}
		else
		{
			Action<bool> onSetPrivacyStateComplete2 = this.OnSetPrivacyStateComplete;
			if (onSetPrivacyStateComplete2 != null)
			{
				onSetPrivacyStateComplete2(false);
			}
		}
		if (this.setPrivacyStateQueue.Count > 0)
		{
			FriendBackendController.PrivacyState privacyState = this.setPrivacyStateQueue.Dequeue();
			this.SetPrivacyState(privacyState);
		}
	}

	private void AddFriendInternal()
	{
		base.StartCoroutine(this.SendAddFriendRequest(new FriendBackendController.FriendRequestRequest
		{
			PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
			MothershipId = "",
			PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
			MothershipToken = "",
			MyFriendLinkId = NetworkSystem.Instance.LocalPlayer.UserId,
			FriendFriendLinkId = this.addFriendTargetPlayer.UserId
		}, new Action<bool>(this.AddFriendComplete)));
	}

	private IEnumerator SendAddFriendRequest(FriendBackendController.FriendRequestRequest data, Action<bool> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.FriendApiBaseUrl + "/api/RequestFriend", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		bool flag = false;
		if (request.result == UnityWebRequest.Result.Success)
		{
			callback(true);
		}
		else
		{
			if (request.responseCode == 409L)
			{
				flag = false;
			}
			long responseCode = request.responseCode;
			if (responseCode >= 500L && responseCode < 600L)
			{
				flag = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (this.addFriendRetryCount < this.maxRetriesOnFail)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.addFriendRetryCount + 1));
				this.addFriendRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.AddFriendInternal();
			}
			else
			{
				GTDev.LogError<string>("Maximum AddFriend retries attempted. Please check your network connection.", null);
				this.addFriendRetryCount = 0;
				callback(false);
			}
		}
		else
		{
			this.addFriendInProgress = false;
		}
		yield break;
	}

	private void AddFriendComplete([CanBeNull] bool success)
	{
		if (success)
		{
			Action<NetPlayer, bool> onAddFriendComplete = this.OnAddFriendComplete;
			if (onAddFriendComplete != null)
			{
				onAddFriendComplete(this.addFriendTargetPlayer, true);
			}
		}
		else
		{
			Action<NetPlayer, bool> onAddFriendComplete2 = this.OnAddFriendComplete;
			if (onAddFriendComplete2 != null)
			{
				onAddFriendComplete2(this.addFriendTargetPlayer, false);
			}
		}
		this.addFriendInProgress = false;
		this.addFriendTargetIdHash = 0;
		this.addFriendTargetPlayer = null;
		if (this.addFriendRequestQueue.Count > 0)
		{
			ValueTuple<int, NetPlayer> valueTuple = this.addFriendRequestQueue.Dequeue();
			this.AddFriend(valueTuple.Item2);
		}
	}

	private void RemoveFriendInternal()
	{
		base.StartCoroutine(this.SendRemoveFriendRequest(new FriendBackendController.RemoveFriendRequest
		{
			PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
			MothershipId = "",
			PlayFabTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
			MyFriendLinkId = NetworkSystem.Instance.LocalPlayer.UserId,
			FriendFriendLinkId = this.removeFriendTarget.Presence.FriendLinkId
		}, new Action<bool>(this.RemoveFriendComplete)));
	}

	private IEnumerator SendRemoveFriendRequest(FriendBackendController.RemoveFriendRequest data, Action<bool> callback)
	{
		UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.FriendApiBaseUrl + "/api/RemoveFriend", "POST");
		byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		bool flag = false;
		if (request.result == UnityWebRequest.Result.Success)
		{
			callback(true);
		}
		else
		{
			long responseCode = request.responseCode;
			if (responseCode >= 500L && responseCode < 600L)
			{
				flag = true;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (this.removeFriendRetryCount < this.maxRetriesOnFail)
			{
				int num = (int)Mathf.Pow(2f, (float)(this.removeFriendRetryCount + 1));
				this.removeFriendRetryCount++;
				yield return new WaitForSeconds((float)num);
				this.AddFriendInternal();
			}
			else
			{
				GTDev.LogError<string>("Maximum AddFriend retries attempted. Please check your network connection.", null);
				this.removeFriendRetryCount = 0;
				callback(false);
			}
		}
		else
		{
			this.removeFriendInProgress = false;
		}
		yield break;
	}

	private void RemoveFriendComplete([CanBeNull] bool success)
	{
		if (success)
		{
			Action<FriendBackendController.Friend, bool> onRemoveFriendComplete = this.OnRemoveFriendComplete;
			if (onRemoveFriendComplete != null)
			{
				onRemoveFriendComplete(this.removeFriendTarget, true);
			}
		}
		else
		{
			Action<FriendBackendController.Friend, bool> onRemoveFriendComplete2 = this.OnRemoveFriendComplete;
			if (onRemoveFriendComplete2 != null)
			{
				onRemoveFriendComplete2(this.removeFriendTarget, false);
			}
		}
		this.removeFriendInProgress = false;
		this.removeFriendTargetIdHash = 0;
		this.removeFriendTarget = null;
		if (this.removeFriendRequestQueue.Count > 0)
		{
			ValueTuple<int, FriendBackendController.Friend> valueTuple = this.removeFriendRequestQueue.Dequeue();
			this.RemoveFriend(valueTuple.Item2);
		}
	}

	private void LogNetPlayersInRoom()
	{
		Debug.Log("Local Player PlayfabId: " + PlayFabAuthenticator.instance.GetPlayFabPlayerId());
		int num = 0;
		foreach (NetPlayer netPlayer in NetworkSystem.Instance.AllNetPlayers)
		{
			Debug.Log(string.Format("[{0}] Player: {1}, ActorNumber: {2}, UserID: {3}, IsMasterClient: {4}", new object[] { num, netPlayer.NickName, netPlayer.ActorNumber, netPlayer.UserId, netPlayer.IsMasterClient }));
			num++;
		}
	}

	private void TestAddFriend()
	{
		this.OnAddFriendComplete -= this.TestAddFriendCompleteCallback;
		this.OnAddFriendComplete += this.TestAddFriendCompleteCallback;
		NetPlayer netPlayer = null;
		if (this.netPlayerIndexToAddFriend >= 0 && this.netPlayerIndexToAddFriend < NetworkSystem.Instance.AllNetPlayers.Length)
		{
			netPlayer = NetworkSystem.Instance.AllNetPlayers[this.netPlayerIndexToAddFriend];
		}
		this.AddFriend(netPlayer);
	}

	private void TestAddFriendCompleteCallback(NetPlayer player, bool success)
	{
		if (success)
		{
			Debug.Log("FriendBackend: TestAddFriendCompleteCallback returned with success = true");
			return;
		}
		Debug.Log("FriendBackend: TestAddFriendCompleteCallback returned with success = false");
	}

	private void TestRemoveFriend()
	{
		this.OnRemoveFriendComplete -= this.TestRemoveFriendCompleteCallback;
		this.OnRemoveFriendComplete += this.TestRemoveFriendCompleteCallback;
		FriendBackendController.Friend friend = null;
		if (this.friendListIndexToRemoveFriend >= 0 && this.friendListIndexToRemoveFriend < this.FriendsList.Count)
		{
			friend = this.FriendsList[this.friendListIndexToRemoveFriend];
		}
		this.RemoveFriend(friend);
	}

	private void TestRemoveFriendCompleteCallback(FriendBackendController.Friend friend, bool success)
	{
		if (success)
		{
			Debug.Log("FriendBackend: TestRemoveFriendCompleteCallback returned with success = true");
			return;
		}
		Debug.Log("FriendBackend: TestRemoveFriendCompleteCallback returned with success = false");
	}

	private void TestGetFriends()
	{
		this.OnGetFriendsComplete -= this.TestGetFriendsCompleteCallback;
		this.OnGetFriendsComplete += this.TestGetFriendsCompleteCallback;
		this.GetFriends();
	}

	private void TestGetFriendsCompleteCallback(bool success)
	{
		if (success)
		{
			Debug.Log("FriendBackend: TestGetFriendsCompleteCallback returned with success = true");
			if (this.FriendsList != null)
			{
				string text = string.Format("Friend Count: {0} Friends: \n", this.FriendsList.Count);
				for (int i = 0; i < this.FriendsList.Count; i++)
				{
					if (this.FriendsList[i] != null && this.FriendsList[i].Presence != null)
					{
						text = string.Concat(new string[]
						{
							text,
							this.FriendsList[i].Presence.UserName,
							", ",
							this.FriendsList[i].Presence.FriendLinkId,
							", ",
							this.FriendsList[i].Presence.RoomId,
							", ",
							this.FriendsList[i].Presence.Region,
							", ",
							this.FriendsList[i].Presence.Zone,
							"\n"
						});
					}
					else
					{
						text += "null friend\n";
					}
				}
				Debug.Log(text);
				return;
			}
		}
		else
		{
			Debug.Log("FriendBackend: TestGetFriendsCompleteCallback returned with success = false");
		}
	}

	private void TestSetPrivacyState()
	{
		this.OnSetPrivacyStateComplete -= this.TestSetPrivacyStateCompleteCallback;
		this.OnSetPrivacyStateComplete += this.TestSetPrivacyStateCompleteCallback;
		this.SetPrivacyState(this.privacyStateToSet);
	}

	private void TestSetPrivacyStateCompleteCallback(bool success)
	{
		if (success)
		{
			Debug.Log(string.Format("SetPrivacyState Success: Status: {0} Error: {1}", this.lastPrivacyStateResponse.StatusCode, this.lastPrivacyStateResponse.Error));
			return;
		}
		Debug.Log(string.Format("SetPrivacyState Failed: Status: {0} Error: {1}", this.lastPrivacyStateResponse.StatusCode, this.lastPrivacyStateResponse.Error));
	}

	[OnEnterPlay_SetNull]
	public static volatile FriendBackendController Instance;

	private int maxRetriesOnFail = 3;

	private int getFriendsRetryCount;

	private int setPrivacyStateRetryCount;

	private int addFriendRetryCount;

	private int removeFriendRetryCount;

	private bool getFriendsInProgress;

	private FriendBackendController.GetFriendsResponse lastGetFriendsResponse;

	private List<FriendBackendController.Friend> lastFriendsList = new List<FriendBackendController.Friend>();

	private bool setPrivacyStateInProgress;

	private FriendBackendController.PrivacyState setPrivacyStateState;

	private FriendBackendController.SetPrivacyStateResponse lastPrivacyStateResponse;

	private Queue<FriendBackendController.PrivacyState> setPrivacyStateQueue = new Queue<FriendBackendController.PrivacyState>();

	private FriendBackendController.PrivacyState lastPrivacyState;

	private bool addFriendInProgress;

	private int addFriendTargetIdHash;

	private NetPlayer addFriendTargetPlayer;

	private Queue<ValueTuple<int, NetPlayer>> addFriendRequestQueue = new Queue<ValueTuple<int, NetPlayer>>();

	private bool removeFriendInProgress;

	private int removeFriendTargetIdHash;

	private FriendBackendController.Friend removeFriendTarget;

	private Queue<ValueTuple<int, FriendBackendController.Friend>> removeFriendRequestQueue = new Queue<ValueTuple<int, FriendBackendController.Friend>>();

	[SerializeField]
	private int netPlayerIndexToAddFriend;

	[SerializeField]
	private int friendListIndexToRemoveFriend;

	[SerializeField]
	private FriendBackendController.PrivacyState privacyStateToSet;

	public class Friend
	{
		public FriendBackendController.FriendPresence Presence { get; set; }

		public DateTime Created { get; set; }
	}

	public class FriendPresence
	{
		public string FriendLinkId { get; set; }

		public string UserName { get; set; }

		public string RoomId { get; set; }

		public string Zone { get; set; }

		public string Region { get; set; }

		public bool? IsPublic { get; set; }
	}

	public class FriendLink
	{
		public string my_playfab_id { get; set; }

		public string my_mothership_id { get; set; }

		public string my_friendlink_id { get; set; }

		public string friend_playfab_id { get; set; }

		public string friend_mothership_id { get; set; }

		public string friend_friendlink_id { get; set; }

		public DateTime created { get; set; }
	}

	[NullableContext(2)]
	[Nullable(0)]
	public class FriendIdResponse
	{
		public string PlayFabId { get; set; }

		public string MothershipId { get; set; } = "";
	}

	public class FriendRequestRequest
	{
		public string PlayFabId { get; set; }

		public string MothershipId { get; set; } = "";

		public string PlayFabTicket { get; set; }

		public string MothershipToken { get; set; }

		public string MyFriendLinkId { get; set; }

		public string FriendFriendLinkId { get; set; }
	}

	public class GetFriendsRequest
	{
		public string PlayFabId { get; set; }

		public string MothershipId { get; set; } = "";

		public string MothershipToken { get; set; }

		public string PlayFabTicket { get; set; }
	}

	public class GetFriendsResponse
	{
		[CanBeNull]
		public FriendBackendController.GetFriendsResult Result { get; set; }

		public int StatusCode { get; set; }

		[Nullable(2)]
		public string Error
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}
	}

	public class GetFriendsResult
	{
		public List<FriendBackendController.Friend> Friends { get; set; }

		public FriendBackendController.PrivacyState MyPrivacyState { get; set; }
	}

	public class SetPrivacyStateRequest
	{
		public string PlayFabId { get; set; }

		public string PlayFabTicket { get; set; }

		public string PrivacyState { get; set; }
	}

	[NullableContext(2)]
	[Nullable(0)]
	public class SetPrivacyStateResponse
	{
		public int StatusCode { get; set; }

		public string Error { get; set; }
	}

	public class RemoveFriendRequest
	{
		public string PlayFabId { get; set; }

		public string MothershipId { get; set; } = "";

		public string PlayFabTicket { get; set; }

		public string MothershipToken { get; set; }

		public string MyFriendLinkId { get; set; }

		public string FriendFriendLinkId { get; set; }
	}

	public enum PendingRequestStatus
	{
		I_REQUESTED,
		THEY_REQUESTED,
		CONFIRMED,
		NOT_FOUND
	}

	public enum PrivacyState
	{
		VISIBLE,
		PUBLIC_ONLY,
		HIDDEN
	}
}
