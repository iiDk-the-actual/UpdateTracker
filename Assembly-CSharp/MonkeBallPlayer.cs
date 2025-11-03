using System;
using UnityEngine;

public class MonkeBallPlayer : MonoBehaviour
{
	private void Awake()
	{
		if (this.gamePlayer == null)
		{
			this.gamePlayer = base.GetComponent<GameBallPlayer>();
		}
	}

	public GameBallPlayer gamePlayer;

	public MonkeBallGoalZone currGoalZone;
}
