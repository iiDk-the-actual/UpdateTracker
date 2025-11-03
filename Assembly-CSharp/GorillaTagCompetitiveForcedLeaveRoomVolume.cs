using System;
using GorillaGameModes;
using UnityEngine;

public class GorillaTagCompetitiveForcedLeaveRoomVolume : MonoBehaviour
{
	private void Start()
	{
		this.VolumeCollider = base.GetComponent<Collider>();
		this.CompetitiveManager = GameMode.GetGameModeInstance(GameModeType.InfectionCompetitive) as GorillaTagCompetitiveManager;
		if (this.CompetitiveManager != null)
		{
			this.CompetitiveManager.RegisterForcedLeaveVolume(this);
		}
	}

	private void OnDestroy()
	{
		if (this.CompetitiveManager != null)
		{
			this.CompetitiveManager.UnregisterForcedLeaveVolume(this);
		}
	}

	public bool ContainsPoint(Vector3 position)
	{
		SphereCollider sphereCollider = this.VolumeCollider as SphereCollider;
		if (sphereCollider != null)
		{
			return Vector3.SqrMagnitude(position - (sphereCollider.transform.position + sphereCollider.center)) <= sphereCollider.radius * sphereCollider.radius;
		}
		BoxCollider boxCollider = this.VolumeCollider as BoxCollider;
		return boxCollider != null && boxCollider.bounds.Contains(position);
	}

	private GorillaTagCompetitiveManager CompetitiveManager;

	private Collider VolumeCollider;
}
