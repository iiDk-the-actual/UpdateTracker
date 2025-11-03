using System;
using System.Collections.Generic;
using Fusion;
using GorillaTag;
using Photon.Realtime;
using UnityEngine;

[Serializable]
public abstract class NetPlayer : ObjectPoolEvents
{
	public abstract bool IsValid { get; }

	public abstract int ActorNumber { get; }

	public abstract string UserId { get; }

	public abstract bool IsMasterClient { get; }

	public abstract bool IsLocal { get; }

	public abstract bool IsNull { get; }

	public abstract string NickName { get; }

	public virtual string SanitizedNickName { get; set; } = string.Empty;

	public abstract string DefaultName { get; }

	public abstract bool InRoom { get; }

	public virtual float JoinedTime { get; private set; }

	public virtual float LeftTime { get; private set; }

	public abstract bool Equals(NetPlayer myPlayer, NetPlayer other);

	public virtual void OnReturned()
	{
		this.LeftTime = Time.time;
		HashSet<int> singleCallRPCStatus = this.SingleCallRPCStatus;
		if (singleCallRPCStatus != null)
		{
			singleCallRPCStatus.Clear();
		}
		this.SanitizedNickName = string.Empty;
	}

	public virtual void OnTaken()
	{
		this.JoinedTime = Time.time;
		HashSet<int> singleCallRPCStatus = this.SingleCallRPCStatus;
		if (singleCallRPCStatus == null)
		{
			return;
		}
		singleCallRPCStatus.Clear();
	}

	public virtual bool CheckSingleCallRPC(NetPlayer.SingleCallRPC RPCType)
	{
		return this.SingleCallRPCStatus.Contains((int)RPCType);
	}

	public virtual void ReceivedSingleCallRPC(NetPlayer.SingleCallRPC RPCType)
	{
		this.SingleCallRPCStatus.Add((int)RPCType);
	}

	public Player GetPlayerRef()
	{
		return (this as PunNetPlayer).PlayerRef;
	}

	public string ToStringFull()
	{
		return string.Format("#{0: 0:00} '{1}', Not sure what to do with inactive yet, Or custom props?", this.ActorNumber, this.NickName);
	}

	public static implicit operator NetPlayer(Player player)
	{
		Utils.Log("Using an implicit cast from Player to NetPlayer. Please make sure this was intended as this has potential to cause errors when switching between network backends");
		NetworkSystem instance = NetworkSystem.Instance;
		return ((instance != null) ? instance.GetPlayer(player) : null) ?? null;
	}

	public static implicit operator NetPlayer(PlayerRef player)
	{
		Utils.Log("Using an implicit cast from PlayerRef to NetPlayer. Please make sure this was intended as this has potential to cause errors when switching between network backends");
		NetworkSystem instance = NetworkSystem.Instance;
		return ((instance != null) ? instance.GetPlayer(player) : null) ?? null;
	}

	public static NetPlayer Get(Player player)
	{
		NetworkSystem instance = NetworkSystem.Instance;
		return ((instance != null) ? instance.GetPlayer(player) : null) ?? null;
	}

	public static NetPlayer Get(PlayerRef player)
	{
		NetworkSystem instance = NetworkSystem.Instance;
		return ((instance != null) ? instance.GetPlayer(player) : null) ?? null;
	}

	public static NetPlayer Get(int actorNr)
	{
		NetworkSystem instance = NetworkSystem.Instance;
		return ((instance != null) ? instance.GetPlayer(actorNr) : null) ?? null;
	}

	private HashSet<int> SingleCallRPCStatus = new HashSet<int>(5);

	public enum SingleCallRPC
	{
		CMS_RequestRoomInitialization,
		CMS_RequestTriggerHistory,
		CMS_SyncTriggerHistory,
		CMS_SyncTriggerCounts,
		RankedSendScoreToLateJoiner,
		Count
	}
}
