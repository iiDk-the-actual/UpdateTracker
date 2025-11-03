using System;
using UnityEngine;

public class GorillaBodyPhysics : MonoBehaviour
{
	private void FixedUpdate()
	{
		this.bodyCollider.transform.position = this.headsetTransform.position + this.bodyColliderOffset;
	}

	public GameObject bodyCollider;

	public Vector3 bodyColliderOffset;

	public Transform headsetTransform;
}
