using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDockable : MonoBehaviour
{
	private void Awake()
	{
	}

	public GameEntityId BestDock()
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
		GameDock gameDock = null;
		for (int i = 0; i < snapPoints.Count; i++)
		{
			if (snapPoints[i].jointType != snapJointType && snapPoints[i].jointType != snapJointType2)
			{
				GameEntity snappedEntity = snapPoints[i].GetSnappedEntity();
				if (!(snappedEntity == null))
				{
					GameDock component = snappedEntity.GetComponent<GameDock>();
					if (!(component == null) && component.CanDock(this))
					{
						Transform transform = component.dockMarker.transform;
						Vector3 zero = Vector3.zero;
						Quaternion identity = Quaternion.identity;
						float num2 = Vector3.Distance(transform.TransformPoint(identity * zero), base.transform.position);
						float num3 = this.dockableRadius + component.dockRadius;
						if (num2 < num && num2 < num3)
						{
							num = num2;
							gameDock = component;
						}
					}
				}
			}
		}
		GamePlayer gamePlayer = GamePlayerLocal.instance.gamePlayer;
		for (int j = 0; j < 2; j++)
		{
			GameEntity grabbedGameEntity = gamePlayer.GetGrabbedGameEntity(j);
			if (!(grabbedGameEntity == null))
			{
				GameDock component2 = grabbedGameEntity.GetComponent<GameDock>();
				if (!(component2 == null) && component2.CanDock(this))
				{
					Transform transform2 = component2.dockMarker.transform;
					Vector3 zero2 = Vector3.zero;
					Quaternion identity2 = Quaternion.identity;
					float num2 = Vector3.Distance(transform2.TransformPoint(identity2 * zero2), base.transform.position);
					float num4 = this.dockableRadius + component2.dockRadius;
					if (num2 < num && num2 < num4)
					{
						num = num2;
						gameDock = component2;
					}
				}
			}
		}
		if (gameDock == null)
		{
			return GameEntityId.Invalid;
		}
		return gameDock.gameEntity.id;
	}

	public Transform GetDockablePoint()
	{
		if (!(this.dockablePoint == null))
		{
			return this.dockablePoint;
		}
		return base.transform;
	}

	public void OnDock(GameEntity gameEntity, GameEntity attachedToGameEntity)
	{
	}

	public void OnUndock(GameEntity gameEntity, GameEntity attachedToGameEntity)
	{
	}

	public GameEntity gameEntity;

	public float dockableRadius = 0.15f;

	public Transform dockablePoint;
}
