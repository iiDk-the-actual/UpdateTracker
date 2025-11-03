using System;
using Photon.Pun;
using UnityEngine;

public abstract class CosmeticCritter : MonoBehaviour
{
	public int Seed { get; protected set; }

	public CosmeticCritterSpawner Spawner { get; protected set; }

	public Type CachedType { get; private set; }

	public int GetGlobalMaxCritters()
	{
		return this.globalMaxCritters;
	}

	public void SetSeedSpawnerTypeAndTime(int seed, CosmeticCritterSpawner spawner, Type type, double time)
	{
		this.Seed = seed;
		this.Spawner = spawner;
		this.CachedType = type;
		this.startTime = time;
	}

	public virtual void OnSpawn()
	{
	}

	public virtual void OnDespawn()
	{
	}

	public virtual void SetRandomVariables()
	{
	}

	public abstract void Tick();

	protected double GetAliveTime()
	{
		if (!PhotonNetwork.InRoom)
		{
			return Time.timeAsDouble - this.startTime;
		}
		return PhotonNetwork.Time - this.startTime;
	}

	public virtual bool Expired()
	{
		return this.GetAliveTime() > (double)this.lifetime || this.GetAliveTime() < 0.0;
	}

	[Tooltip("After this many seconds the critter will forcibly despawn.")]
	[SerializeField]
	protected float lifetime;

	[Tooltip("The maximum number of this kind of critter that can be in the room at any given time.")]
	[SerializeField]
	private int globalMaxCritters;

	protected double startTime;
}
