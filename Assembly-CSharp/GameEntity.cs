using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

public class GameEntity : MonoBehaviour
{
	[DebugReadout]
	public GameEntityId id { get; internal set; }

	[DebugReadout]
	public int typeId { get; private set; }

	[DebugReadout]
	public long createData { get; set; }

	[DebugReadout]
	public int heldByActorNumber { get; internal set; }

	[DebugReadout]
	public int snappedByActorNumber { get; internal set; }

	[DebugReadout]
	public SnapJointType snappedJoint { get; internal set; }

	[DebugReadout]
	public int heldByHandIndex { get; internal set; }

	[DebugReadout]
	public int lastHeldByActorNumber { get; internal set; }

	[DebugReadout]
	public int onlyGrabActorNumber { get; internal set; }

	[DebugReadout]
	public GameEntityId attachedToEntityId { get; internal set; }

	public event GameEntity.StateChangedEvent OnStateChanged;

	public event GameEntity.EntityDestroyedEvent onEntityDestroyed;

	private void Awake()
	{
		this.id = GameEntityId.Invalid;
		this.rigidBody = base.GetComponent<Rigidbody>();
		this.heldByActorNumber = -1;
		this.heldByHandIndex = -1;
		this.onlyGrabActorNumber = -1;
		this.snappedByActorNumber = -1;
		this.attachedToEntityId = GameEntityId.Invalid;
		this.entityComponents = new List<IGameEntityComponent>(1);
		base.GetComponentsInChildren<IGameEntityComponent>(this.entityComponents);
		this.entitySerialize = new List<IGameEntitySerialize>(1);
		base.GetComponentsInChildren<IGameEntitySerialize>(this.entitySerialize);
	}

	public void Create(GameEntityManager manager, int typeId)
	{
		this.manager = manager;
		this.typeId = typeId;
	}

	public void Init(long createData)
	{
		this.createData = createData;
		for (int i = 0; i < this.entityComponents.Count; i++)
		{
			this.entityComponents[i].OnEntityInit();
		}
	}

	public void OnDestroy()
	{
		if (GTAppState.isQuitting)
		{
			return;
		}
		for (int i = 0; i < this.entityComponents.Count; i++)
		{
			this.entityComponents[i].OnEntityDestroy();
		}
		GameEntity.EntityDestroyedEvent entityDestroyedEvent = this.onEntityDestroyed;
		if (entityDestroyedEvent == null)
		{
			return;
		}
		entityDestroyedEvent(this);
	}

	public Vector3 GetVelocity()
	{
		if (this.rigidBody == null)
		{
			return Vector3.zero;
		}
		return this.rigidBody.linearVelocity;
	}

	public void PlayCatchFx()
	{
		if (this.audioSource != null)
		{
			this.audioSource.volume = this.catchSoundVolume;
			this.audioSource.GTPlayOneShot(this.catchSound, 1f);
		}
	}

	public void PlayThrowFx()
	{
		if (this.audioSource != null)
		{
			this.audioSource.volume = this.throwSoundVolume;
			this.audioSource.GTPlayOneShot(this.throwSound, 1f);
		}
	}

	public void PlaySnapFx()
	{
		if (this.audioSource != null)
		{
			this.audioSource.volume = this.snapSoundVolume;
			this.audioSource.GTPlayOneShot(this.snapSound, 1f);
		}
	}

	private bool IsGamePlayer(Collider collider)
	{
		return GamePlayer.GetGamePlayer(collider, false) != null;
	}

	public long GetState()
	{
		return this.state;
	}

	public void RequestState(GameEntityId id, long newState)
	{
		this.manager.RequestState(id, newState);
	}

	public bool IsAuthority()
	{
		return this.manager.IsAuthority();
	}

	public bool IsValidToMigrate()
	{
		return this.manager.IsEntityValidToMigrate(this);
	}

