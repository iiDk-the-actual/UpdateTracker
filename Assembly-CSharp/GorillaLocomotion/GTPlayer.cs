using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AA;
using BoingKit;
using GorillaExtensions;
using GorillaLocomotion.Climbing;
using GorillaLocomotion.Gameplay;
using GorillaLocomotion.Swimming;
using GorillaTag;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaLocomotion
{
	public class GTPlayer : MonoBehaviour
	{
		public static GTPlayer Instance
		{
			get
			{
				return GTPlayer._instance;
			}
		}

		public GTPlayer.HandState LeftHand
		{
			get
			{
				return this.leftHand;
			}
		}

		public GTPlayer.HandState RightHand
		{
			get
			{
				return this.rightHand;
			}
		}

		public int GetMaterialTouchIndex(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).materialTouchIndex;
		}

		public GorillaSurfaceOverride GetSurfaceOverride(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).surfaceOverride;
		}

		public RaycastHit GetTouchHitInfo(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).hitInfo;
		}

		public bool IsHandTouching(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).wasColliding;
		}

		public GorillaVelocityTracker GetHandVelocityTracker(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).velocityTracker;
		}

		public GorillaVelocityTracker GetInteractPointVelocityTracker(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).interactPointVelocityTracker;
		}

		public Transform GetControllerTransform(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).controllerTransform;
		}

		public Transform GetHandFollower(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).handFollower;
		}

		public Vector3 GetHandOffset(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).handOffset;
		}

		public Quaternion GetHandRotOffset(bool isLeftHand)
		{
			return (isLeftHand ? this.leftHand : this.rightHand).handRotOffset;
		}

		public Vector3 GetHandPosition(bool isLeftHand, StiltID stiltID = StiltID.None)
		{
			return ((stiltID != StiltID.None) ? this.stiltStates[(int)stiltID] : (isLeftHand ? this.leftHand : this.rightHand)).lastPosition;
		}

		public void GetHandTapData(bool isLeftHand, StiltID stiltID, out bool wasHandTouching, out bool wasSliding, out int handMatIndex, out GorillaSurfaceOverride surfaceOverride, out RaycastHit handHitInfo, out Vector3 handPosition, out GorillaVelocityTracker handVelocityTracker)
		{
			((stiltID != StiltID.None) ? this.stiltStates[(int)stiltID] : (isLeftHand ? this.leftHand : this.rightHand)).GetHandTapData(out wasHandTouching, out wasSliding, out handMatIndex, out surfaceOverride, out handHitInfo, out handPosition, out handVelocityTracker);
		}

		public void SetHandOffsets(bool isLeftHand, Vector3 handOffset, Quaternion handRotOffset)
		{
			if (isLeftHand)
			{
				this.leftHand.handOffset = handOffset;
				this.leftHand.handRotOffset = handRotOffset;
				return;
			}
			this.rightHand.handOffset = handOffset;
			this.rightHand.handRotOffset = handRotOffset;
		}

		public Vector3 InstantaneousVelocity
		{
			get
			{
				return this.currentVelocity;
			}
		}

		public Vector3 AveragedVelocity
		{
			get
			{
				return this.averagedVelocity;
			}
		}

		public Transform CosmeticsHeadTarget
		{
			get
			{
				return this.cosmeticsHeadTarget;
			}
		}

		public float scale
		{
			get
			{
				return this.scaleMultiplier * this.nativeScale;
			}
		}

		public float NativeScale
		{
			get
			{
				return this.nativeScale;
			}
		}

		public float ScaleMultiplier
		{
			get
			{
				return this.scaleMultiplier;
			}
		}

		public void SetScaleMultiplier(float s)
		{
			this.scaleMultiplier = s;
		}

		public void SetNativeScale(NativeSizeChangerSettings s)
		{
			float num = this.nativeScale;
			if (s != null && s.playerSizeScale > 0f && s.playerSizeScale != 1f)
			{
				this.activeSizeChangerSettings = s;
			}
			else
			{
				this.activeSizeChangerSettings = null;
			}
			if (this.activeSizeChangerSettings == null)
			{
				this.nativeScale = 1f;
			}
			else
			{
				this.nativeScale = this.activeSizeChangerSettings.playerSizeScale;
			}
			if (num != this.nativeScale && NetworkSystem.Instance.InRoom)
			{
				GorillaTagger.Instance.myVRRig != null;
			}
		}

		public bool IsDefaultScale
		{
			get
			{
				return Mathf.Abs(1f - this.scale) < 0.001f;
			}
		}

		public bool turnedThisFrame
		{
			get
			{
				return this.degreesTurnedThisFrame != 0f;
			}
		}

		public List<GTPlayer.MaterialData> materialData
		{
			get
			{
				return this.materialDatasSO.datas;
			}
		}

		protected bool IsFrozen { get; set; }

		public List<WaterVolume> HeadOverlappingWaterVolumes
		{
			get
			{
				return this.headOverlappingWaterVolumes;
			}
		}

		public bool InWater
		{
			get
			{
				return this.bodyInWater;
			}
		}

		public bool HeadInWater
		{
			get
			{
				return this.headInWater;
			}
		}

		public WaterVolume CurrentWaterVolume
		{
			get
			{
				if (this.bodyOverlappingWaterVolumes.Count <= 0)
				{
					return null;
				}
				return this.bodyOverlappingWaterVolumes[0];
			}
		}

		public WaterVolume.SurfaceQuery WaterSurfaceForHead
		{
			get
			{
				return this.waterSurfaceForHead;
			}
		}

		public WaterVolume LeftHandWaterVolume
		{
			get
			{
				return this.leftHandWaterVolume;
			}
		}

		public WaterVolume RightHandWaterVolume
		{
			get
			{
				return this.rightHandWaterVolume;
			}
		}

		public WaterVolume.SurfaceQuery LeftHandWaterSurface
		{
			get
			{
				return this.leftHandWaterSurface;
			}
		}

		public WaterVolume.SurfaceQuery RightHandWaterSurface
		{
			get
			{
				return this.rightHandWaterSurface;
			}
		}

		public Vector3 LastLeftHandPosition
		{
			get
			{
				return this.leftHand.lastPosition;
			}
		}

		public Vector3 LastRightHandPosition
		{
			get
			{
				return this.rightHand.lastPosition;
			}
		}

		public Vector3 RigidbodyVelocity
		{
			get
			{
				return this.playerRigidBody.linearVelocity;
			}
		}

		public Vector3 HeadCenterPosition
		{
			get
			{
				return this.headCollider.transform.position + this.headCollider.transform.rotation * new Vector3(0f, 0f, -0.11f);
			}
		}

		public bool HandContactingSurface
		{
			get
			{
				return this.leftHand.isColliding || this.rightHand.isColliding;
			}
		}

		public bool BodyOnGround
		{
			get
			{
				return this.bodyGroundContactTime >= Time.time - 0.05f;
			}
		}

		public bool IsGroundedHand
		{
			get
			{
				return this.HandContactingSurface || this.isClimbing || this.leftHand.isHolding || this.rightHand.isHolding;
			}
		}

		public bool IsGroundedButt
		{
			get
			{
				return this.BodyOnGround;
			}
		}

		public int ThrusterActiveAtFrame { get; set; }

		public bool IsThrusterActive
		{
			get
			{
				return this.ThrusterActiveAtFrame == Time.frameCount;
			}
		}

		public Quaternion PlayerRotationOverride
		{
			set
			{
				this.playerRotationOverride = value;
				this.playerRotationOverrideFrame = Time.frameCount;
			}
		}

		public bool IsBodySliding { get; set; }

		public GorillaClimbable CurrentClimbable
		{
			get
			{
				return this.currentClimbable;
			}
		}

		public GorillaHandClimber CurrentClimber
		{
			get
			{
				return this.currentClimber;
			}
		}

		public float jumpMultiplier
		{
			get
			{
				return this._jumpMultiplier;
			}
			set
			{
				this._jumpMultiplier = value;
			}
		}

		public float LastTouchedGroundAtNetworkTime { get; private set; }

		public float LastHandTouchedGroundAtNetworkTime { get; private set; }

		public void EnableStilt(StiltID stiltID, bool isLeftHand, Vector3 currentTipWorldPos, float maxArmLength, bool canTag, bool canStun, float customBoostFactor = 0f, GorillaVelocityTracker velocityTracker = null)
		{
			this.stiltStates[(int)stiltID] = new GTPlayer.HandState
			{
				isActive = true,
				controllerTransform = (isLeftHand ? this.leftHand : this.rightHand).controllerTransform,
				velocityTracker = ((velocityTracker != null) ? velocityTracker : (isLeftHand ? this.leftHand : this.rightHand).velocityTracker),
				handRotOffset = Quaternion.identity,
				canTag = canTag,
				canStun = canStun,
				customBoostFactor = customBoostFactor,
				hasCustomBoost = (customBoostFactor > 0f)
			};
			this.stiltStates[(int)stiltID].Init(this, isLeftHand, maxArmLength);
			this.UpdateStiltOffset(stiltID, currentTipWorldPos);
		}

		public void DisableStilt(StiltID stiltID)
		{
			this.stiltStates[(int)stiltID].isActive = false;
		}

		public void UpdateStiltOffset(StiltID stiltID, Vector3 currentTipWorldPos)
		{
			this.stiltStates[(int)stiltID].handOffset = this.stiltStates[(int)stiltID].controllerTransform.InverseTransformPoint(currentTipWorldPos);
		}

		private void Awake()
		{
			if (GTPlayer._instance != null && GTPlayer._instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				GTPlayer._instance = this;
				GTPlayer.hasInstance = true;
			}
			this.InitializeValues();
			this.playerRigidbodyInterpolationDefault = this.playerRigidBody.interpolation;
			this.playerRigidBody.maxAngularVelocity = 0f;
			this.bodyOffsetVector = new Vector3(0f, -this.bodyCollider.height / 2f, 0f);
			this.bodyInitialHeight = this.bodyCollider.height;
			this.bodyInitialRadius = this.bodyCollider.radius;
			this.rayCastNonAllocColliders = new RaycastHit[5];
			this.crazyCheckVectors = new Vector3[7];
			this.emptyHit = default(RaycastHit);
			this.crazyCheckVectors[0] = Vector3.up;
			this.crazyCheckVectors[1] = Vector3.down;
			this.crazyCheckVectors[2] = Vector3.left;
			this.crazyCheckVectors[3] = Vector3.right;
			this.crazyCheckVectors[4] = Vector3.forward;
			this.crazyCheckVectors[5] = Vector3.back;
			this.crazyCheckVectors[6] = Vector3.zero;
			if (this.controllerState == null)
			{
				this.controllerState = base.GetComponent<ConnectedControllerHandler>();
			}
			this.layerChanger = base.GetComponent<LayerChanger>();
			this.bodyTouchedSurfaces = new Dictionary<GameObject, PhysicsMaterial>();
			if (Application.isPlaying)
			{
				Application.onBeforeRender += this.OnBeforeRenderInit;
			}
		}

		protected void Start()
		{
			if (this.mainCamera == null)
			{
				this.mainCamera = Camera.main;
			}
			this.mainCamera.farClipPlane = 500f;
			this.lastScale = this.scale;
			this.layerChanger.InitializeLayers(base.transform);
			float num = Quaternion.Angle(Quaternion.identity, GorillaTagger.Instance.offlineVRRig.transform.rotation) * Mathf.Sign(Vector3.Dot(Vector3.up, GorillaTagger.Instance.offlineVRRig.transform.right));
			this.Turn(num);
		}

		protected void OnDestroy()
		{
			if (GTPlayer._instance == this)
			{
				GTPlayer._instance = null;
				GTPlayer.hasInstance = false;
			}
			if (this.climbHelper)
			{
				Object.Destroy(this.climbHelper.gameObject);
			}
		}

		public void InitializeValues()
		{
			Physics.SyncTransforms();
			this.playerRigidBody = base.GetComponent<Rigidbody>();
			this.velocityHistory = new Vector3[this.velocityHistorySize];
			this.slideAverageHistory = new Vector3[this.velocityHistorySize];
			for (int i = 0; i < this.velocityHistory.Length; i++)
			{
				this.velocityHistory[i] = Vector3.zero;
				this.slideAverageHistory[i] = Vector3.zero;
			}
			this.leftHand.Init(this, true, this.maxArmLength);
			this.rightHand.Init(this, false, this.maxArmLength);
			this.lastHeadPosition = this.headCollider.transform.position;
			this.velocityIndex = 0;
			this.averagedVelocity = Vector3.zero;
			this.slideVelocity = Vector3.zero;
			this.lastPosition = base.transform.position;
			this.lastRealTime = Time.realtimeSinceStartup;
			this.lastOpenHeadPosition = this.headCollider.transform.position;
			this.bodyCollider.transform.position = this.PositionWithOffset(this.headCollider.transform, this.bodyOffset) + this.bodyOffsetVector;
			this.bodyCollider.transform.eulerAngles = new Vector3(0f, this.headCollider.transform.eulerAngles.y, 0f);
			this.ForceRigidBodySync();
		}

		public void SetHalloweenLevitation(float levitateStrength, float levitateDuration, float levitateBlendOutDuration, float levitateBonusStrength, float levitateBonusOffAtYSpeed, float levitateBonusFullAtYSpeed)
		{
			this.halloweenLevitationStrength = levitateStrength;
			this.halloweenLevitationFullStrengthDuration = levitateDuration;
			this.halloweenLevitationTotalDuration = levitateDuration + levitateBlendOutDuration;
			this.halloweenLevitateBonusFullAtYSpeed = levitateBonusFullAtYSpeed;
			this.halloweenLevitateBonusOffAtYSpeed = levitateBonusFullAtYSpeed;
			this.halloweenLevitationBonusStrength = levitateBonusStrength;
		}

		public void TeleportToTrain(bool enable)
		{
			this.teleportToTrain = enable;
		}

		public void TeleportTo(Vector3 position, Quaternion rotation, bool keepVelocity = false, bool center = false)
		{
			if (center)
			{
				Vector3 position2 = base.transform.position;
				Vector3 vector = this.mainCamera.transform.position - position2;
				position -= vector;
			}
			this.ClearHandHolds();
			if (this.playerRigidBody != null)
			{
				this.playerRigidBody.isKinematic = true;
				this.playerRigidBody.position = position;
				this.playerRigidBody.rotation = rotation;
				this.playerRigidBody.isKinematic = false;
			}
			this.playerRigidBody.position = position;
			this.playerRigidBody.rotation = rotation;
			base.transform.position = position;
			base.transform.rotation = rotation;
			this.lastHeadPosition = this.headCollider.transform.position;
			this.lastPosition = position;
			this.lastOpenHeadPosition = this.headCollider.transform.position;
			this.leftHand.OnTeleport();
			this.rightHand.OnTeleport();
			if (!keepVelocity)
			{
				this.playerRigidBody.linearVelocity = Vector3.zero;
			}
			this.bodyCollider.transform.position = this.PositionWithOffset(this.headCollider.transform, this.bodyOffset) + this.bodyOffsetVector;
			this.bodyCollider.transform.eulerAngles = new Vector3(0f, this.headCollider.transform.eulerAngles.y, 0f);
			Physics.SyncTransforms();
			GorillaTagger.Instance.offlineVRRig.transform.position = position;
			GorillaTagger.Instance.offlineVRRig.leftHandLink.BreakLink();
			GorillaTagger.Instance.offlineVRRig.rightHandLink.BreakLink();
			this.ForceRigidBodySync();
		}

		public void TeleportTo(Transform destination, bool matchDestinationRotation = true, bool maintainVelocity = true)
		{
			Vector3 position = base.transform.position;
			Vector3 vector = this.mainCamera.transform.position - position;
			Vector3 vector2 = destination.position - vector;
			float num = destination.rotation.eulerAngles.y - this.mainCamera.transform.rotation.eulerAngles.y;
			Vector3 vector3 = this.currentVelocity;
			if (!maintainVelocity)
			{
				this.SetPlayerVelocity(Vector3.zero);
			}
			else if (matchDestinationRotation)
			{
				vector3 = Quaternion.AngleAxis(num, base.transform.up) * this.currentVelocity;
				this.SetPlayerVelocity(vector3);
			}
			if (matchDestinationRotation)
			{
				this.Turn(num);
			}
			this.TeleportTo(vector2, base.transform.rotation, false, false);
			if (maintainVelocity)
			{
				this.SetPlayerVelocity(vector3);
			}
			this.ForceRigidBodySync();
		}

		public void AddForce(Vector3 force, ForceMode mode)
		{
			if (mode == ForceMode.VelocityChange)
			{
				this.playerRigidBody.AddForce(force * this.playerRigidBody.mass, ForceMode.Impulse);
				return;
			}
			if (mode == ForceMode.Acceleration)
			{
				this.playerRigidBody.AddForce(force * this.playerRigidBody.mass, ForceMode.Force);
				return;
			}
			this.playerRigidBody.AddForce(force, mode);
		}

		public void SetPlayerVelocity(Vector3 newVelocity)
		{
			for (int i = 0; i < this.velocityHistory.Length; i++)
			{
				this.velocityHistory[i] = newVelocity;
			}
			this.playerRigidBody.AddForce(newVelocity - this.playerRigidBody.linearVelocity, ForceMode.VelocityChange);
		}

		public void SetGravityOverride(Object caller, Action<GTPlayer> gravityFunction)
		{
			this.gravityOverrides[caller] = gravityFunction;
		}

		public void UnsetGravityOverride(Object caller)
		{
			this.gravityOverrides.Remove(caller);
		}

		private void ApplyGravityOverrides()
		{
			foreach (KeyValuePair<Object, Action<GTPlayer>> keyValuePair in this.gravityOverrides)
			{
				keyValuePair.Value(this);
			}
		}

		public void ApplyKnockback(Vector3 direction, float speed, bool forceOffTheGround = false)
		{
			if (forceOffTheGround)
			{
				if (this.leftHand.wasColliding || this.rightHand.wasColliding)
				{
					this.leftHand.wasColliding = false;
					this.rightHand.wasColliding = false;
					this.playerRigidBody.transform.position += this.minimumRaycastDistance * this.scale * Vector3.up;
				}
				this.didAJump = true;
				this.SetMaximumSlipThisFrame();
			}
			if (speed > 0.01f)
			{
				float num = Vector3.Dot(this.averagedVelocity, direction);
				float num2 = Mathf.InverseLerp(1.5f, 0.5f, num / speed);
				Vector3 vector = this.averagedVelocity + direction * speed * num2;
				this.playerRigidBody.linearVelocity = vector;
				for (int i = 0; i < this.velocityHistory.Length; i++)
				{
					this.velocityHistory[i] = vector;
				}
			}
		}

		public void FixedUpdate()
		{
			this.AntiTeleportTechnology();
			this.IsFrozen = GorillaTagger.Instance.offlineVRRig.IsFrozen || this.debugFreezeTag;
			bool isDefaultScale = this.IsDefaultScale;
			this.playerRigidBody.useGravity = false;
			if (this.gravityOverrides.Count > 0)
			{
				this.ApplyGravityOverrides();
			}
			else
			{
				if (!this.isClimbing)
				{
					this.playerRigidBody.AddForce(Physics.gravity * this.scale * this.playerRigidBody.mass, ForceMode.Force);
				}
				if (this.halloweenLevitationBonusStrength > 0f || this.halloweenLevitationStrength > 0f)
				{
					float num = Time.time - this.lastTouchedGroundTimestamp;
					if (num < this.halloweenLevitationTotalDuration)
					{
						this.playerRigidBody.AddForce(Vector3.up * this.halloweenLevitationStrength * Mathf.InverseLerp(this.halloweenLevitationFullStrengthDuration, this.halloweenLevitationTotalDuration, num) * this.playerRigidBody.mass, ForceMode.Force);
					}
					float y = this.playerRigidBody.linearVelocity.y;
					if (y <= this.halloweenLevitateBonusFullAtYSpeed)
					{
						this.playerRigidBody.AddForce(Vector3.up * this.halloweenLevitationBonusStrength * this.playerRigidBody.mass, ForceMode.Force);
					}
					else if (y <= this.halloweenLevitateBonusOffAtYSpeed)
					{
						Mathf.InverseLerp(this.halloweenLevitateBonusOffAtYSpeed, this.halloweenLevitateBonusFullAtYSpeed, this.playerRigidBody.linearVelocity.y);
						this.playerRigidBody.AddForce(Vector3.up * this.halloweenLevitationBonusStrength * Mathf.InverseLerp(this.halloweenLevitateBonusOffAtYSpeed, this.halloweenLevitateBonusFullAtYSpeed, this.playerRigidBody.linearVelocity.y) * this.playerRigidBody.mass, ForceMode.Force);
					}
				}
			}
			if (this.enableHoverMode)
			{
				this.playerRigidBody.linearVelocity = this.HoverboardFixedUpdate(this.playerRigidBody.linearVelocity);
			}
			else
			{
				this.didHoverLastFrame = false;
			}
			float fixedDeltaTime = Time.fixedDeltaTime;
			this.bodyInWater = false;
			Vector3 vector = this.swimmingVelocity;
			this.swimmingVelocity = Vector3.MoveTowards(this.swimmingVelocity, Vector3.zero, this.swimmingParams.swimmingVelocityOutOfWaterDrainRate * fixedDeltaTime);
			this.leftHandNonDiveHapticsAmount = 0f;
			this.rightHandNonDiveHapticsAmount = 0f;
			if (this.bodyOverlappingWaterVolumes.Count > 0)
			{
				WaterVolume waterVolume = null;
				float num2 = float.MinValue;
				Vector3 vector2 = this.headCollider.transform.position + Vector3.down * this.swimmingParams.floatingWaterLevelBelowHead * this.scale;
				this.activeWaterCurrents.Clear();
				for (int i = 0; i < this.bodyOverlappingWaterVolumes.Count; i++)
				{
					WaterVolume.SurfaceQuery surfaceQuery;
					if (this.bodyOverlappingWaterVolumes[i].GetSurfaceQueryForPoint(vector2, out surfaceQuery, false))
					{
						float num3 = Vector3.Dot(surfaceQuery.surfacePoint - vector2, surfaceQuery.surfaceNormal);
						if (num3 > num2)
						{
							num2 = num3;
							waterVolume = this.bodyOverlappingWaterVolumes[i];
							this.waterSurfaceForHead = surfaceQuery;
						}
						WaterCurrent waterCurrent = this.bodyOverlappingWaterVolumes[i].Current;
						if (waterCurrent != null && num3 > 0f && !this.activeWaterCurrents.Contains(waterCurrent))
						{
							this.activeWaterCurrents.Add(waterCurrent);
						}
					}
				}
				if (waterVolume != null)
				{
					Vector3 linearVelocity = this.playerRigidBody.linearVelocity;
					float magnitude = linearVelocity.magnitude;
					bool flag = this.headInWater;
					this.headInWater = this.headCollider.transform.position.y < this.waterSurfaceForHead.surfacePoint.y && this.headCollider.transform.position.y > this.waterSurfaceForHead.surfacePoint.y - this.waterSurfaceForHead.maxDepth;
					if (this.headInWater && !flag)
					{
						this.audioSetToUnderwater = true;
						this.audioManager.SetMixerSnapshot(this.audioManager.underwaterSnapshot, 0.1f);
					}
					else if (!this.headInWater && flag)
					{
						this.audioSetToUnderwater = false;
						this.audioManager.UnsetMixerSnapshot(0.1f);
					}
					this.bodyInWater = vector2.y < this.waterSurfaceForHead.surfacePoint.y && vector2.y > this.waterSurfaceForHead.surfacePoint.y - this.waterSurfaceForHead.maxDepth;
					if (this.bodyInWater)
					{
						GTPlayer.LiquidProperties liquidProperties = this.liquidPropertiesList[(int)waterVolume.LiquidType];
						if (waterVolume != null)
						{
							float num7;
							if (this.swimmingParams.extendBouyancyFromSpeed)
							{
								float num4 = Mathf.Clamp(Vector3.Dot(linearVelocity / this.scale, this.waterSurfaceForHead.surfaceNormal), this.swimmingParams.speedToBouyancyExtensionMinMax.x, this.swimmingParams.speedToBouyancyExtensionMinMax.y);
								float num5 = this.swimmingParams.speedToBouyancyExtension.Evaluate(num4);
								this.buoyancyExtension = Mathf.Max(this.buoyancyExtension, num5);
								float num6 = Mathf.InverseLerp(0f, this.swimmingParams.buoyancyFadeDist + this.buoyancyExtension, num2 / this.scale + this.buoyancyExtension);
								this.buoyancyExtension = Spring.DamperDecayExact(this.buoyancyExtension, this.swimmingParams.buoyancyExtensionDecayHalflife, fixedDeltaTime, 1E-05f);
								num7 = num6;
							}
							else
							{
								num7 = Mathf.InverseLerp(0f, this.swimmingParams.buoyancyFadeDist, num2 / this.scale);
							}
							Vector3 vector3 = Physics.gravity * this.scale;
							Vector3 vector4 = liquidProperties.buoyancy * -vector3 * num7;
							if (this.IsFrozen && GorillaGameManager.instance is GorillaFreezeTagManager)
							{
								vector4 *= this.frozenBodyBuoyancyFactor;
							}
							this.playerRigidBody.AddForce(vector4 * this.playerRigidBody.mass, ForceMode.Force);
						}
						Vector3 vector5 = Vector3.zero;
						Vector3 vector6 = Vector3.zero;
						for (int j = 0; j < this.activeWaterCurrents.Count; j++)
						{
							WaterCurrent waterCurrent2 = this.activeWaterCurrents[j];
							Vector3 vector7 = linearVelocity + vector5;
							Vector3 vector8;
							Vector3 vector9;
							if (waterCurrent2.GetCurrentAtPoint(this.bodyCollider.transform.position, vector7, fixedDeltaTime, out vector8, out vector9))
							{
								vector6 += vector8;
								vector5 += vector9;
							}
						}
						if (magnitude > Mathf.Epsilon)
						{
							float num8 = 0.01f;
							Vector3 vector10 = linearVelocity / magnitude;
							Vector3 right = this.leftHand.handFollower.right;
							Vector3 vector11 = -this.rightHand.handFollower.right;
							Vector3 forward = this.leftHand.handFollower.forward;
							Vector3 forward2 = this.rightHand.handFollower.forward;
							Vector3 vector12 = vector10;
							float num9 = 0f;
							float num10 = 0f;
							float num11 = 0f;
							if (this.swimmingParams.applyDiveSteering && !this.disableMovement && isDefaultScale)
							{
								float num12 = Vector3.Dot(linearVelocity - vector6, vector10);
								float num13 = Mathf.Clamp(num12, this.swimmingParams.swimSpeedToRedirectAmountMinMax.x, this.swimmingParams.swimSpeedToRedirectAmountMinMax.y);
								float num14 = this.swimmingParams.swimSpeedToRedirectAmount.Evaluate(num13);
								num13 = Mathf.Clamp(num12, this.swimmingParams.swimSpeedToMaxRedirectAngleMinMax.x, this.swimmingParams.swimSpeedToMaxRedirectAngleMinMax.y);
								float num15 = this.swimmingParams.swimSpeedToMaxRedirectAngle.Evaluate(num13);
								float num16 = Mathf.Acos(Vector3.Dot(vector10, forward)) / 3.1415927f * -2f + 1f;
								float num17 = Mathf.Acos(Vector3.Dot(vector10, forward2)) / 3.1415927f * -2f + 1f;
								float num18 = Mathf.Clamp(num16, this.swimmingParams.palmFacingToRedirectAmountMinMax.x, this.swimmingParams.palmFacingToRedirectAmountMinMax.y);
								float num19 = Mathf.Clamp(num17, this.swimmingParams.palmFacingToRedirectAmountMinMax.x, this.swimmingParams.palmFacingToRedirectAmountMinMax.y);
								float num20 = ((!float.IsNaN(num18)) ? this.swimmingParams.palmFacingToRedirectAmount.Evaluate(num18) : 0f);
								float num21 = ((!float.IsNaN(num19)) ? this.swimmingParams.palmFacingToRedirectAmount.Evaluate(num19) : 0f);
								Vector3 vector13 = Vector3.ProjectOnPlane(vector10, right);
								Vector3 vector14 = Vector3.ProjectOnPlane(vector10, right);
								float num22 = Mathf.Min(vector13.magnitude, 1f);
								float num23 = Mathf.Min(vector14.magnitude, 1f);
								float magnitude2 = this.leftHand.velocityTracker.GetAverageVelocity(false, this.swimmingParams.diveVelocityAveragingWindow, false).magnitude;
								float magnitude3 = this.rightHand.velocityTracker.GetAverageVelocity(false, this.swimmingParams.diveVelocityAveragingWindow, false).magnitude;
								float num24 = Mathf.Clamp(magnitude2, this.swimmingParams.handSpeedToRedirectAmountMinMax.x, this.swimmingParams.handSpeedToRedirectAmountMinMax.y);
								float num25 = Mathf.Clamp(magnitude3, this.swimmingParams.handSpeedToRedirectAmountMinMax.x, this.swimmingParams.handSpeedToRedirectAmountMinMax.y);
								float num26 = this.swimmingParams.handSpeedToRedirectAmount.Evaluate(num24);
								float num27 = this.swimmingParams.handSpeedToRedirectAmount.Evaluate(num25);
								float averageSpeedChangeMagnitudeInDirection = this.leftHand.velocityTracker.GetAverageSpeedChangeMagnitudeInDirection(right, false, this.swimmingParams.diveVelocityAveragingWindow);
								float averageSpeedChangeMagnitudeInDirection2 = this.rightHand.velocityTracker.GetAverageSpeedChangeMagnitudeInDirection(vector11, false, this.swimmingParams.diveVelocityAveragingWindow);
								float num28 = Mathf.Clamp(averageSpeedChangeMagnitudeInDirection, this.swimmingParams.handAccelToRedirectAmountMinMax.x, this.swimmingParams.handAccelToRedirectAmountMinMax.y);
								float num29 = Mathf.Clamp(averageSpeedChangeMagnitudeInDirection2, this.swimmingParams.handAccelToRedirectAmountMinMax.x, this.swimmingParams.handAccelToRedirectAmountMinMax.y);
								float num30 = this.swimmingParams.handAccelToRedirectAmount.Evaluate(num28);
								float num31 = this.swimmingParams.handAccelToRedirectAmount.Evaluate(num29);
								num9 = Mathf.Min(num20, Mathf.Min(num26, num30));
								float num32 = ((Vector3.Dot(vector10, forward) > 0f) ? (Mathf.Min(num9, num14) * num22) : 0f);
								num10 = Mathf.Min(num21, Mathf.Min(num27, num31));
								float num33 = ((Vector3.Dot(vector10, forward2) > 0f) ? (Mathf.Min(num10, num14) * num23) : 0f);
								if (this.swimmingParams.reduceDiveSteeringBelowVelocityPlane)
								{
									Vector3 vector15;
									if (Vector3.Dot(this.headCollider.transform.up, vector10) > 0.95f)
									{
										vector15 = -this.headCollider.transform.forward;
									}
									else
									{
										vector15 = Vector3.Cross(Vector3.Cross(vector10, this.headCollider.transform.up), vector10).normalized;
									}
									Vector3 position = this.headCollider.transform.position;
									Vector3 vector16 = position - this.leftHand.handFollower.position;
									Vector3 vector17 = position - this.rightHand.handFollower.position;
									float reduceDiveSteeringBelowPlaneFadeStartDist = this.swimmingParams.reduceDiveSteeringBelowPlaneFadeStartDist;
									float reduceDiveSteeringBelowPlaneFadeEndDist = this.swimmingParams.reduceDiveSteeringBelowPlaneFadeEndDist;
									float num34 = Vector3.Dot(vector16, Vector3.up);
									float num35 = Vector3.Dot(vector17, Vector3.up);
									float num36 = Vector3.Dot(vector16, vector15);
									float num37 = Vector3.Dot(vector17, vector15);
									float num38 = 1f - Mathf.InverseLerp(reduceDiveSteeringBelowPlaneFadeStartDist, reduceDiveSteeringBelowPlaneFadeEndDist, Mathf.Min(Mathf.Abs(num34), Mathf.Abs(num36)));
									float num39 = 1f - Mathf.InverseLerp(reduceDiveSteeringBelowPlaneFadeStartDist, reduceDiveSteeringBelowPlaneFadeEndDist, Mathf.Min(Mathf.Abs(num35), Mathf.Abs(num37)));
									num32 *= num38;
									num33 *= num39;
								}
								float num40 = num33 + num32;
								Vector3 vector18 = Vector3.zero;
								if (this.swimmingParams.applyDiveSteering && num40 > num8)
								{
									vector18 = ((num32 * vector13 + num33 * vector14) / num40).normalized;
									vector18 = Vector3.Lerp(vector10, vector18, num40);
									vector12 = Vector3.RotateTowards(vector10, vector18, 0.017453292f * num15 * fixedDeltaTime, 0f);
								}
								else
								{
									vector12 = vector10;
								}
								num11 = Mathf.Clamp01((num9 + num10) * 0.5f);
							}
							float num41 = Mathf.Clamp(Vector3.Dot(vector, vector10), 0f, magnitude);
							float num42 = magnitude - num41;
							if (this.swimmingParams.applyDiveSwimVelocityConversion && !this.disableMovement && num11 > num8 && num41 < this.swimmingParams.diveMaxSwimVelocityConversion)
							{
								float num43 = Mathf.Min(this.swimmingParams.diveSwimVelocityConversionRate * fixedDeltaTime, num42) * num11;
								num41 += num43;
								num42 -= num43;
							}
							float num44 = this.swimmingParams.swimUnderWaterDampingHalfLife * liquidProperties.dampingFactor;
							float num45 = this.swimmingParams.baseUnderWaterDampingHalfLife * liquidProperties.dampingFactor;
							float num46 = Spring.DamperDecayExact(num41 / this.scale, num44, fixedDeltaTime, 1E-05f) * this.scale;
							float num47 = Spring.DamperDecayExact(num42 / this.scale, num45, fixedDeltaTime, 1E-05f) * this.scale;
							if (this.swimmingParams.applyDiveDampingMultiplier && !this.disableMovement)
							{
								float num48 = Mathf.Lerp(1f, this.swimmingParams.diveDampingMultiplier, num11);
								num46 = Mathf.Lerp(num41, num46, num48);
								num47 = Mathf.Lerp(num42, num47, num48);
								float num49 = Mathf.Clamp((1f - num9) * (num41 + num42), this.swimmingParams.nonDiveDampingHapticsAmountMinMax.x + num8, this.swimmingParams.nonDiveDampingHapticsAmountMinMax.y - num8);
								float num50 = Mathf.Clamp((1f - num10) * (num41 + num42), this.swimmingParams.nonDiveDampingHapticsAmountMinMax.x + num8, this.swimmingParams.nonDiveDampingHapticsAmountMinMax.y - num8);
								this.leftHandNonDiveHapticsAmount = this.swimmingParams.nonDiveDampingHapticsAmount.Evaluate(num49);
								this.rightHandNonDiveHapticsAmount = this.swimmingParams.nonDiveDampingHapticsAmount.Evaluate(num50);
							}
							this.swimmingVelocity = num46 * vector12 + vector5 * this.scale;
							this.playerRigidBody.linearVelocity = this.swimmingVelocity + num47 * vector12;
						}
					}
				}
			}
			else if (this.audioSetToUnderwater)
			{
				this.audioSetToUnderwater = false;
				this.audioManager.UnsetMixerSnapshot(0.1f);
			}
			this.handleClimbing(Time.fixedDeltaTime);
			this.stuckHandsCheckFixedUpdate();
			this.FixedUpdate_HandHolds(Time.fixedDeltaTime);
		}

		public bool isHoverAllowed { get; private set; }

		public bool enableHoverMode { get; private set; }

		public void SetHoverboardPosRot(Vector3 worldPos, Quaternion worldRot)
		{
			this.hoverboardPlayerLocalPos = this.headCollider.transform.InverseTransformPoint(worldPos);
			this.hoverboardPlayerLocalRot = this.headCollider.transform.InverseTransformRotation(worldRot);
		}

		private void HoverboardLateUpdate()
		{
			Vector3 eulerAngles = this.headCollider.transform.eulerAngles;
			bool flag = false;
			for (int i = 0; i < this.hoverboardCasts.Length; i++)
			{
				GTPlayer.HoverBoardCast hoverBoardCast = this.hoverboardCasts[i];
				RaycastHit raycastHit;
				hoverBoardCast.didHit = Physics.SphereCast(new Ray(this.hoverboardVisual.transform.TransformPoint(hoverBoardCast.localOrigin), this.hoverboardVisual.transform.rotation * hoverBoardCast.localDirection), hoverBoardCast.sphereRadius, out raycastHit, hoverBoardCast.distance, this.locomotionEnabledLayers);
				if (hoverBoardCast.didHit)
				{
					HoverboardCantHover hoverboardCantHover;
					if (raycastHit.collider.TryGetComponent<HoverboardCantHover>(out hoverboardCantHover))
					{
						hoverBoardCast.didHit = false;
					}
					else
					{
						hoverBoardCast.pointHit = raycastHit.point;
						hoverBoardCast.normalHit = raycastHit.normal;
					}
				}
				this.hoverboardCasts[i] = hoverBoardCast;
				if (hoverBoardCast.didHit)
				{
					flag = true;
				}
			}
			this.hasHoverPoint = flag;
			this.bodyCollider.enabled = (this.bodyCollider.transform.position - this.hoverboardVisual.transform.TransformPoint(Vector3.up * this.hoverBodyCollisionRadiusUpOffset)).IsLongerThan(this.hoverBodyHasCollisionsOutsideRadius);
		}

		private Vector3 HoverboardFixedUpdate(Vector3 velocity)
		{
			this.hoverboardVisual.transform.position = this.headCollider.transform.TransformPoint(this.hoverboardPlayerLocalPos);
			this.hoverboardVisual.transform.rotation = this.headCollider.transform.TransformRotation(this.hoverboardPlayerLocalRot);
			if (this.didHoverLastFrame)
			{
				velocity += Vector3.up * this.hoverGeneralUpwardForce * Time.fixedDeltaTime;
			}
			Vector3 position = this.hoverboardVisual.transform.position;
			Vector3 vector = position + velocity * Time.fixedDeltaTime;
			Vector3 vector2 = this.hoverboardVisual.transform.forward;
			Vector3 vector3 = (this.hoverboardCasts[0].didHit ? this.hoverboardCasts[0].normalHit : Vector3.up);
			bool flag = false;
			for (int i = 0; i < this.hoverboardCasts.Length; i++)
			{
				GTPlayer.HoverBoardCast hoverBoardCast = this.hoverboardCasts[i];
				if (hoverBoardCast.didHit)
				{
					Vector3 vector4 = position + Vector3.Project(hoverBoardCast.pointHit - position, vector2);
					Vector3 vector5 = vector + Vector3.Project(hoverBoardCast.pointHit - position, vector2);
					bool flag2 = hoverBoardCast.isSolid || Vector3.Dot(hoverBoardCast.normalHit, hoverBoardCast.pointHit - vector5) + this.hoverIdealHeight > 0f;
					float num = (hoverBoardCast.isSolid ? (Vector3.Dot(hoverBoardCast.normalHit, hoverBoardCast.pointHit - this.hoverboardVisual.transform.TransformPoint(hoverBoardCast.localOrigin + hoverBoardCast.localDirection * hoverBoardCast.distance)) + hoverBoardCast.sphereRadius) : (Vector3.Dot(hoverBoardCast.normalHit, hoverBoardCast.pointHit - vector4) + this.hoverIdealHeight));
					if (flag2)
					{
						flag = true;
						this.boostEnabledUntilTimestamp = Time.time + this.hoverboardBoostGracePeriod;
						if (Vector3.Dot(velocity, hoverBoardCast.normalHit) < 0f)
						{
							velocity = Vector3.ProjectOnPlane(velocity, hoverBoardCast.normalHit);
						}
						this.playerRigidBody.transform.position += hoverBoardCast.normalHit * num;
						Vector3 vector6 = this.turnParent.transform.rotation * (this.hoverboardVisual.IsLeftHanded ? this.leftHand.velocityTracker : this.rightHand.velocityTracker).GetAverageVelocity(false, 0.15f, false);
						if (Vector3.Dot(vector6, hoverBoardCast.normalHit) < 0f)
						{
							velocity -= Vector3.Project(vector6, hoverBoardCast.normalHit) * this.hoverSlamJumpStrengthFactor * Time.fixedDeltaTime;
						}
						vector = position + velocity * Time.fixedDeltaTime;
					}
				}
			}
			float num2 = Mathf.Abs(Mathf.DeltaAngle(0f, Mathf.Acos(Vector3.Dot(this.hoverboardVisual.transform.up, Vector3.ProjectOnPlane(vector3, vector2).normalized)) * 57.29578f));
			float num3 = this.hoverCarveAngleResponsiveness.Evaluate(num2);
			vector2 = (vector2 + Vector3.ProjectOnPlane(this.hoverboardVisual.transform.up, vector3) * this.hoverTiltAdjustsForwardFactor).normalized;
			if (!flag)
			{
				this.didHoverLastFrame = false;
				num3 = 0f;
			}
			Vector3 vector7 = velocity;
			if (this.enableHoverMode && this.hasHoverPoint)
			{
				Vector3 vector8 = Vector3.ProjectOnPlane(velocity, vector3);
				Vector3 vector9 = velocity - vector8;
				Vector3 vector10 = Vector3.Project(vector8, vector2);
				float num4 = vector8.magnitude;
				if (num4 <= this.hoveringSlowSpeed)
				{
					num4 *= this.hoveringSlowStoppingFactor;
				}
				Vector3 vector11 = vector8 - vector10;
				float num5 = 0f;
				bool flag3 = false;
				if (num3 > 0f)
				{
					if (vector11.IsLongerThan(vector10))
					{
						num5 = Mathf.Min((vector11.magnitude - vector10.magnitude) * this.hoverCarveSidewaysSpeedLossFactor * num3, num4);
						if (num5 > 0f && num4 > this.hoverMinGrindSpeed)
						{
							flag3 = true;
							this.hoverboardVisual.PlayGrindHaptic();
						}
						num4 -= num5;
					}
					vector11 *= 1f - num3 * this.sidewaysDrag;
					if (!this.leftHand.isColliding && !this.rightHand.isColliding)
					{
						velocity = (vector10 + vector11).normalized * num4 + vector9;
					}
				}
				else
				{
					velocity = vector8.normalized * num4 + vector9;
				}
				float magnitude = (velocity - vector7).magnitude;
				this.hoverboardAudio.UpdateAudioLoop(velocity.magnitude, this.bodyVelocityTracker.GetAverageVelocity(true, 0.15f, false).magnitude, magnitude, flag3 ? num5 : 0f);
				if (magnitude > 0f && !flag3)
				{
					this.hoverboardVisual.PlayCarveHaptic(magnitude);
				}
			}
			else
			{
				this.hoverboardAudio.UpdateAudioLoop(0f, this.bodyVelocityTracker.GetAverageVelocity(true, 0.15f, false).magnitude, 0f, 0f);
			}
			return velocity;
		}

		public void GrabPersonalHoverboard(bool isLeftHand, Vector3 pos, Quaternion rot, Color col)
		{
			if (this.hoverboardVisual.IsHeld)
			{
				this.hoverboardVisual.DropFreeBoard();
			}
			this.hoverboardVisual.SetIsHeld(isLeftHand, pos, rot, col);
			this.hoverboardVisual.ProxyGrabHandle(isLeftHand);
			FreeHoverboardManager.instance.PreserveMaxHoverboardsConstraint(NetworkSystem.Instance.LocalPlayer.ActorNumber);
		}

		public void SetHoverAllowed(bool allowed, bool force = false)
		{
			if (allowed)
			{
				this.hoverAllowedCount++;
				this.isHoverAllowed = true;
				return;
			}
			this.hoverAllowedCount = ((force || this.hoverAllowedCount == 0) ? 0 : (this.hoverAllowedCount - 1));
			if (this.hoverAllowedCount == 0 && this.isHoverAllowed)
			{
				this.isHoverAllowed = false;
				if (this.enableHoverMode)
				{
					this.SetHoverActive(false);
					VRRig.LocalRig.hoverboardVisual.SetNotHeld();
				}
			}
		}

		public void SetHoverActive(bool enable)
		{
			if (enable && !this.isHoverAllowed)
			{
				return;
			}
			this.enableHoverMode = enable;
			if (!enable)
			{
				this.bodyCollider.enabled = true;
				this.hasHoverPoint = false;
				this.didHoverLastFrame = false;
				for (int i = 0; i < this.hoverboardCasts.Length; i++)
				{
					this.hoverboardCasts[i].didHit = false;
				}
				this.hoverboardAudio.Stop();
			}
		}

		private void BodyCollider()
		{
			if (this.MaxSphereSizeForNoOverlap(this.bodyInitialRadius * this.scale, this.PositionWithOffset(this.headCollider.transform, this.bodyOffset), false, out this.bodyMaxRadius))
			{
				if (this.scale > 0f)
				{
					this.bodyCollider.radius = this.bodyMaxRadius / this.scale;
				}
				if (Physics.SphereCast(this.PositionWithOffset(this.headCollider.transform, this.bodyOffset), this.bodyMaxRadius, Vector3.down, out this.bodyHitInfo, this.bodyInitialHeight * this.scale - this.bodyMaxRadius, this.locomotionEnabledLayers, QueryTriggerInteraction.Ignore))
				{
					this.bodyCollider.height = (this.bodyHitInfo.distance + this.bodyMaxRadius) / this.scale;
				}
				else
				{
					this.bodyHitInfo = this.emptyHit;
					this.bodyCollider.height = this.bodyInitialHeight;
				}
				if (!this.bodyCollider.gameObject.activeSelf)
				{
					this.bodyCollider.gameObject.SetActive(true);
				}
			}
			else
			{
				this.bodyCollider.gameObject.SetActive(false);
			}
			this.bodyCollider.height = Mathf.Lerp(this.bodyCollider.height, this.bodyInitialHeight, this.bodyLerp);
			this.bodyCollider.radius = Mathf.Lerp(this.bodyCollider.radius, this.bodyInitialRadius, this.bodyLerp);
			this.bodyOffsetVector = Vector3.down * this.bodyCollider.height / 2f;
			this.bodyCollider.transform.position = this.PositionWithOffset(this.headCollider.transform, this.bodyOffset) + this.bodyOffsetVector * this.scale;
			this.bodyCollider.transform.eulerAngles = new Vector3(0f, this.headCollider.transform.eulerAngles.y, 0f);
		}

		private Vector3 PositionWithOffset(Transform transformToModify, Vector3 offsetVector)
		{
			return transformToModify.position + transformToModify.rotation * offsetVector * this.scale;
		}

		public void ScaleAwayFromPoint(float oldScale, float newScale, Vector3 scaleCenter)
		{
			if (oldScale < newScale)
			{
				this.lastHeadPosition = GTPlayer.ScalePointAwayFromCenter(this.lastHeadPosition, this.headCollider.radius, oldScale, newScale, scaleCenter);
				this.leftHand.lastPosition = GTPlayer.ScalePointAwayFromCenter(this.leftHand.lastPosition, this.minimumRaycastDistance, oldScale, newScale, scaleCenter);
				this.rightHand.lastPosition = GTPlayer.ScalePointAwayFromCenter(this.rightHand.lastPosition, this.minimumRaycastDistance, oldScale, newScale, scaleCenter);
			}
		}

		private static Vector3 ScalePointAwayFromCenter(Vector3 point, float baseRadius, float oldScale, float newScale, Vector3 scaleCenter)
		{
			float magnitude = (point - scaleCenter).magnitude;
			float num = magnitude + Mathf.Epsilon + baseRadius * (newScale - oldScale);
			return scaleCenter + (point - scaleCenter) * num / magnitude;
		}

		private void OnBeforeRenderInit()
		{
			if (Application.isPlaying && !this.hasCorrectedForTracking && this.mainCamera != null && this.mainCamera.transform.localPosition != Vector3.zero)
			{
				this.ForceRigidBodySync();
				base.transform.position -= this.mainCamera.transform.localPosition;
				this.hasCorrectedForTracking = true;
			}
			Application.onBeforeRender -= this.OnBeforeRenderInit;
		}

		private void LateUpdate()
		{
			Vector3 vector = this.antiDriftLastPosition.GetValueOrDefault();
			if (this.antiDriftLastPosition == null)
			{
				vector = base.transform.position;
				this.antiDriftLastPosition = new Vector3?(vector);
			}
			if ((double)(this.antiDriftLastPosition.Value - base.transform.position).sqrMagnitude < 1E-08)
			{
				base.transform.position = this.antiDriftLastPosition.Value;
			}
			else
			{
				this.antiDriftLastPosition = new Vector3?(base.transform.position);
			}
			if (!this.hasCorrectedForTracking && this.mainCamera.transform.localPosition != Vector3.zero)
			{
				base.transform.position -= this.mainCamera.transform.localPosition;
				this.hasCorrectedForTracking = true;
				Application.onBeforeRender -= this.OnBeforeRenderInit;
			}
			if (this.playerRigidBody.isKinematic)
			{
				return;
			}
			float time = Time.time;
			Vector3 position = this.headCollider.transform.position;
			if (this.playerRotationOverrideFrame < Time.frameCount - 1)
			{
				this.playerRotationOverride = Quaternion.Slerp(Quaternion.identity, this.playerRotationOverride, Mathf.Exp(-this.playerRotationOverrideDecayRate * Time.deltaTime));
			}
			base.transform.rotation = this.playerRotationOverride;
			this.turnParent.transform.localScale = VRRig.LocalRig.transform.localScale;
			this.playerRigidBody.MovePosition(this.playerRigidBody.position + position - this.headCollider.transform.position);
			if (Mathf.Abs(this.lastScale - this.scale) > 0.001f)
			{
				if (this.mainCamera == null)
				{
					this.mainCamera = Camera.main;
				}
				this.mainCamera.nearClipPlane = ((this.scale > 0.5f) ? 0.01f : 0.002f);
			}
			this.lastScale = this.scale;
			this.debugLastRightHandPosition = this.rightHand.lastPosition;
			this.debugPlatformDeltaPosition = this.MovingSurfaceMovement();
			if (this.debugMovement)
			{
				this.tempRealTime = Time.time;
				this.calcDeltaTime = Time.deltaTime;
				this.lastRealTime = this.tempRealTime;
			}
			else
			{
				this.tempRealTime = Time.realtimeSinceStartup;
				this.calcDeltaTime = this.tempRealTime - this.lastRealTime;
				this.lastRealTime = this.tempRealTime;
				if (this.calcDeltaTime > 0.1f)
				{
					this.calcDeltaTime = 0.05f;
				}
			}
			Vector3 vector2;
			if (this.lastFrameHasValidTouchPos && this.lastPlatformTouched != null && GTPlayer.ComputeWorldHitPoint(this.lastHitInfoHand, this.lastFrameTouchPosLocal, out vector2))
			{
				this.refMovement = vector2 - this.lastFrameTouchPosWorld;
			}
			else
			{
				this.refMovement = Vector3.zero;
			}
			Vector3 vector3 = Vector3.zero;
			Quaternion quaternion = Quaternion.identity;
			Vector3 vector4 = this.headCollider.transform.position;
			Vector3 vector5;
			if (this.lastMovingSurfaceContact != GTPlayer.MovingSurfaceContactPoint.NONE && GTPlayer.ComputeWorldHitPoint(this.lastMovingSurfaceHit, this.lastMovingSurfaceTouchLocal, out vector5))
			{
				if (this.wasMovingSurfaceMonkeBlock && (this.lastMonkeBlock == null || this.lastMonkeBlock.state != BuilderPiece.State.AttachedAndPlaced))
				{
					this.movingSurfaceOffset = Vector3.zero;
				}
				else
				{
					this.movingSurfaceOffset = vector5 - this.lastMovingSurfaceTouchWorld;
					vector3 = this.movingSurfaceOffset / this.calcDeltaTime;
					quaternion = this.lastMovingSurfaceHit.collider.transform.rotation * Quaternion.Inverse(this.lastMovingSurfaceRot);
					vector4 = vector5;
				}
			}
			else
			{
				this.movingSurfaceOffset = Vector3.zero;
			}
			float num = 40f * this.scale;
			if (vector3.sqrMagnitude >= num * num)
			{
				this.movingSurfaceOffset = Vector3.zero;
				vector3 = Vector3.zero;
				quaternion = Quaternion.identity;
			}
			if (!this.didAJump && (this.leftHand.wasColliding || this.rightHand.wasColliding))
			{
				base.transform.position = base.transform.position + 4.9f * Vector3.down * this.calcDeltaTime * this.calcDeltaTime * this.scale;
				if (Vector3.Dot(this.averagedVelocity, this.slideAverageNormal) <= 0f && Vector3.Dot(Vector3.up, this.slideAverageNormal) > 0f)
				{
					base.transform.position = base.transform.position - Vector3.Project(Mathf.Min(this.stickDepth * this.scale, Vector3.Project(this.averagedVelocity, this.slideAverageNormal).magnitude * this.calcDeltaTime) * this.slideAverageNormal, Vector3.down);
				}
			}
			if (!this.didAJump && this.anyHandWasSliding)
			{
				base.transform.position = base.transform.position + this.slideVelocity * this.calcDeltaTime;
				this.slideVelocity += 9.8f * Vector3.down * this.calcDeltaTime * this.scale;
			}
			float num2 = ((Time.time > this.boostEnabledUntilTimestamp) ? 0f : (Time.deltaTime * Mathf.Clamp(this.playerRigidBody.linearVelocity.magnitude * this.hoverboardPaddleBoostMultiplier, 0f, this.hoverboardPaddleBoostMax)));
			int num3 = 0;
			Vector3 vector6 = Vector3.zero;
			this.anyHandIsColliding = false;
			this.anyHandIsSliding = false;
			this.anyHandIsSticking = false;
			this.leftHand.FirstIteration(ref vector6, ref num3, num2);
			this.rightHand.FirstIteration(ref vector6, ref num3, num2);
			for (int i = 0; i < 12; i++)
			{
				if (this.stiltStates[i].isActive)
				{
					this.stiltStates[i].FirstIteration(ref vector6, ref num3, 0f);
				}
			}
			if (num3 != 0)
			{
				vector6 /= (float)num3;
			}
			if (this.lastMovingSurfaceContact == GTPlayer.MovingSurfaceContactPoint.RIGHT || this.lastMovingSurfaceContact == GTPlayer.MovingSurfaceContactPoint.LEFT)
			{
				vector6 += this.movingSurfaceOffset;
			}
			else if (this.lastMovingSurfaceContact == GTPlayer.MovingSurfaceContactPoint.BODY)
			{
				Vector3 vector7 = this.lastHeadPosition + this.movingSurfaceOffset - this.headCollider.transform.position;
				vector6 += vector7;
			}
			if (!this.MaxSphereSizeForNoOverlap(this.headCollider.radius * 0.9f * this.scale, this.lastHeadPosition, true, out this.maxSphereSize1) && !this.CrazyCheck2(this.headCollider.radius * 0.9f * 0.75f * this.scale, this.lastHeadPosition))
			{
				this.lastHeadPosition = this.lastOpenHeadPosition;
			}
			Vector3 vector8;
			float num4;
			if (this.IterativeCollisionSphereCast(this.lastHeadPosition, this.headCollider.radius * 0.9f * this.scale, this.headCollider.transform.position + vector6 - this.lastHeadPosition, Vector3.zero, out vector8, false, out num4, out this.junkHit, true))
			{
				vector6 = vector8 - this.headCollider.transform.position;
			}
			if (!this.MaxSphereSizeForNoOverlap(this.headCollider.radius * 0.9f * this.scale, this.lastHeadPosition + vector6, true, out this.maxSphereSize1) || !this.CrazyCheck2(this.headCollider.radius * 0.9f * 0.75f * this.scale, this.lastHeadPosition + vector6))
			{
				this.lastHeadPosition = this.lastOpenHeadPosition;
				vector6 = this.lastHeadPosition - this.headCollider.transform.position;
			}
			else if (this.headCollider.radius * 0.9f * 0.825f * this.scale < this.maxSphereSize1)
			{
				this.lastOpenHeadPosition = this.headCollider.transform.position + vector6;
			}
			if (vector6 != Vector3.zero)
			{
				base.transform.position += vector6;
			}
			if (this.lastMovingSurfaceContact != GTPlayer.MovingSurfaceContactPoint.NONE && quaternion != Quaternion.identity && !this.isClimbing && !this.rightHand.isHolding && !this.leftHand.isHolding)
			{
				this.RotateWithSurface(quaternion, vector4);
			}
			this.lastHeadPosition = this.headCollider.transform.position;
			this.areBothTouching = (!this.leftHand.isColliding && !this.leftHand.wasColliding) || (!this.rightHand.isColliding && !this.rightHand.wasColliding);
			this.HandleHandLink();
			this.HandleTentacleMovement();
			this.anyHandIsColliding = false;
			this.anyHandIsSliding = false;
			this.anyHandIsSticking = false;
			this.leftHand.FinalizeHandPosition();
			this.rightHand.FinalizeHandPosition();
			for (int j = 0; j < 12; j++)
			{
				if (this.stiltStates[j].isActive)
				{
					this.stiltStates[j].FinalizeHandPosition();
					GTPlayer.HandState handState = this.stiltStates[j];
					GorillaTagger.Instance.SetExtraHandPosition((StiltID)j, handState.finalPositionThisFrame, handState.canTag, handState.canStun);
				}
			}
			Vector3 vector9 = this.lastPosition;
			GTPlayer.MovingSurfaceContactPoint movingSurfaceContactPoint = GTPlayer.MovingSurfaceContactPoint.NONE;
			int num5 = -1;
			int num6 = -1;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = this.rightHand.isColliding && this.IsTouchingMovingSurface(this.rightHand.GetLastPosition(), this.rightHand.lastHitInfo, out num5, out flag, out flag2);
			if (flag4 && !flag)
			{
				movingSurfaceContactPoint = GTPlayer.MovingSurfaceContactPoint.RIGHT;
				this.lastMovingSurfaceHit = this.rightHand.lastHitInfo;
			}
			else
			{
				bool flag5 = false;
				BuilderPiece builderPiece = (flag4 ? this.lastMonkeBlock : null);
				if (this.leftHand.isColliding && this.IsTouchingMovingSurface(this.leftHand.GetLastPosition(), this.leftHand.lastHitInfo, out num6, out flag5, out flag3))
				{
					if (flag5 && flag2 == flag3)
					{
						if (flag && num6.Equals(num5) && (double)Vector3.Dot(this.leftHand.lastHitInfo.point - this.leftHand.GetLastPosition(), this.rightHand.lastHitInfo.point - this.rightHand.GetLastPosition()) < 0.3)
						{
							movingSurfaceContactPoint = GTPlayer.MovingSurfaceContactPoint.RIGHT;
							this.lastMovingSurfaceHit = this.rightHand.lastHitInfo;
							this.lastMonkeBlock = builderPiece;
						}
					}
					else
					{
						movingSurfaceContactPoint = GTPlayer.MovingSurfaceContactPoint.LEFT;
						this.lastMovingSurfaceHit = this.leftHand.lastHitInfo;
					}
				}
			}
			this.StoreVelocities();
			if (this.InWater)
			{
				PlayerGameEvents.PlayerSwam((this.lastPosition - vector9).magnitude, this.currentVelocity.magnitude);
			}
			else
			{
				PlayerGameEvents.PlayerMoved((this.lastPosition - vector9).magnitude, this.currentVelocity.magnitude);
			}
			this.didAJump = false;
			bool flag6 = this.exitMovingSurface;
			this.exitMovingSurface = false;
			if (this.leftHand.IsSlipOverriddenToMax() && this.rightHand.IsSlipOverriddenToMax())
			{
				this.didAJump = true;
				this.exitMovingSurface = true;
			}
			else if (this.anyHandIsSliding)
			{
				this.slideAverageNormal = Vector3.zero;
				int num7 = 0;
				this.averageSlipPercentage = 0f;
				bool flag7 = false;
				if (this.leftHand.isSliding)
				{
					this.slideAverageNormal += this.leftHand.slideNormal.normalized;
					this.averageSlipPercentage += this.leftHand.slipPercentage;
					num7++;
				}
				if (this.rightHand.isSliding)
				{
					flag7 = true;
					this.slideAverageNormal += this.rightHand.slideNormal.normalized;
					this.averageSlipPercentage += this.rightHand.slipPercentage;
					num7++;
				}
				for (int k = 0; k < this.stiltStates.Length; k++)
				{
					if (this.stiltStates[k].isActive && this.stiltStates[k].isSliding)
					{
						if (!this.stiltStates[k].isLeftHand)
						{
							flag7 = true;
						}
						this.slideAverageNormal += this.stiltStates[k].slideNormal.normalized;
						this.averageSlipPercentage += this.stiltStates[k].slipPercentage;
						num7++;
					}
				}
				this.slideAverageNormal = this.slideAverageNormal.normalized;
				this.averageSlipPercentage /= (float)num7;
				if (num7 == 1)
				{
					this.surfaceDirection = (flag7 ? Vector3.ProjectOnPlane(this.rightHand.handFollower.forward, this.rightHand.slideNormal) : Vector3.ProjectOnPlane(this.leftHand.handFollower.forward, this.leftHand.slideNormal));
					if (Vector3.Dot(this.slideVelocity, this.surfaceDirection) > 0f)
					{
						this.slideVelocity = Vector3.Project(this.slideVelocity, Vector3.Slerp(this.slideVelocity, this.surfaceDirection.normalized * this.slideVelocity.magnitude, this.slideControl));
					}
					else
					{
						this.slideVelocity = Vector3.Project(this.slideVelocity, Vector3.Slerp(this.slideVelocity, -this.surfaceDirection.normalized * this.slideVelocity.magnitude, this.slideControl));
					}
				}
				if (!this.anyHandWasSliding)
				{
					this.slideVelocity = ((Vector3.Dot(this.playerRigidBody.linearVelocity, this.slideAverageNormal) <= 0f) ? Vector3.ProjectOnPlane(this.playerRigidBody.linearVelocity, this.slideAverageNormal) : this.playerRigidBody.linearVelocity);
				}
				else
				{
					this.slideVelocity = ((Vector3.Dot(this.slideVelocity, this.slideAverageNormal) <= 0f) ? Vector3.ProjectOnPlane(this.slideVelocity, this.slideAverageNormal) : this.slideVelocity);
				}
				this.slideVelocity = this.slideVelocity.normalized * Mathf.Min(this.slideVelocity.magnitude, Mathf.Max(0.5f, this.averagedVelocity.magnitude * 2f));
				this.playerRigidBody.linearVelocity = Vector3.zero;
			}
			else if (this.anyHandIsColliding)
			{
				if (!this.turnedThisFrame)
				{
					this.playerRigidBody.linearVelocity = Vector3.zero;
				}
				else
				{
					this.playerRigidBody.linearVelocity = this.playerRigidBody.linearVelocity.normalized * Mathf.Min(2f, this.playerRigidBody.linearVelocity.magnitude);
				}
			}
			else if (this.anyHandWasSliding)
			{
				this.playerRigidBody.linearVelocity = ((Vector3.Dot(this.slideVelocity, this.slideAverageNormal) <= 0f) ? Vector3.ProjectOnPlane(this.slideVelocity, this.slideAverageNormal) : this.slideVelocity);
			}
			if (this.anyHandIsColliding && !this.disableMovement && !this.turnedThisFrame && !this.didAJump)
			{
				if (this.anyHandIsSliding)
				{
					if (Vector3.Project(this.averagedVelocity, this.slideAverageNormal).magnitude > this.slideVelocityLimit * this.scale && Vector3.Dot(this.averagedVelocity, this.slideAverageNormal) > 0f && Vector3.Project(this.averagedVelocity, this.slideAverageNormal).magnitude > Vector3.Project(this.slideVelocity, this.slideAverageNormal).magnitude)
					{
						this.leftHand.isSliding = false;
						this.rightHand.isSliding = false;
						for (int l = 0; l < this.stiltStates.Length; l++)
						{
							this.stiltStates[l].isSliding = false;
						}
						this.anyHandIsSliding = false;
						this.didAJump = true;
						float num8 = this.ApplyNativeScaleAdjustment(Mathf.Min(this.maxJumpSpeed * this.ExtraVelMaxMultiplier(), this.jumpMultiplier * this.ExtraVelMultiplier() * Vector3.Project(this.averagedVelocity, this.slideAverageNormal).magnitude));
						this.playerRigidBody.linearVelocity = num8 * this.slideAverageNormal.normalized + Vector3.ProjectOnPlane(this.slideVelocity, this.slideAverageNormal);
						if (num8 > this.slideVelocityLimit * this.scale * this.exitMovingSurfaceThreshold)
						{
							this.exitMovingSurface = true;
						}
					}
				}
				else if (this.averagedVelocity.magnitude > this.velocityLimit * this.scale)
				{
					float num9 = ((this.InWater && this.CurrentWaterVolume != null) ? this.liquidPropertiesList[(int)this.CurrentWaterVolume.LiquidType].surfaceJumpFactor : 1f);
					float num10 = this.ApplyNativeScaleAdjustment(this.enableHoverMode ? Mathf.Min(this.hoverMaxPaddleSpeed, this.averagedVelocity.magnitude) : Mathf.Min(this.maxJumpSpeed * this.ExtraVelMaxMultiplier(), this.jumpMultiplier * this.ExtraVelMultiplier() * num9 * this.averagedVelocity.magnitude));
					Vector3 vector10 = num10 * this.averagedVelocity.normalized;
					this.didAJump = true;
					this.playerRigidBody.linearVelocity = vector10;
					if (this.InWater)
					{
						this.swimmingVelocity += vector10 * this.swimmingParams.underwaterJumpsAsSwimVelocityFactor;
					}
					if (num10 > this.velocityLimit * this.scale * this.exitMovingSurfaceThreshold)
					{
						this.exitMovingSurface = true;
					}
				}
			}
			this.stuckHandsCheckLateUpdate(ref this.leftHand.finalPositionThisFrame, ref this.rightHand.finalPositionThisFrame);
			if (this.lastPlatformTouched != null && this.currentPlatform == null)
			{
				if (!this.playerRigidBody.isKinematic)
				{
					this.playerRigidBody.linearVelocity += this.refMovement / this.calcDeltaTime;
				}
				this.refMovement = Vector3.zero;
			}
			if (this.lastMovingSurfaceContact == GTPlayer.MovingSurfaceContactPoint.NONE)
			{
				if (!this.playerRigidBody.isKinematic)
				{
					this.playerRigidBody.linearVelocity += this.lastMovingSurfaceVelocity;
				}
				this.lastMovingSurfaceVelocity = Vector3.zero;
			}
			if (this.enableHoverMode)
			{
				this.HoverboardLateUpdate();
			}
			else
			{
				this.hasHoverPoint = false;
			}
			Vector3 vector11 = Vector3.zero;
			float num11 = 0f;
			float num12 = 0f;
			if (this.bodyInWater)
			{
				Vector3 vector12;
				if (this.GetSwimmingVelocityForHand(this.leftHand.lastPosition, this.leftHand.finalPositionThisFrame, this.leftHand.controllerTransform.right, this.calcDeltaTime, ref this.leftHandWaterVolume, ref this.leftHandWaterSurface, out vector12) && !this.turnedThisFrame)
				{
					num11 = Mathf.InverseLerp(0f, 0.2f, vector12.magnitude) * this.swimmingParams.swimmingHapticsStrength;
					vector11 += vector12;
				}
				Vector3 vector13;
				if (this.GetSwimmingVelocityForHand(this.rightHand.lastPosition, this.rightHand.finalPositionThisFrame, -this.rightHand.controllerTransform.right, this.calcDeltaTime, ref this.rightHandWaterVolume, ref this.rightHandWaterSurface, out vector13) && !this.turnedThisFrame)
				{
					num12 = Mathf.InverseLerp(0f, 0.15f, vector13.magnitude) * this.swimmingParams.swimmingHapticsStrength;
					vector11 += vector13;
				}
			}
			Vector3 vector14 = Vector3.zero;
			Vector3 vector15;
			if (this.swimmingParams.allowWaterSurfaceJumps && time - this.lastWaterSurfaceJumpTimeLeft > this.waterSurfaceJumpCooldown && this.CheckWaterSurfaceJump(this.leftHand.lastPosition, this.leftHand.finalPositionThisFrame, this.leftHand.controllerTransform.right, this.leftHand.velocityTracker.GetAverageVelocity(false, 0.1f, false) * this.scale, this.swimmingParams, this.leftHandWaterVolume, this.leftHandWaterSurface, out vector15))
			{
				if (time - this.lastWaterSurfaceJumpTimeRight > this.waterSurfaceJumpCooldown)
				{
					vector14 += vector15;
				}
				this.lastWaterSurfaceJumpTimeLeft = Time.time;
				GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
			}
			Vector3 vector16;
			if (this.swimmingParams.allowWaterSurfaceJumps && time - this.lastWaterSurfaceJumpTimeRight > this.waterSurfaceJumpCooldown && this.CheckWaterSurfaceJump(this.rightHand.lastPosition, this.rightHand.finalPositionThisFrame, -this.rightHand.controllerTransform.right, this.rightHand.velocityTracker.GetAverageVelocity(false, 0.1f, false) * this.scale, this.swimmingParams, this.rightHandWaterVolume, this.rightHandWaterSurface, out vector16))
			{
				if (time - this.lastWaterSurfaceJumpTimeLeft > this.waterSurfaceJumpCooldown)
				{
					vector14 += vector16;
				}
				this.lastWaterSurfaceJumpTimeRight = Time.time;
				GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
			}
			vector14 = Vector3.ClampMagnitude(vector14, this.swimmingParams.waterSurfaceJumpMaxSpeed * this.scale);
			float num13 = Mathf.Max(num11, this.leftHandNonDiveHapticsAmount);
			if (num13 > 0.001f && time - this.lastWaterSurfaceJumpTimeLeft > GorillaTagger.Instance.tapHapticDuration)
			{
				GorillaTagger.Instance.DoVibration(XRNode.LeftHand, num13, this.calcDeltaTime);
			}
			float num14 = Mathf.Max(num12, this.rightHandNonDiveHapticsAmount);
			if (num14 > 0.001f && time - this.lastWaterSurfaceJumpTimeRight > GorillaTagger.Instance.tapHapticDuration)
			{
				GorillaTagger.Instance.DoVibration(XRNode.RightHand, num14, this.calcDeltaTime);
			}
			if (!this.disableMovement)
			{
				this.swimmingVelocity += vector11;
				if (!this.playerRigidBody.isKinematic)
				{
					this.playerRigidBody.linearVelocity += vector11 + vector14;
				}
			}
			else
			{
				this.swimmingVelocity = Vector3.zero;
			}
			if (GorillaGameManager.instance is GorillaFreezeTagManager)
			{
				if (!this.IsFrozen || !this.primaryButtonPressed)
				{
					this.IsBodySliding = false;
					this.lastSlopeDirection = Vector3.zero;
					if (this.bodyTouchedSurfaces.Count > 0)
					{
						foreach (KeyValuePair<GameObject, PhysicsMaterial> keyValuePair in this.bodyTouchedSurfaces)
						{
							MeshCollider meshCollider;
							if (keyValuePair.Key.TryGetComponent<MeshCollider>(out meshCollider))
							{
								meshCollider.material = keyValuePair.Value;
							}
						}
						this.bodyTouchedSurfaces.Clear();
					}
				}
				else if (this.BodyOnGround && this.primaryButtonPressed)
				{
					float num15 = this.bodyInitialHeight / 2f - this.bodyInitialRadius;
					RaycastHit raycastHit;
					if (Physics.SphereCast(this.bodyCollider.transform.position - new Vector3(0f, num15, 0f), this.bodyInitialRadius - 0.01f, Vector3.down, out raycastHit, 1f, ~LayerMask.GetMask(new string[] { "Gorilla Body Collider", "GorillaInteractable" }), QueryTriggerInteraction.Ignore))
					{
						this.IsBodySliding = true;
						MeshCollider meshCollider2;
						if (!this.bodyTouchedSurfaces.ContainsKey(raycastHit.transform.gameObject) && raycastHit.transform.gameObject.TryGetComponent<MeshCollider>(out meshCollider2))
						{
							this.bodyTouchedSurfaces.Add(raycastHit.transform.gameObject, meshCollider2.material);
							raycastHit.transform.gameObject.GetComponent<MeshCollider>().material = this.slipperyMaterial;
						}
					}
				}
				else
				{
					this.IsBodySliding = false;
					this.lastSlopeDirection = Vector3.zero;
				}
			}
			else
			{
				this.IsBodySliding = false;
				if (this.bodyTouchedSurfaces.Count > 0)
				{
					foreach (KeyValuePair<GameObject, PhysicsMaterial> keyValuePair2 in this.bodyTouchedSurfaces)
					{
						MeshCollider meshCollider3;
						if (keyValuePair2.Key.TryGetComponent<MeshCollider>(out meshCollider3))
						{
							meshCollider3.material = keyValuePair2.Value;
						}
					}
					this.bodyTouchedSurfaces.Clear();
				}
			}
			this.leftHand.OnEndOfFrame();
			this.rightHand.OnEndOfFrame();
			for (int m = 0; m < 12; m++)
			{
				if (this.stiltStates[m].isActive)
				{
					this.stiltStates[m].OnEndOfFrame();
				}
			}
			this.leftHand.PositionHandFollower();
			this.rightHand.PositionHandFollower();
			this.anyHandWasSliding = this.anyHandIsSliding;
			this.anyHandWasColliding = this.anyHandIsColliding;
			this.anyHandWasSticking = this.anyHandIsSticking;
			if (this.anyHandIsSticking)
			{
				this.lastTouchedGroundTimestamp = Time.time;
			}
			if (PhotonNetwork.InRoom)
			{
				if (this.IsGroundedHand || this.IsThrusterActive)
				{
					this.LastHandTouchedGroundAtNetworkTime = (float)PhotonNetwork.Time;
					this.LastTouchedGroundAtNetworkTime = (float)PhotonNetwork.Time;
				}
				else if (this.IsGroundedButt)
				{
					this.LastTouchedGroundAtNetworkTime = (float)PhotonNetwork.Time;
				}
			}
			else
			{
				this.LastHandTouchedGroundAtNetworkTime = 0f;
				this.LastTouchedGroundAtNetworkTime = 0f;
			}
			this.degreesTurnedThisFrame = 0f;
			this.lastPlatformTouched = this.currentPlatform;
			this.currentPlatform = null;
			this.lastMovingSurfaceVelocity = vector3;
			Vector3 vector17;
			if (GTPlayer.ComputeLocalHitPoint(this.lastHitInfoHand, out vector17))
			{
				this.lastFrameHasValidTouchPos = true;
				this.lastFrameTouchPosLocal = vector17;
				this.lastFrameTouchPosWorld = this.lastHitInfoHand.point;
			}
			else
			{
				this.lastFrameHasValidTouchPos = false;
				this.lastFrameTouchPosLocal = Vector3.zero;
				this.lastFrameTouchPosWorld = Vector3.zero;
			}
			this.lastRigidbodyPosition = this.playerRigidBody.transform.position;
			RaycastHit raycastHit2 = this.emptyHit;
			this.BodyCollider();
			if (this.bodyHitInfo.collider != null)
			{
				this.wasBodyOnGround = true;
				raycastHit2 = this.bodyHitInfo;
			}
			else if (movingSurfaceContactPoint == GTPlayer.MovingSurfaceContactPoint.NONE && this.bodyCollider.gameObject.activeSelf)
			{
				bool flag8 = false;
				this.ClearRaycasthitBuffer(ref this.rayCastNonAllocColliders);
				Vector3 vector18 = this.PositionWithOffset(this.headCollider.transform, this.bodyOffset) + (this.bodyInitialHeight * this.scale - this.bodyMaxRadius) * Vector3.down;
				this.bufferCount = Physics.SphereCastNonAlloc(vector18, this.bodyMaxRadius, Vector3.down, this.rayCastNonAllocColliders, this.minimumRaycastDistance * this.scale, this.locomotionEnabledLayers.value);
				if (this.bufferCount > 0)
				{
					this.tempHitInfo = this.rayCastNonAllocColliders[0];
					for (int n = 0; n < this.bufferCount; n++)
					{
						if (this.tempHitInfo.distance > 0f && (!flag8 || this.rayCastNonAllocColliders[n].distance < this.tempHitInfo.distance))
						{
							flag8 = true;
							raycastHit2 = this.rayCastNonAllocColliders[n];
						}
					}
				}
				this.wasBodyOnGround = flag8;
			}
			int num16 = -1;
			bool flag9 = false;
			bool flag10;
			if (this.wasBodyOnGround && movingSurfaceContactPoint == GTPlayer.MovingSurfaceContactPoint.NONE && this.IsTouchingMovingSurface(this.PositionWithOffset(this.headCollider.transform, this.bodyOffset), raycastHit2, out num16, out flag10, out flag9) && !flag10)
			{
				movingSurfaceContactPoint = GTPlayer.MovingSurfaceContactPoint.BODY;
				this.lastMovingSurfaceHit = raycastHit2;
			}
			Vector3 vector19;
			if (movingSurfaceContactPoint != GTPlayer.MovingSurfaceContactPoint.NONE && GTPlayer.ComputeLocalHitPoint(this.lastMovingSurfaceHit, out vector19))
			{
				this.lastMovingSurfaceTouchLocal = vector19;
				this.lastMovingSurfaceTouchWorld = this.lastMovingSurfaceHit.point;
				this.lastMovingSurfaceRot = this.lastMovingSurfaceHit.collider.transform.rotation;
				this.lastAttachedToMovingSurfaceFrame = Time.frameCount;
			}
			else
			{
				movingSurfaceContactPoint = GTPlayer.MovingSurfaceContactPoint.NONE;
				this.lastMovingSurfaceTouchLocal = Vector3.zero;
				this.lastMovingSurfaceTouchWorld = Vector3.zero;
				this.lastMovingSurfaceRot = Quaternion.identity;
			}
			Vector3 vector20 = this.lastMovingSurfaceTouchWorld;
			int num17 = -1;
			bool flag11 = false;
			switch (movingSurfaceContactPoint)
			{
			case GTPlayer.MovingSurfaceContactPoint.NONE:
				if (flag6)
				{
					this.exitMovingSurface = true;
				}
				num17 = -1;
				break;
			case GTPlayer.MovingSurfaceContactPoint.RIGHT:
				num17 = num5;
				flag11 = flag2;
				vector20 = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
				break;
			case GTPlayer.MovingSurfaceContactPoint.LEFT:
				num17 = num6;
				flag11 = flag3;
				vector20 = GorillaTagger.Instance.offlineVRRig.leftHandTransform.position;
				break;
			case GTPlayer.MovingSurfaceContactPoint.BODY:
				num17 = num16;
				flag11 = flag9;
				vector20 = GorillaTagger.Instance.offlineVRRig.bodyTransform.position;
				break;
			}
			if (!flag11)
			{
				this.lastMonkeBlock = null;
			}
			if (num17 != this.lastMovingSurfaceID || this.lastMovingSurfaceContact != movingSurfaceContactPoint || flag11 != this.wasMovingSurfaceMonkeBlock)
			{
				if (num17 == -1)
				{
					if (Time.frameCount - this.lastAttachedToMovingSurfaceFrame > 3)
					{
						VRRig.DetachLocalPlayerFromMovingSurface();
						this.lastMovingSurfaceID = -1;
					}
				}
				else if (flag11)
				{
					if (this.lastMonkeBlock != null)
					{
						VRRig.AttachLocalPlayerToMovingSurface(num17, movingSurfaceContactPoint == GTPlayer.MovingSurfaceContactPoint.LEFT, movingSurfaceContactPoint == GTPlayer.MovingSurfaceContactPoint.BODY, this.lastMonkeBlock.transform.InverseTransformPoint(vector20), flag11);
						this.lastMovingSurfaceID = num17;
					}
					else
					{
						VRRig.DetachLocalPlayerFromMovingSurface();
						this.lastMovingSurfaceID = -1;
					}
				}
				else if (MovingSurfaceManager.instance != null)
				{
					MovingSurface movingSurface;
					if (MovingSurfaceManager.instance.TryGetMovingSurface(num17, out movingSurface))
					{
						VRRig.AttachLocalPlayerToMovingSurface(num17, movingSurfaceContactPoint == GTPlayer.MovingSurfaceContactPoint.LEFT, movingSurfaceContactPoint == GTPlayer.MovingSurfaceContactPoint.BODY, movingSurface.transform.InverseTransformPoint(vector20), flag11);
						this.lastMovingSurfaceID = num17;
					}
					else
					{
						VRRig.DetachLocalPlayerFromMovingSurface();
						this.lastMovingSurfaceID = -1;
					}
				}
				else
				{
					VRRig.DetachLocalPlayerFromMovingSurface();
					this.lastMovingSurfaceID = -1;
				}
			}
			if (this.lastMovingSurfaceContact == GTPlayer.MovingSurfaceContactPoint.NONE && movingSurfaceContactPoint != GTPlayer.MovingSurfaceContactPoint.NONE)
			{
				this.SetPlayerVelocity(Vector3.zero);
			}
			this.lastMovingSurfaceContact = movingSurfaceContactPoint;
			this.wasMovingSurfaceMonkeBlock = flag11;
			if (this.activeSizeChangerSettings != null)
			{
				if (this.activeSizeChangerSettings.ExpireOnDistance > 0f && Vector3.Distance(base.transform.position, this.activeSizeChangerSettings.WorldPosition) > this.activeSizeChangerSettings.ExpireOnDistance)
				{
					this.SetNativeScale(null);
				}
				if (this.activeSizeChangerSettings.ExpireAfterSeconds > 0f && Time.time - this.activeSizeChangerSettings.ActivationTime > this.activeSizeChangerSettings.ExpireAfterSeconds)
				{
					this.SetNativeScale(null);
				}
			}
			HandLink grabbedLink = VRRig.LocalRig.leftHandLink.grabbedLink;
			if (grabbedLink != null)
			{
				double time2 = PhotonNetwork.Time;
				float lastHandTouchedGroundAtNetworkTime = this.LastHandTouchedGroundAtNetworkTime;
				double time3 = PhotonNetwork.Time;
				float lastHandTouchedGroundAtNetworkTime2 = grabbedLink.myRig.LastHandTouchedGroundAtNetworkTime;
			}
			if (this.didAJump || this.anyHandIsColliding || this.anyHandIsSliding || this.anyHandIsSticking || this.IsGroundedHand || this.forceRBSync)
			{
				this.forceRBSync = false;
			}
		}

		private float ApplyNativeScaleAdjustment(float adjustedMagnitude)
		{
			if (this.nativeScale > 0f && this.nativeScale != 1f)
			{
				return adjustedMagnitude *= this.nativeScaleMagnitudeAdjustmentFactor.Evaluate(this.nativeScale);
			}
			return adjustedMagnitude;
		}

		private float RotateWithSurface(Quaternion rotationDelta, Vector3 pivot)
		{
			Quaternion quaternion;
			Quaternion quaternion2;
			QuaternionUtil.DecomposeSwingTwist(rotationDelta, Vector3.up, out quaternion, out quaternion2);
			float num = quaternion2.eulerAngles.y;
			if (num > 270f)
			{
				num -= 360f;
			}
			else if (num > 90f)
			{
				num -= 180f;
			}
			if (Mathf.Abs(num) < 90f * this.calcDeltaTime)
			{
				this.turnParent.transform.RotateAround(pivot, base.transform.up, num);
				return num;
			}
			return 0f;
		}

		private void stuckHandsCheckFixedUpdate()
		{
			Vector3 currentHandPosition = this.leftHand.GetCurrentHandPosition();
			this.stuckLeft = !this.controllerState.LeftValid || (this.leftHand.isColliding && (currentHandPosition - this.leftHand.GetLastPosition()).magnitude > this.unStickDistance * this.scale && !Physics.Raycast(this.headCollider.transform.position, (currentHandPosition - this.headCollider.transform.position).normalized, (currentHandPosition - this.headCollider.transform.position).magnitude, this.locomotionEnabledLayers.value));
			Vector3 currentHandPosition2 = this.rightHand.GetCurrentHandPosition();
			this.stuckRight = !this.controllerState.RightValid || (this.rightHand.isColliding && (currentHandPosition2 - this.rightHand.GetLastPosition()).magnitude > this.unStickDistance * this.scale && !Physics.Raycast(this.headCollider.transform.position, (currentHandPosition2 - this.headCollider.transform.position).normalized, (currentHandPosition2 - this.headCollider.transform.position).magnitude, this.locomotionEnabledLayers.value));
		}

		private void stuckHandsCheckLateUpdate(ref Vector3 finalLeftHandPosition, ref Vector3 finalRightHandPosition)
		{
			if (this.stuckLeft)
			{
				finalLeftHandPosition = this.leftHand.GetCurrentHandPosition();
				this.stuckLeft = (this.leftHand.isColliding = false);
			}
			if (this.stuckRight)
			{
				finalRightHandPosition = this.rightHand.GetCurrentHandPosition();
				this.stuckRight = (this.rightHand.isColliding = false);
			}
		}

		private void handleClimbing(float deltaTime)
		{
			if (this.isClimbing && (this.inOverlay || this.climbHelper == null || this.currentClimbable == null || !this.currentClimbable.isActiveAndEnabled))
			{
				this.EndClimbing(this.currentClimber, false, false);
			}
			Vector3 vector = Vector3.zero;
			if (this.isClimbing && (this.currentClimber.transform.position - this.climbHelper.position).magnitude > 1f)
			{
				this.EndClimbing(this.currentClimber, false, false);
			}
			if (this.isClimbing)
			{
				this.playerRigidBody.linearVelocity = Vector3.zero;
				this.climbHelper.localPosition = Vector3.MoveTowards(this.climbHelper.localPosition, this.climbHelperTargetPos, deltaTime * 12f);
				vector = this.currentClimber.transform.position - this.climbHelper.position;
				vector = ((vector.sqrMagnitude > this.maxArmLength * this.maxArmLength) ? (vector.normalized * this.maxArmLength) : vector);
				if (this.isClimbableMoving)
				{
					Quaternion quaternion = this.currentClimbable.transform.rotation * Quaternion.Inverse(this.lastClimbableRotation);
					this.RotateWithSurface(quaternion, this.currentClimber.handRoot.position);
					this.lastClimbableRotation = this.currentClimbable.transform.rotation;
				}
				this.playerRigidBody.MovePosition(this.playerRigidBody.position - vector);
				if (this.currentSwing)
				{
					this.currentSwing.lastGrabTime = Time.time;
				}
			}
		}

		public void RequestTentacleMove(bool isLeftHand, Vector3 move)
		{
			if (isLeftHand)
			{
				this.hasLeftHandTentacleMove = true;
				this.leftHandTentacleMove = move;
				return;
			}
			this.hasRightHandTentacleMove = true;
			this.rightHandTentacleMove = move;
		}

		public void HandleTentacleMovement()
		{
			Vector3 vector;
			if (this.hasLeftHandTentacleMove)
			{
				if (this.hasRightHandTentacleMove)
				{
					vector = (this.leftHandTentacleMove + this.rightHandTentacleMove) * 0.5f;
					this.hasRightHandTentacleMove = (this.hasLeftHandTentacleMove = false);
				}
				else
				{
					vector = this.leftHandTentacleMove;
					this.hasLeftHandTentacleMove = false;
				}
			}
			else
			{
				if (!this.hasRightHandTentacleMove)
				{
					return;
				}
				vector = this.rightHandTentacleMove;
				this.hasRightHandTentacleMove = false;
			}
			this.playerRigidBody.transform.position += vector;
			this.playerRigidBody.linearVelocity = Vector3.zero;
		}

		public HandLinkAuthorityStatus GetSelfHandLinkAuthority()
		{
			int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
			if (this.IsGroundedHand)
			{
				return new HandLinkAuthorityStatus(HandLinkAuthorityType.HandGrounded);
			}
			if ((double)(this.LastHandTouchedGroundAtNetworkTime + 1f) > PhotonNetwork.Time)
			{
				return new HandLinkAuthorityStatus(HandLinkAuthorityType.ResidualHandGrounded, this.LastHandTouchedGroundAtNetworkTime, actorNumber);
			}
			if (this.IsGroundedButt)
			{
				return new HandLinkAuthorityStatus(HandLinkAuthorityType.ButtGrounded);
			}
			return new HandLinkAuthorityStatus(HandLinkAuthorityType.None, this.LastTouchedGroundAtNetworkTime, actorNumber);
		}

		private void HandleHandLink()
		{
			HandLink leftHandLink = VRRig.LocalRig.leftHandLink;
			HandLink rightHandLink = VRRig.LocalRig.rightHandLink;
			bool flag = leftHandLink.grabbedLink != null;
			bool flag2 = rightHandLink.grabbedLink != null;
			if (!flag && !flag2)
			{
				return;
			}
			HandLinkAuthorityStatus selfHandLinkAuthority = this.GetSelfHandLinkAuthority();
			int num = -1;
			HandLinkAuthorityStatus chainAuthority = new HandLinkAuthorityStatus(HandLinkAuthorityType.None);
			if (flag)
			{
				chainAuthority = leftHandLink.GetChainAuthority(out num);
			}
			int num2 = -1;
			HandLinkAuthorityStatus chainAuthority2 = new HandLinkAuthorityStatus(HandLinkAuthorityType.None);
			if (flag2)
			{
				chainAuthority2 = rightHandLink.GetChainAuthority(out num2);
			}
			if (flag && flag2)
			{
				if (leftHandLink.grabbedPlayer == rightHandLink.grabbedPlayer)
				{
					switch (selfHandLinkAuthority.CompareTo(chainAuthority))
					{
					case -1:
						this.HandLink_PositionChild_LocalPlayer(leftHandLink, rightHandLink);
						return;
					case 0:
						this.HandLink_PositionBoth_BothHands(leftHandLink, rightHandLink);
						return;
					case 1:
						this.HandLink_PositionChild_RemotePlayer_BothHands(leftHandLink, rightHandLink);
						return;
					default:
						return;
					}
				}
				else
				{
					int num3 = selfHandLinkAuthority.CompareTo(chainAuthority);
					int num4 = selfHandLinkAuthority.CompareTo(chainAuthority2);
					switch (num3 * 3 + num4)
					{
					case -3:
					case -2:
						this.HandLink_PositionChild_LocalPlayer(leftHandLink);
						this.HandLink_PositionChild_RemotePlayer(rightHandLink);
						return;
					case -1:
					case 2:
						this.HandLink_PositionChild_LocalPlayer(rightHandLink);
						this.HandLink_PositionChild_RemotePlayer(leftHandLink);
						return;
					case 0:
						this.HandLink_PositionTriple(leftHandLink, rightHandLink);
						return;
					case 1:
						this.HandLink_PositionBoth(leftHandLink);
						this.HandLink_PositionChild_RemotePlayer(rightHandLink);
						return;
					case 3:
						this.HandLink_PositionBoth(rightHandLink);
						this.HandLink_PositionChild_RemotePlayer(leftHandLink);
						return;
					case 4:
						this.HandLink_PositionChild_RemotePlayer(leftHandLink);
						this.HandLink_PositionChild_RemotePlayer(rightHandLink);
						return;
					}
					switch (chainAuthority.CompareTo(chainAuthority2))
					{
					case -1:
						this.HandLink_PositionChild_LocalPlayer(rightHandLink);
						this.HandLink_PositionChild_RemotePlayer(leftHandLink);
						return;
					case 0:
						if (num > num2)
						{
							this.HandLink_PositionChild_LocalPlayer(rightHandLink);
							this.HandLink_PositionChild_RemotePlayer(leftHandLink);
							return;
						}
						if (num < num2)
						{
							this.HandLink_PositionChild_LocalPlayer(leftHandLink);
							this.HandLink_PositionChild_RemotePlayer(rightHandLink);
							return;
						}
						this.HandLink_PositionChild_LocalPlayer(leftHandLink, rightHandLink);
						return;
					case 1:
						this.HandLink_PositionChild_LocalPlayer(leftHandLink);
						this.HandLink_PositionChild_RemotePlayer(rightHandLink);
						return;
					default:
						return;
					}
				}
			}
			else if (flag)
			{
				switch (selfHandLinkAuthority.CompareTo(chainAuthority))
				{
				case -1:
					this.HandLink_PositionChild_LocalPlayer(leftHandLink);
					return;
				case 0:
					this.HandLink_PositionBoth(leftHandLink);
					return;
				case 1:
					this.HandLink_PositionChild_RemotePlayer(leftHandLink);
					return;
				default:
					return;
				}
			}
			else
			{
				switch (selfHandLinkAuthority.CompareTo(chainAuthority2))
				{
				case -1:
					this.HandLink_PositionChild_LocalPlayer(rightHandLink);
					return;
				case 0:
					this.HandLink_PositionBoth(rightHandLink);
					return;
				case 1:
					this.HandLink_PositionChild_RemotePlayer(rightHandLink);
					return;
				default:
					return;
				}
			}
		}

		private void HandLink_PositionTriple(HandLink linkA, HandLink linkB)
		{
			Vector3 vector = linkA.transform.position - linkA.grabbedLink.transform.position;
			Vector3 vector2 = linkB.transform.position - linkB.grabbedLink.transform.position;
			Vector3 vector3 = (vector + vector2) * 0.33f;
			bool flag;
			bool flag2;
			linkA.grabbedLink.myRig.TrySweptOffsetMove(vector - vector3, out flag, out flag2);
			bool flag3;
			bool flag4;
			linkB.grabbedLink.myRig.TrySweptOffsetMove(vector2 - vector3, out flag3, out flag4);
			this.playerRigidBody.MovePosition(this.playerRigidBody.position - vector3);
			this.playerRigidBody.linearVelocity = Vector3.zero;
		}

		private void HandLink_PositionBoth(HandLink link)
		{
			Vector3 vector = (link.grabbedLink.transform.position - link.transform.position) * 0.5f;
			bool flag;
			bool flag2;
			link.grabbedLink.myRig.TrySweptOffsetMove(-vector, out flag, out flag2);
			if (flag || flag2)
			{
				this.HandLink_PositionChild_LocalPlayer(link);
			}
			else
			{
				this.playerRigidBody.transform.position += vector;
			}
			this.playerRigidBody.linearVelocity = Vector3.zero;
		}

		private void HandLink_PositionBoth_BothHands(HandLink link1, HandLink link2)
		{
			Vector3 vector = (link1.grabbedLink.transform.position - link1.transform.position) * 0.5f;
			Vector3 vector2 = (link2.grabbedLink.transform.position - link2.transform.position) * 0.5f;
			Vector3 vector3 = (vector + vector2) * 0.5f;
			bool flag;
			bool flag2;
			link1.grabbedLink.myRig.TrySweptOffsetMove(-vector3, out flag, out flag2);
			if (flag || flag2)
			{
				this.HandLink_PositionChild_LocalPlayer(link1, link2);
			}
			else
			{
				this.playerRigidBody.transform.position += vector3;
			}
			this.playerRigidBody.linearVelocity = Vector3.zero;
		}

		private void HandLink_PositionChild_LocalPlayer(HandLink parentLink)
		{
			Vector3 vector = parentLink.grabbedLink.transform.position - parentLink.transform.position;
			this.playerRigidBody.transform.position += vector;
			this.playerRigidBody.linearVelocity = Vector3.zero;
		}

		private void HandLink_PositionChild_LocalPlayer(HandLink linkA, HandLink linkB)
		{
			Vector3 vector = linkA.grabbedLink.transform.position - linkA.transform.position;
			Vector3 vector2 = linkB.grabbedLink.transform.position - linkB.transform.position;
			this.playerRigidBody.transform.position += (vector + vector2) * 0.5f;
			this.playerRigidBody.linearVelocity = Vector3.zero;
		}

		private void HandLink_PositionChild_RemotePlayer(HandLink childLink)
		{
			Vector3 vector = childLink.transform.position - childLink.grabbedLink.transform.position;
			bool flag;
			bool flag2;
			childLink.grabbedLink.myRig.TrySweptOffsetMove(vector, out flag, out flag2);
			if (flag || flag2)
			{
				this.HandLink_PositionChild_LocalPlayer(childLink);
			}
		}

		private void HandLink_PositionChild_RemotePlayer_BothHands(HandLink childLink1, HandLink childLink2)
		{
			Vector3 vector = childLink1.transform.position - childLink1.grabbedLink.transform.position;
			Vector3 vector2 = childLink2.transform.position - childLink2.grabbedLink.transform.position;
			Vector3 vector3 = (vector + vector2) * 0.5f;
			bool flag;
			bool flag2;
			childLink1.grabbedLink.myRig.TrySweptOffsetMove(vector3, out flag, out flag2);
			if (flag || flag2)
			{
				this.HandLink_PositionChild_LocalPlayer(childLink1, childLink2);
			}
		}

		private bool IterativeCollisionSphereCast(Vector3 startPosition, float sphereRadius, Vector3 movementVector, Vector3 boostVector, out Vector3 endPosition, bool singleHand, out float slipPercentage, out RaycastHit iterativeHitInfo, bool fullSlide)
		{
			slipPercentage = this.defaultSlideFactor;
			if (!this.CollisionsSphereCast(startPosition, sphereRadius, movementVector, out endPosition, out this.tempIterativeHit))
			{
				iterativeHitInfo = this.tempIterativeHit;
				endPosition = Vector3.zero;
				return false;
			}
			this.firstPosition = endPosition;
			iterativeHitInfo = this.tempIterativeHit;
			this.slideFactor = this.GetSlidePercentage(iterativeHitInfo);
			slipPercentage = ((this.slideFactor != this.defaultSlideFactor) ? this.slideFactor : ((!singleHand) ? this.defaultSlideFactor : 0.001f));
			if (fullSlide)
			{
				slipPercentage = 1f;
			}
			this.movementToProjectedAboveCollisionPlane = Vector3.ProjectOnPlane(startPosition + movementVector - this.firstPosition, iterativeHitInfo.normal) * slipPercentage;
			Vector3 vector = Vector3.zero;
			if (boostVector.IsLongerThan(0f))
			{
				vector = Vector3.ProjectOnPlane(boostVector, iterativeHitInfo.normal);
				this.movementToProjectedAboveCollisionPlane += vector;
				this.CollisionsSphereCast(this.firstPosition, sphereRadius, vector, out endPosition, out this.tempIterativeHit);
				this.firstPosition = endPosition;
			}
			if (this.CollisionsSphereCast(this.firstPosition, sphereRadius, this.movementToProjectedAboveCollisionPlane, out endPosition, out this.tempIterativeHit))
			{
				iterativeHitInfo = this.tempIterativeHit;
				return true;
			}
			if (this.CollisionsSphereCast(this.movementToProjectedAboveCollisionPlane + this.firstPosition, sphereRadius, startPosition + movementVector + vector - (this.movementToProjectedAboveCollisionPlane + this.firstPosition), out endPosition, out this.tempIterativeHit))
			{
				iterativeHitInfo = this.tempIterativeHit;
				return true;
			}
			endPosition = Vector3.zero;
			return false;
		}

		private bool CollisionsSphereCast(Vector3 startPosition, float sphereRadius, Vector3 movementVector, out Vector3 finalPosition, out RaycastHit collisionsHitInfo)
		{
			this.MaxSphereSizeForNoOverlap(sphereRadius, startPosition, false, out this.maxSphereSize1);
			bool flag = false;
			this.ClearRaycasthitBuffer(ref this.rayCastNonAllocColliders);
			this.bufferCount = Physics.SphereCastNonAlloc(startPosition, this.maxSphereSize1, movementVector.normalized, this.rayCastNonAllocColliders, movementVector.magnitude, this.locomotionEnabledLayers.value);
			if (this.bufferCount > 0)
			{
				this.tempHitInfo = this.rayCastNonAllocColliders[0];
				for (int i = 0; i < this.bufferCount; i++)
				{
					if (this.tempHitInfo.distance > 0f && (!flag || this.rayCastNonAllocColliders[i].distance < this.tempHitInfo.distance))
					{
						flag = true;
						this.tempHitInfo = this.rayCastNonAllocColliders[i];
					}
				}
			}
			if (flag)
			{
				collisionsHitInfo = this.tempHitInfo;
				finalPosition = collisionsHitInfo.point + collisionsHitInfo.normal * sphereRadius;
				this.ClearRaycasthitBuffer(ref this.rayCastNonAllocColliders);
				this.bufferCount = Physics.RaycastNonAlloc(startPosition, (finalPosition - startPosition).normalized, this.rayCastNonAllocColliders, (finalPosition - startPosition).magnitude, this.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore);
				if (this.bufferCount > 0)
				{
					this.tempHitInfo = this.rayCastNonAllocColliders[0];
					for (int j = 0; j < this.bufferCount; j++)
					{
						if (this.rayCastNonAllocColliders[j].collider && this.rayCastNonAllocColliders[j].distance < this.tempHitInfo.distance)
						{
							this.tempHitInfo = this.rayCastNonAllocColliders[j];
						}
					}
					finalPosition = startPosition + movementVector.normalized * this.tempHitInfo.distance;
				}
				this.MaxSphereSizeForNoOverlap(sphereRadius, finalPosition, false, out this.maxSphereSize2);
				this.ClearRaycasthitBuffer(ref this.rayCastNonAllocColliders);
				this.bufferCount = Physics.SphereCastNonAlloc(startPosition, Mathf.Min(this.maxSphereSize1, this.maxSphereSize2), (finalPosition - startPosition).normalized, this.rayCastNonAllocColliders, (finalPosition - startPosition).magnitude, this.locomotionEnabledLayers.value);
				if (this.bufferCount > 0)
				{
					this.tempHitInfo = this.rayCastNonAllocColliders[0];
					for (int k = 0; k < this.bufferCount; k++)
					{
						if (this.rayCastNonAllocColliders[k].collider != null && this.rayCastNonAllocColliders[k].distance < this.tempHitInfo.distance)
						{
							this.tempHitInfo = this.rayCastNonAllocColliders[k];
						}
					}
					finalPosition = startPosition + this.tempHitInfo.distance * (finalPosition - startPosition).normalized;
					collisionsHitInfo = this.tempHitInfo;
				}
				return true;
			}
			this.ClearRaycasthitBuffer(ref this.rayCastNonAllocColliders);
			this.bufferCount = Physics.RaycastNonAlloc(startPosition, movementVector.normalized, this.rayCastNonAllocColliders, movementVector.magnitude, this.locomotionEnabledLayers.value);
			if (this.bufferCount > 0)
			{
				this.tempHitInfo = this.rayCastNonAllocColliders[0];
				for (int l = 0; l < this.bufferCount; l++)
				{
					if (this.rayCastNonAllocColliders[l].collider != null && this.rayCastNonAllocColliders[l].distance < this.tempHitInfo.distance)
					{
						this.tempHitInfo = this.rayCastNonAllocColliders[l];
					}
				}
				collisionsHitInfo = this.tempHitInfo;
				finalPosition = startPosition;
				return true;
			}
			finalPosition = startPosition + movementVector;
			collisionsHitInfo = default(RaycastHit);
			return false;
		}

		public float GetSlidePercentage(RaycastHit raycastHit)
		{
			if (this.IsFrozen && GorillaGameManager.instance is GorillaFreezeTagManager)
			{
				return this.FreezeTagSlidePercentage();
			}
			this.currentOverride = raycastHit.collider.gameObject.GetComponent<GorillaSurfaceOverride>();
			BasePlatform component = raycastHit.collider.gameObject.GetComponent<BasePlatform>();
			if (component != null)
			{
				this.currentPlatform = component;
			}
			if (this.currentOverride != null)
			{
				if (this.currentOverride.slidePercentageOverride >= 0f)
				{
					return this.currentOverride.slidePercentageOverride;
				}
				this.currentMaterialIndex = this.currentOverride.overrideIndex;
				if (this.currentMaterialIndex < 0 || this.currentMaterialIndex >= this.materialData.Count)
				{
					return this.defaultSlideFactor;
				}
				if (!this.materialData[this.currentMaterialIndex].overrideSlidePercent)
				{
					return this.defaultSlideFactor;
				}
				return this.materialData[this.currentMaterialIndex].slidePercent;
			}
			else
			{
				this.meshCollider = raycastHit.collider as MeshCollider;
				if (this.meshCollider == null || this.meshCollider.sharedMesh == null || this.meshCollider.convex)
				{
					return this.defaultSlideFactor;
				}
				this.collidedMesh = this.meshCollider.sharedMesh;
				if (!this.meshTrianglesDict.TryGetValue(this.collidedMesh, out this.sharedMeshTris))
				{
					this.sharedMeshTris = this.collidedMesh.triangles;
					this.meshTrianglesDict.Add(this.collidedMesh, (int[])this.sharedMeshTris.Clone());
				}
				this.vertex1 = this.sharedMeshTris[raycastHit.triangleIndex * 3];
				this.vertex2 = this.sharedMeshTris[raycastHit.triangleIndex * 3 + 1];
				this.vertex3 = this.sharedMeshTris[raycastHit.triangleIndex * 3 + 2];
				this.slideRenderer = raycastHit.collider.GetComponent<Renderer>();
				if (this.slideRenderer != null)
				{
					this.slideRenderer.GetSharedMaterials(this.tempMaterialArray);
				}
				else
				{
					this.tempMaterialArray.Clear();
				}
				if (this.tempMaterialArray.Count > 1)
				{
					for (int i = 0; i < this.tempMaterialArray.Count; i++)
					{
						this.collidedMesh.GetTriangles(this.trianglesList, i);
						int j = 0;
						while (j < this.trianglesList.Count)
						{
							if (this.trianglesList[j] == this.vertex1 && this.trianglesList[j + 1] == this.vertex2 && this.trianglesList[j + 2] == this.vertex3)
							{
								this.findMatName = this.tempMaterialArray[i].name;
								if (this.findMatName.EndsWith("Uber"))
								{
									string text = this.findMatName;
									this.findMatName = text.Substring(0, text.Length - 4);
								}
								this.foundMatData = this.materialData.Find((GTPlayer.MaterialData matData) => matData.matName == this.findMatName);
								this.currentMaterialIndex = this.materialData.FindIndex((GTPlayer.MaterialData matData) => matData.matName == this.findMatName);
								if (this.currentMaterialIndex == -1)
								{
									this.currentMaterialIndex = 0;
								}
								if (!this.foundMatData.overrideSlidePercent)
								{
									return this.defaultSlideFactor;
								}
								return this.foundMatData.slidePercent;
							}
							else
							{
								j += 3;
							}
						}
					}
				}
				else if (this.tempMaterialArray.Count > 0)
				{
					return this.defaultSlideFactor;
				}
				this.currentMaterialIndex = 0;
				return this.defaultSlideFactor;
			}
		}

		public bool IsTouchingMovingSurface(Vector3 rayOrigin, RaycastHit raycastHit, out int movingSurfaceId, out bool sideTouch, out bool isMonkeBlock)
		{
			movingSurfaceId = -1;
			sideTouch = false;
			isMonkeBlock = false;
			float num = Vector3.Dot(rayOrigin - raycastHit.point, Vector3.up);
			if (num < -0.3f)
			{
				return false;
			}
			if (num < 0f)
			{
				sideTouch = true;
			}
			if (raycastHit.collider == null)
			{
				return false;
			}
			MovingSurface component = raycastHit.collider.GetComponent<MovingSurface>();
			if (component != null)
			{
				isMonkeBlock = false;
				movingSurfaceId = component.GetID();
				return true;
			}
			if (!BuilderTable.IsLocalPlayerInBuilderZone())
			{
				return false;
			}
			BuilderPiece builderPieceFromCollider = BuilderPiece.GetBuilderPieceFromCollider(raycastHit.collider);
			if (builderPieceFromCollider != null && builderPieceFromCollider.IsPieceMoving())
			{
				isMonkeBlock = true;
				movingSurfaceId = builderPieceFromCollider.pieceId;
				this.lastMonkeBlock = builderPieceFromCollider;
				return true;
			}
			sideTouch = false;
			return false;
		}

		public void Turn(float degrees)
		{
			Vector3 vector = this.headCollider.transform.position;
			bool flag = this.rightHand.isColliding || this.rightHand.isHolding;
			bool flag2 = this.leftHand.isColliding || this.leftHand.isHolding;
			if (flag != flag2 && flag)
			{
				vector = this.rightHand.controllerTransform.position;
			}
			if (flag != flag2 && flag2)
			{
				vector = this.leftHand.controllerTransform.position;
			}
			this.turnParent.transform.RotateAround(vector, base.transform.up, degrees);
			this.degreesTurnedThisFrame = degrees;
			this.averagedVelocity = Vector3.zero;
			for (int i = 0; i < this.velocityHistory.Length; i++)
			{
				this.velocityHistory[i] = Quaternion.Euler(0f, degrees, 0f) * this.velocityHistory[i];
				this.averagedVelocity += this.velocityHistory[i];
			}
			this.averagedVelocity /= (float)this.velocityHistorySize;
		}

		public void BeginClimbing(GorillaClimbable climbable, GorillaHandClimber hand, GorillaClimbableRef climbableRef = null)
		{
			if (this.currentClimber != null)
			{
				this.EndClimbing(this.currentClimber, true, false);
			}
			try
			{
				Action<GorillaHandClimber, GorillaClimbableRef> onBeforeClimb = climbable.onBeforeClimb;
				if (onBeforeClimb != null)
				{
					onBeforeClimb(hand, climbableRef);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
			Rigidbody rigidbody;
			climbable.TryGetComponent<Rigidbody>(out rigidbody);
			this.VerifyClimbHelper();
			this.climbHelper.SetParent(climbable.transform);
			this.climbHelper.position = hand.transform.position;
			Vector3 localPosition = this.climbHelper.localPosition;
			if (climbable.snapX)
			{
				GTPlayer.<BeginClimbing>g__SnapAxis|405_0(ref localPosition.x, climbable.maxDistanceSnap);
			}
			if (climbable.snapY)
			{
				GTPlayer.<BeginClimbing>g__SnapAxis|405_0(ref localPosition.y, climbable.maxDistanceSnap);
			}
			if (climbable.snapZ)
			{
				GTPlayer.<BeginClimbing>g__SnapAxis|405_0(ref localPosition.z, climbable.maxDistanceSnap);
			}
			this.climbHelperTargetPos = localPosition;
			climbable.isBeingClimbed = true;
			hand.isClimbing = true;
			this.currentClimbable = climbable;
			this.currentClimber = hand;
			this.isClimbing = true;
			if (climbable.climbOnlyWhileSmall)
			{
				BuilderPiece componentInParent = climbable.GetComponentInParent<BuilderPiece>();
				if (componentInParent != null && componentInParent.IsPieceMoving())
				{
					this.isClimbableMoving = true;
					this.lastClimbableRotation = climbable.transform.rotation;
				}
				else
				{
					this.isClimbableMoving = false;
				}
			}
			else
			{
				this.isClimbableMoving = false;
			}
			GorillaRopeSegment gorillaRopeSegment;
			GorillaZipline gorillaZipline;
			PhotonView photonView;
			PhotonViewXSceneRef photonViewXSceneRef;
			if (climbable.TryGetComponent<GorillaRopeSegment>(out gorillaRopeSegment) && gorillaRopeSegment.swing)
			{
				this.currentSwing = gorillaRopeSegment.swing;
				this.currentSwing.AttachLocalPlayer(hand.xrNode, climbable.transform, this.climbHelperTargetPos, this.averagedVelocity);
			}
			else if (climbable.transform.parent && climbable.transform.parent.TryGetComponent<GorillaZipline>(out gorillaZipline))
			{
				this.currentZipline = gorillaZipline;
			}
			else if (climbable.TryGetComponent<PhotonView>(out photonView))
			{
				VRRig.AttachLocalPlayerToPhotonView(photonView, hand.xrNode, this.climbHelperTargetPos, this.averagedVelocity);
			}
			else if (climbable.TryGetComponent<PhotonViewXSceneRef>(out photonViewXSceneRef))
			{
				VRRig.AttachLocalPlayerToPhotonView(photonViewXSceneRef.photonView, hand.xrNode, this.climbHelperTargetPos, this.averagedVelocity);
			}
			GorillaTagger.Instance.StartVibration(this.currentClimber.xrNode == XRNode.LeftHand, 0.6f, 0.06f);
			if (climbable.clip)
			{
				GorillaTagger.Instance.offlineVRRig.PlayClimbSound(climbable.clip, hand.xrNode == XRNode.LeftHand);
			}
		}

		private void VerifyClimbHelper()
		{
			if (this.climbHelper == null || this.climbHelper.gameObject == null)
			{
				this.climbHelper = new GameObject("Climb Helper").transform;
			}
		}

		public void EndClimbing(GorillaHandClimber hand, bool startingNewClimb, bool doDontReclimb = false)
		{
			if (hand != this.currentClimber)
			{
				return;
			}
			hand.SetCanRelease(true);
			if (!startingNewClimb)
			{
				this.enablePlayerGravity(true);
			}
			Rigidbody rigidbody = null;
			if (this.currentClimbable)
			{
				this.currentClimbable.TryGetComponent<Rigidbody>(out rigidbody);
				this.currentClimbable.isBeingClimbed = false;
			}
			Vector3 vector = Vector3.zero;
			if (this.currentClimber)
			{
				this.currentClimber.isClimbing = false;
				if (doDontReclimb)
				{
					this.currentClimber.dontReclimbLast = this.currentClimbable;
				}
				else
				{
					this.currentClimber.dontReclimbLast = null;
				}
				this.currentClimber.queuedToBecomeValidToGrabAgain = true;
				this.currentClimber.lastAutoReleasePos = this.currentClimber.handRoot.localPosition;
				if (!startingNewClimb && this.currentClimbable)
				{
					GorillaVelocityTracker interactPointVelocityTracker = this.GetInteractPointVelocityTracker(this.currentClimber.xrNode == XRNode.LeftHand);
					if (rigidbody)
					{
						this.playerRigidBody.linearVelocity = rigidbody.linearVelocity;
					}
					else if (this.currentSwing)
					{
						this.playerRigidBody.linearVelocity = this.currentSwing.velocityTracker.GetAverageVelocity(true, 0.25f, false);
					}
					else if (this.currentZipline)
					{
						this.playerRigidBody.linearVelocity = this.currentZipline.GetCurrentDirection() * this.currentZipline.currentSpeed;
					}
					else
					{
						this.playerRigidBody.linearVelocity = Vector3.zero;
					}
					vector = this.turnParent.transform.rotation * -interactPointVelocityTracker.GetAverageVelocity(false, 0.1f, true) * this.scale;
					vector = Vector3.ClampMagnitude(vector, 5.5f * this.scale);
					this.playerRigidBody.AddForce(vector, ForceMode.VelocityChange);
				}
			}
			if (this.currentSwing)
			{
				this.currentSwing.DetachLocalPlayer();
			}
			PhotonView photonView;
			PhotonViewXSceneRef photonViewXSceneRef;
			if (this.currentClimbable.TryGetComponent<PhotonView>(out photonView) || this.currentClimbable.TryGetComponent<PhotonViewXSceneRef>(out photonViewXSceneRef) || this.currentClimbable.IsPlayerAttached)
			{
				VRRig.DetachLocalPlayerFromPhotonView();
			}
			if (!startingNewClimb && vector.magnitude > 2f && this.currentClimbable && this.currentClimbable.clipOnFullRelease)
			{
				GorillaTagger.Instance.offlineVRRig.PlayClimbSound(this.currentClimbable.clipOnFullRelease, hand.xrNode == XRNode.LeftHand);
			}
			this.currentClimbable = null;
			this.currentClimber = null;
			this.currentSwing = null;
			this.currentZipline = null;
			this.isClimbing = false;
		}

		public void ResetRigidbodyInterpolation()
		{
			this.playerRigidBody.interpolation = this.playerRigidbodyInterpolationDefault;
		}

		public RigidbodyInterpolation RigidbodyInterpolation
		{
			get
			{
				return this.playerRigidBody.interpolation;
			}
			set
			{
				this.playerRigidBody.interpolation = value;
			}
		}

		private void enablePlayerGravity(bool useGravity)
		{
			this.playerRigidBody.useGravity = useGravity;
		}

		public void SetVelocity(Vector3 velocity)
		{
			this.playerRigidBody.linearVelocity = velocity;
		}

		internal void RigidbodyMovePosition(Vector3 pos)
		{
			this.playerRigidBody.MovePosition(pos);
		}

		public void TempFreezeHand(bool isLeft, float freezeDuration)
		{
			(isLeft ? this.leftHand : this.rightHand).TempFreezeHand(freezeDuration);
		}

		private void StoreVelocities()
		{
			this.velocityIndex = (this.velocityIndex + 1) % this.velocityHistorySize;
			this.currentVelocity = (base.transform.position - this.lastPosition - this.MovingSurfaceMovement()) / this.calcDeltaTime;
			this.velocityHistory[this.velocityIndex] = this.currentVelocity;
			this.averagedVelocity = this.velocityHistory.Average();
			this.lastPosition = base.transform.position;
		}

		private void AntiTeleportTechnology()
		{
			if ((this.headCollider.transform.position - this.lastHeadPosition).magnitude >= this.teleportThresholdNoVel + this.playerRigidBody.linearVelocity.magnitude * this.calcDeltaTime)
			{
				this.ForceRigidBodySync();
				base.transform.position = base.transform.position + this.lastHeadPosition - this.headCollider.transform.position;
			}
		}

		private bool MaxSphereSizeForNoOverlap(float testRadius, Vector3 checkPosition, bool ignoreOneWay, out float overlapRadiusTest)
		{
			overlapRadiusTest = testRadius;
			this.overlapAttempts = 0;
			int num = 100;
			while (this.overlapAttempts < num && overlapRadiusTest > testRadius * 0.75f)
			{
				this.ClearColliderBuffer(ref this.overlapColliders);
				this.bufferCount = Physics.OverlapSphereNonAlloc(checkPosition, overlapRadiusTest, this.overlapColliders, this.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore);
				if (ignoreOneWay)
				{
					int num2 = 0;
					for (int i = 0; i < this.bufferCount; i++)
					{
						if (this.overlapColliders[i].CompareTag("NoCrazyCheck"))
						{
							num2++;
						}
					}
					if (num2 == this.bufferCount)
					{
						return true;
					}
				}
				if (this.bufferCount <= 0)
				{
					overlapRadiusTest *= 0.995f;
					return true;
				}
				overlapRadiusTest = Mathf.Lerp(testRadius, 0f, (float)this.overlapAttempts / (float)num);
				this.overlapAttempts++;
			}
			return false;
		}

		private bool CrazyCheck2(float sphereSize, Vector3 startPosition)
		{
			for (int i = 0; i < this.crazyCheckVectors.Length; i++)
			{
				if (this.NonAllocRaycast(startPosition, startPosition + this.crazyCheckVectors[i] * sphereSize) > 0)
				{
					return false;
				}
			}
			return true;
		}

		private int NonAllocRaycast(Vector3 startPosition, Vector3 endPosition)
		{
			Vector3 vector = endPosition - startPosition;
			int num = Physics.RaycastNonAlloc(startPosition, vector, this.rayCastNonAllocColliders, vector.magnitude, this.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore);
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				if (!this.rayCastNonAllocColliders[i].collider.gameObject.CompareTag("NoCrazyCheck"))
				{
					num2++;
				}
			}
			return num2;
		}

		private void ClearColliderBuffer(ref Collider[] colliders)
		{
			for (int i = 0; i < colliders.Length; i++)
			{
				colliders[i] = null;
			}
		}

		private void ClearRaycasthitBuffer(ref RaycastHit[] raycastHits)
		{
			for (int i = 0; i < raycastHits.Length; i++)
			{
				raycastHits[i] = this.emptyHit;
			}
		}

		private Vector3 MovingSurfaceMovement()
		{
			return this.refMovement + this.movingSurfaceOffset;
		}

		private static bool ComputeLocalHitPoint(RaycastHit hit, out Vector3 localHitPoint)
		{
			if (hit.collider == null || hit.point.sqrMagnitude < 0.001f)
			{
				localHitPoint = Vector3.zero;
				return false;
			}
			localHitPoint = hit.collider.transform.InverseTransformPoint(hit.point);
			return true;
		}

		private static bool ComputeWorldHitPoint(RaycastHit hit, Vector3 localPoint, out Vector3 worldHitPoint)
		{
			if (hit.collider == null)
			{
				worldHitPoint = Vector3.zero;
				return false;
			}
			worldHitPoint = hit.collider.transform.TransformPoint(localPoint);
			return true;
		}

		private float ExtraVelMultiplier()
		{
			float num = 1f;
			if (this.leftHand.surfaceOverride != null)
			{
				num = Mathf.Max(num, this.leftHand.surfaceOverride.extraVelMultiplier);
			}
			if (this.rightHand.surfaceOverride != null)
			{
				num = Mathf.Max(num, this.rightHand.surfaceOverride.extraVelMultiplier);
			}
			return num;
		}

		private float ExtraVelMaxMultiplier()
		{
			float num = 1f;
			if (this.leftHand.surfaceOverride != null)
			{
				num = Mathf.Max(num, this.leftHand.surfaceOverride.extraVelMaxMultiplier);
			}
			if (this.rightHand.surfaceOverride != null)
			{
				num = Mathf.Max(num, this.rightHand.surfaceOverride.extraVelMaxMultiplier);
			}
			return num * this.scale;
		}

		public void SetMaximumSlipThisFrame()
		{
			this.leftHand.slipSetToMaxFrameIdx = Time.frameCount;
			this.rightHand.slipSetToMaxFrameIdx = Time.frameCount;
		}

		public void SetLeftMaximumSlipThisFrame()
		{
			this.leftHand.slipSetToMaxFrameIdx = Time.frameCount;
		}

		public void SetRightMaximumSlipThisFrame()
		{
			this.rightHand.slipSetToMaxFrameIdx = Time.frameCount;
		}

		public void ChangeLayer(string layerName)
		{
			if (this.layerChanger != null)
			{
				this.layerChanger.ChangeLayer(base.transform.parent, layerName);
			}
		}

		public void RestoreLayer()
		{
			if (this.layerChanger != null)
			{
				this.layerChanger.RestoreOriginalLayers();
			}
		}

		public void OnEnterWaterVolume(Collider playerCollider, WaterVolume volume)
		{
			if (this.activeSizeChangerSettings != null && this.activeSizeChangerSettings.ExpireInWater)
			{
				this.SetNativeScale(null);
			}
			if (playerCollider == this.headCollider)
			{
				if (!this.headOverlappingWaterVolumes.Contains(volume))
				{
					this.headOverlappingWaterVolumes.Add(volume);
					return;
				}
			}
			else if (playerCollider == this.bodyCollider && !this.bodyOverlappingWaterVolumes.Contains(volume))
			{
				this.bodyOverlappingWaterVolumes.Add(volume);
			}
		}

		public void OnExitWaterVolume(Collider playerCollider, WaterVolume volume)
		{
			if (playerCollider == this.headCollider)
			{
				this.headOverlappingWaterVolumes.Remove(volume);
				return;
			}
			if (playerCollider == this.bodyCollider)
			{
				this.bodyOverlappingWaterVolumes.Remove(volume);
			}
		}

		private bool GetSwimmingVelocityForHand(Vector3 startingHandPosition, Vector3 endingHandPosition, Vector3 palmForwardDirection, float dt, ref WaterVolume contactingWaterVolume, ref WaterVolume.SurfaceQuery waterSurface, out Vector3 swimmingVelocityChange)
		{
			contactingWaterVolume = null;
			this.bufferCount = Physics.OverlapSphereNonAlloc(endingHandPosition, this.minimumRaycastDistance, this.overlapColliders, this.waterLayer.value, QueryTriggerInteraction.Collide);
			if (this.bufferCount > 0)
			{
				float num = float.MinValue;
				for (int i = 0; i < this.bufferCount; i++)
				{
					WaterVolume component = this.overlapColliders[i].GetComponent<WaterVolume>();
					WaterVolume.SurfaceQuery surfaceQuery;
					if (component != null && component.GetSurfaceQueryForPoint(endingHandPosition, out surfaceQuery, false) && surfaceQuery.surfacePoint.y > num)
					{
						num = surfaceQuery.surfacePoint.y;
						contactingWaterVolume = component;
						waterSurface = surfaceQuery;
					}
				}
			}
			if (contactingWaterVolume != null)
			{
				Vector3 vector = endingHandPosition - startingHandPosition;
				Vector3 vector2 = Vector3.zero;
				Vector3 vector3 = this.playerRigidBody.transform.position - this.lastRigidbodyPosition;
				if (this.turnedThisFrame)
				{
					Vector3 vector4 = startingHandPosition - this.headCollider.transform.position;
					vector2 = Quaternion.AngleAxis(this.degreesTurnedThisFrame, Vector3.up) * vector4 - vector4;
				}
				float num2 = Vector3.Dot(vector - vector2 - vector3, palmForwardDirection);
				float num3 = 0f;
				if (num2 > 0f)
				{
					Plane surfacePlane = waterSurface.surfacePlane;
					float distanceToPoint = surfacePlane.GetDistanceToPoint(startingHandPosition);
					float distanceToPoint2 = surfacePlane.GetDistanceToPoint(endingHandPosition);
					if (distanceToPoint <= 0f && distanceToPoint2 <= 0f)
					{
						num3 = 1f;
					}
					else if (distanceToPoint > 0f && distanceToPoint2 <= 0f)
					{
						num3 = -distanceToPoint2 / (distanceToPoint - distanceToPoint2);
					}
					else if (distanceToPoint <= 0f && distanceToPoint2 > 0f)
					{
						num3 = -distanceToPoint / (distanceToPoint2 - distanceToPoint);
					}
					if (num3 > Mathf.Epsilon)
					{
						float resistance = this.liquidPropertiesList[(int)contactingWaterVolume.LiquidType].resistance;
						swimmingVelocityChange = -palmForwardDirection * num2 * 2f * resistance * num3;
						Vector3 forward = this.mainCamera.transform.forward;
						if (forward.y < 0f)
						{
							Vector3 vector5 = forward.x0z();
							float magnitude = vector5.magnitude;
							vector5 /= magnitude;
							float num4 = Vector3.Dot(swimmingVelocityChange, vector5);
							if (num4 > 0f)
							{
								Vector3 vector6 = vector5 * num4;
								swimmingVelocityChange = swimmingVelocityChange - vector6 + vector6 * magnitude + Vector3.up * forward.y * num4;
							}
						}
						return true;
					}
				}
			}
			swimmingVelocityChange = Vector3.zero;
			return false;
		}

		private bool CheckWaterSurfaceJump(Vector3 startingHandPosition, Vector3 endingHandPosition, Vector3 palmForwardDirection, Vector3 handAvgVelocity, PlayerSwimmingParameters parameters, WaterVolume contactingWaterVolume, WaterVolume.SurfaceQuery waterSurface, out Vector3 jumpVelocity)
		{
			if (contactingWaterVolume != null)
			{
				Plane surfacePlane = waterSurface.surfacePlane;
				bool flag = handAvgVelocity.sqrMagnitude > parameters.waterSurfaceJumpHandSpeedThreshold * parameters.waterSurfaceJumpHandSpeedThreshold;
				if (surfacePlane.GetSide(startingHandPosition) && !surfacePlane.GetSide(endingHandPosition) && flag)
				{
					float num = Vector3.Dot(palmForwardDirection, -waterSurface.surfaceNormal);
					float num2 = Vector3.Dot(handAvgVelocity.normalized, -waterSurface.surfaceNormal);
					float num3 = parameters.waterSurfaceJumpPalmFacingCurve.Evaluate(Mathf.Clamp(num, 0.01f, 0.99f));
					float num4 = parameters.waterSurfaceJumpHandVelocityFacingCurve.Evaluate(Mathf.Clamp(num2, 0.01f, 0.99f));
					jumpVelocity = -handAvgVelocity * parameters.waterSurfaceJumpAmount * num3 * num4;
					return true;
				}
			}
			jumpVelocity = Vector3.zero;
			return false;
		}

		private bool TryNormalize(Vector3 input, out Vector3 normalized, out float magnitude, float eps = 0.0001f)
		{
			magnitude = input.magnitude;
			if (magnitude > eps)
			{
				normalized = input / magnitude;
				return true;
			}
			normalized = Vector3.zero;
			return false;
		}

		private bool TryNormalizeDown(Vector3 input, out Vector3 normalized, out float magnitude, float eps = 0.0001f)
		{
			magnitude = input.magnitude;
			if (magnitude > 1f)
			{
				normalized = input / magnitude;
				return true;
			}
			if (magnitude >= eps)
			{
				normalized = input;
				return true;
			}
			normalized = Vector3.zero;
			return false;
		}

		private float FreezeTagSlidePercentage()
		{
			if (this.materialData[this.currentMaterialIndex].overrideSlidePercent && this.materialData[this.currentMaterialIndex].slidePercent > this.freezeTagHandSlidePercent)
			{
				return this.materialData[this.currentMaterialIndex].slidePercent;
			}
			return this.freezeTagHandSlidePercent;
		}

		private void OnCollisionStay(global::UnityEngine.Collision collision)
		{
			this.bodyCollisionContactsCount = collision.GetContacts(this.bodyCollisionContacts);
			float num = -1f;
			for (int i = 0; i < this.bodyCollisionContactsCount; i++)
			{
				float num2 = Vector3.Dot(this.bodyCollisionContacts[i].normal, Vector3.up);
				if (num2 > num)
				{
					this.bodyGroundContact = this.bodyCollisionContacts[i];
					num = num2;
				}
			}
			float num3 = 0.5f;
			if (num > num3)
			{
				this.bodyGroundContactTime = Time.time;
			}
		}

		public async void DoLaunch(Vector3 velocity)
		{
			if (this.isClimbing)
			{
				this.EndClimbing(this.CurrentClimber, false, false);
			}
			this.playerRigidBody.linearVelocity = velocity;
			this.disableMovement = true;
			await Task.Delay(1);
			this.disableMovement = false;
		}

		private void OnEnable()
		{
			RoomSystem.JoinedRoomEvent += new Action(this.OnJoinedRoom);
		}

		private void OnJoinedRoom()
		{
			if (this.activeSizeChangerSettings != null && this.activeSizeChangerSettings.ExpireOnRoomJoin)
			{
				this.SetNativeScale(null);
			}
		}

		private void OnDisable()
		{
			RoomSystem.JoinedRoomEvent -= new Action(this.OnJoinedRoom);
		}

		public void ForceRigidBodySync()
		{
			this.forceRBSync = true;
		}

		internal void ClearHandHolds()
		{
			this.leftHand.isHolding = false;
			this.rightHand.isHolding = false;
			this.wasHoldingHandhold = false;
			this.activeHandHold = default(GTPlayer.HandHoldState);
			this.secondaryHandHold = default(GTPlayer.HandHoldState);
			this.OnChangeActiveHandhold();
		}

		internal void AddHandHold(Transform objectHeld, Vector3 localPositionHeld, GorillaGrabber grabber, bool forLeftHand, bool rotatePlayerWhenHeld, out Vector3 grabbedVelocity)
		{
			if (!this.leftHand.isHolding && !this.rightHand.isHolding)
			{
				grabbedVelocity = -this.bodyCollider.attachedRigidbody.linearVelocity;
				this.playerRigidBody.AddForce(grabbedVelocity, ForceMode.VelocityChange);
			}
			else
			{
				grabbedVelocity = Vector3.zero;
			}
			this.secondaryHandHold = this.activeHandHold;
			Vector3 position = grabber.transform.position;
			this.activeHandHold = new GTPlayer.HandHoldState
			{
				grabber = grabber,
				objectHeld = objectHeld,
				localPositionHeld = localPositionHeld,
				localRotationalOffset = grabber.transform.rotation.eulerAngles.y - objectHeld.rotation.eulerAngles.y,
				applyRotation = rotatePlayerWhenHeld
			};
			if (forLeftHand)
			{
				this.leftHand.isHolding = true;
			}
			else
			{
				this.rightHand.isHolding = true;
			}
			this.OnChangeActiveHandhold();
		}

		internal void RemoveHandHold(GorillaGrabber grabber, bool forLeftHand)
		{
			this.activeHandHold.objectHeld == grabber;
			if (this.activeHandHold.grabber == grabber)
			{
				this.activeHandHold = this.secondaryHandHold;
			}
			this.secondaryHandHold = default(GTPlayer.HandHoldState);
			if (forLeftHand)
			{
				this.leftHand.isHolding = false;
			}
			else
			{
				this.rightHand.isHolding = false;
			}
			this.OnChangeActiveHandhold();
		}

		private void OnChangeActiveHandhold()
		{
			if (this.activeHandHold.objectHeld != null)
			{
				PhotonView photonView;
				if (this.activeHandHold.objectHeld.TryGetComponent<PhotonView>(out photonView))
				{
					VRRig.AttachLocalPlayerToPhotonView(photonView, this.activeHandHold.grabber.XrNode, this.activeHandHold.localPositionHeld, this.averagedVelocity);
					return;
				}
				PhotonViewXSceneRef photonViewXSceneRef;
				if (this.activeHandHold.objectHeld.TryGetComponent<PhotonViewXSceneRef>(out photonViewXSceneRef))
				{
					PhotonView photonView2 = photonViewXSceneRef.photonView;
					if (photonView2 != null)
					{
						VRRig.AttachLocalPlayerToPhotonView(photonView2, this.activeHandHold.grabber.XrNode, this.activeHandHold.localPositionHeld, this.averagedVelocity);
						return;
					}
				}
				BuilderPieceHandHold builderPieceHandHold;
				if (this.activeHandHold.objectHeld.TryGetComponent<BuilderPieceHandHold>(out builderPieceHandHold) && builderPieceHandHold.IsHandHoldMoving())
				{
					this.isHandHoldMoving = true;
					this.lastHandHoldRotation = builderPieceHandHold.transform.rotation;
					this.movingHandHoldReleaseVelocity = this.playerRigidBody.linearVelocity;
				}
				else
				{
					this.isHandHoldMoving = false;
					this.lastHandHoldRotation = Quaternion.identity;
					this.movingHandHoldReleaseVelocity = Vector3.zero;
				}
			}
			VRRig.DetachLocalPlayerFromPhotonView();
		}

		private void FixedUpdate_HandHolds(float timeDelta)
		{
			if (this.activeHandHold.objectHeld == null)
			{
				if (this.wasHoldingHandhold)
				{
					this.playerRigidBody.linearVelocity = Vector3.ClampMagnitude(this.secondLastPreHandholdVelocity, 5.5f * this.scale);
				}
				this.wasHoldingHandhold = false;
				return;
			}
			Vector3 vector = this.activeHandHold.objectHeld.TransformPoint(this.activeHandHold.localPositionHeld);
			Vector3 position = this.activeHandHold.grabber.transform.position;
			this.secondLastPreHandholdVelocity = this.lastPreHandholdVelocity;
			this.lastPreHandholdVelocity = this.playerRigidBody.linearVelocity;
			this.wasHoldingHandhold = true;
			if (this.isHandHoldMoving)
			{
				this.lastPreHandholdVelocity = this.movingHandHoldReleaseVelocity;
				this.playerRigidBody.linearVelocity = Vector3.zero;
				Vector3 vector2 = vector - position;
				this.playerRigidBody.transform.position += vector2;
				this.movingHandHoldReleaseVelocity = vector2 / timeDelta;
				Quaternion quaternion = this.activeHandHold.objectHeld.rotation * Quaternion.Inverse(this.lastHandHoldRotation);
				this.RotateWithSurface(quaternion, vector);
				this.lastHandHoldRotation = this.activeHandHold.objectHeld.rotation;
				return;
			}
			this.playerRigidBody.linearVelocity = (vector - position) / timeDelta;
			if (this.activeHandHold.applyRotation)
			{
				this.turnParent.transform.RotateAround(vector, base.transform.up, this.activeHandHold.localRotationalOffset - (this.activeHandHold.grabber.transform.rotation.eulerAngles.y - this.activeHandHold.objectHeld.rotation.eulerAngles.y));
			}
		}

		[CompilerGenerated]
		internal static void <BeginClimbing>g__SnapAxis|405_0(ref float val, float maxDist)
		{
			if (val > maxDist)
			{
				val = maxDist;
				return;
			}
			if (val < -maxDist)
			{
				val = -maxDist;
			}
		}

		private static GTPlayer _instance;

		public static bool hasInstance;

		public Camera mainCamera;

		public SphereCollider headCollider;

		public CapsuleCollider bodyCollider;

		private float bodyInitialRadius;

		private float bodyInitialHeight;

		private RaycastHit bodyHitInfo;

		private RaycastHit lastHitInfoHand;

		public GorillaVelocityTracker bodyVelocityTracker;

		public PlayerAudioManager audioManager;

		[SerializeField]
		private GTPlayer.HandState leftHand;

		[SerializeField]
		private GTPlayer.HandState rightHand;

		private GTPlayer.HandState[] stiltStates = new GTPlayer.HandState[12];

		private bool anyHandIsColliding;

		private bool anyHandWasColliding;

		private bool anyHandIsSliding;

		private bool anyHandWasSliding;

		private bool anyHandIsSticking;

		private bool anyHandWasSticking;

		private bool forceRBSync;

		public Vector3 lastHeadPosition;

		private Vector3 lastRigidbodyPosition;

		private Rigidbody playerRigidBody;

		private RigidbodyInterpolation playerRigidbodyInterpolationDefault;

		public int velocityHistorySize;

		public float maxArmLength = 1f;

		public float unStickDistance = 1f;

		public float velocityLimit;

		public float slideVelocityLimit;

		public float maxJumpSpeed;

		private float _jumpMultiplier;

		public float minimumRaycastDistance = 0.05f;

		public float defaultSlideFactor = 0.03f;

		public float slidingMinimum = 0.9f;

		public float defaultPrecision = 0.995f;

		public float teleportThresholdNoVel = 1f;

		public float frictionConstant = 1f;

		public float slideControl = 0.00425f;

		public float stickDepth = 0.01f;

		private Vector3[] velocityHistory;

		private Vector3[] slideAverageHistory;

		private int velocityIndex;

		private Vector3 currentVelocity;

		private Vector3 averagedVelocity;

		private Vector3 lastPosition;

		public Vector3 bodyOffset;

		public LayerMask locomotionEnabledLayers;

		public LayerMask waterLayer;

		public bool wasHeadTouching;

		public int currentMaterialIndex;

		public Vector3 headSlideNormal;

		public float headSlipPercentage;

		[SerializeField]
		private Transform cosmeticsHeadTarget;

		[SerializeField]
		private float nativeScale = 1f;

		[SerializeField]
		private float scaleMultiplier = 1f;

		private NativeSizeChangerSettings activeSizeChangerSettings;

		public bool debugMovement;

		public bool disableMovement;

		[NonSerialized]
		public bool inOverlay;

		[NonSerialized]
		public bool isUserPresent;

		public GameObject turnParent;

		public GorillaSurfaceOverride currentOverride;

		public MaterialDatasSO materialDatasSO;

		private float degreesTurnedThisFrame;

		private Vector3 bodyOffsetVector;

		private Vector3 movementToProjectedAboveCollisionPlane;

		private MeshCollider meshCollider;

		private Mesh collidedMesh;

		private GTPlayer.MaterialData foundMatData;

		private string findMatName;

		private int vertex1;

		private int vertex2;

		private int vertex3;

		private List<int> trianglesList = new List<int>(1000000);

		private Dictionary<Mesh, int[]> meshTrianglesDict = new Dictionary<Mesh, int[]>(128);

		private int[] sharedMeshTris;

		private float lastRealTime;

		private float calcDeltaTime;

		private float tempRealTime;

		private Vector3 slideVelocity;

		private Vector3 slideAverageNormal;

		private RaycastHit tempHitInfo;

		private RaycastHit junkHit;

		private Vector3 firstPosition;

		private RaycastHit tempIterativeHit;

		private float maxSphereSize1;

		private float maxSphereSize2;

		private Collider[] overlapColliders = new Collider[10];

		private int overlapAttempts;

		private float averageSlipPercentage;

		private Vector3 surfaceDirection;

		public float iceThreshold = 0.9f;

		private float bodyMaxRadius;

		public float bodyLerp = 0.17f;

		private bool areBothTouching;

		private float slideFactor;

		[DebugOption]
		public bool didAJump;

		private bool updateRB;

		private Renderer slideRenderer;

		private RaycastHit[] rayCastNonAllocColliders;

		private Vector3[] crazyCheckVectors;

		private RaycastHit emptyHit;

		private int bufferCount;

		private Vector3 lastOpenHeadPosition;

		private List<Material> tempMaterialArray = new List<Material>(16);

		private Vector3? antiDriftLastPosition;

		private const float CameraFarClipDefault = 500f;

		private const float CameraNearClipDefault = 0.01f;

		private const float CameraNearClipTiny = 0.002f;

		private Dictionary<GameObject, PhysicsMaterial> bodyTouchedSurfaces;

		private bool primaryButtonPressed = true;

		[Header("Swimming")]
		public PlayerSwimmingParameters swimmingParams;

		public WaterParameters waterParams;

		public List<GTPlayer.LiquidProperties> liquidPropertiesList = new List<GTPlayer.LiquidProperties>(16);

		public bool debugDrawSwimming;

		[Header("Slam/Hit effects")]
		public GameObject wizardStaffSlamEffects;

		public GameObject geodeHitEffects;

		[Header("Freeze Tag")]
		public float freezeTagHandSlidePercent = 0.88f;

		public bool debugFreezeTag;

		public float frozenBodyBuoyancyFactor = 1.5f;

		[Space]
		private WaterVolume leftHandWaterVolume;

		private WaterVolume rightHandWaterVolume;

		private WaterVolume.SurfaceQuery leftHandWaterSurface;

		private WaterVolume.SurfaceQuery rightHandWaterSurface;

		private Vector3 swimmingVelocity = Vector3.zero;

		private WaterVolume.SurfaceQuery waterSurfaceForHead;

		private bool bodyInWater;

		private bool headInWater;

		private bool audioSetToUnderwater;

		private float buoyancyExtension;

		private float lastWaterSurfaceJumpTimeLeft = -1f;

		private float lastWaterSurfaceJumpTimeRight = -1f;

		private float waterSurfaceJumpCooldown = 0.1f;

		private float leftHandNonDiveHapticsAmount;

		private float rightHandNonDiveHapticsAmount;

		private List<WaterVolume> headOverlappingWaterVolumes = new List<WaterVolume>(16);

		private List<WaterVolume> bodyOverlappingWaterVolumes = new List<WaterVolume>(16);

		private List<WaterCurrent> activeWaterCurrents = new List<WaterCurrent>(16);

		private Quaternion playerRotationOverride;

		private int playerRotationOverrideFrame = -1;

		private float playerRotationOverrideDecayRate = Mathf.Exp(1.5f);

		private ContactPoint[] bodyCollisionContacts = new ContactPoint[8];

		private int bodyCollisionContactsCount;

		private ContactPoint bodyGroundContact;

		private float bodyGroundContactTime;

		private const float movingSurfaceVelocityLimit = 40f;

		private bool exitMovingSurface;

		private float exitMovingSurfaceThreshold = 6f;

		private bool isClimbableMoving;

		private Quaternion lastClimbableRotation;

		private int lastAttachedToMovingSurfaceFrame;

		private const int MIN_FRAMES_OFF_SURFACE_TO_DETACH = 3;

		private bool isHandHoldMoving;

		private Quaternion lastHandHoldRotation;

		private Vector3 movingHandHoldReleaseVelocity;

		private GTPlayer.MovingSurfaceContactPoint lastMovingSurfaceContact;

		private int lastMovingSurfaceID = -1;

		private BuilderPiece lastMonkeBlock;

		private Quaternion lastMovingSurfaceRot;

		private RaycastHit lastMovingSurfaceHit;

		private Vector3 lastMovingSurfaceTouchLocal;

		private Vector3 lastMovingSurfaceTouchWorld;

		private Vector3 movingSurfaceOffset;

		private bool wasMovingSurfaceMonkeBlock;

		private Vector3 lastMovingSurfaceVelocity;

		private bool wasBodyOnGround;

		private BasePlatform currentPlatform;

		private BasePlatform lastPlatformTouched;

		private Vector3 lastFrameTouchPosLocal;

		private Vector3 lastFrameTouchPosWorld;

		private bool lastFrameHasValidTouchPos;

		private Vector3 refMovement = Vector3.zero;

		private Vector3 platformTouchOffset;

		private Vector3 debugLastRightHandPosition;

		private Vector3 debugPlatformDeltaPosition;

		public double tempFreezeRightHandEnableTime;

		public double tempFreezeLeftHandEnableTime;

		private const float climbingMaxThrowSpeed = 5.5f;

		private const float climbHelperSmoothSnapSpeed = 12f;

		[NonSerialized]
		public bool isClimbing;

		private GorillaClimbable currentClimbable;

		private GorillaHandClimber currentClimber;

		private Vector3 climbHelperTargetPos = Vector3.zero;

		private Transform climbHelper;

		private GorillaRopeSwing currentSwing;

		private GorillaZipline currentZipline;

		[SerializeField]
		private ConnectedControllerHandler controllerState;

		public int sizeLayerMask;

		public bool InReportMenu;

		private LayerChanger layerChanger;

		private bool hasCorrectedForTracking;

		private float halloweenLevitationStrength;

		private float halloweenLevitationFullStrengthDuration;

		private float halloweenLevitationTotalDuration = 1f;

		private float halloweenLevitationBonusStrength;

		private float halloweenLevitateBonusOffAtYSpeed;

		private float halloweenLevitateBonusFullAtYSpeed = 1f;

		private float lastTouchedGroundTimestamp;

		private bool teleportToTrain;

		public bool isAttachedToTrain;

		private bool stuckLeft;

		private bool stuckRight;

		private float lastScale;

		private Vector3 currentSlopDirection;

		private Vector3 lastSlopeDirection = Vector3.zero;

		private readonly Dictionary<Object, Action<GTPlayer>> gravityOverrides = new Dictionary<Object, Action<GTPlayer>>();

		private int hoverAllowedCount;

		[Header("Hoverboard")]
		[SerializeField]
		private float hoverIdealHeight = 0.5f;

		[SerializeField]
		private float hoverCarveSidewaysSpeedLossFactor = 1f;

		[SerializeField]
		private AnimationCurve hoverCarveAngleResponsiveness;

		[SerializeField]
		private HoverboardVisual hoverboardVisual;

		[SerializeField]
		private float sidewaysDrag = 0.1f;

		[SerializeField]
		private float hoveringSlowSpeed = 0.1f;

		[SerializeField]
		private float hoveringSlowStoppingFactor = 0.95f;

		[SerializeField]
		private float hoverboardPaddleBoostMultiplier = 0.1f;

		[SerializeField]
		private float hoverboardPaddleBoostMax = 10f;

		[SerializeField]
		private float hoverboardBoostGracePeriod = 1f;

		[SerializeField]
		private float hoverBodyHasCollisionsOutsideRadius = 0.5f;

		[SerializeField]
		private float hoverBodyCollisionRadiusUpOffset = 0.2f;

		[SerializeField]
		private float hoverGeneralUpwardForce = 8f;

		[SerializeField]
		private float hoverTiltAdjustsForwardFactor = 0.2f;

		[SerializeField]
		private float hoverMinGrindSpeed = 1f;

		[SerializeField]
		private float hoverSlamJumpStrengthFactor = 25f;

		[SerializeField]
		private float hoverMaxPaddleSpeed = 35f;

		[SerializeField]
		private HoverboardAudio hoverboardAudio;

		private bool hasHoverPoint;

		private float boostEnabledUntilTimestamp;

		private GTPlayer.HoverBoardCast[] hoverboardCasts = new GTPlayer.HoverBoardCast[]
		{
			new GTPlayer.HoverBoardCast
			{
				localOrigin = new Vector3(0f, 1f, 0.36f),
				localDirection = Vector3.down,
				distance = 1f,
				sphereRadius = 0.2f,
				intersectToVelocityCap = 0.1f
			},
			new GTPlayer.HoverBoardCast
			{
				localOrigin = new Vector3(0f, 0.05f, 0.36f),
				localDirection = Vector3.forward,
				distance = 0.25f,
				sphereRadius = 0.01f,
				intersectToVelocityCap = 0f,
				isSolid = true
			},
			new GTPlayer.HoverBoardCast
			{
				localOrigin = new Vector3(0f, 0.05f, -0.1f),
				localDirection = -Vector3.forward,
				distance = 0.24f,
				sphereRadius = 0.01f,
				intersectToVelocityCap = 0f,
				isSolid = true
			}
		};

		private Vector3 hoverboardPlayerLocalPos;

		private Quaternion hoverboardPlayerLocalRot;

		private bool didHoverLastFrame;

		private bool hasLeftHandTentacleMove;

		private bool hasRightHandTentacleMove;

		private Vector3 leftHandTentacleMove;

		private Vector3 rightHandTentacleMove;

		private GTPlayer.HandHoldState activeHandHold;

		private GTPlayer.HandHoldState secondaryHandHold;

		public PhysicsMaterial slipperyMaterial;

		private bool wasHoldingHandhold;

		private Vector3 secondLastPreHandholdVelocity;

		private Vector3 lastPreHandholdVelocity;

		[Header("Native Scale Adjustment")]
		[SerializeField]
		private AnimationCurve nativeScaleMagnitudeAdjustmentFactor;

		[Serializable]
		public struct HandState
		{
			public void Init(GTPlayer gtPlayer, bool isLeftHand, float maxArmLength)
			{
				this.gtPlayer = gtPlayer;
				this.isLeftHand = isLeftHand;
				this.maxArmLength = maxArmLength;
				this.lastPosition = this.controllerTransform.position;
				this.lastRotation = this.controllerTransform.rotation;
				if (this.handFollower != null)
				{
					this.handFollower.transform.position = this.lastPosition;
					this.handFollower.transform.rotation = this.lastRotation;
				}
				this.wasColliding = false;
				this.slipSetToMaxFrameIdx = -1;
			}

			public void OnTeleport()
			{
				this.wasColliding = false;
				this.isColliding = false;
				this.isSliding = false;
				this.wasSliding = false;
				this.handFollower.position = this.controllerTransform.position;
				this.handFollower.rotation = this.controllerTransform.rotation;
				this.lastPosition = this.handFollower.transform.position;
				this.lastRotation = this.handFollower.transform.rotation;
			}

			public Vector3 GetLastPosition()
			{
				return this.lastPosition + this.gtPlayer.MovingSurfaceMovement();
			}

			public bool SlipOverriddenToMax()
			{
				return this.slipSetToMaxFrameIdx == Time.frameCount;
			}

			public void FirstIteration(ref Vector3 totalMove, ref int divisor, float paddleBoostFactor)
			{
				if (this.hasCustomBoost)
				{
					this.boostVectorThisFrame = this.gtPlayer.turnParent.transform.rotation * -this.velocityTracker.GetAverageVelocity(false, 0.15f, false) * this.customBoostFactor;
				}
				else
				{
					this.boostVectorThisFrame = (this.gtPlayer.enableHoverMode ? (this.gtPlayer.turnParent.transform.rotation * -this.velocityTracker.GetAverageVelocity(false, 0.15f, false) * paddleBoostFactor) : Vector3.zero);
				}
				Vector3 vector = this.GetCurrentHandPosition() + this.gtPlayer.movingSurfaceOffset;
				Vector3 vector2 = this.GetLastPosition();
				Vector3 vector3 = vector - vector2;
				bool flag = this.gtPlayer.lastMovingSurfaceContact == GTPlayer.MovingSurfaceContactPoint.LEFT;
				if (!this.gtPlayer.didAJump && this.wasSliding && Vector3.Dot(this.gtPlayer.slideAverageNormal, Vector3.up) > 0f)
				{
					vector3 += Vector3.Project(-this.gtPlayer.slideAverageNormal * this.gtPlayer.stickDepth * this.gtPlayer.scale, Vector3.down);
				}
				float num = this.gtPlayer.minimumRaycastDistance * this.gtPlayer.scale;
				if (this.gtPlayer.IsFrozen && GorillaGameManager.instance is GorillaFreezeTagManager)
				{
					num = (this.gtPlayer.minimumRaycastDistance + VRRig.LocalRig.iceCubeRight.transform.localScale.y / 2f) * this.gtPlayer.scale;
				}
				Vector3 vector4 = Vector3.zero;
				if (flag && !this.gtPlayer.exitMovingSurface)
				{
					vector4 = Vector3.Project(-this.gtPlayer.lastMovingSurfaceHit.normal * (this.gtPlayer.stickDepth * this.gtPlayer.scale), Vector3.down);
					if (this.gtPlayer.scale < 0.5f)
					{
						Vector3 normalized = this.gtPlayer.MovingSurfaceMovement().normalized;
						if (normalized != Vector3.zero)
						{
							float num2 = Vector3.Dot(Vector3.up, normalized);
							if ((double)num2 > 0.9 || (double)num2 < -0.9)
							{
								vector4 *= 6f;
								num *= 1.1f;
							}
						}
					}
				}
				Vector3 vector5;
				RaycastHit raycastHit;
				Vector3 vector6;
				if (this.gtPlayer.IterativeCollisionSphereCast(vector2, num, vector3 + vector4, this.boostVectorThisFrame, out vector5, true, out this.slipPercentage, out raycastHit, this.SlipOverriddenToMax()) && !this.isHolding && !this.gtPlayer.InReportMenu)
				{
					if (this.wasColliding && this.slipPercentage <= this.gtPlayer.defaultSlideFactor && !this.boostVectorThisFrame.IsLongerThan(0f))
					{
						vector6 = vector2 - vector;
					}
					else
					{
						vector6 = vector5 - vector;
					}
					this.isSliding = this.slipPercentage > this.gtPlayer.iceThreshold;
					this.slideNormal = this.gtPlayer.tempHitInfo.normal;
					this.isColliding = true;
					this.materialTouchIndex = this.gtPlayer.currentMaterialIndex;
					this.surfaceOverride = this.gtPlayer.currentOverride;
					this.gtPlayer.lastHitInfoHand = raycastHit;
					this.lastHitInfo = raycastHit;
				}
				else
				{
					vector6 = Vector3.zero;
					this.slipPercentage = 0f;
					this.isSliding = false;
					this.slideNormal = Vector3.up;
					this.isColliding = false;
					this.materialTouchIndex = 0;
					this.surfaceOverride = null;
				}
				bool flag2 = (this.isLeftHand ? this.gtPlayer.controllerState.LeftValid : this.gtPlayer.controllerState.RightValid);
				this.isColliding = this.isColliding && flag2;
				this.isSliding = this.isSliding && flag2;
				if (this.isColliding)
				{
					this.gtPlayer.anyHandIsColliding = true;
					if (this.isSliding)
					{
						this.gtPlayer.anyHandIsSliding = true;
					}
					else
					{
						this.gtPlayer.anyHandIsSticking = true;
					}
				}
				if (this.isColliding || this.wasColliding)
				{
					if (!this.surfaceOverride || !this.surfaceOverride.disablePushBackEffect)
					{
						totalMove += vector6;
					}
					divisor++;
				}
			}

			public void FinalizeHandPosition()
			{
				Vector3 vector = this.GetLastPosition();
				if (Time.time < this.tempFreezeUntilTimestamp)
				{
					this.finalPositionThisFrame = vector;
				}
				else
				{
					Vector3 vector2 = this.GetCurrentHandPosition() - vector;
					float num = this.gtPlayer.minimumRaycastDistance * this.gtPlayer.scale;
					if (this.gtPlayer.IsFrozen && GorillaGameManager.instance is GorillaFreezeTagManager)
					{
						num = (this.gtPlayer.minimumRaycastDistance + VRRig.LocalRig.iceCubeRight.transform.localScale.y / 2f) * this.gtPlayer.scale;
					}
					Vector3 vector3;
					float num2;
					RaycastHit raycastHit;
					if (this.gtPlayer.IterativeCollisionSphereCast(vector, num, vector2, this.boostVectorThisFrame, out vector3, this.gtPlayer.areBothTouching, out num2, out raycastHit, false) && !this.isHolding)
					{
						this.isColliding = true;
						this.isSliding = num2 > this.gtPlayer.iceThreshold;
						this.materialTouchIndex = this.gtPlayer.currentMaterialIndex;
						this.surfaceOverride = this.gtPlayer.currentOverride;
						this.gtPlayer.lastHitInfoHand = raycastHit;
						this.lastHitInfo = raycastHit;
						this.finalPositionThisFrame = vector3;
					}
					else
					{
						this.finalPositionThisFrame = this.GetCurrentHandPosition();
					}
				}
				bool flag = (this.isLeftHand ? this.gtPlayer.controllerState.LeftValid : this.gtPlayer.controllerState.RightValid);
				this.isColliding = this.isColliding && flag;
				this.isSliding = this.isSliding && flag;
				if (this.isColliding)
				{
					this.gtPlayer.anyHandIsColliding = true;
					if (this.isSliding)
					{
						this.gtPlayer.anyHandIsSliding = true;
						return;
					}
					this.gtPlayer.anyHandIsSticking = true;
				}
			}

			public bool IsSlipOverriddenToMax()
			{
				return this.slipSetToMaxFrameIdx == Time.frameCount;
			}

			public Vector3 GetCurrentHandPosition()
			{
				Vector3 position = this.gtPlayer.headCollider.transform.position;
				if (this.gtPlayer.inOverlay)
				{
					return position + this.gtPlayer.headCollider.transform.up * -0.5f * this.gtPlayer.scale;
				}
				Vector3 vector = this.gtPlayer.PositionWithOffset(this.controllerTransform, this.handOffset);
				if ((vector - position).IsShorterThan(this.maxArmLength * this.gtPlayer.scale))
				{
					return vector;
				}
				return position + (vector - position).normalized * this.maxArmLength * this.gtPlayer.scale;
			}

			public void PositionHandFollower()
			{
				this.handFollower.position = this.finalPositionThisFrame;
				this.handFollower.rotation = this.lastRotation;
			}

			public void OnEndOfFrame()
			{
				this.wasColliding = this.isColliding;
				this.wasSliding = this.isSliding;
				this.lastPosition = this.finalPositionThisFrame;
				if (Time.time > this.tempFreezeUntilTimestamp)
				{
					this.lastRotation = this.controllerTransform.rotation * this.handRotOffset;
				}
			}

			public void TempFreezeHand(float freezeDuration)
			{
				this.tempFreezeUntilTimestamp = Math.Max(this.tempFreezeUntilTimestamp, Time.time + freezeDuration);
			}

			public void GetHandTapData(out bool wasHandTouching, out bool wasSliding, out int handMatIndex, out GorillaSurfaceOverride surfaceOverride, out RaycastHit handHitInfo, out Vector3 handPosition, out GorillaVelocityTracker handVelocityTracker)
			{
				wasHandTouching = this.wasColliding;
				wasSliding = this.wasSliding;
				handMatIndex = this.materialTouchIndex;
				surfaceOverride = this.surfaceOverride;
				handHitInfo = this.lastHitInfo;
				handPosition = this.finalPositionThisFrame;
				handVelocityTracker = this.velocityTracker;
			}

			[NonSerialized]
			public Vector3 lastPosition;

			[NonSerialized]
			public Quaternion lastRotation;

			[NonSerialized]
			public bool isLeftHand;

			[NonSerialized]
			public bool wasColliding;

			[NonSerialized]
			public bool isColliding;

			[NonSerialized]
			public bool wasSliding;

			[NonSerialized]
			public bool isSliding;

			[NonSerialized]
			public bool isHolding;

			[NonSerialized]
			public Vector3 slideNormal;

			[NonSerialized]
			public float slipPercentage;

			[NonSerialized]
			public Vector3 hitPoint;

			[NonSerialized]
			private Vector3 boostVectorThisFrame;

			[NonSerialized]
			public Vector3 finalPositionThisFrame;

			[NonSerialized]
			public int slipSetToMaxFrameIdx;

			[NonSerialized]
			public int materialTouchIndex;

			[NonSerialized]
			public GorillaSurfaceOverride surfaceOverride;

			[NonSerialized]
			public RaycastHit hitInfo;

			[NonSerialized]
			public RaycastHit lastHitInfo;

			[NonSerialized]
			private GTPlayer gtPlayer;

			[SerializeField]
			public Transform handFollower;

			[SerializeField]
			public Transform controllerTransform;

			[SerializeField]
			public GorillaVelocityTracker velocityTracker;

			[SerializeField]
			public GorillaVelocityTracker interactPointVelocityTracker;

			[SerializeField]
			public Vector3 handOffset;

			[SerializeField]
			public Quaternion handRotOffset;

			[NonSerialized]
			public float tempFreezeUntilTimestamp;

			[NonSerialized]
			public bool canTag;

			[NonSerialized]
			public bool canStun;

			private float maxArmLength;

			[NonSerialized]
			public bool isActive;

			[NonSerialized]
			public float customBoostFactor;

			[NonSerialized]
			public bool hasCustomBoost;
		}

		private enum MovingSurfaceContactPoint
		{
			NONE,
			RIGHT,
			LEFT,
			BODY
		}

		[Serializable]
		public struct MaterialData
		{
			public string matName;

			public bool overrideAudio;

			public AudioClip audio;

			public bool overrideSlidePercent;

			public float slidePercent;

			public int surfaceEffectIndex;
		}

		[Serializable]
		public struct LiquidProperties
		{
			[Range(0f, 2f)]
			[Tooltip("0: no resistance just like air, 1: full resistance like solid geometry")]
			public float resistance;

			[Range(0f, 3f)]
			[Tooltip("0: no buoyancy. 1: Fully compensates gravity. 2: net force is upwards equal to gravity")]
			public float buoyancy;

			[Range(0f, 3f)]
			[Tooltip("Damping Half-life Multiplier")]
			public float dampingFactor;

			[Range(0f, 1f)]
			public float surfaceJumpFactor;
		}

		public enum LiquidType
		{
			Water,
			Lava
		}

		private struct HoverBoardCast
		{
			public Vector3 localOrigin;

			public Vector3 localDirection;

			public float sphereRadius;

			public float distance;

			public float intersectToVelocityCap;

			public bool isSolid;

			public bool didHit;

			public Vector3 pointHit;

			public Vector3 normalHit;
		}

		private struct HandHoldState
		{
			public GorillaGrabber grabber;

			public Transform objectHeld;

			public Vector3 localPositionHeld;

			public float localRotationalOffset;

			public bool applyRotation;
		}
	}
}
