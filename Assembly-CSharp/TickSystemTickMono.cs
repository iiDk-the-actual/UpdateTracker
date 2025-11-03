using System;
using UnityEngine;

internal abstract class TickSystemTickMono : MonoBehaviour, ITickSystemTick
{
	public bool TickRunning { get; set; }

	public virtual void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
	}

	public virtual void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public virtual void Tick()
	{
	}
}
