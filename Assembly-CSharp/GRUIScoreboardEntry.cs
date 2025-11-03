using System;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class GRUIScoreboardEntry : MonoBehaviour
{
	public void Setup(VRRig vrRig, int playerActorId, GRUIScoreboard.ScoreboardScreen screenType)
	{
		this.playerActorId = playerActorId;
		this.Refresh(vrRig, screenType);
	}

	private void Refresh(VRRig vrRig, GRUIScoreboard.ScoreboardScreen screenType)
	{
		GRPlayer grplayer = GRPlayer.Get(vrRig);
		if (!(vrRig != null) || !(grplayer != null))
		{
			this.playerNameLabel.text = "";
			this.playerCurrencyLabel.text = "";
			this.playerTitleLabel.text = "";
			this.playerCutLabel.text = "";
			this.currencySet = 0;
			return;
		}
		if (!this.playerNameLabel.text.Equals(vrRig.playerNameVisible))
		{
			this.playerNameLabel.text = vrRig.playerNameVisible;
		}
		if (screenType != GRUIScoreboard.ScoreboardScreen.DefaultInfo)
		{
			if (screenType == GRUIScoreboard.ScoreboardScreen.ShiftCutCalculation)
			{
				this.defaultUIParent.SetActive(false);
				this.shiftCutParent.SetActive(true);
				if (GhostReactor.instance.shiftManager != null && (GhostReactor.instance.shiftManager.ShiftActive || GhostReactor.instance.shiftManager.ShiftTotalEarned >= 0))
				{
					int num = Mathf.FloorToInt(grplayer.ShiftPlayTime / 60f);
					int num2 = Mathf.FloorToInt(grplayer.ShiftPlayTime - (float)(num * 60));
					this.playerTimeLabel.text = string.Format("{0:00}:{1:00}", num, num2);
					this.playerPercentageLabel.text = "%" + Mathf.Floor(grplayer.ShiftPlayTime / GhostReactor.instance.shiftManager.TotalPlayTime * 100f).ToString();
				}
				else
				{
					this.playerTimeLabel.text = "n/a";
					this.playerPercentageLabel.text = "n/a";
				}
				this.playerTitleLabel.text = this.titleSet;
			}
		}
		else
		{
			this.defaultUIParent.SetActive(true);
			this.shiftCutParent.SetActive(false);
			if (grplayer.ShiftCredits != this.currencySet)
			{
				this.currencySet = grplayer.ShiftCredits;
				this.playerCurrencyLabel.text = this.currencySet.ToString();
			}
			string titleNameAndGrade = GhostReactorProgression.GetTitleNameAndGrade(grplayer.CurrentProgression.redeemedPoints);
			if (titleNameAndGrade != this.titleSet)
			{
				this.titleSet = titleNameAndGrade;
				this.playerTitleLabel.text = this.titleSet;
			}
		}
		if (GhostReactor.instance.shiftManager == null || GhostReactor.instance.shiftManager.ShiftActive)
		{
			this.playerCutLabel.text = "-";
			return;
		}
		this.playerCutLabel.text = grplayer.LastShiftCut.ToString();
	}

	[SerializeField]
	private TMP_Text playerNameLabel;

	[SerializeField]
	private TMP_Text playerCutLabel;

	public GameObject defaultUIParent;

	[SerializeField]
	private TMP_Text playerTitleLabel;

	[SerializeField]
	private TMP_Text playerCurrencyLabel;

	public GameObject shiftCutParent;

	[SerializeField]
	private TMP_Text playerTimeLabel;

	[SerializeField]
	private TMP_Text playerPercentageLabel;

	private int playerActorId = -1;

	private int currencySet = -1;

	private string titleSet = "";
}
