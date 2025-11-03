using System;
using UnityEngine;

public class PlayerColoredCosmetic : MonoBehaviour
{
	public void Awake()
	{
		for (int i = 0; i < this.coloringRules.Length; i++)
		{
			this.coloringRules[i].Init();
		}
	}

	private void InitIfNeeded()
	{
		if (!this.didInit)
		{
			this.didInit = true;
			this.rig = base.GetComponentInParent<VRRig>();
			if (this.rig == null && GorillaTagger.Instance != null)
			{
				this.rig = GorillaTagger.Instance.offlineVRRig;
			}
			this.particleMains = new ParticleSystem.MainModule[this.particleSystems.Length];
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleMains[i] = this.particleSystems[i].main;
			}
		}
	}

	private void OnEnable()
	{
		this.InitIfNeeded();
		if (this.rig != null)
		{
			this.rig.OnColorChanged += this.UpdateColor;
			this.UpdateColor(this.rig.playerColor);
		}
	}

	private void OnDisable()
	{
		if (this.rig != null)
		{
			this.rig.OnColorChanged -= this.UpdateColor;
		}
	}

	public void UpdateColor(Color color)
	{
		this.InitIfNeeded();
		Color color2 = Color.Lerp(color, this.lerpToColor, this.lerpStrength);
		for (int i = 0; i < this.coloringRules.Length; i++)
		{
			this.coloringRules[i].Apply(color2);
		}
		for (int j = 0; j < this.particleSystems.Length; j++)
		{
			this.particleMains[j].startColor = color2;
		}
	}

	private bool didInit;

	private VRRig rig;

	[SerializeField]
	private Color lerpToColor = Color.white;

	[SerializeField]
	[Range(0f, 1f)]
	private float lerpStrength;

	[SerializeField]
	private PlayerColoredCosmetic.ColoringRule[] coloringRules;

	[SerializeField]
	private ParticleSystem[] particleSystems;

	private ParticleSystem.MainModule[] particleMains;

	[Serializable]
	private struct ColoringRule
	{
		public void Init()
		{
			this.hashId = Shader.PropertyToID(this.shaderColorProperty);
			Material[] sharedMaterials = this.meshRenderer.sharedMaterials;
			this.defaultMaterial = sharedMaterials[this.materialIndex];
			this.instancedMaterial = new Material(sharedMaterials[this.materialIndex]);
			sharedMaterials[this.materialIndex] = this.instancedMaterial;
			this.meshRenderer.sharedMaterials = sharedMaterials;
		}

		public void Apply(Color color)
		{
			this.instancedMaterial.SetColor(this.hashId, color);
		}

		[SerializeField]
		private string shaderColorProperty;

		private int hashId;

		[SerializeField]
		private Renderer meshRenderer;

		[SerializeField]
		private int materialIndex;

		private Material instancedMaterial;

		private Material defaultMaterial;
	}
}
