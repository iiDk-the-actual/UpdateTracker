using System;
using UnityEngine;

public abstract class MonoBehaviourTick : MonoBehaviour, ITickSystemTick
{
	public bool TickRunning { get; set; }

	public void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
	}

	public void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public abstract void Tick();
}
