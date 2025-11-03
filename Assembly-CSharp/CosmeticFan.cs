using System;
using UnityEngine;

public class CosmeticFan : MonoBehaviour
{
	private void Start()
	{
		this.spinUpRate = this.maxSpeed / this.spinUpDuration;
		this.spinDownRate = this.maxSpeed / this.spinDownDuration;
	}

	public void Run()
	{
		this.targetSpeed = this.maxSpeed;
		if (this.spinUpDuration > 0f)
		{
			base.enabled = true;
			this.currentAccelRate = this.spinUpRate;
		}
		else
		{
			this.currentSpeed = this.maxSpeed;
		}
		base.enabled = true;
	}

	public void Stop()
	{
		this.targetSpeed = 0f;
		if (this.spinDownDuration > 0f)
		{
			base.enabled = true;
			this.currentAccelRate = this.spinDownRate;
			return;
		}
		this.currentSpeed = 0f;
	}

	public void InstantStop()
	{
		this.targetSpeed = 0f;
		this.currentSpeed = 0f;
		base.enabled = false;
	}

	private void Update()
	{
		this.currentSpeed = Mathf.MoveTowards(this.currentSpeed, this.targetSpeed, this.currentAccelRate * Time.deltaTime);
		base.transform.localRotation = base.transform.localRotation * Quaternion.AngleAxis(this.currentSpeed * Time.deltaTime, this.axis);
		if (this.currentSpeed == 0f && this.targetSpeed == 0f)
		{
			base.enabled = false;
		}
	}

	[SerializeField]
	private Vector3 axis;

	[SerializeField]
	private float spinUpDuration = 0.3f;

	[SerializeField]
	private float spinDownDuration = 0.3f;

	[SerializeField]
	private float maxSpeed = 360f;

	private float currentSpeed;

	private float targetSpeed;

	private float currentAccelRate;

	private float spinUpRate;

	private float spinDownRate;
}
