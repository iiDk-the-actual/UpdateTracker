using System;
using UnityEngine;

public class SafeOwnershipRequestsCallbacks : MonoBehaviour, IRequestableOwnershipGuardCallbacks
{
	private void Awake()
	{
		this._requestableOwnershipGuard.AddCallbackTarget(this);
	}

	void IRequestableOwnershipGuardCallbacks.OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer)
	{
	}

	bool IRequestableOwnershipGuardCallbacks.OnOwnershipRequest(NetPlayer fromPlayer)
	{
		return false;
	}

	void IRequestableOwnershipGuardCallbacks.OnMyOwnerLeft()
	{
	}

	bool IRequestableOwnershipGuardCallbacks.OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer)
	{
		return false;
	}

	void IRequestableOwnershipGuardCallbacks.OnMyCreatorLeft()
	{
	}

	[SerializeField]
	private RequestableOwnershipGuard _requestableOwnershipGuard;
}
