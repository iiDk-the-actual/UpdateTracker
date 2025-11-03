using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GRToolUpgradePurchaseStationShelf : MonoBehaviour
{
	public void Awake()
	{
		for (int i = 0; i < this.gRPurchaseSlots.Count; i++)
		{
			Renderer[] componentsInChildren = this.gRPurchaseSlots[i].SlotPivot.gameObject.GetComponentsInChildren<Renderer>();
			this.slotRenderers.Add(componentsInChildren);
			Material[][] array = new Material[componentsInChildren.Length][];
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				array[j] = componentsInChildren[j].sharedMaterials;
			}
			this.slotOriginalMaterials.Add(array);
		}
	}

	public void SetMaterialOverride(int slotID, Material overrideMaterial)
	{
		if (slotID < 0 || slotID >= this.gRPurchaseSlots.Count)
		{
			return;
		}
		if (this.gRPurchaseSlots[slotID].overrideMaterial == overrideMaterial)
		{
			return;
		}
		if (slotID >= this.slotRenderers.Count)
		{
			return;
		}
		this.gRPurchaseSlots[slotID].overrideMaterial = overrideMaterial;
		for (int i = 0; i < this.slotRenderers[slotID].Length; i++)
		{
			Renderer renderer = this.slotRenderers[slotID][i];
			if (overrideMaterial == null)
			{
				renderer.materials = this.slotOriginalMaterials[slotID][i];
			}
			else
			{
				Material[] array = new Material[renderer.sharedMaterials.Length];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = overrideMaterial;
				}
				renderer.materials = array;
			}
		}
	}

	public void SetBacklightStateAndMaterial(int slotID, bool isEnabled, Material materialOverride)
	{
		if (slotID < 0 || slotID >= this.gRPurchaseSlots.Count)
		{
			return;
		}
		if (this.gRPurchaseSlots[slotID].BacklightRenderer != null)
		{
			if (!isEnabled)
			{
				this.gRPurchaseSlots[slotID].BacklightRenderer.enabled = false;
				return;
			}
			this.gRPurchaseSlots[slotID].BacklightRenderer.enabled = true;
			this.gRPurchaseSlots[slotID].BacklightRenderer.sharedMaterial = materialOverride;
		}
	}

	public string ShelfName;

	private List<Material[][]> slotOriginalMaterials = new List<Material[][]>();

	private List<Renderer[]> slotRenderers = new List<Renderer[]>();

	public List<GRToolUpgradePurchaseStationShelf.GRPurchaseSlot> gRPurchaseSlots;

	[Serializable]
	public class GRPurchaseSlot
	{
		public TMP_Text Name;

		public TMP_Text Price;

		public Transform SlotPivot;

		public GRToolProgressionManager.ToolParts PurchaseID;

		public GameEntity ToolEntityPrefab;

		public float RopeYaw;

		public float RopePitch;

		public MeshRenderer BacklightRenderer;

		[NonSerialized]
		public Material overrideMaterial;

		[NonSerialized]
		public bool canAfford;

		[NonSerialized]
		public string purchaseText = "";
	}
}
