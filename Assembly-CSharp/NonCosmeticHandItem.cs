using System;
using GorillaNetworking;
using UnityEngine;

public class NonCosmeticHandItem : MonoBehaviour
{
	public void EnableItem(bool enable)
	{
		if (this.itemPrefab)
		{
			this.itemPrefab.gameObject.SetActive(enable);
		}
	}

	public bool IsEnabled
	{
		get
		{
			return this.itemPrefab && this.itemPrefab.gameObject.activeSelf;
		}
	}

	public CosmeticsController.CosmeticSlots cosmeticSlots;

	public GameObject itemPrefab;
}
