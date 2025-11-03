using System;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public class VRMap
{
	public Vector3 syncPos
	{
		get
		{
			return this.netSyncPos.CurrentSyncTarget;
		}
		set
		{
			this.netSyncPos.SetNewSyncTarget(value);
		}
	}

	public virtual void Initialize()
	{
	}

	public void MapOther(float lerpValue)
	{
		Vector3 vector;
		Quaternion quaternion;
		this.rigTarget.GetLocalPositionAndRotation(out vector, out quaternion);
		this.rigTarget.SetLocalPositionAndRotation(Vector3.Lerp(vector, this.syncPos, lerpValue), Quaternion.Lerp(quaternion, this.syncRotation, lerpValue));
	}

	public void MapMine(float ratio, Transform playerOffsetTransform)
	{
		Vector3 vector;
		Quaternion quaternion;
		this.rigTarget.GetPositionAndRotation(out vector, out quaternion);
		if (this.overrideTarget != null)
		{
			Vector3 vector2;
			Quaternion quaternion2;
			this.overrideTarget.GetPositionAndRotation(out vector2, out quaternion2);
			this.rigTarget.SetPositionAndRotation(vector2 + quaternion * this.trackingPositionOffset * ratio, quaternion2 * Quaternion.Euler(this.trackingRotationOffset));
		}
		else
		{
			if (!this.hasInputDevice && ConnectedControllerHandler.Instance.GetValidForXRNode(this.vrTargetNode))
			{
				this.myInputDevice = InputDevices.GetDeviceAtXRNode(this.vrTargetNode);
				this.hasInputDevice = true;
				if (this.vrTargetNode != XRNode.LeftHand && this.vrTargetNode != XRNode.RightHand)
				{
					this.hasInputDevice = this.myInputDevice.isValid;
				}
			}
			Quaternion quaternion3;
			Vector3 vector3;
			if (this.hasInputDevice && this.myInputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out quaternion3) && this.myInputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out vector3))
			{
				this.rigTarget.SetPositionAndRotation(vector3 + quaternion * this.trackingPositionOffset * ratio + playerOffsetTransform.position, quaternion3 * Quaternion.Euler(this.trackingRotationOffset));
				this.rigTarget.RotateAround(playerOffsetTransform.position, Vector3.up, playerOffsetTransform.eulerAngles.y);
			}
		}
		if (this.handholdOverrideTarget != null)
		{
			this.rigTarget.position = Vector3.MoveTowards(vector, this.handholdOverrideTarget.position - this.handholdOverrideTargetOffset + quaternion * this.trackingPositionOffset * ratio, Time.deltaTime * 2f);
		}
	}

	public Vector3 GetExtrapolatedControllerPosition()
	{
		Vector3 vector;
		Quaternion quaternion;
		this.rigTarget.GetPositionAndRotation(out vector, out quaternion);
		return vector - quaternion * this.trackingPositionOffset * this.rigTarget.lossyScale.x;
	}

	public virtual void MapOtherFinger(float handSync, float lerpValue)
	{
		this.calcT = handSync;
		this.LerpFinger(lerpValue, true);
	}

	public virtual void MapMyFinger(float lerpValue)
	{
	}

	public virtual void LerpFinger(float lerpValue, bool isOther)
	{
	}

	public XRNode vrTargetNode;

	public Transform overrideTarget;

	public Transform rigTarget;

	public Vector3 trackingPositionOffset;

	public Vector3 trackingRotationOffset;

	public Transform headTransform;

	internal NetworkVector3 netSyncPos = new NetworkVector3();

	public Quaternion syncRotation;

	public float calcT;

	private InputDevice myInputDevice;

	private bool hasInputDevice;

	public Transform handholdOverrideTarget;

	public Vector3 handholdOverrideTargetOffset;
}
