using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class GorillaPlayerCounter : MonoBehaviour
{
	private void Awake()
	{
		this.text = base.gameObject.GetComponent<Text>();
	}

	private void Update()
	{
		if (PhotonNetwork.CurrentRoom != null)
		{
			int num = 0;
			foreach (KeyValuePair<int, Player> keyValuePair in PhotonNetwork.CurrentRoom.Players)
			{
				if ((bool)keyValuePair.Value.CustomProperties["isRedTeam"] == this.isRedTeam)
				{
					num++;
				}
			}
			this.text.text = num.ToString();
		}
	}

	public bool isRedTeam;

	public Text text;

	public string attribute;
}
