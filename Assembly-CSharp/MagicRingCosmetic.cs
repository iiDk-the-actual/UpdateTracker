using System;
using UnityEngine;

public class MagicRingCosmetic : MonoBehaviour
{
	protected void Awake()
	{
		this.materialPropertyBlock = new MaterialPropertyBlock();
		this.defaultEmissiveColor = this.ringRenderer.sharedMaterial.GetColor(ShaderProps._EmissionColor);
	}

	protected void LateUpdate()
	{
		float celsius = this.thermalReceiver.celsius;
		if (celsius >= this.fadeInTemperatureThreshold && this.fadeState != MagicRingCosmetic.FadeState.FadedIn)
		{
			this.fadeInSounds.Play();
			this.fadeState = MagicRingCosmetic.FadeState.FadedIn;
		}
		else if (celsius <= this.fadeOutTemperatureThreshold && this.fadeState != MagicRingCosmetic.FadeState.FadedOut)
		{
			this.fadeOutSounds.Play();
			this.fadeState = MagicRingCosmetic.FadeState.FadedOut;
		}
		this.emissiveAmount = Mathf.MoveTowards(this.emissiveAmount, (this.fadeState == MagicRingCosmetic.FadeState.FadedIn) ? 1f : 0f, Time.deltaTime / this.fadeTime);
		this.ringRenderer.GetPropertyBlock(this.materialPropertyBlock);
		this.materialPropertyBlock.SetColor(ShaderProps._EmissionColor, new Color(this.defaultEmissiveColor.r, this.defaultEmissiveColor.g, this.defaultEmissiveColor.b, this.emissiveAmount));
		this.ringRenderer.SetPropertyBlock(this.materialPropertyBlock);
	}

	[Tooltip("The ring will fade in the emissive texture based on temperature from this ThermalReceiver.")]
	public ThermalReceiver thermalReceiver;

	public Renderer ringRenderer;

	public float fadeInTemperatureThreshold = 200f;

	public float fadeOutTemperatureThreshold = 190f;

	public float fadeTime = 1.5f;

	public SoundBankPlayer fadeInSounds;

	public SoundBankPlayer fadeOutSounds;

	private MagicRingCosmetic.FadeState fadeState;

	private Color defaultEmissiveColor;

	private float emissiveAmount;

	private MaterialPropertyBlock materialPropertyBlock;

	private enum FadeState
	{
		FadedOut,
		FadedIn
	}
}
