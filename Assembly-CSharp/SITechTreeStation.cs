using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using GorillaTag;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class SITechTreeStation : MonoBehaviour, ITouchScreenStation
{
	public SIScreenRegion ScreenRegion
	{
		get
		{
			return this.screenRegion;
		}
	}

	public SITechTreeNode CurrentNode
	{
		get
		{
			return this.techTreeSO.GetTreeNode(this.parentTerminal.ActivePage, this.currentNodeId);
		}
	}

	public SITechTreePage CurrentPage
	{
		get
		{
			return this.parentTerminal.superInfection.techTreeSO.GetTreePage((SITechTreePageId)this.parentTerminal.ActivePage);
		}
	}

	public SIPlayer ActivePlayer
	{
		get
		{
			return this.parentTerminal.activePlayer;
		}
	}

	public string ActivePlayerName
	{
		get
		{
			return this.ActivePlayer.gamePlayer.rig.OwningNetPlayer.SanitizedNickName;
		}
	}

	public bool IsAuthority
	{
		get
		{
			return this.parentTerminal.superInfection.siManager.gameEntityManager.IsAuthority();
		}
	}

	public GameEntityManager GameEntityManager
	{
		get
		{
			return this.parentTerminal.superInfection.siManager.gameEntityManager;
		}
	}

	public SuperInfectionManager SIManager
	{
		get
		{
			return this.parentTerminal.superInfection.siManager;
		}
	}

	private void CollectButtonColliders()
	{
		SITechTreeStation.<>c__DisplayClass76_0 CS$<>8__locals1;
		CS$<>8__locals1.buttons = base.GetComponentsInChildren<SITouchscreenButton>(true).ToList<SITouchscreenButton>();
		SITechTreeStation.<CollectButtonColliders>g__RemoveButtonsInside|76_2((from d in base.GetComponentsInChildren<DestroyIfNotBeta>()
			select d.gameObject).ToArray<GameObject>(), ref CS$<>8__locals1);
		SITechTreeStation.<CollectButtonColliders>g__RemoveButtonsInside|76_2(new GameObject[] { this.techTreeHelpScreen, this.nodePopupScreen }, ref CS$<>8__locals1);
		this._nonPopupButtonColliders = CS$<>8__locals1.buttons.Select((SITouchscreenButton b) => b.GetComponent<Collider>()).ToList<Collider>();
	}

	private void SetNonPopupButtonsEnabled(bool enable)
	{
		foreach (Collider collider in this._nonPopupButtonColliders)
		{
			collider.enabled = enable;
		}
	}

	private void OnEnable()
	{
		SIProgression instance = SIProgression.Instance;
		instance.OnTreeReady = (Action)Delegate.Combine(instance.OnTreeReady, new Action(this.OnProgressionUpdate));
		SIProgression instance2 = SIProgression.Instance;
		instance2.OnInventoryReady = (Action)Delegate.Combine(instance2.OnInventoryReady, new Action(this.OnProgressionUpdate));
		SIProgression instance3 = SIProgression.Instance;
		instance3.OnNodeUnlocked = (Action<SIUpgradeType>)Delegate.Combine(instance3.OnNodeUnlocked, new Action<SIUpgradeType>(this.OnProgressionUpdateNode));
	}

	private void OnDisable()
	{
		SIProgression instance = SIProgression.Instance;
		instance.OnTreeReady = (Action)Delegate.Remove(instance.OnTreeReady, new Action(this.OnProgressionUpdate));
		SIProgression instance2 = SIProgression.Instance;
		instance2.OnInventoryReady = (Action)Delegate.Remove(instance2.OnInventoryReady, new Action(this.OnProgressionUpdate));
		SIProgression instance3 = SIProgression.Instance;
		instance3.OnNodeUnlocked = (Action<SIUpgradeType>)Delegate.Remove(instance3.OnNodeUnlocked, new Action<SIUpgradeType>(this.OnProgressionUpdateNode));
	}

	public void Initialize()
	{
		if (this.initialized)
		{
			return;
		}
		this.initialized = true;
		if (this.parentTerminal == null)
		{
			this.parentTerminal = base.GetComponentInParent<SICombinedTerminal>();
		}
		this.screenData = new Dictionary<SITechTreeStation.TechTreeStationTerminalState, GameObject>();
		this.screenData.Add(SITechTreeStation.TechTreeStationTerminalState.WaitingForScan, this.waitingForScanScreen);
		this.screenData.Add(SITechTreeStation.TechTreeStationTerminalState.TechTreePagesList, this.pagesListScreen);
		this.screenData.Add(SITechTreeStation.TechTreeStationTerminalState.TechTreePage, this.pageScreen);
		this.screenData.Add(SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup, this.nodePopupScreen);
		this.screenData.Add(SITechTreeStation.TechTreeStationTerminalState.HelpScreen, this.techTreeHelpScreen);
		this.techTreeSO.EnsureInitialized();
		this.pageButtons = new List<SIGadgetListEntry>();
		this.techTreePages = new List<SITechTreeUIPage>();
		this.spriteByType.Add(SIResource.ResourceType.TechPoint, this.techPointSprite);
		this.spriteByType.Add(SIResource.ResourceType.StrangeWood, this.strangeWoodSprite);
		this.spriteByType.Add(SIResource.ResourceType.WeirdGear, this.weirdGearSprite);
		this.spriteByType.Add(SIResource.ResourceType.VibratingSpring, this.vibratingSpringSprite);
		this.spriteByType.Add(SIResource.ResourceType.BouncySand, this.bouncySandSprite);
		this.spriteByType.Add(SIResource.ResourceType.FloppyMetal, this.floppyMetalSprite);
		this.techTreeIconById.Add(SITechTreePageId.Thruster, this.thrustersIcon);
		this.techTreeIconById.Add(SITechTreePageId.Stilt, this.longArmsIcon);
		this.techTreeIconById.Add(SITechTreePageId.Dash, this.dashYoYoIcon);
		this.techTreeIconById.Add(SITechTreePageId.Platform, this.platformsIcon);
		for (int i = 0; i < this.techTreeSO.TreePages.Count; i++)
		{
			SITechTreePage sitechTreePage = this.techTreeSO.TreePages[i];
			if (sitechTreePage.IsValid)
			{
				SIGadgetListEntry sigadgetListEntry = Object.Instantiate<SIGadgetListEntry>(this.pageListEntryPrefab, this.pageListParent);
				StaticLodManager.TryAddLateInstantiatedMembers(sigadgetListEntry.gameObject);
				sigadgetListEntry.Configure(this, sitechTreePage, this.parentTerminal.zeroZeroImage, this.parentTerminal.onePointTwoText, SITouchscreenButton.SITouchscreenButtonType.PageSelect, i, -0.07f);
				this.pageButtons.Add(sigadgetListEntry);
				SITechTreeUIPage sitechTreeUIPage = Object.Instantiate<SITechTreeUIPage>(this.pagePrefab, this.pageParent);
				StaticLodManager.TryAddLateInstantiatedMembers(sitechTreeUIPage.gameObject);
				sitechTreeUIPage.Configure(this, sitechTreePage, this.parentTerminal.zeroZeroImage, this.parentTerminal.onePointTwoText);
				this.techTreePages.Add(sitechTreeUIPage);
			}
		}
		this.Reset();
	}

	public void Reset()
	{
		this.currentState = SITechTreeStation.TechTreeStationTerminalState.WaitingForScan;
		this.nodePopupState = SITechTreeStation.NodePopupState.Description;
		this.SetScreenVisibility(this.currentState, this.currentState);
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy)
		{
			this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.WaitingForScan, SITechTreeStation.TechTreeStationTerminalState.WaitingForScan);
		}
		stream.SendNext(this.currentNodeId);
		stream.SendNext(this.helpScreenIndex);
		stream.SendNext((int)this.nodePopupState);
		stream.SendNext((int)this.currentState);
		stream.SendNext((int)this.lastState);
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		this.currentNodeId = (int)stream.ReceiveNext();
		if (this.CurrentNode == null)
		{
			this.currentNodeId = (int)this.CurrentPage.AllNodes[0].Value.upgradeType;
		}
		this.helpScreenIndex = Mathf.Clamp((int)stream.ReceiveNext(), 0, this.helpPopupScreens.Length - 1);
		this.nodePopupState = (SITechTreeStation.NodePopupState)stream.ReceiveNext();
		if (!Enum.IsDefined(typeof(SITechTreeStation.NodePopupState), this.nodePopupState))
		{
			this.nodePopupState = SITechTreeStation.NodePopupState.Description;
		}
		SITechTreeStation.TechTreeStationTerminalState techTreeStationTerminalState = (SITechTreeStation.TechTreeStationTerminalState)stream.ReceiveNext();
		SITechTreeStation.TechTreeStationTerminalState techTreeStationTerminalState2 = (SITechTreeStation.TechTreeStationTerminalState)stream.ReceiveNext();
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy || !Enum.IsDefined(typeof(SITechTreeStation.TechTreeStationTerminalState), techTreeStationTerminalState) || !Enum.IsDefined(typeof(SITechTreeStation.TechTreeStationTerminalState), techTreeStationTerminalState2))
		{
			this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.WaitingForScan, SITechTreeStation.TechTreeStationTerminalState.WaitingForScan);
			return;
		}
		this.UpdateState(techTreeStationTerminalState, techTreeStationTerminalState2);
	}

	public void ZoneDataSerializeWrite(BinaryWriter writer)
	{
		writer.Write(this.currentNodeId);
		writer.Write(this.helpScreenIndex);
		writer.Write((int)this.nodePopupState);
		writer.Write((int)this.currentState);
		writer.Write((int)this.lastState);
	}

	public void ZoneDataSerializeRead(BinaryReader reader)
	{
		this.currentNodeId = reader.ReadInt32();
		if (!Enum.IsDefined(typeof(SIUpgradeType), this.CurrentNode.upgradeType))
		{
			GTDev.LogError<string>("issue with currentnodeid wee woo wee woo", null);
			this.currentNodeId = (int)this.CurrentPage.AllNodes[0].Value.upgradeType;
		}
		this.helpScreenIndex = Mathf.Clamp(reader.ReadInt32(), 0, this.helpPopupScreens.Length - 1);
		this.nodePopupState = (SITechTreeStation.NodePopupState)reader.ReadInt32();
		if (!Enum.IsDefined(typeof(SITechTreeStation.NodePopupState), this.nodePopupState))
		{
			this.nodePopupState = SITechTreeStation.NodePopupState.Description;
		}
		SITechTreeStation.TechTreeStationTerminalState techTreeStationTerminalState = (SITechTreeStation.TechTreeStationTerminalState)reader.ReadInt32();
		SITechTreeStation.TechTreeStationTerminalState techTreeStationTerminalState2 = (SITechTreeStation.TechTreeStationTerminalState)reader.ReadInt32();
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy || !Enum.IsDefined(typeof(SITechTreeStation.TechTreeStationTerminalState), techTreeStationTerminalState) || !Enum.IsDefined(typeof(SITechTreeStation.TechTreeStationTerminalState), techTreeStationTerminalState2))
		{
			this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.WaitingForScan, SITechTreeStation.TechTreeStationTerminalState.WaitingForScan);
			return;
		}
		this.UpdateState(techTreeStationTerminalState, techTreeStationTerminalState2);
	}

	public void UpdateState(SITechTreeStation.TechTreeStationTerminalState newState, SITechTreeStation.TechTreeStationTerminalState newLastState)
	{
		if (!this.IsPopupState(newLastState))
		{
			this.currentState = newLastState;
		}
		this.UpdateState(newState);
	}

	public void UpdateState(SITechTreeStation.TechTreeStationTerminalState newState)
	{
		if (!this.IsPopupState(this.currentState))
		{
			this.lastState = this.currentState;
		}
		this.currentState = newState;
		this.SetScreenVisibility(this.currentState, this.lastState);
		switch (this.currentState)
		{
		case SITechTreeStation.TechTreeStationTerminalState.WaitingForScan:
			break;
		case SITechTreeStation.TechTreeStationTerminalState.TechTreePagesList:
			this.playerNameText.text = this.ActivePlayerName;
			this.screenDescriptionText.text = "TECH TREE PAGES";
			return;
		case SITechTreeStation.TechTreeStationTerminalState.TechTreePage:
		{
			this.playerNameText.text = this.ActivePlayerName;
			this.UpdateNodeData(this.ActivePlayer);
			TMP_Text tmp_Text = this.screenDescriptionText;
			SITechTreePage treePage = this.techTreeSO.GetTreePage((SITechTreePageId)this.parentTerminal.ActivePage);
			tmp_Text.text = ((treePage != null) ? treePage.nickName : null);
			foreach (SIGadgetListEntry sigadgetListEntry in this.pageButtons)
			{
				sigadgetListEntry.selectionIndicator.SetActive(sigadgetListEntry.Id == this.parentTerminal.ActivePage);
			}
			foreach (SITechTreeUIPage sitechTreeUIPage in this.techTreePages)
			{
				sitechTreeUIPage.gameObject.SetActive(sitechTreeUIPage.id == (SITechTreePageId)this.parentTerminal.ActivePage);
			}
			this.techTreeIcon.sprite = this.techTreeIconById[(SITechTreePageId)this.parentTerminal.ActivePage];
			return;
		}
		case SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup:
			switch (this.nodePopupState)
			{
			case SITechTreeStation.NodePopupState.Description:
				this.nodeNameText.text = this.CurrentNode.nickName;
				this.nodeDescriptionText.text = this.CurrentNode.description;
				if (this.ActivePlayer.NodeResearched(this.CurrentNode.upgradeType))
				{
					this.nodeResearched.SetActive(true);
					this.nodeLocked.SetActive(false);
					this.nodeAvailable.SetActive(false);
					this.nodeResearchButton.SetActive(false);
					this.canAffordNode.SetActive(false);
					this.cantAffordNode.SetActive(false);
				}
				else if (this.ActivePlayer.NodeParentsUnlocked(this.CurrentNode.upgradeType))
				{
					this.nodeResearched.SetActive(false);
					this.nodeLocked.SetActive(false);
					this.nodeAvailable.SetActive(true);
					this.nodeResearchButton.SetActive(true);
					bool flag = this.ActivePlayer.PlayerCanAffordNode(this.CurrentNode);
					this.canAffordNode.SetActive(flag);
					this.cantAffordNode.SetActive(!flag);
				}
				else
				{
					this.nodeResearched.SetActive(false);
					this.nodeAvailable.SetActive(false);
					this.nodeLocked.SetActive(true);
					this.nodeResearchButton.SetActive(false);
					this.canAffordNode.SetActive(false);
					this.cantAffordNode.SetActive(false);
				}
				this.nodeResourceTypeText.text = this.FormattedCurrentResourceTypesForNode(this.CurrentNode);
				this.nodeResourceCostText.text = this.FormattedResearchCost(this.CurrentNode);
				this.playerCurrentResourceAmountsText.text = this.FormattedCurrentResourceAmountsForNode(this.CurrentNode);
				break;
			case SITechTreeStation.NodePopupState.NotEnoughResources:
				this.nodeNameResearchMessageText.text = "NOT ENOUGH RESOURCES TO UNLOCK NODE! GATHER MORE AND TRY AGAIN!";
				break;
			case SITechTreeStation.NodePopupState.Success:
				this.nodeNameResearchMessageText.text = "SUCCESSFULLY UNLOCKED TECH NODE!";
				break;
			case SITechTreeStation.NodePopupState.Loading:
				if (this.ActivePlayer.NodeResearched(this.CurrentNode.upgradeType))
				{
					this.nodePopupState = SITechTreeStation.NodePopupState.Success;
					this.nodeNameResearchMessageText.text = "SUCCESSFULLY UNLOCKED TECH NODE!";
				}
				else
				{
					this.nodeNameResearchMessageText.text = "ATTEMPTING TO UNLOCK NODE\n\nLOADING . . .";
				}
				break;
			}
			this.UpdateNodePopupPage();
			return;
		case SITechTreeStation.TechTreeStationTerminalState.HelpScreen:
			this.UpdateHelpButtonPage(this.helpScreenIndex);
			break;
		default:
			return;
		}
	}

	public void SetScreenVisibility(SITechTreeStation.TechTreeStationTerminalState currentState, SITechTreeStation.TechTreeStationTerminalState lastState)
	{
		bool flag = this.IsPopupState(currentState);
		this.background.color = ((currentState == SITechTreeStation.TechTreeStationTerminalState.WaitingForScan) ? Color.white : ((this.ActivePlayer != null && this.ActivePlayer.gamePlayer.IsLocal()) ? this.active : this.notActive));
		foreach (SITechTreeStation.TechTreeStationTerminalState techTreeStationTerminalState in this.screenData.Keys)
		{
			if (techTreeStationTerminalState == SITechTreeStation.TechTreeStationTerminalState.TechTreePagesList)
			{
				this.screenData[techTreeStationTerminalState].SetActive(currentState > SITechTreeStation.TechTreeStationTerminalState.WaitingForScan);
			}
			else
			{
				bool flag2 = techTreeStationTerminalState == currentState || (flag && techTreeStationTerminalState == lastState);
				if (this.screenData[techTreeStationTerminalState].activeSelf != flag2)
				{
					this.screenData[techTreeStationTerminalState].SetActive(flag2);
				}
			}
		}
		if (this.popupScreen.activeSelf != flag)
		{
			this.popupScreen.SetActive(flag);
		}
		bool flag3 = currentState > SITechTreeStation.TechTreeStationTerminalState.WaitingForScan;
		this.screenDescriptionText.gameObject.SetActive(flag3);
		this.playerNameText.gameObject.SetActive(flag3);
		this.SetNonPopupButtonsEnabled(!flag);
	}

	public bool IsPopupState(SITechTreeStation.TechTreeStationTerminalState state)
	{
		return state == SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup || state == SITechTreeStation.TechTreeStationTerminalState.HelpScreen;
	}

	public void PlayerHandScanned(int actorNr)
	{
		if (!this.IsAuthority)
		{
			this.parentTerminal.PlayerHandScanned(actorNr);
			return;
		}
		this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreePage);
	}

	public void AddButton(SITouchscreenButton button, bool isPopupButton = false)
	{
		if (!isPopupButton)
		{
			this._nonPopupButtonColliders.Add(button.GetComponent<Collider>());
		}
	}

	public void TouchscreenButtonPressed(SITouchscreenButton.SITouchscreenButtonType buttonType, int data, int actorNr)
	{
		if (actorNr == SIPlayer.LocalPlayer.ActorNr && this.ActivePlayer == SIPlayer.LocalPlayer && this.currentState == SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup && this.nodePopupState == SITechTreeStation.NodePopupState.Description && buttonType == SITouchscreenButton.SITouchscreenButtonType.Research && !SIPlayer.LocalPlayer.NodeResearched(this.CurrentNode.upgradeType) && SIPlayer.LocalPlayer.NodeParentsUnlocked(this.CurrentNode.upgradeType))
		{
			SIProgression.Instance.TryUnlock(this.CurrentNode.upgradeType);
		}
		if (!this.IsAuthority)
		{
			this.parentTerminal.TouchscreenButtonPressed(buttonType, data, actorNr, SICombinedTerminal.TerminalSubFunction.TechTree);
			this.soundBankPlayer.Play();
			return;
		}
		if (this.ActivePlayer == null || actorNr != this.ActivePlayer.ActorNr)
		{
			return;
		}
		this.soundBankPlayer.Play();
		if (buttonType == SITouchscreenButton.SITouchscreenButtonType.PageSelect)
		{
			this.parentTerminal.SetActivePage(data);
			this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreePage);
			return;
		}
		switch (this.currentState)
		{
		case SITechTreeStation.TechTreeStationTerminalState.WaitingForScan:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Help)
			{
				this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.HelpScreen);
			}
			return;
		case SITechTreeStation.TechTreeStationTerminalState.TechTreePagesList:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Help)
			{
				this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.HelpScreen);
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Select)
			{
				this.parentTerminal.SetActivePage(data);
				this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreePage);
			}
			return;
		case SITechTreeStation.TechTreeStationTerminalState.TechTreePage:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Select)
			{
				this.currentNodeId = data;
				this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreePagesList);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Help)
			{
				this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.HelpScreen);
				return;
			}
			return;
		case SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup:
			if (this.nodePopupState == SITechTreeStation.NodePopupState.Description)
			{
				if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Exit)
				{
					this.UpdateState(this.lastState);
				}
				if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Research)
				{
					if (this.ActivePlayer.PlayerCanAffordNode(this.CurrentNode))
					{
						this.nodePopupState = SITechTreeStation.NodePopupState.Loading;
					}
					else
					{
						this.nodePopupState = SITechTreeStation.NodePopupState.NotEnoughResources;
					}
					this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup);
					return;
				}
			}
			else if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.nodePopupState = SITechTreeStation.NodePopupState.Description;
				this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreeNodePopup);
			}
			return;
		case SITechTreeStation.TechTreeStationTerminalState.HelpScreen:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Exit)
			{
				this.helpScreenIndex = 0;
				this.UpdateState(this.lastState);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Next)
			{
				this.helpScreenIndex = Mathf.Clamp(this.helpScreenIndex + 1, 0, this.helpPopupScreens.Length - 1);
				this.UpdateHelpButtonPage(this.helpScreenIndex);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.helpScreenIndex = Mathf.Clamp(this.helpScreenIndex - 1, 0, this.helpPopupScreens.Length - 1);
				this.UpdateHelpButtonPage(this.helpScreenIndex);
			}
			return;
		default:
			return;
		}
	}

	public void UpdateHelpButtonPage(int helpButtonPageIndex)
	{
		for (int i = 0; i < this.helpPopupScreens.Length; i++)
		{
			this.helpPopupScreens[i].SetActive(i == helpButtonPageIndex);
		}
	}

	public void UpdateNodePopupPage()
	{
		int num = ((this.nodePopupState == SITechTreeStation.NodePopupState.Description) ? 0 : 1);
		if (this.nodePopupScreens[0].activeSelf != (num == 0))
		{
			this.nodePopupScreens[0].SetActive(num == 0);
		}
		if (this.nodePopupScreens[1].activeSelf != (num == 1))
		{
			this.nodePopupScreens[1].SetActive(num == 1);
		}
	}

	public void UpdateNodeData(SIPlayer player)
	{
		if (player == null)
		{
			for (int i = 0; i < this.techTreePages.Count; i++)
			{
				this.techTreePages[i].PopulateDefaultNodeData();
			}
			return;
		}
		for (int j = 0; j < this.techTreePages.Count; j++)
		{
			this.techTreePages[j].PopulatePlayerNodeData(player);
		}
	}

	public string FormattedResearchCost(SITechTreeNode node)
	{
		SIProgression.SINode sinode;
		if (SIProgression.Instance.GetOnlineNode(node.upgradeType, out sinode))
		{
			string text = "";
			text = text + sinode.costs[SIResource.ResourceType.TechPoint].ToString() + "\n";
			foreach (KeyValuePair<SIResource.ResourceType, int> keyValuePair in sinode.costs)
			{
				if (keyValuePair.Key != SIResource.ResourceType.TechPoint)
				{
					text += keyValuePair.Value.ToString();
					return text;
				}
			}
			return text;
		}
		return string.Join<int>("\n", node.nodeCost.Select((SIResource.ResourceCost c) => c.amount));
	}

	public string FormattedCurrentResourceAmountsForNode(SITechTreeNode node)
	{
		string text = "";
		SIProgression.SINode sinode;
		if (SIProgression.Instance.GetOnlineNode(node.upgradeType, out sinode))
		{
			text = text + this.ActivePlayer.CurrentProgression.resourceArray[0].ToString() + "\n";
			using (Dictionary<SIResource.ResourceType, int>.Enumerator enumerator = sinode.costs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<SIResource.ResourceType, int> keyValuePair = enumerator.Current;
					if (keyValuePair.Key != SIResource.ResourceType.TechPoint)
					{
						text = text + this.ActivePlayer.CurrentProgression.resourceArray[(int)keyValuePair.Key].ToString() + "\n";
					}
				}
				return text;
			}
		}
		for (int i = 0; i < node.nodeCost.Length; i++)
		{
			text = text + this.ActivePlayer.CurrentProgression.resourceArray[(int)node.nodeCost[i].type].ToString() + "\n";
		}
		return text;
	}

	public string FormattedCurrentResourceTypesForNode(SITechTreeNode node)
	{
		string text = "";
		SIProgression.SINode sinode;
		if (SIProgression.Instance.GetOnlineNode(node.upgradeType, out sinode))
		{
			text = text + SIResource.ResourceType.TechPoint.ToString().ToUpperInvariant() + "\n";
			using (Dictionary<SIResource.ResourceType, int>.Enumerator enumerator = sinode.costs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<SIResource.ResourceType, int> keyValuePair = enumerator.Current;
					if (keyValuePair.Key != SIResource.ResourceType.TechPoint)
					{
						text = text + keyValuePair.Key.ToString().ToUpperInvariant() + "\n";
						this.resourceCost.sprite = this.spriteByType[keyValuePair.Key];
					}
				}
				return text;
			}
		}
		for (int i = 0; i < node.nodeCost.Length; i++)
		{
			text = text + node.nodeCost[i].type.ToString().ToUpperInvariant() + "\n";
		}
		return text;
	}

	private void OnProgressionUpdate()
	{
		this.UpdateNodeData(this.ActivePlayer);
		this.UpdateState(this.currentState);
	}

	private void OnProgressionUpdateNode(SIUpgradeType type)
	{
		this.OnProgressionUpdate();
	}

	public void SetActivePage()
	{
		if (this.CurrentNode == null)
		{
			this.currentNodeId = this.CurrentPage.AllNodes[0].Value.upgradeType.GetNodeId();
		}
		if (this.ActivePlayer != null)
		{
			this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.TechTreePage);
			return;
		}
		this.UpdateState(SITechTreeStation.TechTreeStationTerminalState.WaitingForScan);
	}

	public bool IsValidPage(int pageId)
	{
		if (pageId < 0)
		{
			return false;
		}
		using (List<SITechTreeUIPage>.Enumerator enumerator = this.techTreePages.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.id == (SITechTreePageId)pageId)
				{
					return true;
				}
			}
		}
		return false;
	}

	GameObject ITouchScreenStation.get_gameObject()
	{
		return base.gameObject;
	}

	[CompilerGenerated]
	internal static void <CollectButtonColliders>g__RemoveButtonsInside|76_2(GameObject[] roots, ref SITechTreeStation.<>c__DisplayClass76_0 A_1)
	{
		for (int i = 0; i < roots.Length; i++)
		{
			foreach (SITouchscreenButton sitouchscreenButton in roots[i].GetComponentsInChildren<SITouchscreenButton>(true))
			{
				A_1.buttons.Remove(sitouchscreenButton);
			}
		}
	}

	private Dictionary<SITechTreeStation.TechTreeStationTerminalState, GameObject> screenData;

	public SITechTreeStation.TechTreeStationTerminalState currentState;

	public SITechTreeStation.TechTreeStationTerminalState lastState;

	public SICombinedTerminal parentTerminal;

	public Sprite techPointSprite;

	public Sprite strangeWoodSprite;

	public Sprite weirdGearSprite;

	public Sprite vibratingSpringSprite;

	public Sprite bouncySandSprite;

	public Sprite floppyMetalSprite;

	public Sprite thrustersIcon;

	public Sprite longArmsIcon;

	public Sprite dashYoYoIcon;

	public Sprite platformsIcon;

	public int currentNodeId;

	public SITechTreeSO techTreeSO;

	public GameObject waitingForScanScreen;

	public GameObject pagesListScreen;

	public GameObject pageScreen;

	public GameObject nodePopupScreen;

	public GameObject techTreeHelpScreen;

	[SerializeField]
	private SIScreenRegion screenRegion;

	public Color active;

	public Color notActive;

	[Header("Main Screen Shared")]
	public TextMeshProUGUI screenDescriptionText;

	public TextMeshProUGUI playerNameText;

	public Image background;

	[Header("Popup Shared")]
	public GameObject popupScreen;

	[Header("Pages List")]
	[SerializeField]
	private Transform pageListParent;

	[SerializeField]
	private SIGadgetListEntry pageListEntryPrefab;

	private List<SIGadgetListEntry> pageButtons;

	[Header("Tree Page")]
	[SerializeField]
	private Transform pageParent;

	[SerializeField]
	private SITechTreeUIPage pagePrefab;

	private List<SITechTreeUIPage> techTreePages;

	[SerializeField]
	private SpriteRenderer techTreeIcon;

	[Header("Node Popup")]
	public GameObject[] nodePopupScreens;

	[Header("Research Node Description")]
	public TextMeshProUGUI nodeNameText;

	public TextMeshProUGUI nodeDescriptionText;

	public TextMeshProUGUI nodeResourceTypeText;

	public TextMeshProUGUI nodeResourceCostText;

	public TextMeshProUGUI playerCurrentResourceAmountsText;

	public GameObject nodeAvailable;

	public GameObject nodeLocked;

	public GameObject nodeResearched;

	public GameObject canAffordNode;

	public GameObject cantAffordNode;

	public GameObject nodeResearchButton;

	public SpriteRenderer techPointCost;

	public SpriteRenderer resourceCost;

	[Header("Research Attempt")]
	public TextMeshProUGUI nodeNameResearchMessageText;

	public SITechTreeStation.NodePopupState nodePopupState;

	[Header("Help")]
	public int helpScreenIndex;

	public GameObject[] helpPopupScreens;

	[Header("Audio")]
	[SerializeField]
	private SoundBankPlayer soundBankPlayer;

	[Header("Main Screen Colliders")]
	[Tooltip("Button colliders to disable while popup screen is shown.  Gets updated live to include page and gadget node buttons.")]
	[SerializeField]
	private List<Collider> _nonPopupButtonColliders;

	private Dictionary<SIResource.ResourceType, Sprite> spriteByType = new Dictionary<SIResource.ResourceType, Sprite>();

	private Dictionary<SITechTreePageId, Sprite> techTreeIconById = new Dictionary<SITechTreePageId, Sprite>();

	private bool initialized;

	public enum NodePopupState
	{
		Description,
		NotEnoughResources,
		Success,
		PurchaseInitiation,
		Loading
	}

	public enum TechTreeStationTerminalState
	{
		WaitingForScan,
		TechTreePagesList,
		TechTreePage,
		TechTreeNodePopup,
		HelpScreen
	}
}
