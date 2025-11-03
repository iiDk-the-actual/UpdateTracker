using System;
using GorillaTag.Reactions;
using UnityEngine;

[RequireComponent(typeof(SpawnWorldEffects))]
public class SpawnWorldEffectsTrigger : MonoBehaviour
{
	private void OnEnable()
	{
		if (this.swe == null)
		{
			this.swe = base.GetComponent<SpawnWorldEffects>();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		this.spawnTime = Time.time;
		this.swe.RequestSpawn(base.transform.position);
	}

	private void OnTriggerStay(Collider other)
	{
		if (Time.time - this.spawnTime < this.spawnCooldown)
		{
			return;
		}
		this.swe.RequestSpawn(base.transform.position);
		this.spawnTime = Time.time;
	}

	private SpawnWorldEffects swe;

	private float spawnTime;

	[SerializeField]
	private float spawnCooldown = 1f;
}
