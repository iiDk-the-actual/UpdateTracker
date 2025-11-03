using System;
using Photon.Pun;
using UnityEngine;

public class SIGadgetGrenadeStun : SIGadgetGrenade
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.state = SIGadgetGrenadeStun.State.Idle;
	}

	protected override void HandleActivated()
	{
	}

	protected override void HandleHitSurface()
	{
		if (this.state == SIGadgetGrenadeStun.State.Thrown)
		{
			this.SetStateAuthority(SIGadgetGrenadeStun.State.Triggered);
		}
	}

	protected override void HandleThrown()
	{
		if (this.state == SIGadgetGrenadeStun.State.Idle)
		{
			this.SetStateAuthority(SIGadgetGrenadeStun.State.Thrown);
		}
	}

	protected override void OnUpdateAuthority(float dt)
	{
	}

	protected override void OnUpdateRemote(float dt)
	{
		SIGadgetGrenadeStun.State state = (SIGadgetGrenadeStun.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetStateAuthority(SIGadgetGrenadeStun.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(SIGadgetGrenadeStun.State newState)
	{
		if (newState == this.state)
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case SIGadgetGrenadeStun.State.Idle:
		case SIGadgetGrenadeStun.State.Thrown:
			break;
		case SIGadgetGrenadeStun.State.Triggered:
			this.TriggerExplosion();
			break;
		default:
			return;
		}
	}

	private void TriggerExplosion()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, this.explosionRadius, UnityLayer.GorillaTagCollider.ToLayerMask());
		for (int i = 0; i < array.Length; i++)
		{
			VRRig componentInParent = array[i].GetComponentInParent<VRRig>();
			if (componentInParent != null)
			{
				Vector3 vector = componentInParent.transform.position - base.transform.position;
				float magnitude = vector.magnitude;
				float num = 1f - magnitude / this.explosionRadius;
				float num2 = this.knockbackStrength * num;
				RoomSystem.LaunchPlayer(componentInParent.OwningNetPlayer, num2 * vector / magnitude);
				RoomSystem.SendStatusEffectToPlayer(RoomSystem.StatusEffects.TaggedTime, componentInParent.OwningNetPlayer);
			}
		}
		if (this.gameEntity.lastHeldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			this.SetStateAuthority(SIGadgetGrenadeStun.State.Idle);
		}
	}

	[SerializeField]
	private float knockbackStrength;

	[SerializeField]
	private float explosionRadius;

	private SIGadgetGrenadeStun.State state;

	private enum State
	{
		Idle,
		Thrown,
		Triggered
	}
}
