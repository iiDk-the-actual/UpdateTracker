using System;
using UnityEngine;

public class RoomStateVisibility : MonoBehaviour
{
	private void Start()
	{
		this.OnRoomChanged();
		RoomSystem.JoinedRoomEvent += new Action(this.OnRoomChanged);
		RoomSystem.LeftRoomEvent += new Action(this.OnRoomChanged);
	}

	private void OnDestroy()
	{
		RoomSystem.JoinedRoomEvent -= new Action(this.OnRoomChanged);
		RoomSystem.LeftRoomEvent -= new Action(this.OnRoomChanged);
	}

	private void OnRoomChanged()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			base.gameObject.SetActive(this.enableOutOfRoom);
			return;
		}
		if (NetworkSystem.Instance.SessionIsPrivate)
		{
			base.gameObject.SetActive(this.enableInPrivateRoom);
			return;
		}
		base.gameObject.SetActive(this.enableInRoom);
	}

	[SerializeField]
	private bool enableOutOfRoom;

	[SerializeField]
	private bool enableInRoom = true;

	[SerializeField]
	private bool enableInPrivateRoom = true;
}
