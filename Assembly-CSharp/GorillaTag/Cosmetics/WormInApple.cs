using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class WormInApple : MonoBehaviour
	{
		public void OnHandTap()
		{
			if (this.blendShapeCosmetic && this.blendShapeCosmetic.GetBlendValue() > 0.5f)
			{
				UnityEvent onHandTapped = this.OnHandTapped;
				if (onHandTapped == null)
				{
					return;
				}
				onHandTapped.Invoke();
			}
		}

		[SerializeField]
		private UpdateBlendShapeCosmetic blendShapeCosmetic;

		public UnityEvent OnHandTapped;
	}
}