	public void SetState(long newState)
	{
		if (this.state != newState)
		{
			long num = this.state;
			this.state = newState;
			GameEntity.StateChangedEvent onStateChanged = this.OnStateChanged;
			if (onStateChanged != null)
			{
				onStateChanged(num, newState);
			}
			for (int i = 0; i < this.entityComponents.Count; i++)
			{
				this.entityComponents[i].OnEntityStateChange(num, newState);
			}
		}
	}

	public GameEntityId MigrateToEntityManager(GameEntityManager newManager)
	{
		Debug.Log(string.Format("Migrating {0} into {1}", base.gameObject, newManager));
		this.manager.RemoveGameEntity(this);
		this.manager = newManager;
		GameEntityId gameEntityId = newManager.AddGameEntity(this);
		this.id = gameEntityId;
		return gameEntityId;
	}

	public void MigrateHeldBy(int actorNumber)
	{
		if (this.heldByActorNumber >= 0)
		{
			this.heldByActorNumber = actorNumber;
		}
	}

	public void MigrateSnappedBy(int actorNumber)
	{
		if (this.snappedByActorNumber >= 0)
		{
			this.snappedByActorNumber = actorNumber;
		}
	}

	public int GetNetId(GameEntityId gameEntityId)
	{
		return this.manager.GetNetIdFromEntityId(gameEntityId);
	}

	public int GetNetId()
	{
		return this.manager.GetNetIdFromEntityId(this.id);
	}

	public static GameEntity Get(Collider collider)
	{
		if (collider == null)
		{
			return null;
		}
		Transform transform = collider.transform;
		while (transform != null)
		{
			GameEntity component = transform.GetComponent<GameEntity>();
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return null;
	}

	public bool IsHeldByLocalPlayer()
	{
		return this.heldByActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber;
	}

	public bool IsSnappedByLocalPlayer()
	{
		return this.snappedByActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber;
	}

	public bool IsHeld()
	{
		return this.heldByActorNumber != -1;
	}

	public int GetLastHeldByPlayerForEntityID(GameEntityId gameEntityId)
	{
		GameEntity gameEntity = this.manager.GetGameEntity(gameEntityId);
		if (gameEntity != null)
		{
			return gameEntity.lastHeldByActorNumber;
		}
		return 0;
	}

	public bool WasLastHeldByLocalPlayer()
	{
		return this.lastHeldByActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber;
	}

	public bool IsAttachedToPlayer(NetPlayer player)
	{
		return player != null && (this.heldByActorNumber == player.ActorNumber || this.snappedByActorNumber == player.ActorNumber);
	}

	public EHandedness EquippedHandedness
	{
		get
		{
			if (this.heldByHandIndex == 0 || (this.snappedJoint & SnapJointType.ArmL) != SnapJointType.None)
			{
				return EHandedness.Left;
			}
			if (this.heldByHandIndex != 1 && (this.snappedJoint & SnapJointType.ArmR) == SnapJointType.None)
			{
				return EHandedness.None;
			}
			return EHandedness.Right;
		}
	}

	public const int Invalid = -1;

	public bool pickupable = true;

	public float pickupRangeFromSurface;

	public bool canHoldingPlayerUpdateState;

	public bool canLastHoldingPlayerUpdateState;

	public bool canSnapPlayerUpdateState;

	public AudioSource audioSource;

	public AudioClip catchSound;

	public float catchSoundVolume = 0.5f;

	public AudioClip throwSound;

	public float throwSoundVolume = 0.5f;

	public AudioClip snapSound;

	public float snapSoundVolume = 0.5f;

	private Rigidbody rigidBody;

	[NonSerialized]
	public GameEntityManager manager;

	public Action OnGrabbed;

	public Action OnReleased;

	public Action OnSnapped;

	public Action OnUnsnapped;

	public Action OnAttached;

	public Action OnDetached;

	public Action OnTick;

	public float MinTimeBetweenTicks;

	[NonSerialized]
	public float LastTickTime;

	private long state;

	private List<IGameEntityComponent> entityComponents;

	public List<IGameEntitySerialize> entitySerialize;

	public delegate void StateChangedEvent(long prevState, long nextState);

	public delegate void EntityDestroyedEvent(GameEntity entity);
}
