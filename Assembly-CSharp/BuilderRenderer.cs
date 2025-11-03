using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

public class BuilderRenderer : MonoBehaviourPostTick
{
	private void Awake()
	{
		this.InitIfNeeded();
	}

	public void InitIfNeeded()
	{
		if (this.initialized)
		{
			return;
		}
		this.initialized = true;
		this.snapPieceShader = Shader.Find("GorillaTag/SnapPiece");
		if (this.renderData == null)
		{
			this.renderData = new BuilderTableDataRenderData();
		}
		this.renderData.materialToIndex = new Dictionary<Material, int>(256);
		this.renderData.materials = new List<Material>(256);
		if (this.renderData.meshToIndex == null)
		{
			this.renderData.meshToIndex = new Dictionary<Mesh, int>(1024);
		}
		if (this.renderData.meshInstanceCount == null)
		{
			this.renderData.meshInstanceCount = new List<int>(1024);
		}
		if (this.renderData.meshes == null)
		{
			this.renderData.meshes = new List<Mesh>(4096);
		}
		if (this.renderData.textureToIndex == null)
		{
			this.renderData.textureToIndex = new Dictionary<Texture2D, int>(256);
		}
		if (this.renderData.textures == null)
		{
			this.renderData.textures = new List<Texture2D>(256);
		}
		if (this.renderData.perTextureMaterial == null)
		{
			this.renderData.perTextureMaterial = new List<Material>(256);
		}
		if (this.renderData.perTexturePropertyBlock == null)
		{
			this.renderData.perTexturePropertyBlock = new List<MaterialPropertyBlock>(256);
		}
		if (this.renderData.sharedMaterial == null)
		{
			this.renderData.sharedMaterial = new Material(this.sharedMaterialBase);
		}
		if (this.renderData.sharedMaterialIndirect == null)
		{
			this.renderData.sharedMaterialIndirect = new Material(this.sharedMaterialIndirectBase);
		}
		this.built = false;
		this.showing = false;
	}

	public void Show(bool show)
	{
		this.showing = show;
	}

	public void BuildRenderer(List<BuilderPiece> piecePrefabs)
	{
		this.InitIfNeeded();
		for (int i = 0; i < piecePrefabs.Count; i++)
		{
			if (piecePrefabs[i] != null)
			{
				this.AddPrefab(piecePrefabs[i]);
			}
			else
			{
				Debug.LogErrorFormat("Prefab at {0} is null", new object[] { i });
			}
		}
		this.BuildSharedMaterial();
		this.BuildSharedMesh();
		this.BuildBuffer();
		this.built = true;
	}

	public void LogDraws()
	{
		Debug.LogFormat("Builder Renderer Counts {0} {1} {2} {3}", new object[]
		{
			this.renderData.subMeshes.Length,
			this.renderData.textures.Count,
			this.renderData.dynamicBatch.totalInstances,
			this.renderData.staticBatch.totalInstances
		});
	}

	public override void PostTick()
	{
		if (!this.built || !this.showing)
		{
			return;
		}
		this.RenderIndirect();
	}

	public void WriteSerializedData()
	{
		if (this.renderData == null)
		{
			return;
		}
		if (this.renderData.sharedMesh != null)
		{
			this.serializeMeshToIndexKeys = new List<Mesh>(this.renderData.meshToIndex.Count);
			this.serializeMeshToIndexValues = new List<int>(this.renderData.meshToIndex.Count);
			foreach (KeyValuePair<Mesh, int> keyValuePair in this.renderData.meshToIndex)
			{
				this.serializeMeshToIndexKeys.Add(keyValuePair.Key);
				this.serializeMeshToIndexValues.Add(keyValuePair.Value);
			}
			this.serializeMeshes = this.renderData.meshes;
			this.serializeMeshInstanceCount = this.renderData.meshInstanceCount;
			this.serializeSubMeshes = new List<BuilderTableSubMesh>(512);
			foreach (BuilderTableSubMesh builderTableSubMesh in this.renderData.subMeshes)
			{
				this.serializeSubMeshes.Add(builderTableSubMesh);
			}
			this.serializeSharedMesh = this.renderData.sharedMesh;
		}
		if (this.renderData.sharedMaterial != null)
		{
			this.serializeTextureToIndexKeys = new List<Texture2D>(this.renderData.textureToIndex.Count);
			this.serializeTextureToIndexValues = new List<int>(this.renderData.textureToIndex.Count);
			foreach (KeyValuePair<Texture2D, int> keyValuePair2 in this.renderData.textureToIndex)
			{
				this.serializeTextureToIndexKeys.Add(keyValuePair2.Key);
				this.serializeTextureToIndexValues.Add(keyValuePair2.Value);
			}
			this.serializeTextures = this.renderData.textures;
			this.serializePerTextureMaterial = this.renderData.perTextureMaterial;
			this.serializePerTexturePropertyBlock = this.renderData.perTexturePropertyBlock;
			this.serializeSharedTexArray = this.renderData.sharedTexArray;
			this.serializeSharedMaterial = this.renderData.sharedMaterial;
			this.serializeSharedMaterialIndirect = this.renderData.sharedMaterialIndirect;
		}
	}

