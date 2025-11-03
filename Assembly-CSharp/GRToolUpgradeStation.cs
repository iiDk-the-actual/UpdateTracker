using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GRToolUpgradeStation : MonoBehaviour
{
	public void Init(GRToolProgressionManager tree, GhostReactor reactor)
	{
		this._reactor = reactor;
		this.defaultCostText = this.CostText.text;
		this.toolProgressionManager = tree;
		this.toolProgressionManager.OnProgressionUpdated += this.ResearchTreeUpdated;
		this.ResetScreen();
	}

	public bool canInsertTool
	{
		get
		{
			return this.currentState == GRToolUpgradeStation.UpgradeStationState.Idle && !this.bIsToolInserted;
		}
	}

	public void ResearchTreeUpdated()
	{
		this.UpdateUI();
	}

	public void Update()
	{
		if (this.currentState == GRToolUpgradeStation.UpgradeStationState.Upgrading)
		{
			this.UpgradingUpdate(PhotonNetwork.Time);
		}
	}

	public void ToolInserted(GRTool tool)
	{
		if (!this.canInsertTool)
		{
			return;
		}
		this.bIsToolInserted = true;
		this.insertedTool = tool;
		this.insertedToolType = this.insertedTool.toolType;
		this.selectedToolUpgrades = this.toolProgressionManager.GetToolUpgrades(this.insertedToolType);
		this.ResetScreen();
		this.UpdateUI();
		this.SelectUpgrade(0);
		this.LocalPlacedToolInUpgradeStation(tool.gameEntity.id);
	}

	public void UpdateUI()
	{
		this.UpdateUpgradeTexts();
		this.UpdateSelectedUpgrade();
	}

	public void UpdateUpgradeTexts()
	{
		this.ToolNameText.text = GRUtils.GetToolName(this.insertedToolType);
		for (int i = 0; i < this.UpgradeTitlesText.Length; i++)
		{
			if (this.selectedToolUpgrades.Count > i)
			{
				this.UpgradeTitlesText[i].text = this.selectedToolUpgrades[i].partMetadata.name;
			}
			else
			{
				this.UpgradeTitlesText[i].text = null;
			}
		}
	}

	public void UnlockAllUpgrades()
	{
	}

	public void UpdateSelectedUpgrade()
	{
		if (this.selectedToolUpgrades != null && this.selectedToolUpgrades.Count > this.selectedUpgradeIndex && this.selectedToolUpgrades[this.selectedUpgradeIndex] != null)
		{
			if (this.selectedToolUpgrades[this.selectedUpgradeIndex].unlocked)
			{
				this.DescriptionText.text = this.selectedToolUpgrades[this.selectedUpgradeIndex].partMetadata.description;
				int researchCost = this.selectedToolUpgrades[this.selectedUpgradeIndex].researchCost;
				this.CostText.text = string.Format(this.defaultCostText, researchCost.ToString());
				GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
				this.CostText.color = ((researchCost > grplayer.ShiftCredits) ? this.lockedColor : this.unlockedColor);
				return;
			}
			this.CostText.text = "NEEDS RESEARCH";
			this.CostText.color = this.lockedColor;
		}
	}

	public void ResetScreen()
	{
		this.DescriptionText.text = "PLEASE INSERT A TOOL";
		for (int i = 0; i < this.UpgradeTitlesText.Length; i++)
		{
			this.UpgradeTitlesText[i].text = "----";
			this.UpgradeTitlesText[i].color = this.lockedColor;
			this.MFD_ButtonTexts[i].color = this.unSelectedColor;
		}
		this.ToolNameText.text = "----";
		this.CostText.text = "-";
		this.ToolNameText.color = this.unSelectedColor;
		this.DescriptionText.color = this.unSelectedColor;
		this.CostText.color = this.unSelectedColor;
	}

	public void SelectUpgrade(int index)
	{
		if (index >= this.selectedToolUpgrades.Count)
		{
			return;
		}
		this.selectedUpgradeIndex = index;
		for (int i = 0; i < this.UpgradeTitlesText.Length; i++)
		{
			if (i < this.selectedToolUpgrades.Count)
			{
				bool unlocked = this.selectedToolUpgrades[i].unlocked;
				this.UpgradeTitlesText[i].color = (unlocked ? this.unlockedColor : this.lockedColor);
				this.UpgradeLockedImage[i].gameObject.SetActive(!unlocked);
			}
			else
			{
				this.UpgradeLockedImage[i].gameObject.SetActive(true);
				this.UpgradeTitlesText[i].color = this.lockedColor;
			}
			this.UpgradeButtons[i].isOn = false;
			this.UpgradeButtons[i].UpdateColor();
		}
		if (this.selectedToolUpgrades != null && this.selectedToolUpgrades.Count > this.selectedUpgradeIndex && this.selectedToolUpgrades[this.selectedUpgradeIndex] != null)
		{
			this.UpgradeButtons[this.selectedUpgradeIndex].isOn = true;
			this.UpgradeButtons[this.selectedUpgradeIndex].UpdateColor();
			this.DescriptionText.color = this.UpgradeTitlesText[this.selectedUpgradeIndex].color;
			this.CostText.color = this.UpgradeTitlesText[this.selectedUpgradeIndex].color;
		}
		this.UpdateUI();
	}

	public void UpgradeTool()
	{
		this._reactor.grManager.ToolUpgradeStationRequestUpgrade(this.selectedToolUpgrades[this.selectedUpgradeIndex].type, this.insertedToolEntity.GetNetId());
	}

	public void LocalPlacedToolInUpgradeStation(GameEntityId entityId)
	{
		GameEntity gameEntity = this._reactor.grManager.gameEntityManager.GetGameEntity(entityId);
		this.currentState = GRToolUpgradeStation.UpgradeStationState.ItemInserted;
		if (gameEntity.heldByActorNumber >= 0)
		{
			GamePlayer gamePlayer = GamePlayer.GetGamePlayer(gameEntity.heldByActorNumber);
			int num = gamePlayer.FindHandIndex(entityId);
			gamePlayer.ClearGrabbedIfHeld(entityId);
			if (gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
			{
				GamePlayerLocal.instance.gamePlayer.ClearGrabbed(num);
				GamePlayerLocal.instance.ClearGrabbed(num);
			}
			gameEntity.heldByActorNumber = -1;
			gameEntity.heldByHandIndex = -1;
			Action onReleased = gameEntity.OnReleased;
			if (onReleased != null)
			{
				onReleased();
			}
			this.PositionInsertedTool(gameEntity);
			this.SelectUpgrade(0);
		}
	}

	public void PositionInsertedTool(GameEntity entity)
	{
		this.insertedToolEntity = entity;
		entity.transform.SetParent(this.startingLocation);
		entity.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		Rigidbody component = entity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.position = this.startingLocation.position;
			component.rotation = this.startingLocation.rotation;
			component.linearVelocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
		}
		entity.pickupable = false;
	}

	public void PayForUpgrade(int Player)
	{
		if (Player == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			int researchCost = this.selectedToolUpgrades[this.selectedUpgradeIndex].researchCost;
			GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
			bool flag = researchCost <= grplayer.ShiftCredits;
			bool unlocked = this.selectedToolUpgrades[this.selectedUpgradeIndex].unlocked;
			if (flag && unlocked)
			{
				UnityEvent onSucceeded = this.IDCardScanner.onSucceeded;
				if (onSucceeded != null)
				{
					onSucceeded.Invoke();
				}
				this.StartUpgrade(PhotonNetwork.Time);
			}
		}
	}

	public void StartUpgrade(double startTime)
	{
		if (this.currentState != GRToolUpgradeStation.UpgradeStationState.ItemInserted)
		{
			return;
		}
		this.upgradeStartTime = startTime;
		this.insertedToolEntity.transform.SetParent(this.startingLocation);
		this.insertedToolEntity.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		this.currentState = GRToolUpgradeStation.UpgradeStationState.Upgrading;
	}

	public void UpgradingUpdate(double currentTime)
	{
		if (currentTime >= this.upgradeStartTime + this.upgradeAnimationLength)
		{
			this.CompleteUpgrade();
		}
	}

	public void CompleteUpgrade()
	{
		this.currentState = GRToolUpgradeStation.UpgradeStationState.Complete;
		this.ResetScreen();
		this.MoveToolToFinished();
	}

	public void MoveItemToUpgradeSlot()
	{
		this.insertedToolEntity.transform.SetParent(this.upgradingLocation);
		this.insertedToolEntity.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		Rigidbody component = this.insertedToolEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.position = this.upgradingLocation.position;
			component.rotation = this.upgradingLocation.rotation;
			component.linearVelocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
		}
		this.insertedToolEntity.pickupable = false;
	}

	public void MoveToolToFinished()
	{
		this.insertedToolEntity.transform.SetParent(this.depositedLocation);
		this.insertedToolEntity.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		this.currentState = GRToolUpgradeStation.UpgradeStationState.Complete;
		Rigidbody component = this.insertedToolEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.position = this.startingLocation.position;
			component.rotation = this.startingLocation.rotation;
			component.linearVelocity = this.ejectionTransform.forward * this.ejectionVelocity;
			component.angularVelocity = Vector3.zero;
		}
		this.insertedToolEntity.pickupable = true;
		this.UpgradeTool();
		this.EjectToolFromEnd();
		this.ResetScreen();
	}

	public void EjectToolFromStart()
	{
		this.insertedToolEntity.transform.SetParent(this.startingLocation);
		this.insertedToolEntity.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		this.insertedToolEntity.transform.SetParent(null, true);
		Rigidbody component = this.insertedToolEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.position = this.startingLocation.position;
			component.rotation = this.startingLocation.rotation;
			component.linearVelocity = this.ejectionTransform.forward * this.ejectionVelocity;
			component.angularVelocity = Vector3.zero;
		}
		this.insertedToolEntity.pickupable = true;
		this.insertedToolEntity = null;
		this.insertedTool = null;
		this.insertedToolType = GRTool.GRToolType.None;
		this.bIsToolInserted = false;
		this.ResetScreen();
		this.currentState = GRToolUpgradeStation.UpgradeStationState.Idle;
	}

	public void EjectToolFromEnd()
	{
		this.insertedToolEntity.transform.SetParent(this.depositedLocation);
		this.insertedToolEntity.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		this.insertedToolEntity.transform.SetParent(null, true);
		Rigidbody component = this.insertedToolEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.position = this.depositedLocation.position;
			component.rotation = this.depositedLocation.rotation;
			component.linearVelocity = this.ejectionTransform.forward * this.ejectionVelocity;
			component.angularVelocity = Vector3.zero;
		}
		this.insertedToolEntity.pickupable = true;
		this.insertedToolEntity = null;
		this.insertedTool = null;
		this.insertedToolType = GRTool.GRToolType.None;
		this.bIsToolInserted = false;
		this.currentState = GRToolUpgradeStation.UpgradeStationState.Idle;
	}

	private GRTool insertedTool;

	private GRTool.GRToolType insertedToolType;

	private GameEntity insertedToolEntity;

	[NonSerialized]
	private GhostReactor _reactor;

	[NonSerialized]
	private GRToolProgressionManager toolProgressionManager;

	[NonSerialized]
	private List<GRToolProgressionTree.GRToolProgressionNode> selectedToolUpgrades = new List<GRToolProgressionTree.GRToolProgressionNode>();

	[NonSerialized]
	public bool bIsToolInserted;

	public Transform startingLocation;

	public Transform upgradingLocation;

	public Transform depositedLocation;

	public Transform ejectionTransform;

	public float ejectionVelocity;

	public Color selectedColor;

	public Color unSelectedColor;

	public Color lockedColor;

	public Color unlockedColor;

	public TMP_Text[] UpgradeTitlesText;

	public TMP_Text[] MFD_ButtonTexts;

	public GorillaPressableButton[] UpgradeButtons;

	public Image[] UpgradeLockedImage;

	public TMP_Text ToolNameText;

	public TMP_Text DescriptionText;

	public TMP_Text CostText;

	private string defaultCostText;

	public IDCardScanner IDCardScanner;

	private int selectedUpgradeIndex;

	private double upgradeStartTime;

	public double upgradeAnimationLength;

	public Vector3 rotationAnimation;

	private GRToolUpgradeStation.UpgradeStationState currentState;

	public GameEntity attachedItem;

	private enum UpgradeStationState
	{
		Idle,
		ItemInserted,
		Upgrading,
		Complete
	}
}
