using System;
using System.Collections.Generic;
using GorillaNetworking;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GRResearchStation : MonoBehaviour
{
	public void Init(GRToolProgressionManager tree, GhostReactor ghostReactor)
	{
		this.toolProgressionManager = tree;
		this.toolProgressionManager.OnProgressionUpdated += this.ResearchTreeUpdated;
		this.reactor = ghostReactor;
		this.totalTools = 0;
		this.selectedToolIndex = 0;
		this._levelString = this.LevelText.text;
		this._costString = this.CostText.text;
		this._researchPointsString = this.ResearchPointsTex.text;
		this._requiredLevelString = this.RequiredLevelText.text;
		this.UpdateUI();
		this.SelectTool(0);
	}

	private void SelectTool(int index)
	{
		if (this.toolProgressionManager == null || this.totalTools == 0)
		{
			return;
		}
		if (index < this.totalTools && index > -1)
		{
			this.selectedToolIndex = index;
			this.selectedToolUpgrades = this.toolProgressionManager.GetToolUpgrades(this.supportedTools[this.selectedToolIndex]);
			this.SelectUpgrade(0);
			this.UpdateUI();
		}
	}

	public void ResearchTreeUpdated()
	{
		this.supportedTools = this.toolProgressionManager.GetSupportedTools();
		this.totalTools = this.supportedTools.Count;
		this.SelectTool(this.selectedToolIndex);
		this.UpdateUI();
	}

	public void UpdateUI()
	{
		this.UpdateToolName();
		this.UpdateUpgradeTitles();
		this.UpdateLocked();
		this.UpdateRequiredLevel();
		this.UpdateCost();
		this.UpdateResearchPoints(this.toolProgressionManager.GetNumberOfResearchPoints());
	}

	public void SelectUpgrade(int UpgradeIndex)
	{
		if (this.toolProgressionManager == null)
		{
			return;
		}
		this.selectedUpgradeIndex = UpgradeIndex;
		if (this.selectedToolUpgrades.Count > this.selectedUpgradeIndex)
		{
			this.currentlySelectedToolUpgrade = this.selectedToolUpgrades[this.selectedUpgradeIndex];
			this.currentlySelectedUpgradeMetadata = this.currentlySelectedToolUpgrade.partMetadata;
			this.SetUpgradeTextColors(this.selectedUpgradeIndex);
			this.UpdateDescriptionText(this.currentlySelectedUpgradeMetadata.description);
		}
		this.UpdateUI();
	}

	private void SetUpgradeTextColors(int index)
	{
		for (int i = 0; i < this.UpgradeTitlesText.Length; i++)
		{
			this.UpgradeButton[i].isOn = false;
			this.UpgradeButton[i].UpdateColor();
		}
		this.UpgradeButton[index].isOn = true;
		this.UpgradeButton[index].UpdateColor();
	}

	private void UpdateUpgradeTitles()
	{
		for (int i = 0; i < this.UpgradeTitlesText.Length; i++)
		{
			if (this.totalTools >= this.selectedToolIndex && this.selectedToolUpgrades.Count > i)
			{
				this.UpgradeTitlesText[i].text = this.selectedToolUpgrades[i].partMetadata.name;
			}
			else
			{
				this.UpgradeTitlesText[i].text = null;
			}
		}
	}

	public void UpdateLocked()
	{
		if (this.currentlySelectedToolUpgrade.unlocked)
		{
			this.UnlockedText.color = this.unlockedToolColor;
			this.UnlockedText.text = "UNLOCKED";
		}
		else
		{
			this.UnlockedText.color = this.lockedToolColor;
			this.UnlockedText.text = "LOCKED";
		}
		for (int i = 0; i < this.UpgradeTitlesText.Length; i++)
		{
			if (this.totalTools >= this.selectedToolIndex && this.selectedToolUpgrades.Count > i)
			{
				bool unlocked = this.selectedToolUpgrades[i].unlocked;
				this.UpgradeTitlesText[i].color = (unlocked ? this.unlockedToolColor : this.lockedToolColor);
				this.LockedImage[i].gameObject.SetActive(!unlocked);
			}
			else
			{
				this.UpgradeTitlesText[i].color = Color.black;
				this.LockedImage[i].gameObject.SetActive(true);
			}
		}
	}

	public void UpdateRequiredLevel()
	{
		int requiredEmployeeLevel = this.toolProgressionManager.GetRequiredEmployeeLevel(this.currentlySelectedToolUpgrade.requiredEmployeeLevel);
		string titleNameFromLevel = GhostReactorProgression.GetTitleNameFromLevel(requiredEmployeeLevel);
		int num = 0;
		GRPlayer grplayer = GRPlayer.Get(PhotonNetwork.LocalPlayer.ActorNumber);
		if (grplayer != null)
		{
			num = GhostReactorProgression.GetTitleLevel(grplayer.CurrentProgression.redeemedPoints);
		}
		string titleNameFromLevel2 = GhostReactorProgression.GetTitleNameFromLevel(num);
		this.RequiredLevelText.text = string.Format(this._requiredLevelString, titleNameFromLevel);
		this.LevelText.text = string.Format(this._levelString, titleNameFromLevel2);
		this.RequiredLevelText.color = ((num >= requiredEmployeeLevel) ? this.unlockedToolColor : this.lockedToolColor);
	}

	public void UpdateDescriptionText(string description)
	{
		this.DescriptionText.text = description;
	}

	public void UpdateCost()
	{
		if (this.selectedToolUpgrades != null && this.selectedToolUpgrades.Count > 0 && this.selectedToolUpgrades.Count > this.selectedUpgradeIndex)
		{
			int numberOfResearchPoints = this.toolProgressionManager.GetNumberOfResearchPoints();
			int researchCost = this.selectedToolUpgrades[this.selectedUpgradeIndex].researchCost;
			this.CostText.text = string.Format(this._costString, researchCost);
			this.CostText.color = ((numberOfResearchPoints >= researchCost) ? this.unlockedToolColor : this.lockedToolColor);
		}
	}

	public void UpdateToolName()
	{
		if (this.supportedTools.Count > 0)
		{
			this.ToolNameText.text = GRUtils.GetToolName(this.supportedTools[this.selectedToolIndex]);
		}
	}

	public void UpdateResearchPoints(int ResearchPoints)
	{
		this.ResearchPointsTex.text = string.Format(this._researchPointsString, ResearchPoints);
	}

	public void MFDButton0Pressed()
	{
		this.SelectUpgrade(0);
	}

	public void MFDButton1Pressed()
	{
		this.SelectUpgrade(1);
	}

	public void MFDButton2Pressed()
	{
		this.SelectUpgrade(2);
	}

	public void MFDButton3Pressed()
	{
		this.SelectUpgrade(3);
	}

	public void MFDButton4Pressed()
	{
		this.SelectUpgrade(4);
	}

	public void MFDButton5Pressed()
	{
		this.SelectUpgrade(5);
	}

	public void NextToolButtonPressed()
	{
		this.selectedToolIndex = (this.selectedToolIndex + 1) % this.totalTools;
		this.SelectTool(this.selectedToolIndex);
	}

	public void PreviousToolButtonPressed()
	{
		this.selectedToolIndex = (this.selectedToolIndex - 1).PositiveModulo(this.totalTools);
		this.SelectTool(this.selectedToolIndex);
	}

	public void UpgradeButtonPressed()
	{
		UnityEvent onSucceeded = this.scanner.onSucceeded;
		if (onSucceeded != null)
		{
			onSucceeded.Invoke();
		}
		GhostReactorProgression.instance.UnlockProgressionTreeNode(this.toolProgressionManager.GetTreeId(), this.currentlySelectedToolUpgrade.id, this.reactor);
	}

	public void ResearchCompleted(bool success, string researchID)
	{
		this.UpdateUI();
	}

	public Color selectedUpgradeColor = Color.yellow;

	public Color unselectedUpgradeColor = Color.black;

	public Color lockedToolColor = Color.red;

	public Color unlockedToolColor = Color.green;

	private int selectedUpgradeIndex;

	[SerializeField]
	private IDCardScanner scanner;

	[SerializeField]
	private TMP_Text BonusText;

	[SerializeField]
	private TMP_Text CostText;

	[SerializeField]
	private TMP_Text DescriptionText;

	[SerializeField]
	private TMP_Text LevelText;

	[SerializeField]
	private TMP_Text ResearchPointsTex;

	[SerializeField]
	private TMP_Text RequiredLevelText;

	[SerializeField]
	private TMP_Text ToolNameText;

	[SerializeField]
	private TMP_Text UnlockedText;

	[SerializeField]
	private TMP_Text[] UpgradePointerText;

	[SerializeField]
	private TMP_Text[] UpgradeTitlesText;

	[SerializeField]
	private Image[] LockedImage;

	[SerializeField]
	private GorillaPressableButton[] UpgradeButton;

	private string _costString;

	private string _levelString;

	private string _researchPointsString;

	private string _requiredLevelString;

	private int selectedToolIndex;

	private int totalTools;

	[NonSerialized]
	private GRToolProgressionManager toolProgressionManager;

	[NonSerialized]
	private List<GRTool.GRToolType> supportedTools = new List<GRTool.GRToolType>();

	[NonSerialized]
	private List<GRToolProgressionTree.GRToolProgressionNode> selectedToolUpgrades = new List<GRToolProgressionTree.GRToolProgressionNode>();

	[NonSerialized]
	private GRToolProgressionTree.GRToolProgressionNode currentlySelectedToolUpgrade = new GRToolProgressionTree.GRToolProgressionNode();

	[NonSerialized]
	private GRToolProgressionManager.ToolProgressionMetaData currentlySelectedUpgradeMetadata = new GRToolProgressionManager.ToolProgressionMetaData();

	[NonSerialized]
	private GhostReactor reactor;
}
