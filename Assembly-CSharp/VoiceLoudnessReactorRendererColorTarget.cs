using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoiceLoudnessReactorRendererColorTarget
{
	public void Inititialize()
	{
		if (this._materials == null)
		{
			this._materials = new List<Material>(this.renderer.materials);
			this._materials[this.materialIndex].EnableKeyword(this.colorProperty);
			this.renderer.SetMaterials(this._materials);
			this.UpdateMaterialColor(0f);
		}
	}

	public void UpdateMaterialColor(float level)
	{
		Color color = this.gradient.Evaluate(level);
		if (this._lastColor == color)
		{
			return;
		}
		this._materials[this.materialIndex].SetColor(this.colorProperty, color);
		this._lastColor = color;
	}

	[SerializeField]
	private string colorProperty = "_BaseColor";

	public Renderer renderer;

	public int materialIndex;

	public Gradient gradient;

	public bool useSmoothedLoudness;

	public float scale = 1f;

	private List<Material> _materials;

	private Color _lastColor = Color.white;
}
