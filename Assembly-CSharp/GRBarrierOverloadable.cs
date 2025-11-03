using System;
using UnityEngine;

public class GRBarrierOverloadable : MonoBehaviour
{
	private void OnEnable()
	{
		this.tool.OnEnergyChange += this.OnEnergyChange;
		this.gameEntity.OnStateChanged += this.OnEntityStateChanged;
	}

	private void OnEnergyChange(GRTool tool, int energyChange, GameEntityId chargingEntityId)
	{
		if (this.state == GRBarrierOverloadable.State.Active && tool.energy >= tool.GetEnergyMax())
		{
			this.SetState(GRBarrierOverloadable.State.Destroyed);
			if (this.gameEntity.IsAuthority())
			{
				this.gameEntity.RequestState(this.gameEntity.id, 1L);
			}
		}
	}

	private void OnEntityStateChanged(long prevState, long nextState)
	{
		if (!this.gameEntity.IsAuthority())
		{
			this.SetState((GRBarrierOverloadable.State)nextState);
		}
	}

	public void SetState(GRBarrierOverloadable.State newState)
	{
		if (this.state != newState)
		{
			this.state = newState;
			GRBarrierOverloadable.State state = this.state;
			if (state == GRBarrierOverloadable.State.Active)
			{
				this.meshRenderer.enabled = true;
				this.collider.enabled = true;
				return;
			}
			if (state != GRBarrierOverloadable.State.Destroyed)
			{
				return;
			}
			this.audioSource.Play();
			this.meshRenderer.enabled = false;
			this.collider.enabled = false;
		}
	}

	public GRTool tool;

	public GameEntity gameEntity;

	public AudioSource audioSource;

	public MeshRenderer meshRenderer;

	public Collider collider;

	private GRBarrierOverloadable.State state;

	public enum State
	{
		Active,
		Destroyed
	}
}
