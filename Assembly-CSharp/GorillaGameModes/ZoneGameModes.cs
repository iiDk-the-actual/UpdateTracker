using System;

namespace GorillaGameModes
{
	[Serializable]
	public struct ZoneGameModes
	{
		public GTZone[] zone;

		public GameModeType[] modes;

		public GameModeType[] privateModes;
	}
}
