using System;
using UnityEngine;

internal abstract class TickSystemPreTickMono : MonoBehaviour, ITickSystemPre
{
	public bool PreTickRunning { get; set; }

	public virtual void OnEnable()
	{
		TickSystem<object>.AddPreTickCallback(this);
	}

	public void OnDisable()
	{
		TickSystem<object>.RemovePreTickCallback(this);
	}

	public virtual void PreTick()
	{
	}
}
