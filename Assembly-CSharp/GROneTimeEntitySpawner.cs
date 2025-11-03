using System;
using UnityEngine;

public class GROneTimeEntitySpawner : MonoBehaviour
{
	private void Start()
	{
		if (this.EntityPrefab == null)
		{
			Debug.Log("Can't  spawn null entity", this);
		}
		base.Invoke("TrySpawn", this.SpawnDelay);
	}

	private void Update()
	{
	}

	private void TrySpawn()
	{
		if (!this.bHasSpawned && this.EntityPrefab != null)
		{
			Debug.Log("trying to spawn entity" + this.EntityPrefab.name, this);
			GameEntityManager gameEntityManager = this.reactor.grManager.gameEntityManager;
			if (gameEntityManager.IsAuthority())
			{
				if (!gameEntityManager.IsZoneActive())
				{
					Debug.Log("delaying spawn attempt because zone not active", this);
					base.Invoke("TrySpawn", 0.2f);
					return;
				}
				Debug.Log("trying to spawn entity", this);
				gameEntityManager.RequestCreateItem(this.EntityPrefab.name.GetStaticHash(), base.transform.position + new Vector3(0f, 0f, 0f), base.transform.rotation, 0L);
				this.bHasSpawned = true;
			}
		}
	}

	public GhostReactor reactor;

	public GameEntity EntityPrefab;

	private bool bHasSpawned;

	private float SpawnDelay = 3f;
}
