using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace MTAssets.EasyMeshCombiner
{
	[AddComponentMenu("MT Assets/Easy Mesh Combiner/Runtime Mesh Combiner")]
	public class RuntimeMeshCombiner : MonoBehaviour
	{
		private void Awake()
		{
			if (this.combineMeshesAtStartUp == RuntimeMeshCombiner.CombineOnStart.OnAwake)
			{
				if (this.showDebugLogs)
				{
					Debug.Log("The merge started in Runtime Combiner \"" + base.gameObject.name + "\".");
				}
				this.CombineMeshes();
			}
		}

		private void Start()
		{
			if (this.combineMeshesAtStartUp == RuntimeMeshCombiner.CombineOnStart.OnStart)
			{
				if (this.showDebugLogs)
				{
					Debug.Log("The merge started in Runtime Combiner \"" + base.gameObject.name + "\".");
				}
				this.CombineMeshes();
			}
		}

		private RuntimeMeshCombiner.GameObjectWithMesh[] GetValidatedTargetGameObjects()
		{
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < this.targetMeshes.Count; i++)
			{
				if (!(this.targetMeshes[i] == null))
				{
					if (this.combineInChildren)
					{
						foreach (Transform transform in this.targetMeshes[i].GetComponentsInChildren<Transform>(true))
						{
							if (!list.Contains(transform))
							{
								list.Add(transform);
							}
						}
					}
					if (!this.combineInChildren)
					{
						Transform component = this.targetMeshes[i].GetComponent<Transform>();
						if (!list.Contains(component))
						{
							list.Add(component);
						}
					}
				}
			}
			List<RuntimeMeshCombiner.GameObjectWithMesh> list2 = new List<RuntimeMeshCombiner.GameObjectWithMesh>();
			for (int k = 0; k < list.Count; k++)
			{
				MeshFilter component2 = list[k].GetComponent<MeshFilter>();
				MeshRenderer component3 = list[k].GetComponent<MeshRenderer>();
				if ((component2 != null || component3 != null) && (this.combineInactives || component3.enabled) && (this.combineInactives || list[k].gameObject.activeSelf) && (this.combineInactives || list[k].gameObject.activeInHierarchy))
				{
					list2.Add(new RuntimeMeshCombiner.GameObjectWithMesh(list[k].gameObject, component2, component3));
				}
			}
			List<RuntimeMeshCombiner.GameObjectWithMesh> list3 = new List<RuntimeMeshCombiner.GameObjectWithMesh>();
			for (int l = 0; l < list2.Count; l++)
			{
				bool flag = true;
				if (list2[l].meshFilter == null)
				{
					if (this.showDebugLogs)
					{
						Debug.LogError("GameObject \"" + list2[l].gameObject.name + "\" does not have the Mesh Filter component, so it is not a valid mesh and will be ignored in the merge process.");
					}
					flag = false;
				}
				if (list2[l].meshRenderer == null)
				{
					if (this.showDebugLogs)
					{
						Debug.LogError("GameObject \"" + list2[l].gameObject.name + "\" does not have the Mesh Renderer component, so it is not a valid mesh and will be ignored in the merge process.");
					}
					flag = false;
				}
				if (list2[l].meshFilter != null && list2[l].meshFilter.sharedMesh == null)
				{
					if (this.showDebugLogs)
					{
						Debug.LogError("GameObject \"" + list2[l].gameObject.name + "\" does not have a Mesh in Mesh Filter component, so it is not a valid mesh and will be ignored in the merge process.");
					}
					flag = false;
				}
				if (list2[l].meshFilter != null && list2[l].meshRenderer != null && list2[l].meshFilter.sharedMesh != null && list2[l].meshFilter.sharedMesh.subMeshCount != list2[l].meshRenderer.sharedMaterials.Length)
				{
					if (this.showDebugLogs)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"The Mesh Renderer component found in GameObject \"",
							list2[l].gameObject.name,
							"\" has more or less material needed. The mesh that is in this GameObject has ",
							list2[l].meshFilter.sharedMesh.subMeshCount.ToString(),
							" submeshes, but has a number of ",
							list2[l].meshRenderer.sharedMaterials.Length.ToString(),
							" materials. This mesh will be ignored during the merge process."
						}));
					}
					flag = false;
				}
				if (list2[l].meshRenderer != null)
				{
					for (int m = 0; m < list2[l].meshRenderer.sharedMaterials.Length; m++)
					{
						if (list2[l].meshRenderer.sharedMaterials[m] == null)
						{
							if (this.showDebugLogs)
							{
								Debug.LogError(string.Concat(new string[]
								{
									"Material ",
									m.ToString(),
									" in Mesh Renderer present in component \"",
									list2[l].gameObject.name,
									"\" is null. For the merge process to work well, all materials must be completed. This GameObject will be ignored in the merge process."
								}));
							}
							flag = false;
						}
					}
				}
				if (list2[l].gameObject.GetComponent<CombinedMeshesManager>() != null)
				{
					if (this.showDebugLogs)
					{
						Debug.LogError("GameObject \"" + list2[l].gameObject.name + "\" is the result of a previous merge, so it will be ignored by this merge.");
					}
					flag = false;
				}
				if (flag)
				{
					list3.Add(list2[l]);
				}
			}
			return list3.ToArray();
		}

		public bool CombineMeshes()
		{
			if (this.isTargetMeshesMerged())
			{
				if (this.showDebugLogs)
				{
					Debug.Log("The Runtime Combiner \"" + base.gameObject.name + "\" meshes are already combined!");
				}
				return true;
			}
			if (this.isTargetMeshesMerged())
			{
				return false;
			}
			if (base.gameObject.GetComponent<MeshFilter>() != null || base.gameObject.GetComponent<MeshRenderer>() != null)
			{
				if (this.showDebugLogs)
				{
					Debug.LogError("Unable to merge. Apparently the GameObject \"" + base.gameObject.name + "\" already contains the Mesh Filter and/or Mesh Renderer component. The Runtime Mesh Combiner needs a GameObject that does not contain these two components. Please remove them or place the Runtime Mesh Combiner in a new GameObject and try again.");
				}
				return false;
			}
			this.originalPosition = base.gameObject.transform.position;
			this.originalEulerAngles = base.gameObject.transform.eulerAngles;
			this.originalScale = base.gameObject.transform.lossyScale;
			base.gameObject.transform.position = Vector3.zero;
			base.gameObject.transform.eulerAngles = Vector3.zero;
			base.gameObject.transform.localScale = Vector3.one;
			RuntimeMeshCombiner.GameObjectWithMesh[] validatedTargetGameObjects = this.GetValidatedTargetGameObjects();
			if (validatedTargetGameObjects.Length == 0)
			{
				if (this.showDebugLogs)
				{
					Debug.LogError("No valid, meshed GameObjects were found in the target GameObjects list. Therefore the merge was interrupted.");
				}
				return false;
			}
			Dictionary<Material, List<RuntimeMeshCombiner.SubMeshToCombine>> dictionary = new Dictionary<Material, List<RuntimeMeshCombiner.SubMeshToCombine>>();
			foreach (RuntimeMeshCombiner.GameObjectWithMesh gameObjectWithMesh in validatedTargetGameObjects)
			{
				for (int j = 0; j < gameObjectWithMesh.meshFilter.sharedMesh.subMeshCount; j++)
				{
					Material material = gameObjectWithMesh.meshRenderer.sharedMaterials[j];
					if (dictionary.ContainsKey(material))
					{
						dictionary[material].Add(new RuntimeMeshCombiner.SubMeshToCombine(gameObjectWithMesh.gameObject.transform, gameObjectWithMesh.meshFilter, gameObjectWithMesh.meshRenderer, j));
					}
					if (!dictionary.ContainsKey(material))
					{
						dictionary.Add(material, new List<RuntimeMeshCombiner.SubMeshToCombine>
						{
							new RuntimeMeshCombiner.SubMeshToCombine(gameObjectWithMesh.gameObject.transform, gameObjectWithMesh.meshFilter, gameObjectWithMesh.meshRenderer, j)
						});
					}
				}
			}
			MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			int num = 0;
			foreach (RuntimeMeshCombiner.GameObjectWithMesh gameObjectWithMesh2 in validatedTargetGameObjects)
			{
				num += gameObjectWithMesh2.meshFilter.sharedMesh.vertexCount;
			}
			List<Mesh> list = new List<Mesh>();
			foreach (KeyValuePair<Material, List<RuntimeMeshCombiner.SubMeshToCombine>> keyValuePair in dictionary)
			{
				List<RuntimeMeshCombiner.SubMeshToCombine> value = keyValuePair.Value;
				List<CombineInstance> list2 = new List<CombineInstance>();
				for (int l = 0; l < value.Count; l++)
				{
					list2.Add(new CombineInstance
					{
						mesh = value[l].meshFilter.sharedMesh,
						subMeshIndex = value[l].subMeshIndex,
						transform = value[l].transform.localToWorldMatrix
					});
				}
				Mesh mesh = new Mesh();
				if (num <= this.MAX_VERTICES_FOR_16BITS_MESH)
				{
					mesh.indexFormat = IndexFormat.UInt16;
				}
				if (num > this.MAX_VERTICES_FOR_16BITS_MESH)
				{
					mesh.indexFormat = IndexFormat.UInt32;
				}
				mesh.CombineMeshes(list2.ToArray(), true, true);
				list.Add(mesh);
			}
			List<CombineInstance> list3 = new List<CombineInstance>();
			foreach (Mesh mesh2 in list)
			{
				list3.Add(new CombineInstance
				{
					mesh = mesh2,
					subMeshIndex = 0,
					transform = Matrix4x4.identity
				});
			}
			Mesh mesh3 = new Mesh();
			if (num <= this.MAX_VERTICES_FOR_16BITS_MESH)
			{
				mesh3.indexFormat = IndexFormat.UInt16;
			}
			if (num > this.MAX_VERTICES_FOR_16BITS_MESH)
			{
				mesh3.indexFormat = IndexFormat.UInt32;
			}
			mesh3.name = base.gameObject.name + " (Temp Merge)";
			mesh3.CombineMeshes(list3.ToArray(), false);
			mesh3.RecalculateBounds();
			if (this.recalculateNormals)
			{
				mesh3.RecalculateNormals();
			}
			if (this.recalculateTangents)
			{
				mesh3.RecalculateTangents();
			}
			if (this.optimizeResultingMesh)
			{
				mesh3.Optimize();
			}
			meshFilter.sharedMesh = mesh3;
			List<Material> list4 = new List<Material>();
			foreach (KeyValuePair<Material, List<RuntimeMeshCombiner.SubMeshToCombine>> keyValuePair2 in dictionary)
			{
				list4.Add(keyValuePair2.Key);
			}
			meshRenderer.sharedMaterials = list4.ToArray();
			if (this.afterMerge == RuntimeMeshCombiner.AfterMerge.DeactiveOriginalGameObjects)
			{
				foreach (RuntimeMeshCombiner.GameObjectWithMesh gameObjectWithMesh3 in validatedTargetGameObjects)
				{
					this.originalGameObjectsWithMeshToRestore.Add(new RuntimeMeshCombiner.OriginalGameObjectWithMesh(gameObjectWithMesh3.gameObject, gameObjectWithMesh3.gameObject.activeSelf, gameObjectWithMesh3.meshRenderer, gameObjectWithMesh3.meshRenderer.enabled));
					gameObjectWithMesh3.gameObject.SetActive(false);
				}
				if (this.addMeshColliderAfter)
				{
					base.gameObject.AddComponent<MeshCollider>();
				}
			}
			if (this.afterMerge == RuntimeMeshCombiner.AfterMerge.DisableOriginalMeshes)
			{
				foreach (RuntimeMeshCombiner.GameObjectWithMesh gameObjectWithMesh4 in validatedTargetGameObjects)
				{
					this.originalGameObjectsWithMeshToRestore.Add(new RuntimeMeshCombiner.OriginalGameObjectWithMesh(gameObjectWithMesh4.gameObject, gameObjectWithMesh4.gameObject.activeSelf, gameObjectWithMesh4.meshRenderer, gameObjectWithMesh4.meshRenderer.enabled));
					gameObjectWithMesh4.meshRenderer.enabled = false;
				}
			}
			RuntimeMeshCombiner.AfterMerge afterMerge = this.afterMerge;
			base.gameObject.transform.position = this.originalPosition;
			base.gameObject.transform.eulerAngles = this.originalEulerAngles;
			base.gameObject.transform.localScale = this.originalScale;
			if (this.showDebugLogs)
			{
				Debug.Log("The merge has been successfully completed in Runtime Combiner \"" + base.gameObject.name + "\"!");
			}
			if (this.onDoneMerge != null)
			{
				this.onDoneMerge.Invoke();
			}
			this.targetMeshesMerged = true;
			return true;
		}

		public bool UndoMerge()
		{
			if (!this.isTargetMeshesMerged())
			{
				if (this.showDebugLogs)
				{
					Debug.Log("The Runtime Combiner \"" + base.gameObject.name + "\" meshes are already uncombined!");
				}
				return true;
			}
			if (this.isTargetMeshesMerged())
			{
				if (this.afterMerge == RuntimeMeshCombiner.AfterMerge.DisableOriginalMeshes)
				{
					foreach (RuntimeMeshCombiner.OriginalGameObjectWithMesh originalGameObjectWithMesh in this.originalGameObjectsWithMeshToRestore)
					{
						if (!(originalGameObjectWithMesh.meshRenderer == null))
						{
							originalGameObjectWithMesh.meshRenderer.enabled = originalGameObjectWithMesh.originalMrState;
						}
					}
				}
				if (this.afterMerge == RuntimeMeshCombiner.AfterMerge.DeactiveOriginalGameObjects)
				{
					foreach (RuntimeMeshCombiner.OriginalGameObjectWithMesh originalGameObjectWithMesh2 in this.originalGameObjectsWithMeshToRestore)
					{
						if (!(originalGameObjectWithMesh2.gameObject == null))
						{
							originalGameObjectWithMesh2.gameObject.SetActive(originalGameObjectWithMesh2.originalGoState);
						}
					}
					if (this.addMeshColliderAfter)
					{
						MeshCollider component = base.GetComponent<MeshCollider>();
						if (component != null)
						{
							Object.Destroy(component);
						}
					}
				}
				RuntimeMeshCombiner.AfterMerge afterMerge = this.afterMerge;
				this.originalGameObjectsWithMeshToRestore.Clear();
				Object.Destroy(base.GetComponent<MeshRenderer>());
				Object.Destroy(base.GetComponent<MeshFilter>());
				if (this.garbageCollectorAfterUndo)
				{
					Resources.UnloadUnusedAssets();
					GC.Collect();
				}
				if (this.showDebugLogs)
				{
					Debug.Log("The Runtime Combiner \"" + base.gameObject.name + "\" merge was successfully undone!");
				}
				if (this.onDoneUnmerge != null)
				{
					this.onDoneUnmerge.Invoke();
				}
				this.targetMeshesMerged = false;
				return true;
			}
			return false;
		}

		public bool isTargetMeshesMerged()
		{
			return this.targetMeshesMerged;
		}

		private int MAX_VERTICES_FOR_16BITS_MESH = 50000;

		private Vector3 originalPosition = Vector3.zero;

		private Vector3 originalEulerAngles = Vector3.zero;

		private Vector3 originalScale = Vector3.zero;

		private List<RuntimeMeshCombiner.OriginalGameObjectWithMesh> originalGameObjectsWithMeshToRestore = new List<RuntimeMeshCombiner.OriginalGameObjectWithMesh>();

		private bool targetMeshesMerged;

		[HideInInspector]
		public RuntimeMeshCombiner.AfterMerge afterMerge;

		[HideInInspector]
		public bool addMeshColliderAfter = true;

		[HideInInspector]
		public RuntimeMeshCombiner.CombineOnStart combineMeshesAtStartUp;

		[HideInInspector]
		public bool combineInChildren;

		[HideInInspector]
		public bool combineInactives;

		[HideInInspector]
		public bool recalculateNormals = true;

		[HideInInspector]
		public bool recalculateTangents = true;

		[HideInInspector]
		public bool optimizeResultingMesh;

		[HideInInspector]
		public List<GameObject> targetMeshes = new List<GameObject>();

		[HideInInspector]
		public bool showDebugLogs = true;

		[HideInInspector]
		public bool garbageCollectorAfterUndo = true;

		public UnityEvent onDoneMerge;

		public UnityEvent onDoneUnmerge;

		private class GameObjectWithMesh
		{
			public GameObjectWithMesh(GameObject gameObject, MeshFilter meshFilter, MeshRenderer meshRenderer)
			{
				this.gameObject = gameObject;
				this.meshFilter = meshFilter;
				this.meshRenderer = meshRenderer;
			}

			public GameObject gameObject;

			public MeshFilter meshFilter;

			public MeshRenderer meshRenderer;
		}

		private class OriginalGameObjectWithMesh
		{
			public OriginalGameObjectWithMesh(GameObject gameObject, bool originalGoState, MeshRenderer meshRenderer, bool originalMrState)
			{
				this.gameObject = gameObject;
				this.originalGoState = originalGoState;
				this.meshRenderer = meshRenderer;
				this.originalMrState = originalMrState;
			}

			public GameObject gameObject;

			public bool originalGoState;

			public MeshRenderer meshRenderer;

			public bool originalMrState;
		}

		private class SubMeshToCombine
		{
			public SubMeshToCombine(Transform transform, MeshFilter meshFilter, MeshRenderer meshRenderer, int subMeshIndex)
			{
				this.transform = transform;
				this.meshFilter = meshFilter;
				this.meshRenderer = meshRenderer;
				this.subMeshIndex = subMeshIndex;
			}

			public Transform transform;

			public MeshFilter meshFilter;

			public MeshRenderer meshRenderer;

			public int subMeshIndex;
		}

		public enum CombineOnStart
		{
			Disabled,
			OnStart,
			OnAwake
		}

		public enum AfterMerge
		{
			DisableOriginalMeshes,
			DeactiveOriginalGameObjects,
			DoNothing
		}
	}
}
