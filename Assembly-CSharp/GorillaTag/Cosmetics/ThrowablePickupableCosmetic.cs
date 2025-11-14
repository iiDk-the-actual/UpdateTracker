using System;
using GorillaExtensions;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class ThrowablePickupableCosmetic : TransferrableObject
	{
		private new void Awake()
		{
			this.transferrableObject = base.GetComponent<TransferrableObject>();
		}

		internal override void OnEnable()
		{
			base.OnEnable();
			if (this._events == null)
			{
				this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
				this.owner = ((this.transferrableObject.myOnlineRig != null) ? this.transferrableObject.myOnlineRig.creator : ((this.transferrableObject.myRig != null) ? (this.transferrableObject.myRig.creator ?? NetworkSystem.Instance.LocalPlayer) : null));
				if (this.owner != null)
				{
					this._events.Init(this.owner);
					this.isLocal = this.owner.IsLocal;
				}
			}
			if (this._events != null)
			{
				this._events.Activate.reliable = true;
				this._events.Deactivate.reliable = true;
				this._events.Activate += this.OnReleaseEvent;
				this._events.Deactivate += this.OnReturnToDockEvent;
			}
		}

		internal override void OnDisable()
		{
			base.OnDisable();
			if (this._events != null)
			{
				this._events.Activate -= this.OnReleaseEvent;
				this._events.Deactivate -= this.OnReturnToDockEvent;
				this._events.Dispose();
				this._events = null;
			}
			if (this.pickupableVariant != null && this.pickupableVariant.enabled)
			{
				this.pickupableVariant.DelayedPickup();
			}
		}

		public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
		{
			if (VRRigCache.Instance.localRig.Rig != this.ownerRig)
			{
				return;
			}
			if (this.pickupableVariant.enabled)
			{
				if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
				{
					this._events.Activate.RaiseOthers(new object[] { false });
				}
				base.transform.position = this.pickupableVariant.transform.position;
				base.transform.rotation = this.pickupableVariant.transform.rotation;
				this.pickupableVariant.Pickup(false);
				if (grabbingHand == EquipmentInteractor.instance.leftHand && this.currentState == TransferrableObject.PositionState.OnLeftArm)
				{
					this.canAutoGrabLeft = false;
					this.interpState = TransferrableObject.InterpolateState.None;
					this.currentState = TransferrableObject.PositionState.InRightHand;
				}
				else if (grabbingHand == EquipmentInteractor.instance.rightHand && this.currentState == TransferrableObject.PositionState.OnRightArm)
				{
					this.canAutoGrabRight = false;
					this.interpState = TransferrableObject.InterpolateState.None;
					this.currentState = TransferrableObject.PositionState.InLeftHand;
				}
			}
			UnityEvent onGrabLocal = this.OnGrabLocal;
			if (onGrabLocal != null)
			{
				onGrabLocal.Invoke();
			}
			base.OnGrab(pointGrabbed, grabbingHand);
		}

		public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
		{
			if (!base.OnRelease(zoneReleased, releasingHand))
			{
				return false;
			}
			if (!(VRRigCache.Instance.localRig.Rig == this.ownerRig))
			{
				return false;
			}
			Vector3 position = base.transform.position;
			bool flag = releasingHand == EquipmentInteractor.instance.leftHand;
			Vector3 averageVelocity = GTPlayer.Instance.GetInteractPointVelocityTracker(flag).GetAverageVelocity(true, 0.15f, false);
			float scale = GTPlayer.Instance.scale;
			bool flag2 = this.DistanceToDock() > this.returnToDockDistanceThreshold;
			if (PhotonNetwork.InRoom && this._events != null)
			{
				if (flag2 && this._events.Activate != null)
				{
					this._events.Activate.RaiseAll(new object[] { true, position, averageVelocity, scale });
				}
				else if (!flag2 && this._events.Deactivate != null)
				{
					this._events.Deactivate.RaiseAll(Array.Empty<object>());
					UnityEvent onReturnToDockPositionLocal = this.OnReturnToDockPositionLocal;
					if (onReturnToDockPositionLocal != null)
					{
						onReturnToDockPositionLocal.Invoke();
					}
				}
			}
			else if (flag2)
			{
				this.OnReleaseEventLocal(position, averageVelocity, scale);
			}
			else
			{
				UnityEvent onReturnToDockPositionLocal2 = this.OnReturnToDockPositionLocal;
				if (onReturnToDockPositionLocal2 != null)
				{
					onReturnToDockPositionLocal2.Invoke();
				}
				UnityEvent onReturnToDockPositionShared = this.OnReturnToDockPositionShared;
				if (onReturnToDockPositionShared != null)
				{
					onReturnToDockPositionShared.Invoke();
				}
			}
			return true;
		}

		private void OnReleaseEvent(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			if (info.senderID != this.ownerRig.creator.ActorNumber)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "OnReleaseEvent");
			if (!this.callLimiterRelease.CheckCallTime(Time.time))
			{
				return;
			}
			object obj = args[0];
			if (obj is bool)
			{
				bool flag = (bool)obj;
				if (flag)
				{
					obj = args[1];
					if (obj is Vector3)
					{
						Vector3 vector = (Vector3)obj;
						obj = args[2];
						if (obj is Vector3)
						{
							Vector3 vector2 = (Vector3)obj;
							obj = args[3];
							if (obj is float)
							{
								float num = (float)obj;
								Vector3 position = base.transform.position;
								Vector3 vector3 = base.transform.forward;
								(ref position).SetValueSafe(in vector);
								if (this.ownerRig.IsPositionInRange(position, 20f))
								{
									vector3 = this.ownerRig.ClampVelocityRelativeToPlayerSafe(vector2, 50f, 100f);
									float num2 = num.ClampSafe(0.01f, 1f);
									this.OnReleaseEventLocal(position, vector3, num2);
									return;
								}
								return;
							}
						}
					}
					return;
				}
				this.pickupableVariant.Pickup(false);
				return;
			}
		}

		private void OnReturnToDockEvent(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			if (info.senderID != this.ownerRig.creator.ActorNumber)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "OnReturnToDockEvent");
			if (!this.callLimiterReturn.CheckCallTime(Time.time))
			{
				return;
			}
			UnityEvent onReturnToDockPositionShared = this.OnReturnToDockPositionShared;
			if (onReturnToDockPositionShared == null)
			{
				return;
			}
			onReturnToDockPositionShared.Invoke();
		}

		private void OnReleaseEventLocal(Vector3 startPosition, Vector3 releaseVelocity, float playerScale)
		{
			this.pickupableVariant.Release(this, startPosition, releaseVelocity, playerScale);
		}

		private float DistanceToDock()
		{
			float num = 0f;
			if (this.currentState == TransferrableObject.PositionState.OnRightShoulder)
			{
				num = Vector3.Distance(this.ownerRig.myBodyDockPositions.rightBackTransform.position, base.transform.position);
			}
			else if (this.currentState == TransferrableObject.PositionState.OnLeftShoulder)
			{
				num = Vector3.Distance(this.ownerRig.myBodyDockPositions.leftBackTransform.position, base.transform.position);
			}
			else if (this.currentState == TransferrableObject.PositionState.OnRightArm)
			{
				num = Vector3.Distance(this.ownerRig.myBodyDockPositions.rightArmTransform.position, base.transform.position);
			}
			else if (this.currentState == TransferrableObject.PositionState.OnLeftArm)
			{
				num = Vector3.Distance(this.ownerRig.myBodyDockPositions.leftArmTransform.position, base.transform.position);
			}
			else if (this.currentState == TransferrableObject.PositionState.OnChest)
			{
				num = Vector3.Distance(this.ownerRig.myBodyDockPositions.chestTransform.position, base.transform.position);
			}
			return num;
		}

		[Tooltip("Child object with the PickupableCosmetic script")]
		[SerializeField]
		private PickupableVariant pickupableVariant;

		[Tooltip("cosmetics released at a greater distance from the dock than the threshold will be placed in world instead of returning to the dock")]
		[SerializeField]
		private float returnToDockDistanceThreshold = 0.7f;

		[FormerlySerializedAs("OnReturnToDockPosition")]
		[Space]
		public UnityEvent OnReturnToDockPositionLocal;

		public UnityEvent OnReturnToDockPositionShared;

		[FormerlySerializedAs("OnGrabFromDockPosition")]
		public UnityEvent OnGrabLocal;

		private RubberDuckEvents _events;

		private TransferrableObject transferrableObject;

		private bool isLocal;

		private NetPlayer owner;

		private CallLimiter callLimiterRelease = new CallLimiter(10, 2f, 0.5f);

		private CallLimiter callLimiterReturn = new CallLimiter(10, 2f, 0.5f);
	}
}
