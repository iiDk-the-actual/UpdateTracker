using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class ProjectileShooterCosmetic : MonoBehaviour, ITickSystemTick
	{
		private bool IsMovementShoot()
		{
			return this.shootActivatorType == ProjectileShooterCosmetic.ShootActivator.VelocityEstimatorThreshold;
		}

		private bool IsRigDirection()
		{
			return this.shootDirectionType == ProjectileShooterCosmetic.ShootDirection.LineFromRigToLaunchTransform;
		}

		public bool shootingAllowed { get; set; } = true;

		private bool IsCoolingDown
		{
			get
			{
				return this.cooldownRemaining > 0f;
			}
		}

		private void Awake()
		{
			this.transferrableObject = base.GetComponent<TransferrableObject>();
			this.rig = ((this.transferrableObject == null) ? base.GetComponentInParent<VRRig>() : this.transferrableObject.ownerRig);
			UnityEvent<int> unityEvent = this.onMovedToNextStep;
			if (unityEvent != null)
			{
				unityEvent.Invoke(this.currentStep);
			}
			this.isLocal = (this.transferrableObject != null && this.transferrableObject.IsMyItem()) || (this.rig != null && this.rig == GorillaTagger.Instance.offlineVRRig);
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (this.IsCoolingDown)
			{
				this.cooldownRemaining -= Time.deltaTime;
				if (this.cooldownRemaining <= 0f)
				{
					this.cooldownRemaining = 0f;
					UnityEvent unityEvent = this.onCooldownFinished;
					if (unityEvent != null)
					{
						unityEvent.Invoke();
					}
					if (this.isPressed)
					{
						this.SetPressState(true);
					}
					if (!this.allowCharging && this.shootActivatorType != ProjectileShooterCosmetic.ShootActivator.VelocityEstimatorThreshold)
					{
						TickSystem<object>.RemoveTickCallback(this);
					}
				}
			}
			if (!this.IsCoolingDown && this.allowCharging)
			{
				if (this.isPressed)
				{
					if (this.chargeTime < this.maxChargeSeconds)
					{
						this.chargeTime += Time.deltaTime;
						if (this.chargeTime >= this.maxChargeSeconds || this.chargeTime >= this.snapToMaxChargeAt)
						{
							this.chargeTime = this.maxChargeSeconds;
							UnityEvent unityEvent2 = this.onMaxCharge;
							if (unityEvent2 != null)
							{
								unityEvent2.Invoke();
							}
						}
					}
					float chargeFrac = this.GetChargeFrac();
					ContinuousPropertyArray continuousPropertyArray = this.continuousChargingProperties;
					if (continuousPropertyArray != null)
					{
						continuousPropertyArray.ApplyAll(chargeFrac);
					}
					UnityEvent<float> unityEvent3 = this.whileCharging;
					if (unityEvent3 != null)
					{
						unityEvent3.Invoke(chargeFrac);
					}
					this.TryRunHaptics((chargeFrac >= 1f) ? this.maxChargeHapticsIntensity : (chargeFrac * this.chargeHapticsIntensity), Time.deltaTime);
					this.lastStep = this.currentStep;
					this.currentStep = Mathf.Clamp(Mathf.FloorToInt(chargeFrac * (float)this.numberOfProgressSteps), 0, this.numberOfProgressSteps - 1);
					if (this.currentStep >= 0 && this.currentStep != this.lastStep)
					{
						UnityEvent<int> unityEvent4 = this.onMovedToNextStep;
						if (unityEvent4 != null)
						{
							unityEvent4.Invoke(this.currentStep);
						}
						if (this.currentStep == this.numberOfProgressSteps - 1)
						{
							UnityEvent<int> unityEvent5 = this.onReachedLastProgressStep;
							if (unityEvent5 != null)
							{
								unityEvent5.Invoke(this.currentStep);
							}
						}
					}
					if (this.shootActivatorType == ProjectileShooterCosmetic.ShootActivator.VelocityEstimatorThreshold)
					{
						Vector3 linearVelocity = this.velocityEstimator.linearVelocity;
						float num = linearVelocity.magnitude;
						float num2 = Vector3.Dot(linearVelocity / num, this.GetVectorFromBodyToLaunchPosition().normalized);
						num *= Mathf.Ceil(num2 - this.velocityEstimatorMinRigDotProduct);
						if (num >= this.velocityEstimatorStartGestureSpeed)
						{
							this.velocityEstimatorThresholdMet = true;
							return;
						}
						if (this.velocityEstimatorThresholdMet && num < this.velocityEstimatorStopGestureSpeed)
						{
							this.TryShoot();
							return;
						}
					}
				}
				else if (this.chargeTime > 0f)
				{
					this.chargeTime -= Time.deltaTime * this.chargeDecaySpeed;
					if (this.chargeTime <= 0f)
					{
						this.chargeTime = 0f;
						TickSystem<object>.RemoveTickCallback(this);
						ContinuousPropertyArray continuousPropertyArray2 = this.continuousChargingProperties;
						if (continuousPropertyArray2 != null)
						{
							continuousPropertyArray2.ApplyAll(0f);
						}
						UnityEvent<float> unityEvent6 = this.whileCharging;
						if (unityEvent6 == null)
						{
							return;
						}
						unityEvent6.Invoke(0f);
						return;
					}
					else
					{
						float chargeFrac2 = this.GetChargeFrac();
						ContinuousPropertyArray continuousPropertyArray3 = this.continuousChargingProperties;
						if (continuousPropertyArray3 != null)
						{
							continuousPropertyArray3.ApplyAll(chargeFrac2);
						}
						UnityEvent<float> unityEvent7 = this.whileCharging;
						if (unityEvent7 == null)
						{
							return;
						}
						unityEvent7.Invoke(chargeFrac2);
					}
				}
			}
		}

		private Vector3 GetVectorFromBodyToLaunchPosition()
		{
			return this.shootFromTransform.position - this.rig.bodyTransform.TransformPoint(this.offsetRigPosition);
		}

		private void GetShootPositionAndRotation(out Vector3 position, out Quaternion rotation)
		{
			ProjectileShooterCosmetic.ShootDirection shootDirection = this.shootDirectionType;
			if (shootDirection != ProjectileShooterCosmetic.ShootDirection.LaunchTransformRotation && shootDirection == ProjectileShooterCosmetic.ShootDirection.LineFromRigToLaunchTransform)
			{
				position = this.shootFromTransform.position;
				rotation = Quaternion.LookRotation(position - this.rig.bodyTransform.TransformPoint(this.offsetRigPosition));
				return;
			}
			this.shootFromTransform.GetPositionAndRotation(out position, out rotation);
		}

		private void Shoot()
		{
			float chargeFrac = this.GetChargeFrac();
			float num = Mathf.Lerp(this.shootMinSpeed, this.shootMaxSpeed, this.chargeToShotSpeedCurve.Evaluate(chargeFrac));
			GameObject gameObject = ObjectPools.instance.Instantiate(in this.projectilePrefab, true);
			gameObject.transform.localScale = Vector3.one * this.rig.scaleFactor;
			IProjectile component = gameObject.GetComponent<IProjectile>();
			if (component != null)
			{
				Vector3 vector;
				Quaternion quaternion;
				this.GetShootPositionAndRotation(out vector, out quaternion);
				Vector3 vector2 = quaternion * Vector3.forward * (num * this.rig.scaleFactor);
				component.Launch(vector, quaternion, vector2, chargeFrac, this.rig, this.currentStep);
				if ((in this.projectileTrailPrefab) != -1)
				{
					this.AttachTrail(in this.projectileTrailPrefab, gameObject, vector, false, false);
				}
			}
			UnityEvent<float> unityEvent = this.onShoot;
			if (unityEvent != null)
			{
				unityEvent.Invoke(chargeFrac);
			}
			this.continuousChargingProperties.ApplyAll(0f);
			UnityEvent<float> unityEvent2 = this.whileCharging;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke(0f);
			}
			if (this.isLocal)
			{
				UnityEvent<float> unityEvent3 = this.onShootLocal;
				if (unityEvent3 != null)
				{
					unityEvent3.Invoke(chargeFrac);
				}
			}
			if (this.allowCharging && this.runChargeCancelledEventOnShoot)
			{
				UnityEvent unityEvent4 = this.onChargeCancelled;
				if (unityEvent4 != null)
				{
					unityEvent4.Invoke();
				}
			}
			this.TryRunHaptics(chargeFrac * this.shootHapticsIntensity, this.shootHapticsDuration);
			this.SetPressState(false);
			this.cooldownRemaining = this.cooldownSeconds;
			this.chargeTime = 0f;
			this.currentStep = -1;
			TickSystem<object>.AddTickCallback(this);
		}

		private bool TryShoot()
		{
			if ((!this.IsCoolingDown && this.shootingAllowed && this.shootActivatorType != ProjectileShooterCosmetic.ShootActivator.ButtonReleasedFullCharge) || (this.shootActivatorType == ProjectileShooterCosmetic.ShootActivator.ButtonReleasedFullCharge && this.chargeTime >= this.maxChargeSeconds))
			{
				this.Shoot();
				return true;
			}
			return false;
		}

		private void TryRunHaptics(float intensity, float duration)
		{
			if (!this.enableHaptics || !this.isLocal || intensity <= 0f)
			{
				return;
			}
			bool flag = this.transferrableObject != null && this.transferrableObject.InLeftHand();
			GorillaTagger.Instance.StartVibration(flag, intensity, duration);
			if (this.hapticsBothHands)
			{
				GorillaTagger.Instance.StartVibration(!flag, intensity, duration);
			}
		}

		private float GetChargeFrac()
		{
			if (!this.allowCharging)
			{
				return 1f;
			}
			if (this.chargeTime <= 0f)
			{
				return 0f;
			}
			if (this.chargeTime < this.maxChargeSeconds)
			{
				return this.chargeRateCurve.Evaluate(this.chargeTime / this.maxChargeSeconds);
			}
			return 1f;
		}

		private void SetPressState(bool pressed)
		{
			this.isPressed = pressed;
			this.velocityEstimatorThresholdMet = false;
		}

		public void OnButtonPressed()
		{
			this.SetPressState(true);
			if (this.shootActivatorType == ProjectileShooterCosmetic.ShootActivator.ButtonPressed)
			{
				this.TryShoot();
				return;
			}
			if (this.allowCharging || this.shootActivatorType == ProjectileShooterCosmetic.ShootActivator.VelocityEstimatorThreshold)
			{
				TickSystem<object>.AddTickCallback(this);
			}
		}

		public void OnButtonReleased()
		{
			if (this.shootActivatorType == ProjectileShooterCosmetic.ShootActivator.VelocityEstimatorThreshold && this.velocityEstimatorThresholdMet)
			{
				return;
			}
			ProjectileShooterCosmetic.ShootActivator shootActivator = this.shootActivatorType;
			if ((shootActivator != ProjectileShooterCosmetic.ShootActivator.ButtonReleased && shootActivator != ProjectileShooterCosmetic.ShootActivator.ButtonReleasedFullCharge) || !this.TryShoot())
			{
				this.SetPressState(false);
				if (this.allowCharging)
				{
					ContinuousPropertyArray continuousPropertyArray = this.continuousChargingProperties;
					if (continuousPropertyArray != null)
					{
						continuousPropertyArray.ApplyAll(0f);
					}
					UnityEvent<float> unityEvent = this.whileCharging;
					if (unityEvent != null)
					{
						unityEvent.Invoke(0f);
					}
					UnityEvent unityEvent2 = this.onChargeCancelled;
					if (unityEvent2 == null)
					{
						return;
					}
					unityEvent2.Invoke();
				}
			}
		}

		public void ResetShoot()
		{
			this.isPressed = false;
			this.velocityEstimatorThresholdMet = false;
			this.currentStep = -1;
			this.lastStep = -1;
			TickSystem<object>.RemoveTickCallback(this);
		}

		private void AttachTrail(int trailHash, GameObject newProjectile, Vector3 location, bool blueTeam, bool orangeTeam)
		{
			GameObject gameObject = ObjectPools.instance.Instantiate(trailHash, true);
			SlingshotProjectileTrail component = gameObject.GetComponent<SlingshotProjectileTrail>();
			if (component.IsNull())
			{
				ObjectPools.instance.Destroy(gameObject);
			}
			newProjectile.transform.position = location;
			component.AttachTrail(newProjectile, blueTeam, orangeTeam, false, default(Color));
		}

		private const string CHARGE_STR = "allowCharging";

		private const string CHARGE_MSG = "only enabled when allowCharging is true.";

		private const string HAPTICS_STR = "enableHaptics";

		private const string MOVE_STR = "IsMovementShoot";

		[SerializeField]
		private HashWrapper projectilePrefab;

		[SerializeField]
		private HashWrapper projectileTrailPrefab;

		[FormerlySerializedAs("launchActivatorType")]
		[SerializeField]
		private ProjectileShooterCosmetic.ShootActivator shootActivatorType;

		[FormerlySerializedAs("launchDirectionType")]
		[SerializeField]
		private ProjectileShooterCosmetic.ShootDirection shootDirectionType;

		[SerializeField]
		private Vector3 offsetRigPosition;

		[FormerlySerializedAs("launchTransform")]
		[SerializeField]
		private Transform shootFromTransform;

		[SerializeField]
		private bool drawShootVector;

		[FormerlySerializedAs("cooldown")]
		[SerializeField]
		private float cooldownSeconds;

		[Space]
		[SerializeField]
		private bool enableHaptics = true;

		[FormerlySerializedAs("hapticsIntensity")]
		[SerializeField]
		private float shootHapticsIntensity = 0.5f;

		[FormerlySerializedAs("hapticsDuration")]
		[SerializeField]
		private float shootHapticsDuration = 0.2f;

		[SerializeField]
		[Tooltip("only enabled when allowCharging is true.")]
		private float chargeHapticsIntensity = 0.3f;

		[SerializeField]
		[Tooltip("only enabled when allowCharging is true.")]
		private float maxChargeHapticsIntensity = 0.3f;

		[SerializeField]
		private bool hapticsBothHands;

		[Space]
		[SerializeField]
		private GorillaVelocityEstimator velocityEstimator;

		[SerializeField]
		private float velocityEstimatorStartGestureSpeed = 0.5f;

		[SerializeField]
		private float velocityEstimatorStopGestureSpeed = 0.2f;

		[SerializeField]
		private float velocityEstimatorMinRigDotProduct = 0.5f;

		[SerializeField]
		private bool logVelocityEstimatorSpeed;

		[FormerlySerializedAs("launchMinSpeed")]
		[SerializeField]
		[Tooltip("only enabled when allowCharging is true.")]
		private float shootMinSpeed;

		[FormerlySerializedAs("launchMaxSpeed")]
		[SerializeField]
		private float shootMaxSpeed;

		[SerializeField]
		private bool allowCharging;

		[SerializeField]
		private float maxChargeSeconds = 2f;

		[SerializeField]
		private float snapToMaxChargeAt = 9999999f;

		[SerializeField]
		private float chargeDecaySpeed = 9999999f;

		[SerializeField]
		private bool runChargeCancelledEventOnShoot;

		[SerializeField]
		private AnimationCurve chargeRateCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private AnimationCurve chargeToShotSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[FormerlySerializedAs("onReadyToShoot")]
		public UnityEvent onCooldownFinished;

		public ContinuousPropertyArray continuousChargingProperties;

		public UnityEvent<float> whileCharging;

		public UnityEvent onMaxCharge;

		public UnityEvent onChargeCancelled;

		[FormerlySerializedAs("onLaunchProjectileShared")]
		public UnityEvent<float> onShoot;

		[FormerlySerializedAs("onOwnerLaunchProjectile")]
		public UnityEvent<float> onShootLocal;

		[SerializeField]
		private int numberOfProgressSteps;

		public UnityEvent<int> onMovedToNextStep;

		public UnityEvent<int> onReachedLastProgressStep;

		private int currentStep = -1;

		private int lastStep = -1;

		private bool isPressed;

		private bool velocityEstimatorThresholdMet;

		private float cooldownRemaining;

		private float chargeTime;

		private TransferrableObject transferrableObject;

		private VRRig rig;

		private bool isLocal;

		private Transform debugShootDirection;

		private enum ShootActivator
		{
			ButtonReleased,
			ButtonPressed,
			ButtonStayed,
			VelocityEstimatorThreshold,
			ButtonReleasedFullCharge
		}

		private enum ShootDirection
		{
			LaunchTransformRotation,
			LineFromRigToLaunchTransform
		}
	}
}
