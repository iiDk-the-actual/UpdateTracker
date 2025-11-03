using System;
using System.Collections.Generic;
using System.Linq;
using GorillaGameModes;
using Photon.Pun;
using UnityEngine;

public class RankedMultiplayerScore : MonoBehaviourTick
{
	public RankedProgressionManager Progression { get; private set; }

	public void Initialize()
	{
		GorillaTagCompetitiveManager.onStateChanged += this.OnStateChanged;
		GorillaTagCompetitiveManager.onRoundStart += this.OnGameStarted;
		GorillaTagCompetitiveManager.onRoundEnd += this.OnGameEnded;
		GorillaTagCompetitiveManager.onPlayerJoined += this.OnPlayerJoined;
		GorillaTagCompetitiveManager.onPlayerLeft += this.OnPlayerLeft;
		GorillaTagCompetitiveManager.onTagOccurred += this.OnTagReported;
		GorillaGameManager instance = GorillaGameManager.instance;
		if (instance != null)
		{
			this.CompetitiveManager = instance as GorillaTagCompetitiveManager;
		}
		this.Progression = RankedProgressionManager.Instance;
		RankedProgressionManager progression = this.Progression;
		progression.OnPlayerEloAcquired = (Action<int, float, int>)Delegate.Combine(progression.OnPlayerEloAcquired, new Action<int, float, int>(this.HandlePlayerEloAcquired));
	}

	private void HandlePlayerEloAcquired(int playerId, float elo, int tier)
	{
		this.CachePlayerRankedProgressionData(playerId, tier, elo);
	}

	private void OnDestroy()
	{
		this.Unsubscribe();
	}

	public void Unsubscribe()
	{
		GorillaTagCompetitiveManager.onStateChanged -= this.OnStateChanged;
		GorillaTagCompetitiveManager.onRoundStart -= this.OnGameStarted;
		GorillaTagCompetitiveManager.onRoundEnd -= this.OnGameEnded;
		GorillaTagCompetitiveManager.onPlayerJoined -= this.OnPlayerJoined;
		GorillaTagCompetitiveManager.onPlayerLeft -= this.OnPlayerLeft;
		GorillaTagCompetitiveManager.onTagOccurred -= this.OnTagReported;
		if (this.Progression != null)
		{
			RankedProgressionManager progression = this.Progression;
			progression.OnPlayerEloAcquired = (Action<int, float, int>)Delegate.Remove(progression.OnPlayerEloAcquired, new Action<int, float, int>(this.HandlePlayerEloAcquired));
		}
	}

	public override void Tick()
	{
		if (this.PerSecondTimer > 0f && Time.time >= this.PerSecondTimer + 1f)
		{
			if (this.CompetitiveManager == null)
			{
				return;
			}
			this.OnPerSecondTimerElapsed(NetworkSystem.Instance.AllNetPlayers.Length, this.CompetitiveManager.currentInfected.Count);
			this.PerSecondTimer = Time.time;
		}
	}

	private void OnPerSecondTimerElapsed(int playersInGame, int infectedPlayers)
	{
		foreach (int num in this.AllPlayerInRoundScores.Keys.ToList<int>())
		{
			RankedMultiplayerScore.PlayerScoreInRound playerScoreInRound = this.AllPlayerInRoundScores[num];
			playerScoreInRound.Infected = this.CompetitiveManager.IsInfected(NetworkSystem.Instance.GetPlayer(num));
			if (!playerScoreInRound.Infected)
			{
				float num2 = (float)infectedPlayers / (float)playersInGame;
				playerScoreInRound.PointsOnDefense += Mathf.Lerp(this.PointsPerUninfectedSecMin, this.PointsPerUninfectedSecMax, num2);
			}
			this.AllPlayerInRoundScores[num] = playerScoreInRound;
		}
	}

	public void ResetMatch()
	{
		this.AllFinalPlayerScores.Clear();
		this.AllPlayerInRoundScores.Clear();
	}

	private void OnStateChanged(GorillaTagCompetitiveManager.GameState state)
	{
		if (state == GorillaTagCompetitiveManager.GameState.StartingCountdown)
		{
			this.OnGameStarted();
			this.Progression.AcquireRoomRankInformation(true);
		}
	}

	public void OnGameStarted()
	{
		this.PerSecondTimer = Time.time;
		if (!this.IsLateJoiner)
		{
			this.ResetMatch();
			for (int i = 0; i < NetworkSystem.Instance.AllNetPlayers.Length; i++)
			{
				this.StartTrackingPlayer(NetworkSystem.Instance.AllNetPlayers[i], false);
			}
		}
	}

