using System;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;

public class SIGadgetLaserZipline : SIGadget
{
	private void Awake()
	{
		this.m_buttonActivatable = base.GetComponent<GameButtonActivatable>();
		this.laserBeam.SetActive(false);
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
	}

	private void OnSnapped()
	{
	}

	private void OnReleased()
	{
	}

	private void OnUnsnapped()
	{
	}

	protected override void OnUpdateAuthority(float dt)
	{
		bool flag = this.m_buttonActivatable.CheckInput(true, true, 0.25f, true);
		if (this.coolingDownUntilNextTouchGround && (GTPlayer.Instance.IsGroundedHand || GTPlayer.Instance.IsGroundedButt))
		{
			this.coolingDownUntilNextTouchGround = false;
		}
		if (flag)
		{
			if (this.isLineBroken)
			{
				return;
			}
			if (!this.wasActive)
			{
				if (Time.time < this.coolingDownUntilTimestamp || this.coolingDownUntilNextTouchGround)
				{
					this.isLineBroken = true;
					return;
				}
				this.laserBeam.SetActive(true);
				this.activatedAtRotation = this.zipline.transform.rotation;
				this.activatedAtPoint = this.zipline.transform.position;
				this.ziplineDirection = this.zipline.transform.forward;
				if (this.ziplineDirection.y > 0f)
				{
					this.ziplineDirection = -this.ziplineDirection;
				}
			}
			else
			{
				this.zipline.transform.rotation = this.activatedAtRotation;
				Vector3 vector = this.activatedAtPoint - this.zipline.transform.position;
				vector = vector.ProjectOnPlane(Vector3.zero, this.ziplineDirection);
				if (vector.sqrMagnitude > 1f)
				{
					this.isLineBroken = true;
					this.laserBeam.SetActive(false);
					return;
				}
				GTPlayer.Instance.transform.position += vector;
			}
			float magnitude = GTPlayer.Instance.RigidbodyVelocity.magnitude;
			float num = Mathf.Lerp(Vector3.Dot(GTPlayer.Instance.RigidbodyVelocity, this.ziplineDirection), magnitude, 0.5f) - this.speedBoost * this.ziplineDirection.y * Time.deltaTime;
			GTPlayer.Instance.SetVelocity(this.ziplineDirection * num);
			this.wasActive = true;
			return;
		}
		else
		{
			if (this.wasActive)
			{
				this.laserBeam.SetActive(false);
				this.zipline.transform.localRotation = Quaternion.identity;
				this.isLineBroken = false;
				this.wasActive = false;
				this.coolingDownUntilTimestamp = Time.time + this.cooldownDuration;
				this.coolingDownUntilNextTouchGround = this.cooldownOnUseUntilTouchGround;
				GTPlayer.Instance.SetVelocity(GTPlayer.Instance.AveragedVelocity);
				return;
			}
			this.isLineBroken = false;
			return;
		}
	}

	protected override void OnUpdateRemote(float dt)
	{
	}

	private void OnEntityStateChanged(long oldState, long newState)
	{
	}

	[SerializeField]
	private GameButtonActivatable m_buttonActivatable;

	[SerializeField]
	private Transform zipline;

	[SerializeField]
	private GameObject laserBeam;

	[SerializeField]
	private float speedBoost;

	[SerializeField]
	private float cooldownDuration;

	[SerializeField]
	private bool cooldownOnUseUntilTouchGround;

	private bool wasActive;

	private bool isLineBroken;

	private Quaternion activatedAtRotation;

	private Vector3 activatedAtPoint;

	private Vector3 ziplineDirection;

	private float coolingDownUntilTimestamp;

	private bool coolingDownUntilNextTouchGround;
}
