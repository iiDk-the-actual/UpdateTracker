using System;
using UnityEngine;

public class BakeBlendShape : MonoBehaviour
{
	private void Update()
	{
		Mesh mesh = new Mesh();
		MeshCollider component = base.GetComponent<MeshCollider>();
		base.GetComponent<SkinnedMeshRenderer>().BakeMesh(mesh);
		component.sharedMesh = null;
		component.sharedMesh = mesh;
	}
}
