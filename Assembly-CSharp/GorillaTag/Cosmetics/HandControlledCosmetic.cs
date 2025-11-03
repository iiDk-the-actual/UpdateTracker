using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class HandControlledCosmetic : MonoBehaviour, ITickSystemTick
	{
		public void Awake()
		{
			this.myRig = base.GetComponentInParent<VRRig>();
			this.initialRotation = base.transform.localRotation;
			base.enabled = false;
			if (this.debugRelativePositionTransform1 != null)
			{
				Object.Destroy(this.debugRelativePositionTransform1.gameObject);
			}
			if (this.debugRelativePositionTransform2 != null)
			{
				Object.Destroy(this.debugRelativePositionTransform2.gameObject);
			}
		}

		private void SetControlIndicatorPoints()
		{
			if (this.myRig.isOfflineVRRig && this.controllingHand != null && this.controlIndicatorCurve != null && this.controlIndicatorCurve.points != null)
			{
				this.controlIndicatorCurve.points[0] = this.controllingHand.position;
				this.controlIndicatorCurve.points[1] = this.controlIndicatorCurve.points[0] + this.myRig.scaleFactor * this.controllingHand.up;
				this.controlIndicatorCurve.points[2] = base.transform.position;
			}
		}

		private Vector3 GetRelativeHandPosition()
		{
			return this.controllingHand.TransformPoint(this.handPositionOffset) - this.myRig.bodyTransform.position;
		}

		public void StartControl(bool leftHand, float flexValue)
		{
			if (!base.enabled || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.lowAngleLimits = this.activeSettings.angleLimits;
			this.highAngleLimits = 360f * Vector3.one - this.lowAngleLimits;
			this.handRotationOffset = (leftHand ? this.leftHandRotation : this.rightHandRotation);
			this.controllingHand = (leftHand ? this.myRig.leftHand.rigTarget.transform : this.myRig.rightHand.rigTarget.transform);
			this.startHandRelativePosition = this.GetRelativeHandPosition();
			this.startHandInverseRotation = Quaternion.Inverse(this.controllingHand.rotation * this.handRotationOffset);
			this.isActive = true;
			this.SetControlIndicatorPoints();
			TickSystem<object>.AddTickCallback(this);
		}

		public void StopControl()
		{
			this.localEuler = base.transform.localRotation.eulerAngles;
			this.isActive = false;
			this.SetControlIndicatorPoints();
		}

		public void OnEnable()
		{
		}

		public void OnDisable()
		{
			base.transform.localRotation = this.initialRotation;
			this.StopControl();
			TickSystem<object>.RemoveTickCallback(this);
		}

		private float ReverseClampDegrees(float value, float low, float high)
		{
			value = Mathf.Repeat(value, 360f);
			if (value <= low || value >= high)
			{
				return value;
			}
			if (value >= 180f)
			{
				return high;
			}
			return low;
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (this.isActive)
			{
				HandControlledCosmetic.RotationControl rotationControl = this.activeSettings.rotationControl;
				if (rotationControl != HandControlledCosmetic.RotationControl.Angle)
				{
					if (rotationControl == HandControlledCosmetic.RotationControl.Translation)
					{
						Vector3 relativeHandPosition = this.GetRelativeHandPosition();
						Vector3 vector = new Vector3(relativeHandPosition.x, 0f, relativeHandPosition.z);
						float num = Vector3.SignedAngle(new Vector3(this.startHandRelativePosition.x, 0f, this.startHandRelativePosition.z), vector, Vector3.up);
						float num2 = 50f * (this.startHandRelativePosition.y - relativeHandPosition.y) / this.myRig.scaleFactor;
						float num3 = Vector3.Distance(this.startHandRelativePosition, relativeHandPosition) / this.myRig.scaleFactor;
						this.localEuler += Time.deltaTime * new Vector3(this.activeSettings.verticalSensitivity.Evaluate(num3) * num2, this.activeSettings.horizontalSensitivity.Evaluate(num3) * num, 0f);
						this.startHandRelativePosition = Vector3.MoveTowards(this.startHandRelativePosition, relativeHandPosition, Time.deltaTime * this.activeSettings.inputDecayCurve.Evaluate(num3));
					}
				}
				else
				{
					Quaternion quaternion = this.controllingHand.rotation * this.handRotationOffset;
					Quaternion quaternion2 = this.startHandInverseRotation * quaternion;
					this.localEuler += this.activeSettings.inputSensitivity * quaternion2.eulerAngles;
					float num4 = 1f - Mathf.Exp(-this.activeSettings.inputDecaySpeed * Time.deltaTime);
					this.startHandInverseRotation = Quaternion.Slerp(this.startHandInverseRotation, Quaternion.Inverse(quaternion), num4);
				}
				for (int i = 0; i < 3; i++)
				{
					this.localEuler[i] = this.ReverseClampDegrees(this.localEuler[i], this.lowAngleLimits[i], this.highAngleLimits[i]);
				}
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, Quaternion.Euler(this.localEuler), 1f - Mathf.Exp(-this.activeSettings.rotationSpeed * Time.deltaTime));
				return;
			}
			Quaternion quaternion3 = Quaternion.Slerp(base.transform.localRotation, this.initialRotation, 1f - Mathf.Exp(-this.inactiveSettings.rotationSpeed * Time.deltaTime));
			base.transform.localRotation = quaternion3;
			this.localEuler = quaternion3.eulerAngles;
		}

		[SerializeField]
		private HandControlledSettingsSO activeSettings;

		[SerializeField]
		private HandControlledSettingsSO inactiveSettings;

		[SerializeField]
		private Vector3 handPositionOffset;

		[SerializeField]
		private Quaternion rightHandRotation;

		[SerializeField]
		private Quaternion leftHandRotation;

		private Quaternion handRotationOffset;

		[SerializeField]
		private BezierCurve controlIndicatorCurve;

		[SerializeField]
		private Transform debugRelativePositionTransform1;

		[SerializeField]
		private Transform debugRelativePositionTransform2;

		private VRRig myRig;

		private Transform controllingHand;

		private Vector3 startHandRelativePosition;

		private Vector3 lowAngleLimits;

		private Vector3 highAngleLimits;

		private Vector3 localEuler;

		private Quaternion startHandInverseRotation;

		private Quaternion initialRotation;

		private bool isActive;

		public enum RotationControl
		{
			Angle,
			Translation
		}
	}
}
