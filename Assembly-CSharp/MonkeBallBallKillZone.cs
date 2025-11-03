using System;
using Photon.Pun;
using UnityEngine;

public class MonkeBallBallKillZone : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		GameBall component = other.transform.GetComponent<GameBall>();
		if (component != null)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				MonkeBallGame.Instance.RequestResetBall(component.id, -1);
				return;
			}
			GameBallManager.Instance.RequestSetBallPosition(component.id);
		}
	}
}
