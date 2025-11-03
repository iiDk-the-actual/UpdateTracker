using System;
using UnityEngine;

namespace GorillaTag.CosmeticSystem
{
	[Serializable]
	public struct CosmeticPlacementInfo
	{
		[Tooltip("The bone to attach the cosmetic to.")]
		public GTHardCodedBones.SturdyEBone parentBone;

		public XformOffset offset;
	}
}
