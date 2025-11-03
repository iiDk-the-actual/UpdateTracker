using System;
using GorillaGameModes;
using GorillaTag;
using Photon.Pun;
using UnityEngine;

public sealed class SuperInfectionGame : GorillaTagManager
{
	public new static SuperInfectionGame instance { get; private set; }

	public override GameModeType GameType()
	{
		return GameModeType.SuperInfect;
	}

	[DebugReadout]
	public ESuperInfectionGameState gameState { get; private set; }

	public override void Awake()
	{
		SuperInfectionGame.instance = this;
		this.gameState = ESuperInfectionGameState.Stopped;
		base.Awake();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		SIProgression instance = SIProgression.Instance;
		if (instance == null)
		{
			return;
		}
		instance.ResetTelemetryIntervalData();
	}

	public override void OnDisable()
	{
		base.OnDisable();
	}

	public override void Tick()
	{
		base.Tick();
	}

	public override void StartPlaying()
	{
		this.gameState = ESuperInfectionGameState.Starting;
		base.StartPlaying();
		if (NetworkSystem.Instance.IsMasterClient)
		{
			SIProgression.Instance.AddRoundTelemetry();
		}
		VRRig.LocalRig.EnableSuperInfectionHands(true);
		for (int i = 0; i < this.currentNetPlayerArray.Length; i++)
		{
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(this.currentNetPlayerArray[i], out rigContainer))
			{
				rigContainer.Rig.EnableSuperInfectionHands(true);
			}
		}
	}

	public override void StopPlaying()
	{
		base.StopPlaying();
		this.gameState = ESuperInfectionGameState.Stopped;
		VRRig.LocalRig.EnableSuperInfectionHands(false);
	}

	public override void OnPlayerEnteredRoom(NetPlayer newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(newPlayer, out rigContainer))
		{
			rigContainer.Rig.EnableSuperInfectionHands(true);
		}
	}

	public override string GameModeName()
	{
		return "SUPER INFECTION";
	}

	public override string GameModeNameRoomLabel()
	{
		string text;
		if (!LocalisationManager.TryGetKeyForCurrentLocale("GAME_MODE_SUPER_INFECTION_ROOM_LABEL", out text, "(SUPER INFECTION GAME)"))
		{
			Debug.LogError("[LOCALIZATION::GORILLA_GAME_MANAGER] Failed to get key for Game Mode [GAME_MODE_SUPER_INFECTION_ROOM_LABEL]");
		}
		return text;
	}

	public override void InfrequentUpdate()
	{
		base.InfrequentUpdate();
	}

	protected override void InfectionRoundStart()
	{
		base.InfectionRoundStart();
		this.gameState = ESuperInfectionGameState.Playing;
	}

	protected override void InfectionRoundEnd()
	{
		base.InfectionRoundEnd();
		this.gameState = ESuperInfectionGameState.RoundRestarting;
		SuperInfectionManager.activeSuperInfectionManager.zoneSuperInfection.ResetPerRoundResources();
	}

	public override bool LocalCanTag(NetPlayer myPlayer, NetPlayer otherPlayer)
	{
		return base.LocalCanTag(myPlayer, otherPlayer);
	}

	public override void UpdatePlayerAppearance(VRRig rig)
	{
		base.UpdatePlayerAppearance(rig);
	}

	public override int MyMatIndex(NetPlayer forPlayer)
	{
		return base.MyMatIndex(forPlayer);
	}

	public override void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnSerializeWrite(stream, info);
		stream.SendNext(this.gameState);
	}

	public override void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnSerializeRead(stream, info);
		ESuperInfectionGameState esuperInfectionGameState = (ESuperInfectionGameState)stream.ReceiveNext();
		if (!Enum.IsDefined(typeof(ESuperInfectionGameState), this.gameState))
		{
			return;
		}
		this.gameState = esuperInfectionGameState;
		if (this.gameState != this._gameState_previous)
		{
			this._OnGameStateChanged();
			this._gameState_previous = this.gameState;
		}
	}

	public void _OnGameStateChanged()
	{
		if (this.gameState == ESuperInfectionGameState.Starting)
		{
			SIProgression.Instance.AddRoundTelemetry();
		}
		GTDev.Log<string>(string.Format("Game state changed to {0} ...\n(was {1}).", this.gameState, this._gameState_previous), null);
	}

	public override void HandleTagBroadcast(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		try
		{
			SIProgression.Instance.HandleTagTelemetry(taggedPlayer, taggingPlayer);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex, this);
		}
		RigContainer rigContainer;
		RigContainer rigContainer2;
		if (!VRRigCache.Instance.TryGetVrrig(taggedPlayer, out rigContainer) || !VRRigCache.Instance.TryGetVrrig(taggingPlayer, out rigContainer2))
		{
			return;
		}
		if (taggingPlayer.ActorNumber != SIPlayer.LocalPlayer.ActorNr)
		{
			return;
		}
		if (SIProgression.Instance.HeldOrSnappedByGadgetPageType[SITechTreePageId.Dash] > 0)
		{
			PlayerGameEvents.MiscEvent("SIDashTag", 1);
		}
		if (SIProgression.Instance.HeldOrSnappedByGadgetPageType[SITechTreePageId.Thruster] > 0)
		{
			PlayerGameEvents.MiscEvent("SIThrusterTag", 1);
		}
		if (SIProgression.Instance.HeldOrSnappedByGadgetPageType[SITechTreePageId.Stilt] > 0)
		{
			PlayerGameEvents.MiscEvent("SIStiltTag", 1);
		}
		if (SIProgression.Instance.HeldOrSnappedByGadgetPageType[SITechTreePageId.Platform] > 0)
		{
			PlayerGameEvents.MiscEvent("SIPlatformTag", 1);
		}
		if (SIProgression.Instance.HeldOrSnappedOthersGadgets)
		{
			PlayerGameEvents.MiscEvent("SIBorrowedGadgetTag", 1);
		}
		PlayerGameEvents.MiscEvent("SIGameModeTag", 1);
	}

	[SerializeField]
	private int _mySuperExampleSerializedField = 123;

	private ESuperInfectionGameState _gameState_previous;
}
