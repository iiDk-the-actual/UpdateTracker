using System;
using Critters.Scripts;
using UnityEngine;

public class CrittersActorSpawnerShim : MonoBehaviour
{
	[ContextMenu("Copy Spawner Data To Shim")]
	private CrittersActorSpawner CopySpawnerDataInPrefab()
	{
		CrittersActorSpawner component = base.gameObject.GetComponent<CrittersActorSpawner>();
		this.spawnerPointTransform = component.spawnPoint.transform;
		this.actorType = component.actorType;
		this.subActorIndex = component.subActorIndex;
		this.insideSpawnerBounds = (BoxCollider)component.insideSpawnerCheck;
		this.spawnDelay = component.spawnDelay;
		this.applyImpulseOnSpawn = component.applyImpulseOnSpawn;
		this.attachSpawnedObjectToSpawnLocation = component.attachSpawnedObjectToSpawnLocation;
		this.colliderTrigger = base.gameObject.GetComponent<BoxCollider>();
		return component;
	}

	[ContextMenu("Replace Spawner With Shim")]
	private void ReplaceSpawnerWithShim()
	{
		CrittersActorSpawner crittersActorSpawner = this.CopySpawnerDataInPrefab();
		if (crittersActorSpawner.spawnPoint.GetComponent<Rigidbody>() != null)
		{
			Object.DestroyImmediate(crittersActorSpawner.spawnPoint.GetComponent<Rigidbody>());
		}
		Object.DestroyImmediate(crittersActorSpawner.spawnPoint);
		Object.DestroyImmediate(crittersActorSpawner);
	}

	public Transform spawnerPointTransform;

	public CrittersActor.CrittersActorType actorType;

	public int subActorIndex;

	public BoxCollider insideSpawnerBounds;

	public int spawnDelay;

	public bool applyImpulseOnSpawn;

	public bool attachSpawnedObjectToSpawnLocation;

	public BoxCollider colliderTrigger;
}
