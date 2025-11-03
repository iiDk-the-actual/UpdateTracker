using System;
using UnityEngine;

public class BuilderBumpGlow : MonoBehaviour
{
	public void Awake()
	{
		this.blendIn = 1f;
		this.intensity = 0f;
		this.UpdateRender();
	}

	public void SetIntensity(float intensity)
	{
		this.intensity = intensity;
		this.UpdateRender();
	}

	public void SetBlendIn(float blendIn)
	{
		this.blendIn = blendIn;
		this.UpdateRender();
	}

	private void UpdateRender()
	{
	}

	public MeshRenderer glowRenderer;

	private float blendIn;

	private float intensity;
}
