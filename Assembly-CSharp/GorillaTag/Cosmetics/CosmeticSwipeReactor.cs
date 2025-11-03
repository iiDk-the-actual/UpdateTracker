using System;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	[RequireComponent(typeof(Collider))]
	public class CosmeticSwipeReactor : MonoBehaviour, ITickSystemTick
	{
		private void Awake()
		{
			this._rig = base.GetComponentInParent<VRRig>();
			if (this._rig == null && base.gameObject.GetComponentInParent<GTPlayer>() != null)
			{
				this._rig = GorillaTagger.Instance.offlineVRRig;
			}
			this.isLocal = this._rig != null && this._rig.isLocal;
			this.col = base.GetComponent<Collider>();
			switch (this.localSwipeAxis)
			{
			case CosmeticSwipeReactor.Axis.X:
				this.swipeDir = Vector3.right;
				return;
			case CosmeticSwipeReactor.Axis.Y:
				this.swipeDir = Vector3.up;
				return;
			case CosmeticSwipeReactor.Axis.Z:
				this.swipeDir = Vector3.forward;
				return;
			default:
				return;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!this.isLocal || !base.enabled)
			{
				return;
			}
			GorillaTriggerColliderHandIndicator component = other.GetComponent<GorillaTriggerColliderHandIndicator>();
			if (component != null)
			{
				if (component.isLeftHand)
				{
					this.handIndicatorL = component;
					Vector3 vector = base.transform.InverseTransformPoint(component.transform.position);
					this.ResetProgress(true, vector);
					this.handInTriggerL = true;
				}
				else
				{
					this.handIndicatorR = component;
					Vector3 vector2 = base.transform.InverseTransformPoint(component.transform.position);
					this.ResetProgress(false, vector2);
					this.handInTriggerR = true;
				}
			}
			if ((this.handInTriggerL || this.handInTriggerR) && !this.TickRunning)
			{
				TickSystem<object>.AddTickCallback(this);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (!this.isLocal || !base.enabled)
			{
				return;
			}
			GorillaTriggerColliderHandIndicator component = other.GetComponent<GorillaTriggerColliderHandIndicator>();
			if (component != null)
			{
				if (component.isLeftHand)
				{
					this.handInTriggerL = false;
					if (this.resetCooldownOnTriggerExit)
					{
						this.isCoolingDownL = false;
						this.cooldownEndL = double.MinValue;
					}
				}
				else
				{
					this.handInTriggerR = false;
					if (this.resetCooldownOnTriggerExit)
					{
						this.isCoolingDownR = false;
						this.cooldownEndR = double.MinValue;
					}
				}
			}
			if (!this.handInTriggerL && !this.handInTriggerR && this.TickRunning)
			{
				TickSystem<object>.RemoveTickCallback(this);
			}
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (this.handInTriggerL)
			{
				this.ProcessHandMovement(this.handIndicatorL, this.startPosL, ref this.lastFramePosL, ref this.swipingUpL, ref this.distanceL, ref this.isCoolingDownL, ref this.cooldownEndL);
			}
			if (this.handInTriggerR)
			{
				this.ProcessHandMovement(this.handIndicatorR, this.startPosR, ref this.lastFramePosR, ref this.swipingUpR, ref this.distanceR, ref this.isCoolingDownR, ref this.cooldownEndR);
			}
			if (!this.handInTriggerL && !this.handInTriggerR && this.TickRunning)
			{
				TickSystem<object>.RemoveTickCallback(this);
			}
		}

		private void ResetProgress(bool left, Vector3 pos)
		{
			if (left)
			{
				this.startPosL = pos;
				this.lastFramePosL = this.startPosL;
				this.distanceL = 0f;
				return;
			}
			this.startPosR = pos;
			this.lastFramePosR = this.startPosR;
			this.distanceR = 0f;
		}

		private void ProcessHandMovement(GorillaTriggerColliderHandIndicator hand, Vector3 start, ref Vector3 last, ref bool swipingUp, ref float dist, ref bool isCoolingDown, ref double cooldownEndTime)
		{
			if (isCoolingDown)
			{
				if (Time.timeAsDouble < cooldownEndTime)
				{
					return;
				}
				isCoolingDown = false;
				cooldownEndTime = double.MinValue;
				this.ResetProgress(hand.isLeftHand, base.transform.InverseTransformPoint(hand.transform.position));
				return;
			}
			else
			{
				Vector3 vector = base.transform.InverseTransformPoint(hand.transform.position);
				float num = Mathf.Abs(this.GetAxisComponent(hand.currentVelocity));
				if (num < this.minimumVelocity * this._rig.scaleFactor || num > this.maximumVelocity * this._rig.scaleFactor)
				{
					this.ResetProgress(hand.isLeftHand, vector);
					return;
				}
				float num2 = this.GetAxisComponent(vector) - this.GetAxisComponent(last);
				if (num2 >= 0f && !swipingUp)
				{
					swipingUp = true;
					this.ResetProgress(hand.isLeftHand, vector);
					return;
				}
				if ((num2 < 0f) & swipingUp)
				{
					swipingUp = false;
					this.ResetProgress(hand.isLeftHand, vector);
					return;
				}
				if ((this.GetLateralMovement(start) - this.GetLateralMovement(vector)).sqrMagnitude > this.lateralMovementTolerance * this.lateralMovementTolerance)
				{
					this.ResetProgress(hand.isLeftHand, vector);
					return;
				}
				last = vector;
				dist += Mathf.Abs(num2);
				GorillaTagger.Instance.StartVibration(hand.isLeftHand, this.swipeHaptics.Evaluate(dist / this.swipeDistance), Time.deltaTime);
				if (dist >= this.swipeDistance)
				{
					if (swipingUp)
					{
						UnityEvent<bool> onSwipe = this.OnSwipe;
						if (onSwipe != null)
						{
							onSwipe.Invoke(hand.isLeftHand);
						}
						cooldownEndTime = Time.timeAsDouble + (double)this.swipeCooldown;
						isCoolingDown = true;
					}
					else
					{
						UnityEvent<bool> onReverseSwipe = this.OnReverseSwipe;
						if (onReverseSwipe != null)
						{
							onReverseSwipe.Invoke(hand.isLeftHand);
						}
						cooldownEndTime = Time.timeAsDouble + (double)this.swipeCooldown;
						isCoolingDown = true;
					}
					this.ResetProgress(hand.isLeftHand, vector);
				}
				return;
			}
		}

		private float GetAxisComponent(Vector3 vec)
		{
			CosmeticSwipeReactor.Axis axis = this.localSwipeAxis;
			if (axis == CosmeticSwipeReactor.Axis.X)
			{
				return vec.x;
			}
			if (axis != CosmeticSwipeReactor.Axis.Y)
			{
				return vec.z;
			}
			return vec.y;
		}

		private Vector2 GetLateralMovement(Vector3 vec)
		{
			CosmeticSwipeReactor.Axis axis = this.localSwipeAxis;
			if (axis == CosmeticSwipeReactor.Axis.X)
			{
				return new Vector2(vec.y, vec.z);
			}
			if (axis != CosmeticSwipeReactor.Axis.Y)
			{
				return new Vector2(vec.x, vec.y);
			}
			return new Vector2(vec.x, vec.z);
		}

		[SerializeField]
		private CosmeticSwipeReactor.Axis localSwipeAxis = CosmeticSwipeReactor.Axis.Y;

		private Vector3 swipeDir = Vector3.up;

		[Tooltip("Distance hand can move perpindicular to the swipe without cancelling the gesture")]
		[SerializeField]
		private float lateralMovementTolerance = 0.1f;

		[Tooltip("How far the hand has to move along the axis to count as a swipe\nThis distance must be contained within the trigger area")]
		[SerializeField]
		private float swipeDistance = 0.3f;

		[SerializeField]
		private float minimumVelocity = 0.1f;

		[SerializeField]
		private float maximumVelocity = 3f;

		[Tooltip("Delay after completing a swipe before starting the next")]
		[SerializeField]
		private float swipeCooldown = 0.25f;

		[SerializeField]
		private bool resetCooldownOnTriggerExit = true;

		[Tooltip("Amplitude of haptics from normalized swiped distance")]
		[SerializeField]
		private AnimationCurve swipeHaptics = AnimationCurve.EaseInOut(0f, 0.02f, 1f, 0.5f);

		public UnityEvent<bool> OnSwipe;

		public UnityEvent<bool> OnReverseSwipe;

		private VRRig _rig;

		private Collider col;

		private bool isLocal;

		private bool handInTriggerR;

		private bool handInTriggerL;

		private GorillaTriggerColliderHandIndicator handIndicatorR;

		private GorillaTriggerColliderHandIndicator handIndicatorL;

		private Vector3 startPosR;

		private Vector3 startPosL;

		private Vector3 lastFramePosR;

		private Vector3 lastFramePosL;

		private float distanceR;

		private float distanceL;

		private bool swipingUpL;

		private bool swipingUpR;

		private double cooldownEndL = double.MinValue;

		private double cooldownEndR = double.MinValue;

		private bool isCoolingDownL;

		private bool isCoolingDownR;

		public enum Axis
		{
			X,
			Y,
			Z
		}
	}
}
