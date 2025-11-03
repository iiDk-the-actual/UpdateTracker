using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;

public class TransferrableObjectHoldablePart_Crank : TransferrableObjectHoldablePart
{
	public void SetOnCrankedCallback(Action<float> onCrankedCallback)
	{
		this.onCrankedCallback = onCrankedCallback;
	}

	private void Awake()
	{
		if (this.rotatingPart == null)
		{
			this.rotatingPart = base.transform;
		}
		Vector3 vector = this.rotatingPart.parent.InverseTransformPoint(this.rotatingPart.TransformPoint(Vector3.right));
		this.lastAngle = Mathf.Atan2(vector.y, vector.x);
		this.baseLocalAngle = this.rotatingPart.localRotation;
		this.baseLocalAngleInverse = Quaternion.Inverse(this.baseLocalAngle);
		this.crankRadius = new Vector2(this.crankHandleX, this.crankHandleY).magnitude;
		this.crankAngleOffset = Mathf.Atan2(this.crankHandleY, this.crankHandleX) * 57.29578f;
		if (this.crankHandleMaxZ < this.crankHandleMinZ)
		{
			float num = this.crankHandleMaxZ;
			float num2 = this.crankHandleMinZ;
			this.crankHandleMinZ = num;
			this.crankHandleMaxZ = num2;
		}
	}

	protected override void UpdateHeld(VRRig rig, bool isHeldLeftHand)
	{
		Vector3 vector4;
		if (rig.isOfflineVRRig)
		{
			Transform controllerTransform = GTPlayer.Instance.GetControllerTransform(isHeldLeftHand);
			Vector3 vector = this.rotatingPart.InverseTransformPoint(controllerTransform.position);
			Vector3 vector2 = (vector.xy().normalized * this.crankRadius).WithZ(Mathf.Clamp(vector.z, this.crankHandleMinZ, this.crankHandleMaxZ));
			Vector3 vector3 = this.rotatingPart.TransformPoint(vector2);
			if (this.maxHandSnapDistance > 0f && (controllerTransform.position - vector3).IsLongerThan(this.maxHandSnapDistance))
			{
				this.OnRelease(null, isHeldLeftHand ? EquipmentInteractor.instance.leftHand : EquipmentInteractor.instance.rightHand);
				return;
			}
			controllerTransform.position = vector3;
			vector4 = controllerTransform.position;
		}
		else
		{
			VRMap vrmap = (isHeldLeftHand ? rig.leftHand : rig.rightHand);
			vector4 = vrmap.GetExtrapolatedControllerPosition();
			vector4 -= vrmap.rigTarget.rotation * GTPlayer.Instance.GetHandOffset(isHeldLeftHand) * rig.scaleFactor;
		}
		Vector3 vector5 = this.baseLocalAngleInverse * Quaternion.Inverse(this.rotatingPart.parent.rotation) * (vector4 - this.rotatingPart.position);
		float num = Mathf.Atan2(vector5.y, vector5.x) * 57.29578f;
		float num2 = Mathf.DeltaAngle(this.lastAngle, num);
		this.lastAngle = num;
		if (num2 != 0f)
		{
			if (this.onCrankedCallback != null)
			{
				this.onCrankedCallback(num2);
			}
			for (int i = 0; i < this.thresholds.Length; i++)
			{
				this.thresholds[i].OnCranked(num2);
			}
		}
		this.rotatingPart.localRotation = this.baseLocalAngle * Quaternion.AngleAxis(num - this.crankAngleOffset, Vector3.forward);
	}

	private void OnDrawGizmosSelected()
	{
		Transform transform = ((this.rotatingPart != null) ? this.rotatingPart : base.transform);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.TransformPoint(new Vector3(this.crankHandleX, this.crankHandleY, this.crankHandleMinZ)), transform.TransformPoint(new Vector3(this.crankHandleX, this.crankHandleY, this.crankHandleMaxZ)));
	}

	[SerializeField]
	private float crankHandleX;

	[SerializeField]
	private float crankHandleY;

	[SerializeField]
	private float crankHandleMinZ;

	[SerializeField]
	private float crankHandleMaxZ;

	[SerializeField]
	private float maxHandSnapDistance;

	private float crankAngleOffset;

	private float crankRadius;

	[SerializeField]
	private Transform rotatingPart;

	private float lastAngle;

	private Quaternion baseLocalAngle;

	private Quaternion baseLocalAngleInverse;

	private Action<float> onCrankedCallback;

	[SerializeField]
	private TransferrableObjectHoldablePart_Crank.CrankThreshold[] thresholds;

	[Serializable]
	private struct CrankThreshold
	{
		public void OnCranked(float deltaAngle)
		{
			this.currentAngle += deltaAngle;
			if (Mathf.Abs(this.currentAngle) > this.angleThreshold)
			{
				this.currentAngle = 0f;
				this.onReached.Invoke();
			}
		}

		public float angleThreshold;

		public UnityEvent onReached;

		[HideInInspector]
		public float currentAngle;
	}
}