	private void ApplySerializedData()
	{
		if (this.serializeSharedMesh != null)
		{
			if (this.renderData == null)
			{
				this.renderData = new BuilderTableDataRenderData();
			}
			this.renderData.meshToIndex = new Dictionary<Mesh, int>(1024);
			for (int i = 0; i < this.serializeMeshToIndexKeys.Count; i++)
			{
				this.renderData.meshToIndex.Add(this.serializeMeshToIndexKeys[i], this.serializeMeshToIndexValues[i]);
			}
			this.renderData.meshes = this.serializeMeshes;
			this.renderData.meshInstanceCount = this.serializeMeshInstanceCount;
			this.renderData.subMeshes = new NativeList<BuilderTableSubMesh>(512, Allocator.Persistent);
			foreach (BuilderTableSubMesh builderTableSubMesh in this.serializeSubMeshes)
			{
				this.renderData.subMeshes.AddNoResize(builderTableSubMesh);
			}
			this.renderData.sharedMesh = this.serializeSharedMesh;
		}
		if (this.serializeSharedMaterial != null)
		{
			if (this.renderData == null)
			{
				this.renderData = new BuilderTableDataRenderData();
			}
			this.renderData.textureToIndex = new Dictionary<Texture2D, int>(256);
			for (int j = 0; j < this.serializeTextureToIndexKeys.Count; j++)
			{
				this.renderData.textureToIndex.Add(this.serializeTextureToIndexKeys[j], this.serializeTextureToIndexValues[j]);
			}
			this.renderData.textures = this.serializeTextures;
			this.renderData.perTextureMaterial = this.serializePerTextureMaterial;
			this.renderData.perTexturePropertyBlock = this.serializePerTexturePropertyBlock;
			this.renderData.sharedTexArray = this.serializeSharedTexArray;
			this.renderData.sharedMaterial = this.serializeSharedMaterial;
			this.renderData.sharedMaterialIndirect = this.serializeSharedMaterialIndirect;
		}
	}

	public void AddPrefab(BuilderPiece prefab)
	{
		BuilderRenderer.meshRenderers.Clear();
		prefab.GetComponentsInChildren<MeshRenderer>(true, BuilderRenderer.meshRenderers);
		for (int i = 0; i < BuilderRenderer.meshRenderers.Count; i++)
		{
			MeshRenderer meshRenderer = BuilderRenderer.meshRenderers[i];
			Material sharedMaterial = meshRenderer.sharedMaterial;
			if (sharedMaterial == null)
			{
				if (!prefab.suppressMaterialWarnings)
				{
					Debug.LogErrorFormat("{0} {1} is missing a buidler material", new object[] { prefab.name, meshRenderer.name });
				}
			}
			else if (!this.AddMaterial(sharedMaterial, prefab.suppressMaterialWarnings))
			{
				if (!prefab.suppressMaterialWarnings)
				{
					Debug.LogWarningFormat("{0} {1} failed to add builder material", new object[] { prefab.name, meshRenderer.name });
				}
			}
			else if (this.renderData.sharedMesh == null)
			{
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				if (component != null)
				{
					Mesh sharedMesh = component.sharedMesh;
					int num;
					if (sharedMesh != null && !this.renderData.meshToIndex.TryGetValue(sharedMesh, out num))
					{
						this.renderData.meshToIndex.Add(sharedMesh, this.renderData.meshToIndex.Count);
						this.renderData.meshInstanceCount.Add(0);
						for (int j = 0; j < 1; j++)
						{
							this.renderData.meshes.Add(sharedMesh);
						}
					}
				}
			}
		}
		if (prefab.materialOptions != null)
		{
			for (int k = 0; k < prefab.materialOptions.options.Count; k++)
			{
				Material material = prefab.materialOptions.options[k].material;
				if (!this.AddMaterial(material, prefab.suppressMaterialWarnings) && !prefab.suppressMaterialWarnings)
				{
					Debug.LogWarningFormat("builder material options {0} bad material index {1}", new object[]
					{
						prefab.materialOptions.name,
						k
					});
				}
			}
		}
	}

	private bool AddMaterial(Material material, bool suppressWarnings = false)
	{
		if (material == null)
		{
			return false;
		}
		if (material.shader != this.snapPieceShader)
		{
			if (!suppressWarnings)
			{
				Debug.LogWarningFormat("builder: material {0} uses non snap piece shader {1}", new object[]
				{
					material.name,
					material.shader.name
				});
			}
			return false;
		}
		if (!material.HasTexture("_BaseMap"))
		{
			if (!suppressWarnings)
			{
				Debug.LogWarningFormat("builder material {0} does not have texture property {1}", new object[] { material.name, "_BaseMap" });
			}
			return false;
		}
		Texture texture = material.GetTexture("_BaseMap");
		if (texture == null)
		{
			if (!suppressWarnings)
			{
				Debug.LogWarningFormat("builder material {0} null texture", new object[] { material.name });
			}
			return false;
		}
		Texture2D texture2D = texture as Texture2D;
		if (texture2D == null)
		{
			if (!suppressWarnings)
			{
				Debug.LogWarningFormat("builder material {0} no texture2d type is {1}", new object[]
				{
					material.name,
					texture.GetType()
				});
			}
			return false;
		}
		if (texture2D.width != 256 || texture2D.height != 256)
		{
			if (!suppressWarnings)
			{
				Debug.LogWarningFormat("builder texture {0} unexpected size {1} {2}", new object[] { texture2D.name, texture2D.width, texture2D.height });
			}
			return false;
		}
		int num;
		if (!this.renderData.materialToIndex.TryGetValue(material, out num))
		{
			this.renderData.materialToIndex.Add(material, this.renderData.materials.Count);
			this.renderData.materials.Add(material);
		}
		int num2;
		if (!this.renderData.textureToIndex.TryGetValue(texture2D, out num2))
		{
			this.renderData.textureToIndex.Add(texture2D, this.renderData.textures.Count);
			this.renderData.textures.Add(texture2D);
			if (this.renderData.textures.Count == 1)
			{
				this.renderData.textureFormat = texture2D.format;
				this.renderData.texWidth = texture2D.width;
				this.renderData.texHeight = texture2D.height;
			}
		}
		return true;
	}

