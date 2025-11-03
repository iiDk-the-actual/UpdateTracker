using System;
using UnityEngine;

namespace GorillaTag
{
	[DefaultExecutionOrder(2000)]
	public class StaticLodGroup : MonoBehaviour
	{
		protected void Awake()
		{
			this.index = StaticLodManager.Register(this);
		}

		protected void OnEnable()
		{
			StaticLodManager.SetEnabled(this.index, true);
		}

		protected void OnDisable()
		{
			StaticLodManager.SetEnabled(this.index, false);
		}

		private void OnDestroy()
		{
			StaticLodManager.Unregister(this.index);
		}

		public const int k_monoDefaultExecutionOrder = 2000;

		private int index;

		public float collisionEnableDistance = 3f;

		public float uiFadeDistanceMax = 10f;
	}
}
