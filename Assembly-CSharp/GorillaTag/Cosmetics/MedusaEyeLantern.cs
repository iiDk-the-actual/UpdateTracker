using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaLocomotion.Climbing;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class MedusaEyeLantern : MonoBehaviour
	{
		private void Awake()
		{
			foreach (MedusaEyeLantern.EyeState eyeState in this.allStates)
			{
				this.allStatesDict.Add(eyeState.eyeState, eyeState);
			}
		}

		private void OnDestroy()
		{
			this.allStatesDict.Clear();
		}

		private void Start()
		{
			if (this.rotatingObjectTransform == null)
			{
				this.rotatingObjectTransform = base.transform;
			}
			this.initialRotation = this.rotatingObjectTransform.localRotation;
			this.SwitchState(MedusaEyeLantern.State.DORMANT);
		}

		private void Update()
		{
			if (!this.transferableParent.InHand() && this.currentState != MedusaEyeLantern.State.DORMANT)
			{
				this.SwitchState(MedusaEyeLantern.State.DORMANT);
			}
			if (!this.transferableParent.InHand())
			{
				return;
			}
			this.UpdateState();
			if (this.velocityTracker == null || this.rotatingObjectTransform == null)
			{
				return;
			}
			Vector3 averageVelocity = this.velocityTracker.GetAverageVelocity(true, 0.15f, false);
			Vector3 vector = new Vector3(averageVelocity.x, 0f, averageVelocity.z);
			float magnitude = vector.magnitude;
			Vector3 normalized = vector.normalized;
			float num = Mathf.Clamp(-normalized.z, -1f, 1f) * this.maxRotationAngle * (magnitude * this.rotationSpeedMultiplier);
			float num2 = Mathf.Clamp(normalized.x, -1f, 1f) * this.maxRotationAngle * (magnitude * this.rotationSpeedMultiplier);
			this.targetRotation = this.initialRotation * Quaternion.Euler(num, 0f, num2);
			if (magnitude > this.sloshVelocityThreshold)
			{
				this.SwitchState(MedusaEyeLantern.State.SLOSHING);
			}
			if ((double)magnitude < 0.01)
			{
				this.targetRotation = this.initialRotation;
			}
			if (!this.EyeIsLockedOn())
			{
				this.rotatingObjectTransform.localRotation = Quaternion.Slerp(this.rotatingObjectTransform.localRotation, this.targetRotation, Time.deltaTime * this.rotationSmoothing);
			}
		}

		public void HandleOnNoOneInRange()
		{
			this.SwitchState(MedusaEyeLantern.State.RESET);
			this.resetTargetTime = Time.time;
			this.rotatingObjectTransform.localRotation = this.initialRotation;
		}

		public void HandleOnNewPlayerDetected(VRRig target, float distance)
		{
			this.targetRig = target;
			if (this.currentState != MedusaEyeLantern.State.SLOSHING)
			{
				this.SwitchState(MedusaEyeLantern.State.TRACKING);
			}
		}

		private void Sloshing()
		{
			Vector3 averageVelocity = this.velocityTracker.GetAverageVelocity(true, 0.15f, false);
			Vector3 vector = new Vector3(averageVelocity.x, 0f, averageVelocity.z);
			if ((double)vector.magnitude < 0.01)
			{
				this.SwitchState(MedusaEyeLantern.State.DORMANT);
			}
		}

		private void FaceTarget()
		{
			if (this.targetRig == null || this.rotatingObjectTransform == null)
			{
				return;
			}
			Vector3 normalized = (this.targetRig.tagSound.transform.position - this.rotatingObjectTransform.position).normalized;
			Vector3 normalized2 = new Vector3(normalized.x, 0f, normalized.z).normalized;
			Debug.DrawRay(this.rotatingObjectTransform.position, this.rotatingObjectTransform.forward * 0.3f, Color.blue);
			Debug.DrawRay(this.rotatingObjectTransform.position, normalized2 * 0.3f, Color.green);
			if (normalized2.sqrMagnitude > 0.001f)
			{
				float num = Mathf.Acos(Mathf.Clamp(Vector3.Dot(this.rotatingObjectTransform.forward.normalized, normalized2), -1f, 1f)) * 57.29578f;
				if (180f - num < this.targetHeadAngleThreshold && this.currentState == MedusaEyeLantern.State.TRACKING)
				{
					this.SwitchState(MedusaEyeLantern.State.WARMUP);
					return;
				}
				Quaternion quaternion = Quaternion.LookRotation(-normalized2, Vector3.up);
				this.rotatingObjectTransform.rotation = Quaternion.RotateTowards(this.rotatingObjectTransform.rotation, quaternion, this.lookAtTargetSpeed * Time.deltaTime);
			}
		}

		private bool IsTargetLookingAtEye()
		{
			if (this.targetRig == null || this.rotatingObjectTransform == null)
			{
				return false;
			}
			Transform transform = this.targetRig.tagSound.transform;
			Vector3 normalized = (this.rotatingObjectTransform.position - this.rotatingObjectTransform.forward * this.faceDistanceOffset - transform.position).normalized;
			float num = Mathf.Acos(Mathf.Clamp(Vector3.Dot(transform.up.normalized, normalized), -1f, 1f)) * 57.29578f;
			Debug.DrawRay(transform.position, transform.up * 0.3f, Color.magenta);
			Debug.DrawRay(transform.position, normalized * 0.3f, Color.yellow);
			return num < this.lookAtEyeAngleThreshold;
		}

		private void UpdateState()
		{
			switch (this.currentState)
			{
			case MedusaEyeLantern.State.SLOSHING:
				this.Sloshing();
				break;
			case MedusaEyeLantern.State.DORMANT:
				this.warmupCounter = 0f;
				this.petrificationStarted = float.PositiveInfinity;
				if (this.targetRig != null && (this.targetRig.transform.position - base.transform.position).IsShorterThan(this.distanceChecker.distanceThreshold))
				{
					this.SwitchState(MedusaEyeLantern.State.TRACKING);
				}
				break;
			case MedusaEyeLantern.State.TRACKING:
				this.FaceTarget();
				break;
			case MedusaEyeLantern.State.WARMUP:
				this.warmupCounter += Time.deltaTime;
				this.FaceTarget();
				if (this.warmupCounter > this.warmUpProgressTime)
				{
					this.SwitchState(MedusaEyeLantern.State.PRIMING);
					this.warmupCounter = 0f;
				}
				break;
			case MedusaEyeLantern.State.PRIMING:
				this.FaceTarget();
				if (this.IsTargetLookingAtEye())
				{
					UnityEvent<VRRig> onPetrification = this.OnPetrification;
					if (onPetrification != null)
					{
						onPetrification.Invoke(this.targetRig);
					}
					this.SwitchState(MedusaEyeLantern.State.PETRIFICATION);
					this.petrificationStarted = Time.time;
				}
				break;
			case MedusaEyeLantern.State.PETRIFICATION:
				if (Time.time - this.petrificationStarted > this.petrificationDuration)
				{
					this.SwitchState(MedusaEyeLantern.State.COOLDOWN);
				}
				break;
			case MedusaEyeLantern.State.COOLDOWN:
				if (Time.time - this.petrificationStarted > this.resetCooldown)
				{
					this.SwitchState(MedusaEyeLantern.State.DORMANT);
					this.petrificationStarted = float.PositiveInfinity;
				}
				break;
			case MedusaEyeLantern.State.RESET:
				if (Time.time - this.resetTargetTime > this.resetTargetTimer)
				{
					this.resetTargetTime = float.PositiveInfinity;
					this.SwitchState(MedusaEyeLantern.State.DORMANT);
				}
				break;
			}
			this.PlayHaptic(this.currentState);
		}

		private void SwitchState(MedusaEyeLantern.State newState)
		{
			this.lastState = this.currentState;
			this.currentState = newState;
			MedusaEyeLantern.EyeState eyeState;
			if (this.lastState != this.currentState && this.allStatesDict.TryGetValue(newState, out eyeState))
			{
				UnityEvent onEnterState = eyeState.onEnterState;
				if (onEnterState != null)
				{
					onEnterState.Invoke();
				}
			}
			MedusaEyeLantern.EyeState eyeState2;
			if (this.lastState != this.currentState && this.allStatesDict.TryGetValue(this.lastState, out eyeState2))
			{
				UnityEvent onExitState = eyeState2.onExitState;
				if (onExitState == null)
				{
					return;
				}
				onExitState.Invoke();
			}
		}

		private void PlayHaptic(MedusaEyeLantern.State state)
		{
			if (!this.transferableParent.IsMyItem())
			{
				return;
			}
			MedusaEyeLantern.EyeState eyeState;
			this.allStatesDict.TryGetValue(state, out eyeState);
			if (this.currentState == MedusaEyeLantern.State.WARMUP)
			{
				float num = Mathf.Clamp01(this.warmupCounter / this.warmUpProgressTime);
				if (eyeState != null && eyeState.hapticStrength != null)
				{
					float num2 = eyeState.hapticStrength.Evaluate(num);
					bool flag = this.transferableParent.InLeftHand();
					GorillaTagger.Instance.StartVibration(flag, num2, Time.deltaTime);
					return;
				}
			}
			else if (eyeState != null && eyeState.hapticStrength != null)
			{
				float num3 = eyeState.hapticStrength.Evaluate(0.5f);
				bool flag2 = this.transferableParent.InLeftHand();
				GorillaTagger.Instance.StartVibration(flag2, num3, Time.deltaTime);
			}
		}

		private bool EyeIsLockedOn()
		{
			return this.currentState == MedusaEyeLantern.State.TRACKING || this.currentState == MedusaEyeLantern.State.WARMUP || this.currentState == MedusaEyeLantern.State.PRIMING;
		}

		[SerializeField]
		private DistanceCheckerCosmetic distanceChecker;

		[SerializeField]
		private TransferrableObject transferableParent;

		[SerializeField]
		private GorillaVelocityTracker velocityTracker;

		[SerializeField]
		private Transform rotatingObjectTransform;

		[Space]
		[Header("Rotation Settings")]
		[SerializeField]
		private float maxRotationAngle = 50f;

		[SerializeField]
		private float sloshVelocityThreshold = 1f;

		[SerializeField]
		private float rotationSmoothing = 10f;

		[SerializeField]
		private float rotationSpeedMultiplier = 5f;

		[Space]
		[Header("Target Tracking Settings")]
		[SerializeField]
		private float lookAtEyeAngleThreshold = 90f;

		[SerializeField]
		private float targetHeadAngleThreshold = 5f;

		[SerializeField]
		private float lookAtTargetSpeed = 5f;

		[SerializeField]
		private float warmUpProgressTime = 3f;

		[SerializeField]
		private float resetCooldown = 5f;

		[SerializeField]
		private float faceDistanceOffset = 0.2f;

		[SerializeField]
		private float petrificationDuration = 0.2f;

		[Space]
		[Header("Eye State Settings")]
		public MedusaEyeLantern.EyeState[] allStates = new MedusaEyeLantern.EyeState[0];

		public UnityEvent<VRRig> OnPetrification;

		private Quaternion initialRotation;

		private Quaternion targetRotation;

		private MedusaEyeLantern.State currentState;

		private MedusaEyeLantern.State lastState;

		private float petrificationStarted = float.PositiveInfinity;

		private float warmupCounter;

		private Dictionary<MedusaEyeLantern.State, MedusaEyeLantern.EyeState> allStatesDict = new Dictionary<MedusaEyeLantern.State, MedusaEyeLantern.EyeState>();

		private VRRig targetRig;

		private float resetTargetTimer = 1f;

		private float resetTargetTime = float.PositiveInfinity;

		[Serializable]
		public class EyeState
		{
			public MedusaEyeLantern.State eyeState;

			public AnimationCurve hapticStrength;

			public UnityEvent onEnterState;

			public UnityEvent onExitState;
		}

		public enum State
		{
			SLOSHING,
			DORMANT,
			TRACKING,
			WARMUP,
			PRIMING,
			PETRIFICATION,
			COOLDOWN,
			RESET
		}
	}
}
