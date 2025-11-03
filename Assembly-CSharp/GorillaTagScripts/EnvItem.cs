using System;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts
{
	public class EnvItem : MonoBehaviour, IPunInstantiateMagicCallback
	{
		public void OnEnable()
		{
		}

		public void OnDisable()
		{
		}

		public void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] instantiationData = info.photonView.InstantiationData;
			this.spawnedByPhotonViewId = (int)instantiationData[0];
		}

		public int spawnedByPhotonViewId;
	}
}
