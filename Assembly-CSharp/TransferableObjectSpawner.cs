using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class TransferableObjectSpawner : MonoBehaviour
{
	public void Awake()
	{
		GameObject[] transferrableObjectsToSpawn = this.TransferrableObjectsToSpawn;
		for (int i = 0; i < transferrableObjectsToSpawn.Length; i++)
		{
			TransferrableObject componentInChildren = transferrableObjectsToSpawn[i].GetComponentInChildren<TransferrableObject>();
			if (componentInChildren.IsNotNull())
			{
				this.objectsToSpawn.Add(componentInChildren);
			}
			else
			{
				Debug.LogError("Failed to add object " + componentInChildren.gameObject.name + " - missing a Transferrable object");
			}
		}
	}

	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			return;
		}
		foreach (GameObject gameObject in this.TransferrableObjectsToSpawn)
		{
			TransferrableObject componentInChildren = gameObject.GetComponentInChildren<TransferrableObject>();
			if (componentInChildren.IsNull())
			{
				Debug.LogError(string.Concat(new string[]
				{
					base.name,
					" at path ",
					this.GetComponentPath(int.MaxValue),
					" has ",
					gameObject.name,
					" assigned to TransferrableObjectsToSpawn collection, but it does not have a TransferrableObject component.  It will not spawn."
				}));
			}
			else if (componentInChildren.worldShareableInstance == null)
			{
				Debug.LogError(string.Concat(new string[]
				{
					base.name,
					" at path ",
					this.GetComponentPath(int.MaxValue),
					" has ",
					gameObject.name,
					" assigned to TransferrableObjectsToSpawn collection, but it's worldShareableInstance is null."
				}));
			}
		}
	}

	public void Update()
	{
		if (this.spawnTrigger == TransferableObjectSpawner.SpawnTrigger.Timer && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.Time > this.lastSpawnTime + this.SpawnDelay)
		{
			this.SpawnTransferrableObject();
			this.lastSpawnTime = PhotonNetwork.Time;
		}
	}

	private bool SpawnOnGround()
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(base.transform.position + Random.insideUnitCircle.x0y() * this.spawnRadius, Vector3.down), out raycastHit, 3f, this.groundRaycastMask))
		{
			this.spawnPosition = raycastHit.point;
			this.spawnRotation = Quaternion.FromToRotation(Vector3.up, raycastHit.normal);
			return true;
		}
		return false;
	}

	private void SpawnAtCurrentLocation()
	{
		this.spawnPosition = base.transform.position;
		this.spawnRotation = base.transform.rotation;
	}

	public void SpawnTransferrableObject()
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		TransferableObjectSpawner.SpawnMode spawnMode = this.spawnMode;
		if (spawnMode != TransferableObjectSpawner.SpawnMode.OnGround)
		{
			if (spawnMode != TransferableObjectSpawner.SpawnMode.AtCurrentTransform)
			{
				return;
			}
			this.SpawnAtCurrentLocation();
		}
		else if (!this.SpawnOnGround())
		{
			return;
		}
		TransferrableObject transferrableObject = null;
		int num = 0;
		foreach (TransferrableObject transferrableObject2 in this.objectsToSpawn)
		{
			if (!transferrableObject2.InHand())
			{
				num++;
				if (Random.Range(0, num) == 0)
				{
					transferrableObject = transferrableObject2;
				}
			}
		}
		if (transferrableObject != null)
		{
			if (!transferrableObject.IsLocalOwnedWorldShareable)
			{
				transferrableObject.WorldShareableRequestOwnership();
			}
			if (transferrableObject.worldShareableInstance != null)
			{
				transferrableObject.transform.SetPositionAndRotation(this.spawnPosition, this.spawnRotation);
				transferrableObject.worldShareableInstance.SetWillTeleport();
				return;
			}
			Debug.LogError("WorldShareableInstance for " + transferrableObject.name + " is null");
		}
	}

	private Vector3 spawnPosition = Vector3.zero;

	private Quaternion spawnRotation = Quaternion.identity;

	[SerializeField]
	private GameObject[] TransferrableObjectsToSpawn;

	private List<TransferrableObject> objectsToSpawn = new List<TransferrableObject>();

	[SerializeField]
	private TransferableObjectSpawner.SpawnMode spawnMode;

	[SerializeField]
	private TransferableObjectSpawner.SpawnTrigger spawnTrigger;

	[SerializeField]
	private double SpawnDelay = 5.0;

	private double lastSpawnTime;

	[SerializeField]
	private LayerMask groundRaycastMask = LayerMask.NameToLayer("Gorilla Object");

	[SerializeField]
	private float spawnRadius = 0.5f;

	private enum SpawnMode
	{
		OnGround,
		AtCurrentTransform
	}

	private enum SpawnTrigger
	{
		Timer
	}
}
