using System;
using GorillaExtensions;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class SuperInfectionSnapPoint : MonoBehaviour
{
	public void Initialize()
	{
		VRRig componentInParent = base.GetComponentInParent<VRRig>(true);
		if (componentInParent == null)
		{
			throw new NullReferenceException("[SuperInfectionSnapPoint]  ERROR!!!  Expected a VRRig to be in parent hierarchy. Path=\"" + base.transform.GetPathQ() + "\"");
		}
		Transform[] array;
		string text;
		if (!GTHardCodedBones.TryGetBoneXforms(componentInParent, out array, out text))
		{
			throw new NullReferenceException("[SuperInfectionSnapPoint]  ERROR!!!  Could not get bone transforms: " + text);
		}
		if (this.overrideParentTransform != null)
		{
			this.parentTransform = this.overrideParentTransform;
		}
		else if (!GTHardCodedBones.TryGetBoneXform(array, this.parentBone.Bone, out this.parentTransform))
		{
			throw new NullReferenceException("[SuperInfectionSnapPoint]  ERROR!!!  " + string.Format("Could not find bone Transform `{0}`.", this.parentBone));
		}
		Vector3 localPosition = base.transform.localPosition;
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		if (this.parentTransform != null)
		{
			base.transform.SetParent(this.parentTransform, false);
		}
		base.transform.localPosition = localPosition;
		base.transform.localEulerAngles = localEulerAngles;
	}

	public void Clear()
	{
		this.Unsnapped();
	}

	public void Snapped(GameEntity entity)
	{
		this.snappedEntity = entity;
		GameSnappable gameSnappable;
		if (this.snappedEntity.TryGetComponent<GameSnappable>(out gameSnappable))
		{
			gameSnappable.snappedToJoint = this;
			return;
		}
		Debug.LogError(string.Format("Snapped: entity {0} has no GameSnappable!?", this.snappedEntity));
	}

	public void Unsnapped()
	{
		GameSnappable gameSnappable;
		if (this.snappedEntity.TryGetComponent<GameSnappable>(out gameSnappable))
		{
			gameSnappable.snappedToJoint = null;
		}
		else
		{
			Debug.LogError(string.Format("Unsnapped: entity {0} has no GameSnappable!?", this.snappedEntity));
		}
		this.snappedEntity = null;
	}

	public bool HasSnapped()
	{
		return this.snappedEntity != null;
	}

	public GameEntity GetSnappedEntity()
	{
		return this.snappedEntity;
	}

	private const string preLog = "[SuperInfectionSnapPoint]  ";

	private const string preErr = "[SuperInfectionSnapPoint]  ERROR!!!  ";

	public GamePlayer playerForPoint;

	public SnapJointType jointType;

	public GTHardCodedBones.SturdyEBone parentBone;

	public Transform overrideParentTransform;

	private Transform parentTransform;

	public bool canSnapOverride;

	public float snapPointRadius;

	private GameEntity snappedEntity;
}
