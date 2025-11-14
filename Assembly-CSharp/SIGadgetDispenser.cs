using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SIGadgetDispenser : MonoBehaviour, ITouchScreenStation
{
	public SIScreenRegion ScreenRegion
	{
		get
		{
			return this.screenRegion;
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

	public SuperInfectionManager SIManager
	{
		get
		{
			return this.parentTerminal.superInfection.siManager;
		}
	}

	public GameEntityManager GameEntityManager
	{
		get
		{
			return this.parentTerminal.superInfection.siManager.gameEntityManager;
		}
	}

	public SITechTreeNode CurrentNode
	{
		get
		{
			return this.parentTerminal.superInfection.techTreeSO.GetTreeNode(this.parentTerminal.ActivePage, this._currentNode);
		}
	}

	public SITechTreePage CurrentPage
	{
		get
		{
			return this.parentTerminal.superInfection.techTreeSO.GetTreePage((SITechTreePageId)this.parentTerminal.ActivePage);
		}
	}

	public SITechTreeSO TechTreeSO
	{
		get
		{
			return this.parentTerminal.superInfection.techTreeSO;
		}
	}

	private void CollectButtonColliders()
	{
		SIGadgetDispenser.<>c__DisplayClass52_0 CS$<>8__locals1;
		CS$<>8__locals1.buttons = base.GetComponentsInChildren<SITouchscreenButton>(true).ToList<SITouchscreenButton>();
		SIGadgetDispenser.<CollectButtonColliders>g__RemoveButtonsInside|52_2((from d in base.GetComponentsInChildren<DestroyIfNotBeta>()
			select d.gameObject).ToArray<GameObject>(), ref CS$<>8__locals1);
		SIGadgetDispenser.<CollectButtonColliders>g__RemoveButtonsInside|52_2(new GameObject[] { this.gadgetDispensedScreen, this.gadgetsHelpScreen }, ref CS$<>8__locals1);
		this._nonPopupButtonColliders = CS$<>8__locals1.buttons.Select((SITouchscreenButton b) => b.GetComponent<Collider>()).ToList<Collider>();
	}

	private void SetNonPopupButtonsEnabled(bool enable)
	{
		foreach (Collider collider in this._nonPopupButtonColliders)
		{
			collider.enabled = enable;
		}
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
		this.screenData = new Dictionary<SIGadgetDispenser.GadgetDispenserTerminalState, GameObject>();
		this.screenData.Add(SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan, this.waitingForScanScreen);
		this.screenData.Add(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetList, this.gadgetListScreen);
		this.screenData.Add(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetInformation, this.gadgetInformationScreen);
		this.screenData.Add(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetDispensed, this.gadgetDispensedScreen);
		this.screenData.Add(SIGadgetDispenser.GadgetDispenserTerminalState.HelpScreen, this.gadgetsHelpScreen);
		this.parentTerminal.superInfection.techTreeSO.EnsureInitialized();
		int num = 0;
		this.gadgetPages = new List<SIGadgetListEntry>();
		for (int i = 0; i < this.parentTerminal.superInfection.techTreeSO.TreePages.Count; i++)
		{
			SITechTreePage sitechTreePage = this.parentTerminal.superInfection.techTreeSO.TreePages[i];
			SIGadgetListEntry sigadgetListEntry = Object.Instantiate<SIGadgetListEntry>(this.pageListEntryPrefab, this.pageListParent);
			sigadgetListEntry.Configure(this, sitechTreePage, this.parentTerminal.zeroZeroImage, this.parentTerminal.onePointTwoText, SITouchscreenButton.SITouchscreenButtonType.Select, i, -0.07f);
			this.gadgetPages.Add(sigadgetListEntry);
			num = Math.Max(num, sitechTreePage.DispensableGadgets.Count);
		}
		this.gadgetEntries = new List<SIDispenserGadgetListEntry>();
		for (int j = 0; j < num; j++)
		{
			SIDispenserGadgetListEntry sidispenserGadgetListEntry = Object.Instantiate<SIDispenserGadgetListEntry>(this.gadgetListEntryPrefab, this.gadgetListParent);
			sidispenserGadgetListEntry.transform.localPosition += new Vector3(0f, (float)j * -0.07f, 0f);
			sidispenserGadgetListEntry.SetStation(this, this.parentTerminal.zeroZeroImage, this.parentTerminal.onePointTwoText);
			this.gadgetEntries.Add(sidispenserGadgetListEntry);
		}
		this.Reset();
	}

	public void Reset()
	{
		this.currentState = SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan;
		this.SetScreenVisibility(this.currentState, this.currentState);
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy)
		{
			this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan, SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan);
		}
		stream.SendNext(this.helpScreenIndex);
		stream.SendNext(this._currentNode);
		stream.SendNext((int)this.currentState);
		stream.SendNext((int)this.lastState);
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		this.helpScreenIndex = Mathf.Clamp((int)stream.ReceiveNext(), 0, this.helpPopupScreens.Length - 1);
		this._currentNode = (int)stream.ReceiveNext();
		if (this.CurrentNode == null && this.CurrentPage != null && this.CurrentPage.AllNodes.Count > 0 && this.CurrentPage.AllNodes[0].Value != null)
		{
			this._currentNode = (int)this.CurrentPage.AllNodes[0].Value.upgradeType;
		}
		SIGadgetDispenser.GadgetDispenserTerminalState gadgetDispenserTerminalState = (SIGadgetDispenser.GadgetDispenserTerminalState)stream.ReceiveNext();
		SIGadgetDispenser.GadgetDispenserTerminalState gadgetDispenserTerminalState2 = (SIGadgetDispenser.GadgetDispenserTerminalState)stream.ReceiveNext();
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy || !Enum.IsDefined(typeof(SIGadgetDispenser.GadgetDispenserTerminalState), gadgetDispenserTerminalState) || !Enum.IsDefined(typeof(SIGadgetDispenser.GadgetDispenserTerminalState), gadgetDispenserTerminalState2))
		{
			this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan, SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan);
			return;
		}
		this.UpdateState(gadgetDispenserTerminalState, gadgetDispenserTerminalState2);
	}

	public void ZoneDataSerializeWrite(BinaryWriter writer)
	{
		writer.Write(this.helpScreenIndex);
		writer.Write(this._currentNode);
		writer.Write((int)this.currentState);
		writer.Write((int)this.lastState);
	}

	public void ZoneDataSerializeRead(BinaryReader reader)
	{
		this.helpScreenIndex = Mathf.Clamp(reader.ReadInt32(), 0, this.helpPopupScreens.Length - 1);
		int num = reader.ReadInt32();
		if (this.CurrentPage != null && this.CurrentPage.AllNodes != null)
		{
			this._currentNode = Mathf.Clamp(num, 0, this.CurrentPage.AllNodes.Count - 1);
		}
		else
		{
			this._currentNode = 0;
		}
		SIGadgetDispenser.GadgetDispenserTerminalState gadgetDispenserTerminalState = (SIGadgetDispenser.GadgetDispenserTerminalState)reader.ReadInt32();
		SIGadgetDispenser.GadgetDispenserTerminalState gadgetDispenserTerminalState2 = (SIGadgetDispenser.GadgetDispenserTerminalState)reader.ReadInt32();
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy || !Enum.IsDefined(typeof(SIGadgetDispenser.GadgetDispenserTerminalState), gadgetDispenserTerminalState) || !Enum.IsDefined(typeof(SIGadgetDispenser.GadgetDispenserTerminalState), gadgetDispenserTerminalState2))
		{
			this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan, SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan);
			return;
		}
		this.UpdateState(gadgetDispenserTerminalState, gadgetDispenserTerminalState2);
	}

	public void UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState newState, SIGadgetDispenser.GadgetDispenserTerminalState newLastState)
	{
		if (!this.IsPopupState(newLastState))
		{
			this.currentState = newLastState;
		}
		this.UpdateState(newState);
	}

	public void UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState newState)
	{
		if (!this.IsPopupState(this.currentState))
		{
			this.lastState = this.currentState;
		}
		this.currentState = newState;
		this.SetScreenVisibility(this.currentState, this.lastState);
		switch (this.currentState)
		{
		case SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan:
			break;
		case SIGadgetDispenser.GadgetDispenserTerminalState.GadgetList:
			this.screenDescription.text = "UNLOCKED " + this.CurrentPage.nickName + " GADGETS";
			this.UpdateGadgetListVisibility();
			return;
		case SIGadgetDispenser.GadgetDispenserTerminalState.GadgetInformation:
			this.screenDescription.text = this.CurrentNode.nickName;
			this.gadgetDescriptionText.text = this.CurrentNode.description;
			return;
		case SIGadgetDispenser.GadgetDispenserTerminalState.GadgetDispensed:
			this.gadgetDispensedText.text = this.ActivePlayerName + " HAS DISPENSED A " + this.CurrentNode.nickName + "!";
			return;
		case SIGadgetDispenser.GadgetDispenserTerminalState.HelpScreen:
			this.UpdateHelpButtonPage(this.helpScreenIndex);
			break;
		default:
			return;
		}
	}

	public void SetScreenVisibility(SIGadgetDispenser.GadgetDispenserTerminalState currentState, SIGadgetDispenser.GadgetDispenserTerminalState lastState)
	{
		bool flag = this.IsPopupState(currentState);
		this.background.color = ((currentState == SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan) ? Color.white : ((this.ActivePlayer != null && this.ActivePlayer.gamePlayer.IsLocal()) ? this.active : this.notActive));
		foreach (SIGadgetDispenser.GadgetDispenserTerminalState gadgetDispenserTerminalState in this.screenData.Keys)
		{
			bool flag2 = gadgetDispenserTerminalState == currentState || (flag && gadgetDispenserTerminalState == lastState);
			if (this.screenData[gadgetDispenserTerminalState].activeSelf != flag2)
			{
				this.screenData[gadgetDispenserTerminalState].SetActive(flag2);
			}
		}
		if (this.popupScreen.activeSelf != flag)
		{
			this.popupScreen.SetActive(flag);
		}
		this.screenDescription.gameObject.SetActive(currentState > SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan);
		this.SetNonPopupButtonsEnabled(!flag);
	}

	public void UpdateGadgetListVisibility()
	{
		foreach (SIDispenserGadgetListEntry sidispenserGadgetListEntry in this.gadgetEntries)
		{
			sidispenserGadgetListEntry.gameObject.SetActive(false);
		}
		int num = 0;
		foreach (SITechTreeNode sitechTreeNode in this.CurrentPage.DispensableGadgets)
		{
			if (this.ActivePlayer.CurrentProgression.IsUnlocked(sitechTreeNode.upgradeType))
			{
				SIDispenserGadgetListEntry sidispenserGadgetListEntry2 = this.gadgetEntries[num++];
				sidispenserGadgetListEntry2.SetTechTreeNode(sitechTreeNode);
				sidispenserGadgetListEntry2.gameObject.SetActive(true);
			}
		}
		this.noDispensableGadgetsMessage.SetActive(num == 0);
	}

	public bool IsPopupState(SIGadgetDispenser.GadgetDispenserTerminalState state)
	{
		return state == SIGadgetDispenser.GadgetDispenserTerminalState.GadgetDispensed || state == SIGadgetDispenser.GadgetDispenserTerminalState.HelpScreen;
	}

	public void PlayerHandScanned(int actorNr)
	{
		this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetList);
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
		if (actorNr == SIPlayer.LocalPlayer.ActorNr && (this.ActivePlayer == null || this.ActivePlayer != SIPlayer.LocalPlayer))
		{
			this.parentTerminal.PlayWrongPlayerBuzz(this.uiCenter);
		}
		else
		{
			this.touchSoundBankPlayer.Play();
		}
		if (!this.IsAuthority)
		{
			this.parentTerminal.TouchscreenButtonPressed(buttonType, data, actorNr, SICombinedTerminal.TerminalSubFunction.GadgetDispenser);
			return;
		}
		if (actorNr != this.ActivePlayer.ActorNr)
		{
			return;
		}
		this.touchSoundBankPlayer.Play();
		switch (this.currentState)
		{
		case SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Help)
			{
				this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.HelpScreen);
			}
			return;
		case SIGadgetDispenser.GadgetDispenserTerminalState.GadgetList:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Help)
			{
				this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.HelpScreen);
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Select)
			{
				SITechTreeNode treeNode = this.TechTreeSO.GetTreeNode((int)this.CurrentPage.pageId, data);
				if (treeNode != null && treeNode.IsDispensableGadget)
				{
					this._currentNode = data;
					this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetInformation);
				}
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Dispense)
			{
				SITechTreeNode treeNode2 = this.TechTreeSO.GetTreeNode((int)this.CurrentPage.pageId, data);
				if (treeNode2 != null && treeNode2.IsDispensableGadget)
				{
					this._currentNode = data;
					this.DispenseGadgetForPlayer(this.ActivePlayer);
					this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetDispensed);
				}
			}
			return;
		case SIGadgetDispenser.GadgetDispenserTerminalState.GadgetInformation:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Help)
			{
				this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.HelpScreen);
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetList);
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Dispense)
			{
				this.DispenseGadgetForPlayer(this.ActivePlayer);
				this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetDispensed);
			}
			return;
		case SIGadgetDispenser.GadgetDispenserTerminalState.GadgetDispensed:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Exit)
			{
				this.UpdateState(this.lastState);
			}
			return;
		case SIGadgetDispenser.GadgetDispenserTerminalState.HelpScreen:
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

	public void DispenseGadgetForPlayer(SIPlayer player)
	{
		int num = 0;
		int staticHash = this.CurrentNode.unlockedGadgetPrefab.name.GetStaticHash();
		for (int i = player.activePlayerGadgets.Count - 1; i >= 0; i--)
		{
			GameEntity gameEntityFromNetId = this.GameEntityManager.GetGameEntityFromNetId(player.activePlayerGadgets[i]);
			if (gameEntityFromNetId == null)
			{
				player.activePlayerGadgets.RemoveAt(i);
			}
			else
			{
				num++;
				if (num >= player.totalGadgetLimit)
				{
					this.GameEntityManager.RequestDestroyItem(gameEntityFromNetId.id);
					break;
				}
			}
		}
		SIUpgradeSet upgrades = player.GetUpgrades(this.CurrentPage.pageId);
		int num2 = 0;
		foreach (GraphNode<SITechTreeNode> graphNode in this.CurrentPage.AllNodes)
		{
			num2 |= 1 << graphNode.Value.upgradeType.GetNodeId();
		}
		upgrades.SetBits(upgrades.GetBits() & num2);
		foreach (SITechTreeNode sitechTreeNode in this.CurrentPage.DispensableGadgets)
		{
			if (sitechTreeNode != this.CurrentNode)
			{
				upgrades.Remove(sitechTreeNode.upgradeType);
			}
		}
		this.GameEntityManager.RequestCreateItem(staticHash, this.gadgetDispensePosition.position, this.gadgetDispensePosition.rotation, upgrades.GetCreateData(player));
		this.dispenseSoundBankPlayer.Play();
	}

	public void SetActivePage()
	{
		if (this.CurrentNode == null)
		{
			this._currentNode = this.CurrentPage.AllNodes[0].Value.upgradeType.GetNodeId();
		}
		if (this.ActivePlayer != null)
		{
			this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.GadgetList);
			return;
		}
		this.UpdateState(SIGadgetDispenser.GadgetDispenserTerminalState.WaitingForScan);
	}

	public bool IsValidPage(int pageId)
	{
		if (pageId < 0)
		{
			return false;
		}
		using (List<SIGadgetListEntry>.Enumerator enumerator = this.gadgetPages.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Id == pageId)
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
	internal static void <CollectButtonColliders>g__RemoveButtonsInside|52_2(GameObject[] roots, ref SIGadgetDispenser.<>c__DisplayClass52_0 A_1)
	{
		for (int i = 0; i < roots.Length; i++)
		{
			foreach (SITouchscreenButton sitouchscreenButton in roots[i].GetComponentsInChildren<SITouchscreenButton>(true))
			{
				A_1.buttons.Remove(sitouchscreenButton);
			}
		}
	}

	public SIGadgetDispenser.GadgetDispenserTerminalState currentState;

	public SIGadgetDispenser.GadgetDispenserTerminalState lastState;

	public Transform gadgetDispensePosition;

	public int _currentNode;

	public SICombinedTerminal parentTerminal;

	public GameObject waitingForScanScreen;

	public GameObject gadgetListScreen;

	public GameObject gadgetInformationScreen;

	public GameObject gadgetDispensedScreen;

	public GameObject gadgetsHelpScreen;

	[SerializeField]
	private SIScreenRegion screenRegion;

	[Header("Main Screen Shared")]
	public TextMeshProUGUI screenDescription;

	public Image background;

	public Color active;

	public Color notActive;

	public Transform uiCenter;

	[Header("Popup Shared")]
	public GameObject popupScreen;

	[Header("Gadgets Type")]
	[SerializeField]
	private RectTransform pageListParent;

	[SerializeField]
	private SIGadgetListEntry pageListEntryPrefab;

	private List<SIGadgetListEntry> gadgetPages;

	[FormerlySerializedAs("noDispensableGadgetsNotif")]
	[Header("Gadgets List")]
	[SerializeField]
	private GameObject noDispensableGadgetsMessage;

	[SerializeField]
	private RectTransform gadgetListParent;

	[SerializeField]
	private SIDispenserGadgetListEntry gadgetListEntryPrefab;

	private List<SIDispenserGadgetListEntry> gadgetEntries;

	[Header("Gadgets Description")]
	public TextMeshProUGUI gadgetDescriptionText;

	[Header("Gadget Dispensed")]
	public TextMeshProUGUI gadgetDispensedText;

	[Header("Help")]
	public int helpScreenIndex;

	public GameObject[] helpPopupScreens;

	[Header("Audio")]
	[SerializeField]
	private SoundBankPlayer touchSoundBankPlayer;

	[SerializeField]
	private SoundBankPlayer dispenseSoundBankPlayer;

	[Header("Main Screen Colliders")]
	[Tooltip("Button colliders to disable while popup screen is shown.  Gets updated live to include page and gadget buttons.")]
	[SerializeField]
	private List<Collider> _nonPopupButtonColliders;

	private Dictionary<SIGadgetDispenser.GadgetDispenserTerminalState, GameObject> screenData;

	private bool initialized;

	public enum GadgetDispenserTerminalState
	{
		WaitingForScan,
		GadgetList,
		GadgetInformation,
		GadgetDispensed,
		HelpScreen
	}
}
