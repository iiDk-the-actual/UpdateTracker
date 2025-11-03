using System;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	public Transform[] ChildrenXfs()
	{
		return base.transform.GetComponentsInChildren<Transform>();
	}
}
