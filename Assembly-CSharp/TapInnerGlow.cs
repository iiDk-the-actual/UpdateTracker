using System;
using UnityEngine;

public class TapInnerGlow : MonoBehaviour
{
	private Material targetMaterial
	{
		get
		{
			if (this._instance.AsNull<Material>() == null)
			{
				return this._instance = this._renderer.material;
			}
			return this._instance;
		}
	}

	public void Tap()
	{
		if (!this._renderer)
		{
			return;
		}
		Material targetMaterial = this.targetMaterial;
		float num = this.tapLength;
		float time = GTShaderGlobals.Time;
		UberShader.InnerGlowSinePeriod.SetValue<float>(targetMaterial, num);
		UberShader.InnerGlowSinePhaseShift.SetValue<float>(targetMaterial, time);
	}

	public Renderer _renderer;

	public float tapLength = 1f;

	[Space]
	private Material _instance;
}
