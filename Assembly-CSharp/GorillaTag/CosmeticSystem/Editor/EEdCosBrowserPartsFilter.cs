using System;

namespace GorillaTag.CosmeticSystem.Editor
{
	[Flags]
	public enum EEdCosBrowserPartsFilter
	{
		None = 0,
		NoParts = 1,
		Holdable = 2,
		Functional = 4,
		Wardrobe = 8,
		Store = 16,
		FirstPerson = 32,
		LocalRig = 64,
		All = 127
	}
}
