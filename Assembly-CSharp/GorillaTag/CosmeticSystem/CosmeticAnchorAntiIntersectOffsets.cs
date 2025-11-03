using System;
using UnityEngine.Serialization;

namespace GorillaTag.CosmeticSystem
{
	[Serializable]
	public struct CosmeticAnchorAntiIntersectOffsets
	{
		public CosmeticAnchorAntiClipEntry nameTag;

		public CosmeticAnchorAntiClipEntry leftArm;

		public CosmeticAnchorAntiClipEntry rightArm;

		public CosmeticAnchorAntiClipEntry chest;

		public CosmeticAnchorAntiClipEntry huntComputer;

		public CosmeticAnchorAntiClipEntry badge;

		public CosmeticAnchorAntiClipEntry builderWatch;

		public CosmeticAnchorAntiClipEntry friendshipBraceletLeft;

		[FormerlySerializedAs("friendshipBradceletRight")]
		public CosmeticAnchorAntiClipEntry friendshipBraceletRight;

		public static readonly CosmeticAnchorAntiIntersectOffsets Identity = new CosmeticAnchorAntiIntersectOffsets
		{
			nameTag = CosmeticAnchorAntiClipEntry.Identity,
			leftArm = CosmeticAnchorAntiClipEntry.Identity,
			rightArm = CosmeticAnchorAntiClipEntry.Identity,
			chest = CosmeticAnchorAntiClipEntry.Identity,
			huntComputer = CosmeticAnchorAntiClipEntry.Identity,
			badge = CosmeticAnchorAntiClipEntry.Identity,
			builderWatch = CosmeticAnchorAntiClipEntry.Identity
		};
	}
}
