using System;
using UnityEngine;

namespace GorillaNetworking.Store
{
	public class DynamicCosmeticStand_Link : MonoBehaviour
	{
		public void SetStandType(HeadModel_CosmeticStand.BustType type)
		{
			this.stand.SetStandType(type);
		}

		public void SpawnItemOntoStand(string PlayFabID)
		{
			this.stand.SpawnItemOntoStand(PlayFabID);
		}

		public void SaveCosmeticMountPosition()
		{
			this.stand.UpdateCosmeticsMountPositions();
		}

		public void ClearCosmeticItems()
		{
			this.stand.ClearCosmetics();
		}

		public DynamicCosmeticStand stand;
	}
}
