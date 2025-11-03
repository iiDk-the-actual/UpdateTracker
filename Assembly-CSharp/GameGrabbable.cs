using System;
using System.Collections.Generic;
using UnityEngine;

public class GameGrabbable : MonoBehaviour
{
	private void Awake()
	{
	}

	public bool GetBestGrabPoint(Vector3 handPos, Quaternion handRot, int handIndex, out GameGrab grab)
	{
		float num = 0.15f;
		bool flag = false;
		grab = default(GameGrab);
		grab.position = base.transform.position;
		grab.rotation = base.transform.rotation;
		bool flag2 = GamePlayer.IsLeftHand(handIndex);
		if (this.snapGrabPoints != null)
		{
			for (int i = 0; i < this.snapGrabPoints.Count; i++)
			{
				GameGrabbable.SnapGrabPoints snapGrabPoints = this.snapGrabPoints[i];
				if (snapGrabPoints.isLeftHand == flag2 && Vector3.Dot(snapGrabPoints.handTransform.rotation * GameGrabbable.GRAB_UP, handRot * GameGrabbable.GRAB_UP) >= 0f && Vector3.Dot(snapGrabPoints.handTransform.rotation * GameGrabbable.GRAB_PALM, handRot * GameGrabbable.GRAB_PALM) >= 0f && (double)(handPos - snapGrabPoints.handTransform.position).sqrMagnitude <= 0.0225)
				{
					grab.position = handPos + handRot * Quaternion.Inverse(snapGrabPoints.handTransform.localRotation) * -snapGrabPoints.handTransform.localPosition;
					grab.rotation = handRot * Quaternion.Inverse(snapGrabPoints.handTransform.localRotation);
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		Vector3 vector = grab.position - handPos;
		if (vector.sqrMagnitude > num * num)
		{
			grab.position = handPos + vector.normalized * num;
		}
		return true;
	}

	public GameEntity gameEntity;

	public List<GameGrabbable.SnapGrabPoints> snapGrabPoints;

	private static readonly Vector3 GRAB_UP = new Vector3(0f, 0f, 1f);

	private static readonly Vector3 GRAB_PALM = new Vector3(1f, 0f, 0f);

	[Serializable]
	public class SnapGrabPoints
	{
		public bool isLeftHand;

		public Transform handTransform;
	}
}
