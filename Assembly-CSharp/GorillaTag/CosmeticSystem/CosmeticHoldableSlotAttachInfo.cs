using System;
using UnityEngine;

namespace GorillaTag.CosmeticSystem
{
	[Serializable]
	public struct CosmeticHoldableSlotAttachInfo
	{
		[Tooltip("The anchor that this holdable cosmetic can attach to.")]
		public GTSturdyEnum<GTHardCodedBones.EHandAndStowSlots> stowSlot;

		public XformOffset offset;
	}
}
