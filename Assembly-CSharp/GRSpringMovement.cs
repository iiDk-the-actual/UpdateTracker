using System;
using UnityEngine;

public class GRSpringMovement
{
	public GRSpringMovement(float _tension, float _dampening)
	{
		this.tension = _tension;
		this.dampening = _dampening;
	}

	public void Reset()
	{
		this.pos = 0f;
		this.target = 0f;
		this.speed = 0f;
		this.wasAlreadyAtTargetLastUpdate = false;
	}

	public void SetHardStopAtTarget(bool _hardStopAtTarget)
	{
		if (this.hardStopAtTarget == _hardStopAtTarget)
		{
			return;
		}
		this.hardStopAtTarget = _hardStopAtTarget;
		this.speed = 0f;
	}

	public void Update()
	{
		this.wasAlreadyAtTargetLastUpdate = this.pos == this.target && this.speed == 0f;
		float num = this.pos;
		float num2 = 0.001f;
		float num3 = Mathf.Min(Time.deltaTime, 0.05f);
		float num4 = 6.2832f / this.tension;
		float num5 = num4 * num4 * (this.target - this.pos) - 2f * this.dampening * num4 * this.speed;
		this.speed += num5 * num3;
		this.pos += this.speed * num3;
		if (this.hardStopAtTarget)
		{
			if ((num <= this.pos && this.pos + num2 >= this.target) || (num >= this.pos && this.pos - num2 <= this.target))
			{
				this.speed = 0f;
				this.pos = this.target;
				return;
			}
		}
		else if (Mathf.Abs(num - this.target) < num2 && Mathf.Abs(this.speed) < num2)
		{
			this.speed = 0f;
			this.pos = this.target;
		}
	}

	public bool HitTargetLastUpdate()
	{
		return this.IsAtTarget() && !this.wasAlreadyAtTargetLastUpdate;
	}

	public bool IsAtTarget()
	{
		return this.pos == this.target && this.speed == 0f;
	}

	public float tension = 1f;

	public float dampening = 0.7f;

	public float target;

	public bool hardStopAtTarget = true;

	public float pos;

	public float speed;

	private bool wasAlreadyAtTargetLastUpdate;
}
