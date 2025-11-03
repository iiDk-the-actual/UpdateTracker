using System;
using UnityEngine;
using UnityEngine.XR;

public class FishingRod : TransferrableObject
{
	public override void OnActivate()
	{
		base.OnActivate();
		Transform transform = base.transform;
		Vector3 vector = transform.up + transform.forward * 640f;
		this.bobRigidbody.AddForce(vector, ForceMode.Impulse);
		this.line.tensionScale = 0.86f;
		this.ReelOut();
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		this.line.tensionScale = 1f;
		this.ReelStop();
	}

	protected override void Start()
	{
		base.Start();
		this.rig = base.GetComponentInParent<VRRig>();
	}

	public void SetBobFloat(bool enable)
	{
		if (!this.bobRigidbody)
		{
			return;
		}
		this._bobFloatPlaneY = this.bobRigidbody.position.y;
		this._bobFloating = enable;
	}

	private void QuickReel()
	{
		if (this._lineResizing)
		{
			return;
		}
		this.bobCollider.enabled = false;
		this.ReelIn();
	}

	public bool IsFreeHandGripping()
	{
		bool flag = base.InLeftHand();
		Transform transform = (flag ? this.rig.rightHandTransform : this.rig.leftHandTransform);
		float magnitude = (this.reelToSync.position - transform.position).magnitude;
		bool flag2 = this._grippingHand || magnitude <= 0.16f;
		this.disableStealing = flag2;
		if (!flag2)
		{
			return false;
		}
		VRMapThumb vrmapThumb = (flag ? this.rig.rightThumb : this.rig.leftThumb);
		VRMapIndex vrmapIndex = (flag ? this.rig.rightIndex : this.rig.leftIndex);
		VRMap vrmap = (flag ? this.rig.rightMiddle : this.rig.leftMiddle);
		float calcT = vrmapThumb.calcT;
		float calcT2 = vrmapIndex.calcT;
		float calcT3 = vrmap.calcT;
		bool flag3 = calcT >= 0.1f && calcT2 >= 0.2f && calcT3 >= 0.2f;
		this._grippingHand = (flag3 ? transform : null);
		return flag3;
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		if (this._grippingHand)
		{
			this._grippingHand = null;
		}
		this.ResetLineLength(this.lineLengthMin * 1.32f);
		return true;
	}

	public void ReelIn()
	{
		this._manualReeling = false;
		FishingRod.SetHandleMotorUse(true, this.reelSpinRate, this.handleJoint, true);
		this._lineResizing = true;
		this._lineExpanding = false;
		float num = (float)this.line.segmentNumber + 0.0001f;
		this.line.segmentMinLength = (this._targetSegmentMin = this.lineLengthMin / num);
		this.line.segmentMaxLength = (this._targetSegmentMax = this.lineLengthMax / num);
	}

	public void ReelOut()
	{
		this._manualReeling = false;
		FishingRod.SetHandleMotorUse(true, this.reelSpinRate, this.handleJoint, false);
		this._lineResizing = true;
		this._lineExpanding = true;
		float num = (float)this.line.segmentNumber + 0.0001f;
		this.line.segmentMinLength = (this._targetSegmentMin = this.lineLengthMin / num);
		this.line.segmentMaxLength = (this._targetSegmentMax = this.lineLengthMax / num);
	}

	public void ReelStop()
	{
		if (this._manualReeling)
		{
			this._localRotDelta = 0f;
		}
		else
		{
			FishingRod.SetHandleMotorUse(false, 0f, this.handleJoint, false);
		}
		this.bobCollider.enabled = true;
		if (this.line)
		{
			this.line.resizeScale = 1f;
		}
		this._lineResizing = false;
		this._lineExpanding = false;
	}

	private static void SetHandleMotorUse(bool useMotor, float spinRate, HingeJoint handleJoint, bool reverse)
	{
		JointMotor motor = handleJoint.motor;
		motor.force = (useMotor ? 1f : 0f) * spinRate;
		motor.targetVelocity = 16384f * (reverse ? (-1f) : 1f);
		handleJoint.motor = motor;
	}

	public override void TriggeredLateUpdate()
	{
		base.TriggeredLateUpdate();
		this._manualReeling = (this._isGrippingHandle = this.IsFreeHandGripping());
		if (ControllerInputPoller.instance && ControllerInputPoller.PrimaryButtonPress(base.InLeftHand() ? XRNode.LeftHand : XRNode.RightHand))
		{
			this.QuickReel();
		}
		if (this._lineResetting && this._sinceReset.HasElapsed(this.line.resizeSpeed))
		{
			this.bobCollider.enabled = true;
			this._lineResetting = false;
		}
		this.handleTransform.localPosition = this.reelFreezeLocalPosition;
	}

	private void ResetLineLength(float length)
	{
		if (!this.line)
		{
			return;
		}
		this._lineResetting = true;
		this.bobCollider.enabled = false;
		this.line.ForceTotalLength(length);
		this._sinceReset = TimeSince.Now();
	}

