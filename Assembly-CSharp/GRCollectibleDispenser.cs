using System;
using GorillaExtensions;
using UnityEngine;

public class GRCollectibleDispenser : MonoBehaviour, IGameEntityComponent
{
	public bool CollectibleAlreadySpawned
	{
		get
		{
			return this.currentCollectible != null;
		}
	}

	public bool ReadyToDispenseNewCollectible
	{
		get
		{
			double num = (double)this.collectibleRespawnTimeMinutes * 60.0;
			bool flag = (ulong)this.collectiblesDispensed < (ulong)((long)this.maxDispenseCount);
			return !this.CollectibleAlreadySpawned && flag && Time.timeAsDouble - this.collectibleDispenseRequestTime > num && Time.timeAsDouble - this.collectibleDispenseTime > num && Time.timeAsDouble - this.collectibleCollectedTime > num;
		}
	}

	public void OnEntityInit()
	{
		GhostReactor reactor = GhostReactorManager.Get(this.gameEntity).reactor;
		if (reactor != null)
		{
			reactor.collectibleDispensers.Add(this);
		}
	}

	public void OnEntityDestroy()
	{
		GhostReactorManager ghostReactorManager = GhostReactorManager.Get(this.gameEntity);
		if (ghostReactorManager != null && ghostReactorManager.reactor != null)
		{
			ghostReactorManager.reactor.collectibleDispensers.Remove(this);
		}
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
		uint num = this.collectiblesDispensed;
		uint num2 = this.collectiblesCollected;
		this.collectiblesDispensed = (uint)(nextState >> 32);
		this.collectiblesCollected = (uint)(nextState & (long)((ulong)(-1)));
		if (num != this.collectiblesDispensed)
		{
			this.collectibleDispenseTime = Time.timeAsDouble;
		}
		if (num2 != this.collectiblesCollected)
		{
			this.collectibleCollectedTime = Time.timeAsDouble;
		}
		if ((ulong)this.collectiblesCollected >= (ulong)((long)this.maxDispenseCount))
		{
			this.stillDispensingModel.gameObject.SetActive(false);
			this.fullyConsumedModel.gameObject.SetActive(true);
		}
	}

	public void RequestDispenseCollectible()
	{
		if (this.ReadyToDispenseNewCollectible && this.gameEntity.IsAuthority())
		{
			this.gameEntity.manager.RequestCreateItem(this.collectiblePrefab.name.GetStaticHash(), this.spawnLocation.position, this.spawnLocation.rotation, (long)this.gameEntity.manager.GetNetIdFromEntityId(this.gameEntity.id));
			this.collectiblesDispensed += 1U;
			this.collectibleDispenseTime = Time.timeAsDouble;
			long num = (long)((ulong)this.collectiblesDispensed);
			long num2 = (long)((ulong)this.collectiblesCollected);
			long num3 = (num << 32) | num2;
			this.gameEntity.RequestState(this.gameEntity.id, num3);
		}
	}

	public void OnCollectibleConsumed()
	{
		if (this.currentCollectible != null && this.currentCollectible.IsNotNull())
		{
			GRCollectible grcollectible = this.currentCollectible;
			grcollectible.OnCollected = (Action)Delegate.Remove(grcollectible.OnCollected, new Action(this.OnCollectibleConsumed));
			GameEntity entity = this.currentCollectible.entity;
			entity.OnGrabbed = (Action)Delegate.Remove(entity.OnGrabbed, new Action(this.OnCollectibleConsumed));
			this.currentCollectible = null;
		}
		this.collectiblesCollected += 1U;
		this.collectibleCollectedTime = Time.timeAsDouble;
		if (this.gameEntity.IsAuthority())
		{
			long num = (long)((ulong)this.collectiblesDispensed);
			long num2 = (long)((ulong)this.collectiblesCollected);
			long num3 = (num << 32) | num2;
			this.gameEntity.RequestState(this.gameEntity.id, num3);
		}
		if ((ulong)this.collectiblesCollected >= (ulong)((long)this.maxDispenseCount))
		{
			this.dispenserExhaustedEffect.Play();
			this.audioSource.PlayOneShot(this.dispenserExhaustedClip, this.dispenserExhaustedVolume);
			this.stillDispensingModel.gameObject.SetActive(false);
			this.fullyConsumedModel.gameObject.SetActive(true);
			return;
		}
		this.collectibleTakenEffect.Play();
		this.audioSource.PlayOneShot(this.collectibleTakenClip, this.collectibleTakenVolume);
	}

	public void GetSpawnedCollectible(GRCollectible collectible)
	{
		this.currentCollectible = collectible;
		collectible.OnCollected = (Action)Delegate.Combine(collectible.OnCollected, new Action(this.OnCollectibleConsumed));
		GameEntity entity = collectible.entity;
		entity.OnGrabbed = (Action)Delegate.Combine(entity.OnGrabbed, new Action(this.OnCollectibleConsumed));
	}

	public GameEntity gameEntity;

	public GameEntity collectiblePrefab;

	public Transform spawnLocation;

	public LayerMask collectibleLayerMask;

	public float collectibleRespawnTimeMinutes = 1.5f;

	public int maxDispenseCount = 3;

	public AudioSource audioSource;

	public Transform stillDispensingModel;

	public Transform fullyConsumedModel;

	public ParticleSystem collectibleTakenEffect;

	public AudioClip collectibleTakenClip;

	public float collectibleTakenVolume;

	public ParticleSystem dispenserExhaustedEffect;

	public AudioClip dispenserExhaustedClip;

	public float dispenserExhaustedVolume;

	private GRCollectible currentCollectible;

	private Coroutine getSpawnedCollectibleCoroutine;

	private static Collider[] overlapColliders = new Collider[10];

	private uint collectiblesDispensed;

	private uint collectiblesCollected;

	private double collectibleDispenseRequestTime = -10000.0;

	private double collectibleDispenseTime = -10000.0;

	private double collectibleCollectedTime = -10000.0;
}
