using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.Serialization;

public class BodyDockPositions : MonoBehaviour
{
	public TransferrableObject[] allObjects
	{
		get
		{
			return this._allObjects;
		}
		set
		{
			this._allObjects = value;
		}
	}

	public void Awake()
	{
		RoomSystem.LeftRoomEvent += new Action(this.OnLeftRoom);
		RoomSystem.PlayerLeftEvent += new Action<NetPlayer>(this.OnPlayerLeftRoom);
	}

	public void OnPlayerLeftRoom(NetPlayer otherPlayer)
	{
		if (object.Equals(this.myRig.creator, otherPlayer))
		{
			this.DeallocateSharableInstances();
		}
	}

	public void OnLeftRoom()
	{
		this.DeallocateSharableInstances();
	}

	public WorldShareableItem AllocateSharableInstance(BodyDockPositions.DropPositions position, NetPlayer owner)
	{
		switch (position)
		{
		case BodyDockPositions.DropPositions.None:
		case BodyDockPositions.DropPositions.LeftArm:
		case BodyDockPositions.DropPositions.RightArm:
		case BodyDockPositions.DropPositions.LeftArm | BodyDockPositions.DropPositions.RightArm:
		case BodyDockPositions.DropPositions.Chest:
		case BodyDockPositions.DropPositions.MaxDropPostions:
		case BodyDockPositions.DropPositions.RightArm | BodyDockPositions.DropPositions.Chest:
		case BodyDockPositions.DropPositions.LeftArm | BodyDockPositions.DropPositions.RightArm | BodyDockPositions.DropPositions.Chest:
			break;
		case BodyDockPositions.DropPositions.LeftBack:
			if (this.leftBackSharableItem == null)
			{
				this.leftBackSharableItem = ObjectPools.instance.Instantiate(this.SharableItemInstance, true).GetComponent<WorldShareableItem>();
				this.leftBackSharableItem.GetComponent<RequestableOwnershipGuard>().SetOwnership(owner, false, true);
				this.leftBackSharableItem.GetComponent<WorldShareableItem>().SetupSharableViewIDs(owner, 3);
			}
			return this.leftBackSharableItem;
		default:
			if (position == BodyDockPositions.DropPositions.RightBack)
			{
				if (this.rightBackShareableItem == null)
				{
					this.rightBackShareableItem = ObjectPools.instance.Instantiate(this.SharableItemInstance, true).GetComponent<WorldShareableItem>();
					this.rightBackShareableItem.GetComponent<RequestableOwnershipGuard>().SetOwnership(owner, false, true);
					this.rightBackShareableItem.GetComponent<WorldShareableItem>().SetupSharableViewIDs(owner, 4);
				}
				return this.rightBackShareableItem;
			}
			if (position != BodyDockPositions.DropPositions.All)
			{
			}
			break;
		}
		throw new ArgumentOutOfRangeException("position", position, null);
	}

	public void DeallocateSharableInstance(WorldShareableItem worldShareable)
	{
		if (worldShareable == null)
		{
			return;
		}
		if (worldShareable == this.leftBackSharableItem)
		{
			if (this.leftBackSharableItem == null)
			{
				return;
			}
			this.leftBackSharableItem.ResetViews();
			ObjectPools.instance.Destroy(this.leftBackSharableItem.gameObject);
			this.leftBackSharableItem = null;
		}
		if (worldShareable == this.rightBackShareableItem)
		{
			if (this.rightBackShareableItem == null)
			{
				return;
			}
			this.rightBackShareableItem.ResetViews();
			ObjectPools.instance.Destroy(this.rightBackShareableItem.gameObject);
			this.rightBackShareableItem = null;
		}
	}

