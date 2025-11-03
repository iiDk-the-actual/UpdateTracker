using System;

namespace GorillaTag.CosmeticSystem
{
	[Serializable]
	public struct CosmeticAnchorAntiClipEntry
	{
		public bool enabled;

		public XformOffset offset;

		public static readonly CosmeticAnchorAntiClipEntry Identity = new CosmeticAnchorAntiClipEntry
		{
			offset = XformOffset.Identity
		};
	}
}
