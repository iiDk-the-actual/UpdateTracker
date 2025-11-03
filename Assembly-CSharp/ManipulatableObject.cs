using System;
using GorillaLocomotion;
using UnityEngine;

public class ManipulatableObject : HoldableObject
{
	protected virtual void OnStartManipulation(GameObject grabbingHand)
	{
	}

	protected virtual void OnStopManipulation(GameObject releasingHand, Vector3 releaseVelocity)
	{
	}

	protected virtual bool ShouldHandDetach(GameObject hand)
	{
		return false;
	}

	protected virtual void OnHeldUpdate(GameObject hand)
	{
	}

	protected virtual void OnReleasedUpdate()
	{
	}

	public virtual void LateUpdate()
	{
		if (this.isHeld)
		{
			if (this.holdingHand == null)
			{
				EquipmentInteractor.instance.ForceDropManipulatableObject(this);
				return;
			}
			this.OnHeldUpdate(this.holdingHand);
			if (this.ShouldHandDetach(this.holdingHand))
			{
				EquipmentInteractor.instance.ForceDropManipulatableObject(this);
				return;
			}
		}
		else
		{
			this.OnReleasedUpdate();
		}
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		bool flag = grabbingHand == EquipmentInteractor.instance.leftHand;
		EquipmentInteractor.instance.UpdateHandEquipment(this, flag);
		this.isHeld = true;
		this.holdingHand = grabbingHand;
		this.OnStartManipulation(this.holdingHand);
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		bool flag = releasingHand == EquipmentInteractor.instance.leftHand;
		Vector3 averageVelocity = GTPlayer.Instance.GetHandVelocityTracker(flag).GetAverageVelocity(true, 0.15f, false);
		if (flag)
		{
			EquipmentInteractor.instance.leftHandHeldEquipment = null;
		}
		else
		{
			EquipmentInteractor.instance.rightHandHeldEquipment = null;
		}
		this.isHeld = false;
		this.holdingHand = null;
		this.OnStopManipulation(releasingHand, averageVelocity);
		return true;
	}

	public override void DropItemCleanup()
	{
	}

	protected bool isHeld;

	protected GameObject holdingHand;
}
