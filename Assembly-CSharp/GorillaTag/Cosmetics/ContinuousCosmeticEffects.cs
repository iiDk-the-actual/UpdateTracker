using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class ContinuousCosmeticEffects : MonoBehaviour
	{
		public void ApplyAll(float f)
		{
			this.continuousProperties.ApplyAll(f);
		}

		[FormerlySerializedAs("properties")]
		[SerializeField]
		private ContinuousPropertyArray continuousProperties;
	}
}
