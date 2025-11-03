using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class CompassNeedleRotator : MonoBehaviour
	{
		protected void OnEnable()
		{
			this.currentVelocity = 0f;
			base.transform.localRotation = Quaternion.identity;
		}

		protected void LateUpdate()
		{
			Transform transform = base.transform;
			Vector3 forward = transform.forward;
			forward.y = 0f;
			forward.Normalize();
			float num = Mathf.SmoothDamp(Vector3.SignedAngle(forward, Vector3.forward, Vector3.up), 0f, ref this.currentVelocity, 0.005f);
			transform.Rotate(transform.up, num, Space.World);
		}

		private const float smoothTime = 0.005f;

		private float currentVelocity;
	}
}
