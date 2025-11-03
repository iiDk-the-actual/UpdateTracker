using System;
using UnityEngine;

namespace GorillaNetworking.Store
{
	public class StoreDisplay : MonoBehaviour
	{
		private void GetAllDynamicCosmeticStands()
		{
			this.Stands = base.GetComponentsInChildren<DynamicCosmeticStand>();
		}

		private void SetDisplayNameForAllStands()
		{
			DynamicCosmeticStand[] componentsInChildren = base.GetComponentsInChildren<DynamicCosmeticStand>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].CopyChildsName();
			}
		}

		public string displayName = "";

		public DynamicCosmeticStand[] Stands;
	}
}
