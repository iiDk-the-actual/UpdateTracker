using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GorillaScoreCounter : MonoBehaviour
{
	private void Awake()
	{
		this.text = base.gameObject.GetComponent<Text>();
		if (this.isRedTeam)
		{
			this.attribute = "redScore";
			return;
		}
		this.attribute = "blueScore";
	}

	private void Update()
	{
		if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties[this.attribute] != null)
		{
			this.text.text = ((int)PhotonNetwork.CurrentRoom.CustomProperties[this.attribute]).ToString();
		}
	}

	public bool isRedTeam;

	public Text text;

	public string attribute;
}
