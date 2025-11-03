using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GRUIScoreboard : MonoBehaviour, IGorillaSliceableSimple
{
	public void SliceUpdate()
	{
		if (this.currentScreen == GRUIScoreboard.ScoreboardScreen.ShiftCutCalculation)
		{
			this.Refresh(GhostReactor.instance.vrRigs);
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void Refresh(List<VRRig> vrRigs)
	{
		if (this.currentScreen == GRUIScoreboard.ScoreboardScreen.ShiftCutCalculation)
		{
			GhostReactor.instance.shiftManager.CalculatePlayerPercentages();
		}
		for (int i = 0; i < this.entries.Count; i++)
		{
			if (!(this.entries[i] == null))
			{
				if (i < vrRigs.Count && vrRigs[i] != null && vrRigs[i].OwningNetPlayer != null)
				{
					this.entries[i].gameObject.SetActive(true);
					this.entries[i].Setup(vrRigs[i], vrRigs[i].OwningNetPlayer.ActorNumber, this.currentScreen);
				}
				else
				{
					this.entries[i].gameObject.SetActive(false);
				}
			}
		}
	}

	public void SwitchToScreen(GRUIScoreboard.ScoreboardScreen screenType)
	{
		this.currentScreen = screenType;
		GRUIScoreboard.ScoreboardScreen scoreboardScreen = this.currentScreen;
		if (scoreboardScreen == GRUIScoreboard.ScoreboardScreen.DefaultInfo)
		{
			this.infoTextParent.SetActive(true);
			this.calcTextParent.SetActive(false);
			this.buttonText.text = "SHOW CUT CALC";
			return;
		}
		if (scoreboardScreen != GRUIScoreboard.ScoreboardScreen.ShiftCutCalculation)
		{
			return;
		}
		this.infoTextParent.SetActive(false);
		this.calcTextParent.SetActive(true);
		this.buttonText.text = "SHOW INFO";
	}

	public void SwitchState()
	{
		if (this.currentScreen == GRUIScoreboard.ScoreboardScreen.DefaultInfo)
		{
			this.SwitchToScreen(GRUIScoreboard.ScoreboardScreen.ShiftCutCalculation);
		}
		else
		{
			this.SwitchToScreen(GRUIScoreboard.ScoreboardScreen.DefaultInfo);
		}
		this.Refresh(GhostReactor.instance.vrRigs);
		GhostReactor.instance.UpdateRemoteScoreboardScreen(this.currentScreen);
	}

	public static bool ValidPage(GRUIScoreboard.ScoreboardScreen screen)
	{
		return screen == GRUIScoreboard.ScoreboardScreen.DefaultInfo || screen == GRUIScoreboard.ScoreboardScreen.ShiftCutCalculation;
	}

	public List<GRUIScoreboardEntry> entries;

	public TMP_Text total;

	public TMP_Text buttonText;

	public GRUIScoreboard.ScoreboardScreen currentScreen;

	public GameObject infoTextParent;

	public GameObject calcTextParent;

	public enum ScoreboardScreen
	{
		DefaultInfo,
		ShiftCutCalculation
	}
}
