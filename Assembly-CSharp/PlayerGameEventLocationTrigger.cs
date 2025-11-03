using System;
using UnityEngine;

public class PlayerGameEventLocationTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == GorillaTagger.Instance.headCollider.gameObject)
		{
			PlayerGameEvents.TriggerEnterLocation(this.locationName);
		}
	}

	[SerializeField]
	private string locationName;
}
