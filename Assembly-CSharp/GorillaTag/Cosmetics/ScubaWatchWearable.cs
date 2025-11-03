using System;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	[ExecuteAlways]
	public class ScubaWatchWearable : MonoBehaviour
	{
		protected void Update()
		{
			GTPlayer instance = GTPlayer.Instance;
			if (this.onLeftHand)
			{
				if (instance.LeftHandWaterVolume != null)
				{
					this.currentDepth = Mathf.Max(-instance.LeftHandWaterSurface.surfacePlane.GetDistanceToPoint(instance.LastLeftHandPosition), 0f);
				}
				else
				{
					this.currentDepth = 0f;
				}
			}
			else if (instance.RightHandWaterVolume != null)
			{
				this.currentDepth = Mathf.Max(-instance.RightHandWaterSurface.surfacePlane.GetDistanceToPoint(instance.LastRightHandPosition), 0f);
			}
			else
			{
				this.currentDepth = 0f;
			}
			float num = (this.currentDepth - this.depthRange.x) / (this.depthRange.y - this.depthRange.x);
			float num2 = Mathf.Lerp(this.dialRotationRange.x, this.dialRotationRange.y, num);
			this.dialNeedle.localRotation = this.initialDialRotation * Quaternion.AngleAxis(num2, this.dialRotationAxis);
		}

		public bool onLeftHand;

		[Tooltip("The transform that will be rotated to indicate the current depth.")]
		public Transform dialNeedle;

		[Tooltip("If your rotation is not zeroed out then click the Auto button to use the current rotation as 0.")]
		public Quaternion initialDialRotation;

		[Tooltip("The range of depth values that the dial will rotate between.")]
		public Vector2 depthRange = new Vector2(0f, 20f);

		[Tooltip("The range of rotation values that the dial will rotate between.")]
		public Vector2 dialRotationRange = new Vector2(0f, 360f);

		[Tooltip("The axis that the dial will rotate around.")]
		public Vector3 dialRotationAxis = Vector3.right;

		[Tooltip("The current depth of the player.")]
		[DebugOption]
		private float currentDepth;
	}
}
