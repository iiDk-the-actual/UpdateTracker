using System;
using UnityEngine;

public class GorillaUITransformFollow : MonoBehaviour
{
	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (this.doesMove)
		{
			base.transform.rotation = this.transformToFollow.rotation;
			base.transform.position = this.transformToFollow.position + this.transformToFollow.rotation * this.offset;
		}
	}

	public Transform transformToFollow;

	public Vector3 offset;

	public bool doesMove;
}
