using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class HypnoRing : MonoBehaviour, ISpawnable
{
	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	void ISpawnable.OnDespawn()
	{
	}

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this.myRig = rig;
	}

	private void Update()
	{
		if ((this.attachedToLeftHand ? this.myRig.leftIndex.calcT : this.myRig.rightIndex.calcT) > 0.5f)
		{
			base.transform.localRotation *= Quaternion.AngleAxis(Time.deltaTime * this.rotationSpeed, Vector3.up);
			this.currentVolume = Mathf.MoveTowards(this.currentVolume, this.maxVolume, Time.deltaTime / this.fadeInDuration);
			this.audioSource.volume = this.currentVolume;
			if (!this.audioSource.isPlaying)
			{
				this.audioSource.GTPlay();
				return;
			}
		}
		else
		{
			this.currentVolume = Mathf.MoveTowards(this.currentVolume, 0f, Time.deltaTime / this.fadeOutDuration);
			if (this.audioSource.isPlaying)
			{
				if (this.currentVolume == 0f)
				{
					this.audioSource.GTStop();
					return;
				}
				this.audioSource.volume = this.currentVolume;
			}
		}
	}

	[SerializeField]
	private bool attachedToLeftHand;

	private VRRig myRig;

	[SerializeField]
	private float rotationSpeed;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private float maxVolume = 1f;

	[SerializeField]
	private float fadeInDuration;

	[SerializeField]
	private float fadeOutDuration;

	private float currentVolume;
}
