using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class GTSignalListener : MonoBehaviour
{
	public int rigActorID { get; private set; } = -1;

	private void Awake()
	{
		this.OnListenerAwake();
	}

	private void OnEnable()
	{
		this.RefreshActorID();
		this.OnListenerEnable();
		GTSignalRelay.Register(this);
	}

	private void OnDisable()
	{
		GTSignalRelay.Unregister(this);
		this.OnListenerDisable();
	}

	private void RefreshActorID()
	{
		this.rig = base.GetComponentInParent<VRRig>(true);
		int num;
		if (!(this.rig == null))
		{
			NetPlayer owningNetPlayer = this.rig.OwningNetPlayer;
			num = ((owningNetPlayer != null) ? owningNetPlayer.ActorNumber : (-1));
		}
		else
		{
			num = -1;
		}
		this.rigActorID = num;
	}

	public virtual bool IsReady()
	{
		return this._callLimits.CheckCallTime(Time.time);
	}

	protected virtual void OnListenerAwake()
	{
	}

	protected virtual void OnListenerEnable()
	{
	}

	protected virtual void OnListenerDisable()
	{
	}

	public virtual void HandleSignalReceived(int sender, object[] args)
	{
	}

	[Space]
	public GTSignalID signal;

	[Space]
	public VRRig rig;

	[Space]
	public bool deafen;

	[FormerlySerializedAs("listenToRigOnly")]
	public bool listenToSelfOnly;

	public bool ignoreSelf;

	[Space]
	public bool callUnityEvent = true;

	[Space]
	[SerializeField]
	private CallLimiter _callLimits = new CallLimiter(10, 0.25f, 0.5f);

	[Space]
	public UnityEvent onSignalReceived;
}
