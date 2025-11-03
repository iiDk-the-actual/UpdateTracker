using System;
using UnityEngine;

[ExecuteAlways]
public class WaterSurfaceMaterialController : MonoBehaviour
{
	protected void OnEnable()
	{
		this.renderer = base.GetComponent<Renderer>();
		this.matPropBlock = new MaterialPropertyBlock();
		this.ApplyProperties();
	}

	private void ApplyProperties()
	{
		this.matPropBlock.SetVector(ShaderProps._ScrollSpeedAndScale, new Vector4(this.ScrollX, this.ScrollY, this.Scale, 0f));
		if (this.renderer)
		{
			this.renderer.SetPropertyBlock(this.matPropBlock);
		}
	}

	public float ScrollX = 0.6f;

	public float ScrollY = 0.6f;

	public float Scale = 1f;

	private Renderer renderer;

	private MaterialPropertyBlock matPropBlock;
}
