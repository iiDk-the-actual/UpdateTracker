using System;
using UnityEngine;

public class GorillaTagCompetitivePrivateRoomBlocker : MonoBehaviour
{
	private void Update()
	{
		this.blocker.SetActive(NetworkSystem.Instance.SessionIsPrivate);
	}

	[SerializeField]
	private GameObject blocker;
}
