using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSnappable : MonoBehaviour
{
	private void Awake()
	{
	}

	public void GetSnapOffset(SnapJointType jointType, out Vector3 positionOffset, out Quaternion rotationOffset)
	{
		foreach (GameSnappable.SnapJointOffset snapJointOffset in this.snapOffsets)
		{
			if ((snapJointOffset.jointType & jointType) != SnapJointType.None)
			{
				positionOffset = snapJointOffset.positionOffset;
				rotationOffset = Quaternion.Euler(snapJointOffset.rotationOffset);
				return;
			}
		}
		positionOffset = Vector3.zero;
		rotationOffset = Quaternion.identity;
	}

	public SuperInfectionSnapPoint BestSnapPoint()
	{
		int heldByHandIndex = this.gameEntity.heldByHandIndex;
		if (heldByHandIndex < 0)
		{
			return null;
		}
		SnapJointType snapJointType = (GamePlayerLocal.IsLeftHand(heldByHandIndex) ? SnapJointType.ArmL : SnapJointType.ArmR);
		SnapJointType snapJointType2 = (GamePlayerLocal.IsLeftHand(heldByHandIndex) ? SnapJointType.ForearmL : SnapJointType.ForearmR);
		List<SuperInfectionSnapPoint> snapPoints = GamePlayerLocal.instance.gamePlayer.snapPointManager.SnapPoints;
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < snapPoints.Count; i++)
		{
			if (snapPoints[i].jointType != snapJointType && snapPoints[i].jointType != snapJointType2 && (snapPoints[i].jointType & this.snapLocationTypes) != SnapJointType.None && !snapPoints[i].HasSnapped())
			{
				Vector3 vector;
				Quaternion quaternion;
				this.GetSnapOffset(snapPoints[i].jointType, out vector, out quaternion);
				float num3 = Vector3.Distance(snapPoints[i].transform.TransformPoint(quaternion * vector), base.transform.position);
				float num4 = this.snapRadius + snapPoints[i].snapPointRadius;
				if (num3 < num && num3 < num4)
				{
					num2 = i;
					num = num3;
				}
			}
		}
		if (num2 >= 0)
		{
			return snapPoints[num2];
		}
		if ((this.snapLocationTypes & SnapJointType.Holster) != SnapJointType.None)
		{
			GameEntityManager currGameEntityManager = GamePlayerLocal.instance.currGameEntityManager;
			IEnumerable<SuperInfectionSnapPoint> points = ((currGameEntityManager != null) ? currGameEntityManager.superInfectionManager : null).GetPoints(SnapJointType.Holster);
			SuperInfectionSnapPoint superInfectionSnapPoint = null;
			float num5 = this.snapRadius;
			foreach (SuperInfectionSnapPoint superInfectionSnapPoint2 in points)
			{
				if (!superInfectionSnapPoint2.HasSnapped())
				{
					Vector3 vector2;
					Quaternion quaternion2;
					this.GetSnapOffset(superInfectionSnapPoint2.jointType, out vector2, out quaternion2);
					float num6 = Vector3.Distance(superInfectionSnapPoint2.transform.TransformPoint(quaternion2 * vector2), base.transform.position);
					if (num6 < num5)
					{
						superInfectionSnapPoint = superInfectionSnapPoint2;
						num5 = num6;
					}
				}
			}
			if (superInfectionSnapPoint != null)
			{
				return superInfectionSnapPoint;
			}
		}
		return null;
	}

	public GameEntityId BestSnapPointDock()
	{
		int heldByHandIndex = this.gameEntity.heldByHandIndex;
		if (heldByHandIndex < 0)
		{
			return GameEntityId.Invalid;
		}
		SnapJointType snapJointType = (GamePlayerLocal.IsLeftHand(heldByHandIndex) ? SnapJointType.ArmL : SnapJointType.ArmR);
		SnapJointType snapJointType2 = (GamePlayerLocal.IsLeftHand(heldByHandIndex) ? SnapJointType.ForearmL : SnapJointType.ForearmR);
		List<SuperInfectionSnapPoint> snapPoints = GamePlayerLocal.instance.gamePlayer.snapPointManager.SnapPoints;
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < snapPoints.Count; i++)
		{
			if (snapPoints[i].jointType != snapJointType && snapPoints[i].jointType != snapJointType2 && (snapPoints[i].jointType & this.snapLocationTypes) != SnapJointType.None && snapPoints[i].HasSnapped())
			{
				Vector3 vector;
				Quaternion quaternion;
				this.GetSnapOffset(snapPoints[i].jointType, out vector, out quaternion);
				float num3 = Vector3.Distance(snapPoints[i].transform.TransformPoint(quaternion * vector), base.transform.position);
				float num4 = this.snapRadius + snapPoints[i].snapPointRadius;
				if (num3 < num && num3 < num4)
				{
					num2 = i;
					num = num3;
				}
			}
		}
		if (num2 < 0)
		{
			return GameEntityId.Invalid;
		}
		return snapPoints[num2].GetSnappedEntity().id;
	}

	public bool CanGrabWithHand(bool leftHand)
	{
		if (this.snappedToJoint == null)
		{
			return true;
		}
		SnapJointType jointType = this.snappedToJoint.jointType;
		return (leftHand && jointType != SnapJointType.ArmL && jointType != SnapJointType.ForearmL) || (!leftHand && jointType != SnapJointType.ArmR && jointType != SnapJointType.ForearmR);
	}

	public void OnSnap()
	{
		this.snapSound.Play(null);
		this.snapHaptic.PlayIfSnappedLocal(this.gameEntity);
	}

	public bool IsSnappedToLeftArm()
	{
		if (this.snappedToJoint == null)
		{
			return false;
		}
		SnapJointType jointType = this.snappedToJoint.jointType;
		return jointType == SnapJointType.ArmL || jointType == SnapJointType.ForearmL;
	}

	public bool IsSnappedToRightArm()
	{
		if (this.snappedToJoint == null)
		{
			return false;
		}
		SnapJointType jointType = this.snappedToJoint.jointType;
		return jointType == SnapJointType.ArmR || jointType == SnapJointType.ForearmR;
	}

	public void OnUnsnap()
	{
		this.unsnapSound.Play(null);
	}

	public GameEntity gameEntity;

	public float snapRadius = 0.15f;

	public SuperInfectionSnapPoint snappedToJoint;

	public AbilitySound snapSound;

	public AbilitySound unsnapSound;

	public AbilityHaptic snapHaptic;

	public SnapJointType snapLocationTypes;

	public List<GameSnappable.SnapJointOffset> snapOffsets;

	[Serializable]
	public struct SnapJointOffset
	{
		public SnapJointType jointType;

		public Vector3 positionOffset;

		public Vector3 rotationOffset;
	}
}
