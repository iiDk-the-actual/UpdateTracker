using System;
using UnityEngine;

public class FreezePosition : MonoBehaviour
{
	private void FixedUpdate()
	{
		if (this.target)
		{
			this.target.localPosition = this.localPosition;
		}
	}

	private void LateUpdate()
	{
		if (this.target)
		{
			this.target.localPosition = this.localPosition;
		}
	}

	public Transform target;

	public Vector3 localPosition;
}
