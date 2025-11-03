using System;
using System.Collections.Generic;
using UnityEngine;

public class FriendSystem : MonoBehaviour
{
	public FriendSystem.PlayerPrivacy LocalPlayerPrivacy
	{
		get
		{
			return this.localPlayerPrivacy;
		}
	}

	public event Action<List<FriendBackendController.Friend>> OnFriendListRefresh;

	public void SetLocalPlayerPrivacy(FriendSystem.PlayerPrivacy privacyState)
	{
		this.localPlayerPrivacy = privacyState;
		FriendBackendController.PrivacyState privacyState2;
		switch (privacyState)
		{
		default:
			privacyState2 = FriendBackendController.PrivacyState.VISIBLE;
			break;
		case FriendSystem.PlayerPrivacy.PublicOnly:
			privacyState2 = FriendBackendController.PrivacyState.PUBLIC_ONLY;
			break;
		case FriendSystem.PlayerPrivacy.Hidden:
			privacyState2 = FriendBackendController.PrivacyState.HIDDEN;
			break;
		}
		FriendBackendController.Instance.SetPrivacyState(privacyState2);
	}

	public void RefreshFriendsList()
	{
		FriendBackendController.Instance.GetFriends();
	}

	public void SendFriendRequest(NetPlayer targetPlayer, GTZone stationZone, FriendSystem.FriendRequestCallback callback)
	{
		FriendSystem.FriendRequestData friendRequestData = new FriendSystem.FriendRequestData
		{
			completionCallback = callback,
			sendingPlayerId = NetworkSystem.Instance.LocalPlayer.UserId.GetHashCode(),
			targetPlayerId = targetPlayer.UserId.GetHashCode(),
			localTimeSent = Time.time,
			zone = stationZone
		};
		this.pendingFriendRequests.Add(friendRequestData);
		FriendBackendController.Instance.AddFriend(targetPlayer);
	}

	public void RemoveFriend(FriendBackendController.Friend friend, FriendSystem.FriendRemovalCallback callback = null)
	{
		this.pendingFriendRemovals.Add(new FriendSystem.FriendRemovalData
		{
			completionCallback = callback,
			targetPlayerId = friend.Presence.FriendLinkId.GetHashCode(),
			localTimeSent = Time.time
		});
		FriendBackendController.Instance.RemoveFriend(friend);
	}

