using System;
using TMPro;
using UnityEngine;

public class RaceVisual : MonoBehaviour
{
	public int raceId { get; private set; }

	public bool TickRunning { get; set; }

	private void Awake()
	{
		this.checkpoints = base.GetComponent<RaceCheckpointManager>();
		this.finishLineText.text = "";
		this.SetScoreboardText("", "");
		this.SetRaceStartScoreboardText("", "");
	}

	private void OnEnable()
	{
		RacingManager.instance.RegisterVisual(this);
	}

	public void Button_StartRace(int laps)
	{
		RacingManager.instance.Button_StartRace(this.raceId, laps);
	}

	public void ShowFinishLineText(string text)
	{
		this.finishLineText.text = text;
	}

	public void UpdateCountdown(int timeRemaining)
	{
		if (timeRemaining != this.lastDisplayedCountdown)
		{
			this.countdownText.text = timeRemaining.ToString();
			this.finishLineText.text = "";
			this.lastDisplayedCountdown = timeRemaining;
		}
	}

	public void SetScoreboardText(string mainText, string timesText)
	{
		foreach (RacingScoreboard racingScoreboard in this.raceScoreboards)
		{
			racingScoreboard.mainDisplay.text = mainText;
			racingScoreboard.timesDisplay.text = timesText;
		}
	}

	public void SetRaceStartScoreboardText(string mainText, string timesText)
	{
		this.raceStartScoreboard.mainDisplay.text = mainText;
		this.raceStartScoreboard.timesDisplay.text = timesText;
	}

	public void ActivateStartingWall(bool enable)
	{
		this.startingWall.SetActive(enable);
	}

	public bool IsPlayerNearCheckpoint(VRRig player, int checkpoint)
	{
		return this.checkpoints.IsPlayerNearCheckpoint(player, checkpoint);
	}

	public void OnCountdownStart(int laps, float goAfterInterval)
	{
		this.raceConsoleVisual.ShowRaceInProgress(laps);
		this.countdownSoundPlayer.Play();
		this.countdownSoundPlayer.time = this.countdownSoundGoTime - goAfterInterval;
	}

	public void OnRaceStart()
	{
		this.finishLineText.text = "GO!";
		this.checkpoints.OnRaceStart();
		this.lastDisplayedCountdown = 0;
		this.startingWall.SetActive(false);
		this.isRaceEndSoundEnabled = false;
	}

	public void OnRaceEnded()
	{
		this.finishLineText.text = "";
		this.lastDisplayedCountdown = 0;
		this.checkpoints.OnRaceEnd();
	}

	public void OnRaceReset()
	{
		this.raceConsoleVisual.ShowCanStartRace();
	}

	public void EnableRaceEndSound()
	{
		this.isRaceEndSoundEnabled = true;
	}

	public void OnCheckpointPassed(int index, SoundBankPlayer checkpointSound)
	{
		if (index == 0 && this.isRaceEndSoundEnabled)
		{
			this.countdownSoundPlayer.PlayOneShot(this.raceEndSound);
		}
		else
		{
			checkpointSound.Play();
		}
		RacingManager.instance.OnCheckpointPassed(this.raceId, index);
	}

	[SerializeField]
	private TextMeshPro finishLineText;

	[SerializeField]
	private TextMeshPro countdownText;

	[SerializeField]
	private RacingScoreboard[] raceScoreboards;

	[SerializeField]
	private RacingScoreboard raceStartScoreboard;

	[SerializeField]
	private RaceConsoleVisual raceConsoleVisual;

	private float nextVisualRefreshTimestamp;

	private RaceCheckpointManager checkpoints;

	[SerializeField]
	private AudioClip raceEndSound;

	[SerializeField]
	private float countdownSoundGoTime;

	[SerializeField]
	private AudioSource countdownSoundPlayer;

	[SerializeField]
	private GameObject startingWall;

	private int lastDisplayedCountdown;

	private bool isRaceEndSoundEnabled;
}
