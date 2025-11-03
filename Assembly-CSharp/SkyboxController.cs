using System;
using UnityEngine;
using UnityEngine.Rendering;

public class SkyboxController : MonoBehaviour
{
	private void Start()
	{
		if (this._dayNightManager.AsNull<BetterDayNightManager>() == null)
		{
			this._dayNightManager = BetterDayNightManager.instance;
		}
		if (this._dayNightManager.AsNull<BetterDayNightManager>() == null)
		{
			return;
		}
		for (int i = 0; i < this._dayNightManager.timeOfDayRange.Length; i++)
		{
			this._totalSecondsInRange += this._dayNightManager.timeOfDayRange[i] * 3600.0;
		}
		this._totalSecondsInRange = Math.Floor(this._totalSecondsInRange);
	}

	private void Update()
	{
		if (!this.lastUpdate.HasElapsed(1f, true))
		{
			return;
		}
		this.UpdateTime();
		this.UpdateSky();
	}

	private void OnValidate()
	{
		this.UpdateSky();
	}

	private void UpdateTime()
	{
		this._currentSeconds = ((ITimeOfDaySystem)this._dayNightManager).currentTimeInSeconds;
		this._currentSeconds = Math.Floor(this._currentSeconds);
		this._currentTime = (float)(this._currentSeconds / this._totalSecondsInRange);
	}

	private void UpdateSky()
	{
		if (this.skyMaterials == null || this.skyMaterials.Length == 0)
		{
			return;
		}
		int num = this.skyMaterials.Length;
		float num2 = Mathf.Clamp(this._currentTime, 0f, 1f);
		float num3 = 1f / (float)num;
		int num4 = (int)(num2 / num3);
		float num5 = (num2 - (float)num4 * num3) / num3;
		this._currentSky = this.skyMaterials[num4];
		this._nextSky = this.skyMaterials[(num4 + 1) % num];
		this.skyFront.sharedMaterial = this._currentSky;
		this.skyBack.sharedMaterial = this._nextSky;
		if (this._currentSky.renderQueue != 3000)
		{
			this.SetFrontToTransparent();
		}
		if (this._nextSky.renderQueue == 3000)
		{
			this.SetBackToOpaque();
		}
		this._currentSky.SetFloat(ShaderProps._SkyAlpha, 1f - num5);
	}

	private void SetFrontToTransparent()
	{
		bool flag = false;
		bool flag2 = false;
		string text = "Transparent";
		int num = 3000;
		BlendMode blendMode = BlendMode.SrcAlpha;
		BlendMode blendMode2 = BlendMode.OneMinusSrcAlpha;
		BlendMode blendMode3 = BlendMode.One;
		BlendMode blendMode4 = BlendMode.OneMinusSrcAlpha;
		Material sharedMaterial = this.skyFront.sharedMaterial;
		sharedMaterial.SetFloat(ShaderProps._ZWrite, flag ? 1f : 0f);
		sharedMaterial.SetShaderPassEnabled("DepthOnly", flag);
		sharedMaterial.SetFloat(ShaderProps._AlphaToMask, flag2 ? 1f : 0f);
		sharedMaterial.SetOverrideTag("RenderType", text);
		sharedMaterial.renderQueue = num;
		sharedMaterial.SetFloat(ShaderProps._SrcBlend, (float)blendMode);
		sharedMaterial.SetFloat(ShaderProps._DstBlend, (float)blendMode2);
		sharedMaterial.SetFloat(ShaderProps._SrcBlendAlpha, (float)blendMode3);
		sharedMaterial.SetFloat(ShaderProps._DstBlendAlpha, (float)blendMode4);
	}

	private void SetFrontToOpaque()
	{
		bool flag = false;
		bool flag2 = true;
		string text = "Opaque";
		int num = 2000;
		BlendMode blendMode = BlendMode.One;
		BlendMode blendMode2 = BlendMode.Zero;
		BlendMode blendMode3 = BlendMode.One;
		BlendMode blendMode4 = BlendMode.Zero;
		Material sharedMaterial = this.skyFront.sharedMaterial;
		sharedMaterial.SetFloat(ShaderProps._ZWrite, flag2 ? 1f : 0f);
		sharedMaterial.SetShaderPassEnabled("DepthOnly", flag2);
		sharedMaterial.SetFloat(ShaderProps._AlphaToMask, flag ? 1f : 0f);
		sharedMaterial.SetOverrideTag("RenderType", text);
		sharedMaterial.renderQueue = num;
		sharedMaterial.SetFloat(ShaderProps._SrcBlend, (float)blendMode);
		sharedMaterial.SetFloat(ShaderProps._DstBlend, (float)blendMode2);
		sharedMaterial.SetFloat(ShaderProps._SrcBlendAlpha, (float)blendMode3);
		sharedMaterial.SetFloat(ShaderProps._DstBlendAlpha, (float)blendMode4);
	}

	private void SetBackToOpaque()
	{
		bool flag = false;
		bool flag2 = true;
		string text = "Opaque";
		int num = 2000;
		BlendMode blendMode = BlendMode.One;
		BlendMode blendMode2 = BlendMode.Zero;
		BlendMode blendMode3 = BlendMode.One;
		BlendMode blendMode4 = BlendMode.Zero;
		Material sharedMaterial = this.skyBack.sharedMaterial;
		sharedMaterial.SetFloat(ShaderProps._ZWrite, flag2 ? 1f : 0f);
		sharedMaterial.SetShaderPassEnabled("DepthOnly", flag2);
		sharedMaterial.SetFloat(ShaderProps._AlphaToMask, flag ? 1f : 0f);
		sharedMaterial.SetOverrideTag("RenderType", text);
		sharedMaterial.renderQueue = num;
		sharedMaterial.SetFloat(ShaderProps._SrcBlend, (float)blendMode);
		sharedMaterial.SetFloat(ShaderProps._DstBlend, (float)blendMode2);
		sharedMaterial.SetFloat(ShaderProps._SrcBlendAlpha, (float)blendMode3);
		sharedMaterial.SetFloat(ShaderProps._DstBlendAlpha, (float)blendMode4);
	}

	public MeshRenderer skyFront;

	public MeshRenderer skyBack;

	public Material[] skyMaterials = new Material[0];

	[Range(0f, 1f)]
	public float lerpValue;

	[NonSerialized]
	private Material _currentSky;

	[NonSerialized]
	private Material _nextSky;

	private TimeSince lastUpdate = TimeSince.Now();

	[Space]
	private BetterDayNightManager _dayNightManager;

	private double _currentSeconds = -1.0;

	private double _totalSecondsInRange = -1.0;

	private float _currentTime = -1f;
}
