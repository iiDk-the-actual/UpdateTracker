using System;
using Photon.Pun;
using Photon.Realtime;

[Serializable]
public class PhotonSignal<T1> : PhotonSignal
{
	public override int argCount
	{
		get
		{
			return 1;
		}
	}

	public new event OnSignalReceived<T1> OnSignal
	{
		add
		{
			if (value == null)
			{
				return;
			}
			this._callbacks = (OnSignalReceived<T1>)Delegate.Remove(this._callbacks, value);
			this._callbacks = (OnSignalReceived<T1>)Delegate.Combine(this._callbacks, value);
		}
		remove
		{
			if (value == null)
			{
				return;
			}
			this._callbacks = (OnSignalReceived<T1>)Delegate.Remove(this._callbacks, value);
		}
	}

	public PhotonSignal(string signalID)
		: base(signalID)
	{
	}

	public PhotonSignal(int signalID)
		: base(signalID)
	{
	}

	public override void ClearListeners()
	{
		this._callbacks = null;
		base.ClearListeners();
	}

	public void Raise(T1 arg1)
	{
		this.Raise(this._receivers, arg1);
	}

	public void Raise(ReceiverGroup receivers, T1 arg1)
	{
		if (!this._enabled)
		{
			return;
		}
		if (this._mute)
		{
			return;
		}
		RaiseEventOptions raiseEventOptions = PhotonSignal.gGroupToOptions[receivers];
		object[] array = PhotonUtils.FetchScratchArray(2 + this.argCount);
		int serverTimestamp = PhotonNetwork.ServerTimestamp;
		array[0] = this._signalID;
		array[1] = serverTimestamp;
		array[2] = arg1;
		if (this._localOnly || !PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
		{
			PhotonSignalInfo photonSignalInfo = new PhotonSignalInfo(PhotonUtils.LocalNetPlayer, serverTimestamp);
			this._Relay(array, photonSignalInfo);
			return;
		}
		PhotonNetwork.RaiseEvent(177, array, raiseEventOptions, PhotonSignal.gSendReliable);
	}

	protected override void _Relay(object[] args, PhotonSignalInfo info)
	{
		T1 t;
		if (!args.TryParseArgs(2, out t))
		{
			return;
		}
		if (!this._safeInvoke)
		{
			PhotonSignal._Invoke<T1>(this._callbacks, t, info);
			return;
		}
		PhotonSignal._SafeInvoke<T1>(this._callbacks, t, info);
	}

	public new static implicit operator PhotonSignal<T1>(string s)
	{
		return new PhotonSignal<T1>(s);
	}

	public new static explicit operator PhotonSignal<T1>(int i)
	{
		return new PhotonSignal<T1>(i);
	}

	private OnSignalReceived<T1> _callbacks;

	private static readonly int kSignature = typeof(PhotonSignal<T1>).FullName.GetStaticHash();
}
