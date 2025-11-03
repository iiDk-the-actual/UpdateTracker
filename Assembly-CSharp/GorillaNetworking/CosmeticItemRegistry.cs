using System;
using System.Collections.Generic;
using GorillaTag.CosmeticSystem;
using UnityEngine;

namespace GorillaNetworking
{
	public class CosmeticItemRegistry
	{
		public void Initialize(GameObject[] cosmeticGObjs)
		{
			if (this._isInitialized)
			{
				return;
			}
			this._isInitialized = true;
			foreach (GameObject gameObject in cosmeticGObjs)
			{
				string text = gameObject.name.Replace("LEFT.", "").Replace("RIGHT.", "").TrimEnd();
				CosmeticItemInstance cosmeticItemInstance;
				if (this.nameToCosmeticMap.ContainsKey(text))
				{
					cosmeticItemInstance = this.nameToCosmeticMap[text];
				}
				else
				{
					cosmeticItemInstance = new CosmeticItemInstance();
					CosmeticSO cosmeticSOFromDisplayName = CosmeticsController.instance.GetCosmeticSOFromDisplayName(text);
					cosmeticItemInstance.clippingOffsets = ((cosmeticSOFromDisplayName != null) ? cosmeticSOFromDisplayName.info.anchorAntiIntersectOffsets : CosmeticsController.instance.defaultClipOffsets);
					cosmeticItemInstance.isHoldableItem = cosmeticSOFromDisplayName != null && cosmeticSOFromDisplayName.info.hasHoldableParts;
					this.nameToCosmeticMap.Add(text, cosmeticItemInstance);
				}
				HoldableObject component = gameObject.GetComponent<HoldableObject>();
				bool flag = gameObject.name.Contains("LEFT.");
				bool flag2 = gameObject.name.Contains("RIGHT.");
				if (cosmeticItemInstance.isHoldableItem && component != null)
				{
					if (component is SnowballThrowable || component is TransferrableObject)
					{
						cosmeticItemInstance.holdableObjects.Add(gameObject);
					}
					else if (flag)
					{
						cosmeticItemInstance.leftObjects.Add(gameObject);
					}
					else if (flag2)
					{
						cosmeticItemInstance.rightObjects.Add(gameObject);
					}
					else
					{
						cosmeticItemInstance.objects.Add(gameObject);
					}
				}
				else if (flag)
				{
					cosmeticItemInstance.leftObjects.Add(gameObject);
				}
				else if (flag2)
				{
					cosmeticItemInstance.rightObjects.Add(gameObject);
				}
				else
				{
					cosmeticItemInstance.objects.Add(gameObject);
				}
				cosmeticItemInstance.dbgname = text;
			}
		}

		public CosmeticItemInstance Cosmetic(string itemName)
		{
			if (!this._isInitialized)
			{
				Debug.LogError("Tried to use CosmeticItemRegistry before it was initialized!");
				return null;
			}
			if (string.IsNullOrEmpty(itemName) || itemName == "NOTHING")
			{
				return null;
			}
			CosmeticItemInstance cosmeticItemInstance;
			if (!this.nameToCosmeticMap.TryGetValue(itemName, out cosmeticItemInstance))
			{
				return null;
			}
			return cosmeticItemInstance;
		}

		private bool _isInitialized;

		private Dictionary<string, CosmeticItemInstance> nameToCosmeticMap = new Dictionary<string, CosmeticItemInstance>();

		private GameObject nullItem;
	}
}
