using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaTag.Cosmetics
{
	public class RCPlane : RCVehicle
	{
		protected override void Awake()
		{
			base.Awake();
			this.pitchAccelMinMax.x = this.pitchVelocityTargetMinMax.x / this.pitchVelocityRampTimeMinMax.x;
			this.pitchAccelMinMax.y = this.pitchVelocityTargetMinMax.y / this.pitchVelocityRampTimeMinMax.y;
			this.rollAccel = this.rollVelocityTarget / this.rollVelocityRampTime;
			this.thrustAccel = this.thrustVelocityTarget / this.thrustAccelTime;
		}

		protected override void AuthorityBeginMobilization()
		{
			base.AuthorityBeginMobilization();
			float x = base.transform.lossyScale.x;
			this.rb.linearVelocity = base.transform.forward * this.initialSpeed * x;
		}

		protected override void AuthorityUpdate(float dt)
		{
			base.AuthorityUpdate(dt);
			this.motorLevel = 0f;
			if (this.localState == RCVehicle.State.Mobilized)
			{
				this.motorLevel = this.activeInput.trigger;
			}
			this.leftAileronLevel = 0f;
			this.rightAileronLevel = 0f;
			float magnitude = this.activeInput.joystick.magnitude;
			if (magnitude > 0.01f)
			{
				float num = Mathf.Abs(this.activeInput.joystick.x) / magnitude;
				float num2 = Mathf.Abs(this.activeInput.joystick.y) / magnitude;
				this.leftAileronLevel = Mathf.Clamp(num * this.activeInput.joystick.x + num2 * -this.activeInput.joystick.y, -1f, 1f);
				this.rightAileronLevel = Mathf.Clamp(num * this.activeInput.joystick.x + num2 * this.activeInput.joystick.y, -1f, 1f);
			}
			if (this.networkSync != null)
			{
				this.networkSync.syncedState.dataA = (byte)Mathf.Clamp(Mathf.FloorToInt(this.motorLevel * 255f), 0, 255);
				this.networkSync.syncedState.dataB = (byte)Mathf.Clamp(Mathf.FloorToInt(this.leftAileronLevel * 126f), -126, 126);
				this.networkSync.syncedState.dataC = (byte)Mathf.Clamp(Mathf.FloorToInt(this.rightAileronLevel * 126f), -126, 126);
			}
		}

		protected override void RemoteUpdate(float dt)
		{
			base.RemoteUpdate(dt);
			if (this.networkSync != null)
			{
				this.motorLevel = Mathf.Clamp01((float)this.networkSync.syncedState.dataA / 255f);
				this.leftAileronLevel = Mathf.Clamp((float)this.networkSync.syncedState.dataB / 126f, -1f, 1f);
				this.rightAileronLevel = Mathf.Clamp((float)this.networkSync.syncedState.dataC / 126f, -1f, 1f);
			}
		}

		protected override void SharedUpdate(float dt)
		{
			base.SharedUpdate(dt);
			switch (this.localState)
			{
			case RCVehicle.State.DockedLeft:
			case RCVehicle.State.DockedRight:
				this.propellerSpinRate = Mathf.MoveTowards(this.propellerSpinRate, 0.6f, 6.6666665f * dt);
				this.propellerAngle += this.propellerSpinRate * 360f * dt;
				this.propeller.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, this.propellerAngle));
				break;
			case RCVehicle.State.Mobilized:
			{
				if (this.localStatePrev != RCVehicle.State.Mobilized)
				{
					this.audioSource.loop = true;
					this.audioSource.clip = this.motorSound;
					this.audioSource.volume = 0f;
					this.audioSource.GTPlay();
				}
				float num = Mathf.Lerp(this.motorSoundVolumeMinMax.x, this.motorSoundVolumeMinMax.y, this.motorLevel);
				this.audioSource.volume = Mathf.MoveTowards(this.audioSource.volume, num, this.motorSoundVolumeMinMax.y / this.motorVolumeRampTime * dt);
				this.propellerSpinRate = Mathf.MoveTowards(this.propellerSpinRate, 5f, 6.6666665f * dt);
				this.propellerAngle += this.propellerSpinRate * 360f * dt;
				this.propeller.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, this.propellerAngle));
				break;
			}
			case RCVehicle.State.Crashed:
				if (this.localStatePrev != RCVehicle.State.Crashed)
				{
					this.audioSource.GTStop();
					this.audioSource.clip = null;
					this.audioSource.loop = false;
					this.audioSource.volume = this.crashSoundVolume;
					if (this.crashSound != null)
					{
						this.audioSource.GTPlayOneShot(this.crashSound, 1f);
					}
				}
				this.propellerSpinRate = Mathf.MoveTowards(this.propellerSpinRate, 0f, 13.333333f * dt);
				this.propellerAngle += this.propellerSpinRate * 360f * dt;
				this.propeller.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, this.propellerAngle));
				break;
			}
			float num2 = Mathf.Lerp(this.aileronAngularRange.x, this.aileronAngularRange.y, Mathf.InverseLerp(-1f, 1f, this.leftAileronLevel));
			float num3 = Mathf.Lerp(this.aileronAngularRange.x, this.aileronAngularRange.y, Mathf.InverseLerp(-1f, 1f, this.rightAileronLevel));
			this.leftAileronAngle = Mathf.MoveTowards(this.leftAileronAngle, num2, this.aileronAngularAcc * Time.deltaTime);
			this.rightAileronAngle = Mathf.MoveTowards(this.rightAileronAngle, num3, this.aileronAngularAcc * Time.deltaTime);
			Quaternion quaternion = Quaternion.Euler(0f, -90f, 90f + this.leftAileronAngle);
			Quaternion quaternion2 = Quaternion.Euler(0f, 90f, -90f + this.rightAileronAngle);
			this.leftAileronLower.localRotation = quaternion;
			this.leftAileronUpper.localRotation = quaternion;
			this.rightAileronLower.localRotation = quaternion2;
			this.rightAileronUpper.localRotation = quaternion2;
		}

		private void FixedUpdate()
		{
			if (!base.HasLocalAuthority || this.localState != RCVehicle.State.Mobilized)
			{
				return;
			}
			float x = base.transform.lossyScale.x;
			float num = this.thrustVelocityTarget * x;
			float num2 = this.thrustAccel * x;
			float fixedDeltaTime = Time.fixedDeltaTime;
			this.pitch = base.NormalizeAngle180(this.pitch);
			this.roll = base.NormalizeAngle180(this.roll);
			float num3 = this.pitch;
			float num4 = this.roll;
			if (this.activeInput.joystick.y >= 0f)
			{
				float num5 = this.activeInput.joystick.y * this.pitchVelocityTargetMinMax.y;
				this.pitchVel = Mathf.MoveTowards(this.pitchVel, num5, this.pitchAccelMinMax.y * fixedDeltaTime);
				this.pitch += this.pitchVel * fixedDeltaTime;
			}
			else
			{
				float num6 = -this.activeInput.joystick.y * this.pitchVelocityTargetMinMax.x;
				this.pitchVel = Mathf.MoveTowards(this.pitchVel, num6, this.pitchAccelMinMax.x * fixedDeltaTime);
				this.pitch += this.pitchVel * fixedDeltaTime;
			}
			float num7 = -this.activeInput.joystick.x * this.rollVelocityTarget;
			this.rollVel = Mathf.MoveTowards(this.rollVel, num7, this.rollAccel * fixedDeltaTime);
			this.roll += this.rollVel * fixedDeltaTime;
			Quaternion quaternion = Quaternion.Euler(new Vector3(this.pitch - num3, 0f, this.roll - num4));
			base.transform.rotation = base.transform.rotation * quaternion;
			this.rb.angularVelocity = Vector3.zero;
			Vector3 linearVelocity = this.rb.linearVelocity;
			float magnitude = linearVelocity.magnitude;
			float num8 = Mathf.Max(Vector3.Dot(base.transform.forward, linearVelocity), 0f);
			float num9 = this.activeInput.trigger * num;
			float num10 = 0.1f * x;
			if (num9 > num10 && num9 > num8)
			{
				float num11 = Mathf.MoveTowards(num8, num9, num2 * fixedDeltaTime);
				this.rb.AddForce(base.transform.forward * (num11 - num8) * this.rb.mass, ForceMode.Impulse);
			}
			float num12 = 0.01f * x;
			float num13 = Vector3.Dot(linearVelocity / Mathf.Max(magnitude, num12), base.transform.forward);
			float num14 = this.liftVsAttackCurve.Evaluate(num13);
			float num15 = Mathf.Lerp(this.liftVsSpeedOutput.x, this.liftVsSpeedOutput.y, Mathf.InverseLerp(this.liftVsSpeedInput.x, this.liftVsSpeedInput.y, magnitude / x));
			float num16 = num14 * num15;
			Vector3 vector = Vector3.RotateTowards(linearVelocity, base.transform.forward * magnitude, this.pitchVelocityFollowRateAngle * 0.017453292f * fixedDeltaTime, this.pitchVelocityFollowRateMagnitude * fixedDeltaTime) - linearVelocity;
			this.rb.AddForce(vector * num16 * this.rb.mass, ForceMode.Impulse);
			float num17 = Vector3.Dot(linearVelocity.normalized, base.transform.up);
			float num18 = this.dragVsAttackCurve.Evaluate(num17);
			this.rb.AddForce(-linearVelocity * this.maxDrag * num18 * this.rb.mass, ForceMode.Force);
			if (this.rb.useGravity)
			{
				float num19 = Mathf.Lerp(this.gravityCompensationRange.x, this.gravityCompensationRange.y, Mathf.InverseLerp(0f, num, num8 / x));
				RCVehicle.AddScaledGravityCompensationForce(this.rb, x, num19);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (base.HasLocalAuthority && this.localState == RCVehicle.State.Mobilized)
			{
				for (int i = 0; i < collision.contactCount; i++)
				{
					ContactPoint contact = collision.GetContact(i);
					if (!this.nonCrashColliders.Contains(contact.thisCollider))
					{
						this.AuthorityBeginCrash();
					}
				}
				return;
			}
			bool flag = collision.collider.gameObject.IsOnLayer(UnityLayer.GorillaThrowable);
			bool flag2 = collision.collider.gameObject.IsOnLayer(UnityLayer.GorillaHand);
			if ((flag || flag2) && this.localState == RCVehicle.State.Mobilized)
			{
				Vector3 vector = Vector3.zero;
				if (flag2)
				{
					GorillaHandClimber component = collision.collider.gameObject.GetComponent<GorillaHandClimber>();
					if (component != null)
					{
						vector = GTPlayer.Instance.GetHandVelocityTracker(component.xrNode == XRNode.LeftHand).GetAverageVelocity(true, 0.15f, false);
					}
				}
				else if (collision.rigidbody != null)
				{
					vector = collision.rigidbody.linearVelocity;
				}
				if (flag || vector.sqrMagnitude > 0.01f)
				{
					if (base.HasLocalAuthority)
					{
						this.AuthorityApplyImpact(vector, flag);
						return;
					}
					if (this.networkSync != null)
					{
						this.networkSync.photonView.RPC("HitRCVehicleRPC", RpcTarget.Others, new object[] { vector, flag });
					}
				}
			}
		}

		public Vector2 pitchVelocityTargetMinMax = new Vector2(-180f, 180f);

		public Vector2 pitchVelocityRampTimeMinMax = new Vector2(-0.75f, 0.75f);

		public float rollVelocityTarget = 180f;

		public float rollVelocityRampTime = 0.75f;

		public float thrustVelocityTarget = 15f;

		public float thrustAccelTime = 2f;

		[SerializeField]
		private float pitchVelocityFollowRateAngle = 60f;

		[SerializeField]
		private float pitchVelocityFollowRateMagnitude = 5f;

		[SerializeField]
		private float maxDrag = 0.1f;

		[SerializeField]
		private Vector2 liftVsSpeedInput = new Vector2(0f, 4f);

		[SerializeField]
		private Vector2 liftVsSpeedOutput = new Vector2(0.5f, 1f);

		[SerializeField]
		private AnimationCurve liftVsAttackCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private AnimationCurve dragVsAttackCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private Vector2 gravityCompensationRange = new Vector2(0.5f, 1f);

		[SerializeField]
		private List<Collider> nonCrashColliders = new List<Collider>();

		[SerializeField]
		private Transform propeller;

		[SerializeField]
		private Transform leftAileronUpper;

		[SerializeField]
		private Transform leftAileronLower;

		[SerializeField]
		private Transform rightAileronUpper;

		[SerializeField]
		private Transform rightAileronLower;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private AudioClip motorSound;

		[SerializeField]
		private AudioClip crashSound;

		[SerializeField]
		private Vector2 motorSoundVolumeMinMax = new Vector2(0.02f, 0.1f);

		[SerializeField]
		private float crashSoundVolume = 0.12f;

		private float motorVolumeRampTime = 1f;

		private float propellerAngle;

		private float propellerSpinRate;

		private const float propellerIdleAcc = 1f;

		private const float propellerIdleSpinRate = 0.6f;

		private const float propellerMaxAcc = 6.6666665f;

		private const float propellerMaxSpinRate = 5f;

		public float initialSpeed = 3f;

		private float pitch;

		private float pitchVel;

		private Vector2 pitchAccelMinMax;

		private float roll;

		private float rollVel;

		private float rollAccel;

		private float thrustAccel;

		private float motorLevel;

		private float leftAileronLevel;

		private float rightAileronLevel;

		private Vector2 aileronAngularRange = new Vector2(-30f, 45f);

		private float aileronAngularAcc = 120f;

		private float leftAileronAngle;

		private float rightAileronAngle;
	}
}
