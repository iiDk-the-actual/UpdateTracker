using System;
using GorillaExtensions;
using UnityEngine;

namespace Critters.Scripts
{
	public class CrittersKillVolume : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (other.attachedRigidbody)
			{
				CrittersActor component = other.attachedRigidbody.GetComponent<CrittersActor>();
				if (component.IsNotNull())
				{
					component.gameObject.SetActive(false);
				}
			}
		}
	}
}
