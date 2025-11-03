using System;
using GorillaExtensions;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTag.Reactions
{
	public class SpawnWorldEffects : MonoBehaviour
	{
		protected void OnEnable()
		{
			if (GorillaComputer.instance == null)
			{
				Debug.LogError("SpawnWorldEffects: Disabling because GorillaComputer not found! Hierarchy path: " + base.transform.GetPath(), this);
				base.enabled = false;
				return;
			}
			if (this._prefabToSpawn != null && !this._isPrefabInPool)
			{
				if (this._prefabToSpawn.CompareTag("Untagged"))
				{
					Debug.LogError("SpawnWorldEffects: Disabling because Spawn Prefab has no tag! Hierarchy path: " + base.transform.GetPath(), this);
					base.enabled = false;
					return;
				}
				this._isPrefabInPool = ObjectPools.instance.DoesPoolExist(this._prefabToSpawn);
				if (!this._isPrefabInPool)
				{
					Debug.LogError("SpawnWorldEffects: Disabling because Spawn Prefab not in pool! Hierarchy path: " + base.transform.GetPath(), this);
					base.enabled = false;
					return;
				}
				this._pool = ObjectPools.instance.GetPoolByObjectType(this._prefabToSpawn);
			}
			this._hasPrefabToSpawn = this._prefabToSpawn != null && this._isPrefabInPool;
		}

		public void RequestSpawn(Vector3 worldPosition)
		{
			this.RequestSpawn(worldPosition, Vector3.up);
		}

		public void RequestSpawn(Vector3 worldPosition, Vector3 normal)
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
			if (this._hasPrefabToSpawn && this._isPrefabInPool && this._pool.GetInactiveCount() > 0)
			{
				FireManager.SpawnFire(this._pool, worldPosition, normal, base.transform.lossyScale.x);
			}
			this._lastCollisionTime = num;
		}

		[Tooltip("The defaults are numbers for the flamethrower hair dryer.")]
		private readonly float _maxParticleHitReactionRate = 2f;

		[Tooltip("Must be in the global object pool and have a tag.")]
		[SerializeField]
		private GameObject _prefabToSpawn;

		private bool _hasPrefabToSpawn;

		private bool _isPrefabInPool;

		private double _lastCollisionTime;

		private SinglePool _pool;
	}
}
