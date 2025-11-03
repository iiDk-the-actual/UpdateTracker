using System;

public enum JoinTriggerVisualState
{
	ConnectionError,
	AlreadyInRoom,
	InPrivateRoom,
	NotConnectedSoloJoin,
	LeaveRoomAndSoloJoin,
	LeaveRoomAndPartyJoin,
	AbandonPartyAndSoloJoin,
	ChangingGameModeSoloJoin
}
