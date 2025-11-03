using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

public class GorillaFireballControllerManager : MonoBehaviour
{
	private void Update()
	{
		if (!this.hasInitialized)
		{
			this.hasInitialized = true;
			List<InputDevice> list = new List<InputDevice>();
			List<InputDevice> list2 = new List<InputDevice>();
			InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, list);
			InputDevices.GetDevicesAtXRNode(XRNode.RightHand, list2);
			if (list.Count == 1)
			{
				this.leftHand = list[0];
			}
			if (list2.Count == 1)
			{
				this.rightHand = list2[0];
			}
		}
		float num = SteamVR_Actions.gorillaTag_LeftTriggerFloat.GetAxis(SteamVR_Input_Sources.LeftHand);
		if (this.leftHandLastState <= this.throwingThreshold && num > this.throwingThreshold)
		{
			this.CreateFireball(true);
		}
		else if (this.leftHandLastState >= this.throwingThreshold && num < this.throwingThreshold)
		{
			this.TryThrowFireball(true);
		}
		this.leftHandLastState = num;
		num = SteamVR_Actions.gorillaTag_RightTriggerFloat.GetAxis(SteamVR_Input_Sources.RightHand);
		if (this.rightHandLastState <= this.throwingThreshold && num > this.throwingThreshold)
		{
			this.CreateFireball(false);
		}
		else if (this.rightHandLastState >= this.throwingThreshold && num < this.throwingThreshold)
		{
			this.TryThrowFireball(false);
		}
		this.rightHandLastState = num;
	}

	public void TryThrowFireball(bool isLeftHand)
	{
		if (isLeftHand && GorillaPlaySpace.Instance.myVRRig.leftHandTransform.GetComponentInChildren<GorillaFireball>() != null)
		{
			GorillaPlaySpace.Instance.myVRRig.leftHandTransform.GetComponentInChildren<GorillaFireball>().ThrowThisThingo();
			return;
		}
		if (!isLeftHand && GorillaPlaySpace.Instance.myVRRig.rightHandTransform.GetComponentInChildren<GorillaFireball>() != null)
		{
			GorillaPlaySpace.Instance.myVRRig.rightHandTransform.GetComponentInChildren<GorillaFireball>().ThrowThisThingo();
		}
	}

	public void CreateFireball(bool isLeftHand)
	{
		object[] array = new object[1];
		Vector3 vector;
		if (isLeftHand)
		{
			array[0] = true;
			vector = GorillaPlaySpace.Instance.myVRRig.leftHandTransform.position;
		}
		else
		{
			array[0] = false;
			vector = GorillaPlaySpace.Instance.myVRRig.rightHandTransform.position;
		}
		PhotonNetwork.Instantiate("GorillaPrefabs/GorillaFireball", vector, Quaternion.identity, 0, array);
	}

	public InputDevice leftHand;

	public InputDevice rightHand;

	public bool hasInitialized;

	public float leftHandLastState;

	public float rightHandLastState;

	public float throwingThreshold = 0.9f;
}