	private void FixedUpdate()
	{
		Transform transform = base.transform;
		this.handleRigidbody.useGravity = !this._manualReeling;
		if (this._bobFloating && this.bobRigidbody)
		{
			float y = this.bobRigidbody.position.y;
			float num = this.bobFloatForce * this.bobRigidbody.mass;
			float num2 = num * Mathf.Clamp01(this._bobFloatPlaneY - y);
			num += num2;
			if (y <= this._bobFloatPlaneY)
			{
				this.bobRigidbody.AddForce(0f, num, 0f);
			}
		}
		if (this._manualReeling)
		{
			if (this._isGrippingHandle && this._grippingHand)
			{
				this.reelTo.position = this._grippingHand.position;
			}
			Vector3 vector = this.reelFrom.InverseTransformPoint(this.reelTo.position);
			vector.x = 0f;
			vector.Normalize();
			vector *= 2f;
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, vector);
			quaternion = (base.InRightHand() ? quaternion : Quaternion.Inverse(quaternion));
			this._localRotDelta = FishingRod.GetSignedDeltaYZ(ref this._lastLocalRot, ref quaternion);
			this._lastLocalRot = quaternion;
			Quaternion quaternion2 = transform.rotation * quaternion;
			this.handleRigidbody.MoveRotation(quaternion2);
		}
		else
		{
			this.reelTo.localPosition = transform.InverseTransformPoint(this.reelToSync.position);
		}
		if (!this.line)
		{
			return;
		}
		if (this._manualReeling)
		{
			this._lineResizing = Mathf.Abs(this._localRotDelta) >= 0.001f;
			this._lineExpanding = Mathf.Sign(this._localRotDelta) >= 0f;
		}
		if (!this._lineResizing)
		{
			return;
		}
		float num3 = (this._manualReeling ? (Mathf.Abs(this._localRotDelta) * 0.66f * Time.fixedDeltaTime) : (this.lineResizeRate * this.lineCastFactor));
		this.line.resizeScale = this.lineCastFactor;
		float num4 = num3 * Time.fixedDeltaTime;
		float num5 = this.line.segmentTargetLength;
		if (this._manualReeling)
		{
			float num6 = 1f / ((float)this.line.segmentNumber + 0.0001f);
			float num7 = this.lineLengthMin * num6;
			float num8 = this.lineLengthMax * num6;
			num4 *= (this._lineExpanding ? 1f : (-1f));
			num4 *= (base.InRightHand() ? (-1f) : 1f);
			float num9 = num5 + num4;
			if (num9 > num7 && num9 < num8)
			{
				num5 += num4;
			}
		}
		else if (this._lineExpanding)
		{
			if (num5 < this._targetSegmentMax)
			{
				num5 += num4;
			}
			else
			{
				this._lineResizing = false;
			}
		}
		else if (num5 > this._targetSegmentMin)
		{
			num5 -= num4;
		}
		else
		{
			this._lineResizing = false;
		}
		if (this._lineResizing)
		{
			this.line.segmentTargetLength = num5;
			return;
		}
		this.ReelStop();
	}

	private static float GetSignedDeltaYZ(ref Quaternion a, ref Quaternion b)
	{
		Vector3 forward = Vector3.forward;
		Vector3 vector = a * forward;
		Vector3 vector2 = b * forward;
		float num = Mathf.Atan2(vector.y, vector.z) * 57.29578f;
		float num2 = Mathf.Atan2(vector2.y, vector2.z) * 57.29578f;
		return Mathf.DeltaAngle(num, num2);
	}

	public Transform handleTransform;

	public HingeJoint handleJoint;

	public Rigidbody handleRigidbody;

	public BoxCollider handleCollider;

	public Rigidbody bobRigidbody;

	public Collider bobCollider;

	public VerletLine line;

	public GorillaVelocityEstimator tipTracker;

	public Rigidbody tipBody;

	[NonSerialized]
	public VRRig rig;

	[Space]
	public Vector3 reelFreezeLocalPosition;

	public Transform reelFrom;

	public Transform reelTo;

	public Transform reelToSync;

	[Space]
	public float reelSpinRate = 1f;

	public float lineResizeRate = 1f;

	public float lineCastFactor = 3f;

	public float lineLengthMin = 0.1f;

	public float lineLengthMax = 8f;

	[Space]
	[NonSerialized]
	private bool _bobFloating;

	public float bobFloatForce = 8f;

	public float bobStaticDrag = 3.2f;

	public float bobDynamicDrag = 1.1f;

	[NonSerialized]
	private float _bobFloatPlaneY;

	[Space]
	[NonSerialized]
	private float _targetSegmentMin;

	[NonSerialized]
	private float _targetSegmentMax;

	[Space]
	[NonSerialized]
	private bool _manualReeling;

	[NonSerialized]
	private bool _lineResizing;

	[NonSerialized]
	private bool _lineExpanding;

	[NonSerialized]
	private bool _lineResetting;

	[NonSerialized]
	private TimeSince _sinceReset;

	[Space]
	[NonSerialized]
	private Quaternion _lastLocalRot = Quaternion.identity;

	[NonSerialized]
	private float _localRotDelta;

	[NonSerialized]
	private bool _isGrippingHandle;

	[NonSerialized]
	private Transform _grippingHand;

	private TimeSince _sinceGripLoss;
}
