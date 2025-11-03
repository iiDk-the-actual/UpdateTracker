using System;
using UnityEngine;

public class TransformFollow : MonoBehaviour
{
	private void Awake()
	{
		this.prevPos = base.transform.position;
	}

	private void LateUpdate()
	{
		this.prevPos = base.transform.position;
		Vector3 vector;
		Quaternion quaternion;
		this.transformToFollow.GetPositionAndRotation(out vector, out quaternion);
		base.transform.SetPositionAndRotation(vector + quaternion * this.offset, quaternion);
	}

	public Transform transformToFollow;

	public Vector3 offset;

	public Vector3 prevPos;
}
