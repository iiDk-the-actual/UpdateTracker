using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionListener : MonoBehaviour
{
	private void Awake()
	{
		this._events = new List<ParticleCollisionEvent>();
	}

	protected virtual void OnCollisionEvent(ParticleCollisionEvent ev)
	{
	}

	public void OnParticleCollision(GameObject other)
	{
		int collisionEvents = this.target.GetCollisionEvents(other, this._events);
		for (int i = 0; i < collisionEvents; i++)
		{
			this.OnCollisionEvent(this._events[i]);
		}
	}

	public ParticleSystem target;

	[SerializeReference]
	private List<ParticleCollisionEvent> _events = new List<ParticleCollisionEvent>();
}
