using System;

namespace GorillaNetworking
{
	public class CustomMapNetworkJoinTrigger : GorillaNetworkJoinTrigger
	{
		public override string GetFullDesiredGameModeString()
		{
			return string.Concat(new string[]
			{
				this.networkZone,
				GorillaComputer.instance.currentQueue,
				CustomMapLoader.LoadedMapModId.ToString(),
				"_",
				CustomMapLoader.LoadedMapModFileId.ToString(),
				base.GetDesiredGameType()
			});
		}

		public override byte GetRoomSize()
		{
			return CustomMapLoader.GetRoomSizeForCurrentlyLoadedMap();
		}
	}
}
