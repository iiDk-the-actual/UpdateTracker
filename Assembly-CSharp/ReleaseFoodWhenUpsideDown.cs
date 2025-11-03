using System;
using Critters.Scripts;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class ReleaseFoodWhenUpsideDown : MonoBehaviour
{
	private void Awake()
	{
		this.latch = false;
	}

	private void Update()
	{
		if (!CrittersManager.instance.LocalAuthority())
		{
			return;
		}
		if (!this.dispenser.heldByPlayer)
		{
			return;
		}
		if (Vector3.Angle(base.transform.up, Vector3.down) < this.angle)
		{
			if (this.latch)
			{
				return;
			}
			this.latch = true;
			if (this.nextSpawnTime > (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)))
			{
				return;
			}
			this.nextSpawnTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time)) + (double)this.spawnDelay;
			CrittersActor crittersActor = CrittersManager.instance.SpawnActor(CrittersActor.CrittersActorType.Food, this.foodSubIndex);
			if (!crittersActor.IsNull())
			{
				CrittersFood crittersFood = (CrittersFood)crittersActor;
				crittersFood.MoveActor(this.spawnPoint.position, this.spawnPoint.rotation, false, true, true);
				crittersFood.SetImpulseVelocity(Vector3.zero, Vector3.zero);
				crittersFood.SpawnData(this.maxFood, this.startingFood, this.startingSize);
				return;
			}
		}
		else
		{
			this.latch = false;
		}
	}

	public CrittersFoodDispenser dispenser;

	public float angle = 30f;

	private bool latch;

	public Transform spawnPoint;

	public float maxFood;

	public float startingFood;

	public float startingSize;

	public int foodSubIndex;

	public float spawnDelay = 0.6f;

	private double nextSpawnTime;
}
