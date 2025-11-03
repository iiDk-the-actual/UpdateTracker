using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class TrashcanCosmetic : MonoBehaviour
	{
		public void OnBasket(bool isLeftHand, Collider other)
		{
			SlingshotProjectile slingshotProjectile;
			if (other.TryGetComponent<SlingshotProjectile>(out slingshotProjectile) && slingshotProjectile.GetDistanceTraveled() >= this.minScoringDistance)
			{
				UnityEvent onScored = this.OnScored;
				if (onScored != null)
				{
					onScored.Invoke();
				}
				slingshotProjectile.DestroyAfterRelease();
			}
		}

		public float minScoringDistance = 2f;

		public UnityEvent OnScored;
	}
}
