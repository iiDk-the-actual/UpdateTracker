using System;
using System.Collections.Generic;
using UnityEngine;

public class GRUIStationEmployeeBadges : MonoBehaviour, IGorillaSliceableSimple
{
	public void Init(GhostReactor reactor)
	{
		this.reactor = reactor;
		for (int i = 0; i < this.badgeDispensers.Count; i++)
		{
			this.badgeDispensers[i].Setup(reactor, i);
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		this.registeredBadges = new List<GRBadge>();
		for (int i = 0; i < this.badgeDispensers.Count; i++)
		{
			this.badgeDispensers[i].index = i;
			this.badgeDispensers[i].actorNr = -1;
		}
		this.dispenserForActorNr = new Dictionary<int, int>();
		VRRigCache.OnRigActivated += this.UpdateRigs;
		VRRigCache.OnRigDeactivated += this.UpdateRigs;
		RoomSystem.JoinedRoomEvent += new Action(this.UpdateRigs);
		this.UpdateRigs();
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		VRRigCache.OnRigActivated -= this.UpdateRigs;
		VRRigCache.OnRigDeactivated -= this.UpdateRigs;
		RoomSystem.JoinedRoomEvent -= new Action(this.UpdateRigs);
	}

	public void UpdateRigs(RigContainer container)
	{
		this.UpdateRigs();
	}

	public void UpdateRigs()
	{
		GRUIStationEmployeeBadges.tempRigs.Clear();
		GRUIStationEmployeeBadges.tempRigs.Add(VRRig.LocalRig);
		if (VRRigCache.Instance != null)
		{
			VRRigCache.Instance.GetAllUsedRigs(GRUIStationEmployeeBadges.tempRigs);
		}
	}

	public void RefreshBadgesAuthority()
	{
		for (int i = 0; i < GRUIStationEmployeeBadges.tempRigs.Count; i++)
		{
			NetPlayer netPlayer = (GRUIStationEmployeeBadges.tempRigs[i].isOfflineVRRig ? NetworkSystem.Instance.LocalPlayer : GRUIStationEmployeeBadges.tempRigs[i].OwningNetPlayer);
			int num;
			if (netPlayer != null && netPlayer.ActorNumber != -1 && !this.dispenserForActorNr.TryGetValue(netPlayer.ActorNumber, out num))
			{
				for (int j = 0; j < this.badgeDispensers.Count; j++)
				{
					if (this.badgeDispensers[j].actorNr == -1)
					{
						this.badgeDispensers[j].CreateBadge(netPlayer, this.reactor.grManager.gameEntityManager);
						break;
					}
				}
			}
		}
		for (int k = this.registeredBadges.Count - 1; k >= 0; k--)
		{
			int num2;
			if (NetworkSystem.Instance.GetNetPlayerByID(this.registeredBadges[k].actorNr) == null || !this.dispenserForActorNr.TryGetValue(this.registeredBadges[k].actorNr, out num2) || num2 != this.registeredBadges[k].dispenserIndex)
			{
				this.reactor.grManager.gameEntityManager.RequestDestroyItem(this.registeredBadges[k].GetComponent<GameEntity>().id);
			}
		}
	}

	public void SliceUpdate()
	{
		if (this.reactor == null || this.reactor.grManager == null)
		{
			return;
		}
		if (!this.reactor.grManager.IsZoneActive())
		{
			return;
		}
		if (this.reactor.grManager.gameEntityManager.IsAuthority())
		{
			this.RefreshBadgesAuthority();
		}
		for (int i = 0; i < this.badgeDispensers.Count; i++)
		{
			this.badgeDispensers[i].Refresh();
		}
	}

	public void RemoveBadge(GRBadge badge)
	{
		if (this.registeredBadges.Contains(badge))
		{
			this.registeredBadges.Remove(badge);
		}
		if (this.badgeDispensers[badge.dispenserIndex].idBadge == badge)
		{
			this.dispenserForActorNr.Remove(badge.actorNr);
			this.badgeDispensers[badge.dispenserIndex].ClearBadge();
		}
	}

	public void LinkBadgeToDispenser(GRBadge badge, long createData)
	{
		if (!this.registeredBadges.Contains(badge))
		{
			this.registeredBadges.Add(badge);
		}
		int num = (int)(createData % 100L);
		if (num > this.badgeDispensers.Count)
		{
			return;
		}
		NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID((int)(createData / 100L));
		if (netPlayerByID != null)
		{
			this.dispenserForActorNr[netPlayerByID.ActorNumber] = num;
			this.badgeDispensers[num].AttachIDBadge(badge, netPlayerByID);
		}
	}

	public GRUIEmployeeBadgeDispenser GetDispenserForPlayer(int actorNumber)
	{
		int num;
		if (!this.dispenserForActorNr.TryGetValue(actorNumber, out num))
		{
			return null;
		}
		return this.badgeDispensers[num];
	}

	[SerializeField]
	public List<GRUIEmployeeBadgeDispenser> badgeDispensers;

	private static List<VRRig> tempRigs = new List<VRRig>(16);

	public Dictionary<int, int> dispenserForActorNr;

	public List<GRBadge> registeredBadges;

	private GhostReactor reactor;
}
