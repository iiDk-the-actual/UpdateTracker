using System;
using GorillaLocomotion;
using UnityEngine;

public class SIGadgetGrenadeGravity : SIGadgetGrenade
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.gravityField.SetActive(false);
		this.state = SIGadgetGrenadeGravity.State.Idle;
		this.stateRemainingDuration = -1f;
		this.isLocalPlayerInEffect = false;
	}

	protected override void HandleActivated()
	{
		if (this.state == SIGadgetGrenadeGravity.State.Idle)
		{
			this.activatedLocally = true;
			this.SetStateAuthority(SIGadgetGrenadeGravity.State.Activated);
			return;
		}
		this.SetStateAuthority(SIGadgetGrenadeGravity.State.Idle);
	}

	protected override void HandleThrown()
	{
	}

	protected override void HandleHitSurface()
	{
	}

	protected override void OnUpdateAuthority(float dt)
	{
		switch (this.state)
		{
		case SIGadgetGrenadeGravity.State.Idle:
			break;
		case SIGadgetGrenadeGravity.State.Activated:
			this.stateRemainingDuration -= dt;
			if (this.stateRemainingDuration <= 0f)
			{
				this.SetStateAuthority(SIGadgetGrenadeGravity.State.Triggered);
				return;
			}
			break;
		case SIGadgetGrenadeGravity.State.Triggered:
			this.stateRemainingDuration -= dt;
			if (this.stateRemainingDuration <= 0f)
			{
				this.SetStateAuthority(SIGadgetGrenadeGravity.State.Idle);
				return;
			}
			if (this.freezePositionOnTrigger)
			{
				this.CheckReenabledFreezePosition();
			}
			break;
		default:
			return;
		}
	}

	protected override void OnUpdateRemote(float dt)
	{
		SIGadgetGrenadeGravity.State state = (SIGadgetGrenadeGravity.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
		if (this.freezePositionOnTrigger)
		{
			this.CheckReenabledFreezePosition();
		}
	}

	private void SetStateAuthority(SIGadgetGrenadeGravity.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(SIGadgetGrenadeGravity.State newState)
	{
		if (newState == this.state || !this.CanChangeState((long)newState))
		{
			return;
		}
		this.state = newState;
		switch (this.state)
		{
		case SIGadgetGrenadeGravity.State.Idle:
			this.activatedLocally = false;
			this.stateRemainingDuration = -1f;
			this.mesh.material = this.idleMat;
			this.DeactivateGravityEffect();
			return;
		case SIGadgetGrenadeGravity.State.Activated:
			this.stateRemainingDuration = this.counterDuration;
			this.mesh.material = this.activatedMat;
			this.DeactivateGravityEffect();
			return;
		case SIGadgetGrenadeGravity.State.Triggered:
			this.stateRemainingDuration = this.triggerDuration;
			this.mesh.material = this.triggeredMat;
			this.ActivateGravityEffect();
			return;
		default:
			return;
		}
	}

	public bool CanChangeState(long newStateIndex)
	{
		return newStateIndex >= 0L && newStateIndex < 3L;
	}

	private void ActivateGravityEffect()
	{
		this.gravityField.SetActive(true);
		if (this.freezePositionOnTrigger)
		{
			this.rb.isKinematic = true;
			this.rb.linearVelocity = Vector3.zero;
		}
	}

	private void DeactivateGravityEffect()
	{
		this.gravityField.SetActive(false);
		if (this.isLocalPlayerInEffect)
		{
			this.isLocalPlayerInEffect = false;
			GTPlayer instance = GTPlayer.Instance;
			if (instance != null)
			{
				instance.UnsetGravityOverride(this);
			}
		}
		if (this.freezePositionOnTrigger && !this.thrownGadget.IsHeld())
		{
			this.rb.isKinematic = false;
		}
	}

	private void CheckReenabledFreezePosition()
	{
		if (this.state == SIGadgetGrenadeGravity.State.Triggered && !this.thrownGadget.IsHeld() && !this.rb.isKinematic)
		{
			this.rb.isKinematic = true;
			this.rb.linearVelocity = Vector3.zero;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		GTPlayer instance = GTPlayer.Instance;
		if (instance != null && collider == instance.headCollider)
		{
			this.isLocalPlayerInEffect = true;
			instance.SetGravityOverride(this, new Action<GTPlayer>(this.GravityOverrideFunction));
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		GTPlayer instance = GTPlayer.Instance;
		if (instance != null && collider == instance.headCollider)
		{
			this.isLocalPlayerInEffect = false;
			instance.UnsetGravityOverride(this);
		}
	}

	public void GravityOverrideFunction(GTPlayer player)
	{
		Vector3 vector = Physics.gravity * this.standardGravityMultiplier;
		Vector3 vector2 = Vector3.zero;
		if (!this.thrownGadget.IsHeldLocal())
		{
			vector2 = (base.transform.position - player.headCollider.transform.position).normalized * this.attractorStrength;
		}
		player.AddForce((vector + vector2) * player.scale, ForceMode.Acceleration);
	}

	[Header("Activation")]
	[SerializeField]
	private float counterDuration = 1f;

	[Header("Gravity Effect")]
	[SerializeField]
	private GameObject gravityField;

	[SerializeField]
	private bool freezePositionOnTrigger;

	[SerializeField]
	private float triggerDuration = 5f;

	[SerializeField]
	private float standardGravityMultiplier = 1f;

	[SerializeField]
	private float attractorStrength;

	[Header("FX")]
	[SerializeField]
	private MeshRenderer mesh;

	[SerializeField]
	private Material idleMat;

	[SerializeField]
	private Material activatedMat;

	[SerializeField]
	private Material triggeredMat;

	private SIGadgetGrenadeGravity.State state;

	private float stateRemainingDuration;

	private bool isLocalPlayerInEffect;

	private enum State
	{
		Idle,
		Activated,
		Triggered,
		Count
	}
}
