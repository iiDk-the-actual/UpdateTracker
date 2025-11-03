using System;
using GorillaExtensions;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class GorillaSkinToggle : MonoBehaviour, ISpawnable
{
	public bool applied
	{
		get
		{
			return this._applied;
		}
	}

	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this._rig = base.GetComponentInParent<VRRig>(true);
		if (this.coloringRules.Length != 0)
		{
			this._activeSkin = GorillaSkin.CopyWithInstancedMaterials(this._skin);
			for (int i = 0; i < this.coloringRules.Length; i++)
			{
				this.coloringRules[i].Init();
			}
			return;
		}
		this._activeSkin = this._skin;
	}

	void ISpawnable.OnDespawn()
	{
	}

	private void OnPlayerColorChanged(Color playerColor)
	{
		foreach (GorillaSkinToggle.ColoringRule coloringRule in this.coloringRules)
		{
			coloringRule.Apply(this._activeSkin, playerColor);
		}
	}

	private void OnEnable()
	{
		if (this.coloringRules.Length != 0)
		{
			this._rig.OnColorChanged += this.OnPlayerColorChanged;
			this.OnPlayerColorChanged(this._rig.playerColor);
		}
		this.Apply();
	}

	private void OnDisable()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.Remove();
		if (this.coloringRules.Length != 0)
		{
			this._rig.OnColorChanged -= this.OnPlayerColorChanged;
		}
	}

	public void Apply()
	{
		GorillaSkin.ApplyToRig(this._rig, this._activeSkin, GorillaSkin.SkinType.cosmetic);
		this._applied = true;
	}

	public void ApplyToMannequin(GameObject mannequin, bool swapMesh = false)
	{
		if (this._skin.IsNull())
		{
			Debug.LogError("No skin set on GorillaSkinToggle");
			return;
		}
		if (mannequin.IsNull())
		{
			Debug.LogError("No mannequin set on GorillaSkinToggle");
			return;
		}
		this._skin.ApplySkinToMannequin(mannequin, swapMesh);
	}

	public void Remove()
	{
		GorillaSkin.ApplyToRig(this._rig, null, GorillaSkin.SkinType.cosmetic);
		float @float = PlayerPrefs.GetFloat("redValue", 0f);
		float float2 = PlayerPrefs.GetFloat("greenValue", 0f);
		float float3 = PlayerPrefs.GetFloat("blueValue", 0f);
		GorillaTagger.Instance.UpdateColor(@float, float2, float3);
		this._applied = false;
	}

	private VRRig _rig;

	[SerializeField]
	private GorillaSkin _skin;

	private GorillaSkin _activeSkin;

	[SerializeField]
	private GorillaSkinToggle.ColoringRule[] coloringRules;

	[Space]
	[SerializeField]
	private bool _applied;

	[Serializable]
	private struct ColoringRule
	{
		public void Init()
		{
			if (string.IsNullOrEmpty(this.shaderColorProperty))
			{
				this.shaderColorProperty = "_BaseColor";
			}
			this.shaderHashId = new ShaderHashId(this.shaderColorProperty);
		}

		public void Apply(GorillaSkin skin, Color color)
		{
			if (this.colorMaterials.HasFlag(GorillaSkinMaterials.Body))
			{
				skin.bodyMaterial.SetColor(this.shaderHashId, color);
			}
			if (this.colorMaterials.HasFlag(GorillaSkinMaterials.Chest))
			{
				skin.chestMaterial.SetColor(this.shaderHashId, color);
			}
			if (this.colorMaterials.HasFlag(GorillaSkinMaterials.Scoreboard))
			{
				skin.scoreboardMaterial.SetColor(this.shaderHashId, color);
			}
		}

		public GorillaSkinMaterials colorMaterials;

		public string shaderColorProperty;

		private ShaderHashId shaderHashId;
	}
}
