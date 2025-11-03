using System;
using System.Collections.Generic;
using System.Linq;
using Critters.Scripts;
using UnityEngine;

public class CopySpawnerData : MonoBehaviour
{
	[ContextMenu("Copy Spawner Data")]
	private void CopySpawnerDataInPrefab()
	{
		if (this.spawnerDataParent != null)
		{
			Object.DestroyImmediate(this.spawnerDataParent.gameObject);
		}
		this.spawnerDataParent = new GameObject().transform;
		this.spawnerDataParent.name = "Spawner Data Parent";
		this.spawnerDataParent.parent = base.transform;
		this.spawnerDataParent.localPosition = Vector3.zero;
		this.spawnerDataParent.localRotation = Quaternion.identity;
		this.CopyEquipmentSpawner();
		this.CopyCageDeposits();
	}

	private void CopyCageDeposits()
	{
		List<CrittersCageDepositShim> list = Object.FindObjectsByType<CrittersCageDepositShim>(FindObjectsSortMode.None).ToList<CrittersCageDepositShim>();
		for (int i = 0; i < list.Count; i++)
		{
			CrittersCageDepositShim crittersCageDepositShim = list[i];
			GameObject gameObject = new GameObject();
			gameObject.transform.position = crittersCageDepositShim.transform.position;
			gameObject.transform.rotation = crittersCageDepositShim.transform.rotation;
			gameObject.layer = crittersCageDepositShim.gameObject.layer;
			gameObject.transform.parent = this.spawnerDataParent;
			gameObject.name = "Cage Deposit";
			CrittersCageDeposit crittersCageDeposit = gameObject.AddComponent<CrittersCageDeposit>();
			crittersCageDeposit.disableGrabOnAttach = crittersCageDepositShim.disableGrabOnAttach;
			crittersCageDeposit.allowMultiAttach = crittersCageDepositShim.allowMultiAttach;
			crittersCageDeposit.snapOnAttach = crittersCageDepositShim.snapOnAttach;
			crittersCageDeposit.depositStartLocation = crittersCageDepositShim.startLocation;
			crittersCageDeposit.depositEndLocation = crittersCageDepositShim.endLocation;
			crittersCageDeposit.submitDuration = crittersCageDepositShim.submitDuration;
			crittersCageDeposit.returnDuration = crittersCageDepositShim.returnDuration;
			crittersCageDeposit.depositStartSound = crittersCageDepositShim.depositStartSound;
			crittersCageDeposit.depositEmptySound = crittersCageDepositShim.depositEmptySound;
			crittersCageDeposit.depositCritterSound = crittersCageDepositShim.depositCritterSound;
			crittersCageDeposit.actorType = crittersCageDepositShim.type;
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.center = crittersCageDepositShim.cageBoxCollider.center;
			boxCollider.size = crittersCageDepositShim.cageBoxCollider.size;
			boxCollider.isTrigger = true;
			boxCollider.includeLayers = crittersCageDepositShim.cageBoxCollider.includeLayers;
			boxCollider.excludeLayers = crittersCageDepositShim.cageBoxCollider.excludeLayers;
			GameObject gameObject2 = new GameObject();
			gameObject2.name = "Attach Point";
			gameObject2.transform.parent = crittersCageDeposit.transform;
			CrittersActor crittersActor = gameObject2.AddComponent<CrittersActor>();
			crittersCageDeposit.attachPoint = crittersActor;
			crittersActor.isSceneActor = true;
			crittersActor.crittersActorType = crittersCageDepositShim.type;
			gameObject2.AddComponent<Rigidbody>().isKinematic = true;
			gameObject2.transform.position = crittersCageDepositShim.attachPointTransform.position;
			gameObject2.transform.rotation = crittersCageDepositShim.attachPointTransform.rotation;
			AudioSource audioSource = gameObject2.AddComponent<AudioSource>();
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.maxDistance = 15f;
			audioSource.spatialBlend = 1f;
			crittersCageDeposit.depositAudio = audioSource;
			GameObject gameObject3 = new GameObject();
			Transform child = crittersCageDepositShim.attachPointTransform.GetChild(0);
			gameObject3.transform.parent = gameObject2.transform;
			gameObject3.transform.position = child.position;
			gameObject3.transform.rotation = child.rotation;
			MeshFilter meshFilter = gameObject3.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
			gameObject3.AddComponent<MeshRenderer>().sharedMaterial = child.GetComponent<MeshRenderer>().sharedMaterial;
			gameObject3.layer = child.gameObject.layer;
			gameObject3.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
			ZoneBasedObject zoneBasedObject = gameObject3.AddComponent<ZoneBasedObject>();
			zoneBasedObject.zones = new GTZone[1];
			zoneBasedObject.zones[0] = GTZone.critters;
		}
	}

