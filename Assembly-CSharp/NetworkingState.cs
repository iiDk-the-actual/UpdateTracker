using System;

public enum NetworkingState
{
	IsOwner,
	IsBlindClient,
	IsClient,
	ForcefullyTakingOver,
	RequestingOwnership,
	RequestingOwnershipWaitingForSight,
	ForcefullyTakingOverWaitingForSight
}
