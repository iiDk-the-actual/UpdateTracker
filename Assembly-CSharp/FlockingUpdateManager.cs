using System;
using System.Collections.Generic;
using UnityEngine;

public class FlockingUpdateManager : MonoBehaviour
{
	protected void Awake()
	{
		if (FlockingUpdateManager.hasInstance && FlockingUpdateManager.instance != null && FlockingUpdateManager.instance != this)
		{
			Object.Destroy(this);
			return;
		}
		FlockingUpdateManager.SetInstance(this);
	}

	public static void CreateManager()
	{
		FlockingUpdateManager.SetInstance(new GameObject("FlockingUpdateManager").AddComponent<FlockingUpdateManager>());
	}

	private static void SetInstance(FlockingUpdateManager manager)
	{
		FlockingUpdateManager.instance = manager;
		FlockingUpdateManager.hasInstance = true;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(manager);
		}
	}

	public static void RegisterFlocking(Flocking flocking)
	{
		if (!FlockingUpdateManager.hasInstance)
		{
			FlockingUpdateManager.CreateManager();
		}
		if (!FlockingUpdateManager.allFlockings.Contains(flocking))
		{
			FlockingUpdateManager.allFlockings.Add(flocking);
		}
	}

	public static void UnregisterFlocking(Flocking flocking)
	{
		if (!FlockingUpdateManager.hasInstance)
		{
			FlockingUpdateManager.CreateManager();
		}
		if (FlockingUpdateManager.allFlockings.Contains(flocking))
		{
			FlockingUpdateManager.allFlockings.Remove(flocking);
		}
	}

	public void Update()
	{
		for (int i = 0; i < FlockingUpdateManager.allFlockings.Count; i++)
		{
			FlockingUpdateManager.allFlockings[i].InvokeUpdate();
		}
	}

	public static FlockingUpdateManager instance;

	public static bool hasInstance = false;

	public static List<Flocking> allFlockings = new List<Flocking>();
}
