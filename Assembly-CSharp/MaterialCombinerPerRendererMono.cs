using System;
using System.Collections.Generic;
using UnityEngine;

public class MaterialCombinerPerRendererMono : MonoBehaviour
{
	protected void Awake()
	{
	}

	public void AddEntry(Renderer r, int slot, int sliceIndex, Color baseColor, Material oldMat)
	{
		this.slotData.Add(new MaterialCombinerPerRendererInfo
		{
			renderer = r,
			slotIndex = slot,
			sliceIndex = sliceIndex,
			baseColor = baseColor,
			oldMat = oldMat
		});
	}

	public bool TryGetData(Renderer r, int slot, out MaterialCombinerPerRendererInfo data)
	{
		foreach (MaterialCombinerPerRendererInfo materialCombinerPerRendererInfo in this.slotData)
		{
			if (materialCombinerPerRendererInfo.renderer == r && materialCombinerPerRendererInfo.slotIndex == slot)
			{
				data = materialCombinerPerRendererInfo;
				return true;
			}
		}
		data = default(MaterialCombinerPerRendererInfo);
		return false;
	}

	public List<MaterialCombinerPerRendererInfo> slotData = new List<MaterialCombinerPerRendererInfo>();
}
