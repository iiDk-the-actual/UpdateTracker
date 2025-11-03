using System;
using UnityEngine;

public class MonkeyeProjectileTarget : MonoBehaviour
{
	private void Awake()
	{
		this.monkeyeAI = base.GetComponent<MonkeyeAI>();
		this.notifier = base.GetComponentInChildren<SlingshotProjectileHitNotifier>();
	}

	private void OnEnable()
	{
		if (this.notifier != null)
		{
			this.notifier.OnProjectileHit += this.Notifier_OnProjectileHit;
			this.notifier.OnPaperPlaneHit += this.Notifier_OnPaperPlaneHit;
		}
	}

	private void OnDisable()
	{
		if (this.notifier != null)
		{
			this.notifier.OnProjectileHit -= this.Notifier_OnProjectileHit;
			this.notifier.OnPaperPlaneHit -= this.Notifier_OnPaperPlaneHit;
		}
	}

	private void Notifier_OnProjectileHit(SlingshotProjectile projectile, Collision collision)
	{
		this.monkeyeAI.SetSleep();
	}

	private void Notifier_OnPaperPlaneHit(PaperPlaneProjectile projectile, Collider collider)
	{
		this.monkeyeAI.SetSleep();
	}

	private MonkeyeAI monkeyeAI;

	private SlingshotProjectileHitNotifier notifier;
}
