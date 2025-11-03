using System;

public interface IRequestableOwnershipGuardCallbacks
{
	void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer);

	bool OnOwnershipRequest(NetPlayer fromPlayer);

	void OnMyOwnerLeft();

	bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer);

	void OnMyCreatorLeft();
}
