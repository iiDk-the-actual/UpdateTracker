using System;
using System.Collections;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class PickupableCosmetic : PickupableVariant
	{
		private void Awake()
		{
			this.rigOwnedPhysicsBody = base.GetComponent<RigOwnedPhysicsBody>();
			this.bodyCollider = base.GetComponent<Collider>();
		}

		private void Start()
		{
			base.enabled = false;
		}

		private void OnEnable()
		{
			if (this.rigOwnedPhysicsBody != null)
			{
				this.rigOwnedPhysicsBody.enabled = true;
			}
		}

		private void OnDisable()
		{
			if (this.rigOwnedPhysicsBody != null)
			{
				this.rigOwnedPhysicsBody.enabled = false;
			}
		}

		protected internal override void Pickup(bool isAutoPickup = false)
		{
			if (!isAutoPickup)
			{
				UnityEvent onPickupShared = this.OnPickupShared;
				if (onPickupShared != null)
				{
					onPickupShared.Invoke();
				}
			}
			this.rb.linearVelocity = Vector3.zero;
			this.rb.isKinematic = true;
			if (this.holdableParent != null)
			{
				base.transform.parent = this.holdableParent.transform;
			}
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.transform.localScale = Vector3.one;
			this.scale = 1f;
			this.placedOnFloorTime = -1f;
			this.placedOnFloor = false;
			this.broken = false;
			this.brokenTime = -1f;
			if (this.isBreakable && this.transferrableParent != null && this.transferrableParent.IsLocalObject())
			{
				int num = (int)this.transferrableParent.itemState;
				num &= ~PickupableCosmetic.breakableBitmask;
				this.transferrableParent.itemState = (TransferrableObject.ItemStates)num;
				if (this.breakEffect != null && this.breakEffect.isPlaying)
				{
					this.breakEffect.Stop();
				}
			}
			this.ShowRenderers(true);
			if (this.interactionPoint != null)
			{
				this.interactionPoint.enabled = true;
			}
			base.enabled = false;
		}

		protected internal override void DelayedPickup()
		{
			base.StartCoroutine(this.DelayedPickup_Internal());
		}

		private IEnumerator DelayedPickup_Internal()
		{
			yield return new WaitForSeconds(1f);
			this.Pickup(false);
			yield break;
		}

		protected internal override void Release(HoldableObject holdable, Vector3 startPosition, Vector3 velocity, float playerScale)
		{
			this.holdableParent = holdable;
			base.transform.parent = null;
			base.transform.position = startPosition;
			base.transform.localScale = Vector3.one * playerScale;
			this.rb.isKinematic = false;
			this.rb.useGravity = true;
			this.rb.linearVelocity = velocity;
			this.rb.detectCollisions = true;
			if (!this.allowPickupFromGround && this.interactionPoint != null)
			{
				this.interactionPoint.enabled = false;
			}
			this.scale = playerScale;
			base.enabled = true;
			this.transferrableParent = this.holdableParent as TransferrableObject;
			this.currentRayIndex = 0;
			this.frameCounter = 0;
		}

		private void FixedUpdate()
		{
			if (this.isBreakable && this.broken)
			{
				if (Time.time > this.respawnDelay + this.brokenTime)
				{
					this.Pickup(false);
				}
				return;
			}
			if (this.isBreakable && this.placedOnFloor)
			{
				bool flag = (this.transferrableParent.itemState & (TransferrableObject.ItemStates)PickupableCosmetic.breakableBitmask) > (TransferrableObject.ItemStates)0;
				if (flag != this.broken && flag)
				{
					this.OnBreakReplicated();
				}
			}
			if (this.autoPickupAfterSeconds > 0f && this.placedOnFloor && Time.time - this.placedOnFloorTime > this.autoPickupAfterSeconds)
			{
				this.Pickup(true);
				ThrowablePickupableCosmetic throwablePickupableCosmetic = this.transferrableParent as ThrowablePickupableCosmetic;
				if (throwablePickupableCosmetic)
				{
					UnityEvent onReturnToDockPositionShared = throwablePickupableCosmetic.OnReturnToDockPositionShared;
					if (onReturnToDockPositionShared != null)
					{
						onReturnToDockPositionShared.Invoke();
					}
				}
			}
			if (this.autoPickupDistance > 0f && this.transferrableParent != null && (this.transferrableParent.ownerRig.transform.position - base.transform.position).IsLongerThan(this.autoPickupDistance))
			{
				this.Pickup(false);
			}
			if (!this.placedOnFloor && base.enabled)
			{
				this.frameCounter++;
				if (this.frameCounter % this.stepEveryNFrames != 0)
				{
					return;
				}
				float num = this.RaycastCheckDist * this.scale;
				int value = this.floorLayerMask.value;
				Vector3[] cachedDirections = this.GetCachedDirections(this.RaycastChecksMax);
				int num2 = 0;
				while (num2 < this.raysPerStep && this.currentRayIndex < cachedDirections.Length)
				{
					Vector3 vector = cachedDirections[this.currentRayIndex];
					this.currentRayIndex++;
					num2++;
					RaycastHit raycastHit;
					if (Physics.Raycast(this.GetSafeRayOrigin(this.raycastOrigin.position, vector), vector, out raycastHit, num, value, QueryTriggerInteraction.Ignore) && (!this.dontStickToWall || Vector3.Angle(raycastHit.normal, Vector3.up) < 40f))
					{
						this.SettleBanner(raycastHit);
						UnityEvent onPlacedShared = this.OnPlacedShared;
						if (onPlacedShared != null)
						{
							onPlacedShared.Invoke();
						}
						this.placedOnFloor = true;
						this.placedOnFloorTime = Time.time;
						break;
					}
				}
				if (this.currentRayIndex >= cachedDirections.Length)
				{
					this.currentRayIndex = 0;
				}
			}
		}

		private void SettleBanner(RaycastHit hitInfo)
		{
			this.rb.isKinematic = true;
			this.rb.useGravity = false;
			this.rb.detectCollisions = false;
			Vector3 normal = hitInfo.normal;
			base.transform.position = hitInfo.point + normal * this.placementOffset;
			Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(base.transform.forward, normal).normalized, normal);
			base.transform.rotation = quaternion;
		}

		private Vector3 GetFibonacciSphereDirection(int index, int total)
		{
			float num = Mathf.Acos(1f - 2f * ((float)index + 0.5f) / (float)total);
			float num2 = 3.1415927f * (1f + Mathf.Sqrt(5f)) * ((float)index + 0.5f);
			float num3 = Mathf.Sin(num) * Mathf.Cos(num2);
			float num4 = Mathf.Sin(num) * Mathf.Sin(num2);
			float num5 = Mathf.Cos(num);
			return new Vector3(num3, num4, num5).normalized;
		}

		private Vector3[] GetCachedDirections(int count)
		{
			if (count <= 0)
			{
				return PickupableCosmetic.tmpEmpty;
			}
			Vector3[] array;
			if (PickupableCosmetic.directionCache.TryGetValue(count, out array))
			{
				return array;
			}
			array = new Vector3[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = this.GetFibonacciSphereDirection(i, count);
			}
			PickupableCosmetic.directionCache[count] = array;
			return array;
		}

		private Vector3 GetSafeRayOrigin(Vector3 rawOrigin, Vector3 dir)
		{
			float num = this.selfSkinOffset;
			if (this.bodyCollider != null)
			{
				float magnitude = this.bodyCollider.bounds.extents.magnitude;
				num = Mathf.Max(this.selfSkinOffset, magnitude * 0.05f);
			}
			return rawOrigin - dir.normalized * num;
		}

		public void BreakPlaceable()
		{
			if (!this.isBreakable || !this.placedOnFloor)
			{
				return;
			}
			if (this.transferrableParent != null && this.transferrableParent.IsLocalObject())
			{
				int num = (int)this.transferrableParent.itemState;
				num |= PickupableCosmetic.breakableBitmask;
				this.transferrableParent.itemState = (TransferrableObject.ItemStates)num;
				return;
			}
			GTDev.LogError<string>("PickupableCosmetic " + base.gameObject.name + " has no TransferrableObject parent. Break effects cannot be replicated", null);
		}

		private void OnBreakReplicated()
		{
			this.PlayBreakEffects();
		}

		protected virtual void PlayBreakEffects()
		{
			if (!this.isBreakable || !this.placedOnFloor || this.broken)
			{
				return;
			}
			this.broken = true;
			this.brokenTime = Time.time;
			if (this.breakEffect != null)
			{
				if (this.breakEffect.isPlaying)
				{
					this.breakEffect.Stop();
				}
				this.breakEffect.Play();
			}
			if (this.interactionPoint != null)
			{
				this.interactionPoint.enabled = false;
			}
			this.ShowRenderers(false);
			UnityEvent onBrokenShared = this.OnBrokenShared;
			if (onBrokenShared == null)
			{
				return;
			}
			onBrokenShared.Invoke();
		}

		protected virtual void ShowRenderers(bool visible)
		{
			if (this.hideOnBreak.IsNullOrEmpty<Renderer>())
			{
				return;
			}
			for (int i = 0; i < this.hideOnBreak.Length; i++)
			{
				Renderer renderer = this.hideOnBreak[i];
				if (!(renderer == null))
				{
					renderer.forceRenderingOff = !visible;
				}
			}
		}

		[SerializeField]
		private InteractionPoint interactionPoint;

		[SerializeField]
		private Rigidbody rb;

		[SerializeField]
		private Transform raycastOrigin;

		[Tooltip("Allow player to grab the placed object")]
		[SerializeField]
		private bool allowPickupFromGround = true;

		[SerializeField]
		private float autoPickupAfterSeconds;

		[SerializeField]
		private float autoPickupDistance;

		[Tooltip("Amount to offset the placed object from the hit position in the hit normal direction")]
		[SerializeField]
		private float placementOffset;

		[Tooltip("Prevent sticking if the hit surface normal is not within 40 degrees of world up")]
		[SerializeField]
		private bool dontStickToWall;

		[Tooltip("Layers to raycast against for placement")]
		[SerializeField]
		private LayerMask floorLayerMask = 134218241;

		[Tooltip("The distance to check if the banner is close to the floor (from a raycast check).")]
		public float RaycastCheckDist = 0.2f;

		[Tooltip("How many checks should we attempt for a raycast.")]
		public int RaycastChecksMax = 12;

		[FormerlySerializedAs("OnPickup")]
		[Space]
		public UnityEvent OnPickupShared;

		[FormerlySerializedAs("OnPlaced")]
		public UnityEvent OnPlacedShared;

		[SerializeField]
		private bool isBreakable;

		[Tooltip("Particle system played OnBrokenShared")]
		[SerializeField]
		private ParticleSystem breakEffect;

		[Tooltip("Renderers disabled OnBrokenShared and enabled OnPickupShared")]
		[SerializeField]
		private Renderer[] hideOnBreak = new Renderer[0];

		[Tooltip("Time after BreakPlaceable to reset item")]
		[SerializeField]
		private float respawnDelay = 0.5f;

		[FormerlySerializedAs("OnBroken")]
		[Space]
		public UnityEvent OnBrokenShared;

		private static int breakableBitmask = 32;

		private bool placedOnFloor;

		private float placedOnFloorTime = -1f;

		private bool broken;

		private float brokenTime = -1f;

		private VRRig cachedLocalRig;

		private HoldableObject holdableParent;

		private TransferrableObject transferrableParent;

		private RigOwnedPhysicsBody rigOwnedPhysicsBody;

		private double throwSettledTime = -1.0;

		private int landingSide;

		private float scale;

		private Collider bodyCollider;

		[Tooltip("How many directions to test per physics tick (spreads work across frames).")]
		[SerializeField]
		[Min(1f)]
		private int raysPerStep = 3;

		[Tooltip("Run a raycast step only every N physics ticks (1 = every FixedUpdate).")]
		[SerializeField]
		[Min(1f)]
		private int stepEveryNFrames = 2;

		[Tooltip("Small skin so rays start just outside our own collider volume.")]
		[SerializeField]
		[Range(0.005f, 0.1f)]
		private float selfSkinOffset = 0.02f;

		[SerializeField]
		private bool debugPlacementRays;

		private int currentRayIndex;

		private int frameCounter;

		private static readonly Dictionary<int, Vector3[]> directionCache = new Dictionary<int, Vector3[]>();

		private static readonly Vector3[] tmpEmpty = Array.Empty<Vector3>();
	}
}
