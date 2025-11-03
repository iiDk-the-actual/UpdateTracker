using System;
using UnityEngine;

public class BeePerchPoint : MonoBehaviour
{
	public Vector3 GetPoint()
	{
		return base.transform.TransformPoint(this.localPosition);
	}

	[SerializeField]
	private Vector3 localPosition;
}
