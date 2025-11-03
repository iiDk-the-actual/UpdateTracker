using System;
using UnityEngine;

public class IgnoreLocalRotation : MonoBehaviour
{
	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}
}
