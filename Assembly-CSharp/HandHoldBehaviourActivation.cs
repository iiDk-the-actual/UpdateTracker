using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HandHoldBehaviourActivation : Tappable
{
	protected override void OnEnable()
	{
		base.OnEnable();
		RoomSystem.PlayerLeftEvent += new Action<NetPlayer>(this.OnPlayerLeftRoom);
		RoomSystem.LeftRoomEvent += new Action(this.OnLeftRoom);
	}

	public override void OnGrabLocal(float tapTime, PhotonMessageInfoWrapped sender)
	{
		byte b = this.m_playerGrabCounts.GetValueOrDefault(sender.Sender.ActorNumber, 0);
		b += 1;
		if (b > 2)
		{
			return;
		}
		this.m_playerGrabCounts[sender.Sender.ActorNumber] = b;
		this.grabs++;
		if (this.grabs < 2)
		{
			this.ActivationStart.Invoke();
		}
	}

	public override void OnReleaseLocal(float tapTime, PhotonMessageInfoWrapped sender)
	{
		byte b;
		if (!this.m_playerGrabCounts.TryGetValue(sender.Sender.ActorNumber, out b) || b < 1)
		{
			return;
		}
		b -= 1;
		this.m_playerGrabCounts[sender.Sender.ActorNumber] = b;
		bool flag = this.grabs > 0;
		this.grabs = Mathf.Max(0, this.grabs - 1);
		if (flag && this.grabs < 1)
		{
			this.ActivationStop.Invoke();
		}
	}

	private void OnPlayerLeftRoom(NetPlayer player)
	{
		byte b;
		if (!this.m_playerGrabCounts.TryGetValue(player.ActorNumber, out b))
		{
			return;
		}
		bool flag = this.grabs > 0;
		this.grabs = Mathf.Max(0, this.grabs - (int)b);
		this.m_playerGrabCounts.Remove(player.ActorNumber);
		if (flag && this.grabs < 1)
		{
			this.ActivationStop.Invoke();
		}
	}

	private void OnLeftRoom()
	{
		byte valueOrDefault = this.m_playerGrabCounts.GetValueOrDefault(NetworkSystem.Instance.LocalPlayer.ActorNumber, 0);
		if (this.grabs > 0 && valueOrDefault < 1)
		{
			this.ActivationStop.Invoke();
		}
		this.grabs = (int)valueOrDefault;
		this.m_playerGrabCounts.Clear();
		this.m_playerGrabCounts[NetworkSystem.Instance.LocalPlayer.ActorNumber] = valueOrDefault;
	}

	[SerializeField]
	private UnityEvent ActivationStart;

	[SerializeField]
	private UnityEvent ActivationStop;

	private int grabs;

	private readonly Dictionary<int, byte> m_playerGrabCounts = new Dictionary<int, byte>(10);
}
