using System;
using System.Collections.Generic;
using UnityEngine;

public class CrittersPool : MonoBehaviour
{
	public static GameObject GetPooled(GameObject prefab)
	{
		CrittersPool crittersPool = CrittersPool.instance;
		if (crittersPool == null)
		{
			return null;
		}
		return crittersPool.GetInstance(prefab);
	}

	public static void Return(GameObject pooledGO)
	{
		CrittersPool crittersPool = CrittersPool.instance;
		if (crittersPool == null)
		{
			return;
		}
		crittersPool.ReturnInstance(pooledGO);
	}

	private void Awake()
	{
		if (CrittersPool.instance != null)
		{
			Object.Destroy(this);
			return;
		}
		CrittersPool.instance = this;
		this.SetupPools();
	}

	private void SetupPools()
	{
		this.pools = new Dictionary<GameObject, List<GameObject>>();
		this.poolParent = new GameObject("CrittersPool")
		{
			transform = 
			{
				parent = base.transform
			}
		}.transform;
		for (int i = 0; i < this.eventEffects.Length; i++)
		{
			CrittersPool.CrittersPoolSettings crittersPoolSettings = this.eventEffects[i];
			if (crittersPoolSettings.poolObject == null || crittersPoolSettings.poolSize <= 0)
			{
				GTDev.Log<string>("CrittersPool.SetupPools Failed. Pool has no poolObject or has size 0.", null);
			}
			else
			{
				List<GameObject> list = new List<GameObject>();
				for (int j = 0; j < crittersPoolSettings.poolSize; j++)
				{
					GameObject gameObject = Object.Instantiate<GameObject>(crittersPoolSettings.poolObject);
					gameObject.transform.SetParent(this.poolParent);
					GameObject gameObject2 = gameObject;
					gameObject2.name += j.ToString();
					gameObject.SetActive(false);
					list.Add(gameObject);
				}
				this.pools.Add(crittersPoolSettings.poolObject, list);
			}
		}
	}

	private GameObject GetInstance(GameObject prefab)
	{
		List<GameObject> list;
		if (this.pools.TryGetValue(prefab, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && !list[i].activeSelf)
				{
					list[i].SetActive(true);
					return list[i];
				}
			}
			GTDev.Log<string>("CrittersPool.GetInstance Failed. No available instance.", null);
			return null;
		}
		GTDev.LogError<string>("CrittersPool.GetInstance Failed. Prefab doesn't have a valid pool setup.", null);
		return null;
	}

	private void ReturnInstance(GameObject instance)
	{
		instance.transform.SetParent(this.poolParent);
		instance.SetActive(false);
	}

	private static CrittersPool instance;

	public CrittersPool.CrittersPoolSettings[] eventEffects;

	private Dictionary<GameObject, List<GameObject>> pools;

	public Transform poolParent;

	[Serializable]
	public class CrittersPoolSettings
	{
		public GameObject poolObject;

		public int poolSize = 20;
	}
}
