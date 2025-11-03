using System;
using UnityEngine;

public class GorillaHandHistory : MonoBehaviour
{
	private void Start()
	{
		this.direction = default(Vector3);
		this.lastPosition = default(Vector3);
	}

	private void FixedUpdate()
	{
		this.direction = this.lastPosition - base.transform.position;
		this.lastLastPosition = this.lastPosition;
		this.lastPosition = base.transform.position;
	}

	public Vector3 direction;

	private Vector3 lastPosition;

	private Vector3 lastLastPosition;
}
