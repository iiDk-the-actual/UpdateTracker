using System;
using Photon.Pun;
using UnityEngine;

public class SIGadgetGrenadeDisrupt : SIGadgetGrenade
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.state = SIGadgetGrenadeDisrupt.State.Idle;
	}

	protected override void OnUpdateRemote(float dt)
	{
		SIGadgetGrenadeDisrupt.State state = (SIGadgetGrenadeDisrupt.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetStateAuthority(SIGadgetGrenadeDisrupt.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(SIGadgetGrenadeDisrupt.State newState)
	{
		if (newState == this.state)
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case SIGadgetGrenadeDisrupt.State.Idle:
		case SIGadgetGrenadeDisrupt.State.Thrown:
			break;
		case SIGadgetGrenadeDisrupt.State.Triggered:
			this.TriggerExplosion();
			break;
		default:
			return;
		}
	}

	private void TriggerExplosion()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, this.explosionRadius);
		for (int i = 0; i < array.Length; i++)
		{
			I_SIDisruptable componentInParent = array[i].GetComponentInParent<I_SIDisruptable>();
			if (componentInParent != null)
			{
				componentInParent.Disrupt(this.disruptTime);
			}
		}
		if (this.gameEntity.lastHeldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			this.SetStateAuthority(SIGadgetGrenadeDisrupt.State.Idle);
		}
		Action grenadeFinished = this.GrenadeFinished;
		if (grenadeFinished == null)
		{
			return;
		}
		grenadeFinished();
	}

	protected override void HandleActivated()
	{
	}

	protected override void HandleHitSurface()
	{
		if (this.state == SIGadgetGrenadeDisrupt.State.Thrown)
		{
			this.SetStateAuthority(SIGadgetGrenadeDisrupt.State.Triggered);
		}
	}

	protected override void HandleThrown()
	{
		if (this.state == SIGadgetGrenadeDisrupt.State.Idle)
		{
			this.SetStateAuthority(SIGadgetGrenadeDisrupt.State.Thrown);
		}
	}

	public float disruptTime;

	[SerializeField]
	private float explosionRadius;

	private SIGadgetGrenadeDisrupt.State state;

	private enum State
	{
		Idle,
		Thrown,
		Triggered
	}
}
