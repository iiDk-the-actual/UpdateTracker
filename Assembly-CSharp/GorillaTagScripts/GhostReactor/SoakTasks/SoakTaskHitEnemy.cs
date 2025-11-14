using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

namespace GorillaTagScripts.GhostReactor.SoakTasks
{
	public sealed class SoakTaskHitEnemy : IGhostReactorSoakTask
	{
		public bool Complete { get; private set; }

		public SoakTaskHitEnemy(GRPlayer grPlayer)
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
			if (this._enemy != null && !SoakTaskHitEnemy.IsLivingEnemy(this._enemy))
			{
				Debug.Log(string.Format("soak enemy {0} is dead", this._enemy.id.index));
				this.Complete = true;
				return true;
			}
			if (this._enemy == null)
			{
				foreach (GameEntity gameEntity in managerForZone.GetGameEntities().ShuffleIntoCollection<List<GameEntity>, GameEntity>())
				{
					if (!(gameEntity == null) && !gameEntity.IsHeld() && !(gameEntity.GetComponent<GameAgent>() == null) && !(gameEntity.GetComponent<GameHittable>() == null) && SoakTaskHitEnemy.IsEnemy(gameEntity))
					{
						this._enemy = gameEntity;
						this._nextHitTime = new float?(Time.time + 0.1f);
						break;
					}
				}
				return this._enemy != null;
			}
			if (this._nextHitTime == null)
			{
				throw new Exception("Invalid state in HitEnemySoakTask.");
			}
			if (Time.time < this._nextHitTime.Value)
			{
				return true;
			}
			Debug.Log(string.Format("soak hitting enemy {0}", this._enemy.id.index));
			GameEntity randomTool = this.GetRandomTool();
			if (randomTool == null)
			{
				Debug.LogError("No club found for soak task hit enemy.");
				return false;
			}
			GameHitData gameHitData = new GameHitData
			{
				hitEntityId = this._enemy.id,
				hitByEntityId = randomTool.id,
				hitTypeId = 0,
				hitEntityPosition = Vector3.zero,
				hitPosition = Vector3.zero,
				hitImpulse = Vector3.zero,
				hitAmount = 1
			};
			managerForZone.RequestHit(gameHitData);
			this._nextHitTime = new float?(Time.time + 0.1f);
			return true;
		}

		public void Reset()
		{
			this._enemy = null;
			this._nextHitTime = null;
			this.Complete = false;
		}

		private GameEntity GetRandomTool()
		{
			GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this._grPlayer.gamePlayer.rig.zoneEntity.currentZone);
			if (managerForZone == null)
			{
				return null;
			}
			foreach (GameEntity gameEntity in managerForZone.GetGameEntities().ShuffleIntoCollection<List<GameEntity>, GameEntity>())
			{
				if (!(gameEntity == null))
				{
					GRTool component = gameEntity.GetComponent<GRTool>();
					if (component != null)
					{
						GRTool.GRToolType toolType = component.toolType;
						if (toolType == GRTool.GRToolType.Club || toolType == GRTool.GRToolType.HockeyStick)
						{
							return gameEntity;
						}
					}
				}
			}
			return null;
		}

		private static bool IsEnemy(GameEntity entity)
		{
			return entity.GetComponent<GREnemyChaser>() != null || entity.GetComponent<GREnemyPest>() != null || entity.GetComponent<GREnemyRanged>() != null || entity.GetComponent<GREnemySummoner>() != null;
		}

		private static bool IsLivingEnemy(GameEntity entity)
		{
			if (SoakTaskHitEnemy.IsEnemy(entity))
			{
				GREnemyChaser component = entity.GetComponent<GREnemyChaser>();
				if (component == null || component.hp <= 0)
				{
					GREnemyPest component2 = entity.GetComponent<GREnemyPest>();
					if (component2 == null || component2.hp <= 0)
					{
						GREnemyRanged component3 = entity.GetComponent<GREnemyRanged>();
						if (component3 == null || component3.hp <= 0)
						{
							GREnemySummoner component4 = entity.GetComponent<GREnemySummoner>();
							return component4 != null && component4.hp > 0;
						}
					}
				}
				return true;
			}
			return false;
		}

		public const float TIME_BETWEEN_HITS = 0.1f;

		private readonly GRPlayer _grPlayer;

		private GameEntity _enemy;

		private float? _nextHitTime;
	}
}
