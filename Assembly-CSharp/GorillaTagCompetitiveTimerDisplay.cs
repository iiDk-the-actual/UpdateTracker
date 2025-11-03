using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GorillaTagCompetitiveTimerDisplay : MonoBehaviour
{
	private void Awake()
	{
		this.prevTime = -1;
		if (this.waitingForPlayersBackground)
		{
			this.waitingForPlayersBackground.SetActive(true);
			this.currentBackground = this.waitingForPlayersBackground;
		}
		if (this.startCountdownBackground)
		{
			this.startCountdownBackground.SetActive(false);
		}
		if (this.playingBackground)
		{
			this.playingBackground.SetActive(false);
		}
		if (this.postRoundBackground)
		{
			this.postRoundBackground.SetActive(false);
		}
		this.timerDisplay.gameObject.SetActive(false);
		if (this.timerDisplay2)
		{
			this.timerDisplay2.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		GorillaTagCompetitiveManager.onStateChanged += this.HandleOnGameStateChanged;
		GorillaTagCompetitiveManager.onUpdateRemainingTime += this.HandleOnTimeChanged;
		GorillaTagCompetitiveManager gorillaTagCompetitiveManager = GorillaGameManager.instance as GorillaTagCompetitiveManager;
		if (gorillaTagCompetitiveManager != null)
		{
			this.HandleOnGameStateChanged(gorillaTagCompetitiveManager.GetCurrentGameState());
		}
		this.myRig = base.GetComponentInParent<VRRig>();
		this.DisplayStandardTimer(false);
	}

	private void OnDisable()
	{
		GorillaTagCompetitiveManager.onStateChanged -= this.HandleOnGameStateChanged;
		GorillaTagCompetitiveManager.onUpdateRemainingTime -= this.HandleOnTimeChanged;
	}

	private void HandleOnGameStateChanged(GorillaTagCompetitiveManager.GameState newState)
	{
		this.SetNewBackground(newState);
		switch (newState)
		{
		case GorillaTagCompetitiveManager.GameState.WaitingForPlayers:
			this.DisplayStandardTimer(false);
			this.resultsDisplay.gameObject.SetActive(false);
			return;
		case GorillaTagCompetitiveManager.GameState.StartingCountdown:
		case GorillaTagCompetitiveManager.GameState.Playing:
			this.DisplayStandardTimer(true);
			return;
		case GorillaTagCompetitiveManager.GameState.PostRound:
			this.DoPostRoundShow();
			return;
		default:
			return;
		}
	}

	private void DisplayStandardTimer(bool bShow)
	{
		if (bShow)
		{
			this.resultsDisplay.gameObject.SetActive(false);
		}
		this.timerDisplay.gameObject.SetActive(bShow);
		if (this.timerDisplay2 != null)
		{
			this.timerDisplay2.gameObject.SetActive(bShow);
		}
	}

	private void DoPostRoundShow()
	{
		GorillaTagCompetitiveManager gorillaTagCompetitiveManager = GorillaGameManager.instance as GorillaTagCompetitiveManager;
		if (gorillaTagCompetitiveManager == null)
		{
			return;
		}
		this.DisplayStandardTimer(false);
		this.resultsDisplay.gameObject.SetActive(true);
		List<VRRig> list = new List<VRRig>();
		List<RankedMultiplayerScore.PlayerScoreInRound> sortedScores = gorillaTagCompetitiveManager.GetScoring().GetSortedScores();
		float num = gorillaTagCompetitiveManager.GetScoring().ComputeGameScore(sortedScores[0].NumTags, sortedScores[0].PointsOnDefense);
		int num2 = 0;
		while (num2 < sortedScores.Count && num2 < 3)
		{
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(sortedScores[num2].PlayerId, out rigContainer))
			{
				float num3 = gorillaTagCompetitiveManager.GetScoring().ComputeGameScore(sortedScores[num2].NumTags, sortedScores[num2].PointsOnDefense);
				if (num2 == 0 || num3.Approx(num, 0.01f))
				{
					list.Add(rigContainer.Rig);
				}
				switch (num2)
				{
				case 0:
					if (this.tintableCelebration != null)
					{
						Color playerColor = rigContainer.Rig.playerColor;
						float num4;
						float num5;
						float num6;
						Color.RGBToHSV(playerColor, out num4, out num5, out num6);
						Color color = Color.HSVToRGB(num4, num5, (num6 < 0.5f) ? (num6 + 0.5f) : (num6 - 0.5f));
						this.tintableCelebration.main.startColor = new ParticleSystem.MinMaxGradient(playerColor, color);
						this.tintableCelebration.gameObject.SetActive(true);
					}
					if (this.goldCelebration != null && rigContainer.Rig == this.myRig)
					{
						this.goldCelebration.gameObject.SetActive(true);
					}
					if (this.celebrationAudio != null)
					{
						this.celebrationAudio.Play();
					}
					break;
				case 1:
					if (this.silverCelebration != null && rigContainer.Rig == this.myRig)
					{
						this.silverCelebration.gameObject.SetActive(true);
					}
					if (this.celebrationAudio != null)
					{
						this.celebrationAudio.Play();
					}
					break;
				case 2:
					if (this.bronzeCelebration != null && rigContainer.Rig == this.myRig)
					{
						this.bronzeCelebration.gameObject.SetActive(true);
					}
					if (this.celebrationAudio != null)
					{
						this.celebrationAudio.Play();
					}
					break;
				}
			}
			num2++;
		}
		for (int i = 0; i < this.postRoundTimerText.Length; i++)
		{
			this.postRoundTimerText[i].text = ((list.Count > 1) ? "SHARED WIN" : "WINNER");
		}
		string text = string.Empty;
		for (int j = 0; j < list.Count; j++)
		{
			text = text + list[j].playerText1.text.ToUpper() + "\n";
		}
		this.resultsDisplay.text = text.Trim();
		if (this.timerDisplay2 != null)
		{
			this.timerDisplay2.text = this.resultsDisplay.text;
		}
	}

	private void HandleOnTimeChanged(float time)
	{
		int num = Mathf.CeilToInt(time);
		num = Mathf.Max(num, 1);
		if (this.prevTime != num)
		{
			this.prevTime = num;
			if (this.currentState == GorillaTagCompetitiveManager.GameState.Playing)
			{
				int num2 = this.prevTime / 60;
				int num3 = this.prevTime % 60;
				this.timerDisplay.text = string.Format("{0}:{1:D2}", num2, num3);
				if (this.timerDisplay2)
				{
					this.timerDisplay2.text = string.Format("{0}:{1:D2}", num2, num3);
					return;
				}
			}
			else if (this.currentState != GorillaTagCompetitiveManager.GameState.PostRound)
			{
				this.timerDisplay.text = this.prevTime.ToString("#00");
				if (this.timerDisplay2)
				{
					this.timerDisplay2.text = this.prevTime.ToString("#00");
				}
			}
		}
	}

	private void SetNewBackground(GorillaTagCompetitiveManager.GameState newState)
	{
		if (this.currentBackground != null)
		{
			this.currentBackground.SetActive(false);
		}
		this.currentState = newState;
		GameObject gameObject = this.SelectBackground(newState);
		this.GetTextColor(newState);
		this.currentBackground = null;
		if (gameObject != null)
		{
			this.currentBackground = gameObject;
			this.currentBackground.SetActive(true);
		}
	}

	private GameObject SelectBackground(GorillaTagCompetitiveManager.GameState newState)
	{
		switch (newState)
		{
		case GorillaTagCompetitiveManager.GameState.WaitingForPlayers:
			return this.waitingForPlayersBackground;
		case GorillaTagCompetitiveManager.GameState.StartingCountdown:
			return this.startCountdownBackground;
		case GorillaTagCompetitiveManager.GameState.Playing:
			return this.playingBackground;
		case GorillaTagCompetitiveManager.GameState.PostRound:
			return this.postRoundBackground;
		default:
			return null;
		}
	}

	private Color GetTextColor(GorillaTagCompetitiveManager.GameState newState)
	{
		switch (newState)
		{
		case GorillaTagCompetitiveManager.GameState.StartingCountdown:
			return this.timerColorStart;
		case GorillaTagCompetitiveManager.GameState.Playing:
			return this.timerColorPlaying;
		case GorillaTagCompetitiveManager.GameState.PostRound:
			return this.timerColorPostRound;
		default:
			return Color.white;
		}
	}

	public TextMeshPro timerDisplay;

	public TextMeshPro timerDisplay2;

	public TextMeshPro resultsDisplay;

	public GameObject waitingForPlayersBackground;

	public GameObject startCountdownBackground;

	public Color timerColorStart = Color.white;

	public GameObject playingBackground;

	public Color timerColorPlaying = Color.white;

	public GameObject postRoundBackground;

	public Color timerColorPostRound = Color.white;

	public TextMeshPro[] postRoundTimerText;

	private GorillaTagCompetitiveManager.GameState currentState;

	private GameObject currentBackground;

	private int prevTime = -1;

	[SerializeField]
	private ParticleSystem tintableCelebration;

	[SerializeField]
	private ParticleSystem goldCelebration;

	[SerializeField]
	private ParticleSystem silverCelebration;

	[SerializeField]
	private ParticleSystem bronzeCelebration;

	private VRRig myRig;

	[SerializeField]
	private AudioSource celebrationAudio;
}
