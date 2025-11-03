using System;
using Photon.Pun;
using UnityEngine;

public class PhotonViewXSceneRef : MonoBehaviour
{
	public PhotonView photonView
	{
		get
		{
			PhotonView photonView;
			if (this.reference.TryResolve<PhotonView>(out photonView))
			{
				return photonView;
			}
			return null;
		}
	}

	[SerializeField]
	private XSceneRef reference;
}
