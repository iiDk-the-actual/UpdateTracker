using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class UpdateBlendShapeCosmetic : MonoBehaviour
	{
		private void Awake()
		{
			this.targetWeight = this.blendStartWeight;
			this.currentWeight = 0f;
		}

		private void Update()
		{
			this.currentWeight = Mathf.Lerp(this.currentWeight, this.targetWeight, Time.deltaTime * this.blendSpeed);
			this.skinnedMeshRenderer.SetBlendShapeWeight(this.blendShapeIndex, this.currentWeight);
		}

		public void SetBlendValue(bool leftHand, float value)
		{
			this.targetWeight = Mathf.Clamp01(this.invertPassedBlend ? (1f - value) : value) * this.maxBlendShapeWeight;
		}

		public void SetBlendValue(float value)
		{
			this.targetWeight = Mathf.Clamp01(this.invertPassedBlend ? (1f - value) : value) * this.maxBlendShapeWeight;
		}

		public void FullyBlend()
		{
			this.targetWeight = this.maxBlendShapeWeight;
		}

		public void ResetBlend()
		{
			this.targetWeight = 0f;
		}

		public float GetBlendValue()
		{
			return this.skinnedMeshRenderer.GetBlendShapeWeight(this.blendShapeIndex);
		}

		[Tooltip("The SkinnedMeshRenderer whose BlendShape weight will be updated. This must reference a mesh that has BlendShapes defined in its import settings.")]
		[SerializeField]
		private SkinnedMeshRenderer skinnedMeshRenderer;

		[Tooltip("Maximum blend shape weight applied when fully blended. Usually 100 for standard Unity BlendShapes.")]
		public float maxBlendShapeWeight = 100f;

		[Tooltip("Index of the BlendShape to control. You can find this index in the SkinnedMeshRenderer inspector under 'BlendShapes'.")]
		[SerializeField]
		private int blendShapeIndex;

		[Tooltip("Speed at which the BlendShape transitions toward its target weight. Higher values make blending more responsive, lower values make it smoother.")]
		[SerializeField]
		private float blendSpeed = 10f;

		[Tooltip("Initial BlendShape weight set when the component awakens. Useful for setting a default deformation state.")]
		[SerializeField]
		private float blendStartWeight;

		[Tooltip("If enabled, inverts the incoming blend value (e.g. 0 → 1, 0.2 → 0.8). Useful when an input should drive the opposite direction of deformation.")]
		[SerializeField]
		private bool invertPassedBlend;

		private float targetWeight;

		private float currentWeight;
	}
}
