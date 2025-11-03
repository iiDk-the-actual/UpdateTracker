using System;
using UnityEngine;

public struct GameNoiseEvent
{
	public bool IsValid()
	{
		return (float)(Time.timeAsDouble - this.eventTime) <= this.duration;
	}

	public Vector3 position;

	public double eventTime;

	public float duration;

	public float magnitude;
}
