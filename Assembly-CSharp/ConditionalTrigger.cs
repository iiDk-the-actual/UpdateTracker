using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ConditionalTrigger : MonoBehaviour, IRigAware
{
	private int intValue
	{
		get
		{
			return (int)this._tracking;
		}
	}

	public void SetProximityFromRig()
	{
		if (this._rig.AsNull<VRRig>() == null)
		{
			ConditionalTrigger.FindRig(out this._rig);
		}
		if (this._rig)
		{
			this._from = this._rig.transform;
		}
	}

	public void SetProximityToRig()
	{
		if (this._rig.AsNull<VRRig>() == null)
		{
			ConditionalTrigger.FindRig(out this._rig);
		}
		if (this._rig)
		{
			this._to = this._rig.transform;
		}
	}

	public void SetProximityFrom(Transform from)
	{
		this._from = from;
	}

	public void SetProxmityTo(Transform to)
	{
		this._to = to;
	}

	public void TrackedSet(TriggerCondition conditions)
	{
		this._tracking = conditions;
	}

	public void TrackedAdd(TriggerCondition conditions)
	{
		this._tracking |= conditions;
	}

	public void TrackedRemove(TriggerCondition conditions)
	{
		this._tracking &= ~conditions;
	}

	public void TrackedSet(int conditions)
	{
		this._tracking = (TriggerCondition)conditions;
	}

	public void TrackedAdd(int conditions)
	{
		this._tracking |= (TriggerCondition)conditions;
	}

	public void TrackedRemove(int conditions)
	{
		this._tracking &= (TriggerCondition)(~(TriggerCondition)conditions);
	}

	public void TrackedClear()
	{
		this._tracking = TriggerCondition.None;
	}

	private void OnEnable()
	{
		this._timeSince = 0f;
	}

	private void Update()
	{
		if (this.IsTracking(TriggerCondition.TimeElapsed))
		{
			this.TrackTimeElapsed();
		}
		if (this.IsTracking(TriggerCondition.Proximity))
		{
			this.TrackProximity();
			return;
		}
		this._distance = 0f;
	}

	private void TrackTimeElapsed()
	{
		if (this._timeSince.HasElapsed(this._interval, true))
		{
			UnityEvent unityEvent = this.onTimeElapsed;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}
	}

	private void TrackProximity()
	{
		if (!this._from || !this._to)
		{
			this._distance = 0f;
			return;
		}
		this._distance = Vector3.Distance(this._to.position, this._from.position);
		if (this._distance >= this._maxDistance)
		{
			UnityEvent unityEvent = this.onMaxDistance;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}
	}

	private bool IsTracking(TriggerCondition condition)
	{
		return (this._tracking & condition) == condition;
	}

	private static void FindRig(out VRRig rig)
	{
		if (PhotonNetwork.InRoom)
		{
			rig = GorillaGameManager.StaticFindRigForPlayer(NetPlayer.Get(PhotonNetwork.LocalPlayer));
			return;
		}
		rig = VRRig.LocalRig;
	}

	public void SetRig(VRRig rig)
	{
		this._rig = rig;
	}

	[Space]
	[SerializeField]
	private TriggerCondition _tracking;

	[Space]
	[SerializeField]
	private Transform _from;

	[SerializeField]
	private Transform _to;

	[SerializeField]
	private float _maxDistance;

	[NonSerialized]
	private float _distance;

	[Space]
	public UnityEvent onMaxDistance;

	[SerializeField]
	private float _interval = 1f;

	[NonSerialized]
	private TimeSince _timeSince;

	[Space]
	public UnityEvent onTimeElapsed;

	[Space]
	private VRRig _rig;
}
