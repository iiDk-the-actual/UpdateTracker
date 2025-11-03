using System;
using UnityEngine;
using UnityEngine.XR;

public static class NetInput
{
	public static VRRig LocalPlayerVRRig
	{
		get
		{
			if (NetInput._localPlayerVRRig == null)
			{
				NetInput._localPlayerVRRig = GameObject.Find("Local VRRig").GetComponentInChildren<VRRig>();
			}
			return NetInput._localPlayerVRRig;
		}
	}

	public static NetworkedInput GetInput()
	{
		NetworkedInput networkedInput = default(NetworkedInput);
		if (NetInput.LocalPlayerVRRig == null)
		{
			return networkedInput;
		}
		networkedInput.headRot_LS = NetInput.LocalPlayerVRRig.head.rigTarget.localRotation;
		networkedInput.rightHandPos_LS = NetInput.LocalPlayerVRRig.rightHand.rigTarget.localPosition;
		networkedInput.rightHandRot_LS = NetInput.LocalPlayerVRRig.rightHand.rigTarget.localRotation;
		networkedInput.leftHandPos_LS = NetInput.LocalPlayerVRRig.leftHand.rigTarget.localPosition;
		networkedInput.leftHandRot_LS = NetInput.LocalPlayerVRRig.leftHand.rigTarget.localRotation;
		networkedInput.handPoseData = NetInput.LocalPlayerVRRig.ReturnHandPosition();
		networkedInput.rootPosition = NetInput.LocalPlayerVRRig.transform.position;
		networkedInput.rootRotation = NetInput.LocalPlayerVRRig.transform.rotation;
		networkedInput.leftThumbTouch = ControllerInputPoller.PrimaryButtonTouch(XRNode.LeftHand) || ControllerInputPoller.SecondaryButtonTouch(XRNode.LeftHand);
		networkedInput.leftThumbPress = ControllerInputPoller.PrimaryButtonPress(XRNode.LeftHand) || ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
		networkedInput.leftIndexValue = ControllerInputPoller.TriggerFloat(XRNode.LeftHand);
		networkedInput.leftMiddleValue = ControllerInputPoller.GripFloat(XRNode.LeftHand);
		networkedInput.rightThumbTouch = ControllerInputPoller.PrimaryButtonTouch(XRNode.RightHand) || ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
		networkedInput.rightThumbPress = ControllerInputPoller.PrimaryButtonPress(XRNode.RightHand) || ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
		networkedInput.rightIndexValue = ControllerInputPoller.TriggerFloat(XRNode.RightHand);
		networkedInput.rightMiddleValue = ControllerInputPoller.GripFloat(XRNode.RightHand);
		networkedInput.scale = NetInput.LocalPlayerVRRig.scaleFactor;
		return networkedInput;
	}

	private static VRRig _localPlayerVRRig;
}