	public void DeallocateSharableInstances()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (this.rightBackShareableItem != null)
		{
			this.rightBackShareableItem.ResetViews();
			ObjectPools.instance.Destroy(this.rightBackShareableItem.gameObject);
		}
		if (this.leftBackSharableItem != null)
		{
			this.leftBackSharableItem.ResetViews();
			ObjectPools.instance.Destroy(this.leftBackSharableItem.gameObject);
		}
		this.leftBackSharableItem = null;
		this.rightBackShareableItem = null;
	}

	public static bool IsPositionLeft(BodyDockPositions.DropPositions pos)
	{
		return pos == BodyDockPositions.DropPositions.LeftArm || pos == BodyDockPositions.DropPositions.LeftBack;
	}

	public int DropZoneStorageUsed(BodyDockPositions.DropPositions dropPosition)
	{
		if (this.myRig == null)
		{
			Debug.Log("BodyDockPositions lost reference to VR Rig, resetting it now", this);
			this.myRig = base.GetComponent<VRRig>();
		}
		if (this.myRig == null)
		{
			Debug.Log("Unable to reset reference");
			return -1;
		}
		for (int i = 0; i < this.myRig.ActiveTransferrableObjectIndexLength(); i++)
		{
			if (this.myRig.ActiveTransferrableObjectIndex(i) >= 0 && this.allObjects[this.myRig.ActiveTransferrableObjectIndex(i)].gameObject.activeInHierarchy && this.allObjects[this.myRig.ActiveTransferrableObjectIndex(i)].storedZone == dropPosition)
			{
				return this.myRig.ActiveTransferrableObjectIndex(i);
			}
		}
		return -1;
	}

	public TransferrableObject ItemPositionInUse(BodyDockPositions.DropPositions dropPosition)
	{
		TransferrableObject.PositionState positionState = this.MapDropPositionToState(dropPosition);
		if (this.myRig == null)
		{
			Debug.Log("BodyDockPositions lost reference to VR Rig, resetting it now", this);
			this.myRig = base.GetComponent<VRRig>();
		}
		if (this.myRig == null)
		{
			Debug.Log("Unable to reset reference");
			return null;
		}
		for (int i = 0; i < this.myRig.ActiveTransferrableObjectIndexLength(); i++)
		{
			if (this.myRig.ActiveTransferrableObjectIndex(i) != -1 && this.allObjects[this.myRig.ActiveTransferrableObjectIndex(i)].gameObject.activeInHierarchy && this.allObjects[this.myRig.ActiveTransferrableObjectIndex(i)].currentState == positionState)
			{
				return this.allObjects[this.myRig.ActiveTransferrableObjectIndex(i)];
			}
		}
		return null;
	}

	private int EnableTransferrableItem(int allItemsIndex, BodyDockPositions.DropPositions startingPosition, TransferrableObject.PositionState startingState)
	{
		if (allItemsIndex < 0 || allItemsIndex >= this.allObjects.Length)
		{
			return -1;
		}
		if (this.myRig != null && this.myRig.isOfflineVRRig)
		{
			for (int i = 0; i < this.myRig.ActiveTransferrableObjectIndexLength(); i++)
			{
				if (this.myRig.ActiveTransferrableObjectIndex(i) == allItemsIndex)
				{
					this.DisableTransferrableItem(allItemsIndex);
				}
			}
			for (int j = 0; j < this.myRig.ActiveTransferrableObjectIndexLength(); j++)
			{
				if (this.myRig.ActiveTransferrableObjectIndex(j) == -1)
				{
					string itemNameFromDisplayName = CosmeticsController.instance.GetItemNameFromDisplayName(this.allObjects[allItemsIndex].gameObject.name);
					if (this.myRig.IsItemAllowed(itemNameFromDisplayName))
					{
						this.myRig.SetActiveTransferrableObjectIndex(j, allItemsIndex);
						this.myRig.SetTransferrablePosStates(j, startingState);
						this.myRig.SetTransferrableItemStates(j, (TransferrableObject.ItemStates)0);
						this.myRig.SetTransferrableDockPosition(j, startingPosition);
						this.EnableTransferrableGameObject(allItemsIndex, startingPosition, startingState);
						return j;
					}
				}
			}
		}
		return -1;
	}

	public BodyDockPositions.DropPositions ItemActive(int allItemsIndex)
	{
		if (!this.allObjects[allItemsIndex].gameObject.activeSelf)
		{
			return BodyDockPositions.DropPositions.None;
		}
		return this.allObjects[allItemsIndex].storedZone;
	}

	public static BodyDockPositions.DropPositions OfflineItemActive(int allItemsIndex)
	{
		if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
		{
			return BodyDockPositions.DropPositions.None;
		}
		BodyDockPositions component = GorillaTagger.Instance.offlineVRRig.GetComponent<BodyDockPositions>();
		if (component == null)
		{
			return BodyDockPositions.DropPositions.None;
		}
		if (!component.allObjects[allItemsIndex].gameObject.activeSelf)
		{
			return BodyDockPositions.DropPositions.None;
		}
		return component.allObjects[allItemsIndex].storedZone;
	}

	public void DisableTransferrableItem(int index)
	{
		TransferrableObject transferrableObject = this.allObjects[index];
		if (transferrableObject.gameObject.activeSelf)
		{
			transferrableObject.gameObject.Disable();
			transferrableObject.storedZone = BodyDockPositions.DropPositions.None;
		}
		if (this.myRig.isOfflineVRRig)
		{
			for (int i = 0; i < this.myRig.ActiveTransferrableObjectIndexLength(); i++)
			{
				if (this.myRig.ActiveTransferrableObjectIndex(i) == index)
				{
					this.myRig.SetActiveTransferrableObjectIndex(i, -1);
				}
			}
		}
	}

	public void DisableAllTransferableItems()
	{
		if (!CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			return;
		}
		for (int i = 0; i < this.myRig.ActiveTransferrableObjectIndexLength(); i++)
		{
			int num = this.myRig.ActiveTransferrableObjectIndex(i);
			if (num >= 0 && num < this.allObjects.Length)
			{
				TransferrableObject transferrableObject = this.allObjects[num];
				transferrableObject.gameObject.Disable();
				transferrableObject.storedZone = BodyDockPositions.DropPositions.None;
				this.myRig.SetActiveTransferrableObjectIndex(i, -1);
				this.myRig.SetTransferrableItemStates(i, (TransferrableObject.ItemStates)0);
				this.myRig.SetTransferrablePosStates(i, TransferrableObject.PositionState.None);
			}
		}
		this.DeallocateSharableInstances();
	}

	private bool AllItemsIndexValid(int allItemsIndex)
	{
		return allItemsIndex != -1 && allItemsIndex < this.allObjects.Length;
	}

	public bool PositionAvailable(int allItemIndex, BodyDockPositions.DropPositions startPos)
	{
		return (this.allObjects[allItemIndex].dockPositions & startPos) > BodyDockPositions.DropPositions.None;
	}

	public BodyDockPositions.DropPositions FirstAvailablePosition(int allItemIndex)
	{
		for (int i = 0; i < 5; i++)
		{
			BodyDockPositions.DropPositions dropPositions = (BodyDockPositions.DropPositions)(1 << i);
			if ((this.allObjects[allItemIndex].dockPositions & dropPositions) != BodyDockPositions.DropPositions.None)
			{
				return dropPositions;
			}
		}
		return BodyDockPositions.DropPositions.None;
	}

	public int TransferrableItemDisable(int allItemsIndex)
	{
		if (BodyDockPositions.OfflineItemActive(allItemsIndex) != BodyDockPositions.DropPositions.None)
		{
			this.DisableTransferrableItem(allItemsIndex);
		}
		return 0;
	}

	public void TransferrableItemDisableAtPosition(BodyDockPositions.DropPositions dropPositions)
	{
		int num = this.DropZoneStorageUsed(dropPositions);
		if (num >= 0)
		{
			this.TransferrableItemDisable(num);
		}
	}

	public void TransferrableItemEnableAtPosition(string itemName, BodyDockPositions.DropPositions dropPosition)
	{
		if (this.DropZoneStorageUsed(dropPosition) >= 0)
		{
			return;
		}
		List<int> list = this.TransferrableObjectIndexFromName(itemName);
		if (list.Count == 0)
		{
			return;
		}
		TransferrableObject.PositionState positionState = this.MapDropPositionToState(dropPosition);
		if (list.Count == 1)
		{
			this.EnableTransferrableItem(list[0], dropPosition, positionState);
			return;
		}
		int num = (BodyDockPositions.IsPositionLeft(dropPosition) ? list[0] : list[1]);
		this.EnableTransferrableItem(num, dropPosition, positionState);
	}

	public bool TransferrableItemActive(string transferrableItemName)
	{
		List<int> list = this.TransferrableObjectIndexFromName(transferrableItemName);
		if (list.Count == 0)
		{
			return false;
		}
		foreach (int num in list)
		{
			if (this.TransferrableItemActive(num))
			{
				return true;
			}
		}
		return false;
	}

	public bool TransferrableItemActiveAtPos(string transferrableItemName, BodyDockPositions.DropPositions dropPosition)
	{
		List<int> list = this.TransferrableObjectIndexFromName(transferrableItemName);
		if (list.Count == 0)
		{
			return false;
		}
		foreach (int num in list)
		{
			BodyDockPositions.DropPositions dropPositions = this.TransferrableItemPosition(num);
			if (dropPositions != BodyDockPositions.DropPositions.None && dropPositions == dropPosition)
			{
				return true;
			}
		}
		return false;
	}

	public bool TransferrableItemActive(int allItemsIndex)
	{
		return this.ItemActive(allItemsIndex) > BodyDockPositions.DropPositions.None;
	}

	public TransferrableObject TransferrableItem(int allItemsIndex)
	{
		return this.allObjects[allItemsIndex];
	}

	public BodyDockPositions.DropPositions TransferrableItemPosition(int allItemsIndex)
	{
		return this.ItemActive(allItemsIndex);
	}

	public bool DisableTransferrableItem(string transferrableItemName)
	{
		List<int> list = this.TransferrableObjectIndexFromName(transferrableItemName);
		if (list.Count == 0)
		{
			return false;
		}
		foreach (int num in list)
		{
			this.DisableTransferrableItem(num);
		}
		return true;
	}

	public BodyDockPositions.DropPositions OppositePosition(BodyDockPositions.DropPositions pos)
	{
		if (pos == BodyDockPositions.DropPositions.LeftArm)
		{
			return BodyDockPositions.DropPositions.RightArm;
		}
		if (pos == BodyDockPositions.DropPositions.RightArm)
		{
			return BodyDockPositions.DropPositions.LeftArm;
		}
		if (pos == BodyDockPositions.DropPositions.LeftBack)
		{
			return BodyDockPositions.DropPositions.RightBack;
		}
		if (pos == BodyDockPositions.DropPositions.RightBack)
		{
			return BodyDockPositions.DropPositions.LeftBack;
		}
		return pos;
	}

	public BodyDockPositions.DockingResult ToggleWithHandedness(string transferrableItemName, bool isLeftHand, bool bothHands)
	{
		List<int> list = this.TransferrableObjectIndexFromName(transferrableItemName);
		if (list.Count == 0)
		{
			return new BodyDockPositions.DockingResult();
		}
		if (!this.AllItemsIndexValid(list[0]))
		{
			return new BodyDockPositions.DockingResult();
		}
		BodyDockPositions.DropPositions dropPositions;
		if (isLeftHand)
		{
			dropPositions = (((this.allObjects[list[0]].dockPositions & BodyDockPositions.DropPositions.RightArm) != BodyDockPositions.DropPositions.None) ? BodyDockPositions.DropPositions.RightArm : BodyDockPositions.DropPositions.LeftBack);
		}
		else
		{
			dropPositions = (((this.allObjects[list[0]].dockPositions & BodyDockPositions.DropPositions.LeftArm) != BodyDockPositions.DropPositions.None) ? BodyDockPositions.DropPositions.LeftArm : BodyDockPositions.DropPositions.RightBack);
		}
		return this.ToggleTransferrableItem(transferrableItemName, dropPositions, bothHands);
	}

	public BodyDockPositions.DockingResult ToggleTransferrableItem(string transferrableItemName, BodyDockPositions.DropPositions startingPos, bool bothHands)
	{
		BodyDockPositions.DockingResult dockingResult = new BodyDockPositions.DockingResult();
		List<int> list = this.TransferrableObjectIndexFromName(transferrableItemName);
		if (list.Count == 0)
		{
			return dockingResult;
		}
		if (bothHands && list.Count == 2)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				BodyDockPositions.DropPositions dropPositions = BodyDockPositions.OfflineItemActive(num);
				if (dropPositions != BodyDockPositions.DropPositions.None)
				{
					this.TransferrableItemDisable(num);
					dockingResult.positionsDisabled.Add(dropPositions);
				}
			}
			if (dockingResult.positionsDisabled.Count >= 1)
			{
				return dockingResult;
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			int num2 = list[j];
			BodyDockPositions.DropPositions dropPositions2 = startingPos;
			if (bothHands && j != 0)
			{
				dropPositions2 = this.OppositePosition(dropPositions2);
			}
			if (!this.PositionAvailable(num2, dropPositions2))
			{
				dropPositions2 = this.FirstAvailablePosition(num2);
				if (dropPositions2 == BodyDockPositions.DropPositions.None)
				{
					return dockingResult;
				}
			}
			if (BodyDockPositions.OfflineItemActive(num2) == dropPositions2)
			{
				this.TransferrableItemDisable(num2);
				dockingResult.positionsDisabled.Add(dropPositions2);
			}
			else
			{
				this.TransferrableItemDisableAtPosition(dropPositions2);
				dockingResult.dockedPosition.Add(dropPositions2);
				TransferrableObject.PositionState positionState = this.MapDropPositionToState(dropPositions2);
				if (this.TransferrableItemActive(num2))
				{
					BodyDockPositions.DropPositions dropPositions3 = this.TransferrableItemPosition(num2);
					dockingResult.positionsDisabled.Add(dropPositions3);
					this.MoveTransferableItem(num2, dropPositions2, positionState);
				}
				else
				{
					this.EnableTransferrableItem(num2, dropPositions2, positionState);
				}
			}
		}
		return dockingResult;
	}

	private void MoveTransferableItem(int allItemsIndex, BodyDockPositions.DropPositions newPosition, TransferrableObject.PositionState newPositionState)
	{
		this.allObjects[allItemsIndex].storedZone = newPosition;
		this.allObjects[allItemsIndex].currentState = newPositionState;
		this.allObjects[allItemsIndex].ResetToDefaultState();
	}

	public void EnableTransferrableGameObject(int allItemsIndex, BodyDockPositions.DropPositions dropZone, TransferrableObject.PositionState startingPosition)
	{
		GameObject gameObject = this.allObjects[allItemsIndex].gameObject;
		TransferrableObject component = gameObject.GetComponent<TransferrableObject>();
		if ((component.dockPositions & dropZone) == BodyDockPositions.DropPositions.None || !component.ValidateState(startingPosition))
		{
			gameObject.Disable();
			return;
		}
		this.MoveTransferableItem(allItemsIndex, dropZone, startingPosition);
		gameObject.SetActive(true);
		ProjectileWeapon component2;
		if ((component2 = gameObject.GetComponent<ProjectileWeapon>()) != null)
		{
			component2.enabled = true;
		}
	}

	public void RefreshTransferrableItems()
	{
		if (!this.myRig)
		{
			this.myRig = base.GetComponentInParent<VRRig>(true);
			if (!this.myRig)
			{
				Debug.LogError("BodyDockPositions.RefreshTransferrableItems: (should never happen) myRig is null and could not be found on same GameObject or parents. Path: " + base.transform.GetPathQ(), this);
			}
		}
		this.objectsToEnable.Clear();
		this.objectsToDisable.Clear();
		for (int i = 0; i < this.myRig.ActiveTransferrableObjectIndexLength(); i++)
		{
			bool flag = true;
			int num = this.myRig.ActiveTransferrableObjectIndex(i);
			if (num != -1)
			{
				if (num < 0 || num >= this.allObjects.Length)
				{
					Debug.LogError(string.Format("Transferrable object index {0} out of range, expected [0..{1})", num, this.allObjects.Length));
				}
				else if (this.myRig.IsItemAllowed(CosmeticsController.instance.GetItemNameFromDisplayName(this.allObjects[num].gameObject.name)))
				{
					for (int j = 0; j < this.allObjects.Length; j++)
					{
						if (j == this.myRig.ActiveTransferrableObjectIndex(i) && this.allObjects[j].gameObject.activeSelf)
						{
							this.allObjects[j].objectIndex = i;
							flag = false;
						}
					}
					if (flag)
					{
						this.objectsToEnable.Add(i);
					}
				}
			}
		}
		for (int k = 0; k < this.allObjects.Length; k++)
		{
			if (this.allObjects[k] != null && this.allObjects[k].gameObject.activeSelf)
			{
				bool flag2 = true;
				for (int l = 0; l < this.myRig.ActiveTransferrableObjectIndexLength(); l++)
				{
					if (this.myRig.ActiveTransferrableObjectIndex(l) == k && this.myRig.IsItemAllowed(CosmeticsController.instance.GetItemNameFromDisplayName(this.allObjects[this.myRig.ActiveTransferrableObjectIndex(l)].gameObject.name)))
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					this.objectsToDisable.Add(k);
				}
			}
		}
		foreach (int num2 in this.objectsToDisable)
		{
			this.DisableTransferrableItem(num2);
		}
		foreach (int num3 in this.objectsToEnable)
		{
			this.EnableTransferrableGameObject(this.myRig.ActiveTransferrableObjectIndex(num3), this.myRig.TransferrableDockPosition(num3), this.myRig.TransferrablePosStates(num3));
		}
		this.UpdateHandState();
	}

	public int ReturnTransferrableItemIndex(int allItemsIndex)
	{
		for (int i = 0; i < this.myRig.ActiveTransferrableObjectIndexLength(); i++)
		{
			if (this.myRig.ActiveTransferrableObjectIndex(i) == allItemsIndex)
			{
				return i;
			}
		}
		return -1;
	}

	public List<int> TransferrableObjectIndexFromName(string transObjectName)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < this.allObjects.Length; i++)
		{
			if (!(this.allObjects[i] == null) && this.allObjects[i].gameObject.name == transObjectName)
			{
				list.Add(i);
			}
		}
		return list;
	}

	private TransferrableObject.PositionState MapDropPositionToState(BodyDockPositions.DropPositions pos)
	{
		if (pos == BodyDockPositions.DropPositions.RightArm)
		{
			return TransferrableObject.PositionState.OnRightArm;
		}
		if (pos == BodyDockPositions.DropPositions.LeftArm)
		{
			return TransferrableObject.PositionState.OnLeftArm;
		}
		if (pos == BodyDockPositions.DropPositions.LeftBack)
		{
			return TransferrableObject.PositionState.OnLeftShoulder;
		}
		if (pos == BodyDockPositions.DropPositions.RightBack)
		{
			return TransferrableObject.PositionState.OnRightShoulder;
		}
		return TransferrableObject.PositionState.OnChest;
	}

	internal int PreviousLeftHandThrowableIndex
	{
		get
		{
			return this.throwableDisabledIndex[0];
		}
	}

	internal int PreviousRightHandThrowableIndex
	{
		get
		{
			return this.throwableDisabledIndex[1];
		}
	}

	internal float PreviousLeftHandThrowableDisabledTime
	{
		get
		{
			return this.throwableDisabledTime[0];
		}
	}

	internal float PreviousRightHandThrowableDisabledTime
	{
		get
		{
			return this.throwableDisabledTime[1];
		}
	}

	private void UpdateHandState()
	{
		for (int i = 0; i < 2; i++)
		{
			GameObject[] array = ((i == 0) ? this.leftHandThrowables : this.rightHandThrowables);
			int num = ((i == 0) ? this.myRig.LeftThrowableProjectileIndex : this.myRig.RightThrowableProjectileIndex);
			for (int j = 0; j < array.Length; j++)
			{
				bool activeSelf = array[j].activeSelf;
				bool flag = j == num;
				array[j].SetActive(flag);
				if (activeSelf && !flag)
				{
					this.throwableDisabledIndex[i] = j;
					this.throwableDisabledTime[i] = Time.time + 0.02f;
				}
			}
		}
	}

	internal GameObject GetLeftHandThrowable()
	{
		return this.GetLeftHandThrowable(this.myRig.LeftThrowableProjectileIndex);
	}

	internal GameObject GetLeftHandThrowable(int throwableIndex)
	{
		if (throwableIndex < 0 || throwableIndex >= this.leftHandThrowables.Length)
		{
			throwableIndex = this.PreviousLeftHandThrowableIndex;
			if (throwableIndex < 0 || throwableIndex >= this.leftHandThrowables.Length || this.PreviousLeftHandThrowableDisabledTime < Time.time)
			{
				return null;
			}
		}
		return this.leftHandThrowables[throwableIndex];
	}

	internal GameObject GetRightHandThrowable()
	{
		return this.GetRightHandThrowable(this.myRig.RightThrowableProjectileIndex);
	}

	internal GameObject GetRightHandThrowable(int throwableIndex)
	{
		if (throwableIndex < 0 || throwableIndex >= this.rightHandThrowables.Length)
		{
			throwableIndex = this.PreviousRightHandThrowableIndex;
			if (throwableIndex < 0 || throwableIndex >= this.rightHandThrowables.Length || this.PreviousRightHandThrowableDisabledTime < Time.time)
			{
				return null;
			}
		}
		return this.rightHandThrowables[throwableIndex];
	}

	public VRRig myRig;

	public GameObject[] leftHandThrowables;

	public GameObject[] rightHandThrowables;

	[FormerlySerializedAs("allObjects")]
	public TransferrableObject[] _allObjects;

	private List<int> objectsToEnable = new List<int>();

	private List<int> objectsToDisable = new List<int>();

	public Transform leftHandTransform;

	public Transform rightHandTransform;

	public Transform chestTransform;

	public Transform leftArmTransform;

	public Transform rightArmTransform;

	public Transform leftBackTransform;

	public Transform rightBackTransform;

	public WorldShareableItem leftBackSharableItem;

	public WorldShareableItem rightBackShareableItem;

	public GameObject SharableItemInstance;

	private int[] throwableDisabledIndex = new int[] { -1, -1 };

	private float[] throwableDisabledTime = new float[2];

	[Flags]
	public enum DropPositions
	{
		LeftArm = 1,
		RightArm = 2,
		Chest = 4,
		LeftBack = 8,
		RightBack = 16,
		MaxDropPostions = 5,
		All = 31,
		None = 0
	}

	public class DockingResult
	{
		public DockingResult()
		{
			this.dockedPosition = new List<BodyDockPositions.DropPositions>(2);
			this.positionsDisabled = new List<BodyDockPositions.DropPositions>(2);
		}

		public List<BodyDockPositions.DropPositions> positionsDisabled;

		public List<BodyDockPositions.DropPositions> dockedPosition;
	}
}
