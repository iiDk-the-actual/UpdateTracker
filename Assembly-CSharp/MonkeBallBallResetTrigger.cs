using System;
using Photon.Pun;
using UnityEngine;

public class MonkeBallBallResetTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		GameBall component = other.transform.GetComponent<GameBall>();
		if (component != null)
		{
			GameBallPlayer gameBallPlayer = ((component.heldByActorNumber < 0) ? null : GameBallPlayer.GetGamePlayer(component.heldByActorNumber));
			if (gameBallPlayer == null)
			{
				gameBallPlayer = ((component.lastHeldByActorNumber < 0) ? null : GameBallPlayer.GetGamePlayer(component.lastHeldByActorNumber));
				if (gameBallPlayer == null)
				{
					return;
				}
			}
			this._lastBall = component;
			int num = gameBallPlayer.teamId;
			if (num == -1)
			{
				num = component.lastHeldByTeamId;
			}
			if (num >= 0 && num < this.teamMaterials.Length)
			{
				this.trigger.sharedMaterial = this.teamMaterials[num];
			}
			if (PhotonNetwork.IsMasterClient)
			{
				MonkeBallGame.Instance.ToggleResetButton(true, num);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		GameBall component = other.transform.GetComponent<GameBall>();
		if (component != null)
		{
			if (component == this._lastBall)
			{
				this.trigger.sharedMaterial = this.neutralMaterial;
				this._lastBall = null;
			}
			if (PhotonNetwork.IsMasterClient)
			{
				MonkeBallGame.Instance.ToggleResetButton(false, -1);
			}
		}
	}

	public Renderer trigger;

	public Material[] teamMaterials;

	public Material neutralMaterial;

	private GameBall _lastBall;
}
