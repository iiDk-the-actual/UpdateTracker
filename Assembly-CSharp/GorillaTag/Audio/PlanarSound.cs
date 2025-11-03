using System;
using UnityEngine;

namespace GorillaTag.Audio
{
	public class PlanarSound : MonoBehaviour
	{
		protected void OnEnable()
		{
			if (Camera.main != null)
			{
				this.cameraXform = Camera.main.transform;
				this.hasCamera = true;
			}
		}

		protected void LateUpdate()
		{
			if (!this.hasCamera)
			{
				return;
			}
			Transform transform = base.transform;
			Vector3 vector = transform.parent.InverseTransformPoint(this.cameraXform.position);
			vector.y = 0f;
			if (this.limitDistance && vector.sqrMagnitude > this.maxDistance * this.maxDistance)
			{
				vector = vector.normalized * this.maxDistance;
			}
			transform.localPosition = vector;
		}

		private Transform cameraXform;

		private bool hasCamera;

		[SerializeField]
		private bool limitDistance;

		[SerializeField]
		private float maxDistance = 1f;
	}
}
