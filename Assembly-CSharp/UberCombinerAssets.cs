using System;
using UnityEngine;

public class UberCombinerAssets : ScriptableObject
{
	public static UberCombinerAssets Instance
	{
		get
		{
			UberCombinerAssets.gInstance == null;
			return UberCombinerAssets.gInstance;
		}
	}

	private void OnEnable()
	{
		this.Setup();
	}

	private void Setup()
	{
	}

	public void ClearMaterialAssets()
	{
	}

	public void ClearPrefabAssets()
	{
	}

	[SerializeField]
	private Object _rootFolder;

	[SerializeField]
	private Object _resourcesFolder;

	[SerializeField]
	private Object _materialsFolder;

	[SerializeField]
	private Object _prefabsFolder;

	[Space]
	public Object MeshBakerDefaultCustomizer;

	public Material ReferenceUberMaterial;

	public Shader TextureArrayCapableShader;

	[Space]
	public string RootFolderPath;

	public string ResourcesFolderPath;

	public string MaterialsFolderPath;

	public string PrefabsFolderPath;

	private static UberCombinerAssets gInstance;
}
