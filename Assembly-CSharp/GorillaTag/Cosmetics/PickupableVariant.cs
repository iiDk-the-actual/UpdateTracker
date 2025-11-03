using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class PickupableVariant : MonoBehaviour
	{
		protected internal virtual void Release(HoldableObject holdable, Vector3 startPosition, Vector3 releaseVelocity, float playerScale)
		{
		}

		protected internal virtual void Pickup()
		{
		}

		protected internal virtual void DelayedPickup()
		{
		}
	}
}
