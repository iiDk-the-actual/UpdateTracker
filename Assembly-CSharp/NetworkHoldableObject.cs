using System;
using Fusion;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public abstract class NetworkHoldableObject : NetworkComponent, IHoldableObject
{
	public virtual bool TwoHanded
	{
		get
		{
			return false;
		}
	}

	public abstract void OnHover(InteractionPoint pointHovered, GameObject hoveringHand);

	public abstract void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand);

	public abstract void DropItemCleanup();

	public virtual bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		return (EquipmentInteractor.instance.rightHandHeldEquipment != this || !(releasingHand != EquipmentInteractor.instance.rightHand)) && (EquipmentInteractor.instance.leftHandHeldEquipment != this || !(releasingHand != EquipmentInteractor.instance.leftHand));
	}

	public override void ReadDataFusion()
	{
	}

	public override void WriteDataFusion()
	{
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
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

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
	}
}
