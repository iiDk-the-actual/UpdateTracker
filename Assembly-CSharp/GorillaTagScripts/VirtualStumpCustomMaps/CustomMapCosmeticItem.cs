using System;
using GorillaNetworking.Store;
using GT_CustomMapSupportRuntime;

namespace GorillaTagScripts.VirtualStumpCustomMaps
{
	[Serializable]
	public struct CustomMapCosmeticItem
	{
		public GTObjectPlaceholder.ECustomMapCosmeticItem customMapItemSlot;

		public HeadModel_CosmeticStand.BustType bustType;

		public string playFabID;
	}
}
