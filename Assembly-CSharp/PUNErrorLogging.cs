using System;
using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using PlayFab;
using UnityEngine;

public class PUNErrorLogging : MonoBehaviour
{
	private void Start()
	{
		PhotonNetwork.InternalEventError = (Action<EventData, Exception>)Delegate.Combine(PhotonNetwork.InternalEventError, new Action<EventData, Exception>(this.PUNError));
		PlayFabTitleDataCache.Instance.GetTitleData("PUNErrorLogging", delegate(string data)
		{
			int num;
			if (!int.TryParse(data, out num))
			{
				return;
			}
			PUNErrorLogging.LogFlags logFlags = (PUNErrorLogging.LogFlags)num;
			this.m_logSerializeView = logFlags.HasFlag(PUNErrorLogging.LogFlags.SerializeView);
			this.m_logOwnershipTransfer = logFlags.HasFlag(PUNErrorLogging.LogFlags.OwnershipTransfer);
			this.m_logOwnershipRequest = logFlags.HasFlag(PUNErrorLogging.LogFlags.OwnershipRequest);
			this.m_logOwnershipUpdate = logFlags.HasFlag(PUNErrorLogging.LogFlags.OwnershipUpdate);
			this.m_logRPC = logFlags.HasFlag(PUNErrorLogging.LogFlags.RPC);
			this.m_logInstantiate = logFlags.HasFlag(PUNErrorLogging.LogFlags.Instantiate);
			this.m_logDestroy = logFlags.HasFlag(PUNErrorLogging.LogFlags.Destroy);
			this.m_logDestroyPlayer = logFlags.HasFlag(PUNErrorLogging.LogFlags.DestroyPlayer);
		}, delegate(PlayFabError error)
		{
		}, false);
	}

	private void PUNError(EventData data, Exception exception)
	{
		NetworkSystem.Instance.GetPlayer(data.Sender);
		byte code = data.Code;
		switch (code)
		{
		case 200:
			this.PrintException(exception, this.m_logRPC);
			return;
		case 201:
		case 206:
			this.PrintException(exception, this.m_logSerializeView);
			return;
		case 202:
			this.PrintException(exception, this.m_logInstantiate);
			return;
		case 203:
		case 205:
		case 208:
		case 211:
			break;
		case 204:
			this.PrintException(exception, this.m_logDestroy);
			return;
		case 207:
			this.PrintException(exception, this.m_logDestroyPlayer);
			return;
		case 209:
			this.PrintException(exception, this.m_logOwnershipRequest);
			return;
		case 210:
			this.PrintException(exception, this.m_logOwnershipTransfer);
			return;
		case 212:
			this.PrintException(exception, this.m_logOwnershipUpdate);
			return;
		default:
			if (code == 254)
			{
				this.PrintException(exception, true);
				return;
			}
			break;
		}
		this.PrintException(exception, true);
	}

	private void PrintException(Exception e, bool print)
	{
		if (print)
		{
			Debug.LogException(e);
		}
	}

	[SerializeField]
	private bool m_logSerializeView = true;

	[SerializeField]
	private bool m_logOwnershipTransfer = true;

	[SerializeField]
	private bool m_logOwnershipRequest = true;

	[SerializeField]
	private bool m_logOwnershipUpdate = true;

	[SerializeField]
	private bool m_logRPC = true;

	[SerializeField]
	private bool m_logInstantiate = true;

	[SerializeField]
	private bool m_logDestroy = true;

	[SerializeField]
	private bool m_logDestroyPlayer = true;

	[Flags]
	private enum LogFlags
	{
		SerializeView = 1,
		OwnershipTransfer = 2,
		OwnershipRequest = 4,
		OwnershipUpdate = 8,
		RPC = 16,
		Instantiate = 32,
		Destroy = 64,
		DestroyPlayer = 128
	}
}
