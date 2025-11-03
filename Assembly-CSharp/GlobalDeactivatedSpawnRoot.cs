using System;
using UnityEngine;

public static class GlobalDeactivatedSpawnRoot
{
	public static Transform GetOrCreate()
	{
		if (!GlobalDeactivatedSpawnRoot._xform)
		{
			GlobalDeactivatedSpawnRoot._xform = new GameObject("GlobalDeactivatedSpawnRoot").transform;
			GlobalDeactivatedSpawnRoot._xform.gameObject.SetActive(false);
			Object.DontDestroyOnLoad(GlobalDeactivatedSpawnRoot._xform.gameObject);
		}
		GlobalDeactivatedSpawnRoot._xform.gameObject.SetActive(false);
		return GlobalDeactivatedSpawnRoot._xform;
	}

	private static Transform _xform;
}
