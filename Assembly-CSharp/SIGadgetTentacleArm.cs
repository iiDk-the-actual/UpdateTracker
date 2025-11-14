using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;

public class SIGadgetTentacleArm : SIGadget
{
	public bool isAnchored { get; private set; }

	private void Awake()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Combine(gameEntity2.OnSnapped, new Action(this.OnSnapped));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this.OnReleased));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this.OnUnsnapped));
		this.gameEntity.OnStateChanged += this.OnEntityStateChanged;
	}

	private void OnGrabbed()
	{
		this.isLeftHanded = this.gameEntity.heldByHandIndex == 0;
	}

	private void OnSnapped()
	{
		this.isLeftHanded = this.gameEntity.snappedJoint == SnapJointType.ArmL;
	}

	private void OnReleased()
	{
		this.ClearClawAnchor();
	}

	private void OnUnsnapped()
	{
	}

	protected override void OnUpdateAuthority(float dt)
	{
		Vector3 position = GTPlayer.Instance.headCollider.transform.position;
		Vector3 position2 = GTPlayer.Instance.bodyCollider.transform.position;
		Vector3 position3 = base.transform.position;
		if (!this.isLeftHanded)
		{
			GTPlayer.HandState rightHand = GTPlayer.Instance.RightHand;
		}
		else
		{
			GTPlayer.HandState leftHand = GTPlayer.Instance.LeftHand;
		}
		Vector3 vector = position3 - position2;
		Component controllerTransform = (this.isLeftHanded ? GTPlayer.Instance.LeftHand : GTPlayer.Instance.RightHand).controllerTransform;
		float num = (this.isLeftHanded ? ControllerInputPoller.instance.leftControllerIndexFloat : ControllerInputPoller.instance.rightControllerIndexFloat);
		bool flag = num >= 0.9f;
		if (this.isGripBroken)
		{
			if (flag)
			{
				num = 0f;
				flag = false;
			}
			else
			{
				this.isGripBroken = false;
			}
		}
		Vector3 vector2 = position3 + vector;
		Quaternion quaternion = controllerTransform.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
		if ((this.knownSafePosition - vector2).IsLongerThan(3f))
		{
			this.knownSafePosition = position3;
		}
		float num2 = 0.15f;
		this.claw.transform.rotation = base.transform.rotation;
		RaycastHit raycastHit;
		bool flag2 = Physics.SphereCast(new Ray(this.knownSafePosition, vector2 - this.knownSafePosition), num2, out raycastHit, (vector2 - this.knownSafePosition).magnitude, this.worldCollisionLayers);
		if (flag2)
		{
			float magnitude = (raycastHit.point - vector2).magnitude;
		}
		if (this.isAnchored)
		{
			if (flag)
			{
				Vector3 position4 = GTPlayer.Instance.transform.position;
				this.clawHoldAdjustment -= position4 - this.lastRequestedPlayerPosition;
				Vector3 vector3 = this.clawAnchorPosition - (vector2 + this.clawHoldAdjustment);
				GTPlayer.Instance.RequestTentacleMove(this.isLeftHanded, vector3);
				this.lastRequestedPlayerPosition = position4 + vector3;
				if ((this.clawAnchorPosition - base.transform.position).IsLongerThan(this.maxTentacleLength))
				{
					this.isGripBroken = true;
					this.ClearClawAnchor();
					return;
				}
				this.claw.transform.position = this.clawAnchorPosition;
				this.claw.transform.rotation = this.clawRotationOnGrab;
				return;
			}
			else
			{
				this.ClearClawAnchor();
			}
		}
		Vector3 vector4 = vector2;
		Quaternion quaternion2 = quaternion;
		if (flag2)
		{
			this.knownSafePosition += (vector2 - this.knownSafePosition).normalized * (raycastHit.distance - num2 * 2.01f);
			this.marker.transform.position = raycastHit.point;
			this.marker.transform.rotation = Quaternion.LookRotation(-raycastHit.normal, quaternion * Vector3.up);
			vector4 = raycastHit.point + raycastHit.normal * Mathf.Lerp(0.1f, 0.01f, num);
			quaternion2 = Quaternion.Lerp(quaternion, Quaternion.LookRotation(-raycastHit.normal, quaternion * Vector3.up), num * 0.5f + 0.5f);
		}
		else
		{
			this.knownSafePosition = vector2;
		}
		this.claw.transform.position = vector4;
		this.claw.transform.rotation = quaternion2;
		if (!this.isAnchored && flag && flag2)
		{
			this.SetClawAnchor(vector4, quaternion2, vector4 - vector2);
		}
	}

	private void SetClawAnchor(Vector3 clawPosition, Quaternion clawRotation, Vector3 adjustment)
	{
		this.isAnchored = true;
		this.clawHoldAdjustment = adjustment;
		this.clawAnchorPosition = clawPosition;
		this.clawRotationOnGrab = clawRotation;
		this.lastRequestedPlayerPosition = GTPlayer.Instance.transform.position;
		GTPlayer.Instance.SetGravityOverride(this, new Action<GTPlayer>(this.GravityOverrideFunction));
	}

	private void ClearClawAnchor()
	{
		this.isAnchored = false;
		GTPlayer.Instance.SetVelocity(GTPlayer.Instance.AveragedVelocity);
		GTPlayer.Instance.UnsetGravityOverride(this);
	}

	private void GravityOverrideFunction(GTPlayer player)
	{
	}

	protected override void OnUpdateRemote(float dt)
	{
	}

	private void OnEntityStateChanged(long oldState, long newState)
	{
	}

	[SerializeField]
	private GameObject claw;

	[SerializeField]
	private LayerMask worldCollisionLayers;

	[SerializeField]
	private Transform marker;

	[SerializeField]
	private float maxTentacleLength;

	private bool isLeftHanded;

	private Vector3 knownSafePosition;

	private Vector3 clawHoldAdjustment;

	private Vector3 clawAnchorPosition;

	private Vector3 lastRequestedPlayerPosition;

	private Quaternion clawRotationOnGrab;

	private bool isGripBroken;
}
