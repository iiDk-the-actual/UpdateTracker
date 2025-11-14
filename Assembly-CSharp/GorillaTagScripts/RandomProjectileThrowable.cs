using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTagScripts
{
	public class RandomProjectileThrowable : MonoBehaviour
	{
		public float TimeEnabled { get; private set; }

		public bool ForceDestroy { get; set; }

		private void OnEnable()
		{
			this.TimeEnabled = Time.time;
			this.currentProjectile = this.projectilePrefab;
		}

		private void OnDisable()
		{
			this.ForceDestroy = false;
		}

		public void ForceDestroyThrowable()
		{
			this.ForceDestroy = true;
		}

		public void UpdateProjectilePrefab()
		{
			this.currentProjectile = this.alternativeProjectilePrefab;
		}

		public GameObject GetProjectilePrefab()
		{
			return this.currentProjectile;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!this.destroyOnTrigger)
			{
				return;
			}
			if (other.gameObject.layer == LayerMask.NameToLayer(this.triggerTag))
			{
				if (this.audioSource && this.triggerClip)
				{
					this.audioSource.GTPlayOneShot(this.triggerClip, 1f);
				}
				UnityEvent onDestroyed = this.OnDestroyed;
				if (onDestroyed != null)
				{
					onDestroyed.Invoke();
				}
				this.DestroyProjectile();
			}
		}

		public void DestroyProjectile()
		{
			base.StartCoroutine(this.DestroyProjectileCoroutine(0.25f));
		}

		private IEnumerator DestroyProjectileCoroutine(float delay)
		{
			yield return new WaitForSeconds(delay);
			UnityAction<bool> onDestroyRandomProjectile = this.OnDestroyRandomProjectile;
			if (onDestroyRandomProjectile != null)
			{
				onDestroyRandomProjectile(false);
			}
			yield break;
		}

		public GameObject projectilePrefab;

		[Tooltip("Use for a different/updated version of the projectile if needed.")]
		public GameObject alternativeProjectilePrefab;

		[FormerlySerializedAs("weightedChance")]
		[Range(0f, 1f)]
		public float spawnChance = 1f;

		[Tooltip("Requires a collider")]
		public bool destroyOnTrigger = true;

		public string triggerTag = "Gorilla Head";

		[FormerlySerializedAs("onMoveToHead")]
		public UnityEvent OnDestroyed;

		public AudioSource audioSource;

		public AudioClip triggerClip;

		[Tooltip("Immediately destroys after the release")]
		public bool destroyAfterRelease;

		[Tooltip("Set a timer to destroy after X seconds is passed and the object is not thrown yet")]
		[FormerlySerializedAs("destroyAfterSeconds")]
		public float autoDestroyAfterSeconds = -1f;

		[Tooltip("If checked, any amount of passed time will be deducted from the lifetime of the slingshot projectile when thrownShould be less than or equal to lifetime of the slingshot projectile")]
		public bool moveOverPassedLifeTime;

		public UnityAction<bool> OnDestroyRandomProjectile;

		private GameObject currentProjectile;
	}
}
