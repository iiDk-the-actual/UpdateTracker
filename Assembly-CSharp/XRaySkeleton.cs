using System;
using UnityEngine;

public class XRaySkeleton : SyncToPlayerColor
{
	protected override void Awake()
	{
		base.Awake();
		this.target = this.renderer.material;
		Material[] materialsToChangeTo = this.rig.materialsToChangeTo;
		this.tagMaterials = new Material[materialsToChangeTo.Length];
		this.tagMaterials[0] = new Material(this.target);
		for (int i = 1; i < materialsToChangeTo.Length; i++)
		{
			Material material = new Material(materialsToChangeTo[i]);
			this.tagMaterials[i] = material;
		}
	}

	public void SetMaterialIndex(int index)
	{
		this.renderer.sharedMaterial = this.tagMaterials[index];
		this._lastMatIndex = index;
	}

	private void Setup()
	{
		this.colorPropertiesToSync = new ShaderHashId[]
		{
			XRaySkeleton._BaseColor,
			XRaySkeleton._EmissionColor
		};
	}

	public override void UpdateColor(Color color)
	{
		if (this._lastMatIndex != 0)
		{
			return;
		}
		Material material = this.tagMaterials[0];
		float num;
		float num2;
		float num3;
		Color.RGBToHSV(color, out num, out num2, out num3);
		Color color2 = Color.HSVToRGB(num, num2, Mathf.Clamp(num3, this.baseValueMinMax.x, this.baseValueMinMax.y));
		material.SetColor(XRaySkeleton._BaseColor, color2);
		float num4;
		float num5;
		float num6;
		Color.RGBToHSV(color, out num4, out num5, out num6);
		Color color3 = Color.HSVToRGB(num4, 0.82f, 0.9f, true);
		color3 = new Color(color3.r * 1.4f, color3.g * 1.4f, color3.b * 1.4f);
		material.SetColor(XRaySkeleton._EmissionColor, ColorUtils.ComposeHDR(new Color32(36, 191, 136, byte.MaxValue), 2f));
		this.renderer.sharedMaterial = material;
	}

	public SkinnedMeshRenderer renderer;

	public Vector2 baseValueMinMax = new Vector2(0.69f, 1f);

	public Material[] tagMaterials = new Material[0];

	private int _lastMatIndex;

	private static readonly ShaderHashId _BaseColor = "_BaseColor";

	private static readonly ShaderHashId _EmissionColor = "_EmissionColor";
}
