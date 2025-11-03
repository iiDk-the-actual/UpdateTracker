using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaTag;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[Serializable]
public class PhotonEvent : IEquatable<PhotonEvent>
{
	public bool reliable
	{
		get
		{
			return this._reliable;
		}
		set
		{
			this._reliable = value;
		}
	}

	public bool failSilent
	{
		get
		{
			return this._failSilent;
		}
		set
		{
			this._failSilent = value;
		}
	}

	private PhotonEvent()
	{
	}

	public PhotonEvent(int eventId)
	{
		if (eventId == -1)
		{
			throw new Exception(string.Format("<{0}> cannot be {1}.", "eventId", -1));
		}
		this._eventId = eventId;
		this.Enable();
	}

	public PhotonEvent(string eventId)
		: this(StaticHash.Compute(eventId))
	{
	}

	public PhotonEvent(int eventId, Action<int, int, object[], PhotonMessageInfoWrapped> callback)
		: this(eventId)
	{
		this.AddCallback(callback);
	}

	public PhotonEvent(string eventId, Action<int, int, object[], PhotonMessageInfoWrapped> callback)
		: this(eventId)
	{
		this.AddCallback(callback);
	}

	~PhotonEvent()
	{
		this.Dispose();
	}

	public void AddCallback(Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (this._disposed)
		{
			return;
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (this._delegate != null)
		{
			foreach (Delegate @delegate in this._delegate.GetInvocationList())
			{
				if (@delegate != null && @delegate.Equals(callback))
				{
					return;
				}
			}
		}
		this._delegate = (Action<int, int, object[], PhotonMessageInfoWrapped>)Delegate.Combine(this._delegate, callback);
	}

	public void RemoveCallback(Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (this._disposed)
		{
			return;
		}
		if (callback != null)
		{
			this._delegate = (Action<int, int, object[], PhotonMessageInfoWrapped>)Delegate.Remove(this._delegate, callback);
		}
	}

	public void Enable()
	{
		if (this._disposed)
		{
			return;
		}
		if (this._enabled)
		{
			return;
		}
		if (Application.isPlaying)
		{
			PhotonEvent.AddPhotonEvent(this);
		}
		this._enabled = true;
	}

	public void Disable()
	{
		if (this._disposed)
		{
			return;
		}
		if (!this._enabled)
		{
			return;
		}
		if (Application.isPlaying)
		{
			PhotonEvent.RemovePhotonEvent(this);
		}
		this._enabled = false;
	}

	public void Dispose()
	{
		this._delegate = null;
		if (this._enabled)
		{
			this._enabled = false;
			if (Application.isPlaying)
			{
				PhotonEvent.RemovePhotonEvent(this);
			}
		}
		this._eventId = -1;
		this._disposed = true;
	}

	public static event Action<EventData, Exception> OnError;

	private void InvokeDelegate(int sender, object[] args, PhotonMessageInfoWrapped info)
	{
		Action<int, int, object[], PhotonMessageInfoWrapped> @delegate = this._delegate;
		if (@delegate == null)
		{
			return;
		}
		@delegate(sender, this._eventId, args, info);
	}

	public void RaiseLocal(params object[] args)
	{
		this.Raise(PhotonEvent.RaiseMode.Local, args);
	}

	public void RaiseOthers(params object[] args)
	{
		this.Raise(PhotonEvent.RaiseMode.RemoteOthers, args);
	}

	public void RaiseAll(params object[] args)
	{
		this.Raise(PhotonEvent.RaiseMode.RemoteAll, args);
	}

	private void Raise(PhotonEvent.RaiseMode mode, params object[] args)
	{
		if (this._disposed)
		{
			return;
		}
		if (!Application.isPlaying)
		{
			return;
		}
		if (!this._enabled)
		{
			return;
		}
		if (args != null && args.Length > 20)
		{
			Debug.LogError(string.Format("{0}: too many event args, max is {1}, trying to send {2}. Stopping!", "PhotonEvent", 20, args.Length));
			return;
		}
		SendOptions sendOptions = (this._reliable ? PhotonEvent.gSendReliable : PhotonEvent.gSendUnreliable);
		switch (mode)
		{
		case PhotonEvent.RaiseMode.Local:
			this.InvokeDelegate(this._eventId, args, new PhotonMessageInfoWrapped(PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.ServerTimestamp));
			return;
		case PhotonEvent.RaiseMode.RemoteOthers:
		{
			object[] array = args.Prepend(this._eventId).ToArray<object>();
			PhotonNetwork.RaiseEvent(176, array, PhotonEvent.gReceiversOthers, sendOptions);
			return;
		}
		case PhotonEvent.RaiseMode.RemoteAll:
		{
			object[] array2 = args.Prepend(this._eventId).ToArray<object>();
			PhotonNetwork.RaiseEvent(176, array2, PhotonEvent.gReceiversAll, sendOptions);
			return;
		}
		default:
			return;
		}
	}

	public bool Equals(PhotonEvent other)
	{
		return !(other == null) && (this._eventId == other._eventId && this._enabled == other._enabled && this._reliable == other._reliable && this._failSilent == other._failSilent) && this._disposed == other._disposed;
	}

	public override bool Equals(object obj)
	{
		PhotonEvent photonEvent = obj as PhotonEvent;
		return photonEvent != null && this.Equals(photonEvent);
	}

	public override int GetHashCode()
	{
		int staticHash = this._eventId.GetStaticHash();
		int num = StaticHash.Compute(this._enabled, this._reliable, this._failSilent, this._disposed);
		return StaticHash.Compute(staticHash, num);
	}

	static PhotonEvent()
	{
		PhotonEvent.gSendUnreliable.Encrypt = true;
		PhotonEvent.gSendReliable = SendOptions.SendReliable;
		PhotonEvent.gSendReliable.Encrypt = true;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void StaticLoadAfterPhotonNetwork()
	{
		PhotonNetwork.NetworkingClient.EventReceived += PhotonEvent.StaticOnEvent;
	}

	public static bool operator ==(PhotonEvent x, PhotonEvent y)
	{
		return EqualityComparer<PhotonEvent>.Default.Equals(x, y);
	}

	public static bool operator !=(PhotonEvent x, PhotonEvent y)
	{
		return !EqualityComparer<PhotonEvent>.Default.Equals(x, y);
	}

	private static void StaticOnEvent(EventData evData)
	{
		if (evData.Code != 176)
		{
			return;
		}
		try
		{
			object[] array = evData.CustomData as object[];
			if (array != null && array.Length != 0 && array.Length <= 21)
			{
				object obj = array[0];
				if (obj is int)
				{
					int sender = (int)obj;
					if (sender != -1)
					{
						ListProcessor<PhotonEvent> listProcessor;
						if (PhotonEvent._photonEvents.TryGetValue(sender, out listProcessor))
						{
							object[] args;
							if (array.Length > 1)
							{
								args = new object[array.Length - 1];
								Array.Copy(array, 1, args, 0, args.Length);
							}
							else
							{
								args = Array.Empty<object>();
							}
							PhotonMessageInfoWrapped info = new PhotonMessageInfoWrapped(evData.Sender, PhotonNetwork.ServerTimestamp);
							listProcessor.ItemProcessor = delegate(in PhotonEvent pEv)
							{
								if (pEv._eventId == -1 || pEv._disposed || !pEv._enabled)
								{
									return;
								}
								pEv.InvokeDelegate(sender, args, info);
							};
							listProcessor.ProcessList();
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Action<EventData, Exception> onError = PhotonEvent.OnError;
			if (onError != null)
			{
				onError(evData, ex);
			}
		}
	}

	private static void AddPhotonEvent(PhotonEvent photonEvent)
	{
		int eventId = photonEvent._eventId;
		if (eventId == -1)
		{
			return;
		}
		ListProcessor<PhotonEvent> listProcessor;
		if (!PhotonEvent._photonEvents.TryGetValue(eventId, out listProcessor))
		{
			listProcessor = new ListProcessor<PhotonEvent>(10, null);
			PhotonEvent._photonEvents.Add(eventId, listProcessor);
		}
		if (listProcessor.Contains(in photonEvent))
		{
			return;
		}
		listProcessor.Add(in photonEvent);
	}

	private static void RemovePhotonEvent(PhotonEvent photonEvent)
	{
		ListProcessor<PhotonEvent> listProcessor;
		if (!PhotonEvent._photonEvents.TryGetValue(photonEvent._eventId, out listProcessor))
		{
			return;
		}
		listProcessor.Remove(in photonEvent);
		if (listProcessor.Count == 0)
		{
			PhotonEvent._photonEvents.Remove(photonEvent._eventId);
		}
	}

	public static PhotonEvent operator +(PhotonEvent photonEvent, Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (photonEvent == null)
		{
			throw new ArgumentNullException("photonEvent");
		}
		photonEvent.AddCallback(callback);
		return photonEvent;
	}

	public static PhotonEvent operator -(PhotonEvent photonEvent, Action<int, int, object[], PhotonMessageInfoWrapped> callback)
	{
		if (photonEvent == null)
		{
			throw new ArgumentNullException("photonEvent");
		}
		photonEvent.RemoveCallback(callback);
		return photonEvent;
	}

	private const int MAX_EVENT_ARGS = 20;

	private const int INVALID_ID = -1;

	[SerializeField]
	private int _eventId = -1;

	[SerializeField]
	private bool _enabled;

	[SerializeField]
	private bool _reliable;

	[SerializeField]
	private bool _failSilent;

	[NonSerialized]
	private bool _disposed;

	private Action<int, int, object[], PhotonMessageInfoWrapped> _delegate;

	public const byte PHOTON_EVENT_CODE = 176;

	private static readonly RaiseEventOptions gReceiversAll = new RaiseEventOptions
	{
		Receivers = ReceiverGroup.All
	};

	private static readonly RaiseEventOptions gReceiversOthers = new RaiseEventOptions
	{
		Receivers = ReceiverGroup.Others
	};

	private static readonly SendOptions gSendReliable;

	private static readonly SendOptions gSendUnreliable = SendOptions.SendUnreliable;

	private static readonly Dictionary<int, ListProcessor<PhotonEvent>> _photonEvents = new Dictionary<int, ListProcessor<PhotonEvent>>(20);

	public enum RaiseMode
	{
		Local,
		RemoteOthers,
		RemoteAll
	}
}
