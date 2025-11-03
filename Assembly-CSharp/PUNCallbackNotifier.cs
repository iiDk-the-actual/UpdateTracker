using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PUNCallbackNotifier : MonoBehaviourPunCallbacks, IOnEventCallback
{
	private void Start()
	{
		this.parentSystem = base.GetComponent<NetworkSystemPUN>();
	}

	private void Update()
	{
	}

	public override void OnConnectedToMaster()
	{
		this.parentSystem.OnConnectedtoMaster();
	}

	public override void OnJoinedRoom()
	{
		this.parentSystem.OnJoinedRoom();
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		this.parentSystem.OnJoinRoomFailed(returnCode, message);
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		this.parentSystem.OnJoinRoomFailed(returnCode, message);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		this.parentSystem.OnCreateRoomFailed(returnCode, message);
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		this.parentSystem.OnPlayerEnteredRoom(newPlayer);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		this.parentSystem.OnPlayerLeftRoom(otherPlayer);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("Disconnect callback, cause:" + cause.ToString());
		this.parentSystem.OnDisconnected(cause);
	}

	public void OnEvent(EventData photonEvent)
	{
		this.parentSystem.RaiseEvent(photonEvent.Code, photonEvent.CustomData, photonEvent.Sender);
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		this.parentSystem.OnMasterClientSwitched(newMasterClient);
	}

	public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		base.OnCustomAuthenticationResponse(data);
		NetworkSystem.Instance.CustomAuthenticationResponse(data);
	}

	private NetworkSystemPUN parentSystem;
}
