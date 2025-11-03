using System;
using JetBrains.Annotations;
using UnityEngine;

public class WorldTargetItem
{
	public bool IsValid()
	{
		return this.itemIdx != -1 && this.owner != null;
	}

	[CanBeNull]
	public static WorldTargetItem GenerateTargetFromPlayerAndID(NetPlayer owner, int itemIdx)
	{
		VRRig vrrig = GorillaGameManager.StaticFindRigForPlayer(owner);
		if (vrrig == null)
		{
			Debug.LogError("Tried to setup a sharable object but the target rig is null...");
			return null;
		}
		Transform component = vrrig.myBodyDockPositions.TransferrableItem(itemIdx).gameObject.GetComponent<Transform>();
		return new WorldTargetItem(owner, itemIdx, component);
	}

	public static WorldTargetItem GenerateTargetFromWorldSharableItem(NetPlayer owner, int itemIdx, Transform transform)
	{
		return new WorldTargetItem(owner, itemIdx, transform);
	}

	private WorldTargetItem(NetPlayer owner, int itemIdx, Transform transform)
	{
		this.owner = owner;
		this.itemIdx = itemIdx;
		this.targetObject = transform;
		this.transferrableObject = transform.GetComponent<TransferrableObject>();
	}

	public override string ToString()
	{
		return string.Format("Id: {0} ({1})", this.itemIdx, this.owner);
	}

	public readonly NetPlayer owner;

	public readonly int itemIdx;

	public readonly Transform targetObject;

	public readonly TransferrableObject transferrableObject;
}
