using System;
using UnityEngine;

public class GorillaIK : MonoBehaviour
{
	private void Awake()
	{
		if (Application.isPlaying && !this.testInEditor)
		{
			this.dU = (this.leftUpperArm.position - this.leftLowerArm.position).magnitude;
			this.dL = (this.leftLowerArm.position - this.leftHand.position).magnitude;
			this.dMax = this.dU + this.dL - this.eps;
			this.initialUpperLeft = this.leftUpperArm.localRotation;
			this.initialLowerLeft = this.leftLowerArm.localRotation;
			this.initialUpperRight = this.rightUpperArm.localRotation;
			this.initialLowerRight = this.rightLowerArm.localRotation;
		}
	}

	private void OnEnable()
	{
		GorillaIKMgr.Instance.RegisterIK(this);
	}

	private void OnDisable()
	{
		GorillaIKMgr.Instance.DeregisterIK(this);
	}

	public void OverrideTargetPos(bool isLeftHand, Vector3 targetWorldPos)
	{
		if (isLeftHand)
		{
			this.hasLeftOverride = true;
			this.leftOverrideWorldPos = targetWorldPos;
			return;
		}
		this.hasRightOverride = true;
		this.rightOverrideWorldPos = targetWorldPos;
	}

	public Vector3 GetShoulderLocalTargetPos_Left()
	{
		return this.leftUpperArm.parent.InverseTransformPoint(this.hasLeftOverride ? this.leftOverrideWorldPos : this.targetLeft.position);
	}

	public Vector3 GetShoulderLocalTargetPos_Right()
	{
		return this.rightUpperArm.parent.InverseTransformPoint(this.hasRightOverride ? this.rightOverrideWorldPos : this.targetRight.position);
	}

	public void ClearOverrides()
	{
		this.hasLeftOverride = false;
		this.hasRightOverride = false;
	}

	private void ArmIK(ref Transform upperArm, ref Transform lowerArm, ref Transform hand, Quaternion initRotUpper, Quaternion initRotLower, Transform target)
	{
		upperArm.localRotation = initRotUpper;
		lowerArm.localRotation = initRotLower;
		float num = Mathf.Clamp((target.position - upperArm.position).magnitude, this.eps, this.dMax);
		float num2 = Mathf.Acos(Mathf.Clamp(Vector3.Dot((hand.position - upperArm.position).normalized, (lowerArm.position - upperArm.position).normalized), -1f, 1f));
		float num3 = Mathf.Acos(Mathf.Clamp(Vector3.Dot((upperArm.position - lowerArm.position).normalized, (hand.position - lowerArm.position).normalized), -1f, 1f));
		float num4 = Mathf.Acos(Mathf.Clamp(Vector3.Dot((hand.position - upperArm.position).normalized, (target.position - upperArm.position).normalized), -1f, 1f));
		float num5 = Mathf.Acos(Mathf.Clamp((this.dL * this.dL - this.dU * this.dU - num * num) / (-2f * this.dU * num), -1f, 1f));
		float num6 = Mathf.Acos(Mathf.Clamp((num * num - this.dU * this.dU - this.dL * this.dL) / (-2f * this.dU * this.dL), -1f, 1f));
		Vector3 normalized = Vector3.Cross(hand.position - upperArm.position, lowerArm.position - upperArm.position).normalized;
		Vector3 normalized2 = Vector3.Cross(hand.position - upperArm.position, target.position - upperArm.position).normalized;
		Quaternion quaternion = Quaternion.AngleAxis((num5 - num2) * 57.29578f, Quaternion.Inverse(upperArm.rotation) * normalized);
		Quaternion quaternion2 = Quaternion.AngleAxis((num6 - num3) * 57.29578f, Quaternion.Inverse(lowerArm.rotation) * normalized);
		Quaternion quaternion3 = Quaternion.AngleAxis(num4 * 57.29578f, Quaternion.Inverse(upperArm.rotation) * normalized2);
		this.newRotationUpper = upperArm.localRotation * quaternion3 * quaternion;
		this.newRotationLower = lowerArm.localRotation * quaternion2;
		upperArm.localRotation = this.newRotationUpper;
		lowerArm.localRotation = this.newRotationLower;
		hand.rotation = target.rotation;
	}

	public Transform headBone;

	public Transform leftUpperArm;

	public Transform leftLowerArm;

	public Transform leftHand;

	public Transform rightUpperArm;

	public Transform rightLowerArm;

	public Transform rightHand;

	public Transform targetLeft;

	public Transform targetRight;

	public Transform targetHead;

	public Quaternion initialUpperLeft;

	public Quaternion initialLowerLeft;

	public Quaternion initialUpperRight;

	public Quaternion initialLowerRight;

	public Quaternion newRotationUpper;

	public Quaternion newRotationLower;

	public float dU;

	public float dL;

	public float dMax;

	public bool testInEditor;

	public bool reset;

	public bool testDefineRot;

	public bool moveOnce;

	public float eps;

	public float upperArmAngle;

	public float elbowAngle;

	private bool hasLeftOverride;

	private Vector3 leftOverrideWorldPos;

	private bool hasRightOverride;

	private Vector3 rightOverrideWorldPos;
}
