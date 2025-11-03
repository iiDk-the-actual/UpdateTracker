using System;
using UnityEngine;

[Serializable]
public struct MaterialCombinerPerRendererInfo
{
	public Renderer renderer;

	public int slotIndex;

	public int sliceIndex;

	public Color baseColor;

	public Material oldMat;

	public bool wasMeshCombined;
}
