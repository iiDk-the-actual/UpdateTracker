using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaTag.Cosmetics
{
	public class RCDragon : RCVehicle
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
			this.shouldFlap = false;
			this.isFlapping = false;
			this.StopBreathFire();
			if (this.animation != null)
			{
				this.animation[this.wingFlapAnimName].speed = this.wingFlapAnimSpeed;
				this.animation[this.crashAnimName].speed = this.crashAnimSpeed;
				this.animation[this.mouthClosedAnimName].layer = 1;
				this.animation[this.mouthBreathFireAnimName].layer = 1;
			}
			this.nextFlapEventAnimTime = this.flapAnimEventTime;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			this.audioSource.GTStop();
		}

		public void StartBreathFire()
		{
			if (!string.IsNullOrEmpty(this.mouthBreathFireAnimName))
			{
				this.animation.CrossFade(this.mouthBreathFireAnimName, 0.1f);
			}
			if (this.fireBreath != null)
			{
				this.fireBreath.SetActive(true);
			}
			this.PlayRandomSound(this.breathFireSound, this.breathFireVolume);
			this.fireBreathTimeRemaining = this.fireBreathDuration;
		}

		public void StopBreathFire()
		{
			if (!string.IsNullOrEmpty(this.mouthClosedAnimName))
			{
				this.animation.CrossFade(this.mouthClosedAnimName, 0.1f);
			}
			if (this.fireBreath != null)
			{
				this.fireBreath.SetActive(false);
			}
			this.fireBreathTimeRemaining = -1f;
		}

		public bool IsBreathingFire()
		{
			return this.fireBreathTimeRemaining >= 0f;
		}

		private void PlayRandomSound(List<AudioClip> clips, float volume)
		{
			if (clips == null || clips.Count == 0)
			{
				return;
			}
			this.PlaySound(clips[Random.Range(0, clips.Count)], volume);
		}

		private void PlaySound(AudioClip clip, float volume)
		{
			if (this.audioSource == null || clip == null)
			{
				return;
			}
			this.audioSource.GTStop();
			this.audioSource.clip = null;
			this.audioSource.loop = false;
			this.audioSource.volume = volume;
			this.audioSource.GTPlayOneShot(clip, 1f);
		}

		protected override void AuthorityUpdate(float dt)
		{
			base.AuthorityUpdate(dt);
			this.motorLevel = 0f;
			if (this.localState == RCVehicle.State.Mobilized)
			{
				this.motorLevel = Mathf.Max(Mathf.Max(Mathf.Abs(this.activeInput.joystick.y), Mathf.Abs(this.activeInput.joystick.x)), this.activeInput.trigger);
				if (!this.IsBreathingFire() && this.activeInput.buttons > 0)
				{
					this.StartBreathFire();
				}
			}
			if (this.networkSync != null)
			{
				this.networkSync.syncedState.dataA = (byte)Mathf.Clamp(Mathf.FloorToInt(this.motorLevel * 255f), 0, 255);
				this.networkSync.syncedState.dataB = this.activeInput.buttons;
				this.networkSync.syncedState.dataC = (this.shouldFlap ? 1 : 0);
			}
		}

		protected override void RemoteUpdate(float dt)
		{
			base.RemoteUpdate(dt);
			if (this.localState == RCVehicle.State.Mobilized && this.networkSync != null)
			{
				this.motorLevel = Mathf.Clamp01((float)this.networkSync.syncedState.dataA / 255f);
				if (!this.IsBreathingFire() && this.networkSync.syncedState.dataB > 0)
				{
					this.StartBreathFire();
				}
				this.shouldFlap = this.networkSync.syncedState.dataC > 0;
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
					if (this.crashCollider != null)
					{
						this.crashCollider.enabled = false;
					}
					if (this.animation != null)
					{
						this.animation.Play(this.dockedAnimName);
					}
					if (this.IsBreathingFire())
					{
						this.StopBreathFire();
						return;
					}
				}
				break;
			case RCVehicle.State.Mobilized:
			{
				if (this.localStatePrev != RCVehicle.State.Mobilized && this.crashCollider != null)
				{
					this.crashCollider.enabled = false;
				}
				if (this.animation != null)
				{
					if (!this.isFlapping && this.shouldFlap)
					{
						this.animation.CrossFade(this.wingFlapAnimName, 0.1f);
						this.nextFlapEventAnimTime = this.flapAnimEventTime;
					}
					else if (this.isFlapping && !this.shouldFlap)
					{
						this.animation.CrossFade(this.idleAnimName, 0.15f);
					}
					this.isFlapping = this.shouldFlap;
					if (this.isFlapping && !this.IsBreathingFire())
					{
						AnimationState animationState = this.animation[this.wingFlapAnimName];
						if (animationState.normalizedTime * animationState.length > this.nextFlapEventAnimTime)
						{
							this.PlayRandomSound(this.wingFlapSound, this.wingFlapVolume);
							this.nextFlapEventAnimTime = (Mathf.Floor(animationState.normalizedTime) + 1f) * animationState.length + this.flapAnimEventTime;
						}
					}
				}
				GTTime.TimeAsDouble();
				if (this.IsBreathingFire())
				{
					this.fireBreathTimeRemaining -= dt;
					if (this.fireBreathTimeRemaining <= 0f)
					{
						this.StopBreathFire();
					}
				}
				float num = Mathf.Lerp(this.motorSoundVolumeMinMax.x, this.motorSoundVolumeMinMax.y, this.motorLevel);
				this.audioSource.volume = Mathf.MoveTowards(this.audioSource.volume, num, this.motorSoundVolumeMinMax.y / this.motorVolumeRampTime * dt);
				break;
			}
			case RCVehicle.State.Crashed:
				if (this.localStatePrev != RCVehicle.State.Crashed)
				{
					this.PlaySound(this.crashSound, this.crashSoundVolume);
					if (this.crashCollider != null)
					{
						this.crashCollider.enabled = true;
					}
					if (this.animation != null)
					{
						this.animation.CrossFade(this.crashAnimName, 0.05f);
					}
					if (this.IsBreathingFire())
					{
						this.StopBreathFire();
						return;
					}
				}
				break;
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
			float x = base.transform.lossyScale.x;
			float fixedDeltaTime = Time.fixedDeltaTime;
			this.shouldFlap = false;
			if (this.localState == RCVehicle.State.Mobilized)
			{
				float num = this.maxAscendSpeed * x;
				float num2 = this.maxHorizontalSpeed * x;
				float num3 = this.ascendAccel * x;
				float num4 = this.ascendWhileFlyingAccelBoost * x;
				float num5 = 0.5f * x;
				float num6 = 45f;
				Vector3 linearVelocity = this.rb.linearVelocity;
				Vector3 normalized = new Vector3(base.transform.forward.x, 0f, base.transform.forward.z).normalized;
				this.turnAngle = Vector3.SignedAngle(Vector3.forward, normalized, Vector3.up);
				this.tiltAngle = Vector3.SignedAngle(normalized, base.transform.forward, base.transform.right);
				float num7 = this.activeInput.joystick.x * this.maxTurnRate;
				this.turnRate = Mathf.MoveTowards(this.turnRate, num7, this.turnAccel * fixedDeltaTime);
				this.turnAngle += this.turnRate * fixedDeltaTime;
				float num8 = Vector3.Dot(normalized, linearVelocity);
				float num9 = Mathf.InverseLerp(-num2, num2, num8);
				float num10 = Mathf.Lerp(-this.maxHorizontalTiltAngle, this.maxHorizontalTiltAngle, num9);
				this.tiltAngle = Mathf.MoveTowards(this.tiltAngle, num10, this.tiltAccel * fixedDeltaTime);
				base.transform.rotation = Quaternion.Euler(new Vector3(this.tiltAngle, this.turnAngle, 0f));
				Vector3 vector = new Vector3(linearVelocity.x, 0f, linearVelocity.z);
				Vector3 vector2 = Vector3.Lerp(normalized * this.activeInput.joystick.y * num2, vector, Mathf.Exp(-this.horizontalAccelTime * fixedDeltaTime));
				this.rb.AddForce((vector2 - vector) * this.rb.mass, ForceMode.Impulse);
				float num11 = this.activeInput.trigger * num;
				if (num11 > 0.01f && linearVelocity.y < num11)
				{
					this.rb.AddForce(Vector3.up * num3 * this.rb.mass, ForceMode.Force);
				}
				bool flag = Mathf.Abs(num8) > num5;
				bool flag2 = Mathf.Abs(this.turnRate) > num6;
				if (flag || flag2)
				{
					this.rb.AddForce(Vector3.up * num4 * this.rb.mass, ForceMode.Force);
				}
				this.shouldFlap = num11 > 0.01f || flag || flag2;
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
		private float ascendWhileFlyingAccelBoost;

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
		private float crashSoundVolume = 0.1f;

		[SerializeField]
		private float breathFireVolume = 0.5f;

		[SerializeField]
		private float wingFlapVolume = 0.1f;

		[SerializeField]
		private Animation animation;

		[SerializeField]
		private string wingFlapAnimName;

		[SerializeField]
		private float wingFlapAnimSpeed = 1f;

		[SerializeField]
		private string dockedAnimName;

		[SerializeField]
		private string idleAnimName;

		[SerializeField]
		private string crashAnimName;

		[SerializeField]
		private float crashAnimSpeed = 1f;

		[SerializeField]
		private string mouthClosedAnimName;

		[SerializeField]
		private string mouthBreathFireAnimName;

		private bool shouldFlap;

		private bool isFlapping;

		private float nextFlapEventAnimTime;

		[SerializeField]
		private float flapAnimEventTime = 0.25f;

		[SerializeField]
		private GameObject fireBreath;

		[SerializeField]
		private float fireBreathDuration;

		private float fireBreathTimeRemaining;

		[SerializeField]
		private Collider crashCollider;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private List<AudioClip> breathFireSound;

		[SerializeField]
		private List<AudioClip> wingFlapSound;

		[SerializeField]
		private AudioClip crashSound;

		private float turnRate;

		private float turnAngle;

		private float tiltAngle;

		private float ascendAccel;

		private float turnAccel;

		private float tiltAccel;

		private float horizontalAccel;

		private float motorVolumeRampTime = 1f;

		private float motorLevel;
	}
}