	private void CopyEquipmentSpawner()
	{
		List<CrittersActorSpawnerShim> list = Object.FindObjectsByType<CrittersActorSpawnerShim>(FindObjectsSortMode.None).ToList<CrittersActorSpawnerShim>();
		for (int i = 0; i < list.Count; i++)
		{
			CrittersActorSpawnerShim crittersActorSpawnerShim = list[i];
			GameObject gameObject = new GameObject();
			gameObject.transform.position = crittersActorSpawnerShim.transform.position;
			gameObject.transform.rotation = crittersActorSpawnerShim.transform.rotation;
			gameObject.layer = crittersActorSpawnerShim.gameObject.layer;
			gameObject.transform.parent = this.spawnerDataParent;
			gameObject.name = "Spawner " + crittersActorSpawnerShim.actorType.ToString();
			CrittersActorSpawner crittersActorSpawner = gameObject.AddComponent<CrittersActorSpawner>();
			crittersActorSpawner.actorType = crittersActorSpawnerShim.actorType;
			crittersActorSpawner.subActorIndex = crittersActorSpawnerShim.subActorIndex;
			crittersActorSpawner.spawnDelay = crittersActorSpawnerShim.spawnDelay;
			crittersActorSpawner.applyImpulseOnSpawn = crittersActorSpawnerShim.applyImpulseOnSpawn;
			crittersActorSpawner.attachSpawnedObjectToSpawnLocation = crittersActorSpawnerShim.attachSpawnedObjectToSpawnLocation;
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.center = crittersActorSpawnerShim.colliderTrigger.center;
			boxCollider.size = crittersActorSpawnerShim.colliderTrigger.size;
			boxCollider.isTrigger = true;
			GameObject gameObject2 = new GameObject();
			gameObject2.name = "Inside Spawner Bounds";
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.position = crittersActorSpawnerShim.insideSpawnerBounds.transform.position;
			gameObject2.transform.rotation = crittersActorSpawnerShim.insideSpawnerBounds.transform.rotation;
			gameObject2.transform.localScale = crittersActorSpawnerShim.insideSpawnerBounds.transform.localScale;
			BoxCollider boxCollider2 = gameObject2.AddComponent<BoxCollider>();
			boxCollider2.size = crittersActorSpawnerShim.insideSpawnerBounds.size;
			boxCollider2.center = crittersActorSpawnerShim.insideSpawnerBounds.center;
			boxCollider2.isTrigger = true;
			gameObject2.layer = crittersActorSpawnerShim.insideSpawnerBounds.gameObject.layer;
			crittersActorSpawner.insideSpawnerCheck = boxCollider2;
			GameObject gameObject3 = new GameObject();
			gameObject3.name = "Spawner Point";
			gameObject3.transform.parent = crittersActorSpawner.transform;
			gameObject3.AddComponent<CrittersActorSpawnerPoint>().isSceneActor = true;
			crittersActorSpawner.spawnPoint = gameObject3.GetComponent<CrittersActorSpawnerPoint>();
			crittersActorSpawner.spawnPoint.crittersActorType = CrittersActor.CrittersActorType.AttachPoint;
			gameObject3.AddComponent<Rigidbody>().isKinematic = true;
			gameObject3.transform.position = crittersActorSpawnerShim.spawnerPointTransform.position;
			gameObject3.transform.rotation = crittersActorSpawnerShim.spawnerPointTransform.rotation;
		}
	}

	public Transform spawnerDataParent;
}
