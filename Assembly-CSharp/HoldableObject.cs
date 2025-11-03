using System;
using UnityEngine;

public abstract class HoldableObject : MonoBehaviour, IHoldableObject
{
	public virtual bool TwoHanded
	{
		get
		{
			return false;
		}
	}

	protected void OnDestroy()
	{
		if (EquipmentInteractor.hasInstance)
		{
			EquipmentInteractor.instance.ForceDropEquipment(this);
		}
	}

	public abstract void OnHover(InteractionPoint pointHovered, GameObject hoveringHand);

	public abstract void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand);

	public abstract void DropItemCleanup();

	public virtual bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		return (EquipmentInteractor.instance.rightHandHeldEquipment != this || !(releasingHand != EquipmentInteractor.instance.rightHand)) && (EquipmentInteractor.instance.leftHandHeldEquipment != this || !(releasingHand != EquipmentInteractor.instance.leftHand));
	}

	GameObject IHoldableObject.get_gameObject()
	{
		return base.gameObject;
	}

	string IHoldableObject.get_name()
	{
		return base.name;
	}

	void IHoldableObject.set_name(string value)
	{
		base.name = value;
	}
}
