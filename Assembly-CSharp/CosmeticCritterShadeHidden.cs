using System;
using UnityEngine;

public class CosmeticCritterShadeHidden : CosmeticCritter
{
	public void SetCenterAndRadius(Vector3 center, float radius)
	{
		this.orbitCenter = center;
		this.orbitRadius = radius;
	}

	public override void SetRandomVariables()
	{
		this.initialAngle = Random.Range(0f, 6.2831855f);
		this.orbitDirection = ((Random.value > 0.5f) ? 1f : (-1f));
	}

	public override void Tick()
	{
		float num = (float)base.GetAliveTime();
		float num2 = this.initialAngle + this.orbitDegreesPerSecond * num * this.orbitDirection;
		float num3 = this.verticalBobMagnitude * Mathf.Sin(num * this.verticalBobFrequency);
		base.transform.position = this.orbitCenter + new Vector3(this.orbitRadius * Mathf.Cos(num2), num3, this.orbitRadius * Mathf.Sin(num2));
	}

	[Space]
	[Tooltip("How quickly the Shade orbits around the point where it spawned (the spawner's position).")]
	[SerializeField]
	private float orbitDegreesPerSecond;

	[Tooltip("The strength of additional up-and-down motion while orbiting.")]
	[SerializeField]
	private float verticalBobMagnitude;

	[Tooltip("The frequency of additional up-and-down motion while orbiting.")]
	[SerializeField]
	private float verticalBobFrequency;

	private Vector3 orbitCenter;

	private float initialAngle;

	private float orbitRadius;

	private float orbitDirection;
}
