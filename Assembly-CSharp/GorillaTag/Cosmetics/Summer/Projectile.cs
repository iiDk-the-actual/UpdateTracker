using System;
using System.Collections.Generic;
using GorillaTag.Reactions;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics.Summer
{
	public class Projectile : MonoBehaviour, IProjectile
	{
		protected void Awake()
		{
			this.rigidbody = base.GetComponentInChildren<Rigidbody>();
			this.impactEffectSpawned = false;
			this.forceComponent = base.GetComponent<ConstantForce>();
		}

		protected void OnEnable()
		{
		}

		public void Launch(Vector3 startPosition, Quaternion startRotation, Vector3 velocity, float chargeFrac, VRRig ownerRig, int progressStep)
		{
			Transform transform = base.transform;
			transform.SetPositionAndRotation(startPosition, startRotation);
			transform.localScale = Vector3.one * ownerRig.scaleFactor;
			if (this.rigidbody != null)
			{
				this.rigidbody.linearVelocity = velocity;
			}
			if (this.audioSource && this.launchAudio)
			{
				this.audioSource.GTPlayOneShot(this.launchAudio, 1f);
			}
			UnityEvent<float> unityEvent = this.onLaunchShared;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(chargeFrac);
		}

		private bool IsTagValid(GameObject obj)
		{
			return this.collisionTags.Contains(obj.tag);
		}

		private void HandleImpact(GameObject hitObject, Vector3 hitPosition, Vector3 hitNormal)
		{
			if (this.impactEffectSpawned)
			{
				return;
			}
			if (this.collisionTags.Count > 0 && !this.IsTagValid(hitObject))
			{
				return;
			}
			if (((1 << hitObject.layer) & this.collisionLayerMasks) == 0)
			{
				return;
			}
			this.SpawnImpactEffect(this.impactEffect, hitPosition, hitNormal);
			if (this.impactEffect != null)
			{
				SoundBankPlayer component = this.impactEffect.GetComponent<SoundBankPlayer>();
				if (component != null && !component.playOnEnable)
				{
					component.Play();
				}
			}
			this.impactEffectSpawned = true;
			if (this.destroyOnCollisionEnter)
			{
				if (this.destroyDelay > 0f)
				{
					base.Invoke("DestroyProjectile", this.destroyDelay);
					return;
				}
				this.DestroyProjectile();
			}
		}

		private void GetColliderHitInfo(Collider other, out Vector3 position, out Vector3 normal)
		{
			Vector3 vector = Time.fixedDeltaTime * 2f * this.rigidbody.linearVelocity;
			Vector3 vector2 = base.transform.position - vector;
			float magnitude = vector.magnitude;
			RaycastHit raycastHit;
			other.Raycast(new Ray(vector2, vector / magnitude), out raycastHit, 2f * magnitude);
			position = raycastHit.point;
			normal = raycastHit.normal;
		}

		private void OnCollisionEnter(Collision other)
		{
			ContactPoint contact = other.GetContact(0);
			this.HandleImpact(other.gameObject, contact.point, contact.normal);
		}

		private void OnCollisionStay(Collision other)
		{
			ContactPoint contact = other.GetContact(0);
			this.HandleImpact(other.gameObject, contact.point, contact.normal);
		}

		private void OnTriggerEnter(Collider other)
		{
			Vector3 vector;
			Vector3 vector2;
			this.GetColliderHitInfo(other, out vector, out vector2);
			this.HandleImpact(other.gameObject, vector, vector2);
		}

		private void OnTriggerStay(Collider other)
		{
			Transform transform = base.transform;
			this.HandleImpact(other.gameObject, transform.position, -transform.forward);
		}

		private void SpawnImpactEffect(GameObject prefab, Vector3 position, Vector3 normal)
		{
			if (prefab != null)
			{
				Vector3 vector = position + normal * this.impactEffectOffset;
				GameObject gameObject = ObjectPools.instance.Instantiate(prefab, vector, true);
				gameObject.transform.up = normal;
				gameObject.transform.position = vector;
			}
			this.onImpactShared.Invoke();
			if (this.spawnWorldEffects != null)
			{
				this.spawnWorldEffects.RequestSpawn(position, normal);
			}
		}

		private void DestroyProjectile()
		{
			this.impactEffectSpawned = false;
			if (this.forceComponent)
			{
				this.forceComponent.enabled = false;
			}
			if (ObjectPools.instance.DoesPoolExist(base.gameObject))
			{
				ObjectPools.instance.Destroy(base.gameObject);
				return;
			}
			Object.Destroy(base.gameObject);
		}

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private GameObject impactEffect;

		[SerializeField]
		private AudioClip launchAudio;

		[SerializeField]
		private LayerMask collisionLayerMasks;

		[SerializeField]
		private List<string> collisionTags = new List<string>();

		[SerializeField]
		private bool destroyOnCollisionEnter;

		[SerializeField]
		private float destroyDelay = 1f;

		[Tooltip("Distance from the surface that the particle should spawn.")]
		[SerializeField]
		private float impactEffectOffset = 0.1f;

		[SerializeField]
		private SpawnWorldEffects spawnWorldEffects;

		private ConstantForce forceComponent;

		public UnityEvent<float> onLaunchShared;

		public UnityEvent onImpactShared;

		private bool impactEffectSpawned;

		private Rigidbody rigidbody;
	}
}
