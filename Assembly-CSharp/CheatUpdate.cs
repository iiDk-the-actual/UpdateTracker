using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;

public class CheatUpdate : MonoBehaviour
{
	private void Start()
	{
		base.StartCoroutine(this.UpdateNumberOfPlayers());
	}

	public IEnumerator UpdateNumberOfPlayers()
	{
		for (;;)
		{
			base.StartCoroutine(this.UpdatePlayerCount());
			yield return new WaitForSeconds(10f);
		}
		yield break;
	}

	private IEnumerator UpdatePlayerCount()
	{
		WWWForm wwwform = new WWWForm();
		wwwform.AddField("player_count", PhotonNetwork.CountOfPlayers - 1);
		wwwform.AddField("game_version", "live");
		wwwform.AddField("game_name", Application.productName);
		Debug.Log(PhotonNetwork.CountOfPlayers - 1);
		using (UnityWebRequest www = UnityWebRequest.Post("http://ntsfranz.crabdance.com/update_monke_count", wwwform))
		{
			yield return www.SendWebRequest();
			if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.Log(www.error);
			}
		}
		UnityWebRequest www = null;
		yield break;
		yield break;
	}
}
