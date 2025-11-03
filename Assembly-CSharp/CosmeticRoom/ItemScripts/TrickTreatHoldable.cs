using System;
using UnityEngine;

namespace CosmeticRoom.ItemScripts
{
	public class TrickTreatHoldable : TransferrableObject
	{
		protected override void LateUpdateLocal()
		{
			base.LateUpdateLocal();
			if (this.candyCollider)
			{
				this.candyCollider.enabled = this.IsMyItem() && this.IsHeld();
			}
		}

		public MeshCollider candyCollider;
	}
}
