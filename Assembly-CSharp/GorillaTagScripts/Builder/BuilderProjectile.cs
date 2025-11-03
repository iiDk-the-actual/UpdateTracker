using System;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderProjectile : MonoBehaviour
	{
		public Vector3 launchPosition { get; private set; }

		public event BuilderProjectile.ProjectileImpactEvent OnImpact;

		public void Launch(Vector3 position, Vector3 velocity, BuilderProjectileLauncher sourceObject, int projectileCount, float scale, int timeStamp)
		{
			this.particleLaunched = true;
			this.timeCreated = Time.time;
			this.projectileSource = sourceObject;
			float num = (NetworkSystem.Instance.ServerTimestamp - timeStamp) / 1000f;
			if (num >= this.lifeTime)
			{
				this.Deactivate();
				return;
			}
			this.timeCreated -= num;
			Vector3 vector = Vector3.ProjectOnPlane(velocity, Vector3.up);
			float num2 = 0.017453292f * Vector3.Angle(vector, velocity);
			float num3 = this.projectileRigidbody.mass * this.gravityMultiplier * ((scale < 1f) ? scale : 1f) * 9.8f;
			Vector3 vector2 = num * Mathf.Cos(num2) * vector;
			float num4 = velocity.z * num * Mathf.Sin(num2) - 0.5f * num3 * num * num;
			this.launchPosition = position + vector2 + num4 * Vector3.down;
			Transform transform = base.transform;
			transform.position = position;
			transform.localScale = Vector3.one * scale;
			base.GetComponent<Collider>().contactOffset = 0.01f * scale;
			RigidbodyWaterInteraction component = base.GetComponent<RigidbodyWaterInteraction>();
			if (component != null)
			{
				component.objectRadiusForWaterCollision = 0.02f * scale;
			}
			this.projectileRigidbody.useGravity = false;
			Vector3 vector3 = this.projectileRigidbody.mass * this.gravityMultiplier * ((scale < 1f) ? scale : 1f) * Physics.gravity;
			this.forceComponent.force = vector3;
			this.projectileRigidbody.linearVelocity = velocity + num * vector3;
			this.projectileId = projectileCount;
			this.projectileRigidbody.position = position;
			this.projectileSource.RegisterProjectile(this);
		}

		protected void Awake()
		{
			this.projectileRigidbody = base.GetComponent<Rigidbody>();
			this.forceComponent = base.GetComponent<ConstantForce>();
			this.initialScale = base.transform.localScale.x;
		}

		public void Deactivate()
		{
			base.transform.localScale = Vector3.one * this.initialScale;
			this.projectileRigidbody.useGravity = true;
			this.forceComponent.force = Vector3.zero;
			this.OnImpact = null;
			this.aoeKnockbackConfig = null;
			this.impactSoundVolumeOverride = null;
			this.impactSoundPitchOverride = null;
			this.impactEffectScaleMultiplier = 1f;
			this.gravityMultiplier = 1f;
			ObjectPools.instance.Destroy(base.gameObject);
		}

		private void SpawnImpactEffect(GameObject prefab, Vector3 position, Vector3 normal)
		{
			Vector3 vector = position + normal * this.impactEffectOffset;
			GameObject gameObject = ObjectPools.instance.Instantiate(prefab, vector, true);
			Vector3 localScale = base.transform.localScale;
			gameObject.transform.localScale = localScale * this.impactEffectScaleMultiplier;
			gameObject.transform.up = normal;
			SurfaceImpactFX component = gameObject.GetComponent<SurfaceImpactFX>();
			if (component != null)
			{
				component.SetScale(localScale.x * this.impactEffectScaleMultiplier);
			}
			SoundBankPlayer component2 = gameObject.GetComponent<SoundBankPlayer>();
			if (component2 != null && !component2.playOnEnable)
			{
				component2.Play(this.impactSoundVolumeOverride, this.impactSoundPitchOverride);
			}
		}

		public void ApplyHitKnockback(Vector3 hitNormal)
		{
			if (this.aoeKnockbackConfig != null && this.aoeKnockbackConfig.Value.applyAOEKnockback)
			{
				Vector3 vector = Vector3.ProjectOnPlane(hitNormal, Vector3.up);
				vector.Normalize();
				Vector3 vector2 = 0.75f * vector + 0.25f * Vector3.up;
				vector2.Normalize();
				GTPlayer instance = GTPlayer.Instance;
				instance.ApplyKnockback(vector2, this.aoeKnockbackConfig.Value.knockbackVelocity, instance.scale < 0.9f);
			}
		}

		private void OnEnable()
		{
			this.timeCreated = 0f;
			this.particleLaunched = false;
		}

		protected void OnDisable()
		{
			this.particleLaunched = false;
			if (this.projectileSource != null)
			{
				this.projectileSource.UnRegisterProjectile(this);
			}
			this.projectileSource = null;
		}

		public void UpdateProjectile()
		{
			if (this.particleLaunched)
			{
				if (Time.time > this.timeCreated + this.lifeTime)
				{
					this.Deactivate();
				}
				if (this.faceDirectionOfTravel)
				{
					Transform transform = base.transform;
					Vector3 position = transform.position;
					Vector3 vector = position - this.previousPosition;
					transform.rotation = ((vector.sqrMagnitude > 0f) ? Quaternion.LookRotation(vector) : transform.rotation);
					this.previousPosition = position;
				}
			}
		}

		private void OnCollisionEnter(Collision other)
		{
			if (!this.particleLaunched)
			{
				return;
			}
			BuilderPieceCollider component = other.transform.GetComponent<BuilderPieceCollider>();
			if (component != null && component.piece.gameObject.Equals(this.projectileSource.gameObject))
			{
				return;
			}
			ContactPoint contact = other.GetContact(0);
			if (other.collider.gameObject.IsOnLayer(UnityLayer.GorillaBodyCollider))
			{
				this.ApplyHitKnockback(-1f * contact.normal);
			}
			this.SpawnImpactEffect(this.surfaceImpactEffectPrefab, contact.point, contact.normal);
			BuilderProjectile.ProjectileImpactEvent onImpact = this.OnImpact;
			if (onImpact != null)
			{
				onImpact(this, contact.point, null);
			}
			this.Deactivate();
		}

		protected void OnCollisionStay(Collision other)
		{
			if (!this.particleLaunched)
			{
				return;
			}
			BuilderPieceCollider component = other.transform.GetComponent<BuilderPieceCollider>();
			if (component != null && component.piece.gameObject.Equals(this.projectileSource.gameObject))
			{
				return;
			}
			ContactPoint contact = other.GetContact(0);
			if (other.collider.gameObject.IsOnLayer(UnityLayer.GorillaBodyCollider))
			{
				this.ApplyHitKnockback(-1f * contact.normal);
			}
			this.SpawnImpactEffect(this.surfaceImpactEffectPrefab, contact.point, contact.normal);
			BuilderProjectile.ProjectileImpactEvent onImpact = this.OnImpact;
			if (onImpact != null)
			{
				onImpact(this, contact.point, null);
			}
			this.Deactivate();
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (!this.particleLaunched)
			{
				return;
			}
			if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null)
			{
				return;
			}
			if (!other.gameObject.IsOnLayer(UnityLayer.GorillaTagCollider))
			{
				return;
			}
			VRRig componentInParent = other.GetComponentInParent<VRRig>();
			NetPlayer netPlayer = ((componentInParent != null) ? componentInParent.creator : null);
			if (netPlayer == null)
			{
				return;
			}
			if (netPlayer.IsLocal)
			{
				return;
			}
			this.SpawnImpactEffect(this.surfaceImpactEffectPrefab, base.transform.position, Vector3.up);
			this.Deactivate();
		}

		public BuilderProjectileLauncher projectileSource;

		[Tooltip("Rotates to point along the Y axis after spawn.")]
		public GameObject surfaceImpactEffectPrefab;

		[Tooltip("Distance from the surface that the particle should spawn.")]
		private float impactEffectOffset;

		public float lifeTime = 20f;

		public bool faceDirectionOfTravel = true;

		private bool particleLaunched;

		private float timeCreated;

		private Rigidbody projectileRigidbody;

		public int projectileId;

		private float initialScale;

		private Vector3 previousPosition;

		[HideInInspector]
		public SlingshotProjectile.AOEKnockbackConfig? aoeKnockbackConfig;

		[HideInInspector]
		public float? impactSoundVolumeOverride;

		[HideInInspector]
		public float? impactSoundPitchOverride;

		[HideInInspector]
		public float impactEffectScaleMultiplier = 1f;

		[HideInInspector]
		public float gravityMultiplier = 1f;

		private ConstantForce forceComponent;

		public delegate void ProjectileImpactEvent(BuilderProjectile projectile, Vector3 impactPos, NetPlayer hitPlayer);
	}
}
