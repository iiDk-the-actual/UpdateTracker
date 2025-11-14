using System;
using UnityEngine;

namespace GorillaTagScripts.GhostReactor
{
	public static class GREnemyTypeExtensions
	{
		public static Type GetComponentType(this GREnemyType enemyType)
		{
			Type type;
			switch (enemyType)
			{
			case GREnemyType.Chaser:
				type = typeof(GREnemyChaser);
				break;
			case GREnemyType.Pest:
				type = typeof(GREnemyPest);
				break;
			case GREnemyType.Phantom:
				type = typeof(GREnemyPhantom);
				break;
			case GREnemyType.Ranged:
				type = typeof(GREnemyRanged);
				break;
			case GREnemyType.Summoner:
				type = typeof(GREnemySummoner);
				break;
			default:
				type = null;
				break;
			}
			return type;
		}

		public static GREnemyType? GetEnemyType(this GameEntity entity)
		{
			GameObject gameObject = entity.gameObject;
			foreach (object obj in Enum.GetValues(typeof(GREnemyType)))
			{
				GREnemyType grenemyType = (GREnemyType)obj;
				Type componentType = grenemyType.GetComponentType();
				if (componentType != null && gameObject.GetComponent(componentType) != null)
				{
					return new GREnemyType?(grenemyType);
				}
			}
			return null;
		}
	}
}