	public void BuildSharedMaterial()
	{
		if (this.renderData.sharedTexArray != null)
		{
			Debug.Log("Already have shared material. Not building new one.");
			return;
		}
		TextureFormat textureFormat = TextureFormat.RGBA32;
		this.renderData.sharedTexArray = new Texture2DArray(this.renderData.texWidth, this.renderData.texHeight, this.renderData.textures.Count, textureFormat, true);
		this.renderData.sharedTexArray.filterMode = FilterMode.Point;
		for (int i = 0; i < this.renderData.textures.Count; i++)
		{
			this.renderData.sharedTexArray.SetPixels(this.renderData.textures[i].GetPixels(), i);
		}
		this.renderData.sharedTexArray.Apply(true, true);
		this.renderData.sharedMaterial.SetTexture("_BaseMapArray", this.renderData.sharedTexArray);
		this.renderData.sharedMaterialIndirect.SetTexture("_BaseMapArray", this.renderData.sharedTexArray);
		this.renderData.sharedMaterialIndirect.enableInstancing = true;
		for (int j = 0; j < this.renderData.textures.Count; j++)
		{
			Material material = new Material(this.renderData.sharedMaterial);
			material.SetInt("_BaseMapArrayIndex", j);
			this.renderData.perTextureMaterial.Add(material);
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetInt("_BaseMapArrayIndex", j);
			this.renderData.perTexturePropertyBlock.Add(materialPropertyBlock);
		}
	}

	public void BuildSharedMesh()
	{
		if (this.renderData.sharedMesh != null)
		{
			Debug.Log("Already have shared mesh. Not building new one.");
			return;
		}
		this.renderData.sharedMesh = new Mesh();
		this.renderData.sharedMesh.indexFormat = IndexFormat.UInt32;
		BuilderRenderer.verticesAll.Clear();
		BuilderRenderer.normalsAll.Clear();
		BuilderRenderer.uv1All.Clear();
		BuilderRenderer.trianglesAll.Clear();
		this.renderData.subMeshes = new NativeList<BuilderTableSubMesh>(512, Allocator.Persistent);
		for (int i = 0; i < this.renderData.meshes.Count; i++)
		{
			Mesh mesh = this.renderData.meshes[i];
			int count = BuilderRenderer.trianglesAll.Count;
			int count2 = BuilderRenderer.verticesAll.Count;
			BuilderRenderer.vertices.Clear();
			BuilderRenderer.normals.Clear();
			BuilderRenderer.uv1.Clear();
			BuilderRenderer.triangles.Clear();
			mesh.GetVertices(BuilderRenderer.vertices);
			mesh.GetNormals(BuilderRenderer.normals);
			mesh.GetUVs(0, BuilderRenderer.uv1);
			mesh.GetTriangles(BuilderRenderer.triangles, 0);
			BuilderRenderer.verticesAll.AddRange(BuilderRenderer.vertices);
			BuilderRenderer.normalsAll.AddRange(BuilderRenderer.normals);
			BuilderRenderer.uv1All.AddRange(BuilderRenderer.uv1);
			BuilderRenderer.trianglesAll.AddRange(BuilderRenderer.triangles);
			int num = BuilderRenderer.trianglesAll.Count - count;
			BuilderTableSubMesh builderTableSubMesh = new BuilderTableSubMesh
			{
				startIndex = count,
				indexCount = num,
				startVertex = count2
			};
			this.renderData.subMeshes.Add(in builderTableSubMesh);
		}
		this.renderData.sharedMesh.SetVertices(BuilderRenderer.verticesAll);
		this.renderData.sharedMesh.SetNormals(BuilderRenderer.normalsAll);
		this.renderData.sharedMesh.SetUVs(0, BuilderRenderer.uv1All);
		this.renderData.sharedMesh.SetTriangles(BuilderRenderer.trianglesAll, 0);
	}

	public void BuildBuffer()
	{
		this.renderData.dynamicBatch = new BuilderTableDataRenderIndirectBatch();
		BuilderRenderer.BuildBatch(this.renderData.dynamicBatch, this.renderData.meshes.Count, 8192, this.renderData.sharedMaterialIndirect);
		this.renderData.staticBatch = new BuilderTableDataRenderIndirectBatch();
		BuilderRenderer.BuildBatch(this.renderData.staticBatch, this.renderData.meshes.Count, 8192, this.renderData.sharedMaterialIndirect);
	}

