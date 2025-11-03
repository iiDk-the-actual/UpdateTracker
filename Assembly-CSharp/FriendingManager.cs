using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

public class FriendingManager : MonoBehaviourPun, IPunObservable, IGorillaSliceableSimple
{
	private void Awake()
	{
		if (FriendingManager.Instance == null)
		{
			FriendingManager.Instance = this;
			PhotonNetwork.AddCallbackTarget(this);
			return;
		}
		Object.Destroy(this);
	}

	private void Start()
	{
		NetworkSystem.Instance.OnPlayerLeft += this.OnPlayerLeftRoom;
		NetworkSystem.Instance.OnMultiplayerStarted += this.ValidateState;
		NetworkSystem.Instance.OnReturnedToSinglePlayer += this.ValidateState;
	}

	private void OnDestroy()
	{
		if (NetworkSystem.Instance != null)
		{
			NetworkSystem.Instance.OnPlayerLeft -= this.OnPlayerLeftRoom;
			NetworkSystem.Instance.OnMultiplayerStarted -= this.ValidateState;
			NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.ValidateState;
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

	public void SliceUpdate()
	{
		this.AuthorityUpdate();
	}

	private void AuthorityUpdate()
	{
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			for (int i = 0; i < this.activeFriendStationData.Count; i++)
			{
				if (this.activeFriendStationData[i].state >= FriendingManager.FriendStationState.ButtonConfirmationTimer0 && this.activeFriendStationData[i].state <= FriendingManager.FriendStationState.ButtonConfirmationTimer4)
				{
					FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
					int num = 4;
					float num2 = (Time.time - friendStationData.progressBarStartTime) / this.progressBarDuration;
					if (num2 < 1f)
					{
						int num3 = Mathf.RoundToInt(num2 * (float)num);
						friendStationData.state = num3 + FriendingManager.FriendStationState.ButtonConfirmationTimer0;
					}
					else
					{
						base.photonView.RPC("NotifyClientsFriendRequestReadyRPC", RpcTarget.All, new object[] { friendStationData.zone });
						friendStationData.state = FriendingManager.FriendStationState.WaitingOnRequestBoth;
					}
					this.activeFriendStationData[i] = friendStationData;
				}
			}
		}
	}

	private void OnPlayerLeftRoom(NetPlayer player)
	{
		this.ValidateState();
	}

	private void ValidateState()
	{
		for (int i = 0; i < this.activeFriendStationData.Count; i++)
		{
			FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
			if (friendStationData.actorNumberA != -1 && NetworkSystem.Instance.GetNetPlayerByID(friendStationData.actorNumberA) == null)
			{
				friendStationData.actorNumberA = -1;
			}
			if (friendStationData.actorNumberB != -1 && NetworkSystem.Instance.GetNetPlayerByID(friendStationData.actorNumberB) == null)
			{
				friendStationData.actorNumberB = -1;
			}
			if (friendStationData.actorNumberA == -1 || friendStationData.actorNumberB == -1)
			{
				friendStationData.state = FriendingManager.FriendStationState.WaitingForPlayers;
			}
			this.activeFriendStationData[i] = friendStationData;
		}
		this.UpdateFriendingStations();
	}

	private void UpdateFriendingStations()
	{
		for (int i = 0; i < this.activeFriendStationData.Count; i++)
		{
			FriendingStation friendingStation;
			if (this.friendingStations.TryGetValue(this.activeFriendStationData[i].zone, out friendingStation))
			{
				friendingStation.UpdateState(this.activeFriendStationData[i]);
			}
		}
	}

	public void RegisterFriendingStation(FriendingStation friendingStation)
	{
		if (!this.friendingStations.ContainsKey(friendingStation.Zone))
		{
			this.friendingStations.Add(friendingStation.Zone, friendingStation);
		}
	}

	public void UnregisterFriendingStation(FriendingStation friendingStation)
	{
		this.friendingStations.Remove(friendingStation.Zone);
	}

	private void DebugLogFriendingStations()
	{
		string text = string.Format("Friending Stations: Count: {0} ", this.friendingStations.Count);
		foreach (KeyValuePair<GTZone, FriendingStation> keyValuePair in this.friendingStations)
		{
			text += string.Format("Station Zone {0}", keyValuePair.Key);
		}
		Debug.Log(text);
	}

	public void PlayerEnteredStation(GTZone zone, NetPlayer netPlayer)
	{
		if (netPlayer != null && netPlayer.ActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			this.localPlayerZone = zone;
		}
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			int num = -1;
			int i = 0;
			while (i < this.activeFriendStationData.Count)
			{
				if (this.activeFriendStationData[i].zone == zone)
				{
					num = i;
					if (this.activeFriendStationData[i].actorNumberA == -1 && this.activeFriendStationData[i].actorNumberB != netPlayer.ActorNumber)
					{
						FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
						friendStationData.actorNumberA = netPlayer.ActorNumber;
						if (friendStationData.actorNumberA != -1 && friendStationData.actorNumberB != -1)
						{
							friendStationData.state = FriendingManager.FriendStationState.WaitingOnFriendStatusBoth;
						}
						else
						{
							friendStationData.state = FriendingManager.FriendStationState.WaitingForPlayers;
						}
						this.activeFriendStationData[i] = friendStationData;
					}
					else if (this.activeFriendStationData[i].actorNumberA != -1 && this.activeFriendStationData[i].actorNumberA != netPlayer.ActorNumber && this.activeFriendStationData[i].actorNumberB == -1)
					{
						FriendingManager.FriendStationData friendStationData2 = this.activeFriendStationData[i];
						friendStationData2.actorNumberB = netPlayer.ActorNumber;
						if (friendStationData2.actorNumberA != -1 && friendStationData2.actorNumberB != -1)
						{
							friendStationData2.state = FriendingManager.FriendStationState.WaitingOnFriendStatusBoth;
						}
						else
						{
							friendStationData2.state = FriendingManager.FriendStationState.WaitingForPlayers;
						}
						this.activeFriendStationData[i] = friendStationData2;
					}
					if (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnFriendStatusBoth)
					{
						base.photonView.RPC("CheckFriendStatusRequestRPC", RpcTarget.All, new object[]
						{
							this.activeFriendStationData[i].zone,
							this.activeFriendStationData[i].actorNumberA,
							this.activeFriendStationData[i].actorNumberB
						});
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			if (num < 0)
			{
				this.activeFriendStationData.Add(new FriendingManager.FriendStationData
				{
					zone = zone,
					actorNumberA = netPlayer.ActorNumber,
					actorNumberB = -1,
					state = FriendingManager.FriendStationState.WaitingForPlayers
				});
			}
			this.UpdateFriendingStations();
		}
	}

	public void PlayerExitedStation(GTZone zone, NetPlayer netPlayer)
	{
		if (netPlayer != null && netPlayer.ActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			this.localPlayerZone = GTZone.none;
		}
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			int num = -1;
			int i = 0;
			while (i < this.activeFriendStationData.Count)
			{
				if (this.activeFriendStationData[i].zone == zone)
				{
					if ((this.activeFriendStationData[i].actorNumberA == netPlayer.ActorNumber && this.activeFriendStationData[i].actorNumberB == -1) || (this.activeFriendStationData[i].actorNumberA == -1 && this.activeFriendStationData[i].actorNumberB == netPlayer.ActorNumber))
					{
						FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
						friendStationData.actorNumberA = -1;
						friendStationData.actorNumberB = -1;
						friendStationData.state = FriendingManager.FriendStationState.WaitingForPlayers;
						this.activeFriendStationData[i] = friendStationData;
						num = i;
						break;
					}
					if (this.activeFriendStationData[i].actorNumberA != -1 && this.activeFriendStationData[i].actorNumberA != netPlayer.ActorNumber && this.activeFriendStationData[i].actorNumberB == netPlayer.ActorNumber)
					{
						FriendingManager.FriendStationData friendStationData2 = this.activeFriendStationData[i];
						friendStationData2.actorNumberB = -1;
						friendStationData2.state = FriendingManager.FriendStationState.WaitingForPlayers;
						this.activeFriendStationData[i] = friendStationData2;
						break;
					}
					if (this.activeFriendStationData[i].actorNumberB != -1 && this.activeFriendStationData[i].actorNumberB != netPlayer.ActorNumber && this.activeFriendStationData[i].actorNumberA == netPlayer.ActorNumber)
					{
						FriendingManager.FriendStationData friendStationData3 = this.activeFriendStationData[i];
						friendStationData3.actorNumberA = -1;
						friendStationData3.state = FriendingManager.FriendStationState.WaitingForPlayers;
						this.activeFriendStationData[i] = friendStationData3;
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			this.UpdateFriendingStations();
			if (num >= 0)
			{
				base.photonView.RPC("StationNoLongerActiveRPC", RpcTarget.Others, new object[] { this.activeFriendStationData[num].zone });
				this.activeFriendStationData.RemoveAt(num);
			}
		}
	}

	private void PlayerPressedButton(GTZone zone, int playerId)
	{
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			int i = 0;
			while (i < this.activeFriendStationData.Count)
			{
				if (this.activeFriendStationData[i].zone == zone)
				{
					if (this.activeFriendStationData[i].actorNumberA == -1 || this.activeFriendStationData[i].actorNumberB == -1)
					{
						break;
					}
					if ((this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnButtonPlayerA && this.activeFriendStationData[i].actorNumberA == playerId) || (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnButtonPlayerB && this.activeFriendStationData[i].actorNumberB == playerId))
					{
						FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
						friendStationData.state = FriendingManager.FriendStationState.ButtonConfirmationTimer0;
						friendStationData.progressBarStartTime = Time.time;
						this.activeFriendStationData[i] = friendStationData;
						return;
					}
					if (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnButtonBoth && this.activeFriendStationData[i].actorNumberA == playerId)
					{
						FriendingManager.FriendStationData friendStationData2 = this.activeFriendStationData[i];
						friendStationData2.state = FriendingManager.FriendStationState.WaitingOnButtonPlayerB;
						this.activeFriendStationData[i] = friendStationData2;
						return;
					}
					if (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnButtonBoth && this.activeFriendStationData[i].actorNumberB == playerId)
					{
						FriendingManager.FriendStationData friendStationData3 = this.activeFriendStationData[i];
						friendStationData3.state = FriendingManager.FriendStationState.WaitingOnButtonPlayerA;
						this.activeFriendStationData[i] = friendStationData3;
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}
	}

	private void PlayerUnpressedButton(GTZone zone, int playerId)
	{
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			int i = 0;
			while (i < this.activeFriendStationData.Count)
			{
				if (this.activeFriendStationData[i].zone == zone)
				{
					if (this.activeFriendStationData[i].actorNumberA == -1 || this.activeFriendStationData[i].actorNumberB == -1)
					{
						break;
					}
					bool flag = this.activeFriendStationData[i].state >= FriendingManager.FriendStationState.ButtonConfirmationTimer0 && this.activeFriendStationData[i].state <= FriendingManager.FriendStationState.ButtonConfirmationTimer4;
					if (flag && this.activeFriendStationData[i].actorNumberA == playerId)
					{
						FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
						friendStationData.state = FriendingManager.FriendStationState.WaitingOnButtonPlayerA;
						this.activeFriendStationData[i] = friendStationData;
						return;
					}
					if (flag && this.activeFriendStationData[i].actorNumberB == playerId)
					{
						FriendingManager.FriendStationData friendStationData2 = this.activeFriendStationData[i];
						friendStationData2.state = FriendingManager.FriendStationState.WaitingOnButtonPlayerB;
						this.activeFriendStationData[i] = friendStationData2;
						return;
					}
					if ((this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnButtonPlayerA && this.activeFriendStationData[i].actorNumberB == playerId) || (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnButtonPlayerB && this.activeFriendStationData[i].actorNumberA == playerId))
					{
						FriendingManager.FriendStationData friendStationData3 = this.activeFriendStationData[i];
						friendStationData3.state = FriendingManager.FriendStationState.WaitingOnButtonBoth;
						this.activeFriendStationData[i] = friendStationData3;
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}
	}

	private void CheckFriendStatusRequest(GTZone zone, int actorNumberA, int actorNumberB)
	{
		FriendSystem.Instance.OnFriendListRefresh -= this.CheckFriendStatusOnFriendListRefresh;
		FriendSystem.Instance.OnFriendListRefresh += this.CheckFriendStatusOnFriendListRefresh;
		FriendSystem.Instance.RefreshFriendsList();
	}

	private void CheckFriendStatusOnFriendListRefresh(List<FriendBackendController.Friend> friendList)
	{
		FriendSystem.Instance.OnFriendListRefresh -= this.CheckFriendStatusOnFriendListRefresh;
		int i = 0;
		while (i < this.activeFriendStationData.Count)
		{
			if (this.activeFriendStationData[i].zone == this.localPlayerZone)
			{
				int actorNumber = NetworkSystem.Instance.LocalPlayer.ActorNumber;
				int num = -1;
				if (this.activeFriendStationData[i].actorNumberA == actorNumber)
				{
					num = this.activeFriendStationData[i].actorNumberB;
				}
				else if (this.activeFriendStationData[i].actorNumberB == actorNumber)
				{
					num = this.activeFriendStationData[i].actorNumberA;
				}
				if (num != -1 && FriendSystem.Instance.CheckFriendshipWithPlayer(num))
				{
					base.photonView.RPC("CheckFriendStatusResponseRPC", RpcTarget.MasterClient, new object[] { this.localPlayerZone, num, true });
					return;
				}
				base.photonView.RPC("CheckFriendStatusResponseRPC", RpcTarget.MasterClient, new object[] { this.localPlayerZone, num, false });
				return;
			}
			else
			{
				i++;
			}
		}
	}

	private void CheckFriendStatusResponse(GTZone zone, int responderActorNumber, int friendTargetActorNumber, bool friends)
	{
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			int i = 0;
			while (i < this.activeFriendStationData.Count)
			{
				if (this.activeFriendStationData[i].zone == zone)
				{
					if (this.activeFriendStationData[i].actorNumberA == -1 || this.activeFriendStationData[i].actorNumberB == -1)
					{
						break;
					}
					if ((this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnFriendStatusPlayerA && this.activeFriendStationData[i].actorNumberA == responderActorNumber) || (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnFriendStatusPlayerB && this.activeFriendStationData[i].actorNumberB == responderActorNumber))
					{
						FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
						if (friends)
						{
							friendStationData.state = FriendingManager.FriendStationState.AlreadyFriends;
						}
						else
						{
							friendStationData.state = FriendingManager.FriendStationState.WaitingOnButtonBoth;
						}
						this.activeFriendStationData[i] = friendStationData;
						return;
					}
					if (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnFriendStatusBoth && this.activeFriendStationData[i].actorNumberA == responderActorNumber)
					{
						FriendingManager.FriendStationData friendStationData2 = this.activeFriendStationData[i];
						if (friends)
						{
							friendStationData2.state = FriendingManager.FriendStationState.WaitingOnFriendStatusPlayerB;
						}
						else
						{
							friendStationData2.state = FriendingManager.FriendStationState.WaitingOnButtonBoth;
						}
						this.activeFriendStationData[i] = friendStationData2;
						return;
					}
					if (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnFriendStatusBoth && this.activeFriendStationData[i].actorNumberB == responderActorNumber)
					{
						FriendingManager.FriendStationData friendStationData3 = this.activeFriendStationData[i];
						if (friends)
						{
							friendStationData3.state = FriendingManager.FriendStationState.WaitingOnFriendStatusPlayerA;
						}
						else
						{
							friendStationData3.state = FriendingManager.FriendStationState.WaitingOnButtonBoth;
						}
						this.activeFriendStationData[i] = friendStationData3;
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}
	}

	private void SendFriendRequestIfApplicable(GTZone zone)
	{
		int i = 0;
		while (i < this.activeFriendStationData.Count)
		{
			if (this.activeFriendStationData[i].zone == zone)
			{
				FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
				int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
				NetPlayer netPlayer = null;
				if (friendStationData.actorNumberA == actorNumber)
				{
					netPlayer = NetworkSystem.Instance.GetNetPlayerByID(friendStationData.actorNumberB);
				}
				else if (friendStationData.actorNumberB == actorNumber)
				{
					netPlayer = NetworkSystem.Instance.GetNetPlayerByID(friendStationData.actorNumberA);
				}
				if (netPlayer == null)
				{
					return;
				}
				FriendingStation friendingStation;
				if (this.friendingStations.TryGetValue(friendStationData.zone, out friendingStation) && (GTPlayer.Instance.HeadCenterPosition - friendingStation.transform.position).sqrMagnitude < this.requiredProximityToStation * this.requiredProximityToStation)
				{
					FriendSystem.Instance.SendFriendRequest(netPlayer, friendStationData.zone, new FriendSystem.FriendRequestCallback(this.FriendRequestCallback));
				}
				return;
			}
			else
			{
				i++;
			}
		}
	}

	private void FriendRequestCompletedAuthority(GTZone zone, int playerId, bool succeeded)
	{
		if (PhotonNetwork.InRoom && base.photonView.IsMine)
		{
			int i = 0;
			while (i < this.activeFriendStationData.Count)
			{
				if (this.activeFriendStationData[i].zone == zone)
				{
					if (!succeeded)
					{
						FriendingManager.FriendStationData friendStationData = this.activeFriendStationData[i];
						friendStationData.state = FriendingManager.FriendStationState.RequestFailed;
						this.activeFriendStationData[i] = friendStationData;
						return;
					}
					if ((this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnRequestPlayerA && this.activeFriendStationData[i].actorNumberA == playerId) || (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnRequestPlayerB && this.activeFriendStationData[i].actorNumberB == playerId))
					{
						FriendingManager.FriendStationData friendStationData2 = this.activeFriendStationData[i];
						friendStationData2.state = FriendingManager.FriendStationState.Friends;
						this.activeFriendStationData[i] = friendStationData2;
						return;
					}
					if (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnRequestBoth && this.activeFriendStationData[i].actorNumberA == playerId)
					{
						FriendingManager.FriendStationData friendStationData3 = this.activeFriendStationData[i];
						friendStationData3.state = FriendingManager.FriendStationState.WaitingOnRequestPlayerB;
						this.activeFriendStationData[i] = friendStationData3;
						return;
					}
					if (this.activeFriendStationData[i].state == FriendingManager.FriendStationState.WaitingOnRequestBoth && this.activeFriendStationData[i].actorNumberB == playerId)
					{
						FriendingManager.FriendStationData friendStationData4 = this.activeFriendStationData[i];
						friendStationData4.state = FriendingManager.FriendStationState.WaitingOnRequestPlayerA;
						this.activeFriendStationData[i] = friendStationData4;
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}
	}

	private void FriendRequestCallback(GTZone zone, int localId, int friendId, bool success)
	{
		if (base.photonView.IsMine)
		{
			this.FriendRequestCompletedAuthority(zone, PhotonNetwork.LocalPlayer.ActorNumber, success);
			return;
		}
		base.photonView.RPC("FriendRequestCompletedRPC", RpcTarget.MasterClient, new object[] { zone, success });
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(this.activeFriendStationData.Count);
			for (int i = 0; i < this.activeFriendStationData.Count; i++)
			{
				FriendingManager.<OnPhotonSerializeView>g__SendFriendStationData|31_0(stream, this.activeFriendStationData[i]);
			}
		}
		else if (stream.IsReading && info.Sender.IsMasterClient)
		{
			int num = (int)stream.ReceiveNext();
			if (num >= 0 && num <= 10)
			{
				this.activeFriendStationData.Clear();
				for (int j = 0; j < num; j++)
				{
					this.activeFriendStationData.Add(FriendingManager.<OnPhotonSerializeView>g__ReceiveFriendStationData|31_1(stream));
				}
			}
		}
		this.UpdateFriendingStations();
	}

	[PunRPC]
	public void CheckFriendStatusRequestRPC(GTZone zone, int actorNumberA, int actorNumberB, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "CheckFriendStatusRequestRPC");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender), out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[12].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.CheckFriendStatusRequest(zone, actorNumberA, actorNumberB);
	}

	[PunRPC]
	public void CheckFriendStatusResponseRPC(GTZone zone, int friendTargetActorNumber, bool friends, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "CheckFriendStatusRequestRPC");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender), out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[12].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.CheckFriendStatusResponse(zone, info.Sender.ActorNumber, friendTargetActorNumber, friends);
	}

	[PunRPC]
	public void FriendButtonPressedRPC(GTZone zone, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "FriendButtonPressedRPC");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender), out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[12].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.PlayerPressedButton(zone, info.Sender.ActorNumber);
	}

	[PunRPC]
	public void FriendButtonUnpressedRPC(GTZone zone, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "FriendButtonUnpressedRPC");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender), out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[12].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.PlayerUnpressedButton(zone, info.Sender.ActorNumber);
	}

	[PunRPC]
	public void StationNoLongerActiveRPC(GTZone zone, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "StationNoLongerActiveRPC");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender), out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[12].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		FriendingStation friendingStation;
		if (info.Sender.IsMasterClient && this.friendingStations.TryGetValue(zone, out friendingStation))
		{
			friendingStation.UpdateState(new FriendingManager.FriendStationData
			{
				zone = zone,
				actorNumberA = -1,
				actorNumberB = -1,
				state = FriendingManager.FriendStationState.WaitingForPlayers
			});
		}
	}

	[PunRPC]
	public void NotifyClientsFriendRequestReadyRPC(GTZone zone, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "NotifyClientsFriendRequestReadyRPC");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender), out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[12].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.SendFriendRequestIfApplicable(zone);
	}

	[PunRPC]
	public void FriendRequestCompletedRPC(GTZone zone, bool succeeded, PhotonMessageInfo info)
	{
		GorillaNot.IncrementRPCCall(info, "FriendRequestCompletedRPC");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender), out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[12].CallLimitSettings.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.FriendRequestCompletedAuthority(zone, info.Sender.ActorNumber, succeeded);
	}

