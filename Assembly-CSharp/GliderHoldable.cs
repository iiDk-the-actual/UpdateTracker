using System;
using System.Collections;
using System.Runtime.InteropServices;
using AA;
using Fusion;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
[NetworkBehaviourWeaved(11)]
public class GliderHoldable : NetworkHoldableObject, IRequestableOwnershipGuardCallbacks
{
	private bool OutOfBounds
	{
		get
		{
			return this.maxDistanceRespawnOrigin != null && (this.maxDistanceRespawnOrigin.position - base.transform.position).sqrMagnitude > this.maxDistanceBeforeRespawn * this.maxDistanceBeforeRespawn;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.transform.parent = null;
		this.defaultMaxDistanceBeforeRespawn = this.maxDistanceBeforeRespawn;
		this.spawnPosition = (this.skyJungleSpawnPostion = base.transform.position);
		this.spawnRotation = (this.skyJungleSpawnRotation = base.transform.rotation);
		this.skyJungleRespawnOrigin = this.maxDistanceRespawnOrigin;
		this.syncedState.Init(this.spawnPosition, this.spawnRotation);
		this.rb = base.GetComponent<Rigidbody>();
		this.yaw = base.transform.rotation.eulerAngles.y;
		this.oneHandRotationRateExp = Mathf.Exp(this.oneHandHoldRotationRate);
		this.twoHandRotationRateExp = Mathf.Exp(this.twoHandHoldRotationRate);
		this.subtlePlayerPitchRateExp = Mathf.Exp(this.subtlePlayerPitchRate);
		this.subtlePlayerRollRateExp = Mathf.Exp(this.subtlePlayerRollRate);
		this.accelSmoothingFollowRateExp = Mathf.Exp(this.accelSmoothingFollowRate);
		this.networkSyncFollowRateExp = Mathf.Exp(this.networkSyncFollowRate);
		this.ownershipGuard.AddCallbackTarget(this);
		this.calmAudio.volume = 0f;
		this.activeAudio.volume = 0f;
		this.whistlingAudio.volume = 0f;
	}

	private void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		if (this.ownershipGuard != null)
		{
			this.ownershipGuard.RemoveCallbackTarget(this);
		}
	}

