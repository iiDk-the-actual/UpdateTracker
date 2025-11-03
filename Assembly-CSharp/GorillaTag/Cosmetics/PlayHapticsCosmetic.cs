using System;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class PlayHapticsCosmetic : MonoBehaviour
	{
		private void Awake()
		{
			this.parentTransferable = base.GetComponentInParent<TransferrableObject>();
		}

		public void PlayHaptics()
		{
			GorillaTagger.Instance.StartVibration(this.leftHand, this.hapticStrength, this.hapticDuration);
		}

		public void PlayHapticsTransferableObject()
		{
			if (this.parentTransferable != null && this.parentTransferable.IsMyItem())
			{
				bool flag = this.parentTransferable.InLeftHand();
				GorillaTagger.Instance.StartVibration(flag, this.hapticStrength, this.hapticDuration);
			}
		}

		public void PlayHaptics(bool isLeftHand)
		{
			GorillaTagger.Instance.StartVibration(isLeftHand, this.hapticStrength, this.hapticDuration);
		}

		public void PlayHapticsBothHands(bool isLeftHand)
		{
			this.PlayHaptics(false);
			this.PlayHaptics(true);
		}

		public void PlayHaptics(bool isLeftHand, float value)
		{
			GorillaTagger.Instance.StartVibration(isLeftHand, this.hapticStrength, this.hapticDuration);
		}

		public void PlayHapticsBothHands(bool isLeftHand, float value)
		{
			this.PlayHaptics(false, value);
			this.PlayHaptics(true, value);
		}

		public void PlayHaptics(bool isLeftHand, Collider other)
		{
			GorillaTagger.Instance.StartVibration(isLeftHand, this.hapticStrength, this.hapticDuration);
		}

		public void PlayHapticsBothHands(bool isLeftHand, Collider other)
		{
			this.PlayHaptics(false, other);
			this.PlayHaptics(true, other);
		}

		public void PlayHaptics(bool isLeftHand, Collision other)
		{
			GorillaTagger.Instance.StartVibration(isLeftHand, this.hapticStrength, this.hapticDuration);
		}

		public void PlayHapticsBothHands(bool isLeftHand, Collision other)
		{
			this.PlayHaptics(false, other);
			this.PlayHaptics(true, other);
		}

		public void PlayHapticsByButtonValue(bool isLeftHand, float strength)
		{
			float num = Mathf.InverseLerp(this.minHapticStrengthThreshold, this.maxHapticStrengthThreshold, strength);
			GorillaTagger.Instance.StartVibration(isLeftHand, num, this.hapticDuration);
		}

		public void PlayHapticsByButtonValueBothHands(bool isLeftHand, float strength)
		{
			this.PlayHapticsByButtonValue(false, strength);
			this.PlayHapticsByButtonValue(true, strength);
		}

		public void PlayHapticsByVelocity(bool isLeftHand, float velocity)
		{
			float num = GTPlayer.Instance.GetInteractPointVelocityTracker(isLeftHand).GetAverageVelocity(true, 0.15f, false).magnitude;
			num = Mathf.InverseLerp(this.minHapticStrengthThreshold, this.maxHapticStrengthThreshold, num);
			GorillaTagger.Instance.StartVibration(isLeftHand, num, this.hapticDuration);
		}

		public void PlayHapticsByVelocityBothHands(bool isLeftHand, float velocity)
		{
			this.PlayHapticsByVelocity(false, velocity);
			this.PlayHapticsByVelocity(true, velocity);
		}

		[SerializeField]
		private float hapticDuration;

		[SerializeField]
		private float hapticStrength;

		[SerializeField]
		private float minHapticStrengthThreshold;

		[SerializeField]
		private float maxHapticStrengthThreshold;

		[Tooltip("Only check this box if you are not setting the left/hand right from the subscriber")]
		[SerializeField]
		private bool leftHand;

		private TransferrableObject parentTransferable;
	}
}
