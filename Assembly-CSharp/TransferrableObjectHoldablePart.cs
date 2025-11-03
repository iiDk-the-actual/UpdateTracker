using System;
using UnityEngine;
using UnityEngine.Events;

public class TransferrableObjectHoldablePart : HoldableObject, ITickSystemTick
{
	public bool TickRunning { get; set; }

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public void Tick()
	{
		VRRig vrrig;
		if (!this.transferrableParentObject.IsLocalObject())
		{
			vrrig = this.transferrableParentObject.myOnlineRig;
			this.isHeld = (this.transferrableParentObject.itemState & this.heldBit) > (TransferrableObject.ItemStates)0;
			TransferrableObject.PositionState currentState = this.transferrableParentObject.currentState;
			if (currentState == TransferrableObject.PositionState.OnRightArm || currentState == TransferrableObject.PositionState.InRightHand)
			{
				this.isHeldLeftHand = this.isHeld;
			}
			else
			{
				this.isHeldLeftHand = false;
			}
		}
		else
		{
			vrrig = VRRig.LocalRig;
		}
		if (this.isHeld)
		{
			if (this.transferrableParentObject.InHand())
			{
				this.UpdateHeld(vrrig, this.isHeldLeftHand);
				return;
			}
			if (this.transferrableParentObject.IsLocalObject())
			{
				this.OnRelease(null, this.isHeldLeftHand ? EquipmentInteractor.instance.leftHand : EquipmentInteractor.instance.rightHand);
			}
		}
	}

	protected virtual void UpdateHeld(VRRig rig, bool isHeldLeftHand)
	{
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (this.transferrableParentObject.ownerRig && !this.transferrableParentObject.ownerRig.isLocal)
		{
			return;
		}
		this.isHeld = true;
		this.isHeldLeftHand = grabbingHand == EquipmentInteractor.instance.leftHand;
		this.transferrableParentObject.itemState |= this.heldBit;
		EquipmentInteractor.instance.UpdateHandEquipment(this, this.isHeldLeftHand);
		UnityEvent unityEvent = this.onGrab;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	public override void DropItemCleanup()
	{
		this.isHeld = false;
		this.isHeldLeftHand = false;
		this.transferrableParentObject.itemState &= ~this.heldBit;
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (EquipmentInteractor.instance.rightHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.rightHand)
		{
			return false;
		}
		if (EquipmentInteractor.instance.leftHandHeldEquipment == this && releasingHand != EquipmentInteractor.instance.leftHand)
		{
			return false;
		}
		EquipmentInteractor.instance.UpdateHandEquipment(null, this.isHeldLeftHand);
		this.isHeld = false;
		this.isHeldLeftHand = false;
		this.transferrableParentObject.itemState &= ~this.heldBit;
		UnityEvent unityEvent = this.onRelease;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		return true;
	}

	[SerializeField]
	protected TransferrableObject transferrableParentObject;

	[SerializeField]
	private TransferrableObject.ItemStates heldBit = TransferrableObject.ItemStates.Part0Held;

	private bool isHeld;

	protected bool isHeldLeftHand;

	public UnityEvent onGrab;

	public UnityEvent onRelease;

	public UnityEvent onDrop;
}
