using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class PhotonUserCache : IInRoomCallbacks, IMatchmakingCallbacks
{
	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player player)
	{
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
	}

	void IMatchmakingCallbacks.OnPreLeavingRoom()
	{
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable changedProperties)
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player player, Hashtable changedProperties)
	{
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
	}
}
