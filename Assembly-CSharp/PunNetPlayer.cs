using System;
using Photon.Pun;
using Photon.Realtime;

[Serializable]
public class PunNetPlayer : NetPlayer
{
	public Player PlayerRef { get; private set; }

	public void InitPlayer(Player playerRef)
	{
		this.PlayerRef = playerRef;
	}

	public override bool IsValid
	{
		get
		{
			return !this.PlayerRef.IsInactive;
		}
	}

	public override int ActorNumber
	{
		get
		{
			Player playerRef = this.PlayerRef;
			if (playerRef == null)
			{
				return -1;
			}
			return playerRef.ActorNumber;
		}
	}

	public override string UserId
	{
		get
		{
			return this.PlayerRef.UserId;
		}
	}

	public override bool IsMasterClient
	{
		get
		{
			return this.PlayerRef.IsMasterClient;
		}
	}

	public override bool IsLocal
	{
		get
		{
			return this.PlayerRef == PhotonNetwork.LocalPlayer;
		}
	}

	public override bool IsNull
	{
		get
		{
			return this.PlayerRef == null;
		}
	}

	public override string NickName
	{
		get
		{
			return this.PlayerRef.NickName;
		}
	}

	public override string DefaultName
	{
		get
		{
			return this.PlayerRef.DefaultName;
		}
	}

	public override bool InRoom
	{
		get
		{
			Room currentRoom = PhotonNetwork.CurrentRoom;
			return currentRoom != null && currentRoom.Players.ContainsValue(this.PlayerRef);
		}
	}

	public override bool Equals(NetPlayer myPlayer, NetPlayer other)
	{
		return myPlayer != null && other != null && ((PunNetPlayer)myPlayer).PlayerRef.Equals(((PunNetPlayer)other).PlayerRef);
	}

	public override void OnReturned()
	{
		base.OnReturned();
	}

	public override void OnTaken()
	{
		base.OnTaken();
		this.PlayerRef = null;
	}
}
