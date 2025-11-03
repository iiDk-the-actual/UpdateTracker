using System;
using UnityEngine;

public class MaterialMapping : ScriptableObject
{
	public void CleanUpData()
	{
	}

	private static string path = "Assets/UberShaderConversion/MaterialMap.asset";

	public static string materialDirectory = "Assets/UberShaderConversion/Materials/";

	private static MaterialMapping instance;

	public ShaderGroup[] map;

	public Material mirrorMat;

	public RenderTexture mirrorTexture;
}
