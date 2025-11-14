using System;
using UnityEngine;

public class SIGadgetBlasterProjectile : MonoBehaviourTick
{
	public override void Tick()
	{
		if (Time.time > this.timeSpawned + this.maxLifetime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public new void OnEnable()
	{
		base.OnEnable();
		this.rb.linearVelocity = base.transform.forward * this.startingVelocity;
		this.timeSpawned = Time.time;
	}

	private void OnTriggerEnter(Collider other)
	{
		SIPlayer componentInParent = other.GetComponentInParent<SIPlayer>();
		if (componentInParent == null)
		{
			return;
		}
		if (componentInParent == this.firedByPlayer)
		{
			return;
		}
		if (this.firedByPlayer != SIPlayer.LocalPlayer || componentInParent == SIPlayer.LocalPlayer)
		{
			return;
		}
		this.parentBlaster.ProjectileHit(componentInParent, this);
	}

	private void OnCollisionEnter(Collision collision)
	{
		this.parentBlaster.ProjectileHit(null, this);
	}

	private void OnDestroy()
	{
	}

	public Rigidbody rb;

	public GameObject hitEffect;

	public GameObject hitEffectPlayer;

	public SIGadgetBlasterProjectile.BlasterProjectileSize projectileSize;

	public float maxLifetime = 10f;

	public float timeSpawned;

	public SIGadgetBlaster parentBlaster;

	public int projectileId;

	public SIPlayer firedByPlayer;

	public float startingVelocity;

	public enum BlasterProjectileSize
	{
		Small,
		Medium,
		Large
	}
}
