using System;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

public class SIGadgetGrenadeKnockBack : SIGadgetGrenade
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.state = SIGadgetGrenadeKnockBack.State.Idle;
	}

	protected override void HandleActivated()
	{
	}

	protected override void HandleHitSurface()
	{
		if (this.state == SIGadgetGrenadeKnockBack.State.Thrown)
		{
			this.SetStateAuthority(SIGadgetGrenadeKnockBack.State.Triggered);
		}
	}

	protected override void HandleThrown()
	{
		if (this.state == SIGadgetGrenadeKnockBack.State.Idle)
		{
			this.SetStateAuthority(SIGadgetGrenadeKnockBack.State.Thrown);
		}
	}

	protected override void OnUpdateAuthority(float dt)
	{
	}

	protected override void OnUpdateRemote(float dt)
	{
		SIGadgetGrenadeKnockBack.State state = (SIGadgetGrenadeKnockBack.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetStateAuthority(SIGadgetGrenadeKnockBack.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(SIGadgetGrenadeKnockBack.State newState)
	{
		if (newState == this.state)
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case SIGadgetGrenadeKnockBack.State.Idle:
		case SIGadgetGrenadeKnockBack.State.Thrown:
			break;
		case SIGadgetGrenadeKnockBack.State.Triggered:
			this.TriggerExplosion();
			break;
		default:
			return;
		}
	}

	private void TriggerExplosion()
	{
		Vector3 vector = GTPlayer.Instance.transform.position - base.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		if (this.explosionRadius * this.explosionRadius > sqrMagnitude)
		{
			float num = Mathf.Sqrt(sqrMagnitude);
			float num2 = 1f - num / this.explosionRadius;
			float num3 = this.knockbackStrength * num2;
			GTPlayer.Instance.ApplyKnockback(vector.normalized, num3, false);
		}
		if (this.gameEntity.lastHeldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			this.SetStateAuthority(SIGadgetGrenadeKnockBack.State.Idle);
		}
		Action grenadeFinished = this.GrenadeFinished;
		if (grenadeFinished == null)
		{
			return;
		}
		grenadeFinished();
	}

	[SerializeField]
	private float knockbackStrength;

	[SerializeField]
	private float explosionRadius;

	private SIGadgetGrenadeKnockBack.State state;

	private enum State
	{
		Idle,
		Thrown,
		Triggered
	}
}
