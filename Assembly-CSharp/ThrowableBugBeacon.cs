using System;
using UnityEngine;

public class ThrowableBugBeacon : MonoBehaviour
{
	public static event ThrowableBugBeacon.ThrowableBugBeaconEvent OnCall;

	public static event ThrowableBugBeacon.ThrowableBugBeaconEvent OnDismiss;

	public static event ThrowableBugBeacon.ThrowableBugBeaconEvent OnLock;

	public static event ThrowableBugBeacon.ThrowableBugBeaconEvent OnUnlock;

	public static event ThrowableBugBeacon.ThrowableBugBeaconFloatEvent OnChangeSpeedMultiplier;

	public ThrowableBug.BugName BugName
	{
		get
		{
			return this.bugName;
		}
	}

	public float Range
	{
		get
		{
			return this.range;
		}
	}

	public void Call()
	{
		if (ThrowableBugBeacon.OnCall != null)
		{
			ThrowableBugBeacon.OnCall(this);
		}
	}

	public void Dismiss()
	{
		if (ThrowableBugBeacon.OnDismiss != null)
		{
			ThrowableBugBeacon.OnDismiss(this);
		}
	}

	public void Lock()
	{
		if (ThrowableBugBeacon.OnLock != null)
		{
			ThrowableBugBeacon.OnLock(this);
		}
	}

	public void Unlock()
	{
		if (ThrowableBugBeacon.OnUnlock != null)
		{
			ThrowableBugBeacon.OnUnlock(this);
		}
	}

	public void ChangeSpeedMultiplier(float f)
	{
		if (ThrowableBugBeacon.OnChangeSpeedMultiplier != null)
		{
			ThrowableBugBeacon.OnChangeSpeedMultiplier(this, f);
		}
	}

	private void OnDisable()
	{
		if (ThrowableBugBeacon.OnUnlock != null)
		{
			ThrowableBugBeacon.OnUnlock(this);
		}
	}

	[SerializeField]
	private float range;

	[SerializeField]
	private ThrowableBug.BugName bugName;

	public delegate void ThrowableBugBeaconEvent(ThrowableBugBeacon tbb);

	public delegate void ThrowableBugBeaconFloatEvent(ThrowableBugBeacon tbb, float f);
}
