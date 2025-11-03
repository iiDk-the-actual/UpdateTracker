using System;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(OnTriggerEventsCosmetic))]
public class SeedPacketTriggerHandler : MonoBehaviour
{
	public void OnTriggerEntered()
	{
		if (this.toggleOnceOnly && this.triggerEntered)
		{
			return;
		}
		this.triggerEntered = true;
		UnityEvent<SeedPacketTriggerHandler> unityEvent = this.onTriggerEntered;
		if (unityEvent != null)
		{
			unityEvent.Invoke(this);
		}
		this.ToggleEffects();
	}

	public void ToggleEffects()
	{
		if (this.particleToPlay)
		{
			this.particleToPlay.Play();
		}
		if (this.soundBankPlayer)
		{
			this.soundBankPlayer.Play();
		}
		if (this.destroyOnTriggerEnter)
		{
			if (this.destroyDelay > 0f)
			{
				base.Invoke("Destroy", this.destroyDelay);
				return;
			}
			this.Destroy();
		}
	}

	private void Destroy()
	{
		this.triggerEntered = false;
		if (ObjectPools.instance.DoesPoolExist(base.gameObject))
		{
			ObjectPools.instance.Destroy(base.gameObject);
			return;
		}
		Object.Destroy(base.gameObject);
	}

	[SerializeField]
	private ParticleSystem particleToPlay;

	[SerializeField]
	private SoundBankPlayer soundBankPlayer;

	[SerializeField]
	private bool destroyOnTriggerEnter;

	[SerializeField]
	private float destroyDelay = 1f;

	[SerializeField]
	private bool toggleOnceOnly;

	[HideInInspector]
	public UnityEvent<SeedPacketTriggerHandler> onTriggerEntered;

	private bool triggerEntered;
}
