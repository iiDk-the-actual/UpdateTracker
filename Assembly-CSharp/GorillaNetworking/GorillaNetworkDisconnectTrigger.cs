using System;
using Photon.Pun;
using UnityEngine;

namespace GorillaNetworking
{
	public class GorillaNetworkDisconnectTrigger : GorillaTriggerBox
	{
		public override void OnBoxTriggered()
		{
			base.OnBoxTriggered();
			if (this.makeSureThisIsEnabled != null)
			{
				this.makeSureThisIsEnabled.SetActive(true);
			}
			GameObject[] array = this.makeSureTheseAreEnabled;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
			if (PhotonNetwork.InRoom)
			{
				if (this.componentTypeToRemove != "" && this.componentTarget.GetComponent(this.componentTypeToRemove) != null)
				{
					Object.Destroy(this.componentTarget.GetComponent(this.componentTypeToRemove));
				}
				PhotonNetwork.Disconnect();
				SkinnedMeshRenderer[] array2 = this.photonNetworkController.offlineVRRig;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].enabled = true;
				}
				PhotonNetwork.ConnectUsingSettings();
			}
		}

		public PhotonNetworkController photonNetworkController;

		public GameObject offlineVRRig;

		public GameObject makeSureThisIsEnabled;

		public GameObject[] makeSureTheseAreEnabled;

		public string componentTypeToRemove;

		public GameObject componentTarget;
	}
}
