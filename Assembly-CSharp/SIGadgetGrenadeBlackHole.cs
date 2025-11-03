using System;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

public class SIGadgetGrenadeBlackHole : SIGadgetGrenade
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.state = SIGadgetGrenadeBlackHole.State.Idle;
	}

	protected override void HandleActivated()
	{
	}

	protected override void HandleHitSurface()
	{
		if (this.state == SIGadgetGrenadeBlackHole.State.Thrown)
		{
			this.SetStateAuthority(SIGadgetGrenadeBlackHole.State.Triggered);
		}
	}

	protected override void HandleThrown()
	{
		if (this.state == SIGadgetGrenadeBlackHole.State.Idle)
		{
			this.SetStateAuthority(SIGadgetGrenadeBlackHole.State.Thrown);
		}
	}

	protected override void OnUpdateAuthority(float dt)
	{
	}

	protected override void OnUpdateRemote(float dt)
	{
		SIGadgetGrenadeBlackHole.State state = (SIGadgetGrenadeBlackHole.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetStateAuthority(SIGadgetGrenadeBlackHole.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(SIGadgetGrenadeBlackHole.State newState)
	{
		if (newState == this.state)
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case SIGadgetGrenadeBlackHole.State.Idle:
		case SIGadgetGrenadeBlackHole.State.Thrown:
			break;
		case SIGadgetGrenadeBlackHole.State.Triggered:
			this.TriggerExplosion();
			break;
		default:
			return;
		}
	}

	private void TriggerExplosion()
	{
		Vector3 vector = base.transform.position - GTPlayer.Instance.transform.position;
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
			this.SetStateAuthority(SIGadgetGrenadeBlackHole.State.Idle);
		}
	}

	[SerializeField]
	private float knockbackStrength;

	[SerializeField]
	private float explosionRadius;

	private SIGadgetGrenadeBlackHole.State state;

	private enum State
	{
		Idle,
		Thrown,
		Triggered
	}
}
