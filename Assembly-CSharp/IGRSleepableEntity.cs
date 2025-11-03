using System;
using UnityEngine;

public interface IGRSleepableEntity
{
	Vector3 Position { get; }

	float WakeUpRadius { get; }

	bool IsSleeping();

	void WakeUp();

	void Sleep();
}
