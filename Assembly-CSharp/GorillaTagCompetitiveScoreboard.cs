using System;
using System.Collections.Generic;
using UnityEngine;

public class GorillaTagCompetitiveScoreboard : MonoBehaviour
{
	private void Awake()
	{
		GorillaTagCompetitiveManager.RegisterScoreboard(this);
		for (int i = 0; i < this.lines.Length; i++)
		{
			this.lines[i].gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		GorillaTagCompetitiveManager.DeregisterScoreboard(this);
	}

	public void UpdateScores(GorillaTagCompetitiveManager.GameState gameState, float activeRoundTime, List<RankedMultiplayerScore.PlayerScoreInRound> scores, Dictionary<int, int> PlayerRankedTiers, Dictionary<int, float> PlayerPredictedEloDeltas, List<NetPlayer> infectedPlayers, RankedProgressionManager progressionManager)
	{
		this.waitingForPlayers.SetActive(gameState == GorillaTagCompetitiveManager.GameState.WaitingForPlayers);
		for (int i = 0; i < this.lines.Length; i++)
		{
			if (gameState != GorillaTagCompetitiveManager.GameState.WaitingForPlayers && scores != null && scores.Count > i)
			{
				RankedMultiplayerScore.PlayerScoreInRound playerScoreInRound = scores[i];
				NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID(playerScoreInRound.PlayerId);
				if (netPlayerByID != null)
				{
					this.lines[i].gameObject.SetActive(true);
					if (PlayerRankedTiers == null || !PlayerRankedTiers.ContainsKey(playerScoreInRound.PlayerId))
					{
						this.lines[i].SetPlayer(netPlayerByID.SanitizedNickName, null);
					}
					else
					{
						this.lines[i].SetPlayer(netPlayerByID.SanitizedNickName, progressionManager.GetProgressionRankIcon(PlayerRankedTiers[playerScoreInRound.PlayerId]));
					}
					if (playerScoreInRound.TaggedTime.Approx(0f, 1E-06f))
					{
						this.lines[i].SetScore(Mathf.Max(activeRoundTime - playerScoreInRound.JoinTime, 0f), playerScoreInRound.NumTags);
					}
					else
					{
						this.lines[i].SetScore(Mathf.Max(playerScoreInRound.TaggedTime - playerScoreInRound.JoinTime, 0f), playerScoreInRound.NumTags);
					}
					if (PlayerPredictedEloDeltas.ContainsKey(playerScoreInRound.PlayerId))
					{
						float num = PlayerPredictedEloDeltas[playerScoreInRound.PlayerId];
						GorillaTagCompetitiveScoreboard.PredictedResult predictedResult = GorillaTagCompetitiveScoreboard.PredictedResult.Even;
						if (num > this.largeEloDelta)
						{
							predictedResult = GorillaTagCompetitiveScoreboard.PredictedResult.Great;
						}
						else if (num > this.smallEloDelta)
						{
							predictedResult = GorillaTagCompetitiveScoreboard.PredictedResult.Good;
						}
						else if (num < -this.largeEloDelta)
						{
							predictedResult = GorillaTagCompetitiveScoreboard.PredictedResult.Poor;
						}
						else if (num < -this.smallEloDelta)
						{
							predictedResult = GorillaTagCompetitiveScoreboard.PredictedResult.Bad;
						}
						this.lines[i].SetPredictedResult(predictedResult);
					}
					this.lines[i].SetInfected(gameState == GorillaTagCompetitiveManager.GameState.Playing && infectedPlayers.Contains(netPlayerByID));
				}
			}
			else
			{
				this.lines[i].gameObject.SetActive(false);
			}
		}
	}

	public void DisplayPredictedResults(bool bShow)
	{
		for (int i = 0; i < this.lines.Length; i++)
		{
			this.lines[i].DisplayPredictedResults(bShow);
		}
	}

	public GorillaTagCompetitiveScoreboardLine[] lines;

	public GameObject waitingForPlayers;

	public float smallEloDelta = 10f;

	public float largeEloDelta = 25f;

	public enum PredictedResult
	{
		Great,
		Good,
		Even,
		Bad,
		Poor
	}
}
