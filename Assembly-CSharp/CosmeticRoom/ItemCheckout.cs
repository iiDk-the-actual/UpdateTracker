using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CosmeticRoom
{
	public class ItemCheckout : MonoBehaviour
	{
		public void InitializeForCustomMap(CompositeTriggerEvents customMapTryOnArea, Scene customMapScene, bool useCustomCounterMesh = true)
		{
			GameObject gameObject = this.checkoutCounterMesh;
			if (gameObject != null)
			{
				gameObject.SetActive(!useCustomCounterMesh);
			}
			GameObject gameObject2 = this.purchaseScreenMesh;
			if (gameObject2 != null)
			{
				gameObject2.SetActive(useCustomCounterMesh);
			}
			this.originalScene = customMapScene;
			customMapTryOnArea.AddCollider(this.checkoutTryOnArea);
			CosmeticsController.instance.AddItemCheckout(this);
		}

		public void RemoveFromCustomMap(CompositeTriggerEvents customMapTryOnArea)
		{
			if (customMapTryOnArea.IsNull())
			{
				return;
			}
			customMapTryOnArea.RemoveCollider(this.checkoutTryOnArea);
		}

		public void UpdateFromCart(List<CosmeticsController.CosmeticItem> currentCart, CosmeticsController.CosmeticItem itemToBuy)
		{
			this.iterator = 0;
			while (this.iterator < this.checkoutCartButtons.Length)
			{
				if (this.iterator < currentCart.Count)
				{
					bool flag = currentCart[this.iterator].itemName == itemToBuy.itemName;
					this.checkoutCartButtons[this.iterator].SetItem(currentCart[this.iterator], flag);
				}
				else
				{
					this.checkoutCartButtons[this.iterator].ClearItem();
				}
				this.iterator++;
			}
		}

		public void UpdatePurchaseText(string newText, string leftPurchaseButtonText, string rightPurchaseButtonText, bool leftButtonOn, bool rightButtonOn)
		{
			if (this.purchaseText.IsNotNull())
			{
				this.purchaseText.text = newText;
			}
			if (this.purchaseTextTMP.IsNotNull())
			{
				this.purchaseTextTMP.text = newText;
			}
			if (!leftPurchaseButtonText.IsNullOrEmpty())
			{
				this.leftPurchaseButton.SetText(leftPurchaseButtonText);
				this.leftPurchaseButton.buttonRenderer.material = (leftButtonOn ? this.leftPurchaseButton.pressedMaterial : this.leftPurchaseButton.unpressedMaterial);
			}
			if (!rightPurchaseButtonText.IsNullOrEmpty())
			{
				this.rightPurchaseButton.SetText(rightPurchaseButtonText);
				this.rightPurchaseButton.buttonRenderer.material = (rightButtonOn ? this.rightPurchaseButton.pressedMaterial : this.rightPurchaseButton.unpressedMaterial);
			}
		}

		public bool IsFromScene(Scene unloadingScene)
		{
			return unloadingScene == this.originalScene;
		}

		public CheckoutCartButton[] checkoutCartButtons;

		public PurchaseItemButton leftPurchaseButton;

		public PurchaseItemButton rightPurchaseButton;

		[HideInInspector]
		public Text purchaseText;

		public TMP_Text purchaseTextTMP;

		public HeadModel checkoutHeadModel;

		public Collider checkoutTryOnArea;

		public GameObject checkoutCounterMesh;

		public GameObject purchaseScreenMesh;

		private Scene originalScene;

		private int iterator;
	}
}
