using System;
using UnityEngine;

public class DelayedDestroyCrittersPooledObject : MonoBehaviour
{
	protected void OnEnable()
	{
		if (ObjectPools.instance == null || !ObjectPools.instance.initialized)
		{
			return;
		}
		this.timeToDie = Time.time + this.destroyDelay;
	}

	protected void LateUpdate()
	{
		if (Time.time >= this.timeToDie)
		{
			CrittersPool.Return(base.gameObject);
		}
	}

	public float destroyDelay = 1f;

	private float timeToDie = -1f;
}
