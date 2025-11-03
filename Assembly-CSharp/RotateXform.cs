using System;
using UnityEngine;

public class RotateXform : MonoBehaviour
{
	private void Update()
	{
		if (!this.xform)
		{
			return;
		}
		Vector3 vector = ((this.mode == RotateXform.Mode.Local) ? this.xform.localEulerAngles : this.xform.eulerAngles);
		float num = Time.deltaTime * this.speedFactor;
		vector.x += this.speed.x * num;
		vector.y += this.speed.y * num;
		vector.z += this.speed.z * num;
		if (this.mode == RotateXform.Mode.Local)
		{
			this.xform.localEulerAngles = vector;
			return;
		}
		this.xform.eulerAngles = vector;
	}

	public Transform xform;

	public Vector3 speed = Vector3.zero;

	public RotateXform.Mode mode;

	public float speedFactor = 0.0625f;

	public enum Mode
	{
		Local,
		World
	}
}
