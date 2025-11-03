using System;
using GorillaExtensions;
using GorillaTag;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UseableObjectEvents))]
public class UseableObject : TransferrableObject
{
	public bool isMidUse
	{
		get
		{
			return this._isMidUse;
		}
	}

	public float useTimeElapsed
	{
		get
		{
			return this._useTimeElapsed;
		}
	}

	public bool justUsed
	{
		get
		{
			if (!this._justUsed)
			{
				return false;
			}
			this._justUsed = false;
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this._events = base.gameObject.GetOrAddComponent<UseableObjectEvents>();
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		UseableObjectEvents events = this._events;
		VRRig myOnlineRig = base.myOnlineRig;
		NetPlayer netPlayer;
		if ((netPlayer = ((myOnlineRig != null) ? myOnlineRig.creator : null)) == null)
		{
			VRRig myRig = base.myRig;
			netPlayer = ((myRig != null) ? myRig.creator : null);
		}
		events.Init(netPlayer);
		this._events.Activate += this.OnObjectActivated;
		this._events.Deactivate += this.OnObjectDeactivated;
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		Object.Destroy(this._events);
	}

	private void OnObjectActivated(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
	{
	}

	private void OnObjectDeactivated(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
	{
	}

	public override void TriggeredLateUpdate()
	{
		base.TriggeredLateUpdate();
		if (this._isMidUse)
		{
			this._useTimeElapsed += Time.deltaTime;
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		if (this.IsMyItem())
		{
			UnityEvent unityEvent = this.onActivateLocal;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this._useTimeElapsed = 0f;
			this._isMidUse = true;
		}
		if (this._raiseActivate)
		{
			UseableObjectEvents events = this._events;
			if (events == null)
			{
				return;
			}
			PhotonEvent activate = events.Activate;
			if (activate == null)
			{
				return;
			}
			activate.RaiseAll(Array.Empty<object>());
		}
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		if (this.IsMyItem())
		{
			UnityEvent unityEvent = this.onDeactivateLocal;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this._isMidUse = false;
			this._justUsed = true;
		}
		if (this._raiseDeactivate)
		{
			UseableObjectEvents events = this._events;
			if (events == null)
			{
				return;
			}
			PhotonEvent deactivate = events.Deactivate;
			if (deactivate == null)
			{
				return;
			}
			deactivate.RaiseAll(Array.Empty<object>());
		}
	}

	public override bool CanActivate()
	{
		return !this.disableActivation;
	}

	public override bool CanDeactivate()
	{
		return !this.disableDeactivation;
	}

	[DebugOption]
	public bool disableActivation;

	[DebugOption]
	public bool disableDeactivation;

	[SerializeField]
	private UseableObjectEvents _events;

	[SerializeField]
	private bool _raiseActivate = true;

	[SerializeField]
	private bool _raiseDeactivate = true;

	[NonSerialized]
	private DateTime _lastActivate;

	[NonSerialized]
	private DateTime _lastDeactivate;

	[NonSerialized]
	private bool _isMidUse;

	[NonSerialized]
	private float _useTimeElapsed;

	[NonSerialized]
	private bool _justUsed;

	[NonSerialized]
	private int tempHandPos;

	public UnityEvent onActivateLocal;

	public UnityEvent onDeactivateLocal;
}
