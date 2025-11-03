using System;
using UnityEngine;

public class VirtualStumpRevivePlayer : MonoBehaviour
{
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
						this.defaultReviveStation.RevivePlayer(component2);
					}
					if (this.ghostReactorManager.IsAuthority())
					{
						this.ghostReactorManager.RequestPlayerRevive(this.defaultReviveStation, component2);
					}
				}
			}
		}
	}

	[SerializeField]
	private GhostReactorManager ghostReactorManager;

	[SerializeField]
	private GRReviveStation defaultReviveStation;
}
