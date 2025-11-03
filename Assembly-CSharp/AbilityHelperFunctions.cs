using System;
using UnityEngine;
using UnityEngine.AI;

public static class AbilityHelperFunctions
{
	public static float EaseOutPower(float t, float power)
	{
		return 1f - Mathf.Pow(1f - t, power);
	}

	public static int RandomRangeUnique(int minInclusive, int maxExclusive, int lastValue)
	{
		int num = maxExclusive - minInclusive;
		if (num <= 1)
		{
			return minInclusive;
		}
		int num2 = Random.Range(minInclusive, maxExclusive);
		if (num2 != lastValue)
		{
			return num2;
		}
		return (num2 + 1) % num;
	}

	public static int GetNavMeshWalkableArea()
	{
		if (AbilityHelperFunctions.navMeshWalkableArea == -1)
		{
			AbilityHelperFunctions.navMeshWalkableArea = NavMesh.GetAreaFromName("walkable");
		}
		return AbilityHelperFunctions.navMeshWalkableArea;
	}

	public static Vector3? GetLocationToInvestigate(Vector3 listenerLocation, float hearingRadius, Vector3? currentInvestigationLocation)
	{
		GameNoiseEvent gameNoiseEvent;
		NavMeshHit navMeshHit;
		if (GRNoiseEventManager.instance.GetMostRecentNoiseEventInRadius(listenerLocation, hearingRadius, out gameNoiseEvent) && NavMesh.SamplePosition(gameNoiseEvent.position, out navMeshHit, 1f, AbilityHelperFunctions.GetNavMeshWalkableArea()))
		{
			return new Vector3?(navMeshHit.position);
		}
		if (currentInvestigationLocation != null)
		{
			return currentInvestigationLocation;
		}
		return null;
	}

	private static int navMeshWalkableArea = -1;
}
