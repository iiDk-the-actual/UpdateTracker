using System;

namespace GorillaTag.CosmeticSystem.Editor
{
	[Flags]
	public enum EEdCosBrowserCategoryFilter
	{
		None = 0,
		Hat = 1,
		Badge = 2,
		Face = 4,
		Paw = 8,
		Chest = 16,
		Fur = 32,
		Shirt = 64,
		Back = 128,
		Arms = 256,
		Pants = 512,
		TagEffect = 1024,
		Set = 4096,
		All = 6143
	}
}
