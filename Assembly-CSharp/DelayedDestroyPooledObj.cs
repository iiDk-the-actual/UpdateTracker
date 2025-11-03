using System;
using UnityEngine;

public class DelayedDestroyPooledObj : MonoBehaviour
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
		if (Time.time > this.timeToDie)
		{
			ObjectPools.instance.Destroy(base.gameObject);
		}
	}

	[Tooltip("Return to the object pool after this many seconds.")]
	public float destroyDelay;

	private float timeToDie = -1f;
}
