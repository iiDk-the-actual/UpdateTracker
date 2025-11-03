using System;
using UnityEngine;

public class GRRangedEnemyProjectile : MonoBehaviour, IGameEntityComponent, IGameHittable, IGameHitter
{
	private void Awake()
	{
		this.particleSystem = base.GetComponentInChildren<ParticleSystem>();
		this.audioSource = base.GetComponentInChildren<AudioSource>();
		this.meshRenderer = base.GetComponentInChildren<MeshRenderer>();
		this.hittable = base.GetComponentInChildren<GameHittable>();
		this.projectileRigidbody = base.GetComponent<Rigidbody>();
		this.entity = base.GetComponent<GameEntity>();
	}

	private void Start()
	{
		if (this.projectileRigidbody != null)
		{
			this.projectileRigidbody.linearVelocity = base.transform.forward * this.projectileSpeed;
		}
		this.projectileHasImpacted = false;
		if (this.owningEntity != null)
		{
			Collider componentInChildren = base.GetComponentInChildren<Collider>();
			if (componentInChildren != null)
			{
				Collider[] componentsInChildren = this.owningEntity.gameObject.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Physics.IgnoreCollision(componentInChildren, componentsInChildren[i]);
				}
			}
		}
	}

	private void Update()
	{
		if (this.entity.IsAuthority() && this.projectileHasImpacted && Time.timeAsDouble > this.projectileImpactTime + (double)this.postImpactLifetime)
		{
			this.entity.manager.RequestDestroyItem(this.entity.id);
		}
	}

	public void OnEntityInit()
	{
		this.owningEntityNetID = (int)this.entity.createData;
		if (this.owningEntityNetID != 0)
		{
			this.owningEntity = this.FindOwningEntity();
			this.projectileLauncher = this.owningEntity.GetComponent<IGameProjectileLauncher>();
			if (this.projectileLauncher != null)
			{
				this.projectileLauncher.OnProjectileInit(this);
			}
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private GameEntity FindOwningEntity()
	{
		if (this.owningEntityNetID != 0)
		{
			GameEntityManager gameEntityManager = GhostReactorManager.Get(this.entity).gameEntityManager;
			GameEntityId entityIdFromNetId = gameEntityManager.GetEntityIdFromNetId(this.owningEntityNetID);
			return gameEntityManager.GetGameEntity(entityIdFromNetId);
		}
		return null;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!this.projectileHasImpacted)
		{
			if (this.canHitPlayer)
			{
				Vector3 position = base.transform.position;
				if ((VRRig.LocalRig.GetMouthPosition() - position).sqrMagnitude < this.projectileHitRadius * this.projectileHitRadius && Time.time > this.lastHitPlayerTime + this.minTimeBetweenHits)
				{
					this.lastHitPlayerTime = Time.time;
					GhostReactorManager.Get(this.entity).RequestEnemyHitPlayer(GhostReactor.EnemyType.Ranged, this.entity.id, VRRig.LocalRig.GetComponent<GRPlayer>(), position);
				}
				if (this.projectileLauncher != null)
				{
					this.projectileLauncher.OnProjectileHit(this, collision);
				}
			}
			this.projectileHasImpacted = true;
			this.projectileImpactTime = Time.timeAsDouble;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!this.projectileHasImpacted)
		{
			GRShieldCollider component = collider.GetComponent<GRShieldCollider>();
			if (component != null)
			{
				component.BlockHittable(this.projectileRigidbody.transform.position, this.projectileRigidbody.linearVelocity.normalized, this.hittable);
			}
		}
	}

	public bool IsHitValid(GameHitData hit)
	{
		return true;
	}

	public void OnHit(GameHitData hit)
	{
		GameHitType hitTypeId = (GameHitType)hit.hitTypeId;
		GRTool gameComponent = this.entity.manager.GetGameComponent<GRTool>(hit.hitByEntityId);
		if (gameComponent != null)
		{
			switch (hitTypeId)
			{
			case GameHitType.Club:
				this.OnHitByClub(gameComponent, hit);
				return;
			case GameHitType.Flash:
				this.OnHitByFlash(gameComponent, hit);
				return;
			case GameHitType.Shield:
				this.OnHitByShield(gameComponent, hit);
				break;
			default:
				return;
			}
		}
	}

	public void OnHitByClub(GRTool tool, GameHitData hit)
	{
		this.projectileHasImpacted = true;
		this.projectileImpactTime = Time.timeAsDouble;
		if (this.projectileRigidbody != null)
		{
			this.PlayImpactFX();
			this.projectileRigidbody.linearVelocity = hit.hitImpulse * (this.projectileRigidbody.linearVelocity.magnitude * 0.7f);
		}
	}

	public void OnHitByFlash(GRTool grTool, GameHitData hit)
	{
	}

	public void OnHitByShield(GRTool tool, GameHitData hit)
	{
		this.projectileHasImpacted = true;
		this.projectileImpactTime = Time.timeAsDouble;
		if (this.projectileRigidbody != null)
		{
			this.PlayImpactFX();
			this.projectileRigidbody.linearVelocity = hit.hitImpulse;
		}
	}

	private void PlayImpactFX()
	{
		if (this.particleSystem != null)
		{
			this.particleSystem.Play();
		}
		if (this.meshRenderer != null)
		{
			this.meshRenderer.enabled = false;
		}
	}

	public void OnSuccessfulHit(GameHitData hit)
	{
		this.PlayImpactFX();
	}

	public void OnSuccessfulHitPlayer(GRPlayer player, Vector3 hitPosition)
	{
		this.PlayImpactFX();
		this.hitSFX.Play(null);
		if (this.applyFreezeEffect)
		{
			player.SetAsFrozen(4f);
		}
	}

	private int owningEntityNetID;

	private GameEntity entity;

	public GameEntity owningEntity;

	private IGameProjectileLauncher projectileLauncher;

	public Rigidbody projectileRigidbody;

	private ParticleSystem particleSystem;

	private AudioSource audioSource;

	private MeshRenderer meshRenderer;

	private GameHittable hittable;

	public float projectileSpeed = 5f;

	public float projectileHitRadius = 1f;

	public float postImpactLifetime = 2f;

	private bool projectileHasImpacted;

	private double projectileImpactTime;

	private float lastHitPlayerTime;

	private float minTimeBetweenHits = 0.5f;

	public bool applyFreezeEffect;

	public bool canHitPlayer = true;

	public AbilitySound hitSFX;
}
