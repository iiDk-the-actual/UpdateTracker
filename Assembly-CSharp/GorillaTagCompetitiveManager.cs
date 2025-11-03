using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaGameModes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GorillaTagCompetitiveManager : GorillaTagManager
{
	public float GetRoundDuration()
	{
		return this.roundDuration;
	}

	public GorillaTagCompetitiveManager.GameState GetCurrentGameState()
	{
		return this.gameState;
	}

	public bool IsMatchActive()
	{
		return this.gameState == GorillaTagCompetitiveManager.GameState.Playing;
	}

	public static event Action<GorillaTagCompetitiveManager.GameState> onStateChanged;

	public static event Action<float> onUpdateRemainingTime;

	public static event Action<NetPlayer> onPlayerJoined;

	public static event Action<NetPlayer> onPlayerLeft;

	public static event Action onRoundStart;

	public static event Action onRoundEnd;

	public static event Action<NetPlayer, NetPlayer> onTagOccurred;

	public static void RegisterScoreboard(GorillaTagCompetitiveScoreboard scoreboard)
	{
		GorillaTagCompetitiveManager.scoreboards.Add(scoreboard);
	}

	public static void DeregisterScoreboard(GorillaTagCompetitiveScoreboard scoreboard)
	{
		GorillaTagCompetitiveManager.scoreboards.Remove(scoreboard);
	}

	public override void StartPlaying()
	{
		base.StartPlaying();
		this.scoring = base.GetComponentInChildren<RankedMultiplayerScore>();
		if (this.scoring != null)
		{
			this.scoring.Initialize();
		}
		VRRig.LocalRig.EnableRankedTimerWatch(true);
		for (int i = 0; i < this.currentNetPlayerArray.Length; i++)
		{
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(this.currentNetPlayerArray[i], out rigContainer))
			{
				rigContainer.Rig.EnableRankedTimerWatch(true);
			}
		}
	}

	public override void StopPlaying()
	{
		base.StopPlaying();
		VRRig.LocalRig.EnableRankedTimerWatch(false);
		if (this.scoring != null)
		{
			this.scoring.ResetMatch();
			this.scoring.Unsubscribe();
		}
		for (int i = 0; i < GorillaTagCompetitiveManager.scoreboards.Count; i++)
		{
			GorillaTagCompetitiveManager.scoreboards[i].UpdateScores(this.gameState, this.lastActiveTime, null, this.scoring.PlayerRankedTiers, this.scoring.ProjectedEloDeltas, this.currentInfected, this.scoring.Progression);
		}
	}

	public override void ResetGame()
	{
		base.ResetGame();
		this.gameState = GorillaTagCompetitiveManager.GameState.None;
	}

	internal override void NetworkLinkSetup(GameModeSerializer netSerializer)
	{
		base.NetworkLinkSetup(netSerializer);
		netSerializer.AddRPCComponent<GorillaTagCompetitiveRPCs>();
	}

	public override void Tick()
	{
		if (this.stateRemainingTime > 0f)
		{
			this.stateRemainingTime -= Time.deltaTime;
			if (this.stateRemainingTime <= 0f)
			{
				this.UpdateState();
			}
			Action<float> action = GorillaTagCompetitiveManager.onUpdateRemainingTime;
			if (action != null)
			{
				action(this.stateRemainingTime);
			}
		}
		base.Tick();
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (Time.time - this.lastWaitingForPlayerPingRoomTime > this.waitingForPlayerPingRoomDuration)
			{
				this.PingRoom();
				this.lastWaitingForPlayerPingRoomTime = Time.time;
			}
			if (Time.time - this.lastWaitingForPlayerPingRoomTime > 3f)
			{
				this.ShowDebugPing = false;
			}
		}
		this.UpdateScoreboards();
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		base.OnMasterClientSwitched(newMasterClient);
		if (NetworkSystem.Instance.IsMasterClient)
		{
			this.PingRoom();
			this.lastWaitingForPlayerPingRoomTime = Time.time;
		}
	}

	public override void OnPlayerEnteredRoom(NetPlayer newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		if (newPlayer == NetworkSystem.Instance.LocalPlayer)
		{
			using (List<GorillaTagCompetitiveForcedLeaveRoomVolume>.Enumerator enumerator = this.forceLeaveRoomVolumes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.ContainsPoint(VRRig.LocalRig.transform.position))
					{
						NetworkSystem.Instance.ReturnToSinglePlayer();
						return;
					}
				}
			}
			object obj;
			if (NetworkSystem.Instance.IsMasterClient)
			{
				GorillaTagCompetitiveServerApi.Instance.RequestCreateMatchId(delegate(string id)
				{
					Hashtable hashtable = new Hashtable();
					hashtable.Add("matchId", id);
					PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable, null, null);
				});
			}
			else if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("matchId", out obj))
			{
				GorillaTagCompetitiveServerApi.Instance.RequestValidateMatchJoin((string)obj, delegate(bool valid)
				{
					if (!valid)
					{
						Debug.LogError("ValidateMatchJoin failed. Leaving room!");
						NetworkSystem.Instance.ReturnToSinglePlayer();
					}
				});
			}
		}
		Action<NetPlayer> action = GorillaTagCompetitiveManager.onPlayerJoined;
		if (action != null)
		{
			action(newPlayer);
		}
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(newPlayer, out rigContainer))
		{
			rigContainer.Rig.EnableRankedTimerWatch(true);
		}
	}

	public override void OnPlayerLeftRoom(NetPlayer otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		Action<NetPlayer> action = GorillaTagCompetitiveManager.onPlayerLeft;
		if (action != null)
		{
			action(otherPlayer);
		}
		RigContainer rigContainer;
		if (VRRigCache.Instance.TryGetVrrig(otherPlayer, out rigContainer))
		{
			rigContainer.Rig.EnableRankedTimerWatch(false);
		}
	}

	public RankedMultiplayerScore GetScoring()
	{
		return this.scoring;
	}

	public override bool LocalCanTag(NetPlayer myPlayer, NetPlayer otherPlayer)
	{
		return base.LocalCanTag(myPlayer, otherPlayer) && this.gameState != GorillaTagCompetitiveManager.GameState.StartingCountdown && this.gameState != GorillaTagCompetitiveManager.GameState.PostRound;
	}

	public override bool LocalIsTagged(NetPlayer player)
	{
		return this.gameState != GorillaTagCompetitiveManager.GameState.StartingCountdown && this.gameState != GorillaTagCompetitiveManager.GameState.PostRound && base.LocalIsTagged(player);
	}

	public override void ReportTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		base.ReportTag(taggedPlayer, taggingPlayer);
	}

	public override GameModeType GameType()
	{
		return GameModeType.InfectionCompetitive;
	}

	public override string GameModeName()
	{
		return "COMP-INFECT";
	}

	public override string GameModeNameRoomLabel()
	{
		string text;
		if (!LocalisationManager.TryGetKeyForCurrentLocale("GAME_MODE_COMP_INF_ROOM_LABEL", out text, "(COMP-INFECT GAME)"))
		{
			Debug.LogError("[LOCALIZATION::GORILLA_GAME_MANAGER] Failed to get key for Game Mode [GAME_MODE_COMP_INF_ROOM_LABEL]");
		}
		return text;
	}

	public override bool CanJoinFrienship(NetPlayer player)
	{
		return false;
	}

	public override void UpdateInfectionState()
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		if (this.gameState == GorillaTagCompetitiveManager.GameState.Playing && this.IsEveryoneTagged())
		{
			this.HandleInfectionRoundComplete();
		}
	}

	public override void HandleTagBroadcast(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		if (!this.currentInfected.Contains(taggingPlayer))
		{
			return;
		}
		RigContainer rigContainer;
		RigContainer rigContainer2;
		if (VRRigCache.Instance.TryGetVrrig(taggedPlayer, out rigContainer) && VRRigCache.Instance.TryGetVrrig(taggingPlayer, out rigContainer2))
		{
			VRRig rig = rigContainer2.Rig;
			VRRig rig2 = rigContainer.Rig;
			if (!rig.IsPositionInRange(rig2.transform.position, 6f) && !rig.CheckTagDistanceRollback(rig2, 6f, 0.2f))
			{
				return;
			}
			if (!NetworkSystem.Instance.IsMasterClient && this.gameState == GorillaTagCompetitiveManager.GameState.Playing && !this.currentInfected.Contains(taggedPlayer))
			{
				base.AddLastTagged(taggedPlayer, taggingPlayer);
				this.currentInfected.Add(taggedPlayer);
			}
			Action<NetPlayer, NetPlayer> action = GorillaTagCompetitiveManager.onTagOccurred;
			if (action == null)
			{
				return;
			}
			action(taggedPlayer, taggingPlayer);
		}
	}

	private void SetState(GorillaTagCompetitiveManager.GameState newState)
	{
		if (newState != this.gameState)
		{
			GorillaTagCompetitiveManager.GameState gameState = this.gameState;
			this.gameState = newState;
			switch (this.gameState)
			{
			case GorillaTagCompetitiveManager.GameState.WaitingForPlayers:
				this.EnterStateWaitingForPlayers();
				break;
			case GorillaTagCompetitiveManager.GameState.StartingCountdown:
				this.EnterStateStartingCountdown();
				break;
			case GorillaTagCompetitiveManager.GameState.Playing:
				this.EnterStatePlaying();
				break;
			case GorillaTagCompetitiveManager.GameState.PostRound:
				this.EnterStatePostRound();
				break;
			}
			Action<GorillaTagCompetitiveManager.GameState> action = GorillaTagCompetitiveManager.onStateChanged;
			if (action != null)
			{
				action(this.gameState);
			}
			Action<float> action2 = GorillaTagCompetitiveManager.onUpdateRemainingTime;
			if (action2 != null)
			{
				action2(this.stateRemainingTime);
			}
			if (this.gameState == GorillaTagCompetitiveManager.GameState.Playing)
			{
				Action action3 = GorillaTagCompetitiveManager.onRoundStart;
				if (action3 != null)
				{
					action3();
				}
			}
			else if (gameState == GorillaTagCompetitiveManager.GameState.Playing)
			{
				Action action4 = GorillaTagCompetitiveManager.onRoundEnd;
				if (action4 != null)
				{
					action4();
				}
			}
			GTDev.Log<string>(string.Format("!! Competitive SetState: {0} at: {1}", this.gameState, Time.time), null);
		}
	}

	private void EnterStateWaitingForPlayers()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			base.SetisCurrentlyTag(true);
			base.ClearInfectionState();
		}
	}

	private void EnterStateStartingCountdown()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (this.isCurrentlyTag)
			{
				base.SetisCurrentlyTag(false);
			}
			this.currentIt = null;
			base.ClearInfectionState();
			GameMode.RefreshPlayers();
			this.CheckForInfected();
			this.stateRemainingTime = this.startCountdownDuration;
		}
	}

	private void EnterStatePlaying()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (this.isCurrentlyTag)
			{
				base.SetisCurrentlyTag(false);
			}
			this.currentIt = null;
			this.stateRemainingTime = this.roundDuration;
			this.PingRoom();
		}
		this.DisplayScoreboardPredictedResults(false);
	}

	private void EnterStatePostRound()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			if (this.isCurrentlyTag)
			{
				base.SetisCurrentlyTag(false);
			}
			this.currentIt = null;
			this.stateRemainingTime = this.postRoundDuration;
		}
		this.DisplayScoreboardPredictedResults(true);
	}

	public override void UpdateState()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			switch (this.gameState)
			{
			case GorillaTagCompetitiveManager.GameState.None:
				this.SetState(GorillaTagCompetitiveManager.GameState.WaitingForPlayers);
				return;
			case GorillaTagCompetitiveManager.GameState.WaitingForPlayers:
				this.UpdateStateWaitingForPlayers();
				return;
			case GorillaTagCompetitiveManager.GameState.StartingCountdown:
				this.UpdateStateStartingCountdown();
				return;
			case GorillaTagCompetitiveManager.GameState.Playing:
				this.UpdateStatePlaying();
				return;
			case GorillaTagCompetitiveManager.GameState.PostRound:
				this.UpdateStatePostRound();
				break;
			default:
				return;
			}
		}
	}

	private void UpdateStateWaitingForPlayers()
	{
		if (this.IsInfectionPossible())
		{
			this.SetState(GorillaTagCompetitiveManager.GameState.StartingCountdown);
			return;
		}
		if (this.isCurrentlyTag && this.currentIt == null)
		{
			int num = Random.Range(0, GameMode.ParticipatingPlayers.Count);
			this.ChangeCurrentIt(GameMode.ParticipatingPlayers[num], false);
		}
	}

	private void UpdateStateStartingCountdown()
	{
		if (!this.IsInfectionPossible())
		{
			this.SetState(GorillaTagCompetitiveManager.GameState.WaitingForPlayers);
			return;
		}
		if (this.stateRemainingTime < 0f)
		{
			this.SetState(GorillaTagCompetitiveManager.GameState.Playing);
			return;
		}
		this.CheckForInfected();
	}

	private void UpdateStatePlaying()
	{
		if (this.IsGameInvalid())
		{
			this.SetState(GorillaTagCompetitiveManager.GameState.WaitingForPlayers);
			return;
		}
		if (this.stateRemainingTime < 0f)
		{
			this.HandleInfectionRoundComplete();
			return;
		}
		if (this.IsEveryoneTagged())
		{
			this.HandleInfectionRoundComplete();
			return;
		}
		this.CheckForInfected();
	}

	private void HandleInfectionRoundComplete()
	{
		foreach (NetPlayer netPlayer in GameMode.ParticipatingPlayers)
		{
			RoomSystem.SendSoundEffectToPlayer(2, 0.25f, netPlayer, true);
		}
		PlayerGameEvents.GameModeCompleteRound();
		GameMode.BroadcastRoundComplete();
		this.lastTaggedActorNr.Clear();
		this.waitingToStartNextInfectionGame = true;
		this.timeInfectedGameEnded = (double)Time.time;
		this.SetState(GorillaTagCompetitiveManager.GameState.PostRound);
	}

	private void UpdateStatePostRound()
	{
		if (this.stateRemainingTime < 0f)
		{
			if (this.IsInfectionPossible())
			{
				this.SetState(GorillaTagCompetitiveManager.GameState.StartingCountdown);
				return;
			}
			this.SetState(GorillaTagCompetitiveManager.GameState.WaitingForPlayers);
		}
	}

	private void PingRoom()
	{
		object obj;
		if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("matchId", out obj))
		{
			GorillaTagCompetitiveServerApi.Instance.RequestPingRoom((string)obj, delegate
			{
				this.ShowDebugPing = true;
			});
		}
	}

	public bool ShowDebugPing { get; set; }

	private bool IsGameInvalid()
	{
		return GameMode.ParticipatingPlayers.Count <= 1;
	}

	private bool IsInfectionPossible()
	{
		return GameMode.ParticipatingPlayers.Count >= this.infectedModeThreshold;
	}

	private bool IsEveryoneTagged()
	{
		bool flag = true;
		foreach (NetPlayer netPlayer in GameMode.ParticipatingPlayers)
		{
			if (!this.currentInfected.Contains(netPlayer))
			{
				flag = false;
				break;
			}
		}
		return flag;
	}

	private void CheckForInfected()
	{
		if (this.currentInfected.Count == 0)
		{
			int num = Random.Range(0, GameMode.ParticipatingPlayers.Count);
			this.AddInfectedPlayer(GameMode.ParticipatingPlayers[num], true);
		}
	}

	public override void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnSerializeWrite(stream, info);
		stream.SendNext(this.gameState);
		stream.SendNext(this.stateRemainingTime);
	}

	public override void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
		NetworkSystem.Instance.GetPlayer(info.Sender);
		base.OnSerializeRead(stream, info);
		GorillaTagCompetitiveManager.GameState gameState = (GorillaTagCompetitiveManager.GameState)stream.ReceiveNext();
		this.stateRemainingTime = (float)stream.ReceiveNext();
		this.SetState(gameState);
	}

	public void UpdateScoreboards()
	{
		List<RankedMultiplayerScore.PlayerScoreInRound> sortedScores = this.scoring.GetSortedScores();
		if (this.gameState == GorillaTagCompetitiveManager.GameState.Playing)
		{
			this.lastActiveTime = Time.time;
		}
		for (int i = 0; i < GorillaTagCompetitiveManager.scoreboards.Count; i++)
		{
			GorillaTagCompetitiveManager.scoreboards[i].UpdateScores(this.gameState, this.lastActiveTime, sortedScores, this.scoring.PlayerRankedTiers, this.scoring.ProjectedEloDeltas, this.currentInfected, this.scoring.Progression);
		}
	}

	public void DisplayScoreboardPredictedResults(bool bShow)
	{
		for (int i = 0; i < GorillaTagCompetitiveManager.scoreboards.Count; i++)
		{
			GorillaTagCompetitiveManager.scoreboards[i].DisplayPredictedResults(bShow);
		}
	}

	public void RegisterForcedLeaveVolume(GorillaTagCompetitiveForcedLeaveRoomVolume volume)
	{
		if (!this.forceLeaveRoomVolumes.Contains(volume))
		{
			this.forceLeaveRoomVolumes.Add(volume);
		}
	}

	public void UnregisterForcedLeaveVolume(GorillaTagCompetitiveForcedLeaveRoomVolume volume)
	{
		this.forceLeaveRoomVolumes.Remove(volume);
	}

	[SerializeField]
	private float startCountdownDuration = 3f;

	[SerializeField]
	private float roundDuration = 300f;

	[SerializeField]
	private float postRoundDuration = 15f;

	[SerializeField]
	private float waitingForPlayerPingRoomDuration = 60f;

	private GorillaTagCompetitiveManager.GameState gameState;

	private float stateRemainingTime;

	private float lastActiveTime;

	private float lastWaitingForPlayerPingRoomTime;

	private RankedMultiplayerScore scoring;

	private List<GorillaTagCompetitiveForcedLeaveRoomVolume> forceLeaveRoomVolumes = new List<GorillaTagCompetitiveForcedLeaveRoomVolume>();

	private static List<GorillaTagCompetitiveScoreboard> scoreboards = new List<GorillaTagCompetitiveScoreboard>();

	public enum GameState
	{
		None,
		WaitingForPlayers,
		StartingCountdown,
		Playing,
		PostRound
	}
}
