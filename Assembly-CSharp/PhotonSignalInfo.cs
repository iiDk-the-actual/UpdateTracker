using System;
using Photon.Pun;

[Serializable]
public struct PhotonSignalInfo
{
	public PhotonSignalInfo(NetPlayer sender, int timestamp)
	{
		this.sender = sender;
		this.timestamp = timestamp;
	}

	public double sentServerTime
	{
		get
		{
			return this.timestamp / 1000.0;
		}
	}

	public override string ToString()
	{
		return string.Format("[{0}: Sender = '{1}' sentTime = {2}]", "PhotonSignalInfo", this.sender.ActorNumber, this.sentServerTime);
	}

	public static implicit operator PhotonMessageInfo(PhotonSignalInfo psi)
	{
		return new PhotonMessageInfo(psi.sender.GetPlayerRef(), psi.timestamp, null);
	}

	public readonly int timestamp;

	public readonly NetPlayer sender;
}
