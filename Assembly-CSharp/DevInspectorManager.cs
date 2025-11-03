using System;
using UnityEngine;

public class DevInspectorManager : MonoBehaviour
{
	public static DevInspectorManager instance
	{
		get
		{
			if (DevInspectorManager._instance == null)
			{
				DevInspectorManager._instance = Object.FindAnyObjectByType<DevInspectorManager>();
			}
			return DevInspectorManager._instance;
		}
	}

	private static DevInspectorManager _instance;
}
