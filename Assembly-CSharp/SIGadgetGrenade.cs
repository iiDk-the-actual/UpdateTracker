using System;
using UnityEngine;

public abstract class SIGadgetGrenade : SIGadget
{
	protected new virtual void OnEnable()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.activatedLocally = false;
		this.thrownGadget.OnActivated += this.HandleActivated;
		this.thrownGadget.OnThrown += this.HandleThrown;
		this.thrownGadget.OnHitSurface += this.HandleHitSurface;
	}

	protected new virtual void OnDisable()
	{
		this.thrownGadget.OnActivated -= this.HandleActivated;
		this.thrownGadget.OnThrown -= this.HandleThrown;
		this.thrownGadget.OnHitSurface -= this.HandleHitSurface;
	}

	protected abstract void HandleActivated();

	protected abstract void HandleThrown();

	protected abstract void HandleHitSurface();

	public override void OnEntityInit()
	{
		base.OnEntityInit();
		GameEntityId entityIdFromNetId = this.gameEntity.manager.GetEntityIdFromNetId((int)this.gameEntity.createData);
		this.parentEntity = this.gameEntity.manager.GetGameEntity(entityIdFromNetId);
		SIGadgetHolsterDisk component = this.parentEntity.GetComponent<SIGadgetHolsterDisk>();
		if (component != null)
		{
			component.RegisterGadget(this);
		}
	}

	public Action GrenadeFinished;

	public Renderer grenadeRenderer;

	[SerializeField]
	protected ThrownGadget thrownGadget;

	protected Rigidbody rb;

	protected GameEntity parentEntity;
}