	internal override void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
	}

	internal override void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		this.Respawn();
		base.OnDisable();
	}

	public void Respawn()
	{
		if ((base.IsValid && base.IsMine) || !NetworkSystem.Instance.InRoom)
		{
			if (EquipmentInteractor.instance != null)
			{
				if (EquipmentInteractor.instance.leftHandHeldEquipment == this)
				{
					this.OnRelease(null, EquipmentInteractor.instance.leftHand);
				}
				if (EquipmentInteractor.instance.rightHandHeldEquipment == this)
				{
					this.OnRelease(null, EquipmentInteractor.instance.rightHand);
				}
			}
			this.rb.isKinematic = true;
			base.transform.position = this.spawnPosition;
			base.transform.rotation = this.spawnRotation;
			this.lastHeldTime = -1f;
			this.syncedState.Init(this.spawnPosition, this.spawnRotation);
		}
	}

	public void CustomMapLoad(Transform placeholderTransform, float respawnDistance)
	{
		this.maxDistanceRespawnOrigin = placeholderTransform;
		this.spawnPosition = placeholderTransform.position;
		this.spawnRotation = placeholderTransform.rotation;
		this.maxDistanceBeforeRespawn = respawnDistance;
		this.Respawn();
	}

	public void CustomMapUnload()
	{
		this.maxDistanceRespawnOrigin = this.skyJungleRespawnOrigin;
		this.spawnPosition = this.skyJungleSpawnPostion;
		this.spawnRotation = this.skyJungleSpawnRotation;
		this.maxDistanceBeforeRespawn = this.defaultMaxDistanceBeforeRespawn;
		this.Respawn();
	}

	public override bool TwoHanded
	{
		get
		{
			return true;
		}
	}

	public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
	{
		if (!base.IsMine && NetworkSystem.Instance.InRoom && !this.pendingOwnershipRequest && this.syncedState.riderId == -1)
		{
			this.ownershipGuard.RequestOwnershipImmediately(delegate
			{
				this.pendingOwnershipRequest = false;
			});
			this.pendingOwnershipRequest = true;
			if (this.reenableOwnershipRequestCoroutine != null)
			{
				base.StopCoroutine(this.reenableOwnershipRequestCoroutine);
			}
			this.reenableOwnershipRequestCoroutine = base.StartCoroutine(this.ReenableOwnershipRequest());
		}
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (base.IsMine || !NetworkSystem.Instance.InRoom || this.pendingOwnershipRequest)
		{
			this.OnGrabAuthority(pointGrabbed, grabbingHand);
			return;
		}
		if (NetworkSystem.Instance.InRoom && !base.IsMine && !this.pendingOwnershipRequest && this.syncedState.riderId == -1)
		{
			this.ownershipGuard.RequestOwnershipImmediately(delegate
			{
				this.pendingOwnershipRequest = false;
			});
			this.pendingOwnershipRequest = true;
			if (this.reenableOwnershipRequestCoroutine != null)
			{
				base.StopCoroutine(this.reenableOwnershipRequestCoroutine);
			}
			this.reenableOwnershipRequestCoroutine = base.StartCoroutine(this.ReenableOwnershipRequest());
			this.OnGrabAuthority(pointGrabbed, grabbingHand);
		}
	}

	public void OnGrabAuthority(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		if (!base.IsMine && NetworkSystem.Instance.InRoom && !this.pendingOwnershipRequest)
		{
			return;
		}
		bool flag = grabbingHand == EquipmentInteractor.instance.leftHand;
		if ((flag && !EquipmentInteractor.instance.isLeftGrabbing) || (!flag && !EquipmentInteractor.instance.isRightGrabbing))
		{
			return;
		}
		if (this.riderId != NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			this.riderId = NetworkSystem.Instance.LocalPlayer.ActorNumber;
			this.cachedRig = this.getNewHolderRig(this.riderId);
		}
		EquipmentInteractor.instance.UpdateHandEquipment(this, flag);
		GorillaTagger.Instance.StartVibration(flag, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
		Vector3 vector = this.ClosestPointInHandle(grabbingHand.transform.position, pointGrabbed);
		if (flag)
		{
			this.leftHold.Activate(grabbingHand.transform, base.transform, vector);
		}
		else
		{
			this.rightHold.Activate(grabbingHand.transform, base.transform, vector);
		}
		if (this.leftHold.active && this.rightHold.active)
		{
			Vector3 handsVector = this.GetHandsVector(this.leftHold.transform.position, this.rightHold.transform.position, GTPlayer.Instance.headCollider.transform.position, true);
			this.twoHandRotationOffsetAxis = Vector3.Cross(handsVector, base.transform.right).normalized;
			if ((double)this.twoHandRotationOffsetAxis.sqrMagnitude < 0.001)
			{
				this.twoHandRotationOffsetAxis = base.transform.right;
				this.twoHandRotationOffsetAngle = 0f;
			}
			else
			{
				this.twoHandRotationOffsetAngle = Vector3.SignedAngle(handsVector, base.transform.right, this.twoHandRotationOffsetAxis);
			}
		}
		this.rb.isKinematic = true;
		this.rb.useGravity = false;
		this.ridersMaterialOverideIndex = 0;
		if (this.cosmeticMaterialOverrides.Length != 0)
		{
			VRRig offlineVRRig = this.cachedRig;
			if (offlineVRRig == null)
			{
				offlineVRRig = GorillaTagger.Instance.offlineVRRig;
			}
			if (offlineVRRig != null)
			{
				for (int i = 0; i < this.cosmeticMaterialOverrides.Length; i++)
				{
					if (this.cosmeticMaterialOverrides[i].cosmeticName != null && offlineVRRig.cosmeticSet != null && offlineVRRig.cosmeticSet.HasItem(this.cosmeticMaterialOverrides[i].cosmeticName))
					{
						this.ridersMaterialOverideIndex = i + 1;
						break;
					}
				}
			}
		}
		this.infectedState = false;
		if (GorillaGameManager.instance as GorillaTagManager != null)
		{
			this.infectedState = this.syncedState.tagged;
		}
		if (this.infectedState)
		{
			this.leafMesh.material = this.GetInfectedMaterial();
		}
		else
		{
			this.leafMesh.material = this.GetMaterialFromIndex((byte)this.ridersMaterialOverideIndex);
		}
		if (EquipmentInteractor.instance.rightHandHeldEquipment != null && EquipmentInteractor.instance.rightHandHeldEquipment.GetType() == typeof(GliderHoldable) && EquipmentInteractor.instance.leftHandHeldEquipment != null && EquipmentInteractor.instance.leftHandHeldEquipment.GetType() == typeof(GliderHoldable) && EquipmentInteractor.instance.leftHandHeldEquipment != EquipmentInteractor.instance.rightHandHeldEquipment)
		{
			this.holdingTwoGliders = true;
		}
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		this.holdingTwoGliders = false;
		bool flag = releasingHand == EquipmentInteractor.instance.leftHand;
		if (this.leftHold.active && this.rightHold.active)
		{
			if (flag)
			{
				this.rightHold.Activate(this.rightHold.transform, base.transform, this.ClosestPointInHandle(this.rightHold.transform.position, this.handle));
			}
			else
			{
				this.leftHold.Activate(this.leftHold.transform, base.transform, this.ClosestPointInHandle(this.leftHold.transform.position, this.handle));
			}
		}
		Vector3 averageVelocity = GTPlayer.Instance.GetHandVelocityTracker(flag).GetAverageVelocity(true, 0.15f, false);
		(flag ? this.leftHold : this.rightHold).Deactivate();
		EquipmentInteractor.instance.UpdateHandEquipment(null, flag);
		if (!this.leftHold.active && !this.rightHold.active)
		{
			this.gliderState = GliderHoldable.GliderState.LocallyDropped;
			this.audioLevel = 0f;
			this.riderId = -1;
			this.cachedRig = null;
			this.subtlePlayerPitch = 0f;
			this.subtlePlayerRoll = 0f;
			this.leftHoldPositionLocal = null;
			this.rightHoldPositionLocal = null;
			this.ridersMaterialOverideIndex = 0;
			if (base.IsMine || !NetworkSystem.Instance.InRoom)
			{
				this.rb.isKinematic = false;
				this.rb.useGravity = true;
				this.rb.linearVelocity = averageVelocity;
				this.syncedState.riderId = -1;
				this.syncedState.tagged = false;
				this.syncedState.materialIndex = 0;
				this.syncedState.position = base.transform.position;
				this.syncedState.rotation = base.transform.rotation;
				this.syncedState.audioLevel = 0;
			}
			this.leafMesh.material = this.baseLeafMaterial;
		}
		return true;
	}

	public override void DropItemCleanup()
	{
	}

	public void FixedUpdate()
	{
		if (!base.IsMine && NetworkSystem.Instance.InRoom && !this.pendingOwnershipRequest)
		{
			return;
		}
		GTPlayer instance = GTPlayer.Instance;
		if (this.holdingTwoGliders)
		{
			instance.AddForce(Physics.gravity, ForceMode.Acceleration);
			return;
		}
		if (this.leftHold.active || this.rightHold.active)
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			this.previousVelocity = this.currentVelocity;
			this.currentVelocity = instance.RigidbodyVelocity;
			float magnitude = this.currentVelocity.magnitude;
			this.accelerationAverage.AddSample((this.currentVelocity - this.previousVelocity) / Time.fixedDeltaTime, Time.fixedTime);
			float rollAngle180Wrapping = this.GetRollAngle180Wrapping();
			float num = this.liftIncreaseVsRoll.Evaluate(Mathf.Clamp01(Mathf.Abs(rollAngle180Wrapping / 180f))) * this.liftIncreaseVsRollMaxAngle;
			Vector3 vector = Vector3.RotateTowards(this.currentVelocity, Quaternion.AngleAxis(num, -base.transform.right) * base.transform.forward * magnitude, this.pitchVelocityFollowRateAngle * 0.017453292f * fixedDeltaTime, this.pitchVelocityFollowRateMagnitude * fixedDeltaTime);
			Vector3 vector2 = vector - this.currentVelocity;
			float num2 = this.NormalizeAngle180(Vector3.SignedAngle(Vector3.ProjectOnPlane(this.currentVelocity, base.transform.right), base.transform.forward, base.transform.right));
			if (num2 > 90f)
			{
				num2 = Mathf.Lerp(0f, 90f, Mathf.InverseLerp(180f, 90f, num2));
			}
			else if (num2 < -90f)
			{
				num2 = Mathf.Lerp(0f, -90f, Mathf.InverseLerp(-180f, -90f, num2));
			}
			float num3 = Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(-90f, 90f, num2));
			Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(-90f, 90f, this.pitch));
			float num4 = this.liftVsAttack.Evaluate(num3);
			instance.AddForce(vector2 * num4, ForceMode.VelocityChange);
			float num5 = this.dragVsAttack.Evaluate(num3);
			float num6 = ((this.syncedState.riderId != -1 && this.syncedState.materialIndex == 1) ? (this.dragVsSpeedMaxSpeed + this.infectedSpeedIncrease) : this.dragVsSpeedMaxSpeed);
			float num7 = this.dragVsSpeed.Evaluate(Mathf.Clamp01(magnitude / num6));
			float num8 = Mathf.Clamp01(num5 * this.attackDragFactor + num7 * this.dragVsSpeedDragFactor);
			instance.AddForce(-this.currentVelocity * num8, ForceMode.Acceleration);
			if (this.pitch > 0f && this.currentVelocity.y > 0f && (this.currentVelocity - this.previousVelocity).y > 0f)
			{
				float num9 = Mathf.InverseLerp(0f, this.pullUpLiftActivationVelocity, this.currentVelocity.y);
				float num10 = Mathf.InverseLerp(0f, this.pullUpLiftActivationAcceleration, (this.currentVelocity - this.previousVelocity).y / fixedDeltaTime);
				float num11 = Mathf.Min(num9, num10);
				instance.AddForce(-Physics.gravity * this.pullUpLiftBonus * num11, ForceMode.Acceleration);
			}
			if (Vector3.Dot(vector, Physics.gravity) > 0f)
			{
				instance.AddForce(-Physics.gravity * this.gravityCompensation, ForceMode.Acceleration);
				return;
			}
		}
		else
		{
			Vector3 vector3 = this.WindResistanceForceOffset(base.transform.up, Vector3.down);
			Vector3 vector4 = base.transform.position - vector3 * this.gravityUprightTorqueMultiplier;
			this.rb.AddForceAtPosition(-this.fallingGravityReduction * Physics.gravity * this.rb.mass, vector4, ForceMode.Force);
		}
	}

	public void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (base.IsMine || !NetworkSystem.Instance.InRoom || this.pendingOwnershipRequest)
		{
			this.AuthorityUpdate(deltaTime);
			return;
		}
		this.RemoteSyncUpdate(deltaTime);
	}

	private void AuthorityUpdate(float dt)
	{
		if (!this.leftHold.active && !this.rightHold.active)
		{
			this.AuthorityUpdateUnheld(dt);
		}
		else if (this.leftHold.active || this.rightHold.active)
		{
			this.AuthorityUpdateHeld(dt);
		}
		this.syncedState.audioLevel = (byte)Mathf.FloorToInt(255f * this.audioLevel);
	}

	private void AuthorityUpdateHeld(float dt)
	{
		if (this.gliderState != GliderHoldable.GliderState.LocallyHeld)
		{
			this.gliderState = GliderHoldable.GliderState.LocallyHeld;
		}
		this.rb.isKinematic = true;
		this.lastHeldTime = Time.time;
		if (this.leftHold.active)
		{
			this.leftHold.holdLocalPos = Vector3.Lerp(Vector3.zero, this.leftHold.holdLocalPos, Mathf.Exp(-5f * dt));
		}
		if (this.rightHold.active)
		{
			this.rightHold.holdLocalPos = Vector3.Lerp(Vector3.zero, this.rightHold.holdLocalPos, Mathf.Exp(-5f * dt));
		}
		Vector3 vector = Vector3.zero;
		if (this.leftHold.active && this.rightHold.active)
		{
			vector = (this.leftHold.transform.TransformPoint(this.leftHold.holdLocalPos) + this.rightHold.transform.TransformPoint(this.rightHold.holdLocalPos)) * 0.5f;
		}
		else if (this.leftHold.active)
		{
			vector = this.leftHold.transform.TransformPoint(this.leftHold.holdLocalPos);
		}
		else if (this.rightHold.active)
		{
			vector = this.rightHold.transform.TransformPoint(this.rightHold.holdLocalPos);
		}
		this.UpdateGliderPosition();
		float magnitude = this.currentVelocity.magnitude;
		if (this.setMaxHandSlipDuringFlight && magnitude > this.maxSlipOverrideSpeedThreshold)
		{
			if (this.leftHold.active)
			{
				GTPlayer.Instance.SetLeftMaximumSlipThisFrame();
			}
			if (this.rightHold.active)
			{
				GTPlayer.Instance.SetRightMaximumSlipThisFrame();
			}
		}
		bool flag = false;
		GorillaTagManager gorillaTagManager = GorillaGameManager.instance as GorillaTagManager;
		if (gorillaTagManager != null)
		{
			flag = gorillaTagManager.IsInfected(NetworkSystem.Instance.LocalPlayer);
		}
		bool flag2 = flag != this.infectedState;
		this.infectedState = flag;
		if (flag2)
		{
			if (this.infectedState)
			{
				this.leafMesh.material = this.GetInfectedMaterial();
			}
			else
			{
				this.leafMesh.material = this.GetMaterialFromIndex(this.syncedState.materialIndex);
			}
		}
		Vector3 average = this.accelerationAverage.GetAverage();
		this.accelerationSmoothed = Mathf.Lerp(average.magnitude, this.accelerationSmoothed, Mathf.Exp(-this.accelSmoothingFollowRateExp * dt));
		float num = Mathf.InverseLerp(this.hapticMaxSpeedInputRange.x, this.hapticMaxSpeedInputRange.y, magnitude);
		float num2 = Mathf.InverseLerp(this.hapticAccelInputRange.x, this.hapticAccelInputRange.y, this.accelerationSmoothed);
		float num3 = Mathf.InverseLerp(this.hapticSpeedInputRange.x, this.hapticSpeedInputRange.y, magnitude);
		this.UpdateAudioSource(this.calmAudio, num * this.audioVolumeMultiplier);
		this.UpdateAudioSource(this.activeAudio, num2 * num * this.audioVolumeMultiplier);
		if (this.infectedState)
		{
			this.UpdateAudioSource(this.whistlingAudio, Mathf.InverseLerp(this.whistlingAudioSpeedInputRange.x, this.whistlingAudioSpeedInputRange.y, magnitude) * num2 * num * this.audioVolumeMultiplier);
		}
		else
		{
			this.UpdateAudioSource(this.whistlingAudio, 0f);
		}
		float num4 = Mathf.Max(num2 * this.hapticAccelOutputMax * num, num3 * this.hapticSpeedOutputMax);
		if (this.rightHold.active)
		{
			GorillaTagger.Instance.DoVibration(XRNode.RightHand, num4, dt);
		}
		if (this.leftHold.active)
		{
			GorillaTagger.Instance.DoVibration(XRNode.LeftHand, num4, dt);
		}
		Vector3 vector2 = this.handle.transform.position + this.handle.transform.rotation * new Vector3(0f, 0f, 1f);
		if (Time.frameCount % 2 == 0)
		{
			Vector3 vector3 = this.handle.transform.rotation * new Vector3(-0.707f, 0f, 0.707f);
			RaycastHit raycastHit;
			if (this.leftWhooshStartTime < Time.time - this.whooshSoundRetriggerThreshold && magnitude > this.whooshSpeedThresholdInput.x && Physics.Raycast(new Ray(vector2, vector3), out raycastHit, this.whooshCheckDistance, GTPlayer.Instance.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore))
			{
				this.leftWhooshStartTime = Time.time;
				this.leftWhooshHitPoint = raycastHit.point;
				this.leftWhooshAudio.GTStop();
				this.leftWhooshAudio.volume = Mathf.Lerp(this.whooshVolumeOutput.x, this.whooshVolumeOutput.y, Mathf.InverseLerp(this.whooshSpeedThresholdInput.x, this.whooshSpeedThresholdInput.y, magnitude));
				this.leftWhooshAudio.GTPlay();
			}
		}
		else
		{
			Vector3 vector4 = this.handle.transform.rotation * new Vector3(0.707f, 0f, 0.707f);
			RaycastHit raycastHit2;
			if (this.rightWhooshStartTime < Time.time - this.whooshSoundRetriggerThreshold && magnitude > this.whooshSpeedThresholdInput.x && Physics.Raycast(new Ray(vector2, vector4), out raycastHit2, this.whooshCheckDistance, GTPlayer.Instance.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore))
			{
				this.rightWhooshStartTime = Time.time;
				this.rightWhooshHitPoint = raycastHit2.point;
				this.rightWhooshAudio.GTStop();
				this.rightWhooshAudio.volume = Mathf.Lerp(this.whooshVolumeOutput.x, this.whooshVolumeOutput.y, Mathf.InverseLerp(this.whooshSpeedThresholdInput.x, this.whooshSpeedThresholdInput.y, magnitude));
				this.rightWhooshAudio.GTPlay();
			}
		}
		Vector3 headCenterPosition = GTPlayer.Instance.HeadCenterPosition;
		if (this.leftWhooshStartTime > Time.time - this.whooshSoundDuration)
		{
			this.leftWhooshAudio.transform.position = this.leftWhooshHitPoint;
		}
		else
		{
			this.leftWhooshAudio.transform.localPosition = new Vector3(-this.whooshAudioPositionOffset.x, this.whooshAudioPositionOffset.y, this.whooshAudioPositionOffset.z);
		}
		if (this.rightWhooshStartTime > Time.time - this.whooshSoundDuration)
		{
			this.rightWhooshAudio.transform.position = this.rightWhooshHitPoint;
		}
		else
		{
			this.rightWhooshAudio.transform.localPosition = new Vector3(this.whooshAudioPositionOffset.x, this.whooshAudioPositionOffset.y, this.whooshAudioPositionOffset.z);
		}
		if (this.extendTagRangeInFlight)
		{
			float num5 = Mathf.Lerp(this.tagRangeOutput.x, this.tagRangeOutput.y, Mathf.InverseLerp(this.tagRangeSpeedInput.x, this.tagRangeSpeedInput.y, magnitude));
			GorillaTagger.Instance.SetTagRadiusOverrideThisFrame(num5);
			if (this.debugDrawTagRange)
			{
				GorillaTagger.Instance.DebugDrawTagCasts(Color.yellow);
			}
		}
		Vector3 normalized = Vector3.ProjectOnPlane(base.transform.right, Vector3.up).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized;
		float num6 = -Vector3.Dot(vector - this.handle.transform.position, normalized2);
		Vector3 vector5 = this.handle.transform.position - normalized2 * (this.riderPosRange.y * 0.5f + this.riderPosRangeOffset + num6);
		float num7 = Vector3.Dot(headCenterPosition - vector5, normalized);
		float num8 = Vector3.Dot(headCenterPosition - vector5, normalized2);
		num7 /= this.riderPosRange.x * 0.5f;
		num8 /= this.riderPosRange.y * 0.5f;
		this.riderPosition.x = Mathf.Sign(num7) * Mathf.Lerp(0f, 1f, Mathf.InverseLerp(this.riderPosRangeNormalizedDeadzone.x, 1f, Mathf.Abs(num7)));
		this.riderPosition.y = Mathf.Sign(num8) * Mathf.Lerp(0f, 1f, Mathf.InverseLerp(this.riderPosRangeNormalizedDeadzone.y, 1f, Mathf.Abs(num8)));
		Vector3 vector6;
		Vector3 vector7;
		if (this.leftHold.active && this.rightHold.active)
		{
			vector6 = this.leftHold.transform.position;
			this.leftHoldPositionLocal = new Vector3?(GTPlayer.Instance.transform.InverseTransformPoint(vector6));
			vector7 = this.rightHold.transform.position;
			this.rightHoldPositionLocal = new Vector3?(GTPlayer.Instance.transform.InverseTransformPoint(vector7));
		}
		else if (this.leftHold.active)
		{
			vector6 = this.leftHold.transform.position;
			this.leftHoldPositionLocal = new Vector3?(GTPlayer.Instance.transform.InverseTransformPoint(vector6));
			Vector3 vector8 = vector6 + this.leftHold.transform.forward * this.oneHandSimulatedHoldOffset.x;
			if (this.rightHoldPositionLocal != null)
			{
				this.rightHoldPositionLocal = new Vector3?(Vector3.Lerp(GTPlayer.Instance.transform.InverseTransformPoint(vector8), this.rightHoldPositionLocal.Value, Mathf.Exp(-5f * dt)));
				vector7 = GTPlayer.Instance.transform.TransformPoint(this.rightHoldPositionLocal.Value);
			}
			else
			{
				vector7 = vector8;
				this.rightHoldPositionLocal = new Vector3?(GTPlayer.Instance.transform.InverseTransformPoint(vector7));
			}
		}
		else
		{
			vector7 = this.rightHold.transform.position;
			this.rightHoldPositionLocal = new Vector3?(GTPlayer.Instance.transform.InverseTransformPoint(vector7));
			Vector3 vector9 = vector7 + this.rightHold.transform.forward * this.oneHandSimulatedHoldOffset.x;
			if (this.leftHoldPositionLocal != null)
			{
				this.leftHoldPositionLocal = new Vector3?(Vector3.Lerp(GTPlayer.Instance.transform.InverseTransformPoint(vector9), this.leftHoldPositionLocal.Value, Mathf.Exp(-5f * dt)));
				vector6 = GTPlayer.Instance.transform.TransformPoint(this.leftHoldPositionLocal.Value);
			}
			else
			{
				vector6 = vector9;
				this.leftHoldPositionLocal = new Vector3?(GTPlayer.Instance.transform.InverseTransformPoint(vector6));
			}
		}
		Vector3 vector10;
		Vector3 vector11;
		this.GetHandsOrientationVectors(vector6, vector7, GTPlayer.Instance.headCollider.transform, false, out vector10, out vector11);
		float num9 = this.riderPosition.y * this.riderPosDirectPitchMax;
		if (!this.leftHold.active || !this.rightHold.active)
		{
			num9 *= this.oneHandPitchMultiplier;
		}
		Spring.CriticalSpringDamperExact(ref this.pitch, ref this.pitchVel, num9, 0f, this.pitchHalfLife, dt);
		this.pitch = Mathf.Clamp(this.pitch, this.pitchMinMax.x, this.pitchMinMax.y);
		Quaternion quaternion = Quaternion.AngleAxis(this.pitch, Vector3.right);
		this.twoHandRotationOffsetAngle = Mathf.Lerp(0f, this.twoHandRotationOffsetAngle, Mathf.Exp(-8f * dt));
		Vector3 vector12 = (this.twoHandGliderInversionOnYawInsteadOfRoll ? vector11 : Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(this.twoHandRotationOffsetAngle, this.twoHandRotationOffsetAxis) * Quaternion.LookRotation(vector10, vector12) * Quaternion.AngleAxis(-90f, Vector3.up);
		float num10 = ((this.leftHold.active && this.rightHold.active) ? this.twoHandRotationRateExp : this.oneHandRotationRateExp);
		base.transform.rotation = Quaternion.Slerp(quaternion2 * quaternion, base.transform.rotation, Mathf.Exp(-num10 * dt));
		if (this.subtlePlayerPitchActive || this.subtlePlayerRollActive)
		{
			float num11 = Mathf.InverseLerp(this.subtlePlayerRotationSpeedRampMinMax.x, this.subtlePlayerRotationSpeedRampMinMax.y, this.currentVelocity.magnitude);
			Quaternion quaternion3 = Quaternion.identity;
			if (this.subtlePlayerRollActive)
			{
				float num12 = this.GetRollAngle180Wrapping();
				if (num12 > 90f)
				{
					num12 = Mathf.Lerp(0f, 90f, Mathf.InverseLerp(180f, 90f, num12));
				}
				else if (num12 < -90f)
				{
					num12 = Mathf.Lerp(0f, -90f, Mathf.InverseLerp(-180f, -90f, num12));
				}
				Vector3 normalized3 = new Vector3(this.currentVelocity.x, 0f, this.currentVelocity.z).normalized;
				Vector3 vector13 = new Vector3(average.x, 0f, average.z);
				float num13 = Vector3.Dot(vector13 - Vector3.Dot(vector13, normalized3) * normalized3, Vector3.Cross(normalized3, Vector3.up));
				this.turnAccelerationSmoothed = Mathf.Lerp(num13, this.turnAccelerationSmoothed, Mathf.Exp(-this.accelSmoothingFollowRateExp * dt));
				float num14 = 0f;
				if (num13 * num12 > 0f)
				{
					num14 = Mathf.InverseLerp(this.subtlePlayerRollAccelMinMax.x, this.subtlePlayerRollAccelMinMax.y, Mathf.Abs(this.turnAccelerationSmoothed));
				}
				float num15 = num12 * this.subtlePlayerRollFactor * Mathf.Min(num11, num14);
				this.subtlePlayerRoll = Mathf.Lerp(num15, this.subtlePlayerRoll, Mathf.Exp(-this.subtlePlayerRollRateExp * dt));
				quaternion3 = Quaternion.AngleAxis(this.subtlePlayerRoll, base.transform.forward);
			}
			Quaternion quaternion4 = Quaternion.identity;
			if (this.subtlePlayerPitchActive)
			{
				float num16 = this.pitch * this.subtlePlayerPitchFactor * Mathf.Min(num11, 1f);
				this.subtlePlayerPitch = Mathf.Lerp(num16, this.subtlePlayerPitch, Mathf.Exp(-this.subtlePlayerPitchRateExp * dt));
				quaternion4 = Quaternion.AngleAxis(this.subtlePlayerPitch, -base.transform.right);
			}
			GTPlayer.Instance.PlayerRotationOverride = quaternion4 * quaternion3;
		}
		this.UpdateGliderPosition();
		if (this.syncedState.riderId != NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			this.riderId = (this.syncedState.riderId = NetworkSystem.Instance.LocalPlayer.ActorNumber);
			this.cachedRig = this.getNewHolderRig(this.riderId);
		}
		this.syncedState.tagged = this.infectedState;
		this.syncedState.materialIndex = (byte)this.ridersMaterialOverideIndex;
		if (this.cachedRig != null)
		{
			this.syncedState.position = this.cachedRig.transform.InverseTransformPoint(base.transform.position);
			this.syncedState.rotation = Quaternion.Inverse(this.cachedRig.transform.rotation) * base.transform.rotation;
		}
		else
		{
			Debug.LogError("Glider failed to get a reference to the local player's VRRig while the player was flying", this);
		}
		this.audioLevel = num2 * num;
		if (this.OutOfBounds)
		{
			this.Respawn();
		}
		if (this.leftHold.active && EquipmentInteractor.instance.leftHandHeldEquipment != this)
		{
			this.OnRelease(null, EquipmentInteractor.instance.leftHand);
		}
		if (this.rightHold.active && EquipmentInteractor.instance.rightHandHeldEquipment != this)
		{
			this.OnRelease(null, EquipmentInteractor.instance.rightHand);
		}
	}

	private void AuthorityUpdateUnheld(float dt)
	{
		this.syncedState.position = base.transform.position;
		this.syncedState.rotation = base.transform.rotation;
		if (this.gliderState != GliderHoldable.GliderState.LocallyDropped)
		{
			this.gliderState = GliderHoldable.GliderState.LocallyDropped;
			this.syncedState.riderId = -1;
			this.syncedState.materialIndex = 0;
			this.syncedState.tagged = false;
			this.leafMesh.material = this.baseLeafMaterial;
		}
		if (this.audioLevel * this.audioVolumeMultiplier > 0.001f)
		{
			this.audioLevel = Mathf.Lerp(0f, this.audioLevel, Mathf.Exp(-2f * dt));
			this.UpdateAudioSource(this.calmAudio, this.audioLevel * this.audioVolumeMultiplier);
			this.UpdateAudioSource(this.activeAudio, this.audioLevel * this.audioVolumeMultiplier);
			this.UpdateAudioSource(this.whistlingAudio, this.audioLevel * this.audioVolumeMultiplier);
		}
		if (this.OutOfBounds || (this.lastHeldTime > 0f && this.lastHeldTime < Time.time - this.maxDroppedTimeToRespawn))
		{
			this.Respawn();
		}
	}

	private void RemoteSyncUpdate(float dt)
	{
		this.rb.isKinematic = true;
		int num = this.syncedState.riderId;
		bool flag = this.riderId != num;
		if (flag)
		{
			this.riderId = num;
			this.cachedRig = this.getNewHolderRig(this.riderId);
		}
		if (this.riderId == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			this.cachedRig = null;
			this.syncedState.riderId = -1;
			this.syncedState.materialIndex = 0;
			this.syncedState.audioLevel = 0;
		}
		if (this.syncedState.riderId == -1)
		{
			base.transform.position = Vector3.Lerp(this.syncedState.position, base.transform.position, Mathf.Exp(-this.networkSyncFollowRateExp * dt));
			base.transform.rotation = Quaternion.Slerp(this.syncedState.rotation, base.transform.rotation, Mathf.Exp(-this.networkSyncFollowRateExp * dt));
		}
		else if (this.cachedRig != null)
		{
			this.positionLocalToVRRig = Vector3.Lerp(this.syncedState.position, this.positionLocalToVRRig, Mathf.Exp(-this.networkSyncFollowRateExp * dt));
			this.rotationLocalToVRRig = Quaternion.Slerp(this.syncedState.rotation, this.rotationLocalToVRRig, Mathf.Exp(-this.networkSyncFollowRateExp * dt));
			base.transform.position = this.cachedRig.transform.TransformPoint(this.positionLocalToVRRig);
			base.transform.rotation = this.cachedRig.transform.rotation * this.rotationLocalToVRRig;
		}
		bool flag2 = false;
		if (GorillaGameManager.instance as GorillaTagManager != null)
		{
			flag2 = this.syncedState.tagged;
		}
		bool flag3 = flag2 != this.infectedState;
		this.infectedState = flag2;
		if (flag3 || flag)
		{
			if (this.infectedState)
			{
				this.leafMesh.material = this.GetInfectedMaterial();
			}
			else
			{
				this.leafMesh.material = this.GetMaterialFromIndex(this.syncedState.materialIndex);
			}
		}
		float num2 = Mathf.Clamp01((float)this.syncedState.audioLevel / 255f);
		if (this.audioLevel != num2)
		{
			this.audioLevel = num2;
			if (this.syncedState.riderId != -1 && this.syncedState.tagged)
			{
				this.UpdateAudioSource(this.calmAudio, this.audioLevel * this.infectedAudioVolumeMultiplier);
				this.UpdateAudioSource(this.activeAudio, this.audioLevel * this.infectedAudioVolumeMultiplier);
				this.UpdateAudioSource(this.whistlingAudio, this.audioLevel * this.infectedAudioVolumeMultiplier);
				return;
			}
			this.UpdateAudioSource(this.calmAudio, this.audioLevel * this.audioVolumeMultiplier);
			this.UpdateAudioSource(this.activeAudio, this.audioLevel * this.audioVolumeMultiplier);
			this.UpdateAudioSource(this.whistlingAudio, 0f);
		}
	}

	private VRRig getNewHolderRig(int riderId)
	{
		if (riderId >= 0)
		{
			NetPlayer netPlayer;
			if (riderId == NetworkSystem.Instance.LocalPlayer.ActorNumber)
			{
				netPlayer = NetworkSystem.Instance.LocalPlayer;
			}
			else
			{
				netPlayer = NetworkSystem.Instance.GetPlayer(riderId);
			}
			RigContainer rigContainer;
			if (netPlayer != null && VRRigCache.Instance.TryGetVrrig(netPlayer, out rigContainer))
			{
				return rigContainer.Rig;
			}
		}
		return null;
	}

	private Vector3 ClosestPointInHandle(Vector3 startingPoint, InteractionPoint interactionPoint)
	{
		CapsuleCollider component = interactionPoint.GetComponent<CapsuleCollider>();
		Vector3 vector = startingPoint;
		if (component != null)
		{
			Vector3 vector2 = ((component.direction == 0) ? Vector3.right : ((component.direction == 1) ? Vector3.up : Vector3.forward));
			Vector3 vector3 = component.transform.rotation * vector2;
			Vector3 vector4 = component.transform.position + component.transform.rotation * component.center;
			float num = Mathf.Clamp(Vector3.Dot(vector - vector4, vector3), -component.height * 0.5f, component.height * 0.5f);
			vector = vector4 + vector3 * num;
		}
		return vector;
	}

	private void UpdateGliderPosition()
	{
		if (this.leftHold.active && this.rightHold.active)
		{
			Vector3 vector = this.leftHold.transform.TransformPoint(this.leftHold.holdLocalPos) + base.transform.TransformVector(this.leftHold.handleLocalPos);
			Vector3 vector2 = this.rightHold.transform.TransformPoint(this.rightHold.holdLocalPos) + base.transform.TransformVector(this.rightHold.handleLocalPos);
			base.transform.position = (vector + vector2) * 0.5f;
			return;
		}
		if (this.leftHold.active)
		{
			base.transform.position = this.leftHold.transform.TransformPoint(this.leftHold.holdLocalPos) + base.transform.TransformVector(this.leftHold.handleLocalPos);
			return;
		}
		if (this.rightHold.active)
		{
			base.transform.position = this.rightHold.transform.TransformPoint(this.rightHold.holdLocalPos) + base.transform.TransformVector(this.rightHold.handleLocalPos);
		}
	}

	private Vector3 GetHandsVector(Vector3 leftHandPos, Vector3 rightHandPos, Vector3 headPos, bool flipBasedOnFacingDir)
	{
		Vector3 vector = rightHandPos - leftHandPos;
		Vector3 vector2 = (rightHandPos + leftHandPos) * 0.5f - headPos;
		Vector3 normalized = Vector3.Cross(Vector3.up, vector2).normalized;
		if (flipBasedOnFacingDir && Vector3.Dot(vector, normalized) < 0f)
		{
			vector = -vector;
		}
		return vector;
	}

	private void GetHandsOrientationVectors(Vector3 leftHandPos, Vector3 rightHandPos, Transform head, bool flipBasedOnFacingDir, out Vector3 handsVector, out Vector3 handsUpVector)
	{
		handsVector = rightHandPos - leftHandPos;
		float magnitude = handsVector.magnitude;
		handsVector /= Mathf.Max(magnitude, 0.001f);
		Vector3 position = head.position;
		float num = 1f;
		Vector3 vector = ((Vector3.Dot(head.right, handsVector) < 0f) ? handsVector : (-handsVector));
		Vector3 normalized = Vector3.ProjectOnPlane(-head.forward, vector).normalized;
		Vector3 vector2 = normalized * num + position;
		Vector3 vector3 = (leftHandPos + rightHandPos) * 0.5f;
		Vector3 vector4 = Vector3.ProjectOnPlane(vector3 - head.position, Vector3.up);
		float magnitude2 = vector4.magnitude;
		vector4 /= Mathf.Max(magnitude2, 0.001f);
		Vector3 normalized2 = Vector3.ProjectOnPlane(-base.transform.forward, Vector3.up).normalized;
		Vector3 vector5 = -vector4 * num + position;
		float num2 = Vector3.Dot(normalized2, -vector4);
		float num3 = Vector3.Dot(normalized2, normalized);
		if (Vector3.Dot(base.transform.up, Vector3.up) < 0f)
		{
			num2 = Mathf.Abs(num2);
			num3 = Mathf.Abs(num3);
		}
		num2 = Mathf.Max(num2, 0f);
		num3 = Mathf.Max(num3, 0f);
		Vector3 vector6 = (vector5 * num2 + vector2 * num3) / Mathf.Max(num2 + num3, 0.001f);
		Vector3 vector7 = vector3 - vector6;
		Vector3 normalized3 = Vector3.Cross(Vector3.up, vector7).normalized;
		if (flipBasedOnFacingDir && Vector3.Dot(handsVector, normalized3) < 0f)
		{
			handsVector = -handsVector;
		}
		handsUpVector = Vector3.Cross(Vector3.ProjectOnPlane(vector7, Vector3.up), handsVector).normalized;
	}

	private Material GetMaterialFromIndex(byte materialIndex)
	{
		if (materialIndex < 1 || (int)materialIndex > this.cosmeticMaterialOverrides.Length)
		{
			return this.baseLeafMaterial;
		}
		return this.cosmeticMaterialOverrides[(int)(materialIndex - 1)].material;
	}

	private float GetRollAngle180Wrapping()
	{
		Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized;
		float num = Vector3.SignedAngle(Vector3.Cross(Vector3.up, normalized).normalized, base.transform.right, base.transform.forward);
		return this.NormalizeAngle180(num);
	}

	private float SignedAngleInPlane(Vector3 from, Vector3 to, Vector3 normal)
	{
		from = Vector3.ProjectOnPlane(from, normal);
		to = Vector3.ProjectOnPlane(to, normal);
		return Vector3.SignedAngle(from, to, normal);
	}

	private float NormalizeAngle180(float angle)
	{
		angle = (angle + 180f) % 360f;
		if (angle < 0f)
		{
			angle += 360f;
		}
		return angle - 180f;
	}

	private void UpdateAudioSource(AudioSource source, float level)
	{
		source.volume = level;
		if (!source.isPlaying && level > 0.01f)
		{
			source.GTPlay();
			return;
		}
		if (source.isPlaying && level < 0.01f && this.syncedState.riderId == -1)
		{
			source.GTStop();
		}
	}

	private Material GetInfectedMaterial()
	{
		if (GorillaGameManager.instance is GorillaFreezeTagManager)
		{
			return this.frozenLeafMaterial;
		}
		return this.infectedLeafMaterial;
	}

	public void OnTriggerStay(Collider other)
	{
		GliderWindVolume component = other.GetComponent<GliderWindVolume>();
		if (component == null)
		{
			return;
		}
		if (!base.IsMine && NetworkSystem.Instance.InRoom && !this.pendingOwnershipRequest)
		{
			return;
		}
		if (Time.frameCount == this.windVolumeForceAppliedFrame)
		{
			return;
		}
		if (this.leftHold.active || this.rightHold.active)
		{
			Vector3 accelFromVelocity = component.GetAccelFromVelocity(GTPlayer.Instance.RigidbodyVelocity);
			GTPlayer.Instance.AddForce(accelFromVelocity, ForceMode.Acceleration);
			this.windVolumeForceAppliedFrame = Time.frameCount;
			return;
		}
		Vector3 accelFromVelocity2 = component.GetAccelFromVelocity(this.rb.linearVelocity);
		Vector3 vector = this.WindResistanceForceOffset(base.transform.up, component.WindDirection);
		Vector3 vector2 = base.transform.position + vector * this.windUprightTorqueMultiplier;
		this.rb.AddForceAtPosition(accelFromVelocity2 * this.rb.mass, vector2, ForceMode.Force);
		this.windVolumeForceAppliedFrame = Time.frameCount;
	}

	private Vector3 WindResistanceForceOffset(Vector3 upDir, Vector3 windDir)
	{
		if (Vector3.Dot(upDir, windDir) < 0f)
		{
			upDir *= -1f;
		}
		return Vector3.ProjectOnPlane(upDir - windDir, upDir);
	}

	[Networked]
	[NetworkedWeaved(0, 11)]
	internal unsafe GliderHoldable.SyncedState Data
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GliderHoldable.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(GliderHoldable.SyncedState*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GliderHoldable.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(GliderHoldable.SyncedState*)(this.Ptr + 0) = value;
		}
	}

	public override void ReadDataFusion()
	{
		int num = this.syncedState.riderId;
		this.syncedState = this.Data;
		if (num != this.syncedState.riderId)
		{
			this.positionLocalToVRRig = this.syncedState.position;
			this.rotationLocalToVRRig = this.syncedState.rotation;
		}
	}

	public override void WriteDataFusion()
	{
		this.Data = this.syncedState;
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		Player sender = info.Sender;
		PunNetPlayer punNetPlayer = (PunNetPlayer)this.ownershipGuard.actualOwner;
		if (sender != ((punNetPlayer != null) ? punNetPlayer.PlayerRef : null))
		{
			return;
		}
		int num = this.syncedState.riderId;
		this.syncedState.riderId = (int)stream.ReceiveNext();
		this.syncedState.tagged = (bool)stream.ReceiveNext();
		this.syncedState.materialIndex = (byte)stream.ReceiveNext();
		this.syncedState.audioLevel = (byte)stream.ReceiveNext();
		Vector3 vector = (Vector3)stream.ReceiveNext();
		(ref this.syncedState.position).SetValueSafe(in vector);
		Quaternion quaternion = (Quaternion)stream.ReceiveNext();
		(ref this.syncedState.rotation).SetValueSafe(in quaternion);
		if (num != this.syncedState.riderId)
		{
			this.positionLocalToVRRig = this.syncedState.position;
			this.rotationLocalToVRRig = this.syncedState.rotation;
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		object sender = info.Sender;
		NetPlayer actualOwner = this.ownershipGuard.actualOwner;
		if (!sender.Equals((actualOwner != null) ? actualOwner.GetPlayerRef() : null))
		{
			return;
		}
		stream.SendNext(this.syncedState.riderId);
		stream.SendNext(this.syncedState.tagged);
		stream.SendNext(this.syncedState.materialIndex);
		stream.SendNext(this.syncedState.audioLevel);
		stream.SendNext(this.syncedState.position);
		stream.SendNext(this.syncedState.rotation);
	}

	private IEnumerator ReenableOwnershipRequest()
	{
		yield return new WaitForSeconds(3f);
		this.pendingOwnershipRequest = false;
		yield break;
	}

	public void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer)
	{
		if (toPlayer == NetworkSystem.Instance.LocalPlayer)
		{
			this.pendingOwnershipRequest = false;
			if (!this.leftHold.active && !this.rightHold.active && (this.spawnPosition - base.transform.position).sqrMagnitude > 1f)
			{
				this.rb.isKinematic = false;
				this.rb.WakeUp();
				this.lastHeldTime = Time.time;
			}
		}
	}

	public bool OnOwnershipRequest(NetPlayer fromPlayer)
	{
		return !base.IsMine || !NetworkSystem.Instance.InRoom || (!this.leftHold.active && !this.rightHold.active);
	}

	public void OnMyOwnerLeft()
	{
	}

	public bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer)
	{
		return false;
	}

	public void OnMyCreatorLeft()
	{
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.Data = this._Data;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._Data = this.Data;
	}

	[Header("Flight Settings")]
	[SerializeField]
	private Vector2 pitchMinMax = new Vector2(-80f, 80f);

	[SerializeField]
	private Vector2 rollMinMax = new Vector2(-70f, 70f);

	[SerializeField]
	private float pitchHalfLife = 0.2f;

	public Vector2 pitchVelocityTargetMinMax = new Vector2(-60f, 60f);

	public Vector2 pitchVelocityRampTimeMinMax = new Vector2(-1f, 1f);

	[SerializeField]
	private float pitchVelocityFollowRateAngle = 60f;

	[SerializeField]
	private float pitchVelocityFollowRateMagnitude = 5f;

	[SerializeField]
	private AnimationCurve liftVsAttack = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve dragVsAttack = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	public float attackDragFactor = 0.1f;

	[SerializeField]
	private AnimationCurve dragVsSpeed = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	public float dragVsSpeedMaxSpeed = 30f;

	[SerializeField]
	[Range(0f, 1f)]
	public float dragVsSpeedDragFactor = 0.2f;

	[SerializeField]
	private AnimationCurve liftIncreaseVsRoll = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float liftIncreaseVsRollMaxAngle = 20f;

	[SerializeField]
	[Range(0f, 1f)]
	private float gravityCompensation = 0.8f;

	[Range(0f, 1f)]
	public float pullUpLiftBonus = 0.1f;

	public float pullUpLiftActivationVelocity = 1f;

	public float pullUpLiftActivationAcceleration = 3f;

	[Header("Body Positioning Control")]
	[SerializeField]
	private float riderPosDirectPitchMax = 70f;

	[SerializeField]
	private Vector2 riderPosRange = new Vector2(2.2f, 0.75f);

	[SerializeField]
	private float riderPosRangeOffset = 0.15f;

	[SerializeField]
	private Vector2 riderPosRangeNormalizedDeadzone = new Vector2(0.15f, 0.05f);

	[Header("Direct Handle Control")]
	[SerializeField]
	private float oneHandHoldRotationRate = 2f;

	private Vector3 oneHandSimulatedHoldOffset = new Vector3(0.5f, -0.35f, 0.25f);

	private float oneHandPitchMultiplier = 0.8f;

	[SerializeField]
	private float twoHandHoldRotationRate = 4f;

	[SerializeField]
	private bool twoHandGliderInversionOnYawInsteadOfRoll;

	[Header("Player Settings")]
	[SerializeField]
	private bool setMaxHandSlipDuringFlight = true;

	[SerializeField]
	private float maxSlipOverrideSpeedThreshold = 5f;

	[Header("Player Camera Rotation")]
	[SerializeField]
	private float subtlePlayerPitchFactor = 0.2f;

	[SerializeField]
	private float subtlePlayerPitchRate = 2f;

	[SerializeField]
	private float subtlePlayerRollFactor = 0.2f;

	[SerializeField]
	private float subtlePlayerRollRate = 2f;

	[SerializeField]
	private Vector2 subtlePlayerRotationSpeedRampMinMax = new Vector2(2f, 8f);

	[SerializeField]
	private Vector2 subtlePlayerRollAccelMinMax = new Vector2(0f, 30f);

	[SerializeField]
	private Vector2 subtlePlayerPitchAccelMinMax = new Vector2(0f, 10f);

	[SerializeField]
	private float accelSmoothingFollowRate = 2f;

	[Header("Haptics")]
	[SerializeField]
	private Vector2 hapticAccelInputRange = new Vector2(5f, 20f);

	[SerializeField]
	private float hapticAccelOutputMax = 0.35f;

	[SerializeField]
	private Vector2 hapticMaxSpeedInputRange = new Vector2(5f, 10f);

	[SerializeField]
	private Vector2 hapticSpeedInputRange = new Vector2(3f, 30f);

	[SerializeField]
	private float hapticSpeedOutputMax = 0.15f;

	[SerializeField]
	private Vector2 whistlingAudioSpeedInputRange = new Vector2(15f, 30f);

	[Header("Audio")]
	[SerializeField]
	private float audioVolumeMultiplier = 0.25f;

	[SerializeField]
	private float infectedAudioVolumeMultiplier = 0.5f;

	[SerializeField]
	private Vector2 whooshSpeedThresholdInput = new Vector2(10f, 25f);

	[SerializeField]
	private Vector2 whooshVolumeOutput = new Vector2(0.2f, 0.75f);

	[SerializeField]
	private float whooshCheckDistance = 2f;

	[Header("Tag Adjustment")]
	[SerializeField]
	private bool extendTagRangeInFlight = true;

	[SerializeField]
	private Vector2 tagRangeSpeedInput = new Vector2(5f, 20f);

	[SerializeField]
	private Vector2 tagRangeOutput = new Vector2(0.03f, 3f);

	[SerializeField]
	private bool debugDrawTagRange = true;

	[Header("Infected State")]
	[SerializeField]
	private float infectedSpeedIncrease = 5f;

	[Header("Glider Materials")]
	[SerializeField]
	private MeshRenderer leafMesh;

	[SerializeField]
	private Material baseLeafMaterial;

	[SerializeField]
	private Material infectedLeafMaterial;

	[SerializeField]
	private Material frozenLeafMaterial;

	[SerializeField]
	private GliderHoldable.CosmeticMaterialOverride[] cosmeticMaterialOverrides;

	[Header("Network Syncing")]
	[SerializeField]
	private float networkSyncFollowRate = 2f;

	[Header("Life Cycle")]
	[SerializeField]
	private Transform maxDistanceRespawnOrigin;

	[SerializeField]
	private float maxDistanceBeforeRespawn = 180f;

	[SerializeField]
	private float maxDroppedTimeToRespawn = 120f;

	[Header("Rigidbody")]
	[SerializeField]
	private float windUprightTorqueMultiplier = 1f;

	[SerializeField]
	private float gravityUprightTorqueMultiplier = 0.5f;

	[SerializeField]
	private float fallingGravityReduction = 0.1f;

	[Header("References")]
	[SerializeField]
	private AudioSource calmAudio;

	[SerializeField]
	private AudioSource activeAudio;

	[SerializeField]
	private AudioSource whistlingAudio;

	[SerializeField]
	private AudioSource leftWhooshAudio;

	[SerializeField]
	private AudioSource rightWhooshAudio;

	[SerializeField]
	private InteractionPoint handle;

	[SerializeField]
	private RequestableOwnershipGuard ownershipGuard;

	private bool subtlePlayerPitchActive = true;

	private bool subtlePlayerRollActive = true;

	private float subtlePlayerPitch;

	private float subtlePlayerRoll;

	private float subtlePlayerPitchRateExp = 0.75f;

	private float subtlePlayerRollRateExp = 0.025f;

	private float defaultMaxDistanceBeforeRespawn = 180f;

	private GliderHoldable.HoldingHand leftHold = new GliderHoldable.HoldingHand();

	private GliderHoldable.HoldingHand rightHold = new GliderHoldable.HoldingHand();

	private GliderHoldable.SyncedState syncedState;

	private Vector3 twoHandRotationOffsetAxis = Vector3.forward;

	private float twoHandRotationOffsetAngle;

	private Rigidbody rb;

	private Vector2 riderPosition = Vector2.zero;

	private Vector3 previousVelocity;

	private Vector3 currentVelocity;

	private float pitch;

	private float yaw;

	private float roll;

	private float pitchVel;

	private float yawVel;

	private float rollVel;

	private float oneHandRotationRateExp;

	private float twoHandRotationRateExp;

	private Quaternion playerFacingRotationOffset = Quaternion.identity;

	private const float accelAveragingWindow = 0.1f;

	private AverageVector3 accelerationAverage = new AverageVector3(0.1f);

	private float accelerationSmoothed;

	private float turnAccelerationSmoothed;

	private float accelSmoothingFollowRateExp = 1f;

	private float networkSyncFollowRateExp = 2f;

	private bool pendingOwnershipRequest;

	private Vector3 positionLocalToVRRig = Vector3.zero;

	private Quaternion rotationLocalToVRRig = Quaternion.identity;

	private Coroutine reenableOwnershipRequestCoroutine;

	private Vector3 spawnPosition;

	private Quaternion spawnRotation;

	private Vector3 skyJungleSpawnPostion;

	private Quaternion skyJungleSpawnRotation;

	private Transform skyJungleRespawnOrigin;

	private float lastHeldTime = -1f;

	private Vector3? leftHoldPositionLocal;

	private Vector3? rightHoldPositionLocal;

	private float whooshSoundDuration = 1f;

	private float whooshSoundRetriggerThreshold = 0.5f;

	private float leftWhooshStartTime = -1f;

	private Vector3 leftWhooshHitPoint = Vector3.zero;

	private Vector3 whooshAudioPositionOffset = new Vector3(0.5f, -0.25f, 0.5f);

	private float rightWhooshStartTime = -1f;

	private Vector3 rightWhooshHitPoint = Vector3.zero;

	private int ridersMaterialOverideIndex;

	private int windVolumeForceAppliedFrame = -1;

	private bool holdingTwoGliders;

	private GliderHoldable.GliderState gliderState;

	private float audioLevel;

	private int riderId = -1;

	[SerializeField]
	private VRRig cachedRig;

	private bool infectedState;

	[WeaverGenerated]
	[DefaultForProperty("Data", 0, 11)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private GliderHoldable.SyncedState _Data;

	private enum GliderState
	{
		LocallyHeld,
		LocallyDropped,
		RemoteSyncing
	}

	private class HoldingHand
	{
		public void Activate(Transform handTransform, Transform gliderTransform, Vector3 worldGrabPoint)
		{
			this.active = true;
			this.transform = handTransform.transform;
			this.holdLocalPos = handTransform.InverseTransformPoint(worldGrabPoint);
			this.handleLocalPos = gliderTransform.InverseTransformVector(gliderTransform.position - worldGrabPoint);
			this.localHoldRotation = Quaternion.Inverse(handTransform.rotation) * gliderTransform.rotation;
		}

		public void Deactivate()
		{
			this.active = false;
			this.transform = null;
			this.holdLocalPos = Vector3.zero;
			this.handleLocalPos = Vector3.zero;
			this.localHoldRotation = Quaternion.identity;
		}

		public bool active;

		public Transform transform;

		public Vector3 holdLocalPos;

		public Vector3 handleLocalPos;

		public Quaternion localHoldRotation;
	}

	[NetworkStructWeaved(11)]
	[StructLayout(LayoutKind.Explicit, Size = 44)]
	internal struct SyncedState : INetworkStruct
	{
		public void Init(Vector3 defaultPosition, Quaternion defaultRotation)
		{
			this.riderId = -1;
			this.materialIndex = 0;
			this.audioLevel = 0;
			this.position = defaultPosition;
			this.rotation = defaultRotation;
		}

		public SyncedState(int id = -1)
		{
			this.riderId = id;
			this.materialIndex = 0;
			this.audioLevel = 0;
			this.tagged = default(NetworkBool);
			this.position = default(Vector3);
			this.rotation = default(Quaternion);
		}

		[FieldOffset(0)]
		public int riderId;

		[FieldOffset(4)]
		public byte materialIndex;

		[FieldOffset(8)]
		public byte audioLevel;

		[FieldOffset(12)]
		public NetworkBool tagged;

		[FieldOffset(16)]
		public Vector3 position;

		[FieldOffset(28)]
		public Quaternion rotation;
	}

	[Serializable]
	private struct CosmeticMaterialOverride
	{
		public string cosmeticName;

		public Material material;
	}
}
