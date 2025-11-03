using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	[RequireComponent(typeof(TransferrableObject))]
	public class SeedPacketHoldable : MonoBehaviour
	{
		private void Awake()
		{
			this.transferrableObject = base.GetComponent<TransferrableObject>();
			this.flowerEffectHash = PoolUtils.GameObjHashCode(this.flowerEffectPrefab);
		}

		private void OnEnable()
		{
			if (this._events == null)
			{
				this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
				NetPlayer netPlayer = ((this.transferrableObject.myOnlineRig != null) ? this.transferrableObject.myOnlineRig.creator : ((this.transferrableObject.myRig != null) ? (this.transferrableObject.myRig.creator ?? NetworkSystem.Instance.LocalPlayer) : null));
				if (netPlayer != null)
				{
					this._events.Init(netPlayer);
				}
			}
			if (this._events != null)
			{
				this._events.Activate += this.SyncTriggerEffect;
			}
		}

		private void OnDisable()
		{
			if (this._events != null)
			{
				this._events.Activate -= this.SyncTriggerEffect;
				this._events.Dispose();
				this._events = null;
			}
		}

		private void OnDestroy()
		{
			this.pooledObjects.Clear();
		}

		private void Update()
		{
			if (!this.transferrableObject.InHand())
			{
				return;
			}
			if (!this.isPouring && Vector3.Angle(base.transform.up, Vector3.down) <= this.pouringAngle)
			{
				this.StartPouring();
				RaycastHit raycastHit;
				if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, this.pouringRaycastDistance, this.raycastLayerMask))
				{
					this.hitPoint = raycastHit.point;
					base.Invoke("SpawnEffect", raycastHit.distance * this.placeEffectDelayMultiplier);
				}
			}
			if (this.isPouring && Time.time - this.pouringStartedTime >= this.cooldown)
			{
				this.isPouring = false;
			}
		}

		private void StartPouring()
		{
			if (this.particles)
			{
				this.particles.Play();
			}
			this.isPouring = true;
			this.pouringStartedTime = Time.time;
		}

		private void SpawnEffect()
		{
			GameObject gameObject = ObjectPools.instance.Instantiate(this.flowerEffectHash, true);
			gameObject.transform.position = this.hitPoint;
			SeedPacketTriggerHandler seedPacketTriggerHandler;
			if (gameObject.TryGetComponent<SeedPacketTriggerHandler>(out seedPacketTriggerHandler))
			{
				this.pooledObjects.Add(seedPacketTriggerHandler);
				seedPacketTriggerHandler.onTriggerEntered.AddListener(new UnityAction<SeedPacketTriggerHandler>(this.SyncTriggerEffectForOthers));
			}
		}

		private void SyncTriggerEffectForOthers(SeedPacketTriggerHandler seedPacketTriggerHandlerTriggerHandlerEvent)
		{
			int num = this.pooledObjects.IndexOf(seedPacketTriggerHandlerTriggerHandlerEvent);
			if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
			{
				this._events.Activate.RaiseOthers(new object[] { num });
			}
		}

		private void SyncTriggerEffect(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			if (args.Length != 1)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "SyncTriggerEffect");
			if (!this.callLimiter.CheckCallTime(Time.time))
			{
				return;
			}
			int num = (int)args[0];
			if (num < 0 && num >= this.pooledObjects.Count)
			{
				return;
			}
			this.pooledObjects[num].ToggleEffects();
		}

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private ParticleSystem particles;

		[SerializeField]
		private float pouringAngle;

		[SerializeField]
		private float pouringRaycastDistance = 5f;

		[SerializeField]
		private LayerMask raycastLayerMask;

		[SerializeField]
		private float placeEffectDelayMultiplier = 10f;

		[SerializeField]
		private GameObject flowerEffectPrefab;

		private List<SeedPacketTriggerHandler> pooledObjects = new List<SeedPacketTriggerHandler>();

		private CallLimiter callLimiter = new CallLimiter(10, 3f, 0.5f);

		private int flowerEffectHash;

		private Vector3 hitPoint;

		private TransferrableObject transferrableObject;

		private bool isPouring = true;

		private float pouringStartedTime;

		private RubberDuckEvents _events;
	}
}
