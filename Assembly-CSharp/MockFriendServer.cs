using System;
using System.Collections.Generic;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

public class MockFriendServer : MonoBehaviourPun
{
	public int LocalPlayerId
	{
		get
		{
			return PhotonNetwork.LocalPlayer.UserId.GetHashCode();
		}
	}

	private void Awake()
	{
		if (MockFriendServer.Instance == null)
		{
			MockFriendServer.Instance = this;
			PhotonNetwork.AddCallbackTarget(this);
			NetworkSystem.Instance.OnMultiplayerStarted += this.OnMultiplayerStarted;
			return;
		}
		Object.Destroy(this);
	}

	private void OnMultiplayerStarted()
	{
		this.RegisterLocalPlayer(this.LocalPlayerId);
	}

	private void Update()
	{
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			this.indexesToRemove.Clear();
			for (int i = 0; i < this.friendRequests.Count; i++)
			{
				if (this.friendRequests[i].requestTime + this.friendRequestExpirationTime < Time.time)
				{
					this.indexesToRemove.Add(i);
				}
			}
			for (int j = 0; j < this.indexesToRemove.Count; j++)
			{
				this.friendRequests.RemoveAt(this.indexesToRemove[j]);
			}
			this.indexesToRemove.Clear();
			for (int k = 0; k < this.friendRequests.Count; k++)
			{
				if (this.friendRequests[k].requestTime + this.friendRequestExpirationTime < Time.time)
				{
					this.indexesToRemove.Add(k);
				}
				else if (this.friendRequests[k].completionTime < Time.time)
				{
					for (int l = k + 1; l < this.friendRequests.Count; l++)
					{
						int num;
						int num2;
						if (this.friendRequests[l].completionTime < Time.time && this.friendRequests[k].requestorPublicId == this.friendRequests[l].requesteePublicId && this.friendRequests[k].requesteePublicId == this.friendRequests[l].requestorPublicId && this.TryLookupPrivateId(this.friendRequests[k].requestorPublicId, out num) && this.TryLookupPrivateId(this.friendRequests[k].requesteePublicId, out num2))
						{
							this.AddFriend(this.friendRequests[k].requestorPublicId, this.friendRequests[k].requesteePublicId, num, num2);
							this.indexesToRemove.Add(l);
							this.indexesToRemove.Add(k);
							base.photonView.RPC("AddFriendPairRPC", RpcTarget.Others, new object[]
							{
								this.friendRequests[k].requestorPublicId,
								this.friendRequests[k].requesteePublicId,
								num,
								num2
							});
							break;
						}
					}
				}
			}
			for (int m = 0; m < this.indexesToRemove.Count; m++)
			{
				this.friendRequests.RemoveAt(this.indexesToRemove[m]);
			}
		}
	}

	public void RegisterLocalPlayer(int localPlayerPublicId)
	{
		int hashCode = PlayFabAuthenticator.instance.GetPlayFabPlayerId().GetHashCode();
		if (base.photonView.IsMine)
		{
			this.RegisterLocalPlayerInternal(localPlayerPublicId, hashCode);
			return;
		}
		base.photonView.RPC("RegisterLocalPlayerRPC", RpcTarget.MasterClient, new object[] { localPlayerPublicId, hashCode });
	}

	public void RequestAddFriend(int targetPlayerId)
	{
		if (base.photonView.IsMine)
		{
			this.RequestAddFriendInternal(this.LocalPlayerId, targetPlayerId);
			return;
		}
		base.photonView.RPC("RequestAddFriendRPC", RpcTarget.MasterClient, new object[] { this.LocalPlayerId, targetPlayerId });
	}

	public void RequestRemoveFriend(int targetPlayerId)
	{
		if (base.photonView.IsMine)
		{
			this.RequestRemoveFriendInternal(this.LocalPlayerId, targetPlayerId);
			return;
		}
		base.photonView.RPC("RequestRemoveFriendRPC", RpcTarget.MasterClient, new object[] { this.LocalPlayerId, targetPlayerId });
	}

	public void GetFriendList(List<int> friendListResult)
	{
		int localPlayerId = this.LocalPlayerId;
		friendListResult.Clear();
		for (int i = 0; i < this.friendPairList.Count; i++)
		{
			if (this.friendPairList[i].publicIdPlayerA == localPlayerId)
			{
				friendListResult.Add(this.friendPairList[i].publicIdPlayerB);
			}
			else if (this.friendPairList[i].publicIdPlayerB == localPlayerId)
			{
				friendListResult.Add(this.friendPairList[i].publicIdPlayerA);
			}
		}
	}

	private void RequestAddFriendInternal(int localPlayerPublicId, int otherPlayerPublicId)
	{
		if (base.photonView.IsMine)
		{
			bool flag = false;
			for (int i = 0; i < this.friendRequests.Count; i++)
			{
				if (this.friendRequests[i].requestorPublicId == localPlayerPublicId && this.friendRequests[i].requesteePublicId == otherPlayerPublicId)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				float time = Time.time;
				float num = Random.Range(this.friendRequestCompletionDelayRange.x, this.friendRequestCompletionDelayRange.y);
				this.friendRequests.Add(new MockFriendServer.FriendRequest
				{
					requestorPublicId = localPlayerPublicId,
					requesteePublicId = otherPlayerPublicId,
					requestTime = time,
					completionTime = time + num
				});
			}
		}
	}

	[PunRPC]
	public void RequestAddFriendRPC(int localPlayerPublicId, int otherPlayerPublicId, PhotonMessageInfo info)
	{
		this.RequestAddFriendInternal(localPlayerPublicId, otherPlayerPublicId);
	}

	private void RequestRemoveFriendInternal(int localPlayerPublicId, int otherPlayerPublicId)
	{
		int num;
		int num2;
		if (base.photonView.IsMine && this.TryLookupPrivateId(localPlayerPublicId, out num) && this.TryLookupPrivateId(otherPlayerPublicId, out num2))
		{
			this.RemoveFriend(num, num2);
		}
	}

	[PunRPC]
	public void RequestRemoveFriendRPC(int localPlayerPublicId, int otherPlayerPublicId, PhotonMessageInfo info)
	{
		this.RequestRemoveFriendInternal(localPlayerPublicId, otherPlayerPublicId);
	}

	private void RegisterLocalPlayerInternal(int publicId, int privateId)
	{
		if (base.photonView.IsMine)
		{
			bool flag = false;
			for (int i = 0; i < this.privateIdLookup.Count; i++)
			{
				if (publicId == this.privateIdLookup[i].playerPublicId || privateId == this.privateIdLookup[i].playerPrivateId)
				{
					MockFriendServer.PrivateIdEncryptionPlaceholder privateIdEncryptionPlaceholder = this.privateIdLookup[i];
					privateIdEncryptionPlaceholder.playerPublicId = publicId;
					privateIdEncryptionPlaceholder.playerPrivateId = privateId;
					this.privateIdLookup[i] = privateIdEncryptionPlaceholder;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.privateIdLookup.Add(new MockFriendServer.PrivateIdEncryptionPlaceholder
				{
					playerPublicId = publicId,
					playerPrivateId = privateId
				});
			}
		}
	}

	[PunRPC]
	public void RegisterLocalPlayerRPC(int playerPublicId, int playerPrivateId, PhotonMessageInfo info)
	{
		this.RegisterLocalPlayerInternal(playerPublicId, playerPrivateId);
	}

	[PunRPC]
	public void AddFriendPairRPC(int publicIdA, int publicIdB, int privateIdA, int privateIdB, PhotonMessageInfo info)
	{
		this.AddFriend(publicIdA, publicIdB, privateIdA, privateIdB);
	}

	private void AddFriend(int publicIdA, int publicIdB, int privateIdA, int privateIdB)
	{
		for (int i = 0; i < this.friendPairList.Count; i++)
		{
			if ((this.friendPairList[i].privateIdPlayerA == privateIdA && this.friendPairList[i].privateIdPlayerB == privateIdB) || (this.friendPairList[i].privateIdPlayerA == privateIdB && this.friendPairList[i].privateIdPlayerB == privateIdA))
			{
				return;
			}
		}
		this.friendPairList.Add(new MockFriendServer.FriendPair
		{
			publicIdPlayerA = publicIdA,
			publicIdPlayerB = publicIdB,
			privateIdPlayerA = privateIdA,
			privateIdPlayerB = privateIdB
		});
	}

	private void RemoveFriend(int privateIdA, int privateIdB)
	{
		this.indexesToRemove.Clear();
		for (int i = 0; i < this.friendPairList.Count; i++)
		{
			if ((this.friendPairList[i].privateIdPlayerA == privateIdA && this.friendPairList[i].privateIdPlayerB == privateIdB) || (this.friendPairList[i].privateIdPlayerA == privateIdB && this.friendPairList[i].privateIdPlayerB == privateIdA))
			{
				this.indexesToRemove.Add(i);
			}
		}
		for (int j = 0; j < this.friendPairList.Count; j++)
		{
			this.friendPairList.RemoveAt(this.indexesToRemove[j]);
		}
	}

	private bool TryLookupPrivateId(int publicId, out int privateId)
	{
		for (int i = 0; i < this.privateIdLookup.Count; i++)
		{
			if (this.privateIdLookup[i].playerPublicId == publicId)
			{
				privateId = this.privateIdLookup[i].playerPrivateId;
				return true;
			}
		}
		privateId = -1;
		return false;
	}

	[OnEnterPlay_SetNull]
	public static volatile MockFriendServer Instance;

	[SerializeField]
	private Vector2 friendRequestCompletionDelayRange = new Vector2(0.5f, 1f);

	[SerializeField]
	private float friendRequestExpirationTime = 10f;

	private List<MockFriendServer.FriendPair> friendPairList = new List<MockFriendServer.FriendPair>();

	private List<MockFriendServer.PrivateIdEncryptionPlaceholder> privateIdLookup = new List<MockFriendServer.PrivateIdEncryptionPlaceholder>();

	private List<MockFriendServer.FriendRequest> friendRequests = new List<MockFriendServer.FriendRequest>();

	private List<int> indexesToRemove = new List<int>();

	public struct FriendPair
	{
		public int publicIdPlayerA;

		public int publicIdPlayerB;

		public int privateIdPlayerA;

		public int privateIdPlayerB;
	}

	public struct PrivateIdEncryptionPlaceholder
	{
		public int playerPublicId;

		public int playerPrivateId;
	}

	public struct FriendRequest
	{
		public int requestorPublicId;

		public int requesteePublicId;

		public float requestTime;

		public float completionTime;
	}
}
