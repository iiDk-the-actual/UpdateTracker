using System;
using GorillaLocomotion;
using UnityEngine;

[Serializable]
internal class HandTransformFollowOffest
{
	internal void UpdatePositionRotation()
	{
		if (this.followTransform == null || this.targetTransforms == null)
		{
			return;
		}
		this.position = this.followTransform.position + this.followTransform.rotation * this.positionOffset * GTPlayer.Instance.scale;
		this.rotation = this.followTransform.rotation * this.rotationOffset;
		foreach (Transform transform in this.targetTransforms)
		{
			transform.position = this.position;
			transform.rotation = this.rotation;
		}
	}

	internal Transform followTransform;

	[SerializeField]
	private Transform[] targetTransforms;

	[SerializeField]
	internal Vector3 positionOffset;

	[SerializeField]
	internal Quaternion rotationOffset;

	private Vector3 position;

	private Quaternion rotation;
}
