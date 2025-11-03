using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;

public class HoverboardHandle : HoldableObject
{
	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
		if (!GTPlayer.Instance.isHoverAllowed)
		{
			return;
		}
		if (Time.frameCount > this.noHapticsUntilFrame)
		{
			GorillaTagger.Instance.StartVibration(hoveringHand == EquipmentInteractor.instance.leftHand, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
		}
		this.noHapticsUntilFrame = Time.frameCount + 1;
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (!GTPlayer.Instance.isHoverAllowed)
		{
			return;
		}
		bool flag = grabbingHand == EquipmentInteractor.instance.leftHand;
		Transform transform = (flag ? VRRig.LocalRig.leftHand.rigTarget : VRRig.LocalRig.rightHand.rigTarget);
		Quaternion quaternion;
		Vector3 vector;
		if (!this.parentVisual.IsHeld)
		{
			quaternion = (flag ? this.defaultHoldAngleLeft : this.defaultHoldAngleRight);
			vector = (flag ? this.defaultHoldPosLeft : this.defaultHoldPosRight);
		}
		else
		{
			quaternion = transform.InverseTransformRotation(this.parentVisual.transform.rotation);
			vector = transform.InverseTransformPoint(this.parentVisual.transform.position);
		}
		this.parentVisual.SetIsHeld(flag, vector, quaternion, this.parentVisual.boardColor);
		EquipmentInteractor.instance.UpdateHandEquipment(this, flag);
	}

	public override void DropItemCleanup()
	{
		if (this.parentVisual.gameObject.activeSelf)
		{
			this.parentVisual.DropFreeBoard();
		}
		this.parentVisual.SetNotHeld();
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
		EquipmentInteractor.instance.UpdateHandEquipment(null, this.parentVisual.IsLeftHanded);
		this.parentVisual.SetNotHeld();
		return true;
	}

	[SerializeField]
	private HoverboardVisual parentVisual;

	[SerializeField]
	private Quaternion defaultHoldAngleLeft;

	[SerializeField]
	private Quaternion defaultHoldAngleRight;

	[SerializeField]
	private Vector3 defaultHoldPosLeft;

	[SerializeField]
	private Vector3 defaultHoldPosRight;

	private int noHapticsUntilFrame = -1;
}