	public bool HasPendingFriendRequest(GTZone zone, int senderId)
	{
		for (int i = 0; i < this.pendingFriendRequests.Count; i++)
		{
			if (this.pendingFriendRequests[i].zone == zone && this.pendingFriendRequests[i].sendingPlayerId == senderId)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckFriendshipWithPlayer(int targetActorNumber)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(targetActorNumber);
		if (player != null)
		{
			int hashCode = player.UserId.GetHashCode();
			List<FriendBackendController.Friend> friendsList = FriendBackendController.Instance.FriendsList;
			for (int i = 0; i < friendsList.Count; i++)
			{
				if (friendsList[i] != null && friendsList[i].Presence != null && friendsList[i].Presence.FriendLinkId.GetHashCode() == hashCode)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Awake()
	{
		if (FriendSystem.Instance == null)
		{
			FriendSystem.Instance = this;
			return;
		}
		Object.Destroy(this);
	}

	private void Start()
	{
		FriendBackendController.Instance.OnGetFriendsComplete += this.OnGetFriendsReturned;
		FriendBackendController.Instance.OnAddFriendComplete += this.OnAddFriendReturned;
		FriendBackendController.Instance.OnRemoveFriendComplete += this.OnRemoveFriendReturned;
	}

	private void OnDestroy()
	{
		if (FriendBackendController.Instance != null)
		{
			FriendBackendController.Instance.OnGetFriendsComplete -= this.OnGetFriendsReturned;
			FriendBackendController.Instance.OnAddFriendComplete -= this.OnAddFriendReturned;
			FriendBackendController.Instance.OnRemoveFriendComplete -= this.OnRemoveFriendReturned;
		}
	}

	private void OnGetFriendsReturned(bool succeeded)
	{
		if (succeeded)
		{
			this.lastFriendsListRefresh = Time.time;
			switch (FriendBackendController.Instance.MyPrivacyState)
			{
			default:
				this.localPlayerPrivacy = FriendSystem.PlayerPrivacy.Visible;
				break;
			case FriendBackendController.PrivacyState.PUBLIC_ONLY:
				this.localPlayerPrivacy = FriendSystem.PlayerPrivacy.PublicOnly;
				break;
			case FriendBackendController.PrivacyState.HIDDEN:
				this.localPlayerPrivacy = FriendSystem.PlayerPrivacy.Hidden;
				break;
			}
			Action<List<FriendBackendController.Friend>> onFriendListRefresh = this.OnFriendListRefresh;
			if (onFriendListRefresh == null)
			{
				return;
			}
			onFriendListRefresh(FriendBackendController.Instance.FriendsList);
		}
	}

	private void OnAddFriendReturned(NetPlayer targetPlayer, bool succeeded)
	{
		int hashCode = targetPlayer.UserId.GetHashCode();
		this.indexesToRemove.Clear();
		for (int i = 0; i < this.pendingFriendRequests.Count; i++)
		{
			if (this.pendingFriendRequests[i].targetPlayerId == hashCode)
			{
				FriendSystem.FriendRequestCallback completionCallback = this.pendingFriendRequests[i].completionCallback;
				if (completionCallback != null)
				{
					completionCallback(this.pendingFriendRequests[i].zone, this.pendingFriendRequests[i].sendingPlayerId, this.pendingFriendRequests[i].targetPlayerId, succeeded);
				}
				this.indexesToRemove.Add(i);
			}
			else if (this.pendingFriendRequests[i].localTimeSent + this.friendRequestExpirationTime < Time.time)
			{
				this.indexesToRemove.Add(i);
			}
		}
		for (int j = this.indexesToRemove.Count - 1; j >= 0; j--)
		{
			this.pendingFriendRequests.RemoveAt(this.indexesToRemove[j]);
		}
	}

	private void OnRemoveFriendReturned(FriendBackendController.Friend friend, bool succeeded)
	{
		if (friend != null && friend.Presence != null)
		{
			int hashCode = friend.Presence.FriendLinkId.GetHashCode();
			this.indexesToRemove.Clear();
			for (int i = 0; i < this.pendingFriendRemovals.Count; i++)
			{
				if (this.pendingFriendRemovals[i].targetPlayerId == hashCode)
				{
					FriendSystem.FriendRemovalCallback completionCallback = this.pendingFriendRemovals[i].completionCallback;
					if (completionCallback != null)
					{
						completionCallback(hashCode, succeeded);
					}
					this.indexesToRemove.Add(i);
				}
				else if (this.pendingFriendRemovals[i].localTimeSent + this.friendRequestExpirationTime < Time.time)
				{
					this.indexesToRemove.Add(i);
				}
			}
			for (int j = this.indexesToRemove.Count - 1; j >= 0; j--)
			{
				this.pendingFriendRemovals.RemoveAt(this.indexesToRemove[j]);
			}
		}
	}

	[OnEnterPlay_SetNull]
	public static volatile FriendSystem Instance;

	[SerializeField]
	private float friendRequestExpirationTime = 10f;

	private FriendSystem.PlayerPrivacy localPlayerPrivacy;

	private List<FriendSystem.FriendRequestData> pendingFriendRequests = new List<FriendSystem.FriendRequestData>();

	private List<FriendSystem.FriendRemovalData> pendingFriendRemovals = new List<FriendSystem.FriendRemovalData>();

	private List<int> indexesToRemove = new List<int>();

	private float lastFriendsListRefresh;

	public delegate void FriendRequestCallback(GTZone zone, int localId, int friendId, bool success);

	private struct FriendRequestData
	{
		public GTZone zone;

		public int sendingPlayerId;

		public int targetPlayerId;

		public float localTimeSent;

		public FriendSystem.FriendRequestCallback completionCallback;
	}

	public delegate void FriendRemovalCallback(int friendId, bool success);

	private struct FriendRemovalData
	{
		public int targetPlayerId;

		public float localTimeSent;

		public FriendSystem.FriendRemovalCallback completionCallback;
	}

	private enum FriendRequestStatus
	{
		Pending,
		Succeeded,
		Failed
	}

	public enum PlayerPrivacy
	{
		Visible,
		PublicOnly,
		Hidden
	}
}
