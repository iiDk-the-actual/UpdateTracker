using System;
using Liv.Lck.Cosmetics;
using UnityEngine;

public class GtLckNetworkCosmeticDependantPlayerIdSupplier : MonoBehaviour, ILckCosmeticDependantPlayerIdSupplier
{
	public event PlayerIdUpdatedEvent PlayerIdUpdated;

	public string GetPlayerId()
	{
		return this.vrrig.OwningNetPlayer.UserId;
	}

	public void UpdatePlayerId()
	{
		Debug.Log("LCK: GtLckNetworkCosmeticDependantPlayerIdSupplier::UpdatePlayerId, ID is now: " + this.vrrig.OwningNetPlayer.UserId);
		PlayerIdUpdatedEvent playerIdUpdated = this.PlayerIdUpdated;
		if (playerIdUpdated == null)
		{
			return;
		}
		playerIdUpdated();
	}

	[SerializeField]
	private VRRig vrrig;
}
