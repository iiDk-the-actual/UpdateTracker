using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class SimpleTransformAnimatorCosmetic : MonoBehaviour, ITickSystemTick
	{
		private void DebugToggle()
		{
			this.Toggle();
		}

		private void DebugA()
		{
			this.TogglePoseA();
		}

		private void DebugB()
		{
			this.TogglePoseB();
		}

		public bool TickRunning { get; set; }

		private void OnEnable()
		{
			this.posBlendCurrent = this.posBlendTarget;
			this.UpdateTransform();
		}

		private void OnDisable()
		{
			if (this.TickRunning)
			{
				TickSystem<object>.RemoveCallbackTarget(this);
				this.TickRunning = false;
			}
		}

		private void CheckAnimationNeeded()
		{
			bool flag = false;
			bool flag2 = Mathf.Approximately(this.posBlendCurrent, this.posBlendTarget);
			switch (this.animMode)
			{
			case SimpleTransformAnimatorCosmetic.animModes.stepToTargetPos:
				flag = !flag2;
				break;
			case SimpleTransformAnimatorCosmetic.animModes.animateOneshot:
				flag = this.loopAnim || !flag2;
				break;
			}
			if (flag && !this.TickRunning)
			{
				TickSystem<object>.AddCallbackTarget(this);
				this.TickRunning = true;
				this.isAnimating = true;
				return;
			}
			if (!flag && this.TickRunning)
			{
				TickSystem<object>.RemoveCallbackTarget(this);
				this.TickRunning = false;
				this.isAnimating = false;
			}
		}

		public void Tick()
		{
			float num = 1f / this.animationDuration;
			this.posBlendCurrent = Mathf.MoveTowards(this.posBlendCurrent, this.posBlendTarget, Time.deltaTime * num);
			switch (this.animMode)
			{
			default:
				this.UpdateTransform();
				this.CheckAnimationNeeded();
				return;
			}
		}

		private void UpdateTransform()
		{
			Vector3 vector = this.targetTransform.position;
			Quaternion quaternion = this.targetTransform.rotation;
			float num = this.InterpolationCurve.Evaluate(this.posBlendCurrent);
			if (this.animatedProperties == SimpleTransformAnimatorCosmetic.animatedPropertyChoices.Position || this.animatedProperties == SimpleTransformAnimatorCosmetic.animatedPropertyChoices.PositionAndRotation)
			{
				vector = Vector3.Lerp(this.poseA.position, this.poseB.position, num);
			}
			if (this.animatedProperties == SimpleTransformAnimatorCosmetic.animatedPropertyChoices.Rotation || this.animatedProperties == SimpleTransformAnimatorCosmetic.animatedPropertyChoices.PositionAndRotation)
			{
				quaternion = Quaternion.Slerp(this.poseA.rotation, this.poseB.rotation, num);
			}
			this.targetTransform.SetPositionAndRotation(vector, quaternion);
		}

		public void Toggle()
		{
			this.animMode = SimpleTransformAnimatorCosmetic.animModes.stepToTargetPos;
			this.posBlendTarget = ((this.posBlendTarget < 0.5f) ? 1f : 0f);
			this.CheckAnimationNeeded();
		}

		public void TogglePoseA()
		{
			this.animMode = SimpleTransformAnimatorCosmetic.animModes.stepToTargetPos;
			this.posBlendTarget = 0f;
			this.CheckAnimationNeeded();
		}

		public void TogglePoseB()
		{
			this.animMode = SimpleTransformAnimatorCosmetic.animModes.stepToTargetPos;
			this.posBlendTarget = 1f;
			this.CheckAnimationNeeded();
		}

		public void playAnimationOneshot()
		{
			this.animMode = SimpleTransformAnimatorCosmetic.animModes.animateOneshot;
			this.posBlendCurrent = 0f;
			this.posBlendTarget = 1f;
			this.CheckAnimationNeeded();
		}

		private void DebugPlayAnimationOneShot()
		{
			this.playAnimationOneshot();
		}

		private SimpleTransformAnimatorCosmetic.animModes animMode;

		[Tooltip("Shapes how the transform will interpolate over the course of the animation.")]
		public AnimationCurve InterpolationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		[Tooltip("The object that will animate (blend) between the poses.")]
		private Transform targetTransform;

		[SerializeField]
		[Tooltip("Start pose (blend value 0).")]
		private Transform poseA;

		[SerializeField]
		[Tooltip("End pose (blend value 1).")]
		private Transform poseB;

		[FormerlySerializedAs("transitionTime")]
		[SerializeField]
		[Tooltip("Total time (in seconds) to animate fully between poses.")]
		private float animationDuration = 1f;

		[SerializeField]
		[Tooltip("Controls what aspect of the transform is affected by the blend.")]
		private SimpleTransformAnimatorCosmetic.animatedPropertyChoices animatedProperties = SimpleTransformAnimatorCosmetic.animatedPropertyChoices.PositionAndRotation;

		private bool loopAnim;

		private float posBlendCurrent;

		private float posBlendTarget;

		private bool isAnimating;

		public enum animatedPropertyChoices
		{
			Position,
			Rotation,
			PositionAndRotation
		}

		public enum animModes
		{
			stepToTargetPos,
			animateBounce,
			animateOneshot
		}
	}
}
