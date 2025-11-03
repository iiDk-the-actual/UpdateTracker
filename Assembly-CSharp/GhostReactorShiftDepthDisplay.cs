using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[Serializable]
public class GhostReactorShiftDepthDisplay
{
	public void Setup()
	{
		this.StopDelveDeeperFX();
	}

	public int GetRewardXP()
	{
		return this.reactor.GetDepthLevel() * 10 + 10;
	}

	public void RefreshDisplay()
	{
		int depthLevel = this.reactor.GetDepthLevel();
		this.reactor.GetDepthLevelConfig(depthLevel);
		this.reactor.GetDepthLevelConfig(depthLevel + 1);
		switch (this.shiftManager.GetState())
		{
		case GhostReactorShiftManager.State.WaitingForShiftStart:
		case GhostReactorShiftManager.State.WaitingForFirstShiftStart:
		case GhostReactorShiftManager.State.ShiftActive:
		{
			foreach (TMP_Text tmp_Text in this.logoFrames)
			{
				tmp_Text.gameObject.SetActive(false);
			}
			this.cachedStringBuilder.Clear();
			this.cachedStringBuilder.Append("<color=grey>Team Goals:</color>\n");
			int num = 0;
			if (this.shiftManager.coresRequiredToDelveDeeper > 0)
			{
				this.cachedStringBuilder.Append(string.Format("Deposit {0} Cores\n", this.shiftManager.coresRequiredToDelveDeeper));
				num++;
			}
			if (this.shiftManager.sentientCoresRequiredToDelveDeeper > 0)
			{
				this.cachedStringBuilder.Append(string.Format("Collect {0} Seeds\n", this.shiftManager.sentientCoresRequiredToDelveDeeper));
				num++;
			}
			if (this.shiftManager.maxPlayerDeaths >= 0)
			{
				this.cachedStringBuilder.Append(string.Format("Limit Incidents to {0}", this.shiftManager.maxPlayerDeaths));
				num++;
			}
			this.jumbotronRequirements.text = this.cachedStringBuilder.ToString();
			int num2 = this.reactor.GetCurrLevelGenConfig().coresRequired * 5;
			int rewardXP = this.GetRewardXP();
			this.cachedStringBuilder.Clear();
			this.cachedStringBuilder.Append("<color=grey>Rewards:</color>\n");
			this.cachedStringBuilder.Append(string.Format("+⑭{0}\n", num2));
			this.cachedStringBuilder.Append(string.Format("+{0} XP\n", rewardXP));
			this.jumbotronRewards.text = this.cachedStringBuilder.ToString();
			break;
		}
		case GhostReactorShiftManager.State.PreparingToDrill:
			this.jumbotronRequirements.text = "";
			this.jumbotronRewards.text = "";
			break;
		case GhostReactorShiftManager.State.Drilling:
			this.jumbotronRequirements.text = "";
			this.jumbotronRewards.text = "";
			break;
		}
		if (this.jumbotronState != null)
		{
			int state = (int)this.shiftManager.GetState();
			if (state >= 0 && state < GhostReactorShiftDepthDisplay.STATE_NAMES.Length)
			{
				this.jumbotronState.text = GhostReactorShiftDepthDisplay.STATE_NAMES[state];
			}
			else
			{
				this.jumbotronState.text = null;
			}
		}
		this.RefreshObjectives();
	}

	public void RefreshObjectives()
	{
		GRShiftStat shiftStats = this.shiftManager.shiftStats;
		bool flag = shiftStats.GetShiftStat(GRShiftStatType.CoresCollected) >= this.shiftManager.coresRequiredToDelveDeeper;
		bool flag2 = shiftStats.GetShiftStat(GRShiftStatType.SentientCoresCollected) >= this.shiftManager.sentientCoresRequiredToDelveDeeper;
		bool flag3 = this.shiftManager.maxPlayerDeaths < 0 || shiftStats.GetShiftStat(GRShiftStatType.PlayerDeaths) <= this.shiftManager.maxPlayerDeaths;
		if (this.shiftManager.ShiftActive && flag && flag2 && flag3)
		{
			this.shiftManager.authorizedToDelveDeeper = true;
		}
		if (this.shiftManager.IsSoaking())
		{
			this.shiftManager.authorizedToDelveDeeper = true;
		}
		if (this.shiftManager.authorizedToDelveDeeper && this.jumbotronRequirements != null)
		{
			this.jumbotronRequirements.text = "<color=green>AUTHORIZED TO\nDELVE DEEPER</color>";
		}
		bool authorizedToDelveDeeper = this.shiftManager.authorizedToDelveDeeper;
		if (this.delveDeeperButton != null)
		{
			this.delveDeeperButton.SetActive(authorizedToDelveDeeper && !this.shiftManager.ShiftActive);
		}
	}

	public void StartDelveDeeperFX()
	{
		this.delveDeeperAudio.Play();
		this.delveDeeperNonspatializedAudio.Play();
		for (int i = 0; i < this.delveDeeperAnims.Count; i++)
		{
			this.delveDeeperAnims[i].Play();
		}
		for (int j = 0; j < this.delveDeeperAnimators.Count; j++)
		{
			this.delveDeeperAnimators[j].enabled = true;
		}
		for (int k = 0; k < this.delveDeeperParticles.Count; k++)
		{
			this.delveDeeperParticles[k].emission.enabled = true;
		}
		GorillaTagger.Instance.StartVibration(false, 0.1f, (float)this.shiftManager.GetDrillingDuration());
		GorillaTagger.Instance.StartVibration(true, 0.1f, (float)this.shiftManager.GetDrillingDuration());
	}

	public void StopDelveDeeperFX()
	{
		this.delveDeeperAudio.Stop();
		this.delveDeeperNonspatializedAudio.Stop();
		for (int i = 0; i < this.delveDeeperAnimators.Count; i++)
		{
			this.delveDeeperAnimators[i].enabled = false;
		}
		for (int j = 0; j < this.delveDeeperParticles.Count; j++)
		{
			this.delveDeeperParticles[j].emission.enabled = false;
		}
	}

	public GhostReactorShiftManager shiftManager;

	public GhostReactor reactor;

	[SerializeField]
	public TMP_Text jumbotronTitle;

	[SerializeField]
	public TMP_Text jumbotronState;

	[SerializeField]
	public TMP_Text jumbotronTime;

	[SerializeField]
	public TMP_Text jumbotronRequirements;

	[SerializeField]
	public TMP_Text jumbotronRewards;

	[SerializeField]
	public List<TMP_Text> logoFrames;

	[SerializeField]
	private GameObject delveDeeperButton;

	[SerializeField]
	private AudioSource delveDeeperAudio;

	[SerializeField]
	private AudioSource delveDeeperNonspatializedAudio;

	[SerializeField]
	private List<Animation> delveDeeperAnims;

	[SerializeField]
	private List<Animator> delveDeeperAnimators;

	[SerializeField]
	private List<ParticleSystem> delveDeeperParticles;

	private static readonly string[] STATE_NAMES = new string[] { "--", "PREPARING ENTRY", "PREPARING ENTRY", "READY", "ACTIVE", "EVALUATING SHIFT", "PREPARE TO DIVE", "DIVING" };

	private StringBuilder cachedStringBuilder = new StringBuilder(256);
}
