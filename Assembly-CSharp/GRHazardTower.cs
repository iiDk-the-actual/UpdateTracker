using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GRHazardTower : MonoBehaviour, IGameEntityComponent, IGameProjectileLauncher
{
	public void OnEntityInit()
	{
		this.gameEntity.MinTimeBetweenTicks = 0.5f;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnTick = (Action)Delegate.Combine(gameEntity.OnTick, new Action(this.OnThink));
		this.senseNearby.Setup(this.fireFrom);
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	public void OnThink()
	{
		if (!this.gameEntity.IsAuthority())
		{
			return;
		}
		double timeAsDouble = Time.timeAsDouble;
		if (timeAsDouble < this.nextFireTime)
		{
			return;
		}
		GRHazardTower.tempRigs.Clear();
		GRHazardTower.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GRHazardTower.tempRigs);
		this.senseNearby.UpdateNearby(GRHazardTower.tempRigs, this.senseLineOfSight);
		float num;
		VRRig vrrig = this.senseNearby.PickClosest(out num);
		if (vrrig == null)
		{
			return;
		}
		Vector3 vector = vrrig.transform.position;
		Vector3 vector2 = Vector3.up * 0.1f;
		vector += vector2;
		GhostReactorManager.Get(this.gameEntity).RequestFireProjectile(this.gameEntity.id, this.fireFrom.position, vector, PhotonNetwork.Time + 0.0);
		this.nextFireTime = timeAsDouble + (double)this.fireCooldownTime;
	}

	public void OnFire(Vector3 fireFromPos, Vector3 fireAtPos, double fireAtTime)
	{
		Vector3 vector;
		if (this.gameEntity.IsAuthority() && GREnemyRanged.CalculateLaunchDirection(fireFromPos, fireAtPos, this.projectileSpeed, out vector))
		{
			this.gameEntity.manager.RequestCreateItem(this.projectilePrefab.name.GetStaticHash(), fireFromPos, Quaternion.LookRotation(vector, Vector3.up), (long)this.gameEntity.GetNetId());
		}
		double timeAsDouble = Time.timeAsDouble;
		this.nextFireTime = timeAsDouble + (double)this.fireCooldownTime;
	}

	public void OnProjectileInit(GRRangedEnemyProjectile projectile)
	{
	}

	public void OnProjectileHit(GRRangedEnemyProjectile projectile, Collision collision)
	{
	}

	public GameEntity gameEntity;

	public GRSenseNearby senseNearby;

	public GRSenseLineOfSight senseLineOfSight;

	public float projectileSpeed;

	public GameEntity projectilePrefab;

	public Transform fireFrom;

	public float fireChargeTime;

	public float fireCooldownTime;

	private double nextFireTime;

	private static List<VRRig> tempRigs = new List<VRRig>(16);
}
