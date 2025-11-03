using System;
using System.Collections.Generic;
using GorillaNetworking;
using UnityEngine;

namespace CosmeticRoom
{
	public class FittingRoom : MonoBehaviour
	{
		public void InitializeForCustomMap(bool useCustomConsoleMesh = true)
		{
			GameObject gameObject = this.consoleMesh;
			if (gameObject != null)
			{
				gameObject.SetActive(!useCustomConsoleMesh);
			}
			CosmeticsController.instance.AddFittingRoom(this);
		}

		public void UpdateFromCart(List<CosmeticsController.CosmeticItem> currentCart, CosmeticsController.CosmeticSet tryOnSet)
		{
			this.iterator = 0;
			while (this.iterator < this.fittingRoomButtons.Length)
			{
				if (this.iterator < currentCart.Count)
				{
					bool flag = CosmeticsController.instance.AnyMatch(tryOnSet, currentCart[this.iterator]);
					this.fittingRoomButtons[this.iterator].SetItem(currentCart[this.iterator], flag);
				}
				else
				{
					this.fittingRoomButtons[this.iterator].ClearItem();
				}
				this.iterator++;
			}
		}

		public FittingRoomButton[] fittingRoomButtons;

		public GameObject consoleMesh;

		private int iterator;
	}
}
