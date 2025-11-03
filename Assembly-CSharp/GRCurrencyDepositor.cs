using System;
using Photon.Pun;
using UnityEngine;

public class GRCurrencyDepositor : MonoBehaviour
{
	public void Init(GhostReactor reactor)
	{
		this.reactor = reactor;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody != null)
		{
			GRCollectible component = other.attachedRigidbody.GetComponent<GRCollectible>();
			if (component != null)
			{
				if ((component.type == ProgressionManager.CoreType.ChaosSeed && !this.collectSentientCores) || (component.type != ProgressionManager.CoreType.ChaosSeed && this.collectSentientCores))
				{
					return;
				}
				if (this.reactor.grManager.IsAuthority())
				{
					this.reactor.grManager.RequestDepositCollectible(component.entity.id);
				}
				this.collectibleDepositedEffect.Play();
				this.audioSource.volume = this.collectibleDepositedClipVolume;
				this.audioSource.PlayOneShot(this.collectibleDepositedClip);
				if (component.entity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					if (GamePlayerLocal.instance.gamePlayer.GetGrabbedGameEntityId(0) == component.entity.id)
					{
						GorillaTagger.Instance.StartVibration(true, 0.5f, 0.15f);
						return;
					}
					if (GamePlayerLocal.instance.gamePlayer.GetGrabbedGameEntityId(1) == component.entity.id)
					{
						GorillaTagger.Instance.StartVibration(false, 0.5f, 0.15f);
					}
				}
			}
		}
	}

	public Transform depositingChargePoint;

	[SerializeField]
	private ParticleSystem collectibleDepositedEffect;

	[SerializeField]
	private AudioClip collectibleDepositedClip;

	[SerializeField]
	private float collectibleDepositedClipVolume;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private bool collectSentientCores;

	private const float hapticStrength = 0.5f;

	private const float hapticDuration = 0.15f;

	[NonSerialized]
	public GhostReactor reactor;
}
