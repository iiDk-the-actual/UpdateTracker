using System;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class ShakeDetectorCosmetic : MonoBehaviour
	{
		public Vector3 HandVelocity { get; private set; }

		private void Awake()
		{
			this.HandVelocity = Vector3.zero;
			this.shakeEndTime = 0f;
		}

		private void UpdateShakeVelocity()
		{
			if (!this.parentTransferrable)
			{
				return;
			}
			if (!this.parentTransferrable.InHand())
			{
				this.HandVelocity = Vector3.zero;
				return;
			}
			if (!this.parentTransferrable.IsMyItem())
			{
				return;
			}
			this.isLeftHand = this.parentTransferrable.InLeftHand();
			this.HandVelocity = GTPlayer.Instance.GetInteractPointVelocityTracker(this.isLeftHand).GetAverageVelocity(true, 0.15f, false);
			this.HandVelocity = Vector3.ClampMagnitude(this.HandVelocity, this.maxHandVelocity);
		}

		public void Update()
		{
			this.UpdateShakeVelocity();
			if (Time.time - this.shakeEndTime > this.cooldown && !this.isShaking && this.HandVelocity.magnitude >= this.shakeStartVelocityThreshold)
			{
				UnityEvent<bool, float> unityEvent = this.onShakeStartLocal;
				if (unityEvent != null)
				{
					unityEvent.Invoke(this.isLeftHand, this.HandVelocity.magnitude);
				}
				this.isShaking = true;
			}
			if (this.isShaking && this.HandVelocity.magnitude < this.shakeEndVelocityThreshold)
			{
				UnityEvent<bool, float> unityEvent2 = this.onShakeEndLocal;
				if (unityEvent2 != null)
				{
					unityEvent2.Invoke(this.isLeftHand, this.HandVelocity.magnitude);
				}
				this.isShaking = false;
				this.shakeEndTime = Time.time;
			}
		}

		[SerializeField]
		private TransferrableObject parentTransferrable;

		[Tooltip("for velocity equal or above this, we fire a Shake Start event")]
		[SerializeField]
		private float shakeStartVelocityThreshold;

		[Tooltip("for velocity under this, we fire a Shake End event")]
		[SerializeField]
		private float shakeEndVelocityThreshold;

		[Tooltip("cooldown starts when shaking ends")]
		[SerializeField]
		private float cooldown;

		[Tooltip("Use for clamping hand velocity value")]
		[SerializeField]
		private float maxHandVelocity = 20f;

		[FormerlySerializedAs("onShakeStart")]
		public UnityEvent<bool, float> onShakeStartLocal;

		[FormerlySerializedAs("onShakeEnd")]
		public UnityEvent<bool, float> onShakeEndLocal;

		private bool isShaking;

		private float shakeEndTime;

		private bool isLeftHand;
	}
}
