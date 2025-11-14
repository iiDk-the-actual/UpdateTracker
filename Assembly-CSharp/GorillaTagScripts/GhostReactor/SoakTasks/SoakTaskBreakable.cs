using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

namespace GorillaTagScripts.GhostReactor.SoakTasks
{
	public sealed class SoakTaskBreakable : IGhostReactorSoakTask
	{
		public bool Complete { get; private set; }

		public SoakTaskBreakable(GRPlayer grPlayer)
		{
			this._grPlayer = grPlayer;
		}

		public bool Update()
		{
			GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this._grPlayer.gamePlayer.rig.zoneEntity.currentZone);
			if (managerForZone == null)
			{
				return false;
			}
			if (this._breakable != null && this._breakable.GetComponent<GRBreakable>().BrokenLocal)
			{
				Debug.Log(string.Format("soak breakable {0} is broken", this._breakable.id.index));
				this._breakable = null;
				this._nextHitTime = null;
				this.Complete = true;
			}
			else
			{
				if (this._breakable == null)
				{
					using (List<GameEntity>.Enumerator enumerator = managerForZone.GetGameEntities().ShuffleIntoCollection<List<GameEntity>, GameEntity>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							GameEntity gameEntity = enumerator.Current;
							if (!(gameEntity == null) && !gameEntity.IsHeld())
							{
								GRBreakable component = gameEntity.gameObject.GetComponent<GRBreakable>();
								if (component != null && !component.BrokenLocal)
								{
									this._breakable = gameEntity;
									this._nextHitTime = new float?(Time.time + 0.1f);
									break;
								}
							}
						}
						return true;
					}
				}
				if (this._breakable != null)
				{
					float? nextHitTime = this._nextHitTime;
					if (nextHitTime != null)
					{
						float valueOrDefault = nextHitTime.GetValueOrDefault();
						if (Time.time >= valueOrDefault)
						{
							Debug.Log(string.Format("soak hit breakable {0}", this._breakable.id.index));
							GameHitData gameHitData = new GameHitData
							{
								hitEntityId = this._breakable.id,
								hitByEntityId = this._breakable.id,
								hitTypeId = 0,
								hitEntityPosition = Vector3.zero,
								hitPosition = Vector3.zero,
								hitImpulse = Vector3.zero,
								hitAmount = 1
							};
							managerForZone.RequestHit(gameHitData);
						}
					}
				}
			}
			return true;
		}

		public void Reset()
		{
			this._breakable = null;
			this._nextHitTime = null;
			this.Complete = false;
		}

		public const float TIME_BETWEEN_HITS = 0.1f;

		private readonly GRPlayer _grPlayer;

		private GameEntity _breakable;

		private float? _nextHitTime;
	}
}
