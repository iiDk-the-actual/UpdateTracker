using System;
using System.Collections.Generic;
using GorillaNetworking;
using UnityEngine;

public class GRReviveStation : MonoBehaviour
{
	public int Index { get; set; }

	public void Init(GhostReactor reactor, int index)
	{
		this.reactor = reactor;
		this.Index = index;
	}

	public void SetReviveCooldownSeconds(double seconds)
	{
		this.reviveCooldownSeconds = seconds;
	}

	public double GetReviveCooldownSeconds()
	{
		return this.reviveCooldownSeconds;
	}

	public double CalculateRemainingReviveCooldownSeconds(int ActorNumber)
	{
		if (this.reviveCooldownSeconds == 0.0)
		{
			return 0.0;
		}
		if (this.cooldownStartTime.ContainsKey(ActorNumber))
		{
			return this.reviveCooldownSeconds - (GorillaComputer.instance.GetServerTime() - this.cooldownStartTime[ActorNumber]).TotalSeconds;
		}
		return 0.0;
	}

	public void RevivePlayer(GRPlayer player)
	{
		if (player != null)
		{
			int actorNumber = player.gamePlayer.rig.OwningNetPlayer.ActorNumber;
			this.cooldownStartTime[actorNumber] = GorillaComputer.instance.GetServerTime();
			if (player.State != GRPlayer.GRPlayerState.Alive || player.Hp < player.MaxHp)
			{
				player.OnPlayerRevive(this.reactor.grManager);
				if (this.audioSource != null)
				{
					this.audioSource.Play();
				}
				if (this.particleEffects != null)
				{
					for (int i = 0; i < this.particleEffects.Length; i++)
					{
						this.particleEffects[i].Play();
					}
				}
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		if (attachedRigidbody != null)
		{
			VRRig component = attachedRigidbody.GetComponent<VRRig>();
			if (component != null)
			{
				GRPlayer component2 = component.GetComponent<GRPlayer>();
				if (component2 != null && (component2.State != GRPlayer.GRPlayerState.Alive || component2.Hp < component2.MaxHp))
				{
					if (!NetworkSystem.Instance.InRoom && component == VRRig.LocalRig)
					{
						this.RevivePlayer(component2);
					}
					if (this.reactor.grManager.IsAuthority() && this.CalculateRemainingReviveCooldownSeconds(component2.gamePlayer.rig.OwningNetPlayer.ActorNumber) <= 0.0)
					{
						this.reactor.grManager.RequestPlayerRevive(this, component2);
					}
				}
			}
		}
	}

	public AudioSource audioSource;

	public ParticleSystem[] particleEffects;

	[SerializeField]
	private double reviveCooldownSeconds;

	private Dictionary<int, DateTime> cooldownStartTime = new Dictionary<int, DateTime>();

	private GhostReactor reactor;
}
