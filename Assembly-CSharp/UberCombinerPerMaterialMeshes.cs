using System;
using UnityEngine;

public class UberCombinerPerMaterialMeshes : MonoBehaviour
{
	public GameObject rootObject;

	public bool deleteSelfOnPrefabBake;

	[Space]
	public GameObject[] objects = new GameObject[0];

	public MeshRenderer[] renderers = new MeshRenderer[0];

	public MeshFilter[] filters = new MeshFilter[0];

	public Material[] materials = new Material[0];
}
