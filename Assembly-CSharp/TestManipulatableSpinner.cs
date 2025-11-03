using System;
using UnityEngine;

public class TestManipulatableSpinner : MonoBehaviour
{
	private void Start()
	{
	}

	private void LateUpdate()
	{
		float angle = this.spinner.angle;
		base.transform.rotation = Quaternion.Euler(0f, angle * this.rotationScale, 0f);
	}

	public ManipulatableSpinner spinner;

	public float rotationScale = 1f;
}
