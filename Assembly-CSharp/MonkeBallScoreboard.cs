using System;
using TMPro;
using UnityEngine;

public class MonkeBallScoreboard : MonoBehaviour
{
	public void Setup(MonkeBallGame game)
	{
		this.game = game;
	}

	public void RefreshScore()
	{
		for (int i = 0; i < this.game.team.Count; i++)
		{
			this.teamDisplays[i].scoreLabel.text = this.game.team[i].score.ToString();
		}
	}

	public void RefreshTeamPlayers(int teamId, int numPlayers)
	{
		this.teamDisplays[teamId].playersLabel.text = string.Format("PLAYERS: {0}", Mathf.Clamp(numPlayers, 0, 99));
	}

	public void PlayScoreFx()
	{
		this.PlayFX(this.scoreSound, this.scoreSoundVolume);
	}

	public void PlayPlayerJoinFx()
	{
		this.PlayFX(this.playerJoinSound, 0.5f);
	}

	public void PlayPlayerLeaveFx()
	{
		this.PlayFX(this.playerLeaveSound, 0.5f);
	}

	public void PlayGameStartFx()
	{
		this.PlayFX(this.gameStartSound, this.gameStartVolume);
	}

	public void PlayGameEndFx()
	{
		this.PlayFX(this.gameEndSound, this.gameEndVolume);
	}

	private void PlayFX(AudioClip clip, float volume)
	{
		if (this.audioSource != null)
		{
			this.audioSource.clip = clip;
			this.audioSource.volume = volume;
			this.audioSource.Play();
		}
	}

	public void RefreshTime(string timeString)
	{
		this.timeRemainingLabel.text = timeString;
	}

	private MonkeBallGame game;

	public MonkeBallScoreboard.TeamDisplay[] teamDisplays;

	public TextMeshPro timeRemainingLabel;

	public AudioSource audioSource;

	public AudioClip scoreSound;

	public float scoreSoundVolume;

	public AudioClip playerJoinSound;

	public AudioClip playerLeaveSound;

	public AudioClip gameStartSound;

	public float gameStartVolume;

	public AudioClip gameEndSound;

	public float gameEndVolume;

	[Serializable]
	public class TeamDisplay
	{
		public TextMeshPro nameLabel;

		public TextMeshPro scoreLabel;

		public TextMeshPro playersLabel;
	}
}
