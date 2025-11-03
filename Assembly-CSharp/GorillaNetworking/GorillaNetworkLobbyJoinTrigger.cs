using System;
using UnityEngine;

namespace GorillaNetworking
{
	public class GorillaNetworkLobbyJoinTrigger : GorillaTriggerBox
	{
		public GameObject[] makeSureThisIsDisabled;

		public GameObject[] makeSureThisIsEnabled;

		public string gameModeName;

		public PhotonNetworkController photonNetworkController;

		public string componentTypeToRemove;

		public GameObject componentRemoveTarget;

		public string componentTypeToAdd;

		public GameObject componentAddTarget;

		public GameObject gorillaParent;

		public GameObject joinFailedBlock;
	}
}
