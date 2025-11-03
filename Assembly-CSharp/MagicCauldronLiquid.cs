using System;
using UnityEngine;

public class MagicCauldronLiquid : MonoBehaviour
{
	private void Test()
	{
		this._animProgress = 0f;
		this._animating = true;
		base.enabled = true;
	}

	public void AnimateColorFromTo(Color a, Color b, float length = 1f)
	{
		this._colorStart = a;
		this._colorEnd = b;
		this._animProgress = 0f;
		this._animating = true;
		this.animLength = length;
		base.enabled = true;
	}

	private void ApplyColor(Color color)
	{
		if (!this._applyMaterial)
		{
			return;
		}
		this._applyMaterial.SetColor(ShaderProps._BaseColor, color);
		this._applyMaterial.Apply();
	}

	private void ApplyWaveParams(float amplitude, float frequency, float scale, float rotation)
	{
		if (!this._applyMaterial)
		{
			return;
		}
		this._applyMaterial.SetFloat(ShaderProps._WaveAmplitude, amplitude);
		this._applyMaterial.SetFloat(ShaderProps._WaveFrequency, frequency);
		this._applyMaterial.SetFloat(ShaderProps._WaveScale, scale);
		this._applyMaterial.Apply();
	}

	private void OnEnable()
	{
		if (this._applyMaterial)
		{
			this._applyMaterial.mode = ApplyMaterialProperty.ApplyMode.MaterialPropertyBlock;
		}
	}

	private void OnDisable()
	{
		this._animating = false;
		this._animProgress = 0f;
	}

	private void Update()
	{
		if (!this._animating)
		{
			return;
		}
		float num = this._animationCurve.Evaluate(this._animProgress / this.animLength);
		float num2 = this._waveCurve.Evaluate(this._animProgress / this.animLength);
		if (num >= 1f)
		{
			this.ApplyColor(this._colorEnd);
			this._animating = false;
			base.enabled = false;
			return;
		}
		Color color = Color.Lerp(this._colorStart, this._colorEnd, num);
		Mathf.Lerp(this.waveNormal.frequency, this.waveAnimating.frequency, num2);
		Mathf.Lerp(this.waveNormal.amplitude, this.waveAnimating.amplitude, num2);
		Mathf.Lerp(this.waveNormal.scale, this.waveAnimating.scale, num2);
		Mathf.Lerp(this.waveNormal.rotation, this.waveAnimating.rotation, num2);
		this.ApplyColor(color);
		this._animProgress += Time.deltaTime;
	}

	[SerializeField]
	private ApplyMaterialProperty _applyMaterial;

	[SerializeField]
	private Color _colorStart;

	[SerializeField]
	private Color _colorEnd;

	[SerializeField]
	private bool _animating;

	[SerializeField]
	private float _animProgress;

	[SerializeField]
	private AnimationCurve _animationCurve = AnimationCurves.EaseOutCubic;

	[SerializeField]
	private AnimationCurve _waveCurve = AnimationCurves.EaseInElastic;

	public float animLength = 1f;

	public MagicCauldronLiquid.WaveParams waveNormal;

	public MagicCauldronLiquid.WaveParams waveAnimating;

	[Serializable]
	public struct WaveParams
	{
		public float amplitude;

		public float frequency;

		public float scale;

		public float rotation;
	}
}
