using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BuilderTableDataRenderData
{
	public const int NUM_SPLIT_MESH_INSTANCE_GROUPS = 1;

	public int texWidth;

	public int texHeight;

	public TextureFormat textureFormat;

	public Dictionary<Material, int> materialToIndex;

	public List<Material> materials;

	public Material sharedMaterial;

	public Material sharedMaterialIndirect;

	public Dictionary<Texture2D, int> textureToIndex;

	public List<Texture2D> textures;

	public List<Material> perTextureMaterial;

	public List<MaterialPropertyBlock> perTexturePropertyBlock;

	public Texture2DArray sharedTexArray;

	public Dictionary<Mesh, int> meshToIndex;

	public List<Mesh> meshes;

	public List<int> meshInstanceCount;

	public NativeList<BuilderTableSubMesh> subMeshes;

	public Mesh sharedMesh;

	public BuilderTableDataRenderIndirectBatch dynamicBatch;

	public BuilderTableDataRenderIndirectBatch staticBatch;

	public JobHandle setupInstancesJobs;
}
