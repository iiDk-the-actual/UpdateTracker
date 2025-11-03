using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class CrittersRigActorSetup : MonoBehaviour
{
	public void OnEnable()
	{
		CrittersManager.RegisterRigActorSetup(this);
	}

	public void OnDisable()
	{
		for (int i = 0; i < this.rigActors.Length; i++)
		{
			this.rigActors[i].actorSet = null;
		}
	}

	private CrittersActor RefreshActorForIndex(int index)
	{
		CrittersRigActorSetup.RigActor rigActor = this.rigActors[index];
		if (rigActor.actorSet.IsNotNull())
		{
			rigActor.actorSet.gameObject.SetActive(false);
		}
		CrittersActor crittersActor = CrittersManager.instance.SpawnActor(rigActor.type, rigActor.subIndex);
		if (crittersActor.IsNull())
		{
			return null;
		}
		crittersActor.isOnPlayer = true;
		crittersActor.rigIndex = index;
		crittersActor.rigPlayerId = this.myRig.Creator.ActorNumber;
		if (crittersActor.rigPlayerId == -1 && PhotonNetwork.InRoom)
		{
			crittersActor.rigPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
		}
		crittersActor.PlacePlayerCrittersActor();
		return crittersActor;
	}

	public void CheckUpdate(ref List<object> refActorData, bool forceCheck = false)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		for (int i = 0; i < this.rigActors.Length; i++)
		{
			CrittersRigActorSetup.RigActor rigActor = this.rigActors[i];
			RigContainer rigContainer;
			if (forceCheck || rigActor.actorSet == null || (rigActor.actorSet.rigPlayerId != this.myRig.Creator.ActorNumber && VRRigCache.Instance.TryGetVrrig(this.myRig.Creator, out rigContainer) && CrittersManager.instance.rigSetupByRig.ContainsKey(this.myRig)))
			{
				CrittersActor crittersActor = this.RefreshActorForIndex(i);
				if (crittersActor != null)
				{
					crittersActor.AddPlayerCrittersActorDataToList(ref refActorData);
				}
			}
		}
	}

	public CrittersRigActorSetup.RigActor[] rigActors;

	public List<object> rigActorData = new List<object>();

	public VRRig myRig;

	[Serializable]
	public struct RigActor
	{
		public Transform location;

		public CrittersActor.CrittersActorType type;

		public int subIndex;

		public CrittersActor actorSet;
	}
}
