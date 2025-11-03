using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;

public class TransferrableObjectHoldablePart_Slide : TransferrableObjectHoldablePart
{
	protected override void UpdateHeld(VRRig rig, bool isHeldLeftHand)
	{
		int num = (isHeldLeftHand ? 0 : 1);
		GTPlayer instance = GTPlayer.Instance;
		if (!rig.isOfflineVRRig)
		{
			Vector3 vector = instance.GetHandOffset(isHeldLeftHand) * rig.scaleFactor;
			VRMap vrmap = (isHeldLeftHand ? rig.leftHand : rig.rightHand);
			this._snapToLine.target.position = vrmap.GetExtrapolatedControllerPosition() - vector;
			return;
		}
		Transform controllerTransform = instance.GetControllerTransform(num == 0);
		Vector3 position = controllerTransform.position;
		Vector3 snappedPoint = this._snapToLine.GetSnappedPoint(position);
		if (this._maxHandSnapDistance > 0f && (controllerTransform.position - snappedPoint).IsLongerThan(this._maxHandSnapDistance))
		{
			this.OnRelease(null, isHeldLeftHand ? EquipmentInteractor.instance.leftHand : EquipmentInteractor.instance.rightHand);
			return;
		}
		controllerTransform.position = snappedPoint;
		this._snapToLine.target.position = snappedPoint;
	}

	[SerializeField]
	private float _maxHandSnapDistance;

	[SerializeField]
	private SnapXformToLine _snapToLine;

	private const int LEFT = 0;

	private const int RIGHT = 1;
}
