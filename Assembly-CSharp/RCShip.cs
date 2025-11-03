using System;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

public class RCShip : RCHoverboard
{
	private byte GetDataB()
	{
		if (!this.hasNetworkSync)
		{
			return 0;
		}
		return this.networkSync.syncedState.dataB;
	}

	private void SetDataB(byte b)
	{
		if (this.hasNetworkSync)
		{
			this.networkSync.syncedState.dataB = b;
		}
	}

	private void WriteCannonBit(bool toLeft)
	{
		if (!this.hasNetworkSync)
		{
			return;
		}
		byte b = this.GetDataB();
		b = (toLeft ? (b | 1) : ((byte)((int)b & -2)));
		this.SetDataB(b);
	}

	private bool ReadCannonBit()
	{
		if (!this.hasNetworkSync)
		{
			return this.cannonToLeft;
		}
		return (this.GetDataB() & 1) > 0;
	}

	private bool ReadFireFlip()
	{
		return (this.GetDataB() & 2) > 0;
	}

	protected override void AuthorityUpdate(float dt)
	{
		base.AuthorityUpdate(dt);
		float trigger = this.activeInput.trigger;
		float num = (float)this.activeInput.buttons;
		if (this.localState == RCVehicle.State.Mobilized && this.localStatePrev != RCVehicle.State.Mobilized)
		{
			this.armedAfterMobilize = false;
			if (trigger >= this.triggerReleaseThreshold)
			{
				this.triggerIsDown = true;
			}
		}
		if (this.localState == RCVehicle.State.Mobilized)
		{
			if (!this.armedAfterMobilize && trigger <= this.triggerReleaseThreshold)
			{
				this.armedAfterMobilize = true;
				this.triggerIsDown = false;
			}
			if (this.armedAfterMobilize)
			{
				if (!this.triggerIsDown && trigger >= this.triggerPressThreshold)
				{
					this.triggerIsDown = true;
					UnityEvent onFire = this.OnFire;
					if (onFire != null)
					{
						onFire.Invoke();
					}
					if (this.hasNetworkSync)
					{
						byte b = this.GetDataB();
						b ^= 2;
						this.SetDataB(b);
						this.lastFireFlip = (b & 2) > 0;
					}
				}
				else if (this.triggerIsDown && trigger <= this.triggerReleaseThreshold)
				{
					this.triggerIsDown = false;
				}
			}
			if (!this.faceIsDown && num >= this.facePressThreshold)
			{
				this.faceIsDown = true;
				this.cannonToLeft = !this.cannonToLeft;
				this.WriteCannonBit(this.cannonToLeft);
			}
			else if (this.faceIsDown && num <= this.faceReleaseThreshold)
			{
				this.faceIsDown = false;
			}
		}
		else
		{
			if (this.faceIsDown && num <= this.faceReleaseThreshold)
			{
				this.faceIsDown = false;
			}
			this.armedAfterMobilize = false;
			if (this.triggerIsDown && trigger <= this.triggerReleaseThreshold)
			{
				this.triggerIsDown = false;
			}
		}
		if (this.hasNetworkSync)
		{
			byte b2 = this.GetDataB();
			if (this.localState == RCVehicle.State.Mobilized && this.rb != null && this.rb.linearVelocity.sqrMagnitude >= this.movingSpeedThreshold * this.movingSpeedThreshold)
			{
				b2 |= 4;
				this.isMovingShared = true;
			}
			else
			{
				b2 = (byte)((int)b2 & -5);
				this.isMovingShared = false;
			}
			this.SetDataB(b2);
			return;
		}
		this.isMovingShared = this.localState == RCVehicle.State.Mobilized && this.rb != null && this.rb.linearVelocity.sqrMagnitude >= this.movingSpeedThreshold * this.movingSpeedThreshold;
	}

	protected override void RemoteUpdate(float dt)
	{
		base.RemoteUpdate(dt);
		if (!this.hasNetworkSync)
		{
			return;
		}
		this.cannonToLeft = this.ReadCannonBit();
		bool flag = this.ReadFireFlip();
		if (!base.HasLocalAuthority)
		{
			if (flag != this.lastFireFlip)
			{
				this.lastFireFlip = flag;
				UnityEvent onFire = this.OnFire;
				if (onFire != null)
				{
					onFire.Invoke();
				}
			}
			byte dataB = this.GetDataB();
			this.isMovingShared = (dataB & 4) > 0;
			return;
		}
		this.lastFireFlip = flag;
		this.isMovingShared = this.localState == RCVehicle.State.Mobilized && this.rb != null && this.rb.linearVelocity.sqrMagnitude >= this.movingSpeedThreshold * this.movingSpeedThreshold;
	}

	protected override void SharedUpdate(float dt)
	{
		base.SharedUpdate(dt);
		if (this.cannonTransform != null)
		{
			float num = (this.cannonToLeft ? this.leftYaw : this.rightYaw);
			Vector3 localEulerAngles = this.cannonTransform.localEulerAngles;
			localEulerAngles.z = Mathf.MoveTowardsAngle(localEulerAngles.z, num, this.cannonYawSpeed * dt);
			this.cannonTransform.localEulerAngles = localEulerAngles;
		}
		if (this.cannonToLeft != this.lastCannonToLeft)
		{
			this.lastCannonToLeft = this.cannonToLeft;
			UnityEvent<bool> onCannonSideChanged = this.OnCannonSideChanged;
			if (onCannonSideChanged != null)
			{
				onCannonSideChanged.Invoke(this.cannonToLeft);
			}
		}
		bool flag = this.localState == RCVehicle.State.Mobilized && this.isMovingShared;
		if (flag != this.lastIsMoving)
		{
			this.lastIsMoving = flag;
			if (flag)
			{
				UnityEvent onMoveStarted = this.OnMoveStarted;
				if (onMoveStarted == null)
				{
					return;
				}
				onMoveStarted.Invoke();
				return;
			}
			else
			{
				UnityEvent onMoveStopped = this.OnMoveStopped;
				if (onMoveStopped == null)
				{
					return;
				}
				onMoveStopped.Invoke();
			}
		}
	}

	[Header("RCShip - Events")]
	public UnityEvent OnFire;

	public UnityEvent<bool> OnCannonSideChanged;

	public UnityEvent OnMoveStarted;

	public UnityEvent OnMoveStopped;

	[Header("RCShip - Cannon Rotation")]
	[SerializeField]
	private Transform cannonTransform;

	[SerializeField]
	private float leftYaw = -45f;

	[SerializeField]
	private float rightYaw = 45f;

	[SerializeField]
	private float cannonYawSpeed = 240f;

	[Header("RCShip - Input")]
	[Range(0f, 1f)]
	[SerializeField]
	private float triggerPressThreshold = 0.6f;

	[Range(0f, 1f)]
	[SerializeField]
	private float triggerReleaseThreshold = 0.1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float facePressThreshold = 0.6f;

	[Range(0f, 1f)]
	[SerializeField]
	private float faceReleaseThreshold = 0.1f;

	[Header("RCShip - Movement Detection")]
	[Tooltip("Minimum speed to consider the ship moving")]
	[SerializeField]
	private float movingSpeedThreshold = 0.05f;

	private bool prevTriggerDown;

	private bool prevFaceDown;

	private bool faceIsDown;

	private bool triggerIsDown;

	private bool armedAfterMobilize;

	private bool cannonToLeft;

	private const byte CannonLeftBit = 1;

	private const byte FireFlipBit = 2;

	private const byte MovingBit = 4;

	private bool lastFireFlip;

	private bool lastCannonToLeft;

	private bool lastIsMoving;

	private bool isMovingShared;
}
