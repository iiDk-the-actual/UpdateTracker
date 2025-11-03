using System;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaTag.Cosmetics
{
	public class RCBlimp : RCVehicle
	{
		protected override void AuthorityBeginDocked()
		{
			base.AuthorityBeginDocked();
			this.turnRate = 0f;
			this.turnAngle = Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(base.transform.forward, Vector3.up), Vector3.up);
			this.motorLevel = 0f;
			if (this.connectedRemote == null)
			{
				base.gameObject.SetActive(false);
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.ascendAccel = this.maxAscendSpeed / this.ascendAccelTime;
			this.turnAccel = this.maxTurnRate / this.turnAccelTime;
			this.horizontalAccel = this.maxHorizontalSpeed / this.horizontalAccelTime;
			this.tiltAccel = this.maxHorizontalTiltAngle / this.horizontalTiltTime;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			this.audioSource.GTStop();
		}

		protected override void AuthorityUpdate(float dt)
		{
			base.AuthorityUpdate(dt);
			this.motorLevel = 0f;
			if (this.localState == RCVehicle.State.Mobilized)
			{
				this.motorLevel = Mathf.Max(Mathf.Max(Mathf.Abs(this.activeInput.joystick.y), Mathf.Abs(this.activeInput.joystick.x)), this.activeInput.trigger);
			}
			if (this.networkSync != null)
			{
				this.networkSync.syncedState.dataA = (byte)Mathf.Clamp(Mathf.FloorToInt(this.motorLevel * 255f), 0, 255);
			}
		}

		protected override void RemoteUpdate(float dt)
		{
			base.RemoteUpdate(dt);
			if (this.localState == RCVehicle.State.Mobilized && this.networkSync != null)
			{
				this.motorLevel = Mathf.Clamp01((float)this.networkSync.syncedState.dataA / 255f);
			}
		}

		protected override void SharedUpdate(float dt)
		{
			base.SharedUpdate(dt);
			switch (this.localState)
			{
			case RCVehicle.State.Disabled:
				break;
			case RCVehicle.State.DockedLeft:
			case RCVehicle.State.DockedRight:
				if (this.localStatePrev != RCVehicle.State.DockedLeft && this.localStatePrev != RCVehicle.State.DockedRight)
				{
					this.audioSource.GTStop();
					this.blimpDeflateBlendWeight = 0f;
					this.blimpMesh.SetBlendShapeWeight(0, 0f);
					this.crashCollider.enabled = false;
				}
				this.leftPropellerSpinRate = Mathf.MoveTowards(this.leftPropellerSpinRate, 0.6f, 6.6666665f * dt);
				this.rightPropellerSpinRate = Mathf.MoveTowards(this.rightPropellerSpinRate, 0.6f, 6.6666665f * dt);
				this.leftPropellerAngle += this.leftPropellerSpinRate * 360f * dt;
				this.rightPropellerAngle += this.rightPropellerSpinRate * 360f * dt;
				this.leftPropeller.transform.localRotation = Quaternion.Euler(new Vector3(this.leftPropellerAngle, 0f, -90f));
				this.rightPropeller.transform.localRotation = Quaternion.Euler(new Vector3(this.rightPropellerAngle, 0f, 90f));
				return;
			case RCVehicle.State.Mobilized:
			{
				if (this.localStatePrev != RCVehicle.State.Mobilized)
				{
					this.audioSource.loop = true;
					this.audioSource.clip = this.motorSound;
					this.audioSource.volume = 0f;
					this.audioSource.GTPlay();
					this.blimpDeflateBlendWeight = 0f;
					this.blimpMesh.SetBlendShapeWeight(0, 0f);
					this.crashCollider.enabled = false;
				}
				float num = Mathf.Lerp(this.motorSoundVolumeMinMax.x, this.motorSoundVolumeMinMax.y, this.motorLevel);
				this.audioSource.volume = Mathf.MoveTowards(this.audioSource.volume, num, this.motorSoundVolumeMinMax.y / this.motorVolumeRampTime * dt);
				this.blimpDeflateBlendWeight = 0f;
				float num2 = this.activeInput.joystick.y * 5f;
				float num3 = this.activeInput.joystick.x * 5f;
				float num4 = Mathf.Clamp(num3 + num2 + 0.6f, -5f, 5f);
				float num5 = Mathf.Clamp(-num3 + num2 + 0.6f, -5f, 5f);
				this.leftPropellerSpinRate = Mathf.MoveTowards(this.leftPropellerSpinRate, num4, 6.6666665f * dt);
				this.rightPropellerSpinRate = Mathf.MoveTowards(this.rightPropellerSpinRate, num5, 6.6666665f * dt);
				this.leftPropellerAngle += this.leftPropellerSpinRate * 360f * dt;
				this.rightPropellerAngle += this.rightPropellerSpinRate * 360f * dt;
				this.leftPropeller.transform.localRotation = Quaternion.Euler(new Vector3(this.leftPropellerAngle, 0f, -90f));
				this.rightPropeller.transform.localRotation = Quaternion.Euler(new Vector3(this.rightPropellerAngle, 0f, 90f));
				break;
			}
			case RCVehicle.State.Crashed:
				if (this.localStatePrev != RCVehicle.State.Crashed)
				{
					this.audioSource.GTStop();
					this.audioSource.clip = null;
					this.audioSource.loop = false;
					this.audioSource.volume = this.deflateSoundVolume;
					if (this.deflateSound != null)
					{
						this.audioSource.GTPlayOneShot(this.deflateSound, 1f);
					}
					this.leftPropellerSpinRate = 0f;
					this.rightPropellerSpinRate = 0f;
					this.leftPropellerAngle = 0f;
					this.rightPropellerAngle = 0f;
					this.leftPropeller.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
					this.rightPropeller.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
					this.crashCollider.enabled = true;
				}
				this.blimpDeflateBlendWeight = Mathf.Lerp(1f, this.blimpDeflateBlendWeight, Mathf.Exp(-this.deflateRate * dt));
				this.blimpMesh.SetBlendShapeWeight(0, this.blimpDeflateBlendWeight * 100f);
				return;
			default:
				return;
			}
		}

		private void FixedUpdate()
		{
			if (!base.HasLocalAuthority)
			{
				return;
			}
			float fixedDeltaTime = Time.fixedDeltaTime;
			float x = base.transform.lossyScale.x;
			if (this.localState == RCVehicle.State.Mobilized)
			{
				float num = this.maxAscendSpeed * x;
				float num2 = this.maxHorizontalSpeed * x;
				float num3 = this.ascendAccel * x;
				Vector3 linearVelocity = this.rb.linearVelocity;
				Vector3 normalized = new Vector3(base.transform.forward.x, 0f, base.transform.forward.z).normalized;
				this.turnAngle = Vector3.SignedAngle(Vector3.forward, normalized, Vector3.up);
				this.tiltAngle = Vector3.SignedAngle(normalized, base.transform.forward, base.transform.right);
				float num4 = this.activeInput.joystick.x * this.maxTurnRate;
				this.turnRate = Mathf.MoveTowards(this.turnRate, num4, this.turnAccel * fixedDeltaTime);
				this.turnAngle += this.turnRate * fixedDeltaTime;
				float num5 = Vector3.Dot(normalized, linearVelocity);
				float num6 = Mathf.InverseLerp(-num2, num2, num5);
				float num7 = Mathf.Lerp(-this.maxHorizontalTiltAngle, this.maxHorizontalTiltAngle, num6);
				this.tiltAngle = Mathf.MoveTowards(this.tiltAngle, num7, this.tiltAccel * fixedDeltaTime);
				base.transform.rotation = Quaternion.Euler(new Vector3(this.tiltAngle, this.turnAngle, 0f));
				Vector3 vector = new Vector3(linearVelocity.x, 0f, linearVelocity.z);
				Vector3 vector2 = Vector3.Lerp(normalized * this.activeInput.joystick.y * num2, vector, Mathf.Exp(-this.horizontalAccelTime * fixedDeltaTime));
				this.rb.AddForce((vector2 - vector) * this.rb.mass, ForceMode.Impulse);
				float num8 = this.activeInput.trigger * num;
				if (num8 > 0.01f && linearVelocity.y < num8)
				{
					this.rb.AddForce(Vector3.up * num3 * this.rb.mass, ForceMode.Force);
				}
				if (this.rb.useGravity)
				{
					RCVehicle.AddScaledGravityCompensationForce(this.rb, x, this.gravityCompensation);
					return;
				}
			}
			else if (this.localState == RCVehicle.State.Crashed && this.rb.useGravity)
			{
				RCVehicle.AddScaledGravityCompensationForce(this.rb, x, this.crashedGravityCompensation);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			bool flag = other.gameObject.IsOnLayer(UnityLayer.GorillaThrowable);
			bool flag2 = other.gameObject.IsOnLayer(UnityLayer.GorillaHand);
			if (!other.isTrigger && base.HasLocalAuthority && this.localState == RCVehicle.State.Mobilized)
			{
				this.AuthorityBeginCrash();
				return;
			}
			if ((flag || flag2) && this.localState == RCVehicle.State.Mobilized)
			{
				Vector3 vector = Vector3.zero;
				if (flag2)
				{
					GorillaHandClimber component = other.gameObject.GetComponent<GorillaHandClimber>();
					if (component != null)
					{
						vector = GTPlayer.Instance.GetHandVelocityTracker(component.xrNode == XRNode.LeftHand).GetAverageVelocity(true, 0.15f, false);
					}
				}
				else if (other.attachedRigidbody != null)
				{
					vector = other.attachedRigidbody.linearVelocity;
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

		[SerializeField]
		private float maxAscendSpeed = 6f;

		[SerializeField]
		private float ascendAccelTime = 3f;

		[SerializeField]
		private float gravityCompensation = 0.9f;

		[SerializeField]
		private float crashedGravityCompensation = 0.5f;

		[SerializeField]
		private float maxTurnRate = 90f;

		[SerializeField]
		private float turnAccelTime = 0.75f;

		[SerializeField]
		private float maxHorizontalSpeed = 6f;

		[SerializeField]
		private float horizontalAccelTime = 2f;

		[SerializeField]
		private float maxHorizontalTiltAngle = 45f;

		[SerializeField]
		private float horizontalTiltTime = 2f;

		[SerializeField]
		private Vector2 motorSoundVolumeMinMax = new Vector2(0.1f, 0.8f);

		[SerializeField]
		private float deflateSoundVolume = 0.1f;

		[SerializeField]
		private Collider crashCollider;

		[SerializeField]
		private Transform leftPropeller;

		[SerializeField]
		private Transform rightPropeller;

		[SerializeField]
		private SkinnedMeshRenderer blimpMesh;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private AudioClip motorSound;

		[SerializeField]
		private AudioClip deflateSound;

		private float turnRate;

		private float turnAngle;

		private float tiltAngle;

		private float ascendAccel;

		private float turnAccel;

		private float tiltAccel;

		private float horizontalAccel;

		private float leftPropellerAngle;

		private float rightPropellerAngle;

		private float leftPropellerSpinRate;

		private float rightPropellerSpinRate;

		private float blimpDeflateBlendWeight;

		private float deflateRate = Mathf.Exp(1f);

		private const float propellerIdleAcc = 1f;

		private const float propellerIdleSpinRate = 0.6f;

		private const float propellerMaxAcc = 6.6666665f;

		private const float propellerMaxSpinRate = 5f;

		private float motorVolumeRampTime = 1f;

		private float motorLevel;
	}
}
