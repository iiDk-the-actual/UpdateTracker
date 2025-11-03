using System;
using UnityEngine;

[Serializable]
public struct PrefabType
{
	public GameObject prefab;

	public string prefabName;

	public bool roomObject;

	public int photonViewCount;
}
