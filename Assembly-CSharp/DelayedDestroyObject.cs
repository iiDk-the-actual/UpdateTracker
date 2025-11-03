using System;
using UnityEngine;

public class DelayedDestroyObject : MonoBehaviour
{
	private void Start()
	{
		this._timeToDie = Time.time + this.lifetime;
	}

	private void LateUpdate()
	{
		if (Time.time >= this._timeToDie)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public float lifetime = 10f;

	private float _timeToDie;
}
