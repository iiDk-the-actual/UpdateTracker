using System;
using UnityEngine;

namespace GorillaTag.Rendering
{
	public sealed class ZoneLiquidEffectable : MonoBehaviour
	{
		private void Awake()
		{
			this.childRenderers = base.GetComponentsInChildren<Renderer>(false);
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		public float radius = 1f;

		[NonSerialized]
		public bool inLiquidVolume;

		[NonSerialized]
		public bool wasInLiquidVolume;

		[NonSerialized]
		public Renderer[] childRenderers;
	}
}
