using System;
using UnityEngine;

public class SIGadgetHolsterDisk : SIGadget, I_SIDisruptable
{
	private void Awake()
	{
		this.SetState(SIGadgetHolsterDisk.State.Unequipped);
		this.referenceGadget.gameObject.SetActive(false);
		this.referenceTransform = this.referenceGadget.transform;
		this.cooldownTimer = 0f;
	}

	private void Start()
	{
		this.CreateGadget();
	}

	private void CreateGadget()
	{
		this.gameEntity.manager.RequestCreateItem(this.referenceGadget.gameObject.name.GetStaticHash(), this.referenceGadget.transform.position, this.referenceGadget.transform.rotation, (long)this.gameEntity.GetNetId());
	}

	public void RegisterGadget(SIGadget gadget)
	{
		this.cachedGadget = gadget;
		this.grenadeGadget = this.cachedGadget.GetComponent<SIGadgetGrenade>();
		this.gadgetRB = this.cachedGadget.GetComponent<Rigidbody>();
		SIGadgetGrenade sigadgetGrenade = this.grenadeGadget;
		sigadgetGrenade.GrenadeFinished = (Action)Delegate.Combine(sigadgetGrenade.GrenadeFinished, new Action(this.GadgetRespawn));
		this.cachedGadget.gameObject.SetActive(false);
		this.GadgetRespawn();
	}

	private new void OnDisable()
	{
		if (this.grenadeGadget != null)
		{
			SIGadgetGrenade sigadgetGrenade = this.grenadeGadget;
			sigadgetGrenade.GrenadeFinished = (Action)Delegate.Remove(sigadgetGrenade.GrenadeFinished, new Action(this.GadgetRespawn));
		}
	}

	protected override void OnUpdateAuthority(float dt)
	{
		base.OnUpdateAuthority(dt);
		switch (this.state)
		{
		case SIGadgetHolsterDisk.State.Unequipped:
		case SIGadgetHolsterDisk.State.Ready:
			break;
		case SIGadgetHolsterDisk.State.OnCooldown:
			this.cooldownTimer += dt;
			this.grenadeGadget.grenadeRenderer.material.SetFloat("_RespawnAmount", this.cooldownTimer / this.cooldownTime);
			if (this.cooldownTimer > this.cooldownTime)
			{
				this.SetState(SIGadgetHolsterDisk.State.Ready);
			}
			break;
		default:
			return;
		}
	}

	private void SetState(SIGadgetHolsterDisk.State newState)
	{
		if (this.state == newState)
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case SIGadgetHolsterDisk.State.Unequipped:
			this.cooldownTimer = 0f;
			return;
		case SIGadgetHolsterDisk.State.OnCooldown:
			break;
		case SIGadgetHolsterDisk.State.Ready:
			this.cachedGadget.gameEntity.pickupable = true;
			break;
		default:
			return;
		}
	}

	public void DiskSnappedToHolster()
	{
		this.cachedGadget.gameObject.SetActive(true);
		this.gameEntity.pickupable = false;
		this.GadgetRespawn();
	}

	public void DiskRemovedFromHolster()
	{
		this.SetState(SIGadgetHolsterDisk.State.Unequipped);
		this.gameEntity.pickupable = true;
		this.cachedGadget.gameObject.SetActive(false);
	}

	public void GadgetRespawn()
	{
		this.cachedGadget.transform.parent = base.transform;
		this.cachedGadget.transform.localPosition = this.referenceTransform.localPosition;
		this.cachedGadget.transform.localRotation = this.referenceTransform.localRotation;
		this.cachedGadget.gameEntity.pickupable = false;
		this.gadgetRB.isKinematic = true;
		this.SetState(SIGadgetHolsterDisk.State.OnCooldown);
		this.cooldownTimer = 0f;
	}

	public void Disrupt(float disruptTime)
	{
		this.SetState(SIGadgetHolsterDisk.State.OnCooldown);
		this.cooldownTimer = -disruptTime;
	}

	public SIGadget referenceGadget;

	public float cooldownTime;

	private SIGadgetHolsterDisk.State state;

	private float cooldownTimer;

	private SIGadgetGrenade grenadeGadget;

	private Rigidbody gadgetRB;

	private SIGadget cachedGadget;

	private Transform referenceTransform;

	private enum State
	{
		Unequipped,
		OnCooldown,
		Ready
	}
}
