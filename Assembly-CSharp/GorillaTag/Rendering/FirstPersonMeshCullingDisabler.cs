using System;
using UnityEngine;

namespace GorillaTag.Rendering
{
	public class FirstPersonMeshCullingDisabler : MonoBehaviour
	{
		protected void Awake()
		{
			MeshFilter[] componentsInChildren = base.GetComponentsInChildren<MeshFilter>();
			if (componentsInChildren == null)
			{
				return;
			}
			this.meshes = new Mesh[componentsInChildren.Length];
			this.xforms = new Transform[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				this.meshes[i] = componentsInChildren[i].mesh;
				this.xforms[i] = componentsInChildren[i].transform;
			}
		}

		protected void OnEnable()
		{
			Camera main = Camera.main;
			if (main == null)
			{
				return;
			}
			Transform transform = main.transform;
			Vector3 position = transform.position;
			Vector3 vector = Vector3.Normalize(transform.forward);
			float nearClipPlane = main.nearClipPlane;
			float num = (main.farClipPlane - nearClipPlane) / 2f + nearClipPlane;
			Vector3 vector2 = position + vector * num;
			for (int i = 0; i < this.meshes.Length; i++)
			{
				Vector3 vector3 = this.xforms[i].InverseTransformPoint(vector2);
				this.meshes[i].bounds = new Bounds(vector3, Vector3.one);
			}
		}

		private Mesh[] meshes;

		private Transform[] xforms;
	}
}
