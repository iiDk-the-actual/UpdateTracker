using System;
using UnityEngine;

public class GhostLab : MonoBehaviourTick, IBuildValidation
{
	private void Awake()
	{
		this.relState = Object.FindFirstObjectByType<GhostLabReliableState>();
		this.doorState = GhostLab.EntranceDoorsState.BothClosed;
		this.doorOpen = new bool[this.relState.singleDoorCount];
	}

	public bool BuildValidationCheck()
	{
		return true;
	}

	public void DoorButtonPress(int buttonIndex, bool forSingleDoor)
	{
		if (!forSingleDoor)
		{
			this.UpdateEntranceDoorsState(buttonIndex);
			return;
		}
		this.UpdateDoorState(buttonIndex);
		this.relState.UpdateSingleDoorState(buttonIndex);
	}

	public void UpdateDoorState(int buttonIndex)
	{
		if ((this.doorOpen[buttonIndex] && this.slidingDoor[buttonIndex].localPosition == this.singleDoorTravelDistance) || (!this.doorOpen[buttonIndex] && this.slidingDoor[buttonIndex].localPosition == Vector3.zero))
		{
			this.doorOpen[buttonIndex] = !this.doorOpen[buttonIndex];
		}
	}

	public void UpdateEntranceDoorsState(int buttonIndex)
	{
		if (this.outerDoor == null || this.innerDoor == null)
		{
			return;
		}
		if (this.doorState == GhostLab.EntranceDoorsState.BothClosed)
		{
			if (!(this.innerDoor.localPosition != Vector3.zero) && !(this.outerDoor.localPosition != Vector3.zero))
			{
				if (buttonIndex == 0 || buttonIndex == 1)
				{
					this.doorState = GhostLab.EntranceDoorsState.OuterDoorOpen;
				}
				if (buttonIndex == 2 || buttonIndex == 3)
				{
					this.doorState = GhostLab.EntranceDoorsState.InnerDoorOpen;
				}
			}
		}
		else if (this.innerDoor.localPosition == this.doorTravelDistance || this.outerDoor.localPosition == this.doorTravelDistance)
		{
			this.doorState = GhostLab.EntranceDoorsState.BothClosed;
		}
		this.relState.UpdateEntranceDoorsState(this.doorState);
	}

	public override void Tick()
	{
		this.SynchStates();
		if (this.innerDoor != null && this.outerDoor != null)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			switch (this.doorState)
			{
			case GhostLab.EntranceDoorsState.InnerDoorOpen:
				zero2 = this.doorTravelDistance;
				break;
			case GhostLab.EntranceDoorsState.OuterDoorOpen:
				zero = this.doorTravelDistance;
				break;
			}
			this.outerDoor.localPosition = Vector3.MoveTowards(this.outerDoor.localPosition, zero, this.doorMoveSpeed * Time.deltaTime);
			this.innerDoor.localPosition = Vector3.MoveTowards(this.innerDoor.localPosition, zero2, this.doorMoveSpeed * Time.deltaTime);
		}
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < this.slidingDoor.Length; i++)
		{
			if (this.doorOpen[i])
			{
				vector = this.singleDoorTravelDistance;
			}
			else
			{
				vector = Vector3.zero;
			}
			this.slidingDoor[i].localPosition = Vector3.MoveTowards(this.slidingDoor[i].localPosition, vector, this.singleDoorMoveSpeed * Time.deltaTime);
		}
	}

	private void SynchStates()
	{
		this.doorState = this.relState.doorState;
		for (int i = 0; i < this.doorOpen.Length; i++)
		{
			this.doorOpen[i] = this.relState.singleDoorOpen[i];
		}
	}

	public bool IsDoorMoving(bool singleDoor, int index)
	{
		if (singleDoor)
		{
			return (this.doorOpen[index] && this.slidingDoor[index].localPosition != this.singleDoorTravelDistance) || (!this.doorOpen[index] && this.slidingDoor[index].localPosition != Vector3.zero);
		}
		if (index == 0 || index == 1)
		{
			return (this.doorState == GhostLab.EntranceDoorsState.OuterDoorOpen && this.outerDoor.localPosition != this.doorTravelDistance) || (this.doorState != GhostLab.EntranceDoorsState.OuterDoorOpen && this.outerDoor.localPosition != Vector3.zero);
		}
		return (this.doorState == GhostLab.EntranceDoorsState.InnerDoorOpen && this.innerDoor.localPosition != this.doorTravelDistance) || (this.doorState != GhostLab.EntranceDoorsState.InnerDoorOpen && this.innerDoor.localPosition != Vector3.zero);
	}

	public Transform outerDoor;

	public Transform innerDoor;

	public Vector3 doorTravelDistance;

	public float doorMoveSpeed;

	public float singleDoorMoveSpeed;

	public GhostLab.EntranceDoorsState doorState;

	public GhostLabReliableState relState;

	public Transform[] slidingDoor;

	public Vector3 singleDoorTravelDistance;

	private bool[] doorOpen;

	public enum EntranceDoorsState
	{
		BothClosed,
		InnerDoorOpen,
		OuterDoorOpen
	}
}
