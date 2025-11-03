using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaNetworking
{
	public class FriendshipBracelet : MonoBehaviour
	{
		protected void Awake()
		{
			this.ownerRig = base.GetComponentInParent<VRRig>();
		}

		private AudioSource GetAudioSource()
		{
			if (!this.isLeftHand)
			{
				return this.ownerRig.rightHandPlayer;
			}
			return this.ownerRig.leftHandPlayer;
		}

		private void OnEnable()
		{
			this.PlayAppearEffects();
		}

		public void PlayAppearEffects()
		{
			this.GetAudioSource().GTPlayOneShot(this.braceletFormedSound, 1f);
			if (this.braceletFormedParticle)
			{
				this.braceletFormedParticle.Play();
			}
		}

		private void OnDisable()
		{
			if (!this.ownerRig.gameObject.activeInHierarchy)
			{
				return;
			}
			this.GetAudioSource().GTPlayOneShot(this.braceletBrokenSound, 1f);
			if (this.braceletBrokenParticle)
			{
				this.braceletBrokenParticle.Play();
			}
		}

		public void UpdateBeads(List<Color> colors, int selfIndex)
		{
			int num = colors.Count - 1;
			int num2 = (this.braceletBeads.Length - num) / 2;
			for (int i = 0; i < this.braceletBeads.Length; i++)
			{
				int num3 = i - num2;
				if (num3 >= 0 && num3 < num)
				{
					this.braceletBeads[i].enabled = true;
					this.braceletBeads[i].material.color = colors[num3];
					this.braceletBananas[i].gameObject.SetActive(num3 == selfIndex);
				}
				else
				{
					this.braceletBeads[i].enabled = false;
					this.braceletBananas[i].gameObject.SetActive(false);
				}
			}
			SkinnedMeshRenderer[] array = this.braceletStrings;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].material.color = colors[colors.Count - 1];
			}
		}

		[SerializeField]
		private SkinnedMeshRenderer[] braceletStrings;

		[SerializeField]
		private MeshRenderer[] braceletBeads;

		[SerializeField]
		private MeshRenderer[] braceletBananas;

		[SerializeField]
		private bool isLeftHand;

		[SerializeField]
		private AudioClip braceletFormedSound;

		[SerializeField]
		private AudioClip braceletBrokenSound;

		[SerializeField]
		private ParticleSystem braceletFormedParticle;

		[SerializeField]
		private ParticleSystem braceletBrokenParticle;

		private VRRig ownerRig;
	}
}
