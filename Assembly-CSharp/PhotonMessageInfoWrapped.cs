using System;
using Fusion;
using Photon.Pun;
using Photon.Realtime;

public struct PhotonMessageInfoWrapped
{
	public double SentServerTime
	{
		get
		{
			return this.sentTick / 1000.0;
		}
	}

	public PhotonMessageInfoWrapped(PhotonMessageInfo info)
	{
		Player sender = info.Sender;
		this.senderID = ((sender != null) ? sender.ActorNumber : (-1));
		this.Sender = NetPlayer.Get(info.Sender);
		this.sentTick = info.SentServerTimestamp;
		this.punInfo = info;
	}

	public PhotonMessageInfoWrapped(RpcInfo info)
	{
		this.senderID = info.Source.PlayerId;
		this.Sender = NetPlayer.Get(info.Source);
		this.sentTick = info.Tick.Raw;
		this.punInfo = default(PhotonMessageInfo);
	}

	public PhotonMessageInfoWrapped(int playerID, int tick)
	{
		this.senderID = playerID;
		this.Sender = NetworkSystem.Instance.GetPlayer(this.senderID);
		this.sentTick = tick;
		this.punInfo = default(PhotonMessageInfo);
	}

	public static implicit operator PhotonMessageInfoWrapped(PhotonMessageInfo info)
	{
		return new PhotonMessageInfoWrapped(info);
	}

	public static implicit operator PhotonMessageInfoWrapped(RpcInfo info)
	{
		return new PhotonMessageInfoWrapped(info);
	}

	public readonly int senderID;

	public readonly int sentTick;

	public readonly PhotonMessageInfo punInfo;

	public readonly NetPlayer Sender;
}
