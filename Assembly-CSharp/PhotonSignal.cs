using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PhotonSignal
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5, T6, T7>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, arg6, arg7, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5, T6>(OnSignalReceived<T1, T2, T3, T4, T5, T6> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, arg6, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4, T5>(OnSignalReceived<T1, T2, T3, T4, T5> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, arg5, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3, T4>(OnSignalReceived<T1, T2, T3, T4> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, arg4, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2, T3>(OnSignalReceived<T1, T2, T3> _event, T1 arg1, T2 arg2, T3 arg3, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, arg3, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1, T2>(OnSignalReceived<T1, T2> _event, T1 arg1, T2 arg2, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, arg2, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke<T1>(OnSignalReceived<T1> _event, T1 arg1, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(arg1, info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _Invoke(OnSignalReceived _event, PhotonSignalInfo info)
	{
		if (_event != null)
		{
			_event(info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8, T9> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5, T6, T7, T8> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5, T6, T7>(OnSignalReceived<T1, T2, T3, T4, T5, T6, T7> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5, T6, T7>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5, T6, T7>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5, T6, T7> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, arg6, arg7, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5, T6>(OnSignalReceived<T1, T2, T3, T4, T5, T6> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5, T6>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5, T6>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5, T6> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, arg6, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4, T5>(OnSignalReceived<T1, T2, T3, T4, T5> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4, T5>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4, T5>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4, T5> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, arg5, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3, T4>(OnSignalReceived<T1, T2, T3, T4> _event, T1 arg1, T2 arg2, T3 arg3, T4 arg4, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3, T4>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3, T4>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3, T4> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, arg4, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2, T3>(OnSignalReceived<T1, T2, T3> _event, T1 arg1, T2 arg2, T3 arg3, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2, T3>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2, T3>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2, T3> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, arg3, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1, T2>(OnSignalReceived<T1, T2> _event, T1 arg1, T2 arg2, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1, T2>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1, T2>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1, T2> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, arg2, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke<T1>(OnSignalReceived<T1> _event, T1 arg1, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived<T1>[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived<T1>>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived<T1> onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(arg1, info);
				}
			}
			catch
			{
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static void _SafeInvoke(OnSignalReceived _event, PhotonSignalInfo info)
	{
		readonly ref OnSignalReceived[] ptr = ref PhotonUtils.FetchDelegatesNonAlloc<OnSignalReceived>(_event);
		for (int i = 0; i < ptr.Length; i++)
		{
			try
			{
				OnSignalReceived onSignalReceived = ptr[i];
				if (onSignalReceived != null)
				{
					onSignalReceived(info);
				}
			}
			catch
			{
			}
		}
	}

	public bool enabled
	{
		get
		{
			return this._enabled;
		}
	}

	public virtual int argCount
	{
		get
		{
			return 0;
		}
	}

	public event OnSignalReceived OnSignal
	{
		add
		{
			if (value == null)
			{
				return;
			}
			this._callbacks = (OnSignalReceived)Delegate.Remove(this._callbacks, value);
			this._callbacks = (OnSignalReceived)Delegate.Combine(this._callbacks, value);
		}
		remove
		{
			if (value == null)
			{
				return;
			}
			this._callbacks = (OnSignalReceived)Delegate.Remove(this._callbacks, value);
		}
	}

	protected PhotonSignal()
	{
		this._refID = PhotonSignal.RefID.Register(this);
	}

	public PhotonSignal(string signalID)
		: this()
	{
		signalID = ((signalID != null) ? signalID.Trim() : null);
		if (string.IsNullOrWhiteSpace(signalID))
		{
			throw new ArgumentNullException("signalID");
		}
		this._signalID = XXHash32.Compute(signalID, 0U);
	}

	public PhotonSignal(int signalID)
		: this()
	{
		this._signalID = signalID;
	}

	public void Raise()
	{
		this.Raise(this._receivers);
	}

	public void Raise(ReceiverGroup receivers)
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
		object[] array = PhotonUtils.FetchScratchArray(2);
		int serverTimestamp = PhotonNetwork.ServerTimestamp;
		array[0] = this._signalID;
		array[1] = serverTimestamp;
		if (this._localOnly || !PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
		{
			PhotonSignalInfo photonSignalInfo = new PhotonSignalInfo(PhotonUtils.LocalNetPlayer, serverTimestamp);
			this._Relay(array, photonSignalInfo);
			return;
		}
		PhotonNetwork.RaiseEvent(177, array, raiseEventOptions, PhotonSignal.gSendReliable);
	}

	public void Enable()
	{
		PhotonNetwork.NetworkingClient.EventReceived += this._EventHandle;
		this._enabled = true;
	}

	public void Disable()
	{
		this._enabled = false;
		PhotonNetwork.NetworkingClient.EventReceived -= this._EventHandle;
	}

	private void _EventHandle(EventData eventData)
	{
		if (!this._enabled)
		{
			return;
		}
		if (this._mute)
		{
			return;
		}
		if (eventData.Code != 177)
		{
			return;
		}
		int sender = eventData.Sender;
		object[] array = eventData.CustomData as object[];
		if (array == null)
		{
			return;
		}
		if (array.Length < 2 + this.argCount)
		{
			return;
		}
		object obj = array[0];
		if (!(obj is int))
		{
			return;
		}
		int num = (int)obj;
		if (num == 0 || num != this._signalID)
		{
			return;
		}
		obj = array[1];
		if (obj is int)
		{
			int num2 = (int)obj;
			NetPlayer netPlayer = PhotonUtils.GetNetPlayer(sender);
			PhotonSignalInfo photonSignalInfo = new PhotonSignalInfo(netPlayer, num2);
			this._Relay(array, photonSignalInfo);
			return;
		}
	}

	protected virtual void _Relay(object[] args, PhotonSignalInfo info)
	{
		if (!this._safeInvoke)
		{
			PhotonSignal._Invoke(this._callbacks, info);
			return;
		}
		PhotonSignal._SafeInvoke(this._callbacks, info);
	}

	public virtual void ClearListeners()
	{
		this._callbacks = null;
	}

	public virtual void Reset()
	{
		this.ClearListeners();
		this.Disable();
	}

	public virtual void Dispose()
	{
		this._signalID = 0;
		this.Reset();
	}

	~PhotonSignal()
	{
		this.Dispose();
	}

	public static implicit operator PhotonSignal(string s)
	{
		return new PhotonSignal(s);
	}

	public static explicit operator PhotonSignal(int i)
	{
		return new PhotonSignal(i);
	}

	static PhotonSignal()
	{
		Dictionary<ReceiverGroup, RaiseEventOptions> dictionary = new Dictionary<ReceiverGroup, RaiseEventOptions>();
		dictionary[ReceiverGroup.Others] = new RaiseEventOptions
		{
			Receivers = ReceiverGroup.Others
		};
		dictionary[ReceiverGroup.All] = new RaiseEventOptions
		{
			Receivers = ReceiverGroup.All
		};
		dictionary[ReceiverGroup.MasterClient] = new RaiseEventOptions
		{
			Receivers = ReceiverGroup.MasterClient
		};
		PhotonSignal.gGroupToOptions = dictionary;
		PhotonSignal.gSendReliable = SendOptions.SendReliable;
		PhotonSignal.gSendUnreliable = SendOptions.SendUnreliable;
		PhotonSignal.gSendReliable.Encrypt = true;
		PhotonSignal.gSendUnreliable.Encrypt = true;
	}

	protected int _signalID;

	protected bool _enabled;

	[SerializeField]
	protected ReceiverGroup _receivers = ReceiverGroup.All;

	[FormerlySerializedAs("mute")]
	[SerializeField]
	protected bool _mute;

	[SerializeField]
	protected bool _safeInvoke = true;

	[SerializeField]
	protected bool _localOnly;

	[NonSerialized]
	private int _refID;

	private OnSignalReceived _callbacks;

	protected static readonly Dictionary<ReceiverGroup, RaiseEventOptions> gGroupToOptions;

	protected static readonly SendOptions gSendReliable;

	protected static readonly SendOptions gSendUnreliable;

	public const byte EVENT_CODE = 177;

	public const int NULL_SIGNAL = 0;

	protected const int HEADER_SIZE = 2;

	private class RefID
	{
		public static int Count
		{
			get
			{
				return PhotonSignal.RefID.gRefCount;
			}
		}

		public RefID()
		{
			this.intValue = StaticHash.ComputeTriple32(PhotonSignal.RefID.gNextID++);
			PhotonSignal.RefID.gRefCount++;
		}

		~RefID()
		{
			PhotonSignal.RefID.gRefCount--;
		}

		public static int Register(PhotonSignal ps)
		{
			if (ps == null)
			{
				return 0;
			}
			PhotonSignal.RefID refID = new PhotonSignal.RefID();
			PhotonSignal.RefID.gRefTable.Add(ps, refID);
			return refID.intValue;
		}

		public int intValue;

		private static int gNextID = 1;

		private static int gRefCount = 0;

		private static readonly ConditionalWeakTable<PhotonSignal, PhotonSignal.RefID> gRefTable = new ConditionalWeakTable<PhotonSignal, PhotonSignal.RefID>();
	}
}
