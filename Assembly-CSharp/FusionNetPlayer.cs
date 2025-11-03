using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FusionNetPlayer : NetPlayer
{
	public PlayerRef PlayerRef { get; private set; }

	public FusionNetPlayer()
	{
		this.PlayerRef = default(PlayerRef);
	}

	public FusionNetPlayer(PlayerRef playerRef)
	{
		this.PlayerRef = playerRef;
	}

	private NetworkRunner runner
	{
		get
		{
			return ((NetworkSystemFusion)NetworkSystem.Instance).runner;
		}
	}

	public override bool IsValid
	{
		get
		{
			return this.validPlayer && this.PlayerRef.IsRealPlayer;
		}
	}

	public override int ActorNumber
	{
		get
		{
			return this.PlayerRef.PlayerId;
		}
	}

	public override string UserId
	{
		get
		{
			return NetworkSystem.Instance.GetUserID(this.PlayerRef.PlayerId);
		}
	}

	public override bool IsMasterClient
	{
		get
		{
			if (!(this.runner == null))
			{
				return (this.IsLocal && this.runner.IsSharedModeMasterClient) || NetworkSystem.Instance.MasterClient == this;
			}
			return this.PlayerRef == default(PlayerRef);
		}
	}

	public override bool IsLocal
	{
		get
		{
			if (!(this.runner == null))
			{
				return this.PlayerRef == this.runner.LocalPlayer;
			}
			return this.PlayerRef == default(PlayerRef);
		}
	}

	public override bool IsNull
	{
		get
		{
			PlayerRef playerRef = this.PlayerRef;
			return false;
		}
	}

	public override string NickName
	{
		get
		{
			return NetworkSystem.Instance.GetNickName(this);
		}
	}

	public override string DefaultName
	{
		get
		{
			if (string.IsNullOrEmpty(this._defaultName))
			{
				this._defaultName = "gorilla" + Random.Range(0, 9999).ToString().PadLeft(4, '0');
			}
			return this._defaultName;
		}
	}

	public override bool InRoom
	{
		get
		{
			using (IEnumerator<PlayerRef> enumerator = this.runner.ActivePlayers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == this.PlayerRef)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public override bool Equals(NetPlayer myPlayer, NetPlayer other)
	{
		return myPlayer != null && other != null && ((FusionNetPlayer)myPlayer).PlayerRef.Equals(((FusionNetPlayer)other).PlayerRef);
	}

	public void InitPlayer(PlayerRef player)
	{
		this.PlayerRef = player;
		this.validPlayer = true;
	}

	public override void OnReturned()
	{
		base.OnReturned();
		this.PlayerRef = default(PlayerRef);
		if (this.PlayerRef.PlayerId != -1)
		{
			Debug.LogError("Returned Player to pool but isnt -1, broken");
		}
	}

	public override void OnTaken()
	{
		base.OnTaken();
	}

	private string _defaultName;

	private bool validPlayer;
}
