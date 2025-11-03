using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTag.Reactions
{
	public class FlameThrowerParticleCollisionHandler : MonoBehaviour
	{
		protected void OnEnable()
		{
			if (GorillaComputer.instance == null)
			{
				Debug.LogError("FlameThrowerParticleCollisionHandler: Disabling because GorillaComputer not found! Hierarchy path: " + base.transform.GetPath(), this);
				base.enabled = false;
				return;
			}
			if (this._prefabToSpawn != null && !this._isPrefabInPool)
			{
				if (this._prefabToSpawn.CompareTag("Untagged"))
				{
					Debug.LogError("FlameThrowerParticleCollisionHandler: Disabling because Spawn Prefab has no tag! Hierarchy path: " + base.transform.GetPath(), this);
					base.enabled = false;
					return;
				}
				this._isPrefabInPool = ObjectPools.instance.DoesPoolExist(this._prefabToSpawn);
				if (!this._isPrefabInPool)
				{
					Debug.LogError("FlameThrowerParticleCollisionHandler: Disabling because Spawn Prefab not in pool! Hierarchy path: " + base.transform.GetPath(), this);
					base.enabled = false;
					return;
				}
				this._pool = ObjectPools.instance.GetPoolByObjectType(this._prefabToSpawn);
			}
			this._hasPrefabToSpawn = this._prefabToSpawn != null && this._isPrefabInPool;
			if (this._particleSystem == null)
			{
				this._particleSystem = base.GetComponent<ParticleSystem>();
			}
			if (this._particleSystem == null)
			{
				Debug.LogError("FlameThrowerParticleCollisionHandler: Disabling because could not find ParticleSystem! Hierarchy path: " + base.transform.GetPath(), this);
				base.enabled = false;
				return;
			}
			if (this._collisionEvents == null)
			{
				this._collisionEvents = new List<ParticleCollisionEvent>(this._particleSystem.main.maxParticles);
			}
		}

		protected void OnParticleCollision(GameObject other)
		{
			if (this._maxParticleHitReactionRate < 1E-05f || !FireManager.hasInstance)
			{
				return;
			}
			double num = GTTime.TimeAsDouble();
			if ((float)(num - this._lastCollisionTime) < 1f / this._maxParticleHitReactionRate)
			{
				return;
			}
			if (this._particleSystem.GetCollisionEvents(other, this._collisionEvents) <= 0)
			{
				return;
			}
			if (this._hasPrefabToSpawn && this._isPrefabInPool && this._pool.GetInactiveCount() > 0)
			{
				ParticleCollisionEvent particleCollisionEvent = this._collisionEvents[0];
				FireManager.SpawnFire(this._pool, particleCollisionEvent.intersection, particleCollisionEvent.normal, base.transform.lossyScale.x);
			}
			if (this._extinguishAmount > 0f)
			{
				FireManager.Extinguish(other, this._extinguishAmount);
			}
			this._lastCollisionTime = num;
		}

		[Tooltip("The defaults are numbers for the flamethrower hair dryer.")]
		private readonly float _maxParticleHitReactionRate = 2f;

		[Tooltip("Must be in the global object pool and have a tag.")]
		[SerializeField]
		private GameObject _prefabToSpawn;

		[Tooltip("How much to extinguish any hit fire by.")]
		[SerializeField]
		private float _extinguishAmount;

		private ParticleSystem _particleSystem;

		private List<ParticleCollisionEvent> _collisionEvents;

		private bool _hasPrefabToSpawn;

		private bool _isPrefabInPool;

		private double _lastCollisionTime;

		private SinglePool _pool;
	}
}
