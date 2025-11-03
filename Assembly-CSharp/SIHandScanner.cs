using System;
using UnityEngine;
using UnityEngine.Events;

public class SIHandScanner : MonoBehaviour
{
	public void HandScanned(SIPlayer scannedPlayer)
	{
		if (!scannedPlayer.gamePlayer.IsLocal())
		{
			return;
		}
		this.onHandScanned.Invoke(NetworkSystem.Instance.LocalPlayerID);
	}

	public UnityEvent<int> onHandScanned;
}
