using System;
using UnityEngine;

public class GRTransformLook : MonoBehaviour
{
	private void Awake()
	{
		if (this.followPlayer)
		{
			this.lookTarget = Camera.main.transform;
		}
	}

	private void LateUpdate()
	{
		base.transform.LookAt(this.lookTarget);
		base.transform.rotation *= Quaternion.Euler(this.offsetRotation);
	}

	public bool followPlayer;

	public Transform lookTarget;

	public Vector3 offsetRotation;
}
