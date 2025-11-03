using System;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderParticleSpawner : MonoBehaviour
	{
		private void Start()
		{
			this.spawnTrigger.onTriggerFirstEntered += this.OnEnter;
			this.spawnTrigger.onTriggerLastExited += this.OnExit;
		}

		private void OnDestroy()
		{
			if (this.spawnTrigger != null)
			{
				this.spawnTrigger.onTriggerFirstEntered -= this.OnEnter;
				this.spawnTrigger.onTriggerLastExited -= this.OnExit;
			}
		}

		public void TrySpawning()
		{
			if (Time.time > this.lastSpawnTime + this.cooldown)
			{
				this.lastSpawnTime = Time.time;
				ObjectPools.instance.Instantiate(this.prefab, this.spawnLocation.position, this.spawnLocation.rotation, this.myPiece.GetScale(), true);
			}
		}

		private void OnEnter()
		{
			if (this.spawnOnEnter)
			{
				this.TrySpawning();
			}
		}

		private void OnExit()
		{
			if (this.spawnOnExit)
			{
				this.TrySpawning();
			}
		}

		[SerializeField]
		private BuilderPiece myPiece;

		public GameObject prefab;

		public float cooldown = 0.1f;

		private float lastSpawnTime;

		[SerializeField]
		private BuilderSmallMonkeTrigger spawnTrigger;

		[SerializeField]
		private bool spawnOnEnter = true;

		[SerializeField]
		private bool spawnOnExit;

		[SerializeField]
		private Transform spawnLocation;
	}
}
