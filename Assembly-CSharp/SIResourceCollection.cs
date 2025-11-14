using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SIResourceCollection : MonoBehaviour, ITouchScreenStation
{
	public SIScreenRegion ScreenRegion
	{
		get
		{
			return this.screenRegion;
		}
	}

	public bool IsAuthority
	{
		get
		{
			return this.SIManager.gameEntityManager.IsAuthority();
		}
	}

	public SIPlayer ActivePlayer
	{
		get
		{
			return this.parentTerminal.activePlayer;
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
		SIResourceCollection.<>c__DisplayClass45_0 CS$<>8__locals1;
		CS$<>8__locals1.buttons = base.GetComponentsInChildren<SITouchscreenButton>(true).ToList<SITouchscreenButton>();
		SIResourceCollection.<CollectButtonColliders>g__RemoveButtonsInside|45_2((from d in base.GetComponentsInChildren<DestroyIfNotBeta>()
			select d.gameObject).ToArray<GameObject>(), ref CS$<>8__locals1);
		SIResourceCollection.<CollectButtonColliders>g__RemoveButtonsInside|45_2(new GameObject[] { this.helpScreen }, ref CS$<>8__locals1);
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
		this.screenData = new Dictionary<SIResourceCollection.ResourceCollectorTerminalState, GameObject>();
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan, this.waitingForScanScreen);
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.CurrentResources, this.currentResourcesScreen);
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.HelpScreen, this.helpScreen);
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.PurchaseRemote, this.purchasingRemote);
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.PurchaseStart, this.purchasingStart);
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.PurchaseInProgress, this.purchaseInProgress);
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.PurchaseSuccess, this.purchasingSuccess);
		this.screenData.Add(SIResourceCollection.ResourceCollectorTerminalState.PurchaseFailure, this.purchasingFailure);
		this.Reset();
	}

	public void Reset()
	{
		this.currentState = SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan;
		this.lastState = this.currentState;
		this.SetScreenVisibility(this.currentState, this.lastState);
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy)
		{
			this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan, SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan);
		}
		stream.SendNext(this.currentHelpButtonPageIndex);
		stream.SendNext((int)this.currentState);
		stream.SendNext((int)this.lastState);
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		this.currentHelpButtonPageIndex = Mathf.Clamp((int)stream.ReceiveNext(), 0, this.helpPopupScreens.Length - 1);
		this.UpdateHelpButtonPage(this.currentHelpButtonPageIndex);
		SIResourceCollection.ResourceCollectorTerminalState resourceCollectorTerminalState = (SIResourceCollection.ResourceCollectorTerminalState)stream.ReceiveNext();
		SIResourceCollection.ResourceCollectorTerminalState resourceCollectorTerminalState2 = (SIResourceCollection.ResourceCollectorTerminalState)stream.ReceiveNext();
		if (!Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState) || !Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState2))
		{
			resourceCollectorTerminalState = SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan;
			resourceCollectorTerminalState2 = SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan;
		}
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy || !Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState) || !Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState2))
		{
			this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan, SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan);
			return;
		}
		this.UpdateState(resourceCollectorTerminalState, resourceCollectorTerminalState2);
	}

	public void ZoneDataSerializeWrite(BinaryWriter writer)
	{
		writer.Write(this.currentHelpButtonPageIndex);
		writer.Write((int)this.currentState);
		writer.Write((int)this.lastState);
	}

	public void ZoneDataSerializeRead(BinaryReader reader)
	{
		this.currentHelpButtonPageIndex = Mathf.Clamp(reader.ReadInt32(), 0, this.helpPopupScreens.Length - 1);
		this.UpdateHelpButtonPage(this.currentHelpButtonPageIndex);
		SIResourceCollection.ResourceCollectorTerminalState resourceCollectorTerminalState = (SIResourceCollection.ResourceCollectorTerminalState)reader.ReadInt32();
		SIResourceCollection.ResourceCollectorTerminalState resourceCollectorTerminalState2 = (SIResourceCollection.ResourceCollectorTerminalState)reader.ReadInt32();
		if (!Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState) || !Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState2))
		{
			resourceCollectorTerminalState = SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan;
			resourceCollectorTerminalState2 = SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan;
		}
		if (this.ActivePlayer == null || !this.ActivePlayer.gameObject.activeInHierarchy || !Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState) || !Enum.IsDefined(typeof(SIResourceCollection.ResourceCollectorTerminalState), resourceCollectorTerminalState2))
		{
			this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan, SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan);
			return;
		}
		this.UpdateState(resourceCollectorTerminalState, resourceCollectorTerminalState2);
	}

	public bool PopupActive()
	{
		return this.IsPopupState(this.currentState);
	}

	public bool IsPopupState(SIResourceCollection.ResourceCollectorTerminalState state)
	{
		return state == SIResourceCollection.ResourceCollectorTerminalState.HelpScreen || state == SIResourceCollection.ResourceCollectorTerminalState.PurchaseInProgress || state == SIResourceCollection.ResourceCollectorTerminalState.PurchaseRemote || state == SIResourceCollection.ResourceCollectorTerminalState.PurchaseStart || state == SIResourceCollection.ResourceCollectorTerminalState.PurchaseFailure || state == SIResourceCollection.ResourceCollectorTerminalState.PurchaseSuccess;
	}

	public bool HasHelpButton(SIResourceCollection.ResourceCollectorTerminalState state)
	{
		return state == SIResourceCollection.ResourceCollectorTerminalState.CurrentResources || state == SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan;
	}

	public void UpdateState(SIResourceCollection.ResourceCollectorTerminalState newState, SIResourceCollection.ResourceCollectorTerminalState newLastState)
	{
		if (!this.IsPopupState(newLastState))
		{
			this.currentState = newLastState;
		}
		this.UpdateState(newState);
	}

	public void UpdateState(SIResourceCollection.ResourceCollectorTerminalState newState)
	{
		if (!this.IsPopupState(this.currentState))
		{
			this.lastState = this.currentState;
		}
		this.currentState = newState;
		this.SetScreenVisibility(this.currentState, this.lastState);
		switch (this.currentState)
		{
		case SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan:
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseInProgress:
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseSuccess:
			break;
		case SIResourceCollection.ResourceCollectorTerminalState.CurrentResources:
			this.currentResourcesResourceCounts.text = this.FormattedPlayerResourceCount(this.ActivePlayer);
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.HelpScreen:
			this.UpdateHelpButtonPage(this.currentHelpButtonPageIndex);
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseRemote:
			if (this.ActivePlayer != null && this.ActivePlayer == SIPlayer.LocalPlayer)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.PurchaseStart);
			}
			this.currentResourceCountsLocal.text = this.FormattedPlayerResourceCountWithMax(this.ActivePlayer);
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseStart:
			if (this.ActivePlayer != null && this.ActivePlayer != SIPlayer.LocalPlayer)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.PurchaseRemote);
			}
			else
			{
				this.shinyRockInfo.text = "PRICE: 500 SHINY ROCKS\n\nYOU HAVE:\n" + ProgressionManager.Instance.GetShinyRocksTotal().ToString() + " SHINY ROCKS";
			}
			this.currentResourceCountsLocal.text = this.FormattedPlayerResourceCountWithMax(this.ActivePlayer);
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseFailure:
			switch (this.failureReason)
			{
			case SIResourceCollection.FailReason.NotEnoughRocks:
				this.failureReasonText.text = "NOT ENOUGH SHINY ROCKS! PLEASE TRY AGAIN LATER, OR PURCHASE MORE SHINY ROCKS!";
				return;
			case SIResourceCollection.FailReason.ResourcesFull:
				this.failureReasonText.text = "YOU ARE ALREADY AT MAX RESOURCES! DONATE YOUR SHINY ROCKS TO A GOOD CAUSE INSTEAD OF US, KNUCKLEHEAD!";
				return;
			case SIResourceCollection.FailReason.Unknown:
				this.failureReasonText.text = "UHHHHH SOMETHING WENT WRONG, I'M NOT SURE WHAT, SORRY TRY AGAIN LATER MAYBE!";
				break;
			default:
				return;
			}
			break;
		default:
			return;
		}
	}

	public string FormattedPlayerResourceCount(SIPlayer player)
	{
		return string.Concat(new string[]
		{
			this.GetFormattedResource(player, SIResource.ResourceType.TechPoint),
			"\n",
			this.GetFormattedResource(player, SIResource.ResourceType.StrangeWood),
			"\n",
			this.GetFormattedResource(player, SIResource.ResourceType.WeirdGear),
			"\n",
			this.GetFormattedResource(player, SIResource.ResourceType.VibratingSpring),
			"\n",
			this.GetFormattedResource(player, SIResource.ResourceType.BouncySand),
			"\n",
			this.GetFormattedResource(player, SIResource.ResourceType.FloppyMetal)
		});
	}

	public string FormattedPlayerResourceCountWithMax(SIPlayer player)
	{
		return string.Concat(new string[]
		{
			this.GetFormattedResource(player, SIResource.ResourceType.StrangeWood),
			" -> 20\n",
			this.GetFormattedResource(player, SIResource.ResourceType.WeirdGear),
			" -> 20\n",
			this.GetFormattedResource(player, SIResource.ResourceType.VibratingSpring),
			" -> 20\n",
			this.GetFormattedResource(player, SIResource.ResourceType.BouncySand),
			" -> 20\n",
			this.GetFormattedResource(player, SIResource.ResourceType.FloppyMetal),
			" -> 20"
		});
	}

	private string GetFormattedResource(SIPlayer player, SIResource.ResourceType resource)
	{
		int resourceMaxCap = SIProgression.Instance.GetResourceMaxCap(resource);
		if (resourceMaxCap == 2147483647)
		{
			return player.CurrentProgression.resourceArray[(int)resource].ToString();
		}
		return string.Format("{0}/{1}", player.CurrentProgression.resourceArray[(int)resource], resourceMaxCap);
	}

	public void UpdateHelpButtonPage(int helpButtonPageIndex)
	{
		for (int i = 0; i < this.helpPopupScreens.Length; i++)
		{
			this.helpPopupScreens[i].SetActive(i == helpButtonPageIndex);
		}
	}

	public void SetScreenVisibility(SIResourceCollection.ResourceCollectorTerminalState currentState, SIResourceCollection.ResourceCollectorTerminalState lastState)
	{
		bool flag = this.IsPopupState(currentState);
		this.background.color = ((currentState == SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan) ? Color.white : ((this.ActivePlayer != null && this.ActivePlayer.gamePlayer.IsLocal()) ? this.active : this.notActive));
		foreach (SIResourceCollection.ResourceCollectorTerminalState resourceCollectorTerminalState in this.screenData.Keys)
		{
			bool flag2 = resourceCollectorTerminalState == currentState || (flag && resourceCollectorTerminalState == lastState);
			if (this.screenData[resourceCollectorTerminalState].activeSelf != flag2)
			{
				this.screenData[resourceCollectorTerminalState].SetActive(flag2);
			}
		}
		if (this.popupScreen.activeSelf != flag)
		{
			this.popupScreen.SetActive(flag);
		}
		this.SetNonPopupButtonsEnabled(!flag);
	}

	public void PlayerHandScanned(int actorNr)
	{
		this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.CurrentResources);
	}

	public void TouchscreenButtonPressed(SITouchscreenButton.SITouchscreenButtonType buttonType, int data, int actorNr)
	{
		if (actorNr == SIPlayer.LocalPlayer.ActorNr && (this.ActivePlayer == null || this.ActivePlayer != SIPlayer.LocalPlayer))
		{
			this.parentTerminal.PlayWrongPlayerBuzz(this.uiCenter);
		}
		else
		{
			this.soundBankPlayer.Play();
		}
		if (actorNr == SIPlayer.LocalPlayer.ActorNr && this.ActivePlayer == SIPlayer.LocalPlayer && this.currentState == SIResourceCollection.ResourceCollectorTerminalState.PurchaseStart && buttonType == SITouchscreenButton.SITouchscreenButtonType.Confirm)
		{
			bool flag = ProgressionManager.Instance.GetShinyRocksTotal() >= 500;
			bool flag2 = SIProgression.ResourcesMaxed();
			if (flag && !flag2)
			{
				ProgressionManager.Instance.PurchaseResources(delegate(ProgressionManager.UserInventory userInventoryResponse)
				{
					SIProgression.Instance.SendPurchaseResourcesData();
					ProgressionManager.Instance.RefreshUserInventory();
					this.TouchscreenButtonPressed(SITouchscreenButton.SITouchscreenButtonType.Collect, -1, SIPlayer.LocalPlayer.ActorNr);
				}, delegate(string error)
				{
					SIResourceCollection.FailReason failReason;
					if (!(error == "Not enough Shiny Rocks to complete this purchase"))
					{
						if (!(error == "already maxed resources"))
						{
							failReason = SIResourceCollection.FailReason.Unknown;
						}
						else
						{
							failReason = SIResourceCollection.FailReason.ResourcesFull;
						}
					}
					else
					{
						failReason = SIResourceCollection.FailReason.NotEnoughRocks;
					}
					this.TouchscreenButtonPressed(SITouchscreenButton.SITouchscreenButtonType.OverrideFailure, (int)failReason, SIPlayer.LocalPlayer.ActorNr);
				});
			}
			else
			{
				buttonType = SITouchscreenButton.SITouchscreenButtonType.OverrideFailure;
				if (!flag)
				{
					data = 0;
				}
				else if (flag2)
				{
					data = 1;
				}
				else
				{
					data = 2;
				}
			}
		}
		if (!this.IsAuthority)
		{
			this.parentTerminal.TouchscreenButtonPressed(buttonType, data, actorNr, SICombinedTerminal.TerminalSubFunction.ResourceCollection);
			return;
		}
		if (this.ActivePlayer == null || actorNr != this.ActivePlayer.ActorNr)
		{
			return;
		}
		this.soundBankPlayer.Play();
		switch (this.currentState)
		{
		case SIResourceCollection.ResourceCollectorTerminalState.WaitingForScan:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Help)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.HelpScreen);
			}
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.CurrentResources:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Purchase)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.PurchaseStart);
			}
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.HelpScreen:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Exit)
			{
				this.currentHelpButtonPageIndex = 0;
				this.UpdateState(this.lastState);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Next)
			{
				this.currentHelpButtonPageIndex = Mathf.Clamp(this.currentHelpButtonPageIndex + 1, 0, this.helpPopupScreens.Length - 1);
				this.UpdateHelpButtonPage(this.currentHelpButtonPageIndex);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Back)
			{
				this.currentHelpButtonPageIndex = Mathf.Clamp(this.currentHelpButtonPageIndex - 1, 0, this.helpPopupScreens.Length - 1);
				this.UpdateHelpButtonPage(this.currentHelpButtonPageIndex);
			}
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseRemote:
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseStart:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Confirm)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.PurchaseInProgress);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Cancel)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.CurrentResources);
				return;
			}
			this.failureReason = (SIResourceCollection.FailReason)data;
			this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.PurchaseFailure);
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseInProgress:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.OverrideFailure)
			{
				this.failureReason = (SIResourceCollection.FailReason)data;
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.PurchaseFailure);
				return;
			}
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Collect)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.PurchaseSuccess);
			}
			return;
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseSuccess:
		case SIResourceCollection.ResourceCollectorTerminalState.PurchaseFailure:
			if (buttonType == SITouchscreenButton.SITouchscreenButtonType.Exit)
			{
				this.UpdateState(SIResourceCollection.ResourceCollectorTerminalState.CurrentResources);
			}
			return;
		default:
			return;
		}
	}

	public void AddButton(SITouchscreenButton button, bool isPopupButton = false)
	{
	}

	GameObject ITouchScreenStation.get_gameObject()
	{
		return base.gameObject;
	}

	[CompilerGenerated]
	internal static void <CollectButtonColliders>g__RemoveButtonsInside|45_2(GameObject[] roots, ref SIResourceCollection.<>c__DisplayClass45_0 A_1)
	{
		for (int i = 0; i < roots.Length; i++)
		{
			foreach (SITouchscreenButton sitouchscreenButton in roots[i].GetComponentsInChildren<SITouchscreenButton>(true))
			{
				A_1.buttons.Remove(sitouchscreenButton);
			}
		}
	}

	public const int REFILL_PURCHASE_SHINY_ROCK_COST = 500;

	private const string lineBreak = "\n";

	private const string appendToMax = " -> 20";

	public SIResourceCollection.ResourceCollectorTerminalState currentState;

	public SIResourceCollection.ResourceCollectorTerminalState lastState;

	public int resourceDepositedCount;

	private int currentHelpButtonPageIndex;

	public GameObject waitingForScanScreen;

	public GameObject currentResourcesScreen;

	public GameObject helpScreen;

	public SICombinedTerminal parentTerminal;

	public Sprite[] resourceImageSprites;

	[SerializeField]
	private SIScreenRegion screenRegion;

	public GameObject[] helpPopupScreens;

	public GameObject purchasingRemote;

	public GameObject purchasingStart;

	public GameObject purchaseInProgress;

	public GameObject purchasingSuccess;

	public GameObject purchasingFailure;

	public GameObject popupScreen;

	public Transform uiCenter;

	[Header("Purchasing Pages")]
	public TextMeshProUGUI shinyRockInfo;

	public TextMeshProUGUI currentResourceCountsLocal;

	public TextMeshProUGUI currentResourceCountsRemote;

	public TextMeshProUGUI failureReasonText;

	public const string failureFull = "YOU ARE ALREADY AT MAX RESOURCES! DONATE YOUR SHINY ROCKS TO A GOOD CAUSE INSTEAD OF US, KNUCKLEHEAD!";

	public const string failureNotEnoughRocks = "NOT ENOUGH SHINY ROCKS! PLEASE TRY AGAIN LATER, OR PURCHASE MORE SHINY ROCKS!";

	public const string failureUnknown = "UHHHHH SOMETHING WENT WRONG, I'M NOT SURE WHAT, SORRY TRY AGAIN LATER MAYBE!";

	private SIResourceCollection.FailReason failureReason;

	public Image background;

	public Color active;

	public Color notActive;

	public TextMeshProUGUI currentResourcesResourceCounts;

	private Dictionary<SIResourceCollection.ResourceCollectorTerminalState, GameObject> screenData;

	private bool initialized;

	[SerializeField]
	private SoundBankPlayer soundBankPlayer;

	[Tooltip("Button colliders to disable while popup screen is shown.")]
	[SerializeField]
	private List<Collider> _nonPopupButtonColliders;

	public enum FailReason
	{
		NotEnoughRocks,
		ResourcesFull,
		Unknown
	}

	public enum ResourceCollectorTerminalState
	{
		WaitingForScan,
		CurrentResources,
		HelpScreen,
		PurchaseRemote,
		PurchaseStart,
		PurchaseInProgress,
		PurchaseSuccess,
		PurchaseFailure
	}
}
