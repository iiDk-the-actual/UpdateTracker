using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class GRToolUpgradePurchaseStationFull : MonoBehaviour, ITickSystemTick
{
	public int SelectedShelf
	{
		get
		{
			return this.selectedShelf;
		}
	}

	public int SelectedItem
	{
		get
		{
			return this.selectedItem;
		}
	}

	public bool TickRunning { get; set; }

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public void Init(GRToolProgressionManager progression, GhostReactor reactor)
	{
		this.reactor = reactor;
		this.grManager = reactor.grManager;
		this.toolProgressionManager = progression;
		this.toolProgressionManager.OnProgressionUpdated += this.ProgressionUpdated;
		this.nextVisibleShelfIndex = -1;
		this.prefabMagnetHeightOffset = this.ropeTop.position.y;
		this.frontBackShelfMovement = new GRSpringMovement(0.5f, 0.7f);
		this.raiseLowerShelfMovement = new GRSpringMovement(1f, 0.7f);
		this.magnetMovement = new GRSpringMovement(1f, 0.7f);
		ProgressionManager.Instance.OnGetShiftCredit += this.OnShiftCreditChanged;
		this.needsUIRefresh = true;
		this.InitPageSelectionWheel();
		this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle);
		this.SetActivePlayer(-1);
	}

	public void OnShiftCreditChanged(string targetMothershipId, int newShiftCredits)
	{
		this.needsUIRefresh = true;
	}

	public void HideOrShowTextBasedOnLocalPlayerDistance()
	{
		Vector3 position = GRPlayer.Get(VRRig.LocalRig).transform.position;
		Vector3 position2 = base.transform.position;
		float num = (this.currentlyShowingText ? 8f : 6f);
		bool flag = (position - position2).sqrMagnitude < num * num;
		if (flag != this.currentlyShowingText)
		{
			this.shelfSelectionText.enabled = flag;
			this.playerInfo.enabled = flag;
			this.itemDescription.enabled = flag;
			this.itemDescriptionName.enabled = flag;
			this.itemDescriptionAnnotation.enabled = flag;
			this.purchaseButtonText.enabled = flag;
			this.pageSelectionWheel.ShowText(flag);
			for (int i = 0; i < this.gameShelves.Count; i++)
			{
				if (!(this.gameShelves[i] == null))
				{
					foreach (GRToolUpgradePurchaseStationShelf.GRPurchaseSlot grpurchaseSlot in this.gameShelves[i].gRPurchaseSlots)
					{
						if (grpurchaseSlot.Name != null)
						{
							grpurchaseSlot.Name.enabled = flag;
						}
						if (grpurchaseSlot.Price != null)
						{
							grpurchaseSlot.Price.enabled = flag;
						}
					}
				}
			}
		}
		this.currentlyShowingText = flag;
	}

	public void Tick()
	{
		this.HideOrShowTextBasedOnLocalPlayerDistance();
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (this.toolProgressionManager == null)
		{
			return;
		}
		if (grplayer != null && (this.lastKnownLocalPlayerCredits != grplayer.ShiftCredits || this.lastKnownLocalPlayerJuice != this.toolProgressionManager.GetNumberOfResearchPoints()))
		{
			this.needsUIRefresh = true;
			this.lastKnownLocalPlayerCredits = grplayer.ShiftCredits;
			this.lastKnownLocalPlayerJuice = this.toolProgressionManager.GetNumberOfResearchPoints();
		}
		this.UpdateActivePlayer();
		this.UpdateSelectionLever();
		this.UpdateShelf();
		this.UpdateMagnet();
		if (this.disablePurchaseButton)
		{
			if (this.purchaseButtonPressed > 0f)
			{
				this.purchaseButtonPressed -= Time.deltaTime;
			}
			else
			{
				this.disablePurchaseButton = false;
			}
		}
		if (this.needsUIRefresh)
		{
			this.needsUIRefresh = false;
			this.UpdateShelfDisplayElements(this.currentVisibleShelfIndex);
			this.UpdateShelfDisplayElements(this.nextVisibleShelfIndex);
			this.UpdateShelfDisplayElements(this.selectedShelf);
			this.UpdatePlayerCurrencyUI();
			this.UpdatePurchaseButtonText();
		}
	}

	public void SetActivePlayer(int actorNum)
	{
		this.currentActivePlayerActorNumber = actorNum;
		this.needsUIRefresh = true;
		if (this.currentActivePlayerActorNumber == -1)
		{
			this.itemDescriptionName.text = "SWIPE FOR ACCESS";
			this.itemDescription.text = "Welcome to the Tool-o-matic v2 automated vending machine. Please swipe your ID card for access.";
			this.itemDescriptionAnnotation.text = "Remember: Compliance leads to success!";
			return;
		}
		if (this.IsValidShelfItemIndex(this.selectedShelf, this.selectedItem) && this.toolProgressionManager != null)
		{
			GRToolProgressionManager.ToolProgressionMetaData partMetadata = this.toolProgressionManager.GetPartMetadata(this.gameShelves[this.selectedShelf].gRPurchaseSlots[this.selectedItem].PurchaseID);
			if (partMetadata != null)
			{
				this.itemDescriptionName.text = partMetadata.name;
				this.itemDescription.text = partMetadata.description;
				this.itemDescriptionAnnotation.text = partMetadata.annotation;
			}
			this.select1.SetButtonState(this.selectedItem == 0);
			this.select2.SetButtonState(this.selectedItem == 1);
			this.select3.SetButtonState(this.selectedItem == 2);
			this.select4.SetButtonState(this.selectedItem == 3);
		}
	}

	public void UpdateActivePlayer()
	{
		if (!this.grManager.IsAuthority())
		{
			return;
		}
		if (this.currentActivePlayerActorNumber != -1)
		{
			GRPlayer grplayer = GRPlayer.Get(this.currentActivePlayerActorNumber);
			if (grplayer != null)
			{
				BoxCollider component = base.GetComponent<BoxCollider>();
				Vector3 position = grplayer.transform.position;
				Vector3 vector = component.transform.worldToLocalMatrix.MultiplyPoint(position) - component.center;
				Vector3 vector2 = component.size * 0.5f;
				if (Mathf.Abs(vector.x) > vector2.x || Mathf.Abs(vector.y) > vector2.y || Mathf.Abs(vector.z) > vector2.z)
				{
					this.grManager.SetActivePlayerAuthority(this, -1);
					return;
				}
			}
			else
			{
				this.currentActivePlayerActorNumber = -1;
			}
		}
	}

	private void UpdateShelf()
	{
		switch (this.shelfMovementState)
		{
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle:
			if (this.currentVisibleShelfIndex != this.selectedShelf)
			{
				this.SetNextShelf(this.selectedShelf);
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfBackward);
				return;
			}
			this.SetNextShelf(-1);
			return;
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfBackward:
		{
			if (this.currentVisibleShelfIndex == this.selectedShelf)
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfForward);
				return;
			}
			this.frontBackShelfMovement.target = 1f;
			this.frontBackShelfMovement.Update();
			float pos = this.frontBackShelfMovement.pos;
			this.gameShelves[this.currentVisibleShelfIndex].transform.position = Vector3.Lerp(this.shelfRootTransform.position, this.shelfBackTransform.position, pos);
			this.UpdateSoundsForMovement(this.frontBackShelfMovement);
			if (this.frontBackShelfMovement.IsAtTarget())
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfUpward);
				return;
			}
			break;
		}
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfForward:
		{
			if (this.currentVisibleShelfIndex != this.selectedShelf)
			{
				this.SetNextShelf(this.selectedShelf);
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfBackward);
				return;
			}
			this.frontBackShelfMovement.target = 0f;
			this.frontBackShelfMovement.Update();
			float pos2 = this.frontBackShelfMovement.pos;
			this.gameShelves[this.currentVisibleShelfIndex].transform.position = Vector3.Lerp(this.shelfRootTransform.position, this.shelfBackTransform.position, pos2);
			this.UpdateSoundsForMovement(this.frontBackShelfMovement);
			if (this.frontBackShelfMovement.IsAtTarget())
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle);
				return;
			}
			break;
		}
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfUpward:
		{
			if (this.nextVisibleShelfIndex == -1)
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle);
				return;
			}
			if (this.nextVisibleShelfIndex != this.selectedShelf && this.raiseLowerShelfMovement.pos <= 0.5f)
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfDownward);
				return;
			}
			this.raiseLowerShelfMovement.target = 1f;
			this.raiseLowerShelfMovement.Update();
			float pos3 = this.raiseLowerShelfMovement.pos;
			this.gameShelves[this.nextVisibleShelfIndex].transform.position = Vector3.Lerp(this.shelfLowerTransform.position, this.shelfRootTransform.position, pos3);
			this.UpdateSoundsForMovement(this.raiseLowerShelfMovement);
			if (this.raiseLowerShelfMovement.IsAtTarget())
			{
				this.SetCurrentShelf(this.nextVisibleShelfIndex);
				if (this.nextVisibleShelfIndex == this.selectedShelf)
				{
					this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle);
					return;
				}
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfBackward);
				return;
			}
			break;
		}
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfDownward:
			if (this.nextVisibleShelfIndex == -1)
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle);
				return;
			}
			if (this.nextVisibleShelfIndex != this.selectedShelf)
			{
				this.raiseLowerShelfMovement.target = 0f;
				this.raiseLowerShelfMovement.Update();
				float pos4 = this.raiseLowerShelfMovement.pos;
				this.gameShelves[this.nextVisibleShelfIndex].transform.position = Vector3.Lerp(this.shelfLowerTransform.position, this.shelfRootTransform.position, pos4);
				this.UpdateSoundsForMovement(this.raiseLowerShelfMovement);
				if (this.raiseLowerShelfMovement.IsAtTarget())
				{
					this.SetNextShelf(this.selectedShelf);
					this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfUpward);
					return;
				}
			}
			else
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfUpward);
			}
			break;
		default:
			return;
		}
	}

	private void UpdateSoundsForMovement(GRSpringMovement movement)
	{
		if (movement.IsAtTarget())
		{
			this.audioSourceLooping.volume = 0f;
			if (movement.HitTargetLastUpdate())
			{
				this.audioSourceClang.Play();
				return;
			}
		}
		else
		{
			this.audioSourceLooping.volume = Mathf.Clamp01(Math.Abs(movement.speed) * this.audioSourceLoopingVolume);
		}
	}

	public void SetCurrentShelf(int idx)
	{
		if (idx == -1)
		{
			return;
		}
		if (idx == this.currentVisibleShelfIndex)
		{
			return;
		}
		if (!this.IsValidShelfItemIndex(idx, 0))
		{
			return;
		}
		if (idx == this.nextVisibleShelfIndex)
		{
			this.SetNextShelf(-1);
		}
		this.UpdateShelfVisibility(this.currentVisibleShelfIndex, false);
		this.frontBackShelfMovement.Reset();
		this.gameShelves[idx].transform.position = this.shelfRootTransform.position;
		this.UpdateShelfVisibility(idx, true);
		this.currentVisibleShelfIndex = idx;
	}

	public void SetNextShelf(int idx)
	{
		if (idx == this.nextVisibleShelfIndex)
		{
			return;
		}
		if (idx == this.currentVisibleShelfIndex)
		{
			return;
		}
		if (this.nextVisibleShelfIndex != -1)
		{
			this.UpdateShelfVisibility(this.nextVisibleShelfIndex, false);
		}
		if (idx != -1)
		{
			this.raiseLowerShelfMovement.Reset();
			this.gameShelves[idx].transform.position = this.shelfLowerTransform.position;
			this.UpdateShelfVisibility(idx, true);
		}
		this.nextVisibleShelfIndex = idx;
	}

	public void ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState newState)
	{
		this.shelfMovementState = newState;
		switch (newState)
		{
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle:
			this.SetCurrentShelf(this.selectedShelf);
			this.SetNextShelf(-1);
			return;
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfBackward:
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfForward:
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfDownward:
			this.audioSourceLooping.volume = 0f;
			this.audioSourceLooping.GTPlay();
			return;
		case GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveNextShelfUpward:
			if (this.currentVisibleShelfIndex == this.selectedShelf)
			{
				this.ChangeShelfMovementState(GRToolUpgradePurchaseStationFull.ShelfMovementState.MoveCurrentShelfForward);
			}
			else
			{
				this.SetNextShelf(this.selectedShelf);
			}
			this.audioSourceLooping.volume = 0f;
			this.audioSourceLooping.GTPlay();
			return;
		default:
			return;
		}
	}

	public void UpdateShelfVisibility(int shelfID, bool isVisible)
	{
		if (!this.IsValidShelfItemIndex(shelfID, 0))
		{
			return;
		}
		this.gameShelves[shelfID].gameObject.SetActive(isVisible);
		if (isVisible)
		{
			this.UpdateShelfDisplayElements(shelfID);
		}
	}

	public void UpdateShelfDisplayElements(int shelfID)
	{
		if (!this.IsValidShelfItemIndex(shelfID, 0))
		{
			return;
		}
		GRToolUpgradePurchaseStationShelf grtoolUpgradePurchaseStationShelf = this.gameShelves[shelfID];
		for (int i = 0; i < grtoolUpgradePurchaseStationShelf.gRPurchaseSlots.Count; i++)
		{
			this.UpdateShelfItemDisplayElements(shelfID, i);
		}
	}

	public void UpdatePurchaseButtonText()
	{
		if (!this.IsValidShelfItemIndex(this.selectedShelf, this.selectedItem))
		{
			this.purchaseButtonText.text = "ERROR";
			return;
		}
		GRToolUpgradePurchaseStationShelf.GRPurchaseSlot grpurchaseSlot = this.gameShelves[this.selectedShelf].gRPurchaseSlots[this.selectedItem];
		Color color = (grpurchaseSlot.canAfford ? this.colorPurchaseButtonCanAfford : this.colorCantBuy);
		string purchaseText = grpurchaseSlot.purchaseText;
		if (color != this.purchaseButtonText.color)
		{
			this.purchaseButtonText.color = color;
		}
		if (purchaseText != this.purchaseButtonText.text)
		{
			this.purchaseButtonText.text = purchaseText;
		}
	}

	public void UpdateShelfItemDisplayElements(int shelf, int slotID)
	{
		if (!this.IsValidShelfItemIndex(shelf, slotID))
		{
			return;
		}
		GRToolUpgradePurchaseStationShelf.GRPurchaseSlot grpurchaseSlot = this.gameShelves[shelf].gRPurchaseSlots[slotID];
		if (this.toolProgressionManager)
		{
			GRToolProgressionManager.ToolProgressionMetaData partMetadata = this.toolProgressionManager.GetPartMetadata(grpurchaseSlot.PurchaseID);
			if (partMetadata == null)
			{
				grpurchaseSlot.Name.text = "ERROR";
				return;
			}
			string text = "ERROR";
			string text2 = "";
			Color white = Color.white;
			bool flag = true;
			int num = 10000;
			int num2;
			this.toolProgressionManager.GetPlayerShiftCredit(out num2);
			int numberOfResearchPoints = this.toolProgressionManager.GetNumberOfResearchPoints();
			grpurchaseSlot.canAfford = false;
			grpurchaseSlot.purchaseText = "LOCKED";
			bool flag2;
			if (this.toolProgressionManager.IsPartUnlocked(grpurchaseSlot.PurchaseID, out flag2))
			{
				if (flag2)
				{
					this.gameShelves[shelf].SetMaterialOverride(slotID, null);
					if (this.toolProgressionManager.GetShiftCreditCost(grpurchaseSlot.PurchaseID, out num))
					{
						text = string.Format("⑭ {0}", num);
					}
					bool flag3 = num2 >= num;
					grpurchaseSlot.Name.text = partMetadata.name;
					grpurchaseSlot.Name.color = ((slotID == this.selectedItem) ? this.colorSelectedItem : this.colorUnselectedItem);
					grpurchaseSlot.Price.text = text;
					grpurchaseSlot.Price.color = (flag3 ? this.colorCanBuyCredits : this.colorCantBuy);
					grpurchaseSlot.Price.fontSize = ((text.Length <= 8) ? 2.25f : 1.6f);
					grpurchaseSlot.canAfford = flag3;
					if (flag3)
					{
						grpurchaseSlot.purchaseText = string.Format("BUY FOR\n⑭ {0}", num);
					}
					else
					{
						grpurchaseSlot.purchaseText = string.Format("NEED\n⑭ {0}", num);
					}
				}
				else
				{
					this.gameShelves[shelf].SetMaterialOverride(slotID, this.unresearchedItemMaterial);
					grpurchaseSlot.Name.text = partMetadata.name;
					grpurchaseSlot.Name.color = ((slotID == this.selectedItem) ? this.colorUnresearchedItem : this.colorUnselectedUnresearchedItem);
					flag = true;
					GRToolProgressionTree.EmployeeLevelRequirement employeeLevelRequirement;
					if (this.toolProgressionManager.GetPartUnlockEmployeeRequiredLevel(grpurchaseSlot.PurchaseID, out employeeLevelRequirement) && this.toolProgressionManager.GetCurrentEmployeeLevel() < employeeLevelRequirement)
					{
						this.toolProgressionManager.GetEmployeeLevelDisplayName(employeeLevelRequirement);
						text2 += string.Format("⑱ {0}\n", employeeLevelRequirement);
						flag = false;
					}
					this.cachedRequiredPartsList.Clear();
					if (this.toolProgressionManager.GetPartUnlockRequiredParentParts(grpurchaseSlot.PurchaseID, out this.cachedRequiredPartsList))
					{
						foreach (GRToolProgressionManager.ToolParts toolParts in this.cachedRequiredPartsList)
						{
							bool flag4 = false;
							GRToolProgressionManager.ToolProgressionMetaData partMetadata2 = this.toolProgressionManager.GetPartMetadata(toolParts);
							if (partMetadata2 == null)
							{
								text2 += "⑱ ERROR\n";
								flag = false;
							}
							else if (!this.toolProgressionManager.IsPartUnlocked(toolParts, out flag4) || !flag4)
							{
								text2 = text2 + "⑱ " + partMetadata2.name + "\n";
								flag = false;
							}
						}
					}
					if (!flag)
					{
						grpurchaseSlot.Price.text = text2;
						grpurchaseSlot.Price.color = this.colorCantBuy;
						grpurchaseSlot.Price.fontSize = ((text2.Length <= 8) ? 2.25f : 1.6f);
						grpurchaseSlot.canAfford = false;
						grpurchaseSlot.purchaseText = "LOCKED";
					}
					else
					{
						if (this.toolProgressionManager.GetPartUnlockJuiceCost(grpurchaseSlot.PurchaseID, out num))
						{
							text = string.Format("⑮ {0}", num);
						}
						bool flag3 = numberOfResearchPoints >= num;
						grpurchaseSlot.Price.text = text;
						grpurchaseSlot.Price.color = (flag3 ? this.colorCanBuyJuice : this.colorCantBuy);
						grpurchaseSlot.Price.fontSize = ((text.Length <= 8) ? 2.25f : 1.6f);
						grpurchaseSlot.canAfford = flag3;
						if (flag3)
						{
							grpurchaseSlot.purchaseText = string.Format("RESEARCH\n⑮ {0}", num);
						}
						else
						{
							grpurchaseSlot.purchaseText = string.Format("NEED\n⑮ {0}", num);
						}
					}
				}
			}
			if (slotID != this.selectedItem)
			{
				this.gameShelves[shelf].SetBacklightStateAndMaterial(slotID, false, this.backlightLocked);
				return;
			}
			if (grpurchaseSlot.Price.color == this.colorCanBuyJuice)
			{
				this.gameShelves[shelf].SetBacklightStateAndMaterial(slotID, true, this.backlightResearch);
				return;
			}
			if (grpurchaseSlot.Price.color == this.colorCanBuyCredits)
			{
				this.gameShelves[shelf].SetBacklightStateAndMaterial(slotID, true, this.backlightPurchase);
				return;
			}
			this.gameShelves[shelf].SetBacklightStateAndMaterial(slotID, true, this.backlightLocked);
		}
	}

	public void UpdatePlayerCurrencyUI()
	{
		if (this.currentActivePlayerActorNumber == -1)
		{
			this.playerInfo.text = "AVAILABLE";
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		GRPlayer grplayer2 = GRPlayer.Get(this.currentActivePlayerActorNumber);
		if (grplayer2 == null)
		{
			this.currentActivePlayerActorNumber = -1;
			this.playerInfo.text = "AVAILABLE";
			return;
		}
		string text2;
		if (grplayer2 == grplayer)
		{
			int shiftCredits = grplayer2.ShiftCredits;
			int numberOfResearchPoints = this.toolProgressionManager.GetNumberOfResearchPoints();
			NetPlayer player = NetworkSystem.Instance.GetPlayer(this.currentActivePlayerActorNumber);
			string text = ((player != null) ? player.SanitizedNickName : "RANDO MONKE");
			string employeeLevelDisplayName = this.toolProgressionManager.GetEmployeeLevelDisplayName(this.toolProgressionManager.GetCurrentEmployeeLevel());
			text2 = string.Format("<color=#c0c0c0>{0}\n{1}</color>\n\n<color=purple><size=2>⑮ {2}</size></color>\n<color=white><size=2>⑭ {3}</size></color>\n", new object[] { text, employeeLevelDisplayName, numberOfResearchPoints, shiftCredits });
		}
		else
		{
			NetPlayer player2 = NetworkSystem.Instance.GetPlayer(this.currentActivePlayerActorNumber);
			text2 = ((player2 != null) ? player2.SanitizedNickName : "RANDO MONKE") ?? "";
		}
		this.playerInfo.text = text2;
	}

	public bool CanLocalPlayerPurchaseItem(int shelf, int slotID)
	{
		if (!this.IsValidShelfItemIndex(shelf, slotID))
		{
			return false;
		}
		if (this.grManager && this.grManager.DebugIsToolStationHacked())
		{
			return true;
		}
		this.UpdateShelfItemDisplayElements(shelf, slotID);
		return this.gameShelves[shelf].gRPurchaseSlots[slotID].canAfford;
	}

	public bool CheckActivePlayer()
	{
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		if (this.currentActivePlayerActorNumber == -1)
		{
			this.RequestActivePlayerToken();
			return false;
		}
		GRPlayer grplayer2 = GRPlayer.Get(this.currentActivePlayerActorNumber);
		if (grplayer2 == null)
		{
			this.currentActivePlayerActorNumber = -1;
		}
		return !(grplayer2 != grplayer);
	}

	public void SelectOption1()
	{
		this.OnLocalSelectionButtonPressed(0);
	}

	public void SelectOption2()
	{
		this.OnLocalSelectionButtonPressed(1);
	}

	public void SelectOption3()
	{
		this.OnLocalSelectionButtonPressed(2);
	}

	public void SelectOption4()
	{
		this.OnLocalSelectionButtonPressed(3);
	}

	public void OnLocalSelectionButtonPressed(int index)
	{
		if (!this.CheckActivePlayer())
		{
			if (index == 0 && this.selectedItem != 0)
			{
				this.select1.SetButtonState(false);
			}
			if (index == 1 && this.selectedItem != 1)
			{
				this.select2.SetButtonState(false);
			}
			if (index == 2 && this.selectedItem != 2)
			{
				this.select3.SetButtonState(false);
			}
			if (index == 3 && this.selectedItem != 3)
			{
				this.select4.SetButtonState(false);
			}
			return;
		}
		if (index != 0)
		{
			this.select1.SetButtonState(false);
		}
		if (index != 1)
		{
			this.select2.SetButtonState(false);
		}
		if (index != 2)
		{
			this.select3.SetButtonState(false);
		}
		if (index != 3)
		{
			this.select4.SetButtonState(false);
		}
		if (this.shelfMovementState == GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle)
		{
			this.SetSelectedShelfAndItem(this.selectedShelf, index, false);
		}
	}

	public void SelectPageDown()
	{
		this.OnLocalSelectionPageChange(1);
	}

	public void SelectPageUp()
	{
		this.OnLocalSelectionPageChange(-1);
	}

	public void OnLocalSelectionPageChange(int delta)
	{
		if (!this.CheckActivePlayer())
		{
			return;
		}
		this.pageSelectionWheel.SetTargetShelf((this.pageSelectionWheel.targetPage + delta + this.gameShelves.Count) % this.gameShelves.Count);
	}

	public void CardSwiped()
	{
		this.RequestActivePlayerToken();
	}

	public void PurchaseButtonPressed()
	{
		if (this.disablePurchaseButton)
		{
			return;
		}
		this.purchaseButtonPressed = this.purchaseButtonCooldown;
		this.disablePurchaseButton = true;
		if (!this.CheckActivePlayer())
		{
			return;
		}
		if (this.shelfMovementState == GRToolUpgradePurchaseStationFull.ShelfMovementState.Idle && this.desiredMagnetEntityTypeId == this.currentMagnetEntityTypeId)
		{
			this.RequestPurchaseItem(this.selectedShelf, this.selectedItem);
		}
	}

	public void DEBUGSetHackToolStation()
	{
	}

	public void RequestActivePlayerToken()
	{
		if (this.lastRequestedActivePlayerTokenTime > Time.time || this.lastRequestedActivePlayerTokenTime + this.requestActivePlayerTokenThrottleTime < Time.time)
		{
			this.lastRequestedActivePlayerTokenTime = Time.time;
			this.grManager.RequestStationExclusivity(this);
		}
	}

	private void UpdateMagnet()
	{
		if (this.desiredMagnetEntityTypeId != this.currentMagnetEntityTypeId || this.currentMagnetEntityTypeId == -1 || this.currentMagnetEntity == null)
		{
			this.magnetMovement.SetHardStopAtTarget(true);
			this.magnetMovement.target = 0f;
			this.magnetMovement.Update();
			Vector3 position = this.ropeTop.transform.position;
			position.y = Mathf.Lerp(this.prefabMagnetHeightOffset, this.prefabMagnetHeightOffset - this.maxMagnetDistance, this.magnetMovement.pos);
			if (position.y != this.ropeTop.transform.position.y)
			{
				this.ropeTop.transform.position = position;
			}
			if (this.magnetMovement.IsAtTarget() && this.grManager.IsAuthority() && this.grManager.IsZoneActive())
			{
				if (this.currentMagnetEntity != null)
				{
					this.currentMagnetEntity.transform.parent = null;
					this.currentMagnetEntity.gameObject.SetActive(false);
					this.grManager.gameEntityManager.RequestDestroyItem(this.currentMagnetEntity.id);
					this.currentMagnetEntity = null;
					this.currentMagnetEntityTypeId = -1;
				}
				if (this.desiredMagnetEntityTypeId != -1)
				{
					GhostReactor.ToolEntityCreateData toolEntityCreateData = default(GhostReactor.ToolEntityCreateData);
					toolEntityCreateData.decayTime = 0f;
					toolEntityCreateData.stationIndex = this.grManager.GetIndexForToolUpgradeStationFull(this);
					this.grManager.gameEntityManager.RequestCreateItem(this.desiredMagnetEntityTypeId, this.ropeEnd.position, this.ropeEnd.rotation, toolEntityCreateData.Pack());
					this.currentMagnetEntityTypeId = this.desiredMagnetEntityTypeId;
					return;
				}
			}
		}
		else if (this.desiredMagnetEntityTypeId == this.currentMagnetEntityTypeId && this.currentMagnetEntity != null)
		{
			this.magnetMovement.SetHardStopAtTarget(false);
			this.magnetMovement.target = 1f;
			this.magnetMovement.Update();
			Vector3 position2 = this.ropeTop.transform.position;
			position2.y = Mathf.Lerp(this.prefabMagnetHeightOffset, this.prefabMagnetHeightOffset - this.maxMagnetDistance, this.magnetMovement.pos);
			if (this.ropeTop.transform.position.y != position2.y)
			{
				this.ropeTop.transform.position = position2;
			}
		}
	}

	public void InitLinkedEntity(GameEntity entity)
	{
		if (this.currentMagnetEntity != null)
		{
			this.currentMagnetEntity.gameObject.SetActive(false);
		}
		entity.pickupable = false;
		Rigidbody component = entity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		GRToolUpgradePurchaseStationMagnetPoint component2 = entity.GetComponent<GRToolUpgradePurchaseStationMagnetPoint>();
		GameDockable component3 = entity.GetComponent<GameDockable>();
		Transform transform = ((component2 != null) ? component2.magnetAttachTransform : ((component3 != null) ? component3.dockablePoint : entity.transform));
		GRToolUpgradePurchaseStationFull.AttachEntityToMagnet_DockGoesToLocation(this.magnet, entity.transform, transform, new Vector3(0f, -0.03f, 0f));
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		for (int i = 0; i < this.gameShelves.Count; i++)
		{
			for (int j = 0; j < this.gameShelves[i].gRPurchaseSlots.Count; j++)
			{
				GRToolUpgradePurchaseStationShelf.GRPurchaseSlot grpurchaseSlot = this.gameShelves[i].gRPurchaseSlots[j];
				if (grpurchaseSlot != null && !(grpurchaseSlot.ToolEntityPrefab == null) && grpurchaseSlot.ToolEntityPrefab.name != null && grpurchaseSlot.ToolEntityPrefab.name.GetStaticHash() == entity.typeId)
				{
					num = grpurchaseSlot.RopeYaw;
					num2 = grpurchaseSlot.RopePitch;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		Quaternion quaternion = Quaternion.Euler(0f, 0f, 180f);
		quaternion = Quaternion.AngleAxis(num, Vector3.up) * quaternion;
		quaternion = Quaternion.AngleAxis(num2, Vector3.forward) * quaternion;
		this.magnet.localRotation = quaternion;
		this.magnet.localPosition = quaternion * new Vector3(0f, 0.055f, 0f);
		this.currentMagnetEntity = entity;
		this.currentMagnetEntityTypeId = entity.typeId;
	}

	public void UpdateSelectionLever()
	{
		GRPlayer grplayer = GRPlayer.Get(VRRig.LocalRig);
		GRPlayer grplayer2 = GRPlayer.Get(this.currentActivePlayerActorNumber);
		bool flag = ControllerInputPoller.GripFloat(XRNode.LeftHand) > 0.7f;
		bool flag2 = ControllerInputPoller.GripFloat(XRNode.RightHand) > 0.7f;
		VRRig offlineVRRig = GorillaTagger.Instance.offlineVRRig;
		Transform handTransform = GamePlayer.GetHandTransform(offlineVRRig, 0);
		Transform handTransform2 = GamePlayer.GetHandTransform(offlineVRRig, 1);
		Vector3 position = this.pageSelectionHandle.transform.position;
		Vector3 vector = handTransform.position - position;
		Vector3 vector2 = handTransform2.position - position;
		float num = this.pageSelectionLever.transform.localRotation.eulerAngles.x;
		float num2 = 0.2f;
		float num3 = (this.bIsGrippingLeft ? 0.15f : 0.1f);
		float num4 = (this.bIsGrippingRight ? 0.15f : 0.1f);
		if (vector.sqrMagnitude > num3 * num3)
		{
			flag = false;
		}
		if (vector2.sqrMagnitude > num4 * num4)
		{
			flag2 = false;
		}
		if (!this.bGripLeftLastFrame && flag)
		{
			this.bIsGrippingLeft = true;
		}
		else if (this.bGripLeftLastFrame && flag)
		{
			Vector3 forward = this.pageSelectionHandle.transform.forward;
			float num5 = Vector3.Dot(vector, forward);
			num += num5 / num2 * 180f / 3.1415925f;
		}
		else
		{
			this.bIsGrippingLeft = false;
		}
		if (!this.bGripRightLastFrame && flag2)
		{
			this.bIsGrippingRight = true;
		}
		else if (this.bGripRightLastFrame && flag2)
		{
			Vector3 forward2 = this.pageSelectionHandle.transform.forward;
			float num6 = Vector3.Dot(vector2, forward2);
			num += num6 / num2 * 180f / 3.1415925f;
		}
		else
		{
			this.bIsGrippingRight = false;
		}
		if (!this.bIsGrippingLeft && !this.bIsGrippingRight && grplayer == grplayer2)
		{
			num = 30f + (num - 30f) * Mathf.Exp(-20f * Time.deltaTime);
		}
		num = Mathf.Clamp(num, 0f, 60f);
		if ((grplayer == grplayer2 || this.currentActivePlayerActorNumber == -1) && this.lastHandleAngle != num)
		{
			this.pageSelectionLever.transform.localRotation = Quaternion.Euler(num, 0f, 0f);
			this.lastHandleAngle = num;
		}
		float num7 = 0f;
		if (this.bIsGrippingLeft || this.bIsGrippingRight)
		{
			num7 = (num - 30f) / 30f;
		}
		this.bGripLeftLastFrame = flag;
		this.bGripRightLastFrame = flag2;
		if (grplayer == grplayer2)
		{
			this.pageSelectionWheel.isBeingDrivenRemotely = false;
			this.pageSelectionWheel.SetRotationSpeed(num7);
			if (this.pageSelectionWheel.targetPage != this.selectedShelf)
			{
				this.SetSelectedShelfAndItem(this.pageSelectionWheel.targetPage, 0, false);
			}
			float num8 = 0.25f;
			this.timeSinceLastHandleBroadcast += Time.deltaTime;
			if (this.timeSinceLastHandleBroadcast > num8 && (Math.Abs(num - this.angleOfLastHandleBroadcast) > 0.02f || Math.Abs(this.pageSelectionWheel.currentAngle - this.selectionWheelAngleOfLastBroadcast) > 0.02f))
			{
				this.timeSinceLastHandleBroadcast = 0f;
				this.angleOfLastHandleBroadcast = num;
				this.selectionWheelAngleOfLastBroadcast = this.pageSelectionWheel.currentAngle;
				this.grManager.BroadcastHandleAndSelectionWheelPosition(this, (int)(num * this.quantMult), (int)(this.selectionWheelAngleOfLastBroadcast * this.quantMult));
				return;
			}
		}
		else if (this.bIsGrippingLeft || this.bIsGrippingRight)
		{
			this.CheckActivePlayer();
		}
	}

	public static void AttachEntityToMagnet_DockGoesToLocation(Transform magnet, Transform entity, Transform dock, Vector3 magnetDockOffset)
	{
		if (magnet == null || entity == null || dock == null)
		{
			return;
		}
		if (!dock.IsChildOf(entity))
		{
			return;
		}
		Matrix4x4 matrix4x = entity.worldToLocalMatrix * dock.localToWorldMatrix;
		Vector3 vector = GRToolUpgradePurchaseStationFull.ExtractLossyScale(matrix4x);
		Vector3 vector2;
		Quaternion quaternion;
		Vector3 vector3;
		GRToolUpgradePurchaseStationFull.DecomposeTRS(Matrix4x4.TRS(magnetDockOffset, Quaternion.identity, vector) * matrix4x.inverse, out vector2, out quaternion, out vector3);
		entity.SetParent(magnet, false);
		entity.localPosition = vector2;
		entity.localRotation = quaternion;
		entity.localScale = vector3;
	}

	public void SetHandleAndSelectionWheelPositionRemote(int handlePos, int wheelPos)
	{
		this.pageSelectionWheel.isBeingDrivenRemotely = true;
		float num = (float)handlePos / this.quantMult;
		num = Mathf.Clamp(num, 0f, 60f);
		this.pageSelectionLever.transform.localRotation = Quaternion.Euler(num, 0f, 0f);
		this.pageSelectionWheel.SetTargetAngle((float)wheelPos / this.quantMult);
	}

	public void ProgressionUpdated()
	{
		this.needsUIRefresh = true;
	}

	public void SetSelectedShelfAndItem(int shelf, int item, bool fromNetworkRPC)
	{
		if (!this.IsValidShelfItemIndex(shelf, item))
		{
			return;
		}
		if (this.toolProgressionManager == null)
		{
			return;
		}
		GRToolProgressionManager.ToolProgressionMetaData partMetadata = this.toolProgressionManager.GetPartMetadata(this.gameShelves[shelf].gRPurchaseSlots[item].PurchaseID);
		if (partMetadata != null)
		{
			this.itemDescriptionName.text = partMetadata.name;
			this.itemDescription.text = partMetadata.description;
			this.itemDescriptionAnnotation.text = partMetadata.annotation;
		}
		this.shelfSelectionText.text = this.gameShelves[shelf].ShelfName;
		if (this.gameShelves[shelf].gRPurchaseSlots[item].ToolEntityPrefab != null)
		{
			this.desiredMagnetEntityTypeId = this.gameShelves[shelf].gRPurchaseSlots[item].ToolEntityPrefab.name.GetStaticHash();
		}
		else
		{
			this.desiredMagnetEntityTypeId = -1;
		}
		bool flag = this.selectedShelf != shelf;
		bool flag2 = this.selectedItem != item;
		this.selectedShelf = shelf;
		this.selectedItem = item;
		this.needsUIRefresh = true;
		if (!fromNetworkRPC)
		{
			if (flag || flag2)
			{
				this.grManager.RequestNetworkShelfAndItemChange(this, this.selectedShelf, this.selectedItem);
				return;
			}
		}
		else
		{
			this.pageSelectionWheel.SetTargetShelf(this.selectedShelf);
			this.select1.SetButtonState(this.selectedItem == 0);
			this.select2.SetButtonState(this.selectedItem == 1);
			this.select3.SetButtonState(this.selectedItem == 2);
			this.select4.SetButtonState(this.selectedItem == 3);
		}
	}

	public void RequestPurchaseItem(int shelf, int item)
	{
		if (!this.IsValidShelfItemIndex(shelf, item))
		{
			return;
		}
		GRToolUpgradePurchaseStationShelf.GRPurchaseSlot grpurchaseSlot = this.gameShelves[shelf].gRPurchaseSlots[item];
		if (!this.CanLocalPlayerPurchaseItem(shelf, item))
		{
			if (this.scanner != null)
			{
				UnityEvent onFailed = this.scanner.onFailed;
				if (onFailed == null)
				{
					return;
				}
				onFailed.Invoke();
			}
			return;
		}
		if (this.scanner != null)
		{
			UnityEvent onSucceeded = this.scanner.onSucceeded;
			if (onSucceeded != null)
			{
				onSucceeded.Invoke();
			}
		}
		bool flag;
		if (!this.grManager.DebugIsToolStationHacked() && (!this.toolProgressionManager.IsPartUnlocked(grpurchaseSlot.PurchaseID, out flag) || !flag))
		{
			this.toolProgressionManager.AttemptToUnlockPart(grpurchaseSlot.PurchaseID);
			return;
		}
		this.grManager.RequestPurchaseToolOrUpgrade(this, shelf, item);
	}

	public ValueTuple<bool, bool> TryPurchaseAuthority(GRPlayer player, int shelf, int item)
	{
		if (this.currentActivePlayerActorNumber == -1)
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		GRPlayer grplayer = GRPlayer.Get(this.currentActivePlayerActorNumber);
		if (grplayer == null)
		{
			this.currentActivePlayerActorNumber = -1;
			return new ValueTuple<bool, bool>(false, false);
		}
		if (player != grplayer)
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		if (!this.grManager.IsAuthority())
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		if (!this.IsValidShelfItemIndex(shelf, item))
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		if (!this.toolProgressionManager)
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		GRToolUpgradePurchaseStationShelf.GRPurchaseSlot grpurchaseSlot = this.gameShelves[shelf].gRPurchaseSlots[item];
		this.toolProgressionManager.GetPartMetadata(grpurchaseSlot.PurchaseID);
		return new ValueTuple<bool, bool>(true, true);
	}

	public void ToolPurchaseResponseLocal(GRPlayer player, int shelf, int item, bool success)
	{
		if (!this.IsValidShelfItemIndex(shelf, item))
		{
			return;
		}
		if (!this.toolProgressionManager)
		{
			return;
		}
		GRToolUpgradePurchaseStationShelf.GRPurchaseSlot grpurchaseSlot = this.gameShelves[shelf].gRPurchaseSlots[item];
		GRToolProgressionManager.ToolProgressionMetaData partMetadata = this.toolProgressionManager.GetPartMetadata(grpurchaseSlot.PurchaseID);
		if (partMetadata == null)
		{
			return;
		}
		if (success)
		{
			int shiftCreditCost = partMetadata.shiftCreditCost;
			if (player != null)
			{
				if (player == GRPlayer.Get(VRRig.LocalRig))
				{
					player.IncrementCoresSpentPlayer(shiftCreditCost);
					player.SendToolPurchasedTelemetry(partMetadata.name, item, shiftCreditCost, 0);
				}
				else
				{
					player.IncrementCoresSpentGroup(shiftCreditCost);
				}
				player.AddItemPurchased(partMetadata.name);
				player.SubtractShiftCredit(shiftCreditCost);
				player.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.SpentCredits, (float)shiftCreditCost);
				this.reactor.RefreshScoreboards();
			}
			if (this.currentMagnetEntity != null)
			{
				this.currentMagnetEntity.transform.parent = null;
				this.currentMagnetEntity.GetComponent<Rigidbody>().isKinematic = false;
				this.currentMagnetEntity.pickupable = true;
				this.currentMagnetEntity.createData = 0L;
				this.currentMagnetEntity = null;
				this.currentMagnetEntityTypeId = -1;
			}
			UnityEvent unityEvent = this.purchaseSucceded;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
			return;
		}
		else
		{
			UnityEvent unityEvent2 = this.purchaseFailed;
			if (unityEvent2 == null)
			{
				return;
			}
			unityEvent2.Invoke();
			return;
		}
	}

	public void InitPageSelectionWheel()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < this.gameShelves.Count; i++)
		{
			list.Add(this.gameShelves[i].ShelfName);
		}
		this.pageSelectionWheel.InitFromNameList(list);
	}

	public static Color ColorFromRGB32(int r, int g, int b)
	{
		return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
	}

	public bool IsValidShelfItemIndex(int shelf, int idx)
	{
		return shelf >= 0 && shelf < this.gameShelves.Count && this.gameShelves[shelf].gRPurchaseSlots != null && idx >= 0 && idx < this.gameShelves[shelf].gRPurchaseSlots.Count && this.gameShelves[shelf].gRPurchaseSlots[idx].PurchaseID > GRToolProgressionManager.ToolParts.None;
	}

	private static Vector3 ExtractLossyScale(Matrix4x4 m)
	{
		float magnitude = new Vector3(m.m00, m.m10, m.m20).magnitude;
		float magnitude2 = new Vector3(m.m01, m.m11, m.m21).magnitude;
		float magnitude3 = new Vector3(m.m02, m.m12, m.m22).magnitude;
		return new Vector3(magnitude, magnitude2, magnitude3);
	}

	private static void DecomposeTRS(Matrix4x4 m, out Vector3 pos, out Quaternion rot, out Vector3 scale)
	{
		pos = m.GetColumn(3);
		Vector3 vector = m.GetColumn(0);
		Vector3 vector2 = m.GetColumn(1);
		Vector3 vector3 = m.GetColumn(2);
		scale = new Vector3(vector.magnitude, vector2.magnitude, vector3.magnitude);
		vector / scale.x;
		Vector3 vector4 = vector2 / scale.y;
		Vector3 vector5 = vector3 / scale.z;
		rot = Quaternion.LookRotation(vector5, vector4);
	}

	private GhostReactor reactor;

	private GhostReactorManager grManager;

	public List<GRToolUpgradePurchaseStationShelf> gameShelves;

	[NonSerialized]
	private GRToolProgressionManager toolProgressionManager;

	private Color colorPurchaseButtonCanAfford = GRToolUpgradePurchaseStationFull.ColorFromRGB32(0, 0, 0);

	private Color colorCanBuyCredits = GRToolUpgradePurchaseStationFull.ColorFromRGB32(140, 229, 37);

	private Color colorCanBuyJuice = GRToolUpgradePurchaseStationFull.ColorFromRGB32(232, 65, 255);

	private Color colorCantBuy = GRToolUpgradePurchaseStationFull.ColorFromRGB32(140, 38, 38);

	private Color colorSelectedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(251, 240, 229);

	private Color colorUnselectedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(147, 145, 140);

	private Color colorUnresearchedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(230, 19, 17);

	private Color colorUnselectedUnresearchedItem = GRToolUpgradePurchaseStationFull.ColorFromRGB32(133, 11, 10);

	private int selectedShelf;

	private int selectedItem;

	[NonSerialized]
	public int currentActivePlayerActorNumber = -1;

	private GRToolUpgradePurchaseStationFull.ShelfMovementState shelfMovementState;

	private int currentVisibleShelfIndex;

	private int nextVisibleShelfIndex;

	private GRSpringMovement frontBackShelfMovement;

	private GRSpringMovement raiseLowerShelfMovement;

	public Transform shelfRootTransform;

	public Transform shelfBackTransform;

	public Transform shelfLowerTransform;

	public TMP_Text shelfSelectionText;

	public TMP_Text playerInfo;

	public TMP_Text itemDescription;

	public TMP_Text itemDescriptionName;

	public TMP_Text itemDescriptionAnnotation;

	public TMP_Text purchaseButtonText;

	public GorillaPhysicalButton select1;

	public GorillaPhysicalButton select2;

	public GorillaPhysicalButton select3;

	public GorillaPhysicalButton select4;

	public AudioSource audioSourceLooping;

	public AudioSource audioSourceClang;

	public float audioSourceLoopingVolume = 0.5f;

	public Material unresearchedItemMaterial;

	public AudioSource interactAudioSource;

	public IDCardScanner scanner;

	public UnityEvent purchaseSucceded;

	public UnityEvent purchaseFailed;

	public Material backlightPurchase;

	public Material backlightResearch;

	public Material backlightLocked;

	private int lastKnownLocalPlayerCredits;

	private int lastKnownLocalPlayerJuice;

	private bool needsUIRefresh;

	public Transform ropeTop;

	public Transform ropeEnd;

	public Transform magnet;

	private GameEntity currentMagnetEntity;

	private int currentMagnetEntityTypeId = -1;

	private int desiredMagnetEntityTypeId = -1;

	private float prefabMagnetHeightOffset;

	public float maxMagnetDistance = 0.75f;

	private GRSpringMovement magnetMovement;

	public GRSelectionWheel pageSelectionWheel;

	public GameObject pageSelectionHandle;

	public GameObject pageSelectionLever;

	public float playerQueueTimeLimit = 30f;

	private bool disablePurchaseButton;

	private float purchaseButtonCooldown = 2f;

	private float purchaseButtonPressed;

	private const int ShelfIndex_None = -1;

	public bool currentlyShowingText = true;

	private List<GRToolProgressionManager.ToolParts> cachedRequiredPartsList = new List<GRToolProgressionManager.ToolParts>(5);

	private float lastRequestedActivePlayerTokenTime;

	private float requestActivePlayerTokenThrottleTime = 0.25f;

	private bool bIsGrippingLeft;

	private bool bIsGrippingRight;

	private bool bGripLeftLastFrame;

	private bool bGripRightLastFrame;

	private float maxHandleRange = 0.09f;

	private float timeSinceLastHandleBroadcast;

	private float angleOfLastHandleBroadcast;

	private float selectionWheelAngleOfLastBroadcast;

	private float quantMult = 100000f;

	private float lastHandleAngle = -10000f;

	public enum ShelfMovementState
	{
		Idle,
		MoveCurrentShelfBackward,
		MoveCurrentShelfForward,
		MoveNextShelfUpward,
		MoveNextShelfDownward,
		Count
	}
}
