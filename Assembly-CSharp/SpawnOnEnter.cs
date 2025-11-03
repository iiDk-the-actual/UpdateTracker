using System;
using UnityEngine;

public class SpawnOnEnter : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		if (Time.time > this.lastSpawnTime + this.cooldown)
		{
			this.lastSpawnTime = Time.time;
			ObjectPools.instance.Instantiate(this.prefab, other.transform.position, true);
		}
	}

	public GameObject prefab;

	public float cooldown = 0.1f;

	private float lastSpawnTime;
}
