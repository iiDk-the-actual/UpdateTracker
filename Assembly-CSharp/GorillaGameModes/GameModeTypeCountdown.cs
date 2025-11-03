using System;
using GameObjectScheduling;

namespace GorillaGameModes
{
	[Serializable]
	public struct GameModeTypeCountdown
	{
		public GameModeType mode;

		public CountdownTextDate countdownTextDate;
	}
}
