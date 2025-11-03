using System;
using UnityEngine;

public class GREntityLifetime : MonoBehaviour
{
	private void Start()
	{
		this.entity = base.GetComponent<GameEntity>();
		base.Invoke("DestroySelf", this.Lifetime);
	}

	private void Update()
	{
	}

	private void DestroySelf()
	{
		if (this.entity != null)
		{
			this.entity.manager.RequestDestroyItem(this.entity.id);
		}
	}

	public float Lifetime = 3f;

	private GameEntity entity;
}