	public static void BuildBatch(BuilderTableDataRenderIndirectBatch indirectBatch, int meshCount, int maxInstances, Material sharedMaterialIndirect)
	{
		indirectBatch.totalInstances = 0;
		indirectBatch.commandCount = meshCount;
		indirectBatch.commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, indirectBatch.commandCount, 20);
		indirectBatch.commandData = new NativeArray<GraphicsBuffer.IndirectDrawIndexedArgs>(indirectBatch.commandCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		indirectBatch.matrixBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxInstances * 2, 64);
		indirectBatch.texIndexBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxInstances * 2, 4);
		indirectBatch.tintBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxInstances * 2, 4);
		indirectBatch.instanceTransform = new TransformAccessArray(maxInstances, 3);
		indirectBatch.instanceTransformIndexToDataIndex = new NativeArray<int>(maxInstances, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		for (int i = 0; i < maxInstances; i++)
		{
			indirectBatch.instanceTransformIndexToDataIndex[i] = -1;
		}
		indirectBatch.pieceIDPerTransform = new List<int>(maxInstances);
		indirectBatch.instanceObjectToWorld = new NativeArray<Matrix4x4>(maxInstances * 2, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		indirectBatch.instanceTexIndex = new NativeArray<int>(maxInstances * 2, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		indirectBatch.instanceTint = new NativeArray<float>(maxInstances * 2, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		indirectBatch.renderMeshes = new NativeList<BuilderTableMeshInstances>(512, Allocator.Persistent);
		for (int j = 0; j < meshCount; j++)
		{
			BuilderTableMeshInstances builderTableMeshInstances = new BuilderTableMeshInstances
			{
				transforms = new TransformAccessArray(maxInstances, 3),
				texIndex = new NativeList<int>(Allocator.Persistent),
				tint = new NativeList<float>(Allocator.Persistent)
			};
			indirectBatch.renderMeshes.Add(in builderTableMeshInstances);
		}
		indirectBatch.rp = new RenderParams(sharedMaterialIndirect);
		indirectBatch.rp.worldBounds = new Bounds(Vector3.zero, 10000f * Vector3.one);
		indirectBatch.rp.matProps = new MaterialPropertyBlock();
		indirectBatch.rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.identity);
		indirectBatch.matrixBuf.SetData<Matrix4x4>(indirectBatch.instanceObjectToWorld);
		indirectBatch.texIndexBuf.SetData<int>(indirectBatch.instanceTexIndex);
		indirectBatch.tintBuf.SetData<float>(indirectBatch.instanceTint);
		indirectBatch.rp.matProps.SetBuffer("_TransformMatrix", indirectBatch.matrixBuf);
		indirectBatch.rp.matProps.SetBuffer("_TexIndex", indirectBatch.texIndexBuf);
		indirectBatch.rp.matProps.SetBuffer("_Tint", indirectBatch.tintBuf);
	}

	private void OnDestroy()
	{
		this.DestroyBuffer();
		this.renderData.subMeshes.Dispose();
	}

	public void DestroyBuffer()
	{
		BuilderRenderer.DestroyBatch(this.renderData.staticBatch);
		BuilderRenderer.DestroyBatch(this.renderData.dynamicBatch);
	}

	public static void DestroyBatch(BuilderTableDataRenderIndirectBatch indirectBatch)
	{
		indirectBatch.commandBuf.Dispose();
		indirectBatch.commandData.Dispose();
		indirectBatch.matrixBuf.Dispose();
		indirectBatch.texIndexBuf.Dispose();
		indirectBatch.tintBuf.Dispose();
		indirectBatch.instanceTransform.Dispose();
		indirectBatch.instanceTransformIndexToDataIndex.Dispose();
		indirectBatch.instanceObjectToWorld.Dispose();
		indirectBatch.instanceTexIndex.Dispose();
		indirectBatch.instanceTint.Dispose();
		foreach (BuilderTableMeshInstances builderTableMeshInstances in indirectBatch.renderMeshes)
		{
			TransformAccessArray transforms = builderTableMeshInstances.transforms;
			transforms.Dispose();
			NativeList<int> texIndex = builderTableMeshInstances.texIndex;
			texIndex.Dispose();
			NativeList<float> tint = builderTableMeshInstances.tint;
			tint.Dispose();
		}
		indirectBatch.renderMeshes.Dispose();
	}

	public void PreRenderIndirect()
	{
		if (!this.built || !this.showing)
		{
			return;
		}
		this.renderData.setupInstancesJobs = default(JobHandle);
		BuilderRenderer.SetupIndirectBatchArgs(this.renderData.staticBatch, this.renderData.subMeshes);
		BuilderRenderer.SetupInstanceDataForMeshStatic setupInstanceDataForMeshStatic = new BuilderRenderer.SetupInstanceDataForMeshStatic
		{
			transformIndexToDataIndex = this.renderData.staticBatch.instanceTransformIndexToDataIndex,
			objectToWorld = this.renderData.staticBatch.instanceObjectToWorld
		};
		this.renderData.setupInstancesJobs = setupInstanceDataForMeshStatic.ScheduleReadOnly(this.renderData.staticBatch.instanceTransform, 32, default(JobHandle));
		JobHandle.ScheduleBatchedJobs();
	}

	public void RenderIndirect()
	{
		this.renderData.setupInstancesJobs.Complete();
		this.RenderIndirectBatch(this.renderData.staticBatch);
	}

	private static void SetupIndirectBatchArgs(BuilderTableDataRenderIndirectBatch indirectBatch, NativeList<BuilderTableSubMesh> subMeshes)
	{
		uint num = 0U;
		for (int i = 0; i < indirectBatch.commandCount; i++)
		{
			BuilderTableMeshInstances builderTableMeshInstances = indirectBatch.renderMeshes[i];
			BuilderTableSubMesh builderTableSubMesh = subMeshes[i];
			GraphicsBuffer.IndirectDrawIndexedArgs indirectDrawIndexedArgs = default(GraphicsBuffer.IndirectDrawIndexedArgs);
			indirectDrawIndexedArgs.indexCountPerInstance = (uint)builderTableSubMesh.indexCount;
			indirectDrawIndexedArgs.startIndex = (uint)builderTableSubMesh.startIndex;
			indirectDrawIndexedArgs.baseVertexIndex = (uint)builderTableSubMesh.startVertex;
			indirectDrawIndexedArgs.startInstance = num;
			indirectDrawIndexedArgs.instanceCount = (uint)(builderTableMeshInstances.transforms.length * 2);
			num += indirectDrawIndexedArgs.instanceCount;
			indirectBatch.commandData[i] = indirectDrawIndexedArgs;
		}
	}

	private void RenderIndirectBatch(BuilderTableDataRenderIndirectBatch indirectBatch)
	{
		indirectBatch.matrixBuf.SetData<Matrix4x4>(indirectBatch.instanceObjectToWorld);
		indirectBatch.texIndexBuf.SetData<int>(indirectBatch.instanceTexIndex);
		indirectBatch.tintBuf.SetData<float>(indirectBatch.instanceTint);
		indirectBatch.commandBuf.SetData<GraphicsBuffer.IndirectDrawIndexedArgs>(indirectBatch.commandData);
		Graphics.RenderMeshIndirect(in indirectBatch.rp, this.renderData.sharedMesh, indirectBatch.commandBuf, indirectBatch.commandCount, 0);
	}

	public void AddPiece(BuilderPiece piece)
	{
		bool isStatic = piece.isStatic;
		BuilderRenderer.meshRenderers.Clear();
		piece.GetComponentsInChildren<MeshRenderer>(false, BuilderRenderer.meshRenderers);
		for (int i = 0; i < BuilderRenderer.meshRenderers.Count; i++)
		{
			MeshRenderer meshRenderer = BuilderRenderer.meshRenderers[i];
			if (meshRenderer.enabled)
			{
				Material material = meshRenderer.material;
				if (material.HasTexture("_BaseMap"))
				{
					Texture2D texture2D = material.GetTexture("_BaseMap") as Texture2D;
					if (!(texture2D == null))
					{
						int num;
						if (!this.renderData.textureToIndex.TryGetValue(texture2D, out num))
						{
							if (!piece.suppressMaterialWarnings)
							{
								Debug.LogWarningFormat("builder piece {0} material {1} texture not found in render data", new object[] { piece.displayName, material.name });
							}
						}
						else
						{
							MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
							if (!(component == null))
							{
								Mesh sharedMesh = component.sharedMesh;
								if (!(sharedMesh == null))
								{
									int num2;
									if (!this.renderData.meshToIndex.TryGetValue(sharedMesh, out num2))
									{
										Debug.LogWarningFormat("builder piece {0} mesh {1} not found in render data", new object[] { piece.displayName, meshRenderer.name });
									}
									else
									{
										int num3 = this.renderData.meshInstanceCount[num2] % 1;
										this.renderData.meshInstanceCount[num2] = this.renderData.meshInstanceCount[num2] + 1;
										num2 += num3;
										int num4 = -1;
										if (isStatic)
										{
											NativeArray<int> instanceTransformIndexToDataIndex = this.renderData.staticBatch.instanceTransformIndexToDataIndex;
											int length = this.renderData.staticBatch.instanceTransform.length;
											if (length + 2 >= instanceTransformIndexToDataIndex.Length)
											{
												GTDev.LogError<string>("Too Many Builder Mesh Instances", null);
												return;
											}
											num4 = length;
											BuilderTableMeshInstances builderTableMeshInstances = this.renderData.staticBatch.renderMeshes[num2];
											int num5 = 0;
											for (int j = 0; j <= num2; j++)
											{
												num5 += this.renderData.staticBatch.renderMeshes[j].transforms.length * 2;
											}
											for (int k = 0; k < length; k++)
											{
												if (this.renderData.staticBatch.instanceTransformIndexToDataIndex[k] >= num5)
												{
													this.renderData.staticBatch.instanceTransformIndexToDataIndex[k] = this.renderData.staticBatch.instanceTransformIndexToDataIndex[k] + 2;
												}
											}
											this.renderData.staticBatch.pieceIDPerTransform.Add(piece.pieceId);
											this.renderData.staticBatch.instanceTransform.Add(meshRenderer.transform);
											this.renderData.staticBatch.instanceTransformIndexToDataIndex[num4] = num5;
											builderTableMeshInstances.transforms.Add(meshRenderer.transform);
											builderTableMeshInstances.texIndex.Add(in num);
											builderTableMeshInstances.tint.Add(in piece.tint);
											int num6 = this.renderData.staticBatch.totalInstances - 1;
											for (int l = num6; l >= num5; l--)
											{
												this.renderData.staticBatch.instanceTexIndex[l + 2] = this.renderData.staticBatch.instanceTexIndex[l];
											}
											for (int m = num6; m >= num5; m--)
											{
												this.renderData.staticBatch.instanceObjectToWorld[m + 2] = this.renderData.staticBatch.instanceObjectToWorld[m];
											}
											for (int n = num6; n >= num5; n--)
											{
												this.renderData.staticBatch.instanceTint[n + 2] = this.renderData.staticBatch.instanceTint[n];
											}
											for (int num7 = 0; num7 < 2; num7++)
											{
												this.renderData.staticBatch.instanceObjectToWorld[num5 + num7] = meshRenderer.transform.localToWorldMatrix;
												this.renderData.staticBatch.instanceTexIndex[num5 + num7] = num;
												this.renderData.staticBatch.instanceTint[num5 + num7] = 1f;
												this.renderData.staticBatch.totalInstances++;
											}
										}
										else
										{
											BuilderTableMeshInstances builderTableMeshInstances2 = this.renderData.dynamicBatch.renderMeshes[num2];
											builderTableMeshInstances2.transforms.Add(meshRenderer.transform);
											builderTableMeshInstances2.texIndex.Add(in num);
											builderTableMeshInstances2.tint.Add(in piece.tint);
											this.renderData.dynamicBatch.totalInstances++;
										}
										piece.renderingIndirect.Add(meshRenderer);
										piece.renderingDirect.Remove(meshRenderer);
										piece.renderingIndirectTransformIndex.Add(num4);
										meshRenderer.enabled = false;
									}
								}
							}
						}
					}
				}
			}
		}
	}

	public void RemovePiece(BuilderPiece piece)
	{
		bool isStatic = piece.isStatic;
		for (int i = 0; i < piece.renderingIndirect.Count; i++)
		{
			MeshRenderer meshRenderer = piece.renderingIndirect[i];
			if (!(meshRenderer == null))
			{
				Material sharedMaterial = meshRenderer.sharedMaterial;
				if (sharedMaterial.HasTexture("_BaseMap"))
				{
					Texture2D texture2D = sharedMaterial.GetTexture("_BaseMap") as Texture2D;
					int num;
					if (!(texture2D == null) && this.renderData.textureToIndex.TryGetValue(texture2D, out num))
					{
						MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
						if (!(component == null))
						{
							Mesh sharedMesh = component.sharedMesh;
							int num2;
							if (this.renderData.meshToIndex.TryGetValue(sharedMesh, out num2))
							{
								Transform transform = meshRenderer.transform;
								bool flag = false;
								int num3 = 0;
								int num4 = -1;
								if (isStatic)
								{
									for (int j = 0; j < num2; j++)
									{
										num3 += this.renderData.staticBatch.renderMeshes[j].transforms.length;
									}
									TransformAccessArray instanceTransform = this.renderData.staticBatch.instanceTransform;
									int length = instanceTransform.length;
									int num5 = piece.renderingIndirectTransformIndex[i];
									num4 = this.renderData.staticBatch.instanceTransformIndexToDataIndex[num5];
									int num6 = this.renderData.staticBatch.instanceTransform.length - 1;
									int num7 = this.renderData.staticBatch.pieceIDPerTransform[num6];
									this.renderData.staticBatch.instanceTransform.RemoveAtSwapBack(num5);
									this.renderData.staticBatch.pieceIDPerTransform.RemoveAtSwapBack(num5);
									this.renderData.staticBatch.instanceTransformIndexToDataIndex[num5] = this.renderData.staticBatch.instanceTransformIndexToDataIndex[num6];
									this.renderData.staticBatch.instanceTransformIndexToDataIndex[num6] = -1;
									BuilderPiece piece2 = piece.GetTable().GetPiece(num7);
									if (piece2 != null)
									{
										for (int k = 0; k < piece2.renderingIndirectTransformIndex.Count; k++)
										{
											if (piece2.renderingIndirectTransformIndex[k] == num6)
											{
												piece2.renderingIndirectTransformIndex[k] = num5;
											}
										}
									}
									for (int l = 0; l < length; l++)
									{
										if (this.renderData.staticBatch.instanceTransformIndexToDataIndex[l] > num4)
										{
											this.renderData.staticBatch.instanceTransformIndexToDataIndex[l] = this.renderData.staticBatch.instanceTransformIndexToDataIndex[l] - 2;
										}
									}
								}
								for (int m = 0; m < 1; m++)
								{
									int num8 = num2 + m;
									if (isStatic)
									{
										BuilderTableMeshInstances builderTableMeshInstances = this.renderData.staticBatch.renderMeshes[num8];
										for (int n = 0; n < builderTableMeshInstances.transforms.length; n++)
										{
											if (builderTableMeshInstances.transforms[n] == transform)
											{
												num3 += n;
												BuilderRenderer.RemoveAt(builderTableMeshInstances.transforms, n);
												builderTableMeshInstances.texIndex.RemoveAt(n);
												builderTableMeshInstances.tint.RemoveAt(n);
												flag = true;
												this.renderData.staticBatch.totalInstances -= 2;
												break;
											}
										}
									}
									else
									{
										BuilderTableMeshInstances builderTableMeshInstances2 = this.renderData.dynamicBatch.renderMeshes[num8];
										for (int num9 = 0; num9 < builderTableMeshInstances2.transforms.length; num9++)
										{
											if (builderTableMeshInstances2.transforms[num9] == transform)
											{
												BuilderRenderer.RemoveAt(builderTableMeshInstances2.transforms, num9);
												builderTableMeshInstances2.texIndex.RemoveAt(num9);
												builderTableMeshInstances2.tint.RemoveAt(num9);
												flag = true;
												this.renderData.dynamicBatch.totalInstances--;
												break;
											}
										}
									}
									if (flag)
									{
										piece.renderingDirect.Add(meshRenderer);
										break;
									}
								}
								if (flag && isStatic)
								{
									int num10 = this.renderData.staticBatch.totalInstances + 1;
									for (int num11 = num4; num11 < num10; num11++)
									{
										this.renderData.staticBatch.instanceTexIndex[num11] = this.renderData.staticBatch.instanceTexIndex[num11 + 2];
									}
									for (int num12 = num4; num12 < num10; num12++)
									{
										this.renderData.staticBatch.instanceObjectToWorld[num12] = this.renderData.staticBatch.instanceObjectToWorld[num12 + 2];
									}
									for (int num13 = num4; num13 < num10; num13++)
									{
										this.renderData.staticBatch.instanceTint[num13] = this.renderData.staticBatch.instanceTint[num13 + 2];
									}
								}
								meshRenderer.enabled = true;
							}
						}
					}
				}
			}
		}
		piece.renderingIndirect.Clear();
		piece.renderingIndirectTransformIndex.Clear();
	}

	public void ChangePieceIndirectMaterial(BuilderPiece piece, List<MeshRenderer> targetRenderers, Material targetMaterial)
	{
		if (targetMaterial == null)
		{
			return;
		}
		if (!targetMaterial.HasTexture("_BaseMap"))
		{
			Debug.LogError("New Material is missing a texture");
			return;
		}
		Texture2D texture2D = targetMaterial.GetTexture("_BaseMap") as Texture2D;
		if (texture2D == null)
		{
			Debug.LogError("New Material does not have a \"_BaseMap\" property");
			return;
		}
		int num;
		if (!this.renderData.textureToIndex.TryGetValue(texture2D, out num))
		{
			Debug.LogError("New Material is not in the texture array");
			return;
		}
		bool isStatic = piece.isStatic;
		for (int i = 0; i < piece.renderingIndirect.Count; i++)
		{
			MeshRenderer meshRenderer = piece.renderingIndirect[i];
			if (!targetRenderers.Contains(meshRenderer))
			{
				Debug.Log("renderer not in target list");
			}
			else
			{
				meshRenderer.material = targetMaterial;
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				if (!(component == null))
				{
					Mesh sharedMesh = component.sharedMesh;
					int num2;
					if (this.renderData.meshToIndex.TryGetValue(sharedMesh, out num2))
					{
						Transform transform = meshRenderer.transform;
						bool flag = false;
						if (isStatic)
						{
							int num3 = piece.renderingIndirectTransformIndex[i];
							int num4 = this.renderData.staticBatch.instanceTransformIndexToDataIndex[num3];
							if (num4 >= 0)
							{
								for (int j = 0; j < 2; j++)
								{
									this.renderData.staticBatch.instanceTexIndex[num4 + j] = num;
								}
							}
						}
						else
						{
							for (int k = 0; k < 1; k++)
							{
								int num5 = num2 + k;
								BuilderTableMeshInstances builderTableMeshInstances = this.renderData.dynamicBatch.renderMeshes[num5];
								for (int l = 0; l < builderTableMeshInstances.transforms.length; l++)
								{
									if (builderTableMeshInstances.transforms[l] == transform)
									{
										this.renderData.dynamicBatch.renderMeshes.ElementAt(num5).texIndex[l] = num;
										flag = true;
										break;
									}
								}
								if (flag)
								{
									break;
								}
							}
						}
					}
				}
			}
		}
	}

	public static void RemoveAt(TransformAccessArray a, int i)
	{
		int length = a.length;
		for (int j = i; j < length - 1; j++)
		{
			a[j] = a[j + 1];
		}
		a.RemoveAtSwapBack(length - 1);
	}

	public void SetPieceTint(BuilderPiece piece, float tint)
	{
		for (int i = 0; i < piece.renderingIndirect.Count; i++)
		{
			MeshRenderer meshRenderer = piece.renderingIndirect[i];
			Material sharedMaterial = meshRenderer.sharedMaterial;
			if (sharedMaterial.HasTexture("_BaseMap"))
			{
				Texture2D texture2D = sharedMaterial.GetTexture("_BaseMap") as Texture2D;
				int num;
				if (!(texture2D == null) && this.renderData.textureToIndex.TryGetValue(texture2D, out num))
				{
					MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
					if (!(component == null))
					{
						Mesh sharedMesh = component.sharedMesh;
						int num2;
						if (this.renderData.meshToIndex.TryGetValue(sharedMesh, out num2))
						{
							Transform transform = meshRenderer.transform;
							if (piece.isStatic)
							{
								int num3 = piece.renderingIndirectTransformIndex[i];
								int num4 = this.renderData.staticBatch.instanceTransformIndexToDataIndex[num3];
								if (num4 >= 0)
								{
									for (int j = 0; j < 2; j++)
									{
										this.renderData.staticBatch.instanceTint[num4 + j] = tint;
									}
								}
							}
							else
							{
								for (int k = 0; k < 1; k++)
								{
									int num5 = num2 + k;
									BuilderTableMeshInstances builderTableMeshInstances = this.renderData.dynamicBatch.renderMeshes[num5];
									for (int l = 0; l < builderTableMeshInstances.transforms.length; l++)
									{
										if (builderTableMeshInstances.transforms[l] == transform)
										{
											builderTableMeshInstances.tint[l] = tint;
											break;
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	public Material sharedMaterialBase;

	public Material sharedMaterialIndirectBase;

	public const int TEX_SIZE = 256;

	private Shader snapPieceShader;

	public BuilderTableDataRenderData renderData;

	[SerializeField]
	[HideInInspector]
	private List<Mesh> serializeMeshToIndexKeys;

	[SerializeField]
	[HideInInspector]
	private List<int> serializeMeshToIndexValues;

	[SerializeField]
	[HideInInspector]
	private List<Mesh> serializeMeshes;

	[SerializeField]
	[HideInInspector]
	private List<int> serializeMeshInstanceCount;

	[SerializeField]
	[HideInInspector]
	private List<BuilderTableSubMesh> serializeSubMeshes;

	[SerializeField]
	[HideInInspector]
	private Mesh serializeSharedMesh;

	[SerializeField]
	[HideInInspector]
	private List<Texture2D> serializeTextureToIndexKeys;

	[SerializeField]
	[HideInInspector]
	private List<int> serializeTextureToIndexValues;

	[SerializeField]
	[HideInInspector]
	private List<Texture2D> serializeTextures;

	[SerializeField]
	[HideInInspector]
	private List<Material> serializePerTextureMaterial;

	[SerializeField]
	[HideInInspector]
	private List<MaterialPropertyBlock> serializePerTexturePropertyBlock;

	[SerializeField]
	[HideInInspector]
	private Texture2DArray serializeSharedTexArray;

	[SerializeField]
	[HideInInspector]
	private Material serializeSharedMaterial;

	[SerializeField]
	[HideInInspector]
	private Material serializeSharedMaterialIndirect;

	private const string texturePropName = "_BaseMap";

	private const string textureArrayPropName = "_BaseMapArray";

	private const string textureArrayIndexPropName = "_BaseMapArrayIndex";

	private const string transformMatrixPropName = "_TransformMatrix";

	private const string texIndexPropName = "_TexIndex";

	private const string tintPropName = "_Tint";

	public const int MAX_STATIC_INSTANCES = 8192;

	public const int MAX_DYNAMIC_INSTANCES = 8192;

	public const int INSTANCES_PER_TRANSFORM = 2;

	private bool initialized;

	private bool built;

	private bool showing;

	private static List<MeshRenderer> meshRenderers = new List<MeshRenderer>(128);

	private const int MAX_TOTAL_VERTS = 65536;

	private const int MAX_TOTAL_TRIS = 65536;

	private static List<Vector3> verticesAll = new List<Vector3>(65536);

	private static List<Vector3> normalsAll = new List<Vector3>(65536);

	private static List<Vector2> uv1All = new List<Vector2>(65536);

	private static List<int> trianglesAll = new List<int>(65536);

	private static List<Vector3> vertices = new List<Vector3>(65536);

	private static List<Vector3> normals = new List<Vector3>(65536);

	private static List<Vector2> uv1 = new List<Vector2>(65536);

	private static List<int> triangles = new List<int>(65536);

	[BurstCompile]
	public struct SetupInstanceDataForMesh : IJobParallelForTransform
	{
		public void Execute(int index, TransformAccess transform)
		{
			int num = index + (int)this.commandData.startInstance;
			this.objectToWorld[num] = transform.localToWorldMatrix;
			this.instanceTexIndex[num] = this.texIndex[index];
			this.instanceTint[num] = this.tint[index];
		}

		[ReadOnly]
		public NativeList<int> texIndex;

		[ReadOnly]
		public NativeList<float> tint;

		[ReadOnly]
		public GraphicsBuffer.IndirectDrawIndexedArgs commandData;

		[ReadOnly]
		public Vector3 cameraPos;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> instanceTexIndex;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Matrix4x4> objectToWorld;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float> instanceTint;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> lodLevel;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> lodDirty;
	}

	[BurstCompile]
	public struct SetupInstanceDataForMeshStatic : IJobParallelForTransform
	{
		public void Execute(int index, TransformAccess transform)
		{
			if (transform.isValid)
			{
				int num = this.transformIndexToDataIndex[index];
				for (int i = 0; i < 2; i++)
				{
					this.objectToWorld[num + i] = transform.localToWorldMatrix;
				}
			}
		}

		[ReadOnly]
		public NativeArray<int> transformIndexToDataIndex;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Matrix4x4> objectToWorld;
	}
}
