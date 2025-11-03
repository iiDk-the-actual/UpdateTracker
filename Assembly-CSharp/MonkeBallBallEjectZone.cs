using System;
using UnityEngine;

public class MonkeBallBallEjectZone : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		GameBall component = collision.gameObject.GetComponent<GameBall>();
		if (component != null && collision.contacts.Length != 0)
		{
			component.SetVelocity(collision.contacts[0].impulse.normalized * this.ejectVelocity);
		}
	}

	public Transform target;

	public float ejectVelocity = 15f;
}
