using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

public class SuperInfectionSnapPointManager : MonoBehaviour
{
	public void Awake()
	{
		VRRig componentInParent = base.GetComponentInParent<VRRig>(true);
		ISpawnable[] componentsInChildren = base.GetComponentsInChildren<ISpawnable>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnSpawn(componentInParent);
		}
	}

	public void Start()
	{
		foreach (SuperInfectionSnapPoint superInfectionSnapPoint in this.SnapPoints)
		{
			superInfectionSnapPoint.Initialize();
			this.snapPointDict[superInfectionSnapPoint.jointType] = superInfectionSnapPoint;
		}
	}

	public void Clear()
	{
		foreach (SuperInfectionSnapPoint superInfectionSnapPoint in this.SnapPoints)
		{
			superInfectionSnapPoint.Clear();
		}
		this.snapPointDict.Clear();
	}

	public SuperInfectionSnapPoint FindSnapPoint(SnapJointType jointType)
	{
		if (jointType == SnapJointType.None)
		{
			return null;
		}
		if (this.snapPointDict.ContainsKey(jointType))
		{
			return this.snapPointDict[jointType];
		}
		return null;
	}

	public static SuperInfectionSnapPoint FindSnapPoint(GamePlayer player, SnapJointType jointType)
	{
		if (player == null)
		{
			return null;
		}
		return player.snapPointManager.FindSnapPoint(jointType);
	}

	public void DropAllSnappedAuthority()
	{
		for (int i = 0; i < this.SnapPoints.Count; i++)
		{
			GameEntity snappedEntity = this.SnapPoints[i].GetSnappedEntity();
			if (!(snappedEntity == null))
			{
				Vector3 position = snappedEntity.transform.position;
				snappedEntity.manager.RequestGrabEntity(snappedEntity.id, false, Vector3.zero, Quaternion.identity);
				snappedEntity.manager.RequestThrowEntity(snappedEntity.id, false, position, Vector3.zero, Vector3.zero);
			}
		}
	}

	public List<SuperInfectionSnapPoint> SnapPoints;

	private Dictionary<SnapJointType, SuperInfectionSnapPoint> snapPointDict = new Dictionary<SnapJointType, SuperInfectionSnapPoint>();
}
