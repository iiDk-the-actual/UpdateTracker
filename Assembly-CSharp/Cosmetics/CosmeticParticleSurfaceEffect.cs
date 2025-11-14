using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace Cosmetics
{
	[RequireComponent(typeof(TransferrableObject))]
	public class CosmeticParticleSurfaceEffect : MonoBehaviour, ITickSystemTick
	{
		private void Awake()
		{
			this.transferrableObject = base.GetComponent<TransferrableObject>();
			if (this.surfaceEffectPrefab != null)
			{
				this.surfaceEffectHash = PoolUtils.GameObjHashCode(this.surfaceEffectPrefab);
			}
		}

		private void OnEnable()
		{
			if (this._events == null)
			{
				this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
				this.owner = ((this.transferrableObject.myOnlineRig != null) ? this.transferrableObject.myOnlineRig.creator : ((this.transferrableObject.myRig != null) ? (this.transferrableObject.myRig.creator ?? NetworkSystem.Instance.LocalPlayer) : null));
				if (this.owner != null)
				{
					this._events.Init(this.owner);
					this.isLocal = this.owner.IsLocal;
				}
			}
			if (this._events != null)
			{
				this._events.Activate.reliable = true;
				this._events.Deactivate.reliable = true;
				this._events.Activate += this.OnSpawnReplicated;
				this._events.Deactivate += this.OnTriggerEffectReplicated;
			}
			if (ObjectPools.instance == null || !ObjectPools.instance.initialized)
			{
				return;
			}
			if (this.surfaceEffectHash != 0)
			{
				this._pool = ObjectPools.instance.GetPoolByHash(this.surfaceEffectHash);
				if (this._pool != null)
				{
					this.foundPool = true;
				}
				else
				{
					GTDev.LogError<string>("CosmeticParticleSurfaceEffect " + base.gameObject.name + " no Object pool found for surface effect prefab. Has it been added to Global Object Pools?", null);
				}
			}
			this.spawnCallLimiter.Reset();
			this.destroyCallLimiter.Reset();
			this.lastHitTime = float.MinValue;
		}

		private void OnDisable()
		{
			this.StopParticles();
			if (this._events != null)
			{
				this._events.Activate -= this.OnSpawnReplicated;
				this._events.Deactivate -= this.OnTriggerEffectReplicated;
				this._events.Dispose();
				this._events = null;
			}
			this.surfaceEffectNum.Clear();
			foreach (SeedPacketTriggerHandler seedPacketTriggerHandler in this.surfaceEffects)
			{
				if (!(seedPacketTriggerHandler == null))
				{
					seedPacketTriggerHandler.onTriggerEntered.RemoveListener(new UnityAction<SeedPacketTriggerHandler>(this.OnTriggerEffectLocal));
				}
			}
			this.surfaceEffects.Clear();
		}

		private void OnDestroy()
		{
			this.surfaceEffectNum.Clear();
			this.surfaceEffects.Clear();
		}

		public void StartParticles()
		{
			if (!this.isSpawning)
			{
				this.isSpawning = true;
				this.particleStartedTime = Time.time;
				if (!this.particles.isPlaying)
				{
					this.particles.Play();
				}
			}
			if (!this.TickRunning)
			{
				TickSystem<object>.AddTickCallback(this);
			}
		}

		public void StopParticles()
		{
			if (this.TickRunning)
			{
				TickSystem<object>.RemoveTickCallback(this);
			}
			this.isSpawning = false;
			this.particleStartedTime = float.MinValue;
			this.lastHitTime = float.MinValue;
			if (this.particles.isPlaying)
			{
				this.particles.Stop();
			}
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (this.transferrableObject == null || !this.transferrableObject.InHand())
			{
				this.StopParticles();
				return;
			}
			if (this.isSpawning && this.stopAfterSeconds > 0f && Time.time >= this.particleStartedTime + this.stopAfterSeconds)
			{
				this.StopParticles();
				return;
			}
			if (!this.isLocal)
			{
				return;
			}
			if (this.isSpawning && Time.time > this.placeEffectCooldown + this.lastHitTime)
			{
				int num = Physics.RaycastNonAlloc(this.rayCastOrigin.position, this.useWorldDirection ? this.worldDirection : this.rayCastOrigin.forward, this.hits, this.rayCastDistance, this.rayCastLayerMask, QueryTriggerInteraction.Ignore);
				if (num > 0)
				{
					int num2 = 0;
					float num3 = this.hits[num2].distance;
					for (int i = 1; i < num; i++)
					{
						if (this.hits[i].distance < num3)
						{
							num2 = i;
							num3 = this.hits[i].distance;
						}
					}
					this.hitPoint = this.hits[num2];
					this.lastHitTime = Time.time;
					base.Invoke("SpawnEffect", num3 * this.placeEffectDelayMultiplier);
				}
			}
		}

		private void SpawnEffect()
		{
			if (!this.isLocal)
			{
				return;
			}
			long num = BitPackUtils.PackWorldPosForNetwork(this.hitPoint.point);
			long num2 = BitPackUtils.PackWorldPosForNetwork(this.hitPoint.normal);
			int num3 = this.currentEffect;
			this.currentEffect++;
			if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
			{
				this._events.Activate.RaiseOthers(new object[] { num, num2, num3 });
			}
			this.SpawnLocal(this.hitPoint.point, this.hitPoint.normal, num3);
		}

		private void OnSpawnReplicated(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (!this || sender != target || this.owner == null || info.senderID != this.owner.ActorNumber)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "OnSpawnReplicated");
			if (!this.spawnCallLimiter.CheckCallTime(Time.time) || args.Length != 3 || !(args[0] is long) || !(args[1] is long) || !(args[2] is int))
			{
				return;
			}
			Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork((long)args[0]);
			Vector3 vector2 = BitPackUtils.UnpackWorldPosFromNetwork((long)args[1]);
			float num = 10000f;
			if ((in vector).IsValid(in num))
			{
				float num2 = 10000f;
				if ((in vector2).IsValid(in num2))
				{
					if (Vector3.Distance(this.rayCastOrigin.position, vector) > this.rayCastDistance + 2f)
					{
						return;
					}
					vector2.Normalize();
					if (vector2 == Vector3.zero)
					{
						vector2 = Vector3.up;
					}
					int num3 = (int)args[2];
					this.SpawnLocal(vector, vector2, num3);
					return;
				}
			}
		}

		private void SpawnLocal(Vector3 position, Vector3 up, int identifier)
		{
			if (this.surfaceEffectHash != 0 && !this.foundPool)
			{
				this._pool = ObjectPools.instance.GetPoolByHash(this.surfaceEffectHash);
				if (this._pool == null)
				{
					return;
				}
				this.foundPool = true;
			}
			if (this.foundPool && this._pool.GetInactiveCount() > 0)
			{
				this.ClearOldObjects();
				GameObject gameObject = this._pool.Instantiate(true);
				gameObject.transform.position = position;
				gameObject.transform.up = up;
				SeedPacketTriggerHandler seedPacketTriggerHandler;
				if (gameObject.TryGetComponent<SeedPacketTriggerHandler>(out seedPacketTriggerHandler))
				{
					int num = this.surfaceEffects.IndexOf(seedPacketTriggerHandler);
					if (num >= 0)
					{
						this.surfaceEffectNum[num] = identifier;
					}
					else
					{
						this.surfaceEffectNum.Add(identifier);
						this.surfaceEffects.Add(seedPacketTriggerHandler);
					}
					seedPacketTriggerHandler.onTriggerEntered.AddListener(new UnityAction<SeedPacketTriggerHandler>(this.OnTriggerEffectLocal));
				}
			}
		}

		private void ClearOldObjects()
		{
			for (int i = this.surfaceEffects.Count - 1; i >= 0; i--)
			{
				if (this.surfaceEffects[i] == null)
				{
					this.surfaceEffects.RemoveAt(i);
					this.surfaceEffectNum.RemoveAt(i);
				}
				else if (!this.surfaceEffects[i].gameObject.activeSelf)
				{
					this.surfaceEffects[i].onTriggerEntered.RemoveListener(new UnityAction<SeedPacketTriggerHandler>(this.OnTriggerEffectLocal));
					this.surfaceEffects.RemoveAt(i);
					this.surfaceEffectNum.RemoveAt(i);
				}
			}
		}

		private void OnTriggerEffectLocal(SeedPacketTriggerHandler seedPacketTriggerHandlerTriggerHandlerEvent)
		{
			int num = this.surfaceEffects.IndexOf(seedPacketTriggerHandlerTriggerHandlerEvent);
			if (num >= 0 && num < this.surfaceEffectNum.Count)
			{
				int num2 = this.surfaceEffectNum[num];
				if (PhotonNetwork.InRoom && this._events != null && this._events.Deactivate != null)
				{
					this._events.Deactivate.RaiseOthers(new object[] { num2 });
				}
				this.surfaceEffects.RemoveAt(num);
				this.surfaceEffectNum.RemoveAt(num);
			}
		}

		private void OnTriggerEffectReplicated(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "OnTriggerEffectReplicated");
			if (!this.destroyCallLimiter.CheckCallTime(Time.time) || args.Length != 1 || !(args[0] is int))
			{
				return;
			}
			this.ClearOldObjects();
			int num = (int)args[0];
			int num2 = this.surfaceEffectNum.IndexOf(num);
			if (num2 >= 0 && num2 < this.surfaceEffects.Count)
			{
				SeedPacketTriggerHandler seedPacketTriggerHandler = this.surfaceEffects[num2];
				if (seedPacketTriggerHandler != null)
				{
					seedPacketTriggerHandler.ToggleEffects();
					seedPacketTriggerHandler.onTriggerEntered.RemoveListener(new UnityAction<SeedPacketTriggerHandler>(this.OnTriggerEffectLocal));
				}
				this.surfaceEffects.RemoveAt(num2);
				this.surfaceEffectNum.RemoveAt(num2);
			}
		}

		[Tooltip("autoStop particle system this many seconds after starting")]
		[SerializeField]
		private float stopAfterSeconds = 3f;

		[Tooltip("particle system to play on start particles")]
		[SerializeField]
		private ParticleSystem particles;

		[Tooltip("Distance in meters to check for a surface hit")]
		[SerializeField]
		private float rayCastDistance = 20f;

		[Tooltip("The position for the start of the rayCast.\nThe forward (z+) axis of this transform will be used as the rayCast direction\nThis should visually line up with the spawned particles")]
		[SerializeField]
		private Transform rayCastOrigin;

		[Tooltip("Use a world direction vector for the raycast instead of the rayCastOrigin forward?")]
		[SerializeField]
		private bool useWorldDirection;

		[SerializeField]
		private Vector3 worldDirection = Vector3.down;

		[Tooltip("Layers to check for surface collision")]
		[SerializeField]
		private LayerMask rayCastLayerMask = 513;

		[Tooltip("Prefab from the global object pool to spawn on surface hit\nIf it should be destroyed on touch, add a SeedPacketTriggerHandler to the prefab")]
		[SerializeField]
		private GameObject surfaceEffectPrefab;

		[Tooltip("Seconds per meter to wait before spawning a surface effect on hit.\n A good value would be somewhat close to 1/particle velocity ")]
		[SerializeField]
		private float placeEffectDelayMultiplier = 3f;

		[Tooltip("Time to wait between spawning surface effects")]
		[SerializeField]
		private float placeEffectCooldown = 2f;

		private float particleStartedTime;

		private bool isSpawning;

		private float lastHitTime = float.MinValue;

		private RaycastHit hitPoint;

		private RaycastHit[] hits = new RaycastHit[5];

		private TransferrableObject transferrableObject;

		private bool isLocal;

		private NetPlayer owner;

		private int surfaceEffectHash;

		private RubberDuckEvents _events;

		private CallLimiter spawnCallLimiter = new CallLimiter(10, 3f, 0.5f);

		private CallLimiter destroyCallLimiter = new CallLimiter(10, 3f, 0.5f);

		private SinglePool _pool;

		private bool foundPool;

		private int currentEffect;

		private List<int> surfaceEffectNum = new List<int>();

		private List<SeedPacketTriggerHandler> surfaceEffects = new List<SeedPacketTriggerHandler>(10);
	}
}
