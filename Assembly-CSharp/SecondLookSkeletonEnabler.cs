using System;
using UnityEngine;

public class SecondLookSkeletonEnabler : Tappable
{
	private void Awake()
	{
		this.isTapped = false;
		this.skele = Object.FindFirstObjectByType<SecondLookSkeleton>();
		this.skele.spookyText = this.spookyText;
	}

	public override void OnTapLocal(float tapStrength, float tapTime, PhotonMessageInfoWrapped info)
	{
		if (!this.isTapped)
		{
			base.OnTapLocal(tapStrength, tapTime, info);
			if (this.skele != null)
			{
				this.skele.tapped = true;
			}
			base.gameObject.SetActive(false);
			this.isTapped = true;
			this.playOnDisappear.GTPlay();
			this.particles.Play();
		}
	}

	public bool isTapped;

	public AudioSource playOnDisappear;

	public ParticleSystem particles;

	public GameObject spookyText;

	private SecondLookSkeleton skele;
}
