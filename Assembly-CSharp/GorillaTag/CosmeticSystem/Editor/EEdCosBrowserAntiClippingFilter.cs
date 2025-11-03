using System;

namespace GorillaTag.CosmeticSystem.Editor
{
	[Flags]
	public enum EEdCosBrowserAntiClippingFilter
	{
		None = 0,
		NameTag = 1,
		LeftArm = 2,
		RightArm = 4,
		Chest = 8,
		HuntComputer = 16,
		Badge = 32,
		BuilderWatch = 64,
		FriendshipBraceletLeft = 128,
		FriendshipBraceletRight = 256,
		All = 511
	}
}