	public void OnGameEnded()
	{
		foreach (int num in this.AllPlayerInRoundScores.Keys.ToList<int>())
		{
			RankedMultiplayerScore.PlayerScoreInRound playerScoreInRound = this.AllPlayerInRoundScores[num];
			if (!playerScoreInRound.Infected)
			{
				playerScoreInRound.TaggedTime = Time.time;
			}
			this.AllPlayerInRoundScores[num] = playerScoreInRound;
		}
		this.PerSecondTimer = -1f;
		this.ReportScore();
		this.WasInfectedInitially = false;
		this.IsLateJoiner = false;
	}

	private void OnPlayerJoined(NetPlayer player)
	{
		if (NetworkSystem.Instance.IsMasterClient && this.CompetitiveManager.IsMatchActive())
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			List<float> list3 = new List<float>();
			List<float> list4 = new List<float>();
			List<bool> list5 = new List<bool>();
			List<float> list6 = new List<float>();
			foreach (KeyValuePair<int, RankedMultiplayerScore.PlayerScoreInRound> keyValuePair in this.AllPlayerInRoundScores)
			{
				list.Add(keyValuePair.Value.PlayerId);
				list2.Add(keyValuePair.Value.NumTags);
				list3.Add(keyValuePair.Value.PointsOnDefense);
				list4.Add(Time.time - keyValuePair.Value.JoinTime);
				list5.Add(keyValuePair.Value.Infected);
				if (!keyValuePair.Value.Infected)
				{
					list6.Add(0f);
				}
				else
				{
					list6.Add(Time.time - keyValuePair.Value.TaggedTime);
				}
			}
			GameMode.ActiveNetworkHandler.SendRPC("SendScoresToLateJoinerRPC", player, new object[]
			{
				list.ToArray(),
				list2.ToArray(),
				list3.ToArray(),
				list4.ToArray(),
				list5.ToArray(),
				list6.ToArray()
			});
		}
		this.StartTrackingPlayer(player, true);
	}

	public void ReceivedScoresForLateJoiner(int[] playerIds, int[] numTags, float[] pointsOnDefense, float[] joinTime, bool[] infected, float[] taggedTime)
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			this.IsLateJoiner = true;
			for (int i = 0; i < playerIds.Length; i++)
			{
				int num = playerIds[i];
				RankedMultiplayerScore.PlayerScoreInRound playerScoreInRound = new RankedMultiplayerScore.PlayerScoreInRound(num, infected[i]);
				playerScoreInRound.NumTags = numTags[i];
				playerScoreInRound.PointsOnDefense = pointsOnDefense[i];
				playerScoreInRound.JoinTime = Time.time - joinTime[i];
				if (!infected[i])
				{
					playerScoreInRound.TaggedTime = 0f;
				}
				else
				{
					playerScoreInRound.TaggedTime = Time.time - taggedTime[i];
				}
				this.AllPlayerInRoundScores.TryAdd(num, playerScoreInRound);
			}
		}
	}

	private void OnPlayerLeft(NetPlayer player)
	{
		this.AllPlayerInRoundScores.Remove(player.ActorNumber);
	}

	private void StartTrackingPlayer(NetPlayer player, bool lateJoin)
	{
		bool flag = lateJoin;
		if (!lateJoin && this.CompetitiveManager != null)
		{
			flag = this.CompetitiveManager.IsInfected(player);
			if (player.ActorNumber == NetworkSystem.Instance.LocalPlayerID)
			{
				this.WasInfectedInitially = true;
			}
		}
		if (player == NetworkSystem.Instance.LocalPlayer)
		{
			this.CachePlayerRankedProgressionData(player.ActorNumber, this.Progression.GetProgressionRankIndex(), this.Progression.GetEloScore());
		}
		this.AllPlayerInRoundScores.TryAdd(player.ActorNumber, new RankedMultiplayerScore.PlayerScoreInRound(player.ActorNumber, flag));
	}

	public RankedMultiplayerScore.PlayerScoreInRound GetInGameScoreForSelf()
	{
		RankedMultiplayerScore.PlayerScoreInRound playerScoreInRound;
		if (this.AllPlayerInRoundScores.TryGetValue(NetworkSystem.Instance.LocalPlayerID, out playerScoreInRound))
		{
			return playerScoreInRound;
		}
		return default(RankedMultiplayerScore.PlayerScoreInRound);
	}

	public void OnTagReported(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
	{
		RankedMultiplayerScore.PlayerScoreInRound playerScoreInRound;
		if (this.AllPlayerInRoundScores.TryGetValue(taggingPlayer.ActorNumber, out playerScoreInRound))
		{
			playerScoreInRound.NumTags++;
			this.AllPlayerInRoundScores[taggingPlayer.ActorNumber] = playerScoreInRound;
		}
		RankedMultiplayerScore.PlayerScoreInRound playerScoreInRound2;
		if (this.AllPlayerInRoundScores.TryGetValue(taggedPlayer.ActorNumber, out playerScoreInRound2))
		{
			playerScoreInRound2.Infected = true;
			playerScoreInRound2.TaggedTime = Time.time;
			this.AllPlayerInRoundScores[taggedPlayer.ActorNumber] = playerScoreInRound2;
		}
	}

	private void ReportScore()
	{
		object obj;
		if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("matchId", out obj))
		{
			foreach (KeyValuePair<int, RankedMultiplayerScore.PlayerScoreInRound> keyValuePair in this.AllPlayerInRoundScores)
			{
				this.AllFinalPlayerScores.Add(new RankedMultiplayerScore.PlayerScore
				{
					PlayerId = keyValuePair.Key,
					GameScore = this.ComputeGameScore(keyValuePair.Value.NumTags, keyValuePair.Value.PointsOnDefense),
					EloScore = (this.PlayerRankedElos.ContainsKey(keyValuePair.Key) ? this.PlayerRankedElos[keyValuePair.Key] : 0f),
					NumTags = keyValuePair.Value.NumTags,
					TimeUntagged = keyValuePair.Value.TaggedTime - keyValuePair.Value.JoinTime,
					PointsOnDefense = keyValuePair.Value.PointsOnDefense
				});
			}
			GorillaTagCompetitiveServerApi.Instance.RequestSubmitMatchScores((string)obj, this.AllFinalPlayerScores);
		}
		this.PredictPlayerEloChanges();
	}

	public float ComputeGameScore(int tags, float pointsOnDefense)
	{
		return (float)(tags * this.PointsPerTag) + pointsOnDefense;
	}

	private void PredictPlayerEloChanges()
	{
		this.VisitedScoreCombintations.Clear();
		this.AllFinalPlayerScores = this.AllFinalPlayerScores.OrderByDescending((RankedMultiplayerScore.PlayerScore s) => s.GameScore).ToList<RankedMultiplayerScore.PlayerScore>();
		float num = this.Progression.MaxEloConstant / (float)(this.AllFinalPlayerScores.Count - 1);
		this.InProgressEloDeltaPerPlayer.Clear();
		for (int i = 0; i < this.AllFinalPlayerScores.Count; i++)
		{
			this.InProgressEloDeltaPerPlayer.Add(this.AllFinalPlayerScores[i].PlayerId, 0f);
		}
		for (int j = 0; j < this.AllFinalPlayerScores.Count; j++)
		{
			for (int k = 0; k < this.AllFinalPlayerScores.Count; k++)
			{
				if (j != k)
				{
					bool flag = this.AllFinalPlayerScores[j].GameScore.Approx(this.AllFinalPlayerScores[k].GameScore, 1E-06f);
					float eloWinProbability = RankedProgressionManager.GetEloWinProbability(this.AllFinalPlayerScores[k].EloScore, this.AllFinalPlayerScores[j].EloScore);
					float eloWinProbability2 = RankedProgressionManager.GetEloWinProbability(this.AllFinalPlayerScores[j].EloScore, this.AllFinalPlayerScores[k].EloScore);
					int num2 = j * this.AllFinalPlayerScores.Count + k;
					if (!this.VisitedScoreCombintations.ContainsKey(num2))
					{
						RankedMultiplayerScore.PlayerScore playerScore = this.AllFinalPlayerScores[j];
						float num3;
						if (flag)
						{
							num3 = 0.5f;
						}
						else
						{
							num3 = (float)((j < k) ? 1 : 0);
						}
						float eloScore = playerScore.EloScore;
						float num4 = RankedProgressionManager.UpdateEloScore(eloScore, eloWinProbability, num3, num);
						Dictionary<int, float> dictionary = this.InProgressEloDeltaPerPlayer;
						int num5 = playerScore.PlayerId;
						dictionary[num5] += num4 - eloScore;
						this.VisitedScoreCombintations.Add(num2, true);
					}
					int num6 = k * this.AllFinalPlayerScores.Count + j;
					if (!this.VisitedScoreCombintations.ContainsKey(num6))
					{
						RankedMultiplayerScore.PlayerScore playerScore2 = this.AllFinalPlayerScores[k];
						float num3;
						if (flag)
						{
							num3 = 0.5f;
						}
						else
						{
							num3 = (float)((k < j) ? 1 : 0);
						}
						float eloScore2 = playerScore2.EloScore;
						float num7 = RankedProgressionManager.UpdateEloScore(eloScore2, eloWinProbability2, num3, num);
						Dictionary<int, float> dictionary = this.InProgressEloDeltaPerPlayer;
						int num5 = playerScore2.PlayerId;
						dictionary[num5] += num7 - eloScore2;
						this.VisitedScoreCombintations.Add(num6, true);
					}
				}
			}
		}
	}

	public void CachePlayerRankedProgressionData(int playerId, int tierIdx, float elo)
	{
		if (this.PlayerRankedTierIndices.ContainsKey(playerId))
		{
			this.PlayerRankedTierIndices[playerId] = tierIdx;
		}
		else
		{
			this.PlayerRankedTierIndices.Add(playerId, tierIdx);
		}
		if (this.PlayerRankedElos.ContainsKey(playerId))
		{
			this.PlayerRankedElos[playerId] = elo;
			return;
		}
		this.PlayerRankedElos.Add(playerId, elo);
	}

	public Dictionary<int, int> PlayerRankedTiers
	{
		get
		{
			return this.PlayerRankedTierIndices;
		}
		set
		{
			this.PlayerRankedTierIndices = value;
		}
	}

	public Dictionary<int, float> PlayerRankedEloScores
	{
		get
		{
			return this.PlayerRankedElos;
		}
		set
		{
			this.PlayerRankedElos = value;
		}
	}

	public Dictionary<int, float> ProjectedEloDeltas
	{
		get
		{
			return this.InProgressEloDeltaPerPlayer;
		}
		set
		{
			this.InProgressEloDeltaPerPlayer = value;
		}
	}

	public List<RankedMultiplayerScore.PlayerScoreInRound> GetSortedScores()
	{
		List<RankedMultiplayerScore.PlayerScoreInRound> list = new List<RankedMultiplayerScore.PlayerScoreInRound>();
		foreach (KeyValuePair<int, RankedMultiplayerScore.PlayerScoreInRound> keyValuePair in this.AllPlayerInRoundScores)
		{
			list.Add(keyValuePair.Value);
		}
		list.Sort((RankedMultiplayerScore.PlayerScoreInRound s1, RankedMultiplayerScore.PlayerScoreInRound s2) => this.ComputeGameScore(s2.NumTags, s2.PointsOnDefense).CompareTo(this.ComputeGameScore(s1.NumTags, s1.PointsOnDefense)));
		return list;
	}

	public static float LongestUntaggedTieEpsilon = 0.2f;

	public static int RESULT_TIE = -1;

	[SerializeField]
	private int PointsPerTag = 30;

	[SerializeField]
	private float PointsPerUninfectedSecMin = 0.5f;

	[SerializeField]
	private float PointsPerUninfectedSecMax = 2f;

	private float PerSecondTimer = -1f;

	private bool WasInfectedInitially;

	private GorillaTagCompetitiveManager CompetitiveManager;

	protected Dictionary<int, RankedMultiplayerScore.PlayerScoreInRound> AllPlayerInRoundScores = new Dictionary<int, RankedMultiplayerScore.PlayerScoreInRound>();

	protected List<RankedMultiplayerScore.PlayerScore> AllFinalPlayerScores = new List<RankedMultiplayerScore.PlayerScore>();

	protected Dictionary<int, bool> VisitedScoreCombintations = new Dictionary<int, bool>();

	protected Dictionary<int, float> InProgressEloDeltaPerPlayer = new Dictionary<int, float>();

	protected Dictionary<int, int> PlayerRankedTierIndices = new Dictionary<int, int>();

	protected Dictionary<int, float> PlayerRankedElos = new Dictionary<int, float>();

	private RankedMultiplayerScore.ResultData PendingResults;

	private RankedMultiplayerScore.RecordHolder<int> ResultsMostTags;

	private RankedMultiplayerScore.RecordHolder<float> ResultsLongestUntagged;

	private bool IsLateJoiner;

	public struct PlayerScore
	{
		public int PlayerId;

		public float GameScore;

		public float EloScore;

		public int NumTags;

		public float TimeUntagged;

		public float PointsOnDefense;
	}

	public struct PlayerScoreInRound
	{
		public PlayerScoreInRound(int id, bool initInfected = false)
		{
			this.PlayerId = id;
			this.NumTags = 0;
			this.PointsOnDefense = 0f;
			this.JoinTime = Time.time;
			this.Infected = initInfected;
			this.TaggedTime = (initInfected ? Time.time : 0f);
		}

		public int PlayerId;

		public int NumTags;

		public float PointsOnDefense;

		public float JoinTime;

		public float TaggedTime;

		public bool Infected;
	}

	public struct ResultData
	{
		public bool IsMostTagsTied()
		{
			return this.MostTagsPlayerId == RankedMultiplayerScore.RESULT_TIE;
		}

		public bool IsLongestUntaggedTied()
		{
			return this.LongestUntaggedPlayerId == RankedMultiplayerScore.RESULT_TIE;
		}

		public float Elo;

		public int Rank;

		public int MostTags;

		public float LongestUntagged;

		public int MostTagsPlayerId;

		public int LongestUntaggedPlayerId;
	}

	public struct RecordHolder<T>
	{
		public int PlayerId;

		public T Value;
	}
}
