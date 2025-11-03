using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public static class NetworkSystemRaiseEvent
{
	public static void RaiseEvent(byte code, object data)
	{
		PhotonNetwork.RaiseEvent(code, data, RaiseEventOptions.Default, SendOptions.SendUnreliable);
	}

	public static void RaiseEvent(byte code, object data, NetEventOptions options, bool reliable)
	{
		PhotonNetwork.RaiseEvent(code, data, new RaiseEventOptions
		{
			TargetActors = options.TargetActors,
			Receivers = (ReceiverGroup)options.Reciever,
			Flags = options.Flags
		}, reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable);
	}

	public static readonly NetEventOptions neoOthers = new NetEventOptions
	{
		Reciever = NetEventOptions.RecieverTarget.others
	};

	public static readonly NetEventOptions neoMaster = new NetEventOptions
	{
		Reciever = NetEventOptions.RecieverTarget.master
	};

	public static readonly NetEventOptions neoTarget = new NetEventOptions
	{
		TargetActors = new int[1]
	};

	public static readonly NetEventOptions newWeb = new NetEventOptions
	{
		Flags = new WebFlags(1)
	};
}
