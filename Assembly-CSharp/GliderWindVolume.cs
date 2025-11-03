using System;
using UnityEngine;

public class GliderWindVolume : MonoBehaviour
{
	public void SetProperties(float speed, float accel, AnimationCurve svaCurve, Vector3 windDirection)
	{
		this.maxSpeed = speed;
		this.maxAccel = accel;
		this.speedVsAccelCurve.CopyFrom(svaCurve);
		this.localWindDirection = windDirection;
	}

	public Vector3 WindDirection
	{
		get
		{
			return base.transform.TransformDirection(this.localWindDirection);
		}
	}

	public Vector3 GetAccelFromVelocity(Vector3 velocity)
	{
		Vector3 windDirection = this.WindDirection;
		float num = Mathf.Clamp(Vector3.Dot(velocity, windDirection), -this.maxSpeed, this.maxSpeed) / this.maxSpeed;
		float num2 = this.speedVsAccelCurve.Evaluate(num) * this.maxAccel;
		return windDirection * num2;
	}

	[SerializeField]
	private float maxSpeed = 30f;

	[SerializeField]
	private float maxAccel = 15f;

	[SerializeField]
	private AnimationCurve speedVsAccelCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[SerializeField]
	private Vector3 localWindDirection = Vector3.up;
}
