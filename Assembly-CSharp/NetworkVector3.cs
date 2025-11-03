using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

internal class NetworkVector3
{
	public Vector3 CurrentSyncTarget
	{
		get
		{
			return this._currentSyncTarget;
		}
	}

	public void SetNewSyncTarget(Vector3 newTarget)
	{
		Vector3 currentSyncTarget = this.CurrentSyncTarget;
		(ref currentSyncTarget).SetValueSafe(in newTarget);
		this.distanceTraveled = currentSyncTarget - this._currentSyncTarget;
		this._currentSyncTarget = currentSyncTarget;
		this.lastSetNetTime = PhotonNetwork.Time;
	}

	public Vector3 GetPredictedFuture()
	{
		float num = (float)(PhotonNetwork.Time - this.lastSetNetTime) * (float)PhotonNetwork.SerializationRate;
		Vector3 vector = this.distanceTraveled * num;
		return this._currentSyncTarget + vector;
	}

	public void Reset()
	{
		this._currentSyncTarget = Vector3.zero;
		this.distanceTraveled = Vector3.zero;
		this.lastSetNetTime = 0.0;
	}

	private double lastSetNetTime;

	private Vector3 _currentSyncTarget = Vector3.zero;

	private Vector3 distanceTraveled = Vector3.zero;
}
