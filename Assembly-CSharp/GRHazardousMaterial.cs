using System;
using GorillaLocomotion;
using UnityEngine;

public class GRHazardousMaterial : MonoBehaviour
{
	public void Init(GhostReactor reactor)
	{
		this.reactor = reactor;
	}

	public void OnLocalPlayerOverlap()
	{
		GRPlayer component = VRRig.LocalRig.GetComponent<GRPlayer>();
		if (component != null && component.State == GRPlayer.GRPlayerState.Alive)
		{
			this.reactor.grManager.RequestPlayerStateChange(component, GRPlayer.GRPlayerState.Ghost);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider == GTPlayer.Instance.headCollider || collider == GTPlayer.Instance.bodyCollider)
		{
			this.OnLocalPlayerOverlap();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider == GTPlayer.Instance.headCollider || collision.collider == GTPlayer.Instance.bodyCollider)
		{
			this.OnLocalPlayerOverlap();
		}
	}

	private GhostReactor reactor;
}
