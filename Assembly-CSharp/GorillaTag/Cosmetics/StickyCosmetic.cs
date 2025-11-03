using System;
using GorillaExtensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class StickyCosmetic : MonoBehaviour
	{
		private void Start()
		{
			this.endRigidbody.isKinematic = false;
			this.endRigidbody.useGravity = false;
			this.UpdateState(StickyCosmetic.ObjectState.Idle);
		}

		public void Extend()
		{
			if (this.currentState == StickyCosmetic.ObjectState.Idle || this.currentState == StickyCosmetic.ObjectState.Extending)
			{
				this.UpdateState(StickyCosmetic.ObjectState.Extending);
			}
		}

		public void Retract()
		{
			this.UpdateState(StickyCosmetic.ObjectState.Retracting);
		}

		private void Extend_Internal()
		{
			if (this.endRigidbody.isKinematic)
			{
				return;
			}
			this.rayLength = Mathf.Lerp(0f, this.maxObjectLength, this.blendShapeCosmetic.GetBlendValue() / this.blendShapeCosmetic.maxBlendShapeWeight);
			this.endRigidbody.MovePosition(this.startPosition.position + this.startPosition.forward * this.rayLength);
		}

		private void Retract_Internal()
		{
			this.endRigidbody.isKinematic = false;
			Vector3 vector = Vector3.MoveTowards(this.endRigidbody.position, this.startPosition.position, this.retractSpeed * Time.fixedDeltaTime);
			this.endRigidbody.MovePosition(vector);
		}

		private void FixedUpdate()
		{
			switch (this.currentState)
			{
			case StickyCosmetic.ObjectState.Extending:
			{
				if (Time.time - this.extendingStartedTime > this.retractAfterSecond)
				{
					this.UpdateState(StickyCosmetic.ObjectState.AutoRetract);
				}
				this.Extend_Internal();
				RaycastHit raycastHit;
				if (Physics.Raycast(this.rayOrigin.position, this.rayOrigin.forward, out raycastHit, this.rayLength, this.collisionLayers))
				{
					this.endRigidbody.isKinematic = true;
					this.endRigidbody.transform.parent = null;
					UnityEvent unityEvent = this.onStick;
					if (unityEvent != null)
					{
						unityEvent.Invoke();
					}
					this.UpdateState(StickyCosmetic.ObjectState.Stuck);
				}
				break;
			}
			case StickyCosmetic.ObjectState.Retracting:
				if (Vector3.Distance(this.endRigidbody.position, this.startPosition.position) <= 0.01f)
				{
					this.endRigidbody.position = this.startPosition.position;
					Transform transform = this.endRigidbody.transform;
					transform.parent = this.endPositionParent;
					transform.localRotation = quaternion.identity;
					transform.localScale = Vector3.one;
					if (this.lastState == StickyCosmetic.ObjectState.AutoUnstuck || this.lastState == StickyCosmetic.ObjectState.AutoRetract)
					{
						this.UpdateState(StickyCosmetic.ObjectState.JustRetracted);
					}
					else
					{
						this.UpdateState(StickyCosmetic.ObjectState.Idle);
					}
				}
				else
				{
					this.Retract_Internal();
				}
				break;
			case StickyCosmetic.ObjectState.Stuck:
				if (this.endRigidbody.isKinematic && (this.endRigidbody.position - this.startPosition.position).IsLongerThan(this.autoRetractThreshold))
				{
					this.UpdateState(StickyCosmetic.ObjectState.AutoUnstuck);
				}
				break;
			case StickyCosmetic.ObjectState.AutoUnstuck:
				this.UpdateState(StickyCosmetic.ObjectState.Retracting);
				break;
			case StickyCosmetic.ObjectState.AutoRetract:
				this.UpdateState(StickyCosmetic.ObjectState.Retracting);
				break;
			}
			Debug.DrawRay(this.rayOrigin.position, this.rayOrigin.forward * this.rayLength, Color.red);
		}

		private void UpdateState(StickyCosmetic.ObjectState newState)
		{
			this.lastState = this.currentState;
			if (this.lastState == StickyCosmetic.ObjectState.Stuck && newState != this.currentState)
			{
				this.onUnstick.Invoke();
			}
			if (this.lastState != StickyCosmetic.ObjectState.Extending && newState == StickyCosmetic.ObjectState.Extending)
			{
				this.extendingStartedTime = Time.time;
			}
			this.currentState = newState;
		}

		[Tooltip("Optional reference to an UpdateBlendShapeCosmetic component. Used to drive extension length based on blend shape weight (e.g. finger flex input).")]
		[SerializeField]
		private UpdateBlendShapeCosmetic blendShapeCosmetic;

		[Tooltip("Defines which physics layers this sticky object can attach to when extending (checked via raycast).")]
		[SerializeField]
		private LayerMask collisionLayers;

		[Tooltip("Transform origin from which the raycast will be fired forward to detect stickable surfaces.")]
		[SerializeField]
		private Transform rayOrigin;

		[Tooltip("Transform representing the start or base position of the sticky object (where extension originates).")]
		[SerializeField]
		private Transform startPosition;

		[Tooltip("Rigidbody controlling the physical end of the sticky object (the part that extends and can attach).")]
		[SerializeField]
		private Rigidbody endRigidbody;

		[Tooltip("Parent transform the end object will reattach to when fully retracted. This keeps local transform resets consistent.")]
		[SerializeField]
		private Transform endPositionParent;

		[Tooltip("Maximum distance the object can extend from its start position (in meters).")]
		[SerializeField]
		private float maxObjectLength = 0.7f;

		[Tooltip("If the sticky object remains stuck but the distance from start exceeds this threshold, it will automatically unstuck and begin retracting.")]
		[SerializeField]
		private float autoRetractThreshold = 1f;

		[Tooltip("Speed (units per second) at which the end rigidbody retracts toward its start position when returning.")]
		[SerializeField]
		private float retractSpeed = 5f;

		[Tooltip("If the sticky end remains extended but doesn’t stick to anything, it will automatically start retracting after this many seconds.")]
		[SerializeField]
		private float retractAfterSecond = 2f;

		[Tooltip("Invoked when the sticky object successfully attaches to a surface.")]
		public UnityEvent onStick;

		[Tooltip("Invoked when the sticky object becomes unstuck — either manually or automatically.")]
		public UnityEvent onUnstick;

		private StickyCosmetic.ObjectState currentState;

		private float rayLength;

		private bool stick;

		private StickyCosmetic.ObjectState lastState;

		private float extendingStartedTime;

		private enum ObjectState
		{
			Extending,
			Retracting,
			Stuck,
			JustRetracted,
			Idle,
			AutoUnstuck,
			AutoRetract
		}
	}
}
