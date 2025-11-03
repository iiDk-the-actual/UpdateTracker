using System;
using UnityEngine;

public class GorillaTagCompetitiveRoundBuzzer : MonoBehaviour
{
	private void OnEnable()
	{
		GorillaTagCompetitiveManager.onStateChanged += this.OnStateChanged;
		GorillaTagCompetitiveManager.onUpdateRemainingTime += this.OnUpdateRemainingTime;
	}

	private void OnDisable()
	{
		GorillaTagCompetitiveManager.onStateChanged -= this.OnStateChanged;
		GorillaTagCompetitiveManager.onUpdateRemainingTime -= this.OnUpdateRemainingTime;
	}

	private void OnStateChanged(GorillaTagCompetitiveManager.GameState newState)
	{
		switch (newState)
		{
		case GorillaTagCompetitiveManager.GameState.WaitingForPlayers:
			this.PlaySFX(this.needMorePlayerClip);
			break;
		case GorillaTagCompetitiveManager.GameState.Playing:
			this.PlaySFX(this.roundStartClip);
			break;
		case GorillaTagCompetitiveManager.GameState.PostRound:
			this.PlaySFX(this.roundEndClip);
			break;
		}
		this.lastState = newState;
	}

	private void OnUpdateRemainingTime(float remainingTime)
	{
		int num = Mathf.CeilToInt(remainingTime);
		int num2 = Mathf.CeilToInt(this.lastStateRemainingTime);
		if (num != num2)
		{
			GorillaTagCompetitiveManager.GameState gameState = this.lastState;
			if (gameState != GorillaTagCompetitiveManager.GameState.StartingCountdown)
			{
				if (gameState == GorillaTagCompetitiveManager.GameState.Playing)
				{
					if (num > 0 && num <= this.roundEndCountdownDuration)
					{
						this.PlaySFX(this.roundEndingCountdownClip);
					}
				}
			}
			else if (num > 0)
			{
				this.PlaySFX(this.roundCountdownClip);
			}
		}
		this.lastStateRemainingTime = remainingTime;
	}

	private void PlaySFX(AudioClip clip)
	{
		this.PlaySFX(clip, 1f);
	}

	private void PlaySFX(AudioClip clip, float volume)
	{
		this.audioSource.PlayOneShot(clip, volume);
	}

	public AudioSource audioSource;

	public AudioClip roundCountdownClip;

	public AudioClip roundStartClip;

	public AudioClip roundEndingCountdownClip;

	public int roundEndCountdownDuration = 5;

	public AudioClip roundEndClip;

	public AudioClip needMorePlayerClip;

	private GorillaTagCompetitiveManager.GameState lastState;

	private float lastStateRemainingTime = -1f;
}
