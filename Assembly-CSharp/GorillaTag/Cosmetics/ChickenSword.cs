using System;
using GorillaExtensions;
using GorillaLocomotion.Climbing;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class ChickenSword : MonoBehaviour
	{
		private void Awake()
		{
			this.lastHitTime = float.PositiveInfinity;
			this.SwitchState(ChickenSword.SwordState.Ready);
		}

		internal void OnEnable()
		{
			if (this._events == null)
			{
				this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
				NetPlayer netPlayer = ((this.transferrableObject.myOnlineRig != null) ? this.transferrableObject.myOnlineRig.creator : ((this.transferrableObject.myRig != null) ? ((this.transferrableObject.myRig.creator != null) ? this.transferrableObject.myRig.creator : NetworkSystem.Instance.LocalPlayer) : null));
				if (netPlayer != null)
				{
					this._events.Init(netPlayer);
				}
				else
				{
					Debug.LogError("Failed to get a reference to the Photon Player needed to hook up the cosmetic event");
				}
			}
			if (this._events != null)
			{
				this._events.Activate += this.OnReachedLastTransformationStep;
			}
		}

		private void OnDisable()
		{
			if (this._events != null)
			{
				this._events.Activate -= this.OnReachedLastTransformationStep;
				this._events.Dispose();
				this._events = null;
			}
		}

		private void Update()
		{
			ChickenSword.SwordState swordState = this.currentState;
			if (swordState != ChickenSword.SwordState.Ready)
			{
				if (swordState != ChickenSword.SwordState.Deflated)
				{
					return;
				}
				if (Time.time - this.lastHitTime > this.rechargeCooldown)
				{
					this.lastHitTime = float.PositiveInfinity;
					this.SwitchState(ChickenSword.SwordState.Ready);
					UnityEvent onRechargedShared = this.OnRechargedShared;
					if (onRechargedShared != null)
					{
						onRechargedShared.Invoke();
					}
					if (this.transferrableObject && this.transferrableObject.IsMyItem())
					{
						UnityEvent<bool> onRechargedLocal = this.OnRechargedLocal;
						if (onRechargedLocal == null)
						{
							return;
						}
						onRechargedLocal.Invoke(this.transferrableObject.InLeftHand());
					}
				}
			}
			else if (this.hitReceievd)
			{
				this.hitReceievd = false;
				this.lastHitTime = Time.time;
				this.SwitchState(ChickenSword.SwordState.Deflated);
				UnityEvent onDeflatedShared = this.OnDeflatedShared;
				if (onDeflatedShared != null)
				{
					onDeflatedShared.Invoke();
				}
				if (this.transferrableObject && this.transferrableObject.IsMyItem())
				{
					UnityEvent<bool> onDeflatedLocal = this.OnDeflatedLocal;
					if (onDeflatedLocal == null)
					{
						return;
					}
					onDeflatedLocal.Invoke(this.transferrableObject.InLeftHand());
					return;
				}
			}
		}

		public void OnHitTargetSync(VRRig playerRig)
		{
			if (this.velocityTracker == null)
			{
				return;
			}
			Vector3 averageVelocity = this.velocityTracker.GetAverageVelocity(true, 0.15f, false);
			if (this.currentState == ChickenSword.SwordState.Ready && averageVelocity.magnitude > this.hitVelocityThreshold)
			{
				this.hitReceievd = true;
				UnityEvent<VRRig> onHitTargetShared = this.OnHitTargetShared;
				if (onHitTargetShared != null)
				{
					onHitTargetShared.Invoke(playerRig);
				}
				if (this.transferrableObject && this.transferrableObject.IsMyItem())
				{
					bool flag = this.transferrableObject.InLeftHand();
					UnityEvent<bool> onHitTargetLocal = this.OnHitTargetLocal;
					if (onHitTargetLocal != null)
					{
						onHitTargetLocal.Invoke(flag);
					}
				}
				if (this.cosmeticSwapper != null && playerRig == GorillaTagger.Instance.offlineVRRig && this.cosmeticSwapper.GetCurrentStepIndex(playerRig) >= this.cosmeticSwapper.GetNumberOfSteps() && PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
				{
					this._events.Activate.RaiseAll(Array.Empty<object>());
				}
			}
		}

		private void OnReachedLastTransformationStep(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "OnReachedLastTransformationStep");
			if (!this.callLimiter.CheckCallTime(Time.time))
			{
				return;
			}
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(info.Sender.ActorNumber), out rigContainer) && rigContainer.Rig.IsPositionInRange(base.transform.position, 6f))
			{
				UnityEvent<VRRig> onReachedLastTransformationStepShared = this.OnReachedLastTransformationStepShared;
				if (onReachedLastTransformationStepShared == null)
				{
					return;
				}
				onReachedLastTransformationStepShared.Invoke(rigContainer.Rig);
			}
		}

		private void SwitchState(ChickenSword.SwordState newState)
		{
			this.currentState = newState;
		}

		[SerializeField]
		private float rechargeCooldown;

		[SerializeField]
		private GorillaVelocityTracker velocityTracker;

		[SerializeField]
		private float hitVelocityThreshold;

		[SerializeField]
		private TransferrableObject transferrableObject;

		[SerializeField]
		private CosmeticSwapper cosmeticSwapper;

		[Space]
		[Space]
		public UnityEvent OnDeflatedShared;

		public UnityEvent<bool> OnDeflatedLocal;

		public UnityEvent OnRechargedShared;

		public UnityEvent<bool> OnRechargedLocal;

		public UnityEvent<VRRig> OnHitTargetShared;

		public UnityEvent<bool> OnHitTargetLocal;

		public UnityEvent<VRRig> OnReachedLastTransformationStepShared;

		private float lastHitTime;

		private ChickenSword.SwordState currentState;

		private bool hitReceievd;

		private RubberDuckEvents _events;

		private CallLimiter callLimiter = new CallLimiter(10, 2f, 0.5f);

		private enum SwordState
		{
			Ready,
			Deflated
		}
	}
}
