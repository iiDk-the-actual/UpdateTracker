using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Fusion;
using Photon.Realtime;

public class RoomConfig
{
	public bool IsJoiningWithFriends
	{
		get
		{
			return this.joinFriendIDs != null && this.joinFriendIDs.Length != 0;
		}
	}

	public void SetFriendIDs(List<string> friendIDs)
	{
		for (int i = 0; i < friendIDs.Count; i++)
		{
			if (friendIDs[i] == NetworkSystem.Instance.GetMyNickName())
			{
				friendIDs.RemoveAt(i);
				i--;
			}
		}
		this.joinFriendIDs = new string[friendIDs.Count];
		for (int j = 0; j < friendIDs.Count; j++)
		{
			this.joinFriendIDs[j] = friendIDs[j];
		}
	}

	public void ClearExpectedUsers()
	{
		if (this.joinFriendIDs == null || this.joinFriendIDs.Length == 0)
		{
			return;
		}
		this.joinFriendIDs = new string[0];
	}

	public RoomOptions ToPUNOpts()
	{
		return new RoomOptions
		{
			IsVisible = this.isPublic,
			IsOpen = this.isJoinable,
			MaxPlayers = this.MaxPlayers,
			CustomRoomProperties = this.CustomProps,
			PublishUserId = true,
			CustomRoomPropertiesForLobby = this.AutoCustomLobbyProps()
		};
	}

	public void SetFusionOpts(NetworkRunner runnerInst)
	{
		runnerInst.SessionInfo.IsVisible = this.isPublic;
		runnerInst.SessionInfo.IsOpen = this.isJoinable;
	}

	public static RoomConfig SPConfig()
	{
		return new RoomConfig
		{
			isPublic = false,
			isJoinable = false,
			MaxPlayers = 1
		};
	}

	public static RoomConfig AnyPublicConfig()
	{
		return new RoomConfig
		{
			isPublic = true,
			isJoinable = true,
			createIfMissing = true,
			MaxPlayers = 10
		};
	}

	private string[] AutoCustomLobbyProps()
	{
		string[] array = new string[this.CustomProps.Count];
		int num = 0;
		foreach (DictionaryEntry dictionaryEntry in this.CustomProps)
		{
			array[num] = (string)dictionaryEntry.Key;
			num++;
		}
		return array;
	}

	public const string Room_GameModePropKey = "gameMode";

	public const string Room_PlatformPropKey = "platform";

	public bool isPublic;

	public bool isJoinable;

	public byte MaxPlayers;

	public Hashtable CustomProps = new Hashtable();

	public bool createIfMissing;

	public string[] joinFriendIDs;
}
