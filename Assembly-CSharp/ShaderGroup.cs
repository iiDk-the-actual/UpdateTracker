using System;
using UnityEngine;

[Serializable]
public struct ShaderGroup
{
	public ShaderGroup(Material material, Shader original, Shader gameplay, Shader baking)
	{
		this.material = material;
		this.originalShader = original;
		this.gameplayShader = gameplay;
		this.bakingShader = baking;
	}

	public Material material;

	public Shader originalShader;

	public Shader gameplayShader;

	public Shader bakingShader;
}
