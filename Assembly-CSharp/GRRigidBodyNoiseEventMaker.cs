using System;
using UnityEngine;

public class GRRigidBodyNoiseEventMaker : MonoBehaviour
{
	public void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > this.velocityThreshold && base.GetComponent<GameEntity>() != null)
		{
			GRNoiseEventManager.instance.AddNoiseEvent(collision.GetContact(0).point, 1f, 1f);
		}
	}

	public float velocityThreshold = 5f;
}
