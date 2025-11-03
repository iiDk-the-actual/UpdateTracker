using System;
using Photon.Pun;
using UnityEngine;

public class PhotonViewCache : MonoBehaviour, IPunInstantiateMagicCallback
{
	public bool Initialized { get; private set; }

	void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
	{
	}

	private PhotonView[] m_photonViews;

	[SerializeField]
	private bool m_isRoomObject;
}
