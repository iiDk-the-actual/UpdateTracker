using System;
using GorillaTag.CosmeticSystem;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class SquirtingFlowerBadgeCosmetic : MonoBehaviour, ISpawnable, IFingerFlexListener
	{
		public VRRig MyRig { get; private set; }

		public bool IsSpawned { get; set; }

		public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

		public void OnSpawn(VRRig rig)
		{
			this.MyRig = rig;
		}

		public void OnDespawn()
		{
		}

		private void Update()
		{
			if (!this.restartTimer && Time.time - this.triggeredTime >= this.coolDownTimer)
			{
				this.restartTimer = true;
			}
		}

		private void OnPlayEffectLocal()
		{
			if (this.particlesToPlay != null)
			{
				this.particlesToPlay.Play();
			}
			if (this.objectToEnable != null)
			{
				this.objectToEnable.SetActive(true);
			}
			if (this.audioSource != null && this.audioToPlay != null)
			{
				this.audioSource.GTPlayOneShot(this.audioToPlay, 1f);
			}
			this.restartTimer = false;
			this.triggeredTime = Time.time;
		}

		public void OnButtonPressed(bool isLeftHand, float value)
		{
			if (!this.FingerFlexValidation(isLeftHand))
			{
				return;
			}
			if (!this.restartTimer || !this.buttonReleased)
			{
				return;
			}
			this.OnPlayEffectLocal();
			this.buttonReleased = false;
		}

		public void OnButtonReleased(bool isLeftHand, float value)
		{
			if (!this.FingerFlexValidation(isLeftHand))
			{
				return;
			}
			this.buttonReleased = true;
		}

		public void OnButtonPressStayed(bool isLeftHand, float value)
		{
		}

		public bool FingerFlexValidation(bool isLeftHand)
		{
			return (!this.leftHand || isLeftHand) && (this.leftHand || !isLeftHand);
		}

		[SerializeField]
		private ParticleSystem particlesToPlay;

		[SerializeField]
		private GameObject objectToEnable;

		[SerializeField]
		private AudioClip audioToPlay;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private float coolDownTimer = 2f;

		[SerializeField]
		private bool leftHand;

		private float triggeredTime;

		private bool restartTimer;

		private bool buttonReleased = true;
	}
}
