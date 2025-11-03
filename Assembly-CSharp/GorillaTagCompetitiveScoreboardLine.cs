using System;
using TMPro;
using UnityEngine;

public class GorillaTagCompetitiveScoreboardLine : MonoBehaviour
{
	public void SetPlayer(string playerName, Sprite icon)
	{
		this.playerNameDisplay.text = playerName;
		this.rankSprite.sprite = icon;
	}

	public void SetScore(float untaggedTime, int tagCount)
	{
		int num = Mathf.FloorToInt(untaggedTime);
		int num2 = num / 60;
		int num3 = num % 60;
		this.untaggedTimeDisplay.text = string.Format("{0}:{1:D2}", num2, num3);
		this.tagCountDisplay.text = tagCount.ToString();
	}

	public void SetPredictedResult(GorillaTagCompetitiveScoreboard.PredictedResult result)
	{
		this.resultSprite.sprite = this.resultSprites[(int)result];
	}

	public void DisplayPredictedResults(bool bShow)
	{
		this.resultSprite.gameObject.SetActive(bShow);
	}

	public void SetInfected(bool infected)
	{
		this.playerNameDisplay.color = (infected ? Color.red : Color.white);
	}

	public SpriteRenderer rankSprite;

	public TMP_Text playerNameDisplay;

	public TMP_Text untaggedTimeDisplay;

	public TMP_Text tagCountDisplay;

	public SpriteRenderer resultSprite;

	public Sprite[] resultSprites;
}
