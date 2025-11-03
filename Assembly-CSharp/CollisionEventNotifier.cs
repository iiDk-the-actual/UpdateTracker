using System;
using UnityEngine;

public class CollisionEventNotifier : MonoBehaviour
{
	public event CollisionEventNotifier.CollisionEvent CollisionEnterEvent;

	public event CollisionEventNotifier.CollisionEvent CollisionExitEvent;

	private void OnCollisionEnter(Collision collision)
	{
		CollisionEventNotifier.CollisionEvent collisionEnterEvent = this.CollisionEnterEvent;
		if (collisionEnterEvent == null)
		{
			return;
		}
		collisionEnterEvent(this, collision);
	}

	private void OnCollisionExit(Collision collision)
	{
		CollisionEventNotifier.CollisionEvent collisionExitEvent = this.CollisionExitEvent;
		if (collisionExitEvent == null)
		{
			return;
		}
		collisionExitEvent(this, collision);
	}

	public delegate void CollisionEvent(CollisionEventNotifier notifier, Collision collision);
}
