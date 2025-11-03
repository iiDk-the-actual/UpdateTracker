using System;
using UnityEngine;

public class RotatableSurface : MonoBehaviour
{
	private void LateUpdate()
	{
		float angle = this.spinner.angle;
		base.transform.localRotation = Quaternion.Euler(0f, angle * this.rotationScale, 0f);
	}

	public ManipulatableSpinner spinner;

	public float rotationScale = 1f;
}
