using System;

public enum ESuperInfectionGameState : short
{
	Uninitialized,
	Stopped,
	Starting,
	WaitingForMorePlayers,
	Playing,
	RoundRestarting
}