	[CompilerGenerated]
	internal static void <OnPhotonSerializeView>g__SendFriendStationData|31_0(PhotonStream stream, FriendingManager.FriendStationData data)
	{
		stream.SendNext((int)data.zone);
		stream.SendNext(data.actorNumberA);
		stream.SendNext(data.actorNumberB);
		stream.SendNext((int)data.state);
	}

	[CompilerGenerated]
	internal static FriendingManager.FriendStationData <OnPhotonSerializeView>g__ReceiveFriendStationData|31_1(PhotonStream stream)
	{
		return new FriendingManager.FriendStationData
		{
			zone = (GTZone)((int)stream.ReceiveNext()),
			actorNumberA = (int)stream.ReceiveNext(),
			actorNumberB = (int)stream.ReceiveNext(),
			state = (FriendingManager.FriendStationState)((int)stream.ReceiveNext())
		};
	}

	[OnEnterPlay_SetNull]
	public static volatile FriendingManager Instance;

	[SerializeField]
	private float progressBarDuration = 3f;

	[SerializeField]
	private float requiredProximityToStation = 3f;

	private List<FriendingManager.FriendStationData> activeFriendStationData = new List<FriendingManager.FriendStationData>(10);

	private Dictionary<GTZone, FriendingStation> friendingStations = new Dictionary<GTZone, FriendingStation>();

	private GTZone localPlayerZone = GTZone.none;

	public enum FriendStationState
	{
		NotInRoom,
		WaitingForPlayers,
		WaitingOnFriendStatusBoth,
		WaitingOnFriendStatusPlayerA,
		WaitingOnFriendStatusPlayerB,
		WaitingOnButtonBoth,
		WaitingOnButtonPlayerA,
		WaitingOnButtonPlayerB,
		ButtonConfirmationTimer0,
		ButtonConfirmationTimer1,
		ButtonConfirmationTimer2,
		ButtonConfirmationTimer3,
		ButtonConfirmationTimer4,
		WaitingOnRequestBoth,
		WaitingOnRequestPlayerA,
		WaitingOnRequestPlayerB,
		RequestFailed,
		Friends,
		AlreadyFriends
	}

	public struct FriendStationData
	{
		public GTZone zone;

		public int actorNumberA;

		public int actorNumberB;

		public FriendingManager.FriendStationState state;

		public float progressBarStartTime;
	}
}
