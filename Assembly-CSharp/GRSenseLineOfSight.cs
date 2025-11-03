using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GRSenseLineOfSight
{
	public bool HasLineOfSight(Vector3 headPos, Vector3 targetPos)
	{
		return GRSenseLineOfSight.HasLineOfSight(headPos, targetPos, this.sightDist, this.visibilityMask.value, this.rayCastMode);
	}

	public static bool HasLineOfSight(Vector3 headPos, Vector3 targetPos, float sightDist, int layerMask, GRSenseLineOfSight.RaycastMode rayCastMode = GRSenseLineOfSight.RaycastMode.Geometry)
	{
		switch (rayCastMode)
		{
		case GRSenseLineOfSight.RaycastMode.Geometry:
			return GRSenseLineOfSight.HasGeoLineOfSight(headPos, targetPos, sightDist, layerMask);
		case GRSenseLineOfSight.RaycastMode.Navmesh:
			return GRSenseLineOfSight.HasNavmeshLineOfSight(headPos, targetPos, sightDist);
		case GRSenseLineOfSight.RaycastMode.GeometryAndNavMesh:
			return GRSenseLineOfSight.HasGeoLineOfSight(headPos, targetPos, sightDist, layerMask) && GRSenseLineOfSight.HasNavmeshLineOfSight(headPos, targetPos, sightDist);
		case GRSenseLineOfSight.RaycastMode.GeometryOrNavMesh:
			return GRSenseLineOfSight.HasNavmeshLineOfSight(headPos, targetPos, sightDist) || GRSenseLineOfSight.HasGeoLineOfSight(headPos, targetPos, sightDist, layerMask);
		default:
			return false;
		}
	}

	public static bool HasGeoLineOfSight(Vector3 headPos, Vector3 targetPos, float sightDist, int layerMask)
	{
		float num = Vector3.Distance(targetPos, headPos);
		return num <= sightDist && Physics.RaycastNonAlloc(new Ray(headPos, targetPos - headPos), GRSenseLineOfSight.visibilityHits, Mathf.Min(num, sightDist), layerMask, QueryTriggerInteraction.Ignore) < 1;
	}

	public static bool HasNavmeshLineOfSight(Vector3 headPos, Vector3 targetPos, float sightDist)
	{
		NavMeshHit navMeshHit;
		NavMeshHit navMeshHit2;
		return (targetPos - headPos).sqrMagnitude <= sightDist * sightDist && NavMesh.SamplePosition(headPos, out navMeshHit, 1f, -1) && !NavMesh.Raycast(navMeshHit.position, targetPos, out navMeshHit2, -1);
	}

	public float sightDist;

	public LayerMask visibilityMask;

	public GRSenseLineOfSight.RaycastMode rayCastMode;

	public static RaycastHit[] visibilityHits = new RaycastHit[16];

	public enum RaycastMode
	{
		Geometry,
		Navmesh,
		GeometryAndNavMesh,
		GeometryOrNavMesh
	}
}
